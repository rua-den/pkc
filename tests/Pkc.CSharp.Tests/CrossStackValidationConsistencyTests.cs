using Pkc.Core;
using Pkc.CSharp;
using Pkc.Frontend;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class CrossStackValidationConsistencyTests
{
    [Fact]
    public async Task Knowledge_correlates_ui_and_backend_requiredness()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pkc-cross-stack-validation-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(root, "ValidationApp.csproj"),
                """
                <Project Sdk="Microsoft.NET.Sdk.Web">
                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                    <Nullable>enable</Nullable>
                  </PropertyGroup>
                </Project>
                """);

            await File.WriteAllTextAsync(
                Path.Combine(root, "ServicesController.cs"),
                """
                using System.ComponentModel.DataAnnotations;
                using Microsoft.AspNetCore.Mvc;

                [ApiController]
                [Route("api/services")]
                public sealed class ServicesController : ControllerBase
                {
                    [HttpPost]
                    public IActionResult Create(CreateServiceRequest request)
                    {
                        if (request.ServiceType == ServiceType.CSP &&
                            string.IsNullOrWhiteSpace(request.MicrosoftSubscriptionId))
                        {
                            return BadRequest("Microsoft Subscription Id is required for CSP.");
                        }

                        return Ok();
                    }
                }

                public enum ServiceType
                {
                    CSP,
                    NCE
                }

                public sealed class CreateServiceRequest
                {
                    [Required]
                    public string ServiceType { get; set; } = string.Empty;

                    public string? MicrosoftSubscriptionId { get; set; }
                }
                """);

            await File.WriteAllTextAsync(Path.Combine(root, "angular.json"), "{}");
            await File.WriteAllTextAsync(
                Path.Combine(root, "service.component.ts"),
                """
                @Component({
                  template: `
                    <select formControlName="serviceType" required>
                      <option value="CSP">CSP</option>
                      <option value="NCE">NCE</option>
                    </select>
                    @if (form.value.serviceType === 'CSP') {
                      <input formControlName="msSubscriptionId" />
                    }
                    <button (click)="submit()">Save</button>
                  `
                })
                export class ServiceComponent {
                  form = this.fb.group({
                    serviceType: ['', Validators.required],
                    msSubscriptionId: ['']
                  });

                  updateValidation() {
                    if (this.form.value.serviceType === 'CSP') {
                      this.form.controls.msSubscriptionId.setValidators([Validators.required]);
                    }
                  }

                  submit() {
                    const request = {
                      serviceType: this.form.value.serviceType,
                      microsoftSubscriptionId: this.form.value.msSubscriptionId
                    };
                    return this.http.post('/api/services', request);
                  }
                }
                """);

            var csharp = await new CSharpEvidenceScanner().ScanAsync(root);
            var frontend = await new FrontendScanner().ScanAsync(root);
            var facts = Merge(csharp, frontend);

            var candidates = new CrossStackFeatureCandidateBuilder().Build(facts);
            candidates = new ValidationConsistencyCandidateEnricher().Enrich(candidates, facts);

            var candidate = Assert.Single(candidates.Candidates);
            var comparisons = candidate.Facts
                .Where(fact => fact.Kind == "ui-backend-validation")
                .ToArray();

            Assert.Contains(
                comparisons,
                fact =>
                    fact.Metadata["uiField"] == "serviceType" &&
                    fact.Metadata["status"] == "consistent");

            Assert.Contains(
                comparisons,
                fact =>
                    fact.Metadata["uiField"] == "msSubscriptionId" &&
                    fact.Metadata["backendField"] == "MicrosoftSubscriptionId" &&
                    fact.Metadata["status"] == "consistent" &&
                    fact.Metadata["reason"] == "matching-conditional-requiredness");

            var knowledge = await new EvidenceAwareKnowledgeSynthesizer().SynthesizeAsync(candidate);

            Assert.Contains(
                knowledge.Rules,
                rule => rule.Contains(
                    "Validation consistency observed: UI field `msSubscriptionId` and backend field `MicrosoftSubscriptionId`",
                    StringComparison.Ordinal));
            Assert.Contains(
                knowledge.Rules,
                rule => rule.Contains("serviceType", StringComparison.OrdinalIgnoreCase) &&
                        rule.Contains("CSP", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static FactDocument Merge(params FactDocument[] documents)
    {
        var facts = documents
            .SelectMany(document => document.Facts)
            .GroupBy(fact => fact.Id, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(fact => fact.Id, StringComparer.Ordinal)
            .ToArray();

        var relations = documents
            .SelectMany(document => document.Relations)
            .GroupBy(
                relation => $"{relation.FromFactId}|{relation.Kind}|{relation.Target}|{relation.Source.Path}|{relation.Source.StartLine}",
                StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
            .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
            .ThenBy(relation => relation.Target, StringComparer.Ordinal)
            .ToArray();

        return new FactDocument("test", facts, relations);
    }
}
