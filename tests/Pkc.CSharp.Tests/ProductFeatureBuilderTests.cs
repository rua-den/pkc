using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class ProductFeatureBuilderTests
{
    [Fact]
    public void Build_groups_status_workflows_into_one_product_feature()
    {
        var workflows = new[]
        {
            Workflow("feature:workplay:complete", "WorkPlay Complete"),
            Workflow("feature:workplay:cancel", "WorkPlay Cancel"),
            Workflow("feature:workplay:reopen", "WorkPlay Reopen")
        };

        var document = new ProductFeatureBuilder().Build(workflows);
        var feature = Assert.Single(document.Features);

        Assert.Equal("0.4.0", document.SchemaVersion);
        Assert.Equal("product-feature:workplay:status-management", feature.Id);
        Assert.Equal("WorkPlay Status Management", feature.Title);
        Assert.Equal("status-management", feature.Category);
        Assert.Equal(3, feature.Workflows.Count);
        Assert.All(feature.Workflows, item => Assert.StartsWith("knowledge/workflows/workplay/", item.MarkdownPath, StringComparison.Ordinal));
    }

    [Fact]
    public void Build_keeps_different_semantic_groups_separate()
    {
        var workflows = new[]
        {
            Workflow("feature:workplay:complete", "WorkPlay Complete"),
            Workflow("feature:workplay:create", "WorkPlay Create"),
            Workflow("feature:workplay:search", "WorkPlay Search")
        };

        var document = new ProductFeatureBuilder().Build(workflows);

        Assert.Equal(3, document.Features.Count);
        Assert.Contains(document.Features, feature => feature.Category == "status-management");
        Assert.Contains(document.Features, feature => feature.Category == "management");
        Assert.Contains(document.Features, feature => feature.Category == "discovery");
    }

    private static FeatureKnowledge Workflow(string id, string title) =>
        new(
            id,
            title,
            "WorkPlay",
            "code-observed",
            ["backend-code", "frontend-static"],
            "Observed workflow.",
            ["Open route `/workplays/:id`.", $"Click `{title.Replace("WorkPlay ", string.Empty, StringComparison.Ordinal)}`."],
            ["UI sends POST request."],
            ["POST /api/workplays/{id}"],
            ["Policy: ManageWorkPlay"],
            [],
            [],
            [],
            [],
            ["Azure DevOps delivery history and product intent have not been analyzed yet."],
            []);
}
