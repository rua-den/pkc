using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class ProductFeatureLifecycleGroupingTests
{
    [Theory]
    [InlineData("WorkPlay Start")]
    [InlineData("Delivery Dispatch Delivery")]
    [InlineData("Delivery Mark Delivery Delivered")]
    public void Build_classifies_lifecycle_actions_as_status_management(string title)
    {
        var area = title.StartsWith("WorkPlay", StringComparison.Ordinal) ? "WorkPlay" : "Delivery";
        var workflow = Workflow(area, title);

        var feature = Assert.Single(new ProductFeatureBuilder().Build([workflow]).Features);
        Assert.Equal("status-management", feature.Category);
    }

    [Fact]
    public void Build_does_not_classify_get_deliveries_as_status_management()
    {
        var feature = Assert.Single(new ProductFeatureBuilder().Build([Workflow("Deliveries", "Deliveries Get Deliveries")]).Features);
        Assert.Equal("discovery", feature.Category);
    }

    private static FeatureKnowledge Workflow(string area, string title) =>
        new(
            $"feature:{area.ToLowerInvariant()}:action",
            title,
            area,
            "code-observed",
            ["backend-code"],
            "Observed workflow.",
            [], [], [], [], [], [], [], [], [], []);
}
