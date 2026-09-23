using Pkc.Frontend;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class AngularUnsupportedComponentImportIndirectionAuthorityRegressionTests
{
    [Fact]
    public async Task Scalar_alias_of_indirect_external_component_imports_fails_closed()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        var root = CreateTestRoot();
        var externalPackageRoot = CreateExternalPackageRoot();
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(externalPackageRoot);

        try
        {
            var appRoot = Path.Combine(root, "frontend");
            var sourceRoot = Path.Combine(appRoot, "src");
            Directory.CreateDirectory(sourceRoot);
            await WriteExternalComponentDeclarationAsync(externalPackageRoot);
            LinkExternalPackage(appRoot, externalPackageRoot);
            await File.WriteAllTextAsync(Path.Combine(appRoot, "angular.json"), "{}");
            await File.WriteAllTextAsync(
                Path.Combine(sourceRoot, "shared-imports.ts"),
                """
                import { ExternalShell } from '@vendor/ui';
                export const SHARED_IMPORTS = [ExternalShell];
                """);
            await File.WriteAllTextAsync(
                Path.Combine(sourceRoot, "price.component.ts"),
                """
                import { Component } from '@angular/core';
                import { SHARED_IMPORTS } from './shared-imports';

                const IMPORTS = SHARED_IMPORTS;

                @Component({
                  selector: 'app-price',
                  imports: [IMPORTS],
                  template: `<div ext-shell>{{ displayPrice }}</div>`
                })
                export class PriceComponent {
                  displayPrice = 42;
                }
                """);

            await AssertRejectedAsync(root);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
            Directory.Delete(externalPackageRoot, recursive: true);
        }
    }

    [Fact]
    public async Task Default_reexport_of_indirect_external_component_imports_fails_closed()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        var root = CreateTestRoot();
        var externalPackageRoot = CreateExternalPackageRoot();
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(externalPackageRoot);

        try
        {
            var appRoot = Path.Combine(root, "frontend");
            var sourceRoot = Path.Combine(appRoot, "src");
            Directory.CreateDirectory(sourceRoot);
            await WriteExternalComponentDeclarationAsync(externalPackageRoot);
            LinkExternalPackage(appRoot, externalPackageRoot);
            await File.WriteAllTextAsync(Path.Combine(appRoot, "angular.json"), "{}");
            await File.WriteAllTextAsync(
                Path.Combine(sourceRoot, "shared-imports.ts"),
                """
                import { ExternalShell } from '@vendor/ui';
                const SHARED_IMPORTS = [ExternalShell];
                export default SHARED_IMPORTS;
                """);
            await File.WriteAllTextAsync(
                Path.Combine(sourceRoot, "shared.ts"),
                """
                export { default as SHARED_IMPORTS } from './shared-imports';
                """);
            await File.WriteAllTextAsync(
                Path.Combine(sourceRoot, "price.component.ts"),
                """
                import { Component } from '@angular/core';
                import { SHARED_IMPORTS } from './shared';

                @Component({
                  selector: 'app-price',
                  imports: [SHARED_IMPORTS],
                  template: `<div ext-shell>{{ displayPrice }}</div>`
                })
                export class PriceComponent {
                  displayPrice = 42;
                }
                """);

            await AssertRejectedAsync(root);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
            Directory.Delete(externalPackageRoot, recursive: true);
        }
    }

    [Fact]
    public async Task Supported_local_array_of_framework_imports_keeps_render_authority()
    {
        var root = CreateTestRoot();
        Directory.CreateDirectory(root);

        try
        {
            var appRoot = Path.Combine(root, "frontend");
            var sourceRoot = Path.Combine(appRoot, "src");
            Directory.CreateDirectory(sourceRoot);
            await File.WriteAllTextAsync(Path.Combine(appRoot, "angular.json"), "{}");
            await File.WriteAllTextAsync(
                Path.Combine(sourceRoot, "shared-imports.ts"),
                """
                import { CommonModule } from '@angular/common';
                export const SHARED_IMPORTS = [CommonModule];
                """);
            await File.WriteAllTextAsync(
                Path.Combine(sourceRoot, "price.component.ts"),
                """
                import { Component } from '@angular/core';
                import { SHARED_IMPORTS } from './shared-imports';

                @Component({
                  selector: 'app-price',
                  imports: [SHARED_IMPORTS],
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
            "pkc-r710-unsupported-import-indirection-tests",
            Guid.NewGuid().ToString("N"));

    private static string CreateExternalPackageRoot() =>
        Path.Combine(
            Path.GetTempPath(),
            "pkc-r710-unsupported-import-package-tests",
            Guid.NewGuid().ToString("N"));

    private static Task WriteExternalComponentDeclarationAsync(string packageRoot) =>
        File.WriteAllTextAsync(
            Path.Combine(packageRoot, "index.d.ts"),
            """
            import * as i0 from '@angular/core';
            export declare class ExternalShell {
              static ɵcmp: i0.ɵɵComponentDeclaration<ExternalShell, "div[ext-shell]", never, {}, {}, never, never, true, never>;
            }
            """);

    private static void LinkExternalPackage(string appRoot, string externalPackageRoot)
    {
        var linkedPackageRoot = Path.Combine(appRoot, "node_modules", "@vendor", "ui");
        Directory.CreateDirectory(Path.GetDirectoryName(linkedPackageRoot)!);
        Directory.CreateSymbolicLink(linkedPackageRoot, externalPackageRoot);
    }

    private static async Task AssertRejectedAsync(string root)
    {
        var facts = await new FrontendScanner().ScanAsync(root);

        Assert.DoesNotContain(facts.Facts, fact =>
            fact.Kind == "ui-member-render" &&
            fact.Metadata.GetValueOrDefault("member") == "displayPrice" &&
            fact.Metadata.ContainsKey("renderAuthority"));
        Assert.DoesNotContain(facts.Facts, fact =>
            fact.Kind == "ui-member-visibility" &&
            fact.Metadata.GetValueOrDefault("member") == "displayPrice");
    }
}
