using Pkc.CSharp;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class BusinessPredicateSemanticAuthorityRegressionTests
{
    [Fact]
    public async Task Custom_method_named_Where_does_not_become_authoritative_business_predicate()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pkc-business-predicate-authority-tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "Demo.csproj"), Project);
            await File.WriteAllTextAsync(Path.Combine(root, "Catalog.cs"), Source);

            var facts = await new CSharpEvidenceScanner().ScanAsync(root);

            Assert.DoesNotContain(facts.Facts, fact =>
                fact.Kind == "business-predicate" &&
                fact.Metadata.TryGetValue("operation", out var operation) &&
                operation == "Where" &&
                fact.Metadata.TryGetValue("source", out var source) &&
                source == "_bucket");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private const string Project = """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>net10.0</TargetFramework>
            <ImplicitUsings>enable</ImplicitUsings>
            <Nullable>enable</Nullable>
          </PropertyGroup>
        </Project>
        """;

    private const string Source = """
        using System;
        using System.Collections.Generic;

        namespace Demo;

        public sealed class Card
        {
            public bool IsPublished { get; init; }
        }

        public sealed class CustomBucket
        {
            private readonly IReadOnlyList<Card> _allCards = [new Card { IsPublished = false }];

            public IReadOnlyList<Card> Where(Func<Card, bool> ignored) => _allCards;
        }

        public sealed class Store
        {
            private readonly CustomBucket _bucket = new();

            public IReadOnlyList<Card> GetCards()
                => _bucket.Where(card => card.IsPublished);
        }
        """;
}
