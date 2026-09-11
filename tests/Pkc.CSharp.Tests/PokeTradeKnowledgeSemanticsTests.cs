using Pkc.CSharp;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class PokeTradeKnowledgeSemanticsTests
{
    [Fact]
    public async Task Synthesis_pairs_each_guard_with_its_own_throw_and_preserves_compound_mutations()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-semantics-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "Sample.cs"), Source);

            var facts = await new CSharpRepositoryScanner().ScanAsync(root);
            var candidate = Assert.Single(new FeatureCandidateBuilder().Build(facts).Candidates);
            var knowledge = await new GroundedKnowledgeSynthesizer().SynthesizeAsync(candidate);

            Assert.Contains(knowledge.Rules, rule =>
                rule.Contains("amount <= 0", StringComparison.Ordinal) &&
                rule.Contains("Amount must be positive", StringComparison.Ordinal));
            Assert.DoesNotContain(knowledge.Rules, rule =>
                rule.Contains("amount <= 0", StringComparison.Ordinal) &&
                rule.Contains("Stock must be available", StringComparison.Ordinal));

            Assert.Contains(knowledge.Rules, rule =>
                rule.Contains("order.Stock <= 0", StringComparison.Ordinal) &&
                rule.Contains("Stock must be available", StringComparison.Ordinal));
            Assert.DoesNotContain(knowledge.Rules, rule =>
                rule.Contains("order.Stock <= 0", StringComparison.Ordinal) &&
                rule.Contains("Amount must be positive", StringComparison.Ordinal));

            Assert.Contains("Applies `+=` to `order.Stock` with `amount`.", knowledge.StateChanges);
            Assert.DoesNotContain(knowledge.StateChanges, change => change.Contains("`Id`", StringComparison.Ordinal));
            Assert.DoesNotContain(knowledge.SideEffects, effect => effect.Contains("DispatchDelivery", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private const string Source = """
        namespace Demo;

        public sealed class Order
        {
            public int Id { get; init; }
            public int Stock { get; set; }
        }

        public sealed class Store
        {
            public void Execute(Order order, int amount)
            {
                if (amount <= 0)
                    throw new InvalidOperationException("Amount must be positive.");

                if (order.Stock <= 0)
                    throw new InvalidOperationException("Stock must be available.");

                order.Stock += amount;
                var created = new Order { Id = 123 };
                DispatchDelivery(created);
            }

            private static void DispatchDelivery(Order order) { }
        }

        [Route("api/orders")]
        public sealed class OrdersController
        {
            private readonly Store _store = new();

            [HttpPost("execute")]
            public void Execute(int amount)
            {
                _store.Execute(new Order(), amount);
            }
        }
        """;
}
