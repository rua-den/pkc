using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class PortableKnowledgePackRendererTests
{
    [Fact]
    public void RenderInstructions_defines_vendor_neutral_reading_and_uncertainty_contract()
    {
        var content = new PortableKnowledgePackRenderer().RenderInstructions();

        Assert.Contains("Start with `index.md`", content, StringComparison.Ordinal);
        Assert.Contains("files under `features/`", content, StringComparison.Ordinal);
        Assert.Contains("files under `workflows/`", content, StringComparison.Ordinal);
        Assert.Contains("authority: code-observed", content, StringComparison.Ordinal);
        Assert.Contains("Prefer an explicit unknown over an unsupported inference", content, StringComparison.Ordinal);
    }

    [Fact]
    public void RenderBundle_preserves_hierarchy_and_file_boundaries()
    {
        var renderer = new PortableKnowledgePackRenderer();
        var files = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["knowledge/workflows/run/post-api-run.md"] = "# Run workflow\nworkflow detail",
            ["knowledge/features/run/operations.md"] = "# Run feature\nfeature detail",
            ["knowledge/index.md"] = "# Index\nindex detail",
            [PortableKnowledgePackRenderer.InstructionsRelativePath] = renderer.RenderInstructions()
        };

        var content = renderer.RenderBundle(files);

        var instructionsPosition = content.IndexOf("PKC_FILE: knowledge/AI_INSTRUCTIONS.md", StringComparison.Ordinal);
        var indexPosition = content.IndexOf("PKC_FILE: knowledge/index.md", StringComparison.Ordinal);
        var featurePosition = content.IndexOf("PKC_FILE: knowledge/features/run/operations.md", StringComparison.Ordinal);
        var workflowPosition = content.IndexOf("PKC_FILE: knowledge/workflows/run/post-api-run.md", StringComparison.Ordinal);

        Assert.True(instructionsPosition >= 0);
        Assert.True(indexPosition > instructionsPosition);
        Assert.True(featurePosition > indexPosition);
        Assert.True(workflowPosition > featurePosition);
        Assert.Contains("workflow detail", content, StringComparison.Ordinal);
    }
}
