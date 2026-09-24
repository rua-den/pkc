using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Pkc.Core;
using Pkc.Core.Discovery;
using Pkc.CSharp;
using Pkc.Frontend;
using Pkc.Knowledge;

var resume = args.Contains("--resume", StringComparer.Ordinal);
var arguments = args.Where(argument => argument != "--resume").ToArray();
var command = arguments.Length > 0 ? arguments[0] : string.Empty;
if (arguments.Length != 2 ||
    (command != "discover" && command != "scan" && command != "build" && command != "run") ||
    (resume && command is not ("build" or "run")))
{
    Console.Error.WriteLine("Usage: pkc <discover|scan|build|run> <repository-path> [--resume]");
    Console.Error.WriteLine("  --resume  (build/run) reuse the last completed scan when repository structure and PKC scanners are unchanged.");
    return 2;
}

var repositoryPath = Path.GetFullPath(arguments[1]);
if (!Directory.Exists(repositoryPath))
{
    Console.Error.WriteLine($"Repository path does not exist: {repositoryPath}");
    return 2;
}

var timings = new List<PhaseTiming>();
var phaseClock = Stopwatch.StartNew();
void Timed(string phase)
{
    timings.Add(new PhaseTiming(phase, phaseClock.Elapsed.TotalSeconds));
    phaseClock.Restart();
}

try
{
    Progress("start", $"PKC {command} started.");

    // Discovery establishes and persists the repository profile and scan plan before any expensive semantic scanner
    // starts. `discover` stops there (plan only); `run` executes the scanners inside the plan scope; `scan` and
    // `build` keep their whole-root scope.
    Progress("discover", "Discovering repository shape before semantic analysis...");
    var scoped = command == "run";
    var pipeline = new DiscoveryFirstScanPipeline(ReportDiscovery, ReportStage, scoped: command == "run");
    var state = await pipeline.DiscoverAsync(repositoryPath);
    Timed("discover");

    if (command == "discover")
    {
        await pipeline.ExecuteAsync(repositoryPath, state, []);
        Console.WriteLine("PKC discover complete (plan only): no semantic scanner started.");
        Console.WriteLine(state.ProfilePath);
        Console.WriteLine(state.PlanPath);
        return 0;
    }

    var outputDirectory = Path.Combine(repositoryPath, ".pkc");
    Directory.CreateDirectory(outputDirectory);

    var options = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    var factsPath = Path.Combine(outputDirectory, "facts.json");
    var scannerIdentity = ScanCheckpoint.IdentityOf([typeof(CSharpEvidenceScanner).Assembly, typeof(FrontendScanner).Assembly]);
    ScanCheckpoint? checkpoint = null;
    if (resume)
    {
        Progress("resume", "Checking the last scan checkpoint...");
        (checkpoint, var reason) = await ScanCheckpoint.TryLoadAsync(repositoryPath, state.Plan.InputFingerprint, scannerIdentity, scoped);
        if (checkpoint is null)
        {
            Progress("resume", $"Cannot resume: {reason}. Running a full scan instead.");
        }
    }

    FactDocument facts;
    ScanCoverage coverage;
    if (checkpoint is not null)
    {
        Progress(
            "resume",
            $"Reusing the scan from {checkpoint.CreatedUtc.ToLocalTime():yyyy-MM-dd HH:mm} ({checkpoint.Facts} facts, {checkpoint.Relations} relations). " +
            "Source edits made after that scan are not reflected; run without --resume for a fresh scan.");
        facts = await ScanCheckpoint.LoadFactsAsync(repositoryPath, checkpoint, options);
        coverage = checkpoint.Coverage;
        ReportCoverage(coverage, await RepositoryDiscoveryArtifacts.WriteCoverageAsync(repositoryPath, coverage));
        Timed("load scan");
    }
    else
    {
        // A full scan is about to replace facts.json: an older checkpoint must never outlive the facts it describes.
        ScanCheckpoint.Delete(repositoryPath);
        var scan = await pipeline.ExecuteAsync(
            repositoryPath,
            state,
            [
                new SemanticScanStage("csharp", async (_, _) =>
                {
                    Progress("scan:csharp", "Scanning C# repository evidence...");
                    var document = await new CSharpEvidenceScanner().ScanAsync(repositoryPath);
                    Progress(
                        "scan:csharp",
                        $"Complete: {document.Facts.Count} facts, {document.Relations.Count} relations.");
                    return document;
                })
                {
                    Scanner = ScanPlanner.CSharpScanner,
                    InScannerSourceScope = CSharpEvidenceScanner.IsInSourceScope
                },
                new SemanticScanStage("frontend", async (_, _) =>
                {
                    Progress("scan:frontend", "Scanning frontend repository evidence...");
                    var document = await new FrontendScanner().ScanAsync(repositoryPath);
                    Progress(
                        "scan:frontend",
                        $"Complete: {document.Facts.Count} facts, {document.Relations.Count} relations.");
                    return document;
                })
                {
                    Scanner = ScanPlanner.FrontendScanner,
                    InScannerSourceScope = FrontendScanner.IsInSourceScope
                }
            ]);

        coverage = scan.Coverage!;
        ReportCoverage(coverage, scan.CoveragePath!);

        Progress("merge", "Merging repository evidence...");
        facts = Merge(scan.Documents[0], scan.Documents[1]);
        Progress("merge", $"Complete: {facts.Facts.Count} facts, {facts.Relations.Count} relations.");
        Timed("scan");
    }

    // Artifact writes after the scan are best-effort: a failure is reported in the summary instead of discarding the
    // analysis. Once facts.json is written, a checkpoint lets a later failure be resumed without re-scanning.
    var writes = new ArtifactWriteLog();
    if (checkpoint is null)
    {
        Progress("write", "Writing scan evidence...");
        if (await writes.TryWriteJsonAsync("Evidence (facts)", factsPath, facts, options))
        {
            try
            {
                await ScanCheckpoint.WriteAsync(
                    repositoryPath,
                    await ScanCheckpoint.CreateAsync(repositoryPath, state.Plan.InputFingerprint, scannerIdentity, facts, timings[^1].Seconds, coverage));
                writes.Record("Scan checkpoint (for --resume)", Path.Combine(repositoryPath, ScanCheckpoint.RelativePath), true);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                writes.Record("Scan checkpoint (for --resume)", Path.Combine(repositoryPath, ScanCheckpoint.RelativePath), false, exception.Message);
            }
        }

        Timed("write evidence");
    }
    else
    {
        writes.Record("Evidence (facts, reused)", factsPath, true);
    }

    Progress("link", "Building cross-stack workflow candidates...");
    var candidates = new CrossStackFeatureCandidateBuilder().Build(facts);
    Progress("enrich", "Enriching workflow candidates with validation and visibility evidence...");
    candidates = new ValidationConsistencyCandidateEnricher().Enrich(candidates, facts);
    candidates = new JointVisibilityCandidateEnricher().Enrich(candidates, facts);
    Progress("link", $"Complete: {candidates.Candidates.Count} workflow candidates.");

    var candidatesPath = Path.Combine(outputDirectory, "feature-candidates.json");
    await writes.TryWriteJsonAsync("Workflow candidates", candidatesPath, candidates, options);
    Timed("link");

    Console.WriteLine($"PKC scan complete: {facts.Facts.Count} facts, {facts.Relations.Count} relations, {candidates.Candidates.Count} workflow candidates");
    Console.WriteLine(factsPath);
    Console.WriteLine(candidatesPath);

    if (command is "build" or "run")
    {
        var synthesizer = new JointVisibilityKnowledgeSynthesizer();
        var workflowRenderer = new MarkdownKnowledgeRenderer();
        var workflows = new List<FeatureKnowledge>();
        var canonicalKnowledgeFiles = new Dictionary<string, string>(StringComparer.Ordinal);

        Progress("synthesize", $"Synthesizing {candidates.Candidates.Count} workflow candidates...");
        var synthesizedCount = 0;
        foreach (var candidate in candidates.Candidates)
        {
            var workflow = await synthesizer.SynthesizeAsync(candidate);
            workflows.Add(workflow);
            var relativePath = workflowRenderer.GetRelativePath(workflow);
            canonicalKnowledgeFiles[relativePath] = workflowRenderer.Render(workflow);

            synthesizedCount++;
            if (ShouldReportProgress(synthesizedCount, candidates.Candidates.Count))
            {
                Progress("synthesize", $"Workflows: {synthesizedCount}/{candidates.Candidates.Count}.");
            }
        }

        Timed("synthesize");

        Progress("product", "Building product features...");
        var productFeatures = new ProductFeatureBuilder().Build(workflows);
        var productFeaturesPath = Path.Combine(outputDirectory, "product-features.json");
        await writes.TryWriteJsonAsync("Product features", productFeaturesPath, productFeatures, options);
        Console.WriteLine(productFeaturesPath);
        Progress("product", $"Complete: {productFeatures.Features.Count} product features.");

        Progress("knowledge", "Rendering canonical knowledge files...");
        var featureRenderer = new ProductFeatureMarkdownRenderer();
        foreach (var feature in productFeatures.Features)
        {
            var relativePath = featureRenderer.GetRelativePath(feature);
            canonicalKnowledgeFiles[relativePath] = featureRenderer.Render(feature);
        }

        const string indexRelativePath = "knowledge/index.md";
        canonicalKnowledgeFiles[indexRelativePath] = featureRenderer.RenderIndex(productFeatures);

        var sourceRepositoryLabel = new DirectoryInfo(repositoryPath).Name;
        var packRenderer = new PortableKnowledgePackRenderer();
        canonicalKnowledgeFiles[PortableKnowledgePackRenderer.InstructionsRelativePath] =
            packRenderer.RenderInstructions(sourceRepositoryLabel);
        Progress("knowledge", $"Complete: {canonicalKnowledgeFiles.Count} canonical files ready.");
        Timed("knowledge");

        if (command == "run")
        {
            var summary = BuildRunSummary(
                command,
                checkpoint?.CreatedUtc,
                state,
                coverage,
                facts,
                candidates.Candidates.Count,
                productFeatures.Features.Count,
                workflows,
                canonicalKnowledgeFiles.Count);

            Progress("workspace", "Rendering AI workspace...");
            var workspaceFiles = new Dictionary<string, string>(
                new AiWorkspaceRenderer().Render(canonicalKnowledgeFiles, sourceRepositoryLabel, WorkspaceOverview.From(summary)),
                StringComparer.Ordinal)
            {
                [AiWorkspaceRenderer.CoverageRelativePath] = ScanCoverageSerializer.SerializePortable(coverage.ToPortable())
            };
            Progress("workspace", "Writing AI workspace...");
            string? workspacePath = null;
            var workspaceTarget = Path.Combine(outputDirectory, "workspace");
            try
            {
                workspacePath = await new AiWorkspaceWriter().WriteAsync(
                    repositoryPath,
                    workspaceFiles);
                writes.Record("AI workspace", workspacePath, true);
                Progress("workspace", $"Complete: {workspaceFiles.Count} workspace files written.");
            }
            catch (WorkspaceReplaceException exception)
            {
                writes.Record("AI workspace", exception.NewWorkspacePath, false, exception.Message);
                Progress("workspace", exception.Message);
            }

            Timed("workspace");

            summary = summary with
            {
                WorkspaceFiles = CountFiles(workspacePath ?? workspaceTarget),
                Timings = timings.ToArray(),
                Artifacts = writes.Results
                    .Select(result => new RunArtifact(result.Name, RelativeTo(repositoryPath, result.Path), result.Succeeded, result.Error))
                    .ToArray()
            };
            await WriteRunSummaryAsync(repositoryPath, summary, sourceRepositoryLabel, options);
            Console.Write(summary.RenderConsole());

            if (workspacePath is not null)
            {
                Console.WriteLine($"PKC run complete: {workflows.Count} workflows, {productFeatures.Features.Count} product features");
                Console.WriteLine($"PKC AI workspace generated: {workspacePath}");
                Console.WriteLine("Workspace status: PREVIEW (pkc verify / READY-PARTIAL-FAILED is not implemented yet).");
                Console.WriteLine();
                Console.WriteLine("Open the generated workspace, not the source root:");
                Console.WriteLine($"  Claude Code: cd \"{workspacePath}\" then run `claude`.");
                Console.WriteLine("  Codex-compatible agent: open the workspace directory; AGENTS.md is the bootstrap.");
                Console.WriteLine("  PRODUCT mode is default. TRACE/ENGINEERING require explicit user intent.");
            }
        }
        else
        {
            Progress("build", "Reconciling and writing legacy knowledge outputs...");
            ReconcileGeneratedKnowledge(repositoryPath, canonicalKnowledgeFiles);
            foreach (var pair in canonicalKnowledgeFiles.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                var outputPath = Resolve(repositoryPath, pair.Key);
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
                await File.WriteAllTextAsync(outputPath, pair.Value);
                Console.WriteLine(outputPath);
            }

            var bundlePath = Path.Combine(repositoryPath, PortableKnowledgePackRenderer.BundleFileName);
            await File.WriteAllTextAsync(
                bundlePath,
                packRenderer.RenderBundle(canonicalKnowledgeFiles, sourceRepositoryLabel));
            Console.WriteLine(bundlePath);

            var archivePath = Path.Combine(repositoryPath, PortableKnowledgePackRenderer.ArchiveFileName);
            WriteKnowledgeArchive(archivePath, canonicalKnowledgeFiles);
            Console.WriteLine(archivePath);
            Progress("build", "Legacy knowledge outputs written.");

            Console.WriteLine($"PKC build complete: {workflows.Count} workflows, {productFeatures.Features.Count} product features, canonical knowledge pack + single-file bundle + ZIP generated");
            Console.WriteLine();
            Console.WriteLine("AI handoff:");
            Console.WriteLine($"  Simplest legacy flow: upload {bundlePath} to the AI, then ask product/system questions.");
            Console.WriteLine($"  Structured legacy flow: provide {Path.Combine(repositoryPath, "knowledge")} when the AI/workspace supports multiple files.");
            Console.WriteLine($"  Archive: {archivePath} is for sharing/storage or archive-capable destinations; ZIP parsing is not required.");
            Console.WriteLine("  Preferred product UX: use `pkc run <repository-path>` for the isolated Claude/Codex workspace.");
        }
    }

    foreach (var failure in writes.Failures)
    {
        Console.Error.WriteLine($"PKC {command}: {failure.Name} was not written ({failure.Error}).");
    }

    if (writes.Failures.Any())
    {
        Console.Error.WriteLine(checkpoint is null && File.Exists(Path.Combine(repositoryPath, ScanCheckpoint.RelativePath))
            ? $"PKC {command} completed with write failures. Fix the cause and re-run with --resume to reuse this scan."
            : $"PKC {command} completed with write failures.");
        return 1;
    }

    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"PKC {command} failed: {exception.Message}");
    if (command is "build" or "run" && File.Exists(Path.Combine(repositoryPath, ScanCheckpoint.RelativePath)))
    {
        Console.Error.WriteLine($"The completed scan was kept. Re-run `pkc {command} <repository-path> --resume` to continue without scanning again.");
    }

    return 1;
}

static RunSummary BuildRunSummary(
    string command,
    DateTimeOffset? resumedFromScanUtc,
    DiscoveryState state,
    ScanCoverage coverage,
    FactDocument facts,
    int workflowCandidates,
    int productFeatures,
    IReadOnlyCollection<FeatureKnowledge> workflows,
    int knowledgeFiles)
{
    int Components(OwnershipStatus ownership) => state.Profile.Components.Count(component => component.Ownership == ownership);

    return new RunSummary(
        RunSummary.CurrentSchemaVersion,
        command,
        coverage.Scoped,
        resumedFromScanUtc,
        new RunRepositoryStats(
            coverage.Files,
            Components(OwnershipStatus.Host),
            Components(OwnershipStatus.Owned),
            Components(OwnershipStatus.TestOnly),
            Components(OwnershipStatus.Unknown),
            coverage.Stages.Sum(stage => stage.PlannedFiles),
            coverage.Stages.Sum(stage => stage.ExecutableFiles),
            coverage.Stages.Sum(stage => stage.WithheldByScannerScope.Count),
            coverage.Coverage(PlanCoverage.TestEvidence),
            coverage.Coverage(PlanCoverage.Indexed),
            coverage.Coverage(PlanCoverage.NotAnalyzable),
            coverage.ExcludedAreas,
            coverage.UnknownAreas.Count),
        facts.Facts.Count,
        facts.Relations.Count,
        workflowCandidates,
        productFeatures,
        workflows.Select(workflow => workflow.Area).Distinct(StringComparer.Ordinal).Count(),
        knowledgeFiles,
        0,
        KnowledgeGrounding.From(workflows),
        RunSummary.TopAreasOf(workflows),
        [],
        []);
}

static async Task WriteRunSummaryAsync(string repositoryPath, RunSummary summary, string repositoryLabel, JsonSerializerOptions options)
{
    try
    {
        await File.WriteAllTextAsync(Resolve(repositoryPath, RunSummary.MarkdownRelativePath), summary.RenderMarkdown(repositoryLabel));
        await JsonArtifactFile.WriteAsync(Resolve(repositoryPath, RunSummary.JsonRelativePath), summary, options);
    }
    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
    {
        Progress("summary", $"Run summary was not written: {exception.Message}");
    }
}

static int CountFiles(string directory) =>
    Directory.Exists(directory) ? Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories).Count() : 0;

static string RelativeTo(string repositoryPath, string path) =>
    Path.GetRelativePath(repositoryPath, path).Replace('\\', '/');

static void Progress(string phase, string message) =>
    Console.Error.WriteLine($"[pkc:{phase}] {message}");

static void ReportDiscovery(DiscoveryState state)
{
    var profile = state.Profile;
    var plan = state.Plan;
    int Coverage(PlanCoverage coverage) => plan.Summary.FilesByCoverage.FirstOrDefault(count => count.Coverage == coverage)?.Files ?? 0;
    int Areas(ScanMode mode) => profile.Areas.Count(area => area.ScanMode == mode);
    int Files(ScanMode mode) => profile.Inventory.FilesByScanMode.FirstOrDefault(count => count.ScanMode == mode)?.Files ?? 0;
    var unknownRoleFiles = profile.Inventory.FilesByRole.FirstOrDefault(count => count.Role == SourceRole.Unknown)?.Files ?? 0;

    Progress(
        "discover",
        $"Complete: {profile.Inventory.Files} files, {profile.Manifests.Count} manifests, {profile.Areas.Count} areas.");
    Progress(
        "discover",
        $"Scope: deep {Files(ScanMode.DeepScan)} files, test-evidence {Files(ScanMode.TestEvidence)} files, " +
        $"light-index {Files(ScanMode.LightIndex)} files, runtime-index {Files(ScanMode.RuntimeDependencyIndex)} files, safe-auto-exclude {Areas(ScanMode.SafeAutoExclude)} areas, " +
        $"unknown-mode {Areas(ScanMode.Unknown)} areas, unknown-role {unknownRoleFiles} files.");
    Progress(
        "discover",
        $"Components: {profile.Components.Count(component => component.Ownership == OwnershipStatus.Host)} hosts, " +
        $"{profile.Components.Count(component => component.Ownership == OwnershipStatus.Owned)} owned, " +
        $"{profile.Components.Count(component => component.Ownership == OwnershipStatus.TestOnly)} test, " +
        $"{profile.Components.Count(component => component.Ownership == OwnershipStatus.Unknown)} unknown-ownership; " +
        $"{profile.Edges.Count} references ({profile.Edges.Count(edge => edge.Kind == "runtime-plugin-load")} runtime-plugin), " +
        $"{profile.UnresolvedReferences.Count} unresolved.");
    Progress(
        "plan",
        $"Scan plan: {plan.Scopes.Count} scopes, {plan.Summary.HostWaves} host waves; semantic {Coverage(PlanCoverage.Semantic)} files, " +
        $"indexed {Coverage(PlanCoverage.Indexed)}, test-evidence {Coverage(PlanCoverage.TestEvidence)}, " +
        $"not-analyzable {Coverage(PlanCoverage.NotAnalyzable)}, unknown {Coverage(PlanCoverage.Unknown)}; " +
        $"excluded {plan.Summary.Exclusions} areas, unknown {plan.Summary.UnknownAreas} areas.");
    Progress("discover", $"Repository profile: {state.ProfilePath}");
    Progress("plan", $"Scan plan: {state.PlanPath}");
}

static void ReportStage(SemanticStageExecution execution)
{
    Progress(
        $"scope:{execution.Name}",
        execution.Scoped
            ? $"Plan scope: {execution.PlannedFiles} planned semantic files; {execution.ExecutableFiles} within the scanner source scope, " +
              $"{execution.WithheldByScannerScope.Count} withheld by scanner name scope."
            : $"Whole-root scope ({execution.PlannedFiles} planned semantic files; plan scope applies to `pkc run`).");
}

static void ReportCoverage(ScanCoverage coverage, string coveragePath)
{
    Progress(
        "coverage",
        $"Semantic: {coverage.Stages.Sum(stage => stage.ExecutableFiles)} of {coverage.Stages.Sum(stage => stage.PlannedFiles)} planned files executable" +
        $" ({coverage.Stages.Sum(stage => stage.WithheldByScannerScope.Count)} withheld by scanner name scope); " +
        (coverage.Scoped
            ? $"withheld by plan: test-evidence {coverage.Withheld(ScanMode.TestEvidence)}, light-index {coverage.Withheld(ScanMode.LightIndex)}, " +
              $"runtime-index {coverage.Withheld(ScanMode.RuntimeDependencyIndex)} files, excluded {coverage.ExcludedAreas} areas; "
            : "whole-root scope (plan not applied); ") +
        $"not-analyzable {coverage.Coverage(PlanCoverage.NotAnalyzable)} files, unknown {coverage.Coverage(PlanCoverage.Unknown)} files / {coverage.UnknownAreas.Count} areas.");
    Progress("coverage", $"Scan coverage: {coveragePath}");
}

static bool ShouldReportProgress(int completed, int total) =>
    completed > 0 && total > 0 &&
    (completed == 1 || completed == total || completed % 25 == 0);

static string Resolve(string repositoryPath, string relativePath) =>
    Path.Combine(repositoryPath, relativePath.Replace('/', Path.DirectorySeparatorChar));

static void ReconcileGeneratedKnowledge(
    string repositoryPath,
    IReadOnlyDictionary<string, string> canonicalFiles)
{
    var knowledgeDirectory = Path.Combine(repositoryPath, "knowledge");
    if (!Directory.Exists(knowledgeDirectory))
    {
        return;
    }

    var currentGeneratedFiles = canonicalFiles.Keys
        .Select(path => Path.GetFullPath(Resolve(repositoryPath, path)))
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    foreach (var file in Directory.EnumerateFiles(knowledgeDirectory, "*.md", SearchOption.AllDirectories))
    {
        var fullPath = Path.GetFullPath(file);
        if (currentGeneratedFiles.Contains(fullPath) || !HasGeneratedFrontMatter(file))
        {
            continue;
        }

        File.Delete(file);
    }

    foreach (var directory in Directory.EnumerateDirectories(knowledgeDirectory, "*", SearchOption.AllDirectories)
                 .OrderByDescending(path => path.Length))
    {
        if (!Directory.EnumerateFileSystemEntries(directory).Any())
        {
            Directory.Delete(directory);
        }
    }
}

static bool HasGeneratedFrontMatter(string path)
{
    using var reader = File.OpenText(path);
    if (!string.Equals(reader.ReadLine()?.Trim(), "---", StringComparison.Ordinal))
    {
        return false;
    }

    for (var lineNumber = 0; lineNumber < 64; lineNumber++)
    {
        var line = reader.ReadLine();
        if (line is null || string.Equals(line.Trim(), "---", StringComparison.Ordinal))
        {
            return false;
        }

        if (string.Equals(line.Trim(), "generated: true", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
    }

    return false;
}

static void WriteKnowledgeArchive(string archivePath, IReadOnlyDictionary<string, string> canonicalFiles)
{
    if (File.Exists(archivePath))
    {
        File.Delete(archivePath);
    }

    using var archive = ZipFile.Open(archivePath, ZipArchiveMode.Create);
    foreach (var pair in canonicalFiles.OrderBy(pair => pair.Key, StringComparer.Ordinal))
    {
        var entry = archive.CreateEntry(pair.Key.Replace('\\', '/'), CompressionLevel.Optimal);
        using var stream = entry.Open();
        using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        writer.Write(pair.Value);
    }
}

static FactDocument Merge(params FactDocument[] documents)
{
    var facts = documents
        .SelectMany(document => document.Facts)
        .GroupBy(fact => fact.Id, StringComparer.Ordinal)
        .Select(group => group.First())
        .OrderBy(fact => fact.Id, StringComparer.Ordinal)
        .ToArray();

    var relations = documents
        .SelectMany(document => document.Relations)
        .GroupBy(
            relation => $"{relation.FromFactId}|{relation.Kind}|{relation.Target}|{relation.Source.Path}|{relation.Source.StartLine}",
            StringComparer.Ordinal)
        .Select(group => group.First())
        .OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
        .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
        .ThenBy(relation => relation.Target, StringComparer.Ordinal)
        .ToArray();

    return new FactDocument("0.4.4", facts, relations);
}
