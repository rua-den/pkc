using Pkc.Core;
using Pkc.Frontend;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class AngularNestedNodeModulesProjectionAuthorityRegressionTests
{
    [Fact]
    public async Task Nested_angular_app_resolves_external_component_selector_from_nearest_node_modules()
    {
        var root = CreateTestRoot();
        Directory.CreateDirectory(root);

        try
        {
            var appRoot = await WriteAngularAppAsync(root);
            var packageRoot = Path.Combine(appRoot, "node_modules", "@vendor", "ui");
            Directory.CreateDirectory(packageRoot);
            await WriteExternalComponentDeclarationAsync(packageRoot);

            await AssertProjectionAuthorityRejectedAsync(root);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Workspace_symlink_package_inside_repository_resolves_component_selector()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        var root = CreateTestRoot();
        Directory.CreateDirectory(root);

        try
        {
            var appRoot = await WriteAngularAppAsync(root);
            var workspacePackageRoot = Path.Combine(root, "packages", "vendor-ui-dist");
            Directory.CreateDirectory(workspacePackageRoot);
            await WriteExternalComponentDeclarationAsync(workspacePackageRoot);

            var linkedPackageRoot = Path.Combine(appRoot, "node_modules", "@vendor", "ui");
            Directory.CreateDirectory(Path.GetDirectoryName(linkedPackageRoot)!);
            Directory.CreateSymbolicLink(linkedPackageRoot, workspacePackageRoot);

            await AssertProjectionAuthorityRejectedAsync(root);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Symlink_package_outside_repository_fails_closed_for_importing_component()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        var root = CreateTestRoot();
        var externalPackageRoot = Path.Combine(
            Path.GetTempPath(),
            "pkc-r710-external-linked-package-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(externalPackageRoot);

        try
        {
            var appRoot = await WriteAngularAppAsync(root);
            await WriteExternalComponentDeclarationAsync(externalPackageRoot);

            var linkedPackageRoot = Path.Combine(appRoot, "node_modules", "@vendor", "ui");
            Directory.CreateDirectory(Path.GetDirectoryName(linkedPackageRoot)!);
            Directory.CreateSymbolicLink(linkedPackageRoot, externalPackageRoot);

            await AssertProjectionAuthorityRejectedAsync(root);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
            Directory.Delete(externalPackageRoot, recursive: true);
        }
    }

    private static string CreateTestRoot() =>
        Path.Combine(
            Path.GetTempPath(),
            "pkc-r710-nested-node-modules-tests",
            Guid.NewGuid().ToString("N"));

    private static async Task<string> WriteAngularAppAsync(string root)
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
