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
            await File.WriteAllTextAsync(Path.Combine(root, "Catalog.cs"), Source);
            var facts = await new CSharpEvidenceScanner().ScanAsync(root);

            var predicate = Assert.Single(facts.Facts, fact =>
                fact.Kind == "business-predicate" &&
                fact.Metadata.TryGetValue("operation", out var operation) &&
                operation == "Where");

            Assert.Equal("_cards", predicate.Metadata["source"]);
            Assert.Equal("inclusion", predicate.Metadata["effect"]);
            Assert.Contains("card.IsPublished", predicate.Metadata["expression"], StringComparison.Ordinal);
            Assert.Contains("card.WebEnabled", predicate.Metadata["expression"], StringComparison.Ordinal);
            Assert.Contains("card.SaleStartsAt <= now", predicate.Metadata["expression"], StringComparison.Ordinal);
            Assert.Contains("card.SaleEndsAt == null || now < card.SaleEndsAt", predicate.Metadata["expression"], StringComparison.Ordinal);
            Assert.Contains("card.Stock > 0", predicate.Metadata["expression"], StringComparison.Ordinal);

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
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private const string Source = """
        using System;
        using System.Collections.Generic;
        using System.Linq;

        namespace Demo;

        public sealed class PokemonCard
        {
            public bool IsPublished { get; init; }
            public bool WebEnabled { get; init; }
            public DateTimeOffset SaleStartsAt { get; init; }
            public DateTimeOffset? SaleEndsAt { get; init; }
            public int Stock { get; init; }
        }

        public sealed class Store
        {
            private readonly List<PokemonCard> _cards = [];

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
