using Pkc.Core;
using Pkc.CSharp;
using Pkc.Frontend;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class JointVisibilityRegressionTests
{
    [Fact]
    public async Task Exact_selected_backend_item_and_exact_render_visibility_compose()
    {
        var root = CreateRoot("positive");
        try
        {
            await WriteFixtureAsync(root);
            var facts = await ScanAsync(root);
            var candidate = FindCandidate(facts);

            var projection = Assert.Single(candidate.Facts, fact =>
                fact.Kind == "value-transfer" &&
                fact.Metadata.GetValueOrDefault("mechanism") == "api-projection" &&
                fact.Metadata.ContainsKey("selectionInvocationSpanStart"));
            var predicate = Assert.Single(candidate.Facts, fact =>
                fact.Id == projection.Metadata["selectionPredicateFactId"]);
            Assert.Equal("observable", predicate.Metadata["businessRuleAuthority"]);
            Assert.Equal("selected-api-response-item", predicate.Metadata["observableContext"]);
            Assert.Equal(projection.Id, predicate.Metadata["selectedApiProjectionFactId"]);

            var rendered = Assert.Single(candidate.Facts, fact =>
                fact.Kind == "value-terminal-source" &&
                fact.Metadata.GetValueOrDefault("boundary") == "rendered UI value");
            var visibility = Assert.Single(candidate.Facts, fact =>
                fact.Kind == "ui-member-visibility" &&
                fact.Metadata.GetValueOrDefault("renderFactId") ==
                rendered.Metadata.GetValueOrDefault("frontendRenderFactId"));
            Assert.Equal("displayPrice > 0", visibility.Metadata["condition"]);

            var joint = Assert.Single(candidate.Facts, fact => fact.Kind == "joint-visibility");
            Assert.Equal(predicate.Id, joint.Metadata["backendPredicateFactId"]);
            Assert.Equal("product.IsPublished", joint.Metadata["backendCondition"]);
            Assert.Equal("displayPrice > 0", joint.Metadata["frontendCondition"]);
            Assert.Equal("observable", joint.Metadata["jointVisibilityAuthority"]);
            Assert.Equal(rendered.Id, joint.Metadata["r79TerminalFactId"]);

            var knowledge = await new JointVisibilityKnowledgeSynthesizer().SynthesizeAsync(candidate);
            Assert.Contains(knowledge.Rules, rule =>
                rule.Contains("Combined observable visibility", StringComparison.Ordinal) &&
                rule.Contains("product.IsPublished", StringComparison.Ordinal) &&
                rule.Contains("displayPrice > 0", StringComparison.Ordinal));

            var markdown = new MarkdownKnowledgeRenderer().Render(knowledge);
            Assert.Contains("Combined observable visibility", markdown, StringComparison.Ordinal);
            Assert.Contains("product.IsPublished", markdown, StringComparison.Ordinal);
            Assert.Contains("displayPrice > 0", markdown, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Static_hidden_ancestor_blocks_r79_and_r710_authority()
    {
        var root = CreateRoot("static-hidden-ancestor");
        try
        {
            await WriteFixtureAsync(root);
            await File.WriteAllTextAsync(
                Path.Combine(root, "price.component.ts"),
                HiddenPriceComponentSource);

            var facts = await ScanAsync(root);
            Assert.DoesNotContain(facts.Facts, fact =>
                fact.Kind == "ui-member-render" &&
                fact.Metadata.GetValueOrDefault("member") == "displayPrice" &&
                fact.Metadata.ContainsKey("renderAuthority"));
            Assert.DoesNotContain(facts.Facts, fact =>
                fact.Kind == "ui-member-visibility" &&
                fact.Metadata.GetValueOrDefault("member") == "displayPrice");

            var candidate = FindCandidate(facts);
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

    [Fact]
    public async Task Unrelated_same_text_predicate_cannot_replace_exact_selected_predicate()
    {
        var root = CreateRoot("unrelated-predicate");
        try
        {
            await WriteFixtureAsync(root);
            var facts = await ScanAsync(root);
            var projection = Assert.Single(facts.Facts, fact =>
                fact.Kind == "value-transfer" &&
                fact.Metadata.ContainsKey("selectionPredicateFactId"));
            var originalPredicateId = projection.Metadata["selectionPredicateFactId"];
            var originalPredicate = Assert.Single(facts.Facts, fact => fact.Id == originalPredicateId);

            var unrelated = originalPredicate with
            {
                Id = originalPredicate.Id + ":unrelated",
                Metadata = new Dictionary<string, string>(originalPredicate.Metadata, StringComparer.Ordinal)
                {
                    ["selectedApiProjectionFactId"] = "different-projection"
                }
            };
            facts = facts with
            {
                Facts = facts.Facts.Append(unrelated).ToArray(),
                Relations = facts.Relations.Append(new EvidenceRelation(
                    projection.Metadata["scopeFactId"],
                    "contains-condition",
                    unrelated.Id,
                    unrelated.Source)).ToArray()
            };

            var candidate = FindCandidate(facts);
            var joint = Assert.Single(candidate.Facts, fact => fact.Kind == "joint-visibility");
            Assert.Equal(originalPredicateId, joint.Metadata["backendPredicateFactId"]);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Ambiguous_frontend_visibility_fails_closed_and_keeps_r79()
    {
        var root = CreateRoot("ambiguous-visibility");
        try
        {
            await WriteFixtureAsync(root);
            var facts = await ScanAsync(root);
            var visibility = Assert.Single(facts.Facts, fact => fact.Kind == "ui-member-visibility");
            var duplicate = visibility with
            {
                Id = visibility.Id + ":duplicate",
                Metadata = new Dictionary<string, string>(visibility.Metadata, StringComparer.Ordinal)
                {
                    ["condition"] = "isAllowed"
                }
            };
            facts = facts with { Facts = facts.Facts.Append(duplicate).ToArray() };

            var candidate = FindCandidate(facts);
            Assert.Contains(candidate.Facts, fact =>
                fact.Kind == "value-terminal-source" &&
                fact.Metadata.GetValueOrDefault("boundary") == "rendered UI value");
            Assert.Equal(2, candidate.Facts.Count(fact => fact.Kind == "ui-member-visibility"));
            Assert.DoesNotContain(candidate.Facts, fact => fact.Kind == "joint-visibility");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Nested_frontend_visibility_fails_closed_and_keeps_r79()
    {
        var root = CreateRoot("nested-visibility");
        try
        {
            await WriteFixtureAsync(root);
            await File.WriteAllTextAsync(
                Path.Combine(root, "price.component.ts"),
                NestedPriceComponentSource);

            var facts = await ScanAsync(root);
            Assert.DoesNotContain(facts.Facts, fact => fact.Kind == "ui-member-visibility");

            var candidate = FindCandidate(facts);
            Assert.Contains(candidate.Facts, fact =>
                fact.Kind == "value-terminal-source" &&
                fact.Metadata.GetValueOrDefault("boundary") == "rendered UI value");
            Assert.DoesNotContain(candidate.Facts, fact => fact.Kind == "joint-visibility");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Observed_only_backend_condition_is_not_upgraded_by_frontend_visibility()
    {
        var root = CreateRoot("authority-downgrade");
        try
        {
            await WriteFixtureAsync(root);
            var facts = await ScanAsync(root);
            var projection = Assert.Single(facts.Facts, fact =>
                fact.Kind == "value-transfer" &&
                fact.Metadata.ContainsKey("selectionPredicateFactId"));
            var predicateId = projection.Metadata["selectionPredicateFactId"];

            facts = facts with
            {
                Facts = facts.Facts.Select(fact =>
                {
                    if (fact.Id != predicateId)
                    {
                        return fact;
                    }

                    var metadata = new Dictionary<string, string>(fact.Metadata, StringComparer.Ordinal)
                    {
                        ["businessRuleAuthority"] = "observed-only",
                        ["observableEffectResolution"] = "not-proven"
                    };
                    metadata.Remove("observableContext");
                    return fact with { Metadata = metadata };
                }).ToArray(),
                Relations = facts.Relations.Select(relation =>
                    relation.Target == predicateId && relation.Kind == "contains-condition"
                        ? relation with { Kind = "observes-predicate" }
                        : relation).ToArray()
            };

            var candidate = FindCandidate(facts);
            var joint = Assert.Single(candidate.Facts, fact => fact.Kind == "joint-visibility");
            Assert.Equal("observed-only", joint.Metadata["jointVisibilityAuthority"]);

            var knowledge = await new JointVisibilityKnowledgeSynthesizer().SynthesizeAsync(candidate);
            Assert.DoesNotContain(knowledge.Rules, rule =>
                rule.Contains("Combined observable visibility", StringComparison.Ordinal));
            Assert.Contains(knowledge.Unknowns, unknown =>
                unknown.Contains("not promoted to an authoritative combined rule", StringComparison.Ordinal));
            Assert.Contains(knowledge.Evidence, evidence =>
                evidence.FactId == joint.Id &&
                evidence.Description.Contains("authority `observed-only`", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static async Task<FactDocument> ScanAsync(string root)
    {
        var csharp = await new CSharpEvidenceScanner().ScanAsync(root);
        var frontend = await new FrontendScanner().ScanAsync(root);
        return Merge(csharp, frontend);
    }

    private static FeatureCandidate FindCandidate(FactDocument facts)
    {
        var candidates = new CrossStackFeatureCandidateBuilder().Build(facts);
        candidates = new JointVisibilityCandidateEnricher().Enrich(candidates, facts);
        return Assert.Single(candidates.Candidates, candidate =>
            candidate.Facts.Any(fact =>
                fact.Id == candidate.SeedFactId &&
                fact.Kind == "endpoint" &&
                fact.Name == "GetVisiblePrice"));
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
            "pkc-r710-tests",
            name,
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static async Task WriteFixtureAsync(string root)
    {
        await File.WriteAllTextAsync(Path.Combine(root, "VisibilityApp.csproj"), ProjectSource);
        await File.WriteAllTextAsync(Path.Combine(root, "PricesController.cs"), BackendSource);
        await File.WriteAllTextAsync(Path.Combine(root, "angular.json"), "{}");
        await File.WriteAllTextAsync(
            Path.Combine(root, "package.json"),
            "{\"dependencies\":{\"@angular/core\":\"22.0.0\"}}");
        await File.WriteAllTextAsync(Path.Combine(root, "price-result.ts"), PriceResultSource);
        await File.WriteAllTextAsync(Path.Combine(root, "price-api.ts"), PriceApiSource);
        await File.WriteAllTextAsync(Path.Combine(root, "price.component.ts"), PriceComponentSource);
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

    private const string NestedPriceComponentSource = """
        import { Component, inject } from '@angular/core';
        import { PriceApi } from './price-api';

        @Component({
          selector: 'app-price',
          template: `
            @if (displayPrice > 0) {
              @if (displayPrice < 100) {
                <strong>{{ displayPrice }}</strong>
              }
            }
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

    private const string HiddenPriceComponentSource = """
        import { Component, inject } from '@angular/core';
        import { PriceApi } from './price-api';

        @Component({
          selector: 'app-price',
          template: `
            <section hidden>
              <div>
                @if (displayPrice > 0) {
                  <strong>{{ displayPrice }}</strong>
                }
              </div>
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

    private const string PriceComponentSource = """
        import { Component, inject } from '@angular/core';
        import { PriceApi } from './price-api';

        @Component({
          selector: 'app-price',
          template: `
            @if (displayPrice > 0) {
              <strong>{{ displayPrice }}</strong>
            }
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
