using Pkc.CSharp;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class WherePipelineCallbackAuthorityRegressionTests
{
    [Fact]
    public async Task Side_effecting_ordering_callback_downgrades_returned_filter_authority()
    {
        var knowledge = await BuildKnowledgeAsync(
            """
            public IReadOnlyList<Card> GetCards() =>
                _cards
                    .Where(card => card.IsPublished)
                    .OrderBy(card => card.IsPublished = false)
                    .ToArray();
            """,
            MutableCardDeclaration);

        Assert.DoesNotContain(knowledge.Rules, rule =>
            rule.Contains("Includes items from `_cards` only when", StringComparison.Ordinal) &&
            rule.Contains("card.IsPublished", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Equality_comparer_pipeline_downgrades_returned_filter_authority()
    {
        var knowledge = await BuildKnowledgeAsync(
            """
            public IReadOnlyList<Card> GetCards() =>
                _cards
                    .Where(card => card.IsPublished)
                    .Distinct(new MutatingComparer())
                    .ToArray();

            private sealed class MutatingComparer : IEqualityComparer<Card>
            {
                public bool Equals(Card? x, Card? y) => ReferenceEquals(x, y);

                public int GetHashCode(Card card)
                {
                    card.IsPublished = false;
                    return 0;
                }
            }
            """,
            MutableCardDeclaration);

        Assert.DoesNotContain(knowledge.Rules, rule =>
            rule.Contains("Includes items from `_cards` only when", StringComparison.Ordinal) &&
            rule.Contains("card.IsPublished", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Default_item_equality_pipeline_downgrades_returned_filter_authority()
    {
        var knowledge = await BuildKnowledgeAsync(
            """
            public IReadOnlyList<Card> GetCards() =>
                _cards
                    .Where(card => card.IsPublished)
                    .ToHashSet()
                    .ToArray();
            """,
            MutatingEqualityCardDeclaration);

        Assert.DoesNotContain(knowledge.Rules, rule =>
            rule.Contains("Includes items from `_cards` only when", StringComparison.Ordinal) &&
            rule.Contains("card.IsPublished", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Callback_free_enumerable_and_queryable_pipeline_remains_authoritative()
    {
        var knowledge = await BuildKnowledgeAsync(
            """
            public IReadOnlyList<Card> GetCards() =>
                _cards
                    .Where(card => card.IsPublished)
                    .AsQueryable()
                    .Skip(0)
                    .Take(2)
                    .Reverse()
                    .AsEnumerable()
                    .ToList();
            """,
            MutableCardDeclaration);

        Assert.Contains(knowledge.Rules, rule =>
            rule.Contains("Includes items from `_cards` only when", StringComparison.Ordinal) &&
            rule.Contains("card.IsPublished", StringComparison.Ordinal));
    }

    private static async Task<FeatureKnowledge> BuildKnowledgeAsync(
        string methodBody,
        string cardDeclaration)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pkc-where-pipeline-callback-authority-tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "Demo.csproj"), Project);
            await File.WriteAllTextAsync(Path.Combine(root, "Catalog.cs"), Source(methodBody, cardDeclaration));

            var facts = await new CSharpEvidenceScanner().ScanAsync(root);

            Assert.Contains(facts.Facts, fact =>
                fact.Kind == "business-predicate" &&
                fact.Metadata.TryGetValue("operation", out var operation) &&
                operation == "Where" &&
                fact.Metadata.TryGetValue("expression", out var expression) &&
                expression.Contains("card.IsPublished", StringComparison.Ordinal));

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

    private static string Source(string methodBody, string cardDeclaration) => $$"""
        using System;
        using System.Collections.Generic;
        using System.Linq;

        namespace Demo;

        {{cardDeclaration}}

        public sealed class Store
        {
            private readonly List<Card> _cards =
            [
                new() { IsPublished = true, Blocked = false }
            ];

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

    private const string MutableCardDeclaration = """
        public sealed class Card
        {
            public bool IsPublished { get; set; }
            public bool Blocked { get; set; }
        }
        """;

    private const string MutatingEqualityCardDeclaration = """
        public sealed class Card
        {
            public bool IsPublished { get; set; }
            public bool Blocked { get; set; }

            public override int GetHashCode()
            {
                IsPublished = false;
                return 0;
            }

            public override bool Equals(object? other) => ReferenceEquals(this, other);
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
