using System.Globalization;
using Pkc.Core;

namespace Pkc.CSharp;

internal sealed class CSharpBehaviorFactCollisionDisambiguator
{
    internal const string MutationCollisionOrdinalMetadata = "mutationCollisionOrdinal";

    public FactDocument Disambiguate(FactDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var collisionIds = document.Facts
            .Where(fact => fact.Kind == "mutation")
            .GroupBy(fact => fact.Id, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToHashSet(StringComparer.Ordinal);

        if (collisionIds.Count == 0)
        {
            return document;
        }

        var occurrenceById = new Dictionary<string, int>(StringComparer.Ordinal);
        var occurrenceByFingerprint = new Dictionary<string, int>(StringComparer.Ordinal);
        var replacementIds = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        var facts = new List<EvidenceFact>(document.Facts.Count);

        foreach (var fact in document.Facts)
        {
            if (fact.Kind != "mutation" || !collisionIds.Contains(fact.Id))
            {
                facts.Add(fact);
                continue;
            }

            var occurrence = NextOccurrence(occurrenceById, fact.Id);
            var fingerprint = MutationFingerprint(fact);
            var fingerprintOccurrence = NextOccurrence(occurrenceByFingerprint, fingerprint);
            var metadata = new Dictionary<string, string>(fact.Metadata, StringComparer.Ordinal)
            {
                [MutationCollisionOrdinalMetadata] = fingerprintOccurrence.ToString(CultureInfo.InvariantCulture)
            };
            var replacementId = $"{fact.Id}:occurrence:{occurrence + 1}";

            facts.Add(fact with
            {
                Id = replacementId,
                Metadata = metadata
            });

            if (!replacementIds.TryGetValue(fact.Id, out var ids))
            {
                ids = [];
                replacementIds[fact.Id] = ids;
            }

            ids.Add(replacementId);
        }

        var relations = new List<EvidenceRelation>(document.Relations.Count);
        foreach (var relation in document.Relations)
        {
            if (relation.Kind == "mutates" && replacementIds.TryGetValue(relation.Target, out var targets))
            {
                foreach (var target in targets)
                {
                    relations.Add(relation with { Target = target });
                }

                continue;
            }

            relations.Add(relation);
        }

        var distinctRelations = relations
            .GroupBy(
                relation => $"{relation.FromFactId}|{relation.Kind}|{relation.Target}|{relation.Source.Path}|{relation.Source.StartLine}|{relation.Source.EndLine}",
                StringComparer.Ordinal)
            .Select(group => group.First())
            .ToArray();

        return document with
        {
            Facts = facts,
            Relations = distinctRelations
        };
    }

    private static int NextOccurrence(IDictionary<string, int> occurrences, string key)
    {
        if (!occurrences.TryGetValue(key, out var occurrence))
        {
            occurrences[key] = 1;
            return 0;
        }

        occurrences[key] = occurrence + 1;
        return occurrence;
    }

    private static string MutationFingerprint(EvidenceFact fact)
    {
        fact.Metadata.TryGetValue("operator", out var mutationOperator);
        fact.Metadata.TryGetValue("value", out var value);
        return $"{fact.Id}\u001f{mutationOperator}\u001f{value}";
    }
}
