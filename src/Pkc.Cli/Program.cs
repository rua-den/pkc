using System.Text.Json;
using System.Text.Json.Serialization;
using Pkc.CSharp;
using Pkc.Knowledge;

if (args.Length != 2 || !string.Equals(args[0], "scan", StringComparison.OrdinalIgnoreCase))
{
    Console.Error.WriteLine("Usage: pkc scan <repository-path>");
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
    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"PKC scan failed: {exception.Message}");
    return 1;
}
