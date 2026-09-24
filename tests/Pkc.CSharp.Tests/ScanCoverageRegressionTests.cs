using System.Diagnostics;
using System.Text.Json;
using Pkc.Core;
using Pkc.Core.Discovery;
using Pkc.Knowledge;
using Xunit;
using Fixture = Pkc.CSharp.Tests.ScopedSemanticExecutionRegressionTests;

namespace Pkc.CSharp.Tests;

public sealed class ScanCoverageRegressionTests
{
    [Fact]
    public async Task Discover_is_plan_only_and_starts_no_semantic_scanner()
    {
        using var fixture = await Fixture.ScopeFixture.CreateAsync();
        var staleCoverage = Path.Combine(fixture.Root, ".pkc", "discovery", "scan-coverage.json");
        Directory.CreateDirectory(Path.GetDirectoryName(staleCoverage)!);
        await File.WriteAllTextAsync(staleCoverage, "{}");

        var (exitCode, stdout, stderr) = await RunCliAsync("discover", fixture.Root);

        Assert.Equal(0, exitCode);
        Assert.Contains("PKC discover complete (plan only): no semantic scanner started.", stdout, StringComparison.Ordinal);
        Assert.Contains("[pkc:plan] Scan plan:", stderr, StringComparison.Ordinal);
        Assert.DoesNotContain("[pkc:scan:", stderr, StringComparison.Ordinal);
        Assert.DoesNotContain("[pkc:scope:", stderr, StringComparison.Ordinal);
        Assert.True(File.Exists(Path.Combine(fixture.Root, ".pkc", "discovery", "repository-profile.json")));
        Assert.True(File.Exists(Path.Combine(fixture.Root, ".pkc", "discovery", "scan-plan.json")));
        Assert.False(File.Exists(staleCoverage));
        Assert.False(File.Exists(Path.Combine(fixture.Root, ".pkc", "facts.json")));
        Assert.False(Directory.Exists(Path.Combine(fixture.Root, ".pkc", "workspace")));
    }

    [Fact]
    public async Task Coverage_record_reports_plan_and_scanner_scope_withholding_byte_stably_and_locally()
    {
        using var fixture = await Fixture.ScopeFixture.CreateAsync();

        // The first run creates `.pkc/`, which later runs report as an excluded tool-state area; compare warm runs.
        await RunStubStagesAsync(fixture.Root, scoped: true);
        var scoped = await RunStubStagesAsync(fixture.Root, scoped: true);
        var coverage = Assert.IsType<ScanCoverage>(scoped.Coverage);
        var json = await File.ReadAllTextAsync(scoped.CoveragePath!);

        Assert.Equal(ScanCoverageSerializer.Serialize(coverage), json);
        Assert.Equal(Path.Combine(fixture.Root, ".pkc", "discovery", "scan-coverage.json"), scoped.CoveragePath);
        Assert.Equal(scoped.State.Plan.InputFingerprint, coverage.PlanFingerprint);
        Assert.True(coverage.Scoped);

        // Test project (csproj + source) and the Angular spec are test evidence; the declared LibMan file is a runtime dependency.
        Assert.Equal(3, coverage.Withheld(ScanMode.TestEvidence));
        Assert.Equal(1, coverage.Withheld(ScanMode.RuntimeDependencyIndex));
        Assert.Equal(scoped.State.Scope.WithheldFiles.Count, coverage.WithheldByPlan.Sum(count => count.Files));
        Assert.Equal(scoped.State.Plan.Exclusions.Count, coverage.ExcludedAreas);
        Assert.Contains(coverage.Stages, stage => stage.Name == "csharp" && stage.WithheldByScannerScope.SequenceEqual([Fixture.KnowledgeFile]));
        Assert.Contains(coverage.Stages, stage => stage.Name == "frontend" && stage.WithheldByScannerScope.SequenceEqual([Fixture.FrontendBuildToolFile]));

        Assert.DoesNotContain(fixture.Root, json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(fixture.Root.Replace('\\', '/'), json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("http.get", json, StringComparison.Ordinal);

        var again = await RunStubStagesAsync(fixture.Root, scoped: true);
        Assert.Equal(json, await File.ReadAllTextAsync(again.CoveragePath!));

        var whole = await RunStubStagesAsync(fixture.Root, scoped: false);
        Assert.False(whole.Coverage!.Scoped);
        Assert.Empty(whole.Coverage.WithheldByPlan);
    }

    [Fact]
    public async Task Portable_workspace_coverage_is_count_only()
    {
        using var fixture = await Fixture.ScopeFixture.CreateAsync();
        var result = await RunStubStagesAsync(fixture.Root, scoped: true);
        var coverage = result.Coverage!;
        var portable = coverage.ToPortable();
        var json = ScanCoverageSerializer.SerializePortable(portable);

        Assert.Equal(coverage.Files, portable.Files);
        Assert.Equal(coverage.Stages.Sum(stage => stage.PlannedFiles), portable.SemanticPlannedFiles);
        Assert.Equal(coverage.Stages.Sum(stage => stage.ExecutableFiles), portable.SemanticExecutableFiles);
        Assert.Equal(2, portable.WithheldByScannerScopeFiles);
        Assert.Equal(coverage.Coverage(PlanCoverage.TestEvidence), portable.TestEvidenceFiles);
        Assert.Equal(coverage.ExcludedAreas, portable.ExcludedAreas);

        // No repository paths or names travel with the workspace.
        foreach (var file in result.State.Profile.Files)
        {
            Assert.DoesNotContain(file, json, StringComparison.OrdinalIgnoreCase);
        }

        foreach (var marker in new[] { "Shop", "vendorkit", "unknownkit", "src/", "web/", "knowledge/", fixture.Root })
        {
            Assert.DoesNotContain(marker, json, StringComparison.OrdinalIgnoreCase);
        }

        var contract = new AiWorkspaceRenderer().Render(new Dictionary<string, string>())[AiWorkspaceRenderer.AnswerContractRelativePath];
        Assert.Contains("`_meta/coverage.json`", contract, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Run_progress_coverage_and_workspace_summary_reflect_the_actual_execution()
    {
        using var fixture = await Fixture.ScopeFixture.CreateAsync();

        var (exitCode, _, stderr) = await RunCliAsync("run", fixture.Root);

        Assert.Equal(0, exitCode);
        var coveragePath = Path.Combine(fixture.Root, ".pkc", "discovery", "scan-coverage.json");
        using var coverage = JsonDocument.Parse(await File.ReadAllTextAsync(coveragePath));
        var stages = coverage.RootElement.GetProperty("stages").EnumerateArray().ToArray();
        Assert.Equal(new[] { "csharp", "frontend" }, stages.Select(stage => stage.GetProperty("name").GetString()).ToArray());

        foreach (var stage in stages)
        {
            var name = stage.GetProperty("name").GetString();
            var stagePlanned = stage.GetProperty("plannedFiles").GetInt32();
            var stageExecutable = stage.GetProperty("executableFiles").GetInt32();
            var withheld = stage.GetProperty("withheldByScannerScope").GetArrayLength();
            Assert.Contains(
                $"[pkc:scope:{name}] Plan scope: {stagePlanned} planned semantic files; {stageExecutable} within the scanner source scope, {withheld} withheld by scanner name scope.",
                stderr,
                StringComparison.Ordinal);
        }

        var planned = stages.Sum(stage => stage.GetProperty("plannedFiles").GetInt32());
        var executable = stages.Sum(stage => stage.GetProperty("executableFiles").GetInt32());
        Assert.Contains($"[pkc:coverage] Semantic: {executable} of {planned} planned files executable (2 withheld by scanner name scope); " +
                        "withheld by plan: test-evidence 3, light-index 0, runtime-index 1 files,", stderr, StringComparison.Ordinal);
        AssertOrder(stderr, "[pkc:plan] Scan plan:", "[pkc:scope:csharp]", "[pkc:scan:csharp]", "[pkc:scope:frontend]", "[pkc:coverage]", "[pkc:workspace]");

        var workspaceCoverage = await File.ReadAllTextAsync(Path.Combine(fixture.Root, ".pkc", "workspace", "_meta", "coverage.json"));
        var local = JsonSerializer.Deserialize<ScanCoverage>(await File.ReadAllTextAsync(coveragePath), JsonOptions())!;
        Assert.Equal(ScanCoverageSerializer.SerializePortable(local.ToPortable()), workspaceCoverage);
    }

    private static JsonSerializerOptions JsonOptions() => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper) }
    };

    private static void AssertOrder(string text, params string[] markers)
    {
        var previous = -1;
        foreach (var marker in markers)
        {
            var index = text.IndexOf(marker, StringComparison.Ordinal);
            Assert.True(index > previous, $"Expected '{marker}' after the previous marker.");
            previous = index;
        }
    }

    private static Task<DiscoveryFirstScanResult> RunStubStagesAsync(string root, bool scoped) =>
        new DiscoveryFirstScanPipeline(scoped: scoped).RunAsync(root,
        [
            new SemanticScanStage("csharp", (_, _) => Task.FromResult(new FactDocument("stub", [], [])))
            {
                Scanner = ScanPlanner.CSharpScanner,
                InScannerSourceScope = CSharpEvidenceScanner.IsInSourceScope
            },
            new SemanticScanStage("frontend", (_, _) => Task.FromResult(new FactDocument("stub", [], [])))
            {
                Scanner = ScanPlanner.FrontendScanner,
                InScannerSourceScope = Frontend.FrontendScanner.IsInSourceScope
            }
        ]);

    private static async Task<(int ExitCode, string Stdout, string Stderr)> RunCliAsync(string command, string root)
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add(FindCli());
        startInfo.ArgumentList.Add(command);
        startInfo.ArgumentList.Add(root);

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
}
