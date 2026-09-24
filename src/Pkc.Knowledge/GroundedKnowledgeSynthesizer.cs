using Pkc.Core;

namespace Pkc.Knowledge;

public sealed class GroundedKnowledgeSynthesizer : IKnowledgeSynthesizer
{
    private static readonly string[] TrustedPublicationPrefixes = ["Publish", "Emit", "Enqueue", "Produce"];
    private const int MaxListedUnresolvedDispatches = 5;

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
        var unknowns = candidate.Unknowns.Select(FriendlyUnknown).Concat(BuildUnresolvedDispatchUnknowns(candidate)).ToArray();

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
            action.Metadata.TryGetValue("visibilityCondition", out var visibilityCondition);

            var screen = string.IsNullOrWhiteSpace(action.Container)
                ? string.Empty
                : $" on `{action.Container}`";

            var guardParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(permission))
            {
                guardParts.Add($"permission `{permission}`");
            }

            if (!string.IsNullOrWhiteSpace(visibilityCondition))
            {
                guardParts.Add($"visible when `{visibilityCondition}`");
            }

            var guardNote = guardParts.Count == 0
                ? string.Empty
                : $"; {string.Join("; ", guardParts)}";

            result.Add($"Click `{label ?? action.Name}`{screen}{guardNote}.");
        }

        return result.Distinct(StringComparer.Ordinal).ToArray();
    }

    private static IReadOnlyList<string> BuildUiToBackend(FeatureCandidate candidate)
    {
        var result = new List<string>();

        foreach (var fact in candidate.Facts.Where(fact => fact.Kind == "ui-api-call"))
        {
            fact.Metadata.TryGetValue("httpMethod", out var method);
            fact.Metadata.TryGetValue("url", out var url);
            result.Add($"UI sends `{method ?? "HTTP"} {url ?? fact.Name}`.");
        }

        foreach (var binding in candidate.Facts.Where(fact => fact.Kind == "ui-result-binding"))
        {
            if (!binding.Metadata.TryGetValue("apiMethod", out var apiMethod) ||
                !binding.Metadata.TryGetValue("target", out var target))
            {
                continue;
            }

            var component = string.IsNullOrWhiteSpace(binding.Container)
                ? string.Empty
                : $" on `{binding.Container}`";
            result.Add($"UI assigns the result of API method `{apiMethod}` to `{target}`{component}.");
        }

        foreach (var render in candidate.Facts.Where(fact => fact.Kind == "ui-list-render"))
        {
            if (!render.Metadata.TryGetValue("item", out var item) ||
                !render.Metadata.TryGetValue("collection", out var collection))
            {
                continue;
            }

            var component = string.IsNullOrWhiteSpace(render.Container)
                ? "The UI"
                : $"`{render.Container}`";
            result.Add($"{component} renders each `{item}` from `{collection}` in the list.");
        }

        return result.Distinct(StringComparer.Ordinal).ToArray();
    }

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

    private static IReadOnlyList<string> BuildPermissions(
        FeatureCandidate candidate,
        EvidenceFact? endpoint)
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

            if (permissions.Count == 0 &&
                endpoint.Metadata.TryGetValue("authorization", out var authorization))
            {
                permissions.Add($"Authorization required: {authorization}");
            }

            if (endpoint.Metadata.TryGetValue("authorizationRequirements", out var requirements))
            {
                permissions.AddRange(requirements
                    .Split(" | ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(requirement => $"Requires `{requirement}` (repository-specific authorization attribute; its exact semantics are defined in the repository)."));
            }
        }

        permissions.AddRange(candidate.Facts
            .Where(fact => fact.Kind == "ui-action")
            .Select(fact => fact.Metadata.TryGetValue("permission", out var permission) ? permission : null)
            .Where(permission => !string.IsNullOrWhiteSpace(permission))
            .Select(permission => $"UI visibility guard: {permission}"));

        permissions.AddRange(candidate.Facts
            .Where(fact => fact.Kind == "authorization-policy")
            .Select(fact =>
            {
                fact.Metadata.TryGetValue("policyName", out var policyName);
                fact.Metadata.TryGetValue("definition", out var definition);
                var name = policyName ?? fact.Name;
                return string.IsNullOrWhiteSpace(definition)
                    ? $"Policy definition observed: {name}."
                    : $"Policy definition observed: {name} → `{definition}`.";
            }));

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

            if (condition.Kind == "business-predicate")
            {
                rules.Add(DescribeBusinessPredicate(condition, expression));
                continue;
            }

            var ownedThrows = throws
                .Where(throwFact =>
                    throwOwner.TryGetValue(throwFact.Id, out var ownerId) &&
                    ownerId == condition.Id)
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

                var exception = string.IsNullOrWhiteSpace(exceptionType)
                    ? "an exception"
                    : $"`{exceptionType}`";

                var detail = string.IsNullOrWhiteSpace(throwExpression)
                    ? string.Empty
                    : $" using `{throwExpression}`";

                rules.Add($"When `{expression}`, the implementation throws {exception}{detail}.");
            }
        }

        foreach (var guard in candidate.Facts.Where(fact => fact.Kind == "guard-condition"))
        {
            var exception = guard.Metadata.TryGetValue("exceptionType", out var exceptionType) ? $"throws `{exceptionType}`" : "throws";
            var message = guard.Metadata.TryGetValue("message", out var text) ? $"; message: `{text}`" : string.Empty;
            rules.Add($"Rejects ({exception}) when `{guard.Metadata.GetValueOrDefault("condition")}`{message}.");
        }

        foreach (var gate in candidate.Facts.Where(fact => fact.Kind == "boolean-gate"))
        {
            rules.Add(DescribeBooleanGate(gate));
        }

        foreach (var mapped in candidate.Facts.Where(fact => fact.Kind == "mapped-field-rule"))
        {
            rules.Add(DescribeMappedFieldRule(mapped));
            if (mapped.Metadata.TryGetValue("documentation", out var documentation))
            {
                rules.Add($"Developer documentation for the rule behind `{mapped.Name}` (a code comment, not verified behavior; where it disagrees with the conditions above, the conditions are what the code does): \"{documentation}\"");
            }
        }

        foreach (var configured in candidate.Facts.Where(fact => fact.Kind == "configured-object"))
        {
            if (!configured.Metadata.TryGetValue("source", out var source) ||
                !configured.Metadata.TryGetValue("assignments", out var assignments))
            {
                continue;
            }

            rules.Add($"Configured item in `{source}`: `{assignments}`.");
        }

        foreach (var property in candidate.Facts.Where(fact => fact.Kind == "computed-property"))
        {
            if (!property.Metadata.TryGetValue("expression", out var expression))
            {
                continue;
            }

            var displayName = string.IsNullOrWhiteSpace(property.Container)
                ? property.Name
                : $"{property.Container}.{property.Name}";

            rules.Add($"Computed property `{displayName}` = `{expression}`.");
        }

        foreach (var construction in candidate.Facts.Where(fact => fact.Kind == "object-construction"))
        {
            construction.Metadata.TryGetValue("type", out var type);
            construction.Metadata.TryGetValue("operation", out var operation);
            construction.Metadata.TryGetValue("assignments", out var assignments);

            var typeName = type ?? construction.Name;
            if (string.IsNullOrWhiteSpace(assignments))
            {
                continue;
            }

            var via = string.IsNullOrWhiteSpace(operation)
                ? string.Empty
                : $" via `{operation}`";

            rules.Add($"Creates `{typeName}`{via} with `{assignments}`.");
        }

        foreach (var loop in candidate.Facts.Where(fact => fact.Kind == "loop"))
        {
            if (!loop.Metadata.TryGetValue("collection", out var collection))
            {
                continue;
            }

            loop.Metadata.TryGetValue("iterator", out var iterator);
            var iteratorText = string.IsNullOrWhiteSpace(iterator)
                ? string.Empty
                : $" as `{iterator}`";

            rules.Add($"Iterates `{collection}`{iteratorText}.");
        }

        return rules.Distinct(StringComparer.Ordinal).ToArray();
    }

    private static string DescribeBooleanGate(EvidenceFact gate)
    {
        var target = gate.Metadata.GetValueOrDefault("target") ?? gate.Name;
        var operands = Indexed(gate.Metadata, "operand").Select(pair =>
        {
            var setting = gate.Metadata.GetValueOrDefault($"operandSetting.{pair.Index}");
            var source = gate.Metadata.GetValueOrDefault($"operandSource.{pair.Index}");
            if (!string.IsNullOrWhiteSpace(setting))
            {
                return $"setting `{setting}` is on (`{pair.Value}` = `{source ?? pair.Value}`)";
            }

            return string.IsNullOrWhiteSpace(source) ? $"`{pair.Value}`" : $"`{pair.Value}` (= `{source}`)";
        });
        return $"`{target}` is true only when all of these hold: {string.Join("; ", operands)}.";
    }

    private static string DescribeMappedFieldRule(EvidenceFact rule)
    {
        var destination = rule.Metadata.GetValueOrDefault("destinationType") ?? rule.Container ?? "the result";
        var ruleSource = rule.Metadata.GetValueOrDefault("ruleSource");
        var location = rule.Metadata.GetValueOrDefault("ruleLocation");
        var origin = string.Equals(ruleSource, "inline-lambda", StringComparison.Ordinal) || string.IsNullOrWhiteSpace(ruleSource)
            ? $"mapping rule at `{location ?? rule.Source.Path}`"
            : $"rule `{ruleSource}` at `{location}`";
        var conjuncts = Indexed(rule.Metadata, "conjunct").ToArray();
        if (conjuncts.Length == 1)
        {
            return $"Result field `{rule.Name}` of `{destination}` is computed as `{conjuncts[0].Value}` ({origin}){Note(rule, 0)}.";
        }

        var lines = conjuncts.Select(pair => $"\n   {pair.Index + 1}. `{pair.Value}`{Note(rule, pair.Index)}");
        return $"Result field `{rule.Name}` of `{destination}` is true only when ALL of these hold ({origin}):{string.Concat(lines)}";
    }

    private static string Note(EvidenceFact fact, int index) =>
        fact.Metadata.TryGetValue($"note.{index}", out var note) ? $" — dev comment: \"{note}\"" : string.Empty;

    private static IEnumerable<(int Index, string Value)> Indexed(IReadOnlyDictionary<string, string> metadata, string prefix)
    {
        for (var index = 0; metadata.TryGetValue($"{prefix}.{index}", out var value); index++)
        {
            yield return (index, value);
        }
    }

    private static IEnumerable<string> BuildUnresolvedDispatchUnknowns(FeatureCandidate candidate)
    {
        var targets = candidate.Relations
            .Where(relation => relation.Kind == "unresolved-dispatch")
            .Select(relation => relation.Target)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        if (targets.Length == 0)
        {
            yield break;
        }

        var listed = string.Join(", ", targets.Take(MaxListedUnresolvedDispatches).Select(target => $"`{target}`"));
        var more = targets.Length > MaxListedUnresolvedDispatches ? $" and {targets.Length - MaxListedUnresolvedDispatches} more" : string.Empty;
        yield return $"Backend evidence stops at {targets.Length} interface call(s) whose implementation PKC could not prove (no direct DI registration and not exactly one implementing class): {listed}{more}. Behavior behind them is not proven.";
    }

    private static string DescribeBusinessPredicate(EvidenceFact fact, string expression)
    {
        fact.Metadata.TryGetValue("source", out var source);
        fact.Metadata.TryGetValue("effect", out var effect);
        fact.Metadata.TryGetValue("observableContext", out var observableContext);
        var sourceText = string.IsNullOrWhiteSpace(source) ? "the source collection" : $"`{source}`";

        if (string.Equals(observableContext, "return", StringComparison.Ordinal))
        {
            return effect switch
            {
                "existence" => $"Returns whether at least one item from {sourceText} satisfies `{expression}`.",
                "universal-requirement" => $"Returns whether every item from {sourceText} satisfies `{expression}`.",
                "selection" => $"Returns an item from {sourceText} selected where `{expression}`.",
                _ => DescribePredicateEffect(effect, sourceText, expression)
            };
        }

        if (string.Equals(observableContext, "yield-return", StringComparison.Ordinal))
        {
            return effect switch
            {
                "existence" => $"Yields whether at least one item from {sourceText} satisfies `{expression}`.",
                "universal-requirement" => $"Yields whether every item from {sourceText} satisfies `{expression}`.",
                "selection" => $"Yields an item from {sourceText} selected where `{expression}`.",
                _ => DescribePredicateEffect(effect, sourceText, expression)
            };
        }

        return DescribePredicateEffect(effect, sourceText, expression);
    }

    private static string DescribePredicateEffect(string? effect, string sourceText, string expression) =>
        effect switch
        {
            "inclusion" => $"Includes items from {sourceText} only when `{expression}`.",
            "existence" => $"Checks whether at least one item from {sourceText} satisfies `{expression}`.",
            "universal-requirement" => $"Checks whether every item from {sourceText} satisfies `{expression}`.",
            "selection" => $"Selects an item from {sourceText} where `{expression}`.",
            _ => $"Business predicate on {sourceText}: `{expression}`."
        };

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
            "+=" when !string.IsNullOrWhiteSpace(value) =>
                $"Applies `+=` to `{displayTarget}` with `{value}`.",

            "-=" when !string.IsNullOrWhiteSpace(value) =>
                $"Applies `-=` to `{displayTarget}` with `{value}`.",

            "*=" when !string.IsNullOrWhiteSpace(value) =>
                $"Applies `*=` to `{displayTarget}` with `{value}`.",

            "/=" when !string.IsNullOrWhiteSpace(value) =>
                $"Applies `/=` to `{displayTarget}` with `{value}`.",

            "%=" when !string.IsNullOrWhiteSpace(value) =>
                $"Applies `%=` to `{displayTarget}` with `{value}`.",

            string op when op?.Contains("Increment", StringComparison.Ordinal) == true =>
                $"Increments `{displayTarget}` by 1.",

            string op when op?.Contains("Decrement", StringComparison.Ordinal) == true =>
                $"Decrements `{displayTarget}` by 1.",

            _ when string.IsNullOrWhiteSpace(value) =>
                $"Mutates `{displayTarget}`.",

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
        return TrustedPublicationPrefixes.Any(prefix =>
            methodName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyList<string> BuildFlow(
        FeatureCandidate candidate,
        IReadOnlyDictionary<string, EvidenceFact> factsById)
    {
        var flow = new List<string>();
        foreach (var relation in candidate.Relations.Where(relation =>
                     relation.Kind is "invokes" or "dispatches" or "dispatches-sole-implementation"))
        {
            var source = factsById.TryGetValue(relation.FromFactId, out var sourceFact)
                ? DisplayFact(sourceFact)
                : relation.FromFactId;

            if (relation.Kind == "invokes")
            {
                flow.Add($"{source} → {relation.Target}");
                continue;
            }

            if (relation.Kind == "dispatches-sole-implementation")
            {
                var implementation = factsById.TryGetValue(relation.Target, out var implementationFact)
                    ? DisplayFact(implementationFact)
                    : relation.Target;
                flow.Add($"{source} → {implementation} (inferred: the only implementing class, declared at {relation.Source.Path}:L{relation.Source.StartLine}; not proven by a DI registration)");
                continue;
            }

            var target = factsById.TryGetValue(relation.Target, out var targetFact)
                ? DisplayFact(targetFact)
                : relation.Target;
            flow.Add($"{source} via DI registration at {relation.Source.Path}:L{relation.Source.StartLine} -> {target}");
        }

        return flow.Distinct(StringComparer.Ordinal).ToArray();
    }

    private static IReadOnlyList<KnowledgeEvidence> BuildEvidence(
        FeatureCandidate candidate,
        IReadOnlyDictionary<string, EvidenceFact> factsById,
        EvidenceFact? endpoint) =>
        candidate.Facts
            .Where(fact =>
                fact.Kind is
                    "endpoint" or
                    "method" or
                    "condition" or
                    "business-predicate" or
                    "boolean-gate" or
                    "guard-condition" or
                    "mapped-field-rule" or
                    "configured-object" or
                    "throw" or
                    "computed-property" or
                    "object-construction" or
                    "loop" or
                    "authorization-policy" or
                    "ui-screen" or
                    "ui-route" or
                    "ui-action" or
                    "ui-api-call" or
                    "ui-result-binding" or
                    "ui-list-render" ||
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

        var semanticCandidate =
            fact.Metadata.TryGetValue("stateMutationCandidate", out var stateCandidate) &&
            stateCandidate == "true";

        var syntacticMemberCandidate =
            !string.IsNullOrWhiteSpace(target) &&
            target.Contains('.', StringComparison.Ordinal);

        if (!semanticCandidate && !syntacticMemberCandidate)
        {
            return false;
        }

        if (IsObjectInitializerLikeMutation(candidate, factsById, fact) ||
            IsImplementationCounterMutation(fact))
        {
            return false;
        }

        if (endpoint is null ||
            !string.Equals(fact.Container, endpoint.Name, StringComparison.Ordinal))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(target) ||
            target.Contains('.', StringComparison.Ordinal))
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
        if (!mutation.Metadata.TryGetValue("target", out var target) ||
            target.Contains('.', StringComparison.Ordinal) ||
            !mutation.Metadata.TryGetValue("targetSymbol", out var targetSymbol))
        {
            return false;
        }

        var sourceRelation = candidate.Relations.FirstOrDefault(relation =>
            relation.Kind == "mutates" &&
            relation.Target == mutation.Id);

        if (sourceRelation is null ||
            !factsById.TryGetValue(sourceRelation.FromFactId, out var sourceMethod) ||
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

        "condition" when fact.Metadata.TryGetValue("expression", out var expression) =>
            $"Condition: {expression}",

        "business-predicate" when fact.Metadata.TryGetValue("expression", out var predicate) =>
            $"Business predicate: {predicate}",

        "boolean-gate" => $"Boolean gate for {fact.Name}",

        "guard-condition" when fact.Metadata.TryGetValue("condition", out var guardCondition) =>
            $"Guard {fact.Metadata.GetValueOrDefault("guard")}: {guardCondition}",

        "mapped-field-rule" => $"Mapping rule for result field {fact.Container}.{fact.Name}",

        "configured-object"
            when fact.Metadata.TryGetValue("source", out var configuredSource) &&
                 fact.Metadata.TryGetValue("assignments", out var configuredAssignments) =>
            $"Configured item in {configuredSource}: {configuredAssignments}",

        "throw" when fact.Metadata.TryGetValue("exceptionType", out var type) =>
            $"Throws {type}",

        "mutation" when fact.Metadata.TryGetValue("target", out var target) =>
            $"Mutation: {target}",

        "computed-property" when fact.Metadata.TryGetValue("expression", out var computed) =>
            $"Computed property {DisplayFact(fact)} = {computed}",

        "object-construction" when fact.Metadata.TryGetValue("assignments", out var assignments) =>
            $"Constructs {fact.Name}: {assignments}",

        "loop" when fact.Metadata.TryGetValue("collection", out var collection) =>
            $"Iterates {collection}",

        "authorization-policy" when fact.Metadata.TryGetValue("definition", out var definition) =>
            $"Authorization policy {fact.Name}: {definition}",

        "ui-screen" => $"UI screen/component {fact.Name}",

        "ui-route" when fact.Metadata.TryGetValue("path", out var path) =>
            $"UI route {path}",

        "ui-action" => $"UI action {fact.Name}",

        "ui-api-call"
            when fact.Metadata.TryGetValue("httpMethod", out var method) &&
                 fact.Metadata.TryGetValue("url", out var url) =>
            $"UI API call {method} {url}",

        "ui-result-binding"
            when fact.Metadata.TryGetValue("apiMethod", out var apiMethod) &&
                 fact.Metadata.TryGetValue("target", out var bindingTarget) =>
            $"UI result binding {apiMethod} -> {bindingTarget}",

        "ui-list-render"
            when fact.Metadata.TryGetValue("item", out var item) &&
                 fact.Metadata.TryGetValue("collection", out var renderCollection) =>
            $"UI list renders {item} from {renderCollection}",

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
        "frontend-ui-not-analyzed" =>
            "Frontend/UI entry point and user interaction path have not been analyzed yet.",

        "delivery-history-not-analyzed" =>
            "Azure DevOps delivery history and product intent have not been analyzed yet.",

        _ => unknown
    };
}
