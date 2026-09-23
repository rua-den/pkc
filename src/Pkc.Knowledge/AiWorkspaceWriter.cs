using System.Text.Json;

namespace Pkc.Knowledge;

public sealed class AiWorkspaceWriter
{
    public const string WorkspaceDirectoryName = "workspace";
    public const string GitIsolationRelativePath = "_meta/git-isolation.json";
    public const string SourceContextRelativePath = "_meta/source-context.json";

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

        var gitIsolation = new PkcLocalGitExclude().EnsureIgnored(repositoryRoot);
        var gitIsolationPath = ResolveInsideWorkspace(workspaceRoot, GitIsolationRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(gitIsolationPath)!);
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        await File.WriteAllTextAsync(
            gitIsolationPath,
            JsonSerializer.Serialize(
                new
                {
                    schemaVersion = "0.1-preview",
                    generatedOutput = ".pkc/",
                    localGitExcludePattern = PkcLocalGitExclude.ExcludePattern,
                    localGitExcludeStatus = gitIsolation.Status.ToString(),
                    trackedGitignoreModified = false,
                    note = "PKC uses Git's local exclude when a supported Git checkout is detected; tracked .gitignore is never modified."
                },
                jsonOptions),
            cancellationToken);

        var sourceContextPath = ResolveInsideWorkspace(workspaceRoot, SourceContextRelativePath);
        await File.WriteAllTextAsync(
            sourceContextPath,
            JsonSerializer.Serialize(
                new
                {
                    schemaVersion = "0.1-preview",
                    sourceRootRelativePath = "../..",
                    productMayReadSource = false,
                    traceMayReadSource = false,
                    engineeringMayReadSource = true,
                    benchmarkPhase2MayReadSource = true,
                    requiresApprovedSourceEnabledContext = true,
                    note = "The source root is co-located with this generated workspace. PRODUCT and TRACE remain workspace-only; ENGINEERING and benchmark phase 2 may inspect source only in an approved company context."
                },
                jsonOptions),
            cancellationToken);

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
