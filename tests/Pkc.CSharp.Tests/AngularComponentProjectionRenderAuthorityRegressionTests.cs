using Pkc.Core;
using Pkc.Frontend;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class AngularComponentProjectionRenderAuthorityRegressionTests
{
    [Fact]
    public async Task Component_element_without_proven_projection_does_not_create_render_or_visibility_authority()
    {
        var facts = await ScanAsync(
            """
            import { Component } from '@angular/core';

            @Component({
              selector: 'app-shell',
              template: `<span>Shell</span>`
            })
            export class ShellComponent {}
            """,
            """
            import { Component } from '@angular/core';
            import { ShellComponent } from './shell.component';

            @Component({
              selector: 'app-price',
              imports: [ShellComponent],
              template: `
                @if (isAllowed) {
                  <app-shell>{{ displayPrice }}</app-shell>
                }
              `
            })
            export class PriceComponent {
              isAllowed = true;
              displayPrice = 42;
            }
            """);

        AssertNoAuthoritativeRenderOrVisibility(facts, "displayPrice");
    }

    [Fact]
    public async Task Attribute_selector_component_without_proven_projection_does_not_create_render_or_visibility_authority()
    {
        var facts = await ScanAsync(
            """
            import { Component } from '@angular/core';

            @Component({
              selector: 'div[app-shell]',
              template: `<span>Shell</span>`
            })
            export class ShellComponent {}
            """,
            """
            import { Component } from '@angular/core';
            import { ShellComponent } from './shell.component';

            @Component({
              selector: 'app-price',
              imports: [ShellComponent],
              template: `
                @if (isAllowed) {
                  <div app-shell>{{ displayPrice }}</div>
                }
              `
            })
            export class PriceComponent {
              isAllowed = true;
              displayPrice = 42;
            }
            """);

        AssertNoAuthoritativeRenderOrVisibility(facts, "displayPrice");
    }

    [Fact]
    public async Task Closed_projection_boundary_before_plain_html_preserves_later_supported_render()
    {
        var facts = await ScanAsync(
            """
            import { Component } from '@angular/core';

            @Component({
              selector: 'app-shell',
              template: `<span>Shell</span>`
            })
            export class ShellComponent {}
            """,
            """
            import { Component } from '@angular/core';
            import { ShellComponent } from './shell.component';

            @Component({
              selector: 'app-price',
              imports: [ShellComponent],
              template: `
                <app-shell>{{ projectedPrice }}</app-shell>
                @if (isAllowed) {
                  <strong>{{ displayPrice }}</strong>
                }
              `
            })
            export class PriceComponent {
              isAllowed = true;
              projectedPrice = 7;
              displayPrice = 42;
            }
            """);

        AssertNoAuthoritativeRenderOrVisibility(facts, "projectedPrice");
        Assert.Contains(facts.Facts, fact =>
            fact.Kind == "ui-member-render" &&
            fact.Metadata.GetValueOrDefault("member") == "displayPrice" &&
            fact.Metadata.ContainsKey("renderAuthority"));
        Assert.Contains(facts.Facts, fact =>
            fact.Kind == "ui-member-visibility" &&
            fact.Metadata.GetValueOrDefault("member") == "displayPrice" &&
            fact.Metadata.GetValueOrDefault("condition") == "isAllowed");
    }

    [Fact]
    public async Task External_attribute_component_declaration_without_projection_does_not_create_authority()
    {
        var facts = await ScanAsync(
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
            """,
            """
            import { Component } from '@angular/core';

            @Component({
              selector: 'unused-shell',
              template: `<span>unused</span>`
            })
            export class UnusedComponent {}
            """,
            new Dictionary<string, string>
            {
                ["node_modules/@vendor/ui/index.d.ts"] =
                    """
                    import * as i0 from '@angular/core';
                    export declare class ExternalShell {
                      static ɵcmp: i0.ɵɵComponentDeclaration<ExternalShell, "div[ext-shell]", never, {}, {}, never, never, true, never>;
                    }
                    export declare class ExternalDirective {
                      static ɵdir: i0.ɵɵDirectiveDeclaration<ExternalDirective, "div[ext-shell]", never, {}, {}, never, never, true>;
                    }
                    """
            });

        AssertNoAuthoritativeRenderOrVisibility(facts, "displayPrice");
    }

    [Fact]
    public async Task External_directive_declaration_does_not_suppress_text_and_later_html_remains_supported()
    {
        var facts = await ScanAsync(
            """
            import { Component } from '@angular/core';
            import { ExternalDirective } from '@vendor/ui';

            @Component({
              selector: 'app-price',
              imports: [ExternalDirective],
              template: `
                <div ext-directive>{{ directiveText }}</div>
                <strong>{{ displayPrice }}</strong>
              `
            })
            export class PriceComponent {
              directiveText = 'directive';
              displayPrice = 42;
            }
            """,
            """
            import { Component } from '@angular/core';

            @Component({
              selector: 'unused-shell',
              template: `<span>unused</span>`
            })
            export class UnusedComponent {}
            """,
            new Dictionary<string, string>
            {
                ["node_modules/@vendor/ui/index.d.ts"] =
                    """
                    import * as i0 from '@angular/core';
                    export declare class ExternalDirective {
                      static ɵdir: i0.ɵɵDirectiveDeclaration<ExternalDirective, "div[ext-directive]", never, {}, {}, never, never, true>;
                    }
                    """
            });

        Assert.Contains(facts.Facts, fact =>
            fact.Kind == "ui-member-render" &&
            fact.Metadata.GetValueOrDefault("member") == "directiveText" &&
            fact.Metadata.ContainsKey("renderAuthority"));
        Assert.Contains(facts.Facts, fact =>
            fact.Kind == "ui-member-render" &&
            fact.Metadata.GetValueOrDefault("member") == "displayPrice" &&
            fact.Metadata.ContainsKey("renderAuthority"));
    }

    [Fact]
    public async Task External_class_and_combined_component_declarations_are_projection_boundaries()
    {
        var facts = await ScanAsync(
            """
            import { Component } from '@angular/core';
            import { ExternalClassShell, ExternalCombinedShell } from '@vendor/ui';

            @Component({
              selector: 'app-price',
              imports: [ExternalClassShell, ExternalCombinedShell],
              template: `
                <div class="ext-class">{{ classPrice }}</div>
                <div ext-combined class="ext-combined">{{ combinedPrice }}</div>
              `
            })
            export class PriceComponent {
              classPrice = 7;
              combinedPrice = 42;
            }
            """,
            """
            import { Component } from '@angular/core';

            @Component({
              selector: 'unused-shell',
              template: `<span>unused</span>`
            })
            export class UnusedComponent {}
            """,
            new Dictionary<string, string>
            {
                ["node_modules/@vendor/ui/index.d.ts"] =
                    """
                    import * as i0 from '@angular/core';
                    export declare class ExternalClassShell {
                      static ɵcmp: i0.ɵɵComponentDeclaration<ExternalClassShell, "div.ext-class", never, {}, {}, never, never, true, never>;
                    }
                    export declare class ExternalCombinedShell {
                      static ɵcmp: i0.ɵɵComponentDeclaration<ExternalCombinedShell, "div[ext-combined].ext-combined", never, {}, {}, never, never, true, never>;
                    }
                    """
            });

        AssertNoAuthoritativeRenderOrVisibility(facts, "classPrice");
        AssertNoAuthoritativeRenderOrVisibility(facts, "combinedPrice");
    }

    [Fact]
    public async Task External_quoted_value_component_selector_is_projection_boundary()
    {
        var facts = await ScanAsync(
            """
            import { Component } from '@angular/core';
            import { ExternalShell } from '@vendor/ui';

            @Component({
              selector: 'app-price',
              imports: [ExternalShell],
              template: `<div ext-shell="active">{{ displayPrice }}</div>`
            })
            export class PriceComponent {
              displayPrice = 42;
            }
            """,
            """
            import { Component } from '@angular/core';
            @Component({ selector: 'unused-shell', template: `<span>unused</span>` })
            export class UnusedComponent {}
            """,
            new Dictionary<string, string>
            {
                ["node_modules/@vendor/ui/index.d.ts"] =
                    """
                    import * as i0 from '@angular/core';
                    export declare class ExternalShell {
                      static ɵcmp: i0.ɵɵComponentDeclaration<ExternalShell, "div[ext-shell=\"active\"]", never, {}, {}, never, never, true, never>;
                    }
                    """
            });

        AssertNoAuthoritativeRenderOrVisibility(facts, "displayPrice");
    }

    [Fact]
    public async Task Malformed_external_component_declaration_does_not_bridge_to_later_declaration()
    {
        var facts = await ScanAsync(
            """
            import { Component } from '@angular/core';
            import { ExternalShell } from '@vendor/ui';

            @Component({
              selector: 'app-price',
              imports: [ExternalShell],
              template: `<div later-shell>{{ displayPrice }}</div>`
            })
            export class PriceComponent {
              displayPrice = 42;
            }
            """,
            """
            import { Component } from '@angular/core';
            @Component({ selector: 'unused-shell', template: `<span>unused</span>` })
            export class UnusedComponent {}
            """,
            new Dictionary<string, string>
            {
                ["node_modules/@vendor/ui/index.d.ts"] =
                    """
                    import * as i0 from '@angular/core';
                    export declare class Broken {
                      static ɵcmp: i0.ɵɵComponentDeclaration<Broken, "div[broken], never>;
                    }
                    export declare class Later {
                      static ɵcmp: i0.ɵɵComponentDeclaration<Later, "div[later-shell]", never, {}, {}, never, never, true, never>;
                    }
                    """
            });

        AssertNoAuthoritativeRenderOrVisibility(facts, "displayPrice");
    }

    [Fact]
    public async Task Unimported_external_component_selector_does_not_affect_product_html()
    {
        var facts = await ScanAsync(
            """
            import { Component } from '@angular/core';

            @Component({
              selector: 'app-price',
              template: `<div ext-shell>{{ displayPrice }}</div>`
            })
            export class PriceComponent {
              displayPrice = 42;
            }
            """,
            """
            import { Component } from '@angular/core';
            @Component({ selector: 'unused-shell', template: `<span>unused</span>` })
            export class UnusedComponent {}
            """,
            new Dictionary<string, string>
            {
                ["node_modules/@vendor/ui/index.d.ts"] =
                    """
                    import * as i0 from '@angular/core';
                    export declare class ExternalShell {
                      static ɵcmp: i0.ɵɵComponentDeclaration<ExternalShell, 'div[ext-shell]', never, {}, {}, never, never, true, never>;
                    }
                    """
            });

        Assert.Contains(facts.Facts, fact =>
            fact.Kind == "ui-member-render" &&
            fact.Metadata.GetValueOrDefault("member") == "displayPrice" &&
            fact.Metadata.ContainsKey("renderAuthority"));
    }

    private static void AssertNoAuthoritativeRenderOrVisibility(FactDocument facts, string member)
    {
        Assert.DoesNotContain(facts.Facts, fact =>
            fact.Kind == "ui-member-render" &&
            fact.Metadata.GetValueOrDefault("member") == member &&
            fact.Metadata.ContainsKey("renderAuthority"));
        Assert.DoesNotContain(facts.Facts, fact =>
            fact.Kind == "ui-member-visibility" &&
            fact.Metadata.GetValueOrDefault("member") == member);
    }

    private static async Task<FactDocument> ScanAsync(
        string shellComponentSource,
        string priceComponentSource,
        IReadOnlyDictionary<string, string>? additionalFiles = null)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pkc-r710-component-projection-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "angular.json"), "{}");
            await File.WriteAllTextAsync(
                Path.Combine(root, "package.json"),
                "{\"dependencies\":{\"@angular/core\":\"22.0.0\"}}");
            await File.WriteAllTextAsync(
                Path.Combine(root, "shell.component.ts"),
                shellComponentSource);
            await File.WriteAllTextAsync(
                Path.Combine(root, "price.component.ts"),
                priceComponentSource);
            if (additionalFiles is not null)
            {
                foreach (var file in additionalFiles)
                {
                    var path = Path.Combine(root, file.Key.Replace('/', Path.DirectorySeparatorChar));
                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                    await File.WriteAllTextAsync(path, file.Value);
                }
            }
            return await new FrontendScanner().ScanAsync(root);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
