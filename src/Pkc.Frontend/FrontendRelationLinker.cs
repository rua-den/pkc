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
            .Where(fact =>
                fact.Kind == "ui-api-call" &&
                !string.IsNullOrWhiteSpace(fact.Container))
            .GroupBy(fact => fact.Container!, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.ToArray(),
                StringComparer.Ordinal);

        foreach (var action in document.Facts.Where(fact => fact.Kind == "ui-action"))
        {
            var candidates = ResolveApiCalls(action, "handler", "calledMethods", apiCallsByHandler)
                .Where(apiCall => SameFramework(action, apiCall))
                .DistinctBy(apiCall => apiCall.Id, StringComparer.Ordinal)
                .ToArray();

            if (candidates.Length != 1)
            {
                continue;
            }

            AddRelation(
                new EvidenceRelation(action.Id, "triggers-api", candidates[0].Id, action.Source),
                relations,
                seen);
        }

        foreach (var screen in document.Facts.Where(fact => fact.Kind == "ui-screen"))
        {
            if (!screen.Metadata.TryGetValue("loadMethods", out var loadMethods) ||
                string.IsNullOrWhiteSpace(loadMethods))
            {
                continue;
            }

            foreach (var methodName in SplitMethods(loadMethods))
            {
                if (!apiCallsByHandler.TryGetValue(methodName, out var apiCalls))
                {
                    continue;
                }

                foreach (var apiCall in apiCalls.Where(apiCall => SameFramework(screen, apiCall)))
                {
                    AddRelation(
                        new EvidenceRelation(screen.Id, "loads-api", apiCall.Id, screen.Source),
                        relations,
                        seen);
                }
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

    private static IEnumerable<EvidenceFact> ResolveApiCalls(
        EvidenceFact action,
        string directKey,
        string calledMethodsKey,
        IReadOnlyDictionary<string, EvidenceFact[]> apiCallsByHandler)
    {
        if (action.Metadata.TryGetValue(directKey, out var handler) &&
            !string.IsNullOrWhiteSpace(handler) &&
            apiCallsByHandler.TryGetValue(handler, out var direct))
        {
            foreach (var apiCall in direct)
            {
                yield return apiCall;
            }
        }

        if (!action.Metadata.TryGetValue(calledMethodsKey, out var calledMethods))
        {
            yield break;
        }

        foreach (var calledMethod in SplitMethods(calledMethods))
        {
            if (!apiCallsByHandler.TryGetValue(calledMethod, out var nested))
            {
                continue;
            }

            foreach (var apiCall in nested)
            {
                yield return apiCall;
            }
        }
    }

    private static IEnumerable<string> SplitMethods(string value) =>
        value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static bool SameFramework(EvidenceFact left, EvidenceFact right)
    {
        var hasLeft = left.Metadata.TryGetValue("framework", out var leftFramework);
        var hasRight = right.Metadata.TryGetValue("framework", out var rightFramework);

        return !hasLeft ||
               !hasRight ||
               string.Equals(leftFramework, rightFramework, StringComparison.Ordinal);
    }

    private static void AddRelation(
        EvidenceRelation relation,
        ICollection<EvidenceRelation> relations,
        ISet<string> seen)
    {
        if (seen.Add(RelationKey(relation)))
        {
            relations.Add(relation);
        }
    }

    private static string RelationKey(EvidenceRelation relation) =>
        $"{relation.FromFactId}|{relation.Kind}|{relation.Target}|{relation.Source.Path}|{relation.Source.StartLine}";
}
