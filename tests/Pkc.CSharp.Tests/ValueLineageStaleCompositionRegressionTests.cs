using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Pkc.Core;
using Pkc.CSharp;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class ValueLineageStaleCompositionRegressionTests
{
    [Fact]
    public async Task Receiver_reassignment_does_not_preserve_stale_lineage()
    {
        await AssertNoStaleLineageAsync(
            "GetReceiverReassignment",
            new Dictionary<string, decimal>
            {
                ["GroupPrice"] = 120m,
                ["ProductPrice"] = 0m,
                ["ServicePrice"] = 0m
            });
    }

    [Fact]
    public async Task Opaque_helper_mutation_does_not_preserve_stale_lineage()
    {
        await AssertNoStaleLineageAsync(
            "GetOpaqueMutation",
            new Dictionary<string, decimal>
            {
                ["GroupPrice"] = 100m,
                ["ProductPrice"] = 50m,
                ["ServicePrice"] = 50m
            });
    }

    private static async Task AssertNoStaleLineageAsync(
        string endpointName,
        IReadOnlyDictionary<string, decimal> expectedValues)
    {
        var root = CreateRoot("stale-lineage");
        try
        {
            await WriteProjectAsync(root, "DemoLineage", FixtureSource);

            var runtime = CompileFixture(FixtureSource);
            try
            {
                var result = Invoke(runtime.Assembly, endpointName);
                foreach (var (propertyName, expectedValue) in expectedValues)
                {
                    Assert.Equal(expectedValue, ReadDecimal(result, propertyName));
                }
            }
            finally
            {
                runtime.Context.Unload();
            }

            var facts = await new CSharpEvidenceScanner().ScanAsync(root);
            var transfers = facts.Facts
                .Where(fact => fact.Kind == "value-transfer" &&
                               fact.Metadata.GetValueOrDefault("scopeName") == endpointName)
                .OrderBy(fact => fact.Source.StartLine)
                .ToArray();

            Assert.Equal(2, transfers.Length);
            var serviceTransfer = Assert.Single(transfers, fact =>
                fact.Metadata.GetValueOrDefault("sourceOccurrence") == "product.Price" &&
                fact.Metadata.GetValueOrDefault("targetOccurrence") == "service.Price");
            Assert.Equal(
                "blocked-by-intervening-or-unproven-write",
                serviceTransfer.Metadata["compositionStatus"]);
            Assert.False(serviceTransfer.Metadata.ContainsKey("predecessorTransferFactId"));

            var knowledge = await SynthesizeAsync(facts, endpointName);
            Assert.DoesNotContain(knowledge.ValueLineage, item =>
                item.Contains("Proven stored lineage chain", StringComparison.Ordinal));
            Assert.Contains(knowledge.ValueLineage, item =>
                item.Contains(
                    "Immediate stored snapshot copy: `product.Price` → `service.Price`",
                    StringComparison.Ordinal));
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
            <AssemblyName>__ASSEMBLY__</AssemblyName>
          </PropertyGroup>
        </Project>
        """;

    private const string FixtureSource = """
        using System;

        namespace Demo;

        [AttributeUsage(AttributeTargets.Method)]
        public sealed class HttpGetAttribute : Attribute { }

        public sealed class ProductGroup { public decimal Price { get; set; } }
        public sealed class Product { public decimal Price { get; set; } }
        public sealed class Service { public decimal Price { get; set; } }
        public sealed record Result(decimal GroupPrice, decimal ProductPrice, decimal ServicePrice);

        public sealed class PricesController
        {
            [HttpGet]
            public Result GetReceiverReassignment()
            {
                var group = new ProductGroup();
                var product = new Product();
                var service = new Service();
                group.Price = 100m;
                product.Price = group.Price;
                product = new Product();
                service.Price = product.Price;
                group.Price = 120m;
                return new Result(group.Price, product.Price, service.Price);
            }

            [HttpGet]
            public Result GetOpaqueMutation()
            {
                var group = new ProductGroup();
                var product = new Product();
                var service = new Service();
                group.Price = 100m;
                product.Price = group.Price;
                Mutate(product);
                service.Price = product.Price;
                return new Result(group.Price, product.Price, service.Price);
            }

            private static void Mutate(Product product)
            {
                product.Price = 50m;
            }
        }
        """;
}
