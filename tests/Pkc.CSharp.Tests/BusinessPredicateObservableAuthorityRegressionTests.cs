using Pkc.CSharp;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class BusinessPredicateObservableAuthorityRegressionTests
{
    [Fact]
    public async Task Local_query_predicate_that_does_not_affect_return_is_not_an_authoritative_rule()
    {
        var knowledge = await BuildKnowledgeAsync("""
            public IReadOnlyList<Card> GetCards()
            {
                var published = _cards.Where(card => card.IsPublished).ToArray();
                Audit(published.Length);
                return _cards;
            }

            private static void Audit(int count) { }
            """);

        Assert.DoesNotContain(knowledge.Rules, rule =>
            rule.Contains("Includes items from `_cards` only when", StringComparison.Ordinal) &&
            rule.Contains("card.IsPublished", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Any_guard_that_throws_is_not_rendered_as_a_positive_existence_requirement()
    {
        var knowledge = await BuildKnowledgeAsync("""
            public IReadOnlyList<Card> GetCards()
            {
                if (_cards.Any(card => card.Blocked))
                {
                    throw new InvalidOperationException("Blocked card cannot be returned.");
                }

                return _cards;
            }
            """);

        Assert.DoesNotContain(knowledge.Rules, rule =>
            rule.Contains("Requires at least one item from `_cards`", StringComparison.Ordinal) &&
            rule.Contains("card.Blocked", StringComparison.Ordinal));

        Assert.Contains(knowledge.Rules, rule =>
            rule.Contains("_cards.Any(card => card.Blocked)", StringComparison.Ordinal) &&
            rule.Contains("throws", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Returned_filter_predicate_remains_authoritative()
    {
        var knowledge = await BuildKnowledgeAsync("""
            public IReadOnlyList<Card> GetCards() =>
                _cards.Where(card => card.IsPublished).ToArray();
            """);

        Assert.Contains(knowledge.Rules, rule =>
            rule.Contains("Includes items from `_cards` only when", StringComparison.Ordinal) &&
            rule.Contains("card.IsPublished", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Returned_filter_through_supported_projection_pipeline_remains_authoritative()
    {
        var knowledge = await BuildKnowledgeAsync("""
            public IReadOnlyList<Card> GetCards() =>
                _cards
                    .Where(card => card.IsPublished)
                    .Select(card => card)
                    .ToArray();
            """);

        Assert.Contains(knowledge.Rules, rule =>
            rule.Contains("Includes items from `_cards` only when", StringComparison.Ordinal) &&
            rule.Contains("card.IsPublished", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Non_identity_projection_does_not_preserve_returned_filter_authority()
    {
        var knowledge = await BuildKnowledgeAsync(
            """
            public IReadOnlyList<Card> GetCards() =>
                _cards
                    .Where(card => card.IsPublished)
                    .Select(_ => _cards[0])
                    .ToArray();
            """,
            "[new Card(IsPublished: false, Blocked: false), new Card(IsPublished: true, Blocked: false)]");

        Assert.DoesNotContain(knowledge.Rules, rule =>
            rule.Contains("Includes items from `_cards` only when", StringComparison.Ordinal) &&
            rule.Contains("card.IsPublished", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Predicate_preserving_same_type_method_group_projection_remains_authoritative()
    {
        var knowledge = await BuildKnowledgeAsync(
            """
            public IReadOnlyList<Card> GetCards() =>
                _cards
                    .Where(card => card.IsPublished)
                    .Select(CloneCard)
                    .ToArray();

            private static Card CloneCard(Card card) => new()
            {
                IsPublished = card.IsPublished,
                Blocked = card.Blocked
            };
            """,
            "[new() { IsPublished = true, Blocked = false }, new() { IsPublished = false, Blocked = true }]",
            AutoPropertyCardDeclaration);

        Assert.Contains(knowledge.Rules, rule =>
            rule.Contains("Includes items from `_cards` only when", StringComparison.Ordinal) &&
            rule.Contains("card.IsPublished", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Whole_item_helper_dependency_downgrades_same_type_projection_authority()
    {
        var knowledge = await BuildKnowledgeAsync(
            """
            public IReadOnlyList<Card> GetCards() =>
                _cards
                    .Where(card => card.IsPublished && IsAllowed(card))
                    .Select(CloneCard)
                    .ToArray();

            private static bool IsAllowed(Card card) => !card.Blocked;

            private static Card CloneCard(Card card) => new()
            {
                IsPublished = card.IsPublished,
                Blocked = true
            };
            """,
            "[new() { IsPublished = true, Blocked = false }]",
            AutoPropertyCardDeclaration);

        Assert.DoesNotContain(knowledge.Rules, rule =>
            rule.Contains("Includes items from `_cards` only when", StringComparison.Ordinal) &&
            rule.Contains("IsAllowed(card)", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Same_type_method_group_that_changes_predicate_member_is_not_authoritative()
    {
        var knowledge = await BuildKnowledgeAsync(
            """
            public IReadOnlyList<Card> GetCards() =>
                _cards
                    .Where(card => card.IsPublished)
                    .Select(RewriteCard)
                    .ToArray();

            private static Card RewriteCard(Card card) => new()
            {
                IsPublished = false,
                Blocked = card.Blocked
            };
            """,
            "[new() { IsPublished = true, Blocked = false }, new() { IsPublished = false, Blocked = true }]",
            AutoPropertyCardDeclaration);

        Assert.DoesNotContain(knowledge.Rules, rule =>
            rule.Contains("Includes items from `_cards` only when", StringComparison.Ordinal) &&
            rule.Contains("card.IsPublished", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Filter_passed_to_helper_that_discards_result_is_not_authoritative()
    {
        var knowledge = await BuildKnowledgeAsync("""
            public IReadOnlyList<Card> GetCards() =>
                ReturnAll(_cards.Where(card => card.IsPublished).ToArray());

            private IReadOnlyList<Card> ReturnAll(IReadOnlyList<Card> ignored) => _cards;
            """);

        Assert.DoesNotContain(knowledge.Rules, rule =>
            rule.Contains("Includes items from `_cards` only when", StringComparison.Ordinal) &&
            rule.Contains("card.IsPublished", StringComparison.Ordinal));
    }

    private static async Task<FeatureKnowledge> BuildKnowledgeAsync(
        string methodBody,
        string cardsInitializer = "[new(true, false), new(false, true)]",
        string cardDeclaration = "public sealed record Card(bool IsPublished, bool Blocked);")
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pkc-business-predicate-observable-authority-tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "Demo.csproj"), Project);
            await File.WriteAllTextAsync(
                Path.Combine(root, "Catalog.cs"),
                Source(methodBody, cardsInitializer, cardDeclaration));

            var facts = await new CSharpEvidenceScanner().ScanAsync(root);

            Assert.Contains(facts.Facts, fact =>
                fact.Kind == "business-predicate" &&
                fact.Metadata.TryGetValue("expression", out var expression) &&
                (expression.Contains("card.IsPublished", StringComparison.Ordinal) ||
                 expression.Contains("card.Blocked", StringComparison.Ordinal)));

            var candidate = Assert.Single(
                new FeatureCandidateBuilder().Build(facts).Candidates,
                candidate => candidate.Name.Contains("Get Cards", StringComparison.Ordinal));

            return await new GroundedKnowledgeSynthesizer().SynthesizeAsync(candidate);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static string Source(
        string methodBody,
        string cardsInitializer,
        string cardDeclaration) => $$"""
        using System;
        using System.Collections.Generic;
        using System.Linq;

        namespace Demo;

        {{cardDeclaration}}

        public sealed class Store
        {
            private readonly List<Card> _cards = {{cardsInitializer}};

            {{methodBody}}
        }

        [Route("api/cards")]
        public sealed class CardsController
        {
            private readonly Store _store = new();

            [HttpGet]
            public IReadOnlyList<Card> GetCards() => _store.GetCards();
        }
        """;

    private const string AutoPropertyCardDeclaration = """
        public sealed class Card
        {
            public bool IsPublished { get; init; }
            public bool Blocked { get; init; }
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
