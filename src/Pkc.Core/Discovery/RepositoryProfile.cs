using System.Text.Json.Serialization;

namespace Pkc.Core.Discovery;

/// <summary>
/// What a source area is. Kept separate from <see cref="ScanMode"/> so later checkpoints can
/// refine ownership without changing how the area is scanned, and vice versa.
/// </summary>
public enum SourceRole
{
    Unknown,
    TestEvidence,
    GeneratedOrRestorable,
    Infrastructure,
    ToolState
}

/// <summary>
/// How semantic analysis should treat an area. <see cref="DeepScan"/> preserves the existing
/// whole-root semantic behavior for every area that has not been proven otherwise.
/// </summary>
public enum ScanMode
{
    DeepScan,
    LightIndex,
    TestEvidence,
    SafeAutoExclude,
    Unknown
}

public enum DiscoveryConfidence
{
    High,
    Unknown
}

public sealed record DiscoveryEvidence(string Kind, string Path);

public sealed record LanguageCount(string Language, int Files);

public sealed record RoleCount(SourceRole Role, int Files);

public sealed record ScanModeCount(ScanMode ScanMode, int Files);

public sealed record RepositoryManifest(string Path, string Kind);

/// <summary>
/// A file-pattern refinement inside an area. <see cref="Pattern"/> is a glob relative to the owning area.
/// </summary>
public sealed record SourceAreaOverride(
    string Pattern,
    SourceRole Role,
    ScanMode ScanMode,
    DiscoveryConfidence Confidence,
    IReadOnlyList<DiscoveryEvidence> Evidence,
    int FileCount);

public sealed record SourceArea(
    string Path,
    IReadOnlyList<string> Kinds,
    SourceRole Role,
    ScanMode ScanMode,
    DiscoveryConfidence Confidence,
    IReadOnlyList<DiscoveryEvidence> Evidence,
    bool Enumerated,
    int FileCount,
    IReadOnlyList<LanguageCount> Languages,
    IReadOnlyList<SourceAreaOverride> Overrides);

public sealed record RepositoryInventory(
    int Files,
    int Directories,
    IReadOnlyList<LanguageCount> Languages,
    IReadOnlyList<RoleCount> FilesByRole,
    IReadOnlyList<ScanModeCount> FilesByScanMode);

public sealed record FileClassification(
    string AreaPath,
    SourceRole Role,
    ScanMode ScanMode,
    DiscoveryConfidence Confidence);

/// <summary>
/// Deterministic, shallow description of a repository produced before any semantic analysis.
/// All paths are repository-relative with '/' separators; the repository root is ".".
/// </summary>
public sealed record RepositoryProfile(
    string SchemaVersion,
    RepositoryInventory Inventory,
    IReadOnlyList<RepositoryManifest> Manifests,
    IReadOnlyList<SourceArea> Areas,
    IReadOnlyList<string> ContentReads)
{
    // Keyed to the Areas instance so `with { Areas = ... }` copies never reuse a stale lookup.
    private (IReadOnlyList<SourceArea> Source, Dictionary<string, SourceArea> ByPath)? _areaLookup;

    /// <summary>Every enumerated (not safely excluded) file, ordinal-sorted. Local-only; not serialized.</summary>
    [JsonIgnore]
    public IReadOnlyList<string> Files { get; init; } = [];

    public FileClassification Classify(string relativePath)
    {
        var (nearest, overrideOwner, match) = Resolve(relativePath);
        return match is null || overrideOwner is null
            ? new FileClassification(nearest.Path, nearest.Role, nearest.ScanMode, nearest.Confidence)
            : new FileClassification(overrideOwner.Path, match.Role, match.ScanMode, match.Confidence);
    }

    internal (SourceArea Nearest, SourceArea? OverrideOwner, SourceAreaOverride? Override) Resolve(string relativePath)
    {
        if (_areaLookup is not { } lookup || !ReferenceEquals(lookup.Source, Areas))
        {
            lookup = (Areas, Areas.ToDictionary(area => area.Path, StringComparer.Ordinal));
            _areaLookup = lookup;
        }

        var path = DiscoveryPaths.Normalize(relativePath);
        var chain = DiscoveryPaths.SelfAndAncestors(path)
            .Select(candidate => lookup.ByPath.GetValueOrDefault(candidate))
            .OfType<SourceArea>()
            .ToArray();
        var nearest = chain[0];

        if (nearest.ScanMode == ScanMode.SafeAutoExclude || nearest.Role == SourceRole.Infrastructure)
        {
            return (nearest, null, null);
        }

        foreach (var area in chain)
        {
            var withinArea = DiscoveryPaths.RelativeTo(area.Path, path);
            foreach (var candidate in area.Overrides)
            {
                if (DiscoveryGlob.IsMatch(candidate.Pattern, withinArea))
                {
                    return (nearest, area, candidate);
                }
            }
        }

        return (nearest, null, null);
    }
}
