using Pkc.CSharp;
using Pkc.Core;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class ConditionalStateEffectMixedDepthRegressionTests
{
    [Fact]
    public async Task Direct_and_nested_mutations_in_the_same_branch_render_without_crashing()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-conditional-mixed-depth", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "Sample.cs"), """
                public sealed class Worker
                {
                    [HttpPost]
                    public void Execute(Entity entity, bool enabled, bool nested)
                    {
                        if (enabled)
                        {
                            entity.Primary = true;
                            if (nested)
                            {
                                entity.Secondary = true;
                            }
                        }
                    }
                }

                public sealed class Entity
                {
                    public bool Primary { get; set; }
                    public bool Secondary { get; set; }
                }
                """);

            var facts = await new CSharpEvidenceScanner().ScanAsync(root);
            var candidate = Assert.Single(new FeatureCandidateBuilder().Build(facts).Candidates);
            var knowledge = await new GroundedKnowledgeSynthesizer().SynthesizeAsync(candidate);

            var rendered = string.Join("\n", knowledge.StateChanges);
            Assert.Contains("When `enabled`:", rendered);
            Assert.Contains("Sets `entity.Primary` to `true`.", rendered);
            Assert.Contains("When `nested`:", rendered);
            Assert.Contains("Sets `entity.Secondary` to `true`.", rendered);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
