using System.Text.RegularExpressions;

namespace Pkc.Knowledge;

public sealed partial class ProductFeatureBuilder
{
    private static readonly string[] StatusTerms =
    [
        "status", "complete", "cancel", "reopen", "activate", "deactivate", "approve", "reject",
        "submit", "close", "archive", "restore", "suspend", "resume"
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
        var rules = workflows
            .SelectMany(workflow => workflow.Rules.Select(rule => $"{ActionName(workflow)}: {rule}"))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
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
        value.Contains(term, StringComparison.OrdinalIgnoreCase);

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
