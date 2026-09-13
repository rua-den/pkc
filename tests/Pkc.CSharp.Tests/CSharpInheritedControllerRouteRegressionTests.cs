using Pkc.CSharp;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class CSharpInheritedControllerRouteRegressionTests
{
    [Fact]
    public async Task ScanAsync_composes_inherited_controller_route_with_action_route()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "Controllers.cs"), Source);

            var result = await new CSharpRepositoryScanner().ScanAsync(root);

            Assert.Contains(result.Facts, fact =>
                fact.Kind == "endpoint" &&
                fact.Name == "Restart" &&
                fact.Metadata.TryGetValue("controllerRouteTemplate", out var controllerRoute) &&
                controllerRoute == "[controller]" &&
                fact.Metadata.TryGetValue("fullRoute", out var fullRoute) &&
                fullRoute == "System/Restart");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private const string Source = """
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
}
