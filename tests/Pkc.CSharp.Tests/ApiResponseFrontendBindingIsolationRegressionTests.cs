using Pkc.Core;
using Pkc.CSharp;
using Pkc.Frontend;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class ApiResponseFrontendBindingIsolationRegressionTests
{
    [Fact]
    public async Task Explicit_wire_location_is_retained_and_unrelated_resolved_flow_stays_disconnected()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pkc-v047-d-tests",
            "resolved-unrelated-flow",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await WriteFixtureAsync(root);

            var backend = await new CSharpEvidenceScanner().ScanAsync(root);
            var frontend = await new FrontendScanner().ScanAsync(root);
            var facts = Merge(backend, frontend);

            Assert.Contains(frontend.Facts, fact =>
                fact.Kind == "ui-api-response-member" &&
                fact.Metadata.GetValueOrDefault("serviceType") == "other-price-api.ts#OtherPriceApi" &&
                fact.Metadata.GetValueOrDefault("resultMember") == "displayPrice");
            Assert.Contains(frontend.Facts, fact =>
                fact.Kind == "ui-result-member-binding" &&
                fact.Metadata.GetValueOrDefault("componentIdentity") == "other-price.component.ts#OtherPriceComponent" &&
                fact.Metadata.GetValueOrDefault("resultMember") == "displayPrice");
            Assert.Contains(frontend.Facts, fact =>
                fact.Kind == "ui-member-render" &&
                fact.Metadata.GetValueOrDefault("componentIdentity") == "other-price.component.ts#OtherPriceComponent" &&
                fact.Metadata.GetValueOrDefault("member") == "displayPrice");

            var candidate = Assert.Single(
                new CrossStackFeatureCandidateBuilder().Build(facts).Candidates,
                item => item.Facts.Any(fact =>
                    fact.Id == item.SeedFactId &&
                    fact.Kind == "endpoint" &&
                    fact.Name == "GetPrice"));

            var backendTerminal = Assert.Single(candidate.Facts, fact =>
                fact.Kind == "value-terminal-source" &&
                fact.Metadata.GetValueOrDefault("scopeName") == "GetPrice" &&
                fact.Metadata.GetValueOrDefault("boundary") == "API response field");
            Assert.Equal("displayPrice", backendTerminal.Metadata["wireName"]);
            Assert.StartsWith("PricesController.cs:L", backendTerminal.Metadata["wireContractLocation"]);

            var rendered = Assert.Single(candidate.Facts, fact =>
                fact.Kind == "value-terminal-source" &&
                fact.Metadata.GetValueOrDefault("boundary") == "rendered UI value");
            Assert.Equal("price.component.ts#PriceComponent", rendered.Metadata["frontendComponentIdentity"]);
            Assert.Equal("price-api.ts#PriceApi", rendered.Metadata["frontendServiceType"]);
            Assert.DoesNotContain(candidate.Facts, fact =>
                fact.Kind == "value-terminal-source" &&
                fact.Metadata.GetValueOrDefault("boundary") == "rendered UI value" &&
                fact.Metadata.GetValueOrDefault("frontendComponentIdentity") == "other-price.component.ts#OtherPriceComponent");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
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

    private static async Task WriteFixtureAsync(string root)
    {
        await File.WriteAllTextAsync(Path.Combine(root, "BindingApp.csproj"), ProjectSource);
        await File.WriteAllTextAsync(Path.Combine(root, "PricesController.cs"), BackendSource);
        await File.WriteAllTextAsync(Path.Combine(root, "angular.json"), "{}");
        await File.WriteAllTextAsync(
            Path.Combine(root, "package.json"),
            "{\"dependencies\":{\"@angular/core\":\"22.0.0\"}}");
        await File.WriteAllTextAsync(Path.Combine(root, "price-result.ts"), PriceResultSource);
        await File.WriteAllTextAsync(Path.Combine(root, "price-api.ts"), PriceApiSource);
        await File.WriteAllTextAsync(Path.Combine(root, "price.component.ts"), PriceComponentSource);
        await File.WriteAllTextAsync(Path.Combine(root, "other-price-api.ts"), OtherPriceApiSource);
        await File.WriteAllTextAsync(Path.Combine(root, "other-price.component.ts"), OtherPriceComponentSource);
    }

    private const string ProjectSource = """
        <Project Sdk="Microsoft.NET.Sdk.Web">
          <PropertyGroup>
            <TargetFramework>net8.0</TargetFramework>
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
            [HttpGet]
            public PriceResponse GetPrice()
            {
                var entity = new ProductEntity();
                entity.Price = 42m;
                return new PriceResponse { DisplayPrice = entity.Price };
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

    private const string PriceComponentSource = """
        import { Component, inject } from '@angular/core';
        import { PriceApi } from './price-api';

        @Component({
          template: `<strong>{{ displayPrice }}</strong>`
        })
        export class PriceComponent {
          private readonly api = inject(PriceApi);
          displayPrice = 0;

          reload() {
            this.api.getPrice().subscribe(result => this.displayPrice = result.displayPrice);
          }
        }
        """;

    private const string OtherPriceApiSource = """
        import { PriceResult } from './price-result';

        export class OtherPriceApi {
          private readonly http: any;

          getPrice() {
            return this.http.get<PriceResult>('/api/other-prices');
          }
        }
        """;

    private const string OtherPriceComponentSource = """
        import { Component, inject } from '@angular/core';
        import { OtherPriceApi } from './other-price-api';

        @Component({
          template: `<strong>{{ displayPrice }}</strong>`
        })
        export class OtherPriceComponent {
          private readonly api = inject(OtherPriceApi);
          displayPrice = 0;

          reload() {
            this.api.getPrice().subscribe(result => this.displayPrice = result.displayPrice);
          }
        }
        """;
}
