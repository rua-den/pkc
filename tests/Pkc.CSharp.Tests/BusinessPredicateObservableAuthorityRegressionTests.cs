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

    private static async Task<FeatureKnowledge> BuildKnowledgeAsync(string methodBody)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pkc-business-predicate-observable-authority-tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "Demo.csproj"), Project);
            await File.WriteAllTextAsync(Path.Combine(root, "Catalog.cs"), Source(methodBody));

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

    private static string Source(string methodBody) => $$"""
        using System;
        using System.Collections.Generic;
        using System.Linq;

        namespace Demo;

        public sealed record Card(bool IsPublished, bool Blocked);

        public sealed class Store
        {
            private readonly List<Card> _cards = [new(true, false), new(false, true)];

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
