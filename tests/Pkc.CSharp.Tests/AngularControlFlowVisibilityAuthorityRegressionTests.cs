using Pkc.Core;
using Pkc.Frontend;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class AngularControlFlowVisibilityAuthorityRegressionTests
{
    [Theory]
    [InlineData("@for (item of items; track item) {", "}")]
    [InlineData("@defer {", "}")]
    public async Task Unsupported_nested_control_block_fails_closed(string openBlock, string closeBlock)
    {
        var source = ComponentSource
            .Replace("__OPEN_BLOCK__", openBlock, StringComparison.Ordinal)
            .Replace("__CLOSE_BLOCK__", closeBlock, StringComparison.Ordinal);
        var facts = await ScanAsync(source);

        Assert.Contains(facts.Facts, fact =>
            fact.Kind == "ui-member-render" &&
            fact.Metadata.GetValueOrDefault("member") == "displayPrice" &&
            fact.Metadata.ContainsKey("renderAuthority"));
        Assert.DoesNotContain(facts.Facts, fact =>
            fact.Kind == "ui-member-visibility" &&
            fact.Metadata.GetValueOrDefault("member") == "displayPrice");
    }

    [Fact]
    public async Task Plain_text_apostrophe_cannot_extend_closed_if_over_later_render()
    {
        var facts = await ScanAsync("""
            import { Component } from '@angular/core';

            @Component({
              selector: 'app-price',
              template: `
                @if (isAllowed) {
                  <span>it's literal text</span>
                }
                <strong>{{ displayPrice }}</strong>
                @if (later) {
                  <span>later'</span>
                }
              `
            })
            export class PriceComponent {
              isAllowed = true;
              later = true;
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
    public async Task Plain_text_apostrophe_inside_real_if_preserves_supported_visibility()
    {
        var facts = await ScanAsync("""
            import { Component } from '@angular/core';

            @Component({
              selector: 'app-price',
              template: `
                @if (isAllowed) {
                  <span>it's visible text</span>
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
            fact.Kind == "ui-member-visibility" &&
            fact.Metadata.GetValueOrDefault("member") == "displayPrice" &&
            fact.Metadata.GetValueOrDefault("condition") == "isAllowed");
    }

    private static async Task<FactDocument> ScanAsync(string componentSource)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pkc-r710-control-flow-tests",
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
              __OPEN_BLOCK__
                <strong>{{ displayPrice }}</strong>
              __CLOSE_BLOCK__
            }
          `
        })
        export class PriceComponent {
          displayPrice = 42;
          items = [1];
        }
        """;
}
