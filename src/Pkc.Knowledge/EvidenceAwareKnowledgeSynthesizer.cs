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
        var valueTransferFacts = candidate.Facts
            .Where(fact => fact.Kind == "value-transfer")
            .OrderBy(fact => fact.Source.Path, StringComparer.Ordinal)
            .ThenBy(fact => fact.Source.StartLine)
            .ThenBy(fact => fact.Id, StringComparer.Ordinal)
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
            .Concat(valueTransferFacts.Select(fact => new KnowledgeEvidence(
                fact.Id,
                fact.Kind,
                DescribeValueTransferEvidence(fact),
                fact.Source)))
            .GroupBy(item => item.FactId, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(item => item.Source.Path, StringComparer.Ordinal)
            .ThenBy(item => item.Source.StartLine)
            .ToArray();

        return knowledge with
        {
            Rules = rules,
            ValueLineage = BuildValueLineage(valueTransferFacts),
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

    private static IReadOnlyList<string> BuildValueLineage(IReadOnlyList<EvidenceFact> transfers)
    {
        if (transfers.Count == 0)
        {
            return [];
        }

        var snapshotTransfers = transfers
            .Where(IsStoredSnapshotTransfer)
            .ToArray();
        var byId = snapshotTransfers.ToDictionary(fact => fact.Id, StringComparer.Ordinal);
        var predecessorIds = snapshotTransfers
            .Select(fact => fact.Metadata.TryGetValue("predecessorTransferFactId", out var predecessor)
                ? predecessor
                : null)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Cast<string>()
            .ToHashSet(StringComparer.Ordinal);

        var result = new List<string>();
        foreach (var leaf in snapshotTransfers.Where(fact => !predecessorIds.Contains(fact.Id)))
        {
            var chain = BuildTransferChain(leaf, byId);
            if (chain.Count < 2)
            {
                continue;
            }

            var nodes = new List<string>();
            if (chain[0].Metadata.TryGetValue("sourceOccurrence", out var firstSource))
            {
                nodes.Add(firstSource);
            }

            nodes.AddRange(chain.Select(fact =>
                fact.Metadata.TryGetValue("targetOccurrence", out var target)
                    ? target
                    : fact.Name));

            var proofLocations = string.Join(
                ", ",
                chain.Select(fact => $"`{fact.Source.Path}:L{fact.Source.StartLine}`"));
            result.Add($"Proven stored lineage chain: {string.Join(" → ", nodes.Select(node => $"`{node}`"))}. Each edge is a direct scalar auto-property copy stored as a snapshot. Edge proof: {proofLocations}.");
        }

        result.AddRange(transfers.Select(DescribeValueTransfer));
        return result.Distinct(StringComparer.Ordinal).ToArray();
    }

    private static bool IsStoredSnapshotTransfer(EvidenceFact fact) =>
        fact.Metadata.TryGetValue("mechanism", out var mechanism) &&
        string.Equals(mechanism, "copy", StringComparison.Ordinal) &&
        fact.Metadata.TryGetValue("temporalSemantics", out var temporal) &&
        string.Equals(temporal, "snapshot", StringComparison.Ordinal);

    private static IReadOnlyList<EvidenceFact> BuildTransferChain(
        EvidenceFact leaf,
        IReadOnlyDictionary<string, EvidenceFact> byId)
    {
        var reversed = new List<EvidenceFact>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var current = leaf;

        while (seen.Add(current.Id))
        {
            reversed.Add(current);
            if (!current.Metadata.TryGetValue("predecessorTransferFactId", out var predecessorId) ||
                !byId.TryGetValue(predecessorId, out var predecessor))
            {
                break;
            }

            current = predecessor;
        }

        reversed.Reverse();
        return reversed;
    }

    private static string DescribeValueTransfer(EvidenceFact fact)
    {
        fact.Metadata.TryGetValue("sourceOccurrence", out var source);
        fact.Metadata.TryGetValue("targetOccurrence", out var target);
        fact.Metadata.TryGetValue("compositionStatus", out var compositionStatus);
        fact.Metadata.TryGetValue("temporalSemantics", out var temporalSemantics);

        var sourceText = source ?? "source value";
        var targetText = target ?? fact.Name;
        var location = $"`{fact.Source.Path}:L{fact.Source.StartLine}`";

        if (string.Equals(temporalSemantics, "dynamic", StringComparison.Ordinal))
        {
            fact.Metadata.TryGetValue("getterOccurrence", out var getterOccurrence);
            var getterProof = string.IsNullOrWhiteSpace(getterOccurrence)
                ? string.Empty
                : $" The modeled getter reads `{getterOccurrence}`.";
            return $"Dynamic read-time dependency: `{sourceText}` → `{targetText}` at {location}.{getterProof} Reading `{targetText}` resolves the upstream value through the retained reference at read time, so a later change to `{sourceText}` can affect a later read without another scalar-copy assignment.";
        }

        if (string.Equals(compositionStatus, "blocked-by-intervening-or-unproven-write", StringComparison.Ordinal))
        {
            return $"Immediate stored snapshot copy: `{sourceText}` → `{targetText}` at {location}. This edge is not composed with an earlier `{sourceText}` lineage because the source value could not be proven unchanged before this assignment.";
        }

        return $"Stored snapshot copy: `{sourceText}` → `{targetText}` at {location}. For this observed direct scalar auto-property assignment, changing `{sourceText}` later does not automatically update the stored target without another write.";
    }

    private static string DescribeValueTransferEvidence(EvidenceFact fact)
    {
        fact.Metadata.TryGetValue("sourceOccurrence", out var source);
        fact.Metadata.TryGetValue("targetOccurrence", out var target);
        fact.Metadata.TryGetValue("mechanism", out var mechanism);
        fact.Metadata.TryGetValue("temporalSemantics", out var temporal);
        fact.Metadata.TryGetValue("semanticProject", out var project);

        if (string.Equals(temporal, "dynamic", StringComparison.Ordinal))
        {
            return $"Value lineage dynamic dependency: `{source ?? "source"}` → `{target ?? fact.Name}` ({mechanism ?? "reference"}; dynamic read-time semantics; target-project proof `{project ?? "unknown"}`).";
        }

        return $"Value lineage transfer: `{source ?? "source"}` → `{target ?? fact.Name}` ({mechanism ?? "transfer"}; {temporal ?? "temporal semantics unknown"}; target-project proof `{project ?? "unknown"}`).";
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
