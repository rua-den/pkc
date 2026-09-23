using Pkc.Core;
using Pkc.Frontend;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class AngularUiValueLineageKnowledgeTests
{
    [Fact]
    public async Task Synthesizer_renders_complete_dialog_form_display_lineage_chain()
    {
        var source = new SourceLocation("frontend/edit-user.component.ts", 10, 10);
        var endpoint = new EvidenceFact(
            "endpoint:update",
            "endpoint",
            "Update",
            "UsersController",
            new SourceLocation("backend/UsersController.cs", 20, 30),
            [],
            new Dictionary<string, string>
            {
                ["httpMethod"] = "PATCH",
                ["fullRoute"] = "/api/users/{id}"
            });
        var first = new EvidenceFact(
            "lineage:data-to-control",
            "value-transfer",
            "data-to-control",
            "EditUserDialogComponent",
            source,
            [],
            new Dictionary<string, string>
            {
                ["lineageDomain"] = "angular-ui",
                ["componentIdentity"] = "frontend/edit-user.component.ts#EditUserDialogComponent",
                ["controlIdentity"] = "EditUserDialogComponent.form.email",
                ["sourceOccurrence"] = "MAT_DIALOG_DATA.data.email",
                ["targetOccurrence"] = "EditUserDialogComponent.form.email",
                ["mechanism"] = "copy",
                ["temporalSemantics"] = "snapshot"
            });
        var second = new EvidenceFact(
            "lineage:control-to-display",
            "value-transfer",
            "control-to-display",
            "EditUserDialogComponent",
            new SourceLocation("frontend/edit-user.component.html", 1, 1),
            [],
            new Dictionary<string, string>
            {
                ["lineageDomain"] = "angular-ui",
                ["componentIdentity"] = "frontend/edit-user.component.ts#EditUserDialogComponent",
                ["controlIdentity"] = "EditUserDialogComponent.form.email",
                ["sourceOccurrence"] = "EditUserDialogComponent.form.email",
                ["targetOccurrence"] = "EditUserDialogComponent.displayed.email",
                ["predecessorTransferFactId"] = first.Id,
                ["mechanism"] = "form-control-binding",
                ["temporalSemantics"] = "dynamic"
            });
        var candidate = new FeatureCandidate(
            "feature:users:update",
            "Users Update",
            "Users",
            endpoint.Id,
            ["backend-code", "frontend-static"],
            [],
            [endpoint, first, second],
            []);

        var knowledge = await new EvidenceAwareKnowledgeSynthesizer().SynthesizeAsync(candidate);
        var markdown = new MarkdownKnowledgeRenderer().Render(knowledge);

        Assert.Contains(
            "Proven UI value lineage chain: `MAT_DIALOG_DATA.data.email` → `EditUserDialogComponent.form.email` → `EditUserDialogComponent.displayed.email`",
            markdown,
            StringComparison.Ordinal);

        var discontinuousMetadata = new Dictionary<string, string>(second.Metadata, StringComparer.Ordinal)
        {
            ["sourceOccurrence"] = "OtherComponent.form.email"
        };
        var discontinuous = second with { Metadata = discontinuousMetadata };
        var discontinuousKnowledge = await new EvidenceAwareKnowledgeSynthesizer().SynthesizeAsync(
            candidate with { Facts = [endpoint, first, discontinuous] });
        Assert.DoesNotContain(
            discontinuousKnowledge.ValueLineage,
            item => item.Contains("Proven UI value lineage chain", StringComparison.Ordinal));

        var mismatchedMetadata = new Dictionary<string, string>(second.Metadata, StringComparer.Ordinal)
        {
            ["componentIdentity"] = "frontend/other.component.ts#OtherComponent"
        };
        var mismatched = second with { Metadata = mismatchedMetadata };
        var mismatchedKnowledge = await new EvidenceAwareKnowledgeSynthesizer().SynthesizeAsync(
            candidate with { Facts = [endpoint, first, mismatched] });
        Assert.DoesNotContain(
            mismatchedKnowledge.ValueLineage,
            item => item.Contains("Proven UI value lineage chain", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Cross_stack_builder_attaches_only_exact_component_lineage_to_endpoint_candidate()
    {
        var backendSource = new SourceLocation("backend/UsersController.cs", 20, 30);
        var frontendSource = new SourceLocation("frontend/edit-user.component.ts", 10, 10);
        var endpoint = new EvidenceFact(
            "endpoint:update",
            "endpoint",
            "Update",
            "UsersController",
            backendSource,
            [],
            new Dictionary<string, string>
            {
                ["httpMethod"] = "PATCH",
                ["fullRoute"] = "/api/users/{id}"
            });
        var api = new EvidenceFact(
            "api:update",
            "ui-api-call",
            "PATCH /api/users/${id}",
            "onSubmit",
            frontendSource,
            [],
            new Dictionary<string, string>
            {
                ["httpMethod"] = "PATCH",
                ["routeKey"] = "/api/users/{param}"
            });
        var screen = new EvidenceFact(
            "screen:edit-user",
            "ui-screen",
            "EditUserDialogComponent",
            null,
            frontendSource,
            [],
            new Dictionary<string, string>());
        var action = new EvidenceFact(
            "action:submit",
            "ui-action",
            "button",
            "EditUserDialogComponent",
            frontendSource,
            [],
            new Dictionary<string, string> { ["handler"] = "onSubmit" });
        var exact = UiLineageFact(
            "lineage:exact",
            frontendSource,
            "frontend/edit-user.component.ts#EditUserDialogComponent");
        var exactDisplay = new EvidenceFact(
            "lineage:exact-display",
            "value-transfer",
            "lineage:exact-display",
            "EditUserDialogComponent",
            new SourceLocation("frontend/edit-user.component.html", 1, 1),
            [],
            new Dictionary<string, string>
            {
                ["lineageDomain"] = "angular-ui",
                ["controlIdentity"] = "EditUserDialogComponent.form.email",
                ["componentIdentity"] = "frontend/edit-user.component.ts#EditUserDialogComponent",
                ["sourceOccurrence"] = "EditUserDialogComponent.form.email",
                ["targetOccurrence"] = "EditUserDialogComponent.displayed.email",
                ["predecessorTransferFactId"] = exact.Id,
                ["mechanism"] = "form-control-binding",
                ["temporalSemantics"] = "dynamic"
            });
        var collision = UiLineageFact(
            "lineage:collision",
            new SourceLocation("frontend/other.component.ts", 10, 10),
            "frontend/other.component.ts#EditUserDialogComponent");
        var document = new FactDocument(
            "0.4.6",
            [endpoint, api, screen, action, exact, exactDisplay, collision],
            [new EvidenceRelation(action.Id, "triggers-api", api.Id, frontendSource)]);

        var candidate = Assert.Single(new CrossStackFeatureCandidateBuilder().Build(document).Candidates);
        Assert.Contains(candidate.Facts, fact => fact.Id == exact.Id);
        Assert.DoesNotContain(candidate.Facts, fact => fact.Id == collision.Id);

        var knowledge = await new EvidenceAwareKnowledgeSynthesizer().SynthesizeAsync(candidate);
        var markdown = new MarkdownKnowledgeRenderer().Render(knowledge);
        Assert.Contains("Proven UI value lineage chain", markdown, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Frontend_scanner_external_template_reaches_builder_and_markdown_lineage()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-angular-external-template-lineage", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "angular.json"), "{}");
            var componentPath = Path.Combine(root, "edit-user.component.ts");
            await File.WriteAllTextAsync(
                componentPath,
                """
                import { Component, inject } from '@angular/core';
                import { HttpClient } from '@angular/common/http';
                import { FormBuilder } from '@angular/forms';
                import { MAT_DIALOG_DATA } from '@angular/material/dialog';
                interface User { id: string; email: string; }
                @Component({ templateUrl: './edit-user.component.html' })
                export class EditUserDialogComponent {
                  private data: User = inject(MAT_DIALOG_DATA);
                  private fb = inject(FormBuilder);
                  private http = inject(HttpClient);
                  form = this.fb.group({ email: [this.data.email] });
                  onSubmit() { return this.http.patch(`/api/users/${this.data.id}`, this.form.getRawValue()); }
                }
                """);
            await File.WriteAllTextAsync(
                Path.Combine(root, "edit-user.component.html"),
                """
                <form [formGroup]="form">
                  <input formControlName="email" />
                  <button (click)="onSubmit()">Save</button>
                </form>
                """);

            var frontend = await new FrontendScanner().ScanAsync(root);
            var endpoint = new EvidenceFact(
                "endpoint:update",
                "endpoint",
                "Update",
                "UsersController",
                new SourceLocation("backend/UsersController.cs", 20, 30),
                [],
                new Dictionary<string, string>
                {
                    ["httpMethod"] = "PATCH",
                    ["fullRoute"] = "/api/users/{id}"
                });
            var document = new FactDocument(
                "0.4.6",
                frontend.Facts.Concat([endpoint]).ToArray(),
                frontend.Relations);
            var candidate = Assert.Single(new CrossStackFeatureCandidateBuilder().Build(document).Candidates);
            Assert.Contains(candidate.Facts, fact =>
                fact.Kind == "value-transfer" &&
                fact.Source.Path == "edit-user.component.html");

            var knowledge = await new EvidenceAwareKnowledgeSynthesizer().SynthesizeAsync(candidate);
            var markdown = new MarkdownKnowledgeRenderer().Render(knowledge);
            Assert.Contains("Proven UI value lineage chain", markdown, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static EvidenceFact UiLineageFact(string id, SourceLocation source, string componentIdentity) =>
        new(
            id,
            "value-transfer",
            id,
            "EditUserDialogComponent",
            source,
            [],
            new Dictionary<string, string>
            {
                ["lineageDomain"] = "angular-ui",
                ["controlIdentity"] = "EditUserDialogComponent.form.email",
                ["componentIdentity"] = componentIdentity,
                ["sourceOccurrence"] = "MAT_DIALOG_DATA.data.email",
                ["targetOccurrence"] = "EditUserDialogComponent.form.email",
                ["mechanism"] = "copy",
                ["temporalSemantics"] = "snapshot"
            });
}
