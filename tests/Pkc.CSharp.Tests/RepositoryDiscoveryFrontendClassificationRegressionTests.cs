using System.Text;
using Pkc.Core.Discovery;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class RepositoryDiscoveryFrontendClassificationRegressionTests
{
    private const string Web = "src/Shop.Web";

    [Fact]
    public async Task Declared_restore_content_is_indexed_as_third_party_runtime_not_deep_scanned()
    {
        using var fixture = await LegacyFrontendFixture.CreateAsync();
        var profile = new RepositoryDiscovery().Discover(fixture.Root);

        foreach (var path in new[]
                 {
                     $"{Web}/Scripts/jquery-3.7.1.js",
                     $"{Web}/Scripts/jquery-3.7.1.min.js",
                     $"{Web}/Scripts/jquery-3.7.1.min.map",
                     $"{Web}/wwwroot/lib/datatables/datatables.min.js",
                     $"{Web}/wwwroot/lib/datatables/datatables.min.css"
                 })
        {
            var classification = profile.Classify(path);
            Assert.Equal(SourceRole.ThirdPartyRuntime, classification.Role);
            Assert.Equal(ScanMode.RuntimeDependencyIndex, classification.ScanMode);
            Assert.Equal(DiscoveryConfidence.High, classification.Confidence);
        }

        // Runtime use through a bundle never promotes vendor internals into deep scanning.
        Assert.Contains(
            $"{Web}/Scripts/jquery-3.7.1.js",
            Assert.Single(profile.GeneratedArtifacts, artifact => artifact.Kind == "bundle-output").Inputs);
        Assert.Equal(ScanMode.RuntimeDependencyIndex, profile.Classify($"{Web}/Scripts/jquery-3.7.1.js").ScanMode);
    }

    [Fact]
    public async Task Locally_modified_or_added_vendor_files_get_a_narrow_first_party_carve_out()
    {
        using var fixture = await LegacyFrontendFixture.CreateAsync();
        var profile = new RepositoryDiscovery().Discover(fixture.Root);

        var modified = profile.Classify($"{Web}/Scripts/jquery.validate.js");
        Assert.Equal(SourceRole.ThirdPartyModified, modified.Role);
        Assert.Equal(ScanMode.DeepScan, modified.ScanMode);

        var added = profile.Classify($"{Web}/wwwroot/lib/datatables/datatables.custom-export.js");
        Assert.Equal(SourceRole.Unknown, added.Role);
        Assert.Equal(ScanMode.DeepScan, added.ScanMode);

        // The carve-outs are file-level: the rest of the vendor tree stays indexed.
        var destination = Assert.Single(profile.Areas, area => area.Path == $"{Web}/wwwroot/lib/datatables");
        Assert.Equal(SourceRole.ThirdPartyRuntime, destination.Role);
        Assert.Equal(ScanMode.RuntimeDependencyIndex, destination.ScanMode);
        var carveOut = Assert.Single(destination.Overrides);
        Assert.Equal("datatables.custom-export.js", carveOut.Pattern);
        Assert.Contains(carveOut.Evidence, evidence => evidence.Kind == "vendor-destination-undeclared-file");

        // Without a declared file list the destination content is not provably vendor: included, third-party unknown.
        var undeclaredList = profile.Classify($"{Web}/wwwroot/lib/chartjs/chart.umd.js");
        Assert.Equal(SourceRole.ThirdPartyUnknown, undeclaredList.Role);
        Assert.Equal(ScanMode.DeepScan, undeclaredList.ScanMode);

        Assert.Contains($"{Web}/Scripts/jquery.validate.js", profile.ByteComparisons);
        Assert.Contains("packages/jQuery.Validation.1.19.5/Content/Scripts/jquery.validate.js", profile.ByteComparisons);
    }

    [Fact]
    public async Task Generated_bundles_and_minified_outputs_carry_input_provenance()
    {
        using var fixture = await LegacyFrontendFixture.CreateAsync();
        var profile = new RepositoryDiscovery().Discover(fixture.Root);

        var bundle = Assert.Single(profile.GeneratedArtifacts, artifact => artifact.Kind == "bundle-output");
        Assert.Equal($"{Web}/wwwroot/js/site.bundle.min.js", bundle.Path);
        Assert.Equal(
            new[]
            {
                $"{Web}/Scripts/app/grid-wrapper.js",
                $"{Web}/Scripts/app/orders.js",
                $"{Web}/Scripts/jquery-3.7.1.js"
            },
            bundle.Inputs);
        Assert.Equal(new[] { $"{Web}/Scripts/missing.js" }, bundle.UnresolvedInputs);
        Assert.Contains(bundle.Evidence, evidence => evidence.Kind == "bundleconfig-output" && evidence.Path == $"{Web}/bundleconfig.json");

        var bundleClassification = profile.Classify(bundle.Path);
        Assert.Equal(SourceRole.GeneratedOrRestorable, bundleClassification.Role);
        Assert.Equal(ScanMode.LightIndex, bundleClassification.ScanMode);

        var minified = Assert.Single(profile.GeneratedArtifacts, artifact => artifact.Path == $"{Web}/Scripts/app/orders.min.js");
        Assert.Equal("source-map-output", minified.Kind);
        Assert.Equal(new[] { $"{Web}/Scripts/app/orders.js" }, minified.Inputs);
        Assert.Equal(ScanMode.LightIndex, profile.Classify(minified.Path).ScanMode);

        // A declared vendor classification outranks the generated classification of a vendor minified file.
        Assert.Equal(SourceRole.ThirdPartyRuntime, profile.Classify($"{Web}/Scripts/jquery-3.7.1.min.js").Role);

        // First-party sources and wrappers stay in deep scan.
        foreach (var path in new[] { $"{Web}/Scripts/app/orders.js", $"{Web}/Scripts/app/grid-wrapper.js" })
        {
            Assert.Equal(ScanMode.DeepScan, profile.Classify(path).ScanMode);
            Assert.Equal(SourceRole.Unknown, profile.Classify(path).Role);
        }
    }

    [Fact]
    public async Task Names_banners_and_copied_content_never_create_vendor_authority()
    {
        using var fixture = await LegacyFrontendFixture.CreateAsync();
        var profile = new RepositoryDiscovery().Discover(fixture.Root);

        foreach (var path in new[]
                 {
                     // Byte-identical copy of a package file, but not at a declared content path.
                     $"{Web}/Scripts/legacy/jquery-3.7.1.js",
                     // License/version banner with no manifest.
                     $"{Web}/Scripts/vendor/acme-grid.js",
                     // Minified-looking name without a resolvable source map.
                     $"{Web}/Scripts/vendor/acme-grid.min.js"
                 })
        {
            var classification = profile.Classify(path);
            Assert.Equal(SourceRole.Unknown, classification.Role);
            Assert.Equal(ScanMode.DeepScan, classification.ScanMode);
        }

        // Package transforms are rewritten into first-party code; they are never compared or classified as vendor.
        Assert.Equal(SourceRole.Unknown, profile.Classify($"{Web}/App_Start/Config.cs").Role);
        Assert.DoesNotContain(profile.ByteComparisons, path => path.EndsWith(".pp", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Unrestorable_declarations_are_reported_and_classification_is_deterministic()
    {
        var first = await LegacyFrontendFixture.CreateAsync(reverseCreationOrder: false);
        var second = await LegacyFrontendFixture.CreateAsync(reverseCreationOrder: true);
        try
        {
            var discovery = new RepositoryDiscovery();
            var profile = discovery.Discover(first.Root);
            var unresolved = profile.UnresolvedReferences
                .Select(reference => $"{reference.From} | {reference.Reference} | {reference.Reason}")
                .ToArray();
            Assert.Equal(
                new[]
                {
                    $"{Web}/libman.json | lodash@4.17.21 | libman-destination-undeclared",
                    $"{Web}/packages.config | Modernizr 2.8.3 | nuget-package-not-restored"
                },
                unresolved);

            var json = RepositoryProfileSerializer.Serialize(profile);
            Assert.Equal(json, RepositoryProfileSerializer.Serialize(discovery.Discover(second.Root)));
            Assert.DoesNotContain(first.Root, json, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(LegacyFrontendFixture.SourceMarker, json, StringComparison.Ordinal);
            Assert.Equal(profile.ByteComparisons.OrderBy(path => path, StringComparer.Ordinal), profile.ByteComparisons);
            Assert.Equal(profile.GeneratedArtifacts.Select(a => a.Path).OrderBy(path => path, StringComparer.Ordinal),
                profile.GeneratedArtifacts.Select(a => a.Path));

            var manifestKinds = profile.Manifests.ToDictionary(manifest => manifest.Path, manifest => manifest.Kind, StringComparer.Ordinal);
            Assert.All(profile.ContentReads, read => Assert.Contains(
                manifestKinds[read],
                new[]
                {
                    "dotnet-project", "dotnet-solution", "msbuild-directory-props", "msbuild-directory-targets",
                    "angular-workspace", "typescript-config", "nuget-packages-config", "libman-manifest",
                    "legacy-bundle-config", "source-map"
                }));
        }
        finally
        {
            first.Dispose();
            second.Dispose();
        }
    }

    private sealed class LegacyFrontendFixture : IDisposable
    {
        public const string SourceMarker = "PRIVATE_CHECKOUT_RULE_MARKER";

        private const string JqueryBody = "/*! jQuery v3.7.1 | (c) OpenJS Foundation and other contributors | jquery.org/license */\n(function(){ /* jquery */ })();\n";
        private const string JqueryMin = "/*! jQuery v3.7.1 */!function(){}();\n//# sourceMappingURL=jquery-3.7.1.min.map\n";
        private const string JqueryMap = """{"version":3,"file":"jquery-3.7.1.min.js","sources":["jquery-3.7.1.js"],"mappings":""}""";
        private const string ValidateBody = "/*! jQuery Validation Plugin v1.19.5 */\n(function(){ /* validate */ })();\n";

        private LegacyFrontendFixture(string root) => Root = root;

        public string Root { get; }

        public static async Task<LegacyFrontendFixture> CreateAsync(bool reverseCreationOrder = false)
        {
            var root = Path.Combine(Path.GetTempPath(), "pkc-discovery-frontend", Guid.NewGuid().ToString("N"), "repo");
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

            return new LegacyFrontendFixture(root);
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
            // Restored packages.config packages (the folders are SAFE_AUTO_EXCLUDE from RD1).
            yield return ("packages/jQuery.3.7.1/jQuery.3.7.1.nupkg", "PK");
            yield return ("packages/jQuery.3.7.1/Content/Scripts/jquery-3.7.1.js", JqueryBody);
            yield return ("packages/jQuery.3.7.1/Content/Scripts/jquery-3.7.1.min.js", JqueryMin);
            yield return ("packages/jQuery.3.7.1/Content/Scripts/jquery-3.7.1.min.map", JqueryMap);
            yield return ("packages/jQuery.Validation.1.19.5/jQuery.Validation.1.19.5.nupkg", "PK");
            yield return ("packages/jQuery.Validation.1.19.5/Content/Scripts/jquery.validate.js", ValidateBody);
            yield return ("packages/WebGrease.1.6.0/WebGrease.1.6.0.nupkg", "PK");
            yield return ("packages/WebGrease.1.6.0/Content/App_Start/Config.cs.pp", "namespace $rootnamespace$ {}");

            yield return ($"{Web}/Shop.Web.csproj", """
                <?xml version="1.0" encoding="utf-8"?>
                <Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
                  <PropertyGroup>
                    <ProjectTypeGuids>{349c5851-65df-11da-9384-00065b846f21};{fae04ec0-301f-11d3-bf4b-00c04f79efbc}</ProjectTypeGuids>
                  </PropertyGroup>
                </Project>
                """);
            yield return ($"{Web}/packages.config", """
                <?xml version="1.0" encoding="utf-8"?>
                <packages>
                  <package id="jQuery" version="3.7.1" targetFramework="net48" />
                  <package id="jQuery.Validation" version="1.19.5" targetFramework="net48" />
                  <package id="Modernizr" version="2.8.3" targetFramework="net48" />
                  <package id="WebGrease" version="1.6.0" targetFramework="net48" />
                </packages>
                """);
            yield return ($"{Web}/App_Start/Config.cs", $"// {SourceMarker}\nnamespace Shop.Web {{}}\n");

            // NuGet content, untouched and locally modified.
            yield return ($"{Web}/Scripts/jquery-3.7.1.js", JqueryBody);
            yield return ($"{Web}/Scripts/jquery-3.7.1.min.js", JqueryMin);
            yield return ($"{Web}/Scripts/jquery-3.7.1.min.map", JqueryMap);
            yield return ($"{Web}/Scripts/jquery.validate.js", ValidateBody + "// local patch: allow empty postcode\n");

            // First-party page script, its minified output with a map, and a wrapper around vendor code.
            yield return ($"{Web}/Scripts/app/orders.js", $"// {SourceMarker}\nfunction loadOrders() {{}}\n");
            yield return ($"{Web}/Scripts/app/orders.min.js", "function loadOrders(){}\n//# sourceMappingURL=orders.min.js.map\n");
            yield return ($"{Web}/Scripts/app/orders.min.js.map", """{"version":3,"file":"orders.min.js","sources":["orders.js"],"mappings":""}""");
            yield return ($"{Web}/Scripts/app/grid-wrapper.js", "function createGrid(el) { return $(el).DataTable(); }\n");

            // Look-alikes without declarations.
            yield return ($"{Web}/Scripts/legacy/jquery-3.7.1.js", JqueryBody);
            yield return ($"{Web}/Scripts/vendor/acme-grid.js", "/*! acme-grid v2.1 | MIT License */\nfunction AcmeGrid() {}\n");
            yield return ($"{Web}/Scripts/vendor/acme-grid.min.js", "function AcmeGrid(){}\n");

            // LibMan: one library with a file list (plus a locally added file), one without, one without destination.
            yield return ($"{Web}/libman.json", """
                {
                  "version": "1.0",
                  "defaultProvider": "cdnjs",
                  "libraries": [
                    {
                      "library": "datatables@1.13.6",
                      "destination": "wwwroot/lib/datatables/",
                      "files": [ "datatables.min.js", "datatables.min.css" ]
                    },
                    { "library": "chart.js@4.4.0", "destination": "wwwroot/lib/chartjs" },
                    { "library": "lodash@4.17.21" }
                  ]
                }
                """);
            yield return ($"{Web}/wwwroot/lib/datatables/datatables.min.js", "/*! DataTables 1.13.6 */");
            yield return ($"{Web}/wwwroot/lib/datatables/datatables.min.css", "table.dataTable{}");
            yield return ($"{Web}/wwwroot/lib/datatables/datatables.custom-export.js", "$.fn.dataTable.ext.buttons.exportCsv = {};\n");
            yield return ($"{Web}/wwwroot/lib/chartjs/chart.umd.js", "/*! Chart.js v4.4.0 */");

            // BundlerMinifier bundle mixing vendor and first-party inputs.
            yield return ($"{Web}/bundleconfig.json", """
                [
                  {
                    "outputFileName": "wwwroot/js/site.bundle.min.js",
                    "inputFiles": [
                      "Scripts/jquery-3.7.1.js",
                      "Scripts/app/*.js",
                      "!Scripts/app/*.min.js",
                      "Scripts/missing.js"
                    ]
                  }
                ]
                """);
            yield return ($"{Web}/wwwroot/js/site.bundle.min.js", "!function(){}();");
        }
    }
}
