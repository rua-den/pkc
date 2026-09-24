using Pkc.Frontend;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class AngularSemicolonlessComponentImportIndirectionAuthorityRegressionTests
{
    [Fact]
    public async Task Semicolonless_scalar_alias_of_indirect_external_component_imports_fails_closed()
    {
        await AssertScalarAliasRejectedAsync(BuildComponentSource("\n", duplicateAlias: false));
    }

    [Fact]
    public async Task Duplicate_semicolonless_scalar_alias_names_across_scopes_fail_closed()
    {
        await AssertScalarAliasRejectedAsync(BuildComponentSource("\n", duplicateAlias: true));
    }

    [Theory]
    [InlineData("\r")]
    [InlineData("\u2028")]
    [InlineData("\u2029")]
    public async Task Semicolonless_scalar_alias_respects_typescript_line_terminators(string lineTerminator)
    {
        await AssertScalarAliasRejectedAsync(BuildComponentSource(lineTerminator, duplicateAlias: false));
    }

    private static string BuildComponentSource(string lineTerminator, bool duplicateAlias)
    {
        var lines = new List<string>
        {
            "import { Component } from '@angular/core';",
            "import { SHARED_IMPORTS } from './shared-imports';",
            string.Empty,
            "const IMPORTS = SHARED_IMPORTS"
        };

        if (duplicateAlias)
        {
            lines.Add(string.Empty);
            lines.Add("function preserveScope() {");
            lines.Add("  const IMPORTS = SHARED_IMPORTS");
            lines.Add("  return IMPORTS");
            lines.Add("}");
        }

        lines.Add(string.Empty);
        lines.Add("@Component({");
        lines.Add("  selector: 'app-price',");
        lines.Add("  imports: [IMPORTS],");
        lines.Add("  template: `<div ext-shell>{{ displayPrice }}</div>`");
        lines.Add("})");
        lines.Add("export class PriceComponent {");
        lines.Add("  displayPrice = 42;");
        lines.Add("}");
        return string.Join(lineTerminator, lines);
    }

    private static async Task AssertScalarAliasRejectedAsync(string componentSource)
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
                componentSource);

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
            Directory.Delete(externalPackageRoot, recursive: true);
        }
    }

    private static string CreateTestRoot() =>
        Path.Combine(
            Path.GetTempPath(),
            "pkc-r710-semicolonless-import-indirection-tests",
            Guid.NewGuid().ToString("N"));

    private static string CreateExternalPackageRoot() =>
        Path.Combine(
            Path.GetTempPath(),
            "pkc-r710-semicolonless-import-package-tests",
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

    private static void LinkExternalPackage(
        string appRoot,
        string externalPackageRoot)
    {
        var linkedPackageRoot = Path.Combine(appRoot, "node_modules", "@vendor", "ui");
        Directory.CreateDirectory(Path.GetDirectoryName(linkedPackageRoot)!);
        Directory.CreateSymbolicLink(linkedPackageRoot, externalPackageRoot);
    }
}
