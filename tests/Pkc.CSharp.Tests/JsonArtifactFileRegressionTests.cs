using System.Text.Json;
using System.Text.Json.Serialization;
using Pkc.Core;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class JsonArtifactFileRegressionTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    [Fact]
    public async Task Streamed_artifacts_are_byte_identical_to_the_previous_string_based_output()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-json-artifact", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var facts = Enumerable.Range(0, 2_000)
                .Select(index => new EvidenceFact(
                    $"cs:fact:{index:D5}",
                    index % 2 == 0 ? "endpoint" : "value-terminal-source",
                    $"Name{index} — \"quoted\" <tag> & ÆØÅ",
                    index % 3 == 0 ? null : $"Container{index}",
                    new SourceLocation($"src/Area{index % 7}/File{index}.cs", index + 1, index + 3),
                    index % 5 == 0 ? ["HttpGet", "Route(\"api/x\")"] : [],
                    new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["boundary"] = "API response field",
                        ["note"] = "line1\nline2\ttab"
                    }))
                .ToArray();
            var relations = facts
                .Skip(1)
                .Select((fact, index) => new EvidenceRelation(facts[index].Id, "calls", fact.Id, fact.Source))
                .ToArray();
            var document = new FactDocument("test", facts, relations);

            var expectedPath = Path.Combine(root, "expected.json");
            var actualPath = Path.Combine(root, "actual.json");
            await File.WriteAllTextAsync(expectedPath, JsonSerializer.Serialize(document, Options));
            await JsonArtifactFile.WriteAsync(actualPath, document, Options);

            Assert.Equal(await File.ReadAllBytesAsync(expectedPath), await File.ReadAllBytesAsync(actualPath));

            // Overwrites rather than appends.
            await JsonArtifactFile.WriteAsync(actualPath, new FactDocument("small", [], []), Options);
            Assert.Equal(JsonSerializer.Serialize(new FactDocument("small", [], []), Options), await File.ReadAllTextAsync(actualPath));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Cli_streams_large_json_artifacts_instead_of_materializing_one_string()
    {
        var source = File.ReadAllText(FindProgramSource());

        Assert.DoesNotContain("JsonSerializer.Serialize(facts", source, StringComparison.Ordinal);
        Assert.DoesNotContain("JsonSerializer.Serialize(candidates", source, StringComparison.Ordinal);
        Assert.DoesNotContain("JsonSerializer.Serialize(productFeatures", source, StringComparison.Ordinal);
        // ArtifactWriteLog.TryWriteJsonAsync streams through JsonArtifactFile.WriteAsync and records failures.
        Assert.Contains("writes.TryWriteJsonAsync(\"Evidence (facts)\", factsPath, facts, options)", source, StringComparison.Ordinal);
        Assert.Contains("writes.TryWriteJsonAsync(\"Workflow candidates\", candidatesPath, candidates, options)", source, StringComparison.Ordinal);
        Assert.Contains("writes.TryWriteJsonAsync(\"Product features\", productFeaturesPath, productFeatures, options)", source, StringComparison.Ordinal);
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

        throw new FileNotFoundException("src/Pkc.Cli/Program.cs was not found.");
    }
}
