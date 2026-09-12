using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class ProductFeatureCapabilityFlowTests
{
    [Fact]
    public void Product_feature_keeps_cross_component_flow_and_drops_plumbing()
    {
        var workflow = new FeatureKnowledge(
            "feature:run:post-api-run",
            "Run POST /api/run",
            "Run",
            "code-observed",
            ["backend-code"],
            "Observed workflow.",
            [],
            [],
            ["POST /api/run"],
            ["Authorization required"],
            [],
            [],
            [],
            [
                "Run.POST /api/run → App.RunService.RunAsync",
                "App.RunService.RunAsync → App.ProjectContextBuilder.BuildAsync",
                "App.ProjectContextBuilder.BuildAsync → App.MemoryContextBuilder.BuildAsync",
                "App.RunService.RunAsync → Runtime.AgentLoop.RunAsync",
                "Runtime.AgentLoop.RunAsync → Core.IBrain.ThinkAsync",
                "Runtime.AgentLoop.RunAsync → Core.ActionId.New",
                "App.ProjectContextBuilder.BuildAsync → App.ProjectContextBuilder.NormalizeMentionText",
                "App.RunService.RunAsync → Core.ProjectId.ToString"
            ],
            [],
            []);

        var feature = Assert.Single(new ProductFeatureBuilder().Build([workflow]).Features);
        var reference = Assert.Single(feature.Workflows);

        Assert.Contains("Run.POST /api/run → App.RunService.RunAsync", reference.Flow);
        Assert.Contains("App.RunService.RunAsync → App.ProjectContextBuilder.BuildAsync", reference.Flow);
        Assert.Contains("App.ProjectContextBuilder.BuildAsync → App.MemoryContextBuilder.BuildAsync", reference.Flow);
        Assert.Contains("Runtime.AgentLoop.RunAsync → Core.IBrain.ThinkAsync", reference.Flow);

        Assert.DoesNotContain(reference.Flow, item => item.Contains("ActionId.New", StringComparison.Ordinal));
        Assert.DoesNotContain(reference.Flow, item => item.Contains("NormalizeMentionText", StringComparison.Ordinal));
        Assert.DoesNotContain(reference.Flow, item => item.Contains("ProjectId.ToString", StringComparison.Ordinal));

        var markdown = new ProductFeatureMarkdownRenderer().Render(feature);
        Assert.Contains("## Observed capability flow", markdown, StringComparison.Ordinal);
        Assert.Contains("ProjectContextBuilder.BuildAsync", markdown, StringComparison.Ordinal);
        Assert.Contains("MemoryContextBuilder.BuildAsync", markdown, StringComparison.Ordinal);
        Assert.Contains("IBrain.ThinkAsync", markdown, StringComparison.Ordinal);
        Assert.DoesNotContain("ProjectId.ToString", markdown, StringComparison.Ordinal);
    }
}
