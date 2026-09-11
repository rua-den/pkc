using Pkc.CSharp;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class CSharpErrorSemanticsTests
{
    [Fact]
    public async Task Synthesis_keeps_null_coalescing_throw_and_controller_error_response_mapping()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-error-semantics", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "Sample.cs"), Source);
            var facts = await new CSharpEvidenceScanner().ScanAsync(root);

            Assert.Contains(facts.Facts, fact =>
                fact.Kind == "condition" &&
                fact.Metadata.TryGetValue("sourceKind", out var sourceKind) &&
                sourceKind == "null-coalescing-throw" &&
                fact.Metadata["expression"].Contains("SingleOrDefault", StringComparison.Ordinal));

            Assert.Contains(facts.Facts, fact =>
                fact.Kind == "condition" &&
                fact.Metadata.TryGetValue("sourceKind", out var sourceKind) &&
                sourceKind == "exception-response" &&
                fact.Metadata["expression"].Contains("NotFound", StringComparison.Ordinal));

            var candidate = Assert.Single(new FeatureCandidateBuilder().Build(facts).Candidates);
            var knowledge = await new GroundedKnowledgeSynthesizer().SynthesizeAsync(candidate);

            Assert.Contains(knowledge.Rules, rule =>
                rule.Contains("SingleOrDefault", StringComparison.Ordinal) &&
                rule.Contains("KeyNotFoundException", StringComparison.Ordinal));

            Assert.Contains(knowledge.Rules, rule =>
                rule.Contains("catch KeyNotFoundException", StringComparison.Ordinal) &&
                rule.Contains("NotFound", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private const string Source = """
        using System.Collections.Generic;
        using System.Linq;

        namespace Demo;

        public sealed class Item { public int Id { get; init; } }

        public sealed class Store
        {
            private readonly List<Item> _items = [];
            public Item Find(int id) => _items.SingleOrDefault(item => item.Id == id)
                ?? throw new KeyNotFoundException($"Item {id} was not found.");
        }

        [Route("api/items")]
        public sealed class ItemsController
        {
            private readonly Store _store = new();

            [HttpGet("{id:int}")]
            public object Get(int id)
            {
                try { return Ok(_store.Find(id)); }
                catch (KeyNotFoundException exception) { return NotFound(new { message = exception.Message }); }
            }

            private object Ok(object value) => value;
            private object NotFound(object value) => value;
        }
        """;
}
