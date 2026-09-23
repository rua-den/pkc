using Pkc.CSharp;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class DirectDiDispatchAuthorityScopeRegressionTests
{
    [Fact]
    public async Task Scan_does_not_dispatch_from_uninvoked_helper_registration()
    {
        var root = CreateTempDirectory("dead-helper");
        try
        {
            await WriteWebProjectAsync(root, "Fixture");
            await File.WriteAllTextAsync(Path.Combine(root, "Program.cs"), """
                using Microsoft.AspNetCore.Builder;
                using Microsoft.AspNetCore.Http;
                using Microsoft.Extensions.DependencyInjection;

                var builder = WebApplication.CreateBuilder(args);
                builder.Services.AddScoped<IRunService>(_ => new RealRunService());
                var app = builder.Build();
                app.MapPost("/api/run", (IRunService service) => Results.Ok(service.RunAsync()));
                app.Run();

                public interface IRunService { string RunAsync(); }
                public sealed class RealRunService : IRunService { public string RunAsync() => "real"; }
                public sealed class FakeRunService : IRunService { public string RunAsync() => "fake"; }

                public static class UnusedRegistrations
                {
                    public static void Configure(IServiceCollection services)
                    {
                        services.AddScoped<IRunService, FakeRunService>();
                    }
                }
                """);

            var document = await new CSharpEvidenceScanner().ScanAsync(root);
            var endpoint = FindEndpoint(document, "/api/run");
            Assert.DoesNotContain(document.Relations, relation =>
                relation.FromFactId == endpoint.Id && relation.Kind == "dispatches");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Scan_does_not_dispatch_registration_from_different_host_using_same_contract_assembly()
    {
        var root = CreateTempDirectory("host-collision");
        try
        {
            var contracts = Path.Combine(root, "Contracts");
            var hostA = Path.Combine(root, "HostA");
            var hostB = Path.Combine(root, "HostB");
            Directory.CreateDirectory(contracts);
            Directory.CreateDirectory(hostA);
            Directory.CreateDirectory(hostB);

            await File.WriteAllTextAsync(Path.Combine(contracts, "Contracts.csproj"), """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
                </Project>
                """);
            await File.WriteAllTextAsync(Path.Combine(contracts, "IRunService.cs"), """
                namespace Shared;
                public interface IRunService { string RunAsync(); }
                """);

            await WriteWebProjectAsync(hostA, "HostA", "../Contracts/Contracts.csproj");
            await File.WriteAllTextAsync(Path.Combine(hostA, "Program.cs"), """
                using Microsoft.AspNetCore.Builder;
                using Microsoft.Extensions.DependencyInjection;
                using Shared;

                var builder = WebApplication.CreateBuilder(args);
                builder.Services.AddScoped<IRunService, WrongRunService>();
                var app = builder.Build();
                app.Run();

                public sealed class WrongRunService : IRunService
                {
                    public string RunAsync() => "wrong-host";
                }
                """);

            await WriteWebProjectAsync(hostB, "HostB", "../Contracts/Contracts.csproj");
            await File.WriteAllTextAsync(Path.Combine(hostB, "Program.cs"), """
                using Microsoft.AspNetCore.Builder;
                using Microsoft.AspNetCore.Http;
                using Microsoft.Extensions.DependencyInjection;
                using Shared;

                var builder = WebApplication.CreateBuilder(args);
                builder.Services.AddScoped<IRunService>(_ => new RealRunService());
                var app = builder.Build();
                app.MapPost("/api/run", (IRunService service) => Results.Ok(service.RunAsync()));
                app.Run();

                public sealed class RealRunService : IRunService
                {
                    public string RunAsync() => "real-host";
                }
                """);

            var document = await new CSharpEvidenceScanner().ScanAsync(root);
            var endpoint = Assert.Single(document.Facts, fact =>
                fact.Kind == "endpoint" &&
                fact.Source.Path.StartsWith("HostB/", StringComparison.Ordinal) &&
                fact.Metadata.TryGetValue("fullRoute", out var route) && route == "/api/run");

            Assert.DoesNotContain(document.Relations, relation =>
                relation.FromFactId == endpoint.Id && relation.Kind == "dispatches");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static EvidenceFact FindEndpoint(FactDocument document, string route) =>
        Assert.Single(document.Facts, fact =>
            fact.Kind == "endpoint" &&
            fact.Metadata.TryGetValue("fullRoute", out var fullRoute) && fullRoute == route);

    private static string CreateTempDirectory(string suffix)
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "pkc-di-authority-" + suffix + "-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static Task WriteWebProjectAsync(
        string directory,
        string assemblyName,
        string? projectReference = null)
    {
        var reference = projectReference is null
            ? string.Empty
            : $"<ProjectReference Include=\"{projectReference}\" />";
        return File.WriteAllTextAsync(Path.Combine(directory, assemblyName + ".csproj"), $$"""
            <Project Sdk="Microsoft.NET.Sdk.Web">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
                <AssemblyName>{{assemblyName}}</AssemblyName>
              </PropertyGroup>
              <ItemGroup>
                <FrameworkReference Include="Microsoft.AspNetCore.App" />
                {{reference}}
              </ItemGroup>
            </Project>
            """);
    }
}
