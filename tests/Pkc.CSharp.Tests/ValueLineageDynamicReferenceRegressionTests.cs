using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Pkc.Core;
using Pkc.CSharp;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class ValueLineageDynamicReferenceRegressionTests
{
    [Fact]
    public async Task Readonly_reference_getter_tracks_upstream_value_at_read_time()
    {
        var root = CreateRoot("dynamic-positive");
        try
        {
            await WriteProjectAsync(root, FixtureSource);

            var runtime = CompileFixture(FixtureSource);
            try
            {
                var result = Invoke(runtime.Assembly, "GetDynamicReference");
                Assert.Equal(100m, ReadDecimal(result, "Before"));
                Assert.Equal(120m, ReadDecimal(result, "After"));
            }
            finally
            {
                runtime.Context.Unload();
            }

            var facts = await new CSharpEvidenceScanner().ScanAsync(root);
            var dynamicFact = Assert.Single(facts.Facts, fact =>
                fact.Kind == "value-transfer" &&
                fact.Metadata.GetValueOrDefault("scopeName") == "GetDynamicReference" &&
                fact.Metadata.GetValueOrDefault("temporalSemantics") == "dynamic");

            Assert.Equal("reference", dynamicFact.Metadata["mechanism"]);
            Assert.Equal("group.Price", dynamicFact.Metadata["sourceOccurrence"]);
            Assert.Equal("service.CurrentGroupPrice", dynamicFact.Metadata["targetOccurrence"]);
            Assert.Equal("project-semantic", dynamicFact.Metadata["analysisMode"]);
            Assert.Equal(
                "readonly-field-constructor-binding-expression-bodied-scalar-getter-read-write-read",
                dynamicFact.Metadata["proof"]);
            Assert.NotEqual(
                dynamicFact.Metadata["sourceReceiverIdentity"],
                dynamicFact.Metadata["targetReceiverIdentity"]);
            Assert.Contains("_group.Price", dynamicFact.Metadata["getterOccurrence"], StringComparison.Ordinal);

            var candidate = FindCandidate(facts, "GetDynamicReference");
            Assert.Contains(candidate.Facts, fact => fact.Id == dynamicFact.Id);

            var knowledge = await new EvidenceAwareKnowledgeSynthesizer().SynthesizeAsync(candidate);
            Assert.Contains(knowledge.ValueLineage, item =>
                item.Contains(
                    "Dynamic read-time dependency: `group.Price` → `service.CurrentGroupPrice`",
                    StringComparison.Ordinal) &&
                item.Contains("later change to `group.Price` can affect a later read", StringComparison.Ordinal) &&
                item.Contains("without another scalar-copy assignment", StringComparison.Ordinal));
            Assert.DoesNotContain(knowledge.ValueLineage, item =>
                item.Contains("Proven stored lineage chain", StringComparison.Ordinal));
            Assert.DoesNotContain(knowledge.ValueLineage, item =>
                item.Contains("Stored snapshot copy", StringComparison.Ordinal) &&
                item.Contains("service.CurrentGroupPrice", StringComparison.Ordinal));

            var markdown = new MarkdownKnowledgeRenderer().Render(knowledge);
            Assert.Contains("## Value lineage", markdown, StringComparison.Ordinal);
            Assert.Contains("Dynamic read-time dependency", markdown, StringComparison.Ordinal);
            Assert.Contains("`group.Price` → `service.CurrentGroupPrice`", markdown, StringComparison.Ordinal);
            Assert.Contains("dynamic read-time semantics", markdown, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData("GetMutableBinding")]
    [InlineData("GetCustomGetter")]
    [InlineData("GetAliasedReference")]
    [InlineData("GetReassignedReference")]
    [InlineData("GetOpaqueEffect")]
    [InlineData("GetBranchAmbiguity")]
    public async Task Unsupported_dynamic_reference_shapes_fail_closed(string endpointName)
    {
        var root = CreateRoot("dynamic-negative");
        try
        {
            await WriteProjectAsync(root, FixtureSource);

            var facts = await new CSharpEvidenceScanner().ScanAsync(root);
            Assert.DoesNotContain(facts.Facts, fact =>
                fact.Kind == "value-transfer" &&
                fact.Metadata.GetValueOrDefault("scopeName") == endpointName &&
                fact.Metadata.GetValueOrDefault("temporalSemantics") == "dynamic");

            var candidate = FindCandidate(facts, endpointName);
            var knowledge = await new EvidenceAwareKnowledgeSynthesizer().SynthesizeAsync(candidate);
            Assert.DoesNotContain(knowledge.ValueLineage, item =>
                item.Contains("Dynamic read-time dependency", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Missing_project_semantics_do_not_produce_dynamic_lineage()
    {
        var root = CreateRoot("dynamic-no-project");
        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "Catalog.cs"), FixtureSource);

            var facts = await new CSharpEvidenceScanner().ScanAsync(root);
            Assert.DoesNotContain(facts.Facts, fact =>
                fact.Kind == "value-transfer" &&
                fact.Metadata.GetValueOrDefault("temporalSemantics") == "dynamic");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
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

    private static async Task WriteProjectAsync(string root, string source)
    {
        await File.WriteAllTextAsync(Path.Combine(root, "DemoLineage.csproj"), ProjectTemplate);
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
            $"PkcV047Dynamic_{Guid.NewGuid():N}",
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
        var context = new AssemblyLoadContext($"pkc-v047-dynamic-{Guid.NewGuid():N}", isCollectible: true);
        return new RuntimeFixture(context, context.LoadFromStream(stream));
    }

    private static object Invoke(Assembly assembly, string methodName)
    {
        var controllerType = assembly.GetType("Demo.PricesController", throwOnError: true)!;
        var controller = Activator.CreateInstance(controllerType)!;
        var method = controllerType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(method);
        return method!.Invoke(controller, null)!;
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
            <AssemblyName>DemoLineage</AssemblyName>
          </PropertyGroup>
        </Project>
        """;

    private const string FixtureSource = """
        using System;

        namespace Demo;

        [AttributeUsage(AttributeTargets.Method)]
        public sealed class HttpGetAttribute : Attribute { }

        public sealed class ProductGroup
        {
            public decimal Price { get; set; }
        }

        public sealed class DynamicPriceService
        {
            private readonly ProductGroup _group;

            public DynamicPriceService(ProductGroup group)
            {
                _group = group;
            }

            public decimal CurrentGroupPrice => _group.Price;
        }

        public sealed class MutableBindingService
        {
            private ProductGroup _group;

            public MutableBindingService(ProductGroup group)
            {
                _group = group;
            }

            public decimal CurrentGroupPrice => _group.Price;
        }

        public sealed class CustomGetterService
        {
            private readonly ProductGroup _group;

            public CustomGetterService(ProductGroup group)
            {
                _group = group;
            }

            public decimal CurrentGroupPrice
            {
                get
                {
                    return _group.Price + 1m;
                }
            }
        }

        public sealed record DynamicResult(decimal Before, decimal After);

        public sealed class PricesController
        {
            [HttpGet]
            public DynamicResult GetDynamicReference()
            {
                var group = new ProductGroup();
                group.Price = 100m;
                var service = new DynamicPriceService(group);
                var before = service.CurrentGroupPrice;
                group.Price = 120m;
                var after = service.CurrentGroupPrice;
                return new DynamicResult(before, after);
            }

            [HttpGet]
            public DynamicResult GetMutableBinding()
            {
                var group = new ProductGroup();
                group.Price = 100m;
                var service = new MutableBindingService(group);
                var before = service.CurrentGroupPrice;
                group.Price = 120m;
                var after = service.CurrentGroupPrice;
                return new DynamicResult(before, after);
            }

            [HttpGet]
            public DynamicResult GetCustomGetter()
            {
                var group = new ProductGroup();
                group.Price = 100m;
                var service = new CustomGetterService(group);
                var before = service.CurrentGroupPrice;
                group.Price = 120m;
                var after = service.CurrentGroupPrice;
                return new DynamicResult(before, after);
            }

            [HttpGet]
            public DynamicResult GetAliasedReference()
            {
                var group = new ProductGroup();
                var alias = group;
                group.Price = 100m;
                var service = new DynamicPriceService(alias);
                var before = service.CurrentGroupPrice;
                group.Price = 120m;
                var after = service.CurrentGroupPrice;
                return new DynamicResult(before, after);
            }

            [HttpGet]
            public DynamicResult GetReassignedReference()
            {
                var group = new ProductGroup();
                group.Price = 100m;
                var service = new DynamicPriceService(group);
                var before = service.CurrentGroupPrice;
                group = new ProductGroup();
                group.Price = 120m;
                var after = service.CurrentGroupPrice;
                return new DynamicResult(before, after);
            }

            [HttpGet]
            public DynamicResult GetOpaqueEffect()
            {
                var group = new ProductGroup();
                group.Price = 100m;
                var service = new DynamicPriceService(group);
                var before = service.CurrentGroupPrice;
                Touch(group);
                var after = service.CurrentGroupPrice;
                return new DynamicResult(before, after);
            }

            [HttpGet]
            public DynamicResult GetBranchAmbiguity()
            {
                var group = new ProductGroup();
                group.Price = 100m;
                var service = new DynamicPriceService(group);
                var before = service.CurrentGroupPrice;
                if (before > 0m)
                {
                    group.Price = 120m;
                }
                var after = service.CurrentGroupPrice;
                return new DynamicResult(before, after);
            }

            private static void Touch(ProductGroup group)
            {
                group.Price = 120m;
            }
        }
        """;
}
