using Pkc.Core.Discovery;

namespace Pkc.Frontend;

internal static class FrontendSourceScope
{
    private static readonly HashSet<string> ExcludedDirectoryNames =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".git",
            ".pkc",
            "bin",
            "obj",
            "node_modules",
            "dist",
            "build",
            "coverage",
            "knowledge",
            "test",
            "tests",
            "__tests__",
            "spike",
            "spikes"
        };

    private static readonly string[] ExcludedFileSuffixes =
    [
        ".spec.ts",
        ".spec.tsx",
        ".spec.js",
        ".spec.jsx",
        ".test.ts",
        ".test.tsx",
        ".test.js",
        ".test.jsx"
    ];

    public static bool IsProductSource(string relativePath) =>
        IsProductSourceByName(relativePath) && !SemanticSourceScope.Excludes(relativePath);

    /// <summary>The accepted scanner-internal product-source scope, independent of any scan plan.</summary>
    public static bool IsProductSourceByName(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return false;
        }

        var normalized = relativePath.Replace('\\', '/');
        if (normalized.Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Any(segment => ExcludedDirectoryNames.Contains(segment)))
        {
            return false;
        }

        var fileName = normalized.Split('/').LastOrDefault() ?? normalized;
        return !ExcludedFileSuffixes.Any(suffix =>
            fileName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
    }

    internal static bool IsExcludedDirectoryName(string name) =>
        ExcludedDirectoryNames.Contains(name);
}
