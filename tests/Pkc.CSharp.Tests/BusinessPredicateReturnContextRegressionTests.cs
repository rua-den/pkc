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
        Assert.Contains(knowledge.Rules, rule =>
            rule.Contains("Returns whether at least one item from `_cards` satisfies `card.Blocked`.", StringComparison.Ordinal));
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
        Assert.Contains(knowledge.Rules, rule =>
            rule.Contains("Returns whether every item from `_cards` satisfies `card.IsPublished`.", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Negated_any_return_is_observed_only_because_polarity_is_inverted()
    {
        var knowledge = await BuildKnowledgeAsync(
            "Has No Blocked Cards",
            "public bool HasNoBlockedCards() => !_cards.Any(card => card.Blocked);",
            "public bool HasNoBlockedCards() => _store.HasNoBlockedCards();",
            "Any",
            expectObservable: false);

        Assert.DoesNotContain(knowledge.Rules, rule =>
            rule.Contains("card.Blocked", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Negated_all_return_is_observed_only_because_polarity_is_inverted()
    {
        var knowledge = await BuildKnowledgeAsync(
            "Not All Cards Published",
            "public bool NotAllCardsPublished() => !_cards.All(card => card.IsPublished);",
            "public bool NotAllCardsPublished() => _store.NotAllCardsPublished();",
            "All",
            expectObservable: false);

        Assert.DoesNotContain(knowledge.Rules, rule =>
            rule.Contains("card.IsPublished", StringComparison.Ordinal));
    }

    [Fact]
    public async Task First_or_default_null_test_is_not_rendered_as_returning_a_selected_item()
    {
        var knowledge = await BuildKnowledgeAsync(
            "Has No Published Card",
            "public bool HasNoPublishedCard() => _cards.FirstOrDefault(card => card.IsPublished) is null;",
            "public bool HasNoPublishedCard() => _store.HasNoPublishedCard();",
            "FirstOrDefault",
            expectObservable: false);

        Assert.DoesNotContain(knowledge.Rules, rule =>
            rule.Contains("Returns an item", StringComparison.Ordinal) &&
            rule.Contains("card.IsPublished", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Single_or_default_null_test_is_not_rendered_as_returning_a_selected_item()
    {
        var knowledge = await BuildKnowledgeAsync(
            "Has One Published Card",
            "public bool HasOnePublishedCard() => _cards.SingleOrDefault(card => card.IsPublished) is not null;",
            "public bool HasOnePublishedCard() => _store.HasOnePublishedCard();",
            "SingleOrDefault",
            expectObservable: false);

        Assert.DoesNotContain(knowledge.Rules, rule =>
            rule.Contains("Returns an item", StringComparison.Ordinal) &&
            rule.Contains("card.IsPublished", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Direct_first_return_remains_an_observable_selection()
    {
        var knowledge = await BuildKnowledgeAsync(
            "First Published Card",
            "public Card FirstPublishedCard() => _cards.First(card => card.IsPublished);",
            "public Card FirstPublishedCard() => _store.FirstPublishedCard();",
            "First");

        Assert.Contains(knowledge.Rules, rule =>
            rule.Contains("Returns an item from `_cards` selected where `card.IsPublished`.", StringComparison.Ordinal));
    }

    private static async Task<FeatureKnowledge> BuildKnowledgeAsync(
        string endpointName,
        string storeMethod,
        string controllerMethod,
        string operation,
        bool expectObservable = true)
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

            if (expectObservable)
            {
                Assert.Equal("observable", predicate.Metadata["businessRuleAuthority"]);
                Assert.Equal("return", predicate.Metadata["observableContext"]);
                Assert.Equal("semantic-return-value-path", predicate.Metadata["observableEffectResolution"]);
            }
            else
            {
                Assert.Equal("observed-only", predicate.Metadata["businessRuleAuthority"]);
                Assert.Equal("not-proven", predicate.Metadata["observableEffectResolution"]);
                Assert.False(predicate.Metadata.ContainsKey("observableContext"));
            }

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
