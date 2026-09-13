using Pkc.Frontend;
using Xunit;

namespace Pkc.Frontend.Tests;

public sealed class FrontendProductSourceScopeTests
{
    [Fact]
    public async Task React_scanner_excludes_test_and_spike_sources()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-react-scope-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await WriteAsync(root, "src/Orders.tsx", ReactSource("OrdersPage"));
            await WriteAsync(root, "src/Orders.test.tsx", ReactSource("OrdersTestPage"));
            await WriteAsync(root, "tests/OrdersFixture.tsx", ReactSource("OrdersFixturePage"));
            await WriteAsync(root, "spikes/OrdersSpike.tsx", ReactSource("OrdersSpikePage"));

            var result = await new FrontendScanner().ScanAsync(root);

            Assert.Contains(result.Facts, fact => fact.Source.Path == "src/Orders.tsx");
            Assert.DoesNotContain(result.Facts, fact => IsNonProductSource(fact.Source.Path));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Angular_scanner_excludes_spec_test_and_spike_sources()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-angular-scope-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "angular.json"), "{}");
            await WriteAsync(root, "src/app/orders.component.ts", AngularSource("OrdersComponent"));
            await WriteAsync(root, "src/app/orders.component.spec.ts", AngularSource("OrdersSpecComponent"));
            await WriteAsync(root, "tests/orders.fixture.ts", AngularSource("OrdersFixtureComponent"));
            await WriteAsync(root, "spikes/orders.spike.ts", AngularSource("OrdersSpikeComponent"));

            var result = await new FrontendScanner().ScanAsync(root);

            Assert.Contains(result.Facts, fact => fact.Source.Path == "src/app/orders.component.ts");
            Assert.DoesNotContain(result.Facts, fact => IsNonProductSource(fact.Source.Path));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static async Task WriteAsync(string root, string relativePath, string content)
    {
        var path = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, content);
    }

    private static bool IsNonProductSource(string path) =>
        path.EndsWith(".spec.ts", StringComparison.OrdinalIgnoreCase) ||
        path.EndsWith(".test.tsx", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("tests/", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("spikes/", StringComparison.OrdinalIgnoreCase);

    private static string ReactSource(string component) => $$"""
        export function {{component}}() {
          async function save() {
            await fetch('/api/orders', { method: 'POST' });
          }

          return <button onClick={save}>Save</button>;
        }
        """;

    private static string AngularSource(string component) => $$"""
        @Component({ template: `<button (click)="save()">Save</button>` })
        export class {{component}} {
          save() {
            return this.http.post('/api/orders', {});
          }
        }
        """;
}
