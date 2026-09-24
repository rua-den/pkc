using System.Text.Json;

namespace Pkc.Knowledge;

public sealed class AiWorkspaceWriter
{
    public const string WorkspaceDirectoryName = "workspace";
    public const string GitIsolationRelativePath = "_meta/git-isolation.json";
    public const string SourceContextRelativePath = "_meta/source-context.json";
    public const string StagingDirectoryName = "workspace.staging";
    public const string PreviousDirectoryName = "workspace.previous";
    public const string UnswappedDirectoryName = "workspace.new";

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

        var pkcRoot = Path.GetDirectoryName(workspaceRoot)!;
        var stagingRoot = Path.Combine(pkcRoot, StagingDirectoryName);

        // Everything is written to a staging folder first; the current workspace is only replaced once the new one is
        // complete, so a failed write never destroys the last good workspace.
        DeleteDirectoryIfExists(stagingRoot);
        Directory.CreateDirectory(stagingRoot);
        try
        {
            await WriteContentAsync(repositoryRoot, stagingRoot, files, cancellationToken);
        }
        catch
        {
            TryDeleteDirectory(stagingRoot);
            throw;
        }

        return Swap(pkcRoot, workspaceRoot, stagingRoot);
    }

    private static async Task WriteContentAsync(
        string repositoryRoot,
        string workspaceRoot,
        IReadOnlyDictionary<string, string> files,
        CancellationToken cancellationToken)
    {
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
    }

    /// <summary>
    /// Replaces the current workspace with the staged one and keeps the replaced workspace as
    /// <see cref="PreviousDirectoryName"/>. If the current workspace cannot be moved (for example because an assistant
    /// session has it open), the new workspace is preserved as <see cref="UnswappedDirectoryName"/> instead of lost.
    /// </summary>
    private static string Swap(string pkcRoot, string workspaceRoot, string stagingRoot)
    {
        var previousRoot = Path.Combine(pkcRoot, PreviousDirectoryName);
        var movedCurrent = false;
        try
        {
            if (Directory.Exists(workspaceRoot))
            {
                DeleteDirectoryIfExists(previousRoot);
                Directory.Move(workspaceRoot, previousRoot);
                movedCurrent = true;
            }

            Directory.Move(stagingRoot, workspaceRoot);
            return workspaceRoot;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            if (movedCurrent && !Directory.Exists(workspaceRoot))
            {
                try
                {
                    Directory.Move(previousRoot, workspaceRoot);
                }
                catch (Exception inner) when (inner is IOException or UnauthorizedAccessException)
                {
                    // Keep going: the new workspace is still preserved below.
                }
            }

            var unswappedRoot = Path.Combine(pkcRoot, UnswappedDirectoryName);
            try
            {
                DeleteDirectoryIfExists(unswappedRoot);
                Directory.Move(stagingRoot, unswappedRoot);
            }
            catch (Exception inner) when (inner is IOException or UnauthorizedAccessException)
            {
                unswappedRoot = stagingRoot;
            }

            throw new WorkspaceReplaceException(workspaceRoot, unswappedRoot, exception);
        }
    }

    private static void DeleteDirectoryIfExists(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            DeleteDirectoryIfExists(path);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // Best-effort cleanup of an incomplete staging folder.
        }
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

/// <summary>The new workspace was generated completely but could not replace the current one.</summary>
public sealed class WorkspaceReplaceException(string workspacePath, string newWorkspacePath, Exception inner)
    : IOException(
        $"The new workspace was generated but could not replace {workspacePath} ({inner.Message}). " +
        $"It is preserved at {newWorkspacePath}; close programs using the workspace and rename it, or re-run.",
        inner)
{
    public string WorkspacePath { get; } = workspacePath;

    public string NewWorkspacePath { get; } = newWorkspacePath;
}
