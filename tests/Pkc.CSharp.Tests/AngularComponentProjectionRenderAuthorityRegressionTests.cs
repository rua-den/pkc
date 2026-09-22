using Pkc.Core;
using Pkc.Frontend;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class AngularComponentProjectionRenderAuthorityRegressionTests
{
    [Fact]
    public async Task Component_element_without_proven_projection_does_not_create_render_or_visibility_authority()
    {
        var facts = await ScanAsync(
            """
            import { Component } from '@angular/core';

            @Component({
              selector: 'app-shell',
              template: `<span>Shell</span>`
            })
            export class ShellComponent {}
            """,
            """
            import { Component } from '@angular/core';
            import { ShellComponent } from './shell.component';

            @Component({
              selector: 'app-price',
              imports: [ShellComponent],
              template: `
                @if (isAllowed) {
                  <app-shell>{{ displayPrice }}</app-shell>
                }
              `
            })
            export class PriceComponent {
              isAllowed = true;
              displayPrice = 42;
            }
            """);

        AssertNoAuthoritativeRenderOrVisibility(facts, "displayPrice");
    }

    [Fact]
    public async Task Attribute_selector_component_without_proven_projection_does_not_create_render_or_visibility_authority()
    {
        var facts = await ScanAsync(
            """
            import { Component } from '@angular/core';

            @Component({
              selector: 'div[app-shell]',
              template: `<span>Shell</span>`
            })
            export class ShellComponent {}
            """,
            """
            import { Component } from '@angular/core';
            import { ShellComponent } from './shell.component';

            @Component({
              selector: 'app-price',
              imports: [ShellComponent],
              template: `
                @if (isAllowed) {
                  <div app-shell>{{ displayPrice }}</div>
                }
              `
            })
            export class PriceComponent {
              isAllowed = true;
              displayPrice = 42;
            }
            """);

        AssertNoAuthoritativeRenderOrVisibility(facts, "displayPrice");
    }

    [Fact]
    public async Task Closed_projection_boundary_before_plain_html_preserves_later_supported_render()
    {
        var facts = await ScanAsync(
            """
            import { Component } from '@angular/core';

            @Component({
              selector: 'app-shell',
              template: `<span>Shell</span>`
            })
            export class ShellComponent {}
            """,
            """
            import { Component } from '@angular/core';
            import { ShellComponent } from './shell.component';

            @Component({
              selector: 'app-price',
              imports: [ShellComponent],
              template: `
                <app-shell>{{ projectedPrice }}</app-shell>
                @if (isAllowed) {
                  <strong>{{ displayPrice }}</strong>
                }
              `
            })
            export class PriceComponent {
              isAllowed = true;
              projectedPrice = 7;
              displayPrice = 42;
            }
            """);

        AssertNoAuthoritativeRenderOrVisibility(facts, "projectedPrice");
        Assert.Contains(facts.Facts, fact =>
            fact.Kind == "ui-member-render" &&
            fact.Metadata.GetValueOrDefault("member") == "displayPrice" &&
            fact.Metadata.ContainsKey("renderAuthority"));
        Assert.Contains(facts.Facts, fact =>
            fact.Kind == "ui-member-visibility" &&
            fact.Metadata.GetValueOrDefault("member") == "displayPrice" &&
            fact.Metadata.GetValueOrDefault("condition") == "isAllowed");
    }

    private static void AssertNoAuthoritativeRenderOrVisibility(FactDocument facts, string member)
    {
        Assert.DoesNotContain(facts.Facts, fact =>
            fact.Kind == "ui-member-render" &&
            fact.Metadata.GetValueOrDefault("member") == member &&
            fact.Metadata.ContainsKey("renderAuthority"));
        Assert.DoesNotContain(facts.Facts, fact =>
            fact.Kind == "ui-member-visibility" &&
            fact.Metadata.GetValueOrDefault("member") == member);
    }

    private static async Task<FactDocument> ScanAsync(
        string shellComponentSource,
        string priceComponentSource)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pkc-r710-component-projection-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "angular.json"), "{}");
            await File.WriteAllTextAsync(
                Path.Combine(root, "package.json"),
                "{\"dependencies\":{\"@angular/core\":\"22.0.0\"}}");
            await File.WriteAllTextAsync(
                Path.Combine(root, "shell.component.ts"),
                shellComponentSource);
            await File.WriteAllTextAsync(
                Path.Combine(root, "price.component.ts"),
                priceComponentSource);
            return await new FrontendScanner().ScanAsync(root);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
