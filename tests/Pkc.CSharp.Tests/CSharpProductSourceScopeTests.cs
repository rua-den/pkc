using Pkc.CSharp;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class CSharpProductSourceScopeTests
{
    [Fact]
    public async Task Scan_excludes_test_and_spike_endpoints_from_product_evidence()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-source-scope-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await WriteWebProjectAsync(root, "src/App", "/production");
            await WriteWebProjectAsync(root, "tests/FakeHost", "/test-only");
            await WriteWebProjectAsync(root, "spikes/PrototypeHost", "/spike-only");

            var document = await new CSharpEvidenceScanner().ScanAsync(root);

            var endpoints = document.Facts
                .Where(fact => fact.Kind == "endpoint")
                .ToArray();

            var endpoint = Assert.Single(endpoints);
            Assert.Equal("/production", endpoint.Metadata["fullRoute"]);
            Assert.StartsWith("src/App/", endpoint.Source.Path, StringComparison.Ordinal);

            Assert.DoesNotContain(document.Facts, fact =>
                fact.Source.Path.StartsWith("tests/", StringComparison.OrdinalIgnoreCase) ||
                fact.Source.Path.StartsWith("spikes/", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(document.Relations, relation =>
                relation.Source.Path.StartsWith("tests/", StringComparison.OrdinalIgnoreCase) ||
                relation.Source.Path.StartsWith("spikes/", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static async Task WriteWebProjectAsync(string root, string relativeDirectory, string route)
    {
        var directory = Path.Combine(root, relativeDirectory.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(directory);

        var projectName = Path.GetFileName(directory);
        await File.WriteAllTextAsync(Path.Combine(directory, $"{projectName}.csproj"), """
            <Project Sdk="Microsoft.NET.Sdk.Web">
              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """);

        await File.WriteAllTextAsync(Path.Combine(directory, "Program.cs"), $$"""
            var app = WebApplication.CreateBuilder(args).Build();
            app.MapGet("{{route}}", () => Results.Ok());
            """);
    }
}
