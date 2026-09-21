using Pkc.Core;

namespace Pkc.Knowledge;

internal sealed class ApiFrontendBindingCandidateEnricher
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
        var endpoint = document.Facts.FirstOrDefault(fact => fact.Id == candidate.SeedFactId);
        if (endpoint is null || endpoint.Kind != "endpoint")
        {
            return candidate;
        }

        var apiCalls = candidate.Facts
            .Where(fact => fact.Kind == "ui-api-call")
            .ToArray();
        if (apiCalls.Length == 0)
        {
            return candidate;
        }

        var terminals = document.Facts
            .Where(fact => fact.Kind == "value-terminal-source")
            .Where(fact => string.Equals(
                fact.Metadata.GetValueOrDefault("boundary"),
                "API response field",
                StringComparison.Ordinal))
            .Where(fact => string.Equals(
                fact.Metadata.GetValueOrDefault("endpointFactId"),
                endpoint.Id,
                StringComparison.Ordinal))
            .Where(fact => !string.IsNullOrWhiteSpace(fact.Metadata.GetValueOrDefault("wireName")))
            .ToArray();
        if (terminals.Length == 0)
        {
            return candidate;
        }

        var facts = candidate.Facts.ToDictionary(fact => fact.Id, StringComparer.Ordinal);
        var relations = candidate.Relations.ToList();
        var relationKeys = relations.Select(RelationKey).ToHashSet(StringComparer.Ordinal);

        foreach (var apiCall in apiCalls)
        {
            foreach (var terminal in terminals)
            {
                AddExactBinding(
                    document,
                    endpoint,
                    apiCall,
                    terminal,
                    facts,
                    relations,
                    relationKeys);
            }
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

    private static void AddExactBinding(
        FactDocument document,
        EvidenceFact endpoint,
        EvidenceFact apiCall,
        EvidenceFact terminal,
        IDictionary<string, EvidenceFact> facts,
        ICollection<EvidenceRelation> relations,
        ISet<string> relationKeys)
    {
        if (!terminal.Metadata.TryGetValue("wireName", out var wireName) ||
            string.IsNullOrWhiteSpace(wireName) ||
            !apiCall.Metadata.TryGetValue("ownerClass", out var serviceType) ||
            string.IsNullOrWhiteSpace(serviceType) ||
            string.IsNullOrWhiteSpace(apiCall.Container) ||
            !apiCall.Metadata.TryGetValue("httpMethod", out var httpMethod) ||
            !apiCall.Metadata.TryGetValue("routeKey", out var routeKey))
        {
            return;
        }

        var responseMembers = document.Facts
            .Where(fact => fact.Kind == "ui-api-response-member")
            .Where(fact => string.Equals(
                fact.Metadata.GetValueOrDefault("serviceType"),
                serviceType,
                StringComparison.Ordinal))
            .Where(fact => string.Equals(
                fact.Metadata.GetValueOrDefault("apiMethod"),
                apiCall.Container,
                StringComparison.Ordinal))
            .Where(fact => string.Equals(
                fact.Metadata.GetValueOrDefault("httpMethod"),
                httpMethod,
                StringComparison.OrdinalIgnoreCase))
            .Where(fact => string.Equals(
                fact.Metadata.GetValueOrDefault("routeKey"),
                routeKey,
                StringComparison.Ordinal))
            .Where(fact => string.Equals(
                fact.Metadata.GetValueOrDefault("resultMember"),
                wireName,
                StringComparison.Ordinal))
            .ToArray();
        if (responseMembers.Length != 1)
        {
            return;
        }

        var responseMember = responseMembers[0];
        var bindings = document.Facts
            .Where(fact => fact.Kind == "ui-result-member-binding")
            .Where(fact => string.Equals(
                fact.Metadata.GetValueOrDefault("serviceType"),
                serviceType,
                StringComparison.Ordinal))
            .Where(fact => string.Equals(
                fact.Metadata.GetValueOrDefault("apiMethod"),
                apiCall.Container,
                StringComparison.Ordinal))
            .Where(fact => string.Equals(
                fact.Metadata.GetValueOrDefault("resultMember"),
                wireName,
                StringComparison.Ordinal))
            .ToArray();
        if (bindings.Length != 1)
        {
            return;
        }

        var binding = bindings[0];
        if (!binding.Metadata.TryGetValue("target", out var target) ||
            string.IsNullOrWhiteSpace(target) ||
            !binding.Metadata.TryGetValue("componentIdentity", out var componentIdentity) ||
            string.IsNullOrWhiteSpace(componentIdentity) ||
            !binding.Metadata.TryGetValue("component", out var component) ||
            string.IsNullOrWhiteSpace(component))
        {
            return;
        }

        var renders = document.Facts
            .Where(fact => fact.Kind == "ui-member-render")
            .Where(fact => string.Equals(
                fact.Metadata.GetValueOrDefault("componentIdentity"),
                componentIdentity,
                StringComparison.Ordinal))
            .Where(fact => string.Equals(
                fact.Metadata.GetValueOrDefault("member"),
                target,
                StringComparison.Ordinal))
            .ToArray();
        if (renders.Length == 0)
        {
            return;
        }

        facts[terminal.Id] = terminal;
        facts[responseMember.Id] = responseMember;
        facts[binding.Id] = binding;
        facts[apiCall.Id] = apiCall;

        foreach (var render in renders)
        {
            facts[render.Id] = render;
            var resultParameter = binding.Metadata.GetValueOrDefault("resultParameter") ?? "result";
            var backendMember = terminal.Metadata.GetValueOrDefault("returnedOccurrence") ?? terminal.Name;
            var returnedOccurrence =
                $"{backendMember} → {resultParameter}.{wireName} → {component}.{target} → rendered {target}";
            var bindingLocation = DescribeLocation(binding.Source);
            var renderLocation = DescribeLocation(render.Source);
            var responseMemberLocation = responseMember.Metadata.GetValueOrDefault("resultMemberLocation") ??
                                         DescribeLocation(responseMember.Source);
            var id = $"cross-lineage:{endpoint.Id}:{terminal.Id}:{binding.Id}:{render.Id}";
            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["knowledgeClass"] = "value-lineage",
                ["scopeFactId"] = endpoint.Id,
                ["scopeName"] = endpoint.Name,
                ["analysisMode"] = "explicit-wire+bounded-frontend-dataflow",
                ["analysisConfidence"] = "high",
                ["proof"] = "json-property-name+typed-http-result-member+exact-result-receiver-assignment+exact-rendered-member",
                ["boundary"] = "rendered UI value",
                ["returnedOccurrence"] = returnedOccurrence,
                ["sourceFactId"] = terminal.Id,
                ["sourceMechanism"] = "explicit-wire-api-ui-binding",
                ["sourceFactLocation"] = bindingLocation,
                ["sourceOccurrence"] = wireName,
                ["endpointFactId"] = endpoint.Id,
                ["backendTerminalFactId"] = terminal.Id,
                ["backendProjectionFactId"] = terminal.Metadata.GetValueOrDefault("sourceFactId") ?? string.Empty,
                ["backendResponseMember"] = backendMember,
                ["backendResponseLocation"] = terminal.Metadata.GetValueOrDefault("responseLocation") ?? DescribeLocation(terminal.Source),
                ["wireName"] = wireName,
                ["wireContract"] = terminal.Metadata.GetValueOrDefault("wireContract") ?? "unknown",
                ["wireContractLocation"] = terminal.Metadata.GetValueOrDefault("wireContractLocation") ?? "unknown",
                ["frontendApiCallFactId"] = apiCall.Id,
                ["frontendApiLocation"] = DescribeLocation(responseMember.Source),
                ["frontendServiceType"] = serviceType,
                ["frontendApiMethod"] = apiCall.Container,
                ["frontendResponseType"] = responseMember.Metadata.GetValueOrDefault("responseType") ?? "unknown",
                ["frontendResultMember"] = wireName,
                ["frontendResultMemberLocation"] = responseMemberLocation,
                ["frontendBindingFactId"] = binding.Id,
                ["frontendBindingLocation"] = bindingLocation,
                ["frontendComponentIdentity"] = componentIdentity,
                ["frontendComponentMember"] = target,
                ["frontendRenderFactId"] = render.Id,
                ["frontendRenderLocation"] = renderLocation
            };
            if (terminal.Metadata.TryGetValue("semanticProject", out var semanticProject))
            {
                metadata["semanticProject"] = semanticProject;
            }

            var composed = new EvidenceFact(
                id,
                "value-terminal-source",
                $"API response `{wireName}` to rendered `{component}.{target}`",
                component,
                render.Source,
                [],
                metadata);
            facts[id] = composed;
            AddRelation(
                new EvidenceRelation(terminal.Id, "feeds-ui-value", id, terminal.Source),
                relations,
                relationKeys);
            AddRelation(
                new EvidenceRelation(apiCall.Id, "feeds-ui-value", id, responseMember.Source),
                relations,
                relationKeys);
            AddRelation(
                new EvidenceRelation(binding.Id, "feeds-ui-value", id, binding.Source),
                relations,
                relationKeys);
            AddRelation(
                new EvidenceRelation(render.Id, "renders-ui-value", id, render.Source),
                relations,
                relationKeys);
        }
    }

    private static string DescribeLocation(SourceLocation source) =>
        $"{source.Path}:L{source.StartLine}";

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
