using System.Text.RegularExpressions;
using Pkc.Core;

namespace Pkc.CSharp;

internal sealed class CSharpValidationEvidenceEnricher
{
    private static readonly Regex MissingStringCallRegex = new(
        @"(?:string|System\.String)\.IsNullOr(?:WhiteSpace|Empty)\s*\(\s*(?<target>[A-Za-z_$][A-Za-z0-9_$.]*)\s*\)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex MissingNullComparisonRegex = new(
        @"(?:(?<target>[A-Za-z_$][A-Za-z0-9_$.]*)\s*(?:==|is)\s*null\b|null\s*==\s*(?<target2>[A-Za-z_$][A-Za-z0-9_$.]*))",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public FactDocument Enrich(FactDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var facts = document.Facts.ToList();
        var relations = document.Relations.ToList();
        var factIds = facts.Select(fact => fact.Id).ToHashSet(StringComparer.Ordinal);
        var relationKeys = relations.Select(RelationKey).ToHashSet(StringComparer.Ordinal);

        AddDataAnnotationRequiredFacts(document, facts, relations, factIds, relationKeys);
        AddGuardRequiredFacts(document, facts, relations, factIds, relationKeys);

        return new FactDocument(
            "0.4.4-csharp-validation",
            facts.OrderBy(fact => fact.Id, StringComparer.Ordinal).ToArray(),
            relations
                .OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
                .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
                .ThenBy(relation => relation.Target, StringComparer.Ordinal)
                .ToArray());
    }

    private static void AddDataAnnotationRequiredFacts(
        FactDocument document,
        ICollection<EvidenceFact> facts,
        ICollection<EvidenceRelation> relations,
        ISet<string> factIds,
        ISet<string> relationKeys)
    {
        var callables = document.Facts
            .Where(fact => fact.Kind is "endpoint" or "method")
            .Where(fact => fact.Metadata.ContainsKey("parameters"))
            .ToArray();

        foreach (var property in document.Facts.Where(fact => fact.Kind == "property"))
        {
            if (!property.Attributes.Any(IsRequiredAttribute) || string.IsNullOrWhiteSpace(property.Container))
            {
                continue;
            }

            var requestType = property.Container!;
            var simpleType = requestType.Split('.').Last();
            var validation = CreateValidationFact(
                $"{property.Id}:backend-validation:required",
                property.Name,
                requestType,
                property.Source,
                "data-annotation",
                condition: null,
                targetExpression: property.Name,
                property.Metadata);
            AddFact(validation, facts, factIds);

            foreach (var callable in callables)
            {
                var parameters = callable.Metadata["parameters"];
                if (!ContainsIdentifier(parameters, simpleType))
                {
                    continue;
                }

                AddRelation(
                    new EvidenceRelation(callable.Id, "validates-field", validation.Id, validation.Source),
                    relations,
                    relationKeys);
            }
        }
    }

    private static void AddGuardRequiredFacts(
        FactDocument document,
        ICollection<EvidenceFact> facts,
        ICollection<EvidenceRelation> relations,
        ISet<string> factIds,
        ISet<string> relationKeys)
    {
        var ownersByCondition = document.Relations
            .Where(relation => relation.Kind == "contains-condition")
            .GroupBy(relation => relation.Target, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);

        foreach (var condition in document.Facts.Where(fact => fact.Kind == "condition"))
        {
            if (!condition.Metadata.TryGetValue("expression", out var expression) ||
                string.IsNullOrWhiteSpace(expression) ||
                !ownersByCondition.TryGetValue(condition.Id, out var owners))
            {
                continue;
            }

            foreach (var target in ExtractMissingTargets(expression))
            {
                var field = target.Split('.').Last();
                if (string.IsNullOrWhiteSpace(field))
                {
                    continue;
                }

                var validation = CreateValidationFact(
                    $"{condition.Id}:backend-validation:{field.ToLowerInvariant()}",
                    field,
                    requestType: null,
                    condition.Source,
                    "guard",
                    expression,
                    target,
                    condition.Metadata);
                AddFact(validation, facts, factIds);

                foreach (var owner in owners)
                {
                    AddRelation(
                        new EvidenceRelation(owner.FromFactId, "validates-field", validation.Id, validation.Source),
                        relations,
                        relationKeys);
                }
            }
        }
    }

    private static IEnumerable<string> ExtractMissingTargets(string expression)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (Match match in MissingStringCallRegex.Matches(expression))
        {
            var target = match.Groups["target"].Value;
            if (seen.Add(target))
            {
                yield return target;
            }
        }

        foreach (Match match in MissingNullComparisonRegex.Matches(expression))
        {
            var target = match.Groups["target"].Success
                ? match.Groups["target"].Value
                : match.Groups["target2"].Value;
            if (!string.IsNullOrWhiteSpace(target) && seen.Add(target))
            {
                yield return target;
            }
        }
    }

    private static EvidenceFact CreateValidationFact(
        string id,
        string field,
        string? requestType,
        SourceLocation source,
        string sourceKind,
        string? condition,
        string targetExpression,
        IReadOnlyDictionary<string, string> sourceMetadata)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["field"] = field,
            ["behavior"] = "required",
            ["sourceKind"] = sourceKind,
            ["targetExpression"] = targetExpression,
            ["analysisMode"] = "derived-csharp-validation",
            ["analysisConfidence"] = sourceMetadata.TryGetValue("analysisConfidence", out var confidence)
                ? confidence
                : sourceKind == "data-annotation" ? "high" : "medium"
        };

        if (!string.IsNullOrWhiteSpace(requestType))
        {
            metadata["requestType"] = requestType;
        }

        if (!string.IsNullOrWhiteSpace(condition))
        {
            metadata["condition"] = condition;
        }

        if (sourceMetadata.TryGetValue("analysisMode", out var sourceMode))
        {
            metadata["derivedFromAnalysisMode"] = sourceMode;
        }

        return new EvidenceFact(
            id,
            "backend-field-validation",
            $"{field}:required",
            requestType,
            source,
            [],
            metadata);
    }

    private static bool IsRequiredAttribute(string attribute)
    {
        var name = attribute.Split('(')[0].Trim().Split('.').Last();
        return string.Equals(name, "Required", StringComparison.Ordinal) ||
               string.Equals(name, "RequiredAttribute", StringComparison.Ordinal);
    }

    private static bool ContainsIdentifier(string value, string identifier) =>
        Regex.IsMatch(
            value,
            $@"(?<![A-Za-z0-9_]){Regex.Escape(identifier)}(?![A-Za-z0-9_])",
            RegexOptions.CultureInvariant);

    private static void AddFact(
        EvidenceFact fact,
        ICollection<EvidenceFact> facts,
        ISet<string> ids)
    {
        if (ids.Add(fact.Id))
        {
            facts.Add(fact);
        }
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
        $"{relation.FromFactId}|{relation.Kind}|{relation.Target}|{relation.Source.Path}|{relation.Source.StartLine}";
}
