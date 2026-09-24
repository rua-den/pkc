using System.Text;
using Pkc.Core;
using Pkc.Core.Discovery;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class RepositoryDiscoveryInventoryRegressionTests
{
    private static readonly string[] ExpectedGeneratedOrRestorableAreas =
    [
        "frontend/admin-portal/.angular",
        "frontend/admin-portal/dist/admin-portal",
        "frontend/admin-portal/node_modules",
        "packages/Newtonsoft.Json.13.0.3",
        "src/Backoffice.Web/bin",
        "src/Portal.Api/bin",
        "src/Portal.Api/obj",
        "tests/Portal.Api.Tests/bin",
        "tests/Portal.Api.Tests/obj"
    ];

    // First-party files that live under directory names commonly (and unsafely) treated as disposable.
    private static readonly string[] AmbiguouslyNamedFirstPartyFiles =
    [
        "Content/print.css",
        "Scripts/site-legacy.js",
        "build/build.ps1",
        "dist/legacy-bundle.js",
        "frontend/admin-portal/src/app/plugins/export.ts",
        "frontend/admin-portal/src/app/vendor/chart-wrapper.ts",
        "legacy/js/checkout.js",
        "old/Billing/InvoiceCalculator.cs",
        "packages/InternalTools/Tool.cs",
        "plugins/Reporting/ReportPlugin.cs",
        "src/Backoffice.Web/Content/themes/custom/theme.js",
        "src/Backoffice.Web/Scripts/app/orders.js",
        "src/Shared.Domain/legacy/LegacyPricing.cs",
        "themes/corporate/theme.js",
        "tools/bin/deploy-helper.ps1",
        "vendor/acme-grid/acme-grid.js",
        "vendor/acme-grid/custom-patch.js"
    ];

    [Fact]
    public async Task Inventory_is_deterministic_and_independent_of_checkout_location_and_creation_order()
    {
        var first = await MixedRepositoryFixture.CreateAsync(reverseCreationOrder: false);
        var second = await MixedRepositoryFixture.CreateAsync(reverseCreationOrder: true);
        try
        {
            var discovery = new RepositoryDiscovery();
            var firstJson = RepositoryProfileSerializer.Serialize(discovery.Discover(first.Root));
            var repeatedJson = RepositoryProfileSerializer.Serialize(discovery.Discover(first.Root));
            var relocatedJson = RepositoryProfileSerializer.Serialize(discovery.Discover(second.Root));

            Assert.Equal(firstJson, repeatedJson);
            Assert.Equal(firstJson, relocatedJson);
            Assert.DoesNotContain(first.Root, firstJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(first.Root.Replace('\\', '/'), firstJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("\r\n", firstJson, StringComparison.Ordinal);

            var profile = discovery.Discover(first.Root);
            AssertOrdinalSorted(profile.Areas.Select(area => area.Path));
            AssertOrdinalSorted(profile.Manifests.Select(manifest => manifest.Path));
            AssertOrdinalSorted(profile.ContentReads);
            AssertOrdinalSorted(profile.Files);
            AssertOrdinalSorted(profile.Inventory.Languages.Select(language => language.Language));
        }
        finally
        {
            first.Dispose();
            second.Dispose();
        }
    }

    [Fact]
    public async Task Only_structurally_proven_generated_or_restorable_areas_are_safe_auto_excluded()
    {
        using var fixture = await MixedRepositoryFixture.CreateAsync();
        var profile = new RepositoryDiscovery().Discover(fixture.Root);

        var generated = profile.Areas
            .Where(area => area.Role == SourceRole.GeneratedOrRestorable)
            .Select(area => area.Path)
            .ToArray();
        Assert.Equal(ExpectedGeneratedOrRestorableAreas, generated);

        foreach (var area in profile.Areas.Where(area => area.ScanMode == ScanMode.SafeAutoExclude))
        {
            Assert.Contains(area.Role, new[] { SourceRole.GeneratedOrRestorable, SourceRole.ToolState });
            Assert.Equal(DiscoveryConfidence.High, area.Confidence);
            Assert.NotEmpty(area.Evidence);
            Assert.False(area.Enumerated);
            // Exclusion authority must come from a structural manifest/layout fact, never from the directory itself.
            Assert.All(area.Evidence, evidence => Assert.NotEqual(area.Path, evidence.Path));
        }

        Assert.Equal(
            new[] { ".git", ".pkc" },
            profile.Areas.Where(area => area.Role == SourceRole.ToolState).Select(area => area.Path).ToArray());

        // Excluded areas are not walked, so discovery stays bounded on restore trees.
        Assert.DoesNotContain(profile.Files, file =>
            ExpectedGeneratedOrRestorableAreas.Any(area => IsUnder(file, area)) ||
            IsUnder(file, ".git") ||
            IsUnder(file, ".pkc"));

        // Structural proof that is invalidated or absent keeps the area included.
        foreach (var path in new[]
                 {
                     "src/Worker/bin/Worker.dll",
                     "src/Worker/obj/Worker.assets.json",
                     "frontend/admin-portal/dist/stale-report.html"
                 })
        {
            Assert.Contains(path, profile.Files);
            var classification = profile.Classify(path);
            Assert.NotEqual(ScanMode.SafeAutoExclude, classification.ScanMode);
            Assert.NotEqual(SourceRole.GeneratedOrRestorable, classification.Role);
        }
    }

    [Fact]
    public async Task Directory_names_alone_never_create_exclusion_authority()
    {
        using var fixture = await MixedRepositoryFixture.CreateAsync();
        var profile = new RepositoryDiscovery().Discover(fixture.Root);

        foreach (var path in AmbiguouslyNamedFirstPartyFiles)
        {
            Assert.Contains(path, profile.Files);
            var classification = profile.Classify(path);
            Assert.Equal(SourceRole.Unknown, classification.Role);
            Assert.Equal(ScanMode.DeepScan, classification.ScanMode);
            Assert.Equal(DiscoveryConfidence.Unknown, classification.Confidence);
        }

        var ambiguousNames = new[] { "legacy", "vendor", "plugins", "themes", "scripts", "content", "old", "packages", "dist", "build", "bin" };
        Assert.DoesNotContain(profile.Areas, area =>
            area.ScanMode == ScanMode.SafeAutoExclude &&
            ambiguousNames.Contains(area.Path.Split('/').Last(), StringComparer.OrdinalIgnoreCase) &&
            area.Evidence.Count == 0);

        // Every enumerated file is attributed to exactly one area; UNKNOWN is kept, not dropped.
        Assert.Equal(profile.Files.Count, profile.Areas.Where(area => area.Enumerated).Sum(area => area.FileCount));
        Assert.Equal(profile.Files.Count, profile.Inventory.Files);
        Assert.Equal(profile.Files.Count, profile.Inventory.Languages.Sum(language => language.Files));
    }

    [Fact]
    public async Task Tests_are_identified_from_structural_evidence_and_never_create_production_authority()
    {
        using var fixture = await MixedRepositoryFixture.CreateAsync();
        var profile = new RepositoryDiscovery().Discover(fixture.Root);

        var xunitProject = Area(profile, "tests/Portal.Api.Tests");
        Assert.Equal(SourceRole.TestEvidence, xunitProject.Role);
        Assert.Equal(ScanMode.TestEvidence, xunitProject.ScanMode);
        Assert.Equal(DiscoveryConfidence.High, xunitProject.Confidence);
        Assert.Contains(xunitProject.Evidence, evidence =>
            evidence.Kind == "test-framework-package" && evidence.Path == "tests/Portal.Api.Tests/Portal.Api.Tests.csproj");

        var markedProject = Area(profile, "src/Verification");
        Assert.Equal(SourceRole.TestEvidence, markedProject.Role);
        Assert.Contains(markedProject.Evidence, evidence => evidence.Kind == "msbuild-is-test-project");

        var legacyProject = Area(profile, "tests/Legacy.Specs");
        Assert.Equal(SourceRole.TestEvidence, legacyProject.Role);
        Assert.Contains(legacyProject.Evidence, evidence => evidence.Kind == "test-framework-reference");

        // A production project stored under a test-looking directory name is not test evidence.
        var contracts = Area(profile, "tests/Portal.Contracts");
        Assert.Equal(SourceRole.Unknown, contracts.Role);
        Assert.Equal(ScanMode.DeepScan, contracts.ScanMode);

        // A project merely *named* like a test project is not test evidence.
        var namedLikeTests = Area(profile, "src/Portal.Tests.Fixtures");
        Assert.Equal(SourceRole.Unknown, namedLikeTests.Role);

        // Angular spec files are test evidence only because the workspace test target includes them.
        var spec = profile.Classify("frontend/admin-portal/src/app/orders/orders.component.spec.ts");
        Assert.Equal(SourceRole.TestEvidence, spec.Role);
        Assert.Equal(ScanMode.TestEvidence, spec.ScanMode);
        var component = profile.Classify("frontend/admin-portal/src/app/orders/orders.component.ts");
        Assert.Equal(SourceRole.Unknown, component.Role);
        Assert.Equal(ScanMode.DeepScan, component.ScanMode);
        var declarations = profile.Classify("frontend/admin-portal/src/typings.d.ts");
        Assert.Equal(SourceRole.Unknown, declarations.Role);
        var unconfiguredSpec = profile.Classify("legacy/js/checkout.spec.js");
        Assert.Equal(SourceRole.Unknown, unconfiguredSpec.Role);

        // Production-side areas never carry evidence sourced from test areas.
        var testAreas = profile.Areas.Where(area => area.Role == SourceRole.TestEvidence).Select(area => area.Path).ToArray();
        foreach (var area in profile.Areas.Where(area => !testAreas.Any(testArea => IsUnder(area.Path, testArea))))
        {
            Assert.DoesNotContain(area.Evidence, evidence => testAreas.Any(testArea => IsUnder(evidence.Path, testArea)));
        }

        Assert.Equal(SourceRole.Unknown, Area(profile, "src/Portal.Api").Role);
        Assert.Equal(SourceRole.Unknown, Area(profile, "src/Shared.Domain").Role);
    }

    [Fact]
    public async Task Infrastructure_and_manifests_are_inventoried_without_reading_source_or_config_values()
    {
        using var fixture = await MixedRepositoryFixture.CreateAsync();
        var profile = new RepositoryDiscovery().Discover(fixture.Root);

        foreach (var path in new[]
                 {
                     ".github/workflows/ci.yml",
                     "azure-pipelines.yml",
                     "deploy/Dockerfile",
                     "deploy/docker-compose.yml",
                     "infra/main.bicep",
                     "infra/terraform/main.tf"
                 })
        {
            var classification = profile.Classify(path);
            Assert.Equal(SourceRole.Infrastructure, classification.Role);
            Assert.Equal(ScanMode.LightIndex, classification.ScanMode);
        }

        var manifestKinds = profile.Manifests.ToDictionary(manifest => manifest.Path, manifest => manifest.Kind, StringComparer.Ordinal);
        Assert.Equal("dotnet-solution", manifestKinds["MixedLegacy.sln"]);
        Assert.Equal("dotnet-global-json", manifestKinds["global.json"]);
        Assert.Equal("msbuild-directory-props", manifestKinds["Directory.Build.props"]);
        Assert.Equal("msbuild-central-packages", manifestKinds["Directory.Packages.props"]);
        Assert.Equal("dotnet-project", manifestKinds["src/Portal.Api/Portal.Api.csproj"]);
        Assert.Equal("nuget-packages-config", manifestKinds["src/Backoffice.Web/packages.config"]);
        Assert.Equal("angular-workspace", manifestKinds["frontend/admin-portal/angular.json"]);
        Assert.Equal("node-package", manifestKinds["frontend/admin-portal/package.json"]);
        Assert.Equal("node-lockfile", manifestKinds["frontend/admin-portal/package-lock.json"]);
        Assert.Equal("typescript-config", manifestKinds["frontend/admin-portal/tsconfig.spec.json"]);
        Assert.Equal("legacy-bundle-config", manifestKinds["src/Backoffice.Web/bundleconfig.json"]);
        Assert.Equal("runtime-configuration", manifestKinds["src/Portal.Api/appsettings.json"]);
        Assert.Equal("runtime-configuration", manifestKinds["src/Backoffice.Web/Web.config"]);

        Assert.Contains(Area(profile, "frontend/admin-portal").Kinds, kind => kind == "angular-workspace");
        Assert.Contains(Area(profile, "frontend/admin-portal").Kinds, kind => kind == "node-package");
        Assert.Contains(Area(profile, "src/Backoffice.Web").Kinds, kind => kind == "dotnet-project");

        // Discovery reads only bounded build/workspace manifests; never source bodies or runtime configuration.
        var readableManifestSuffixes = new[] { ".csproj", ".vbproj", ".fsproj", ".sln", ".slnx", ".config", ".map", "/Directory.Build.props", "/Directory.Build.targets", "/angular.json", ".json" };
        Assert.NotEmpty(profile.ContentReads);
        foreach (var read in profile.ContentReads)
        {
            var kind = manifestKinds[read];
            Assert.Contains(kind, new[] { "dotnet-project", "dotnet-solution", "msbuild-directory-props", "msbuild-directory-targets", "angular-workspace", "typescript-config", "nuget-packages-config", "libman-manifest", "legacy-bundle-config", "source-map" });
            Assert.Contains(readableManifestSuffixes, suffix => ("/" + read).EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
        }

        var json = RepositoryProfileSerializer.Serialize(profile);
        Assert.DoesNotContain(MixedRepositoryFixture.PrivateConfigValue, json, StringComparison.Ordinal);
        Assert.DoesNotContain(MixedRepositoryFixture.PrivateSourceMarker, json, StringComparison.Ordinal);
    }

    [Fact]
    public void Discovery_has_no_semantic_analyzer_dependency()
    {
        var references = typeof(RepositoryDiscovery).Assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain(references, name => name.StartsWith("Microsoft.CodeAnalysis", StringComparison.Ordinal));
        Assert.DoesNotContain(references, name => name.StartsWith("Microsoft.Build", StringComparison.Ordinal));
        Assert.DoesNotContain(references, name => name.StartsWith("Pkc.CSharp", StringComparison.Ordinal));
        Assert.DoesNotContain(references, name => name.StartsWith("Pkc.Frontend", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Discovery_and_persisted_profile_exist_before_any_semantic_scanner_stage_begins()
    {
        using var fixture = await MixedRepositoryFixture.CreateAsync();
        var observed = new List<string>();
        var profilePath = Path.Combine(fixture.Root, ".pkc", "discovery", "repository-profile.json");

        var pipeline = new DiscoveryFirstScanPipeline(
            onDiscovered: (profile, path) => observed.Add($"discovered:{profile.Areas.Count > 0}:{path == profilePath}"));

        var result = await pipeline.RunAsync(fixture.Root,
        [
            new SemanticScanStage("csharp", (profile, _) =>
            {
                observed.Add($"csharp:{File.Exists(profilePath)}:{profile.Files.Count > 0}");
                return Task.FromResult(new FactDocument("test", [], []));
            }),
            new SemanticScanStage("frontend", (profile, _) =>
            {
                observed.Add($"frontend:{File.Exists(profilePath)}");
                return Task.FromResult(new FactDocument("test", [], []));
            })
        ]);

        Assert.Equal(new[] { "discovered:True:True", "csharp:True:True", "frontend:True" }, observed);
        Assert.Equal(2, result.Documents.Count);
        Assert.Equal(RepositoryProfileSerializer.Serialize(result.Profile), await File.ReadAllTextAsync(profilePath));

        // A rerun must not inventory its own discovery output.
        var rerun = new RepositoryDiscovery().Discover(fixture.Root);
        Assert.DoesNotContain(rerun.Files, file => IsUnder(file, ".pkc"));
        Assert.Equal(RepositoryProfileSerializer.Serialize(result.Profile), RepositoryProfileSerializer.Serialize(rerun));
    }

    [Fact]
    public async Task Semantic_stages_never_run_when_discovery_cannot_establish_a_profile()
    {
        var missing = Path.Combine(Path.GetTempPath(), "pkc-discovery-missing", Guid.NewGuid().ToString("N"));
        var stageRan = false;
        var pipeline = new DiscoveryFirstScanPipeline();

        await Assert.ThrowsAsync<DirectoryNotFoundException>(() => pipeline.RunAsync(missing,
        [
            new SemanticScanStage("csharp", (_, _) =>
            {
                stageRan = true;
                return Task.FromResult(new FactDocument("test", [], []));
            })
        ]));

        Assert.False(stageRan);
    }

    private static SourceArea Area(RepositoryProfile profile, string path) =>
        Assert.Single(profile.Areas, area => area.Path == path);

    private static bool IsUnder(string path, string area) =>
        string.Equals(path, area, StringComparison.Ordinal) ||
        path.StartsWith(area + "/", StringComparison.Ordinal);

    private static void AssertOrdinalSorted(IEnumerable<string> values)
    {
        var array = values.ToArray();
        Assert.Equal(array.OrderBy(value => value, StringComparer.Ordinal).ToArray(), array);
    }

    private sealed class MixedRepositoryFixture : IDisposable
    {
        public const string PrivateConfigValue = "Server=private-db.internal;Password=hunter2";
        public const string PrivateSourceMarker = "PRIVATE_PRICING_FORMULA_MARKER";

        private MixedRepositoryFixture(string root) => Root = root;

        public string Root { get; }

        public static async Task<MixedRepositoryFixture> CreateAsync(bool reverseCreationOrder = false)
        {
            var root = Path.Combine(Path.GetTempPath(), "pkc-discovery-tests", Guid.NewGuid().ToString("N"), "repo");
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

            return new MixedRepositoryFixture(root);
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
            const string sdkWebProject = """
                <Project Sdk="Microsoft.NET.Sdk.Web">
                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                  </PropertyGroup>
                </Project>
                """;
            const string sdkLibrary = """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                  </PropertyGroup>
                </Project>
                """;

            // Tool/VCS state.
            yield return (".git/HEAD", "ref: refs/heads/main\n");
            yield return (".git/config", "[core]\n");
            yield return (".git/objects/ab/cdef", "blob");
            yield return (".pkc/stale.json", "{}");

            // Root build/solution metadata.
            yield return ("MixedLegacy.sln", "Microsoft Visual Studio Solution File, Format Version 12.00\n");
            yield return ("global.json", """{ "sdk": { "version": "8.0.100" } }""");
            yield return ("Directory.Build.props", """
                <Project>
                  <PropertyGroup>
                    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
                  </PropertyGroup>
                </Project>
                """);
            yield return ("Directory.Packages.props", "<Project />");

            // Modern API host with restored/build output.
            yield return ("src/Portal.Api/Portal.Api.csproj", sdkWebProject);
            yield return ("src/Portal.Api/Program.cs", $"// {PrivateSourceMarker}\nvar app = WebApplication.Create(); app.Run();\n");
            yield return ("src/Portal.Api/Controllers/OrdersController.cs", "public sealed class OrdersController {}\n");
            yield return ("src/Portal.Api/appsettings.json", $$"""{ "ConnectionStrings": { "Main": "{{PrivateConfigValue}}" } }""");
            yield return ("src/Portal.Api/bin/Debug/net8.0/Portal.Api.dll", "MZ");
            yield return ("src/Portal.Api/bin/Debug/net8.0/Generated.cs", "class Generated {}");
            yield return ("src/Portal.Api/obj/project.assets.json", "{}");
            yield return ("src/Portal.Api/obj/Debug/net8.0/Portal.Api.AssemblyInfo.cs", "// generated");

            // Legacy non-SDK MVC application with checked-in-looking script/content trees.
            yield return ("src/Backoffice.Web/Backoffice.Web.csproj", """
                <?xml version="1.0" encoding="utf-8"?>
                <Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
                  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ">
                    <OutputPath>bin\</OutputPath>
                  </PropertyGroup>
                  <ItemGroup>
                    <Reference Include="System.Web.Mvc, Version=5.2.7.0" />
                    <Compile Include="Controllers\HomeController.cs" />
                  </ItemGroup>
                </Project>
                """);
            yield return ("src/Backoffice.Web/Controllers/HomeController.cs", "public class HomeController {}\n");
            yield return ("src/Backoffice.Web/Web.config", $"<configuration><connectionStrings><add connectionString=\"{PrivateConfigValue}\" /></connectionStrings></configuration>");
            yield return ("src/Backoffice.Web/packages.config", "<packages><package id=\"jQuery\" version=\"3.7.1\" /></packages>");
            yield return ("src/Backoffice.Web/bundleconfig.json", "[]");
            yield return ("src/Backoffice.Web/Scripts/app/orders.js", "function loadOrders() {}\n");
            yield return ("src/Backoffice.Web/Scripts/jquery-3.7.1.min.js", "/*! jQuery v3.7.1 */");
            yield return ("src/Backoffice.Web/Content/site.css", "body {}");
            yield return ("src/Backoffice.Web/Content/themes/custom/theme.js", "function applyTheme() {}\n");
            yield return ("src/Backoffice.Web/bin/Backoffice.Web.dll", "MZ");

            // Shared library with a first-party `legacy` folder.
            yield return ("src/Shared.Domain/Shared.Domain.csproj", sdkLibrary);
            yield return ("src/Shared.Domain/Order.cs", "public sealed class Order {}\n");
            yield return ("src/Shared.Domain/legacy/LegacyPricing.cs", "public static class LegacyPricing {}\n");

            // Worker whose output layout is redirected; bin/obj next to it are not proven build output.
            yield return ("src/Worker/Directory.Build.props", """
                <Project>
                  <PropertyGroup>
                    <UseArtifactsOutput>true</UseArtifactsOutput>
                  </PropertyGroup>
                </Project>
                """);
            yield return ("src/Worker/Worker.csproj", sdkLibrary);
            yield return ("src/Worker/Worker.cs", "public sealed class Worker {}\n");
            yield return ("src/Worker/bin/Worker.dll", "MZ");
            yield return ("src/Worker/obj/Worker.assets.json", "{}");

            // Test project in a non-test-named folder, marked structurally.
            yield return ("src/Verification/Verification.csproj", """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <IsTestProject>true</IsTestProject>
                  </PropertyGroup>
                </Project>
                """);
            yield return ("src/Verification/SmokeChecks.cs", "public sealed class SmokeChecks {}\n");

            // Project named like tests but with no test framework evidence.
            yield return ("src/Portal.Tests.Fixtures/Portal.Tests.Fixtures.csproj", sdkLibrary);
            yield return ("src/Portal.Tests.Fixtures/SampleData.cs", "public static class SampleData {}\n");

            // Real xunit test project.
            yield return ("tests/Portal.Api.Tests/Portal.Api.Tests.csproj", """
                <Project Sdk="Microsoft.NET.Sdk">
                  <ItemGroup>
                    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
                    <PackageReference Include="xunit" Version="2.9.2" />
                    <ProjectReference Include="..\..\src\Portal.Api\Portal.Api.csproj" />
                  </ItemGroup>
                </Project>
                """);
            yield return ("tests/Portal.Api.Tests/OrdersTests.cs", "public sealed class OrdersTests {}\n");
            yield return ("tests/Portal.Api.Tests/bin/Debug/Portal.Api.Tests.dll", "MZ");
            yield return ("tests/Portal.Api.Tests/obj/project.assets.json", "{}");

            // Legacy MSTest project using an assembly reference.
            yield return ("tests/Legacy.Specs/Legacy.Specs.csproj", """
                <?xml version="1.0" encoding="utf-8"?>
                <Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
                  <ItemGroup>
                    <Reference Include="Microsoft.VisualStudio.QualityTools.UnitTestFramework, Version=10.0.0.0" />
                  </ItemGroup>
                </Project>
                """);
            yield return ("tests/Legacy.Specs/PricingSpecs.cs", "public class PricingSpecs {}\n");

            // Production contract library stored under a test-looking folder.
            yield return ("tests/Portal.Contracts/Portal.Contracts.csproj", sdkLibrary);
            yield return ("tests/Portal.Contracts/OrderContract.cs", "public sealed record OrderContract;\n");

            // Angular workspace.
            yield return ("frontend/admin-portal/angular.json", """
                {
                  "version": 1,
                  "projects": {
                    "admin-portal": {
                      "root": "",
                      "sourceRoot": "src",
                      "architect": {
                        "build": {
                          "builder": "@angular-devkit/build-angular:browser",
                          "options": {
                            "outputPath": "dist/admin-portal",
                            "tsConfig": "tsconfig.app.json"
                          }
                        },
                        "test": {
                          "builder": "@angular-devkit/build-angular:karma",
                          "options": {
                            "tsConfig": "tsconfig.spec.json"
                          }
                        }
                      }
                    }
                  }
                }
                """);
            yield return ("frontend/admin-portal/package.json", """{ "name": "admin-portal", "private": true }""");
            yield return ("frontend/admin-portal/package-lock.json", "{}");
            yield return ("frontend/admin-portal/tsconfig.json", "{ /* base */ \"compilerOptions\": {}, }");
            yield return ("frontend/admin-portal/tsconfig.app.json", """{ "files": ["src/main.ts"], "include": ["src/**/*.d.ts"] }""");
            yield return ("frontend/admin-portal/tsconfig.spec.json", """
                {
                  // karma test compilation
                  "extends": "./tsconfig.json",
                  "include": ["src/**/*.spec.ts", "src/**/*.d.ts"],
                }
                """);
            yield return ("frontend/admin-portal/src/main.ts", "bootstrapApplication(AppComponent);\n");
            yield return ("frontend/admin-portal/src/typings.d.ts", "declare const APP_VERSION: string;\n");
            yield return ("frontend/admin-portal/src/app/app.component.ts", "export class AppComponent {}\n");
            yield return ("frontend/admin-portal/src/app/orders/orders.component.ts", "export class OrdersComponent {}\n");
            yield return ("frontend/admin-portal/src/app/orders/orders.component.spec.ts", "describe('orders', () => {});\n");
            yield return ("frontend/admin-portal/src/app/vendor/chart-wrapper.ts", "export class ChartWrapper {}\n");
            yield return ("frontend/admin-portal/src/app/plugins/export.ts", "export function exportOrders() {}\n");
            yield return ("frontend/admin-portal/node_modules/@angular/core/package.json", """{ "name": "@angular/core" }""");
            yield return ("frontend/admin-portal/node_modules/@angular/core/index.js", "module.exports = {};");
            yield return ("frontend/admin-portal/node_modules/rxjs/index.js", "module.exports = {};");
            yield return ("frontend/admin-portal/dist/admin-portal/main.js", "(()=>{})();");
            yield return ("frontend/admin-portal/dist/stale-report.html", "<html></html>");
            yield return ("frontend/admin-portal/.angular/cache/state.json", "{}");

            // Loose legacy JavaScript / C# under ambiguous names.
            yield return ("legacy/js/checkout.js", "function checkout() {}\n");
            yield return ("legacy/js/checkout.spec.js", "describe('checkout', function () {});\n");
            yield return ("vendor/acme-grid/acme-grid.js", "/*! acme-grid v2.1 */");
            yield return ("vendor/acme-grid/custom-patch.js", "AcmeGrid.prototype.exportCsv = function () {};\n");
            yield return ("plugins/Reporting/ReportPlugin.cs", "public sealed class ReportPlugin {}\n");
            yield return ("themes/corporate/theme.js", "function corporateTheme() {}\n");
            yield return ("old/Billing/InvoiceCalculator.cs", "public static class InvoiceCalculator {}\n");
            yield return ("Scripts/site-legacy.js", "function legacySite() {}\n");
            yield return ("Content/print.css", "@media print {}");
            yield return ("dist/legacy-bundle.js", "var bundle = 1;");
            yield return ("build/build.ps1", "Write-Host build");
            yield return ("tools/bin/deploy-helper.ps1", "Write-Host deploy");

            // `packages`: one restored NuGet package (structurally proven) and one first-party folder.
            yield return ("packages/Newtonsoft.Json.13.0.3/Newtonsoft.Json.13.0.3.nupkg", "PK");
            yield return ("packages/Newtonsoft.Json.13.0.3/lib/net45/Newtonsoft.Json.dll", "MZ");
            yield return ("packages/InternalTools/Tool.cs", "public static class Tool {}\n");

            // Infrastructure.
            yield return ("deploy/Dockerfile", "FROM mcr.microsoft.com/dotnet/aspnet:8.0\n");
            yield return ("deploy/docker-compose.yml", "services: {}\n");
            yield return ("azure-pipelines.yml", "trigger: none\n");
            yield return (".github/workflows/ci.yml", "on: push\n");
            yield return ("infra/main.bicep", "param location string\n");
            yield return ("infra/terraform/main.tf", "terraform {}\n");
        }
    }
}
