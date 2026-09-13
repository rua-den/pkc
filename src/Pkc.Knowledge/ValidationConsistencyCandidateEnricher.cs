using System.Text.RegularExpressions;
using Pkc.Core;

namespace Pkc.Knowledge;

public sealed class ValidationConsistencyCandidateEnricher
{
    private static readonly Regex ParameterRegex = new(
        @"(?<type>[A-Za-z_][A-Za-z0-9_.<>?\[\]]*)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex EqualityRegex = new(
        @"^\s*(?<left>'[^']*'|""[^""]*""|[A-Za-z_$][A-Za-z0-9_$?.]*|-?[0-9]+(?:\.[0-9]+)?)\s*(?:===|==)\s*(?<right>'[^']*'|""[^""]*""|[A-Za-z_$][A-Za-z0-9_$?.]*|-?[0-9]+(?:\.[0-9]+)?)\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public FeatureCandidateDocument Enrich(
        FeatureCandidateDocument candidates,
        FactDocument document)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(document);

        return candidates with
        {
            Candidates = candidates.Candidates
                .Select(candidate => EnrichCandidate(candidate, document))
                .ToArray()
        };
    }

    private static FeatureCandidate EnrichCandidate(
        FeatureCandidate candidate,
        FactDocument document)
    {
        var endpoint = document.Facts.FirstOrDefault(fact => fact.Id == candidate.SeedFactId);
        if (endpoint is null)
        {
            return candidate;
        }

        var bindings = candidate.Facts
            .Where(fact => fact.Kind == "ui-field-binding")
            .ToArray();
        if (bindings.Length == 0)
        {
            return candidate;
        }

        var facts = candidate.Facts.ToDictionary(fact => fact.Id, StringComparer.Ordinal);
        var relations = candidate.Relations.ToList();
        var relationKeys = relations
            .Select(RelationKey)
            .ToHashSet(StringComparer.Ordinal);
        var requestTypes = ParseParameterTypes(endpoint);

        foreach (var binding in bindings)
        {
            if (!binding.Metadata.TryGetValue("field", out var uiField) ||
                !binding.Metadata.TryGetValue("requestField", out var requestField))
            {
                continue;
            }

            binding.Metadata.TryGetValue("component", out var component);
            var uiRequired = document.Facts
                .Where(fact => fact.Kind == "ui-field-validation")
                .Where(fact => fact.Metadata.TryGetValue("field", out var field) &&
                               string.Equals(field, uiField, StringComparison.OrdinalIgnoreCase))
                .Where(fact => string.IsNullOrWhiteSpace(component) ||
                               string.Equals(fact.Container, component, StringComparison.Ordinal) ||
                               fact.Metadata.TryGetValue("component", out var factComponent) &&
                               string.Equals(factComponent, component, StringComparison.Ordinal))
                .Where(IsRequired)
                .ToArray();

            var backendRequired = document.Facts
                .Where(fact => fact.Kind == "backend-field-validation")
                .Where(fact => fact.Metadata.TryGetValue("field", out var field) &&
                               string.Equals(field, requestField, StringComparison.OrdinalIgnoreCase))
                .Where(IsRequired)
                .Where(fact => BelongsToEndpoint(fact, endpoint, requestTypes))
                .ToArray();

            foreach (var backend in backendRequired)
            {
                facts[backend.Id] = backend;
                AddRelation(
                    new EvidenceRelation(endpoint.Id, "validates-request-field", backend.Id, backend.Source),
                    relations,
                    relationKeys);
            }

            if (uiRequired.Length == 0 && backendRequired.Length == 0)
            {
                continue;
            }

            var comparison = Compare(uiRequired, backendRequired);
            var comparisonFact = CreateComparisonFact(
                binding,
                endpoint,
                uiField,
                requestField,
                uiRequired,
                backendRequired,
                comparison);
            facts[comparisonFact.Id] = comparisonFact;
            AddRelation(
                new EvidenceRelation(binding.Id, "validation-correlates", comparisonFact.Id, binding.Source),
                relations,
                relationKeys);
        }

        return candidate with
        {
            Facts = facts.Values.OrderBy(fact => fact.Id, StringComparer.Ordinal).ToArray(),
            Relations = relations
                .OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
                .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
                .ThenBy(relation => relation.Target, StringComparer.Ordinal)
                .ToArray()
        };
    }

    private static ValidationComparison Compare(
        IReadOnlyList<EvidenceFact> uiRequired,
        IReadOnlyList<EvidenceFact> backendRequired)
    {
        if (backendRequired.Count > 0 && uiRequired.Count == 0)
        {
            return new ValidationComparison(
                "possible-mismatch",
                "backend-required-ui-required-not-observed",
                null,
                PreferredCondition(backendRequired));
        }

        if (uiRequired.Count > 0 && backendRequired.Count == 0)
        {
            return new ValidationComparison(
                "unknown",
                "ui-required-backend-required-not-observed",
                PreferredCondition(uiRequired),
                null);
        }

        var uiConditions = BuildRequirednessConditionSet(uiRequired);
        var backendConditions = BuildRequirednessConditionSet(backendRequired);

        if (!uiConditions.IsProven || !backendConditions.IsProven)
        {
            return new ValidationComparison(
                "possible-mismatch",
                "requiredness-condition-equivalence-unproven",
                PreferredCondition(uiRequired),
                PreferredCondition(backendRequired));
        }

        if (uiConditions.Keys.SetEquals(backendConditions.Keys))
        {
            return new ValidationComparison(
                "consistent",
                uiConditions.Keys.Contains(string.Empty)
                    ? "both-required"
                    : "matching-conditional-requiredness",
                PreferredCondition(uiRequired),
                PreferredCondition(backendRequired));
        }

        return new ValidationComparison(
            "possible-mismatch",
            "requiredness-condition-differs",
            PreferredCondition(uiRequired),
            PreferredCondition(backendRequired));
    }

    private static RequirednessConditionSet BuildRequirednessConditionSet(
        IReadOnlyList<EvidenceFact> facts)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var fact in facts)
        {
            if (!fact.Metadata.TryGetValue("condition", out var condition) ||
                string.IsNullOrWhiteSpace(condition))
            {
                if (fact.Metadata.ContainsKey("conditionKey"))
                {
                    return new RequirednessConditionSet(false, keys);
                }

                return new RequirednessConditionSet(
                    true,
                    new HashSet<string>(StringComparer.Ordinal) { string.Empty });
            }

            if (!TryCanonicalizeCondition(condition, out var key))
            {
                return new RequirednessConditionSet(false, keys);
            }

            keys.Add(key);
        }

        return new RequirednessConditionSet(true, keys);
    }

    private static bool TryCanonicalizeCondition(string condition, out string key)
    {
        key = string.Empty;
        var normalizedCondition = StripOuterParentheses(condition.Trim());
        if (string.IsNullOrWhiteSpace(normalizedCondition) ||
            normalizedCondition.Contains("||", StringComparison.Ordinal) ||
            normalizedCondition.Contains("!=", StringComparison.Ordinal) ||
            normalizedCondition.Contains("!==", StringComparison.Ordinal))
        {
            return false;
        }

        var terms = normalizedCondition
            .Split("&&", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (terms.Length == 0)
        {
            return false;
        }

        var canonicalTerms = new List<string>(terms.Length);
        foreach (var rawTerm in terms)
        {
            var term = StripOuterParentheses(rawTerm.Trim());
            var match = EqualityRegex.Match(term);
            if (!match.Success)
            {
                return false;
            }

            var left = CanonicalOperand(match.Groups["left"].Value);
            var right = CanonicalOperand(match.Groups["right"].Value);
            if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
            {
                return false;
            }

            canonicalTerms.Add(string.CompareOrdinal(left, right) <= 0
                ? $"{left}={right}"
                : $"{right}={left}");
        }

        canonicalTerms.Sort(StringComparer.Ordinal);
        key = string.Join("&&", canonicalTerms.Distinct(StringComparer.Ordinal));
        return key.Length > 0;
    }

    private static string CanonicalOperand(string value)
    {
        var trimmed = value.Trim();
        if ((trimmed.StartsWith('"') && trimmed.EndsWith('"')) ||
            (trimmed.StartsWith('\'') && trimmed.EndsWith('\'')))
        {
            return NormalizeToken(trimmed[1..^1]);
        }

        var normalized = trimmed.Replace("?.", ".", StringComparison.Ordinal);
        var terminal = normalized.Split('.').LastOrDefault() ?? normalized;
        return NormalizeToken(terminal);
    }

    private static string StripOuterParentheses(string value)
    {
        var result = value.Trim();
        while (result.Length >= 2 && result[0] == '(' && result[^1] == ')' && HasSingleOuterPair(result))
        {
            result = result[1..^1].Trim();
        }

        return result;
    }

    private static bool HasSingleOuterPair(string value)
    {
        var depth = 0;
        for (var index = 0; index < value.Length; index++)
        {
            if (value[index] == '(') depth++;
            else if (value[index] == ')') depth--;

            if (depth == 0 && index < value.Length - 1)
            {
                return false;
            }

            if (depth < 0)
            {
                return false;
            }
        }

        return depth == 0;
    }

    private static bool BelongsToEndpoint(
        EvidenceFact validation,
        EvidenceFact endpoint,
        ISet<string> requestTypes)
    {
        if (validation.Metadata.TryGetValue("method", out var method) &&
            string.Equals(method, endpoint.Name, StringComparison.Ordinal))
        {
            return true;
        }

        return validation.Metadata.TryGetValue("declaringType", out var declaringType) &&
               requestTypes.Contains(SimpleTypeName(declaringType));
    }

    private static HashSet<string> ParseParameterTypes(EvidenceFact endpoint)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!endpoint.Metadata.TryGetValue("parameters", out var parameters))
        {
            return result;
        }

        foreach (Match match in ParameterRegex.Matches(parameters))
        {
            result.Add(SimpleTypeName(match.Groups["type"].Value));
        }

        return result;
    }

    private static string SimpleTypeName(string value)
    {
        var normalized = value.Trim().TrimEnd('?');
        var generic = normalized.IndexOf('<');
        if (generic >= 0)
        {
            normalized = normalized[..generic];
        }

        return normalized.Split('.').LastOrDefault() ?? normalized;
    }

    private static bool IsRequired(EvidenceFact fact) =>
        fact.Metadata.TryGetValue("behavior", out var behavior) &&
        string.Equals(behavior, "required", StringComparison.OrdinalIgnoreCase);

    private static string? PreferredCondition(IReadOnlyList<EvidenceFact> facts) =>
        facts.Select(fact => fact.Metadata.TryGetValue("condition", out var condition) ? condition : null)
            .FirstOrDefault(condition => !string.IsNullOrWhiteSpace(condition));

    private static string NormalizeToken(string value) =>
        new(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());

    private static EvidenceFact CreateComparisonFact(
        EvidenceFact binding,
        EvidenceFact endpoint,
        string uiField,
        string requestField,
        IReadOnlyList<EvidenceFact> uiRequired,
        IReadOnlyList<EvidenceFact> backendRequired,
        ValidationComparison comparison)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["uiField"] = uiField,
            ["requestField"] = requestField,
            ["backendField"] = backendRequired.FirstOrDefault()?.Metadata.GetValueOrDefault("field") ?? requestField,
            ["behavior"] = "required",
            ["status"] = comparison.Status,
            ["reason"] = comparison.Reason,
            ["endpoint"] = endpoint.Name,
            ["analysisMode"] = "cross-stack-validation-correlation",
            ["analysisConfidence"] = comparison.Status == "consistent" ? "high" : "medium"
        };

        if (!string.IsNullOrWhiteSpace(comparison.UiCondition))
        {
            metadata["uiCondition"] = comparison.UiCondition;
        }

        if (!string.IsNullOrWhiteSpace(comparison.BackendCondition))
        {
            metadata["backendCondition"] = comparison.BackendCondition;
        }

        metadata["uiEvidenceCount"] = uiRequired.Count.ToString();
        metadata["backendEvidenceCount"] = backendRequired.Count.ToString();

        return new EvidenceFact(
            $"xval:{binding.Source.Path}:{binding.Source.StartLine}:ui-backend-validation:{uiField}->{requestField}",
            "ui-backend-validation",
            $"{uiField}->{requestField}",
            binding.Metadata.GetValueOrDefault("component") ?? binding.Container,
            binding.Source,
            [],
            metadata);
    }

    private static void AddRelation(
        EvidenceRelation relation,
        ICollection<EvidenceRelation> relations,
        ISet<string> keys)
    {
        if (keys.Add(RelationKey(relation)))
        {
            relations.Add(relation);
        }
    }

    private static string RelationKey(EvidenceRelation relation) =>
        $"{relation.FromFactId}|{relation.Kind}|{relation.Target}";

    private sealed record RequirednessConditionSet(
        bool IsProven,
        HashSet<string> Keys);

    private sealed record ValidationComparison(
        string Status,
        string Reason,
        string? UiCondition,
        string? BackendCondition);
}
