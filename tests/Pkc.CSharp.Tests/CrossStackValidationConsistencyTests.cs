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

            var workflowRenderer = new MarkdownKnowledgeRenderer();
            var markdown = workflowRenderer.Render(knowledge);
            Assert.Contains("UI field `serviceType` offers option `CSP`", markdown, StringComparison.Ordinal);
            Assert.Contains("UI field `serviceType` offers option `NCE`", markdown, StringComparison.Ordinal);
            Assert.Contains("UI field `msSubscriptionId` is visible when", markdown, StringComparison.Ordinal);
            Assert.Contains("UI field `msSubscriptionId` maps to request field `microsoftSubscriptionId`", markdown, StringComparison.Ordinal);
            Assert.Contains(
                "Validation consistency observed: UI field `msSubscriptionId` and backend field `MicrosoftSubscriptionId`",
                markdown,
                StringComparison.Ordinal);
            Assert.Contains("CSP", markdown, StringComparison.Ordinal);

            var workflowPath = workflowRenderer.GetRelativePath(knowledge);
            var bundle = new PortableKnowledgePackRenderer().RenderBundle(
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [workflowPath] = markdown
                },
                "validation-app");

            Assert.Contains($"PKC_FILE: {workflowPath}", bundle, StringComparison.Ordinal);
            Assert.Contains("UI field `msSubscriptionId` maps to request field `microsoftSubscriptionId`", bundle, StringComparison.Ordinal);
            Assert.Contains(
                "Validation consistency observed: UI field `msSubscriptionId` and backend field `MicrosoftSubscriptionId`",
                bundle,
                StringComparison.Ordinal);
            Assert.Contains("CSP", bundle, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Backend_required_without_ui_required_is_possible_mismatch()
    {
        var (candidates, document) = BuildClassificationScenario(
            uiValidation: null,
            backendValidation: Validation("backend", "backend-field-validation", "microsoftSubscriptionId", method: "Create"));

        var candidate = Assert.Single(new ValidationConsistencyCandidateEnricher().Enrich(candidates, document).Candidates);
        var comparison = Assert.Single(candidate.Facts, fact => fact.Kind == "ui-backend-validation");

        Assert.Equal("possible-mismatch", comparison.Metadata["status"]);
        Assert.Equal("backend-required-ui-required-not-observed", comparison.Metadata["reason"]);

        var knowledge = await new EvidenceAwareKnowledgeSynthesizer().SynthesizeAsync(candidate);
        Assert.Contains(knowledge.Rules, rule => rule.Contains("Possible UI/backend validation mismatch", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Ui_required_without_backend_required_is_unknown()
    {
        var (candidates, document) = BuildClassificationScenario(
            uiValidation: Validation("ui", "ui-field-validation", "msSubscriptionId", component: "ServiceComponent"),
            backendValidation: null);

        var candidate = Assert.Single(new ValidationConsistencyCandidateEnricher().Enrich(candidates, document).Candidates);
        var comparison = Assert.Single(candidate.Facts, fact => fact.Kind == "ui-backend-validation");

        Assert.Equal("unknown", comparison.Metadata["status"]);
        Assert.Equal("ui-required-backend-required-not-observed", comparison.Metadata["reason"]);

        var knowledge = await new EvidenceAwareKnowledgeSynthesizer().SynthesizeAsync(candidate);
        Assert.Contains(knowledge.Rules, rule => rule.Contains("remains unknown", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Different_required_conditions_are_possible_mismatch()
    {
        var (candidates, document) = BuildClassificationScenario(
            uiValidation: Validation(
                "ui",
                "ui-field-validation",
                "msSubscriptionId",
                component: "ServiceComponent",
                condition: "serviceType === 'CSP'"),
            backendValidation: Validation(
                "backend",
                "backend-field-validation",
                "microsoftSubscriptionId",
                method: "Create",
                condition: "request.ServiceType == ServiceType.NCE"));

        var candidate = Assert.Single(new ValidationConsistencyCandidateEnricher().Enrich(candidates, document).Candidates);
        var comparison = Assert.Single(candidate.Facts, fact => fact.Kind == "ui-backend-validation");

        Assert.Equal("possible-mismatch", comparison.Metadata["status"]);
        Assert.Equal("requiredness-condition-differs", comparison.Metadata["reason"]);

        var knowledge = await new EvidenceAwareKnowledgeSynthesizer().SynthesizeAsync(candidate);
        var markdown = new MarkdownKnowledgeRenderer().Render(knowledge);
        Assert.Contains("Possible UI/backend validation mismatch", markdown, StringComparison.Ordinal);
        Assert.Contains("CSP", markdown, StringComparison.Ordinal);
        Assert.Contains("NCE", markdown, StringComparison.Ordinal);
    }

    private static (FeatureCandidateDocument Candidates, FactDocument Document) BuildClassificationScenario(
        EvidenceFact? uiValidation,
        EvidenceFact? backendValidation)
    {
        var endpoint = new EvidenceFact(
            "endpoint",
            "endpoint",
            "Create",
            "ServicesController",
            new SourceLocation("ServicesController.cs", 1, 1),
            [],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["httpMethod"] = "POST",
                ["fullRoute"] = "/api/services",
                ["parameters"] = "CreateServiceRequest request"
            });

        var binding = new EvidenceFact(
            "binding",
            "ui-field-binding",
            "msSubscriptionId->microsoftSubscriptionId",
            "submit",
            new SourceLocation("service.component.ts", 10, 10),
            [],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["field"] = "msSubscriptionId",
                ["requestField"] = "microsoftSubscriptionId",
                ["component"] = "ServiceComponent"
            });

        var candidate = new FeatureCandidate(
            "feature:services:create",
            "Services Create",
            "Services",
            endpoint.Id,
            ["backend-code", "frontend-static"],
            [],
            [endpoint, binding],
            []);

        var facts = new List<EvidenceFact> { endpoint, binding };
        if (uiValidation is not null)
        {
            facts.Add(uiValidation);
        }
        if (backendValidation is not null)
        {
            facts.Add(backendValidation);
        }

        return (
            new FeatureCandidateDocument("test", [candidate]),
            new FactDocument("test", facts, []));
    }

    private static EvidenceFact Validation(
        string id,
        string kind,
        string field,
        string? component = null,
        string? method = null,
        string? condition = null)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["field"] = field,
            ["behavior"] = "required"
        };

        if (!string.IsNullOrWhiteSpace(component))
        {
            metadata["component"] = component;
        }
        if (!string.IsNullOrWhiteSpace(method))
        {
            metadata["method"] = method;
        }
        if (!string.IsNullOrWhiteSpace(condition))
        {
            metadata["condition"] = condition;
        }

        return new EvidenceFact(
            id,
            kind,
            field,
            component ?? method,
            new SourceLocation(kind == "ui-field-validation" ? "service.component.ts" : "ServicesController.cs", 20, 20),
            [],
            metadata);
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
