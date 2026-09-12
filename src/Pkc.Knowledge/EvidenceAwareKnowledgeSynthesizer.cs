using Pkc.Core;

namespace Pkc.Knowledge;

public sealed class EvidenceAwareKnowledgeSynthesizer : IKnowledgeSynthesizer
{
    private const string AspNetSignIn =
        "Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions.SignInAsync";

    private const string AspNetSignOut =
        "Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions.SignOutAsync";

    private static readonly HashSet<string> UiBehaviorKinds = new(StringComparer.Ordinal)
    {
        "ui-field",
        "ui-field-option",
        "ui-field-validation",
        "ui-field-visibility",
        "ui-field-enabled-state",
        "ui-field-binding"
    };

    private readonly GroundedKnowledgeSynthesizer _inner = new();

    public async ValueTask<FeatureKnowledge> SynthesizeAsync(
        FeatureCandidate candidate,
        CancellationToken cancellationToken = default)
    {
        var knowledge = await _inner.SynthesizeAsync(candidate, cancellationToken);
        var responseFacts = candidate.Facts
            .Where(fact => fact.Kind == "endpoint-response")
            .ToArray();
        var uiBehaviorFacts = candidate.Facts
            .Where(fact => UiBehaviorKinds.Contains(fact.Kind))
            .ToArray();
        var backendValidationFacts = candidate.Facts
            .Where(fact => fact.Kind == "backend-field-validation")
            .ToArray();
        var consistencyFacts = candidate.Facts
            .Where(fact => fact.Kind == "ui-backend-validation")
            .ToArray();

        var rules = knowledge.Rules
            .Concat(responseFacts.Select(DescribeResponseRule))
            .Concat(uiBehaviorFacts.Select(DescribeUiBehaviorRule))
            .Concat(backendValidationFacts.Select(DescribeBackendValidationRule))
            .Concat(consistencyFacts.Select(DescribeValidationConsistencyRule))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var sideEffects = knowledge.SideEffects
            .Concat(BuildSemanticSideEffects(candidate))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var evidence = knowledge.Evidence
            .Concat(responseFacts.Select(fact => new KnowledgeEvidence(
                fact.Id,
                fact.Kind,
                $"Endpoint response: {DescribeResponseRule(fact)}",
                fact.Source)))
            .Concat(uiBehaviorFacts.Select(fact => new KnowledgeEvidence(
                fact.Id,
                fact.Kind,
                DescribeUiBehaviorRule(fact),
                fact.Source)))
            .Concat(backendValidationFacts.Select(fact => new KnowledgeEvidence(
                fact.Id,
                fact.Kind,
                DescribeBackendValidationRule(fact),
                fact.Source)))
            .Concat(consistencyFacts.Select(fact => new KnowledgeEvidence(
                fact.Id,
                fact.Kind,
                DescribeValidationConsistencyRule(fact),
                fact.Source)))
            .GroupBy(item => item.FactId, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(item => item.Source.Path, StringComparer.Ordinal)
            .ThenBy(item => item.Source.StartLine)
            .ToArray();

        return knowledge with
        {
            Rules = rules,
            SideEffects = sideEffects,
            Evidence = evidence
        };
    }

    private static string DescribeResponseRule(EvidenceFact fact)
    {
        if (fact.Metadata.TryGetValue("conditionExpression", out var condition) &&
            fact.Metadata.TryGetValue("whenTrue", out var whenTrue) &&
            fact.Metadata.TryGetValue("whenFalse", out var whenFalse))
        {
            return $"When `{condition}`, returns `{whenTrue}`; otherwise returns `{whenFalse}`.";
        }

        if (fact.Metadata.TryGetValue("response", out var response))
        {
            return $"Returns `{response}`.";
        }

        return "Endpoint response observed, but its expression was not captured.";
    }

    private static string DescribeUiBehaviorRule(EvidenceFact fact)
    {
        fact.Metadata.TryGetValue("field", out var field);
        var fieldName = field ?? fact.Name;

        if (fact.Kind == "ui-field")
        {
            fact.Metadata.TryGetValue("element", out var element);
            var elementText = string.IsNullOrWhiteSpace(element)
                ? string.Empty
                : $" as `{element}`";
            return $"UI exposes field `{fieldName}`{elementText}.";
        }

        if (fact.Kind == "ui-field-option")
        {
            fact.Metadata.TryGetValue("value", out var value);
            fact.Metadata.TryGetValue("label", out var label);
            var optionValue = value ?? fact.Name;
            return string.IsNullOrWhiteSpace(label) || string.Equals(label, optionValue, StringComparison.Ordinal)
                ? $"UI field `{fieldName}` offers option `{optionValue}`."
                : $"UI field `{fieldName}` offers option `{optionValue}` labeled `{label}`.";
        }

        if (fact.Kind == "ui-field-validation")
        {
            fact.Metadata.TryGetValue("behavior", out var behavior);
            fact.Metadata.TryGetValue("condition", out var condition);
            var validation = behavior ?? "validation";
            return string.IsNullOrWhiteSpace(condition)
                ? $"UI field `{fieldName}` is `{validation}`."
                : $"UI field `{fieldName}` is `{validation}` when `{condition}`.";
        }

        if (fact.Kind == "ui-field-visibility")
        {
            fact.Metadata.TryGetValue("condition", out var condition);
            return string.IsNullOrWhiteSpace(condition)
                ? $"UI field `{fieldName}` has observed visibility behavior."
                : $"UI field `{fieldName}` is visible when `{condition}`.";
        }

        if (fact.Kind == "ui-field-enabled-state")
        {
            fact.Metadata.TryGetValue("behavior", out var behavior);
            fact.Metadata.TryGetValue("condition", out var condition);
            var state = behavior ?? "state-controlled";
            return string.IsNullOrWhiteSpace(condition)
                ? $"UI field `{fieldName}` is `{state}`."
                : $"UI field `{fieldName}` is `{state}` when `{condition}`.";
        }

        if (fact.Kind == "ui-field-binding")
        {
            fact.Metadata.TryGetValue("requestField", out var requestField);
            return string.IsNullOrWhiteSpace(requestField)
                ? $"UI field `{fieldName}` participates in request binding."
                : $"UI field `{fieldName}` maps to request field `{requestField}`.";
        }

        return $"{fact.Kind}: {fact.Name}";
    }

    private static string DescribeBackendValidationRule(EvidenceFact fact)
    {
        fact.Metadata.TryGetValue("field", out var field);
        fact.Metadata.TryGetValue("behavior", out var behavior);
        fact.Metadata.TryGetValue("condition", out var condition);
        var fieldName = field ?? fact.Name;
        var validation = behavior ?? "validation";

        return string.IsNullOrWhiteSpace(condition)
            ? $"Backend field `{fieldName}` is `{validation}`."
            : $"Backend field `{fieldName}` is `{validation}` when `{condition}`.";
    }

    private static string DescribeValidationConsistencyRule(EvidenceFact fact)
    {
        fact.Metadata.TryGetValue("uiField", out var uiField);
        fact.Metadata.TryGetValue("backendField", out var backendField);
        fact.Metadata.TryGetValue("status", out var status);
        fact.Metadata.TryGetValue("uiCondition", out var uiCondition);
        fact.Metadata.TryGetValue("backendCondition", out var backendCondition);

        var ui = uiField ?? fact.Name;
        var backend = backendField ?? fact.Metadata.GetValueOrDefault("requestField") ?? fact.Name;

        if (string.Equals(status, "consistent", StringComparison.Ordinal))
        {
            if (!string.IsNullOrWhiteSpace(uiCondition) || !string.IsNullOrWhiteSpace(backendCondition))
            {
                return $"Validation consistency observed: UI field `{ui}` and backend field `{backend}` are both `required` under matching condition evidence (UI: `{uiCondition ?? "unconditional"}`; backend: `{backendCondition ?? "unconditional"}`).";
            }

            return $"Validation consistency observed: UI field `{ui}` and backend field `{backend}` are both `required`.";
        }

        if (string.Equals(status, "possible-mismatch", StringComparison.Ordinal))
        {
            return $"Possible UI/backend validation mismatch for `{ui}` → `{backend}` (UI condition: `{uiCondition ?? "required not observed"}`; backend condition: `{backendCondition ?? "required not observed"}`).";
        }

        return $"UI/backend validation comparison for `{ui}` → `{backend}` remains unknown because requiredness was not observed on both sides.";
    }

    private static IEnumerable<string> BuildSemanticSideEffects(FeatureCandidate candidate)
    {
        foreach (var relation in candidate.Relations.Where(relation => relation.Kind == "invokes"))
        {
            if (string.Equals(relation.Target, AspNetSignIn, StringComparison.Ordinal))
            {
                yield return "Signs in the current HTTP context using ASP.NET authentication.";
            }
            else if (string.Equals(relation.Target, AspNetSignOut, StringComparison.Ordinal))
            {
                yield return "Signs out the current HTTP context using ASP.NET authentication.";
            }
        }
    }
}
