using Pkc.Core;
using Pkc.Frontend;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class JointVisibilityTemplateAuthorityRegressionTests
{
    [Fact]
    public async Task Same_line_html_comment_at_if_does_not_create_render_visibility()
    {
        await AssertNoVisibilityAsync("""
            import { Component } from '@angular/core';

            @Component({
              selector: 'app-price',
              template: `
                <!-- @if (isAllowed) { -->
                <strong>{{ displayPrice }}</strong>
                <!-- } -->
              `
            })
            export class PriceComponent {
              displayPrice = 42;
            }
            """);
    }

    [Fact]
    public async Task Multiline_html_comment_at_if_does_not_create_render_visibility()
    {
        await AssertNoVisibilityAsync("""
            import { Component } from '@angular/core';

            @Component({
              selector: 'app-price',
              template: `
                <!--
                @if (isAllowed) {
                -->
                <strong>{{ displayPrice }}</strong>
                <!--
                }
                -->
              `
            })
            export class PriceComponent {
              displayPrice = 42;
            }
            """);
    }

    [Fact]
    public async Task Html_comment_interpolation_is_not_an_authoritative_render()
    {
        await AssertNoAuthoritativeRenderAsync("""
            import { Component } from '@angular/core';

            @Component({
              selector: 'app-price',
              template: `
                <!-- {{ displayPrice }} -->
              `
            })
            export class PriceComponent {
              displayPrice = 42;
            }
            """);
    }

    [Fact]
    public async Task Bare_ng_template_interpolation_is_not_an_authoritative_render_or_visibility()
    {
        await AssertNoAuthoritativeRenderAsync("""
            import { Component } from '@angular/core';

            @Component({
              selector: 'app-price',
              template: `
                <ng-template>
                  @if (displayPrice > 0) {
                    <strong>{{ displayPrice }}</strong>
                  }
                </ng-template>
              `
            })
            export class PriceComponent {
              displayPrice = 42;
            }
            """);
    }

    private static async Task AssertNoVisibilityAsync(string componentSource)
    {
        var facts = await ScanAsync(componentSource);

        Assert.Contains(facts.Facts, fact =>
            fact.Kind == "ui-member-render" &&
            fact.Metadata.GetValueOrDefault("member") == "displayPrice" &&
            fact.Metadata.ContainsKey("renderAuthority"));
        Assert.DoesNotContain(facts.Facts, fact =>
            fact.Kind == "ui-member-visibility" &&
            fact.Metadata.GetValueOrDefault("member") == "displayPrice");
    }

    private static async Task AssertNoAuthoritativeRenderAsync(string componentSource)
    {
        var facts = await ScanAsync(componentSource);

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
            "pkc-r710-template-authority-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "angular.json"), "{}");
            await File.WriteAllTextAsync(
                Path.Combine(root, "package.json"),
                "{\"dependencies\":{\"@angular/core\":\"22.0.0\"}}");
            await File.WriteAllTextAsync(
                Path.Combine(root, "price.component.ts"),
                componentSource);

            return await new FrontendScanner().ScanAsync(root);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
