using Pkc.Core;
using Pkc.CSharp;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

/// <summary>
/// A result-field rule reached only through an app service registered by assembly scanning, a query, and a
/// mapping-profile member rule built by a reusable expression factory — plus a settings/role gate and a
/// repository-specific authorization attribute. Mirrors a real "when is X shown?" miss with invented names.
/// </summary>
public sealed class RuleSurfaceRegressionTests
{
    [Fact]
    public async Task Scanner_follows_the_sole_implementation_and_extracts_mapping_rules_gates_and_custom_authorization()
    {
        var root = await CreateFixtureAsync(secondImplementation: false);
        try
        {
            var document = await new CSharpEvidenceScanner().ScanAsync(root);

            var endpoint = Endpoint(document, "GetOrdersAsync");
            var implementation = Assert.Single(document.Facts, fact =>
                fact.Kind == "method" && fact.Name == "GetOrdersAsync" && fact.Container == "Fixture.OrdersAppService");
            var dispatch = Assert.Single(document.Relations, relation =>
                relation.FromFactId == endpoint.Id && relation.Kind == CSharpProjectSemanticEnricher.SoleImplementationDispatchRelation);
            Assert.Equal(implementation.Id, dispatch.Target);
            Assert.Equal("AppServices.cs", dispatch.Source.Path);

            var rule = Assert.Single(document.Facts, fact => fact.Kind == "mapped-field-rule");
            Assert.Equal("IsAllowedEditing", rule.Name);
            Assert.Equal("Fixture.OrderRow", rule.Metadata["destinationType"]);
            Assert.Contains("OrderRules.IsEditableOnline", rule.Metadata["ruleSource"], StringComparison.Ordinal);
            Assert.Equal("4", rule.Metadata["conjunctCount"]);
            Assert.Equal("enabled == true", rule.Metadata["conjunct.0"]);
            Assert.Equal("(!string.IsNullOrEmpty(x.SubscriptionId) || x.IsBundle)", rule.Metadata["conjunct.1"]);
            Assert.Equal("Order must have a subscription, or be a bundle", rule.Metadata["note.1"]);
            Assert.False(rule.Metadata.ContainsKey("note.2"));
            Assert.Equal("Order must be active (no past stop date)", rule.Metadata["note.3"]);
            Assert.Equal("Determines whether an order line can be edited online. Requires enabled.", rule.Metadata["documentation"]);

            var query = Assert.Single(document.Facts, fact => fact.Kind == "method" && fact.Name == "ExecuteAsync");
            Assert.Contains(document.Relations, relation =>
                relation.FromFactId == query.Id && relation.Kind == "applies-mapped-field-rule" && relation.Target == rule.Id);

            var gate = Assert.Single(document.Facts, fact => fact.Kind == "boolean-gate");
            Assert.Equal("request.AllowEditing", gate.Metadata["target"]);
            Assert.Equal("isManager", gate.Metadata["operand.0"]);
            Assert.False(gate.Metadata.ContainsKey("operandSource.0"));
            Assert.Equal("settings.GetBool(SettingKeys.Orders_Enable_Editing)", gate.Metadata["operandSource.1"]);
            Assert.Equal("SettingKeys.Orders_Enable_Editing", gate.Metadata["operandSetting.1"]);

            Assert.Equal("TeamAuthorize(\"IsManager\", \"True\")", Endpoint(document, "ChangeQuantity").Metadata["authorizationRequirements"]);
            Assert.DoesNotContain(document.Relations, relation => relation.Kind == CSharpProjectSemanticEnricher.UnresolvedDispatchRelation);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Knowledge_states_the_full_rule_with_developer_comments_setting_gate_and_inferred_flow()
    {
        var root = await CreateFixtureAsync(secondImplementation: false);
        try
        {
            var (list, change) = await SynthesizeAsync(root);

            Assert.Contains(list.Flow, line => line.Contains("Fixture.OrdersAppService.GetOrdersAsync (inferred: the only implementing class", StringComparison.Ordinal));
            var rule = Assert.Single(list.Rules, line => line.StartsWith("Result field `IsAllowedEditing` of `Fixture.OrderRow` is true only when ALL of these hold", StringComparison.Ordinal));
            Assert.Contains("\n   2. `(!string.IsNullOrEmpty(x.SubscriptionId) || x.IsBundle)` — dev comment: \"Order must have a subscription, or be a bundle\"", rule, StringComparison.Ordinal);
            Assert.Contains("\n   4. `(!x.StopDate.HasValue || x.StopDate.Value.Date < now)` — dev comment: \"Order must be active (no past stop date)\"", rule, StringComparison.Ordinal);
            Assert.Contains(list.Rules, line => line.StartsWith("Developer documentation for the rule behind `IsAllowedEditing` (a code comment, not verified behavior", StringComparison.Ordinal));
            Assert.Contains(list.Rules, line => line ==
                "`request.AllowEditing` is true only when all of these hold: `isManager`; setting `SettingKeys.Orders_Enable_Editing` is on (`enableEditing` = `settings.GetBool(SettingKeys.Orders_Enable_Editing)`).");

            Assert.Contains(change.Permissions, line => line.StartsWith("Requires `TeamAuthorize(\"IsManager\", \"True\")`", StringComparison.Ordinal));
            Assert.Contains(change.Rules, line => line == "Rejects (throws `InvalidOperationException`) when `id <= 0`; message: `\"Order id must be positive\"`.");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Several_implementations_are_not_guessed_and_the_dead_end_is_named()
    {
        var root = await CreateFixtureAsync(secondImplementation: true);
        try
        {
            var document = await new CSharpEvidenceScanner().ScanAsync(root);
            var endpoint = Endpoint(document, "GetOrdersAsync");

            Assert.DoesNotContain(document.Relations, relation =>
                relation.FromFactId == endpoint.Id &&
                relation.Kind is "dispatches" or CSharpProjectSemanticEnricher.SoleImplementationDispatchRelation);
            Assert.Contains(document.Relations, relation =>
                relation.FromFactId == endpoint.Id &&
                relation.Kind == CSharpProjectSemanticEnricher.UnresolvedDispatchRelation &&
                relation.Target == "Fixture.IOrdersAppService.GetOrdersAsync");

            var (list, _) = await SynthesizeAsync(root, document);
            Assert.Contains(list.Unknowns, line =>
                line.StartsWith("Backend evidence stops at 1 interface call(s) whose implementation PKC could not prove", StringComparison.Ordinal) &&
                line.Contains("`Fixture.IOrdersAppService.GetOrdersAsync`", StringComparison.Ordinal));
            Assert.DoesNotContain(list.Rules, line => line.StartsWith("Result field `IsAllowedEditing`", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static async Task<(FeatureKnowledge List, FeatureKnowledge Change)> SynthesizeAsync(string root, FactDocument? document = null)
    {
        document ??= await new CSharpEvidenceScanner().ScanAsync(root);
        var candidates = new CrossStackFeatureCandidateBuilder().Build(document);
        var synthesizer = new JointVisibilityKnowledgeSynthesizer();
        var list = await synthesizer.SynthesizeAsync(Assert.Single(candidates.Candidates, candidate => candidate.Id.EndsWith(":getordersasync", StringComparison.Ordinal)));
        var change = await synthesizer.SynthesizeAsync(Assert.Single(candidates.Candidates, candidate => candidate.Id.EndsWith(":changequantity", StringComparison.Ordinal)));
        return (list, change);
    }

    private static EvidenceFact Endpoint(FactDocument document, string name) =>
        Assert.Single(document.Facts, fact => fact.Kind == "endpoint" && fact.Name == name);

    private static async Task<string> CreateFixtureAsync(bool secondImplementation)
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-rule-surface-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        await File.WriteAllTextAsync(Path.Combine(root, "Fixture.csproj"), """
            <Project Sdk="Microsoft.NET.Sdk.Web">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
                <Nullable>enable</Nullable>
                <ImplicitUsings>enable</ImplicitUsings>
                <AssemblyName>Fixture</AssemblyName>
              </PropertyGroup>
            </Project>
            """);
        await File.WriteAllTextAsync(Path.Combine(root, "Program.cs"), """
            using Fixture;

            var builder = WebApplication.CreateBuilder(args);
            AutoRegistration.AddAppServices(builder.Services);
            builder.Services.AddControllers();
            var app = builder.Build();
            app.MapControllers();
            app.Run();
            """);
        await File.WriteAllTextAsync(Path.Combine(root, "OrdersController.cs"), """
            using Microsoft.AspNetCore.Mvc;

            namespace Fixture;

            [ApiController]
            [Route("api/orders")]
            public class OrdersController : ControllerBase
            {
                private readonly IOrdersAppService _app;

                public OrdersController(IOrdersAppService app)
                {
                    _app = app;
                }

                [HttpGet]
                public async Task<IActionResult> GetOrdersAsync()
                {
                    var isManager = User.IsInRole("Manager");
                    var rows = await _app.GetOrdersAsync(isManager);
                    return Ok(rows);
                }

                [HttpPut("{id}/quantity")]
                [TeamAuthorize("IsManager", "True")]
                public IActionResult ChangeQuantity(int id)
                {
                    Guard.Against<InvalidOperationException>(id <= 0, "Order id must be positive");
                    return Ok(id);
                }
            }

            public static class Guard
            {
                public static void Against<TException>(bool condition, string message)
                    where TException : Exception
                {
                    if (condition)
                    {
                        throw (TException)Activator.CreateInstance(typeof(TException), message)!;
                    }
                }
            }

            public sealed class TeamAuthorizeAttribute(string claim, string value) : Attribute
            {
                public string Claim { get; } = claim;
                public string Value { get; } = value;
            }
            """);
        await File.WriteAllTextAsync(Path.Combine(root, "AppServices.cs"), $$"""
            namespace Fixture;

            public static class AutoRegistration
            {
                public static void AddAppServices(IServiceCollection services)
                {
                    foreach (var type in typeof(AutoRegistration).Assembly.GetTypes().Where(type => type.Name.EndsWith("AppService")))
                    {
                        foreach (var contract in type.GetInterfaces())
                        {
                            services.AddScoped(contract, type);
                        }
                    }
                }
            }

            public interface IOrdersAppService
            {
                Task<List<OrderDto>> GetOrdersAsync(bool isManager);
            }

            public class OrdersAppService(GetOrdersQuery query) : IOrdersAppService
            {
                public async Task<List<OrderDto>> GetOrdersAsync(bool isManager)
                {
                    var settings = new SettingsReader();
                    var enableEditing = settings.GetBool(SettingKeys.Orders_Enable_Editing);
                    var request = new OrdersRequest();
                    request.AllowEditing = isManager && enableEditing;
                    var rows = await query.ExecuteAsync(request);
                    return rows.Select(row => new OrderDto { IsAllowedEditing = row.IsAllowedEditing }).ToList();
                }
            }
            {{(secondImplementation ? """

            public class CachedOrdersAppService : IOrdersAppService
            {
                public Task<List<OrderDto>> GetOrdersAsync(bool isManager) => Task.FromResult(new List<OrderDto>());
            }
            """ : string.Empty)}}
            """);
        await File.WriteAllTextAsync(Path.Combine(root, "Orders.cs"), """
            using System.Linq.Expressions;

            namespace Fixture;

            public static class SettingKeys
            {
                // Deliberately not const: real settings keys are often plain static fields.
                public static string Orders_Enable_Editing = "Orders.EnableEditing";
            }

            public sealed class SettingsReader
            {
                public bool GetBool(string key) => key.Length > 0;
            }

            public sealed class OrdersRequest
            {
                public bool AllowEditing { get; set; }
            }

            public sealed class Order
            {
                public string? SubscriptionId { get; set; }
                public bool IsBundle { get; set; }
                public bool IsEditableOnline { get; set; }
                public DateTime? StopDate { get; set; }
            }

            public sealed class OrderRow
            {
                public bool IsAllowedEditing { get; set; }
            }

            public sealed class OrderDto
            {
                public bool IsAllowedEditing { get; set; }
            }

            public class GetOrdersQuery
            {
                public Task<List<OrderRow>> ExecuteAsync(OrdersRequest request) =>
                    Task.FromResult(new List<OrderRow> { new() { IsAllowedEditing = request.AllowEditing } });
            }

            public static class OrderRules
            {
                /// <summary>
                /// Determines whether an order line can be edited online. Requires <paramref name="enabled"/>.
                /// </summary>
                public static Expression<Func<Order, bool>> IsEditableOnline(bool? enabled)
                {
                    var now = DateTime.Now.Date;

                    return x => enabled == true &&

                        // Order must have a subscription, or be a bundle
                        (!string.IsNullOrEmpty(x.SubscriptionId) || x.IsBundle) &&
                        x.IsEditableOnline &&

                        // Order must be active (no past stop date)
                        (!x.StopDate.HasValue || x.StopDate.Value.Date < now);
                }
            }

            public class MiniProfile
            {
                public MapExpression<TSource, TDestination> CreateMap<TSource, TDestination>() => new();
            }

            public sealed class MapExpression<TSource, TDestination>
            {
                public MapExpression<TSource, TDestination> ForMember<TMember>(
                    Expression<Func<TDestination, TMember>> destination,
                    Action<MemberOptions<TSource>> options) => this;
            }

            public sealed class MemberOptions<TSource>
            {
                public void MapFrom<TResult>(Expression<Func<TSource, TResult>> source)
                {
                }
            }

            public sealed class OrderRowProfile : MiniProfile
            {
                public OrderRowProfile()
                {
                    bool? enabled = null;
                    CreateMap<Order, OrderRow>()
                        .ForMember(d => d.IsAllowedEditing, o => o.MapFrom(OrderRules.IsEditableOnline(enabled)));
                }
            }
            """);
        return root;
    }
}
