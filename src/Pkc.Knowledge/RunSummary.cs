using System.Globalization;
using System.Text;

namespace Pkc.Knowledge;

public sealed record RunRepositoryStats(
    int Files,
    int Hosts,
    int OwnedComponents,
    int TestProjects,
    int UnknownOwnershipComponents,
    int PlannedSemanticFiles,
    int ExecutedSemanticFiles,
    int WithheldByScannerScopeFiles,
    int TestEvidenceFiles,
    int IndexedFiles,
    int NotAnalyzableFiles,
    int ExcludedAreas,
    int UnknownAreas);

public sealed record KnowledgeGrounding(
    int Workflows,
    int WithRules,
    int WithPermissions,
    int WithStateChanges,
    int WithSideEffects,
    int WithUiSteps,
    int WithUiToBackend)
{
    private const string Placeholder = "No grounded information available yet.";

    public static KnowledgeGrounding From(IReadOnlyCollection<FeatureKnowledge> workflows)
    {
        static bool Has(IReadOnlyList<string> items) =>
            items.Any(item => !string.IsNullOrWhiteSpace(item) && !item.Contains(Placeholder, StringComparison.Ordinal));

        return new KnowledgeGrounding(
            workflows.Count,
            workflows.Count(workflow => Has(workflow.Rules)),
            workflows.Count(workflow => Has(workflow.Permissions)),
            workflows.Count(workflow => Has(workflow.StateChanges)),
            workflows.Count(workflow => Has(workflow.SideEffects)),
            workflows.Count(workflow => Has(workflow.UiSteps)),
            workflows.Count(workflow => Has(workflow.UiToBackend)));
    }
}

public sealed record AreaCount(string Area, int Workflows);

public sealed record PhaseTiming(string Phase, double Seconds);

public sealed record RunArtifact(string Name, string Path, bool Written, string? Error);

/// <summary>
/// What one PKC run produced, in one place: repository shape, analysis coverage, evidence volume, knowledge output,
/// grounding quality, timings and artifacts. Paths are relative to the repository root; no source bodies or
/// configuration values.
/// </summary>
public sealed record RunSummary(
    string SchemaVersion,
    string Command,
    bool Scoped,
    DateTimeOffset? ResumedFromScanUtc,
    RunRepositoryStats Repository,
    int Facts,
    int Relations,
    int WorkflowCandidates,
    int ProductFeatures,
    int Areas,
    int KnowledgeFiles,
    int WorkspaceFiles,
    KnowledgeGrounding Grounding,
    IReadOnlyList<AreaCount> TopAreas,
    IReadOnlyList<PhaseTiming> Timings,
    IReadOnlyList<RunArtifact> Artifacts)
{
    public const string CurrentSchemaVersion = "0.1.0-run-summary";
    public const string MarkdownRelativePath = ".pkc/RUN_SUMMARY.md";
    public const string JsonRelativePath = ".pkc/run-summary.json";

    /// <summary>Where the generated workspace ended up; <c>.pkc/workspace.new</c> when it could not replace the current one.</summary>
    public string WorkspaceRelativePath { get; init; } = ".pkc/workspace";

    public double TotalSeconds => Timings.Sum(timing => timing.Seconds);

    public static IReadOnlyList<AreaCount> TopAreasOf(IEnumerable<FeatureKnowledge> workflows, int take = 10) =>
        workflows
            .GroupBy(workflow => workflow.Area, StringComparer.Ordinal)
            .Select(group => new AreaCount(group.Key, group.Count()))
            .OrderByDescending(area => area.Workflows)
            .ThenBy(area => area.Area, StringComparer.Ordinal)
            .Take(take)
            .ToArray();

    public string RenderConsole()
    {
        var r = Repository;
        var g = Grounding;
        // ASCII only: Windows consoles often cannot display box-drawing characters.
        var builder = new StringBuilder();
        builder.AppendLine();
        builder.AppendLine("======================== PKC run summary ========================");
        Line(builder, "Repository", $"{N(r.Files)} files | {N(r.Hosts)} apps | {N(r.OwnedComponents)} owned libraries | {N(r.TestProjects)} test projects");
        Line(builder, "Analyzed", $"{N(r.ExecutedSemanticFiles)} of {N(r.PlannedSemanticFiles)} planned files" +
                                  $" (skipped: {N(r.TestEvidenceFiles)} test | {N(r.IndexedFiles)} generated/vendor | {N(r.ExcludedAreas)} build/restore areas)");
        Line(builder, "Evidence", ResumedFromScanUtc is { } resumed
            ? $"{N(Facts)} facts | {N(Relations)} relations (reused scan from {resumed.ToLocalTime():yyyy-MM-dd HH:mm})"
            : $"{N(Facts)} facts | {N(Relations)} relations");
        Line(builder, "Knowledge", $"{N(ProductFeatures)} product features | {N(g.Workflows)} workflows | {N(Areas)} areas");
        Line(builder, "Grounded", $"rules {Pct(g.WithRules, g.Workflows)} | permissions {Pct(g.WithPermissions, g.Workflows)} | " +
                                  $"state changes {Pct(g.WithStateChanges, g.Workflows)} | UI steps {Pct(g.WithUiSteps, g.Workflows)}");
        var phases = string.Join(" | ", Timings.Where(t => t.Seconds >= 1).Select(t => $"{t.Phase} {Duration(t.Seconds)}"));
        Line(builder, "Time", phases.Length == 0 ? Duration(TotalSeconds) : $"{Duration(TotalSeconds)} ({phases})");
        Line(builder, "Workspace", $"{WorkspaceRelativePath} ({N(WorkspaceFiles)} files)");
        Line(builder, "Summary", $"{MarkdownRelativePath} | {JsonRelativePath}");
        foreach (var failed in Artifacts.Where(artifact => !artifact.Written))
        {
            Line(builder, "Warning", $"{failed.Name} not written: {failed.Error}");
        }

        builder.AppendLine("=================================================================");
        return builder.ToString();
    }

    public string RenderMarkdown(string repositoryLabel)
    {
        var r = Repository;
        var g = Grounding;
        var builder = new StringBuilder();
        builder.AppendLine($"# PKC run summary — `{repositoryLabel}`");
        builder.AppendLine();
        builder.AppendLine($"Command `pkc {Command}` · plan scope {(Scoped ? "applied" : "not applied (whole repository)")} · total {Duration(TotalSeconds)}" +
                           (ResumedFromScanUtc is { } resumed ? $" · **resumed** from the scan of {resumed.ToLocalTime():yyyy-MM-dd HH:mm}" : string.Empty));
        builder.AppendLine();
        builder.AppendLine("## At a glance");
        builder.AppendLine();
        builder.AppendLine("| | |");
        builder.AppendLine("| --- | --- |");
        builder.AppendLine($"| Repository | {N(r.Files)} files · {N(r.Hosts)} apps/hosts · {N(r.OwnedComponents)} owned libraries · {N(r.TestProjects)} test projects |");
        builder.AppendLine($"| Analyzed deeply | {N(r.ExecutedSemanticFiles)} of {N(r.PlannedSemanticFiles)} planned files |");
        builder.AppendLine($"| Evidence extracted | {N(Facts)} facts · {N(Relations)} relations |");
        builder.AppendLine($"| Knowledge generated | {N(ProductFeatures)} product features · {N(g.Workflows)} workflows · {N(Areas)} areas |");
        builder.AppendLine($"| Workspace | `{WorkspaceRelativePath}` — {N(WorkspaceFiles)} files ({N(KnowledgeFiles)} knowledge files) |");
        builder.AppendLine();
        builder.AppendLine("## What was analyzed");
        builder.AppendLine();
        builder.AppendLine("| Area of the repository | Files / areas | Treatment |");
        builder.AppendLine("| --- | ---: | --- |");
        builder.AppendLine($"| Own code (planned for deep analysis) | {N(r.PlannedSemanticFiles)} | analyzed ({N(r.ExecutedSemanticFiles)}), {N(r.WithheldByScannerScopeFiles)} skipped by scanner rules |");
        builder.AppendLine($"| Tests | {N(r.TestEvidenceFiles)} | kept out of product knowledge |");
        builder.AppendLine($"| Generated / vendor / infrastructure | {N(r.IndexedFiles)} | indexed only |");
        builder.AppendLine($"| Other files without a supported analyzer | {N(r.NotAnalyzableFiles)} | not analyzed |");
        builder.AppendLine($"| Build / restore output | {N(r.ExcludedAreas)} areas | skipped |");
        builder.AppendLine($"| Unknown areas | {N(r.UnknownAreas)} | kept visible |");
        builder.AppendLine();
        builder.AppendLine("## How well the knowledge is grounded");
        builder.AppendLine();
        builder.AppendLine("Share of workflows whose knowledge contains grounded evidence for each aspect. Low numbers mean the AI will answer \"not proven\" for that kind of question.");
        builder.AppendLine();
        builder.AppendLine("| Aspect | Workflows | Share |");
        builder.AppendLine("| --- | ---: | ---: |");
        Row(builder, "Business rules / conditions", g.WithRules, g.Workflows);
        Row(builder, "Permissions", g.WithPermissions, g.Workflows);
        Row(builder, "State / data changes", g.WithStateChanges, g.Workflows);
        Row(builder, "Side effects", g.WithSideEffects, g.Workflows);
        Row(builder, "UI steps", g.WithUiSteps, g.Workflows);
        Row(builder, "UI → backend link", g.WithUiToBackend, g.Workflows);
        builder.AppendLine();
        builder.AppendLine("## Largest areas");
        builder.AppendLine();
        builder.AppendLine("| Area | Workflows |");
        builder.AppendLine("| --- | ---: |");
        foreach (var area in TopAreas)
        {
            builder.AppendLine($"| {area.Area} | {N(area.Workflows)} |");
        }

        builder.AppendLine();
        builder.AppendLine("## Time");
        builder.AppendLine();
        builder.AppendLine("| Phase | Duration |");
        builder.AppendLine("| --- | ---: |");
        foreach (var timing in Timings)
        {
            builder.AppendLine($"| {timing.Phase} | {Duration(timing.Seconds)} |");
        }

        builder.AppendLine();
        builder.AppendLine("## Files produced");
        builder.AppendLine();
        builder.AppendLine("| Output | Path | Status |");
        builder.AppendLine("| --- | --- | --- |");
        foreach (var artifact in Artifacts)
        {
            builder.AppendLine($"| {artifact.Name} | `{artifact.Path}` | {(artifact.Written ? "written" : "**not written** — " + artifact.Error)} |");
        }

        builder.AppendLine();
        builder.AppendLine("## Use it");
        builder.AppendLine();
        builder.AppendLine($"Open `{WorkspaceRelativePath}` (not the repository root) in the company-approved AI assistant and ask product or QA questions.");
        builder.AppendLine("If a later step failed after scanning, re-run with `--resume` to reuse this scan.");
        return builder.ToString();
    }

    private static void Line(StringBuilder builder, string label, string value) =>
        builder.AppendLine($"  {label,-11}{value}");

    private static void Row(StringBuilder builder, string label, int count, int total) =>
        builder.AppendLine($"| {label} | {N(count)} | {Pct(count, total)} |");

    internal static string N(int value) => value.ToString("N0", CultureInfo.InvariantCulture);

    internal static string Pct(int count, int total) =>
        total == 0 ? "0%" : (100.0 * count / total).ToString("0", CultureInfo.InvariantCulture) + "%";

    internal static string Duration(double seconds)
    {
        var span = TimeSpan.FromSeconds(Math.Round(seconds));
        return span.TotalHours >= 1
            ? $"{(int)span.TotalHours}h {span.Minutes:D2}m"
            : span.TotalMinutes >= 1 ? $"{(int)span.TotalMinutes}m {span.Seconds:D2}s" : $"{span.Seconds}s";
    }
}

/// <summary>Count-only view of the run for the portable workspace: no paths, timings or repository names.</summary>
public sealed record WorkspaceOverview(
    int ProductFeatures,
    int Workflows,
    int Areas,
    KnowledgeGrounding Grounding,
    IReadOnlyList<AreaCount> TopAreas,
    int AnalyzedFiles,
    int TestEvidenceFiles,
    int NotAnalyzedFiles,
    int UnknownAreas)
{
    public static WorkspaceOverview From(RunSummary summary) =>
        new(
            summary.ProductFeatures,
            summary.Grounding.Workflows,
            summary.Areas,
            summary.Grounding,
            summary.TopAreas,
            summary.Repository.ExecutedSemanticFiles,
            summary.Repository.TestEvidenceFiles,
            summary.Repository.NotAnalyzableFiles + summary.Repository.IndexedFiles + summary.Repository.WithheldByScannerScopeFiles,
            summary.Repository.UnknownAreas);
}
