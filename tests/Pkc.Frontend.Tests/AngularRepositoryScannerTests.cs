using Pkc.Frontend;
using Xunit;

namespace Pkc.Frontend.Tests;

public sealed class AngularRepositoryScannerTests
{
    [Fact]
    public async Task Scan_extracts_angular_ui_and_links_component_action_to_service_api()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-angular-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "workplays.component.ts"), """
                @Component({ template: `
                  @if (hasPermission('ManageWorkPlay')) {
                    <button (click)="startWorkPlay(item.id)">Start</button>
                  }
                ` })
                export class WorkPlaysComponent {
                  startWorkPlay(id: number) { return this.api.startWorkPlay(id); }
                }
                """);
            await File.WriteAllTextAsync(Path.Combine(root, "api.service.ts"), """
                export class ApiService {
                  startWorkPlay(id: number) { return this.http.patch<WorkPlay>(`/api/workplays/${id}/start`, {}); }
                }
                """);
            await File.WriteAllTextAsync(Path.Combine(root, "app.routes.ts"), """
                export const routes = [{ path: 'workplays', component: WorkPlaysComponent }];
                """);

            var document = await new FrontendScanner().ScanAsync(root);

            Assert.Equal("0.4.3-frontend", document.SchemaVersion);
            Assert.Contains(document.Facts, fact => fact.Kind == "ui-screen" && fact.Name == "WorkPlaysComponent");
            Assert.Contains(document.Facts, fact => fact.Kind == "ui-route" && fact.Metadata["path"] == "/workplays");

            var action = Assert.Single(document.Facts, fact =>
                fact.Kind == "ui-action" &&
                fact.Metadata["handler"] == "startWorkPlay" &&
                fact.Metadata["permission"] == "ManageWorkPlay");

            var apiCall = Assert.Single(document.Facts, fact =>
                fact.Kind == "ui-api-call" &&
                fact.Metadata["httpMethod"] == "PATCH" &&
                fact.Metadata["routeKey"] == "/api/workplays/{param}/start" &&
                fact.Container == "startWorkPlay");

            Assert.Equal("angular-static", action.Metadata["framework"]);
            Assert.Equal("regex-fallback", action.Metadata["analysisMode"]);
            Assert.Equal("low", action.Metadata["analysisConfidence"]);
            Assert.NotEqual(action.Source.Path, apiCall.Source.Path);
            Assert.Contains(document.Relations, relation =>
                relation.FromFactId == action.Id &&
                relation.Kind == "triggers-api" &&
                relation.Target == apiCall.Id);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }
}
