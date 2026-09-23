using Pkc.Frontend;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class AngularUnresolvedExternalComponentImportAuthorityRegressionTests
{
    [Fact]
    public async Task Unresolved_external_symbol_used_in_component_imports_fails_closed()
    {
        var root = CreateTestRoot();
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "angular.json"), "{}");
            await File.WriteAllTextAsync(
                Path.Combine(root, "price.component.ts"),
                """
                import { Component } from '@angular/core';
                import { ExternalShell } from '@vendor/ui';

                @Component({
                  selector: 'app-price',
                  imports: [ExternalShell],
                  template: `
                    @if (isAllowed) {
                      <div ext-shell>{{ displayPrice }}</div>
                    }
                  `
                })
                export class PriceComponent {
                  isAllowed = true;
                  displayPrice = 42;
                }
                """);

            var facts = await new FrontendScanner().ScanAsync(root);

            Assert.DoesNotContain(facts.Facts, fact =>
                fact.Kind == "ui-member-render" &&
                fact.Metadata.GetValueOrDefault("member") == "displayPrice" &&
                fact.Metadata.ContainsKey("renderAuthority"));
            Assert.DoesNotContain(facts.Facts, fact =>
                fact.Kind == "ui-member-visibility" &&
                fact.Metadata.GetValueOrDefault("member") == "displayPrice");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Unresolved_external_symbol_not_used_in_component_imports_keeps_ordinary_render_authority()
    {
        var root = CreateTestRoot();
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "angular.json"), "{}");
            await File.WriteAllTextAsync(
                Path.Combine(root, "price.component.ts"),
                """
                import { Component } from '@angular/core';
                import { map } from 'rxjs';

                @Component({
                  selector: 'app-price',
                  imports: [],
                  template: `<span>{{ displayPrice }}</span>`
                })
                export class PriceComponent {
                  displayPrice = 42;
                }
                """);

            var facts = await new FrontendScanner().ScanAsync(root);

            Assert.Contains(facts.Facts, fact =>
                fact.Kind == "ui-member-render" &&
                fact.Metadata.GetValueOrDefault("member") == "displayPrice" &&
                fact.Metadata.ContainsKey("renderAuthority"));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static string CreateTestRoot() =>
        Path.Combine(
            Path.GetTempPath(),
            "pkc-r710-unresolved-external-import-tests",
            Guid.NewGuid().ToString("N"));
}
