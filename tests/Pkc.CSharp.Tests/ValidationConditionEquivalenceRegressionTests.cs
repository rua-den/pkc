using Pkc.Core;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class ValidationConditionEquivalenceRegressionTests
{
    [Fact]
    public void Compound_condition_must_match_in_full()
    {
        var (candidates, document) = BuildScenario(
            [Validation("ui", "ui-field-validation", "targetId", component: "TargetComponent", condition: "serviceType === 'CSP' && region === 'US'")],
            [Validation("backend", "backend-field-validation", "TargetId", method: "Create", condition: "request.ServiceType == ServiceType.CSP")]);

        var comparison = Compare(candidates, document);

        Assert.Equal("possible-mismatch", comparison.Metadata["status"]);
        Assert.NotEqual("high", comparison.Metadata["analysisConfidence"]);
    }

    [Fact]
    public void Equivalent_compound_conditions_allow_reversed_equality_operands()
    {
        var (candidates, document) = BuildScenario(
            [Validation("ui", "ui-field-validation", "targetId", component: "TargetComponent", condition: "'CSP' === serviceType && 'US' === region")],
            [Validation("backend", "backend-field-validation", "TargetId", method: "Create", condition: "request.ServiceType == ServiceType.CSP && request.Region == Region.US")]);

        var comparison = Compare(candidates, document);

        Assert.Equal("consistent", comparison.Metadata["status"]);
        Assert.Equal("matching-conditional-requiredness", comparison.Metadata["reason"]);
        Assert.Equal("high", comparison.Metadata["analysisConfidence"]);
    }

    [Fact]
    public void Conditional_and_unconditional_requiredness_are_not_equivalent()
    {
        var (candidates, document) = BuildScenario(
            [Validation("ui", "ui-field-validation", "targetId", component: "TargetComponent")],
            [Validation("backend", "backend-field-validation", "TargetId", method: "Create", condition: "request.ServiceType == ServiceType.CSP")]);

        var comparison = Compare(candidates, document);

        Assert.Equal("possible-mismatch", comparison.Metadata["status"]);
        Assert.Equal("requiredness-condition-differs", comparison.Metadata["reason"]);
    }

    [Fact]
    public void Multiple_requiredness_facts_must_match_as_a_set()
    {
        var (candidates, document) = BuildScenario(
            [
                Validation("ui-csp", "ui-field-validation", "targetId", component: "TargetComponent", condition: "serviceType === 'CSP'"),
                Validation("ui-nce", "ui-field-validation", "targetId", component: "TargetComponent", condition: "serviceType === 'NCE'")
            ],
            [Validation("backend-csp", "backend-field-validation", "TargetId", method: "Create", condition: "request.ServiceType == ServiceType.CSP")]);

        var comparison = Compare(candidates, document);

        Assert.Equal("possible-mismatch", comparison.Metadata["status"]);
        Assert.Equal("requiredness-condition-differs", comparison.Metadata["reason"]);
    }

    private static EvidenceFact Compare(FeatureCandidateDocument candidates, FactDocument document)
    {
        var candidate = Assert.Single(new ValidationConsistencyCandidateEnricher().Enrich(candidates, document).Candidates);
        return Assert.Single(candidate.Facts, fact => fact.Kind == "ui-backend-validation");
    }

    private static (FeatureCandidateDocument Candidates, FactDocument Document) BuildScenario(
        IReadOnlyList<EvidenceFact> uiValidations,
        IReadOnlyList<EvidenceFact> backendValidations)
    {
        var endpoint = new EvidenceFact(
            "endpoint",
            "endpoint",
            "Create",
            "TargetsController",
            new SourceLocation("TargetsController.cs", 1, 1),
            [],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["httpMethod"] = "POST",
                ["fullRoute"] = "/api/targets",
                ["parameters"] = "CreateTargetRequest request"
            });

        var binding = new EvidenceFact(
            "binding",
            "ui-field-binding",
            "targetId->TargetId",
            "submit",
            new SourceLocation("target.component.ts", 10, 10),
            [],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["field"] = "targetId",
                ["requestField"] = "TargetId",
                ["component"] = "TargetComponent"
            });

        var candidate = new FeatureCandidate(
            "feature:targets:create",
            "Targets Create",
            "Targets",
            endpoint.Id,
            ["backend-code", "frontend-static"],
            [],
            [endpoint, binding],
            []);

        var facts = new List<EvidenceFact> { endpoint, binding };
        facts.AddRange(uiValidations);
        facts.AddRange(backendValidations);

        return (
            new FeatureCandidateDocument("test", [candidate]),
            new FactDocument("test", facts, []));
    }

    private static EvidenceFact Validation(
        string id,
        string kind,
        string field,
        string? component = null,
        string? method = null,
        string? condition = null)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["field"] = field,
            ["behavior"] = "required"
        };

        if (!string.IsNullOrWhiteSpace(component)) metadata["component"] = component;
        if (!string.IsNullOrWhiteSpace(method)) metadata["method"] = method;
        if (!string.IsNullOrWhiteSpace(condition)) metadata["condition"] = condition;

        return new EvidenceFact(
            id,
            kind,
            field,
            component ?? method,
            new SourceLocation(kind == "ui-field-validation" ? "target.component.ts" : "TargetsController.cs", 20, 20),
            [],
            metadata);
    }
}
