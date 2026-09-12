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

    public ProductFeatureDocument Build(IReadOnlyList<FeatureKnowledge> workflows)
    {
        ArgumentNullException.ThrowIfNull(workflows);

        var groups = workflows
            .GroupBy(workflow => new FeatureKey(workflow.Area, Classify(workflow)), FeatureKeyComparer.Instance)
            .OrderBy(group => group.Key.Area, StringComparer.Ordinal)
            .ThenBy(group => group.Key.Category, StringComparer.Ordinal)
            .Select(BuildFeature)
            .ToArray();

        return new ProductFeatureDocument("0.4.0", groups);
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
