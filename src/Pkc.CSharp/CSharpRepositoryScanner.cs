using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pkc.Core;
using Pkc.Core.Discovery;

namespace Pkc.CSharp;

public sealed class CSharpRepositoryScanner
{
    private static readonly HashSet<string> ExcludedDirectoryNames =
        new(StringComparer.OrdinalIgnoreCase) { ".git", ".pkc", "bin", "obj" };

    private static readonly string[] PublicationMethodPrefixes =
        ["Publish", "Emit", "Dispatch", "Send", "Enqueue", "Produce", "Notify"];

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

        var trees = new List<SyntaxTree>(files.Length);
        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var relativePath = NormalizePath(Path.GetRelativePath(rootPath, file));
            var sourceText = await File.ReadAllTextAsync(file, cancellationToken);
            trees.Add(CSharpSyntaxTree.ParseText(sourceText, path: relativePath, cancellationToken: cancellationToken));
        }

        var compilation = CSharpCompilation.Create(
            "PKC.RepositoryAnalysis",
            trees,
            GetPlatformReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        foreach (var tree in trees)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var relativePath = NormalizePath(tree.FilePath);
            var syntaxRoot = await tree.GetRootAsync(cancellationToken);
            var semanticModel = compilation.GetSemanticModel(tree, ignoreAccessibility: true);

            ExtractTypeFacts(syntaxRoot, relativePath, facts);
            ExtractMethodFacts(syntaxRoot, relativePath, facts, relations, semanticModel, cancellationToken);
            ExtractPropertyFacts(syntaxRoot, relativePath, facts);
            ExtractEnumMemberFacts(syntaxRoot, relativePath, facts);
        }

        return new FactDocument(
            "0.1.1",
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

            var route = GetRouteTemplate(declaration.AttributeLists);
            if (!string.IsNullOrWhiteSpace(route))
            {
                metadata["routeTemplate"] = route;
            }

            AddAuthorizationMetadata(metadata, declaration.AttributeLists);

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
        ICollection<EvidenceRelation> relations,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
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

                    var actionRoute = FirstStringArgument(httpAttribute.Attribute) ?? GetRouteTemplate(attributes);
                    if (!string.IsNullOrWhiteSpace(actionRoute))
                    {
                        metadata["routeTemplate"] = actionRoute;
                    }

                    var containingType = method.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
                    var controllerRoute = containingType is null ? null : GetRouteTemplate(containingType.AttributeLists);
                    if (!string.IsNullOrWhiteSpace(controllerRoute))
                    {
                        metadata["controllerRouteTemplate"] = controllerRoute;
                    }

                    var fullRoute = CombineRoute(controllerRoute, actionRoute, containingType?.Identifier.ValueText, name);
                    if (!string.IsNullOrWhiteSpace(fullRoute))
                    {
                        metadata["fullRoute"] = fullRoute;
                    }
                }

                var containingDeclaration = method.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
                if (containingDeclaration is not null)
                {
                    AddAuthorizationMetadata(metadata, containingDeclaration.AttributeLists.Concat(attributes));
                    if (HasAttribute(containingDeclaration.AttributeLists.Concat(attributes), "AllowAnonymous"))
                    {
                        metadata["allowAnonymous"] = "true";
                    }
                }
                else
                {
                    AddAuthorizationMetadata(metadata, attributes);
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

            ExtractBehaviorFacts(method, fact, relativePath, facts, relations, semanticModel, cancellationToken);
        }
    }

    private static void ExtractBehaviorFacts(
        BaseMethodDeclarationSyntax method,
        EvidenceFact methodFact,
        string relativePath,
        ICollection<EvidenceFact> facts,
        ICollection<EvidenceRelation> relations,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        foreach (var condition in method.DescendantNodes().OfType<IfStatementSyntax>())
        {
            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["expression"] = condition.Condition.ToString(),
                ["guardCandidate"] = condition.Statement.DescendantNodesAndSelf().Any(node => node is ThrowStatementSyntax or ThrowExpressionSyntax)
                    .ToString().ToLowerInvariant()
            };

            var conditionFact = CreateBehaviorFact(condition, relativePath, "condition", "if", methodFact.Name, metadata);
            facts.Add(conditionFact);
            relations.Add(new EvidenceRelation(methodFact.Id, "contains-condition", conditionFact.Id, conditionFact.Source));
        }

        foreach (var throwStatement in method.DescendantNodes().OfType<ThrowStatementSyntax>())
        {
            var expression = throwStatement.Expression;
            var exceptionType = expression is ObjectCreationExpressionSyntax creation
                ? creation.Type.ToString()
                : expression is null
                    ? "rethrow"
                    : semanticModel.GetTypeInfo(expression, cancellationToken).Type?.ToDisplayString() ?? "unknown";

            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["exceptionType"] = exceptionType,
                ["expression"] = expression?.ToString() ?? "throw"
            };

            var throwFact = CreateBehaviorFact(throwStatement, relativePath, "throw", exceptionType, methodFact.Name, metadata);
            facts.Add(throwFact);
            relations.Add(new EvidenceRelation(methodFact.Id, "throws", throwFact.Id, throwFact.Source));
        }

        foreach (var throwExpression in method.DescendantNodes().OfType<ThrowExpressionSyntax>())
        {
            var expression = throwExpression.Expression;
            var exceptionType = expression is ObjectCreationExpressionSyntax creation
                ? creation.Type.ToString()
                : semanticModel.GetTypeInfo(expression, cancellationToken).Type?.ToDisplayString() ?? "unknown";

            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["exceptionType"] = exceptionType,
                ["expression"] = expression.ToString()
            };

            var throwFact = CreateBehaviorFact(throwExpression, relativePath, "throw", exceptionType, methodFact.Name, metadata);
            facts.Add(throwFact);
            relations.Add(new EvidenceRelation(methodFact.Id, "throws", throwFact.Id, throwFact.Source));
        }

        foreach (var assignment in method.DescendantNodes().OfType<AssignmentExpressionSyntax>())
        {
            var targetSymbol = semanticModel.GetSymbolInfo(assignment.Left, cancellationToken).Symbol;
            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["target"] = assignment.Left.ToString(),
                ["value"] = assignment.Right.ToString(),
                ["operator"] = assignment.OperatorToken.ValueText,
                ["targetSymbolKind"] = targetSymbol?.Kind.ToString() ?? "unknown",
                ["stateMutationCandidate"] = (targetSymbol is IPropertySymbol or IFieldSymbol).ToString().ToLowerInvariant()
            };

            if (targetSymbol is not null)
            {
                metadata["targetSymbol"] = targetSymbol.ToDisplayString();
            }

            var mutationFact = CreateBehaviorFact(assignment, relativePath, "mutation", assignment.Left.ToString(), methodFact.Name, metadata);
            facts.Add(mutationFact);
            relations.Add(new EvidenceRelation(methodFact.Id, "mutates", mutationFact.Id, mutationFact.Source));
        }

        foreach (var unary in method.DescendantNodes().Where(node =>
                     node.IsKind(SyntaxKind.PreIncrementExpression) ||
                     node.IsKind(SyntaxKind.PreDecrementExpression) ||
                     node.IsKind(SyntaxKind.PostIncrementExpression) ||
                     node.IsKind(SyntaxKind.PostDecrementExpression)))
        {
            var expression = unary switch
            {
                PrefixUnaryExpressionSyntax prefix => prefix.Operand,
                PostfixUnaryExpressionSyntax postfix => postfix.Operand,
                _ => null
            };

            if (expression is null)
            {
                continue;
            }

            var targetSymbol = semanticModel.GetSymbolInfo(expression, cancellationToken).Symbol;
            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["target"] = expression.ToString(),
                ["operator"] = unary.Kind().ToString(),
                ["targetSymbolKind"] = targetSymbol?.Kind.ToString() ?? "unknown",
                ["stateMutationCandidate"] = (targetSymbol is IPropertySymbol or IFieldSymbol).ToString().ToLowerInvariant()
            };

            var mutationFact = CreateBehaviorFact(unary, relativePath, "mutation", expression.ToString(), methodFact.Name, metadata);
            facts.Add(mutationFact);
            relations.Add(new EvidenceRelation(methodFact.Id, "mutates", mutationFact.Id, mutationFact.Source));
        }

        foreach (var invocation in method.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            var syntaxTarget = invocation.Expression.ToString();
            relations.Add(new EvidenceRelation(
                methodFact.Id,
                "invokes-syntax",
                syntaxTarget,
                GetLocation(invocation, relativePath)));

            var symbolInfo = semanticModel.GetSymbolInfo(invocation, cancellationToken);
            var methodSymbol = symbolInfo.Symbol as IMethodSymbol ?? symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();
            var resolvedTarget = methodSymbol is null ? null : GetMethodTarget(methodSymbol);

            if (!string.IsNullOrWhiteSpace(resolvedTarget))
            {
                relations.Add(new EvidenceRelation(
                    methodFact.Id,
                    "invokes",
                    resolvedTarget,
                    GetLocation(invocation, relativePath)));
            }

            var calledName = methodSymbol?.Name ?? GetInvocationName(invocation.Expression);
            if (IsPublicationCandidate(calledName))
            {
                relations.Add(new EvidenceRelation(
                    methodFact.Id,
                    "message-publication-candidate",
                    resolvedTarget ?? syntaxTarget,
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
        var id = $"cs:{relativePath}:{location.StartLine}:{kind}:{name}";

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

    private static EvidenceFact CreateBehaviorFact(
        SyntaxNode node,
        string relativePath,
        string kind,
        string name,
        string container,
        IReadOnlyDictionary<string, string> metadata)
    {
        var location = GetLocation(node, relativePath);
        var id = $"cs:{relativePath}:{location.StartLine}:{kind}:{name}";
        return new EvidenceFact(id, kind, name, container, location, [], metadata);
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
        var argument = attribute.ArgumentList?.Arguments
            .FirstOrDefault(item => item.NameEquals is null && item.NameColon is null);
        return argument is null ? null : StringLiteralValue(argument.Expression);
    }

    private static string? NamedStringArgument(AttributeSyntax attribute, string name)
    {
        var argument = attribute.ArgumentList?.Arguments.FirstOrDefault(item =>
            string.Equals(item.NameEquals?.Name.Identifier.ValueText, name, StringComparison.Ordinal));
        return argument is null ? null : StringLiteralValue(argument.Expression);
    }

    private static string? StringLiteralValue(ExpressionSyntax expression) =>
        expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression)
            ? literal.Token.ValueText
            : null;

    private static string? GetRouteTemplate(SyntaxList<AttributeListSyntax> lists)
    {
        var route = lists.SelectMany(list => list.Attributes)
            .FirstOrDefault(attribute => NormalizeAttributeName(attribute.Name.ToString()) == "Route");
        return route is null ? null : FirstStringArgument(route);
    }

    private static void AddAuthorizationMetadata(
        IDictionary<string, string> metadata,
        IEnumerable<AttributeListSyntax> lists)
    {
        var attributes = lists.SelectMany(list => list.Attributes).ToArray();

        // Organization-specific authorization attributes (for example a claim or module requirement) are permission
        // evidence too; they are recorded verbatim because their semantics are defined by the target repository.
        var requirements = attributes
            .Where(attribute => NormalizeAttributeName(attribute.Name.ToString()) is var name &&
                                name != "Authorize" &&
                                name.EndsWith("Authorize", StringComparison.Ordinal))
            .Select(attribute => attribute.ToString())
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (requirements.Length > 0)
        {
            metadata["authorizationRequirements"] = string.Join(" | ", requirements);
        }

        var authorizeAttributes = attributes
            .Where(attribute => NormalizeAttributeName(attribute.Name.ToString()) == "Authorize")
            .ToArray();

        if (authorizeAttributes.Length == 0)
        {
            return;
        }

        metadata["authorization"] = string.Join(" | ", authorizeAttributes.Select(attribute => attribute.ToString()));

        var policies = authorizeAttributes
            .Select(attribute => NamedStringArgument(attribute, "Policy") ?? FirstStringArgument(attribute))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (policies.Length > 0)
        {
            metadata["authorizationPolicies"] = string.Join(", ", policies);
        }

        var roles = authorizeAttributes
            .Select(attribute => NamedStringArgument(attribute, "Roles"))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (roles.Length > 0)
        {
            metadata["authorizationRoles"] = string.Join(", ", roles);
        }
    }

    private static bool HasAttribute(IEnumerable<AttributeListSyntax> lists, string name) =>
        lists.SelectMany(list => list.Attributes)
            .Any(attribute => NormalizeAttributeName(attribute.Name.ToString()) == name);

    private static string? CombineRoute(string? controllerRoute, string? actionRoute, string? controllerName, string actionName)
    {
        var controller = NormalizeRoutePart(controllerRoute);
        var action = NormalizeRoutePart(actionRoute);
        var combined = string.Join("/", new[] { controller, action }.Where(part => !string.IsNullOrWhiteSpace(part)));
        if (combined.Length == 0)
        {
            return null;
        }

        var controllerToken = controllerName?.EndsWith("Controller", StringComparison.Ordinal) == true
            ? controllerName[..^"Controller".Length]
            : controllerName;

        return combined
            .Replace("[controller]", controllerToken ?? string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("[action]", actionName, StringComparison.OrdinalIgnoreCase);
    }

    private static string? NormalizeRoutePart(string? route) =>
        string.IsNullOrWhiteSpace(route) ? null : route.Trim().Trim('/');

    private static string GetMethodTarget(IMethodSymbol symbol)
    {
        var type = symbol.ContainingType?.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
        return string.IsNullOrWhiteSpace(type) ? symbol.Name : $"{type}.{symbol.Name}";
    }

    private static string GetInvocationName(ExpressionSyntax expression) => expression switch
    {
        MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.ValueText,
        IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
        GenericNameSyntax generic => generic.Identifier.ValueText,
        _ => expression.ToString()
    };

    private static bool IsPublicationCandidate(string? methodName) =>
        !string.IsNullOrWhiteSpace(methodName) &&
        PublicationMethodPrefixes.Any(prefix => methodName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

    private static IReadOnlyList<MetadataReference> GetPlatformReferences()
    {
        var trustedPlatformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        if (string.IsNullOrWhiteSpace(trustedPlatformAssemblies))
        {
            return [];
        }

        return trustedPlatformAssemblies
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Where(File.Exists)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
            .ToArray();
    }

    private static bool IsExcluded(string rootPath, string path)
    {
        var relative = Path.GetRelativePath(rootPath, path);
        return relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(segment => ExcludedDirectoryNames.Contains(segment)) ||
            SemanticSourceScope.Excludes(rootPath, path);
    }

    private static string NormalizePath(string path) => path.Replace('\\', '/');
}
