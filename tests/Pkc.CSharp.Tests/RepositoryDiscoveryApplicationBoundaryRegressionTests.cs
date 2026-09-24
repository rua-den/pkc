using System.Text;
using Pkc.Core.Discovery;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class RepositoryDiscoveryApplicationBoundaryRegressionTests
{
    private const string Api = "dotnet:src/Platform.Api/Platform.Api.csproj";
    private const string AdminMvc = "dotnet:src/Platform.Admin.Mvc/Platform.Admin.Mvc.csproj";
    private const string Worker = "dotnet:src/Platform.Worker/Platform.Worker.csproj";
    private const string Tool = "dotnet:src/Platform.Tool/Platform.Tool.csproj";
    private const string Domain = "dotnet:src/Platform.Domain/Platform.Domain.csproj";
    private const string Infrastructure = "dotnet:src/Platform.Infrastructure/Platform.Infrastructure.csproj";
    private const string Reporting = "dotnet:src/Platform.Reporting/Platform.Reporting.csproj";
    private const string ReportingApi = "dotnet:src/Platform.Reporting.Api/Platform.Reporting.Api.csproj";
    private const string Conditional = "dotnet:src/Platform.Conditional/Platform.Conditional.csproj";
    private const string Broken = "dotnet:src/Platform.Broken/Platform.Broken.csproj";
    private const string InheritedOutput = "dotnet:src/Inherited/Platform.InheritedOutput/Platform.InheritedOutput.csproj";
    private const string ApiTests = "dotnet:tests/Platform.Api.Tests/Platform.Api.Tests.csproj";
    private const string TestHost = "dotnet:tests/Platform.TestHost/Platform.TestHost.csproj";
    private const string AngularAdmin = "angular:frontend/workspace/angular.json#admin";
    private const string AngularSharedUi = "angular:frontend/workspace/angular.json#shared-ui";
    private const string AngularUndeclared = "angular:frontend/workspace/angular.json#undeclared";
    private const string AngularBuilderOnly = "angular:frontend/workspace/angular.json#builder-only";
    private const string AngularCustomBuilder = "angular:frontend/workspace/angular.json#custom-builder";

    [Fact]
    public async Task Application_and_component_identity_comes_from_build_manifests()
    {
        using var fixture = await PlatformFixture.CreateAsync();
        var profile = new RepositoryDiscovery().Discover(fixture.Root);

        var kinds = profile.Components.ToDictionary(component => component.Id, component => component.Kind, StringComparer.Ordinal);
        Assert.Equal(ComponentKind.WebHost, kinds[Api]);
        Assert.Equal(ComponentKind.WebHost, kinds[AdminMvc]);
        Assert.Equal(ComponentKind.WebHost, kinds[ReportingApi]);
        Assert.Equal(ComponentKind.WorkerHost, kinds[Worker]);
        Assert.Equal(ComponentKind.ExecutableHost, kinds[Tool]);
        Assert.Equal(ComponentKind.Library, kinds[Domain]);
        Assert.Equal(ComponentKind.Library, kinds[Infrastructure]);
        Assert.Equal(ComponentKind.Library, kinds[Reporting]);
        Assert.Equal(ComponentKind.TestProject, kinds[ApiTests]);
        Assert.Equal(ComponentKind.TestProject, kinds[TestHost]);
        Assert.Equal(ComponentKind.AngularApplication, kinds[AngularAdmin]);
        Assert.Equal(ComponentKind.AngularLibrary, kinds[AngularSharedUi]);

        // Identity that depends on unevaluated inheritance or is undeclared stays UNKNOWN.
        Assert.Equal(ComponentKind.Unknown, kinds[InheritedOutput]);
        Assert.Equal(DiscoveryConfidence.Unknown, Component(profile, InheritedOutput).Confidence);
        Assert.Equal(ComponentKind.Unknown, kinds[AngularUndeclared]);
        Assert.Equal(ComponentKind.Unknown, kinds[AngularCustomBuilder]);

        // Without projectType, an official Angular application builder is manifest evidence of an application.
        Assert.Equal(ComponentKind.AngularApplication, kinds[AngularBuilderOnly]);
        Assert.Contains(Component(profile, AngularBuilderOnly).Evidence, evidence => evidence.Kind == "angular-application-builder");
        Assert.Equal("frontend/workspace", Component(profile, AngularBuilderOnly).AreaPath);

        Assert.Equal("src/Platform.Api", Component(profile, Api).AreaPath);
        Assert.Equal("frontend/workspace/projects/admin", Component(profile, AngularAdmin).AreaPath);
        Assert.Equal(new[] { "Platform.sln" }, Component(profile, Api).Solutions);
        Assert.Equal(new[] { "Platform.sln", "legacy/Admin.slnx" }, Component(profile, AdminMvc).Solutions);
        Assert.Empty(Component(profile, Tool).Solutions);

        Assert.Contains(Component(profile, AdminMvc).Evidence, evidence => evidence.Kind == "msbuild-web-application-project-type");
        Assert.Contains(Component(profile, Api).Evidence, evidence => evidence.Kind == "msbuild-web-sdk");
        Assert.Contains(Component(profile, Worker).Evidence, evidence => evidence.Kind == "msbuild-worker-sdk");
        Assert.All(profile.Components.Where(component => component.Kind != ComponentKind.Unknown),
            component => Assert.NotEmpty(component.Evidence));
    }

    [Fact]
    public async Task Shared_modules_have_every_production_host_that_references_them_as_owner()
    {
        using var fixture = await PlatformFixture.CreateAsync();
        var profile = new RepositoryDiscovery().Discover(fixture.Root);

        Assert.Equal(new[] { AdminMvc, Api, Worker }, Component(profile, Domain).Owners);
        Assert.Equal(OwnershipStatus.Owned, Component(profile, Domain).Ownership);
        Assert.Equal(new[] { Api, Worker }, Component(profile, Infrastructure).Owners);

        Assert.Equal(new[] { Api }, Component(profile, Api).Owners);
        Assert.Equal(OwnershipStatus.Host, Component(profile, Api).Ownership);
        Assert.Equal(OwnershipStatus.Host, Component(profile, AngularAdmin).Ownership);

        var edges = profile.Edges
            .Select(edge => $"{edge.From} -> {edge.To} [{edge.Kind}/{edge.Confidence}]")
            .ToArray();
        Assert.Equal(
            new[]
            {
                $"{AdminMvc} -> {Domain} [project-reference/High]",
                $"{Api} -> {Conditional} [project-reference/Unknown]",
                $"{Api} -> {Domain} [project-reference/High]",
                $"{Api} -> {Infrastructure} [project-reference/High]",
                $"{Infrastructure} -> {Domain} [project-reference/High]",
                $"{Worker} -> {Infrastructure} [project-reference/High]",
                $"{ApiTests} -> {Api} [project-reference/High]",
                $"{ApiTests} -> {Reporting} [project-reference/High]",
                $"{TestHost} -> {Domain} [project-reference/High]"
            }.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            edges);
        Assert.All(profile.Edges, edge => Assert.NotEmpty(edge.Evidence));

        // A conditional reference is visible but does not prove ownership.
        Assert.Empty(Component(profile, Conditional).Owners);
        Assert.Equal(OwnershipStatus.Unknown, Component(profile, Conditional).Ownership);
    }

    [Fact]
    public async Task Tests_attach_as_evidence_and_never_create_production_ownership()
    {
        using var fixture = await PlatformFixture.CreateAsync();
        var profile = new RepositoryDiscovery().Discover(fixture.Root);

        var testIds = profile.Components
            .Where(component => component.Kind == ComponentKind.TestProject)
            .Select(component => component.Id)
            .ToArray();
        Assert.Equal(new[] { ApiTests, TestHost }, testIds);

        foreach (var component in profile.Components)
        {
            Assert.DoesNotContain(component.Owners, owner => testIds.Contains(owner));
        }

        foreach (var test in testIds)
        {
            Assert.Equal(OwnershipStatus.TestOnly, Component(profile, test).Ownership);
            Assert.Empty(Component(profile, test).Owners);
            Assert.Equal(SourceRole.TestEvidence, Component(profile, test).Role);
        }

        // Reporting is referenced only by a test project: still no production owner.
        Assert.Empty(Component(profile, Reporting).Owners);
        Assert.Equal(OwnershipStatus.Unknown, Component(profile, Reporting).Ownership);
        Assert.Equal(new[] { ApiTests }, Component(profile, Reporting).TestReferences);
        Assert.Equal(new[] { ApiTests }, Component(profile, Api).TestReferences);
        Assert.Equal(new[] { TestHost }, Component(profile, Domain).TestReferences);
        Assert.Equal(new[] { AdminMvc, Api, Worker }, Component(profile, Domain).Owners);
    }

    [Fact]
    public async Task Similar_names_never_create_edges_or_ownership()
    {
        using var fixture = await PlatformFixture.CreateAsync();
        var profile = new RepositoryDiscovery().Discover(fixture.Root);

        Assert.DoesNotContain(profile.Edges, edge =>
            (edge.From == ReportingApi && edge.To == Reporting) ||
            (edge.From == Reporting && edge.To == ReportingApi));
        Assert.DoesNotContain(ReportingApi, Component(profile, Reporting).Owners);
        Assert.Equal(new[] { ReportingApi }, Component(profile, ReportingApi).Owners);

        Assert.DoesNotContain(profile.Edges, edge =>
            edge.From.StartsWith("angular:", StringComparison.Ordinal) ||
            edge.To.StartsWith("angular:", StringComparison.Ordinal));
        Assert.Empty(Component(profile, AngularSharedUi).Owners);
        Assert.Equal(OwnershipStatus.Unknown, Component(profile, AngularSharedUi).Ownership);
    }

    [Fact]
    public async Task Unresolvable_references_are_reported_instead_of_guessed()
    {
        using var fixture = await PlatformFixture.CreateAsync();
        var profile = new RepositoryDiscovery().Discover(fixture.Root);

        var unresolved = profile.UnresolvedReferences
            .Select(reference => $"{reference.From} | {reference.Reference} | {reference.Reason}")
            .ToArray();
        Assert.Equal(
            new[]
            {
                $"{Broken} | $(SharedRoot)/Shared.csproj | msbuild-property-not-evaluated",
                $"{Broken} | src/Missing/Missing.csproj | project-file-not-found"
            },
            unresolved);
        Assert.DoesNotContain(profile.Edges, edge => edge.From == Broken);
        Assert.Equal(OwnershipStatus.Unknown, Component(profile, Broken).Ownership);
    }

    [Fact]
    public async Task Application_graph_is_deterministic_and_reads_only_manifests()
    {
        var first = await PlatformFixture.CreateAsync(reverseCreationOrder: false);
        var second = await PlatformFixture.CreateAsync(reverseCreationOrder: true);
        try
        {
            var discovery = new RepositoryDiscovery();
            var json = RepositoryProfileSerializer.Serialize(discovery.Discover(first.Root));
            Assert.Equal(json, RepositoryProfileSerializer.Serialize(discovery.Discover(second.Root)));
            Assert.DoesNotContain(first.Root, json, StringComparison.OrdinalIgnoreCase);

            var profile = discovery.Discover(first.Root);
            Assert.Equal(profile.Components.Select(c => c.Id).OrderBy(id => id, StringComparer.Ordinal), profile.Components.Select(c => c.Id));
            Assert.All(profile.Components, component =>
            {
                Assert.Equal(component.Owners.OrderBy(id => id, StringComparer.Ordinal), component.Owners);
                Assert.Equal(component.TestReferences.OrderBy(id => id, StringComparer.Ordinal), component.TestReferences);
            });

            var manifestKinds = profile.Manifests.ToDictionary(manifest => manifest.Path, manifest => manifest.Kind, StringComparer.Ordinal);
            Assert.All(profile.ContentReads, read => Assert.Contains(
                manifestKinds[read],
                new[] { "dotnet-project", "dotnet-solution", "msbuild-directory-props", "msbuild-directory-targets", "angular-workspace", "typescript-config" }));
            Assert.Contains("Platform.sln", profile.ContentReads);
            Assert.Contains("legacy/Admin.slnx", profile.ContentReads);
            Assert.DoesNotContain(PlatformFixture.SourceMarker, json, StringComparison.Ordinal);
        }
        finally
        {
            first.Dispose();
            second.Dispose();
        }
    }

    private static RepositoryComponent Component(RepositoryProfile profile, string id) =>
        Assert.Single(profile.Components, component => component.Id == id);

    private sealed class PlatformFixture : IDisposable
    {
        public const string SourceMarker = "PRIVATE_COMPOSITION_MARKER";

        private PlatformFixture(string root) => Root = root;

        public string Root { get; }

        public static async Task<PlatformFixture> CreateAsync(bool reverseCreationOrder = false)
        {
            var root = Path.Combine(Path.GetTempPath(), "pkc-discovery-apps", Guid.NewGuid().ToString("N"), "repo");
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

            return new PlatformFixture(root);
        }

        public void Dispose()
        {
            var parent = Directory.GetParent(Root)!.FullName;
            if (Directory.Exists(parent))
            {
                Directory.Delete(parent, recursive: true);
            }
        }

        private static string SdkProject(string sdk, string properties = "", params string[] references) => $"""
            <Project Sdk="{sdk}">
              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>{properties}
              </PropertyGroup>
              <ItemGroup>
            {string.Concat(references.Select(reference => $"    <ProjectReference Include=\"{reference}\" />\n"))}  </ItemGroup>
            </Project>
            """;

        private static IEnumerable<(string Path, string Content)> Files()
        {
            yield return ("Platform.sln", """
                Microsoft Visual Studio Solution File, Format Version 12.00
                Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "Platform.Api", "src\Platform.Api\Platform.Api.csproj", "{11111111-1111-1111-1111-111111111111}"
                EndProject
                Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Platform.Admin.Mvc", "src\Platform.Admin.Mvc\Platform.Admin.Mvc.csproj", "{22222222-2222-2222-2222-222222222222}"
                EndProject
                Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "src", "src", "{33333333-3333-3333-3333-333333333333}"
                EndProject
                Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "Ghost", "src\Ghost\Ghost.csproj", "{44444444-4444-4444-4444-444444444444}"
                EndProject
                """);
            yield return ("legacy/Admin.slnx", """
                <Solution>
                  <Project Path="../src/Platform.Admin.Mvc/Platform.Admin.Mvc.csproj" />
                </Solution>
                """);

            yield return ("src/Platform.Api/Platform.Api.csproj", """
                <Project Sdk="Microsoft.NET.Sdk.Web">
                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                  </PropertyGroup>
                  <ItemGroup>
                    <ProjectReference Include="..\Platform.Domain\Platform.Domain.csproj" />
                    <ProjectReference Include="../Platform.Infrastructure/Platform.Infrastructure.csproj" />
                  </ItemGroup>
                  <ItemGroup Condition="'$(Configuration)' == 'Debug'">
                    <ProjectReference Include="..\Platform.Conditional\Platform.Conditional.csproj" />
                  </ItemGroup>
                </Project>
                """);
            yield return ("src/Platform.Api/Program.cs", $"// {SourceMarker}\nvar app = WebApplication.Create(); app.Run();\n");

            yield return ("src/Platform.Admin.Mvc/Platform.Admin.Mvc.csproj", """
                <?xml version="1.0" encoding="utf-8"?>
                <Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
                  <PropertyGroup>
                    <ProjectTypeGuids>{349c5851-65df-11da-9384-00065b846f21};{fae04ec0-301f-11d3-bf4b-00c04f79efbc}</ProjectTypeGuids>
                    <OutputType>Library</OutputType>
                  </PropertyGroup>
                  <ItemGroup>
                    <ProjectReference Include="..\Platform.Domain\Platform.Domain.csproj">
                      <Project>{55555555-5555-5555-5555-555555555555}</Project>
                    </ProjectReference>
                  </ItemGroup>
                </Project>
                """);
            yield return ("src/Platform.Admin.Mvc/Global.asax.cs", "public class MvcApplication {}\n");

            yield return ("src/Platform.Worker/Platform.Worker.csproj",
                SdkProject("Microsoft.NET.Sdk.Worker", "", "..\\Platform.Infrastructure\\Platform.Infrastructure.csproj"));
            yield return ("src/Platform.Worker/Worker.cs", "public sealed class Worker {}\n");

            yield return ("src/Platform.Tool/Platform.Tool.csproj",
                SdkProject("Microsoft.NET.Sdk", "\n    <OutputType>Exe</OutputType>"));
            yield return ("src/Platform.Tool/Program.cs", "System.Console.WriteLine();\n");

            yield return ("src/Platform.Domain/Platform.Domain.csproj", SdkProject("Microsoft.NET.Sdk"));
            yield return ("src/Platform.Domain/Order.cs", "public sealed class Order {}\n");

            yield return ("src/Platform.Infrastructure/Platform.Infrastructure.csproj",
                SdkProject("Microsoft.NET.Sdk", "", "..\\Platform.Domain\\Platform.Domain.csproj"));

            // Similar names, no reference between them.
            yield return ("src/Platform.Reporting/Platform.Reporting.csproj", SdkProject("Microsoft.NET.Sdk"));
            yield return ("src/Platform.Reporting.Api/Platform.Reporting.Api.csproj", SdkProject("Microsoft.NET.Sdk.Web"));

            yield return ("src/Platform.Conditional/Platform.Conditional.csproj", SdkProject("Microsoft.NET.Sdk"));

            yield return ("src/Platform.Broken/Platform.Broken.csproj",
                SdkProject("Microsoft.NET.Sdk", "", "..\\Missing\\Missing.csproj", "$(SharedRoot)\\Shared.csproj"));

            // OutputType is inherited from an unevaluated Directory.Build.props: identity stays UNKNOWN.
            yield return ("src/Inherited/Directory.Build.props", """
                <Project>
                  <PropertyGroup>
                    <OutputType>Exe</OutputType>
                  </PropertyGroup>
                </Project>
                """);
            yield return ("src/Inherited/Platform.InheritedOutput/Platform.InheritedOutput.csproj", SdkProject("Microsoft.NET.Sdk"));

            yield return ("tests/Platform.Api.Tests/Platform.Api.Tests.csproj", """
                <Project Sdk="Microsoft.NET.Sdk">
                  <ItemGroup>
                    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
                    <PackageReference Include="xunit" Version="2.9.2" />
                    <ProjectReference Include="..\..\src\Platform.Api\Platform.Api.csproj" />
                    <ProjectReference Include="..\..\src\Platform.Reporting\Platform.Reporting.csproj" />
                  </ItemGroup>
                </Project>
                """);

            // A web-SDK test host is test evidence, not a production web host.
            yield return ("tests/Platform.TestHost/Platform.TestHost.csproj", """
                <Project Sdk="Microsoft.NET.Sdk.Web">
                  <PropertyGroup>
                    <IsTestProject>true</IsTestProject>
                  </PropertyGroup>
                  <ItemGroup>
                    <ProjectReference Include="..\..\src\Platform.Domain\Platform.Domain.csproj" />
                  </ItemGroup>
                </Project>
                """);

            yield return ("frontend/workspace/angular.json", """
                {
                  "version": 1,
                  "projects": {
                    "admin": { "projectType": "application", "root": "projects/admin", "sourceRoot": "projects/admin/src" },
                    "shared-ui": { "projectType": "library", "root": "projects/shared-ui" },
                    "undeclared": { "root": "projects/undeclared" },
                    "builder-only": {
                      "root": "",
                      "architect": { "build": { "builder": "@angular-devkit/build-angular:application" } }
                    },
                    "custom-builder": {
                      "root": "projects/custom-builder",
                      "architect": { "build": { "builder": "@acme/builders:application" } }
                    }
                  }
                }
                """);
            yield return ("frontend/workspace/package.json", """{ "name": "workspace", "private": true }""");
            yield return ("frontend/workspace/projects/admin/src/main.ts", "bootstrapApplication(AppComponent);\n");
            yield return ("frontend/workspace/projects/shared-ui/src/public-api.ts", "export * from './button';\n");
        }
    }
}
