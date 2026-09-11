using System.Text;
using System.Text.RegularExpressions;

namespace Pkc.Knowledge;

public sealed partial class MarkdownKnowledgeRenderer
{
    public string GetRelativePath(FeatureKnowledge knowledge)
    {
        ArgumentNullException.ThrowIfNull(knowledge);
        var action = knowledge.Id.Split(':', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? knowledge.Title;
        return $"knowledge/workflows/{Slug(knowledge.Area)}/{Slug(action)}.md";
    }

    public string Render(FeatureKnowledge knowledge)
    {
        ArgumentNullException.ThrowIfNull(knowledge);

        var builder = new StringBuilder();
        builder.AppendLine("---");
        builder.AppendLine($"id: {Yaml(knowledge.Id)}");
        builder.AppendLine($"title: {Yaml(knowledge.Title)}");
        builder.AppendLine($"area: {Yaml(knowledge.Area)}");
        builder.AppendLine("type: \"workflow\"");
        builder.AppendLine($"authority: {Yaml(knowledge.Authority)}");
        builder.AppendLine("generated: true");
        builder.AppendLine("coverage:");
        foreach (var coverage in knowledge.Coverage)
        {
            builder.AppendLine($"  - {coverage}");
        }
        builder.AppendLine("---");
        builder.AppendLine();
        builder.AppendLine($"# {knowledge.Title}");
        builder.AppendLine();
        builder.AppendLine("> This workflow describes behavior observed in the current implementation. It is not yet business-approved truth.");
        builder.AppendLine();
        builder.AppendLine("## What this workflow represents");
        builder.AppendLine();
        builder.AppendLine(knowledge.Summary);
        builder.AppendLine();

        AppendListSection(builder, "How to do it in the UI", knowledge.UiSteps);
        AppendListSection(builder, "UI to backend", knowledge.UiToBackend);
        AppendListSection(builder, "Backend entry point", knowledge.EntryPoints);
        AppendListSection(builder, "Permissions", knowledge.Permissions);
        AppendListSection(builder, "Observed business rules", knowledge.Rules);
        AppendListSection(builder, "State changes", knowledge.StateChanges);
        AppendListSection(builder, "Side effects", knowledge.SideEffects);
        AppendListSection(builder, "Backend flow", knowledge.Flow);
        AppendListSection(builder, "Important unknowns", knowledge.Unknowns);

        builder.AppendLine("## Evidence");
        builder.AppendLine();
        if (knowledge.Evidence.Count == 0)
        {
            builder.AppendLine("- No source evidence recorded.");
        }
        else
        {
            foreach (var evidence in knowledge.Evidence)
            {
                builder.AppendLine($"- `{evidence.Source.Path}:L{evidence.Source.StartLine}-L{evidence.Source.EndLine}` — {evidence.Description}");
            }
        }

        return builder.ToString();
    }

    private static void AppendListSection(StringBuilder builder, string title, IReadOnlyList<string> items)
    {
        builder.AppendLine($"## {title}");
        builder.AppendLine();

        if (items.Count == 0)
        {
            builder.AppendLine("- No grounded information available yet.");
        }
        else
        {
            foreach (var item in items)
            {
                builder.AppendLine($"- {item}");
            }
        }

        builder.AppendLine();
    }

    private static string Yaml(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";

    private static string Slug(string value)
    {
        var slug = NonSlugRegex().Replace(value.ToLowerInvariant(), "-").Trim('-');
        return slug.Length == 0 ? "unknown" : slug;
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonSlugRegex();
}
