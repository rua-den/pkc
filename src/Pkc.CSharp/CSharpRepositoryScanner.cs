using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pkc.Core;

namespace Pkc.CSharp;

public sealed class CSharpRepositoryScanner
{
    private static readonly HashSet<string> ExcludedDirectoryNames =
        new(StringComparer.OrdinalIgnoreCase) { ".git", ".pkc", "bin", "obj" };

    public async Task<FactDocument> ScanAsync(string repositoryPath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryPath);

        var rootPath = Path.GetFullPath(repositoryPath);
        if (!Directory.Exists(rootPath))
        {
            throw new DirectoryNotFoundException($"Repository path does not exist: {rootPath}");
        }

        var facts = new List<EvidenceFact>();
        var relations = new List<EvidenceRelation>();

        var files = Directory.EnumerateFiles(rootPath, "*.cs", SearchOption.AllDirectories)
            .Where(path => !IsExcluded(rootPath, path))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var relativePath = NormalizePath(Path.GetRelativePath(rootPath, file));
            var sourceText = await File.ReadAllTextAsync(file, cancellationToken);
            var tree = CSharpSyntaxTree.ParseText(sourceText, path: relativePath, cancellationToken: cancellationToken);
            var syntaxRoot = await tree.GetRootAsync(cancellationToken);

            ExtractTypeFacts(syntaxRoot, relativePath, facts);
            ExtractMethodFacts(syntaxRoot, relativePath, facts, relations);
            ExtractPropertyFacts(syntaxRoot, relativePath, facts);
            ExtractEnumMemberFacts(syntaxRoot, relativePath, facts);
        }

        return new FactDocument(
            "0.1",
            facts.OrderBy(fact => fact.Id, StringComparer.Ordinal).ToArray(),
            relations
                .OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
                .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
                .ThenBy(relation => relation.Target, StringComparer.Ordinal)
                .ToArray());
    }

    private static void ExtractTypeFacts(SyntaxNode root, string relativePath, ICollection<EvidenceFact> facts)
    {
        foreach (var declaration in root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
        {
            var kind = declaration switch
            {
                ClassDeclarationSyntax => "class",
                InterfaceDeclarationSyntax => "interface",
                StructDeclarationSyntax => "struct",
                RecordDeclarationSyntax => "record",
                EnumDeclarationSyntax => "enum",
                _ => "type"
            };

            var metadata = new Dictionary<string, string>(StringComparer.Ordinal);
            if (declaration is TypeDeclarationSyntax typeDeclaration && typeDeclaration.BaseList is not null)
            {
                metadata["baseTypes"] = string.Join(", ", typeDeclaration.BaseList.Types.Select(type => type.Type.ToString()));
            }

            var namespaceName = GetNamespace(declaration);
            if (!string.IsNullOrEmpty(namespaceName))
            {
                metadata["namespace"] = namespaceName;
            }

            facts.Add(CreateFact(
                declaration,
                relativePath,
                kind,
                declaration.Identifier.ValueText,
                GetContainingType(declaration),
                declaration.AttributeLists,
                metadata));
        }
    }

    private static void ExtractMethodFacts(
        SyntaxNode root,
        string relativePath,
        ICollection<EvidenceFact> facts,
        ICollection<EvidenceRelation> relations)
    {
        foreach (var method in root.DescendantNodes().OfType<BaseMethodDeclarationSyntax>())
        {
            var name = method switch
            {
                MethodDeclarationSyntax declaration => declaration.Identifier.ValueText,
                ConstructorDeclarationSyntax declaration => declaration.Identifier.ValueText,
                DestructorDeclarationSyntax declaration => $"~{declaration.Identifier.ValueText}",
                OperatorDeclarationSyntax declaration => $"operator {declaration.OperatorToken.ValueText}",
                ConversionOperatorDeclarationSyntax declaration => $"operator {declaration.Type}",
                _ => method.Kind().ToString()
            };

            var attributes = method.AttributeLists;
            var metadata = new Dictionary<string, string>(StringComparer.Ordinal);
            var kind = method is ConstructorDeclarationSyntax ? "constructor" : "method";

            if (method is MethodDeclarationSyntax methodDeclaration)
            {
                metadata["returnType"] = methodDeclaration.ReturnType.ToString();
                metadata["parameters"] = string.Join(", ", methodDeclaration.ParameterList.Parameters.Select(parameter => parameter.ToString()));

                var httpAttribute = attributes.SelectMany(list => list.Attributes)
                    .Select(attribute => new { Attribute = attribute, Name = NormalizeAttributeName(attribute.Name.ToString()) })
                    .FirstOrDefault(item => TryGetHttpMethod(item.Name, out _));

                if (httpAttribute is not null && TryGetHttpMethod(httpAttribute.Name, out var httpMethod))
                {
                    kind = "endpoint";
                    metadata["httpMethod"] = httpMethod;
                    var route = FirstStringArgument(httpAttribute.Attribute);
                    if (!string.IsNullOrWhiteSpace(route))
                    {
                        metadata["routeTemplate"] = route;
                    }
                }

                var authorize = attributes.SelectMany(list => list.Attributes)
                    .FirstOrDefault(attribute => NormalizeAttributeName(attribute.Name.ToString()) == "Authorize");
                if (authorize is not null)
                {
                    metadata["authorization"] = authorize.ArgumentList?.Arguments.ToString() ?? "required";
                }
            }

            var fact = CreateFact(
                method,
                relativePath,
                kind,
                name,
                GetContainingType(method),
                attributes,
                metadata);

            facts.Add(fact);

            foreach (var invocation in method.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                relations.Add(new EvidenceRelation(
                    fact.Id,
                    "invokes-syntax",
                    invocation.Expression.ToString(),
                    GetLocation(invocation, relativePath)));
            }
        }
    }

    private static void ExtractPropertyFacts(SyntaxNode root, string relativePath, ICollection<EvidenceFact> facts)
    {
        foreach (var property in root.DescendantNodes().OfType<PropertyDeclarationSyntax>())
        {
            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["type"] = property.Type.ToString()
            };

            facts.Add(CreateFact(
                property,
                relativePath,
                "property",
                property.Identifier.ValueText,
                GetContainingType(property),
                property.AttributeLists,
                metadata));
        }
    }

    private static void ExtractEnumMemberFacts(SyntaxNode root, string relativePath, ICollection<EvidenceFact> facts)
    {
        foreach (var member in root.DescendantNodes().OfType<EnumMemberDeclarationSyntax>())
        {
            facts.Add(CreateFact(
                member,
                relativePath,
                "enum-member",
                member.Identifier.ValueText,
                GetContainingType(member),
                member.AttributeLists,
                new Dictionary<string, string>(StringComparer.Ordinal)));
        }
    }

    private static EvidenceFact CreateFact(
        SyntaxNode node,
        string relativePath,
        string kind,
        string name,
        string? container,
        SyntaxList<AttributeListSyntax> attributeLists,
        IReadOnlyDictionary<string, string> metadata)
    {
        var location = GetLocation(node, relativePath);
        var id = $"cs:{relativePath}:{location.StartLine}:{name}";

        return new EvidenceFact(
            id,
            kind,
            name,
            container,
            location,
            attributeLists
                .SelectMany(list => list.Attributes)
                .Select(attribute => attribute.ToString())
                .OrderBy(attribute => attribute, StringComparer.Ordinal)
                .ToArray(),
            metadata);
    }

    private static SourceLocation GetLocation(SyntaxNode node, string relativePath)
    {
        var span = node.GetLocation().GetLineSpan();
        return new SourceLocation(
            relativePath,
            span.StartLinePosition.Line + 1,
            span.EndLinePosition.Line + 1);
    }

    private static string? GetContainingType(SyntaxNode node)
    {
        var type = node.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
        if (type is null)
        {
            return null;
        }

        var namespaceName = GetNamespace(type);
        return string.IsNullOrEmpty(namespaceName)
            ? type.Identifier.ValueText
            : $"{namespaceName}.{type.Identifier.ValueText}";
    }

    private static string? GetNamespace(SyntaxNode node) =>
        node.AncestorsAndSelf().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString();

    private static bool TryGetHttpMethod(string attributeName, out string httpMethod)
    {
        httpMethod = attributeName switch
        {
            "HttpGet" => "GET",
            "HttpPost" => "POST",
            "HttpPut" => "PUT",
            "HttpPatch" => "PATCH",
            "HttpDelete" => "DELETE",
            "HttpHead" => "HEAD",
            "HttpOptions" => "OPTIONS",
            _ => string.Empty
        };

        return httpMethod.Length > 0;
    }

    private static string NormalizeAttributeName(string value)
    {
        var simpleName = value.Split('.').Last();
        return simpleName.EndsWith("Attribute", StringComparison.Ordinal)
            ? simpleName[..^"Attribute".Length]
            : simpleName;
    }

    private static string? FirstStringArgument(AttributeSyntax attribute)
    {
        var expression = attribute.ArgumentList?.Arguments.FirstOrDefault()?.Expression;
        return expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression)
            ? literal.Token.ValueText
            : null;
    }

    private static bool IsExcluded(string rootPath, string path)
    {
        var relative = Path.GetRelativePath(rootPath, path);
        return relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(segment => ExcludedDirectoryNames.Contains(segment));
    }

    private static string NormalizePath(string path) => path.Replace('\\', '/');
}
