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
        var synthesizer = new EvidenceAwareKnowledgeSynthesizer();
        var workflowRenderer = new MarkdownKnowledgeRenderer();
        var workflows = new List<FeatureKnowledge>();
        var canonicalKnowledgeFiles = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var candidate in candidates.Candidates)
        {
            var workflow = await synthesizer.SynthesizeAsync(candidate);
            workflows.Add(workflow);
            var relativePath = workflowRenderer.GetRelativePath(workflow);
            var content = workflowRenderer.Render(workflow);
            var outputPath = Resolve(repositoryPath, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            await File.WriteAllTextAsync(outputPath, content);
            canonicalKnowledgeFiles[relativePath] = content;
            Console.WriteLine(outputPath);
        }

        var productFeatures = new ProductFeatureBuilder().Build(workflows);
        var productFeaturesPath = Path.Combine(outputDirectory, "product-features.json");
        await File.WriteAllTextAsync(productFeaturesPath, JsonSerializer.Serialize(productFeatures, options));
        Console.WriteLine(productFeaturesPath);

        var featureRenderer = new ProductFeatureMarkdownRenderer();
        foreach (var feature in productFeatures.Features)
        {
            var relativePath = featureRenderer.GetRelativePath(feature);
            var content = featureRenderer.Render(feature);
            var outputPath = Resolve(repositoryPath, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            await File.WriteAllTextAsync(outputPath, content);
            canonicalKnowledgeFiles[relativePath] = content;
            Console.WriteLine(outputPath);
        }

        const string indexRelativePath = "knowledge/index.md";
        var indexContent = featureRenderer.RenderIndex(productFeatures);
        var indexPath = Resolve(repositoryPath, indexRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(indexPath)!);
        await File.WriteAllTextAsync(indexPath, indexContent);
        canonicalKnowledgeFiles[indexRelativePath] = indexContent;
        Console.WriteLine(indexPath);

        var sourceRepositoryLabel = new DirectoryInfo(repositoryPath).Name;
        var packRenderer = new PortableKnowledgePackRenderer();
        var instructionsContent = packRenderer.RenderInstructions(sourceRepositoryLabel);
        var instructionsPath = Resolve(repositoryPath, PortableKnowledgePackRenderer.InstructionsRelativePath);
        await File.WriteAllTextAsync(instructionsPath, instructionsContent);
        canonicalKnowledgeFiles[PortableKnowledgePackRenderer.InstructionsRelativePath] = instructionsContent;
        Console.WriteLine(instructionsPath);

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
