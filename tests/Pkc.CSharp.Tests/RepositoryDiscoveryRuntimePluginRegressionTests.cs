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

    private static RepositoryComponent Component(RepositoryProfile profile, string id) =>
        Assert.Single(profile.Components, component => component.Id == id);

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
}
