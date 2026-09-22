using Pkc.Core;
using Pkc.Frontend;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class AngularHiddenBindingRenderAuthorityRegressionTests
{
    [Theory]
    [InlineData("[hidden]=\"true\"")]
    [InlineData("bind-hidden=\"true\"")]
    [InlineData("[hidden]=\"isHidden\"")]
    [InlineData("[hidden]=\"False\"")]
    public async Task Hidden_binding_that_can_suppress_presentation_fails_closed(string hiddenBinding)
    {
        var source = ComponentSource.Replace("__HIDDEN_BINDING__", hiddenBinding, StringComparison.Ordinal);
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
    public async Task Literal_false_hidden_binding_preserves_render_authority()
    {
        var source = ComponentSource.Replace("__HIDDEN_BINDING__", "[hidden]=\"false\"", StringComparison.Ordinal);
        var facts = await ScanAsync(source);

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
            "pkc-r710-hidden-binding-tests",
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
            <section __HIDDEN_BINDING__>
              @if (displayPrice > 0) {
                <strong>{{ displayPrice }}</strong>
              }
            </section>
          `
        })
        export class PriceComponent {
          displayPrice = 42;
          isHidden = true;
          False = true;
        }
        """;
}
