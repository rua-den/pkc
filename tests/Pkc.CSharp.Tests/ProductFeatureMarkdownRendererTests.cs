using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class ProductFeatureMarkdownRendererTests
{
    [Fact]
    public void RenderIndex_orients_ai_with_grounded_surface_and_conditional_behavior()
    {
        var document = new ProductFeatureDocument(
            "0.4.0",
            [
                Feature(
                    "Run",
                    "operations",
                    "Run Operations",
                    [Workflow("feature:run:post-api-run", "Run POST /api/run", ["POST /api/run"], ["Authorization required: RequireAuthorization"]),
                     Workflow("feature:run:post-internal-dev-run", "Run POST /internal/dev/run", ["POST /internal/dev/run"], [])],
                    ["POST /internal/dev/run: Condition observed: `endpoint is registered only when developmentRunEndpointEnabled`."]),
                Feature(
                    "Projects",
                    "discovery",
                    "Projects Discovery",
                    [Workflow("feature:projects:get-api-projects", "Projects GET /api/projects", ["GET /api/projects"], ["Authorization required: RequireAuthorization"])],
                    [])
            ]);

        var markdown = new ProductFeatureMarkdownRenderer().RenderIndex(document);

        Assert.Contains("## Observed capability surface", markdown, StringComparison.Ordinal);
        Assert.Contains("**Run** — 2 observed workflows", markdown, StringComparison.Ordinal);
        Assert.Contains("`POST /api/run`", markdown, StringComparison.Ordinal);
        Assert.Contains("`POST /internal/dev/run`", markdown, StringComparison.Ordinal);
        Assert.Contains("authorization/permission evidence present", markdown, StringComparison.Ordinal);
        Assert.Contains("**Projects** — 1 observed workflow", markdown, StringComparison.Ordinal);
        Assert.Contains("## Conditional or environment-dependent surface", markdown, StringComparison.Ordinal);
        Assert.Contains("developmentRunEndpointEnabled", markdown, StringComparison.Ordinal);
        Assert.Contains("without inventing product intent or an unsupported user journey", markdown, StringComparison.Ordinal);
    }

    private static ProductFeature Feature(
        string area,
        string category,
        string title,
        IReadOnlyList<ProductWorkflowReference> workflows,
        IReadOnlyList<string> rules) =>
        new(
            $"product-feature:{area.ToLowerInvariant()}:{category}",
            title,
            area,
            category,
            "code-observed",
            ["backend-code"],
            "Observed feature.",
            workflows,
            workflows.SelectMany(workflow => workflow.Permissions).Distinct(StringComparer.Ordinal).ToArray(),
            rules,
            ["Product intent has not been analyzed yet."]);

    private static ProductWorkflowReference Workflow(
        string id,
        string title,
        IReadOnlyList<string> entryPoints,
        IReadOnlyList<string> permissions) =>
        new(
            id,
            title,
            $"knowledge/workflows/{id.Split(':')[1]}/{id.Split(':')[2]}.md",
            ["backend-code"],
            [],
            entryPoints,
            permissions,
            [],
            [],
            [],
            []);
}
