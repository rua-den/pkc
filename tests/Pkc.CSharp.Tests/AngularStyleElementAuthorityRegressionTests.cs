using Pkc.Core;
using Pkc.Frontend;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class AngularStyleElementAuthorityRegressionTests
{
    [Fact]
    public async Task Style_element_literal_interpolation_does_not_create_render_or_visibility_authority()
    {
        var facts = await ScanAsync("""
            import { Component } from '@angular/core';

            @Component({
              selector: 'app-price',
              template: `
                @if (isAllowed) {
                  <style>
                    .price::before { content: "{{ displayPrice }}"; }
                  </style>
                }
              `
            })
            export class PriceComponent {
              isAllowed = true;
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
    public async Task Style_braces_cannot_extend_closed_if_over_later_visible_render()
    {
        var facts = await ScanAsync("""
            import { Component } from '@angular/core';

            @Component({
              selector: 'app-price',
              template: `
                @if (isAllowed) {
                  <style>
                    .before::after { content: "{"; }
                  </style>
                }
                <strong>{{ displayPrice }}</strong>
                <style>
                  .after::after { content: "}"; }
                </style>
              `
            })
            export class PriceComponent {
              isAllowed = true;
              displayPrice = 42;
            }
            """);

        Assert.Contains(facts.Facts, fact =>
            fact.Kind == "ui-member-render" &&
            fact.Metadata.GetValueOrDefault("member") == "displayPrice" &&
            fact.Metadata.ContainsKey("renderAuthority"));
        Assert.DoesNotContain(facts.Facts, fact =>
            fact.Kind == "ui-member-visibility" &&
            fact.Metadata.GetValueOrDefault("member") == "displayPrice");
    }

    [Fact]
    public async Task Fake_if_inside_style_cannot_control_later_visible_render()
    {
        var facts = await ScanAsync("""
            import { Component } from '@angular/core';

            @Component({
              selector: 'app-price',
              template: `
                <style>
                  .before::after { content: "@if (fake) {{"; }
                </style>
                <strong>{{ displayPrice }}</strong>
                <style>
                  .after::after { content: "}"; }
                </style>
              `
            })
            export class PriceComponent {
              fake = true;
              displayPrice = 42;
            }
            """);

        Assert.Contains(facts.Facts, fact =>
            fact.Kind == "ui-member-render" &&
            fact.Metadata.GetValueOrDefault("member") == "displayPrice" &&
            fact.Metadata.ContainsKey("renderAuthority"));
        Assert.DoesNotContain(facts.Facts, fact =>
            fact.Kind == "ui-member-visibility" &&
            fact.Metadata.GetValueOrDefault("member") == "displayPrice");
    }

    [Fact]
    public async Task Closed_style_before_real_if_preserves_supported_visibility()
    {
        var facts = await ScanAsync("""
            import { Component } from '@angular/core';

            @Component({
              selector: 'app-price',
              template: `
                <style>
                  .price { font-weight: 600; }
                </style>
                @if (isAllowed) {
                  <strong>{{ displayPrice }}</strong>
                }
              `
            })
            export class PriceComponent {
              isAllowed = true;
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
            fact.Metadata.GetValueOrDefault("condition") == "isAllowed");
    }

    private static async Task<FactDocument> ScanAsync(string componentSource)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pkc-r710-style-element-tests",
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
