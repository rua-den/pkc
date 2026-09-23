using System.Diagnostics;
using System.IO.Compression;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class RepeatedBuildKnowledgeParityTests
{
    [Fact]
    public async Task Rebuild_removes_stale_generated_knowledge_but_preserves_user_files()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pkc-repeated-build-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(root, "ParityApp.csproj"),
                """
                <Project Sdk="Microsoft.NET.Sdk.Web">
                  <PropertyGroup>
                    <TargetFramework>net10.0</TargetFramework>
                    <Nullable>enable</Nullable>
                  </PropertyGroup>
                </Project>
                """);

            var sourcePath = Path.Combine(root, "OrdersController.cs");
            await File.WriteAllTextAsync(sourcePath, Source(includeObsolete: true));

            await RunPkcBuildAsync(root);

            var workflowsRoot = Path.Combine(root, "knowledge", "workflows");
            var obsoletePath = Directory.EnumerateFiles(workflowsRoot, "*.md", SearchOption.AllDirectories)
                .Single(path => File.ReadAllText(path).Contains("Obsolete", StringComparison.Ordinal));
            var obsoleteRelativePath = Path.GetRelativePath(root, obsoletePath).Replace('\\', '/');

            var userFile = Path.Combine(root, "knowledge", "user-notes.md");
            const string userContent = "# User-owned notes\n\nDo not delete this file.\n";
            await File.WriteAllTextAsync(userFile, userContent);

            await File.WriteAllTextAsync(sourcePath, Source(includeObsolete: false));
            await RunPkcBuildAsync(root);

            Assert.False(File.Exists(obsoletePath));
            Assert.True(File.Exists(userFile));
            Assert.Equal(userContent, await File.ReadAllTextAsync(userFile));

            var bundle = await File.ReadAllTextAsync(Path.Combine(root, "PKC_KNOWLEDGE.md"));
            Assert.DoesNotContain(obsoleteRelativePath, bundle, StringComparison.Ordinal);
            Assert.DoesNotContain("Obsolete", bundle, StringComparison.Ordinal);

            using var archive = ZipFile.OpenRead(Path.Combine(root, "PKC_KNOWLEDGE.zip"));
            Assert.DoesNotContain(
                archive.Entries,
                entry => string.Equals(entry.FullName, obsoleteRelativePath, StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static string Source(bool includeObsolete) => includeObsolete
        ? """
          using Microsoft.AspNetCore.Mvc;

          [ApiController]
          [Route("api/orders")]
          public sealed class OrdersController : ControllerBase
          {
              [HttpGet("current")]
              public IActionResult Current() => Ok();

              [HttpGet("obsolete")]
              public IActionResult Obsolete() => Ok();
          }
          """
        : """
          using Microsoft.AspNetCore.Mvc;

          [ApiController]
          [Route("api/orders")]
          public sealed class OrdersController : ControllerBase
          {
              [HttpGet("current")]
              public IActionResult Current() => Ok();
          }
          """;

    private static async Task RunPkcBuildAsync(string repositoryPath)
    {
        var repositoryRoot = FindRepositoryRoot();
        var outputDirectory = new DirectoryInfo(AppContext.BaseDirectory);
        var targetFramework = outputDirectory.Name;
        var configuration = outputDirectory.Parent?.Name ?? "Release";
        var cliPath = Path.Combine(
            repositoryRoot,
            "src",
            "Pkc.Cli",
            "bin",
            configuration,
            targetFramework,
            "pkc.dll");

        Assert.True(File.Exists(cliPath), $"PKC CLI was not built at {cliPath}");

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add(cliPath);
        startInfo.ArgumentList.Add("build");
        startInfo.ArgumentList.Add(repositoryPath);

        using var process = new Process { StartInfo = startInfo };
        Assert.True(process.Start());
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        var stdout = await stdoutTask;
        var stderr = await stderrTask;

        Assert.True(
            process.ExitCode == 0,
            $"PKC build failed with exit code {process.ExitCode}.{Environment.NewLine}{stdout}{Environment.NewLine}{stderr}");
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "PKC.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate PKC.sln from the test output directory.");
    }
}
