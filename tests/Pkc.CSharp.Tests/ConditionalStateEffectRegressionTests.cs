using Pkc.CSharp;
using Pkc.Core;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class ConditionalStateEffectRegressionTests
{
    [Fact]
    public async Task If_else_mutations_render_as_alternative_groups()
    {
        var knowledge = await Synthesize("""
            public sealed class Worker
            {
                public void Execute(Entity entity, bool noOpenRecords)
                {
                    if (noOpenRecords)
                    {
                        entity.A = false;
                    }
                    else
                    {
                        entity.B = true;
                        entity.C = false;
                        entity.D = false;
                    }
                }
            }

            public sealed class Entity
            {
                public bool A { get; set; }
                public bool B { get; set; }
                public bool C { get; set; }
                public bool D { get; set; }
            }
            """);

        var rendered = string.Join("\n", knowledge.StateChanges);
        Assert.Contains("When `noOpenRecords`:", rendered);
        Assert.Contains("Otherwise (when `noOpenRecords` is false):", rendered);
        Assert.DoesNotContain(knowledge.StateChanges, item =>
            item.StartsWith("Sets `entity.A`", StringComparison.Ordinal));
        Assert.Contains("entity.A", rendered);
        Assert.Contains("entity.B", rendered);
        var workflowMarkdown = new MarkdownKnowledgeRenderer().Render(knowledge);
        Assert.Contains("- Otherwise (when `noOpenRecords` is false):", workflowMarkdown);
        Assert.Contains("  - Sets `entity.B` to `true`.", workflowMarkdown);

        var feature = Assert.Single(new ProductFeatureBuilder().Build(new[] { knowledge }).Features);
        var featureMarkdown = new ProductFeatureMarkdownRenderer().Render(feature);
        Assert.Contains("When `noOpenRecords`:", featureMarkdown);
        Assert.Contains("  - Otherwise (when `noOpenRecords` is false):", featureMarkdown);
    }

    [Fact]
    public async Task Plain_if_preserves_unconditional_siblings()
    {
        var knowledge = await Synthesize("""
            public sealed class Worker
            {
                public void Execute(Entity entity, bool enabled)
                {
                    entity.Before = true;
                    if (enabled) entity.Conditional = true;
                    entity.After = false;
                }
            }

            public sealed class Entity
            {
                public bool Before { get; set; }
                public bool Conditional { get; set; }
                public bool After { get; set; }
            }
            """);

        Assert.Contains("Sets `entity.Before` to `true`.", knowledge.StateChanges);
        Assert.Contains("When `enabled`:", string.Join("\n", knowledge.StateChanges));
        Assert.Contains("Sets `entity.After` to `false`.", knowledge.StateChanges);
    }

    [Fact]
    public async Task Else_if_chain_and_switch_have_separate_arms()
    {
        var knowledge = await Synthesize("""
            public sealed class Worker
            {
                public void Execute(Entity entity, int mode)
                {
                    if (mode == 1) entity.A = true;
                    else if (mode == 2) entity.B = true;
                    else entity.C = true;

                    switch (mode)
                    {
                        case 3:
                        case 5: entity.D = true; break;
                        case 4: entity.E = true; break;
                        default: entity.F = true; break;
                    }
                }
            }

            public sealed class Entity
            {
                public bool A { get; set; }
                public bool B { get; set; }
                public bool C { get; set; }
                public bool D { get; set; }
                public bool E { get; set; }
                public bool F { get; set; }
            }
            """);

        var rendered = string.Join("\n", knowledge.StateChanges);
        Assert.Contains("When `mode == 1`:", rendered);
        Assert.Contains("When `mode == 2` after earlier branches do not match:", rendered);
        Assert.Contains("Otherwise (when all earlier branches do not match):", rendered);
        Assert.Contains("case:3|5", rendered);
        Assert.Contains("case:4", rendered);
        Assert.Contains("Otherwise", rendered);
        var switchGroup = Assert.Single(knowledge.StateChanges, item =>
            item.Contains("entity.D", StringComparison.Ordinal));
        Assert.Contains("entity.E", switchGroup);
        Assert.Contains("entity.F", switchGroup);
    }

    [Fact]
    public async Task Nested_and_non_renderable_conditions_keep_context()
    {
        var knowledge = await Synthesize("""
            public sealed class Worker
            {
                public void Execute(Entity entity, bool enabled, bool other)
                {
                    if (enabled)
                    {
                        if (other) entity.Nested = true;
                    }
                    if (GetCondition()) entity.Hidden = true;
                }

                private bool GetCondition() => true;
            }

            public sealed class Entity
            {
                public bool Nested { get; set; }
                public bool Hidden { get; set; }
            }
            """);

        var rendered = string.Join("\n", knowledge.StateChanges);
        Assert.Contains("When `enabled`:", rendered);
        Assert.Contains("When `other`:", rendered);
        Assert.Contains("condition not shown", rendered);
    }

    [Fact]
    public async Task Else_only_mutations_keep_the_governing_condition()
    {
        var knowledge = await Synthesize("""
            public sealed class Worker
            {
                public void Execute(Entity entity, bool enabled)
                {
                    if (enabled) { }
                    else entity.Value = true;
                }
            }

            public sealed class Entity
            {
                public bool Value { get; set; }
            }
            """);

        var rendered = string.Join("\n", knowledge.StateChanges);
        Assert.Contains("Otherwise (when `enabled` is false):", rendered);
        Assert.Contains("entity.Value", rendered);
    }

    [Fact]
    public async Task Non_renderable_else_if_keeps_alternative_wording_when_siblings_are_filtered()
    {
        var knowledge = await Synthesize("""
            public sealed class Worker
            {
                public void Execute(Entity entity, bool first)
                {
                    var local = false;
                    if (first) local = true;
                    else if (GetCondition()) entity.Value = true;
                    else local = false;
                }

                private bool GetCondition() => true;
            }

            public sealed class Entity
            {
                public bool Value { get; set; }
            }
            """);

        var rendered = string.Join("\n", knowledge.StateChanges);
        Assert.Contains("In an alternative branch (condition not shown):", rendered);
        Assert.DoesNotContain("In one branch (condition not shown):", rendered);
    }

    [Fact]
    public async Task Deep_nested_alternatives_remain_separate_with_bounded_indentation()
    {
        var knowledge = await Synthesize("""
            public sealed class Worker
            {
                public void Execute(Entity entity, bool a, bool b, bool c, bool d, bool e)
                {
                    if (a)
                    {
                        if (b)
                        {
                            if (c)
                            {
                                if (d)
                                {
                                    if (e) entity.Yes = true;
                                    else entity.No = true;
                                }
                                else entity.No = true;
                            }
                        }
                    }
                }
            }

            public sealed class Entity
            {
                public bool Yes { get; set; }
                public bool No { get; set; }
            }
            """);

        var rendered = string.Join("\n", knowledge.StateChanges);
        Assert.Contains("When `d`:", rendered);
        Assert.Contains("Otherwise (when `d` is false):", rendered);
        Assert.Contains("Under additional conditions:", rendered);
        Assert.Contains("Under `e`:", rendered);
        Assert.Contains("Under otherwise:", rendered);
        Assert.DoesNotContain("            ", rendered);

        var feature = Assert.Single(new ProductFeatureBuilder().Build(new[] { knowledge }).Features);
        var markdown = new ProductFeatureMarkdownRenderer().Render(feature);
        Assert.DoesNotContain("- - Sets", markdown);
    }

    [Fact]
    public async Task Unconditional_only_output_remains_flat_and_stable()
    {
        var knowledge = await Synthesize("""
            public sealed class Worker
            {
                public void Execute(Entity entity)
                {
                    entity.A = false;
                    entity.B = true;
                }
            }

            public sealed class Entity
            {
                public bool A { get; set; }
                public bool B { get; set; }
            }
            """);

        Assert.Equal(
            new[] { "Sets `entity.A` to `false`.", "Sets `entity.B` to `true`." },
            knowledge.StateChanges);
        var markdown = new MarkdownKnowledgeRenderer().Render(knowledge);
        Assert.Contains(
            "## State changes\r\n\r\n- Sets `entity.A` to `false`.\r\n- Sets `entity.B` to `true`.\r\n\r\n## Side effects",
            markdown);
    }

    [Fact]
    public async Task Lambda_mutations_do_not_inherit_the_outer_method_branch()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-conditional-lambda", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "Sample.cs"), """
                public sealed class Worker
                {
                    [HttpPost]
                    public void Execute(Entity entity, bool enabled)
                    {
                        if (enabled)
                        {
                            Action action = () => entity.Value = true;
                            action();
                            void SetLocal() { entity.Value = false; }
                            SetLocal();
                        }
                    }
                }

                public sealed class Entity
                {
                    public bool Value { get; set; }
                }
                """);

            var facts = await new CSharpEvidenceScanner().ScanAsync(root);
            var mutations = facts.Facts.Where(fact => fact.Kind == "mutation").ToArray();
            Assert.Equal(2, mutations.Length);
            Assert.All(mutations, mutation =>
            {
                Assert.Equal("[]", mutation.Metadata["branchPath"]);
                Assert.Equal("false", mutation.Metadata["branchConditional"]);
            });
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Branch_metadata_is_deterministic_across_file_creation_order()
    {
        var first = await ScanTwoFiles(false);
        var second = await ScanTwoFiles(true);

        var firstMutations = first.Facts.Where(fact => fact.Kind == "mutation")
            .Select(StableFact)
            .ToArray();
        var secondMutations = second.Facts.Where(fact => fact.Kind == "mutation")
            .Select(StableFact)
            .ToArray();
        Assert.Equal(firstMutations, secondMutations);
    }

    private static async Task<FeatureKnowledge> Synthesize(string source)
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-conditional-state-effects", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            source = source.Replace("public void Execute", "[HttpPost]\n                public void Execute", StringComparison.Ordinal);
            await File.WriteAllTextAsync(Path.Combine(root, "Sample.cs"), source);
            var facts = await new CSharpEvidenceScanner().ScanAsync(root);
            var candidate = Assert.Single(new FeatureCandidateBuilder().Build(facts).Candidates);
            return await new GroundedKnowledgeSynthesizer().SynthesizeAsync(candidate);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static async Task<FactDocument> ScanTwoFiles(bool reverse)
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-conditional-order", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var files = new Dictionary<string, string>
            {
                ["A.cs"] = """
                    public sealed class A
                    {
                        [HttpPost]
                        public void Execute(Entity entity, bool enabled)
                        {
                            if (enabled) entity.A = true;
                            else entity.B = false;
                        }
                    }
                    """,
                ["B.cs"] = """
                    public sealed class Entity
                    {
                        public bool A { get; set; }
                        public bool B { get; set; }
                    }
                    """
            };
            foreach (var pair in (reverse ? files.Reverse() : files).ToArray())
            {
                await File.WriteAllTextAsync(Path.Combine(root, pair.Key), pair.Value);
            }

            return await new CSharpEvidenceScanner().ScanAsync(root);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static string StableFact(EvidenceFact fact) =>
        $"{fact.Id}|{string.Join(";", fact.Metadata.OrderBy(item => item.Key).Select(item => $"{item.Key}={item.Value}"))}";
}
