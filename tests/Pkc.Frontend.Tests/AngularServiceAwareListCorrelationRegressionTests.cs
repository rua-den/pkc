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

    [Fact]
    public async Task Same_service_class_name_in_different_modules_does_not_cross_link_list_flow()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-angular-module-service-correlation", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(Path.Combine(root, "catalog"));
        Directory.CreateDirectory(Path.Combine(root, "admin"));

        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(root, "package.json"),
                "{\"dependencies\":{\"@angular/core\":\"22.0.0\"}}");
            await File.WriteAllTextAsync(Path.Combine(root, "catalog.component.ts"), SameNamedComponentSource);
            await File.WriteAllTextAsync(Path.Combine(root, "catalog", "cards-api.ts"), SameNamedCatalogApiSource);
            await File.WriteAllTextAsync(Path.Combine(root, "admin", "cards-api.ts"), SameNamedAdminApiSource);

            var frontend = await new AngularFrontendAdapter().ScanAsync(root);
            var catalogEndpoint = Endpoint("backend:catalog-module", "CardsController", "api/cards");
            var adminEndpoint = Endpoint("backend:admin-module", "AdminCardsController", "api/admin/cards");

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

    [Fact]
    public async Task Inactive_import_like_text_does_not_override_active_service_module()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-angular-inactive-import-correlation", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(Path.Combine(root, "catalog"));
        Directory.CreateDirectory(Path.Combine(root, "admin"));

        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(root, "package.json"),
                "{\"dependencies\":{\"@angular/core\":\"22.0.0\"}}");
            await File.WriteAllTextAsync(Path.Combine(root, "catalog.component.ts"), InactiveImportComponentSource);
            await File.WriteAllTextAsync(Path.Combine(root, "catalog", "cards-api.ts"), SameNamedCatalogApiSource);
            await File.WriteAllTextAsync(Path.Combine(root, "admin", "cards-api.ts"), SameNamedAdminApiSource);

            var frontend = await new AngularFrontendAdapter().ScanAsync(root);
            var catalogEndpoint = Endpoint("backend:catalog-inactive-import", "CardsController", "api/cards");
            var adminEndpoint = Endpoint("backend:admin-inactive-import", "AdminCardsController", "api/admin/cards");

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

    private const string SameNamedComponentSource = """
        import { Component, inject } from '@angular/core';
        import { CardsApi } from './catalog/cards-api';

        @Component({
          selector: 'app-catalog',
          template: `
            @for (card of cards; track card.id) {
              <article>{{ card.name }}</article>
            }
          `
        })
        export class CatalogComponent {
          private readonly api = inject(CardsApi);
          cards = [];

          reload() {
            this.api.getCards().subscribe(cards => this.cards = cards);
          }
        }
        """;

    private const string InactiveImportComponentSource = """
        import { Component, inject } from '@angular/core';
        import { CardsApi } from './catalog/cards-api';

        // Historical note only; this is not an active import:
        // import { CardsApi } from './admin/cards-api';
        /*
        import { CardsApi } from './admin/cards-api';
        */

        @Component({
          selector: 'app-catalog',
          template: `
            @for (card of cards; track card.id) {
              <article>{{ card.name }}</article>
            }
          `
        })
        export class CatalogComponent {
          private readonly stringNote = "import { CardsApi } from './admin/cards-api';";
          private readonly templateNote = `
        import { CardsApi } from './admin/cards-api';
          `;
          private readonly api = inject(CardsApi);
          cards = [];

          reload() {
            this.api.getCards().subscribe(cards => this.cards = cards);
          }
        }
        """;

    private const string SameNamedCatalogApiSource = """
        export class CardsApi {
          private readonly http: any;
          getCards() { return this.http.get('/api/cards'); }
        }
        """;

    private const string SameNamedAdminApiSource = """
        export class CardsApi {
          private readonly http: any;
          getCards() { return this.http.get('/api/admin/cards'); }
        }
        """;
}
