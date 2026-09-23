using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class AiWorkspaceRendererTests
{
    [Fact]
    public void Render_includes_bootloaders_policy_routing_catalog_preview_manifest_and_privacy_boundary()
    {
        var canonical = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["knowledge/index.md"] = "# Index",
            ["knowledge/features/orders/operations.md"] = "# Orders",
            ["knowledge/workflows/orders/pack.md"] = "# Pack"
        };

        var files = new AiWorkspaceRenderer().Render(canonical, "team-repo");

        Assert.Contains(AiWorkspaceRenderer.ClaudeRelativePath, files.Keys);
        Assert.Contains(AiWorkspaceRenderer.AgentsRelativePath, files.Keys);
        Assert.Contains(AiWorkspaceRenderer.StartHereRelativePath, files.Keys);
        Assert.Contains(AiWorkspaceRenderer.AnswerContractRelativePath, files.Keys);
        Assert.Contains(AiWorkspaceRenderer.ManifestRelativePath, files.Keys);
        Assert.Contains(AiWorkspaceRenderer.CatalogRelativePath, files.Keys);
        Assert.Equal("# Pack", files["knowledge/workflows/orders/pack.md"]);

        var claude = files[AiWorkspaceRenderer.ClaudeRelativePath];
        var agents = files[AiWorkspaceRenderer.AgentsRelativePath];
        var startHere = files[AiWorkspaceRenderer.StartHereRelativePath];
        var contract = files[AiWorkspaceRenderer.AnswerContractRelativePath];
        var manifest = files[AiWorkspaceRenderer.ManifestRelativePath];

        Assert.Contains("Do not inspect parent/source directories", claude, StringComparison.Ordinal);
        Assert.Contains("Default mode is PRODUCT", agents, StringComparison.Ordinal);
        Assert.Contains("Use `_meta/catalog.json`", startHere, StringComparison.Ordinal);

        Assert.Contains("workspace is intentionally source-free", claude, StringComparison.Ordinal);
        Assert.Contains("Do not copy source-code bodies", claude, StringComparison.Ordinal);
        Assert.Contains("approved source-enabled/company environment", claude, StringComparison.Ordinal);
        Assert.Contains("Do not run a full AI benchmark after every edit", claude, StringComparison.Ordinal);
        Assert.Contains("Phase 1: answer from this generated workspace only", contract, StringComparison.Ordinal);
        Assert.Contains("do not paste source-code bodies", contract, StringComparison.Ordinal);

        Assert.Contains("\"workspaceStatus\": \"PREVIEW\"", manifest, StringComparison.Ordinal);
        Assert.Contains("\"workspaceContainsSourceCode\": false", manifest, StringComparison.Ordinal);
        Assert.Contains("\"rawFactFilesIncluded\": false", manifest, StringComparison.Ordinal);
        Assert.Contains("\"productAnswersMayReadSourceByDefault\": false", manifest, StringComparison.Ordinal);
        Assert.Contains("\"sourceCrossCheckRequiresApprovedContext\": true", manifest, StringComparison.Ordinal);
        Assert.Contains("\"defaultLevel\": \"deterministic\"", manifest, StringComparison.Ordinal);
        Assert.Contains("\"targetedAiForSemanticChanges\": true", manifest, StringComparison.Ordinal);
        Assert.Contains("\"fullAiForCheckpointsOnly\": true", manifest, StringComparison.Ordinal);
        Assert.Contains("\"kind\": \"workflow\"", files[AiWorkspaceRenderer.CatalogRelativePath], StringComparison.Ordinal);
    }

    [Fact]
    public async Task Writer_isolated_workspace_preserves_team_agent_files_and_removes_stale_generated_files()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-workspace-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var teamClaude = Path.Combine(root, "CLAUDE.md");
            var teamAgents = Path.Combine(root, "AGENTS.md");
            await File.WriteAllTextAsync(teamClaude, "team claude instructions");
            await File.WriteAllTextAsync(teamAgents, "team agent instructions");

            var staleDirectory = Path.Combine(root, ".pkc", "workspace", "knowledge");
            Directory.CreateDirectory(staleDirectory);
            var staleFile = Path.Combine(staleDirectory, "stale.md");
            await File.WriteAllTextAsync(staleFile, "stale");

            var canonical = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["knowledge/index.md"] = "# Index",
                ["knowledge/workflows/orders/pack.md"] = "# Pack"
            };
            var files = new AiWorkspaceRenderer().Render(canonical, "team-repo");

            var workspace = await new AiWorkspaceWriter().WriteAsync(root, files);

            Assert.Equal("team claude instructions", await File.ReadAllTextAsync(teamClaude));
            Assert.Equal("team agent instructions", await File.ReadAllTextAsync(teamAgents));
            Assert.False(File.Exists(staleFile));
            Assert.True(File.Exists(Path.Combine(workspace, "CLAUDE.md")));
            Assert.True(File.Exists(Path.Combine(workspace, "AGENTS.md")));
            Assert.True(File.Exists(Path.Combine(workspace, "knowledge", "START_HERE.md")));
            Assert.True(File.Exists(Path.Combine(workspace, "knowledge", "index.md")));
            Assert.True(File.Exists(Path.Combine(workspace, "_policy", "answer-contract.md")));
            Assert.True(File.Exists(Path.Combine(workspace, "_meta", "manifest.json")));
            Assert.True(File.Exists(Path.Combine(workspace, "_meta", "catalog.json")));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public async Task Writer_rejects_workspace_path_traversal()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-workspace-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var files = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["../escape.md"] = "nope"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => new AiWorkspaceWriter().WriteAsync(root, files));
            Assert.False(File.Exists(Path.Combine(root, "escape.md")));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }
}
