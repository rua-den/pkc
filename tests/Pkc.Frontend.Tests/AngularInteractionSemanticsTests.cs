using Pkc.Frontend;
using Xunit;

namespace Pkc.Frontend.Tests;

public sealed class AngularInteractionSemanticsTests
{
    [Fact]
    public async Task Scan_links_two_hop_refresh_action_and_preserves_visibility_condition()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pkc-angular-interaction-tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "angular.json"), "{}");

            await File.WriteAllTextAsync(
                Path.Combine(root, "orders.component.ts"),
                """
                @Component({
                  template: `
                    @if (hasPermission('ViewOrders') && mode === 'ready') {
                      <button (click)="reload()">Refresh</button>
                    }
                  `
                })
                export class OrdersComponent {
                  private readonly api = inject(ApiService);
                  mode = 'ready';
                  hasPermission(permission: string) { return permission === 'ViewOrders'; }
                  reload() { this.api.getOrders().subscribe(items => this.orders = items); }
                }
                """);

            await File.WriteAllTextAsync(
                Path.Combine(root, "api.service.ts"),
                """
                export class ApiService {
                  getOrders() { return this.http.get<Order[]>('/api/orders'); }
                }
                """);

            await File.WriteAllTextAsync(
                Path.Combine(root, "app.routes.ts"),
                """
                export const routes = [
                  { path: 'orders', component: OrdersComponent }
                ];
                """);

            var document = await new FrontendScanner().ScanAsync(root);

            var action = Assert.Single(
                document.Facts,
                fact => fact.Kind == "ui-action" && fact.Name == "Refresh");

            Assert.Equal("ViewOrders", action.Metadata["permission"]);
            Assert.Contains(
                "mode === 'ready'",
                action.Metadata["visibilityCondition"],
                StringComparison.Ordinal);

            var apiCall = Assert.Single(
                document.Facts,
                fact =>
                    fact.Kind == "ui-api-call" &&
                    fact.Metadata["routeKey"] == "/api/orders");

            Assert.Contains(
                document.Relations,
                relation =>
                    relation.FromFactId == action.Id &&
                    relation.Kind == "triggers-api" &&
                    relation.Target == apiCall.Id);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
