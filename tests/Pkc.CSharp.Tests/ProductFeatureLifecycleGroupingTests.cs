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
        var workflow = new FeatureKnowledge(
            $"feature:{area.ToLowerInvariant()}:action",
            title,
            area,
            "code-observed",
            ["backend-code"],
            "Observed workflow.",
            [], [], [], [], [], [], [], [], [], []);

        var feature = Assert.Single(new ProductFeatureBuilder().Build([workflow]).Features);
        Assert.Equal("status-management", feature.Category);
    }
}
