using Pkc.Core;
using Pkc.CSharp;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class CSharpProjectSemanticEnricherTests
{
    [Fact]
    public async Task Enrich_marks_node_match_failure_when_project_loads_but_fact_location_does_not_match()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-csharp-semantic-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "Fixture.csproj"), """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                  </PropertyGroup>
                </Project>
                """);

            await File.WriteAllTextAsync(Path.Combine(root, "Service.cs"), """
                namespace Fixture;

                public sealed class Service
                {
                    public void Run() { }
                }
                """);

            var fact = new EvidenceFact(
                "method:fixture:run",
                "method",
                "Run",
                "Fixture.Service",
                new SourceLocation("Service.cs", 999, 999),
                [],
                new Dictionary<string, string>());

            var document = new FactDocument("test", [fact], []);
            var enriched = await new CSharpProjectSemanticEnricher().EnrichAsync(root, document);

            var result = Assert.Single(enriched.Facts);
            Assert.Equal("project-semantic", result.Metadata["analysisMode"]);
            Assert.Equal("target-project", result.Metadata["semanticContext"]);
            Assert.Equal("medium", result.Metadata["analysisConfidence"]);
            Assert.Equal("failed", result.Metadata["semanticNodeMatch"]);
            Assert.Equal(
                "target-project-loaded-but-fact-node-match-failed",
                result.Metadata["analysisCaveat"]);
            Assert.False(result.Metadata.ContainsKey("semanticSymbol"));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Enrich_loads_each_target_project_even_when_another_project_references_it()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-csharp-multiproject-tests", Guid.NewGuid().ToString("N"));
        var app = Path.Combine(root, "App");
        var core = Path.Combine(root, "Core");
        Directory.CreateDirectory(app);
        Directory.CreateDirectory(core);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(core, "Core.csproj"), """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                  </PropertyGroup>
                </Project>
                """);

            await File.WriteAllTextAsync(Path.Combine(core, "CoreService.cs"), """
                namespace Fixture.Core;

                public sealed class CoreService
                {
                    public string Execute() => "ok";
                }
                """);

            await File.WriteAllTextAsync(Path.Combine(app, "App.csproj"), """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                  </PropertyGroup>
                  <ItemGroup>
                    <ProjectReference Include="../Core/Core.csproj" />
                  </ItemGroup>
                </Project>
                """);

            await File.WriteAllTextAsync(Path.Combine(app, "AppService.cs"), """
                using Fixture.Core;

                namespace Fixture.App;

                public sealed class AppService
                {
                    public string Run() => new CoreService().Execute();
                }
                """);

            var fact = new EvidenceFact(
                "method:core:execute",
                "method",
                "Execute",
                "Fixture.Core.CoreService",
                new SourceLocation("Core/CoreService.cs", 5, 5),
                [],
                new Dictionary<string, string>());

            var document = new FactDocument("test", [fact], []);
            var enriched = await new CSharpProjectSemanticEnricher().EnrichAsync(root, document);

            var result = Assert.Single(enriched.Facts);
            Assert.Equal("project-semantic", result.Metadata["analysisMode"]);
            Assert.Equal("target-project", result.Metadata["semanticContext"]);
            Assert.Equal("Core/Core.csproj", result.Metadata["semanticProject"]);
            Assert.Equal("high", result.Metadata["analysisConfidence"]);
            Assert.Equal("matched", result.Metadata["semanticNodeMatch"]);
            Assert.Contains("CoreService.Execute", result.Metadata["semanticSymbol"], StringComparison.Ordinal);
            Assert.False(result.Metadata.ContainsKey("analysisFallbackReason"));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
