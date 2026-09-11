using Pkc.Frontend;
using Xunit;

namespace Pkc.Frontend.Tests;

public sealed class AngularPageLoadSemanticsTests
{
    [Fact]
    public async Task Scan_links_component_initial_load_through_reload_to_api_call()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-angular-load-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "angular.json"), "{}");
            await File.WriteAllTextAsync(Path.Combine(root, "catalog.component.ts"), """
                @Component({ template: `<h1>Catalog</h1>` })
                export class CatalogComponent {
                  private readonly api = inject(ApiService);
                  ngOnInit() { this.reload(); }
                  reload() { this.api.getCards().subscribe(cards => this.cards = cards); }
                }
                """);
            await File.WriteAllTextAsync(Path.Combine(root, "api.service.ts"), """
                export class ApiService {
                  getCards() { return this.http.get<Card[]>('/api/cards'); }
                }
                """);
            await File.WriteAllTextAsync(Path.Combine(root, "app.routes.ts"), """
                export const routes = [{ path: 'catalog', component: CatalogComponent }];
                """);

            var document = await new FrontendScanner().ScanAsync(root);
            var screen = Assert.Single(document.Facts, fact => fact.Kind == "ui-screen" && fact.Name == "CatalogComponent");
            var apiCall = Assert.Single(document.Facts, fact => fact.Kind == "ui-api-call" && fact.Metadata["routeKey"] == "/api/cards");

            Assert.Contains("getCards", screen.Metadata["loadMethods"], StringComparison.Ordinal);
            Assert.Contains(document.Relations, relation =>
                relation.FromFactId == screen.Id && relation.Kind == "loads-api" && relation.Target == apiCall.Id);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
