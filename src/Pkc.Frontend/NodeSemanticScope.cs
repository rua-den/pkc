using System.Diagnostics;
using System.Text.Json;
using Pkc.Core.Discovery;

namespace Pkc.Frontend;

/// <summary>
/// Hands the ambient plan boundary to node-based TypeScript analyzers, which walk the repository themselves.
/// Without an ambient scope nothing is written and the analyzers keep their accepted behavior.
/// </summary>
internal static class NodeSemanticScope
{
    public const string EnvironmentVariable = "PKC_SEMANTIC_SCOPE";

    /// <summary>Returns the temporary scope file the caller must delete, or null when no scope is ambient.</summary>
    public static string? Apply(ProcessStartInfo startInfo, string rootPath)
    {
        if (SemanticSourceScope.Current is not { } scope)
        {
            return null;
        }

        var root = Path.GetFullPath(rootPath);
        string Relative(string path) =>
            Path.GetRelativePath(root, Path.Combine(scope.RootPath, path.Replace('/', Path.DirectorySeparatorChar)))
                .Replace('\\', '/');

        var payload = new
        {
            excludedAreas = scope.ExcludedAreas.Select(Relative).ToArray(),
            withheldFiles = scope.WithheldFiles
                .Where(path => path.EndsWith(".ts", StringComparison.OrdinalIgnoreCase))
                .Select(Relative)
                .ToArray()
        };

        var scopePath = Path.Combine(Path.GetTempPath(), $"pkc-semantic-scope-{Guid.NewGuid():N}.json");
        File.WriteAllText(scopePath, JsonSerializer.Serialize(payload));
        startInfo.Environment[EnvironmentVariable] = scopePath;
        return scopePath;
    }

    public static void TryDelete(string? path)
    {
        if (path is null)
        {
            return;
        }

        try
        {
            File.Delete(path);
        }
        catch
        {
            // Best-effort cleanup of a temporary scope file.
        }
    }
}
