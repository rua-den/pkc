using System.Diagnostics;
using System.Text.Json;
using Pkc.Core;
using Pkc.Core.Discovery;
using Pkc.Knowledge;
using Xunit;
using Fixture = Pkc.CSharp.Tests.ScopedSemanticExecutionRegressionTests;

namespace Pkc.CSharp.Tests;

/// <summary>
/// A failure after the expensive scan must not waste the run: artifact writes are atomic, the previous workspace
/// survives a failed write, a completed scan can be resumed, and the run reports what it produced.
/// </summary>
public sealed class ResumableRunRegressionTests
{
    [Fact]
    public async Task A_failed_json_write_keeps_the_previous_artifact_and_leaves_no_temporary_file()
    {
        using var directory = new TemporaryDirectory();
        var path = Path.Combine(directory.Path, "facts.json");
        await File.WriteAllTextAsync(path, "previous");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            JsonArtifactFile.WriteAsync(path, new ThrowingDocument(), new JsonSerializerOptions()));

        Assert.Equal("previous", await File.ReadAllTextAsync(path));
        Assert.Equal(new[] { "facts.json" }, Directory.GetFiles(directory.Path).Select(Path.GetFileName).ToArray());
    }

    [Fact]
    public async Task Artifact_write_log_records_a_failure_instead_of_aborting()
    {
        using var directory = new TemporaryDirectory();
        var blocker = Path.Combine(directory.Path, "blocker");
        await File.WriteAllTextAsync(blocker, "a file where a directory is expected");
        var log = new ArtifactWriteLog();

        var written = await log.TryWriteJsonAsync("Blocked", Path.Combine(blocker, "out.json"), new { A = 1 }, new JsonSerializerOptions());
        var ok = await log.TryWriteJsonAsync("Fine", Path.Combine(directory.Path, "ok.json"), new { A = 1 }, new JsonSerializerOptions());

        Assert.False(written);
        Assert.True(ok);
        var failure = Assert.Single(log.Failures);
        Assert.Equal("Blocked", failure.Name);
        Assert.False(string.IsNullOrWhiteSpace(failure.Error));
    }

    [Fact]
    public async Task A_failed_workspace_write_keeps_the_current_workspace_and_removes_staging()
    {
        using var directory = new TemporaryDirectory();
        var writer = new AiWorkspaceWriter();
        var workspace = await writer.WriteAsync(directory.Path, new Dictionary<string, string> { ["START_HERE.md"] = "good" });

        await Assert.ThrowsAsync<InvalidOperationException>(() => writer.WriteAsync(
            directory.Path,
            new Dictionary<string, string> { ["START_HERE.md"] = "new", ["../escape.md"] = "bad" }));

        Assert.Equal("good", await File.ReadAllTextAsync(Path.Combine(workspace, "START_HERE.md")));
        Assert.False(Directory.Exists(Path.Combine(directory.Path, ".pkc", AiWorkspaceWriter.StagingDirectoryName)));
    }

    [Fact]
    public async Task A_successful_workspace_write_keeps_the_replaced_workspace_as_previous()
    {
        using var directory = new TemporaryDirectory();
        var writer = new AiWorkspaceWriter();
        await writer.WriteAsync(directory.Path, new Dictionary<string, string> { ["START_HERE.md"] = "first" });

        var workspace = await writer.WriteAsync(directory.Path, new Dictionary<string, string> { ["START_HERE.md"] = "second" });

        Assert.Equal("second", await File.ReadAllTextAsync(Path.Combine(workspace, "START_HERE.md")));
        var previous = Path.Combine(directory.Path, ".pkc", AiWorkspaceWriter.PreviousDirectoryName);
        Assert.Equal("first", await File.ReadAllTextAsync(Path.Combine(previous, "START_HERE.md")));
        Assert.False(Directory.Exists(Path.Combine(directory.Path, ".pkc", AiWorkspaceWriter.StagingDirectoryName)));
    }

    [Fact]
    public async Task A_workspace_that_cannot_be_replaced_is_preserved_as_new_instead_of_lost()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Directory rename is only blocked by open handles on Windows.
        }

        using var directory = new TemporaryDirectory();
        var writer = new AiWorkspaceWriter();
        var workspace = await writer.WriteAsync(directory.Path, new Dictionary<string, string> { ["START_HERE.md"] = "current" });

        WorkspaceReplaceException exception;
        await using (File.Open(Path.Combine(workspace, "START_HERE.md"), FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            exception = await Assert.ThrowsAsync<WorkspaceReplaceException>(() =>
                writer.WriteAsync(directory.Path, new Dictionary<string, string> { ["START_HERE.md"] = "generated" }));
        }

        Assert.Equal("current", await File.ReadAllTextAsync(Path.Combine(workspace, "START_HERE.md")));
        Assert.Equal(Path.Combine(directory.Path, ".pkc", AiWorkspaceWriter.UnswappedDirectoryName), exception.NewWorkspacePath);
        Assert.Equal("generated", await File.ReadAllTextAsync(Path.Combine(exception.NewWorkspacePath, "START_HERE.md")));
    }

    [Fact]
    public async Task Scan_checkpoint_is_reused_only_while_plan_scanner_scope_and_facts_still_match()
    {
        using var directory = new TemporaryDirectory();
        var repository = directory.Path;
        var facts = new FactDocument("0.4.4", [], []);
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await JsonArtifactFile.WriteAsync(Path.Combine(repository, ".pkc", "facts.json"), facts, options);
        var coverage = new ScanCoverage(
            ScanCoverage.CurrentSchemaVersion,
            "plan-1",
            true,
            10,
            [new CoverageCount(PlanCoverage.Semantic, 6), new CoverageCount(PlanCoverage.TestEvidence, 4)],
            [new WithheldCount(ScanMode.TestEvidence, 4)],
            1,
            ["unknown-area"],
            [new SemanticStageExecution("csharp", ScanPlanner.CSharpScanner, true, 6, 5, ["Scripts/x.cs"])]);

        Assert.Equal("no scan checkpoint from a completed scan", (await ScanCheckpoint.TryLoadAsync(repository, "plan-1", "scanner-1", true)).Reason);

        await ScanCheckpoint.WriteAsync(repository, await ScanCheckpoint.CreateAsync(repository, "plan-1", "scanner-1", facts, 12.34, coverage));

        var (loaded, reason) = await ScanCheckpoint.TryLoadAsync(repository, "plan-1", "scanner-1", true);
        Assert.Null(reason);
        Assert.NotNull(loaded);
        Assert.Equal(12.3, loaded!.ScanSeconds);
        Assert.Equal(ScanCoverageSerializer.Serialize(coverage), ScanCoverageSerializer.Serialize(loaded.Coverage));
        Assert.Equal(JsonSerializer.Serialize(facts, options), JsonSerializer.Serialize(await ScanCheckpoint.LoadFactsAsync(repository, loaded, options), options));

        Assert.Contains("manifests changed", (await ScanCheckpoint.TryLoadAsync(repository, "plan-2", "scanner-1", true)).Reason);
        Assert.Contains("scanner code changed", (await ScanCheckpoint.TryLoadAsync(repository, "plan-1", "scanner-2", true)).Reason);
        Assert.Contains("whole-root", (await ScanCheckpoint.TryLoadAsync(repository, "plan-1", "scanner-1", false)).Reason ?? string.Empty, StringComparison.Ordinal);

        await File.AppendAllTextAsync(Path.Combine(repository, ".pkc", "facts.json"), " ");
        Assert.Contains("no longer matches", (await ScanCheckpoint.TryLoadAsync(repository, "plan-1", "scanner-1", true)).Reason);

        ScanCheckpoint.Delete(repository);
        Assert.False(File.Exists(Path.Combine(repository, ScanCheckpoint.RelativePath.Replace('/', Path.DirectorySeparatorChar))));
    }

    [Fact]
    public async Task Run_resume_reuses_the_completed_scan_and_reproduces_the_same_knowledge_and_workspace()
    {
        using var fixture = await Fixture.ScopeFixture.CreateAsync();
        var pkc = Path.Combine(fixture.Root, ".pkc");

        var first = await RunCliAsync("run", fixture.Root);
        Assert.Equal(0, first.ExitCode);
        Assert.True(File.Exists(Path.Combine(fixture.Root, ScanCheckpoint.RelativePath.Replace('/', Path.DirectorySeparatorChar))));
        var productFeatures = await File.ReadAllTextAsync(Path.Combine(pkc, "product-features.json"));
        var workspace = await SnapshotAsync(Path.Combine(pkc, "workspace"));

        var resumed = await RunCliAsync("run", fixture.Root, "--resume");

        Assert.Equal(0, resumed.ExitCode);
        Assert.Contains("[pkc:resume] Reusing the scan from", resumed.Stderr, StringComparison.Ordinal);
        Assert.DoesNotContain("[pkc:scan:csharp]", resumed.Stderr, StringComparison.Ordinal);
        Assert.DoesNotContain("[pkc:scan:frontend]", resumed.Stderr, StringComparison.Ordinal);
        Assert.Equal(productFeatures, await File.ReadAllTextAsync(Path.Combine(pkc, "product-features.json")));
        Assert.Equal(workspace, await SnapshotAsync(Path.Combine(pkc, "workspace")));
        Assert.Equal(workspace, await SnapshotAsync(Path.Combine(pkc, AiWorkspaceWriter.PreviousDirectoryName)));

        Assert.Contains("PKC run summary", resumed.Stdout, StringComparison.Ordinal);
        Assert.Contains("reused scan from", resumed.Stdout, StringComparison.Ordinal);
        var summaryMarkdown = await File.ReadAllTextAsync(Path.Combine(fixture.Root, RunSummary.MarkdownRelativePath.Replace('/', Path.DirectorySeparatorChar)));
        Assert.Contains("**resumed**", summaryMarkdown, StringComparison.Ordinal);
        using var summary = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(fixture.Root, RunSummary.JsonRelativePath.Replace('/', Path.DirectorySeparatorChar))));
        Assert.True(summary.RootElement.GetProperty("workspaceFiles").GetInt32() > 0);
        Assert.All(summary.RootElement.GetProperty("artifacts").EnumerateArray(), artifact => Assert.True(artifact.GetProperty("written").GetBoolean()));
        Assert.DoesNotContain(fixture.Root, summary.RootElement.GetRawText(), StringComparison.OrdinalIgnoreCase);

        var startHere = await File.ReadAllTextAsync(Path.Combine(pkc, "workspace", AiWorkspaceRenderer.StartHereRelativePath));
        Assert.Contains("## What this workspace contains", startHere, StringComparison.Ordinal);
        Assert.DoesNotContain(fixture.Root, startHere, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Resume_is_rejected_for_commands_that_do_not_build_knowledge()
    {
        using var directory = new TemporaryDirectory();

        var (exitCode, _, stderr) = await RunCliAsync("scan", directory.Path, "--resume");

        Assert.Equal(2, exitCode);
        Assert.Contains("[--resume]", stderr, StringComparison.Ordinal);
    }

    [Fact]
    public void Answer_contract_defines_a_business_answer_format()
    {
        var contract = new AiWorkspaceRenderer().Render(new Dictionary<string, string>())[AiWorkspaceRenderer.AnswerContractRelativePath];

        foreach (var marker in new[] { "## Answer format", "**Short answer:**", "| When | Then |", "| ID | Scenario | Preconditions | Steps | Expected result | Basis |", "✅", "⚠️", "💡", "is true only when ALL of these hold", "the code condition decides" })
        {
            Assert.Contains(marker, contract, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Start_here_shows_what_the_workspace_contains_only_when_an_overview_is_given()
    {
        var renderer = new AiWorkspaceRenderer();
        var overview = new WorkspaceOverview(
            12,
            100,
            7,
            new KnowledgeGrounding(100, 60, 10, 40, 0, 0, 0),
            [new AreaCount("Orders", 30), new AreaCount("Billing", 20)],
            900,
            300,
            50,
            2);

        var plain = renderer.Render(new Dictionary<string, string>())[AiWorkspaceRenderer.StartHereRelativePath];
        var withOverview = renderer.Render(new Dictionary<string, string>(), overview: overview)[AiWorkspaceRenderer.StartHereRelativePath];

        Assert.DoesNotContain("## What this workspace contains", plain, StringComparison.Ordinal);
        Assert.Contains("## What this workspace contains", withOverview, StringComparison.Ordinal);
        Assert.Contains("12 product features, 100 workflows, 7 areas.", withOverview, StringComparison.Ordinal);
        Assert.Contains("Largest areas: Orders (30), Billing (20).", withOverview, StringComparison.Ordinal);
        Assert.Contains("| Business rules / conditions | 60% |", withOverview, StringComparison.Ordinal);
        Assert.Contains("Screen/UI questions", withOverview, StringComparison.Ordinal);
        Assert.Contains("permissions are ⚠️ not proven", withOverview, StringComparison.Ordinal);
        Assert.Contains("Side effects", withOverview, StringComparison.Ordinal);
    }

    [Fact]
    public void Run_summary_renders_counts_timings_and_failed_artifacts()
    {
        var summary = new RunSummary(
            RunSummary.CurrentSchemaVersion,
            "run",
            true,
            null,
            new RunRepositoryStats(51_000, 3, 40, 10, 2, 9_000, 8_900, 100, 2_000, 4_000, 30_000, 5, 1),
            263_571,
            2_037_879,
            4_186,
            703,
            25,
            4_700,
            4_709,
            new KnowledgeGrounding(4_000, 2_400, 350, 1_400, 3, 0, 0),
            [new AreaCount("Orders", 400)],
            [new PhaseTiming("scan", 1_500), new PhaseTiming("workspace", 90)],
            [new RunArtifact("AI workspace", ".pkc/workspace", true, null), new RunArtifact("Product features", ".pkc/product-features.json", false, "disk full")]);

        var console = summary.RenderConsole();
        var markdown = summary.RenderMarkdown("sample");

        Assert.Contains("263,571 facts | 2,037,879 relations", console, StringComparison.Ordinal);
        Assert.Contains("703 product features | 4,000 workflows | 25 areas", console, StringComparison.Ordinal);
        Assert.Contains("rules 60%", console, StringComparison.Ordinal);
        Assert.Contains("26m 30s", console, StringComparison.Ordinal);
        Assert.Contains("Product features not written: disk full", console, StringComparison.Ordinal);
        Assert.Contains("| Permissions | 350 | 9% |", markdown, StringComparison.Ordinal);
        Assert.Contains("| Orders | 400 |", markdown, StringComparison.Ordinal);
        Assert.Contains("**not written** — disk full", markdown, StringComparison.Ordinal);
    }

    private static async Task<string> SnapshotAsync(string directory)
    {
        var entries = new List<string>();
        foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
        {
            // Git-isolation status legitimately differs on any second run ("Added" then "AlreadyPresent").
            if (Path.GetRelativePath(directory, file).Replace('\\', '/') == AiWorkspaceWriter.GitIsolationRelativePath)
            {
                continue;
            }

            entries.Add(Path.GetRelativePath(directory, file) + "\n" + await File.ReadAllTextAsync(file));
        }

        return string.Join("\n---\n", entries);
    }

    private static async Task<(int ExitCode, string Stdout, string Stderr)> RunCliAsync(params string[] arguments)
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add(FindCli());
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo)!;
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return (process.ExitCode, await stdout, await stderr);
    }

    private static string FindCli()
    {
        var baseDirectory = AppContext.BaseDirectory;
        var configuration = baseDirectory.Contains($"{Path.DirectorySeparatorChar}Release{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
            ? "Release"
            : "Debug";
        var directory = new DirectoryInfo(baseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "src", "Pkc.Cli", "bin", configuration, "net10.0", "pkc.dll");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("pkc.dll was not found; build src/Pkc.Cli first.");
    }

    private sealed class ThrowingDocument
    {
        public string First => "written before the failure";

        public string Second => throw new InvalidOperationException("serialization failed midway");
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "pkc-resumable-run", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
