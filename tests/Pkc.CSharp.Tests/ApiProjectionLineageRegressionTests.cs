using Pkc.Core;
using Pkc.CSharp;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class ApiProjectionLineageRegressionTests
{
    [Fact]
    public async Task Exact_entity_property_maps_to_renamed_dto_property_and_api_response()
    {
        var root = CreateRoot("renamed-api-projection");
        try
        {
            await WriteProjectAsync(root, FixtureSource);
            var facts = await new CSharpEvidenceScanner().ScanAsync(root);

            var projection = Assert.Single(facts.Facts, fact =>
                fact.Kind == "value-transfer" &&
                fact.Metadata.GetValueOrDefault("scopeName") == "GetPrice" &&
                fact.Metadata.GetValueOrDefault("mechanism") == "api-projection");
            var responseSource = Assert.Single(facts.Facts, fact =>
                fact.Kind == "value-terminal-source" &&
                fact.Metadata.GetValueOrDefault("scopeName") == "GetPrice" &&
                fact.Metadata.GetValueOrDefault("boundary") == "API response field");

            Assert.Equal("project-semantic", projection.Metadata["analysisMode"]);
            Assert.Equal("DemoApiProjection.csproj", projection.Metadata["semanticProject"]);
            Assert.StartsWith("DemoApiProjection,", projection.Metadata["sourceAssemblyIdentity"]);
            Assert.StartsWith("DemoApiProjection,", projection.Metadata["targetAssemblyIdentity"]);
            Assert.Equal("entity.Price", projection.Metadata["sourceOccurrence"]);
            Assert.Equal("Demo.PriceResponse.DisplayPrice", projection.Metadata["targetOccurrence"]);
            Assert.Contains("Demo.ProductEntity.Price", projection.Metadata["sourceMemberIdentity"], StringComparison.Ordinal);
            Assert.Contains("Demo.PriceResponse.DisplayPrice", projection.Metadata["targetMemberIdentity"], StringComparison.Ordinal);
            Assert.Contains("Demo.PriceResponse", projection.Metadata["responseTypeIdentity"], StringComparison.Ordinal);
            Assert.Equal("direct-return-object-initializer", projection.Metadata["responseBoundary"]);
            Assert.StartsWith("Catalog.cs:L", projection.Metadata["sourceMemberLocation"]);
            Assert.StartsWith("Catalog.cs:L", projection.Metadata["targetMemberLocation"]);
            Assert.StartsWith("Catalog.cs:L", projection.Metadata["projectionLocation"]);
            Assert.StartsWith("Catalog.cs:L", projection.Metadata["responseLocation"]);

            Assert.Equal(projection.Id, responseSource.Metadata["sourceFactId"]);
            Assert.Equal("api-projection", responseSource.Metadata["sourceMechanism"]);
            Assert.Equal("Demo.PriceResponse.DisplayPrice", responseSource.Metadata["returnedOccurrence"]);
            Assert.Equal(projection.Metadata["targetMemberIdentity"], responseSource.Metadata["returnedMemberIdentity"]);
            Assert.Equal("DemoApiProjection.csproj", responseSource.Metadata["semanticProject"]);

            var candidate = FindCandidate(facts, "GetPrice");
            Assert.Contains(candidate.Facts, fact => fact.Id == projection.Id);
            Assert.Contains(candidate.Facts, fact => fact.Id == responseSource.Id);

            var knowledge = await new EvidenceAwareKnowledgeSynthesizer().SynthesizeAsync(candidate);
            Assert.Contains(knowledge.ValueLineage, item =>
                item.Contains("entity.Price", StringComparison.Ordinal) &&
                item.Contains("PriceResponse.DisplayPrice", StringComparison.Ordinal));
            Assert.Contains(knowledge.ValueLineage, item =>
                item.Contains("Last proven source before API response field", StringComparison.Ordinal) &&
                item.Contains("api-projection", StringComparison.Ordinal));
            Assert.Contains(knowledge.Evidence, evidence =>
                evidence.FactId == projection.Id &&
                evidence.Source.Path == "Catalog.cs");

            var markdown = new MarkdownKnowledgeRenderer().Render(knowledge);
            Assert.Contains("## Value lineage", markdown, StringComparison.Ordinal);
            Assert.Contains("entity.Price", markdown, StringComparison.Ordinal);
            Assert.Contains("PriceResponse.DisplayPrice", markdown, StringComparison.Ordinal);
            Assert.Contains("Last proven source before API response field", markdown, StringComparison.Ordinal);
            Assert.Contains("Catalog.cs:L", markdown, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData("GetSameNameWithoutMapping")]
    [InlineData("GetCustomSourceAccessor")]
    [InlineData("GetCustomTargetAccessor")]
    [InlineData("GetUserDefinedConversion")]
    [InlineData("GetAliasedSource")]
    [InlineData("GetOpaqueEffect")]
    [InlineData("GetCustomResponseConstructor")]
    public async Task Unsupported_or_unproven_projection_shapes_fail_closed(string endpointName)
    {
        var root = CreateRoot(endpointName);
        try
        {
            await WriteProjectAsync(root, FixtureSource);
            var facts = await new CSharpEvidenceScanner().ScanAsync(root);

            Assert.DoesNotContain(facts.Facts, fact =>
                fact.Metadata.GetValueOrDefault("scopeName") == endpointName &&
                (fact.Metadata.GetValueOrDefault("mechanism") == "api-projection" ||
                 fact.Kind == "value-terminal-source" &&
                 fact.Metadata.GetValueOrDefault("boundary") == "API response field"));

            var candidate = FindCandidate(facts, endpointName);
            var knowledge = await new EvidenceAwareKnowledgeSynthesizer().SynthesizeAsync(candidate);
            Assert.DoesNotContain(knowledge.ValueLineage, item =>
                item.Contains("Last proven source before API response field", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Same_named_types_and_members_keep_exact_namespace_identity()
    {
        var root = CreateRoot("same-name-symbol-identity");
        try
        {
            await WriteProjectAsync(root, FixtureSource);
            var facts = await new CSharpEvidenceScanner().ScanAsync(root);

            var projection = FindProjection(facts, "GetNamespaceQualifiedPrice");
            Assert.Contains("Demo.Primary.ProductEntity.Price", projection.Metadata["sourceMemberIdentity"], StringComparison.Ordinal);
            Assert.DoesNotContain("Demo.Collision.ProductEntity.Price", projection.Metadata["sourceMemberIdentity"], StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Cross_project_same_name_type_keeps_exact_assembly_identity()
    {
        var root = CreateRoot("cross-project-symbol-identity");
        try
        {
            await WriteProjectAsync(root, FixtureSource);
            var facts = await new CSharpEvidenceScanner().ScanAsync(root);

            var localProjection = FindProjection(facts, "GetNamespaceQualifiedPrice");
            Assert.Equal("DemoApiProjection.csproj", localProjection.Metadata["semanticProject"]);
            Assert.StartsWith("DemoApiProjection,", localProjection.Metadata["sourceAssemblyIdentity"]);
            Assert.Contains("Demo.Primary.ProductEntity.Price", localProjection.Metadata["sourceMemberIdentity"], StringComparison.Ordinal);
            Assert.DoesNotContain("CollisionLib", localProjection.Metadata["sourceMemberIdentity"], StringComparison.Ordinal);

            var externalProjection = FindProjection(facts, "GetExternalAssemblyPrice");
            Assert.Equal("DemoApiProjection.csproj", externalProjection.Metadata["semanticProject"]);
            Assert.StartsWith("CollisionLib,", externalProjection.Metadata["sourceAssemblyIdentity"]);
            Assert.Contains("Demo.ExternalCollision.ProductEntity.Price", externalProjection.Metadata["sourceMemberIdentity"], StringComparison.Ordinal);
            Assert.DoesNotContain("DemoApiProjection,", externalProjection.Metadata["sourceMemberIdentity"], StringComparison.Ordinal);
            Assert.StartsWith("CollisionLib/ProductEntity.cs:L", externalProjection.Metadata["sourceMemberLocation"]);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Missing_project_semantics_do_not_produce_api_projection_lineage()
    {
        var root = CreateRoot("api-projection-no-project");
        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "Catalog.cs"), FixtureSource);
            var facts = await new CSharpEvidenceScanner().ScanAsync(root);

            Assert.DoesNotContain(facts.Facts, fact =>
                fact.Metadata.GetValueOrDefault("mechanism") == "api-projection" ||
                fact.Kind == "value-terminal-source" &&
                fact.Metadata.GetValueOrDefault("boundary") == "API response field");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static EvidenceFact FindProjection(FactDocument facts, string endpointName) =>
        Assert.Single(facts.Facts, fact =>
            fact.Kind == "value-transfer" &&
            fact.Metadata.GetValueOrDefault("scopeName") == endpointName &&
            fact.Metadata.GetValueOrDefault("mechanism") == "api-projection");

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
            "pkc-v047-c-tests",
            name,
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static async Task WriteProjectAsync(string root, string source)
    {
        var collisionRoot = Path.Combine(root, "CollisionLib");
        Directory.CreateDirectory(collisionRoot);
        await File.WriteAllTextAsync(Path.Combine(collisionRoot, "CollisionLib.csproj"), CollisionProjectTemplate);
        await File.WriteAllTextAsync(Path.Combine(collisionRoot, "ProductEntity.cs"), CollisionSource);
        await File.WriteAllTextAsync(Path.Combine(root, "DemoApiProjection.csproj"), ProjectTemplate);
        await File.WriteAllTextAsync(Path.Combine(root, "Catalog.cs"), source);
    }

    private const string ProjectTemplate = """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>net8.0</TargetFramework>
            <ImplicitUsings>enable</ImplicitUsings>
            <Nullable>enable</Nullable>
            <AssemblyName>DemoApiProjection</AssemblyName>
          </PropertyGroup>
          <ItemGroup>
            <Compile Remove="CollisionLib/**/*.cs" />
            <ProjectReference Include="CollisionLib/CollisionLib.csproj" />
          </ItemGroup>
        </Project>
        """;

    private const string CollisionProjectTemplate = """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>net8.0</TargetFramework>
            <Nullable>enable</Nullable>
            <AssemblyName>CollisionLib</AssemblyName>
          </PropertyGroup>
        </Project>
        """;

    private const string CollisionSource = """
        namespace Demo.ExternalCollision
        {
            public sealed class ProductEntity
            {
                public decimal Price { get; set; }
            }
        }
        """;

    private const string FixtureSource = """
        using System;

        namespace Demo
        {
            [AttributeUsage(AttributeTargets.Method)]
            public sealed class HttpGetAttribute : Attribute { }

            public sealed class ProductEntity
            {
                public decimal Price { get; set; }
            }

            public sealed class PriceResponse
            {
                public decimal DisplayPrice { get; set; }
            }

            public sealed class SameNameResponse
            {
                public decimal Price { get; set; }
            }

            public sealed class CustomSource
            {
                private decimal _price;
                public decimal Price
                {
                    get => _price + 1m;
                    set => _price = value;
                }
            }

            public sealed class CustomTargetResponse
            {
                private decimal _displayPrice;
                public decimal DisplayPrice
                {
                    get => _displayPrice;
                    set => _displayPrice = value + 1m;
                }
            }

            public sealed class CustomConstructedResponse
            {
                public CustomConstructedResponse() { }
                public decimal DisplayPrice { get; set; }
            }

            public readonly struct Money
            {
                public Money(decimal value) => Value = value;
                public decimal Value { get; }
                public static implicit operator decimal(Money value) => value.Value;
            }

            public sealed class MoneyEntity
            {
                public Money Price { get; set; }
            }

            public sealed class PricesController
            {
                [HttpGet]
                public PriceResponse GetPrice()
                {
                    var entity = new ProductEntity();
                    entity.Price = 42m;
                    return new PriceResponse { DisplayPrice = entity.Price };
                }

                [HttpGet]
                public SameNameResponse GetSameNameWithoutMapping()
                {
                    var entity = new ProductEntity();
                    entity.Price = 42m;
                    return new SameNameResponse { Price = 0m };
                }

                [HttpGet]
                public PriceResponse GetCustomSourceAccessor()
                {
                    var entity = new CustomSource();
                    entity.Price = 42m;
                    return new PriceResponse { DisplayPrice = entity.Price };
                }

                [HttpGet]
                public CustomTargetResponse GetCustomTargetAccessor()
                {
                    var entity = new ProductEntity();
                    entity.Price = 42m;
                    return new CustomTargetResponse { DisplayPrice = entity.Price };
                }

                [HttpGet]
                public PriceResponse GetUserDefinedConversion()
                {
                    var entity = new MoneyEntity();
                    return new PriceResponse { DisplayPrice = entity.Price };
                }

                [HttpGet]
                public PriceResponse GetAliasedSource()
                {
                    var entity = new ProductEntity();
                    var alias = entity;
                    entity.Price = 42m;
                    return new PriceResponse { DisplayPrice = alias.Price };
                }

                [HttpGet]
                public PriceResponse GetOpaqueEffect()
                {
                    var entity = new ProductEntity();
                    entity.Price = 42m;
                    Touch(entity);
                    return new PriceResponse { DisplayPrice = entity.Price };
                }

                [HttpGet]
                public CustomConstructedResponse GetCustomResponseConstructor()
                {
                    var entity = new ProductEntity();
                    entity.Price = 42m;
                    return new CustomConstructedResponse { DisplayPrice = entity.Price };
                }

                private static void Touch(ProductEntity entity) => entity.Price = 5m;
            }
        }

        namespace Demo.Primary
        {
            public sealed class ProductEntity
            {
                public decimal Price { get; set; }
            }

            public sealed class PriceResponse
            {
                public decimal DisplayPrice { get; set; }
            }

            public sealed class IdentityController
            {
                [Demo.HttpGet]
                public PriceResponse GetNamespaceQualifiedPrice()
                {
                    var entity = new ProductEntity();
                    entity.Price = 9m;
                    return new PriceResponse { DisplayPrice = entity.Price };
                }

                [Demo.HttpGet]
                public PriceResponse GetExternalAssemblyPrice()
                {
                    var entity = new Demo.ExternalCollision.ProductEntity();
                    entity.Price = 11m;
                    return new PriceResponse { DisplayPrice = entity.Price };
                }
            }
        }

        namespace Demo.Collision
        {
            public sealed class ProductEntity
            {
                public decimal Price { get; set; }
            }
        }
        """;
}
