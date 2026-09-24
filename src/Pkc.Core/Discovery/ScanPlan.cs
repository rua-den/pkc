using System.Security.Cryptography;
using System.Text;

namespace Pkc.Core.Discovery;

/// <summary>What a scope contributes to coverage once executed.</summary>
public enum PlanCoverage
{
    /// <summary>Deep scan with at least one supported semantic scanner for its files.</summary>
    Semantic,

    /// <summary>Deep scan requested but no semantic scanner supports these file types.</summary>
    NotAnalyzable,

    /// <summary>Runtime-dependency or light index: identity/provenance only, no semantic analysis.</summary>
    Indexed,

    /// <summary>Test evidence: never production authority.</summary>
    TestEvidence,

    /// <summary>Scan decision could not be made; must be reported, never silently dropped.</summary>
    Unknown
}

/// <summary>
/// A set of files sharing one planning decision: area (+ optional override pattern), attributed components and
/// scanner set. Every enumerated file belongs to exactly one scope.
/// </summary>
public sealed record ScanScope(
    string Id,
    string AreaPath,
    string? Pattern,
    SourceRole Role,
    ScanMode ScanMode,
    DiscoveryConfidence Confidence,
    IReadOnlyList<string> Scanners,
    PlanCoverage Coverage,
    IReadOnlyList<string> Components,
    IReadOnlyList<DiscoveryEvidence> Evidence,
    int FileCount,
    IReadOnlyList<string> Files);

public sealed record ScanExclusion(string Path, SourceRole Role, IReadOnlyList<DiscoveryEvidence> Evidence);

/// <summary>
/// A bounded execution unit. Host waves contain the host and every component it owns; shared components appear in
/// each owning wave. Unowned, test and unattributed waves keep everything else visible.
/// </summary>
public sealed record ScanWave(
    string Id,
    string Kind,
    string? Host,
    IReadOnlyList<string> Components,
    IReadOnlyList<string> Scopes,
    int SemanticFiles);

public sealed record CoverageCount(PlanCoverage Coverage, int Files);

public sealed record ScanPlanSummary(
    int Files,
    int SemanticFiles,
    IReadOnlyList<CoverageCount> FilesByCoverage,
    int Exclusions,
    int UnknownAreas,
    int HostWaves);

/// <summary>
/// Deterministic, inspectable scan plan derived only from the repository profile. Local-only; contains paths,
/// classifications and evidence but never source bodies, configuration values, timestamps or absolute paths.
/// </summary>
public sealed record ScanPlan(
    string SchemaVersion,
    string ProfileSchemaVersion,
    string InputFingerprint,
    ScanPlanSummary Summary,
    IReadOnlyList<ScanScope> Scopes,
    IReadOnlyList<ScanExclusion> Exclusions,
    IReadOnlyList<string> UnknownAreas,
    IReadOnlyList<ScanWave> Waves);

public static class ScanPlanner
{
    public const string SchemaVersion = "0.1.0-scan-plan";

    public const string CSharpScanner = "csharp-semantic";
    public const string FrontendScanner = "frontend-semantic";

    private static readonly HashSet<ComponentKind> HostKinds =
    [
        ComponentKind.WebHost,
        ComponentKind.WorkerHost,
        ComponentKind.ExecutableHost,
        ComponentKind.AngularApplication
    ];

    public static ScanPlan Build(string repositoryPath, RepositoryProfile profile)
    {
        var componentsByArea = profile.Components
            .GroupBy(component => component.AreaPath, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(component => component.Id).OrderBy(id => id, StringComparer.Ordinal).ToArray(),
                StringComparer.Ordinal);

        var scopes = profile.Files
            .Select(file =>
            {
                var (nearest, overrideOwner, match) = profile.Resolve(file);
                var components = DiscoveryPaths.SelfAndAncestors(DiscoveryPaths.Parent(file))
                    .Select(directory => componentsByArea.GetValueOrDefault(directory))
                    .FirstOrDefault(ids => ids is not null) ?? [];
                var scanMode = match?.ScanMode ?? nearest.ScanMode;
                var scanners = scanMode == ScanMode.DeepScan ? Scanners(file) : [];
                return (File: file, Nearest: nearest, OverrideOwner: overrideOwner, Match: match, Components: components, Scanners: scanners);
            })
            .GroupBy(entry => ScopeKey(entry.OverrideOwner?.Path ?? entry.Nearest.Path, entry.Match?.Pattern, entry.Components, entry.Scanners),
                StringComparer.Ordinal)
            .Select(group =>
            {
                var first = group.First();
                var area = first.OverrideOwner ?? first.Nearest;
                var role = first.Match?.Role ?? first.Nearest.Role;
                var scanMode = first.Match?.ScanMode ?? first.Nearest.ScanMode;
                var files = group.Select(entry => entry.File).OrderBy(file => file, StringComparer.Ordinal).ToArray();
                return new ScanScope(
                    group.Key,
                    area.Path,
                    first.Match?.Pattern,
                    role,
                    scanMode,
                    first.Match?.Confidence ?? first.Nearest.Confidence,
                    first.Scanners,
                    Coverage(scanMode, first.Scanners),
                    first.Components,
                    first.Match?.Evidence ?? first.Nearest.Evidence,
                    files.Length,
                    files);
            })
            .OrderBy(scope => scope.Id, StringComparer.Ordinal)
            .ToArray();

        var exclusions = profile.Areas
            .Where(area => area.ScanMode == ScanMode.SafeAutoExclude)
            .Select(area => new ScanExclusion(area.Path, area.Role, area.Evidence))
            .ToArray();
        var unknownAreas = profile.Areas
            .Where(area => area.ScanMode == ScanMode.Unknown)
            .Select(area => area.Path)
            .ToArray();

        var waves = Waves(profile, scopes);
        var coverage = scopes
            .GroupBy(scope => scope.Coverage)
            .OrderBy(group => group.Key)
            .Select(group => new CoverageCount(group.Key, group.Sum(scope => scope.FileCount)))
            .ToArray();

        return new ScanPlan(
            SchemaVersion,
            profile.SchemaVersion,
            Fingerprint(repositoryPath, profile),
            new ScanPlanSummary(
                profile.Files.Count,
                scopes.Where(scope => scope.Coverage == PlanCoverage.Semantic).Sum(scope => scope.FileCount),
                coverage,
                exclusions.Length,
                unknownAreas.Length,
                waves.Count(wave => wave.Kind == "host")),
            scopes,
            exclusions,
            unknownAreas,
            waves);
    }

    private static IReadOnlyList<ScanWave> Waves(RepositoryProfile profile, IReadOnlyList<ScanScope> scopes)
    {
        ScanWave Wave(string id, string kind, string? host, IReadOnlyCollection<string> components, IEnumerable<ScanScope> members)
        {
            var included = members.OrderBy(scope => scope.Id, StringComparer.Ordinal).ToArray();
            return new ScanWave(
                id,
                kind,
                host,
                components.OrderBy(component => component, StringComparer.Ordinal).ToArray(),
                included.Select(scope => scope.Id).ToArray(),
                included.Where(scope => scope.Coverage == PlanCoverage.Semantic).Sum(scope => scope.FileCount));
        }

        // Test scopes never join production waves, even when their files sit under a production component.
        var productionScopes = scopes.Where(scope => scope.Coverage != PlanCoverage.TestEvidence).ToArray();
        var waves = new List<ScanWave>();
        foreach (var host in profile.Components
                     .Where(component => HostKinds.Contains(component.Kind))
                     .OrderBy(component => component.Id, StringComparer.Ordinal))
        {
            var members = profile.Components
                .Where(component => component.Owners.Contains(host.Id))
                .Select(component => component.Id)
                .Append(host.Id)
                .ToHashSet(StringComparer.Ordinal);
            waves.Add(Wave($"host:{host.Id}", "host", host.Id, members,
                productionScopes.Where(scope => scope.Components.Any(members.Contains))));
        }

        var unowned = profile.Components
            .Where(component => component.Ownership == OwnershipStatus.Unknown)
            .Select(component => component.Id)
            .ToHashSet(StringComparer.Ordinal);
        waves.Add(Wave("unowned", "unowned", null, unowned, productionScopes.Where(scope => scope.Components.Any(unowned.Contains))));

        var tests = profile.Components
            .Where(component => component.Ownership == OwnershipStatus.TestOnly)
            .Select(component => component.Id)
            .ToHashSet(StringComparer.Ordinal);
        waves.Add(Wave("test", "test", null, tests,
            scopes.Where(scope => scope.Coverage == PlanCoverage.TestEvidence || scope.Components.Any(tests.Contains))));

        waves.Add(Wave("unattributed", "unattributed", null, [], productionScopes.Where(scope => scope.Components.Count == 0)));
        return waves;
    }

    private static IReadOnlyList<string> Scanners(string file) =>
        Path.GetExtension(file).ToLowerInvariant() switch
        {
            ".cs" => [CSharpScanner],
            ".ts" or ".tsx" or ".js" or ".jsx" or ".mjs" or ".cjs" or ".html" or ".htm" => [FrontendScanner],
            _ => []
        };

    private static PlanCoverage Coverage(ScanMode scanMode, IReadOnlyList<string> scanners) =>
        scanMode switch
        {
            ScanMode.DeepScan => scanners.Count > 0 ? PlanCoverage.Semantic : PlanCoverage.NotAnalyzable,
            ScanMode.TestEvidence => PlanCoverage.TestEvidence,
            ScanMode.LightIndex or ScanMode.RuntimeDependencyIndex => PlanCoverage.Indexed,
            _ => PlanCoverage.Unknown
        };

    private static string ScopeKey(string area, string? pattern, IReadOnlyList<string> components, IReadOnlyList<string> scanners)
    {
        var builder = new StringBuilder(area);
        if (pattern is not null)
        {
            builder.Append('#').Append(pattern);
        }

        builder.Append(" [").Append(scanners.Count == 0 ? "-" : string.Join(',', scanners)).Append(']');
        if (components.Count > 0)
        {
            builder.Append(" @").Append(string.Join(',', components));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Staleness fingerprint over the enumerated file paths and the contents discovery actually read
    /// (manifests, byte-compared files, runtime loader files). Source bodies discovery never reads are not tracked.
    /// </summary>
    private static string Fingerprint(string repositoryPath, RepositoryProfile profile)
    {
        var root = Path.GetFullPath(repositoryPath);
        var tracked = profile.ContentReads
            .Concat(profile.ByteComparisons)
            .Concat(profile.Edges.SelectMany(edge => edge.Evidence)
                .Concat(profile.UnresolvedReferences.SelectMany(reference => reference.Evidence))
                .Where(evidence => evidence.Kind == "runtime-loader")
                .Select(evidence => evidence.Path))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(path => path, StringComparer.Ordinal);

        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        void Add(string value) => hash.AppendData(Encoding.UTF8.GetBytes(value + "\n"));

        Add(profile.SchemaVersion);
        foreach (var file in profile.Files)
        {
            Add("file\t" + file);
        }

        foreach (var path in tracked)
        {
            var full = Path.Combine(root, path.Replace('/', Path.DirectorySeparatorChar));
            var content = File.Exists(full) ? Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(full))) : "missing";
            Add("content\t" + path + "\t" + content);
        }

        return "sha256:" + Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }
}
