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
    var csharpFacts = await new CSharpRepositoryScanner().ScanAsync(repositoryPath);
    var frontendFacts = await ScanFrontendAsync(repositoryPath);
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
        var synthesizer = new GroundedKnowledgeSynthesizer();
        var workflowRenderer = new MarkdownKnowledgeRenderer();
        var workflows = new List<FeatureKnowledge>();

        foreach (var candidate in candidates.Candidates)
        {
            var workflow = await synthesizer.SynthesizeAsync(candidate);
            workflows.Add(workflow);
            var outputPath = Resolve(repositoryPath, workflowRenderer.GetRelativePath(workflow));
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            await File.WriteAllTextAsync(outputPath, workflowRenderer.Render(workflow));
            Console.WriteLine(outputPath);
        }

        var productFeatures = new ProductFeatureBuilder().Build(workflows);
        var productFeaturesPath = Path.Combine(outputDirectory, "product-features.json");
        await File.WriteAllTextAsync(productFeaturesPath, JsonSerializer.Serialize(productFeatures, options));
        Console.WriteLine(productFeaturesPath);

        var featureRenderer = new ProductFeatureMarkdownRenderer();
        foreach (var feature in productFeatures.Features)
        {
            var outputPath = Resolve(repositoryPath, featureRenderer.GetRelativePath(feature));
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            await File.WriteAllTextAsync(outputPath, featureRenderer.Render(feature));
            Console.WriteLine(outputPath);
        }

        var indexPath = Resolve(repositoryPath, "knowledge/index.md");
        Directory.CreateDirectory(Path.GetDirectoryName(indexPath)!);
        await File.WriteAllTextAsync(indexPath, featureRenderer.RenderIndex(productFeatures));
        Console.WriteLine(indexPath);
        Console.WriteLine($"PKC build complete: {workflows.Count} workflows, {productFeatures.Features.Count} product features, 1 knowledge index generated");
    }

    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"PKC {command} failed: {exception.Message}");
    return 1;
}

static async Task<FactDocument> ScanFrontendAsync(string repositoryPath)
{
    var isAngular = Directory.EnumerateFiles(repositoryPath, "angular.json", SearchOption.AllDirectories)
        .Any(path => !path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(segment => string.Equals(segment, "node_modules", StringComparison.OrdinalIgnoreCase)));

    return isAngular
        ? await new AngularRepositoryScanner().ScanAsync(repositoryPath)
        : await new FrontendRepositoryScanner().ScanAsync(repositoryPath);
}

static string Resolve(string repositoryPath, string relativePath) =>
    Path.Combine(repositoryPath, relativePath.Replace('/', Path.DirectorySeparatorChar));

static FactDocument Merge(params FactDocument[] documents) =>
    new(
        "0.4.1",
        documents.SelectMany(document => document.Facts).OrderBy(fact => fact.Id, StringComparer.Ordinal).ToArray(),
        documents.SelectMany(document => document.Relations)
            .OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
            .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
            .ThenBy(relation => relation.Target, StringComparer.Ordinal)
            .ToArray());
