using Pkc.Frontend;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class AngularIfCommentBraceVisibilityRegressionTests
{
    [Fact]
    public async Task Comment_open_brace_does_not_extend_if_scope_over_later_render()
    {
        var facts = await ScanAsync("""
            import { Component } from '@angular/core';

            @Component({
              selector: 'app-price',
              template: `
                @if (isAllowed) {
                  <!-- { -->
                }
                <strong>{{ displayPrice }}</strong>
                <!-- } -->
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
    public async Task Comment_close_brace_does_not_shorten_real_if_scope()
    {
        var facts = await ScanAsync("""
            import { Component } from '@angular/core';

            @Component({
              selector: 'app-price',
              template: `
                @if (isAllowed) {
                  <!-- } -->
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

    private static async Task<Pkc.Core.FactDocument> ScanAsync(string componentSource)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pkc-r710-comment-brace-tests",
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
