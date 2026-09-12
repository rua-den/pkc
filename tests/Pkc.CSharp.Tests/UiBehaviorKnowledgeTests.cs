using Pkc.Core;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class UiBehaviorKnowledgeTests
{
    [Fact]
    public async Task Synthesize_preserves_field_validation_visibility_options_and_binding()
    {
        var source = new SourceLocation("frontend/service-config.component.ts", 10, 30);
        var endpoint = Fact(
            "endpoint",
            "endpoint",
            "POST /api/services",
            "Services",
            source,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["httpMethod"] = "POST",
                ["fullRoute"] = "/api/services"
            });

        var facts = new EvidenceFact[]
        {
            endpoint,
            Fact("field-service-type", "ui-field", "serviceType", "ServiceConfigComponent", source,
                Metadata(("field", "serviceType"), ("element", "select"))),
            Fact("option-csp", "ui-field-option", "serviceType:CSP", "ServiceConfigComponent", source,
                Metadata(("field", "serviceType"), ("value", "CSP"), ("label", "CSP"))),
            Fact("required-service-type", "ui-field-validation", "serviceType:required", "ServiceConfigComponent", source,
                Metadata(("field", "serviceType"), ("behavior", "required"))),
            Fact("visible-ms-sub", "ui-field-visibility", "msSubscriptionId:visible", "ServiceConfigComponent", source,
                Metadata(("field", "msSubscriptionId"), ("behavior", "visible"), ("condition", "serviceType === 'CSP'"))),
            Fact("required-ms-sub", "ui-field-validation", "msSubscriptionId:required", "ServiceConfigComponent", source,
                Metadata(("field", "msSubscriptionId"), ("behavior", "required"), ("condition", "serviceType === 'CSP'"))),
            Fact("bind-ms-sub", "ui-field-binding", "msSubscriptionId->microsoftSubscriptionId", "save", source,
                Metadata(("field", "msSubscriptionId"), ("requestField", "microsoftSubscriptionId")))
        };

        var candidate = new FeatureCandidate(
            "feature:services:post-api-services",
            "Services POST /api/services",
            "Services",
            endpoint.Id,
            ["backend-code", "frontend-static"],
            [],
            facts,
            []);

        var knowledge = await new EvidenceAwareKnowledgeSynthesizer().SynthesizeAsync(candidate);

        Assert.Contains("UI exposes field `serviceType` as `select`.", knowledge.Rules);
        Assert.Contains("UI field `serviceType` offers option `CSP`.", knowledge.Rules);
        Assert.Contains("UI field `serviceType` is `required`.", knowledge.Rules);
        Assert.Contains("UI field `msSubscriptionId` is visible when `serviceType === 'CSP'`.", knowledge.Rules);
        Assert.Contains("UI field `msSubscriptionId` is `required` when `serviceType === 'CSP'`.", knowledge.Rules);
        Assert.Contains("UI field `msSubscriptionId` maps to request field `microsoftSubscriptionId`.", knowledge.Rules);

        Assert.Contains(knowledge.Evidence, item => item.Kind == "ui-field-validation" && item.FactId == "required-ms-sub");
        Assert.Contains(knowledge.Evidence, item => item.Kind == "ui-field-binding" && item.FactId == "bind-ms-sub");
    }

    private static EvidenceFact Fact(
        string id,
        string kind,
        string name,
        string? container,
        SourceLocation source,
        IReadOnlyDictionary<string, string> metadata) =>
        new(id, kind, name, container, source, [], metadata);

    private static IReadOnlyDictionary<string, string> Metadata(params (string Key, string Value)[] items) =>
        items.ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
}
