using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Pkc.Core;
using Pkc.CSharp;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class ValueLineageSnapshotRegressionTests
{
    [Fact]
    public async Task Scalar_copy_snapshot_lineage_reaches_workflow_markdown_and_matches_runtime_behavior()
    {
        var root = CreateRoot("snapshot");

        try
        {
            await WriteProjectAsync(root, "DemoLineage", FixtureSource);

            var runtime = CompileFixture(FixtureSource);
            try
            {
                var result = Invoke(runtime.Assembly, "GetSnapshot");
                Assert.Equal(120m, ReadDecimal(result, "GroupPrice"));
                Assert.Equal(100m, ReadDecimal(result, "ProductPrice"));
                Assert.Equal(100m, ReadDecimal(result, "ServicePrice"));
            }
            finally
            {
                runtime.Context.Unload();
            }

            var facts = await new CSharpEvidenceScanner().ScanAsync(root);
            var transfers = facts.Facts
                .Where(fact => fact.Kind == "value-transfer" &&
                               fact.Metadata.GetValueOrDefault("scopeName") == "GetSnapshot")
                .OrderBy(fact => fact.Source.StartLine)
                .ToArray();

            Assert.Equal(2, transfers.Length);
            var groupToProduct = Assert.Single(transfers, fact =>
                fact.Metadata.GetValueOrDefault("sourceOccurrence") == "group.Price" &&
                fact.Metadata.GetValueOrDefault("targetOccurrence") == "product.Price");
            var productToService = Assert.Single(transfers, fact =>
                fact.Metadata.GetValueOrDefault("sourceOccurrence") == "product.Price" &&
                fact.Metadata.GetValueOrDefault("targetOccurrence") == "service.Price");

            Assert.Equal("copy", groupToProduct.Metadata["mechanism"]);
            Assert.Equal("snapshot", groupToProduct.Metadata["temporalSemantics"]);
            Assert.Equal("project-semantic", groupToProduct.Metadata["analysisMode"]);
            Assert.Equal("continues-proven-chain", productToService.Metadata["compositionStatus"]);
            Assert.Equal(groupToProduct.Id, productToService.Metadata["predecessorTransferFactId"]);
            Assert.NotEqual(
                groupToProduct.Metadata["sourceReceiverIdentity"],
                groupToProduct.Metadata["targetReceiverIdentity"]);

            var candidate = FindCandidate(facts, "GetSnapshot");
            Assert.Equal(2, candidate.Facts.Count(fact => fact.Kind == "value-transfer"));

            var knowledge = await new EvidenceAwareKnowledgeSynthesizer().SynthesizeAsync(candidate);
            Assert.Contains(knowledge.ValueLineage, item =>
                item.Contains("Proven stored lineage chain", StringComparison.Ordinal) &&
                item.Contains("`group.Price` → `product.Price` → `service.Price`", StringComparison.Ordinal));
            Assert.Contains(knowledge.ValueLineage, item =>
                item.Contains("Stored snapshot copy: `group.Price` → `product.Price`", StringComparison.Ordinal));
            Assert.Contains(knowledge.ValueLineage, item =>
                item.Contains("Stored snapshot copy: `product.Price` → `service.Price`", StringComparison.Ordinal));
            Assert.DoesNotContain(knowledge.Rules, rule =>
                rule.Contains("group.Price", StringComparison.Ordinal) ||
                rule.Contains("product.Price", StringComparison.Ordinal));

            var markdown = new MarkdownKnowledgeRenderer().Render(knowledge);
            Assert.Contains("## Value lineage", markdown, StringComparison.Ordinal);
            Assert.Contains("`group.Price` → `product.Price` → `service.Price`", markdown, StringComparison.Ordinal);
            Assert.Contains("direct scalar auto-property copy stored as a snapshot", markdown, StringComparison.Ordinal);
            Assert.Contains("Catalog.cs:L", markdown, StringComparison.Ordinal);
            Assert.Contains("Value lineage transfer", markdown, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task False_composition_setter_branch_and_mutable_reference_shapes_fail_closed()
    {
        var root = CreateRoot("negative-shapes");

        try
        {
            await WriteProjectAsync(root, "DemoLineage", FixtureSource);

            var runtime = CompileFixture(FixtureSource);
            try
            {
                var collision = Invoke(runtime.Assembly, "GetCollision");
                Assert.Equal(100m, ReadDecimal(collision, "ProductAPrice"));
                Assert.Equal(99m, ReadDecimal(collision, "ProductBPrice"));
                Assert.Equal(99m, ReadDecimal(collision, "ServicePrice"));

                var overwritten = Invoke(runtime.Assembly, "GetOverwritten");
                Assert.Equal(50m, ReadDecimal(overwritten, "ProductPrice"));
                Assert.Equal(50m, ReadDecimal(overwritten, "ServicePrice"));

                Assert.Equal(15m, Assert.IsType<decimal>(Invoke(runtime.Assembly, "GetSetter")));
                Assert.Equal(7, Assert.IsType<int>(Invoke(runtime.Assembly, "GetSharedReference")));

                var branchTrue = Invoke(runtime.Assembly, "GetBranch", true);
                Assert.Equal(100m, ReadDecimal(branchTrue, "ProductPrice"));
                Assert.Equal(0m, ReadDecimal(branchTrue, "ServicePrice"));

                var branchFalse = Invoke(runtime.Assembly, "GetBranch", false);
                Assert.Equal(0m, ReadDecimal(branchFalse, "ProductPrice"));
                Assert.Equal(0m, ReadDecimal(branchFalse, "ServicePrice"));
            }
            finally
            {
                runtime.Context.Unload();
            }

            var facts = await new CSharpEvidenceScanner().ScanAsync(root);

            var collisionKnowledge = await SynthesizeAsync(facts, "GetCollision");
            Assert.DoesNotContain(collisionKnowledge.ValueLineage, item =>
                item.Contains("Proven stored lineage chain", StringComparison.Ordinal));
            Assert.Contains(collisionKnowledge.ValueLineage, item =>
                item.Contains("`group.Price` → `productA.Price`", StringComparison.Ordinal));
            Assert.Contains(collisionKnowledge.ValueLineage, item =>
                item.Contains("`productB.Price` → `service.Price`", StringComparison.Ordinal));
            Assert.DoesNotContain(collisionKnowledge.ValueLineage, item =>
                item.Contains("dto.Price", StringComparison.OrdinalIgnoreCase) ||
                item.Contains("component.price", StringComparison.OrdinalIgnoreCase));

            var overwriteKnowledge = await SynthesizeAsync(facts, "GetOverwritten");
            Assert.DoesNotContain(overwriteKnowledge.ValueLineage, item =>
                item.Contains("Proven stored lineage chain", StringComparison.Ordinal));
            Assert.Contains(overwriteKnowledge.ValueLineage, item =>
                item.Contains("not composed with an earlier `product.Price` lineage", StringComparison.Ordinal));

            foreach (var endpoint in new[] { "GetBranch", "GetSetter", "GetSharedReference" })
            {
                var candidate = FindCandidate(facts, endpoint);
                Assert.DoesNotContain(candidate.Facts, fact => fact.Kind == "value-transfer");
                var knowledge = await new EvidenceAwareKnowledgeSynthesizer().SynthesizeAsync(candidate);
                Assert.Empty(knowledge.ValueLineage);
            }
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Missing_target_project_semantics_do_not_produce_proven_lineage()
    {
        var root = CreateRoot("no-project");

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "Catalog.cs"), FixtureSource);

            var facts = await new CSharpEvidenceScanner().ScanAsync(root);

            Assert.DoesNotContain(facts.Facts, fact => fact.Kind == "value-transfer");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Same_display_members_in_distinct_projects_keep_project_and_assembly_identity()
    {
        var root = CreateRoot("project-collision");

        try
        {
            var projectA = Path.Combine(root, "A");
            var projectB = Path.Combine(root, "B");
            Directory.CreateDirectory(projectA);
            Directory.CreateDirectory(projectB);

            await WriteProjectAsync(
                projectA,
                "AssemblyA",
                ProjectCollisionSource.Replace("__CONTROLLER__", "AController", StringComparison.Ordinal));
            await WriteProjectAsync(
                projectB,
                "AssemblyB",
                ProjectCollisionSource.Replace("__CONTROLLER__", "BController", StringComparison.Ordinal));

            var facts = await new CSharpEvidenceScanner().ScanAsync(root);
            var transfers = facts.Facts
                .Where(fact => fact.Kind == "value-transfer")
                .ToArray();

            Assert.Equal(2, transfers.Length);
            Assert.All(transfers, transfer =>
                Assert.Equal("Shared.ProductGroup.Price", transfer.Metadata["sourceMember"]));
            Assert.Equal(
                2,
                transfers.Select(transfer => transfer.Metadata["sourceMemberIdentity"])
                    .Distinct(StringComparer.Ordinal)
                    .Count());
            Assert.Equal(
                2,
                transfers.Select(transfer => transfer.Metadata["semanticProject"])
                    .Distinct(StringComparer.Ordinal)
                    .Count());
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static async Task<FeatureKnowledge> SynthesizeAsync(FactDocument facts, string endpointName)
    {
        var candidate = FindCandidate(facts, endpointName);
        return await new EvidenceAwareKnowledgeSynthesizer().SynthesizeAsync(candidate);
    }

    private static FeatureCandidate FindCandidate(FactDocument facts, string endpointName)
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
            "pkc-v047-lineage-tests",
            name,
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static async Task WriteProjectAsync(string root, string assemblyName, string source)
    {
        var project = ProjectTemplate.Replace("__ASSEMBLY__", assemblyName, StringComparison.Ordinal);
        await File.WriteAllTextAsync(Path.Combine(root, $"{assemblyName}.csproj"), project);
        await File.WriteAllTextAsync(Path.Combine(root, "Catalog.cs"), source);
    }

    private static RuntimeFixture CompileFixture(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        var trustedAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string ??
                                throw new InvalidOperationException("Trusted platform assemblies are unavailable.");
        var references = trustedAssemblies
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(path => MetadataReference.CreateFromFile(path))
            .ToArray();
        var compilation = CSharpCompilation.Create(
            $"PkcV047Runtime_{Guid.NewGuid():N}",
            [tree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var stream = new MemoryStream();
        var emit = compilation.Emit(stream);
        Assert.True(
            emit.Success,
            string.Join(
                Environment.NewLine,
                emit.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)));

        stream.Position = 0;
        var context = new AssemblyLoadContext($"pkc-v047-{Guid.NewGuid():N}", isCollectible: true);
        return new RuntimeFixture(context, context.LoadFromStream(stream));
    }

    private static object Invoke(Assembly assembly, string methodName, params object?[] arguments)
    {
        var controllerType = assembly.GetType("Demo.PricesController", throwOnError: true)!;
        var controller = Activator.CreateInstance(controllerType)!;
        var method = controllerType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(method);
        return method!.Invoke(controller, arguments)!;
    }

    private static decimal ReadDecimal(object instance, string propertyName)
    {
        var property = instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(property);
        return Assert.IsType<decimal>(property!.GetValue(instance));
    }

    private sealed record RuntimeFixture(AssemblyLoadContext Context, Assembly Assembly);

    private const string ProjectTemplate = """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>net8.0</TargetFramework>
            <ImplicitUsings>enable</ImplicitUsings>
            <Nullable>enable</Nullable>
            <AssemblyName>__ASSEMBLY__</AssemblyName>
          </PropertyGroup>
        </Project>
        """;

    private const string FixtureSource = """
        using System;

        namespace Demo;

        [AttributeUsage(AttributeTargets.Class)]
        public sealed class RouteAttribute : Attribute
        {
            public RouteAttribute(string template) { }
        }

        [AttributeUsage(AttributeTargets.Method)]
        public sealed class HttpGetAttribute : Attribute
        {
            public HttpGetAttribute() { }
            public HttpGetAttribute(string template) { }
        }

        public sealed class ProductGroup
        {
            public decimal Price { get; set; }
        }

        public sealed class Product
        {
            public decimal Price { get; set; }
        }

        public sealed class Service
        {
            public decimal Price { get; set; }
        }

        public sealed class Dto
        {
            public decimal Price { get; set; }
        }

        public sealed class Component
        {
            public decimal price { get; set; }
        }

        public sealed class PlainPrice
        {
            public decimal Price { get; set; }
        }

        public sealed class AdjustedPrice
        {
            private decimal _stored;

            public decimal Price
            {
                get => _stored;
                set => _stored = value + 10m;
            }
        }

        public sealed class Box
        {
            public MutablePayload Payload { get; set; } = new();
        }

        public sealed class MutablePayload
        {
            public int Value { get; set; }
        }

        public sealed record SnapshotResult(decimal GroupPrice, decimal ProductPrice, decimal ServicePrice);
        public sealed record CollisionResult(
            decimal ProductAPrice,
            decimal ProductBPrice,
            decimal ServicePrice,
            decimal DtoPrice,
            decimal ComponentPrice);
        public sealed record OverwriteResult(decimal ProductPrice, decimal ServicePrice);
        public sealed record BranchResult(decimal ProductPrice, decimal ServicePrice);

        [Route("api/prices")]
        public sealed class PricesController
        {
            [HttpGet("snapshot")]
            public SnapshotResult GetSnapshot()
            {
                var group = new ProductGroup();
                var product = new Product();
                var service = new Service();
                group.Price = 100m;
                product.Price = group.Price;
                service.Price = product.Price;
                group.Price = 120m;
                return new SnapshotResult(group.Price, product.Price, service.Price);
            }

            [HttpGet("collision")]
            public CollisionResult GetCollision()
            {
                var group = new ProductGroup();
                var productA = new Product();
                var productB = new Product();
                var service = new Service();
                var dto = new Dto();
                var component = new Component();
                group.Price = 100m;
                productB.Price = 99m;
                dto.Price = 77m;
                component.price = 66m;
                productA.Price = group.Price;
                service.Price = productB.Price;
                return new CollisionResult(
                    productA.Price,
                    productB.Price,
                    service.Price,
                    dto.Price,
                    component.price);
            }

            [HttpGet("overwritten")]
            public OverwriteResult GetOverwritten()
            {
                var group = new ProductGroup();
                var product = new Product();
                var service = new Service();
                group.Price = 100m;
                product.Price = group.Price;
                product.Price = 50m;
                service.Price = product.Price;
                return new OverwriteResult(product.Price, service.Price);
            }

            [HttpGet("branch")]
            public BranchResult GetBranch(bool flag)
            {
                var group = new ProductGroup();
                var product = new Product();
                var service = new Service();
                group.Price = 100m;

                if (flag)
                {
                    product.Price = group.Price;
                }
                else
                {
                    service.Price = product.Price;
                }

                return new BranchResult(product.Price, service.Price);
            }

            [HttpGet("setter")]
            public decimal GetSetter()
            {
                var source = new PlainPrice();
                var target = new AdjustedPrice();
                source.Price = 5m;
                target.Price = source.Price;
                return target.Price;
            }

            [HttpGet("shared-reference")]
            public int GetSharedReference()
            {
                var source = new Box();
                var target = new Box();
                source.Payload = new MutablePayload();
                target.Payload = source.Payload;
                source.Payload.Value = 7;
                return target.Payload.Value;
            }
        }
        """;

    private const string ProjectCollisionSource = """
        using System;

        namespace Shared;

        [AttributeUsage(AttributeTargets.Method)]
        public sealed class HttpGetAttribute : Attribute
        {
        }

        public sealed class ProductGroup
        {
            public decimal Price { get; set; }
        }

        public sealed class Product
        {
            public decimal Price { get; set; }
        }

        public sealed class __CONTROLLER__
        {
            [HttpGet]
            public decimal Copy()
            {
                var group = new ProductGroup();
                var product = new Product();
                group.Price = 10m;
                product.Price = group.Price;
                return product.Price;
            }
        }
        """;
}