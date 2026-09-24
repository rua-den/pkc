using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Pkc.Core;
using Pkc.Core.Discovery;
using Pkc.CSharp;
using Pkc.Frontend;
using Pkc.Knowledge;

var command = args.Length > 0 ? args[0] : string.Empty;
if (args.Length != 2 || (command != "scan" && command != "build" && command != "run"))
{
    Console.Error.WriteLine("Usage: pkc <scan|build|run> <repository-path>");
    return 2;
}

var repositoryPath = Path.GetFullPath(args[1]);
if (!Directory.Exists(repositoryPath))
{
    Console.Error.WriteLine($"Repository path does not exist: {repositoryPath}");
    return 2;
}

try
{
    Progress("start", $"PKC {command} started.");

    // Discovery establishes and persists the repository profile before any expensive semantic scanner starts.
    // RD1 keeps each scanner's existing whole-root semantic scope; scoped execution belongs to a later checkpoint.
    Progress("discover", "Discovering repository shape before semantic analysis...");
    var scan = await new DiscoveryFirstScanPipeline(ReportDiscovery).RunAsync(
        repositoryPath,
        [
            new SemanticScanStage("csharp", async (_, _) =>
            {
                Progress("scan:csharp", "Scanning C# repository evidence...");
                var document = await new CSharpEvidenceScanner().ScanAsync(repositoryPath);
                Progress(
                    "scan:csharp",
                    $"Complete: {document.Facts.Count} facts, {document.Relations.Count} relations.");
                return document;
            }),
            new SemanticScanStage("frontend", async (_, _) =>
            {
                Progress("scan:frontend", "Scanning frontend repository evidence...");
                var document = await new FrontendScanner().ScanAsync(repositoryPath);
                Progress(
                    "scan:frontend",
                    $"Complete: {document.Facts.Count} facts, {document.Relations.Count} relations.");
                return document;
            })
        ]);
    var csharpFacts = scan.Documents[0];
    var frontendFacts = scan.Documents[1];

    Progress("merge", "Merging repository evidence...");
    var facts = Merge(csharpFacts, frontendFacts);
    Progress("merge", $"Complete: {facts.Facts.Count} facts, {facts.Relations.Count} relations.");

    Progress("link", "Building cross-stack workflow candidates...");
    var candidates = new CrossStackFeatureCandidateBuilder().Build(facts);
    Progress("enrich", "Enriching workflow candidates with validation and visibility evidence...");
    candidates = new ValidationConsistencyCandidateEnricher().Enrich(candidates, facts);
    candidates = new JointVisibilityCandidateEnricher().Enrich(candidates, facts);
    Progress("link", $"Complete: {candidates.Candidates.Count} workflow candidates.");

    var outputDirectory = Path.Combine(repositoryPath, ".pkc");
    Directory.CreateDirectory(outputDirectory);

    var options = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    Progress("write", "Writing scan artifacts...");
    var factsPath = Path.Combine(outputDirectory, "facts.json");
    await File.WriteAllTextAsync(factsPath, JsonSerializer.Serialize(facts, options));

    var candidatesPath = Path.Combine(outputDirectory, "feature-candidates.json");
    await File.WriteAllTextAsync(candidatesPath, JsonSerializer.Serialize(candidates, options));
    Progress("write", "Scan artifacts written.");

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

        Progress("product", "Building product features...");
        var productFeatures = new ProductFeatureBuilder().Build(workflows);
        var productFeaturesPath = Path.Combine(outputDirectory, "product-features.json");
        await File.WriteAllTextAsync(productFeaturesPath, JsonSerializer.Serialize(productFeatures, options));
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

        if (command == "run")
        {
            Progress("workspace", "Rendering AI workspace...");
            var workspaceFiles = new AiWorkspaceRenderer().Render(
                canonicalKnowledgeFiles,
                sourceRepositoryLabel);
            Progress("workspace", "Writing AI workspace...");
            var workspacePath = await new AiWorkspaceWriter().WriteAsync(
                repositoryPath,
                workspaceFiles);
            Progress("workspace", $"Complete: {workspaceFiles.Count} workspace files written.");

            Console.WriteLine($"PKC run complete: {workflows.Count} workflows, {productFeatures.Features.Count} product features");
            Console.WriteLine($"PKC AI workspace generated: {workspacePath}");
            Console.WriteLine("Workspace status: PREVIEW (pkc verify / READY-PARTIAL-FAILED is not implemented yet).");
            Console.WriteLine();
            Console.WriteLine("Open the generated workspace, not the source root:");
            Console.WriteLine($"  Claude Code: cd \"{workspacePath}\" then run `claude`.");
            Console.WriteLine("  Codex-compatible agent: open the workspace directory; AGENTS.md is the bootstrap.");
            Console.WriteLine("  PRODUCT mode is default. TRACE/ENGINEERING require explicit user intent.");
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

    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"PKC {command} failed: {exception.Message}");
    return 1;
}

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
