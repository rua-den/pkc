using Pkc.Core;
using Pkc.CSharp;
using Pkc.Frontend;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class BackendValidationProvenanceRegressionTests
{
    [Fact]
    public async Task Missing_validated_parameter_provenance_cannot_be_proven_by_another_endpoint_parameter()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pkc-backend-validation-provenance-tests",
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
                Path.Combine(root, "TargetsController.cs"),
                """
                using Microsoft.AspNetCore.Mvc;

                [ApiController]
                [Route("api/targets")]
                public sealed class TargetsController : ControllerBase
                {
                    private readonly StateTracker _state = new();

                    [HttpPost]
                    public IActionResult Create(CreateTargetRequest request, StateTracker tracker)
                    {
                        if (tracker.Status == Status.Active && this._state.TargetId == null)
                        {
                            return BadRequest();
                        }

                        return Ok();
                    }
                }

                public sealed class CreateTargetRequest
                {
                    public string? TargetId { get; set; }
                }

                public sealed class StateTracker
                {
                    public Status Status { get; set; }
                    public string? TargetId { get; set; }
                }

                public enum Status
                {
                    Active,
                    Inactive
                }
                """);

            await File.WriteAllTextAsync(Path.Combine(root, "angular.json"), "{}");
            await File.WriteAllTextAsync(
                Path.Combine(root, "target.component.ts"),
                """
                @Component({
                  template: `
                    <select formControlName="status">
                      <option value="Active">Active</option>
                      <option value="Inactive">Inactive</option>
                    </select>
                    <input formControlName="targetId" />
                    <button (click)="submit()">Save</button>
                  `
                })
                export class TargetComponent {
                  form = this.fb.group({
                    status: [''],
                    targetId: ['']
                  });

                  updateValidation() {
                    if (this.form.value.status === 'Active') {
                      this.form.controls.targetId.setValidators([Validators.required]);
                    }
                  }

                  submit() {
                    const request = {
                      targetId: this.form.value.targetId
                    };
                    return this.http.post('/api/targets', request);
                  }
                }
                """);

            var csharp = await new CSharpEvidenceScanner().ScanAsync(root);
            var backendValidation = Assert.Single(
                csharp.Facts,
                fact =>
                    fact.Kind == "backend-field-validation" &&
                    fact.Metadata.TryGetValue("field", out var field) &&
                    field == "TargetId" &&
                    fact.Metadata.TryGetValue("method", out var method) &&
                    method == "Create" &&
                    fact.Metadata.TryGetValue("condition", out var condition) &&
                    condition.Contains("tracker.Status", StringComparison.Ordinal));

            Assert.False(backendValidation.Metadata.ContainsKey("parameterName"));

            var frontend = await new FrontendScanner().ScanAsync(root);
            var facts = Merge(csharp, frontend);
            var candidates = new CrossStackFeatureCandidateBuilder().Build(facts);
            candidates = new ValidationConsistencyCandidateEnricher().Enrich(candidates, facts);

            var comparison = candidates.Candidates
                .SelectMany(candidate => candidate.Facts)
                .Single(fact =>
                    fact.Kind == "ui-backend-validation" &&
                    fact.Metadata.TryGetValue("uiField", out var uiField) &&
                    uiField == "targetId" &&
                    fact.Metadata.TryGetValue("backendField", out var backendField) &&
                    backendField == "TargetId");

            Assert.NotEqual("consistent", comparison.Metadata["status"]);
            Assert.NotEqual("high", comparison.Metadata["analysisConfidence"]);
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
