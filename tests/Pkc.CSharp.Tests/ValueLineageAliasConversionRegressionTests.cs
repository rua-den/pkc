using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Pkc.Core;
using Pkc.CSharp;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class ValueLineageAliasConversionRegressionTests
{
    [Fact]
    public async Task User_defined_conversion_allocation_does_not_restore_stale_composition()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pkc-v047-lineage-tests",
            "alias-user-conversion",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "DemoLineage.csproj"), ProjectTemplate);
            await File.WriteAllTextAsync(Path.Combine(root, "Catalog.cs"), FixtureSource);

            var runtime = CompileFixture(FixtureSource);
            try
            {
                var result = Invoke(runtime.Assembly, "GetConvertedAlias");
                Assert.Equal(100m, ReadDecimal(result, "GroupPrice"));
                Assert.Equal(50m, ReadDecimal(result, "ProductPrice"));
                Assert.Equal(50m, ReadDecimal(result, "ServicePrice"));
            }
            finally
            {
                runtime.Context.Unload();
            }

            var facts = await new CSharpEvidenceScanner().ScanAsync(root);
            var transfers = facts.Facts
                .Where(fact => fact.Kind == "value-transfer" &&
                               fact.Metadata.GetValueOrDefault("scopeName") == "GetConvertedAlias")
                .OrderBy(fact => fact.Source.StartLine)
                .ToArray();

            Assert.Equal(2, transfers.Length);
            Assert.Contains(transfers, fact =>
                fact.Metadata.GetValueOrDefault("sourceOccurrence") == "group.Price" &&
                fact.Metadata.GetValueOrDefault("targetOccurrence") == "product.Price");
            var serviceTransfer = Assert.Single(transfers, fact =>
                fact.Metadata.GetValueOrDefault("sourceOccurrence") == "product.Price" &&
                fact.Metadata.GetValueOrDefault("targetOccurrence") == "service.Price");
            Assert.Equal(
                "blocked-by-intervening-or-unproven-write",
                serviceTransfer.Metadata["compositionStatus"]);
            Assert.False(serviceTransfer.Metadata.ContainsKey("predecessorTransferFactId"));

            var candidate = FindCandidate(facts, "GetConvertedAlias");
            var knowledge = await new EvidenceAwareKnowledgeSynthesizer().SynthesizeAsync(candidate);
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

    private static FeatureCandidate FindCandidate(FactDocument facts, string endpointName)
    {
        var candidates = new CrossStackFeatureCandidateBuilder().Build(facts);
        return Assert.Single(candidates.Candidates, candidate =>
            candidate.Facts.Any(fact =>
                fact.Id == candidate.SeedFactId &&
                fact.Kind == "endpoint" &&
                fact.Name == endpointName));
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
            $"PkcV047AliasConversion_{Guid.NewGuid():N}",
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
        var context = new AssemblyLoadContext($"pkc-v047-alias-{Guid.NewGuid():N}", isCollectible: true);
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

        public sealed class ProductGroup { public decimal Price { get; set; } }
        public sealed class Product { public decimal Price { get; set; } }
        public sealed class Service { public decimal Price { get; set; } }
        public sealed record Result(decimal GroupPrice, decimal ProductPrice, decimal ServicePrice);

        public sealed class ProductAlias
        {
            private readonly Product _product;

            public ProductAlias(Product product)
            {
                _product = product;
            }

            public static implicit operator Product(ProductAlias alias) => alias._product;
        }

        public sealed class PricesController
        {
            [HttpGet]
            public Result GetConvertedAlias()
            {
                var group = new ProductGroup();
                var product = new Product();
                Product alias = new ProductAlias(product);
                var service = new Service();
                group.Price = 100m;
                product.Price = group.Price;
                alias.Price = 50m;
                service.Price = product.Price;
                return new Result(group.Price, product.Price, service.Price);
            }
        }
        """;
}
