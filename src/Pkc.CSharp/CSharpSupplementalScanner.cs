using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pkc.Core;

namespace Pkc.CSharp;

internal sealed class CSharpSupplementalScanner
{
    private static readonly HashSet<string> ExcludedDirectoryNames =
        new(StringComparer.OrdinalIgnoreCase) { ".git", ".pkc", "bin", "obj", "knowledge" };

    private static readonly HashSet<string> CollectionMutationMethods =
        new(StringComparer.Ordinal) { "Add", "AddRange", "Insert", "Enqueue", "Push" };

    public async Task<FactDocument> ScanAsync(
        string repositoryPath,
        FactDocument baseline,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryPath);
        ArgumentNullException.ThrowIfNull(baseline);

        var rootPath = Path.GetFullPath(repositoryPath);
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

            ExtractComputedProperties(syntaxRoot, relativePath, facts);
            ExtractMethodSemantics(syntaxRoot, relativePath, baseline, facts, relations);
            ExtractAuthorizationPolicies(syntaxRoot, relativePath, facts);
        }

        return new FactDocument(
            "0.4.2-csharp-supplemental",
            facts.OrderBy(fact => fact.Id, StringComparer.Ordinal).ToArray(),
            relations
                .OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
                .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
                .ThenBy(relation => relation.Target, StringComparer.Ordinal)
                .ToArray());
    }

    private static void ExtractComputedProperties(
        SyntaxNode root,
        string relativePath,
        ICollection<EvidenceFact> facts)
    {
        foreach (var property in root.DescendantNodes().OfType<PropertyDeclarationSyntax>())
        {
            var expression = property.ExpressionBody?.Expression
                ?? property.AccessorList?.Accessors
                    .FirstOrDefault(accessor =>
                        accessor.IsKind(SyntaxKind.GetAccessorDeclaration) &&
                        accessor.ExpressionBody is not null)
                    ?.ExpressionBody?.Expression;

            if (expression is null)
            {
                continue;
            }

            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["type"] = property.Type.ToString(),
                ["expression"] = expression.ToString()
            };

            facts.Add(CreateFact(
                property,
                relativePath,
                "computed-property",
                property.Identifier.ValueText,
                GetContainingType(property),
                metadata));
        }
    }

    private static void ExtractMethodSemantics(
        SyntaxNode root,
        string relativePath,
        FactDocument baseline,
        ICollection<EvidenceFact> facts,
        ICollection<EvidenceRelation> relations)
    {
        foreach (var method in root.DescendantNodes().OfType<BaseMethodDeclarationSyntax>())
        {
            var owner = FindBaselineMethod(method, relativePath, baseline);
            if (owner is null)
            {
                continue;
            }

            foreach (var loop in method.DescendantNodes().OfType<ForEachStatementSyntax>())
            {
                var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["iterator"] = loop.Identifier.ValueText,
                    ["collection"] = loop.Expression.ToString()
                };

                var loopFact = CreateFact(
                    loop,
                    relativePath,
                    "loop",
                    "foreach",
                    owner.Name,
                    metadata);

                facts.Add(loopFact);
                relations.Add(new EvidenceRelation(owner.Id, "contains-loop", loopFact.Id, loopFact.Source));
            }

            foreach (var creation in method.DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
            {
                if (creation.Initializer is null)
                {
                    continue;
                }

                var invocation = creation.Ancestors()
                    .OfType<InvocationExpressionSyntax>()
                    .FirstOrDefault(candidate =>
                        candidate.ArgumentList.Arguments.Any(argument =>
                            argument.Expression.Span.Contains(creation.Span)));

                if (invocation is null)
                {
                    continue;
                }

                var operationName = GetInvocationName(invocation.Expression);
                if (!CollectionMutationMethods.Contains(operationName))
                {
                    continue;
                }

                var assignments = creation.Initializer.Expressions
                    .OfType<AssignmentExpressionSyntax>()
                    .Where(assignment => assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
                    .Select(assignment => new
                    {
                        Target = assignment.Left.ToString(),
                        Value = assignment.Right.ToString()
                    })
                    .Where(item =>
                        !(string.Equals(item.Target, "Id", StringComparison.Ordinal) &&
                          item.Value.StartsWith("_next", StringComparison.Ordinal)))
                    .Select(item => $"{item.Target} = {item.Value}")
                    .ToArray();

                if (assignments.Length == 0)
                {
                    continue;
                }

                var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["type"] = creation.Type.ToString(),
                    ["operation"] = invocation.Expression.ToString(),
                    ["assignments"] = string.Join("; ", assignments)
                };

                var constructionFact = CreateFact(
                    creation,
                    relativePath,
                    "object-construction",
                    creation.Type.ToString(),
                    owner.Name,
                    metadata);

                facts.Add(constructionFact);
                relations.Add(new EvidenceRelation(owner.Id, "constructs", constructionFact.Id, constructionFact.Source));
            }
        }
    }

    private static void ExtractAuthorizationPolicies(
        SyntaxNode root,
        string relativePath,
        ICollection<EvidenceFact> facts)
    {
        foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (!string.Equals(GetInvocationName(invocation.Expression), "AddPolicy", StringComparison.Ordinal))
            {
                continue;
            }

            if (invocation.ArgumentList.Arguments.Count < 2)
            {
                continue;
            }

            var nameExpression = invocation.ArgumentList.Arguments[0].Expression;
            if (nameExpression is not LiteralExpressionSyntax literal ||
                !literal.IsKind(SyntaxKind.StringLiteralExpression))
            {
                continue;
            }

            var policyName = literal.Token.ValueText;
            var definition = invocation.ArgumentList.Arguments[1].Expression.ToString();

            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["policyName"] = policyName,
                ["definition"] = definition
            };

            facts.Add(CreateFact(
                invocation,
                relativePath,
                "authorization-policy",
                policyName,
                null,
                metadata));
        }
    }

    private static EvidenceFact? FindBaselineMethod(
        BaseMethodDeclarationSyntax method,
        string relativePath,
        FactDocument baseline)
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

        var location = GetLocation(method, relativePath);
        var container = GetContainingType(method);

        return baseline.Facts.FirstOrDefault(fact =>
            fact.Kind is "method" or "endpoint" or "constructor" &&
            string.Equals(fact.Source.Path, relativePath, StringComparison.Ordinal) &&
            fact.Source.StartLine == location.StartLine &&
            string.Equals(fact.Name, name, StringComparison.Ordinal) &&
            string.Equals(fact.Container, container, StringComparison.Ordinal));
    }

    private static string GetInvocationName(ExpressionSyntax expression) => expression switch
    {
        IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
        MemberAccessExpressionSyntax member => member.Name.Identifier.ValueText,
        MemberBindingExpressionSyntax memberBinding => memberBinding.Name.Identifier.ValueText,
        _ => expression.ToString().Split('.').LastOrDefault() ?? expression.ToString()
    };

    private static EvidenceFact CreateFact(
        SyntaxNode node,
        string relativePath,
        string kind,
        string name,
        string? container,
        IReadOnlyDictionary<string, string> metadata)
    {
        var location = GetLocation(node, relativePath);
        return new EvidenceFact(
            $"cs:{relativePath}:{location.StartLine}:{kind}:{name}",
            kind,
            name,
            container,
            location,
            [],
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
        var type = node.Ancestors().OfType<BaseTypeDeclarationSyntax>().FirstOrDefault();
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
        node.AncestorsAndSelf()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .FirstOrDefault()
            ?.Name.ToString();

    private static bool IsExcluded(string rootPath, string path)
    {
        var relative = Path.GetRelativePath(rootPath, path);
        return relative
            .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(segment => ExcludedDirectoryNames.Contains(segment));
    }

    private static string NormalizePath(string path) => path.Replace('\\', '/');
}
