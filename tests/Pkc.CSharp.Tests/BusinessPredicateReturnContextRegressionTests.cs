using Pkc.CSharp;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class BusinessPredicateReturnContextRegressionTests
{
    [Fact]
    public async Task Direct_any_return_is_observable_but_not_rendered_as_a_requirement()
    {
        var knowledge = await BuildKnowledgeAsync(
            "Has Blocked Cards",
            "public bool HasBlockedCards() => _cards.Any(card => card.Blocked);",
            "public bool HasBlockedCards() => _store.HasBlockedCards();",
            "Any");

        Assert.DoesNotContain(knowledge.Rules, rule =>
            rule.Contains("Requires at least one item", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Direct_all_return_is_observable_but_not_rendered_as_a_requirement()
    {
        var knowledge = await BuildKnowledgeAsync(
            "All Cards Published",
            "public bool AllCardsPublished() => _cards.All(card => card.IsPublished);",
            "public bool AllCardsPublished() => _store.AllCardsPublished();",
            "All");

        Assert.DoesNotContain(knowledge.Rules, rule =>
            rule.Contains("Requires every item", StringComparison.Ordinal));
    }

    private static async Task<FeatureKnowledge> BuildKnowledgeAsync(
        string endpointName,
        string storeMethod,
        string controllerMethod,
        string operation)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pkc-business-predicate-return-context-tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "Demo.csproj"), Project);
            await File.WriteAllTextAsync(Path.Combine(root, "Catalog.cs"), Source(storeMethod, controllerMethod));

            var facts = await new CSharpEvidenceScanner().ScanAsync(root);
            var predicate = Assert.Single(facts.Facts, fact =>
                fact.Kind == "business-predicate" &&
                fact.Metadata.TryGetValue("operation", out var value) &&
                value == operation);

            Assert.Equal("observable", predicate.Metadata["businessRuleAuthority"]);
            Assert.Equal("return", predicate.Metadata["observableContext"]);

            var candidate = Assert.Single(
                new FeatureCandidateBuilder().Build(facts).Candidates,
                candidate => candidate.Name.Contains(endpointName, StringComparison.Ordinal));

            return await new GroundedKnowledgeSynthesizer().SynthesizeAsync(candidate);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static string Source(string storeMethod, string controllerMethod) => $$"""
        using System.Collections.Generic;
        using System.Linq;

        namespace Demo;

        public sealed record Card(bool IsPublished, bool Blocked);

        public sealed class Store
        {
            private readonly List<Card> _cards = [new(true, false), new(false, true)];
            {{storeMethod}}
        }

        [Route("api/cards")]
        public sealed class CardsController
        {
            private readonly Store _store = new();

            [HttpGet]
            {{controllerMethod}}
        }
        """;

    private const string Project = """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>net8.0</TargetFramework>
            <ImplicitUsings>enable</ImplicitUsings>
            <Nullable>enable</Nullable>
          </PropertyGroup>
        </Project>
        """;
}
