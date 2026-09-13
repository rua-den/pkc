using Pkc.CSharp;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class CSharpInheritedControllerRouteRegressionTests
{
    [Fact]
    public async Task ScanAsync_composes_inherited_controller_route_with_action_route()
    {
        var result = await ScanAsync(InheritedRouteSource);

        Assert.Contains(result.Facts, fact =>
            fact.Kind == "endpoint" &&
            fact.Name == "Restart" &&
            fact.Metadata.TryGetValue("controllerRouteTemplate", out var controllerRoute) &&
            controllerRoute == "[controller]" &&
            fact.Metadata.TryGetValue("fullRoute", out var fullRoute) &&
            fullRoute == "System/Restart");
    }

    [Fact]
    public async Task ScanAsync_explicit_empty_controller_route_overrides_inherited_route()
    {
        var result = await ScanAsync(ExplicitRouteOverrideSource);

        Assert.Contains(result.Facts, fact =>
            fact.Kind == "endpoint" &&
            fact.Name == "GetItem" &&
            fact.Metadata.TryGetValue("fullRoute", out var fullRoute) &&
            fullRoute == "Items/{id}");

        Assert.DoesNotContain(result.Facts, fact =>
            fact.Kind == "endpoint" &&
            fact.Name == "GetItem" &&
            fact.Metadata.TryGetValue("fullRoute", out var fullRoute) &&
            fullRoute == "Library/Items/{id}");
    }

    private static async Task<Pkc.Core.FactDocument> ScanAsync(string source)
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "Controllers.cs"), source);
            return await new CSharpEvidenceScanner().ScanAsync(root);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private const string InheritedRouteSource = """
        namespace Demo.Api;

        [Route("[controller]")]
        public abstract class ApiControllerBase
        {
        }

        public sealed class SystemController : ApiControllerBase
        {
            [HttpPost("Restart")]
            public void Restart()
            {
            }
        }
        """;

    private const string ExplicitRouteOverrideSource = """
        namespace Demo.Api;

        [Route("[controller]")]
        public abstract class ApiControllerBase
        {
        }

        [Route("")]
        public sealed class LibraryController : ApiControllerBase
        {
            [HttpGet("Items/{id}")]
            public void GetItem()
            {
            }
        }
        """;
}
