using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Operations;
using Pkc.Core;

namespace Pkc.CSharp;

internal sealed class CSharpApiProjectionLineageEnricher
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
                var project = await workspace.OpenProjectAsync(fullProjectPath, cancellationToken: cancellationToken);
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
                // C proof is optional and conservative. If exact target-project semantics
                // cannot be re-established, retain existing lower-authority evidence only.
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
        if (HasUnsupportedControlFlow(body) ||
            HasUnsupportedStatementShape(body) ||
            HasReferenceAmbiguity(method, model, cancellationToken) ||
            HasOpaqueEffect(body, model, cancellationToken))
        {
            return ExtractionResult.Empty;
        }

        if (body.Statements.LastOrDefault() is not ReturnStatementSyntax returnStatement ||
            returnStatement.Expression is null ||
            UnwrapParentheses(returnStatement.Expression) is not ObjectCreationExpressionSyntax responseCreation ||
            model.GetOperation(responseCreation, cancellationToken) is not IObjectCreationOperation responseOperation ||
            responseOperation.Type is not INamedTypeSymbol responseType ||
            !IsImplicitParameterlessConstruction(responseOperation) ||
            responseCreation.Initializer is null ||
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
                !IsSupportedScalarAutoProperty(targetProperty))
            {
                return ExtractionResult.Empty;
            }

            if (model.GetConversion(assignment.Right, cancellationToken).IsUserDefined)
            {
                return ExtractionResult.Empty;
            }

            if (model.GetConstantValue(assignment.Right, cancellationToken).HasValue)
            {
                continue;
            }

            if (!TryResolveSource(
                    assignment.Right,
                    model,
                    cancellationToken,
                    out var sourceProperty,
                    out var sourceLocal,
                    out var sourceOccurrence) ||
                !IsSupportedScalarAutoProperty(sourceProperty) ||
                !SymbolEqualityComparer.Default.Equals(sourceProperty.Type, targetProperty.Type) ||
                !IsSafeFreshReferenceLocal(sourceLocal, model, cancellationToken))
            {
                return ExtractionResult.Empty;
            }

            var projectionLocation = GetLocation(assignment, endpoint.Source.Path);
            var responseLocation = GetLocation(returnStatement, endpoint.Source.Path);
            var projectionLocationText = DescribeLocation(assignment, endpoint.Source.Path);
            var responseLocationText = DescribeLocation(returnStatement, endpoint.Source.Path);
            var sourceMember = BuildMemberDisplay(sourceProperty);
            var targetMember = BuildMemberDisplay(targetProperty);
            var sourceMemberIdentity = BuildMemberIdentity(sourceProperty);
            var targetMemberIdentity = BuildMemberIdentity(targetProperty);
            var responseTypeIdentity = BuildTypeIdentity(responseType);
            var projectionId = $"cs-lineage:{endpoint.Source.Path}:{assignment.SpanStart}:api-projection";

            var projection = new EvidenceFact(
                projectionId,
                "value-transfer",
                $"{sourceOccurrence} -> {targetMember}",
                endpoint.Container,
                projectionLocation,
                [],
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["knowledgeClass"] = "value-lineage",
                    ["scopeFactId"] = endpoint.Id,
                    ["scopeName"] = endpoint.Name,
                    ["semanticProject"] = projectPath,
                    ["analysisMode"] = "project-semantic",
                    ["analysisConfidence"] = "high",
                    ["proof"] = "direct-return-project-semantic-object-initializer-projection",
                    ["mechanism"] = "api-projection",
                    ["temporalSemantics"] = "snapshot",
                    ["compositionStatus"] = "direct-api-response-projection",
                    ["sourceOccurrence"] = sourceOccurrence,
                    ["targetOccurrence"] = targetMember,
                    ["sourceReceiver"] = sourceLocal.Name,
                    ["sourceReceiverIdentity"] = BuildReceiverIdentity(sourceLocal),
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
                    ["projectionLocation"] = projectionLocationText,
                    ["responseLocation"] = responseLocationText,
                    ["responseBoundary"] = "direct-return-object-initializer",
                    ["endpointFactId"] = endpoint.Id
                });
            facts.Add(projection);
            relations.Add(new EvidenceRelation(
                endpoint.Id,
                "returns-response",
                projection.Id,
                projectionLocation));

            var terminal = new EvidenceFact(
                $"cs-lineage:{endpoint.Source.Path}:{assignment.SpanStart}:api-response-terminal",
                "value-terminal-source",
                $"last source before API response field: {targetMember}",
                endpoint.Container,
                responseLocation,
                [],
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["knowledgeClass"] = "value-lineage",
                    ["scopeFactId"] = endpoint.Id,
                    ["scopeName"] = endpoint.Name,
                    ["semanticProject"] = projectPath,
                    ["analysisMode"] = "project-semantic",
                    ["analysisConfidence"] = "high",
                    ["proof"] = "direct-return-project-semantic-object-initializer-response-field",
                    ["boundary"] = "API response field",
                    ["returnedOccurrence"] = targetMember,
                    ["returnedMemberIdentity"] = targetMemberIdentity,
                    ["sourceFactId"] = projection.Id,
                    ["sourceMechanism"] = "api-projection",
                    ["sourceFactLocation"] = projectionLocationText,
                    ["sourceOccurrence"] = sourceOccurrence,
                    ["responseTypeIdentity"] = responseTypeIdentity,
                    ["responseLocation"] = responseLocationText
                });
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

    private static bool TryResolveSource(
        ExpressionSyntax expression,
        SemanticModel model,
        CancellationToken cancellationToken,
        out IPropertySymbol property,
        out ILocalSymbol receiver,
        out string occurrence)
    {
        var operation = UnwrapSafeConversions(model.GetOperation(expression, cancellationToken));
        if (operation is not IPropertyReferenceOperation propertyReference ||
            propertyReference.Instance is null ||
            UnwrapSafeConversions(propertyReference.Instance) is not ILocalReferenceOperation localReference)
        {
            property = default!;
            receiver = default!;
            occurrence = string.Empty;
            return false;
        }

        property = propertyReference.Property;
        receiver = localReference.Local;
        occurrence = UnwrapParentheses(expression).ToString();
        return true;
    }

    private static IOperation? UnwrapSafeConversions(IOperation? operation)
    {
        while (operation is IParenthesizedOperation or IConversionOperation)
        {
            if (operation is IConversionOperation currentConversion &&
                currentConversion.OperatorMethod is not null)
            {
                return null;
            }

            operation = operation switch
            {
                IParenthesizedOperation parenthesized => parenthesized.Operand,
                IConversionOperation safeConversion => safeConversion.Operand,
                _ => operation
            };
        }

        return operation;
    }

    private static bool HasReferenceAmbiguity(
        MethodDeclarationSyntax method,
        SemanticModel model,
        CancellationToken cancellationToken)
    {
        var body = method.Body!;
        if (method.ParameterList.Parameters
                .Select(parameter => model.GetDeclaredSymbol(parameter, cancellationToken))
                .OfType<IParameterSymbol>()
                .Any(parameter => parameter.Type.IsReferenceType) ||
            body.DescendantNodes().OfType<DeclarationExpressionSyntax>().Any() ||
            body.DescendantNodes().OfType<AssignmentExpressionSyntax>()
                .Any(assignment => UnwrapParentheses(assignment.Left) is TupleExpressionSyntax))
        {
            return true;
        }

        foreach (var variable in body.DescendantNodes().OfType<VariableDeclaratorSyntax>())
        {
            if (model.GetDeclaredSymbol(variable, cancellationToken) is ILocalSymbol { Type.IsReferenceType: true } &&
                (variable.Initializer is null ||
                 !IsSafeFreshReferenceAllocation(variable.Initializer.Value, model, cancellationToken)))
            {
                return true;
            }
        }

        foreach (var assignment in body.DescendantNodes().OfType<AssignmentExpressionSyntax>())
        {
            var symbol = model.GetSymbolInfo(UnwrapParentheses(assignment.Left), cancellationToken).Symbol;
            if ((symbol is ILocalSymbol local && local.Type.IsReferenceType) ||
                (symbol is IParameterSymbol parameter && parameter.Type.IsReferenceType))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsSafeFreshReferenceLocal(
        ILocalSymbol local,
        SemanticModel model,
        CancellationToken cancellationToken)
    {
        if (local.DeclaringSyntaxReferences.Length != 1 ||
            local.DeclaringSyntaxReferences[0].GetSyntax(cancellationToken) is not VariableDeclaratorSyntax variable ||
            variable.Initializer is null)
        {
            return false;
        }

        return IsSafeFreshReferenceAllocation(variable.Initializer.Value, model, cancellationToken);
    }

    private static bool IsSafeFreshReferenceAllocation(
        ExpressionSyntax expression,
        SemanticModel model,
        CancellationToken cancellationToken)
    {
        if (model.GetConversion(expression, cancellationToken).IsUserDefined)
        {
            return false;
        }

        return UnwrapSafeConversions(model.GetOperation(expression, cancellationToken)) is
            IObjectCreationOperation objectCreation &&
            IsImplicitParameterlessConstruction(objectCreation);
    }

    private static bool IsImplicitParameterlessConstruction(IObjectCreationOperation creation) =>
        creation.Constructor is { IsImplicitlyDeclared: true } constructor &&
        constructor.Parameters.Length == 0;

    private static bool HasOpaqueEffect(
        BlockSyntax body,
        SemanticModel model,
        CancellationToken cancellationToken)
    {
        if (body.DescendantNodes().OfType<InvocationExpressionSyntax>().Any())
        {
            return true;
        }

        foreach (var creation in body.DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
        {
            if (model.GetOperation(creation, cancellationToken) is not IObjectCreationOperation operation ||
                !IsImplicitParameterlessConstruction(operation))
            {
                return true;
            }
        }

        foreach (var variable in body.DescendantNodes().OfType<VariableDeclaratorSyntax>())
        {
            if (variable.Initializer is not null &&
                model.GetConversion(variable.Initializer.Value, cancellationToken).IsUserDefined)
            {
                return true;
            }
        }

        foreach (var assignment in body.DescendantNodes().OfType<AssignmentExpressionSyntax>())
        {
            if (model.GetConversion(assignment.Right, cancellationToken).IsUserDefined)
            {
                return true;
            }
        }

        foreach (var memberAccess in body.DescendantNodes().OfType<MemberAccessExpressionSyntax>())
        {
            if (model.GetSymbolInfo(memberAccess, cancellationToken).Symbol is IPropertySymbol property &&
                !IsSupportedScalarAutoProperty(property))
            {
                return true;
            }
        }

        foreach (var binary in body.DescendantNodes().OfType<BinaryExpressionSyntax>())
        {
            if (model.GetSymbolInfo(binary, cancellationToken).Symbol is IMethodSymbol method &&
                method.MethodKind == MethodKind.UserDefinedOperator)
            {
                return true;
            }
        }

        foreach (var unary in body.DescendantNodes().OfType<PrefixUnaryExpressionSyntax>())
        {
            if (model.GetSymbolInfo(unary, cancellationToken).Symbol is IMethodSymbol method &&
                method.MethodKind == MethodKind.UserDefinedOperator)
            {
                return true;
            }
        }

        foreach (var unary in body.DescendantNodes().OfType<PostfixUnaryExpressionSyntax>())
        {
            if (model.GetSymbolInfo(unary, cancellationToken).Symbol is IMethodSymbol method &&
                method.MethodKind == MethodKind.UserDefinedOperator)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasUnsupportedStatementShape(BlockSyntax body)
    {
        foreach (var statement in body.Statements)
        {
            switch (statement)
            {
                case LocalDeclarationStatementSyntax:
                case ReturnStatementSyntax:
                case EmptyStatementSyntax:
                    continue;
                case ExpressionStatementSyntax
                    {
                        Expression: AssignmentExpressionSyntax assignment
                    } when assignment.IsKind(SyntaxKind.SimpleAssignmentExpression):
                    continue;
                default:
                    return true;
            }
        }

        return false;
    }

    private static bool HasUnsupportedControlFlow(BlockSyntax body)
    {
        if (body.DescendantNodes().Any(node => node is
            IfStatementSyntax or
            SwitchStatementSyntax or
            SwitchExpressionSyntax or
            ForStatementSyntax or
            ForEachStatementSyntax or
            ForEachVariableStatementSyntax or
            WhileStatementSyntax or
            DoStatementSyntax or
            TryStatementSyntax or
            ConditionalExpressionSyntax or
            AnonymousFunctionExpressionSyntax or
            LocalFunctionStatementSyntax or
            UsingStatementSyntax or
            LockStatementSyntax or
            FixedStatementSyntax or
            GotoStatementSyntax or
            LabeledStatementSyntax or
            ThrowStatementSyntax))
        {
            return true;
        }

        var returns = body.DescendantNodes().OfType<ReturnStatementSyntax>().ToArray();
        return returns.Any(statement => statement.Parent != body) ||
               returns.Length != 1 ||
               !ReferenceEquals(body.Statements.LastOrDefault(), returns[0]);
    }

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

    private static SourceLocation GetLocation(SyntaxNode node, string relativePath)
    {
        var span = node.GetLocation().GetLineSpan();
        return new SourceLocation(
            relativePath,
            span.StartLinePosition.Line + 1,
            span.EndLinePosition.Line + 1);
    }

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

    private sealed record ExtractionResult(
        IReadOnlyList<EvidenceFact> Facts,
        IReadOnlyList<EvidenceRelation> Relations)
    {
        public static ExtractionResult Empty { get; } = new([], []);
    }
}
