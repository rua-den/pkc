using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pkc.Core;

namespace Pkc.CSharp;

internal sealed class CSharpValidationEvidenceScanner
{
    public async Task<FactDocument> ScanAsync(
        string repositoryPath,
        CancellationToken cancellationToken = default)
    {
        var rootPath = Path.GetFullPath(repositoryPath);
        var facts = new List<EvidenceFact>();

        foreach (var file in Directory.EnumerateFiles(rootPath, "*.cs", SearchOption.AllDirectories)
                     .Where(path => !CSharpSourceScope.IsExcludedRelativePath(
                         Path.GetRelativePath(rootPath, path).Replace('\\', '/')))
                     .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var text = await File.ReadAllTextAsync(file, cancellationToken);
            var relativePath = Path.GetRelativePath(rootPath, file).Replace('\\', '/');
            var root = await CSharpSyntaxTree.ParseText(text, path: relativePath)
                .GetRootAsync(cancellationToken);

            ExtractPropertyValidation(root, relativePath, facts);
            ExtractConditionalValidation(root, relativePath, facts);
        }

        return new FactDocument(
            "0.4.4-csharp-validation",
            facts
                .GroupBy(fact => fact.Id, StringComparer.Ordinal)
                .Select(group => group.First())
                .OrderBy(fact => fact.Id, StringComparer.Ordinal)
                .ToArray(),
            []);
    }

    private static void ExtractPropertyValidation(
        SyntaxNode root,
        string relativePath,
        ICollection<EvidenceFact> facts)
    {
        foreach (var property in root.DescendantNodes().OfType<PropertyDeclarationSyntax>())
        {
            var requiredAttribute = property.AttributeLists
                .SelectMany(list => list.Attributes)
                .FirstOrDefault(attribute =>
                {
                    var name = attribute.Name.ToString();
                    return name is "Required" or "RequiredAttribute" ||
                           name.EndsWith(".Required", StringComparison.Ordinal) ||
                           name.EndsWith(".RequiredAttribute", StringComparison.Ordinal);
                });

            if (requiredAttribute is null)
            {
                continue;
            }

            var declaringType = property.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
            var declaringTypeName = declaringType?.Identifier.ValueText;
            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["field"] = property.Identifier.ValueText,
                ["property"] = property.Identifier.ValueText,
                ["behavior"] = "required",
                ["validationSource"] = "data-annotations",
                ["analysisMode"] = "csharp-syntax-validation",
                ["analysisConfidence"] = "high"
            };

            if (!string.IsNullOrWhiteSpace(declaringTypeName))
            {
                metadata["declaringType"] = declaringTypeName;
            }

            facts.Add(CreateFact(
                property,
                relativePath,
                property.Identifier.ValueText,
                declaringTypeName,
                metadata));
        }
    }

    private static void ExtractConditionalValidation(
        SyntaxNode root,
        string relativePath,
        ICollection<EvidenceFact> facts)
    {
        foreach (var condition in root.DescendantNodes().OfType<IfStatementSyntax>())
        {
            if (!TryFindMissingValueCheck(condition.Condition, out var fieldAccess, out var missingExpression))
            {
                continue;
            }

            var propertyName = fieldAccess.Name.Identifier.ValueText;
            var parameterName = FindRootIdentifier(fieldAccess.Expression);
            var requirementExpression = RemoveMissingPredicate(condition.Condition, missingExpression);
            var conditionKey = TryBuildConditionKey(requirementExpression);
            var method = condition.Ancestors().OfType<BaseMethodDeclarationSyntax>().FirstOrDefault();
            var methodName = method switch
            {
                MethodDeclarationSyntax declaration => declaration.Identifier.ValueText,
                ConstructorDeclarationSyntax declaration => declaration.Identifier.ValueText,
                _ => null
            };

            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["field"] = propertyName,
                ["property"] = propertyName,
                ["behavior"] = "required",
                ["validationSource"] = "guard-condition",
                ["validationExpression"] = condition.Condition.ToString(),
                ["analysisMode"] = "csharp-syntax-validation",
                ["analysisConfidence"] = "medium"
            };

            if (!string.IsNullOrWhiteSpace(parameterName))
            {
                metadata["parameterName"] = parameterName;
            }

            if (!string.IsNullOrWhiteSpace(methodName))
            {
                metadata["method"] = methodName;
            }

            if (requirementExpression is not null)
            {
                metadata["condition"] = requirementExpression.ToString();
            }

            if (!string.IsNullOrWhiteSpace(conditionKey))
            {
                metadata["conditionKey"] = conditionKey;
            }

            facts.Add(CreateFact(
                condition,
                relativePath,
                propertyName,
                methodName,
                metadata));
        }
    }

    private static bool TryFindMissingValueCheck(
        ExpressionSyntax expression,
        out MemberAccessExpressionSyntax fieldAccess,
        out ExpressionSyntax missingExpression)
    {
        foreach (var candidate in FlattenLogicalAnd(expression))
        {
            if (candidate is InvocationExpressionSyntax invocation &&
                invocation.Expression is MemberAccessExpressionSyntax methodAccess &&
                methodAccess.Expression.ToString() == "string" &&
                methodAccess.Name.Identifier.ValueText is "IsNullOrWhiteSpace" or "IsNullOrEmpty" &&
                invocation.ArgumentList.Arguments.Count == 1 &&
                invocation.ArgumentList.Arguments[0].Expression is MemberAccessExpressionSyntax member)
            {
                fieldAccess = member;
                missingExpression = candidate;
                return true;
            }

            if (candidate is BinaryExpressionSyntax binary &&
                binary.IsKind(SyntaxKind.EqualsExpression))
            {
                if (binary.Left is MemberAccessExpressionSyntax leftMember &&
                    binary.Right.IsKind(SyntaxKind.NullLiteralExpression))
                {
                    fieldAccess = leftMember;
                    missingExpression = candidate;
                    return true;
                }

                if (binary.Right is MemberAccessExpressionSyntax rightMember &&
                    binary.Left.IsKind(SyntaxKind.NullLiteralExpression))
                {
                    fieldAccess = rightMember;
                    missingExpression = candidate;
                    return true;
                }
            }
        }

        fieldAccess = null!;
        missingExpression = null!;
        return false;
    }

    private static IEnumerable<ExpressionSyntax> FlattenLogicalAnd(ExpressionSyntax expression)
    {
        if (expression is BinaryExpressionSyntax binary &&
            binary.IsKind(SyntaxKind.LogicalAndExpression))
        {
            foreach (var left in FlattenLogicalAnd(binary.Left))
            {
                yield return left;
            }

            foreach (var right in FlattenLogicalAnd(binary.Right))
            {
                yield return right;
            }

            yield break;
        }

        yield return expression;
    }

    private static ExpressionSyntax? RemoveMissingPredicate(
        ExpressionSyntax expression,
        ExpressionSyntax missingExpression)
    {
        var remaining = FlattenLogicalAnd(expression)
            .Where(item => item.Span != missingExpression.Span)
            .ToArray();

        if (remaining.Length == 0)
        {
            return null;
        }

        ExpressionSyntax current = remaining[0];
        for (var index = 1; index < remaining.Length; index++)
        {
            current = SyntaxFactory.BinaryExpression(
                SyntaxKind.LogicalAndExpression,
                current,
                remaining[index]);
        }

        return current;
    }

    private static string? TryBuildConditionKey(ExpressionSyntax? expression)
    {
        if (expression is null)
        {
            return null;
        }

        foreach (var candidate in FlattenLogicalAnd(expression))
        {
            if (candidate is not BinaryExpressionSyntax binary ||
                !binary.IsKind(SyntaxKind.EqualsExpression))
            {
                continue;
            }

            var field = FinalIdentifier(binary.Left);
            var value = FinalValue(binary.Right);
            if (string.IsNullOrWhiteSpace(field) || string.IsNullOrWhiteSpace(value))
            {
                field = FinalIdentifier(binary.Right);
                value = FinalValue(binary.Left);
            }

            if (!string.IsNullOrWhiteSpace(field) && !string.IsNullOrWhiteSpace(value))
            {
                return $"{NormalizeToken(field)}={NormalizeToken(value)}";
            }
        }

        return null;
    }

    private static string? FinalIdentifier(ExpressionSyntax expression) => expression switch
    {
        MemberAccessExpressionSyntax member => member.Name.Identifier.ValueText,
        IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
        _ => null
    };

    private static string? FinalValue(ExpressionSyntax expression) => expression switch
    {
        LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.StringLiteralExpression) =>
            literal.Token.ValueText,
        MemberAccessExpressionSyntax member => member.Name.Identifier.ValueText,
        IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
        _ => null
    };

    private static string? FindRootIdentifier(ExpressionSyntax expression)
    {
        while (expression is MemberAccessExpressionSyntax member)
        {
            expression = member.Expression;
        }

        return expression is IdentifierNameSyntax identifier
            ? identifier.Identifier.ValueText
            : null;
    }

    private static string NormalizeToken(string value) =>
        new(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());

    private static EvidenceFact CreateFact(
        SyntaxNode node,
        string relativePath,
        string field,
        string? container,
        IReadOnlyDictionary<string, string> metadata)
    {
        var span = node.GetLocation().GetLineSpan();
        var source = new SourceLocation(
            relativePath,
            span.StartLinePosition.Line + 1,
            span.EndLinePosition.Line + 1);

        return new EvidenceFact(
            $"csval:{relativePath}:{source.StartLine}:backend-field-validation:{field}",
            "backend-field-validation",
            field,
            container,
            source,
            [],
            metadata);
    }
}
