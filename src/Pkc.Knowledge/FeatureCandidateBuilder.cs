using System.Text.RegularExpressions;
using Pkc.Core;

namespace Pkc.Knowledge;

public sealed partial class FeatureCandidateBuilder
{
    private const int MaxCallDepth = 5;

    private static readonly HashSet<string> BehaviorRelationKinds = new(StringComparer.Ordinal)
    {
        "contains-condition",
        "throws",
        "mutates"
    };

    public FeatureCandidateDocument Build(FactDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var factsById = document.Facts.ToDictionary(fact => fact.Id, StringComparer.Ordinal);
        var relationsBySource = document.Relations
            .GroupBy(relation => relation.FromFactId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);

        var callableByTarget = document.Facts
            .Where(IsCallable)
            .Where(fact => !string.IsNullOrWhiteSpace(fact.Container))
            .GroupBy(fact => $"{fact.Container}.{fact.Name}", StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);

        var candidates = document.Facts
            .Where(fact => fact.Kind == "endpoint")
            .OrderBy(fact => fact.Id, StringComparer.Ordinal)
            .Select(endpoint => BuildCandidate(endpoint, factsById, relationsBySource, callableByTarget))
            .ToArray();

        return new FeatureCandidateDocument("0.1.2", candidates);
    }

    private static FeatureCandidate BuildCandidate(
        EvidenceFact endpoint,
        IReadOnlyDictionary<string, EvidenceFact> factsById,
        IReadOnlyDictionary<string, EvidenceRelation[]> relationsBySource,
        IReadOnlyDictionary<string, EvidenceFact[]> callableByTarget)
    {
        var includedFacts = new Dictionary<string, EvidenceFact>(StringComparer.Ordinal);
        var includedRelations = new List<EvidenceRelation>();
        var seenRelations = new HashSet<string>(StringComparer.Ordinal);
        var queue = new Queue<(EvidenceFact Fact, int Depth)>();
        var expanded = new HashSet<string>(StringComparer.Ordinal);

        queue.Enqueue((endpoint, 0));

        while (queue.Count > 0)
        {
            var (fact, depth) = queue.Dequeue();
            includedFacts[fact.Id] = fact;

            if (!expanded.Add(fact.Id) || !relationsBySource.TryGetValue(fact.Id, out var relations))
            {
                continue;
            }

            foreach (var relation in relations)
            {
                if (BehaviorRelationKinds.Contains(relation.Kind))
                {
                    AddRelation(relation, includedRelations, seenRelations);
                    if (factsById.TryGetValue(relation.Target, out var behaviorFact))
                    {
                        includedFacts[behaviorFact.Id] = behaviorFact;
                    }

                    continue;
                }

                if (relation.Kind == "message-publication-candidate")
                {
                    AddRelation(relation, includedRelations, seenRelations);
                    continue;
                }

                if (relation.Kind != "invokes")
                {
                    continue;
                }

                AddRelation(relation, includedRelations, seenRelations);
                if (depth >= MaxCallDepth || !callableByTarget.TryGetValue(relation.Target, out var targets))
                {
                    continue;
                }

                foreach (var target in targets)
                {
                    queue.Enqueue((target, depth + 1));
                }
            }
        }

        var area = InferArea(endpoint.Container);
        var name = $"{area} {SplitWords(endpoint.Name)}".Trim();
        var id = $"feature:{Slug(area)}:{Slug(endpoint.Name)}";

        return new FeatureCandidate(
            id,
            name,
            area,
            endpoint.Id,
            ["backend-code"],
            ["frontend-ui-not-analyzed", "delivery-history-not-analyzed"],
            includedFacts.Values.OrderBy(fact => fact.Id, StringComparer.Ordinal).ToArray(),
            includedRelations
                .OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
                .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
                .ThenBy(relation => relation.Target, StringComparer.Ordinal)
                .ToArray());
    }

    private static bool IsCallable(EvidenceFact fact) =>
        fact.Kind is "method" or "endpoint" or "constructor";

    private static void AddRelation(
        EvidenceRelation relation,
        ICollection<EvidenceRelation> relations,
        ISet<string> seen)
    {
        var key = $"{relation.FromFactId}|{relation.Kind}|{relation.Target}|{relation.Source.Path}|{relation.Source.StartLine}";
        if (seen.Add(key))
        {
            relations.Add(relation);
        }
    }

    private static string InferArea(string? container)
    {
        var typeName = string.IsNullOrWhiteSpace(container)
            ? "Unknown"
            : container.Split('.').Last();

        return typeName.EndsWith("Controller", StringComparison.Ordinal)
            ? typeName[..^"Controller".Length]
            : typeName;
    }

    private static string SplitWords(string value) =>
        PascalBoundaryRegex().Replace(value, "$1 $2");

    private static string Slug(string value)
    {
        var slug = NonSlugRegex().Replace(value.ToLowerInvariant(), "-").Trim('-');
        return slug.Length == 0 ? "unknown" : slug;
    }

    [GeneratedRegex("([a-z0-9])([A-Z])")]
    private static partial Regex PascalBoundaryRegex();

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonSlugRegex();
}
