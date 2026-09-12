using Pkc.CSharp;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class CSharpInitializerMutationTests
{
    [Fact]
    public async Task Build_does_not_render_dictionary_initializer_entries_as_domain_state_changes()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-initializer-mutation-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "Fixture.csproj"), """
                <Project Sdk="Microsoft.NET.Sdk.Web">
                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                  </PropertyGroup>
                </Project>
                """);

            await File.WriteAllTextAsync(Path.Combine(root, "Program.cs"), """
                var builder = WebApplication.CreateBuilder(args);
                builder.Services.AddSingleton<ApprovalService>();
                var app = builder.Build();

                app.MapPost("/api/approve", (ApprovalService service) =>
                {
                    service.BuildRequest();
                    return Results.Ok();
                });

                public static class Keys
                {
                    public const string Branch = "branch";
                }

                public sealed class ApprovalService
                {
                    public void BuildRequest()
                    {
                        Dictionary<string, string> target = new()
                        {
                            [Keys.Branch] = "main"
                        };
                    }
                }
                """);

            var document = await new CSharpEvidenceScanner().ScanAsync(root);

            var initializerAssignment = Assert.Single(document.Facts, fact =>
                fact.Kind == "initializer-assignment" &&
                fact.Metadata.TryGetValue("target", out var target) &&
                target == "[Keys.Branch]");
            Assert.Equal("initializer", initializerAssignment.Metadata["mutationContext"]);
            Assert.Equal("false", initializerAssignment.Metadata["stateMutationCandidate"]);

            var candidate = Assert.Single(new FeatureCandidateBuilder().Build(document).Candidates);
            var knowledge = await new GroundedKnowledgeSynthesizer().SynthesizeAsync(candidate);

            Assert.DoesNotContain(knowledge.StateChanges, change =>
                change.Contains("Keys.Branch", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
