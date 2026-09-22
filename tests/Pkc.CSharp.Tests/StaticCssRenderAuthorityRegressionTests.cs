using Pkc.Core;
using Pkc.CSharp;
using Pkc.Frontend;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class StaticCssRenderAuthorityRegressionTests
{
    [Fact]
    public async Task Static_display_none_ancestor_is_not_an_authoritative_render_or_visibility()
    {
        var facts = await ScanFrontendAsync("""
            import { Component } from '@angular/core';

            @Component({
              selector: 'app-price',
              template: `
                <section style="display: none">
                  @if (displayPrice > 0) {
                    <strong>{{ displayPrice }}</strong>
                  }
                </section>
              `
            })
            export class PriceComponent {
              displayPrice = 42;
            }
            """);

        Assert.DoesNotContain(facts.Facts, fact =>
            fact.Kind == "ui-member-render" &&
            fact.Metadata.GetValueOrDefault("member") == "displayPrice" &&
            fact.Metadata.ContainsKey("renderAuthority"));
        Assert.DoesNotContain(facts.Facts, fact =>
            fact.Kind == "ui-member-visibility" &&
            fact.Metadata.GetValueOrDefault("member") == "displayPrice");
    }

    [Fact]
    public async Task Static_display_block_does_not_remove_render_authority()
    {
        var facts = await ScanFrontendAsync("""
            import { Component } from '@angular/core';

            @Component({
              selector: 'app-price',
              template: `
                <section style="display: block">
                  @if (displayPrice > 0) {
                    <strong>{{ displayPrice }}</strong>
                  }
                </section>
              `
            })
            export class PriceComponent {
              displayPrice = 42;
            }
            """);

        Assert.Contains(facts.Facts, fact =>
            fact.Kind == "ui-member-render" &&
            fact.Metadata.GetValueOrDefault("member") == "displayPrice" &&
            fact.Metadata.ContainsKey("renderAuthority"));
        Assert.Contains(facts.Facts, fact =>
            fact.Kind == "ui-member-visibility" &&
            fact.Metadata.GetValueOrDefault("member") == "displayPrice" &&
            fact.Metadata.GetValueOrDefault("condition") == "displayPrice > 0");
    }

    [Fact]
    public async Task Static_display_none_ancestor_blocks_r79_and_r710_authority()
    {
        var root = CreateRoot("end-to-end-display-none");
        try
        {
            await WriteFullFixtureAsync(root);

            var csharp = await new CSharpEvidenceScanner().ScanAsync(root);
            var frontend = await new FrontendScanner().ScanAsync(root);
            var facts = Merge(csharp, frontend);

            Assert.DoesNotContain(facts.Facts, fact =>
                fact.Kind == "ui-member-render" &&
                fact.Metadata.GetValueOrDefault("member") == "displayPrice" &&
                fact.Metadata.ContainsKey("renderAuthority"));
            Assert.DoesNotContain(facts.Facts, fact =>
                fact.Kind == "ui-member-visibility" &&
                fact.Metadata.GetValueOrDefault("member") == "displayPrice");

            var candidates = new CrossStackFeatureCandidateBuilder().Build(facts);
            candidates = new JointVisibilityCandidateEnricher().Enrich(candidates, facts);
            var candidate = Assert.Single(candidates.Candidates, candidate =>
                candidate.Facts.Any(fact =>
                    fact.Id == candidate.SeedFactId &&
                    fact.Kind == "endpoint" &&
                    fact.Name == "GetVisiblePrice"));

            Assert.DoesNotContain(candidate.Facts, fact =>
                fact.Kind == "value-terminal-source" &&
                fact.Metadata.GetValueOrDefault("boundary") == "rendered UI value");
            Assert.DoesNotContain(candidate.Facts, fact => fact.Kind == "joint-visibility");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static async Task<FactDocument> ScanFrontendAsync(string componentSource)
    {
        var root = CreateRoot("frontend-only");
        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "angular.json"), "{}");
            await File.WriteAllTextAsync(
                Path.Combine(root, "package.json"),
                "{\"dependencies\":{\"@angular/core\":\"22.0.0\"}}");
            await File.WriteAllTextAsync(Path.Combine(root, "price.component.ts"), componentSource);
            return await new FrontendScanner().ScanAsync(root);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static async Task WriteFullFixtureAsync(string root)
    {
        await File.WriteAllTextAsync(Path.Combine(root, "VisibilityApp.csproj"), ProjectSource);
        await File.WriteAllTextAsync(Path.Combine(root, "PricesController.cs"), BackendSource);
        await File.WriteAllTextAsync(Path.Combine(root, "angular.json"), "{}");
        await File.WriteAllTextAsync(
            Path.Combine(root, "package.json"),
            "{\"dependencies\":{\"@angular/core\":\"22.0.0\"}}");
        await File.WriteAllTextAsync(Path.Combine(root, "price-result.ts"), PriceResultSource);
        await File.WriteAllTextAsync(Path.Combine(root, "price-api.ts"), PriceApiSource);
        await File.WriteAllTextAsync(Path.Combine(root, "price.component.ts"), DisplayNonePriceComponentSource);
    }

    private static FactDocument Merge(params FactDocument[] documents)
    {
        var facts = documents
            .SelectMany(document => document.Facts)
            .GroupBy(fact => fact.Id, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(fact => fact.Id, StringComparer.Ordinal)
            .ToArray();
        var relations = documents
            .SelectMany(document => document.Relations)
            .GroupBy(
                relation => $"{relation.FromFactId}|{relation.Kind}|{relation.Target}|{relation.Source.Path}|{relation.Source.StartLine}",
                StringComparer.Ordinal)
            .Select(group => group.First())
            .ToArray();
        return new FactDocument("test", facts, relations);
    }

    private static string CreateRoot(string name)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pkc-r710-static-css-tests",
            name,
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private const string ProjectSource = """
        <Project Sdk="Microsoft.NET.Sdk.Web">
          <PropertyGroup>
            <TargetFramework>net8.0</TargetFramework>
            <ImplicitUsings>enable</ImplicitUsings>
            <Nullable>enable</Nullable>
          </PropertyGroup>
        </Project>
        """;

    private const string BackendSource = """
        using Microsoft.AspNetCore.Mvc;
        using System.Text.Json.Serialization;

        namespace Demo;

        public sealed class ProductEntity
        {
            public decimal Price { get; set; }
            public bool IsPublished { get; set; }
        }

        public sealed class PriceResponse
        {
            [JsonPropertyName("displayPrice")]
            public decimal DisplayPrice { get; set; }
        }

        [ApiController]
        [Route("api/prices")]
        public sealed class PricesController : ControllerBase
        {
            private readonly ProductEntity[] _products =
            [
                new ProductEntity { Price = 42m, IsPublished = true },
                new ProductEntity { Price = 5m, IsPublished = false }
            ];

            [HttpGet]
            public PriceResponse GetVisiblePrice()
            {
                var product = _products.Single(product => product.IsPublished);
                return new PriceResponse { DisplayPrice = product.Price };
            }
        }
        """;

    private const string PriceResultSource = """
        export interface PriceResult {
          displayPrice: number;
        }
        """;

    private const string PriceApiSource = """
        import { PriceResult } from './price-result';

        export class PriceApi {
          private readonly http: any;

          getPrice() {
            return this.http.get<PriceResult>('/api/prices');
          }
        }
        """;

    private const string DisplayNonePriceComponentSource = """
        import { Component, inject } from '@angular/core';
        import { PriceApi } from './price-api';

        @Component({
          selector: 'app-price',
          template: `
            <section style="display: none">
              @if (displayPrice > 0) {
                <strong>{{ displayPrice }}</strong>
              }
            </section>
          `
        })
        export class PriceComponent {
          private readonly api = inject(PriceApi);
          displayPrice = 0;

          reload() {
            this.api.getPrice().subscribe(result => this.displayPrice = result.displayPrice);
          }
        }
        """;
}
