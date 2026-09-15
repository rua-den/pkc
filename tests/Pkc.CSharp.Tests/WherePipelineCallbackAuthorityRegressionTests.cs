using Pkc.CSharp;
using Pkc.Core;
using Pkc.Knowledge;
using System.Linq.Expressions;
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
    public async Task Callback_free_enumerable_pipeline_remains_authoritative()
    {
        var knowledge = await BuildKnowledgeAsync(
            """
            public IReadOnlyList<Card> GetCards() =>
                _cards
                    .Where(card => card.IsPublished)
                    .Skip(0)
                    .Take(2)
                    .Reverse()
                    .ToList();
            """,
            MutableCardDeclaration);

        Assert.Contains(knowledge.Rules, rule =>
            rule.Contains("Includes items from `_cards` only when", StringComparison.Ordinal) &&
            rule.Contains("card.IsPublished", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Enumerable_where_crossing_a_queryable_hop_is_observed_only()
    {
        var knowledge = await BuildKnowledgeAsync(
            """
            public IReadOnlyList<Card> GetCards() =>
                _cards
                    .Where(card => card.IsPublished)
                    .AsQueryable()
                    .Take(2)
                    .ToList();
            """,
            MutableCardDeclaration,
            facts =>
            {
                var predicate = Assert.Single(facts.Facts, fact => fact.Kind == "business-predicate");
                Assert.Equal("observed-only", predicate.Metadata["businessRuleAuthority"]);
                Assert.DoesNotContain("observableContext", predicate.Metadata.Keys);
            });

        Assert.DoesNotContain(knowledge.Rules, rule =>
            rule.Contains("Includes items from `_cards` only when", StringComparison.Ordinal) &&
            rule.Contains("card.IsPublished", StringComparison.Ordinal));

        Assert.Contains(knowledge.Evidence, evidence =>
            evidence.Kind == "business-predicate" &&
            evidence.Description.Contains("card.IsPublished", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Provider_that_retains_expression_but_ignores_where_is_observed_only()
    {
        var knowledge = await BuildKnowledgeAsync(
            """
            private readonly IQueryable<Card> _query;

            public Store()
            {
                _query = new IgnoringQuery<Card>(new[]
                {
                    new Card { IsPublished = false, Blocked = false }
                });
            }

            public IReadOnlyList<Card> GetCards() => _query.Where(card => card.IsPublished).ToList();

            private sealed class IgnoringQuery<T> : IQueryable<T>, IQueryProvider
            {
                private readonly IEnumerable<T> _items;

                public IgnoringQuery(IEnumerable<T> items)
                    : this(items, Expression.Constant(items.AsQueryable())) { }

                private IgnoringQuery(IEnumerable<T> items, Expression expression)
                {
                    _items = items;
                    Expression = expression;
                }

                public Type ElementType => typeof(T);
                public Expression Expression { get; }
                public IQueryProvider Provider => this;

                public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
                System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

                public IQueryable CreateQuery(Expression expression) => new IgnoringQuery<T>(_items, expression);

                public IQueryable<TElement> CreateQuery<TElement>(Expression expression) =>
                    (IQueryable<TElement>)(object)new IgnoringQuery<T>(_items, expression);

                public object? Execute(Expression expression) => _items;

                public TResult Execute<TResult>(Expression expression) =>
                    (TResult)(object)_items;
            }
            """,
            MutableCardDeclaration,
            facts =>
            {
                var predicate = Assert.Single(facts.Facts, fact => fact.Kind == "business-predicate");
                Assert.Equal("observed-only", predicate.Metadata["businessRuleAuthority"]);
                Assert.DoesNotContain("observableContext", predicate.Metadata.Keys);
            });

        Assert.DoesNotContain(knowledge.Rules, rule =>
            rule.Contains("Includes items from `_query` only when", StringComparison.Ordinal) &&
            rule.Contains("card.IsPublished", StringComparison.Ordinal));

        Assert.Contains(knowledge.Evidence, evidence =>
            evidence.Kind == "business-predicate" &&
            evidence.Description.Contains("card.IsPublished", StringComparison.Ordinal));
    }

    [Fact]
    public void Ignoring_provider_returns_unpublished_item_when_get_cards_executes()
    {
        var store = new RuntimeStore();

        var returned = store.GetCards();

        var card = Assert.Single(returned);
        Assert.False(card.IsPublished);
    }

    private static async Task<FeatureKnowledge> BuildKnowledgeAsync(
        string methodBody,
        string cardDeclaration,
        Action<FactDocument>? inspectFacts = null)
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

            inspectFacts?.Invoke(facts);

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
        using System.Linq.Expressions;
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

    private sealed class RuntimeStore
    {
        private readonly IQueryable<RuntimeCard> _query =
            new RuntimeIgnoringQuery<RuntimeCard>(new[] { new RuntimeCard(false) });

        public IReadOnlyList<RuntimeCard> GetCards() =>
            _query.Where(card => card.IsPublished).ToList();
    }

    private sealed record RuntimeCard(bool IsPublished);

    private sealed class RuntimeIgnoringQuery<T> : IQueryable<T>, IQueryProvider
    {
        private readonly IEnumerable<T> _items;

        public RuntimeIgnoringQuery(IEnumerable<T> items)
            : this(items, Expression.Constant(items.AsQueryable())) { }

        private RuntimeIgnoringQuery(IEnumerable<T> items, Expression expression)
        {
            _items = items;
            Expression = expression;
        }

        public Type ElementType => typeof(T);
        public Expression Expression { get; }
        public IQueryProvider Provider => this;

        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        public IQueryable CreateQuery(Expression expression) => new RuntimeIgnoringQuery<T>(_items, expression);

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) =>
            (IQueryable<TElement>)(object)new RuntimeIgnoringQuery<T>(_items, expression);

        public object? Execute(Expression expression) => _items;

        public TResult Execute<TResult>(Expression expression) => (TResult)(object)_items;
    }
}
