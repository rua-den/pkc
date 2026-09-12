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
        IsExcludedRelativePath(Path.GetRelativePath(rootPath, path));

    public static bool IsExcludedRelativePath(string relativePath) =>
        relativePath
            .Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries)
            .Any(segment => ExcludedDirectoryNames.Contains(segment));
}
