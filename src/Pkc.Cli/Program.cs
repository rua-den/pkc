using System.Text.Json;
using System.Text.Json.Serialization;
using Pkc.CSharp;
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
    var scanner = new CSharpRepositoryScanner();
    var facts = await scanner.ScanAsync(repositoryPath);
    var candidates = new FeatureCandidateBuilder().Build(facts);

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

    Console.WriteLine($"PKC scan complete: {facts.Facts.Count} facts, {facts.Relations.Count} relations, {candidates.Candidates.Count} feature candidates");
    Console.WriteLine(factsPath);
    Console.WriteLine(candidatesPath);

    if (command == "build")
    {
        var synthesizer = new GroundedKnowledgeSynthesizer();
        var renderer = new MarkdownKnowledgeRenderer();
        var generated = 0;

        foreach (var candidate in candidates.Candidates)
        {
            var knowledge = await synthesizer.SynthesizeAsync(candidate);
            var relativePath = renderer.GetRelativePath(knowledge);
            var outputPath = Path.Combine(repositoryPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            await File.WriteAllTextAsync(outputPath, renderer.Render(knowledge));
            Console.WriteLine(outputPath);
            generated++;
        }

        Console.WriteLine($"PKC build complete: {generated} Markdown knowledge files generated");
    }

    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"PKC {command} failed: {exception.Message}");
    return 1;
}
