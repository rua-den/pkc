using Pkc.Core;
using Pkc.Frontend;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class AngularNonBindableRenderAuthorityRegressionTests
{
    [Theory]
    [InlineData("<strong ngNonBindable>{{ displayPrice }}</strong>")]
    [InlineData("<section ngNonBindable><strong>{{ displayPrice }}</strong></section>")]
    public async Task Non_bindable_subtree_does_not_create_value_render_authority(string markup)
    {
        var source = ComponentSource.Replace("__MARKUP__", markup, StringComparison.Ordinal);
        var facts = await ScanAsync(source);

        Assert.DoesNotContain(facts.Facts, fact =>
            fact.Kind == "ui-member-render" &&
            fact.Metadata.GetValueOrDefault("member") == "displayPrice" &&
            fact.Metadata.ContainsKey("renderAuthority"));
        Assert.DoesNotContain(facts.Facts, fact =>
            fact.Kind == "ui-member-visibility" &&
            fact.Metadata.GetValueOrDefault("member") == "displayPrice");
    }

    [Fact]
    public async Task Closed_non_bindable_sibling_does_not_suppress_later_active_render()
    {
        var facts = await ScanAsync("""
            import { Component } from '@angular/core';

            @Component({
              selector: 'app-price',
              template: `
                <span ngNonBindable>{{ ignored }}</span>
                @if (displayPrice > 0) {
                  <strong>{{ displayPrice }}</strong>
                }
              `
            })
            export class PriceComponent {
              ignored = 'literal';
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

    private static async Task<FactDocument> ScanAsync(string componentSource)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pkc-r710-non-bindable-tests",
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

    private const string ComponentSource = """
        import { Component } from '@angular/core';

        @Component({
          selector: 'app-price',
          template: `
            @if (displayPrice > 0) {
              __MARKUP__
            }
          `
        })
        export class PriceComponent {
          displayPrice = 42;
        }
        """;
}
