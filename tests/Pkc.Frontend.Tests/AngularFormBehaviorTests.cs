using Pkc.Frontend;
using Xunit;

namespace Pkc.Frontend.Tests;

public sealed class AngularFormBehaviorTests
{
    [Fact]
    public async Task Scan_preserves_field_options_validation_visibility_and_request_binding()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pkc-angular-form-behavior-tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "angular.json"), "{}");

            await File.WriteAllTextAsync(
                Path.Combine(root, "service-config.component.ts"),
                """
                @Component({
                  template: `
                    <form>
                      <select formControlName="serviceType" required>
                        <option value="CSP">CSP</option>
                        <option value="NCE">NCE</option>
                      </select>

                      @if (form.value.serviceType === 'CSP') {
                        <input formControlName="msSubscriptionId">
                      }

                      <button (click)="save()">Save</button>
                    </form>
                  `
                })
                export class ServiceConfigComponent {
                  private readonly http = inject(HttpClient);
                  private readonly fb = inject(FormBuilder);

                  form = this.fb.group({
                    serviceType: ['', Validators.required],
                    msSubscriptionId: ['']
                  });

                  onTypeChanged() {
                    if (this.form.value.serviceType === 'CSP') {
                      this.form.controls.msSubscriptionId.setValidators(Validators.required);
                    }
                  }

                  save() {
                    const request = {
                      type: this.form.value.serviceType,
                      microsoftSubscriptionId: this.form.value.msSubscriptionId
                    };
                    return this.http.post<ServiceRequest>('/api/services', request);
                  }
                }
                """);

            var document = await new FrontendScanner().ScanAsync(root);

            var serviceType = Assert.Single(
                document.Facts,
                fact => fact.Kind == "ui-field" && fact.Metadata["field"] == "serviceType");
            Assert.Equal("select", serviceType.Metadata["element"]);
            Assert.Equal("medium", serviceType.Metadata["analysisConfidence"]);

            Assert.Contains(
                document.Facts,
                fact =>
                    fact.Kind == "ui-field-option" &&
                    fact.Metadata["field"] == "serviceType" &&
                    fact.Metadata["value"] == "CSP");
            Assert.Contains(
                document.Facts,
                fact =>
                    fact.Kind == "ui-field-option" &&
                    fact.Metadata["field"] == "serviceType" &&
                    fact.Metadata["value"] == "NCE");

            Assert.Contains(
                document.Facts,
                fact =>
                    fact.Kind == "ui-field-validation" &&
                    fact.Metadata["field"] == "serviceType" &&
                    fact.Metadata["behavior"] == "required" &&
                    !fact.Metadata.ContainsKey("condition"));

            Assert.Contains(
                document.Facts,
                fact =>
                    fact.Kind == "ui-field-visibility" &&
                    fact.Metadata["field"] == "msSubscriptionId" &&
                    fact.Metadata["condition"].Contains("serviceType === 'CSP'", StringComparison.Ordinal));

            Assert.Contains(
                document.Facts,
                fact =>
                    fact.Kind == "ui-field-validation" &&
                    fact.Metadata["field"] == "msSubscriptionId" &&
                    fact.Metadata["behavior"] == "required" &&
                    fact.Metadata["condition"].Contains("serviceType === 'CSP'", StringComparison.Ordinal));

            Assert.Contains(
                document.Facts,
                fact =>
                    fact.Kind == "ui-field-binding" &&
                    fact.Metadata["field"] == "msSubscriptionId" &&
                    fact.Metadata["requestField"] == "microsoftSubscriptionId" &&
                    fact.Container == "save");

            Assert.Contains(
                document.Facts,
                fact =>
                    fact.Kind == "ui-api-call" &&
                    fact.Metadata["routeKey"] == "/api/services" &&
                    fact.Container == "save");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Scan_proves_dialog_data_to_form_control_to_displayed_email_lineage()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pkc-angular-form-lineage-tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "angular.json"), "{}");
            await File.WriteAllTextAsync(
                Path.Combine(root, "edit-user-dialog.component.ts"),
                """
                import { Component, inject } from '@angular/core';
                import { FormBuilder, Validators } from '@angular/forms';
                import { MAT_DIALOG_DATA } from '@angular/material/dialog';

                interface User { email: string; }

                @Component({
                  template: `
                    <form [formGroup]="form">
                      <input formControlName="email" type="email" />
                    </form>
                  `
                })
                export class EditUserDialogComponent {
                  private data: User = inject(MAT_DIALOG_DATA);
                  private fb = inject(FormBuilder);

                  form = this.fb.nonNullable.group({
                    email: [this.data.email, [Validators.required]]
                  });
                }
                """);

            var document = await new FrontendScanner().ScanAsync(root);

            Assert.Contains(
                document.Facts,
                fact => fact.Kind == "value-transfer" &&
                        fact.Metadata.GetValueOrDefault("sourceOccurrence") == "MAT_DIALOG_DATA.data.email" &&
                        fact.Metadata.GetValueOrDefault("targetOccurrence") == "EditUserDialogComponent.form.email" &&
                        fact.Metadata.GetValueOrDefault("mechanism") == "copy");
            Assert.Contains(
                document.Facts,
                fact => fact.Kind == "value-transfer" &&
                        fact.Metadata.GetValueOrDefault("sourceOccurrence") == "EditUserDialogComponent.form.email" &&
                        fact.Metadata.GetValueOrDefault("targetOccurrence") == "EditUserDialogComponent.displayed.email" &&
                        fact.Metadata.GetValueOrDefault("mechanism") == "form-control-binding");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Scan_fails_closed_when_dialog_data_ownership_is_ambiguous()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-angular-form-lineage-collision-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "angular.json"), "{}");
            await File.WriteAllTextAsync(
                Path.Combine(root, "ambiguous.component.ts"),
                """
                import { Component, inject } from '@angular/core';
                import { FormBuilder } from '@angular/forms';
                import { MAT_DIALOG_DATA } from '@angular/material/dialog';

                interface User { email: string; }

                @Component({
                  template: `<input formControlName="email" />`
                })
                export class AmbiguousComponent {
                  private data: User = inject(MAT_DIALOG_DATA);
                  private otherData: User = inject(MAT_DIALOG_DATA);
                  private fb = inject(FormBuilder);
                  form = this.fb.group({ email: [this.data.email] });
                }
                """);

            var document = await new FrontendScanner().ScanAsync(root);

            Assert.DoesNotContain(document.Facts, fact => fact.Kind == "value-transfer");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Scan_fails_closed_when_field_is_owned_by_a_sibling_form()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-angular-form-lineage-sibling-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "angular.json"), "{}");
            await File.WriteAllTextAsync(
                Path.Combine(root, "sibling-form.component.ts"),
                """
                import { Component, inject } from '@angular/core';
                import { FormBuilder } from '@angular/forms';
                import { MAT_DIALOG_DATA } from '@angular/material/dialog';

                interface User { email: string; }

                @Component({
                  template: `
                    <form [formGroup]="form"></form>
                    <input formControlName="email" />
                  `
                })
                export class SiblingFormComponent {
                  private data: User = inject(MAT_DIALOG_DATA);
                  private fb = inject(FormBuilder);
                  form = this.fb.group({ email: [this.data.email] });
                }
                """);

            var document = await new FrontendScanner().ScanAsync(root);

            Assert.DoesNotContain(document.Facts, fact => fact.Kind == "value-transfer");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Scan_does_not_cross_attribute_lineage_between_components_in_one_file()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-angular-form-lineage-component-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "angular.json"), "{}");
            await File.WriteAllTextAsync(
                Path.Combine(root, "two-components.ts"),
                """
                import { Component, inject } from '@angular/core';
                import { FormBuilder } from '@angular/forms';
                import { MAT_DIALOG_DATA } from '@angular/material/dialog';

                interface User { email: string; }

                @Component({ template: `<form [formGroup]="form"><input formControlName="email" /></form>` })
                export class FirstComponent {
                  private data: User = inject(MAT_DIALOG_DATA);
                  private fb = inject(FormBuilder);
                  form = this.fb.group({ email: [this.data.email] });
                }

                @Component({ template: `<form [formGroup]="form"><input formControlName="email" /></form>` })
                export class SecondComponent {
                  private fb = inject(FormBuilder);
                  form = this.fb.group({ email: [''] });
                }
                """);

            var document = await new FrontendScanner().ScanAsync(root);

            var lineage = document.Facts.Where(fact => fact.Kind == "value-transfer").ToArray();
            Assert.NotEmpty(lineage);
            Assert.All(lineage, fact => Assert.Equal("FirstComponent", fact.Container));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Scan_fails_closed_for_shadow_tokens_without_exact_named_imports()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-angular-form-lineage-import-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "angular.json"), "{}");
            await File.WriteAllTextAsync(
                Path.Combine(root, "shadowed.component.ts"),
                """
                import { Component } from '@angular/core';
                interface User { email: string; }
                const MAT_DIALOG_DATA = Symbol('local');
                const FormBuilder = class { group(value: unknown) { return value; } };
                const inject = (_token: unknown): any => ({ email: 'local' });

                @Component({ template: `<form [formGroup]="form"><input formControlName="email" /></form>` })
                export class ShadowedComponent {
                  private data: User = inject(MAT_DIALOG_DATA);
                  private fb = inject(FormBuilder);
                  form = this.fb.group({ email: [this.data.email] });
                }
                """);

            var document = await new FrontendScanner().ScanAsync(root);

            Assert.DoesNotContain(document.Facts, fact => fact.Kind == "value-transfer");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Scan_fails_closed_when_same_form_control_is_declared_twice()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-angular-form-lineage-duplicate-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "angular.json"), "{}");
            await File.WriteAllTextAsync(
                Path.Combine(root, "duplicate-control.component.ts"),
                """
                import { Component, inject } from '@angular/core';
                import { FormBuilder } from '@angular/forms';
                import { MAT_DIALOG_DATA } from '@angular/material/dialog';

                interface User { email: string; }

                @Component({
                  template: `<form [formGroup]="form"><input formControlName="email" /><input formControlName="email" /></form>`
                })
                export class DuplicateControlComponent {
                  private data: User = inject(MAT_DIALOG_DATA);
                  private fb = inject(FormBuilder);
                  form = this.fb.group({ email: [this.data.email] });
                }
                """);

            var document = await new FrontendScanner().ScanAsync(root);

            Assert.DoesNotContain(document.Facts, fact => fact.Kind == "value-transfer");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData("this.data.email.trim()")]
    [InlineData("this.data.email || ''")]
    public async Task Scan_fails_closed_when_initializer_is_not_a_direct_property(string initializer)
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-angular-form-lineage-expression-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "angular.json"), "{}");
            await File.WriteAllTextAsync(
                Path.Combine(root, "expression.component.ts"),
                """
                import { Component, inject } from '@angular/core';
                import { FormBuilder } from '@angular/forms';
                import { MAT_DIALOG_DATA } from '@angular/material/dialog';
                interface User { email: string; }
                @Component({ template: `<form [formGroup]="form"><input formControlName="email" /></form>` })
                export class ExpressionComponent {
                  private data: User = inject(MAT_DIALOG_DATA);
                  private fb = inject(FormBuilder);
                  form = this.fb.group({ email: [__INIT__] });
                }
                """.Replace("__INIT__", initializer, StringComparison.Ordinal));

            var document = await new FrontendScanner().ScanAsync(root);

            Assert.DoesNotContain(document.Facts, fact => fact.Kind == "value-transfer");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Scan_fails_closed_for_nested_form_group_lineage()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-angular-form-lineage-nested-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "angular.json"), "{}");
            await File.WriteAllTextAsync(
                Path.Combine(root, "nested.component.ts"),
                """
                import { Component, inject } from '@angular/core';
                import { FormBuilder } from '@angular/forms';
                import { MAT_DIALOG_DATA } from '@angular/material/dialog';
                interface User { email: string; }
                @Component({ template: `<form [formGroup]="form"><div formGroupName="profile"><input formControlName="email" /></div></form>` })
                export class NestedComponent {
                  private data: User = inject(MAT_DIALOG_DATA);
                  private fb = inject(FormBuilder);
                  form = this.fb.group({ profile: this.fb.group({ email: [this.data.email] }) });
                }
                """);

            var document = await new FrontendScanner().ScanAsync(root);

            Assert.DoesNotContain(document.Facts, fact => fact.Kind == "value-transfer");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Scan_fails_closed_when_imports_and_members_are_spoofed_inside_template_text()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-angular-form-lineage-lexical-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "angular.json"), "{}");
            await File.WriteAllTextAsync(
                Path.Combine(root, "spoofed.component.ts"),
                """
                import { Component } from '@angular/core';
                interface User { email: string; }
                const spoof = `
                import { inject } from '@angular/core';
                import { FormBuilder } from '@angular/forms';
                import { MAT_DIALOG_DATA } from '@angular/material/dialog';
                `;
                @Component({ template: `<form [formGroup]="form"><input formControlName="email" /></form>` })
                export class SpoofedComponent {
                  private fake = `
                    private data: User = inject(MAT_DIALOG_DATA);
                    private fb = inject(FormBuilder);
                    form = this.fb.group({ email: [this.data.email] });
                  `;
                }
                """);

            var document = await new FrontendScanner().ScanAsync(root);

            Assert.DoesNotContain(document.Facts, fact => fact.Kind == "value-transfer");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
