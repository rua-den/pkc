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
}
