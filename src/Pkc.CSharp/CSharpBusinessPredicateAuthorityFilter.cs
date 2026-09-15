using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Pkc.Core;

namespace Pkc.CSharp;

internal sealed class CSharpBusinessPredicateAuthorityFilter
{
    private static readonly object RegistrationGate = new();

    private static readonly HashSet<string> CallbackFreeZeroArgumentWherePipelineTargets = new(StringComparer.Ordinal)
    {
        "System.Linq.Enumerable.Reverse",
        "System.Linq.Queryable.Reverse",
        "System.Linq.Enumerable.AsEnumerable",
        "System.Linq.Queryable.AsQueryable",
        "System.Linq.Enumerable.ToArray",
        "System.Linq.Enumerable.ToList"
    };

    private static readonly HashSet<string> CallbackFreeSliceWherePipelineTargets = new(StringComparer.Ordinal)
    {
        "System.Linq.Enumerable.Skip",
        "System.Linq.Queryable.Skip",
        "System.Linq.Enumerable.Take",
        "System.Linq.Queryable.Take"
    };

    private static readonly HashSet<string> ProjectionTargets = new(StringComparer.Ordinal)
    {
        "System.Linq.Enumerable.Select",
        "System.Linq.Queryable.Select"
    };

    public async Task<FactDocument> FilterAsync(
        string repositoryPath,
        FactDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryPath);
        ArgumentNullException.ThrowIfNull(document);

        var predicates = document.Facts
            .Where(fact => fact.Kind == "business-predicate")
            .ToArray();
        if (predicates.Length == 0)
        {
            return document;
        }

        var root = Path.GetFullPath(repositoryPath);
        var authorities = await ResolveInvocationAuthoritiesAsync(root, predicates, cancellationToken);
        var rejected = new HashSet<string>(StringComparer.Ordinal);
        var observedOnly = new HashSet<string>(StringComparer.Ordinal);
        var facts = document.Facts.ToDictionary(fact => fact.Id, StringComparer.Ordinal);

        foreach (var predicate in predicates)
        {
            if (!TryGetInvocationSpanStart(predicate, out var spanStart) ||
                !predicate.Metadata.TryGetValue("operation", out var operation) ||
                !authorities.TryGetValue(new InvocationKey(predicate.Source.Path, spanStart), out var authority) ||
                !IsSupportedTarget(authority.Target, operation))
            {
                rejected.Add(predicate.Id);
                continue;
            }

            var metadata = new Dictionary<string, string>(predicate.Metadata, StringComparer.Ordinal);
            if (authority.ObservableContext is null)
            {
                metadata["businessRuleAuthority"] = "observed-only";
                metadata["observableEffectResolution"] = "not-proven";
                observedOnly.Add(predicate.Id);
            }
            else
            {
                metadata["businessRuleAuthority"] = "observable";
                metadata["observableContext"] = authority.ObservableContext;
                metadata["observableEffectResolution"] = "semantic-return-value-path";
            }

            facts[predicate.Id] = predicate with { Metadata = metadata };
        }

        var relations = document.Relations
            .Where(relation => !rejected.Contains(relation.Target))
            .Select(relation =>
                observedOnly.Contains(relation.Target) && relation.Kind == "contains-condition"
                    ? relation with { Kind = "observes-predicate" }
                    : relation)
            .ToArray();

        return document with
        {
            Facts = facts.Values
                .Where(fact => !rejected.Contains(fact.Id))
                .OrderBy(fact => fact.Id, StringComparer.Ordinal)
                .ToArray(),
            Relations = relations
                .OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
                .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
                .ThenBy(relation => relation.Target, StringComparer.Ordinal)
                .ToArray()
        };
    }

    private static async Task<IReadOnlyDictionary<InvocationKey, InvocationAuthority>> ResolveInvocationAuthoritiesAsync(
        string rootPath,
        IReadOnlyList<EvidenceFact> predicates,
        CancellationToken cancellationToken)
    {
        var requested = predicates
            .Select(predicate => TryGetInvocationSpanStart(predicate, out var spanStart)
                ? new InvocationKey(predicate.Source.Path, spanStart)
                : default)
            .Where(key => key.Path is not null)
            .Distinct()
            .ToArray();

        if (requested.Length == 0)
        {
            return new Dictionary<InvocationKey, InvocationAuthority>();
        }

        var requestedByPath = requested
            .GroupBy(key => key.Path!, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
        var candidates = requested.ToDictionary(
            key => key,
            _ => new HashSet<InvocationAuthority>());

        var projectFiles = Directory.EnumerateFiles(rootPath, "*.csproj", SearchOption.AllDirectories)
            .Where(path => !CSharpSourceScope.IsExcluded(rootPath, path))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (projectFiles.Length == 0)
        {
            return new Dictionary<InvocationKey, InvocationAuthority>();
        }

        EnsureMsBuildRegistered();

        foreach (var projectFile in projectFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var workspace = MSBuildWorkspace.Create();
                var project = await workspace.OpenProjectAsync(projectFile, cancellationToken: cancellationToken);
                var compilation = await project.GetCompilationAsync(cancellationToken);
                if (compilation is null)
                {
                    continue;
                }

                foreach (var tree in compilation.SyntaxTrees)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (string.IsNullOrWhiteSpace(tree.FilePath))
                    {
                        continue;
                    }

                    var fullPath = Path.GetFullPath(tree.FilePath);
                    if (!File.Exists(fullPath) || !IsUnderRoot(rootPath, fullPath))
                    {
                        continue;
                    }

                    var relativePath = NormalizePath(Path.GetRelativePath(rootPath, fullPath));
                    if (!requestedByPath.TryGetValue(relativePath, out var keys))
                    {
                        continue;
                    }

                    var syntaxRoot = await tree.GetRootAsync(cancellationToken);
                    var semanticModel = compilation.GetSemanticModel(tree, ignoreAccessibility: true);

                    foreach (var key in keys)
                    {
                        var invocation = FindInvocationAtSpan(syntaxRoot, key.SpanStart);
                        if (invocation is null)
                        {
                            continue;
                        }

                        var symbolInfo = semanticModel.GetSymbolInfo(invocation, cancellationToken);
                        var methodSymbol = symbolInfo.Symbol as IMethodSymbol ??
                                           symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().SingleOrDefault();
                        if (methodSymbol is null)
                        {
                            continue;
                        }

                        candidates[key].Add(new InvocationAuthority(
                            GetMethodTarget(methodSymbol),
                            GetObservableContext(invocation, methodSymbol, semanticModel, cancellationToken)));
                    }
                }
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                // Authority is conservative. A project that cannot be loaded contributes no proof.
            }
        }

        return candidates
            .Where(pair => pair.Value.Count == 1)
            .ToDictionary(pair => pair.Key, pair => pair.Value.Single());
    }

    private static string? GetObservableContext(
        InvocationExpressionSyntax invocation,
        IMethodSymbol methodSymbol,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        var containingMethod = invocation.Ancestors().OfType<BaseMethodDeclarationSyntax>().FirstOrDefault();
        if (containingMethod is null)
        {
            return null;
        }

        var nestedFunctionBoundary = invocation.Ancestors()
            .TakeWhile(node => node != containingMethod)
            .Any(node => node is AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax);
        if (nestedFunctionBoundary)
        {
            return null;
        }

        if (TryGetObservableExpression(invocation, containingMethod, out var expression, out var context) &&
            HasProvenObservableValuePath(invocation, expression, methodSymbol.Name, semanticModel, cancellationToken))
        {
            return context;
        }

        return null;
    }

    private static bool TryGetObservableExpression(
        InvocationExpressionSyntax invocation,
        BaseMethodDeclarationSyntax containingMethod,
        out ExpressionSyntax expression,
        out string context)
    {
        var returnStatement = invocation.Ancestors().OfType<ReturnStatementSyntax>().FirstOrDefault();
        if (returnStatement?.Expression is not null && ContainsNode(returnStatement.Expression, invocation))
        {
            expression = returnStatement.Expression;
            context = "return";
            return true;
        }

        var arrow = invocation.Ancestors().OfType<ArrowExpressionClauseSyntax>().FirstOrDefault();
        if (arrow?.Parent == containingMethod && ContainsNode(arrow.Expression, invocation))
        {
            expression = arrow.Expression;
            context = "return";
            return true;
        }

        var yieldReturn = invocation.Ancestors().OfType<YieldStatementSyntax>()
            .FirstOrDefault(statement => statement.ReturnOrBreakKeyword.ValueText == "return");
        if (yieldReturn?.Expression is not null && ContainsNode(yieldReturn.Expression, invocation))
        {
            expression = yieldReturn.Expression;
            context = "yield-return";
            return true;
        }

        expression = null!;
        context = string.Empty;
        return false;
    }

    private static bool HasProvenObservableValuePath(
        InvocationExpressionSyntax invocation,
        ExpressionSyntax observableExpression,
        string operation,
        SemanticModel semanticModel,
        CancellationToken cancellationToken) =>
        operation switch
        {
            "Where" => IsWhereEffectPreserved(
                invocation,
                observableExpression,
                semanticModel,
                cancellationToken),
            "Any" or "All" or "First" or "FirstOrDefault" or "Single" or "SingleOrDefault" =>
                IsDirectValuePath(invocation, observableExpression),
            _ => false
        };

    private static bool IsDirectValuePath(
        InvocationExpressionSyntax invocation,
        ExpressionSyntax observableExpression)
    {
        SyntaxNode current = invocation;
        while (current != observableExpression)
        {
            if (current.Parent is ParenthesizedExpressionSyntax parenthesized &&
                parenthesized.Expression == current)
            {
                current = parenthesized;
                continue;
            }

            return false;
        }

        return true;
    }

    private static bool IsWhereEffectPreserved(
        InvocationExpressionSyntax invocation,
        ExpressionSyntax observableExpression,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        SyntaxNode current = invocation;
        while (current != observableExpression)
        {
            if (current.Parent is ParenthesizedExpressionSyntax parenthesized &&
                parenthesized.Expression == current)
            {
                current = parenthesized;
                continue;
            }

            if (current.Parent is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Expression == current &&
                memberAccess.Parent is InvocationExpressionSyntax pipelineInvocation &&
                pipelineInvocation.Expression == memberAccess &&
                IsAllowedWherePipelineInvocation(invocation, pipelineInvocation, semanticModel, cancellationToken))
            {
                current = pipelineInvocation;
                continue;
            }

            return false;
        }

        return true;
    }

    private static bool IsAllowedWherePipelineInvocation(
        InvocationExpressionSyntax whereInvocation,
        InvocationExpressionSyntax pipelineInvocation,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(pipelineInvocation, cancellationToken);
        var methodSymbol = symbolInfo.Symbol as IMethodSymbol ??
                           symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().SingleOrDefault();
        if (methodSymbol is null)
        {
            return false;
        }

        var target = GetMethodTarget(methodSymbol);
        if (IsCallbackFreeWherePipelineInvocation(target, methodSymbol, pipelineInvocation))
        {
            return true;
        }

        return ProjectionTargets.Contains(target) &&
               IsItemSemanticsPreservingProjection(
                   whereInvocation,
                   pipelineInvocation,
                   semanticModel,
                   cancellationToken);
    }

    private static bool IsCallbackFreeWherePipelineInvocation(
        string target,
        IMethodSymbol methodSymbol,
        InvocationExpressionSyntax invocation)
    {
        var parameters = methodSymbol.ReducedFrom is { } reducedFrom
            ? reducedFrom.Parameters.Skip(1).ToArray()
            : methodSymbol.Parameters.ToArray();
        var argumentCount = invocation.ArgumentList.Arguments.Count;

        if (CallbackFreeZeroArgumentWherePipelineTargets.Contains(target))
        {
            return argumentCount == 0 && parameters.Length == 0;
        }

        if (!CallbackFreeSliceWherePipelineTargets.Contains(target) ||
            argumentCount != 1 ||
            parameters.Length != 1)
        {
            return false;
        }

        var parameterType = parameters[0].Type;
        return parameterType.SpecialType == SpecialType.System_Int32 ||
               parameterType is INamedTypeSymbol { Name: "Range" } rangeType &&
               string.Equals(rangeType.ContainingNamespace?.ToDisplayString(), "System", StringComparison.Ordinal);
    }

    private static bool IsItemSemanticsPreservingProjection(
        InvocationExpressionSyntax whereInvocation,
        InvocationExpressionSyntax selectInvocation,
        SemanticModel semanticModel,
        CancellationToken cancellationToken) =>
        IsIdentityProjection(selectInvocation, semanticModel, cancellationToken) ||
        IsPredicatePreservingSameTypeMethodGroupProjection(
            whereInvocation,
            selectInvocation,
            semanticModel,
            cancellationToken);

    private static bool IsIdentityProjection(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        if (invocation.ArgumentList.Arguments.Count != 1)
        {
            return false;
        }

        return invocation.ArgumentList.Arguments[0].Expression switch
        {
            SimpleLambdaExpressionSyntax simpleLambda => IsIdentityLambda(
                simpleLambda.Parameter,
                simpleLambda.Body,
                semanticModel,
                cancellationToken),
            ParenthesizedLambdaExpressionSyntax parenthesizedLambda
                when parenthesizedLambda.ParameterList.Parameters.Count > 0 => IsIdentityLambda(
                    parenthesizedLambda.ParameterList.Parameters[0],
                    parenthesizedLambda.Body,
                    semanticModel,
                    cancellationToken),
            _ => false
        };
    }

    private static bool IsIdentityLambda(
        ParameterSyntax sourceParameter,
        SyntaxNode body,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        if (body is not ExpressionSyntax expression)
        {
            return false;
        }

        while (expression is ParenthesizedExpressionSyntax parenthesized)
        {
            expression = parenthesized.Expression;
        }

        if (expression is not IdentifierNameSyntax identifier)
        {
            return false;
        }

        var parameterSymbol = semanticModel.GetDeclaredSymbol(sourceParameter, cancellationToken);
        var returnedSymbol = semanticModel.GetSymbolInfo(identifier, cancellationToken).Symbol;
        return parameterSymbol is not null &&
               returnedSymbol is not null &&
               SymbolEqualityComparer.Default.Equals(parameterSymbol, returnedSymbol);
    }

    private static bool IsPredicatePreservingSameTypeMethodGroupProjection(
        InvocationExpressionSyntax whereInvocation,
        InvocationExpressionSyntax selectInvocation,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        if (selectInvocation.ArgumentList.Arguments.Count != 1)
        {
            return false;
        }

        var selector = selectInvocation.ArgumentList.Arguments[0].Expression;
        if (selector is LambdaExpressionSyntax)
        {
            return false;
        }

        var selectorInfo = semanticModel.GetSymbolInfo(selector, cancellationToken);
        var projector = selectorInfo.Symbol as IMethodSymbol ??
                        selectorInfo.CandidateSymbols.OfType<IMethodSymbol>().SingleOrDefault();
        if (projector is null ||
            projector.Parameters.Length != 1 ||
            !SymbolEqualityComparer.Default.Equals(projector.Parameters[0].Type, projector.ReturnType) ||
            !IsClosedItemType(projector.ReturnType) ||
            projector.DeclaringSyntaxReferences.Length != 1)
        {
            return false;
        }

        if (!TryGetWherePredicateMembers(
                whereInvocation,
                semanticModel,
                cancellationToken,
                out var requiredMembers) ||
            requiredMembers.Any(member => !IsDirectlyStoredMember(member, cancellationToken)))
        {
            return false;
        }

        var declaration = projector.DeclaringSyntaxReferences[0].GetSyntax(cancellationToken) as MethodDeclarationSyntax;
        if (declaration is null || declaration.ParameterList.Parameters.Count != 1)
        {
            return false;
        }

        var returnedExpression = GetSingleReturnedExpression(declaration);
        if (returnedExpression is null)
        {
            return false;
        }

        var declarationModel = semanticModel.Compilation.GetSemanticModel(declaration.SyntaxTree, ignoreAccessibility: true);
        var createdType = declarationModel.GetTypeInfo(returnedExpression, cancellationToken).Type;
        if (!SymbolEqualityComparer.Default.Equals(createdType, projector.ReturnType))
        {
            return false;
        }

        var initializer = GetObjectInitializer(returnedExpression);
        if (initializer is null)
        {
            return false;
        }

        var sourceParameter = declarationModel.GetDeclaredSymbol(
            declaration.ParameterList.Parameters[0],
            cancellationToken);
        if (sourceParameter is null ||
            !IsSafeDirectCloneInitializer(
                initializer,
                sourceParameter,
                declarationModel,
                cancellationToken))
        {
            return false;
        }

        return requiredMembers.All(requiredMember =>
            initializer.Expressions
                .OfType<AssignmentExpressionSyntax>()
                .Any(assignment => IsDirectMemberCopy(
                    assignment,
                    requiredMember,
                    sourceParameter,
                    declarationModel,
                    cancellationToken)));
    }

    private static bool TryGetWherePredicateMembers(
        InvocationExpressionSyntax whereInvocation,
        SemanticModel semanticModel,
        CancellationToken cancellationToken,
        out IReadOnlyList<ISymbol> members)
    {
        members = Array.Empty<ISymbol>();

        if (whereInvocation.ArgumentList.Arguments.Count != 1 ||
            whereInvocation.ArgumentList.Arguments[0].Expression is not LambdaExpressionSyntax predicateLambda)
        {
            return false;
        }

        var sourceParameterSyntax = predicateLambda switch
        {
            SimpleLambdaExpressionSyntax simpleLambda => simpleLambda.Parameter,
            ParenthesizedLambdaExpressionSyntax parenthesizedLambda
                when parenthesizedLambda.ParameterList.Parameters.Count > 0 =>
                    parenthesizedLambda.ParameterList.Parameters[0],
            _ => null
        };
        if (sourceParameterSyntax is null)
        {
            return false;
        }

        var sourceParameter = semanticModel.GetDeclaredSymbol(sourceParameterSyntax, cancellationToken);
        if (sourceParameter is null)
        {
            return false;
        }

        var collectedMembers = new List<ISymbol>();
        foreach (var identifier in predicateLambda.Body.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>())
        {
            var referencedSymbol = semanticModel.GetSymbolInfo(identifier, cancellationToken).Symbol;
            if (!SymbolEqualityComparer.Default.Equals(referencedSymbol, sourceParameter))
            {
                continue;
            }

            SyntaxNode receiver = identifier;
            while (receiver.Parent is ParenthesizedExpressionSyntax parenthesized &&
                   parenthesized.Expression == receiver)
            {
                receiver = parenthesized;
            }

            if (receiver.Parent is not MemberAccessExpressionSyntax memberAccess ||
                memberAccess.Expression != receiver)
            {
                return false;
            }

            var member = semanticModel.GetSymbolInfo(memberAccess, cancellationToken).Symbol;
            if (member is not IPropertySymbol and not IFieldSymbol)
            {
                return false;
            }

            if (!collectedMembers.Any(existing => SymbolEqualityComparer.Default.Equals(existing, member)))
            {
                collectedMembers.Add(member);
            }
        }

        if (collectedMembers.Count == 0)
        {
            return false;
        }

        members = collectedMembers;
        return true;
    }

    private static bool IsSafeDirectCloneInitializer(
        InitializerExpressionSyntax initializer,
        IParameterSymbol sourceParameter,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        if (initializer.Expressions.Count == 0)
        {
            return false;
        }

        foreach (var expression in initializer.Expressions)
        {
            if (expression is not AssignmentExpressionSyntax assignment ||
                !assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
            {
                return false;
            }

            var targetMember = semanticModel.GetSymbolInfo(assignment.Left, cancellationToken).Symbol;
            if (targetMember is null ||
                !IsDirectlyStoredMember(targetMember, cancellationToken) ||
                !IsDirectMemberCopy(
                    assignment,
                    targetMember,
                    sourceParameter,
                    semanticModel,
                    cancellationToken))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsDirectMemberCopy(
        AssignmentExpressionSyntax assignment,
        ISymbol requiredMember,
        IParameterSymbol sourceParameter,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        if (!assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
        {
            return false;
        }

        var targetMember = semanticModel.GetSymbolInfo(assignment.Left, cancellationToken).Symbol;
        if (!SymbolEqualityComparer.Default.Equals(targetMember, requiredMember) ||
            assignment.Right is not MemberAccessExpressionSyntax sourceMemberAccess)
        {
            return false;
        }

        var sourceMember = semanticModel.GetSymbolInfo(sourceMemberAccess, cancellationToken).Symbol;
        return SymbolEqualityComparer.Default.Equals(sourceMember, requiredMember) &&
               IsReferenceToSymbol(
                   sourceMemberAccess.Expression,
                   sourceParameter,
                   semanticModel,
                   cancellationToken);
    }

    private static bool IsReferenceToSymbol(
        ExpressionSyntax expression,
        ISymbol expectedSymbol,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
        {
            expression = parenthesized.Expression;
        }

        if (expression is not IdentifierNameSyntax identifier)
        {
            return false;
        }

        var symbol = semanticModel.GetSymbolInfo(identifier, cancellationToken).Symbol;
        return symbol is not null && SymbolEqualityComparer.Default.Equals(symbol, expectedSymbol);
    }

    private static ExpressionSyntax? GetSingleReturnedExpression(MethodDeclarationSyntax declaration)
    {
        if (declaration.ExpressionBody?.Expression is not null)
        {
            return declaration.ExpressionBody.Expression;
        }

        return declaration.Body?.Statements.Count == 1 &&
               declaration.Body.Statements[0] is ReturnStatementSyntax returnStatement
            ? returnStatement.Expression
            : null;
    }

    private static InitializerExpressionSyntax? GetObjectInitializer(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
        {
            expression = parenthesized.Expression;
        }

        return expression switch
        {
            ObjectCreationExpressionSyntax objectCreation => objectCreation.Initializer,
            ImplicitObjectCreationExpressionSyntax implicitObjectCreation => implicitObjectCreation.Initializer,
            _ => null
        };
    }

    private static bool IsClosedItemType(ITypeSymbol type) =>
        type.TypeKind == TypeKind.Struct ||
        type is INamedTypeSymbol { IsSealed: true };

    private static bool IsDirectlyStoredMember(ISymbol member, CancellationToken cancellationToken) =>
        member switch
        {
            IFieldSymbol field => !field.IsStatic,
            IPropertySymbol property => IsAutoProperty(property, cancellationToken),
            _ => false
        };

    private static bool IsAutoProperty(IPropertySymbol property, CancellationToken cancellationToken)
    {
        if (property.IsStatic || property.DeclaringSyntaxReferences.Length != 1)
        {
            return false;
        }

        var declaration = property.DeclaringSyntaxReferences[0].GetSyntax(cancellationToken) as PropertyDeclarationSyntax;
        return declaration?.AccessorList is not null &&
               declaration.AccessorList.Accessors.Count > 0 &&
               declaration.AccessorList.Accessors.All(accessor =>
                   accessor.Body is null && accessor.ExpressionBody is null);
    }

    private static bool ContainsNode(SyntaxNode container, SyntaxNode candidate) =>
        container == candidate || container.DescendantNodes().Any(node => node == candidate);

    private static InvocationExpressionSyntax? FindInvocationAtSpan(SyntaxNode root, int spanStart)
    {
        if (spanStart < 0 || spanStart >= root.FullSpan.End)
        {
            return null;
        }

        var token = root.FindToken(spanStart);
        return token.Parent?
            .AncestorsAndSelf()
            .OfType<InvocationExpressionSyntax>()
            .FirstOrDefault(invocation => invocation.SpanStart == spanStart);
    }

    private static bool TryGetInvocationSpanStart(EvidenceFact predicate, out int spanStart)
    {
        spanStart = -1;
        var separator = predicate.Id.LastIndexOf(':');
        return separator >= 0 &&
               separator + 1 < predicate.Id.Length &&
               int.TryParse(predicate.Id[(separator + 1)..], out spanStart);
    }

    private static bool IsSupportedTarget(string target, string operation) =>
        string.Equals(target, $"System.Linq.Enumerable.{operation}", StringComparison.Ordinal) ||
        string.Equals(target, $"System.Linq.Queryable.{operation}", StringComparison.Ordinal);

    private static string GetMethodTarget(IMethodSymbol symbol)
    {
        var type = symbol.ContainingType?.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
        return string.IsNullOrWhiteSpace(type) ? symbol.Name : $"{type}.{symbol.Name}";
    }

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

    private static bool IsUnderRoot(string rootPath, string path)
    {
        var relative = Path.GetRelativePath(rootPath, path);
        return relative != ".." &&
               !relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal) &&
               !Path.IsPathRooted(relative);
    }

    private static string NormalizePath(string path) => path.Replace('\\', '/');

    private readonly record struct InvocationKey(string? Path, int SpanStart);

    private sealed record InvocationAuthority(string Target, string? ObservableContext);
}
