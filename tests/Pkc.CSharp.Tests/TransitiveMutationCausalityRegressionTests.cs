using Pkc.CSharp;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class TransitiveMutationCausalityRegressionTests
{
    [Fact]
    public async Task Cross_stack_candidate_does_not_promote_unproven_transitive_runtime_object_mutations()
    {
        var root = CreateRoot("runtime-selected-object");

        try
        {
            await WriteProjectAsync(root, FixtureSource);

            var facts = await new CSharpEvidenceScanner().ScanAsync(root);
            var userMutation = Assert.Single(facts.Facts, fact =>
                fact.Kind == "mutation" &&
                fact.Metadata.GetValueOrDefault("target") == "user.UpdatedAt");
            var contactMutation = Assert.Single(facts.Facts, fact =>
                fact.Kind == "mutation" &&
                fact.Metadata.GetValueOrDefault("target") == "contact.UpdatedAt");

            Assert.Equal("runtime-pattern-variable", userMutation.Metadata["mutationReceiverOrigin"]);
            Assert.Equal("user", userMutation.Metadata["mutationReceiver"]);
            Assert.Equal("caller-object-unproven", userMutation.Metadata["mutationCausalityBoundary"]);
            Assert.Equal("runtime-pattern-variable", contactMutation.Metadata["mutationReceiverOrigin"]);
            Assert.Equal("contact", contactMutation.Metadata["mutationReceiver"]);
            Assert.Equal("caller-object-unproven", contactMutation.Metadata["mutationCausalityBoundary"]);

            var candidate = FindCandidate(facts, "CreateUser");

            Assert.DoesNotContain(candidate.Facts, fact =>
                fact.Kind == "mutation" &&
                fact.Metadata.GetValueOrDefault("target") == "user.UpdatedAt");
            Assert.DoesNotContain(candidate.Facts, fact =>
                fact.Kind == "mutation" &&
                fact.Metadata.GetValueOrDefault("target") == "contact.UpdatedAt");
            Assert.Contains(candidate.Unknowns, item =>
                item.Contains("transitive helper mutations", StringComparison.Ordinal));

            var knowledge = await new EvidenceAwareKnowledgeSynthesizer().SynthesizeAsync(candidate);
            Assert.DoesNotContain(knowledge.StateChanges, item =>
                item.Contains("UpdatedAt", StringComparison.Ordinal));

            var markdown = new MarkdownKnowledgeRenderer().Render(knowledge);
            Assert.DoesNotContain("Sets `user.UpdatedAt`", markdown, StringComparison.Ordinal);
            Assert.DoesNotContain("Sets `contact.UpdatedAt`", markdown, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Cross_stack_candidate_keeps_transitive_local_domain_object_mutation()
    {
        var root = CreateRoot("local-domain-object");

        try
        {
            await WriteProjectAsync(root, FixtureSource);

            var facts = await new CSharpEvidenceScanner().ScanAsync(root);
            var rawMutation = Assert.Single(facts.Facts, fact =>
                fact.Kind == "mutation" &&
                fact.Metadata.GetValueOrDefault("target") == "item.Stock");
            Assert.NotEqual(
                "runtime-pattern-variable",
                rawMutation.Metadata.GetValueOrDefault("mutationReceiverOrigin"));

            var candidate = FindCandidate(facts, "Restock");
            Assert.Contains(candidate.Facts, fact =>
                fact.Kind == "mutation" &&
                fact.Metadata.GetValueOrDefault("target") == "item.Stock");
            Assert.DoesNotContain(candidate.Unknowns, item =>
                item.Contains("transitive helper mutations", StringComparison.Ordinal));

            var knowledge = await new EvidenceAwareKnowledgeSynthesizer().SynthesizeAsync(candidate);
            Assert.Contains("Applies `+=` to `item.Stock` with `2`.", knowledge.StateChanges);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Cross_stack_candidate_keeps_transitive_self_owned_domain_mutation()
    {
        var root = CreateRoot("self-owned-domain-mutation");

        try
        {
            await WriteProjectAsync(root, FixtureSource);

            var facts = await new CSharpEvidenceScanner().ScanAsync(root);
            var candidate = FindCandidate(facts, "Complete");

            Assert.Contains(candidate.Facts, fact =>
                fact.Kind == "mutation" &&
                fact.Metadata.GetValueOrDefault("target") == "Status" &&
                fact.Metadata.GetValueOrDefault("targetSymbol") == "Demo.WorkPlay.Status");

            var knowledge = await new EvidenceAwareKnowledgeSynthesizer().SynthesizeAsync(candidate);
            Assert.Contains("Sets `Status` to `status`.", knowledge.StateChanges);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static FeatureCandidate FindCandidate(Pkc.Core.FactDocument facts, string endpointName)
    {
        var candidates = new CrossStackFeatureCandidateBuilder().Build(facts);
        return Assert.Single(candidates.Candidates, candidate =>
            candidate.Facts.Any(fact =>
                fact.Id == candidate.SeedFactId &&
                fact.Kind == "endpoint" &&
                fact.Name == endpointName));
    }

    private static string CreateRoot(string name)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pkc-transitive-mutation-causality-tests",
            name,
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static async Task WriteProjectAsync(string root, string source)
    {
        await File.WriteAllTextAsync(Path.Combine(root, "DemoMutation.csproj"), ProjectTemplate);
        await File.WriteAllTextAsync(Path.Combine(root, "DemoMutation.cs"), source);
    }

    private const string ProjectTemplate = """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>net8.0</TargetFramework>
            <ImplicitUsings>enable</ImplicitUsings>
            <Nullable>enable</Nullable>
            <AssemblyName>DemoMutation</AssemblyName>
          </PropertyGroup>
        </Project>
        """;

    private const string FixtureSource = """
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using System.Threading.Tasks;

        namespace Demo;

        [AttributeUsage(AttributeTargets.Class)]
        public sealed class RouteAttribute : Attribute
        {
            public RouteAttribute(string template) { }
        }

        [AttributeUsage(AttributeTargets.Method)]
        public sealed class HttpPostAttribute : Attribute
        {
            public HttpPostAttribute() { }
            public HttpPostAttribute(string template) { }
        }

        public enum EntryState
        {
            Added,
            Modified
        }

        public sealed class Entry
        {
            public object Entity { get; init; } = new object();
            public EntryState State { get; init; }
        }

        public sealed class User
        {
            public DateTime UpdatedAt { get; set; }
        }

        public sealed class Contact
        {
            public DateTime UpdatedAt { get; set; }
        }

        public sealed class FakeContext
        {
            private readonly List<Entry> _entries = new();

            public void Add(object entity) =>
                _entries.Add(new Entry { Entity = entity, State = EntryState.Added });

            public Task SaveChangesAsync()
            {
                SetTimestamps();
                return Task.CompletedTask;
            }

            private void SetTimestamps()
            {
                var entries = _entries.Where(entry => entry.State == EntryState.Modified);
                foreach (var entry in entries)
                {
                    if (entry.Entity is User user)
                        user.UpdatedAt = DateTime.UtcNow;
                    else if (entry.Entity is Contact contact)
                        contact.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        [Route("api/users")]
        public sealed class UsersController
        {
            private readonly FakeContext _db = new();

            [HttpPost("create")]
            public async Task CreateUser()
            {
                var user = new User();
                _db.Add(user);
                await _db.SaveChangesAsync();
            }
        }

        public sealed class StockItem
        {
            public int Stock { get; set; }
        }

        public sealed class InventoryStore
        {
            private readonly List<StockItem> _items = [new()];

            public void Restock() => ApplyRestock();

            private void ApplyRestock()
            {
                var item = _items[0];
                item.Stock += 2;
            }
        }

        [Route("api/inventory")]
        public sealed class InventoryController
        {
            private readonly InventoryStore _store = new();

            [HttpPost("restock")]
            public void Restock() => _store.Restock();
        }

        public enum WorkPlayStatus
        {
            Draft,
            Completed
        }

        public sealed class WorkPlay
        {
            public WorkPlayStatus Status { get; private set; }

            public void SetStatus(WorkPlayStatus status) => Status = status;
        }

        public sealed class WorkPlayService
        {
            public void ChangeStatus(WorkPlay workPlay, WorkPlayStatus status) =>
                workPlay.SetStatus(status);
        }

        [Route("api/workplays")]
        public sealed class WorkPlayController
        {
            private readonly WorkPlayService _service = new();

            [HttpPost("complete")]
            public void Complete()
            {
                var workPlay = new WorkPlay();
                _service.ChangeStatus(workPlay, WorkPlayStatus.Completed);
            }
        }
        """;
}
