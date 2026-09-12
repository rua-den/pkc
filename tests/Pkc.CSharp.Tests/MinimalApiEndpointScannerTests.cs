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
}
