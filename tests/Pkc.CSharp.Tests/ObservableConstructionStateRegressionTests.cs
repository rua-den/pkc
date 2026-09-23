using Pkc.CSharp;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class ObservableConstructionStateRegressionTests
{
    [Fact]
    public async Task Returned_constructed_object_used_by_downstream_call_promotes_initializer_state()
    {
        var knowledge = await BuildKnowledgeAsync(
            """
            _repository.Add(order);
            return Ok(order);
            """);

        Assert.Contains(knowledge.StateChanges, change =>
            change.Contains("`order.Status`", StringComparison.Ordinal) &&
            change.Contains("`OrderStatus.New`", StringComparison.Ordinal));
        Assert.Contains(knowledge.StateChanges, change =>
            change.Contains("`order.BonusAwarded`", StringComparison.Ordinal) &&
            change.Contains("`request.Price * 0.05m`", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Return_only_projection_does_not_promote_initializer_state()
    {
        var knowledge = await BuildKnowledgeAsync(
            """
            return Ok(order);
            """);

        Assert.DoesNotContain(knowledge.StateChanges, change =>
            change.Contains("order.Status", StringComparison.Ordinal) ||
            change.Contains("order.BonusAwarded", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Downstream_call_for_different_local_does_not_promote_initializer_state()
    {
        var knowledge = await BuildKnowledgeAsync(
            """
            var other = new Order();
            _repository.Add(other);
            return Ok(order);
            """);

        Assert.DoesNotContain(knowledge.StateChanges, change =>
            change.Contains("order.Status", StringComparison.Ordinal) ||
            change.Contains("order.BonusAwarded", StringComparison.Ordinal));
    }

    private static async Task<FeatureKnowledge> BuildKnowledgeAsync(string tail)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pkc-observable-construction-state-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "Fixture.csproj"), Project);
            await File.WriteAllTextAsync(Path.Combine(root, "OrdersController.cs"), Source(tail));

            var document = await new CSharpEvidenceScanner().ScanAsync(root);
            var candidate = Assert.Single(
                new FeatureCandidateBuilder().Build(document).Candidates,
                item => item.Name.Contains("Create", StringComparison.Ordinal));
            return await new GroundedKnowledgeSynthesizer().SynthesizeAsync(candidate);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static string Source(string tail) => $$"""
        using Microsoft.AspNetCore.Mvc;

        namespace Demo;

        public enum OrderStatus
        {
            New,
            Packed
        }

        public sealed class CreateOrderRequest
        {
            public decimal Price { get; set; }
        }

        public sealed class Order
        {
            public OrderStatus Status { get; set; }
            public decimal BonusAwarded { get; set; }
        }

        public interface IOrderRepository
        {
            void Add(Order order);
        }

        [ApiController]
        [Route("api/orders")]
        public sealed class OrdersController(IOrderRepository repository) : ControllerBase
        {
            private readonly IOrderRepository _repository = repository;

            [HttpPost]
            public ActionResult<Order> Create(CreateOrderRequest request)
            {
                var order = new Order
                {
                    Status = OrderStatus.New,
                    BonusAwarded = request.Price * 0.05m
                };

                {{tail}}
            }
        }
        """;

    private const string Project = """
        <Project Sdk="Microsoft.NET.Sdk.Web">
          <PropertyGroup>
            <TargetFramework>net8.0</TargetFramework>
            <ImplicitUsings>enable</ImplicitUsings>
            <Nullable>enable</Nullable>
          </PropertyGroup>
        </Project>
        """;
}
