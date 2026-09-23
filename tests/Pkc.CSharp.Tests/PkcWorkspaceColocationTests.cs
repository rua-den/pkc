using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class PkcWorkspaceColocationTests
{
    [Fact]
    public async Task Writer_adds_root_pkc_to_local_git_exclude_without_touching_tracked_gitignore()
    {
        var root = CreateTempDirectory();
        try
        {
            var gitDirectory = Path.Combine(root, ".git");
            Directory.CreateDirectory(Path.Combine(gitDirectory, "info"));
            await File.WriteAllTextAsync(Path.Combine(gitDirectory, "HEAD"), "ref: refs/heads/main\n");
            await File.WriteAllTextAsync(Path.Combine(gitDirectory, "config"), "[core]\n\trepositoryformatversion = 0\n");
            await File.WriteAllTextAsync(Path.Combine(gitDirectory, "info", "exclude"), "bin/\n");

            var trackedGitignore = Path.Combine(root, ".gitignore");
            await File.WriteAllTextAsync(trackedGitignore, "obj/\n");

            var files = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["CLAUDE.md"] = "# generated",
                ["knowledge/START_HERE.md"] = "# start"
            };

            var writer = new AiWorkspaceWriter();
            var workspace = await writer.WriteAsync(root, files);
            await writer.WriteAsync(root, files);

            Assert.Equal("obj/\n", await File.ReadAllTextAsync(trackedGitignore));

            var exclude = await File.ReadAllTextAsync(Path.Combine(gitDirectory, "info", "exclude"));
            Assert.Contains(PkcLocalGitExclude.ExcludePattern, exclude, StringComparison.Ordinal);
            Assert.Equal(1, exclude.Split(PkcLocalGitExclude.ExcludePattern, StringSplitOptions.None).Length - 1);

            var gitIsolation = await File.ReadAllTextAsync(Path.Combine(workspace, "_meta", "git-isolation.json"));
            Assert.Contains("\"localGitExcludeStatus\": \"AlreadyPresent\"", gitIsolation, StringComparison.Ordinal);
            Assert.Contains("\"trackedGitignoreModified\": false", gitIsolation, StringComparison.Ordinal);

            var sourceContext = await File.ReadAllTextAsync(Path.Combine(workspace, "_meta", "source-context.json"));
            Assert.Contains("\"sourceRootRelativePath\": \"../..\"", sourceContext, StringComparison.Ordinal);
            Assert.Contains("\"productMayReadSource\": false", sourceContext, StringComparison.Ordinal);
            Assert.Contains("\"engineeringMayReadSource\": true", sourceContext, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Writer_uses_common_git_exclude_for_worktree_checkout()
    {
        var parent = CreateTempDirectory();
        try
        {
            var root = Path.Combine(parent, "worktree");
            Directory.CreateDirectory(root);

            var commonGit = Path.Combine(parent, "repo.git");
            var worktreeGit = Path.Combine(commonGit, "worktrees", "team");
            Directory.CreateDirectory(worktreeGit);
            await File.WriteAllTextAsync(Path.Combine(commonGit, "HEAD"), "ref: refs/heads/main\n");
            await File.WriteAllTextAsync(Path.Combine(commonGit, "config"), "[core]\n\trepositoryformatversion = 0\n");
            await File.WriteAllTextAsync(Path.Combine(worktreeGit, "HEAD"), "ref: refs/heads/team\n");
            await File.WriteAllTextAsync(
                Path.Combine(worktreeGit, "commondir"),
                Path.GetRelativePath(worktreeGit, commonGit) + Environment.NewLine);
            await File.WriteAllTextAsync(
                Path.Combine(root, ".git"),
                "gitdir: " + worktreeGit + Environment.NewLine);

            var files = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["CLAUDE.md"] = "# generated"
            };

            await new AiWorkspaceWriter().WriteAsync(root, files);

            var excludePath = Path.Combine(commonGit, "info", "exclude");
            Assert.True(File.Exists(excludePath));
            Assert.Contains(
                PkcLocalGitExclude.ExcludePattern,
                await File.ReadAllTextAsync(excludePath),
                StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(parent, recursive: true);
        }
    }

    [Fact]
    public async Task Writer_records_not_git_repository_without_failing_workspace_generation()
    {
        var root = CreateTempDirectory();
        try
        {
            var workspace = await new AiWorkspaceWriter().WriteAsync(
                root,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["CLAUDE.md"] = "# generated"
                });

            var metadata = await File.ReadAllTextAsync(Path.Combine(workspace, "_meta", "git-isolation.json"));
            Assert.Contains("\"localGitExcludeStatus\": \"NotGitRepository\"", metadata, StringComparison.Ordinal);
            Assert.True(File.Exists(Path.Combine(workspace, "CLAUDE.md")));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "pkc-colocation-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
