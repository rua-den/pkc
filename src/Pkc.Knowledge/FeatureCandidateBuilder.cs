using System.Text.RegularExpressions;
using Pkc.Core;

namespace Pkc.Knowledge;

public sealed partial class FeatureCandidateBuilder
{
    private const int MaxCallDepth = 5;

    private static readonly HashSet<string> BehaviorRelationKinds = new(StringComparer.Ordinal)
    {
        "contains-condition",
        "observes-predicate",
        "contains-configuration",
        "throws",
        "mutates",
        "constructs",
        "contains-loop",
        "handles-exception",
        "returns-response"
    };

    private static readonly Regex TemplateParameterRegex = new(@"\$\{[^}]+\}", RegexOptions.Compiled);
    private static readonly Regex RouteParameterRegex = new(@"\{[^}/]+\}|:[A-Za-z0-9_]+", RegexOptions.Compiled);

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
            .Select(endpoint => BuildCandidate(endpoint, document, factsById, relationsBySource, callableByTarget))
            .ToArray();

        return new FeatureCandidateDocument("0.4.4", candidates);
    }

    private static FeatureCandidate BuildCandidate(
        EvidenceFact endpoint,
        FactDocument document,
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

        AddEndpointContextEvidence(endpoint, document, includedFacts, includedRelations, seenRelations);

        var hasFrontend = AddFrontendEvidence(endpoint, document, includedFacts, includedRelations, seenRelations);
        var area = InferArea(endpoint.Container);
        var name = $"{area} {SplitWords(endpoint.Name)}".Trim();
        var id = $"feature:{Slug(area)}:{Slug(endpoint.Name)}";
        var coverage = hasFrontend ? new[] { "backend-code", "frontend-static" } : new[] { "backend-code" };
        var unknowns = hasFrontend
            ? new[] { "delivery-history-not-analyzed" }
            : new[] { "frontend-ui-not-analyzed", "delivery-history-not-analyzed" };

        return new FeatureCandidate(
            id,
            name,
            area,
            endpoint.Id,
            coverage,
            unknowns,
            includedFacts.Values.OrderBy(fact => fact.Id, StringComparer.Ordinal).ToArray(),
            includedRelations
                .OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
                .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
                .ThenBy(relation => relation.Target, StringComparer.Ordinal)
                .ToArray());
    }

    private static void AddEndpointContextEvidence(
        EvidenceFact endpoint,
        FactDocument document,
        IDictionary<string, EvidenceFact> includedFacts,
        ICollection<EvidenceRelation> includedRelations,
        ISet<string> seenRelations)
    {
        var signature = string.Join(
            " ",
            new[]
            {
                endpoint.Metadata.TryGetValue("returnType", out var returnType) ? returnType : string.Empty,
                endpoint.Metadata.TryGetValue("parameters", out var parameters) ? parameters : string.Empty
            });

        foreach (var property in document.Facts.Where(fact => fact.Kind == "computed-property"))
        {
            var typeName = property.Container?.Split('.').LastOrDefault();
            if (string.IsNullOrWhiteSpace(typeName) || !ContainsIdentifier(signature, typeName))
            {
                continue;
            }

            includedFacts[property.Id] = property;
            AddRelation(
                new EvidenceRelation(endpoint.Id, "exposes-computed-property", property.Id, property.Source),
                includedRelations,
                seenRelations);
        }

        if (!endpoint.Metadata.TryGetValue("authorizationPolicies", out var policies))
        {
            return;
        }

        var requestedPolicies = policies
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var policy in document.Facts.Where(fact => fact.Kind == "authorization-policy"))
        {
            var policyName = policy.Metadata.TryGetValue("policyName", out var name)
                ? name
                : policy.Name;

            if (!requestedPolicies.Contains(policyName))
            {
                continue;
            }

            includedFacts[policy.Id] = policy;
            AddRelation(
                new EvidenceRelation(endpoint.Id, "uses-policy-definition", policy.Id, policy.Source),
                includedRelations,
                seenRelations);
        }
    }

    private static bool ContainsIdentifier(string value, string identifier) =>
        Regex.IsMatch(
            value,
            $@"(?<![A-Za-z0-9_]){Regex.Escape(identifier)}(?![A-Za-z0-9_])",
            RegexOptions.CultureInvariant);

    private static bool AddFrontendEvidence(
        EvidenceFact endpoint,
        FactDocument document,
        IDictionary<string, EvidenceFact> includedFacts,
        ICollection<EvidenceRelation> includedRelations,
        ISet<string> seenRelations)
    {
        if (!endpoint.Metadata.TryGetValue("httpMethod", out var endpointMethod) ||
            !endpoint.Metadata.TryGetValue("fullRoute", out var endpointRoute))
        {
            return false;
        }

        var endpointRouteKey = NormalizeRouteKey(endpointRoute);
        var apiCalls = document.Facts
            .Where(fact => fact.Kind == "ui-api-call")
            .Where(fact => fact.Metadata.TryGetValue("httpMethod", out var method) &&
                           string.Equals(method, endpointMethod, StringComparison.OrdinalIgnoreCase))
            .Where(fact => fact.Metadata.TryGetValue("routeKey", out var routeKey) &&
                           string.Equals(routeKey, endpointRouteKey, StringComparison.Ordinal))
            .ToArray();

        if (apiCalls.Length == 0)
        {
            return false;
        }

        foreach (var apiCall in apiCalls)
        {
            includedFacts[apiCall.Id] = apiCall;
            AddRelation(
                new EvidenceRelation(apiCall.Id, "calls-endpoint", endpoint.Id, apiCall.Source),
                includedRelations,
                seenRelations);

            AddActionEvidence(apiCall, document, includedFacts, includedRelations, seenRelations);
            AddLoadEvidence(apiCall, document, includedFacts, includedRelations, seenRelations);
        }

        return true;
    }

    private static void AddActionEvidence(
        EvidenceFact apiCall,
        FactDocument document,
        IDictionary<string, EvidenceFact> includedFacts,
        ICollection<EvidenceRelation> includedRelations,
        ISet<string> seenRelations)
    {
        var actionRelations = document.Relations
            .Where(relation => relation.Kind == "triggers-api" && relation.Target == apiCall.Id)
            .ToArray();

        var actions = actionRelations
            .Select(relation => document.Facts.FirstOrDefault(fact => fact.Id == relation.FromFactId))
            .Where(fact => fact?.Kind == "ui-action")
            .Cast<EvidenceFact>()
            .ToArray();

        if (actions.Length == 0)
        {
            var sameFileFacts = document.Facts.Where(fact =>
                fact.Kind == "ui-action" &&
                string.Equals(fact.Source.Path, apiCall.Source.Path, StringComparison.Ordinal));

            actions = sameFileFacts
                .Where(action => action.Metadata.TryGetValue("handler", out var handler) &&
                                 string.Equals(handler, apiCall.Container, StringComparison.Ordinal))
                .ToArray();
        }

        foreach (var action in actions)
        {
            includedFacts[action.Id] = action;
            AddRelation(
                new EvidenceRelation(action.Id, "triggers-api", apiCall.Id, action.Source),
                includedRelations,
                seenRelations);
            AddScreenEvidence(action, document, includedFacts, includedRelations, seenRelations);
        }
    }

    private static void AddLoadEvidence(
        EvidenceFact apiCall,
        FactDocument document,
        IDictionary<string, EvidenceFact> includedFacts,
        ICollection<EvidenceRelation> includedRelations,
        ISet<string> seenRelations)
    {
        var loadRelations = document.Relations
            .Where(relation => relation.Kind == "loads-api" && relation.Target == apiCall.Id)
            .ToArray();

        foreach (var loadRelation in loadRelations)
        {
            var source = document.Facts.FirstOrDefault(fact => fact.Id == loadRelation.FromFactId);
            if (source is null)
            {
                continue;
            }

            includedFacts[source.Id] = source;
            AddRelation(loadRelation, includedRelations, seenRelations);
            AddScreenEvidence(source, document, includedFacts, includedRelations, seenRelations);
        }
    }

    private static void AddScreenEvidence(
        EvidenceFact source,
        FactDocument document,
        IDictionary<string, EvidenceFact> includedFacts,
        ICollection<EvidenceRelation> includedRelations,
        ISet<string> seenRelations)
    {
        foreach (var screen in document.Facts.Where(fact =>
                     fact.Kind == "ui-screen" &&
                     (string.Equals(fact.Name, source.Container, StringComparison.Ordinal) ||
                      string.Equals(fact.Source.Path, source.Source.Path, StringComparison.Ordinal))))
        {
            includedFacts[screen.Id] = screen;

            foreach (var route in document.Facts.Where(fact =>
                         fact.Kind == "ui-route" &&
                         fact.Metadata.TryGetValue("component", out var component) &&
                         string.Equals(component, screen.Name, StringComparison.Ordinal)))
            {
                includedFacts[route.Id] = route;
                AddRelation(
                    new EvidenceRelation(route.Id, "renders-screen", screen.Name, route.Source),
                    includedRelations,
                    seenRelations);
            }
        }
    }

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

    private static bool IsCallable(EvidenceFact fact) =>
        fact.Kind is "method" or "endpoint" or "constructor";

    private static string InferArea(string? container)
    {
        if (string.IsNullOrWhiteSpace(container)) return "Unknown";
        var value = container.Split('.').Last();
        return value.EndsWith("Controller", StringComparison.Ordinal)
            ? value[..^"Controller".Length]
            : value;
    }

    private static string SplitWords(string value) =>
        Regex.Replace(value, "([a-z0-9])([A-Z])", "$1 $2");

    private static string Slug(string value)
    {
        var slug = NonSlugRegex().Replace(value.ToLowerInvariant(), "-").Trim('-');
        return slug.Length == 0 ? "unknown" : slug;
    }

    private static string NormalizeRouteKey(string value)
    {
        var route = value.Split('?', '#')[0].Trim();
        route = TemplateParameterRegex.Replace(route, "{param}");
        route = RouteParameterRegex.Replace(route, "{param}");
        if (!route.StartsWith('/')) route = "/" + route;
        return route.TrimEnd('/').ToLowerInvariant();
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonSlugRegex();
}
