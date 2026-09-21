using Pkc.Core;

namespace Pkc.CSharp;

internal sealed class CSharpSelectedApiPredicateAuthorityEnricher
{
    public FactDocument Enrich(FactDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var facts = document.Facts.ToDictionary(fact => fact.Id, StringComparer.Ordinal);
        var relations = document.Relations.ToList();

        var projections = document.Facts
            .Where(fact => fact.Kind == "value-transfer")
            .Where(fact => string.Equals(
                fact.Metadata.GetValueOrDefault("mechanism"),
                "api-projection",
                StringComparison.Ordinal))
            .Where(fact => fact.Metadata.ContainsKey("selectionInvocationSpanStart"))
            .ToArray();

        foreach (var projection in projections)
        {
            if (!projection.Metadata.TryGetValue("scopeFactId", out var scopeFactId) ||
                string.IsNullOrWhiteSpace(scopeFactId) ||
                !projection.Metadata.TryGetValue("selectionInvocationSpanStart", out var spanText) ||
                !int.TryParse(spanText, out var selectionSpanStart) ||
                !projection.Metadata.TryGetValue("selectionOperation", out var selectionOperation))
            {
                continue;
            }

            var predicates = document.Facts
                .Where(fact => fact.Kind == "business-predicate")
                .Where(fact => string.Equals(fact.Source.Path, projection.Source.Path, StringComparison.Ordinal))
                .Where(fact => string.Equals(
                    fact.Metadata.GetValueOrDefault("operation"),
                    selectionOperation,
                    StringComparison.Ordinal))
                .Where(fact => TryGetInvocationSpanStart(fact, out var spanStart) &&
                               spanStart == selectionSpanStart)
                .Where(fact => document.Relations.Any(relation =>
                    relation.FromFactId == scopeFactId &&
                    relation.Target == fact.Id &&
                    relation.Kind is "contains-condition" or "observes-predicate"))
                .ToArray();

            var terminals = document.Facts
                .Where(fact => fact.Kind == "value-terminal-source")
                .Where(fact => string.Equals(
                    fact.Metadata.GetValueOrDefault("boundary"),
                    "API response field",
                    StringComparison.Ordinal))
                .Where(fact => string.Equals(
                    fact.Metadata.GetValueOrDefault("sourceFactId"),
                    projection.Id,
                    StringComparison.Ordinal))
                .Where(fact => string.Equals(
                    fact.Metadata.GetValueOrDefault("scopeFactId"),
                    scopeFactId,
                    StringComparison.Ordinal))
                .ToArray();

            if (predicates.Length != 1 || terminals.Length != 1)
            {
                continue;
            }

            var predicate = predicates[0];
            var terminal = terminals[0];
            var predicateMetadata = new Dictionary<string, string>(predicate.Metadata, StringComparer.Ordinal)
            {
                ["businessRuleAuthority"] = "observable",
                ["observableContext"] = "selected-api-response-item",
                ["observableEffectResolution"] = "semantic-selection-to-api-projection-path",
                ["selectedApiProjectionFactId"] = projection.Id,
                ["selectedApiTerminalFactId"] = terminal.Id
            };
            facts[predicate.Id] = predicate with { Metadata = predicateMetadata };

            var projectionMetadata = new Dictionary<string, string>(projection.Metadata, StringComparer.Ordinal)
            {
                ["selectionPredicateFactId"] = predicate.Id,
                ["selectionPredicateAuthority"] = "observable"
            };
            facts[projection.Id] = projection with { Metadata = projectionMetadata };

            var terminalMetadata = new Dictionary<string, string>(terminal.Metadata, StringComparer.Ordinal)
            {
                ["selectionPredicateFactId"] = predicate.Id,
                ["selectionPredicateAuthority"] = "observable"
            };
            facts[terminal.Id] = terminal with { Metadata = terminalMetadata };

            for (var index = 0; index < relations.Count; index++)
            {
                var relation = relations[index];
                if (relation.FromFactId == scopeFactId &&
                    relation.Target == predicate.Id &&
                    relation.Kind == "observes-predicate")
                {
                    relations[index] = relation with { Kind = "contains-condition" };
                }
            }

            relations.Add(new EvidenceRelation(
                predicate.Id,
                "selects-api-response-item",
                projection.Id,
                predicate.Source));
        }

        return document with
        {
            Facts = facts.Values.OrderBy(fact => fact.Id, StringComparer.Ordinal).ToArray(),
            Relations = relations
                .GroupBy(RelationKey, StringComparer.Ordinal)
                .Select(group => group.First())
                .OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
                .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
                .ThenBy(relation => relation.Target, StringComparer.Ordinal)
                .ToArray()
        };
    }

    private static bool TryGetInvocationSpanStart(EvidenceFact predicate, out int spanStart)
    {
        spanStart = -1;
        var separator = predicate.Id.LastIndexOf(':');
        return separator >= 0 &&
               separator + 1 < predicate.Id.Length &&
               int.TryParse(predicate.Id[(separator + 1)..], out spanStart);
    }

    private static string RelationKey(EvidenceRelation relation) =>
        $"{relation.FromFactId}|{relation.Kind}|{relation.Target}|{relation.Source.Path}|{relation.Source.StartLine}";
}
