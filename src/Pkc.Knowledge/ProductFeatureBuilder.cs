using System.Text.RegularExpressions;

namespace Pkc.Knowledge;

public sealed partial class ProductFeatureBuilder
{
    private static readonly string[] StatusTerms =
    [
        "status", "start", "started", "complete", "completed", "cancel", "cancelled", "canceled", "reopen",
        "activate", "deactivate", "approve", "reject", "submit", "close", "archive", "restore", "suspend",
        "resume", "dispatch", "dispatched", "deliver", "delivered", "ship", "shipped"
    ];

    private static readonly string[] ManagementTerms =
    [
        "create", "update", "edit", "delete", "add", "remove", "assign", "unassign", "configure", "set"
    ];

    private static readonly string[] DiscoveryTerms =
    [
        "get", "list", "search", "find", "view", "detail", "export", "report"
    ];

    private static readonly HashSet<string> LowSignalFlowMethods = new(StringComparer.Ordinal)
    {
        "ToString",
        "Parse",
        "TryParse",
        "New",
        "Append",
        "GetHashCode",
        "Equals"
    };

    public ProductFeatureDocument Build(IReadOnlyList<FeatureKnowledge> workflows)
    {
        ArgumentNullException.ThrowIfNull(workflows);

        var groups = workflows
            .GroupBy(workflow => new FeatureKey(workflow.Area, Classify(workflow)), FeatureKeyComparer.Instance)
            .OrderBy(group => group.Key.Area, StringComparer.Ordinal)
            .ThenBy(group => group.Key.Category, StringComparer.Ordinal)
            .Select(BuildFeature)
            .ToArray();

        return new ProductFeatureDocument("0.4.4", groups);
    }

    private static ProductFeature BuildFeature(IGrouping<FeatureKey, FeatureKnowledge> group)
    {
        var workflows = group.OrderBy(item => item.Title, StringComparer.Ordinal).ToArray();
        var category = group.Key.Category;
        var featureId = $"product-feature:{Slug(group.Key.Area)}:{category}";
        var title = category switch
        {
            "status-management" => $"{group.Key.Area} Status Management",
            "management" => $"{group.Key.Area} Management",
            "discovery" => $"{group.Key.Area} Discovery",
            _ => $"{group.Key.Area} Operations"
        };

        var references = workflows.Select(workflow => new ProductWorkflowReference(
            workflow.Id,
            workflow.Title,
            WorkflowPath(workflow),
            workflow.Coverage,
            workflow.UiSteps,
            workflow.EntryPoints,
            workflow.Permissions,
            workflow.Rules,
            workflow.StateChanges,
            workflow.SideEffects,
            BuildProductFlow(workflow.Flow),
            workflow.Unknowns)).ToArray();

        var coverage = workflows.SelectMany(item => item.Coverage).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
        var permissions = workflows.SelectMany(item => item.Permissions).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
        var rules = BuildProductRules(workflows);
        var unknowns = workflows.SelectMany(item => item.Unknowns).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();

        var summary = $"Observed product feature synthesized from {workflows.Length} {Pluralize("workflow", workflows.Length)} in the {group.Key.Area} area.";

        return new ProductFeature(
            featureId,
            title,
            group.Key.Area,
            category,
            "code-observed",
            coverage,
            summary,
            references,
            permissions,
            rules,
            unknowns);
    }

    private static IReadOnlyList<string> BuildProductRules(IReadOnlyList<FeatureKnowledge> workflows)
    {
        var occurrences = workflows
            .SelectMany(workflow => workflow.Rules
                .Where(IsProductImpactingRule)
                .Select(rule => new RuleOccurrence(ActionName(workflow), rule)))
            .ToArray();

        return occurrences
            .GroupBy(item => item.Rule, StringComparer.Ordinal)
            .Select(group =>
            {
                var actions = group.Select(item => item.Action).Distinct(StringComparer.Ordinal).ToArray();
                return actions.Length > 1
                    ? group.Key
                    : $"{actions[0]}: {group.Key}";
            })
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<string> BuildProductFlow(IReadOnlyList<string> flow)
    {
        var edges = flow
            .Select(ParseFlow)
            .Where(item => item is not null)
            .Cast<FlowEdge>()
            .Where(IsProductFlowEdge)
            .Distinct()
            .ToArray();

        var distances = EntryPointDistances(edges);
        var outgoing = edges
            .GroupBy(edge => edge.Source, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
        var incoming = edges
            .GroupBy(edge => edge.Target, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);

        return edges
            .OrderByDescending(edge => FlowScore(edge, distances, outgoing, incoming))
            .ThenBy(edge => distances.GetValueOrDefault(edge.Source, int.MaxValue))
            .ThenBy(item => item.Source, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .Take(16)
            .Select(item => $"{item.Source} → {item.Target}")
            .ToArray();
    }

    private static FlowEdge? ParseFlow(string value)
    {
        var separator = value.IndexOf(" → ", StringComparison.Ordinal);
        if (separator <= 0 || separator + 3 >= value.Length)
        {
            return null;
        }

        return new FlowEdge(value[..separator].Trim(), value[(separator + 3)..].Trim());
    }

    private static bool IsProductFlowEdge(FlowEdge edge)
    {
        var targetMethod = MethodName(edge.Target);
        if (LowSignalFlowMethods.Contains(targetMethod))
        {
            return false;
        }

        var sourceOwner = SymbolOwner(edge.Source);
        var targetOwner = SymbolOwner(edge.Target);
        if (!string.IsNullOrWhiteSpace(sourceOwner) &&
            string.Equals(sourceOwner, targetOwner, StringComparison.Ordinal))
        {
            return false;
        }

        return true;
    }

    private static IReadOnlyDictionary<string, int> EntryPointDistances(IReadOnlyList<FlowEdge> edges)
    {
        var adjacency = edges
            .GroupBy(edge => edge.Source, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(edge => edge.Target).Distinct(StringComparer.Ordinal).ToArray(),
                StringComparer.Ordinal);
        var distances = new Dictionary<string, int>(StringComparer.Ordinal);
        var queue = new Queue<string>();

        foreach (var source in edges.Select(edge => edge.Source)
                     .Where(LooksLikeEntryPoint)
                     .Distinct(StringComparer.Ordinal))
        {
            distances[source] = 0;
            queue.Enqueue(source);
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!adjacency.TryGetValue(current, out var targets))
            {
                continue;
            }

            foreach (var target in targets)
            {
                var distance = distances[current] + 1;
                if (distances.TryGetValue(target, out var existing) && existing <= distance)
                {
                    continue;
                }

                distances[target] = distance;
                queue.Enqueue(target);
            }
        }

        return distances;
    }

    private static int FlowScore(
        FlowEdge edge,
        IReadOnlyDictionary<string, int> distances,
        IReadOnlyDictionary<string, int> outgoing,
        IReadOnlyDictionary<string, int> incoming)
    {
        var score = 0;
        if (LooksLikeEntryPoint(edge.Source))
        {
            score += 200;
        }

        if (distances.TryGetValue(edge.Source, out var distance))
        {
            score += Math.Max(0, 100 - (distance * 20));
        }

        if (CrossesComponentBoundary(edge))
        {
            score += 40;
        }

        if (outgoing.ContainsKey(edge.Target))
        {
            score += 15;
        }

        if (incoming.GetValueOrDefault(edge.Target) > 1)
        {
            score += 5;
        }

        return score;
    }

    private static bool CrossesComponentBoundary(FlowEdge edge)
    {
        var source = ComponentKey(edge.Source);
        var target = ComponentKey(edge.Target);
        return !string.IsNullOrWhiteSpace(source) &&
               !string.IsNullOrWhiteSpace(target) &&
               !string.Equals(source, target, StringComparison.Ordinal);
    }

    private static string ComponentKey(string value)
    {
        if (LooksLikeEntryPoint(value))
        {
            return value.Split('.', 2)[0];
        }

        var owner = SymbolOwner(value);
        var separator = owner.IndexOf('.');
        return separator > 0 ? owner[..separator] : owner;
    }

    private static bool LooksLikeEntryPoint(string value) =>
        value.Contains(".GET /", StringComparison.Ordinal) ||
        value.Contains(".POST /", StringComparison.Ordinal) ||
        value.Contains(".PUT /", StringComparison.Ordinal) ||
        value.Contains(".PATCH /", StringComparison.Ordinal) ||
        value.Contains(".DELETE /", StringComparison.Ordinal);

    private static string SymbolOwner(string value)
    {
        if (LooksLikeEntryPoint(value))
        {
            return value;
        }

        var lastDot = value.LastIndexOf('.');
        return lastDot > 0 ? value[..lastDot] : value;
    }

    private static string MethodName(string value)
    {
        var lastDot = value.LastIndexOf('.');
        return lastDot >= 0 && lastDot + 1 < value.Length
            ? value[(lastDot + 1)..]
            : value;
    }

    private static bool IsProductImpactingRule(string rule)
    {
        if (string.IsNullOrWhiteSpace(rule) ||
            rule.StartsWith("Iterates `", StringComparison.Ordinal))
        {
            return false;
        }

        if (!rule.StartsWith("Condition observed:", StringComparison.Ordinal))
        {
            return true;
        }

        return rule.Contains("=> return ", StringComparison.Ordinal) ||
               rule.Contains("endpoint is registered only when", StringComparison.Ordinal) ||
               rule.Contains(".Status", StringComparison.OrdinalIgnoreCase) ||
               rule.Contains(" status ", StringComparison.OrdinalIgnoreCase) ||
               rule.Contains("IsFinal", StringComparison.Ordinal) ||
               rule.Contains("IsCompleted", StringComparison.Ordinal) ||
               rule.Contains("IsCancelled", StringComparison.Ordinal) ||
               rule.Contains("IsCanceled", StringComparison.Ordinal);
    }

    private static string Classify(FeatureKnowledge workflow)
    {
        var value = ActionName(workflow).ToLowerInvariant();
        if (StatusTerms.Any(term => ContainsWord(value, term))) return "status-management";
        if (ManagementTerms.Any(term => ContainsWord(value, term))) return "management";
        if (DiscoveryTerms.Any(term => ContainsWord(value, term))) return "discovery";
        return "operations";
    }

    private static string ActionName(FeatureKnowledge workflow)
    {
        var prefix = workflow.Area + " ";
        return workflow.Title.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? workflow.Title[prefix.Length..]
            : workflow.Title;
    }

    private static bool ContainsWord(string value, string term) =>
        Regex.IsMatch(value, $@"\b{Regex.Escape(term)}\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static string WorkflowPath(FeatureKnowledge workflow)
    {
        var action = workflow.Id.Split(':', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? workflow.Title;
        return $"knowledge/workflows/{Slug(workflow.Area)}/{Slug(action)}.md";
    }

    private static string Pluralize(string value, int count) => count == 1 ? value : value + "s";

    private static string Slug(string value)
    {
        var slug = NonSlugRegex().Replace(value.ToLowerInvariant(), "-").Trim('-');
        return slug.Length == 0 ? "unknown" : slug;
    }

    private sealed record FeatureKey(string Area, string Category);
    private sealed record RuleOccurrence(string Action, string Rule);
    private sealed record FlowEdge(string Source, string Target);

    private sealed class FeatureKeyComparer : IEqualityComparer<FeatureKey>
    {
        public static FeatureKeyComparer Instance { get; } = new();

        public bool Equals(FeatureKey? x, FeatureKey? y) =>
            x is not null && y is not null &&
            string.Equals(x.Area, y.Area, StringComparison.Ordinal) &&
            string.Equals(x.Category, y.Category, StringComparison.Ordinal);

        public int GetHashCode(FeatureKey obj) => HashCode.Combine(obj.Area, obj.Category);
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonSlugRegex();
}
