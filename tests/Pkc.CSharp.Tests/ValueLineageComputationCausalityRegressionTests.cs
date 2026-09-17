using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Pkc.Core;
using Pkc.CSharp;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class ValueLineageComputationCausalityRegressionTests
{
    [Fact]
    public async Task Multi_input_derivation_reaches_terminal_return_and_markdown()
    {
        var root = CreateRoot("derived-return");
        try
        {
            await WriteProjectAsync(root, FixtureSource);

            var runtime = CompileFixture(FixtureSource);
            try
            {
                Assert.Equal(90m, InvokeDecimal(runtime.Assembly, "GetDerivedPrice"));
            }
            finally
            {
                runtime.Context.Unload();
            }

            var facts = await new CSharpEvidenceScanner().ScanAsync(root);
            var derivation = Assert.Single(facts.Facts, fact =>
                fact.Kind == "value-transfer" &&
                fact.Metadata.GetValueOrDefault("scopeName") == "GetDerivedPrice" &&
                fact.Metadata.GetValueOrDefault("mechanism") == "derivation");

            Assert.Equal("snapshot", derivation.Metadata["temporalSemantics"]);
            Assert.Equal("service.Price - service.Discount", derivation.Metadata["expression"]);
            Assert.Equal("service.NetPrice", derivation.Metadata["targetOccurrence"]);
            var inputs = derivation.Metadata["inputOccurrences"]
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            Assert.Equal(2, inputs.Length);
            Assert.Contains("service.Price", inputs);
            Assert.Contains("service.Discount", inputs);

            var terminal = Assert.Single(facts.Facts, fact =>
                fact.Kind == "value-terminal-source" &&
                fact.Metadata.GetValueOrDefault("scopeName") == "GetDerivedPrice");
            Assert.Equal("return", terminal.Metadata["boundary"]);
            Assert.Equal("service.NetPrice", terminal.Metadata["returnedOccurrence"]);
            Assert.Equal(derivation.Id, terminal.Metadata["sourceFactId"]);
            Assert.Equal("derivation", terminal.Metadata["sourceMechanism"]);

            var candidate = FindCandidate(facts, "GetDerivedPrice");
            Assert.Contains(candidate.Facts, fact => fact.Id == derivation.Id);
            Assert.Contains(candidate.Facts, fact => fact.Id == terminal.Id);

            var knowledge = await new EvidenceAwareKnowledgeSynthesizer().SynthesizeAsync(candidate);
            Assert.Contains(knowledge.ValueLineage, item =>
                item.Contains("Stored derivation", StringComparison.Ordinal) &&
                item.Contains("service.Price", StringComparison.Ordinal) &&
                item.Contains("service.Discount", StringComparison.Ordinal) &&
                item.Contains("service.NetPrice", StringComparison.Ordinal));
            Assert.Contains(knowledge.ValueLineage, item =>
                item.Contains("Last proven source before return", StringComparison.Ordinal) &&
                item.Contains("service.NetPrice", StringComparison.Ordinal) &&
                item.Contains("derivation", StringComparison.OrdinalIgnoreCase));

            var markdown = new MarkdownKnowledgeRenderer().Render(knowledge);
            Assert.Contains("## Value lineage", markdown, StringComparison.Ordinal);
            Assert.Contains("Stored derivation", markdown, StringComparison.Ordinal);
            Assert.Contains("Last proven source before return", markdown, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Later_override_preserves_original_origin_and_becomes_terminal_source()
    {
        var root = CreateRoot("override-return");
        try
        {
            await WriteProjectAsync(root, FixtureSource);

            var runtime = CompileFixture(FixtureSource);
            try
            {
                Assert.Equal(120m, InvokeDecimal(runtime.Assembly, "GetOverriddenPrice"));
            }
            finally
            {
                runtime.Context.Unload();
            }

            var facts = await new CSharpEvidenceScanner().ScanAsync(root);
            var serviceCopy = Assert.Single(facts.Facts, fact =>
                fact.Kind == "value-transfer" &&
                fact.Metadata.GetValueOrDefault("scopeName") == "GetOverriddenPrice" &&
                fact.Metadata.GetValueOrDefault("sourceOccurrence") == "product.Price" &&
                fact.Metadata.GetValueOrDefault("targetOccurrence") == "service.Price");
            var causality = Assert.Single(facts.Facts, fact =>
                fact.Kind == "value-causality" &&
                fact.Metadata.GetValueOrDefault("scopeName") == "GetOverriddenPrice" &&
                fact.Metadata.GetValueOrDefault("causalRole") == "override");

            Assert.Equal("service.Price", causality.Metadata["targetOccurrence"]);
            Assert.Equal("120m", causality.Metadata["valueExpression"]);
            Assert.Equal(serviceCopy.Id, causality.Metadata["priorValueFactId"]);
            Assert.Equal("product.Price", causality.Metadata["priorSourceOccurrence"]);

            var terminal = Assert.Single(facts.Facts, fact =>
                fact.Kind == "value-terminal-source" &&
                fact.Metadata.GetValueOrDefault("scopeName") == "GetOverriddenPrice");
            Assert.Equal(causality.Id, terminal.Metadata["sourceFactId"]);
            Assert.Equal("override", terminal.Metadata["sourceMechanism"]);

            var candidate = FindCandidate(facts, "GetOverriddenPrice");
            var knowledge = await new EvidenceAwareKnowledgeSynthesizer().SynthesizeAsync(candidate);

            Assert.Contains(knowledge.ValueLineage, item =>
                item.Contains("Proven stored lineage chain", StringComparison.Ordinal) &&
                item.Contains("group.Price", StringComparison.Ordinal) &&
                item.Contains("product.Price", StringComparison.Ordinal) &&
                item.Contains("service.Price", StringComparison.Ordinal));
            Assert.Contains(knowledge.StateChanges, item =>
                item.Contains("Later override", StringComparison.Ordinal) &&
                item.Contains("service.Price", StringComparison.Ordinal) &&
                item.Contains("120m", StringComparison.Ordinal) &&
                item.Contains("product.Price", StringComparison.Ordinal));
            Assert.Contains(knowledge.ValueLineage, item =>
                item.Contains("Last proven source before return", StringComparison.Ordinal) &&
                item.Contains("override", StringComparison.OrdinalIgnoreCase) &&
                item.Contains("service.Price", StringComparison.Ordinal));

            var markdown = new MarkdownKnowledgeRenderer().Render(knowledge);
            Assert.Contains("Proven stored lineage chain", markdown, StringComparison.Ordinal);
            Assert.Contains("Later override", markdown, StringComparison.Ordinal);
            Assert.Contains("Last proven source before return", markdown, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData("GetCustomGetterDerivation")]
    [InlineData("GetInvocationDerivation")]
    [InlineData("GetBranchDerivation")]
    [InlineData("GetAliasedDerivation")]
    [InlineData("GetCompoundWriteAfterCopy")]
    [InlineData("GetUnaryWriteAfterCopy")]
    public async Task Unsupported_computation_shapes_fail_closed(string endpointName)
    {
        var root = CreateRoot("computation-negative");
        try
        {
            await WriteProjectAsync(root, FixtureSource);
            var facts = await new CSharpEvidenceScanner().ScanAsync(root);

            Assert.DoesNotContain(facts.Facts, fact =>
                fact.Metadata.GetValueOrDefault("scopeName") == endpointName &&
                (fact.Metadata.GetValueOrDefault("mechanism") == "derivation" ||
                 fact.Kind == "value-causality" ||
                 fact.Kind == "value-terminal-source"));

            var candidate = FindCandidate(facts, endpointName);
            var knowledge = await new EvidenceAwareKnowledgeSynthesizer().SynthesizeAsync(candidate);
            Assert.DoesNotContain(knowledge.ValueLineage, item =>
                item.Contains("Stored derivation", StringComparison.Ordinal) ||
                item.Contains("Last proven source before return", StringComparison.Ordinal));
            Assert.DoesNotContain(knowledge.StateChanges, item =>
                item.Contains("Later override", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Missing_project_semantics_do_not_produce_b_lineage_or_causality()
    {
        var root = CreateRoot("b-no-project");
        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "Catalog.cs"), FixtureSource);
            var facts = await new CSharpEvidenceScanner().ScanAsync(root);

            Assert.DoesNotContain(facts.Facts, fact =>
                fact.Kind is "value-causality" or "value-terminal-source" ||
                fact.Metadata.GetValueOrDefault("mechanism") == "derivation");
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
            "pkc-v047-b-tests",
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
            $"PkcV047B_{Guid.NewGuid():N}",
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
        var context = new AssemblyLoadContext($"pkc-v047-b-{Guid.NewGuid():N}", isCollectible: true);
        return new RuntimeFixture(context, context.LoadFromStream(stream));
    }

    private static decimal InvokeDecimal(Assembly assembly, string methodName)
    {
        var controllerType = assembly.GetType("Demo.PricesController", throwOnError: true)!;
        var controller = Activator.CreateInstance(controllerType)!;
        var method = controllerType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(method);
        return Assert.IsType<decimal>(method!.Invoke(controller, null));
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

        public sealed class Product
        {
            public decimal Price { get; set; }
        }

        public sealed class Service
        {
            public decimal Price { get; set; }
            public decimal Discount { get; set; }
            public decimal NetPrice { get; set; }
            public decimal CustomDiscount => Discount + 1m;
        }

        public sealed class PricesController
        {
            [HttpGet]
            public decimal GetDerivedPrice()
            {
                var group = new ProductGroup();
                var product = new Product();
                var service = new Service();
                group.Price = 100m;
                product.Price = group.Price;
                service.Price = product.Price;
                service.Discount = 10m;
                service.NetPrice = service.Price - service.Discount;
                return service.NetPrice;
            }

            [HttpGet]
            public decimal GetOverriddenPrice()
            {
                var group = new ProductGroup();
                var product = new Product();
                var service = new Service();
                group.Price = 100m;
                product.Price = group.Price;
                service.Price = product.Price;
                service.Price = 120m;
                return service.Price;
            }

            [HttpGet]
            public decimal GetCustomGetterDerivation()
            {
                var service = new Service();
                service.Price = 100m;
                service.Discount = 10m;
                service.NetPrice = service.Price - service.CustomDiscount;
                return service.NetPrice;
            }

            [HttpGet]
            public decimal GetInvocationDerivation()
            {
                var service = new Service();
                service.Price = 100m;
                service.Discount = 10m;
                service.NetPrice = Compute(service.Price, service.Discount);
                return service.NetPrice;
            }

            [HttpGet]
            public decimal GetBranchDerivation()
            {
                var service = new Service();
                service.Price = 100m;
                service.Discount = 10m;
                if (service.Discount > 0m)
                {
                    service.NetPrice = service.Price - service.Discount;
                }
                return service.NetPrice;
            }

            [HttpGet]
            public decimal GetAliasedDerivation()
            {
                var service = new Service();
                var alias = service;
                service.Price = 100m;
                alias.Discount = 10m;
                service.NetPrice = service.Price - service.Discount;
                return service.NetPrice;
            }

            [HttpGet]
            public decimal GetCompoundWriteAfterCopy()
            {
                var group = new ProductGroup();
                var product = new Product();
                var service = new Service();
                group.Price = 100m;
                product.Price = group.Price;
                service.Price = product.Price;
                service.Price += 5m;
                return service.Price;
            }

            [HttpGet]
            public decimal GetUnaryWriteAfterCopy()
            {
                var group = new ProductGroup();
                var product = new Product();
                var service = new Service();
                group.Price = 100m;
                product.Price = group.Price;
                service.Price = product.Price;
                service.Price++;
                return service.Price;
            }

            private static decimal Compute(decimal price, decimal discount) => price - discount;
        }
        """;
}
