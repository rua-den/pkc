using Pkc.Core;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class WorkflowFlowNoiseTests
{
    [Fact]
    public void Build_keeps_application_calls_but_drops_framework_plumbing_from_workflow_relations()
    {
        var endpoint = new EvidenceFact(
            "endpoint",
            "endpoint",
            "Run",
            "Demo.RunController",
            new SourceLocation("RunController.cs", 10, 20),
            [],
            new Dictionary<string, string>
            {
                ["httpMethod"] = "POST",
                ["fullRoute"] = "/api/run"
            });
        var service = new EvidenceFact(
            "service",
            "method",
            "RunAsync",
            "Demo.RunService",
            new SourceLocation("RunService.cs", 5, 9),
            [],
            new Dictionary<string, string>());

        var document = new FactDocument(
            "test",
            [endpoint, service],
            [
                new EvidenceRelation(endpoint.Id, "invokes", "Demo.RunService.RunAsync", endpoint.Source),
                new EvidenceRelation(endpoint.Id, "invokes", "System.String.Trim", endpoint.Source),
                new EvidenceRelation(endpoint.Id, "invokes", "Microsoft.AspNetCore.Http.Results.Ok", endpoint.Source),
                new EvidenceRelation(endpoint.Id, "invokes", "Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions.SignInAsync", endpoint.Source)
            ]);

        var candidate = Assert.Single(new CrossStackFeatureCandidateBuilder().Build(document).Candidates);

        Assert.Contains(candidate.Relations, relation =>
            relation.Kind == "invokes" && relation.Target == "Demo.RunService.RunAsync");
        Assert.Contains(candidate.Relations, relation =>
            relation.Kind == "invokes" && relation.Target.Contains("SignInAsync", StringComparison.Ordinal));
        Assert.DoesNotContain(candidate.Relations, relation =>
            relation.Kind == "invokes" && relation.Target.StartsWith("System.", StringComparison.Ordinal));
        Assert.DoesNotContain(candidate.Relations, relation =>
            relation.Kind == "invokes" && relation.Target.StartsWith("Microsoft.AspNetCore.Http.Results.", StringComparison.Ordinal));
    }
}
