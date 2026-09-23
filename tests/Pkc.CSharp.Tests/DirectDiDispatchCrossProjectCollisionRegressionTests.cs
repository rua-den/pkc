using Pkc.CSharp;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class DirectDiDispatchCrossProjectCollisionRegressionTests
{
    [Fact]
    public async Task Scan_does_not_dispatch_across_same_named_service_types_from_different_projects()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pkc-di-cross-project-collision-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var projectA = Path.Combine(root, "ProjectA");
            var projectB = Path.Combine(root, "ProjectB");
            Directory.CreateDirectory(projectA);
            Directory.CreateDirectory(projectB);

            await WriteWebProjectAsync(projectA, "ProjectA");
            await WriteWebProjectAsync(projectB, "ProjectB");

            await File.WriteAllTextAsync(Path.Combine(projectA, "Program.cs"), """
                using Microsoft.AspNetCore.Builder;
                using Microsoft.Extensions.DependencyInjection;
                using Shared;

                var builder = WebApplication.CreateBuilder(args);
                builder.Services.AddScoped<IRunService, WrongRunService>();
                var app = builder.Build();
                app.Run();
                """);

            await File.WriteAllTextAsync(Path.Combine(projectA, "Shared.cs"), """
                namespace Shared;

                public interface IRunService
                {
                    string RunAsync();
                }

                public sealed class WrongRunService : IRunService
                {
                    public string RunAsync() => "wrong-project";
                }
                """);

            await File.WriteAllTextAsync(Path.Combine(projectB, "Program.cs"), """
                using Microsoft.AspNetCore.Builder;
                using Microsoft.AspNetCore.Http;
                using Microsoft.Extensions.DependencyInjection;
                using Shared;

                var builder = WebApplication.CreateBuilder(args);
                builder.Services.AddScoped<IRunService>(_ => new RealRunService());
                var app = builder.Build();
                app.MapPost("/api/run", (IRunService service) => Results.Ok(service.RunAsync()));
                app.Run();
                """);

            await File.WriteAllTextAsync(Path.Combine(projectB, "Shared.cs"), """
                namespace Shared;

                public interface IRunService
                {
                    string RunAsync();
                }

                public sealed class RealRunService : IRunService
                {
                    public string RunAsync() => "real-project";
                }
                """);

            var document = await new CSharpEvidenceScanner().ScanAsync(root);
            var endpoint = Assert.Single(document.Facts, fact =>
                fact.Kind == "endpoint" &&
                fact.Source.Path.StartsWith("ProjectB/", StringComparison.Ordinal) &&
                fact.Metadata.TryGetValue("fullRoute", out var route) && route == "/api/run");

            Assert.Contains(document.Relations, relation =>
                relation.FromFactId == endpoint.Id &&
                relation.Kind == "invokes" &&
                relation.Target.EndsWith("Shared.IRunService.RunAsync", StringComparison.Ordinal));

            Assert.DoesNotContain(document.Relations, relation =>
                relation.FromFactId == endpoint.Id && relation.Kind == "dispatches");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static Task WriteWebProjectAsync(string directory, string assemblyName) =>
        File.WriteAllTextAsync(Path.Combine(directory, assemblyName + ".csproj"), $$"""
            <Project Sdk="Microsoft.NET.Sdk.Web">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
                <AssemblyName>{{assemblyName}}</AssemblyName>
                <Nullable>enable</Nullable>
              </PropertyGroup>
              <ItemGroup>
                <FrameworkReference Include="Microsoft.AspNetCore.App" />
              </ItemGroup>
            </Project>
            """);
}
