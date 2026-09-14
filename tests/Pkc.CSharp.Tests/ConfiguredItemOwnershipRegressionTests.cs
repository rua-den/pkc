using Pkc.CSharp;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class ConfiguredItemOwnershipRegressionTests
{
    [Fact]
    public async Task Nested_object_initializer_is_not_an_additional_item_of_source_collection()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pkc-configured-item-ownership-tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "Demo.csproj"), Project);
            await File.WriteAllTextAsync(Path.Combine(root, "Catalog.cs"), Source);

            var facts = await new CSharpEvidenceScanner().ScanAsync(root);

            var configured = facts.Facts
                .Where(fact =>
                    fact.Kind == "configured-object" &&
                    fact.Metadata.TryGetValue("source", out var source) &&
                    source == "_cards")
                .ToArray();

            var card = Assert.Single(configured);
            Assert.Contains("Name = \"A\"", card.Metadata["assignments"], StringComparison.Ordinal);
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
        using System.Collections.Generic;
        using System.Linq;

        namespace Demo;

        public sealed class CardMetadata
        {
            public string Name { get; init; } = "";
        }

        public sealed class Card
        {
            public string Name { get; init; } = "";
            public bool IsPublished { get; init; }
            public CardMetadata Metadata { get; init; } = new();
        }

        public sealed class Store
        {
            private readonly List<Card> _cards =
            [
                new Card
                {
                    Name = "A",
                    IsPublished = true,
                    Metadata = new CardMetadata
                    {
                        Name = "Internal metadata"
                    }
                }
            ];

            public IReadOnlyList<Card> GetCards()
                => _cards.Where(card => card.IsPublished).ToArray();
        }
        """;
}
