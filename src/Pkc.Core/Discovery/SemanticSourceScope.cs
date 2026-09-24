using System.Collections.Concurrent;

namespace Pkc.Core.Discovery;

/// <summary>
/// Plan-derived boundary for deep semantic scanners. A path is withheld only when discovery positively classified it
/// as safely excluded, generated/light-indexed, a runtime dependency or test evidence. Deep-scan and UNKNOWN paths
/// stay visible, and paths discovery never saw are withheld only when they sit inside a safely excluded area.
/// Scanners consult the ambient scope in addition to their own accepted source scopes; without an ambient scope
/// their behavior is unchanged.
/// </summary>
public sealed class SemanticSourceScope
{
    private static readonly AsyncLocal<SemanticSourceScope?> Ambient = new();

    private static readonly HashSet<ScanMode> WithheldModes =
    [
        ScanMode.SafeAutoExclude,
        ScanMode.LightIndex,
        ScanMode.RuntimeDependencyIndex,
        ScanMode.TestEvidence
    ];

    private readonly RepositoryProfile _profile;
    private readonly HashSet<string> _files;
    private readonly Dictionary<string, string> _filesIgnoreCase;
    private readonly ConcurrentDictionary<string, bool> _decisions = new(StringComparer.Ordinal);

    private SemanticSourceScope(string repositoryPath, RepositoryProfile profile)
    {
        RootPath = Path.GetFullPath(repositoryPath);
        _profile = profile;
        _files = profile.Files.ToHashSet(StringComparer.Ordinal);
        _filesIgnoreCase = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in profile.Files)
        {
            _filesIgnoreCase.TryAdd(file, file);
        }

        WithheldFiles = profile.Files.Where(Withholds).ToArray();
        ExcludedAreas = profile.Areas
            .Where(area => area.ScanMode == ScanMode.SafeAutoExclude)
            .Select(area => area.Path)
            .ToArray();
    }

    public static SemanticSourceScope? Current => Ambient.Value;

    public string RootPath { get; }

    /// <summary>Enumerated files the plan withholds from deep semantic scanners, ordinal-sorted.</summary>
    public IReadOnlyList<string> WithheldFiles { get; }

    /// <summary>Safely excluded areas; discovery never enumerated their contents.</summary>
    public IReadOnlyList<string> ExcludedAreas { get; }

    public static SemanticSourceScope FromProfile(string repositoryPath, RepositoryProfile profile) =>
        new(repositoryPath, profile);

    /// <summary>Makes this scope ambient for the calling flow until the returned handle is disposed.</summary>
    public IDisposable Enter()
    {
        var previous = Ambient.Value;
        Ambient.Value = this;
        return new Restore(previous);
    }

    /// <summary>True when an ambient scope withholds <paramref name="path"/> (absolute or relative to <paramref name="rootPath"/>).</summary>
    public static bool Excludes(string rootPath, string path) =>
        Ambient.Value is { } scope &&
        scope.Withholds(Path.GetRelativePath(scope.RootPath, Path.GetFullPath(Path.Combine(rootPath, path))));

    /// <summary>True when an ambient scope withholds repository-relative <paramref name="relativePath"/>.</summary>
    public static bool Excludes(string relativePath) =>
        Ambient.Value is { } scope && scope.Withholds(relativePath);

    public bool Withholds(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath))
        {
            return false;
        }

        var path = DiscoveryPaths.Normalize(relativePath);
        if (path == DiscoveryPaths.Root || path == ".." || path.StartsWith("../", StringComparison.Ordinal))
        {
            return false;
        }

        return _decisions.GetOrAdd(path, Decide);
    }

    private bool Decide(string path)
    {
        var known = _files.Contains(path) ? path : _filesIgnoreCase.GetValueOrDefault(path);
        if (known is not null)
        {
            return WithheldModes.Contains(_profile.Classify(known).ScanMode);
        }

        // Not enumerated by discovery: only a safely excluded ancestor withholds it; anything else stays visible.
        return _profile.Resolve(path).Nearest.ScanMode == ScanMode.SafeAutoExclude;
    }

    private sealed class Restore(SemanticSourceScope? previous) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Ambient.Value = previous;
        }
    }
}
