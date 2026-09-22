using Pkc.Core;
using Pkc.Frontend;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class AngularSvgOpacityRenderAuthorityRegressionTests
{
    [Theory]
    [InlineData("opacity=\"0\"")]
    [InlineData("style=\"opacity: 0\"")]
    [InlineData("[attr.opacity]=\"textOpacity\"")]
    [InlineData("[style.opacity]=\"textOpacity\"")]
    public async Task Svg_text_with_zero_or_unproven_opacity_is_not_authoritative_visible_render(string attributes)
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
              textOpacity = 1;
            }
            """.Replace("ATTRIBUTES", attributes, StringComparison.Ordinal);

        var facts = await ScanAsync(source);

        AssertNoAuthoritativeRenderOrVisibility(facts);
    }

    [Fact]
    public async Task Svg_group_zero_opacity_suppresses_descendant_text_authority()
    {
        var facts = await ScanAsync("""
            import { Component } from '@angular/core';

            @Component({
              selector: 'app-price',
              template: `
                @if (isAllowed) {
                  <svg>
                    <g opacity="0">
                      <text>{{ displayPrice }}</text>
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

        AssertNoAuthoritativeRenderOrVisibility(facts);
    }

    [Fact]
    public async Task Svg_positive_static_opacity_does_not_unlock_native_svg_authority()
    {
        var facts = await ScanAsync("""
            import { Component } from '@angular/core';

            @Component({
              selector: 'app-price',
              template: `
                @if (isAllowed) {
                  <svg>
                    <text opacity="0.5">{{ displayPrice }}</text>
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

    private static async Task<FactDocument> ScanAsync(string componentSource)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pkc-r710-svg-opacity-tests",
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
