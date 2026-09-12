using Pkc.Core;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class ValidationConsistencyClassificationTests
{
    [Fact]
    public async Task Backend_required_without_ui_required_is_possible_mismatch()
    {
        var scenario = BuildScenario(
            uiCondition: null,
            includeUiValidation: false,
            backendCondition: "request.ServiceType == ServiceType.CSP",
            includeBackendValidation: true);

        var enriched = new ValidationConsistencyCandidateEnricher().Enrich(scenario.Candidates, scenario.Facts);
        var candidate = Assert.Single(enriched.Candidates);
        var comparison = Assert.Single(candidate.Facts, fact => fact.Kind == "ui-backend-validation");

        Assert.Equal("possible-mismatch", comparison.Metadata["status"]);
        Assert.Equal("backend-required-ui-required-not-observed", comparison.Metadata["reason"]);

        var knowledge = await new EvidenceAwareKnowledgeSynthesizer().SynthesizeAsync(candidate);
        Assert.Contains(
            knowledge.Rules,
            rule => rule.Contains("Possible UI/backend validation mismatch", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Ui_required_without_backend_required_remains_unknown()
    {
        var scenario = BuildScenario(
            uiCondition: "form.value.serviceType === 'CSP'",
            includeUiValidation: true,
            backendCondition: null,
            includeBackendValidation: false);

        var enriched = new ValidationConsistencyCandidateEnricher().Enrich(scenario.Candidates, scenario.Facts);
        var candidate = Assert.Single(enriched.Candidates);
        var comparison = Assert.Single(candidate.Facts, fact => fact.Kind == "ui-backend-validation");

        Assert.Equal("unknown", comparison.Metadata["status"]);
        Assert.Equal("ui-required-backend-required-not-observed", comparison.Metadata["reason"]);

        var knowledge = await new EvidenceAwareKnowledgeSynthesizer().SynthesizeAsync(candidate);
        Assert.Contains(
            knowledge.Rules,
            rule => rule.Contains("remains unknown", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Different_required_conditions_are_possible_mismatch()
    {
        var scenario = BuildScenario(
            uiCondition: "form.value.serviceType === 'CSP'",
            includeUiValidation: true,
            backendCondition: "request.ServiceType == ServiceType.NCE",
            includeBackendValidation: true);

        var enriched = new ValidationConsistencyCandidateEnricher().Enrich(scenario.Candidates, scenario.Facts);
        var candidate = Assert.Single(enriched.Candidates);
        var comparison = Assert.Single(candidate.Facts, fact => fact.Kind == "ui-backend-validation");

        Assert.Equal("possible-mismatch", comparison.Metadata["status"]);
        Assert.Equal("requiredness-condition-differs", comparison.Metadata["reason"]);
        Assert.Equal("form.value.serviceType === 'CSP'", comparison.Metadata["uiCondition"]);
        Assert.Equal("request.ServiceType == ServiceType.NCE", comparison.Metadata["backendCondition"]);

        var knowledge = await new EvidenceAwareKnowledgeSynthesizer().SynthesizeAsync(candidate);
        Assert.Contains(
            knowledge.Rules,
            rule => rule.Contains("Possible UI/backend validation mismatch", StringComparison.Ordinal) &&
                    rule.Contains("CSP", StringComparison.Ordinal) &&
                    rule.Contains("NCE", StringComparison.Ordinal));
    }

    private static Scenario BuildScenario(
        string? uiCondition,
        bool includeUiValidation,
        string? backendCondition,
        bool includeBackendValidation)
    {
        var endpoint = Fact(
            "endpoint:create",
            "endpoint",
            "Create",
            "ServicesController",
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["httpMethod"] = "POST",
                ["fullRoute"] = "/api/services",
                ["parameters"] = "CreateServiceRequest request"
            });

        var binding = Fact(
            "binding:subscription",
            "ui-field-binding",
            "msSubscriptionId->MicrosoftSubscriptionId",
            "submit",
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["field"] = "msSubscriptionId",
                ["requestField"] = "MicrosoftSubscriptionId",
                ["component"] = "ServiceComponent"
            });

        var allFacts = new List<EvidenceFact> { endpoint, binding };
        if (includeUiValidation)
        {
            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["field"] = "msSubscriptionId",
                ["behavior"] = "required",
                ["component"] = "ServiceComponent"
            };
            if (!string.IsNullOrWhiteSpace(uiCondition))
            {
                metadata["condition"] = uiCondition;
            }

            allFacts.Add(Fact(
                "ui-validation:subscription",
                "ui-field-validation",
                "msSubscriptionId:required",
                "ServiceComponent",
                metadata));
        }

        if (includeBackendValidation)
        {
            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["field"] = "MicrosoftSubscriptionId",
                ["behavior"] = "required",
                ["method"] = "Create"
            };
            if (!string.IsNullOrWhiteSpace(backendCondition))
            {
                metadata["condition"] = backendCondition;
                metadata["conditionKey"] = backendCondition.Contains("NCE", StringComparison.Ordinal)
                    ? "servicetype=nce"
                    : "servicetype=csp";
            }

            allFacts.Add(Fact(
                "backend-validation:subscription",
                "backend-field-validation",
                "MicrosoftSubscriptionId",
                "Create",
                metadata));
        }

        var document = new FactDocument("test", allFacts, []);
        var candidate = new FeatureCandidate(
            "feature:services:create",
            "Services Create",
            "Services",
            endpoint.Id,
            ["backend-code", "frontend-static"],
            [],
            [endpoint, binding],
            []);

        return new Scenario(
            document,
            new FeatureCandidateDocument("test", [candidate]));
    }

    private static EvidenceFact Fact(
        string id,
        string kind,
        string name,
        string? container,
        IReadOnlyDictionary<string, string> metadata) =>
        new(
            id,
            kind,
            name,
            container,
            new SourceLocation("fixture.cs", 1, 1),
            [],
            metadata);

    private sealed record Scenario(
        FactDocument Facts,
        FeatureCandidateDocument Candidates);
}
