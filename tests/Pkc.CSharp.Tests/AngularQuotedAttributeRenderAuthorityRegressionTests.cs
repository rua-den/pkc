using Pkc.Core;
using Pkc.Frontend;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class AngularQuotedAttributeRenderAuthorityRegressionTests
{
    [Theory]
    [InlineData("<strong title=\"price > {{ displayPrice }}\"></strong>")]
    [InlineData("<strong title='price > {{ displayPrice }}'></strong>")]
    public async Task Greater_than_inside_quoted_attribute_does_not_turn_attribute_interpolation_into_visible_text(string markup)
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
    public async Task Greater_than_inside_quoted_attribute_before_real_text_does_not_suppress_active_render()
    {
        var facts = await ScanAsync(ComponentSource.Replace(
            "__MARKUP__",
            "<strong title=\"price > literal\">{{ displayPrice }}</strong>",
            StringComparison.Ordinal));

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
            "pkc-r710-quoted-attribute-tests",
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
