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

        var uniqueOwnersById = document.Facts
            .Where(fact => fact.Kind is "method" or "endpoint" or "constructor")
            .GroupBy(fact => fact.Id, StringComparer.Ordinal)
            .Where(group => group.Count() == 1)
            .ToDictionary(group => group.Key, group => group.Single(), StringComparer.Ordinal);

        var occurrenceById = new Dictionary<string, int>(StringComparer.Ordinal);
        var occurrenceByFingerprint = new Dictionary<string, int>(StringComparer.Ordinal);
        var replacementFacts = new Dictionary<string, List<EvidenceFact>>(StringComparer.Ordinal);
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
            var replacement = fact with
            {
                Id = replacementId,
                Metadata = metadata
            };

            facts.Add(replacement);

            if (!replacementFacts.TryGetValue(fact.Id, out var replacements))
            {
                replacements = [];
                replacementFacts[fact.Id] = replacements;
            }

            replacements.Add(replacement);
        }

        var relations = new List<EvidenceRelation>(document.Relations.Count);
        foreach (var relation in document.Relations)
        {
            if (relation.Kind == "mutates" && replacementFacts.TryGetValue(relation.Target, out var replacements))
            {
                foreach (var target in ReplacementTargetsForRelation(relation, replacements, uniqueOwnersById))
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

    private static IEnumerable<string> ReplacementTargetsForRelation(
        EvidenceRelation relation,
        IReadOnlyList<EvidenceFact> replacements,
        IReadOnlyDictionary<string, EvidenceFact> uniqueOwnersById)
    {
        if (uniqueOwnersById.TryGetValue(relation.FromFactId, out var owner))
        {
            var ownedTargets = replacements
                .Where(fact => string.Equals(fact.Container, owner.Name, StringComparison.Ordinal))
                .Select(fact => fact.Id)
                .ToArray();
            if (ownedTargets.Length > 0)
            {
                return ownedTargets;
            }
        }

        var containers = replacements
            .Select(fact => fact.Container)
            .Distinct(StringComparer.Ordinal)
            .Take(2)
            .ToArray();

        return containers.Length == 1
            ? replacements.Select(fact => fact.Id).ToArray()
            : [];
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
