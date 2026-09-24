namespace Pkc.Core.Discovery;

public sealed record WithheldCount(ScanMode ScanMode, int Files);

/// <summary>
/// Local coverage record for one PKC execution: what the plan offered to semantic scanners, what each stage could
/// execute, and what was withheld and why. Local-only; contains repository-relative paths but never source bodies,
/// configuration values, timestamps or absolute paths.
/// </summary>
public sealed record ScanCoverage(
    string SchemaVersion,
    string PlanFingerprint,
    bool Scoped,
    int Files,
    IReadOnlyList<CoverageCount> FilesByCoverage,
    IReadOnlyList<WithheldCount> WithheldByPlan,
    int ExcludedAreas,
    IReadOnlyList<string> UnknownAreas,
    IReadOnlyList<SemanticStageExecution> Stages)
{
    public const string CurrentSchemaVersion = "0.1.0-scan-coverage";

    public int Coverage(PlanCoverage coverage) =>
        FilesByCoverage.FirstOrDefault(count => count.Coverage == coverage)?.Files ?? 0;

    public int Withheld(ScanMode scanMode) =>
        WithheldByPlan.FirstOrDefault(count => count.ScanMode == scanMode)?.Files ?? 0;

    public static ScanCoverage Build(DiscoveryState state, IReadOnlyList<SemanticStageExecution> stages, bool scoped)
    {
        var withheld = scoped
            ? state.Scope.WithheldFiles
                .GroupBy(file => state.Profile.Classify(file).ScanMode)
                .OrderBy(group => group.Key)
                .Select(group => new WithheldCount(group.Key, group.Count()))
                .ToArray()
            : [];

        return new ScanCoverage(
            CurrentSchemaVersion,
            state.Plan.InputFingerprint,
            scoped,
            state.Plan.Summary.Files,
            state.Plan.Summary.FilesByCoverage,
            withheld,
            state.Plan.Summary.Exclusions,
            state.Plan.UnknownAreas,
            stages);
    }

    /// <summary>
    /// Portable, count-only summary for the generated workspace: no paths, names, source bodies or configuration
    /// values, so it can travel with the workspace without exposing repository structure.
    /// </summary>
    public PortableScanCoverage ToPortable() =>
        new(
            CurrentSchemaVersion,
            Scoped,
            Files,
            Stages.Sum(stage => stage.PlannedFiles),
            Stages.Sum(stage => stage.ExecutableFiles),
            Stages.Sum(stage => stage.WithheldByScannerScope.Count),
            Coverage(PlanCoverage.TestEvidence),
            Coverage(PlanCoverage.Indexed),
            Coverage(PlanCoverage.NotAnalyzable),
            Coverage(PlanCoverage.Unknown),
            UnknownAreas.Count,
            ExcludedAreas,
            "Counts only. Test evidence, indexed (generated, infrastructure, runtime-dependency) and not-analyzable files " +
            "were not semantically analyzed; unknown areas were kept visible but may be incomplete. Answers that depend on " +
            "those areas must say they are not proven.");
}

public sealed record PortableScanCoverage(
    string SchemaVersion,
    bool Scoped,
    int Files,
    int SemanticPlannedFiles,
    int SemanticExecutableFiles,
    int WithheldByScannerScopeFiles,
    int TestEvidenceFiles,
    int IndexedFiles,
    int NotAnalyzableFiles,
    int UnknownFiles,
    int UnknownAreas,
    int ExcludedAreas,
    string Note);
