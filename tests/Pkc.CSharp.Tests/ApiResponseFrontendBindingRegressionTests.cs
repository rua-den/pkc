using Pkc.Core;
using Pkc.CSharp;
using Pkc.Frontend;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class ApiResponseFrontendBindingRegressionTests
{
    [Fact]
    public async Task Explicit_wire_identity_composes_api_response_member_to_rendered_value()
    {
        var root = CreateRoot("explicit-wire-binding");
        try
        {
            await WriteFixtureAsync(root);
            var facts = await ScanAsync(root);
            var candidate = FindCandidate(facts, "GetPrice");

            var backendTerminal = Assert.Single(candidate.Facts, fact =>
                fact.Kind == "value-terminal-source" &&
                fact.Metadata.GetValueOrDefault("scopeName") == "GetPrice" &&
                fact.Metadata.GetValueOrDefault("boundary") == "API response field");
            Assert.Equal("displayPrice", backendTerminal.Metadata["wireName"]);
            Assert.Equal(
                "System.Text.Json.Serialization.JsonPropertyNameAttribute",
                backendTerminal.Metadata["wireContract"]);

            var binding = Assert.Single(candidate.Facts, fact =>
                fact.Kind == "value-terminal-source" &&
                fact.Metadata.GetValueOrDefault("boundary") == "rendered UI value");

            Assert.Equal("explicit-wire-api-ui-binding", binding.Metadata["sourceMechanism"]);
            Assert.Equal("displayPrice", binding.Metadata["wireName"]);
            Assert.Equal("price-api.ts#PriceApi", binding.Metadata["frontendServiceType"]);
            Assert.Equal("price-result.ts#PriceResult", binding.Metadata["frontendResponseType"]);
            Assert.Equal("displayPrice", binding.Metadata["frontendResultMember"]);
            Assert.Equal("displayPrice", binding.Metadata["frontendComponentMember"]);
            Assert.Contains("PriceResponse.DisplayPrice", binding.Metadata["returnedOccurrence"], StringComparison.Ordinal);
            Assert.Contains("result.displayPrice", binding.Metadata["returnedOccurrence"], StringComparison.Ordinal);
            Assert.Contains("PriceComponent.displayPrice", binding.Metadata["returnedOccurrence"], StringComparison.Ordinal);
            Assert.Contains("rendered displayPrice", binding.Metadata["returnedOccurrence"], StringComparison.Ordinal);
            Assert.DoesNotContain("result.DisplayPrice", binding.Metadata["returnedOccurrence"], StringComparison.Ordinal);
            Assert.StartsWith("PricesController.cs:L", binding.Metadata["backendResponseLocation"]);
            Assert.StartsWith("price-api.ts:L", binding.Metadata["frontendApiLocation"]);
            Assert.StartsWith("price-result.ts:L", binding.Metadata["frontendResultMemberLocation"]);
            Assert.StartsWith("price.component.ts:L", binding.Metadata["frontendBindingLocation"]);
            Assert.StartsWith("price.component.ts:L", binding.Metadata["frontendRenderLocation"]);

            var knowledge = await new EvidenceAwareKnowledgeSynthesizer().SynthesizeAsync(candidate);
            Assert.Contains(knowledge.ValueLineage, item =>
                item.Contains("Last proven source before rendered UI value", StringComparison.Ordinal) &&
                item.Contains("result.displayPrice", StringComparison.Ordinal) &&
                item.Contains("PriceComponent.displayPrice", StringComparison.Ordinal));

            var markdown = new MarkdownKnowledgeRenderer().Render(knowledge);
            Assert.Contains("## Value lineage", markdown, StringComparison.Ordinal);
            Assert.Contains("Last proven source before rendered UI value", markdown, StringComparison.Ordinal);
            Assert.Contains("result.displayPrice", markdown, StringComparison.Ordinal);
            Assert.Contains("PriceComponent.displayPrice", markdown, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Same_name_and_casing_collisions_do_not_create_property_authority()
    {
        var root = CreateRoot("wire-collision");
        try
        {
            await WriteFixtureAsync(root);
            var facts = await ScanAsync(root);
            var candidate = FindCandidate(facts, "GetPrice");

            var responseMembers = facts.Facts
                .Where(fact => fact.Kind == "ui-api-response-member")
                .Where(fact => fact.Metadata.GetValueOrDefault("serviceType") == "price-api.ts#PriceApi")
                .ToArray();
            Assert.Contains(responseMembers, fact => fact.Metadata["resultMember"] == "displayPrice");
            Assert.Contains(responseMembers, fact => fact.Metadata["resultMember"] == "DisplayPrice");

            var binding = Assert.Single(candidate.Facts, fact =>
                fact.Kind == "value-terminal-source" &&
                fact.Metadata.GetValueOrDefault("boundary") == "rendered UI value");
            Assert.Equal("displayPrice", binding.Metadata["frontendResultMember"]);
            Assert.DoesNotContain("result.DisplayPrice", binding.Metadata["returnedOccurrence"], StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Unresolved_service_or_result_receiver_does_not_create_member_binding()
    {
        var root = CreateRoot("receiver-negative");
        try
        {
            await WriteFixtureAsync(root);
            var frontend = await new FrontendScanner().ScanAsync(root);

            Assert.DoesNotContain(frontend.Facts, fact =>
                fact.Kind == "ui-result-member-binding" &&
                fact.Source.Path == "ambiguous-service.component.ts");
            Assert.DoesNotContain(frontend.Facts, fact =>
                fact.Kind == "ui-result-member-binding" &&
                fact.Source.Path == "ambiguous-result.component.ts");

            var facts = Merge(await new CSharpEvidenceScanner().ScanAsync(root), frontend);
            var candidate = FindCandidate(facts, "GetPrice");
            Assert.DoesNotContain(candidate.Facts, fact =>
                fact.Kind == "value-terminal-source" &&
                fact.Metadata.GetValueOrDefault("boundary") == "rendered UI value" &&
                (fact.Metadata.GetValueOrDefault("frontendComponentIdentity") == "ambiguous-service.component.ts#AmbiguousServiceComponent" ||
                 fact.Metadata.GetValueOrDefault("frontendComponentIdentity") == "ambiguous-result.component.ts#AmbiguousResultComponent"));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData("GetImplicitPrice")]
    [InlineData("GetFallbackPrice")]
    public async Task Missing_explicit_wire_or_typed_frontend_contract_fails_closed_and_keeps_C_lineage(
        string endpointName)
    {
        var root = CreateRoot(endpointName);
        try
        {
            await WriteFixtureAsync(root);
            var facts = await ScanAsync(root);
            var candidate = FindCandidate(facts, endpointName);

            Assert.Contains(candidate.Facts, fact =>
                fact.Kind == "value-transfer" &&
                fact.Metadata.GetValueOrDefault("scopeName") == endpointName &&
                fact.Metadata.GetValueOrDefault("mechanism") == "api-projection");
            Assert.Contains(candidate.Facts, fact =>
                fact.Kind == "value-terminal-source" &&
                fact.Metadata.GetValueOrDefault("scopeName") == endpointName &&
                fact.Metadata.GetValueOrDefault("boundary") == "API response field");
            Assert.DoesNotContain(candidate.Facts, fact =>
                fact.Kind == "value-terminal-source" &&
                fact.Metadata.GetValueOrDefault("boundary") == "rendered UI value");

            if (endpointName == "GetImplicitPrice")
            {
                var terminal = Assert.Single(candidate.Facts, fact =>
                    fact.Kind == "value-terminal-source" &&
                    fact.Metadata.GetValueOrDefault("scopeName") == endpointName &&
                    fact.Metadata.GetValueOrDefault("boundary") == "API response field");
                Assert.False(terminal.Metadata.ContainsKey("wireName"));
            }
            else
            {
                Assert.DoesNotContain(facts.Facts, fact =>
                    fact.Kind == "ui-api-response-member" &&
                    fact.Metadata.GetValueOrDefault("routeKey") == "/api/prices/fallback");
            }
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static async Task<FactDocument> ScanAsync(string root) =>
        Merge(
            await new CSharpEvidenceScanner().ScanAsync(root),
            await new FrontendScanner().ScanAsync(root));

    private static FeatureCandidate FindCandidate(FactDocument facts, string endpointName)
    {
        var candidates = new CrossStackFeatureCandidateBuilder().Build(facts);
        return Assert.Single(candidates.Candidates, candidate =>
            candidate.Facts.Any(fact =>
                fact.Id == candidate.SeedFactId &&
                fact.Kind == "endpoint" &&
                fact.Name == endpointName));
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
            "pkc-v047-d-tests",
            name,
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
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
        await File.WriteAllTextAsync(Path.Combine(root, "implicit-price-api.ts"), ImplicitPriceApiSource);
        await File.WriteAllTextAsync(Path.Combine(root, "implicit-price.component.ts"), ImplicitPriceComponentSource);
        await File.WriteAllTextAsync(Path.Combine(root, "fallback-price-api.ts"), FallbackPriceApiSource);
        await File.WriteAllTextAsync(Path.Combine(root, "fallback-price.component.ts"), FallbackPriceComponentSource);
        await File.WriteAllTextAsync(Path.Combine(root, "other-price-api.ts"), OtherPriceApiSource);
        await File.WriteAllTextAsync(Path.Combine(root, "ambiguous-service.component.ts"), AmbiguousServiceComponentSource);
        await File.WriteAllTextAsync(Path.Combine(root, "ambiguous-result.component.ts"), AmbiguousResultComponentSource);
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

        public sealed class ImplicitPriceResponse
        {
            public decimal DisplayPrice { get; set; }
        }

        public sealed class FallbackPriceResponse
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

            [HttpGet("implicit")]
            public ImplicitPriceResponse GetImplicitPrice()
            {
                var entity = new ProductEntity();
                entity.Price = 43m;
                return new ImplicitPriceResponse { DisplayPrice = entity.Price };
            }

            [HttpGet("fallback")]
            public FallbackPriceResponse GetFallbackPrice()
            {
                var entity = new ProductEntity();
                entity.Price = 44m;
                return new FallbackPriceResponse { DisplayPrice = entity.Price };
            }
        }
        """;

    private const string PriceResultSource = """
        export interface PriceResult {
          displayPrice: number;
          DisplayPrice: number;
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
          selector: 'app-price',
          template: `
            <strong>{{ displayPrice }}</strong>
            <small>{{ DisplayPrice }}</small>
          `
        })
        export class PriceComponent {
          private readonly api = inject(PriceApi);
          displayPrice = 0;
          DisplayPrice = 0;

          reload() {
            this.api.getPrice().subscribe(result => this.displayPrice = result.displayPrice);
          }
        }
        """;

    private const string ImplicitPriceApiSource = """
        import { PriceResult } from './price-result';

        export class ImplicitPriceApi {
          private readonly http: any;

          getPrice() {
            return this.http.get<PriceResult>('/api/prices/implicit');
          }
        }
        """;

    private const string ImplicitPriceComponentSource = """
        import { Component, inject } from '@angular/core';
        import { ImplicitPriceApi } from './implicit-price-api';

        @Component({
          template: `<strong>{{ displayPrice }}</strong>`
        })
        export class ImplicitPriceComponent {
          private readonly api = inject(ImplicitPriceApi);
          displayPrice = 0;

          reload() {
            this.api.getPrice().subscribe(result => this.displayPrice = result.displayPrice);
          }
        }
        """;

    private const string FallbackPriceApiSource = """
        export class FallbackPriceApi {
          private readonly http: any;

          getPrice() {
            return this.http.get('/api/prices/fallback');
          }
        }
        """;

    private const string FallbackPriceComponentSource = """
        import { Component, inject } from '@angular/core';
        import { FallbackPriceApi } from './fallback-price-api';

        @Component({
          template: `<strong>{{ displayPrice }}</strong>`
        })
        export class FallbackPriceComponent {
          private readonly api = inject(FallbackPriceApi);
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

    private const string AmbiguousServiceComponentSource = """
        import { Component, inject } from '@angular/core';
        import { PriceApi } from './price-api';
        import { OtherPriceApi } from './other-price-api';

        @Component({
          template: `<strong>{{ ambiguousPrice }}</strong>`
        })
        export class AmbiguousServiceComponent {
          private readonly primary = inject(PriceApi);
          private readonly secondary = inject(OtherPriceApi);
          private readonly api = Math.random() > 0.5 ? this.primary : this.secondary;
          ambiguousPrice = 0;

          reload() {
            this.api.getPrice().subscribe(result => this.ambiguousPrice = result.displayPrice);
          }
        }
        """;

    private const string AmbiguousResultComponentSource = """
        import { Component, inject } from '@angular/core';
        import { PriceApi } from './price-api';
        import { PriceResult } from './price-result';

        @Component({
          template: `<strong>{{ ambiguousPrice }}</strong>`
        })
        export class AmbiguousResultComponent {
          private readonly api = inject(PriceApi);
          private readonly other: PriceResult = { displayPrice: 1, DisplayPrice: 2 };
          ambiguousPrice = 0;

          reload() {
            this.api.getPrice().subscribe(result => this.ambiguousPrice = this.other.displayPrice);
          }
        }
        """;
}
