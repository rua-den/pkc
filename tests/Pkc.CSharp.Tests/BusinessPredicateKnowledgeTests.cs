using Pkc.CSharp;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class BusinessPredicateKnowledgeTests
{
    [Fact]
    public async Task Query_inclusion_predicate_preserves_boolean_business_logic_in_knowledge()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pkc-business-predicate-tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "Demo.csproj"), Project);
            await File.WriteAllTextAsync(Path.Combine(root, "Catalog.cs"), Source);

            var facts = await new CSharpEvidenceScanner().ScanAsync(root);

            var predicate = Assert.Single(facts.Facts, fact =>
                fact.Kind == "business-predicate" &&
                fact.Metadata.TryGetValue("operation", out var operation) &&
                operation == "Where");

            Assert.Equal("_cards", predicate.Metadata["source"]);
            Assert.Equal("inclusion", predicate.Metadata["effect"]);
            Assert.Equal("project-semantic", predicate.Metadata["operationResolution"]);
            Assert.Equal("System.Linq.Enumerable.Where", predicate.Metadata["operationSymbol"]);
            Assert.Contains("card.IsPublished", predicate.Metadata["expression"], StringComparison.Ordinal);
            Assert.Contains("card.WebEnabled", predicate.Metadata["expression"], StringComparison.Ordinal);
            Assert.Contains("card.SaleStartsAt <= now", predicate.Metadata["expression"], StringComparison.Ordinal);
            Assert.Contains("card.SaleEndsAt == null || now < card.SaleEndsAt", predicate.Metadata["expression"], StringComparison.Ordinal);
            Assert.Contains("card.Stock > 0", predicate.Metadata["expression"], StringComparison.Ordinal);

            var configured = Assert.Single(facts.Facts, fact =>
                fact.Kind == "configured-object" &&
                fact.Metadata.TryGetValue("source", out var source) &&
                source == "_cards");
            Assert.Contains("Name = \"Mewtwo VSTAR\"", configured.Metadata["assignments"], StringComparison.Ordinal);
            Assert.Contains("IsPublished = true", configured.Metadata["assignments"], StringComparison.Ordinal);
            Assert.Contains("WebEnabled = true", configured.Metadata["assignments"], StringComparison.Ordinal);
            Assert.Contains("SaleStartsAt = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero)", configured.Metadata["assignments"], StringComparison.Ordinal);
            Assert.Contains("SaleEndsAt = null", configured.Metadata["assignments"], StringComparison.Ordinal);
            Assert.Contains("Stock = 5", configured.Metadata["assignments"], StringComparison.Ordinal);

            var candidate = Assert.Single(
                new FeatureCandidateBuilder().Build(facts).Candidates,
                candidate => candidate.Name.Contains("Get Cards", StringComparison.Ordinal));

            var knowledge = await new GroundedKnowledgeSynthesizer().SynthesizeAsync(candidate);

            Assert.Contains(knowledge.Rules, rule =>
                rule.Contains("Includes items from `_cards` only when", StringComparison.Ordinal) &&
                rule.Contains("card.IsPublished", StringComparison.Ordinal) &&
                rule.Contains("card.WebEnabled", StringComparison.Ordinal) &&
                rule.Contains("card.SaleStartsAt <= now", StringComparison.Ordinal) &&
                rule.Contains("card.SaleEndsAt == null || now < card.SaleEndsAt", StringComparison.Ordinal) &&
                rule.Contains("card.Stock > 0", StringComparison.Ordinal));

            Assert.Contains(knowledge.Rules, rule =>
                rule.Contains("Configured item in `_cards`", StringComparison.Ordinal) &&
                rule.Contains("Mewtwo VSTAR", StringComparison.Ordinal) &&
                rule.Contains("2026, 9, 1", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private const string Project = """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>net8.0</TargetFramework>
            <ImplicitUsings>enable</ImplicitUsings>
            <Nullable>enable</Nullable>
          </PropertyGroup>
        </Project>
        """;

    private const string Source = """
        using System;
        using System.Collections.Generic;
        using System.Linq;

        namespace Demo;

        public sealed class PokemonCard
        {
            public string Name { get; init; } = "";
            public bool IsPublished { get; init; }
            public bool WebEnabled { get; init; }
            public DateTimeOffset SaleStartsAt { get; init; }
            public DateTimeOffset? SaleEndsAt { get; init; }
            public int Stock { get; init; }
        }

        public sealed class Store
        {
            private readonly List<PokemonCard> _cards =
            [
                new()
                {
                    Name = "Mewtwo VSTAR",
                    IsPublished = true,
                    WebEnabled = true,
                    SaleStartsAt = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero),
                    SaleEndsAt = null,
                    Stock = 5
                }
            ];

            public IReadOnlyList<PokemonCard> GetCards(DateTimeOffset now) => _cards
                .Where(card =>
                    card.IsPublished &&
                    card.WebEnabled &&
                    card.SaleStartsAt <= now &&
                    (card.SaleEndsAt == null || now < card.SaleEndsAt) &&
                    card.Stock > 0)
                .ToArray();
        }

        [Route("api/cards")]
        public sealed class CardsController
        {
            private readonly Store _store = new();

            [HttpGet]
            public IReadOnlyList<PokemonCard> GetCards() => _store.GetCards(DateTimeOffset.UtcNow);
        }
        """;
}
