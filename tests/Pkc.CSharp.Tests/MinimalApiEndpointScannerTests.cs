using Pkc.CSharp;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class MinimalApiEndpointScannerTests
{
    [Fact]
    public async Task Scan_extracts_minimal_api_route_auth_guard_response_availability_and_semantic_handler_call()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-minimal-api-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "Fixture.csproj"), """
                <Project Sdk="Microsoft.NET.Sdk.Web">
                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                    <Nullable>enable</Nullable>
                  </PropertyGroup>
                </Project>
                """);

            await File.WriteAllTextAsync(Path.Combine(root, "Program.cs"), """
                using Microsoft.AspNetCore.Builder;
                using Microsoft.AspNetCore.Http;
                using Microsoft.Extensions.DependencyInjection;

                WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
                builder.Services.AddSingleton<RunService>();
                WebApplication app = builder.Build();

                bool enableRunEndpoint = true;
                if (enableRunEndpoint)
                {
                    app.MapPost(
                            "/api/run",
                            async (RunRequest request, RunService service) =>
                            {
                                if (string.IsNullOrWhiteSpace(request.Message))
                                {
                                    return Results.BadRequest(new { error = "message is required" });
                                }

                                string result = await service.RunAsync(request.Message);
                                return Results.Ok(new { result });
                            })
                        .RequireAuthorization();
                }

                public sealed record RunRequest(string Message);

                public sealed class RunService
                {
                    public Task<string> RunAsync(string message) => Task.FromResult(message.Trim());
                }
                """);

            var document = await new CSharpEvidenceScanner().ScanAsync(root);

            var endpoint = Assert.Single(document.Facts, fact =>
                fact.Kind == "endpoint" &&
                fact.Metadata.TryGetValue("endpointStyle", out var style) && style == "minimal-api" &&
                fact.Metadata.TryGetValue("fullRoute", out var route) && route == "/api/run");

            Assert.Equal("POST", endpoint.Metadata["httpMethod"]);
            Assert.Equal("Run", endpoint.Container);
            Assert.Equal("RequireAuthorization", endpoint.Metadata["authorization"]);
            Assert.Equal("enableRunEndpoint", endpoint.Metadata["availabilityCondition"]);
            Assert.Equal("project-semantic", endpoint.Metadata["analysisMode"]);
            Assert.Equal("high", endpoint.Metadata["analysisConfidence"]);
            Assert.Equal("matched", endpoint.Metadata["semanticNodeMatch"]);
            Assert.Equal("semantic", endpoint.Metadata["endpointRegistrationResolution"]);

            Assert.Contains(document.Facts, fact =>
                fact.Kind == "condition" &&
                fact.Container == endpoint.Name &&
                fact.Metadata.TryGetValue("sourceKind", out var sourceKind) &&
                sourceKind == "endpoint-registration-condition" &&
                fact.Metadata.TryGetValue("expression", out var expression) &&
                expression == "endpoint is registered only when enableRunEndpoint");

            Assert.Contains(document.Facts, fact =>
                fact.Kind == "condition" &&
                fact.Container == endpoint.Name &&
                fact.Metadata.TryGetValue("conditionExpression", out var conditionExpression) &&
                conditionExpression.Contains("string.IsNullOrWhiteSpace(request.Message)", StringComparison.Ordinal) &&
                fact.Metadata.TryGetValue("response", out var response) &&
                response.Contains("Results.BadRequest", StringComparison.Ordinal) &&
                fact.Metadata.TryGetValue("expression", out var expression) &&
                expression.Contains("=> return Results.BadRequest", StringComparison.Ordinal));

            Assert.Contains(document.Relations, relation =>
                relation.FromFactId == endpoint.Id &&
                relation.Kind == "invokes" &&
                relation.Target.EndsWith("RunService.RunAsync", StringComparison.Ordinal));

            var candidate = Assert.Single(new FeatureCandidateBuilder().Build(document).Candidates);
            Assert.Equal(endpoint.Id, candidate.SeedFactId);
            Assert.Contains(candidate.Facts, fact =>
                fact.Kind == "condition" &&
                fact.Metadata.TryGetValue("sourceKind", out var sourceKind) &&
                sourceKind == "endpoint-registration-condition");
            Assert.Contains(candidate.Facts, fact =>
                fact.Kind == "condition" &&
                fact.Metadata.TryGetValue("response", out var response) &&
                response.Contains("Results.BadRequest", StringComparison.Ordinal));
            Assert.Contains(candidate.Relations, relation =>
                relation.Kind == "invokes" &&
                relation.Target.EndsWith("RunService.RunAsync", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Scan_keeps_endpoint_response_metadata_when_minimal_api_is_declared_inside_a_method()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-minimal-api-method-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "Fixture.csproj"), """
                <Project Sdk="Microsoft.NET.Sdk.Web">
                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                  </PropertyGroup>
                </Project>
                """);

            await File.WriteAllTextAsync(Path.Combine(root, "OwnerEndpoints.cs"), """
                using Microsoft.AspNetCore.Builder;
                using Microsoft.AspNetCore.Http;
                using Microsoft.AspNetCore.Routing;

                public static class OwnerEndpoints
                {
                    public static IEndpointRouteBuilder MapOwnerEndpoints(this IEndpointRouteBuilder endpoints)
                    {
                        endpoints.MapPost(
                            "/auth/login",
                            (LoginRequest request) =>
                            {
                                if (string.IsNullOrWhiteSpace(request.Password))
                                {
                                    return Results.Unauthorized();
                                }

                                return Results.Ok(new { authenticated = true });
                            });

                        return endpoints;
                    }
                }

                public sealed record LoginRequest(string Password);
                """);

            var document = await new CSharpEvidenceScanner().ScanAsync(root);
            var endpoint = Assert.Single(document.Facts, fact =>
                fact.Kind == "endpoint" &&
                fact.Metadata.TryGetValue("fullRoute", out var route) && route == "/auth/login");

            var endpointConditions = document.Relations
                .Where(relation => relation.FromFactId == endpoint.Id && relation.Kind == "contains-condition")
                .Select(relation => document.Facts.Single(fact => fact.Id == relation.Target))
                .ToArray();

            Assert.Contains(endpointConditions, fact =>
                fact.Metadata.TryGetValue("conditionExpression", out var condition) &&
                condition == "string.IsNullOrWhiteSpace(request.Password)" &&
                fact.Metadata.TryGetValue("response", out var response) &&
                response == "Results.Unauthorized()" &&
                fact.Metadata.TryGetValue("expression", out var expression) &&
                expression.Contains("=> return Results.Unauthorized()", StringComparison.Ordinal));

            var candidate = Assert.Single(new FeatureCandidateBuilder().Build(document).Candidates);
            var knowledge = await new EvidenceAwareKnowledgeSynthesizer().SynthesizeAsync(candidate);
            Assert.Contains(knowledge.Rules, rule =>
                rule.Contains("string.IsNullOrWhiteSpace(request.Password) => return Results.Unauthorized()", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Scan_dispatches_interface_call_through_exact_direct_di_registration_to_concrete_behavior()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-minimal-api-di-dispatch-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "Fixture.csproj"), """
                <Project Sdk="Microsoft.NET.Sdk.Web">
                  <PropertyGroup>
                    <TargetFramework>net10.0</TargetFramework>
                    <Nullable>enable</Nullable>
                  </PropertyGroup>
                  <ItemGroup>
                    <FrameworkReference Include="Microsoft.AspNetCore.App" />
                  </ItemGroup>
                </Project>
                """);

            await File.WriteAllTextAsync(Path.Combine(root, "Program.cs"), """
                using Microsoft.AspNetCore.Builder;
                using Microsoft.AspNetCore.Http;
                using Microsoft.Extensions.DependencyInjection;

                WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
                builder.Services.AddScoped<IRunService, RunService>();
                WebApplication app = builder.Build();

                app.MapPost(
                    "/api/run",
                    (RunRequest request, IRunService service) =>
                    {
                        var result = service.RunAsync(request.Message);
                        return Results.Ok(new { result });
                    });

                public sealed record RunRequest(string Message);

                public interface IRunService
                {
                    string RunAsync(string message);
                    string RunAsync(int message);
                }

                public sealed class RunService : IRunService
                {
                    private int _calls;

                    public string RunAsync(string message)
                    {
                        if (string.IsNullOrWhiteSpace(message))
                        {
                            throw new ArgumentException("message is required");
                        }

                        _calls++;
                        return message.Trim();
                    }

                    public string RunAsync(int message) => message.ToString();
                }
                """);

            var document = await new CSharpEvidenceScanner().ScanAsync(root);
            var endpoint = Assert.Single(document.Facts, fact =>
                fact.Kind == "endpoint" &&
                fact.Metadata.TryGetValue("fullRoute", out var route) && route == "/api/run");

            var interfaceInvocation = Assert.Single(document.Relations, relation =>
                relation.FromFactId == endpoint.Id &&
                relation.Kind == "invokes" &&
                relation.Target.EndsWith("IRunService.RunAsync", StringComparison.Ordinal));

            var concrete = Assert.Single(document.Facts, fact =>
                fact.Kind == "method" &&
                fact.Container == "RunService" &&
                fact.Name == "RunAsync" &&
                fact.Metadata.TryGetValue("parameters", out var parameters) &&
                parameters.Contains("string message", StringComparison.Ordinal));

            var dispatch = Assert.Single(document.Relations, relation =>
                relation.FromFactId == endpoint.Id &&
                relation.Kind == "dispatches" &&
                relation.Target == concrete.Id);
            Assert.Equal("Program.cs", dispatch.Source.Path);
            Assert.True(dispatch.Source.StartLine > 0);

            var candidate = Assert.Single(new FeatureCandidateBuilder().Build(document).Candidates);
            Assert.Contains(candidate.Relations, relation =>
                relation.FromFactId == endpoint.Id &&
                relation.Kind == "invokes" &&
                relation.Target == interfaceInvocation.Target);
            Assert.Contains(candidate.Relations, relation =>
                relation.FromFactId == endpoint.Id &&
                relation.Kind == "dispatches" &&
                relation.Target == concrete.Id);
            Assert.Contains(candidate.Facts, fact => fact.Id == concrete.Id);
            Assert.Contains(candidate.Relations, relation =>
                relation.FromFactId == concrete.Id && relation.Kind == "contains-condition");
            Assert.Contains(candidate.Relations, relation =>
                relation.FromFactId == concrete.Id && relation.Kind == "mutates");

            var knowledge = await new GroundedKnowledgeSynthesizer().SynthesizeAsync(candidate);
            Assert.Contains(
                $"Run.POST /api/run via DI registration at {dispatch.Source.Path}:L{dispatch.Source.StartLine} -> RunService.RunAsync",
                knowledge.Flow);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Scan_fails_closed_for_ambiguous_direct_di_registrations()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-minimal-api-di-ambiguous-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "Fixture.csproj"), """
                <Project Sdk="Microsoft.NET.Sdk.Web">
                  <PropertyGroup>
                    <TargetFramework>net10.0</TargetFramework>
                  </PropertyGroup>
                  <ItemGroup>
                    <FrameworkReference Include="Microsoft.AspNetCore.App" />
                  </ItemGroup>
                </Project>
                """);

            await File.WriteAllTextAsync(Path.Combine(root, "Program.cs"), """
                using Microsoft.AspNetCore.Builder;
                using Microsoft.AspNetCore.Http;
                using Microsoft.Extensions.DependencyInjection;

                WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
                builder.Services.AddScoped<IRunService, FirstRunService>();
                builder.Services.AddTransient<IRunService, SecondRunService>();
                WebApplication app = builder.Build();
                app.MapPost("/api/run", (IRunService service) => Results.Ok(service.RunAsync()));

                public interface IRunService { string RunAsync(); }
                public sealed class FirstRunService : IRunService { public string RunAsync() => "first"; }
                public sealed class SecondRunService : IRunService { public string RunAsync() => "second"; }
                """);

            var document = await new CSharpEvidenceScanner().ScanAsync(root);
            var endpoint = Assert.Single(document.Facts, fact =>
                fact.Kind == "endpoint" &&
                fact.Metadata.TryGetValue("fullRoute", out var route) && route == "/api/run");

            Assert.DoesNotContain(document.Relations, relation =>
                relation.FromFactId == endpoint.Id && relation.Kind == "dispatches");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Scan_fails_closed_for_conditional_direct_di_registration()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-minimal-api-di-conditional-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "Fixture.csproj"), """
                <Project Sdk="Microsoft.NET.Sdk.Web">
                  <PropertyGroup>
                    <TargetFramework>net10.0</TargetFramework>
                  </PropertyGroup>
                  <ItemGroup>
                    <FrameworkReference Include="Microsoft.AspNetCore.App" />
                  </ItemGroup>
                </Project>
                """);

            await File.WriteAllTextAsync(Path.Combine(root, "Program.cs"), """
                using Microsoft.AspNetCore.Builder;
                using Microsoft.AspNetCore.Http;
                using Microsoft.Extensions.DependencyInjection;

                WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
                if (DateTime.UtcNow.DayOfWeek == DayOfWeek.Monday)
                {
                    builder.Services.AddScoped<IRunService, RunService>();
                }

                WebApplication app = builder.Build();
                app.MapPost("/api/run", (IRunService service) => Results.Ok(service.RunAsync()));

                public interface IRunService { string RunAsync(); }
                public sealed class RunService : IRunService { public string RunAsync() => "run"; }
                """);

            var document = await new CSharpEvidenceScanner().ScanAsync(root);
            var endpoint = Assert.Single(document.Facts, fact =>
                fact.Kind == "endpoint" &&
                fact.Metadata.TryGetValue("fullRoute", out var route) && route == "/api/run");

            Assert.DoesNotContain(document.Relations, relation =>
                relation.FromFactId == endpoint.Id && relation.Kind == "dispatches");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Scan_fails_closed_for_conditional_access_and_switch_expression_registrations()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-minimal-api-di-expression-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "Fixture.csproj"), """
                <Project Sdk="Microsoft.NET.Sdk.Web">
                  <PropertyGroup>
                    <TargetFramework>net10.0</TargetFramework>
                  </PropertyGroup>
                  <ItemGroup>
                    <FrameworkReference Include="Microsoft.AspNetCore.App" />
                  </ItemGroup>
                </Project>
                """);

            await File.WriteAllTextAsync(Path.Combine(root, "Program.cs"), """
                using Microsoft.AspNetCore.Builder;
                using Microsoft.AspNetCore.Http;
                using Microsoft.Extensions.DependencyInjection;

                WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
                IServiceCollection? services = DateTime.UtcNow.DayOfWeek == DayOfWeek.Monday
                    ? builder.Services
                    : null;
                services?.AddScoped<IRunService, RunService>();
                _ = DateTime.UtcNow.Day switch
                {
                    1 => builder.Services.AddTransient<IRunService, RunService>(),
                    _ => builder.Services
                };

                WebApplication app = builder.Build();
                app.MapPost("/api/run", (IRunService service) => Results.Ok(service.RunAsync()));

                public interface IRunService { string RunAsync(); }
                public sealed class RunService : IRunService { public string RunAsync() => "run"; }
                """);

            var document = await new CSharpEvidenceScanner().ScanAsync(root);
            var endpoint = Assert.Single(document.Facts, fact =>
                fact.Kind == "endpoint" &&
                fact.Metadata.TryGetValue("fullRoute", out var route) && route == "/api/run");

            Assert.DoesNotContain(document.Relations, relation =>
                relation.FromFactId == endpoint.Id && relation.Kind == "dispatches");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
