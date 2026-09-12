using System.Text.RegularExpressions;
using Pkc.Core;

namespace Pkc.Knowledge;

public sealed class CrossStackFeatureCandidateBuilder
{
    private static readonly Regex TemplateParameterRegex = new(@"\$\{[^}]+\}", RegexOptions.Compiled);
    private static readonly Regex RouteParameterRegex = new(@"\{[^}/]+\}|:[A-Za-z0-9_]+", RegexOptions.Compiled);

    private const string CSharpFallbackWarning =
        "Some C# evidence in this workflow was analyzed without the target project's full MSBuild reference graph. Treat semantic symbol and call resolution as lower confidence.";

    private const string FrontendFallbackWarning =
        "Some frontend evidence in this workflow comes from conservative regex/template fallback analysis. Treat exact UI structure and linkage as lower confidence than AST-backed evidence.";

    public FeatureCandidateDocument Build(FactDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var baseline = new FeatureCandidateBuilder().Build(document);
        return new FeatureCandidateDocument("0.4.4", baseline.Candidates.Select(candidate => Enrich(candidate, document)).ToArray());
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

                var screens = FindScreensForAction(document, action).ToArray();
                foreach (var screen in screens)
                {
                    facts[screen.Id] = screen;
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
