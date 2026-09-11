using System.Text.Json;
using System.Text.Json.Serialization;
using Pkc.CSharp;

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
    var document = await scanner.ScanAsync(repositoryPath);

    var outputDirectory = Path.Combine(repositoryPath, ".pkc");
    Directory.CreateDirectory(outputDirectory);

    var outputPath = Path.Combine(outputDirectory, "facts.json");
    var options = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(document, options));

    Console.WriteLine($"PKC scan complete: {document.Facts.Count} facts, {document.Relations.Count} relations");
    Console.WriteLine(outputPath);
    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"PKC scan failed: {exception.Message}");
    return 1;
}
