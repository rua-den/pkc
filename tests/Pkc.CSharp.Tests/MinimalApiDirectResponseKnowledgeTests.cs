using Pkc.CSharp;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class MinimalApiDirectResponseKnowledgeTests
{
    [Fact]
    public async Task Scan_and_synthesize_preserve_expression_conditional_and_success_responses()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-minimal-api-direct-response-tests", Guid.NewGuid().ToString("N"));
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
                        endpoints.MapGet(
                            "/api/session",
                            () => Results.Ok(new { authenticated = true, owner = "Owner" }));

                        endpoints.MapGet(
                            "/login",
                            (HttpContext context) =>
                                context.User.Identity?.IsAuthenticated == true
                                    ? Results.Redirect("/")
                                    : Results.Content("login"));

                        endpoints.MapPost(
                            "/auth/logout",
                            () =>
                            {
                                return Results.Ok(new { authenticated = false });
                            });

                        return endpoints;
                    }
                }
                """);

            var document = await new CSharpEvidenceScanner().ScanAsync(root);
            var candidates = new CrossStackFeatureCandidateBuilder().Build(document).Candidates;
            var synthesizer = new EvidenceAwareKnowledgeSynthesizer();

            var session = await synthesizer.SynthesizeAsync(FindCandidate(candidates, document, "/api/session"));
            Assert.Contains(session.Rules, rule =>
                rule == "Returns `Results.Ok(new { authenticated = true, owner = \"Owner\" })`." );

            var login = await synthesizer.SynthesizeAsync(FindCandidate(candidates, document, "/login"));
            Assert.Contains(login.Rules, rule =>
                rule.Contains("context.User.Identity?.IsAuthenticated == true", StringComparison.Ordinal) &&
                rule.Contains("Results.Redirect(\"/\")", StringComparison.Ordinal) &&
                rule.Contains("Results.Content(\"login\")", StringComparison.Ordinal));

            var logout = await synthesizer.SynthesizeAsync(FindCandidate(candidates, document, "/auth/logout"));
            Assert.Contains(logout.Rules, rule =>
                rule == "Returns `Results.Ok(new { authenticated = false })`." );

            Assert.All(
                document.Facts.Where(fact => fact.Kind == "endpoint-response"),
                fact => Assert.Equal("project-semantic", fact.Metadata["analysisMode"]));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static FeatureCandidate FindCandidate(
        IReadOnlyList<FeatureCandidate> candidates,
        Pkc.Core.FactDocument document,
        string route)
    {
        var endpoint = Assert.Single(document.Facts, fact =>
            fact.Kind == "endpoint" &&
            fact.Metadata.TryGetValue("fullRoute", out var fullRoute) &&
            fullRoute == route);

        return Assert.Single(candidates, candidate => candidate.SeedFactId == endpoint.Id);
    }
}
