using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Operations;
using Pkc.Core;

namespace Pkc.CSharp;

internal sealed class CSharpSelectedApiProjectionLineageEnricher
{
    private static readonly object RegistrationGate = new();

    public async Task<FactDocument> EnrichAsync(
        string repositoryPath,
        FactDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryPath);
        ArgumentNullException.ThrowIfNull(document);

        var rootPath = Path.GetFullPath(repositoryPath);
        var endpoints = document.Facts
            .Where(fact => fact.Kind == "endpoint")
            .Where(fact => fact.Metadata.GetValueOrDefault("analysisMode") == "project-semantic")
            .Where(fact => !string.IsNullOrWhiteSpace(fact.Metadata.GetValueOrDefault("semanticProject")))
            .ToArray();
        if (endpoints.Length == 0)
        {
            return document;
        }

        EnsureMsBuildRegistered();
        var facts = document.Facts.ToDictionary(fact => fact.Id, StringComparer.Ordinal);
        var relations = document.Relations.ToList();

        foreach (var projectGroup in endpoints.GroupBy(
                     endpoint => endpoint.Metadata["semanticProject"],
                     StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var projectPath = projectGroup.Key;
            var fullProjectPath = Path.GetFullPath(Path.Combine(
                rootPath,
                projectPath.Replace('/', Path.DirectorySeparatorChar)));
            if (!File.Exists(fullProjectPath))
            {
                continue;
            }

            try
            {
                using var workspace = MSBuildWorkspace.Create();
                var project = await workspace.OpenProjectAsync(
                    fullProjectPath,
                    cancellationToken: cancellationToken);
                var compilation = await project.GetCompilationAsync(cancellationToken);
                if (compilation is null)
                {
                    continue;
                }

                foreach (var endpoint in projectGroup)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var fullSourcePath = Path.GetFullPath(Path.Combine(
                        rootPath,
                        endpoint.Source.Path.Replace('/', Path.DirectorySeparatorChar)));
                    var tree = compilation.SyntaxTrees.FirstOrDefault(candidate =>
                        !string.IsNullOrWhiteSpace(candidate.FilePath) &&
                        string.Equals(
                            Path.GetFullPath(candidate.FilePath),
                            fullSourcePath,
                            StringComparison.OrdinalIgnoreCase));
                    if (tree is null)
                    {
                        continue;
                    }

                    var syntaxRoot = await tree.GetRootAsync(cancellationToken);
                    var method = syntaxRoot.DescendantNodes()
                        .OfType<MethodDeclarationSyntax>()
                        .FirstOrDefault(candidate =>
                            candidate.Identifier.ValueText == endpoint.Name &&
                            StartLine(candidate) == endpoint.Source.StartLine);
                    if (method?.Body is null)
                    {
                        continue;
                    }

                    var model = compilation.GetSemanticModel(tree, ignoreAccessibility: true);
                    var extraction = Extract(
                        endpoint,
                        projectPath,
                        rootPath,
                        method,
                        model,
                        cancellationToken);
                    foreach (var fact in extraction.Facts)
                    {
                        facts[fact.Id] = fact;
                    }

                    relations.AddRange(extraction.Relations);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                // Optional stronger proof: preserve all previously accepted evidence.
            }
        }

        return document with
        {
            Facts = facts.Values.OrderBy(fact => fact.Id, StringComparer.Ordinal).ToArray(),
            Relations = relations
                .GroupBy(RelationKey, StringComparer.Ordinal)
                .Select(group => group.First())
                .OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
                .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
                .ThenBy(relation => relation.Target, StringComparer.Ordinal)
                .ToArray()
        };
    }

    private static ExtractionResult Extract(
        EvidenceFact endpoint,
        string projectPath,
        string rootPath,
        MethodDeclarationSyntax method,
        SemanticModel model,
        CancellationToken cancellationToken)
    {
        var body = method.Body!;
        if (body.Statements.Count != 2 ||
            body.Statements[0] is not LocalDeclarationStatementSyntax selectionDeclaration ||
            selectionDeclaration.Declaration.Variables.Count != 1 ||
            body.Statements[1] is not ReturnStatementSyntax returnStatement ||
            returnStatement.Expression is null)
        {
            return ExtractionResult.Empty;
        }

        var variable = selectionDeclaration.Declaration.Variables[0];
        if (variable.Initializer?.Value is not InvocationExpressionSyntax selectionInvocation ||
            model.GetDeclaredSymbol(variable, cancellationToken) is not ILocalSymbol selectedLocal ||
            !selectedLocal.Type.IsReferenceType ||
            !TryDescribeSelection(
                selectionInvocation,
                selectedLocal,
                model,
                cancellationToken,
                out var selection))
        {
            return ExtractionResult.Empty;
        }

        if (UnwrapParentheses(returnStatement.Expression) is not ObjectCreationExpressionSyntax responseCreation ||
            model.GetOperation(responseCreation, cancellationToken) is not IObjectCreationOperation responseOperation ||
            responseOperation.Type is not INamedTypeSymbol responseType ||
            !IsImplicitParameterlessConstruction(responseOperation) ||
            responseCreation.Initializer is null ||
            responseCreation.Initializer.Expressions.Count != 1 ||
            model.GetDeclaredSymbol(method, cancellationToken) is not IMethodSymbol methodSymbol ||
            !SymbolEqualityComparer.Default.Equals(methodSymbol.ReturnType, responseType))
        {
            return ExtractionResult.Empty;
        }

        var facts = new List<EvidenceFact>();
        var relations = new List<EvidenceRelation>();

        foreach (var expression in responseCreation.Initializer.Expressions)
        {
            if (expression is not AssignmentExpressionSyntax assignment ||
                !assignment.IsKind(SyntaxKind.SimpleAssignmentExpression) ||
                model.GetSymbolInfo(assignment.Left, cancellationToken).Symbol is not IPropertySymbol targetProperty ||
                !SymbolEqualityComparer.Default.Equals(targetProperty.ContainingType, responseType) ||
                !IsSupportedScalarAutoProperty(targetProperty) ||
                model.GetConversion(assignment.Right, cancellationToken).IsUserDefined ||
                !TryResolveSelectedSource(
                    assignment.Right,
                    selectedLocal,
                    model,
                    cancellationToken,
                    out var sourceProperty,
                    out var sourceOccurrence) ||
                !IsSupportedScalarAutoProperty(sourceProperty) ||
                !SymbolEqualityComparer.Default.Equals(sourceProperty.Type, targetProperty.Type))
            {
                return ExtractionResult.Empty;
            }

            var projectionLocation = GetLocation(assignment, endpoint.Source.Path);
            var responseLocation = GetLocation(returnStatement, endpoint.Source.Path);
            var sourceMember = BuildMemberDisplay(sourceProperty);
            var targetMember = BuildMemberDisplay(targetProperty);
            var sourceMemberIdentity = BuildMemberIdentity(sourceProperty);
            var targetMemberIdentity = BuildMemberIdentity(targetProperty);
            var responseTypeIdentity = BuildTypeIdentity(responseType);
            var projectionId = $"cs-lineage:{endpoint.Source.Path}:{assignment.SpanStart}:api-projection";

            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["knowledgeClass"] = "value-lineage",
                ["scopeFactId"] = endpoint.Id,
                ["scopeName"] = endpoint.Name,
                ["semanticProject"] = projectPath,
                ["analysisMode"] = "project-semantic",
                ["analysisConfidence"] = "high",
                ["proof"] = "enumerable-selection+direct-return-project-semantic-object-initializer-projection",
                ["mechanism"] = "api-projection",
                ["temporalSemantics"] = "snapshot",
                ["compositionStatus"] = "direct-api-response-projection",
                ["sourceOccurrence"] = sourceOccurrence,
                ["targetOccurrence"] = targetMember,
                ["sourceReceiver"] = selectedLocal.Name,
                ["sourceReceiverIdentity"] = BuildReceiverIdentity(selectedLocal),
                ["sourceMember"] = sourceMember,
                ["targetMember"] = targetMember,
                ["sourceMemberIdentity"] = sourceMemberIdentity,
                ["targetMemberIdentity"] = targetMemberIdentity,
                ["sourceTypeIdentity"] = BuildTypeIdentity(sourceProperty.ContainingType),
                ["targetTypeIdentity"] = BuildTypeIdentity(targetProperty.ContainingType),
                ["responseTypeIdentity"] = responseTypeIdentity,
                ["sourceAssemblyIdentity"] = sourceProperty.ContainingAssembly?.Identity.ToString() ?? "unknown-assembly",
                ["targetAssemblyIdentity"] = targetProperty.ContainingAssembly?.Identity.ToString() ?? "unknown-assembly",
                ["sourceMemberLocation"] = DescribeSymbolLocation(sourceProperty, rootPath),
                ["targetMemberLocation"] = DescribeSymbolLocation(targetProperty, rootPath),
                ["projectionLocation"] = DescribeLocation(assignment, endpoint.Source.Path),
                ["responseLocation"] = DescribeLocation(returnStatement, endpoint.Source.Path),
                ["responseBoundary"] = "direct-return-object-initializer",
                ["endpointFactId"] = endpoint.Id,
                ["selectionOperation"] = selection.Operation,
                ["selectionSource"] = selection.Source,
                ["selectionPredicateExpression"] = selection.PredicateExpression,
                ["selectionInvocationSpanStart"] = selectionInvocation.SpanStart.ToString(),
                ["selectionLocation"] = DescribeLocation(selectionInvocation, endpoint.Source.Path),
                ["selectedItemTypeIdentity"] = BuildTypeIdentity(selectedLocal.Type)
            };

            var projection = new EvidenceFact(
                projectionId,
                "value-transfer",
                $"{sourceOccurrence} -> {targetMember}",
                endpoint.Container,
                projectionLocation,
                [],
                metadata);
            facts.Add(projection);
            relations.Add(new EvidenceRelation(
                endpoint.Id,
                "returns-response",
                projection.Id,
                projectionLocation));

            var terminalMetadata = new Dictionary<string, string>(metadata, StringComparer.Ordinal)
            {
                ["proof"] = "enumerable-selection+direct-return-project-semantic-object-initializer-response-field",
                ["boundary"] = "API response field",
                ["returnedOccurrence"] = targetMember,
                ["returnedMemberIdentity"] = targetMemberIdentity,
                ["sourceFactId"] = projection.Id,
                ["sourceMechanism"] = "api-projection",
                ["sourceFactLocation"] = DescribeLocation(assignment, endpoint.Source.Path)
            };

            var terminal = new EvidenceFact(
                $"cs-lineage:{endpoint.Source.Path}:{assignment.SpanStart}:api-response-terminal",
                "value-terminal-source",
                $"last source before API response field: {targetMember}",
                endpoint.Container,
                responseLocation,
                [],
                terminalMetadata);
            facts.Add(terminal);
            relations.Add(new EvidenceRelation(
                endpoint.Id,
                "returns-response",
                terminal.Id,
                responseLocation));
        }

        return facts.Count == 0
            ? ExtractionResult.Empty
            : new ExtractionResult(facts, relations);
    }

    private static bool TryDescribeSelection(
        InvocationExpressionSyntax invocation,
        ILocalSymbol selectedLocal,
        SemanticModel model,
        CancellationToken cancellationToken,
        out SelectionDescription description)
    {
        description = default!;
        if (model.GetOperation(invocation, cancellationToken) is not IInvocationOperation invocationOperation)
        {
            return false;
        }

        var target = invocationOperation.TargetMethod.ReducedFrom ?? invocationOperation.TargetMethod;
        var containingType = target.ContainingType?.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
        if (!string.Equals(containingType, "System.Linq.Enumerable", StringComparison.Ordinal) ||
            (target.Name != "Single" && target.Name != "First"))
        {
            return false;
        }

        ExpressionSyntax sourceExpression;
        LambdaExpressionSyntax predicate;
        if (invocation.Expression is MemberAccessExpressionSyntax member &&
            invocation.ArgumentList.Arguments.Count == 1 &&
            invocation.ArgumentList.Arguments[0].Expression is LambdaExpressionSyntax extensionPredicate)
        {
            sourceExpression = member.Expression;
            predicate = extensionPredicate;
        }
        else if (invocation.Expression is MemberAccessExpressionSyntax staticMember &&
                 string.Equals(staticMember.Expression.ToString(), "Enumerable", StringComparison.Ordinal) &&
                 invocation.ArgumentList.Arguments.Count == 2 &&
                 invocation.ArgumentList.Arguments[1].Expression is LambdaExpressionSyntax staticPredicate)
        {
            sourceExpression = invocation.ArgumentList.Arguments[0].Expression;
            predicate = staticPredicate;
        }
        else
        {
            return false;
        }

        if (predicate.Body is not ExpressionSyntax predicateBody ||
            predicateBody.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>().Any())
        {
            return false;
        }

        ParameterSyntax? parameterSyntax = predicate switch
        {
            SimpleLambdaExpressionSyntax simple => simple.Parameter,
            ParenthesizedLambdaExpressionSyntax parenthesized
                when parenthesized.ParameterList.Parameters.Count == 1 =>
                parenthesized.ParameterList.Parameters[0],
            _ => null
        };
        if (parameterSyntax is null ||
            model.GetDeclaredSymbol(parameterSyntax, cancellationToken) is not IParameterSymbol parameter ||
            !SymbolEqualityComparer.Default.Equals(parameter.Type, selectedLocal.Type))
        {
            return false;
        }

        description = new SelectionDescription(
            target.Name,
            sourceExpression.ToString(),
            predicateBody.ToString());
        return true;
    }

    private static bool TryResolveSelectedSource(
        ExpressionSyntax expression,
        ILocalSymbol selectedLocal,
        SemanticModel model,
        CancellationToken cancellationToken,
        out IPropertySymbol property,
        out string occurrence)
    {
        var operation = UnwrapSafeConversions(model.GetOperation(expression, cancellationToken));
        if (operation is not IPropertyReferenceOperation propertyReference ||
            propertyReference.Instance is null ||
            UnwrapSafeConversions(propertyReference.Instance) is not ILocalReferenceOperation localReference ||
            !SymbolEqualityComparer.Default.Equals(localReference.Local, selectedLocal))
        {
            property = default!;
            occurrence = string.Empty;
            return false;
        }

        property = propertyReference.Property;
        occurrence = UnwrapParentheses(expression).ToString();
        return true;
    }

    private static IOperation? UnwrapSafeConversions(IOperation? operation)
    {
        while (operation is IParenthesizedOperation or IConversionOperation)
        {
            if (operation is IConversionOperation conversion && conversion.OperatorMethod is not null)
            {
                return null;
            }

            operation = operation switch
            {
                IParenthesizedOperation parenthesized => parenthesized.Operand,
                IConversionOperation conversion => conversion.Operand,
                _ => operation
            };
        }

        return operation;
    }

    private static bool IsImplicitParameterlessConstruction(IObjectCreationOperation creation) =>
        creation.Constructor is { IsImplicitlyDeclared: true } constructor &&
        constructor.Parameters.Length == 0;

    private static bool IsSupportedScalarAutoProperty(IPropertySymbol property)
    {
        if (property.IsStatic ||
            property.IsIndexer ||
            property.ExplicitInterfaceImplementations.Length > 0 ||
            property.GetMethod is null ||
            property.SetMethod is null ||
            property.GetMethod.IsAbstract ||
            property.GetMethod.IsVirtual ||
            property.GetMethod.IsOverride ||
            property.SetMethod.IsAbstract ||
            property.SetMethod.IsVirtual ||
            property.SetMethod.IsOverride ||
            !IsSupportedScalarType(property.Type) ||
            property.DeclaringSyntaxReferences.Length != 1 ||
            property.DeclaringSyntaxReferences[0].GetSyntax() is not PropertyDeclarationSyntax syntax ||
            syntax.ExpressionBody is not null ||
            syntax.AccessorList is null)
        {
            return false;
        }

        var getter = syntax.AccessorList.Accessors.FirstOrDefault(accessor =>
            accessor.IsKind(SyntaxKind.GetAccessorDeclaration));
        var setter = syntax.AccessorList.Accessors.FirstOrDefault(accessor =>
            accessor.IsKind(SyntaxKind.SetAccessorDeclaration) ||
            accessor.IsKind(SyntaxKind.InitAccessorDeclaration));
        return getter is not null &&
               setter is not null &&
               getter.Body is null &&
               getter.ExpressionBody is null &&
               setter.Body is null &&
               setter.ExpressionBody is null;
    }

    private static bool IsSupportedScalarType(ITypeSymbol type) =>
        type.IsValueType &&
        (type.TypeKind == TypeKind.Enum || type.SpecialType != SpecialType.None);

    private static string BuildMemberDisplay(IPropertySymbol property) =>
        $"{property.ContainingType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)}.{property.Name}";

    private static string BuildMemberIdentity(IPropertySymbol property) =>
        $"{property.ContainingAssembly?.Identity.ToString() ?? "unknown-assembly"}|{BuildMemberDisplay(property)}";

    private static string BuildTypeIdentity(ITypeSymbol type) =>
        $"{type.ContainingAssembly?.Identity.ToString() ?? "unknown-assembly"}|{type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)}";

    private static string BuildReceiverIdentity(ISymbol symbol)
    {
        var containing = symbol.ContainingSymbol?.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat) ?? "unknown-scope";
        var location = symbol.Locations.FirstOrDefault(candidate => candidate.IsInSource);
        return $"{symbol.Kind}|{containing}|{symbol.Name}|{location?.SourceSpan.Start ?? -1}";
    }

    private static string DescribeSymbolLocation(ISymbol symbol, string rootPath)
    {
        var location = symbol.Locations.FirstOrDefault(candidate => candidate.IsInSource);
        if (location is null)
        {
            return "unknown";
        }

        var span = location.GetLineSpan();
        var relativePath = string.IsNullOrWhiteSpace(span.Path)
            ? "unknown"
            : Path.GetRelativePath(rootPath, Path.GetFullPath(span.Path))
                .Replace(Path.DirectorySeparatorChar, '/');
        return $"{relativePath}:L{span.StartLinePosition.Line + 1}";
    }

    private static string DescribeLocation(SyntaxNode node, string relativePath) =>
        $"{relativePath}:L{StartLine(node)}";

    private static SourceLocation GetLocation(SyntaxNode node, string relativePath)
    {
        var span = node.GetLocation().GetLineSpan();
        return new SourceLocation(
            relativePath,
            span.StartLinePosition.Line + 1,
            span.EndLinePosition.Line + 1);
    }

    private static ExpressionSyntax UnwrapParentheses(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
        {
            expression = parenthesized.Expression;
        }

        return expression;
    }

    private static int StartLine(SyntaxNode node) =>
        node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

    private static string RelationKey(EvidenceRelation relation) =>
        $"{relation.FromFactId}|{relation.Kind}|{relation.Target}|{relation.Source.Path}|{relation.Source.StartLine}";

    private static void EnsureMsBuildRegistered()
    {
        lock (RegistrationGate)
        {
            if (!MSBuildLocator.IsRegistered)
            {
                MSBuildLocator.RegisterDefaults();
            }
        }
    }

    private sealed record SelectionDescription(
        string Operation,
        string Source,
        string PredicateExpression);

    private sealed record ExtractionResult(
        IReadOnlyList<EvidenceFact> Facts,
        IReadOnlyList<EvidenceRelation> Relations)
    {
        public static ExtractionResult Empty { get; } = new([], []);
    }
}
