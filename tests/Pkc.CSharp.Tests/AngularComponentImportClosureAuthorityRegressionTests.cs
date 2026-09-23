using Pkc.Frontend;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class AngularComponentImportClosureAuthorityRegressionTests
{
    [Fact]
    public async Task External_component_risk_reached_through_local_shared_imports_fails_closed()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        var root = CreateTestRoot();
        var externalPackageRoot = Path.Combine(
            Path.GetTempPath(),
            "pkc-r710-indirect-external-package-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(externalPackageRoot);

        try
        {
            var appRoot = await WriteAngularAppAsync(root, useBarrel: false);
            await WriteExternalComponentDeclarationAsync(externalPackageRoot);
            LinkExternalPackage(appRoot, externalPackageRoot);

            await AssertProjectionAuthorityRejectedAsync(root);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
            Directory.Delete(externalPackageRoot, recursive: true);
        }
    }

    [Fact]
    public async Task External_component_risk_reached_through_local_barrel_fails_closed()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        var root = CreateTestRoot();
        var externalPackageRoot = Path.Combine(
            Path.GetTempPath(),
            "pkc-r710-indirect-external-package-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(externalPackageRoot);

        try
        {
            var appRoot = await WriteAngularAppAsync(root, useBarrel: true);
            await WriteExternalComponentDeclarationAsync(externalPackageRoot);
            LinkExternalPackage(appRoot, externalPackageRoot);

            await AssertProjectionAuthorityRejectedAsync(root);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
            Directory.Delete(externalPackageRoot, recursive: true);
        }
    }

    [Fact]
    public async Task Local_shared_framework_imports_keep_ordinary_render_authority()
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
            "pkc-r710-component-import-closure-tests",
            Guid.NewGuid().ToString("N"));

    private static async Task<string> WriteAngularAppAsync(
        string root,
        bool useBarrel)
    {
        var appRoot = Path.Combine(root, "frontend");
        var sourceRoot = Path.Combine(appRoot, "src");
        Directory.CreateDirectory(sourceRoot);
        await File.WriteAllTextAsync(Path.Combine(appRoot, "angular.json"), "{}");
        await File.WriteAllTextAsync(
            Path.Combine(appRoot, "package.json"),
            "{\"dependencies\":{\"@angular/core\":\"22.0.0\",\"@vendor/ui\":\"1.0.0\"}}");
        await File.WriteAllTextAsync(
            Path.Combine(sourceRoot, "shared-imports.ts"),
            """
            import { ExternalShell } from '@vendor/ui';
            export const SHARED_IMPORTS = [ExternalShell];
            """);

        if (useBarrel)
        {
            await File.WriteAllTextAsync(
                Path.Combine(sourceRoot, "shared.ts"),
                """
                export { SHARED_IMPORTS } from './shared-imports';
                """);
        }

        var importPath = useBarrel ? "./shared" : "./shared-imports";
        var componentSource = """
            import { Component } from '@angular/core';
            import { SHARED_IMPORTS } from '__IMPORT_PATH__';

            @Component({
              selector: 'app-price',
              imports: [SHARED_IMPORTS],
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
            """.Replace("__IMPORT_PATH__", importPath, StringComparison.Ordinal);
        await File.WriteAllTextAsync(
            Path.Combine(sourceRoot, "price.component.ts"),
            componentSource);

        return appRoot;
    }

    private static Task WriteExternalComponentDeclarationAsync(string packageRoot) =>
        File.WriteAllTextAsync(
            Path.Combine(packageRoot, "index.d.ts"),
            """
            import * as i0 from '@angular/core';
            export declare class ExternalShell {
              static ɵcmp: i0.ɵɵComponentDeclaration<ExternalShell, "div[ext-shell]", never, {}, {}, never, never, true, never>;
            }
            """);

    private static void LinkExternalPackage(
        string appRoot,
        string externalPackageRoot)
    {
        var linkedPackageRoot = Path.Combine(appRoot, "node_modules", "@vendor", "ui");
        Directory.CreateDirectory(Path.GetDirectoryName(linkedPackageRoot)!);
        Directory.CreateSymbolicLink(linkedPackageRoot, externalPackageRoot);
    }

    private static async Task AssertProjectionAuthorityRejectedAsync(string root)
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
