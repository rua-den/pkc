using Pkc.Core;

namespace Pkc.Knowledge;

public sealed class JointVisibilityCandidateEnricher
{
    public FeatureCandidateDocument Enrich(
        FeatureCandidateDocument candidates,
        FactDocument document)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(document);

        return candidates with
        {
            Candidates = candidates.Candidates
                .Select(candidate => Enrich(candidate, document))
                .ToArray()
        };
    }

    private static FeatureCandidate Enrich(
        FeatureCandidate candidate,
        FactDocument document)
    {
        var renderedTerminals = candidate.Facts
            .Where(fact => fact.Kind == "value-terminal-source")
            .Where(fact => string.Equals(
                fact.Metadata.GetValueOrDefault("boundary"),
                "rendered UI value",
                StringComparison.Ordinal))
            .ToArray();
        if (renderedTerminals.Length == 0)
        {
            return candidate;
        }

        var facts = candidate.Facts.ToDictionary(fact => fact.Id, StringComparer.Ordinal);
        var relations = candidate.Relations.ToList();
        var relationKeys = relations.Select(RelationKey).ToHashSet(StringComparer.Ordinal);

        foreach (var renderedTerminal in renderedTerminals)
        {
            if (!renderedTerminal.Metadata.TryGetValue("frontendRenderFactId", out var renderFactId) ||
                string.IsNullOrWhiteSpace(renderFactId))
            {
                continue;
            }

            var visibilities = document.Facts
                .Where(fact => fact.Kind == "ui-member-visibility")
                .Where(fact => string.Equals(
                    fact.Metadata.GetValueOrDefault("renderFactId"),
                    renderFactId,
                    StringComparison.Ordinal))
                .ToArray();

            foreach (var visibility in visibilities)
            {
                facts[visibility.Id] = visibility;
                AddRelation(
                    new EvidenceRelation(
                        renderFactId,
                        "controlled-by-visibility",
                        visibility.Id,
                        visibility.Source),
                    relations,
                    relationKeys);
            }

            if (visibilities.Length != 1 ||
                !renderedTerminal.Metadata.TryGetValue("backendProjectionFactId", out var projectionFactId) ||
                string.IsNullOrWhiteSpace(projectionFactId))
            {
                continue;
            }

            var projection = document.Facts.SingleOrDefault(fact =>
                fact.Id == projectionFactId &&
                fact.Kind == "value-transfer");
            if (projection is null ||
                !projection.Metadata.TryGetValue("selectionPredicateFactId", out var predicateFactId) ||
                string.IsNullOrWhiteSpace(predicateFactId))
            {
                continue;
            }

            var predicate = document.Facts.SingleOrDefault(fact =>
                fact.Id == predicateFactId &&
                fact.Kind == "business-predicate");
            if (predicate is null ||
                !string.Equals(
                    predicate.Metadata.GetValueOrDefault("selectedApiProjectionFactId"),
                    projection.Id,
                    StringComparison.Ordinal) ||
                !predicate.Metadata.TryGetValue("expression", out var backendCondition) ||
                string.IsNullOrWhiteSpace(backendCondition))
            {
                continue;
            }

            var visibilityFact = visibilities[0];
            if (!visibilityFact.Metadata.TryGetValue("condition", out var frontendCondition) ||
                string.IsNullOrWhiteSpace(frontendCondition))
            {
                continue;
            }

            facts[projection.Id] = projection;
            facts[predicate.Id] = predicate;

            var backendAuthority = predicate.Metadata.GetValueOrDefault("businessRuleAuthority") ?? "unknown";
            var component = renderedTerminal.Container ?? "UI";
            var member = renderedTerminal.Metadata.GetValueOrDefault("frontendComponentMember") ?? "value";
            var componentIdentity = renderedTerminal.Metadata.GetValueOrDefault("frontendComponentIdentity") ?? "unknown";
            var jointId = $"joint-visibility:{renderedTerminal.Id}:{predicate.Id}:{visibilityFact.Id}";
            var joint = new EvidenceFact(
                jointId,
                "joint-visibility",
                $"{component}.{member}",
                component,
                visibilityFact.Source,
                [],
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["knowledgeClass"] = "business-conditions",
                    ["scopeFactId"] = candidate.SeedFactId,
                    ["r79TerminalFactId"] = renderedTerminal.Id,
                    ["backendProjectionFactId"] = projection.Id,
                    ["backendPredicateFactId"] = predicate.Id,
                    ["backendCondition"] = backendCondition,
                    ["backendAuthority"] = backendAuthority,
                    ["frontendVisibilityFactId"] = visibilityFact.Id,
                    ["frontendRenderFactId"] = renderFactId,
                    ["frontendCondition"] = frontendCondition,
                    ["frontendComponentIdentity"] = componentIdentity,
                    ["frontendComponentMember"] = member,
                    ["jointVisibilityAuthority"] = string.Equals(
                        backendAuthority,
                        "observable",
                        StringComparison.Ordinal)
                            ? "observable"
                            : "observed-only",
                    ["analysisMode"] = "exact-r79-selection+bounded-render-visibility",
                    ["analysisConfidence"] = "high",
                    ["proof"] = "exact-selected-api-projection+r79-render-identity+exact-render-visibility"
                });

            facts[joint.Id] = joint;
            AddRelation(
                new EvidenceRelation(
                    predicate.Id,
                    "contributes-backend-visibility",
                    joint.Id,
                    predicate.Source),
                relations,
                relationKeys);
            AddRelation(
                new EvidenceRelation(
                    visibilityFact.Id,
                    "contributes-frontend-visibility",
                    joint.Id,
                    visibilityFact.Source),
                relations,
                relationKeys);
            AddRelation(
                new EvidenceRelation(
                    renderedTerminal.Id,
                    "describes-visibility",
                    joint.Id,
                    renderedTerminal.Source),
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

    private static void AddRelation(
        EvidenceRelation relation,
        ICollection<EvidenceRelation> relations,
        ISet<string> relationKeys)
    {
        if (relationKeys.Add(RelationKey(relation)))
        {
            relations.Add(relation);
        }
    }

    private static string RelationKey(EvidenceRelation relation) =>
        $"{relation.FromFactId}|{relation.Kind}|{relation.Target}";
}
