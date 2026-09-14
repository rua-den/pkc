using Pkc.Core;
using Pkc.Frontend;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.Frontend.Tests;

public sealed class AngularServiceAwareListCorrelationRegressionTests
{
    [Fact]
    public async Task Same_method_name_on_different_services_does_not_cross_link_list_flow()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-angular-service-correlation", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(root, "package.json"),
                "{\"dependencies\":{\"@angular/core\":\"22.0.0\"}}");
            await File.WriteAllTextAsync(Path.Combine(root, "catalog.component.ts"), ComponentSource);
            await File.WriteAllTextAsync(Path.Combine(root, "catalog-api.ts"), CatalogApiSource);
            await File.WriteAllTextAsync(Path.Combine(root, "admin-api.ts"), AdminApiSource);

            var frontend = await new AngularFrontendAdapter().ScanAsync(root);
            var catalogEndpoint = Endpoint("backend:catalog", "CardsController", "api/cards");
            var adminEndpoint = Endpoint("backend:admin", "AdminCardsController", "api/admin/cards");

            var document = new FactDocument(
                "test",
                frontend.Facts.Concat([catalogEndpoint, adminEndpoint]).ToArray(),
                frontend.Relations);

            var candidates = new CrossStackFeatureCandidateBuilder().Build(document).Candidates;
            var catalog = Assert.Single(candidates, candidate => candidate.SeedFactId == catalogEndpoint.Id);
            var admin = Assert.Single(candidates, candidate => candidate.SeedFactId == adminEndpoint.Id);

            Assert.Contains(catalog.Facts, fact => fact.Kind == "ui-result-binding");
            Assert.Contains(catalog.Facts, fact => fact.Kind == "ui-list-render");
            Assert.DoesNotContain(admin.Facts, fact => fact.Kind == "ui-result-binding");
            Assert.DoesNotContain(admin.Facts, fact => fact.Kind == "ui-list-render");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static EvidenceFact Endpoint(string id, string controller, string route) =>
        new(
            id,
            "endpoint",
            "GetCards",
            controller,
            new SourceLocation("Backend.cs", 1, 1),
            [],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["httpMethod"] = "GET",
                ["fullRoute"] = route
            });

    private const string ComponentSource = """
        import { Component, inject } from '@angular/core';
        import { CatalogApi } from './catalog-api';

        @Component({
          selector: 'app-catalog',
          template: `
            @for (card of cards; track card.id) {
              <article>{{ card.name }}</article>
            }
          `
        })
        export class CatalogComponent {
          private readonly catalogApi = inject(CatalogApi);
          cards = [];

          reload() {
            this.catalogApi.getCards().subscribe(cards => this.cards = cards);
          }
        }
        """;

    private const string CatalogApiSource = """
        export class CatalogApi {
          private readonly http: any;
          getCards() { return this.http.get('/api/cards'); }
        }
        """;

    private const string AdminApiSource = """
        export class AdminApi {
          private readonly http: any;
          getCards() { return this.http.get('/api/admin/cards'); }
        }
        """;
}
