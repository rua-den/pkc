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
    ToolState,

    /// <summary>Declared by a restore manifest and byte-identical to (or listed by) the declared distribution.</summary>
    ThirdPartyRuntime,

    /// <summary>At a declared vendor content path but different from the declared distribution.</summary>
    ThirdPartyModified,

    /// <summary>Inside a declared vendor destination whose exact file set is not declared.</summary>
    ThirdPartyUnknown
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
    Unknown,

    /// <summary>Runtime-visible dependency: identity/ownership indexed, internals not deep-scanned.</summary>
    RuntimeDependencyIndex
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

public enum ComponentKind
{
    Unknown,
    WebHost,
    WorkerHost,
    ExecutableHost,
    Library,
    TestProject,
    AngularApplication,
    AngularLibrary
}

public enum OwnershipStatus
{
    /// <summary>No production host provably reaches this component.</summary>
    Unknown,

    /// <summary>A deployable/runtime host; it owns itself.</summary>
    Host,

    /// <summary>At least one production host reaches it through proven references.</summary>
    Owned,

    /// <summary>Test evidence; never a production owner or owned node.</summary>
    TestOnly
}

/// <summary>
/// A buildable unit declared by a manifest (.NET project or Angular workspace project).
/// <see cref="Owners"/> lists production host component ids that reach this component through
/// unconditional, resolved project references; tests and similar names never contribute.
/// </summary>
public sealed record RepositoryComponent(
    string Id,
    string Manifest,
    string AreaPath,
    ComponentKind Kind,
    SourceRole Role,
    DiscoveryConfidence Confidence,
    IReadOnlyList<DiscoveryEvidence> Evidence,
    IReadOnlyList<string> Solutions,
    OwnershipStatus Ownership,
    IReadOnlyList<string> Owners,
    IReadOnlyList<string> TestReferences);

public sealed record ComponentEdge(
    string From,
    string To,
    string Kind,
    DiscoveryConfidence Confidence,
    IReadOnlyList<DiscoveryEvidence> Evidence);

public sealed record UnresolvedReference(
    string From,
    string Reference,
    string Reason,
    IReadOnlyList<DiscoveryEvidence> Evidence);

/// <summary>A tracked file proven to be generated from other repository files (bundle output or source-mapped output).</summary>
public sealed record GeneratedArtifact(
    string Path,
    string Kind,
    IReadOnlyList<string> Inputs,
    IReadOnlyList<string> UnresolvedInputs,
    IReadOnlyList<DiscoveryEvidence> Evidence);

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
    IReadOnlyList<RepositoryComponent> Components,
    IReadOnlyList<ComponentEdge> Edges,
    IReadOnlyList<UnresolvedReference> UnresolvedReferences,
    IReadOnlyList<GeneratedArtifact> GeneratedArtifacts,
    IReadOnlyList<string> ByteComparisons,
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
