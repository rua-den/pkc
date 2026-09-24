using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class CliProgressOutputRegressionTests
{
    [Fact]
    public void Long_running_cli_phases_report_progress_before_the_work_begins()
    {
        var source = File.ReadAllText(FindProgramSource());

        AssertBefore(
            source,
            "Progress(\"scan:csharp\", \"Scanning C# repository evidence...\");",
            "new CSharpEvidenceScanner().ScanAsync(repositoryPath)");
        AssertBefore(
            source,
            "Progress(\"scan:frontend\", \"Scanning frontend repository evidence...\");",
            "new FrontendScanner().ScanAsync(repositoryPath)");
        AssertBefore(
            source,
            "Progress(\"link\", \"Building cross-stack workflow candidates...\");",
            "new CrossStackFeatureCandidateBuilder().Build(facts)");
        AssertBefore(
            source,
            "Progress(\"synthesize\", $\"Synthesizing {candidates.Candidates.Count} workflow candidates...\");",
            "synthesizer.SynthesizeAsync(candidate)");
        AssertBefore(
            source,
            "Progress(\"product\", \"Building product features...\");",
            "new ProductFeatureBuilder().Build(workflows)");
        AssertBefore(
            source,
            "Progress(\"workspace\", \"Rendering AI workspace...\");",
            "new AiWorkspaceRenderer().Render(");
        AssertBefore(
            source,
            "Progress(\"workspace\", \"Writing AI workspace...\");",
            "new AiWorkspaceWriter().WriteAsync(");
    }

    [Fact]
    public void Repository_discovery_runs_before_expensive_semantic_scanners()
    {
        var source = File.ReadAllText(FindProgramSource());

        AssertBefore(
            source,
            "Progress(\"discover\", \"Discovering repository shape before semantic analysis...\");",
            "new DiscoveryFirstScanPipeline(");

        // Semantic scanners are constructed exactly once, and only as stages of the discovery-first pipeline.
        foreach (var scanner in new[] { "new CSharpEvidenceScanner()", "new FrontendScanner()" })
        {
            var index = source.IndexOf(scanner, StringComparison.Ordinal);
            Assert.True(index >= 0, $"Missing semantic scanner: {scanner}");
            Assert.Equal(index, source.LastIndexOf(scanner, StringComparison.Ordinal));
            AssertBefore(source, "new DiscoveryFirstScanPipeline(", scanner);
            AssertBefore(source, "new SemanticScanStage(", scanner);
        }
    }

    [Fact]
    public void Progress_lines_use_stderr_with_a_stable_pkc_stage_prefix()
    {
        var source = File.ReadAllText(FindProgramSource());

        Assert.Contains(
            "Console.Error.WriteLine($\"[pkc:{phase}] {message}\");",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "ShouldReportProgress(synthesizedCount, candidates.Candidates.Count)",
            source,
            StringComparison.Ordinal);
    }

    private static void AssertBefore(string source, string progressMarker, string workMarker)
    {
        var progressIndex = source.IndexOf(progressMarker, StringComparison.Ordinal);
        var workIndex = source.IndexOf(workMarker, StringComparison.Ordinal);

        Assert.True(progressIndex >= 0, $"Missing progress marker: {progressMarker}");
        Assert.True(workIndex >= 0, $"Missing work marker: {workMarker}");
        Assert.True(
            progressIndex < workIndex,
            $"Expected progress marker before work marker. Progress: {progressMarker}; work: {workMarker}");
    }

    private static string FindProgramSource()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "src", "Pkc.Cli", "Program.cs");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not locate src/Pkc.Cli/Program.cs from the test output directory.");
    }
}
