using System.Text;
using System.Text.Json;
using Pkc.Core;
using Pkc.Core.Discovery;
using Pkc.Frontend;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class ScopedSemanticExecutionRegressionTests
{
    internal const string TestProjectDirectory = "src/Shop.Api.Tests/";
    internal const string DeclaredVendorFile = "src/Shop.Api/wwwroot/lib/vendorkit/vendor-api.ts";
    internal const string UndeclaredVendorFile = "src/Shop.Api/wwwroot/lib/vendorkit/local-patch-api.ts";
    internal const string UnknownVendorFile = "src/Shop.Api/wwwroot/lib/unknownkit/unknown-api.ts";
    internal const string BuildOutputFile = "web/public-out/stale-api.ts";
    internal const string KnowledgeFile = "src/Shop.Api/knowledge/NotesController.cs";
    internal const string FrontendBuildToolFile = "web/build/release-api.ts";

    [Fact]
    public async Task Run_scope_withholds_test_vendor_and_generated_sources_but_keeps_production_and_unknown()
    {
        using var fixture = await ScopeFixture.CreateAsync();

        var scoped = await RunAsync(fixture.Root, scoped: true);
        var backend = scoped.Result.Documents[0];
        var frontend = scoped.Result.Documents[1];

        Assert.Contains(backend.Facts, fact => fact.Kind == "endpoint" && fact.Name == "GetOrders" &&
                                               fact.Source.Path == "src/Shop.Api/OrdersController.cs");
        Assert.DoesNotContain(backend.Facts, fact => fact.Source.Path.StartsWith(TestProjectDirectory, StringComparison.Ordinal));
        Assert.DoesNotContain(backend.Relations, relation => relation.Source.Path.StartsWith(TestProjectDirectory, StringComparison.Ordinal));

        Assert.Contains(ApiCallPaths(frontend), path => path == "web/src/app/orders-api.ts");
        Assert.Contains(ApiCallPaths(frontend), path => path == UnknownVendorFile);
        Assert.Contains(ApiCallPaths(frontend), path => path == UndeclaredVendorFile);
        Assert.DoesNotContain(frontend.Facts, fact => fact.Source.Path == DeclaredVendorFile);
        Assert.DoesNotContain(frontend.Facts, fact => fact.Source.Path == BuildOutputFile);

        // The same scanners without the plan scope still see those files, so the scope is what withholds them.
        var whole = await RunAsync(fixture.Root, scoped: false);
        Assert.Contains(whole.Result.Documents[0].Facts, fact => fact.Source.Path.StartsWith(TestProjectDirectory, StringComparison.Ordinal));
        Assert.Contains(ApiCallPaths(whole.Result.Documents[1]), path => path == DeclaredVendorFile);
        Assert.Contains(ApiCallPaths(whole.Result.Documents[1]), path => path == BuildOutputFile);
    }

    [Fact]
    public async Task Accepted_semantics_are_unchanged_inside_the_selected_scope()
    {
        using var fixture = await ScopeFixture.CreateAsync();
        var scoped = await RunAsync(fixture.Root, scoped: true);
        var whole = await RunAsync(fixture.Root, scoped: false);
        var scope = scoped.Result.State.Scope;

        for (var index = 0; index < 2; index++)
        {
            var expected = whole.Result.Documents[index].Facts.Where(fact => !scope.Withholds(fact.Source.Path)).ToArray();
            var expectedIds = expected.Select(fact => fact.Id).ToHashSet(StringComparer.Ordinal);
            var expectedRelations = whole.Result.Documents[index].Relations
                .Where(relation => expectedIds.Contains(relation.FromFactId) && !scope.Withholds(relation.Source.Path))
                .ToArray();

            Assert.Equal(Json(expected), Json(scoped.Result.Documents[index].Facts));
            Assert.Equal(Json(expectedRelations), Json(scoped.Result.Documents[index].Relations));
        }
    }

    [Fact]
    public async Task Planned_files_withheld_by_scanner_name_scopes_are_reported_and_the_scope_is_only_ambient_inside_stages()
    {
        using var fixture = await ScopeFixture.CreateAsync();
        var scoped = await RunAsync(fixture.Root, scoped: true);

        var csharp = Assert.Single(scoped.Result.Executions, execution => execution.Name == "csharp");
        Assert.True(csharp.Scoped);
        Assert.Equal(ScanPlanner.CSharpScanner, csharp.Scanner);
        Assert.Equal(new[] { KnowledgeFile }, csharp.WithheldByScannerScope);
        Assert.Equal(csharp.PlannedFiles - 1, csharp.ExecutableFiles);

        var frontend = Assert.Single(scoped.Result.Executions, execution => execution.Name == "frontend");
        Assert.Equal(new[] { FrontendBuildToolFile }, frontend.WithheldByScannerScope);

        // Plan-withheld files are not "planned semantic" work at all.
        var planned = scoped.Result.State.Plan.Scopes
            .Where(item => item.Coverage == PlanCoverage.Semantic)
            .SelectMany(item => item.Files)
            .ToHashSet(StringComparer.Ordinal);
        Assert.DoesNotContain(DeclaredVendorFile, planned);
        Assert.DoesNotContain(BuildOutputFile, planned);
        Assert.Contains(UnknownVendorFile, planned);

        Assert.Equal(new[] { "csharp:True", "frontend:True" }, scoped.AmbientInsideStages);
        Assert.Null(SemanticSourceScope.Current);

        var whole = await RunAsync(fixture.Root, scoped: false);
        Assert.Equal(new[] { "csharp:False", "frontend:False" }, whole.AmbientInsideStages);
        Assert.All(whole.Result.Executions, execution => Assert.False(execution.Scoped));
    }

    [Fact]
    public async Task Scope_decisions_follow_the_plan_and_fail_open_for_paths_discovery_never_saw()
    {
        using var fixture = await ScopeFixture.CreateAsync();
        var profile = new RepositoryDiscovery().Discover(fixture.Root);
        var scope = SemanticSourceScope.FromProfile(fixture.Root, profile);

        Assert.True(scope.Withholds("src/Shop.Api.Tests/FakeOrdersController.cs"));
        Assert.True(scope.Withholds("src/Shop.Api.Tests/Shop.Api.Tests.csproj"));
        Assert.True(scope.Withholds("web/src/app/orders-api.spec.ts"));
        Assert.True(scope.Withholds(DeclaredVendorFile));
        Assert.True(scope.Withholds(BuildOutputFile));
        Assert.True(scope.Withholds("web/node_modules/rxjs/index.ts"));
        Assert.True(scope.Withholds("web/node_modules/never-enumerated/deep/file.ts"));
        Assert.True(scope.Withholds(@"SRC\shop.api.tests\FakeOrdersController.cs"));

        Assert.False(scope.Withholds("src/Shop.Api/OrdersController.cs"));
        Assert.False(scope.Withholds(UndeclaredVendorFile));
        Assert.False(scope.Withholds(UnknownVendorFile));
        Assert.False(scope.Withholds(KnowledgeFile));
        Assert.False(scope.Withholds("src/Shop.Api/CreatedAfterDiscovery.cs"));
        Assert.False(scope.Withholds("../outside.cs"));
        Assert.False(scope.Withholds("."));

        Assert.Contains("web/public-out", scope.ExcludedAreas);
        Assert.Contains(DeclaredVendorFile, scope.WithheldFiles);
        Assert.DoesNotContain(UnknownVendorFile, scope.WithheldFiles);

        Assert.False(SemanticSourceScope.Excludes(DeclaredVendorFile));
        using (scope.Enter())
        {
            Assert.True(SemanticSourceScope.Excludes(DeclaredVendorFile));
            Assert.True(SemanticSourceScope.Excludes(fixture.Root, Path.Combine(fixture.Root, "src", "Shop.Api.Tests", "FakeOrdersController.cs")));
            Assert.False(SemanticSourceScope.Excludes(fixture.Root, Path.Combine(fixture.Root, "src", "Shop.Api", "OrdersController.cs")));
        }

        Assert.False(SemanticSourceScope.Excludes(DeclaredVendorFile));
    }

    private static async Task<(DiscoveryFirstScanResult Result, IReadOnlyList<string> AmbientInsideStages)> RunAsync(string root, bool scoped)
    {
        var ambient = new List<string>();
        var result = await new DiscoveryFirstScanPipeline(scoped: scoped).RunAsync(root,
        [
            new SemanticScanStage("csharp", async (_, cancellationToken) =>
            {
                ambient.Add($"csharp:{SemanticSourceScope.Current is not null}");
                return await new CSharpEvidenceScanner().ScanAsync(root, cancellationToken);
            })
            {
                Scanner = ScanPlanner.CSharpScanner,
                InScannerSourceScope = CSharpEvidenceScanner.IsInSourceScope
            },
            new SemanticScanStage("frontend", async (_, cancellationToken) =>
            {
                ambient.Add($"frontend:{SemanticSourceScope.Current is not null}");
                return await new FrontendScanner().ScanAsync(root, cancellationToken);
            })
            {
                Scanner = ScanPlanner.FrontendScanner,
                InScannerSourceScope = FrontendScanner.IsInSourceScope
            }
        ]);
        return (result, ambient);
    }

    private static IEnumerable<string> ApiCallPaths(FactDocument document) =>
        document.Facts.Where(fact => fact.Kind == "ui-api-call").Select(fact => fact.Source.Path);

    private static string Json<T>(IEnumerable<T> items) => JsonSerializer.Serialize(items.ToArray());

    internal sealed class ScopeFixture : IDisposable
    {
        private ScopeFixture(string root) => Root = root;

        public string Root { get; }

        public static async Task<ScopeFixture> CreateAsync()
        {
            var root = Path.Combine(Path.GetTempPath(), "pkc-scoped-execution", Guid.NewGuid().ToString("N"), "repo");
            Directory.CreateDirectory(root);
            foreach (var (path, content) in Files())
            {
                var fullPath = Path.Combine(root, path.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
                await File.WriteAllTextAsync(fullPath, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            }

            return new ScopeFixture(root);
        }

        public void Dispose()
        {
            var parent = Directory.GetParent(Root)!.FullName;
            if (Directory.Exists(parent))
            {
                Directory.Delete(parent, recursive: true);
            }
        }

        private static string Controller(string name, string route, string action) => $$"""
            using Microsoft.AspNetCore.Mvc;

            namespace Shop;

            [ApiController]
            [Route("{{route}}")]
            public sealed class {{name}} : ControllerBase
            {
                [HttpGet]
                public IActionResult {{action}}() => Ok();
            }
            """;

        private static string ApiService(string name, string url) => $$"""
            export class {{name}} {
              private readonly http: any;

              load() {
                return this.http.get('{{url}}');
              }
            }
            """;

        private static IEnumerable<(string Path, string Content)> Files()
        {
            yield return (".git/HEAD", "ref: refs/heads/main\n");

            yield return ("src/Shop.Api/Shop.Api.csproj", """
                <Project Sdk="Microsoft.NET.Sdk.Web">
                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                  </PropertyGroup>
                  <ItemGroup>
                    <ProjectReference Include="..\Shop.Shared\Shop.Shared.csproj" />
                  </ItemGroup>
                </Project>
                """);
            yield return ("src/Shop.Api/OrdersController.cs", Controller("OrdersController", "api/orders", "GetOrders"));
            yield return (KnowledgeFile, Controller("NotesController", "api/notes", "GetNotes"));
            yield return ("src/Shop.Shared/Shop.Shared.csproj", """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                  </PropertyGroup>
                </Project>
                """);
            yield return ("src/Shop.Shared/Money.cs", "namespace Shop;\n\npublic readonly record struct Money(decimal Amount);\n");

            // Test project outside any directory named test/tests: only discovery knows it is test evidence.
            yield return ("src/Shop.Api.Tests/Shop.Api.Tests.csproj", """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                  </PropertyGroup>
                  <ItemGroup>
                    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
                    <ProjectReference Include="..\Shop.Api\Shop.Api.csproj" />
                  </ItemGroup>
                </Project>
                """);
            yield return ("src/Shop.Api.Tests/FakeOrdersController.cs", Controller("FakeOrdersController", "api/test-fakes", "GetFakes"));

            // LibMan: one declared library (plus a locally added file), one without a file list (UNKNOWN).
            yield return ("src/Shop.Api/libman.json", """
                {
                  "version": "1.0",
                  "defaultProvider": "cdnjs",
                  "libraries": [
                    { "library": "vendorkit@2.0.0", "destination": "wwwroot/lib/vendorkit/", "files": [ "vendor-api.ts" ] },
                    { "library": "unknownkit@1.0.0", "destination": "wwwroot/lib/unknownkit/" }
                  ]
                }
                """);
            yield return (DeclaredVendorFile, ApiService("VendorApi", "/api/vendor"));
            yield return (UndeclaredVendorFile, ApiService("LocalPatchApi", "/api/local-patch"));
            yield return (UnknownVendorFile, ApiService("UnknownApi", "/api/unknown"));

            yield return ("web/angular.json", """
                {
                  "version": 1,
                  "projects": {
                    "web": {
                      "projectType": "application",
                      "root": "",
                      "sourceRoot": "src",
                      "architect": {
                        "build": { "options": { "outputPath": "public-out", "tsConfig": "tsconfig.app.json" } },
                        "test": { "options": { "tsConfig": "tsconfig.spec.json" } }
                      }
                    }
                  }
                }
                """);
            yield return ("web/package.json", """{ "name": "web", "private": true, "dependencies": { "@angular/core": "22.0.0" } }""");
            yield return ("web/tsconfig.spec.json", """{ "include": ["src/**/*.spec.ts"] }""");
            yield return ("web/src/app/orders-api.ts", ApiService("OrdersApi", "/api/orders"));
            yield return ("web/src/app/orders-api.spec.ts", ApiService("OrdersApiSpecDouble", "/api/spec-double"));
            yield return (BuildOutputFile, ApiService("StaleApi", "/api/stale"));
            yield return (FrontendBuildToolFile, ApiService("ReleaseApi", "/api/release"));
            yield return ("web/node_modules/rxjs/index.ts", ApiService("RxApi", "/api/rx"));
        }
    }
}
