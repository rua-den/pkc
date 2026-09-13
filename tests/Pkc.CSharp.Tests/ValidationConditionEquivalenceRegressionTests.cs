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

    [Fact]
    public void Signed_numeric_literals_remain_semantically_distinct()
    {
        var (candidates, document) = BuildScenario(
            [Validation("ui", "ui-field-validation", "targetId", component: "TargetComponent", condition: "retryCount === -1")],
            [Validation("backend", "backend-field-validation", "TargetId", method: "Create", condition: "request.RetryCount == 1")]);

        AssertConservativeMismatch(Compare(candidates, document));
    }

    [Fact]
    public void Decimal_literals_remain_semantically_distinct()
    {
        var (candidates, document) = BuildScenario(
            [Validation("ui", "ui-field-validation", "targetId", component: "TargetComponent", condition: "threshold === 1.2")],
            [Validation("backend", "backend-field-validation", "TargetId", method: "Create", condition: "request.Threshold == 12")]);

        AssertConservativeMismatch(Compare(candidates, document));
    }

    [Fact]
    public void Decimal_structure_is_not_treated_as_member_access()
    {
        var (candidates, document) = BuildScenario(
            [Validation("ui", "ui-field-validation", "targetId", component: "TargetComponent", condition: "threshold === 1.2")],
            [Validation("backend", "backend-field-validation", "TargetId", method: "Create", condition: "request.Threshold == 2")]);

        AssertConservativeMismatch(Compare(candidates, document));
    }

    [Fact]
    public void Quoted_punctuation_remains_semantically_significant()
    {
        var (candidates, document) = BuildScenario(
            [Validation("ui", "ui-field-validation", "targetId", component: "TargetComponent", condition: "code === 'A-B'")],
            [Validation("backend", "backend-field-validation", "TargetId", method: "Create", condition: "request.Code == \"AB\"")]);

        AssertConservativeMismatch(Compare(candidates, document));
    }

    [Fact]
    public void Distinct_member_paths_do_not_collapse_to_the_same_terminal_member()
    {
        var (candidates, document) = BuildScenario(
            [Validation("ui", "ui-field-validation", "targetId", component: "TargetComponent", condition: "primary.status === 'active'")],
            [Validation("backend", "backend-field-validation", "TargetId", method: "Create", condition: "request.Secondary.Status == Status.Active")]);

        AssertConservativeMismatch(Compare(candidates, document));
    }

    [Fact]
    public void Multi_segment_member_path_case_is_not_erased()
    {
        var (candidates, document) = BuildScenario(
            [Validation("ui", "ui-field-validation", "targetId", component: "TargetComponent", condition: "primary.Status === 'active'")],
            [Validation("backend", "backend-field-validation", "TargetId", method: "Create", condition: "request.Primary.status == \"active\"")]);

        AssertConservativeMismatch(Compare(candidates, document));
    }

    [Fact]
    public void Frontend_loose_equality_is_not_proven_equivalent_to_backend_equality()
    {
        var (candidates, document) = BuildScenario(
            [Validation("ui", "ui-field-validation", "targetId", component: "TargetComponent", condition: "serviceType == 'CSP'")],
            [Validation("backend", "backend-field-validation", "TargetId", method: "Create", condition: "request.ServiceType == ServiceType.CSP")]);

        AssertConservativeMismatch(Compare(candidates, document));
    }

    [Fact]
    public void Angular_value_path_is_stripped_only_for_the_bound_form()
    {
        var (candidates, document) = BuildScenario(
            [Validation(
                "ui",
                "ui-field-validation",
                "targetId",
                component: "TargetComponent",
                condition: "customer.value.status === 'Active'",
                framework: "angular-static")],
            [Validation("backend", "backend-field-validation", "TargetId", method: "Create", condition: "request.Status == Status.Active")],
            bindingForm: "targetForm");

        AssertConservativeMismatch(Compare(candidates, document));
    }

    [Fact]
    public void Known_angular_bound_form_context_can_match_backend_field()
    {
        var (candidates, document) = BuildScenario(
            [Validation(
                "ui",
                "ui-field-validation",
                "targetId",
                component: "TargetComponent",
                condition: "targetForm.value.status === 'Active'",
                framework: "angular-static")],
            [Validation("backend", "backend-field-validation", "TargetId", method: "Create", condition: "request.Status == Status.Active")],
            bindingForm: "targetForm");

        var comparison = Compare(candidates, document);

        Assert.Equal("consistent", comparison.Metadata["status"]);
        Assert.Equal("high", comparison.Metadata["analysisConfidence"]);
    }

    [Fact]
    public void Backend_request_parameter_root_is_case_sensitive()
    {
        var (candidates, document) = BuildScenario(
            [Validation("ui", "ui-field-validation", "targetId", component: "TargetComponent", condition: "status === 'Active'")],
            [Validation("backend", "backend-field-validation", "TargetId", method: "Create", condition: "Request.Status == Status.Active")]);

        AssertConservativeMismatch(Compare(candidates, document));
    }

    [Fact]
    public void Unbound_identifier_equality_is_not_proven_across_stacks()
    {
        var (candidates, document) = BuildScenario(
            [Validation("ui", "ui-field-validation", "targetId", component: "TargetComponent", condition: "left === right")],
            [Validation("backend", "backend-field-validation", "TargetId", method: "Create", condition: "left == right")]);

        AssertConservativeMismatch(Compare(candidates, document));
    }

    private static void AssertConservativeMismatch(EvidenceFact comparison)
    {
        Assert.NotEqual("consistent", comparison.Metadata["status"]);
        Assert.NotEqual("high", comparison.Metadata["analysisConfidence"]);
    }

    private static EvidenceFact Compare(FeatureCandidateDocument candidates, FactDocument document)
    {
        var candidate = Assert.Single(new ValidationConsistencyCandidateEnricher().Enrich(candidates, document).Candidates);
        return Assert.Single(candidate.Facts, fact => fact.Kind == "ui-backend-validation");
    }

    private static (FeatureCandidateDocument Candidates, FactDocument Document) BuildScenario(
        IReadOnlyList<EvidenceFact> uiValidations,
        IReadOnlyList<EvidenceFact> backendValidations,
        string bindingForm = "targetForm")
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
                ["component"] = "TargetComponent",
                ["form"] = bindingForm
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
        string? condition = null,
        string? framework = null)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["field"] = field,
            ["behavior"] = "required"
        };

        if (!string.IsNullOrWhiteSpace(component)) metadata["component"] = component;
        if (!string.IsNullOrWhiteSpace(method)) metadata["method"] = method;
        if (!string.IsNullOrWhiteSpace(condition)) metadata["condition"] = condition;
        if (!string.IsNullOrWhiteSpace(framework)) metadata["framework"] = framework;

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
