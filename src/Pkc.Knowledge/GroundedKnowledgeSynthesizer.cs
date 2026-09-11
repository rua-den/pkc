using Pkc.Core;

namespace Pkc.Knowledge;

public sealed class GroundedKnowledgeSynthesizer : IKnowledgeSynthesizer
{
    public ValueTask<FeatureKnowledge> SynthesizeAsync(
        FeatureCandidate candidate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        cancellationToken.ThrowIfCancellationRequested();

        var factsById = candidate.Facts.ToDictionary(fact => fact.Id, StringComparer.Ordinal);
        factsById.TryGetValue(candidate.SeedFactId, out var endpoint);

        var uiSteps = BuildUiSteps(candidate);
        var uiToBackend = BuildUiToBackend(candidate);
        var entryPoints = BuildEntryPoints(endpoint);
        var permissions = BuildPermissions(candidate, endpoint);
        var rules = BuildRules(candidate, factsById);
        var stateChanges = BuildStateChanges(candidate, endpoint);
        var sideEffects = BuildSideEffects(candidate);
        var flow = BuildFlow(candidate, factsById);
        var evidence = BuildEvidence(candidate, endpoint);
        var unknowns = candidate.Unknowns.Select(FriendlyUnknown).ToArray();

        var action = endpoint?.Name ?? candidate.Name;
        var summary = candidate.Coverage.Contains("frontend-static", StringComparer.Ordinal)
            ? $"Observed UI and backend behavior for the {action} action in {candidate.Area}."
            : $"Code-observed backend behavior for the {action} action in {candidate.Area}.";

        var knowledge = new FeatureKnowledge(
            candidate.Id,
            candidate.Name,
            candidate.Area,
            "code-observed",
            candidate.Coverage,
            summary,
            uiSteps,
            uiToBackend,
            entryPoints,
            permissions,
            rules,
            stateChanges,
            sideEffects,
            flow,
            unknowns,
            evidence);

        return ValueTask.FromResult(knowledge);
    }

    private static IReadOnlyList<string> BuildUiSteps(FeatureCandidate candidate)
    {
        var result = new List<string>();

        foreach (var route in candidate.Facts.Where(fact => fact.Kind == "ui-route"))
        {
            if (route.Metadata.TryGetValue("path", out var path))
            {
                result.Add($"Open route `{path}`.");
            }
        }

        foreach (var action in candidate.Facts.Where(fact => fact.Kind == "ui-action"))
        {
            action.Metadata.TryGetValue("label", out var label);
            action.Metadata.TryGetValue("permission", out var permission);
            var screen = string.IsNullOrWhiteSpace(action.Container) ? string.Empty : $" on `{action.Container}`";
            var permissionNote = string.IsNullOrWhiteSpace(permission) ? string.Empty : $"; UI guard: `{permission}`";
            result.Add($"Click `{label ?? action.Name}`{screen}{permissionNote}.");
        }

        return result.Distinct(StringComparer.Ordinal).ToArray();
    }

    private static IReadOnlyList<string> BuildUiToBackend(FeatureCandidate candidate) =>
        candidate.Facts
            .Where(fact => fact.Kind == "ui-api-call")
            .Select(fact =>
            {
                fact.Metadata.TryGetValue("httpMethod", out var method);
                fact.Metadata.TryGetValue("url", out var url);
                return $"UI sends `{method ?? "HTTP"} {url ?? fact.Name}`.";
            })
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    private static IReadOnlyList<string> BuildEntryPoints(EvidenceFact? endpoint)
    {
        if (endpoint is null)
        {
            return [];
        }

        endpoint.Metadata.TryGetValue("httpMethod", out var method);
        endpoint.Metadata.TryGetValue("fullRoute", out var route);

        if (string.IsNullOrWhiteSpace(method) && string.IsNullOrWhiteSpace(route))
        {
            return [];
        }

        var normalizedRoute = string.IsNullOrWhiteSpace(route)
            ? string.Empty
            : $"/{route.Trim('/')}";

        return [$"{method ?? "HTTP"} {normalizedRoute}".Trim()];
    }

    private static IReadOnlyList<string> BuildPermissions(FeatureCandidate candidate, EvidenceFact? endpoint)
    {
        var permissions = new List<string>();

        if (endpoint is not null)
        {
            if (endpoint.Metadata.TryGetValue("authorizationPolicies", out var policies))
            {
                permissions.AddRange(SplitMetadataList(policies).Select(policy => $"Policy: {policy}"));
            }

            if (endpoint.Metadata.TryGetValue("authorizationRoles", out var roles))
            {
                permissions.AddRange(SplitMetadataList(roles).Select(role => $"Role: {role}"));
            }

            if (permissions.Count == 0 && endpoint.Metadata.TryGetValue("authorization", out var authorization))
            {
                permissions.Add($"Authorization required: {authorization}");
            }
        }

        permissions.AddRange(candidate.Facts
            .Where(fact => fact.Kind == "ui-action")
            .Select(fact => fact.Metadata.TryGetValue("permission", out var permission) ? permission : null)
            .Where(permission => !string.IsNullOrWhiteSpace(permission))
            .Select(permission => $"UI visibility guard: {permission}"));

        return permissions.Distinct(StringComparer.Ordinal).ToArray();
    }

    private static IReadOnlyList<string> BuildRules(
        FeatureCandidate candidate,
        IReadOnlyDictionary<string, EvidenceFact> factsById)
    {
        var rules = new List<string>();
        var conditions = candidate.Relations.Where(relation => relation.Kind == "contains-condition");

        foreach (var conditionRelation in conditions)
        {
            if (!factsById.TryGetValue(conditionRelation.Target, out var condition) ||
                !condition.Metadata.TryGetValue("expression", out var expression))
            {
                continue;
            }

            var throwFacts = candidate.Relations
                .Where(relation => relation.FromFactId == conditionRelation.FromFactId && relation.Kind == "throws")
                .Select(relation => factsById.TryGetValue(relation.Target, out var fact) ? fact : null)
                .Where(fact => fact is not null)
                .Cast<EvidenceFact>()
                .ToArray();

            if (throwFacts.Length == 0)
            {
                rules.Add($"Condition observed: `{expression}`.");
                continue;
            }

            foreach (var throwFact in throwFacts)
            {
                throwFact.Metadata.TryGetValue("exceptionType", out var exceptionType);
                throwFact.Metadata.TryGetValue("expression", out var throwExpression);
                var exception = string.IsNullOrWhiteSpace(exceptionType) ? "an exception" : $"`{exceptionType}`";
                var detail = string.IsNullOrWhiteSpace(throwExpression) ? string.Empty : $" using `{throwExpression}`";
                rules.Add($"When `{expression}`, the implementation throws {exception}{detail}.");
            }
        }

        return rules.Distinct(StringComparer.Ordinal).ToArray();
    }

    private static IReadOnlyList<string> BuildStateChanges(FeatureCandidate candidate, EvidenceFact? endpoint) =>
        candidate.Facts
            .Where(fact => IsKnowledgeMutation(fact, endpoint))
            .Select(fact =>
            {
                fact.Metadata.TryGetValue("target", out var target);
                fact.Metadata.TryGetValue("value", out var value);
                return string.IsNullOrWhiteSpace(value)
                    ? $"Mutates `{target ?? fact.Name}`."
                    : $"Sets `{target ?? fact.Name}` to `{value}`.";
            })
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    private static IReadOnlyList<string> BuildSideEffects(FeatureCandidate candidate) =>
        candidate.Relations
            .Where(relation => relation.Kind == "message-publication-candidate")
            .Select(relation => $"Calls publication-like method `{relation.Target}`.")
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    private static IReadOnlyList<string> BuildFlow(
        FeatureCandidate candidate,
        IReadOnlyDictionary<string, EvidenceFact> factsById) =>
        candidate.Relations
            .Where(relation => relation.Kind == "invokes")
            .Select(relation =>
            {
                var source = factsById.TryGetValue(relation.FromFactId, out var sourceFact)
                    ? DisplayFact(sourceFact)
                    : relation.FromFactId;
                return $"{source} → {relation.Target}";
            })
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    private static IReadOnlyList<KnowledgeEvidence> BuildEvidence(FeatureCandidate candidate, EvidenceFact? endpoint) =>
        candidate.Facts
            .Where(fact =>
                fact.Kind is "endpoint" or "method" or "condition" or "throw" or "ui-screen" or "ui-route" or "ui-action" or "ui-api-call" ||
                IsKnowledgeMutation(fact, endpoint))
            .Select(fact => new KnowledgeEvidence(
                fact.Id,
                fact.Kind,
                DescribeFact(fact),
                fact.Source))
            .OrderBy(item => item.Source.Path, StringComparer.Ordinal)
            .ThenBy(item => item.Source.StartLine)
            .ToArray();

    private static bool IsKnowledgeMutation(EvidenceFact fact, EvidenceFact? endpoint)
    {
        if (fact.Kind != "mutation" ||
            !fact.Metadata.TryGetValue("stateMutationCandidate", out var stateCandidate) ||
            stateCandidate != "true")
        {
            return false;
        }

        if (endpoint is null || !string.Equals(fact.Container, endpoint.Name, StringComparison.Ordinal))
        {
            return true;
        }

        if (!fact.Metadata.TryGetValue("target", out var target) || target.Contains('.', StringComparison.Ordinal))
        {
            return true;
        }

        if (!fact.Metadata.TryGetValue("value", out var value) ||
            !endpoint.Metadata.TryGetValue("parameters", out var parameters))
        {
            return true;
        }

        return !parameters.Contains(value, StringComparison.Ordinal);
    }

    private static string DescribeFact(EvidenceFact fact) => fact.Kind switch
    {
        "endpoint" => $"Endpoint {DisplayFact(fact)}",
        "method" => $"Method {DisplayFact(fact)}",
        "condition" when fact.Metadata.TryGetValue("expression", out var expression) => $"Condition: {expression}",
        "throw" when fact.Metadata.TryGetValue("exceptionType", out var type) => $"Throws {type}",
        "mutation" when fact.Metadata.TryGetValue("target", out var target) => $"Mutation: {target}",
        "ui-screen" => $"UI screen/component {fact.Name}",
        "ui-route" when fact.Metadata.TryGetValue("path", out var path) => $"UI route {path}",
        "ui-action" => $"UI action {fact.Name}",
        "ui-api-call" when fact.Metadata.TryGetValue("httpMethod", out var method) && fact.Metadata.TryGetValue("url", out var url) => $"UI API call {method} {url}",
        _ => $"{fact.Kind}: {fact.Name}"
    };

    private static string DisplayFact(EvidenceFact fact) =>
        string.IsNullOrWhiteSpace(fact.Container)
            ? fact.Name
            : $"{fact.Container}.{fact.Name}";

    private static IEnumerable<string> SplitMetadataList(string value) =>
        value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static string FriendlyUnknown(string unknown) => unknown switch
    {
        "frontend-ui-not-analyzed" => "Frontend/UI entry point and user interaction path have not been analyzed yet.",
        "delivery-history-not-analyzed" => "Azure DevOps delivery history and product intent have not been analyzed yet.",
        _ => unknown
    };
}
