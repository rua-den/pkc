namespace Pkc.Knowledge;

public enum PkcLocalGitExcludeStatus
{
    Added,
    AlreadyPresent,
    NotGitRepository,
    UnsupportedGitLayout,
    Failed
}

public sealed record PkcLocalGitExcludeResult(
    PkcLocalGitExcludeStatus Status,
    string? ExcludePath = null);

public sealed class PkcLocalGitExclude
{
    public const string ExcludePattern = "/.pkc/";

    public PkcLocalGitExcludeResult EnsureIgnored(string repositoryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryPath);

        try
        {
            var repositoryRoot = Path.GetFullPath(repositoryPath);
            var gitDirectory = ResolveGitDirectory(repositoryRoot);
            if (gitDirectory is null)
            {
                return new PkcLocalGitExcludeResult(PkcLocalGitExcludeStatus.NotGitRepository);
            }

            if (!LooksLikeGitDirectory(gitDirectory))
            {
                return new PkcLocalGitExcludeResult(PkcLocalGitExcludeStatus.UnsupportedGitLayout);
            }

            var commonDirectory = ResolveCommonGitDirectory(gitDirectory);
            if (!LooksLikeGitDirectory(commonDirectory))
            {
                return new PkcLocalGitExcludeResult(PkcLocalGitExcludeStatus.UnsupportedGitLayout);
            }

            var infoDirectory = Path.Combine(commonDirectory, "info");
            var excludePath = Path.Combine(infoDirectory, "exclude");
            if (File.Exists(excludePath) && File.ReadLines(excludePath).Any(CoversPkcOutput))
            {
                return new PkcLocalGitExcludeResult(PkcLocalGitExcludeStatus.AlreadyPresent, excludePath);
            }

            Directory.CreateDirectory(infoDirectory);
            var prefix = string.Empty;
            if (File.Exists(excludePath))
            {
                var existing = File.ReadAllText(excludePath);
                if (existing.Length > 0 && existing[^1] is not '\n' and not '\r')
                {
                    prefix = Environment.NewLine;
                }
            }

            File.AppendAllText(
                excludePath,
                prefix + "# PKC generated workspace (local only)" + Environment.NewLine +
                ExcludePattern + Environment.NewLine);

            return new PkcLocalGitExcludeResult(PkcLocalGitExcludeStatus.Added, excludePath);
        }
        catch (IOException)
        {
            return new PkcLocalGitExcludeResult(PkcLocalGitExcludeStatus.Failed);
        }
        catch (UnauthorizedAccessException)
        {
            return new PkcLocalGitExcludeResult(PkcLocalGitExcludeStatus.Failed);
        }
    }

    private static string? ResolveGitDirectory(string repositoryRoot)
    {
        var dotGit = Path.Combine(repositoryRoot, ".git");
        if (Directory.Exists(dotGit))
        {
            return Path.GetFullPath(dotGit);
        }

        if (!File.Exists(dotGit))
        {
            return null;
        }

        var pointer = File.ReadLines(dotGit).FirstOrDefault()?.Trim();
        const string prefix = "gitdir:";
        if (pointer is null || !pointer.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var value = pointer[prefix.Length..].Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Path.GetFullPath(Path.IsPathRooted(value)
            ? value
            : Path.Combine(repositoryRoot, value));
    }

    private static string ResolveCommonGitDirectory(string gitDirectory)
    {
        var commonDirectoryFile = Path.Combine(gitDirectory, "commondir");
        if (!File.Exists(commonDirectoryFile))
        {
            return gitDirectory;
        }

        var value = File.ReadLines(commonDirectoryFile).FirstOrDefault()?.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            return gitDirectory;
        }

        return Path.GetFullPath(Path.IsPathRooted(value)
            ? value
            : Path.Combine(gitDirectory, value));
    }

    private static bool LooksLikeGitDirectory(string path) =>
        Directory.Exists(path) && File.Exists(Path.Combine(path, "HEAD"));

    private static bool CoversPkcOutput(string line)
    {
        var value = line.Trim();
        return value is ".pkc" or ".pkc/" or "/.pkc" or "/.pkc/";
    }
}
