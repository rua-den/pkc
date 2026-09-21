using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Pkc.Core;
using Pkc.CSharp;
using Pkc.Frontend;
using Pkc.Knowledge;

var command = args.Length > 0 ? args[0] : string.Empty;
if (args.Length != 2 || (command != "scan" && command != "build"))
{
    Console.Error.WriteLine("Usage: pkc <scan|build> <repository-path>");
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
    var csharpFacts = await new CSharpEvidenceScanner().ScanAsync(repositoryPath);
    var frontendFacts = await new FrontendScanner().ScanAsync(repositoryPath);
    var facts = Merge(csharpFacts, frontendFacts);
    var candidates = new CrossStackFeatureCandidateBuilder().Build(facts);
    candidates = new ValidationConsistencyCandidateEnricher().Enrich(candidates, facts);
    candidates = new JointVisibilityCandidateEnricher().Enrich(candidates, facts);

    var outputDirectory = Path.Combine(repositoryPath, ".pkc");
    Directory.CreateDirectory(outputDirectory);

    var options = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    var factsPath = Path.Combine(outputDirectory, "facts.json");
    await File.WriteAllTextAsync(factsPath, JsonSerializer.Serialize(facts, options));

    var candidatesPath = Path.Combine(outputDirectory, "feature-candidates.json");
    await File.WriteAllTextAsync(candidatesPath, JsonSerializer.Serialize(candidates, options));

    Console.WriteLine($"PKC scan complete: {facts.Facts.Count} facts, {facts.Relations.Count} relations, {candidates.Candidates.Count} workflow candidates");
    Console.WriteLine(factsPath);
    Console.WriteLine(candidatesPath);

    if (command == "build")
    {
        var synthesizer = new JointVisibilityKnowledgeSynthesizer();
        var workflowRenderer = new MarkdownKnowledgeRenderer();
        var workflows = new List<FeatureKnowledge>();
        var canonicalKnowledgeFiles = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var candidate in candidates.Candidates)
        {
            var workflow = await synthesizer.SynthesizeAsync(candidate);
            workflows.Add(workflow);
            var relativePath = workflowRenderer.GetRelativePath(workflow);
            canonicalKnowledgeFiles[relativePath] = workflowRenderer.Render(workflow);
        }

        var productFeatures = new ProductFeatureBuilder().Build(workflows);
        var productFeaturesPath = Path.Combine(outputDirectory, "product-features.json");
        await File.WriteAllTextAsync(productFeaturesPath, JsonSerializer.Serialize(productFeatures, options));
        Console.WriteLine(productFeaturesPath);

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

        ReconcileGeneratedKnowledge(repositoryPath, canonicalKnowledgeFiles);
        foreach (var pair in canonicalKnowledgeFiles.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            var outputPath = Resolve(repositoryPath, pair.Key);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            await File.WriteAllTextAsync(outputPath, pair.Value);
            Console.WriteLine(outputPath);
        }

        var bundlePath = Path.Combine(repositoryPath, PortableKnowledgePackRenderer.BundleFileName);
        await File.WriteAllTextAsync(bundlePath, packRenderer.RenderBundle(canonicalKnowledgeFiles, sourceRepositoryLabel));
        Console.WriteLine(bundlePath);

        var archivePath = Path.Combine(repositoryPath, PortableKnowledgePackRenderer.ArchiveFileName);
        WriteKnowledgeArchive(archivePath, canonicalKnowledgeFiles);
        Console.WriteLine(archivePath);

        Console.WriteLine($"PKC build complete: {workflows.Count} workflows, {productFeatures.Features.Count} product features, canonical knowledge pack + single-file bundle + ZIP generated");
        Console.WriteLine();
        Console.WriteLine("AI handoff:");
        Console.WriteLine($"  Simplest: upload {bundlePath} to the AI, then ask product/system questions.");
        Console.WriteLine($"  Structured: provide {Path.Combine(repositoryPath, "knowledge")} when the AI/workspace supports multiple files.");
        Console.WriteLine($"  Archive: {archivePath} is for sharing/storage or archive-capable destinations; ZIP parsing is not required.");
    }

    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"PKC {command} failed: {exception.Message}");
    return 1;
}

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
