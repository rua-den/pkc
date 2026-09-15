using Pkc.CSharp;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class ProjectionConstructionAuthorityRegressionTests
{
    [Fact]
    public async Task Source_mutating_copy_constructor_downgrades_same_type_projection_authority()
    {
        var knowledge = await BuildKnowledgeAsync(
            """
            public IReadOnlyList<Card> GetCards() =>
                _cards
                    .Where(card => card.IsPublished)
                    .Select(CloneCard)
                    .ToArray();

            private static Card CloneCard(Card card) => new Card(card)
            {
                IsPublished = card.IsPublished,
                Blocked = card.Blocked
            };
            """,
            """
            public sealed class Card
            {
                public bool IsPublished { get; set; }
                public bool Blocked { get; set; }

                public Card() { }

                public Card(Card source)
                {
                    source.IsPublished = false;
                }
            }
            """);

        Assert.DoesNotContain(knowledge.Rules, rule =>
            rule.Contains("Includes items from `_cards` only when", StringComparison.Ordinal) &&
            rule.Contains("card.IsPublished", StringComparison.Ordinal));
    }

    [Fact]
    public async Task User_defined_parameterless_constructor_downgrades_same_type_projection_authority()
    {
        var knowledge = await BuildKnowledgeAsync(
            """
            public IReadOnlyList<Card> GetCards()
            {
                Card.Current = _cards[0];
                return _cards
                    .Where(card => card.IsPublished)
                    .Select(CloneCard)
                    .ToArray();
            }

            private static Card CloneCard(Card card) => new()
            {
                IsPublished = card.IsPublished,
                Blocked = card.Blocked
            };
            """,
            """
            public sealed class Card
            {
                public static Card? Current { get; set; }
                public bool IsPublished { get; set; }
                public bool Blocked { get; set; }

                public Card()
                {
                    if (Current is not null)
                    {
                        Current.IsPublished = false;
                    }
                }
            }
            """);

        Assert.DoesNotContain(knowledge.Rules, rule =>
            rule.Contains("Includes items from `_cards` only when", StringComparison.Ordinal) &&
            rule.Contains("card.IsPublished", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Implicit_constructor_with_instance_initializer_downgrades_same_type_projection_authority()
    {
        var knowledge = await BuildKnowledgeAsync(
            """
            public IReadOnlyList<Card> GetCards()
            {
                Card.Current = _cards[0];
                return _cards
                    .Where(card => card.IsPublished)
                    .Select(CloneCard)
                    .ToArray();
            }

            private static Card CloneCard(Card card) => new()
            {
                IsPublished = card.IsPublished,
                Blocked = card.Blocked
            };
            """,
            """
            public sealed class Card
            {
                public static Card? Current { get; set; }
                private readonly bool _constructionSideEffect = MutateCurrent();

                public bool IsPublished { get; set; }
                public bool Blocked { get; set; }

                private static bool MutateCurrent()
                {
                    if (Current is not null)
                    {
                        Current.IsPublished = false;
                    }

                    return false;
                }
            }
            """);

        Assert.DoesNotContain(knowledge.Rules, rule =>
            rule.Contains("Includes items from `_cards` only when", StringComparison.Ordinal) &&
            rule.Contains("card.IsPublished", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Implicit_constructor_with_effectful_base_constructor_downgrades_same_type_projection_authority()
    {
        var knowledge = await BuildKnowledgeAsync(
            """
            public IReadOnlyList<Card> GetCards()
            {
                Card.Current = _cards[0];
                return _cards
                    .Where(card => card.IsPublished)
                    .Select(CloneCard)
                    .ToArray();
            }

            private static Card CloneCard(Card card) => new()
            {
                IsPublished = card.IsPublished,
                Blocked = card.Blocked
            };
            """,
            """
            public class CardBase
            {
                public CardBase()
                {
                    if (Card.Current is not null)
                    {
                        Card.Current.IsPublished = false;
                    }
                }
            }

            public sealed class Card : CardBase
            {
                public static Card? Current { get; set; }
                public bool IsPublished { get; set; }
                public bool Blocked { get; set; }
            }
            """);

        Assert.DoesNotContain(knowledge.Rules, rule =>
            rule.Contains("Includes items from `_cards` only when", StringComparison.Ordinal) &&
            rule.Contains("card.IsPublished", StringComparison.Ordinal));
    }

    private static async Task<FeatureKnowledge> BuildKnowledgeAsync(string methodBody, string cardDeclaration)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pkc-projection-construction-authority-tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "Demo.csproj"), Project);
            await File.WriteAllTextAsync(Path.Combine(root, "Catalog.cs"), Source(methodBody, cardDeclaration));

            var facts = await new CSharpEvidenceScanner().ScanAsync(root);
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
                new Card { IsPublished = true, Blocked = false }
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
