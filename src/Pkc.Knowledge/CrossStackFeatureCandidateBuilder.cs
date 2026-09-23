using System.Text.RegularExpressions;
using Pkc.Core;

namespace Pkc.Knowledge;

public sealed class CrossStackFeatureCandidateBuilder
{
    private static readonly Regex TemplateParameterRegex = new(@"\$\{[^}]+\}", RegexOptions.Compiled);
    private static readonly Regex RouteParameterRegex = new(@"\{[^}/]+\}|:[A-Za-z0-9_]+", RegexOptions.Compiled);

    private static readonly HashSet<string> ScreenBehaviorKinds = new(StringComparer.Ordinal)
    {
        "ui-field",
        "ui-field-option",
        "ui-field-validation",
        "ui-field-visibility",
        "ui-field-enabled-state",
        "ui-result-binding",
        "ui-list-render",
        "value-transfer"
    };

    private const string CSharpFallbackWarning =
        "Some C# evidence in this workflow was analyzed without the target project's full MSBuild reference graph. Treat semantic symbol and call resolution as lower confidence.";

    private const string FrontendFallbackWarning =
        "Some frontend evidence in this workflow comes from conservative regex/template fallback analysis. Treat exact UI structure and linkage as lower confidence than AST-backed evidence.";

    private const string TransitiveMutationCausalityWarning =
        "Some transitive helper mutations were not promoted into this workflow because PKC could not prove that the helper mutated the endpoint's affected object. The raw mutation evidence remains available in the compiled fact set.";

    public FeatureCandidateDocument Build(FactDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var baseline = new FeatureCandidateBuilder().Build(document);
        var enriched = new FeatureCandidateDocument(
            "0.4.6",
            baseline.Candidates
                .Select(candidate => Enrich(candidate, document))
                .Select(FilterUnprovenTransitiveMutations)
                .ToArray());
        return new ApiFrontendBindingCandidateEnricher().Enrich(enriched, document);
    }

    private static FeatureCandidate Enrich(FeatureCandidate candidate, FactDocument document)
    {
        candidate = AddAnalysisWarnings(FilterFlowNoise(candidate));

        var endpoint = document.Facts.FirstOrDefault(fact => fact.Id == candidate.SeedFactId);
        if (endpoint is null ||
            !endpoint.Metadata.TryGetValue("httpMethod", out var endpointMethod) ||
            !endpoint.Metadata.TryGetValue("fullRoute", out var endpointRoute))
            return candidate;

        var routeKey = NormalizeRouteKey(endpointRoute);
        var apiCalls = document.Facts.Where(fact => fact.Kind == "ui-api-call")
            .Where(fact => fact.Metadata.TryGetValue("httpMethod", out var method) && string.Equals(method, endpointMethod, StringComparison.OrdinalIgnoreCase))
            .Where(fact => fact.Metadata.TryGetValue("routeKey", out var key) && string.Equals(key, routeKey, StringComparison.Ordinal))
            .ToArray();
        if (apiCalls.Length == 0) return candidate;

        var factsById = document.Facts.ToDictionary(fact => fact.Id, StringComparer.Ordinal);
        var facts = candidate.Facts.ToDictionary(fact => fact.Id, StringComparer.Ordinal);
        var relations = candidate.Relations.ToList();
        var relationKeys = relations.Select(RelationKey).ToHashSet(StringComparer.Ordinal);

        foreach (var apiCall in apiCalls)
        {
            facts[apiCall.Id] = apiCall;
            AddRelation(new EvidenceRelation(apiCall.Id, "calls-endpoint", endpoint.Id, apiCall.Source), relations, relationKeys);
            AddBindingsForApiCall(document, apiCall, facts, relations, relationKeys);
            AddResultFlowForApiCall(document, apiCall, facts, relations, relationKeys);

            var actionRelations = document.Relations
                .Where(relation => relation.Kind == "triggers-api" && relation.Target == apiCall.Id)
                .ToArray();

            var actions = actionRelations
                .Select(relation => factsById.TryGetValue(relation.FromFactId, out var fact) ? fact : null)
                .Where(fact => fact?.Kind == "ui-action")
                .Cast<EvidenceFact>()
                .ToArray();

            if (actions.Length == 0 && !string.IsNullOrWhiteSpace(apiCall.Container))
            {
                actions = document.Facts.Where(fact => fact.Kind == "ui-action")
                    .Where(fact => fact.Metadata.TryGetValue("handler", out var handler) && string.Equals(handler, apiCall.Container, StringComparison.Ordinal))
                    .ToArray();
            }

            foreach (var action in actions)
            {
                facts[action.Id] = action;
                AddRelation(new EvidenceRelation(action.Id, "triggers-api", apiCall.Id, action.Source), relations, relationKeys);
            }

            var screens = actions
                .SelectMany(action => FindScreensForAction(document, action))
                .Concat(FindScreensForApiCall(document, apiCall))
                .Concat(FindScreensForResultFlow(document, apiCall))
                .DistinctBy(screen => screen.Id, StringComparer.Ordinal)
                .ToArray();

            foreach (var screen in screens)
            {
                facts[screen.Id] = screen;
                AddScreenBehavior(document, screen, facts, relations, relationKeys);

                foreach (var route in document.Facts.Where(fact =>
                             fact.Kind == "ui-route" &&
                             fact.Metadata.TryGetValue("component", out var component) &&
                             string.Equals(component, screen.Name, StringComparison.Ordinal)))
                {
                    facts[route.Id] = route;
                    AddRelation(new EvidenceRelation(route.Id, "renders-screen", screen.Name, route.Source), relations, relationKeys);
                }
            }
        }

        var coverage = candidate.Coverage.Append("frontend-static").Distinct(StringComparer.Ordinal).ToArray();
        var unknowns = candidate.Unknowns.Where(item => item != "frontend-ui-not-analyzed").ToArray();
        var enriched = candidate with
        {
            Coverage = coverage,
            Unknowns = unknowns,
            Facts = facts.Values.OrderBy(fact => fact.Id, StringComparer.Ordinal).ToArray(),
            Relations = relations.OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
                .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
                .ThenBy(relation => relation.Target, StringComparer.Ordinal)
                .ToArray()
        };

        return AddAnalysisWarnings(FilterFlowNoise(enriched));
    }

    private static FeatureCandidate FilterUnprovenTransitiveMutations(FeatureCandidate candidate)
    {
        var factsById = candidate.Facts.ToDictionary(fact => fact.Id, StringComparer.Ordinal);
        var removedMutationIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var mutation in candidate.Facts.Where(fact => fact.Kind == "mutation"))
        {
            var owners = candidate.Relations
                .Where(relation => relation.Kind == "mutates" && relation.Target == mutation.Id)
                .Select(relation => factsById.TryGetValue(relation.FromFactId, out var owner) ? owner : null)
                .Where(owner => owner is not null)
                .Cast<EvidenceFact>()
                .DistinctBy(owner => owner.Id, StringComparer.Ordinal)
                .ToArray();

            if (owners.Any(owner => string.Equals(owner.Id, candidate.SeedFactId, StringComparison.Ordinal)))
            {
                continue;
            }

            if (owners.Length == 1 && IsSupportedTransitiveMutation(mutation))
            {
                continue;
            }

            removedMutationIds.Add(mutation.Id);
        }

        if (removedMutationIds.Count == 0)
        {
            return candidate;
        }

        return candidate with
        {
            Facts = candidate.Facts
                .Where(fact => !removedMutationIds.Contains(fact.Id))
                .ToArray(),
            Relations = candidate.Relations
                .Where(relation =>
                    !removedMutationIds.Contains(relation.FromFactId) &&
                    !removedMutationIds.Contains(relation.Target))
                .ToArray(),
            Unknowns = candidate.Unknowns
                .Append(TransitiveMutationCausalityWarning)
                .Distinct(StringComparer.Ordinal)
                .ToArray()
        };
    }

    private static bool IsSupportedTransitiveMutation(EvidenceFact mutation) =>
        !mutation.Metadata.TryGetValue("mutationReceiverOrigin", out var origin) ||
        !string.Equals(origin, "runtime-pattern-variable", StringComparison.Ordinal);

    private static void AddScreenBehavior(
        FactDocument document,
        EvidenceFact screen,
        IDictionary<string, EvidenceFact> facts,
        ICollection<EvidenceRelation> relations,
        ISet<string> relationKeys)
    {
        foreach (var behavior in document.Facts.Where(fact =>
                     ScreenBehaviorKinds.Contains(fact.Kind) &&
                     (string.Equals(fact.Container, screen.Name, StringComparison.Ordinal) ||
                      fact.Metadata.TryGetValue("component", out var component) &&
                      string.Equals(component, screen.Name, StringComparison.Ordinal)))
                 .Where(fact => fact.Kind != "value-transfer" ||
                                (fact.Metadata.TryGetValue("componentIdentity", out var componentIdentity) &&
                                 string.Equals(componentIdentity, $"{screen.Source.Path}#{screen.Name}", StringComparison.Ordinal))))
        {
            facts[behavior.Id] = behavior;
            AddRelation(
                new EvidenceRelation(screen.Id, "contains-ui-behavior", behavior.Id, behavior.Source),
                relations,
                relationKeys);

            foreach (var relation in document.Relations.Where(relation =>
                         relation.FromFactId == behavior.Id &&
                         relation.Kind == "feeds-list"))
            {
                AddRelation(relation, relations, relationKeys);
                var target = document.Facts.FirstOrDefault(fact => fact.Id == relation.Target);
                if (target is not null)
                {
                    facts[target.Id] = target;
                }
            }
        }
    }

    private static void AddBindingsForApiCall(
        FactDocument document,
        EvidenceFact apiCall,
        IDictionary<string, EvidenceFact> facts,
        ICollection<EvidenceRelation> relations,
        ISet<string> relationKeys)
    {
        if (string.IsNullOrWhiteSpace(apiCall.Container))
        {
            return;
        }

        foreach (var binding in document.Facts.Where(fact =>
                     fact.Kind == "ui-field-binding" &&
                     string.Equals(fact.Container, apiCall.Container, StringComparison.Ordinal)))
        {
            facts[binding.Id] = binding;
            AddRelation(
                new EvidenceRelation(binding.Id, "binds-api", apiCall.Id, binding.Source),
                relations,
                relationKeys);
        }
    }

    private static void AddResultFlowForApiCall(
        FactDocument document,
        EvidenceFact apiCall,
        IDictionary<string, EvidenceFact> facts,
        ICollection<EvidenceRelation> relations,
        ISet<string> relationKeys)
    {
        if (string.IsNullOrWhiteSpace(apiCall.Container))
        {
            return;
        }

        foreach (var binding in document.Facts.Where(fact => IsResultBindingForApiCall(fact, apiCall)))
        {
            facts[binding.Id] = binding;
            AddRelation(
                new EvidenceRelation(apiCall.Id, "feeds-ui-binding", binding.Id, binding.Source),
                relations,
                relationKeys);

            foreach (var relation in document.Relations.Where(relation =>
                         relation.FromFactId == binding.Id && relation.Kind == "feeds-list"))
            {
                AddRelation(relation, relations, relationKeys);
                var render = document.Facts.FirstOrDefault(fact => fact.Id == relation.Target);
                if (render is not null)
                {
                    facts[render.Id] = render;
                }
            }
        }
    }

    private static bool IsResultBindingForApiCall(EvidenceFact binding, EvidenceFact apiCall)
    {
        if (binding.Kind != "ui-result-binding" ||
            string.IsNullOrWhiteSpace(apiCall.Container) ||
            !binding.Metadata.TryGetValue("apiMethod", out var apiMethod) ||
            !string.Equals(apiMethod, apiCall.Container, StringComparison.Ordinal) ||
            !binding.Metadata.TryGetValue("serviceType", out var serviceType) ||
            string.IsNullOrWhiteSpace(serviceType) ||
            !apiCall.Metadata.TryGetValue("ownerClass", out var ownerClass) ||
            string.IsNullOrWhiteSpace(ownerClass))
        {
            return false;
        }

        return string.Equals(serviceType, ownerClass, StringComparison.Ordinal);
    }

    private static FeatureCandidate FilterFlowNoise(FeatureCandidate candidate)
    {
        var relations = candidate.Relations
            .Where(relation => relation.Kind != "invokes" || !IsFlowNoiseTarget(relation.Target))
            .ToArray();

        return candidate with { Relations = relations };
    }

    private static bool IsFlowNoiseTarget(string target) =>
        target.StartsWith("System.", StringComparison.Ordinal) ||
        target.StartsWith("string.", StringComparison.Ordinal) ||
        target.StartsWith("char.", StringComparison.Ordinal) ||
        target.StartsWith("object.", StringComparison.Ordinal) ||
        target.StartsWith("Microsoft.AspNetCore.Http.Results.", StringComparison.Ordinal) ||
        target.StartsWith("Microsoft.Extensions.", StringComparison.Ordinal);

    private static FeatureCandidate AddAnalysisWarnings(FeatureCandidate candidate)
    {
        var unknowns = candidate.Unknowns.ToList();

        if (candidate.Facts.Any(fact =>
                fact.Metadata.TryGetValue("analysisMode", out var mode) &&
                string.Equals(mode, "loose-roslyn-fallback", StringComparison.Ordinal)))
        {
            unknowns.Add(CSharpFallbackWarning);
        }

        if (candidate.Facts.Any(fact =>
                fact.Metadata.TryGetValue("analysisMode", out var mode) &&
                (mode.Contains("regex-fallback", StringComparison.Ordinal) ||
                 mode.Contains("template-regex-fallback", StringComparison.Ordinal))))
        {
            unknowns.Add(FrontendFallbackWarning);
        }

        return candidate with
        {
            Unknowns = unknowns.Distinct(StringComparer.Ordinal).ToArray()
        };
    }

    private static IEnumerable<EvidenceFact> FindScreensForAction(FactDocument document, EvidenceFact action)
    {
        if (!string.IsNullOrWhiteSpace(action.Container))
        {
            var byName = document.Facts.Where(fact =>
                fact.Kind == "ui-screen" && string.Equals(fact.Name, action.Container, StringComparison.Ordinal)).ToArray();
            if (byName.Length > 0)
            {
                return byName;
            }
        }

        return document.Facts.Where(fact =>
            fact.Kind == "ui-screen" && string.Equals(fact.Source.Path, action.Source.Path, StringComparison.Ordinal));
    }

    private static IEnumerable<EvidenceFact> FindScreensForApiCall(FactDocument document, EvidenceFact apiCall) =>
        document.Facts.Where(fact =>
            fact.Kind == "ui-screen" &&
            string.Equals(fact.Source.Path, apiCall.Source.Path, StringComparison.Ordinal));

    private static IEnumerable<EvidenceFact> FindScreensForResultFlow(FactDocument document, EvidenceFact apiCall)
    {
        if (string.IsNullOrWhiteSpace(apiCall.Container))
        {
            return [];
        }

        var components = document.Facts
            .Where(fact => IsResultBindingForApiCall(fact, apiCall) && !string.IsNullOrWhiteSpace(fact.Container))
            .Select(fact => fact.Container!)
            .ToHashSet(StringComparer.Ordinal);

        return document.Facts.Where(fact =>
            fact.Kind == "ui-screen" && components.Contains(fact.Name));
    }

    private static void AddRelation(EvidenceRelation relation, ICollection<EvidenceRelation> relations, ISet<string> keys)
    {
        if (keys.Add(RelationKey(relation))) relations.Add(relation);
    }

    private static string RelationKey(EvidenceRelation relation) => $"{relation.FromFactId}|{relation.Kind}|{relation.Target}";

    private static string NormalizeRouteKey(string value)
    {
        var route = value.Split('?', '#')[0].Trim();
        route = TemplateParameterRegex.Replace(route, "{param}");
        route = RouteParameterRegex.Replace(route, "{param}");
        if (!route.StartsWith('/')) route = "/" + route;
        return route.TrimEnd('/').ToLowerInvariant();
    }
}
