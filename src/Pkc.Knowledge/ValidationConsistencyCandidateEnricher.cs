using System.Globalization;
using System.Text;
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

    private static readonly Regex PathRegex = new(
        @"^[A-Za-z_$][A-Za-z0-9_$]*(?:\.[A-Za-z_$][A-Za-z0-9_$]*)*$",
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
        var requestParameterNames = ParseParameterNames(endpoint);

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

            var comparison = Compare(uiRequired, backendRequired, requestParameterNames);
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
        IReadOnlyList<EvidenceFact> backendRequired,
        ISet<string> requestParameterNames)
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

        var uiConditions = BuildRequirednessConditionSet(
            uiRequired,
            isBackend: false,
            requestParameterNames);
        var backendConditions = BuildRequirednessConditionSet(
            backendRequired,
            isBackend: true,
            requestParameterNames);

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
        IReadOnlyList<EvidenceFact> facts,
        bool isBackend,
        ISet<string> requestParameterNames)
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

            if (!TryCanonicalizeCondition(
                    fact,
                    condition,
                    isBackend,
                    requestParameterNames,
                    out var key))
            {
                return new RequirednessConditionSet(false, keys);
            }

            keys.Add(key);
        }

        return new RequirednessConditionSet(true, keys);
    }

    private static bool TryCanonicalizeCondition(
        EvidenceFact fact,
        string condition,
        bool isBackend,
        ISet<string> requestParameterNames,
        out string key)
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
            if (!match.Success ||
                !TryParseOperand(match.Groups["left"].Value, out var left) ||
                !TryParseOperand(match.Groups["right"].Value, out var right) ||
                !TryCanonicalizeEquality(
                    fact,
                    left,
                    right,
                    isBackend,
                    requestParameterNames,
                    out var canonicalTerm))
            {
                return false;
            }

            canonicalTerms.Add(canonicalTerm);
        }

        canonicalTerms.Sort(StringComparer.Ordinal);
        key = string.Join("&&", canonicalTerms.Distinct(StringComparer.Ordinal));
        return key.Length > 0;
    }

    private static bool TryParseOperand(string value, out ConditionOperand operand)
    {
        var trimmed = value.Trim();
        if ((trimmed.StartsWith('"') && trimmed.EndsWith('"')) ||
            (trimmed.StartsWith('\'') && trimmed.EndsWith('\'')))
        {
            var literal = trimmed[1..^1];
            if (literal.Contains('\\'))
            {
                operand = default!;
                return false;
            }

            operand = new ConditionOperand(
                ConditionOperandKind.StringLiteral,
                literal,
                []);
            return true;
        }

        if (decimal.TryParse(
                trimmed,
                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out var number))
        {
            operand = new ConditionOperand(
                ConditionOperandKind.NumberLiteral,
                number.ToString("G29", CultureInfo.InvariantCulture),
                []);
            return true;
        }

        if (!PathRegex.IsMatch(trimmed))
        {
            operand = default!;
            return false;
        }

        operand = new ConditionOperand(
            ConditionOperandKind.Path,
            trimmed,
            trimmed.Split('.'));
        return true;
    }

    private static bool TryCanonicalizeEquality(
        EvidenceFact fact,
        ConditionOperand left,
        ConditionOperand right,
        bool isBackend,
        ISet<string> requestParameterNames,
        out string term)
    {
        if (TryCanonicalizeFieldValue(
                fact,
                left,
                right,
                isBackend,
                requestParameterNames,
                out term) ||
            TryCanonicalizeFieldValue(
                fact,
                right,
                left,
                isBackend,
                requestParameterNames,
                out term))
        {
            return true;
        }

        var leftKey = SerializeOperand(left);
        var rightKey = SerializeOperand(right);
        term = string.CompareOrdinal(leftKey, rightKey) <= 0
            ? $"{leftKey}={rightKey}"
            : $"{rightKey}={leftKey}";
        return true;
    }

    private static bool TryCanonicalizeFieldValue(
        EvidenceFact fact,
        ConditionOperand fieldOperand,
        ConditionOperand valueOperand,
        bool isBackend,
        ISet<string> requestParameterNames,
        out string term)
    {
        term = string.Empty;
        if (fieldOperand.Kind != ConditionOperandKind.Path ||
            !TryCanonicalizeFieldPath(
                fact,
                fieldOperand,
                isBackend,
                requestParameterNames,
                out var fieldPath,
                out var fieldTerminal))
        {
            return false;
        }

        string valueKind;
        string value;
        switch (valueOperand.Kind)
        {
            case ConditionOperandKind.StringLiteral:
                valueKind = "symbol";
                value = valueOperand.Value;
                break;
            case ConditionOperandKind.NumberLiteral:
                valueKind = "number";
                value = valueOperand.Value;
                break;
            case ConditionOperandKind.Path when TryCanonicalizeEnumSymbol(
                valueOperand,
                fieldTerminal,
                out var enumSymbol):
                valueKind = "symbol";
                value = enumSymbol;
                break;
            default:
                return false;
        }

        term = $"field:{Encode(fieldPath)}=value:{valueKind}:{Encode(value)}";
        return true;
    }

    private static bool TryCanonicalizeFieldPath(
        EvidenceFact fact,
        ConditionOperand operand,
        bool isBackend,
        ISet<string> requestParameterNames,
        out string fieldPath,
        out string fieldTerminal)
    {
        var segments = operand.Segments.ToList();
        if (segments.Count == 0)
        {
            fieldPath = string.Empty;
            fieldTerminal = string.Empty;
            return false;
        }

        if (isBackend &&
            segments.Count > 1 &&
            requestParameterNames.Contains(segments[0]))
        {
            segments.RemoveAt(0);
        }
        else if (!isBackend && IsAngularFact(fact))
        {
            if (segments.Count > 1 &&
                string.Equals(segments[0], "this", StringComparison.Ordinal))
            {
                segments.RemoveAt(0);
            }

            if (segments.Count == 3 &&
                string.Equals(segments[1], "value", StringComparison.Ordinal))
            {
                segments = [segments[2]];
            }
            else if (segments.Count == 4 &&
                     string.Equals(segments[1], "controls", StringComparison.Ordinal) &&
                     string.Equals(segments[3], "value", StringComparison.Ordinal))
            {
                segments = [segments[2]];
            }
        }

        if (segments.Count == 0)
        {
            fieldPath = string.Empty;
            fieldTerminal = string.Empty;
            return false;
        }

        var normalized = segments.Count == 1
            ? [segments[0].ToLowerInvariant()]
            : segments.ToArray();
        fieldPath = string.Join('.', normalized);
        fieldTerminal = normalized[^1];
        return true;
    }

    private static bool TryCanonicalizeEnumSymbol(
        ConditionOperand operand,
        string fieldTerminal,
        out string symbol)
    {
        symbol = string.Empty;
        if (operand.Kind != ConditionOperandKind.Path ||
            operand.Segments.Count != 2)
        {
            return false;
        }

        var typeName = operand.Segments[0];
        var memberName = operand.Segments[1];
        if (typeName.Length == 0 ||
            memberName.Length == 0 ||
            !char.IsUpper(typeName[0]) ||
            !char.IsUpper(memberName[0]) ||
            !string.Equals(typeName, fieldTerminal, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        symbol = memberName;
        return true;
    }

    private static string SerializeOperand(ConditionOperand operand) => operand.Kind switch
    {
        ConditionOperandKind.StringLiteral => $"string:{Encode(operand.Value)}",
        ConditionOperandKind.NumberLiteral => $"number:{Encode(operand.Value)}",
        ConditionOperandKind.Path => $"path:{Encode(string.Join('.', operand.Segments))}",
        _ => throw new ArgumentOutOfRangeException(nameof(operand.Kind))
    };

    private static string Encode(string value) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(value));

    private static bool IsAngularFact(EvidenceFact fact) =>
        fact.Metadata.TryGetValue("framework", out var framework) &&
        framework.StartsWith("angular", StringComparison.OrdinalIgnoreCase);

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

    private static HashSet<string> ParseParameterNames(EvidenceFact endpoint)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!endpoint.Metadata.TryGetValue("parameters", out var parameters))
        {
            return result;
        }

        foreach (Match match in ParameterRegex.Matches(parameters))
        {
            result.Add(match.Groups["name"].Value);
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

    private enum ConditionOperandKind
    {
        StringLiteral,
        NumberLiteral,
        Path
    }

    private sealed record ConditionOperand(
        ConditionOperandKind Kind,
        string Value,
        IReadOnlyList<string> Segments);

    private sealed record RequirednessConditionSet(
        bool IsProven,
        HashSet<string> Keys);

    private sealed record ValidationComparison(
        string Status,
        string Reason,
        string? UiCondition,
        string? BackendCondition);
}
