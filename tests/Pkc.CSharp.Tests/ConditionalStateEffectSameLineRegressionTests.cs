using Pkc.CSharp;
using Pkc.Core;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class ConditionalStateEffectSameLineRegressionTests
{
    [Fact]
    public async Task Same_line_if_else_assignments_to_same_target_survive_and_keep_alternative_arms()
    {
        var (facts, knowledge) = await ScanAndSynthesize("""
            public sealed class Worker
            {
                [HttpPost]
                public void Execute(Entity entity, bool enabled) { if (enabled) entity.Value = true; else entity.Value = false; }
            }

            public sealed class Entity
            {
                public bool Value { get; set; }
            }
            """);

        var mutations = facts.Facts
            .Where(fact => fact.Kind == "mutation" && fact.Metadata.TryGetValue("target", out var target) && target == "entity.Value")
            .ToArray();
        Assert.Equal(2, mutations.Length);
        Assert.Equal(2, mutations.Select(fact => fact.Id).Distinct(StringComparer.Ordinal).Count());

        var rendered = string.Join("\n", knowledge.StateChanges);
        Assert.Contains("When `enabled`:", rendered);
        Assert.Contains("Sets `entity.Value` to `true`.", rendered);
        Assert.Contains("Otherwise (when `enabled` is false):", rendered);
        Assert.Contains("Sets `entity.Value` to `false`.", rendered);
    }

    [Fact]
    public async Task Identical_same_line_if_else_assignments_use_occurrence_ordinal()
    {
        var (facts, knowledge) = await ScanAndSynthesize("""
            public sealed class Worker
            {
                [HttpPost]
                public void Execute(Entity entity, bool enabled) { if (enabled) entity.Value = true; else entity.Value = true; }
            }

            public sealed class Entity
            {
                public bool Value { get; set; }
            }
            """);

        var mutations = facts.Facts
            .Where(fact => fact.Kind == "mutation" && fact.Metadata.TryGetValue("target", out var target) && target == "entity.Value")
            .ToArray();
        Assert.Equal(2, mutations.Length);
        Assert.All(mutations, fact => Assert.True(fact.Metadata.ContainsKey("mutationCollisionOrdinal")));

        var rendered = string.Join("\n", knowledge.StateChanges);
        Assert.Contains("When `enabled`:", rendered);
        Assert.Contains("Otherwise (when `enabled` is false):", rendered);
        Assert.Equal(2, CountOccurrences(rendered, "Sets `entity.Value` to `true`."));
    }

    [Fact]
    public async Task Same_line_unary_mutations_to_same_target_survive_and_keep_their_arms()
    {
        var (facts, knowledge) = await ScanAndSynthesize("""
            public sealed class Worker
            {
                [HttpPost]
                public void Execute(Entity entity, bool enabled) { if (enabled) entity.Count++; else entity.Count--; }
            }

            public sealed class Entity
            {
                public int Count { get; set; }
            }
            """);

        var mutations = facts.Facts
            .Where(fact => fact.Kind == "mutation" && fact.Metadata.TryGetValue("target", out var target) && target == "entity.Count")
            .ToArray();
        Assert.Equal(2, mutations.Length);

        var rendered = string.Join("\n", knowledge.StateChanges);
        Assert.Contains("When `enabled`:", rendered);
        Assert.Contains("Increments `entity.Count` by 1.", rendered);
        Assert.Contains("Otherwise (when `enabled` is false):", rendered);
        Assert.Contains("Decrements `entity.Count` by 1.", rendered);
    }

    private static async Task<(FactDocument Facts, FeatureKnowledge Knowledge)> ScanAndSynthesize(string source)
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-conditional-same-line", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "Sample.cs"), source);
            var facts = await new CSharpEvidenceScanner().ScanAsync(root);
            var candidate = Assert.Single(new FeatureCandidateBuilder().Build(facts).Candidates);
            var knowledge = await new GroundedKnowledgeSynthesizer().SynthesizeAsync(candidate);
            return (facts, knowledge);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var start = 0;
        while ((start = text.IndexOf(value, start, StringComparison.Ordinal)) >= 0)
        {
            count++;
            start += value.Length;
        }

        return count;
    }
}
