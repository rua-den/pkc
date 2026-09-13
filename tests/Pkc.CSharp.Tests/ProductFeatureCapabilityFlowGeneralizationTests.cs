using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class ProductFeatureCapabilityFlowGeneralizationTests
{
    [Fact]
    public void Capability_flow_prefers_structural_path_over_type_name_suffixes()
    {
        var flow = new List<string>
        {
            "Execute.POST /api/execute → App.ExecutionCoordinator.ExecuteAsync",
            "App.ExecutionCoordinator.ExecuteAsync → Context.ContextProvider.LoadAsync",
            "Context.ContextProvider.LoadAsync → Persistence.ConversationRepository.ReadAsync",
            "App.ExecutionCoordinator.ExecuteAsync → Domain.UseCase.ApplyAsync"
        };

        for (var index = 1; index <= 20; index++)
        {
            flow.Add($"App.ExecutionCoordinator.ExecuteAsync → App.HelperService{index:00}.RunAsync");
        }

        var workflow = new FeatureKnowledge(
            "feature:execute:post-api-execute",
            "Execute POST /api/execute",
            "Execute",
            "code-observed",
            ["backend-code"],
            "Observed workflow.",
            [],
            [],
            ["POST /api/execute"],
            [],
            [],
            [],
            [],
            flow,
            [],
            []);

        var feature = Assert.Single(new ProductFeatureBuilder().Build([workflow]).Features);
        var reference = Assert.Single(feature.Workflows);

        Assert.Contains(
            "App.ExecutionCoordinator.ExecuteAsync → Context.ContextProvider.LoadAsync",
            reference.Flow);
        Assert.Contains(
            "Context.ContextProvider.LoadAsync → Persistence.ConversationRepository.ReadAsync",
            reference.Flow);
        Assert.Contains(
            "App.ExecutionCoordinator.ExecuteAsync → Domain.UseCase.ApplyAsync",
            reference.Flow);
        Assert.True(reference.Flow.Count <= 16);
    }
}
