using Pkc.Core.Discovery;

namespace Pkc.CSharp;

internal static class CSharpSourceScope
{
    private static readonly HashSet<string> ExcludedDirectoryNames =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".git",
            ".pkc",
            "bin",
            "obj",
            "knowledge",
            "test",
            "tests",
            "spikes"
        };

    public static bool IsExcluded(string rootPath, string path) =>
        IsExcludedByName(Path.GetRelativePath(rootPath, path)) || SemanticSourceScope.Excludes(rootPath, path);

    public static bool IsExcludedRelativePath(string relativePath) =>
        IsExcludedByName(relativePath) || SemanticSourceScope.Excludes(relativePath);

    /// <summary>The accepted scanner-internal name scope, independent of any scan plan.</summary>
    public static bool IsExcludedByName(string relativePath) =>
        relativePath
            .Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries)
            .Any(segment => ExcludedDirectoryNames.Contains(segment));
}
