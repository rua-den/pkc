using Pkc.Frontend;
using Xunit;

namespace Pkc.Frontend.Tests;

public sealed class FrontendRepositoryScannerTests
{
    [Fact]
    public async Task ScanAsync_extracts_route_action_permission_and_api_call()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-frontend-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "WorkPlayDetail.tsx"), Source);

            var scanner = new FrontendRepositoryScanner();
            var result = await scanner.ScanAsync(root);

            Assert.Equal("0.3.0-frontend", result.SchemaVersion);
            Assert.Contains(result.Facts, fact => fact.Kind == "ui-screen" && fact.Name == "WorkPlayDetailPage");
            Assert.Contains(result.Facts, fact =>
                fact.Kind == "ui-route" &&
                fact.Metadata.TryGetValue("path", out var path) && path == "/workplays/:id");
            Assert.Contains(result.Facts, fact =>
                fact.Kind == "ui-action" &&
                fact.Name == "Complete" &&
                fact.Metadata.TryGetValue("handler", out var handler) && handler == "handleComplete" &&
                fact.Metadata.TryGetValue("permission", out var permission) && permission == "ManageWorkPlay");
            Assert.Contains(result.Facts, fact =>
                fact.Kind == "ui-api-call" &&
                fact.Container == "handleComplete" &&
                fact.Metadata.TryGetValue("httpMethod", out var method) && method == "POST" &&
                fact.Metadata.TryGetValue("routeKey", out var routeKey) && routeKey == "/api/workplays/{param}/complete");
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
