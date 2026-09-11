using Pkc.CSharp;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class CSharpAdvancedEvidenceTests
{
    [Fact]
    public async Task Scan_and_synthesis_preserve_domain_construction_computation_loop_and_policy_semantics()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pkc-csharp-advanced-tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "Sample.cs"), Source);

            var facts = await new CSharpEvidenceScanner().ScanAsync(root);

            Assert.Contains(facts.Facts, fact =>
                fact.Kind == "computed-property" &&
                fact.Name == "Total" &&
                fact.Metadata["expression"].Contains("Quantity * line.UnitPrice", StringComparison.Ordinal));

            Assert.Contains(facts.Facts, fact =>
                fact.Kind == "object-construction" &&
                fact.Name == "WorkPlay" &&
                fact.Metadata["assignments"].Contains("QuantityToBuy = shortage + order.ReorderLevel", StringComparison.Ordinal) &&
                fact.Metadata["assignments"].Contains("Type = \"PurchaseStock\"", StringComparison.Ordinal) &&
                !fact.Metadata["assignments"].Contains("_nextWorkPlayId", StringComparison.Ordinal));

            Assert.Contains(facts.Facts, fact =>
                fact.Kind == "loop" &&
                fact.Metadata["collection"].Contains("OrderStatus.AwaitingStock", StringComparison.Ordinal) &&
                fact.Metadata["collection"].Contains("OrderBy", StringComparison.Ordinal));

            Assert.Contains(facts.Facts, fact =>
                fact.Kind == "authorization-policy" &&
                fact.Name == "ManageWorkPlay" &&
                fact.Metadata["definition"].Contains("RequireAssertion", StringComparison.Ordinal));

            var candidate = Assert.Single(
                new FeatureCandidateBuilder().Build(facts).Candidates,
                candidate => candidate.Name.Contains("Execute", StringComparison.Ordinal));

            var knowledge = await new GroundedKnowledgeSynthesizer().SynthesizeAsync(candidate);

            Assert.Contains(knowledge.Rules, rule =>
                rule.Contains("Computed property", StringComparison.Ordinal) &&
                rule.Contains("Order.Total", StringComparison.Ordinal) &&
                rule.Contains("Quantity * line.UnitPrice", StringComparison.Ordinal));

            Assert.Contains(knowledge.Rules, rule =>
                rule.Contains("Creates `WorkPlay`", StringComparison.Ordinal) &&
                rule.Contains("QuantityToBuy = shortage + order.ReorderLevel", StringComparison.Ordinal) &&
                rule.Contains("PurchaseStock", StringComparison.Ordinal));

            Assert.Contains(knowledge.Rules, rule =>
                rule.Contains("Iterates", StringComparison.Ordinal) &&
                rule.Contains("OrderStatus.AwaitingStock", StringComparison.Ordinal) &&
                rule.Contains("OrderBy", StringComparison.Ordinal));

            Assert.Contains(knowledge.Permissions, permission =>
                permission.Contains("Policy definition observed: ManageWorkPlay", StringComparison.Ordinal) &&
                permission.Contains("RequireAssertion", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private const string Source = """
        using System;
        using System.Collections.Generic;
        using System.Linq;

        namespace Demo;

        public enum OrderStatus
        {
            AwaitingStock,
            ReadyForDelivery
        }

        public sealed class OrderLine
        {
            public int Quantity { get; init; }
            public decimal UnitPrice { get; init; }
        }

        public sealed class Order
        {
            public int Id { get; init; }
            public int ReorderLevel { get; init; }
            public OrderStatus Status { get; set; }
            public List<OrderLine> Lines { get; init; } = [];
            public decimal Total => Lines.Sum(line => line.Quantity * line.UnitPrice);
        }

        public sealed class WorkPlay
        {
            public int Id { get; init; }
            public int OrderId { get; init; }
            public int QuantityToBuy { get; init; }
            public string Type { get; init; } = "";
            public string Reason { get; init; } = "";
        }

        public sealed class Store
        {
            private readonly List<Order> _orders = [];
            private readonly List<WorkPlay> _workPlays = [];
            private int _nextWorkPlayId = 1;

            public Order Execute(Order order)
            {
                var shortage = 2;
                _workPlays.Add(new WorkPlay
                {
                    Id = _nextWorkPlayId++,
                    OrderId = order.Id,
                    QuantityToBuy = shortage + order.ReorderLevel,
                    Type = "PurchaseStock",
                    Reason = $"Order #{order.Id} is short."
                });

                Fulfill();
                return order;
            }

            private void Fulfill()
            {
                foreach (var order in _orders
                    .Where(order => order.Status == OrderStatus.AwaitingStock)
                    .OrderBy(order => order.Id))
                {
                    order.Status = OrderStatus.ReadyForDelivery;
                }
            }
        }

        public sealed class SecuritySetup
        {
            public void Configure(dynamic options)
            {
                options.AddPolicy("ManageWorkPlay", policy => policy.RequireAssertion(_ => true));
            }
        }

        [Route("api/orders")]
        public sealed class OrdersController
        {
            private readonly Store _store = new();

            [HttpPost("execute")]
            [Authorize(Policy = "ManageWorkPlay")]
            public Order Execute(Order order) => _store.Execute(order);
        }
        """;
}
