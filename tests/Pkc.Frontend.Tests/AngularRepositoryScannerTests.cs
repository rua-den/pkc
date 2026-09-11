using Pkc.Frontend;
using Xunit;

namespace Pkc.Frontend.Tests;

public sealed class AngularRepositoryScannerTests
{
    [Fact]
    public async Task Scan_extracts_angular_routes_actions_permissions_and_http_calls()
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
                  startWorkPlay(id: number) { }
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

            var document = await new AngularRepositoryScanner().ScanAsync(root);

            Assert.Contains(document.Facts, fact => fact.Kind == "ui-screen" && fact.Name == "WorkPlaysComponent");
            Assert.Contains(document.Facts, fact => fact.Kind == "ui-route" && fact.Metadata["path"] == "/workplays");
            Assert.Contains(document.Facts, fact => fact.Kind == "ui-action" && fact.Metadata["handler"] == "startWorkPlay" && fact.Metadata["permission"] == "ManageWorkPlay");
            Assert.Contains(document.Facts, fact => fact.Kind == "ui-api-call" && fact.Metadata["httpMethod"] == "PATCH" && fact.Metadata["routeKey"] == "/api/workplays/{param}/start" && fact.Container == "startWorkPlay");
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }
}
