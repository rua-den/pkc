namespace Pkc.Knowledge;

public sealed class AiWorkspaceWriter
{
    public const string WorkspaceDirectoryName = "workspace";

    public async Task<string> WriteAsync(
        string repositoryPath,
        IReadOnlyDictionary<string, string> files,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryPath);
        ArgumentNullException.ThrowIfNull(files);

        var repositoryRoot = Path.GetFullPath(repositoryPath);
        var workspaceRoot = Path.GetFullPath(Path.Combine(
            repositoryRoot,
            ".pkc",
            WorkspaceDirectoryName));

        if (Directory.Exists(workspaceRoot))
        {
            Directory.Delete(workspaceRoot, recursive: true);
        }

        Directory.CreateDirectory(workspaceRoot);

        foreach (var pair in files.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var outputPath = ResolveInsideWorkspace(workspaceRoot, pair.Key);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            await File.WriteAllTextAsync(outputPath, pair.Value, cancellationToken);
        }

        return workspaceRoot;
    }

    private static string ResolveInsideWorkspace(string workspaceRoot, string relativePath)
    {
        if (Path.IsPathRooted(relativePath))
        {
            throw new InvalidOperationException($"Workspace path must be relative: {relativePath}");
        }

        var normalized = relativePath.Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(workspaceRoot, normalized));
        var rootWithSeparator = workspaceRoot.TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Workspace path escapes generated root: {relativePath}");
        }

        return fullPath;
    }
}
