using Pkc.Frontend;
using Xunit;

namespace Pkc.Frontend.Tests;

public sealed class FrontendRepositoryScannerTests
{
    [Fact]
    public async Task ScanAsync_extracts_react_ui_and_links_action_to_api()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-frontend-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "WorkPlayDetail.tsx"), Source);

            var result = await new FrontendScanner().ScanAsync(root);

            Assert.Equal("0.4.1-frontend", result.SchemaVersion);
            Assert.Contains(result.Facts, fact => fact.Kind == "ui-screen" && fact.Name == "WorkPlayDetailPage");
            Assert.Contains(result.Facts, fact =>
                fact.Kind == "ui-route" &&
                fact.Metadata.TryGetValue("path", out var path) && path == "/workplays/:id");

            var action = Assert.Single(result.Facts.Where(fact =>
                fact.Kind == "ui-action" &&
                fact.Name == "Complete" &&
                fact.Metadata.TryGetValue("handler", out var handler) && handler == "handleComplete"));

            var apiCall = Assert.Single(result.Facts.Where(fact =>
                fact.Kind == "ui-api-call" &&
                fact.Container == "handleComplete" &&
                fact.Metadata.TryGetValue("httpMethod", out var method) && method == "POST" &&
                fact.Metadata.TryGetValue("routeKey", out var routeKey) && routeKey == "/api/workplays/{param}/complete"));

            Assert.Equal("ManageWorkPlay", action.Metadata["permission"]);
            Assert.Equal("react-static", action.Metadata["framework"]);
            Assert.Contains(result.Relations, relation =>
                relation.FromFactId == action.Id &&
                relation.Kind == "triggers-api" &&
                relation.Target == apiCall.Id);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private const string Source = """
        export function WorkPlayDetailPage({ id }: { id: string }) {
          async function handleComplete() {
            await fetch(`/api/workplays/${id}/complete`, { method: "POST" });
          }

          return hasPermission("ManageWorkPlay") && (
            <button onClick={handleComplete}>Complete</button>
          );
        }

        export function Routes() {
          return <Route path="/workplays/:id" element={<WorkPlayDetailPage />} />;
        }
        """;
}
