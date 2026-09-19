using Pkc.Core;
using Pkc.CSharp;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class ApiProjectionCrossAssemblyCollisionRegressionTests
{
    [Fact]
    public async Task Same_fully_qualified_member_name_across_assemblies_keeps_exact_assembly_identity()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pkc-v047-c-tests",
            "cross-assembly-full-name-collision",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var collisionRoot = Path.Combine(root, "CollisionLib");
            Directory.CreateDirectory(collisionRoot);
            await File.WriteAllTextAsync(
                Path.Combine(collisionRoot, "CollisionLib.csproj"),
                CollisionProjectTemplate);
            await File.WriteAllTextAsync(
                Path.Combine(collisionRoot, "ProductEntity.cs"),
                CollisionSource);
            await File.WriteAllTextAsync(
                Path.Combine(root, "DemoApiProjection.csproj"),
                ProjectTemplate);
            await File.WriteAllTextAsync(
                Path.Combine(root, "Catalog.cs"),
                MainSource);

            var facts = await new CSharpEvidenceScanner().ScanAsync(root);
            var local = FindProjection(facts, "GetLocalPrice");
            var external = FindProjection(facts, "GetExternalPrice");

            Assert.Equal("DemoApiProjection.csproj", local.Metadata["semanticProject"]);
            Assert.Equal("DemoApiProjection.csproj", external.Metadata["semanticProject"]);

            Assert.Equal(
                "Demo.Primary.ProductEntity.Price",
                local.Metadata["sourceMember"]);
            Assert.Equal(
                "Demo.Primary.ProductEntity.Price",
                external.Metadata["sourceMember"]);

            Assert.StartsWith("DemoApiProjection,", local.Metadata["sourceAssemblyIdentity"]);
            Assert.StartsWith("CollisionLib,", external.Metadata["sourceAssemblyIdentity"]);
            Assert.NotEqual(
                local.Metadata["sourceMemberIdentity"],
                external.Metadata["sourceMemberIdentity"]);
            Assert.Contains("DemoApiProjection", local.Metadata["sourceMemberIdentity"], StringComparison.Ordinal);
            Assert.Contains("CollisionLib", external.Metadata["sourceMemberIdentity"], StringComparison.Ordinal);

            Assert.StartsWith("Catalog.cs:L", local.Metadata["sourceMemberLocation"]);
            Assert.StartsWith("CollisionLib/ProductEntity.cs:L", external.Metadata["sourceMemberLocation"]);
            Assert.StartsWith("DemoApiProjection,", local.Metadata["targetAssemblyIdentity"]);
            Assert.StartsWith("DemoApiProjection,", external.Metadata["targetAssemblyIdentity"]);
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
            <ProjectReference Include="CollisionLib/CollisionLib.csproj" Aliases="collision" />
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
        namespace Demo.Primary
        {
            public sealed class ProductEntity
            {
                public decimal Price { get; set; }
            }
        }
        """;

    private const string MainSource = """
        extern alias collision;
        using System;

        namespace Demo
        {
            [AttributeUsage(AttributeTargets.Method)]
            public sealed class HttpGetAttribute : Attribute { }
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
                public PriceResponse GetLocalPrice()
                {
                    var entity = new ProductEntity();
                    entity.Price = 9m;
                    return new PriceResponse { DisplayPrice = entity.Price };
                }

                [Demo.HttpGet]
                public PriceResponse GetExternalPrice()
                {
                    var entity = new collision::Demo.Primary.ProductEntity();
                    entity.Price = 11m;
                    return new PriceResponse { DisplayPrice = entity.Price };
                }
            }
        }
        """;
}
