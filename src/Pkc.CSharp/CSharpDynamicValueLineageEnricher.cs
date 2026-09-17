using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Operations;
using Pkc.Core;

namespace Pkc.CSharp;

internal sealed class CSharpDynamicValueLineageEnricher
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
            .Where(fact => fact.Metadata.TryGetValue("analysisMode", out var mode) &&
                           string.Equals(mode, "project-semantic", StringComparison.Ordinal))
            .Where(fact => fact.Metadata.TryGetValue("semanticProject", out var project) &&
                           !string.IsNullOrWhiteSpace(project))
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
                        string.Equals(Path.GetFullPath(candidate.FilePath), fullSourcePath, StringComparison.OrdinalIgnoreCase));
                    if (tree is null)
                    {
                        continue;
                    }

                    var root = await tree.GetRootAsync(cancellationToken);
                    var method = root.DescendantNodes()
                        .OfType<MethodDeclarationSyntax>()
                        .FirstOrDefault(candidate =>
                            candidate.Identifier.ValueText == endpoint.Name &&
                            StartLine(candidate) == endpoint.Source.StartLine);
                    if (method?.Body is null)
                    {
                        continue;
                    }

                    var semanticModel = compilation.GetSemanticModel(tree, ignoreAccessibility: true);
                    foreach (var dependency in ExtractEndpointDependencies(
                                 endpoint,
                                 rootPath,
                                 projectPath,
                                 method,
                                 semanticModel,
                                 compilation,
                                 cancellationToken))
                    {
                        facts[dependency.Id] = dependency;
                        relations.Add(new EvidenceRelation(
                            endpoint.Id,
                            "mutates",
                            dependency.Id,
                            dependency.Source));
                    }
                }
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                // Dynamic lineage is an optional conservative proof. If exact target-project
                // semantics cannot be re-established, keep the existing evidence unchanged.
            }
        }

        return document with
        {
            Facts = facts.Values
                .OrderBy(fact => fact.Id, StringComparer.Ordinal)
                .ToArray(),
            Relations = relations
                .GroupBy(RelationKey, StringComparer.Ordinal)
                .Select(group => group.First())
                .OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
                .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
                .ThenBy(relation => relation.Target, StringComparer.Ordinal)
                .ToArray()
        };
    }

    private static IReadOnlyList<EvidenceFact> ExtractEndpointDependencies(
        EvidenceFact endpoint,
        string rootPath,
        string projectPath,
        MethodDeclarationSyntax method,
        SemanticModel endpointModel,
        Compilation compilation,
        CancellationToken cancellationToken)
    {
        if (HasUnsupportedControlFlow(method.Body!) ||
            method.Body!.DescendantNodes().OfType<InvocationExpressionSyntax>().Any() ||
            method.ParameterList.Parameters
                .Select(parameter => endpointModel.GetDeclaredSymbol(parameter, cancellationToken))
                .OfType<IParameterSymbol>()
                .Any(parameter => parameter.Type.IsReferenceType) ||
            HasReferenceReassignment(method.Body, endpointModel, cancellationToken))
        {
            return [];
        }

        var referenceLocals = method.Body.DescendantNodes()
            .OfType<VariableDeclaratorSyntax>()
            .Select(variable => endpointModel.GetDeclaredSymbol(variable, cancellationToken))
            .OfType<ILocalSymbol>()
            .Where(local => local.Type.IsReferenceType)
            .ToArray();

        var dependencies = new List<EvidenceFact>();
        foreach (var variable in method.Body.Statements
                     .OfType<LocalDeclarationStatementSyntax>()
                     .SelectMany(statement => statement.Declaration.Variables))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (variable.Initializer is null ||
                endpointModel.GetDeclaredSymbol(variable, cancellationToken) is not ILocalSymbol serviceLocal ||
                serviceLocal.Type is not INamedTypeSymbol serviceType ||
                !serviceType.IsReferenceType ||
                !TryGetObjectCreation(
                    variable.Initializer.Value,
                    endpointModel,
                    cancellationToken,
                    out var creation) ||
                creation.Constructor is null ||
                creation.Arguments.Length != 1 ||
                creation.Arguments[0].Value is not ILocalReferenceOperation upstreamReference ||
                !upstreamReference.Local.Type.IsReferenceType)
            {
                continue;
            }

            var upstreamLocal = upstreamReference.Local;
            if (referenceLocals.Length != 2 ||
                !referenceLocals.Any(local => SymbolEqualityComparer.Default.Equals(local, serviceLocal)) ||
                !referenceLocals.Any(local => SymbolEqualityComparer.Default.Equals(local, upstreamLocal)) ||
                !TryResolveConstructorBinding(
                    creation.Constructor,
                    compilation,
                    cancellationToken,
                    out var boundField) ||
                !SymbolEqualityComparer.Default.Equals(boundField.Type, upstreamLocal.Type))
            {
                continue;
            }

            foreach (var dynamicProperty in serviceType.GetMembers().OfType<IPropertySymbol>())
            {
                if (!TryResolveDynamicProperty(
                        dynamicProperty,
                        boundField,
                        compilation,
                        cancellationToken,
                        out var sourceProperty,
                        out var getterSyntax))
                {
                    continue;
                }

                var reads = method.Body.DescendantNodes()
                    .OfType<MemberAccessExpressionSyntax>()
                    .Where(read =>
                        SymbolEqualityComparer.Default.Equals(
                            endpointModel.GetSymbolInfo(read, cancellationToken).Symbol,
                            dynamicProperty) &&
                        SymbolEqualityComparer.Default.Equals(
                            endpointModel.GetSymbolInfo(read.Expression, cancellationToken).Symbol,
                            serviceLocal))
                    .OrderBy(read => read.SpanStart)
                    .ToArray();
                if (reads.Length < 2)
                {
                    continue;
                }

                var firstRead = reads[0];
                var lastRead = reads[^1];
                var mutations = method.Body.DescendantNodes()
                    .OfType<AssignmentExpressionSyntax>()
                    .Where(assignment =>
                        assignment.IsKind(SyntaxKind.SimpleAssignmentExpression) &&
                        assignment.SpanStart > firstRead.SpanStart &&
                        assignment.SpanStart < lastRead.SpanStart &&
                        IsExactPropertyAccess(
                            assignment.Left,
                            upstreamLocal,
                            sourceProperty,
                            endpointModel,
                            cancellationToken))
                    .OrderBy(assignment => assignment.SpanStart)
                    .ToArray();
                if (mutations.Length != 1 ||
                    HasObjectCreationBetween(method.Body, firstRead.SpanStart, lastRead.SpanStart))
                {
                    continue;
                }

                var mutation = mutations[0];
                var sourceOccurrence = $"{upstreamLocal.Name}.{sourceProperty.Name}";
                var targetOccurrence = $"{serviceLocal.Name}.{dynamicProperty.Name}";
                var sourcePath = RelativePath(rootPath, getterSyntax.SyntaxTree.FilePath, endpoint.Source.Path);
                var location = GetLocation(getterSyntax, sourcePath);
                var id = $"cs-lineage:{endpoint.Source.Path}:{firstRead.SpanStart}:dynamic-read";
                var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["knowledgeClass"] = "value-lineage",
                    ["scopeFactId"] = endpoint.Id,
                    ["scopeName"] = endpoint.Name,
                    ["semanticProject"] = projectPath,
                    ["analysisMode"] = "project-semantic",
                    ["analysisConfidence"] = "high",
                    ["proof"] = "readonly-field-constructor-binding-expression-bodied-scalar-getter-read-write-read",
                    ["mechanism"] = "reference",
                    ["temporalSemantics"] = "dynamic",
                    ["compositionStatus"] = "dynamic-reference",
                    ["sourceOccurrence"] = sourceOccurrence,
                    ["targetOccurrence"] = targetOccurrence,
                    ["sourceReceiver"] = upstreamLocal.Name,
                    ["targetReceiver"] = serviceLocal.Name,
                    ["sourceReceiverIdentity"] = BuildReceiverIdentity(upstreamLocal),
                    ["targetReceiverIdentity"] = BuildReceiverIdentity(serviceLocal),
                    ["sourceMember"] = MemberDisplay(sourceProperty),
                    ["targetMember"] = MemberDisplay(dynamicProperty),
                    ["sourceMemberIdentity"] = BuildMemberIdentity(sourceProperty),
                    ["targetMemberIdentity"] = BuildMemberIdentity(dynamicProperty),
                    ["bindingFieldIdentity"] = BuildMemberIdentity(boundField),
                    ["constructorIdentity"] = creation.Constructor.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                    ["getterOccurrence"] = getterSyntax.ExpressionBody!.Expression.ToString(),
                    ["readBeforeLocation"] = $"{endpoint.Source.Path}:L{StartLine(firstRead)}",
                    ["upstreamMutationLocation"] = $"{endpoint.Source.Path}:L{StartLine(mutation)}",
                    ["readAfterLocation"] = $"{endpoint.Source.Path}:L{StartLine(lastRead)}"
                };

                dependencies.Add(new EvidenceFact(
                    id,
                    "value-transfer",
                    $"{sourceOccurrence} -> {targetOccurrence}",
                    endpoint.Container,
                    location,
                    [],
                    metadata));
            }
        }

        return dependencies;
    }

    private static bool TryResolveConstructorBinding(
        IMethodSymbol constructor,
        Compilation compilation,
        CancellationToken cancellationToken,
        out IFieldSymbol boundField)
    {
        boundField = default!;
        if (constructor.MethodKind != MethodKind.Constructor ||
            constructor.IsStatic ||
            constructor.Parameters.Length != 1 ||
            constructor.DeclaringSyntaxReferences.Length != 1 ||
            constructor.ContainingType.BaseType is not { SpecialType: SpecialType.System_Object } ||
            constructor.ContainingType.DeclaringSyntaxReferences.Length != 1 ||
            constructor.DeclaringSyntaxReferences[0].GetSyntax(cancellationToken) is not ConstructorDeclarationSyntax syntax ||
            syntax.Initializer is not null ||
            syntax.ExpressionBody is not null ||
            syntax.Body is null ||
            syntax.Body.Statements.Count != 1 ||
            syntax.Body.Statements[0] is not ExpressionStatementSyntax expressionStatement ||
            expressionStatement.Expression is not AssignmentExpressionSyntax assignment ||
            !assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
        {
            return false;
        }

        var model = compilation.GetSemanticModel(syntax.SyntaxTree, ignoreAccessibility: true);
        if (model.GetSymbolInfo(assignment.Left, cancellationToken).Symbol is not IFieldSymbol field ||
            model.GetSymbolInfo(assignment.Right, cancellationToken).Symbol is not IParameterSymbol parameter ||
            !SymbolEqualityComparer.Default.Equals(parameter, constructor.Parameters[0]) ||
            field.IsStatic ||
            !field.IsReadOnly ||
            field.DeclaredAccessibility != Accessibility.Private ||
            !field.Type.IsReferenceType ||
            !SymbolEqualityComparer.Default.Equals(field.Type, parameter.Type) ||
            field.DeclaringSyntaxReferences.Length != 1 ||
            field.DeclaringSyntaxReferences[0].GetSyntax(cancellationToken) is not VariableDeclaratorSyntax fieldSyntax ||
            fieldSyntax.Initializer is not null)
        {
            return false;
        }

        if (constructor.ContainingType.DeclaringSyntaxReferences[0].GetSyntax(cancellationToken) is not ClassDeclarationSyntax typeSyntax ||
            typeSyntax.Members.OfType<FieldDeclarationSyntax>()
                .SelectMany(declaration => declaration.Declaration.Variables)
                .Any(variable => variable.Initializer is not null) ||
            typeSyntax.Members.OfType<PropertyDeclarationSyntax>()
                .Any(property => property.Initializer is not null) ||
            constructor.ContainingType.InstanceConstructors.Count(method => !method.IsImplicitlyDeclared) != 1 ||
            constructor.ContainingType.GetMembers()
                .OfType<IFieldSymbol>()
                .Where(candidate => !candidate.IsStatic && !candidate.IsImplicitlyDeclared)
                .Any(candidate => !SymbolEqualityComparer.Default.Equals(candidate, field)))
        {
            return false;
        }

        boundField = field;
        return true;
    }

    private static bool TryResolveDynamicProperty(
        IPropertySymbol property,
        IFieldSymbol boundField,
        Compilation compilation,
        CancellationToken cancellationToken,
        out IPropertySymbol sourceProperty,
        out PropertyDeclarationSyntax syntax)
    {
        sourceProperty = default!;
        syntax = default!;
        if (property.IsStatic ||
            property.IsIndexer ||
            property.ExplicitInterfaceImplementations.Length > 0 ||
            property.GetMethod is null ||
            property.SetMethod is not null ||
            property.GetMethod.IsAbstract ||
            property.GetMethod.IsVirtual ||
            property.GetMethod.IsOverride ||
            !IsSupportedScalarType(property.Type) ||
            property.DeclaringSyntaxReferences.Length != 1 ||
            property.DeclaringSyntaxReferences[0].GetSyntax(cancellationToken) is not PropertyDeclarationSyntax propertySyntax ||
            propertySyntax.AccessorList is not null ||
            propertySyntax.ExpressionBody?.Expression is not MemberAccessExpressionSyntax sourceAccess)
        {
            return false;
        }

        var model = compilation.GetSemanticModel(propertySyntax.SyntaxTree, ignoreAccessibility: true);
        if (!SymbolEqualityComparer.Default.Equals(
                model.GetSymbolInfo(sourceAccess.Expression, cancellationToken).Symbol,
                boundField) ||
            model.GetSymbolInfo(sourceAccess, cancellationToken).Symbol is not IPropertySymbol upstreamProperty ||
            !SymbolEqualityComparer.Default.Equals(upstreamProperty.ContainingType, boundField.Type) ||
            !SymbolEqualityComparer.Default.Equals(upstreamProperty.Type, property.Type) ||
            !IsSupportedStoredScalarAutoProperty(upstreamProperty, cancellationToken))
        {
            return false;
        }

        sourceProperty = upstreamProperty;
        syntax = propertySyntax;
        return true;
    }

    private static bool IsSupportedStoredScalarAutoProperty(
        IPropertySymbol property,
        CancellationToken cancellationToken)
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
            property.DeclaringSyntaxReferences[0].GetSyntax(cancellationToken) is not PropertyDeclarationSyntax syntax ||
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

    private static bool HasReferenceReassignment(
        BlockSyntax body,
        SemanticModel model,
        CancellationToken cancellationToken) =>
        body.DescendantNodes()
            .OfType<AssignmentExpressionSyntax>()
            .Any(assignment =>
            {
                var symbol = model.GetSymbolInfo(assignment.Left, cancellationToken).Symbol;
                return symbol is ILocalSymbol or IParameterSymbol &&
                       model.GetTypeInfo(assignment.Left, cancellationToken).Type is { IsReferenceType: true };
            });

    private static bool TryGetObjectCreation(
        ExpressionSyntax expression,
        SemanticModel model,
        CancellationToken cancellationToken,
        out IObjectCreationOperation creation)
    {
        IOperation? operation = model.GetOperation(expression, cancellationToken);
        while (operation is IParenthesizedOperation or IConversionOperation)
        {
            if (operation is IConversionOperation userConversion && userConversion.OperatorMethod is not null)
            {
                creation = default!;
                return false;
            }

            operation = operation switch
            {
                IParenthesizedOperation parenthesized => parenthesized.Operand,
                IConversionOperation conversionOperation => conversionOperation.Operand,
                _ => operation
            };
        }

        if (operation is IObjectCreationOperation objectCreation)
        {
            creation = objectCreation;
            return true;
        }

        creation = default!;
        return false;
    }

    private static bool IsExactPropertyAccess(
        ExpressionSyntax expression,
        ILocalSymbol receiver,
        IPropertySymbol property,
        SemanticModel model,
        CancellationToken cancellationToken)
    {
        expression = UnwrapParentheses(expression);
        return expression is MemberAccessExpressionSyntax memberAccess &&
               SymbolEqualityComparer.Default.Equals(
                   model.GetSymbolInfo(memberAccess.Expression, cancellationToken).Symbol,
                   receiver) &&
               SymbolEqualityComparer.Default.Equals(
                   model.GetSymbolInfo(memberAccess, cancellationToken).Symbol,
                   property);
    }

    private static bool HasObjectCreationBetween(BlockSyntax body, int start, int end) =>
        body.DescendantNodes()
            .OfType<ObjectCreationExpressionSyntax>()
            .Any(creation => creation.SpanStart > start && creation.SpanStart < end);

    private static bool HasUnsupportedControlFlow(BlockSyntax body)
    {
        var unsupported = body.DescendantNodes().Any(node => node is
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
            FixedStatementSyntax);
        if (unsupported)
        {
            return true;
        }

        var returns = body.DescendantNodes().OfType<ReturnStatementSyntax>().ToArray();
        return returns.Any(statement => statement.Parent != body) ||
               returns.Length > 1 ||
               (returns.Length == 1 && !ReferenceEquals(body.Statements.LastOrDefault(), returns[0]));
    }

    private static bool IsSupportedScalarType(ITypeSymbol type) =>
        type.IsValueType &&
        (type.TypeKind == TypeKind.Enum || type.SpecialType != SpecialType.None);

    private static ExpressionSyntax UnwrapParentheses(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
        {
            expression = parenthesized.Expression;
        }

        return expression;
    }

    private static string BuildReceiverIdentity(ISymbol symbol)
    {
        var containing = symbol.ContainingSymbol?.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat) ?? "unknown-scope";
        var location = symbol.Locations.FirstOrDefault(candidate => candidate.IsInSource);
        var sourceStart = location?.SourceSpan.Start ?? -1;
        return $"{symbol.Kind}|{containing}|{symbol.Name}|{sourceStart}";
    }

    private static string MemberDisplay(ISymbol symbol) =>
        $"{symbol.ContainingType?.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat) ?? "unknown-type"}.{symbol.Name}";

    private static string BuildMemberIdentity(ISymbol symbol)
    {
        var assemblyIdentity = symbol.ContainingAssembly?.Identity.ToString() ?? "unknown-assembly";
        return $"{assemblyIdentity}|{MemberDisplay(symbol)}";
    }

    private static string RelativePath(string rootPath, string? filePath, string fallback)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return fallback;
        }

        var fullPath = Path.GetFullPath(filePath);
        var relative = Path.GetRelativePath(rootPath, fullPath);
        if (relative.StartsWith("..", StringComparison.Ordinal))
        {
            return fallback;
        }

        return relative.Replace('\\', '/');
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
}
