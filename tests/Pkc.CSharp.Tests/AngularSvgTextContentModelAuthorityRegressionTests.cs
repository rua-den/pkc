using Pkc.Core;
using Pkc.Frontend;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class AngularSvgTextContentModelAuthorityRegressionTests
{
    [Fact]
    public async Task Svg_graphics_child_inside_text_is_not_authoritative_visible_render()
    {
        var facts = await ScanAsync("""
            import { Component } from '@angular/core';

            @Component({
              selector: 'app-price',
              template: `
                @if (isAllowed) {
                  <svg>
                    <text><g>{{ displayPrice }}</g></text>
                  </svg>
                }
              `
            })
            export class PriceComponent {
              isAllowed = true;
              displayPrice = 42;
            }
            """);

        AssertNoAuthoritativeRenderOrVisibility(facts);
    }

    [Fact]
    public async Task Svg_nested_text_is_not_authoritative_visible_render()
    {
        var facts = await ScanAsync("""
            import { Component } from '@angular/core';

            @Component({
              selector: 'app-price',
              template: `
                @if (isAllowed) {
                  <svg>
                    <text><text>{{ displayPrice }}</text></text>
                  </svg>
                }
              `
            })
            export class PriceComponent {
              isAllowed = true;
              displayPrice = 42;
            }
            """);

        AssertNoAuthoritativeRenderOrVisibility(facts);
    }

    [Fact]
    public async Task Svg_text_path_without_proven_path_is_not_authoritative_visible_render()
    {
        var facts = await ScanAsync("""
            import { Component } from '@angular/core';

            @Component({
              selector: 'app-price',
              template: `
                @if (isAllowed) {
                  <svg>
                    <text><textPath>{{ displayPrice }}</textPath></text>
                  </svg>
                }
              `
            })
            export class PriceComponent {
              isAllowed = true;
              displayPrice = 42;
            }
            """);

        AssertNoAuthoritativeRenderOrVisibility(facts);
    }

    [Fact]
    public async Task Svg_switch_branch_is_not_authoritative_without_selection_proof()
    {
        var facts = await ScanAsync("""
            import { Component } from '@angular/core';

            @Component({
              selector: 'app-price',
              template: `
                @if (isAllowed) {
                  <svg>
                    <switch>
                      <text>Static first branch</text>
                      <text>{{ displayPrice }}</text>
                    </switch>
                  </svg>
                }
              `
            })
            export class PriceComponent {
              isAllowed = true;
              displayPrice = 42;
            }
            """);

        AssertNoAuthoritativeRenderOrVisibility(facts);
    }

    [Theory]
    [InlineData("systemLanguage=\"zz\"")]
    [InlineData("requiredExtensions=\"https://example.invalid/extension\"")]
    [InlineData("[attr.systemLanguage]=\"language\"")]
    public async Task Svg_conditional_processing_is_not_authoritative_without_runtime_proof(string attributes)
    {
        var source = """
            import { Component } from '@angular/core';

            @Component({
              selector: 'app-price',
              template: `
                @if (isAllowed) {
                  <svg>
                    <text ATTRIBUTES>{{ displayPrice }}</text>
                  </svg>
                }
              `
            })
            export class PriceComponent {
              isAllowed = true;
              displayPrice = 42;
              language = 'en';
            }
            """.Replace("ATTRIBUTES", attributes, StringComparison.Ordinal);

        var facts = await ScanAsync(source);

        AssertNoAuthoritativeRenderOrVisibility(facts);
    }

    [Fact]
    public async Task Svg_group_text_and_tspan_remain_authoritative_visible_render()
    {
        var facts = await ScanAsync("""
            import { Component } from '@angular/core';

            @Component({
              selector: 'app-price',
              template: `
                @if (isAllowed) {
                  <svg>
                    <g>
                      <text><tspan>{{ displayPrice }}</tspan></text>
                    </g>
                  </svg>
                }
              `
            })
            export class PriceComponent {
              isAllowed = true;
              displayPrice = 42;
            }
            """);

        AssertAuthoritativeRenderAndVisibility(facts);
    }

    private static void AssertNoAuthoritativeRenderOrVisibility(FactDocument facts)
    {
        Assert.DoesNotContain(facts.Facts, fact =>
            fact.Kind == "ui-member-render" &&
            fact.Metadata.GetValueOrDefault("member") == "displayPrice" &&
            fact.Metadata.ContainsKey("renderAuthority"));
        Assert.DoesNotContain(facts.Facts, fact =>
            fact.Kind == "ui-member-visibility" &&
            fact.Metadata.GetValueOrDefault("member") == "displayPrice");
    }

    private static void AssertAuthoritativeRenderAndVisibility(FactDocument facts)
    {
        Assert.Contains(facts.Facts, fact =>
            fact.Kind == "ui-member-render" &&
            fact.Metadata.GetValueOrDefault("member") == "displayPrice" &&
            fact.Metadata.ContainsKey("renderAuthority"));
        Assert.Contains(facts.Facts, fact =>
            fact.Kind == "ui-member-visibility" &&
            fact.Metadata.GetValueOrDefault("member") == "displayPrice" &&
            fact.Metadata.GetValueOrDefault("condition") == "isAllowed");
    }

    private static async Task<FactDocument> ScanAsync(string componentSource)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pkc-r710-svg-text-content-model-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

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
}
