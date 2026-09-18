using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Pkc.CSharp;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class ValueLineageDeconstructionAliasRegressionTests
{
    [Theory]
    [InlineData("GetDeconstructionAliasWriteAfterDerivation")]
    [InlineData("GetDeconstructionDeclarationAliasWriteAfterDerivation")]
    public async Task Deconstruction_reference_alias_does_not_keep_stale_terminal_source(string endpointName)
    {
        var root = CreateRoot(endpointName);
        try
        {
            await WriteProjectAsync(root, FixtureSource);

            var runtime = CompileFixture(FixtureSource);
            try
            {
                Assert.Equal(5m, InvokeDecimal(runtime.Assembly, endpointName));
            }
            finally
            {
                runtime.Context.Unload();
            }

            var facts = await new CSharpEvidenceScanner().ScanAsync(root);
            var derivation = Assert.Single(facts.Facts, fact =>
                fact.Kind == "value-transfer" &&
                fact.Metadata.GetValueOrDefault("scopeName") == endpointName &&
                fact.Metadata.GetValueOrDefault("mechanism") == "derivation");

            Assert.Equal("service.Price - service.Discount", derivation.Metadata["expression"]);
            Assert.Equal("service.NetPrice", derivation.Metadata["targetOccurrence"]);
            Assert.DoesNotContain(facts.Facts, fact =>
                fact.Kind == "value-terminal-source" &&
                fact.Metadata.GetValueOrDefault("scopeName") == endpointName);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static string CreateRoot(string name)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pkc-v047-b-deconstruction-alias-tests",
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
            $"PkcV047BDeconstructionAlias_{Guid.NewGuid():N}",
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
        var context = new AssemblyLoadContext(
            $"pkc-v047-b-deconstruction-alias-{Guid.NewGuid():N}",
            isCollectible: true);
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

        public sealed class Service
        {
            public decimal Price { get; set; }
            public decimal Discount { get; set; }
            public decimal NetPrice { get; set; }
        }

        public sealed class PricesController
        {
            [HttpGet]
            public decimal GetDeconstructionAliasWriteAfterDerivation()
            {
                var service = new Service();
                var alias = new Service();
                var other = new Service();
                service.Price = 100m;
                service.Discount = 10m;
                service.NetPrice = service.Price - service.Discount;
                (alias, other) = (service, alias);
                alias.NetPrice = 5m;
                return service.NetPrice;
            }

            [HttpGet]
            public decimal GetDeconstructionDeclarationAliasWriteAfterDerivation()
            {
                var service = new Service();
                service.Price = 100m;
                service.Discount = 10m;
                service.NetPrice = service.Price - service.Discount;
                var (alias, other) = (service, new Service());
                alias.NetPrice = 5m;
                return service.NetPrice;
            }
        }
        """;
}