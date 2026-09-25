using System.Text;
using Pkc.Core.Discovery;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class RepositoryDiscoveryRuntimePluginRegressionTests
{
    private const string Host = "dotnet:src/Portal.Host/Portal.Host.csproj";
    private const string Core = "dotnet:src/Portal.Core/Portal.Core.csproj";
    private const string Reporting = "dotnet:src/Plugins/Portal.Reporting/Portal.Reporting.csproj";
    private const string Billing = "dotnet:src/Plugins/Portal.Billing/Portal.Billing.csproj";
    private const string Exporter = "dotnet:src/Plugins/Portal.Exporter/Portal.Exporter.csproj";
    private const string Audit = "dotnet:src/Plugins/Portal.Audit/Portal.Audit.csproj";
    private const string PluginTests = "dotnet:tests/Portal.Plugins.Tests/Portal.Plugins.Tests.csproj";
    private const string Loader = "src/Portal.Host/Plugins/PluginLoader.cs";

    [Fact]
    public async Task Plugins_loaded_by_identity_and_copied_into_the_host_get_provenance_bearing_runtime_edges()
    {
        using var fixture = await PluginFixture.CreateAsync();
        var profile = new RepositoryDiscovery().Discover(fixture.Root);

        var runtimeEdges = profile.Edges.Where(edge => edge.Kind == "runtime-plugin-load").ToArray();
        Assert.Equal(new[] { $"{Host} -> {Billing}", $"{Host} -> {Reporting}" },
            runtimeEdges.Select(edge => $"{edge.From} -> {edge.To}").ToArray());
        Assert.All(runtimeEdges, edge => Assert.Equal(DiscoveryConfidence.High, edge.Confidence));

        var reporting = Assert.Single(runtimeEdges, edge => edge.To == Reporting);
        Assert.Contains(new DiscoveryEvidence("runtime-loader", Loader), reporting.Evidence);
        Assert.Contains(new DiscoveryEvidence("runtime-plugin-identity", "src/Plugins/Portal.Reporting/Portal.Reporting.csproj"), reporting.Evidence);
        Assert.Contains(new DiscoveryEvidence("msbuild-output-path-into-host", "src/Plugins/Portal.Reporting/Portal.Reporting.csproj"), reporting.Evidence);

        var billing = Assert.Single(runtimeEdges, edge => edge.To == Billing);
        Assert.Contains(new DiscoveryEvidence("msbuild-copy-into-host", "src/Plugins/Portal.Billing/Portal.Billing.csproj"), billing.Evidence);

        // Runtime edges carry ownership; project-reference edges are unchanged.
        Assert.Equal(new[] { Host }, Component(profile, Reporting).Owners);
        Assert.Equal(new[] { Host }, Component(profile, Billing).Owners);
        Assert.Equal(OwnershipStatus.Owned, Component(profile, Billing).Ownership);
        Assert.Equal(new[] { Host }, Component(profile, Core).Owners);
        Assert.Contains(profile.Edges, edge => edge.From == Reporting && edge.To == Core && edge.Kind == "project-reference");
    }

    [Fact]
    public async Task Missing_loader_identity_or_copy_provenance_never_creates_an_authoritative_edge()
    {
        using var fixture = await PluginFixture.CreateAsync();
        var profile = new RepositoryDiscovery().Discover(fixture.Root);

        var unresolved = profile.UnresolvedReferences
            .Select(reference => $"{reference.From} | {reference.Reference} | {reference.Reason}")
            .ToArray();
        Assert.Equal(
            new[]
            {
                $"{Audit} | {Host} | plugin-copy-without-identified-loader",
                $"{Host} | Portal.Exporter | runtime-plugin-copy-unproven",
                $"{Host} | Portal.Ghost | runtime-plugin-identity-not-found",
                $"{Host} | {Loader} | runtime-loader-identity-unresolved"
            },
            unresolved);

        foreach (var plugin in new[] { Exporter, Audit })
        {
            Assert.DoesNotContain(profile.Edges, edge => edge.To == plugin);
            Assert.Equal(OwnershipStatus.Unknown, Component(profile, plugin).Ownership);
            Assert.Empty(Component(profile, plugin).Owners);
        }
    }

    [Fact]
    public async Task Test_only_plugin_loads_are_test_evidence_not_production_edges()
    {
        using var fixture = await PluginFixture.CreateAsync();
        var profile = new RepositoryDiscovery().Discover(fixture.Root);

        Assert.DoesNotContain(profile.Edges, edge => edge.From == PluginTests);
        Assert.Equal(new[] { PluginTests }, Component(profile, Exporter).TestReferences);
        Assert.DoesNotContain(PluginTests, Component(profile, Exporter).Owners);
    }

    [Fact]
    public async Task Composition_probes_are_bounded_recorded_and_deterministic()
    {
        var first = await PluginFixture.CreateAsync(reverseCreationOrder: false);
        var second = await PluginFixture.CreateAsync(reverseCreationOrder: true);
        try
        {
            var discovery = new RepositoryDiscovery();
            var profile = discovery.Discover(first.Root);

            // Only host and test project sources are probed; libraries and plugins are never read.
            var probes = profile.CompositionProbes.ToDictionary(probe => probe.Component, probe => probe.Files, StringComparer.Ordinal);
            Assert.Equal(2, probes[Host]);
            Assert.Equal(1, probes[PluginTests]);
            Assert.DoesNotContain(Reporting, probes.Keys);
            Assert.DoesNotContain(Core, probes.Keys);

            var json = RepositoryProfileSerializer.Serialize(profile);
            Assert.Equal(json, RepositoryProfileSerializer.Serialize(discovery.Discover(second.Root)));
            Assert.DoesNotContain(first.Root, json, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(PluginFixture.SourceMarker, json, StringComparison.Ordinal);
        }
        finally
        {
            first.Dispose();
            second.Dispose();
        }
    }

    [Theory]
    [InlineData(false, "LoadFrom")]
    [InlineData(true, "LoadFrom")]
    [InlineData(true, "LoadFile")]
    [InlineData(true, "UnsafeLoadFrom")]
    public async Task Folder_scan_method_group_shape_is_recovered(bool directCall, string api)
    {
        using var fixture = await FolderScanPluginFixture.CreateAsync(directCall, api);
        AssertFolderScanEdges(new RepositoryDiscovery().Discover(fixture.Root));
    }

    [Fact]
    public async Task Folder_scan_spanning_members_and_a_fluent_chain_is_recovered()
    {
        using var fixture = await FolderScanPluginFixture.CreateAsync(false, memberSpanning: true);
        AssertFolderScanEdges(new RepositoryDiscovery().Discover(fixture.Root));
    }

    private static void AssertFolderScanEdges(RepositoryProfile profile)
    {
        var edges = profile.Edges.Where(edge => edge.Kind == "runtime-plugin-load").ToArray();
        Assert.Equal(new[]
        {
            "dotnet:src/Shape.Host/Shape.Host.csproj -> dotnet:src/Shape.Plugins/Shape.One/Shape.One.csproj",
            "dotnet:src/Shape.Host/Shape.Host.csproj -> dotnet:src/Shape.Plugins/Shape.Two/Shape.Two.csproj"
        }, edges.Select(edge => $"{edge.From} -> {edge.To}").ToArray());
        Assert.All(edges, edge => Assert.Equal(DiscoveryConfidence.High, edge.Confidence));
        Assert.All(edges, edge =>
        {
            Assert.Contains(new DiscoveryEvidence("runtime-loader", "src/Shape.Host/Loader.cs"), edge.Evidence);
            Assert.Contains(new DiscoveryEvidence("runtime-plugin-identity", edge.To["dotnet:".Length..]), edge.Evidence);
            Assert.Contains(new DiscoveryEvidence("msbuild-copy-into-host-scan-directory", edge.To["dotnet:".Length..]), edge.Evidence);
            Assert.Equal(new[] { "msbuild-copy-into-host-scan-directory", "runtime-loader", "runtime-plugin-identity" },
                edge.Evidence.Select(evidence => evidence.Kind).ToArray());
            Assert.Equal(new[] { edge.From }, Component(profile, edge.To).Owners);
            Assert.Equal(OwnershipStatus.Owned, Component(profile, edge.To).Ownership);
        });
        Assert.Empty(profile.UnresolvedReferences);
        Assert.Equal(edges[0].Evidence.OrderBy(item => item.Kind).ThenBy(item => item.Path), edges[0].Evidence);
    }

    [Fact]
    public async Task Folder_scan_positive_output_is_deterministic_when_files_are_created_in_reverse_order()
    {
        using var first = await FolderScanPluginFixture.CreateAsync(false, "LoadFrom", "mods", reverseCreationOrder: false);
        using var second = await FolderScanPluginFixture.CreateAsync(false, "LoadFrom", "mods", reverseCreationOrder: true);

        var discovery = new RepositoryDiscovery();
        Assert.Equal(RepositoryProfileSerializer.Serialize(discovery.Discover(first.Root)),
            RepositoryProfileSerializer.Serialize(discovery.Discover(second.Root)));
    }

    [Fact]
    public async Task Folder_scan_nested_relative_directory_is_normalized()
    {
        using var fixture = await FolderScanPluginFixture.CreateAsync(false, "LoadFrom", "nested/mods");
        var profile = new RepositoryDiscovery().Discover(fixture.Root);

        Assert.Equal(2, profile.Edges.Count(edge => edge.Kind == "runtime-plugin-load"));
        Assert.DoesNotContain(profile.UnresolvedReferences, reference => reference.Reason.StartsWith("runtime-plugin-", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("enumeration-without-load", null)]
    [InlineData("method-group-without-scan", "runtime-loader-identity-unresolved")]
    [InlineData("delivery-without-loader", "plugin-copy-without-identified-loader")]
    [InlineData("wrong-scan-directory", "runtime-plugin-scan-directory-mismatch")]
    [InlineData("missing-project-name", "runtime-plugin-scan-directory-mismatch")]
    [InlineData("literal-project-subfolder", "runtime-plugin-scan-directory-mismatch")]
    [InlineData("assembly-name-mismatch", null)]
    [InlineData("duplicate-assembly-identity", "runtime-plugin-identity-ambiguous")]
    [InlineData("item-outside-target", null)]
    [InlineData("item-non-output", null)]
    [InlineData("item-conflicting-definition", null)]
    [InlineData("item-exclude", null)]
    [InlineData("item-conditional-definition", null)]
    [InlineData("conditional-copy", null)]
    [InlineData("plugin-output-override", "runtime-plugin-scan-directory-mismatch")]
    [InlineData("host-output-override", "runtime-plugin-scan-directory-mismatch")]
    [InlineData("test-only-loader", null)]
    [InlineData("nonliteral-scan-directory", "runtime-loader-identity-unresolved")]
    [InlineData("similar-folder-without-copy-proof", "runtime-plugin-scan-directory-mismatch")]
    [InlineData("noninterpolated-convention", "runtime-loader-identity-unresolved")]
    [InlineData("unrelated-inline-load", "runtime-loader-identity-unresolved")]
    [InlineData("loader-before-convention", "runtime-loader-identity-unresolved")]
    [InlineData("reassigned-base-dirs-paths", "runtime-loader-identity-unresolved")]
    [InlineData("same-line-unrelated-load", "runtime-plugin-identity-not-found")]
    [InlineData("multiple-scan-directories", "runtime-loader-identity-unresolved")]
    [InlineData("different-target-item", null)]
    [InlineData("conditional-target", null)]
    [InlineData("host-token-destination", "runtime-plugin-scan-directory-mismatch")]
    [InlineData("method-group-assignment-no-scan", "runtime-loader-identity-unresolved")]
    [InlineData("inert-api-string", null)]
    [InlineData("tuple-unrelated-load", "runtime-plugin-identity-not-found")]
    public async Task Folder_scan_negative_matrix_never_promotes_an_edge(string scenario, string? expectedReason)
    {
        using var fixture = await NegativeFolderScanFixture.CreateAsync(scenario);
        var profile = new RepositoryDiscovery().Discover(fixture.Root);

        Assert.DoesNotContain(profile.Edges, edge => edge.Kind == "runtime-plugin-load");
        if (expectedReason is not null)
        {
            Assert.Contains(profile.UnresolvedReferences, reference => reference.Reason == expectedReason);
        }
    }

    private static RepositoryComponent Component(RepositoryProfile profile, string id) =>
        Assert.Single(profile.Components, component => component.Id == id);

    private sealed class NegativeFolderScanFixture : IDisposable
    {
        private NegativeFolderScanFixture(string root) => Root = root;
        public string Root { get; }

        public static async Task<NegativeFolderScanFixture> CreateAsync(string scenario)
        {
            var root = Path.Combine(Path.GetTempPath(), "pkc-discovery-folder-scan-negative", Guid.NewGuid().ToString("N"), "repo");
            Directory.CreateDirectory(root);
            var hostSource = scenario == "enumeration-without-load"
                ? "var basePath = Path.Combine(AppContext.BaseDirectory, \"mods\"); var dirs = Directory.GetDirectories(basePath);"
                : scenario == "delivery-without-loader"
                    ? "var value = 1;"
                : scenario == "nonliteral-scan-directory"
                    ? "var mode = \"mods\"; var basePath = Path.Combine(AppContext.BaseDirectory, mode); var dirs = Directory.GetDirectories(basePath); var paths = dirs.Select(dir => Path.Combine(dir, $\"{Path.GetFileName(dir)}.dll\")); paths.Select(Assembly.LoadFrom);"
                : scenario == "noninterpolated-convention"
                    ? "var basePath = Path.Combine(AppContext.BaseDirectory, \"mods\"); var dirs = Directory.GetDirectories(basePath); var paths = dirs.Select(dir => Path.Combine(dir, \"{Path.GetFileName(dir)}.dll\")); paths.Select(Assembly.LoadFrom);"
                : scenario == "unrelated-inline-load"
                    ? "var basePath = Path.Combine(AppContext.BaseDirectory, \"mods\"); var dirs = Directory.GetDirectories(basePath); var other = \"other\"; Assembly.LoadFrom(Path.Combine(other, $\"{Path.GetFileName(other)}.dll\"));"
                : scenario == "loader-before-convention"
                    ? "var paths = dirs.Select(Assembly.LoadFrom);\nvar basePath = Path.Combine(AppContext.BaseDirectory, \"mods\");\nvar dirs = Directory.GetDirectories(basePath);\nvar paths = dirs.Select(dir => Path.Combine(dir, $\"{Path.GetFileName(dir)}.dll\"));"
                : scenario == "reassigned-base-dirs-paths"
                    ? "var basePath = Path.Combine(AppContext.BaseDirectory, \"mods\");\nvar dirs = Directory.GetDirectories(basePath);\nvar paths = dirs.Select(dir => Path.Combine(dir, $\"{Path.GetFileName(dir)}.dll\"));\nbasePath = Path.Combine(AppContext.BaseDirectory, \"other\");\ndirs = Directory.GetDirectories(basePath);\npaths = dirs.Select(dir => Path.Combine(dir, $\"{Path.GetFileName(dir)}.dll\"));\npaths.Select(Assembly.LoadFrom);"
                : scenario == "same-line-unrelated-load"
                    ? "var basePath = Path.Combine(AppContext.BaseDirectory, \"mods\"); var dirs = Directory.GetDirectories(basePath); var paths = dirs.Select(dir => Path.Combine(dir, $\"{Path.GetFileName(dir)}.dll\")); Assembly.LoadFrom(Path.Combine(\"other\", \"Other.dll\"));"
                : scenario == "multiple-scan-directories"
                    ? "var basePath = Path.Combine(AppContext.BaseDirectory, \"mods\");\nvar dirs = Directory.GetDirectories(basePath);\nvar otherBase = Path.Combine(AppContext.BaseDirectory, \"other\");\nvar otherDirs = Directory.GetDirectories(otherBase);\nvar paths = dirs.Select(dir => Path.Combine(dir, $\"{Path.GetFileName(dir)}.dll\"));\npaths.Select(Assembly.LoadFrom);"
                : scenario == "method-group-assignment-no-scan"
                    ? "Func<string, Assembly> loader = Assembly.LoadFrom;"
                : scenario == "inert-api-string"
                    ? "var marker = \"Assembly.LoadFrom\";\nvar basePath = Path.Combine(AppContext.BaseDirectory, \"mods\");\nvar dirs = Directory.GetDirectories(basePath);\nvar paths = dirs.Select(dir => Path.Combine(dir, $\"{Path.GetFileName(dir)}.dll\"));"
                : scenario == "tuple-unrelated-load"
                    ? "var basePath = Path.Combine(AppContext.BaseDirectory, \"mods\");\nvar dirs = Directory.GetDirectories(basePath);\nvar paths = dirs.Select(dir => Tuple.Create(Assembly.LoadFrom(\"Other.dll\"), Path.Combine(dir, $\"{Path.GetFileName(dir)}.dll\")));"
                        : scenario == "method-group-without-scan"
                        ? "var paths = new string[0]; paths.Select(Assembly.LoadFrom);"
                        : "var basePath = Path.Combine(AppContext.BaseDirectory, \"mods\"); var dirs = Directory.GetDirectories(basePath); var paths = dirs.Select(dir => Path.Combine(dir, $\"{Path.GetFileName(dir)}.dll\")); paths.Select(Assembly.LoadFrom);";
            if (scenario == "test-only-loader")
            {
                hostSource = "var app = WebApplication.Create();";
            }

            var destination = scenario switch
            {
                "wrong-scan-directory" => "$(MSBuildProjectDirectory)\\..\\..\\Shape.Host\\$(OutDir)other\\$(ProjectName)\\%(RecursiveDir)",
                "missing-project-name" => "$(MSBuildProjectDirectory)\\..\\..\\Shape.Host\\$(OutDir)mods",
                "literal-project-subfolder" => "$(MSBuildProjectDirectory)\\..\\..\\Shape.Host\\$(OutDir)mods\\Shape.One",
                "similar-folder-without-copy-proof" => "$(MSBuildProjectDirectory)\\..\\..\\Shape.Host\\$(OutDir)mods\\Other",
                "host-token-destination" => "$(MSBuildProjectDirectory)\\..\\..\\Shape.Host\\Host$(OutDir)mods\\$(ProjectName)\\%(RecursiveDir)",
                _ => "$(MSBuildProjectDirectory)\\..\\..\\Shape.Host\\$(OutDir)mods\\$(ProjectName)\\%(RecursiveDir)"
            };
            var item = scenario switch
            {
                "item-outside-target" => "<ItemGroup><PluginFiles Include=\"$(TargetDir)\\**\\*.*\" /></ItemGroup><Target Name=\"CopyPlugin\" AfterTargets=\"Build\"><Copy SourceFiles=\"@(PluginFiles)\" DestinationFolder=\"" + destination + "\" /></Target>",
                "item-non-output" => "<Target Name=\"CopyPlugin\" AfterTargets=\"Build\"><ItemGroup><PluginFiles Include=\"src\\**\\*.*\" /></ItemGroup><Copy SourceFiles=\"@(PluginFiles)\" DestinationFolder=\"" + destination + "\" /></Target>",
                "item-conflicting-definition" => "<Target Name=\"CopyPlugin\" AfterTargets=\"Build\"><ItemGroup><PluginFiles Include=\"$(TargetDir)\\**\\*.*\" /><PluginFiles Remove=\"$(TargetDir)\\obj\\**\\*.*\" /></ItemGroup><Copy SourceFiles=\"@(PluginFiles)\" DestinationFolder=\"" + destination + "\" /></Target>",
                "item-exclude" => "<Target Name=\"CopyPlugin\" AfterTargets=\"Build\"><ItemGroup><PluginFiles Include=\"$(TargetDir)\\**\\*.*\" Exclude=\"$(TargetDir)\\obj\\**\\*.*\" /></ItemGroup><Copy SourceFiles=\"@(PluginFiles)\" DestinationFolder=\"" + destination + "\" /></Target>",
                "item-conditional-definition" => "<Target Name=\"CopyPlugin\" AfterTargets=\"Build\"><ItemGroup Condition=\"'$(Configuration)' == 'Release'\"><PluginFiles Include=\"$(TargetDir)\\**\\*.*\" /></ItemGroup><Copy SourceFiles=\"@(PluginFiles)\" DestinationFolder=\"" + destination + "\" /></Target>",
                "conditional-copy" => "<Target Name=\"CopyPlugin\" AfterTargets=\"Build\"><ItemGroup><PluginFiles Include=\"$(TargetDir)\\**\\*.*\" /></ItemGroup><Copy Condition=\"'$(Configuration)' == 'Release'\" SourceFiles=\"@(PluginFiles)\" DestinationFolder=\"" + destination + "\" /></Target>",
                "different-target-item" => "<ItemGroup><PluginFiles Include=\"$(TargetDir)\\**\\*.*\" /></ItemGroup><Target Name=\"CopyPlugin\" AfterTargets=\"Build\"><Copy SourceFiles=\"@(PluginFiles)\" DestinationFolder=\"" + destination + "\" /></Target>",
                "conditional-target" => "<Target Condition=\"'$(Configuration)' == 'Release'\" Name=\"CopyPlugin\" AfterTargets=\"Build\"><ItemGroup><PluginFiles Include=\"$(TargetDir)\\**\\*.*\" /></ItemGroup><Copy SourceFiles=\"@(PluginFiles)\" DestinationFolder=\"" + destination + "\" /></Target>",
                _ => "<Target Name=\"CopyPlugin\" AfterTargets=\"Build\"><ItemGroup><PluginFiles Include=\"$(TargetDir)\\**\\*.*\" /></ItemGroup><Copy SourceFiles=\"@(PluginFiles)\" DestinationFolder=\"" + destination + "\" /></Target>"
            };
            var pluginProperties = scenario == "plugin-output-override" ? "<PropertyGroup><OutputPath>..\\other\\</OutputPath></PropertyGroup>" : "";
            if (scenario == "assembly-name-mismatch")
            {
                pluginProperties = "<PropertyGroup><AssemblyName>Different.Plugin</AssemblyName></PropertyGroup>";
            }
            var plugin = "<Project Sdk=\"Microsoft.NET.Sdk\">" + pluginProperties + item + "</Project>";
            var files = new Dictionary<string, string>
            {
                ["src/Shape.Host/Shape.Host.csproj"] = scenario == "host-output-override"
                    ? "<Project Sdk=\"Microsoft.NET.Sdk.Web\"><PropertyGroup><OutputPath>..\\other\\</OutputPath></PropertyGroup></Project>"
                    : "<Project Sdk=\"Microsoft.NET.Sdk.Web\" />",
                ["src/Shape.Host/Loader.cs"] = "using System.Reflection;\n" + hostSource,
                ["src/Shape.Plugins/Shape.One/Shape.One.csproj"] = plugin,
                ["src/Shape.Plugins/Shape.One/Plugin.cs"] = "public sealed class Plugin {}"
            };
            if (scenario == "duplicate-assembly-identity")
            {
                files["src/Shape.Plugins/Shape.Two/Shape.Two.csproj"] = plugin.Replace("</Project>", "<PropertyGroup><AssemblyName>Shape.One</AssemblyName></PropertyGroup></Project>");
                files["src/Shape.Plugins/Shape.Two/Plugin.cs"] = "public sealed class PluginTwo {}";
            }
            if (scenario == "test-only-loader")
            {
                files.Remove("src/Shape.Host/Loader.cs");
                files["tests/Shape.Tests/Shape.Tests.csproj"] = "<Project Sdk=\"Microsoft.NET.Sdk\"><ItemGroup><PackageReference Include=\"Microsoft.NET.Test.Sdk\" Version=\"17.11.1\" /></ItemGroup></Project>";
                files["tests/Shape.Tests/Loader.cs"] = "using System.Reflection;\n" + hostSource.Replace("var app = WebApplication.Create();", "var basePath = Path.Combine(AppContext.BaseDirectory, \"mods\"); var dirs = Directory.GetDirectories(basePath); var paths = dirs.Select(dir => Path.Combine(dir, $\"{Path.GetFileName(dir)}.dll\")); paths.Select(Assembly.LoadFrom);");
            }
            foreach (var pair in files)
            {
                var fullPath = Path.Combine(root, pair.Key.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
                await File.WriteAllTextAsync(fullPath, pair.Value, new UTF8Encoding(false));
            }
            return new NegativeFolderScanFixture(root);
        }

        public void Dispose()
        {
            var parent = Directory.GetParent(Root)!.FullName;
            if (Directory.Exists(parent)) Directory.Delete(parent, recursive: true);
        }
    }

    private sealed class PluginFixture : IDisposable
    {
        public const string SourceMarker = "PRIVATE_PLUGIN_POLICY_MARKER";

        private PluginFixture(string root) => Root = root;

        public string Root { get; }

        public static async Task<PluginFixture> CreateAsync(bool reverseCreationOrder = false)
        {
            var root = Path.Combine(Path.GetTempPath(), "pkc-discovery-plugins", Guid.NewGuid().ToString("N"), "repo");
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

            return new PluginFixture(root);
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
            yield return ("src/Portal.Host/Portal.Host.csproj", """
                <Project Sdk="Microsoft.NET.Sdk.Web">
                  <ItemGroup>
                    <ProjectReference Include="..\Portal.Core\Portal.Core.csproj" />
                  </ItemGroup>
                </Project>
                """);
            yield return ("src/Portal.Host/Program.cs", $"// {SourceMarker}\nvar app = WebApplication.Create(); PluginLoader.LoadAll(); app.Run();\n");
            yield return (Loader, """
                using System.Reflection;

                public static class PluginLoader
                {
                    public static void LoadAll(string configuredPath)
                    {
                        var reporting = Assembly.LoadFrom(Path.Combine(AppContext.BaseDirectory, "plugins", "Portal.Reporting.dll"));
                        var billing = Assembly.Load("Portal.Billing.Plugin");
                        var dynamic = Assembly.LoadFrom(configuredPath);
                        var ghost = Assembly.Load("Portal.Ghost");
                        var exporter = Assembly.Load(new AssemblyName("Portal.Exporter"));
                        // var audit = Assembly.Load("Portal.Audit");
                    }
                }
                """);

            yield return ("src/Portal.Core/Portal.Core.csproj", """<Project Sdk="Microsoft.NET.Sdk" />""");
            yield return ("src/Portal.Core/Contracts.cs", "public interface IPortalPlugin {}\n");

            // Output redirected into the host tree.
            yield return ("src/Plugins/Portal.Reporting/Portal.Reporting.csproj", """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <OutputPath>..\..\Portal.Host\bin\plugins\</OutputPath>
                  </PropertyGroup>
                  <ItemGroup>
                    <ProjectReference Include="..\..\Portal.Core\Portal.Core.csproj" />
                  </ItemGroup>
                </Project>
                """);
            yield return ("src/Plugins/Portal.Reporting/ReportingPlugin.cs", "public sealed class ReportingPlugin : IPortalPlugin {}\n");

            // Custom assembly name, copied into the host tree by an MSBuild Copy task.
            yield return ("src/Plugins/Portal.Billing/Portal.Billing.csproj", """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <AssemblyName>Portal.Billing.Plugin</AssemblyName>
                  </PropertyGroup>
                  <Target Name="CopyPlugin" AfterTargets="Build">
                    <Copy SourceFiles="$(TargetPath)" DestinationFolder="$(MSBuildProjectDirectory)\..\..\Portal.Host\bin\$(Configuration)\plugins" />
                  </Target>
                </Project>
                """);

            // Loaded by identity but never delivered to the host.
            yield return ("src/Plugins/Portal.Exporter/Portal.Exporter.csproj", """<Project Sdk="Microsoft.NET.Sdk" />""");

            // Delivered to the host but never loaded by identity.
            yield return ("src/Plugins/Portal.Audit/Portal.Audit.csproj", """
                <Project Sdk="Microsoft.NET.Sdk">
                  <Target Name="CopyPlugin" AfterTargets="Build">
                    <Copy SourceFiles="$(TargetPath)" DestinationFolder="..\..\Portal.Host\bin\plugins" />
                  </Target>
                </Project>
                """);

            yield return ("tests/Portal.Plugins.Tests/Portal.Plugins.Tests.csproj", """
                <Project Sdk="Microsoft.NET.Sdk">
                  <ItemGroup>
                    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
                  </ItemGroup>
                </Project>
                """);
            yield return ("tests/Portal.Plugins.Tests/LoaderTests.cs", """
                public sealed class LoaderTests
                {
                    public void Loads() => System.Reflection.Assembly.Load("Portal.Exporter");
                }
                """);
        }
    }

    private sealed class FolderScanPluginFixture : IDisposable
    {
        private FolderScanPluginFixture(string root) => Root = root;
        public string Root { get; }

        public static async Task<FolderScanPluginFixture> CreateAsync(bool directCall, string api = "LoadFrom", string scanDirectory = "mods", bool reverseCreationOrder = false, bool memberSpanning = false)
        {
            var root = Path.Combine(Path.GetTempPath(), "pkc-discovery-folder-scan", Guid.NewGuid().ToString("N"), "repo");
            Directory.CreateDirectory(root);
            var call = directCall
                ? "dirs.Select(dir => Assembly." + api + "(Path.Combine(dir, $\"{Path.GetFileName(dir)}.dll\")));"
                : "paths.Select(Assembly." + api + ");";
            var files = new Dictionary<string, string>
            {
                ["src/Shape.Host/Shape.Host.csproj"] = "<Project Sdk=\"Microsoft.NET.Sdk.Web\" />",
                ["src/Shape.Host/Loader.cs"] = memberSpanning ? MemberSpanningLoader(scanDirectory) : $"using System.Reflection;\nvar basePath = Path.Combine(AppContext.BaseDirectory, \"{scanDirectory}\");\nvar dirs = Directory.GetDirectories(basePath);\nvar paths = dirs.Select(dir => Path.Combine(dir, $\"{{Path.GetFileName(dir)}}.dll\"));\n{call}\n",
                ["src/Shape.Plugins/Shape.One/Shape.One.csproj"] = $"<Project Sdk=\"Microsoft.NET.Sdk\"><Target Name=\"CopyPlugin\" AfterTargets=\"Build\"><ItemGroup><PluginFiles Include=\"$(TargetDir)\\**\\*.*\" /></ItemGroup><Copy SourceFiles=\"@(PluginFiles)\" DestinationFolder=\"$(MSBuildProjectDirectory)\\..\\..\\Shape.Host\\$(OutDir){scanDirectory}\\$(ProjectName)\\%(RecursiveDir)\" /></Target></Project>",
                ["src/Shape.Plugins/Shape.One/Plugin.cs"] = "public sealed class Plugin {}",
                ["src/Shape.Plugins/Shape.Two/Shape.Two.csproj"] = $"<Project Sdk=\"Microsoft.NET.Sdk\"><Target Name=\"CopyPlugin\" AfterTargets=\"Build\"><ItemGroup><PluginFiles Include=\"$(TargetDir)\\**\\*.*\" /></ItemGroup><Copy SourceFiles=\"@(PluginFiles)\" DestinationFolder=\"$(MSBuildProjectDirectory)\\..\\..\\Shape.Host\\$(OutDir){scanDirectory}\\$(ProjectName)\\%(RecursiveDir)\" /></Target></Project>",
                ["src/Shape.Plugins/Shape.Two/Plugin.cs"] = "public sealed class Plugin {}"
            };
            files["src/Shape.Plugins/Shape.One/Shape.One.csproj"] = files["src/Shape.Plugins/Shape.One/Shape.One.csproj"].ToString()!.Replace(
                "</Target></Project>",
                "<Copy SourceFiles=\"@(PluginFiles)\" DestinationFolder=\"$(MSBuildProjectDirectory)\\..\\..\\Shape.Tests\\$(OutDir)tests\\$(ProjectName)\\%(RecursiveDir)\" /><Copy Condition=\"'$(Configuration)' == 'Release'\" SourceFiles=\"@(PluginFiles)\" DestinationFolder=\"$(PluginExternal)\\mods\\$(ProjectName)\" /></Target></Project>");
            var entries = reverseCreationOrder ? files.Reverse() : files;
            foreach (var pair in entries)
            {
                var fullPath = Path.Combine(root, pair.Key.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
                await File.WriteAllTextAsync(fullPath, pair.Value, new UTF8Encoding(false));
            }

            return new FolderScanPluginFixture(root);
        }

        // Loader split across members: a fluent projection chain over a helper that returns the enumerated directories.
        private static string MemberSpanningLoader(string scanDirectory) => $$"""
            using System.Reflection;

            public static class ShapeScanner
            {
                private static List<Assembly>? _loaded;
                public static List<Assembly> Loaded =>
                    _loaded ??= Folders
                        .Select(folder => Path.Combine(folder, $"{Path.GetFileName(folder)}.dll"))
                        .Where(File.Exists)
                        .Select(Assembly.LoadFrom)
                        .ToList();

                private static List<string> Folders => FindFolders();

                private static List<string> FindFolders()
                {
                    var root = Path.Combine(AppContext.BaseDirectory, "{{scanDirectory}}");
                    if (!Directory.Exists(root))
                    {
                        return [];
                    }

                    return Directory.GetDirectories(root).ToList();
                }
            }
            """;

        public void Dispose()
        {
            var parent = Directory.GetParent(Root)!.FullName;
            if (Directory.Exists(parent)) Directory.Delete(parent, recursive: true);
        }
    }
}
