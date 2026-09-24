using System.Text;
using Pkc.Core;
using Pkc.Core.Discovery;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class ScanPlanRegressionTests
{
    private const string Api = "dotnet:src/App.Api/App.Api.csproj";
    private const string Worker = "dotnet:src/App.Worker/App.Worker.csproj";
    private const string Shared = "dotnet:src/App.Shared/App.Shared.csproj";
    private const string Orphan = "dotnet:src/App.Orphan/App.Orphan.csproj";
    private const string Tests = "dotnet:tests/App.Tests/App.Tests.csproj";
    private const string Web = "angular:frontend/web/angular.json#web";

    [Fact]
    public async Task Every_enumerated_file_is_planned_exactly_once_with_role_mode_scanners_and_coverage()
    {
        using var fixture = await PlanFixture.CreateAsync();
        var (profile, plan) = Plan(fixture.Root);

        var planned = plan.Scopes.SelectMany(scope => scope.Files).ToArray();
        Assert.Equal(profile.Files.OrderBy(file => file, StringComparer.Ordinal), planned.OrderBy(file => file, StringComparer.Ordinal));
        Assert.Equal(planned.Length, planned.Distinct(StringComparer.Ordinal).Count());
        Assert.All(plan.Scopes, scope => Assert.Equal(scope.Files.Count, scope.FileCount));

        var shared = ScopeOf(plan, "src/App.Shared/Money.cs");
        Assert.Equal(ScanMode.DeepScan, shared.ScanMode);
        Assert.Equal(new[] { "csharp-semantic" }, shared.Scanners);
        Assert.Equal(PlanCoverage.Semantic, shared.Coverage);
        Assert.Equal(new[] { Shared }, shared.Components);

        var component = ScopeOf(plan, "frontend/web/src/app/orders.component.ts");
        Assert.Equal(new[] { "frontend-semantic" }, component.Scanners);
        Assert.Equal(new[] { Web }, component.Components);

        var spec = ScopeOf(plan, "frontend/web/src/app/orders.component.spec.ts");
        Assert.Equal(ScanMode.TestEvidence, spec.ScanMode);
        Assert.Empty(spec.Scanners);
        Assert.Equal(PlanCoverage.TestEvidence, spec.Coverage);
        Assert.Equal("src/**/*.spec.ts", spec.Pattern);

        var test = ScopeOf(plan, "tests/App.Tests/ApiTests.cs");
        Assert.Equal(PlanCoverage.TestEvidence, test.Coverage);
        Assert.Empty(test.Scanners);

        var infrastructure = ScopeOf(plan, "deploy/Dockerfile");
        Assert.Equal(ScanMode.LightIndex, infrastructure.ScanMode);
        Assert.Equal(PlanCoverage.Indexed, infrastructure.Coverage);

        var docs = ScopeOf(plan, "docs/readme.md");
        Assert.Equal(ScanMode.DeepScan, docs.ScanMode);
        Assert.Equal(PlanCoverage.NotAnalyzable, docs.Coverage);

        var loose = ScopeOf(plan, "legacy/checkout.js");
        Assert.Equal(SourceRole.Unknown, loose.Role);
        Assert.Equal(PlanCoverage.Semantic, loose.Coverage);
        Assert.Empty(loose.Components);

        Assert.All(plan.Scopes.Where(scope => scope.ScanMode != ScanMode.DeepScan || scope.Role != SourceRole.Unknown),
            scope => Assert.NotEmpty(scope.Evidence));
    }

    [Fact]
    public async Task Exclusions_are_explicit_and_never_planned_for_scanning()
    {
        using var fixture = await PlanFixture.CreateAsync();
        var (_, plan) = Plan(fixture.Root);

        Assert.Equal(
            new[] { ".git", "frontend/web/node_modules", "src/App.Api/bin", "src/App.Api/obj" },
            plan.Exclusions.Select(exclusion => exclusion.Path).ToArray());
        Assert.All(plan.Exclusions, exclusion => Assert.NotEmpty(exclusion.Evidence));
        Assert.DoesNotContain(plan.Scopes.SelectMany(scope => scope.Files), file =>
            plan.Exclusions.Any(exclusion => file == exclusion.Path || file.StartsWith(exclusion.Path + "/", StringComparison.Ordinal)));
    }

    [Fact]
    public async Task Application_waves_follow_ownership_and_keep_unowned_tests_and_unattributed_scopes()
    {
        using var fixture = await PlanFixture.CreateAsync();
        var (_, plan) = Plan(fixture.Root);

        Assert.Equal(
            new[] { $"host:{Web}", $"host:{Api}", $"host:{Worker}", "unowned", "test", "unattributed" },
            plan.Waves.Select(wave => wave.Id).ToArray());

        var api = Wave(plan, $"host:{Api}");
        Assert.Equal(new[] { Api, Shared }, api.Components);
        var worker = Wave(plan, $"host:{Worker}");
        Assert.Equal(new[] { Shared, Worker }, worker.Components);
        Assert.Equal(new[] { Orphan }, Wave(plan, "unowned").Components);
        Assert.Equal(new[] { Tests }, Wave(plan, "test").Components);
        Assert.Empty(Wave(plan, "unattributed").Components);

        // Every semantic scope belongs to at least one wave; UNKNOWN and unattributed scopes are not dropped.
        var waveScopes = plan.Waves.SelectMany(wave => wave.Scopes).ToHashSet(StringComparer.Ordinal);
        Assert.All(plan.Scopes.Where(scope => scope.Coverage == PlanCoverage.Semantic),
            scope => Assert.Contains(scope.Id, waveScopes));
        Assert.Contains(ScopeOf(plan, "legacy/checkout.js").Id, Wave(plan, "unattributed").Scopes);
        Assert.Contains(ScopeOf(plan, "src/App.Orphan/Unused.cs").Id, Wave(plan, "unowned").Scopes);

        // Test scopes are never part of a production wave.
        var testScope = ScopeOf(plan, "tests/App.Tests/ApiTests.cs").Id;
        Assert.All(plan.Waves.Where(wave => wave.Kind == "host"), wave => Assert.DoesNotContain(testScope, wave.Scopes));
        var testEvidenceScopes = plan.Scopes.Where(scope => scope.Coverage == PlanCoverage.TestEvidence).Select(scope => scope.Id).ToArray();
        Assert.Contains(ScopeOf(plan, "frontend/web/src/app/orders.component.spec.ts").Id, testEvidenceScopes);
        Assert.All(plan.Waves.Where(wave => wave.Kind != "test"),
            wave => Assert.DoesNotContain(wave.Scopes, scope => testEvidenceScopes.Contains(scope)));

        // Program.cs + OrdersEndpoint.cs (host) + Money.cs (owned shared library).
        Assert.Equal(3, api.SemanticFiles);
        Assert.Equal(plan.Summary.SemanticFiles, plan.Scopes.Where(scope => scope.Coverage == PlanCoverage.Semantic).Sum(scope => scope.FileCount));
    }

    [Fact]
    public async Task Plan_is_byte_stable_privacy_safe_and_its_fingerprint_detects_manifest_changes()
    {
        var first = await PlanFixture.CreateAsync(reverseCreationOrder: false);
        var second = await PlanFixture.CreateAsync(reverseCreationOrder: true);
        try
        {
            var firstJson = ScanPlanSerializer.Serialize(Plan(first.Root).Plan);
            Assert.Equal(firstJson, ScanPlanSerializer.Serialize(Plan(first.Root).Plan));
            Assert.Equal(firstJson, ScanPlanSerializer.Serialize(Plan(second.Root).Plan));
            Assert.DoesNotContain(first.Root, firstJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(first.Root.Replace('\\', '/'), firstJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(PlanFixture.SourceMarker, firstJson, StringComparison.Ordinal);
            Assert.DoesNotContain(PlanFixture.PrivateConfigValue, firstJson, StringComparison.Ordinal);
            Assert.StartsWith("sha256:", Plan(first.Root).Plan.InputFingerprint, StringComparison.Ordinal);

            var before = Plan(first.Root).Plan.InputFingerprint;
            await File.AppendAllTextAsync(Path.Combine(first.Root, "src", "App.Shared", "App.Shared.csproj"), "\n<!-- changed -->\n");
            Assert.NotEqual(before, Plan(first.Root).Plan.InputFingerprint);

            // Source body edits outside bounded reads do not claim to be tracked, and do not change the plan.
            var unchanged = Plan(second.Root).Plan.InputFingerprint;
            await File.AppendAllTextAsync(Path.Combine(second.Root, "src", "App.Shared", "Money.cs"), "// edit\n");
            Assert.Equal(unchanged, Plan(second.Root).Plan.InputFingerprint);
        }
        finally
        {
            first.Dispose();
            second.Dispose();
        }
    }

    [Fact]
    public async Task Pipeline_persists_the_plan_before_semantic_stages_run()
    {
        using var fixture = await PlanFixture.CreateAsync();
        var planPath = Path.Combine(fixture.Root, ".pkc", "discovery", "scan-plan.json");
        var observed = new List<string>();

        var result = await new DiscoveryFirstScanPipeline().RunAsync(fixture.Root,
        [
            new SemanticScanStage("csharp", (state, _) =>
            {
                observed.Add($"{File.Exists(planPath)}:{state.Plan.Scopes.Count > 0}:{state.PlanPath == planPath}");
                return Task.FromResult(new FactDocument("test", [], []));
            })
        ]);

        Assert.Equal(new[] { "True:True:True" }, observed);
        Assert.Equal(ScanPlanSerializer.Serialize(result.State.Plan), await File.ReadAllTextAsync(planPath));
    }

    private static (RepositoryProfile Profile, ScanPlan Plan) Plan(string root)
    {
        var profile = new RepositoryDiscovery().Discover(root);
        return (profile, ScanPlanner.Build(root, profile));
    }

    private static ScanScope ScopeOf(ScanPlan plan, string file) =>
        Assert.Single(plan.Scopes, scope => scope.Files.Contains(file));

    private static ScanWave Wave(ScanPlan plan, string id) =>
        Assert.Single(plan.Waves, wave => wave.Id == id);

    private sealed class PlanFixture : IDisposable
    {
        public const string SourceMarker = "PRIVATE_SETTLEMENT_MARKER";
        public const string PrivateConfigValue = "Password=plan-secret";

        private PlanFixture(string root) => Root = root;

        public string Root { get; }

        public static async Task<PlanFixture> CreateAsync(bool reverseCreationOrder = false)
        {
            var root = Path.Combine(Path.GetTempPath(), "pkc-scan-plan", Guid.NewGuid().ToString("N"), "repo");
            Directory.CreateDirectory(root);
            var files = Files().ToList();
            if (reverseCreationOrder)
            {
                files.Reverse();
            }

            foreach (var (path, content) in files)
            {
                var fullPath = Path.Combine(root, path.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
                await File.WriteAllTextAsync(fullPath, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            }

            return new PlanFixture(root);
        }

        public void Dispose()
        {
            var parent = Directory.GetParent(Root)!.FullName;
            if (Directory.Exists(parent))
            {
                Directory.Delete(parent, recursive: true);
            }
        }

        private static IEnumerable<(string Path, string Content)> Files()
        {
            yield return (".git/HEAD", "ref: refs/heads/main\n");
            yield return ("src/App.Api/App.Api.csproj", """
                <Project Sdk="Microsoft.NET.Sdk.Web">
                  <ItemGroup>
                    <ProjectReference Include="..\App.Shared\App.Shared.csproj" />
                  </ItemGroup>
                </Project>
                """);
            yield return ("src/App.Api/Program.cs", $"// {SourceMarker}\nvar app = WebApplication.Create(); app.Run();\n");
            yield return ("src/App.Api/OrdersEndpoint.cs", "public static class OrdersEndpoint {}\n");
            yield return ("src/App.Api/appsettings.json", $$"""{ "Db": "{{PrivateConfigValue}}" }""");
            yield return ("src/App.Api/bin/App.Api.dll", "MZ");
            yield return ("src/App.Api/obj/project.assets.json", "{}");
            yield return ("src/App.Worker/App.Worker.csproj", """
                <Project Sdk="Microsoft.NET.Sdk.Worker">
                  <ItemGroup>
                    <ProjectReference Include="..\App.Shared\App.Shared.csproj" />
                  </ItemGroup>
                </Project>
                """);
            yield return ("src/App.Worker/Worker.cs", "public sealed class Worker {}\n");
            yield return ("src/App.Shared/App.Shared.csproj", """<Project Sdk="Microsoft.NET.Sdk" />""");
            yield return ("src/App.Shared/Money.cs", "public readonly record struct Money(decimal Amount);\n");
            yield return ("src/App.Orphan/App.Orphan.csproj", """<Project Sdk="Microsoft.NET.Sdk" />""");
            yield return ("src/App.Orphan/Unused.cs", "public static class Unused {}\n");
            yield return ("tests/App.Tests/App.Tests.csproj", """
                <Project Sdk="Microsoft.NET.Sdk">
                  <ItemGroup>
                    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
                    <ProjectReference Include="..\..\src\App.Api\App.Api.csproj" />
                  </ItemGroup>
                </Project>
                """);
            yield return ("tests/App.Tests/ApiTests.cs", "public sealed class ApiTests {}\n");
            yield return ("frontend/web/angular.json", """
                {
                  "version": 1,
                  "projects": {
                    "web": {
                      "projectType": "application",
                      "root": "",
                      "sourceRoot": "src",
                      "architect": { "test": { "options": { "tsConfig": "tsconfig.spec.json" } } }
                    }
                  }
                }
                """);
            yield return ("frontend/web/package.json", """{ "name": "web", "private": true }""");
            yield return ("frontend/web/tsconfig.spec.json", """{ "include": ["src/**/*.spec.ts"] }""");
            yield return ("frontend/web/src/main.ts", "bootstrapApplication(AppComponent);\n");
            yield return ("frontend/web/src/app/orders.component.ts", "export class OrdersComponent {}\n");
            yield return ("frontend/web/src/app/orders.component.html", "<p>{{ total }}</p>\n");
            yield return ("frontend/web/src/app/orders.component.spec.ts", "describe('orders', () => {});\n");
            yield return ("frontend/web/node_modules/rxjs/index.js", "module.exports = {};");
            yield return ("legacy/checkout.js", "function checkout() {}\n");
            yield return ("docs/readme.md", "# Readme\n");
            yield return ("deploy/Dockerfile", "FROM mcr.microsoft.com/dotnet/aspnet:8.0\n");
        }
    }
}
