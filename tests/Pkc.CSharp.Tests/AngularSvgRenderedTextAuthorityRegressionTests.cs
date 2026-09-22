using Pkc.Core;
using Pkc.Frontend;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class AngularSvgRenderedTextAuthorityRegressionTests
{
    [Fact]
    public async Task Svg_raw_text_interpolation_is_not_authoritative_visible_render()
    {
        var facts = await ScanAsync("""
            import { Component } from '@angular/core';

            @Component({
              selector: 'app-price',
              template: `
                @if (isAllowed) {
                  <svg>
                    {{ displayPrice }}
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
    [InlineData("display=\"none\"")]
    [InlineData("visibility=\"hidden\"")]
    [InlineData("visibility=\"collapse\"")]
    public async Task Svg_text_with_static_presentation_suppression_is_not_authoritative_visible_render(string attributes)
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
            }
            """.Replace("ATTRIBUTES", attributes, StringComparison.Ordinal);

        var facts = await ScanAsync(source);

        AssertNoAuthoritativeRenderOrVisibility(facts);
    }

    [Theory]
    [InlineData("title")]
    [InlineData("desc")]
    public async Task Svg_descriptive_text_interpolation_is_not_authoritative_visible_render(string elementName)
    {
        var source = """
            import { Component } from '@angular/core';

            @Component({
              selector: 'app-price',
              template: `
                @if (isAllowed) {
                  <svg>
                    <ELEMENT>{{ displayPrice }}</ELEMENT>
                  </svg>
                }
              `
            })
            export class PriceComponent {
              isAllowed = true;
              displayPrice = 42;
            }
            """.Replace("ELEMENT", elementName, StringComparison.Ordinal);

        var facts = await ScanAsync(source);

        AssertNoAuthoritativeRenderOrVisibility(facts);
    }

    [Fact]
    public async Task Svg_text_element_interpolation_remains_authoritative_visible_render()
    {
        var facts = await ScanAsync("""
            import { Component } from '@angular/core';

            @Component({
              selector: 'app-price',
              template: `
                @if (isAllowed) {
                  <svg>
                    <text>{{ displayPrice }}</text>
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

    [Fact]
    public async Task Svg_foreign_object_html_interpolation_remains_authoritative_visible_render()
    {
        var facts = await ScanAsync("""
            import { Component } from '@angular/core';

            @Component({
              selector: 'app-price',
              template: `
                @if (isAllowed) {
                  <svg>
                    <foreignObject>
                      <div>{{ displayPrice }}</div>
                    </foreignObject>
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
            "pkc-r710-svg-text-tests",
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
