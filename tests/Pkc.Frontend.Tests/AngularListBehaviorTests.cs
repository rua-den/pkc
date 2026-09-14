using Pkc.Frontend;
using Xunit;

namespace Pkc.Frontend.Tests;

public sealed class AngularListBehaviorTests
{
    [Fact]
    public async Task Scan_exposes_api_result_binding_and_rendered_collection()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-list-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "package.json"), "{\"dependencies\":{\"@angular/core\":\"22.0.0\"}}");
            await File.WriteAllTextAsync(Path.Combine(root, "catalog.component.ts"), Source);

            var document = await new AngularFrontendAdapter().ScanAsync(root);

            var render = Assert.Single(document.Facts, fact => fact.Kind == "ui-list-render");
            Assert.Equal("CatalogComponent", render.Container);
            Assert.Equal("card", render.Metadata["item"]);
            Assert.Equal("cards", render.Metadata["collection"]);

            var binding = Assert.Single(document.Facts, fact => fact.Kind == "ui-result-binding");
            Assert.Equal("CatalogComponent", binding.Container);
            Assert.Equal("getCards", binding.Metadata["apiMethod"]);
            Assert.Equal("cards", binding.Metadata["target"]);

            Assert.Contains(document.Relations, relation =>
                relation.FromFactId == binding.Id &&
                relation.Kind == "feeds-list" &&
                relation.Target == render.Id);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private const string Source = """
        import { Component } from '@angular/core';

        @Component({
          selector: 'app-catalog',
          template: `
            @for (card of cards; track card.id) {
              <article>{{ card.name }}</article>
            }
          `
        })
        export class CatalogComponent {
          cards = [];
          api = new CatalogApi();

          reload() {
            this.api.getCards().subscribe(cards => this.cards = cards);
          }
        }

        class CatalogApi {
          getCards() { return stream; }
        }
        """;
}
