using Pkc.Core;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class AuthenticationSideEffectKnowledgeTests
{
    [Theory]
    [InlineData(
        "Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions.SignInAsync",
        "Signs in the current HTTP context using ASP.NET authentication.")]
    [InlineData(
        "Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions.SignOutAsync",
        "Signs out the current HTTP context using ASP.NET authentication.")]
    public async Task Synthesize_surfaces_exact_AspNet_authentication_side_effects(
        string target,
        string expected)
    {
        var endpoint = new EvidenceFact(
            "endpoint",
            "endpoint",
            "POST /auth/action",
            "Auth",
            new SourceLocation("OwnerAuthentication.cs", 10, 20),
            [],
            new Dictionary<string, string>
            {
                ["httpMethod"] = "POST",
                ["fullRoute"] = "/auth/action"
            });

        var candidate = new FeatureCandidate(
            "feature:auth:action",
            "Auth action",
            "Auth",
            endpoint.Id,
            ["backend-code"],
            [],
            [endpoint],
            [new EvidenceRelation(endpoint.Id, "invokes", target, endpoint.Source)]);

        var knowledge = await new EvidenceAwareKnowledgeSynthesizer().SynthesizeAsync(candidate);

        Assert.Contains(expected, knowledge.SideEffects);
    }

    [Fact]
    public async Task Synthesize_does_not_promote_unrelated_method_named_signin()
    {
        var endpoint = new EvidenceFact(
            "endpoint",
            "endpoint",
            "POST /auth/action",
            "Auth",
            new SourceLocation("Auth.cs", 1, 2),
            [],
            new Dictionary<string, string>
            {
                ["httpMethod"] = "POST",
                ["fullRoute"] = "/auth/action"
            });

        var candidate = new FeatureCandidate(
            "feature:auth:action",
            "Auth action",
            "Auth",
            endpoint.Id,
            ["backend-code"],
            [],
            [endpoint],
            [new EvidenceRelation(endpoint.Id, "invokes", "Demo.CustomAuth.SignInAsync", endpoint.Source)]);

        var knowledge = await new EvidenceAwareKnowledgeSynthesizer().SynthesizeAsync(candidate);

        Assert.Empty(knowledge.SideEffects);
    }
}
