using Pkc.Core;

namespace Pkc.Frontend;

internal static class FrontendRelationLinker
{
    public static FactDocument Link(FactDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var relations = document.Relations.ToList();
        var seen = relations
            .Select(RelationKey)
            .ToHashSet(StringComparer.Ordinal);

        var apiCallsByHandler = document.Facts
            .Where(fact => fact.Kind == "ui-api-call" && !string.IsNullOrWhiteSpace(fact.Container))
            .GroupBy(fact => fact.Container!, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);

        foreach (var action in document.Facts.Where(fact => fact.Kind == "ui-action"))
        {
            if (!action.Metadata.TryGetValue("handler", out var handler) ||
                string.IsNullOrWhiteSpace(handler) ||
                !apiCallsByHandler.TryGetValue(handler, out var apiCalls))
            {
                continue;
            }

            var sameFramework = apiCalls
                .Where(apiCall => SameFramework(action, apiCall))
                .ToArray();

            if (sameFramework.Length != 1)
            {
                continue;
            }

            var relation = new EvidenceRelation(action.Id, "triggers-api", sameFramework[0].Id, action.Source);
            if (seen.Add(RelationKey(relation)))
            {
                relations.Add(relation);
            }
        }

        return new FactDocument(
            document.SchemaVersion,
            document.Facts,
            relations
                .OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
                .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
                .ThenBy(relation => relation.Target, StringComparer.Ordinal)
                .ToArray());
    }

    private static bool SameFramework(EvidenceFact left, EvidenceFact right)
    {
        var hasLeft = left.Metadata.TryGetValue("framework", out var leftFramework);
        var hasRight = right.Metadata.TryGetValue("framework", out var rightFramework);

        return !hasLeft || !hasRight || string.Equals(leftFramework, rightFramework, StringComparison.Ordinal);
    }

    private static string RelationKey(EvidenceRelation relation) =>
        $"{relation.FromFactId}|{relation.Kind}|{relation.Target}|{relation.Source.Path}|{relation.Source.StartLine}";
}
