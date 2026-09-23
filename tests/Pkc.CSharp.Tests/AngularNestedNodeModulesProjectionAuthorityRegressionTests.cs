using Pkc.Core;
using Pkc.Frontend;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class AngularNestedNodeModulesProjectionAuthorityRegressionTests
{
    [Fact]
    public async Task Nested_angular_app_resolves_external_component_selector_from_nearest_node_modules()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pkc-r710-nested-node-modules-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var appRoot = Path.Combine(root, "frontend");
            var sourceRoot = Path.Combine(appRoot, "src");
            Directory.CreateDirectory(sourceRoot);
            await File.WriteAllTextAsync(Path.Combine(appRoot, "angular.json"), "{}");
            await File.WriteAllTextAsync(
                Path.Combine(appRoot, "package.json"),
                "{\"dependencies\":{\"@angular/core\":\"22.0.0\",\"@vendor/ui\":\"1.0.0\"}}");
            await File.WriteAllTextAsync(
                Path.Combine(sourceRoot, "price.component.ts"),
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
            await File.WriteAllTextAsync(
                Path.Combine(sourceRoot, "unused.component.ts"),
                """
                import { Component } from '@angular/core';
                @Component({ selector: 'unused-shell', template: `<span>unused</span>` })
                export class UnusedComponent {}
                """);

            var packageRoot = Path.Combine(appRoot, "node_modules", "@vendor", "ui");
            Directory.CreateDirectory(packageRoot);
            await File.WriteAllTextAsync(
                Path.Combine(packageRoot, "index.d.ts"),
                """
                import * as i0 from '@angular/core';
                export declare class ExternalShell {
                  static ɵcmp: i0.ɵɵComponentDeclaration<ExternalShell, "div[ext-shell]", never, {}, {}, never, never, true, never>;
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
}
