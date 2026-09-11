using Pkc.Frontend;
using Xunit;

namespace Pkc.Frontend.Tests;

public sealed class AngularComplexServiceSignatureTests
{
    [Fact]
    public async Task Scan_links_component_action_to_service_method_with_object_shaped_parameter()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-angular-complex-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "catalog.component.ts"), """
                @Component({ template: `<button (click)="createOrder()">Place order</button>` })
                export class CatalogComponent {
                  createOrder() { return this.api.createOrder({ customerName: 'Ash', deliveryAddress: 'Pallet', lines: [] }); }
                }
                """);
            await File.WriteAllTextAsync(Path.Combine(root, "api.service.ts"), """
                export class ApiService {
                  createOrder(request: { customerName: string; deliveryAddress: string; lines: { cardId: number; quantity: number }[] }) {
                    return this.http.post<Order>('/api/orders', request);
                  }
                }
                """);
            await File.WriteAllTextAsync(Path.Combine(root, "app.routes.ts"), """
                export const routes = [{ path: 'catalog', component: CatalogComponent }];
                """);

            var document = await new FrontendScanner().ScanAsync(root);
            var action = Assert.Single(document.Facts, fact => fact.Kind == "ui-action" && fact.Name == "Place order");
            var apiCall = Assert.Single(document.Facts, fact => fact.Kind == "ui-api-call" && fact.Metadata["routeKey"] == "/api/orders");

            Assert.Equal("createOrder", apiCall.Container);
            Assert.Contains(document.Relations, relation =>
                relation.FromFactId == action.Id && relation.Kind == "triggers-api" && relation.Target == apiCall.Id);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
