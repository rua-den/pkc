using Pkc.Core;

namespace Pkc.Knowledge;

public sealed class GroundedKnowledgeSynthesizer : IKnowledgeSynthesizer
{
    private static readonly string[] TrustedPublicationPrefixes = ["Publish", "Emit", "Enqueue", "Produce"];

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
        var stateChanges = BuildStateChanges(candidate, factsById, endpoint);
        var sideEffects = BuildSideEffects(candidate);
        var flow = BuildFlow(candidate, factsById);
        var evidence = BuildEvidence(candidate, factsById, endpoint);
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
        var conditions = candidate.Relations
            .Where(relation => relation.Kind == "contains-condition")
            .Select(relation => factsById.TryGetValue(relation.Target, out var fact) ? fact : null)
            .Where(fact => fact is not null)
            .Cast<EvidenceFact>()
            .ToArray();

        var throws = candidate.Relations
            .Where(relation => relation.Kind == "throws")
            .Select(relation => factsById.TryGetValue(relation.Target, out var fact) ? fact : null)
            .Where(fact => fact is not null)
            .Cast<EvidenceFact>()
            .ToArray();

        var throwOwner = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var throwFact in throws)
        {
            var owner = conditions
                .Where(condition => Contains(condition.Source, throwFact.Source))
                .OrderBy(condition => condition.Source.EndLine - condition.Source.StartLine)
                .ThenByDescending(condition => condition.Source.StartLine)
                .FirstOrDefault();

            if (owner is not null)
            {
                throwOwner[throwFact.Id] = owner.Id;
            }
        }

        foreach (var condition in conditions)
        {
            if (!condition.Metadata.TryGetValue("expression", out var expression))
            {
                continue;
            }

            var ownedThrows = throws
                .Where(throwFact => throwOwner.TryGetValue(throwFact.Id, out var ownerId) && ownerId == condition.Id)
                .ToArray();

            if (ownedThrows.Length == 0)
            {
                rules.Add($"Condition observed: `{expression}`.");
                continue;
            }

            foreach (var throwFact in ownedThrows)
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

    private static bool Contains(SourceLocation parent, SourceLocation child) =>
        string.Equals(parent.Path, child.Path, StringComparison.Ordinal) &&
        parent.StartLine <= child.StartLine &&
        parent.EndLine >= child.EndLine;

    private static IReadOnlyList<string> BuildStateChanges(
        FeatureCandidate candidate,
        IReadOnlyDictionary<string, EvidenceFact> factsById,
        EvidenceFact? endpoint) =>
        candidate.Facts
            .Where(fact => IsKnowledgeMutation(candidate, factsById, fact, endpoint))
            .Select(DescribeMutation)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    private static string DescribeMutation(EvidenceFact fact)
    {
        fact.Metadata.TryGetValue("target", out var target);
        fact.Metadata.TryGetValue("value", out var value);
        fact.Metadata.TryGetValue("operator", out var mutationOperator);
        var displayTarget = target ?? fact.Name;

        return mutationOperator switch
        {
            "+=" when !string.IsNullOrWhiteSpace(value) => $"Applies `+=` to `{displayTarget}` with `{value}`.",
            "-=" when !string.IsNullOrWhiteSpace(value) => $"Applies `-=` to `{displayTarget}` with `{value}`.",
            "*=" when !string.IsNullOrWhiteSpace(value) => $"Applies `*=` to `{displayTarget}` with `{value}`.",
            "/=" when !string.IsNullOrWhiteSpace(value) => $"Applies `/=` to `{displayTarget}` with `{value}`.",
            "%=" when !string.IsNullOrWhiteSpace(value) => $"Applies `%=` to `{displayTarget}` with `{value}`.",
            string op when op?.Contains("Increment", StringComparison.Ordinal) == true => $"Increments `{displayTarget}` by 1.",
            string op when op?.Contains("Decrement", StringComparison.Ordinal) == true => $"Decrements `{displayTarget}` by 1.",
            _ when string.IsNullOrWhiteSpace(value) => $"Mutates `{displayTarget}`.",
            _ => $"Sets `{displayTarget}` to `{value}`."
        };
    }

    private static IReadOnlyList<string> BuildSideEffects(FeatureCandidate candidate) =>
        candidate.Relations
            .Where(relation => relation.Kind == "message-publication-candidate")
            .Where(relation => IsTrustedPublicationTarget(relation.Target))
            .Select(relation => $"Calls publication-like method `{relation.Target}`.")
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    private static bool IsTrustedPublicationTarget(string target)
    {
        var methodName = target.Split('.').LastOrDefault() ?? target;
        return TrustedPublicationPrefixes.Any(prefix => methodName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

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

    private static IReadOnlyList<KnowledgeEvidence> BuildEvidence(
        FeatureCandidate candidate,
        IReadOnlyDictionary<string, EvidenceFact> factsById,
        EvidenceFact? endpoint) =>
        candidate.Facts
            .Where(fact =>
                fact.Kind is "endpoint" or "method" or "condition" or "throw" or "ui-screen" or "ui-route" or "ui-action" or "ui-api-call" ||
                IsKnowledgeMutation(candidate, factsById, fact, endpoint))
            .Select(fact => new KnowledgeEvidence(
                fact.Id,
                fact.Kind,
                DescribeFact(fact),
                fact.Source))
            .OrderBy(item => item.Source.Path, StringComparer.Ordinal)
            .ThenBy(item => item.Source.StartLine)
            .ToArray();

    private static bool IsKnowledgeMutation(
        FeatureCandidate candidate,
        IReadOnlyDictionary<string, EvidenceFact> factsById,
        EvidenceFact fact,
        EvidenceFact? endpoint)
    {
        if (fact.Kind != "mutation")
        {
            return false;
        }

        fact.Metadata.TryGetValue("target", out var target);
        var semanticCandidate = fact.Metadata.TryGetValue("stateMutationCandidate", out var stateCandidate) && stateCandidate == "true";
        var syntacticMemberCandidate = !string.IsNullOrWhiteSpace(target) && target.Contains('.', StringComparison.Ordinal);
        if (!semanticCandidate && !syntacticMemberCandidate)
        {
            return false;
        }

        if (IsObjectInitializerLikeMutation(candidate, factsById, fact) || IsImplementationCounterMutation(fact))
        {
            return false;
        }

        if (endpoint is null || !string.Equals(fact.Container, endpoint.Name, StringComparison.Ordinal))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(target) || target.Contains('.', StringComparison.Ordinal))
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

    private static bool IsImplementationCounterMutation(EvidenceFact mutation)
    {
        if (!mutation.Metadata.TryGetValue("target", out var target) ||
            !mutation.Metadata.TryGetValue("operator", out var mutationOperator))
        {
            return false;
        }

        return target.StartsWith("_next", StringComparison.OrdinalIgnoreCase) &&
               mutationOperator.Contains("Increment", StringComparison.Ordinal);
    }

    private static bool IsObjectInitializerLikeMutation(
        FeatureCandidate candidate,
        IReadOnlyDictionary<string, EvidenceFact> factsById,
        EvidenceFact mutation)
    {
        if (!mutation.Metadata.TryGetValue("target", out var target) || target.Contains('.', StringComparison.Ordinal) ||
            !mutation.Metadata.TryGetValue("targetSymbol", out var targetSymbol))
        {
            return false;
        }

        var sourceRelation = candidate.Relations.FirstOrDefault(relation =>
            relation.Kind == "mutates" && relation.Target == mutation.Id);
        if (sourceRelation is null || !factsById.TryGetValue(sourceRelation.FromFactId, out var sourceMethod) ||
            string.IsNullOrWhiteSpace(sourceMethod.Container))
        {
            return false;
        }

        var lastDot = targetSymbol.LastIndexOf('.');
        if (lastDot <= 0)
        {
            return false;
        }

        var targetOwner = targetSymbol[..lastDot];
        return !string.Equals(targetOwner, sourceMethod.Container, StringComparison.Ordinal);
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
