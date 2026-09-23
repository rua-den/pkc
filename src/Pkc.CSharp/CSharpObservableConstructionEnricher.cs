using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Pkc.Core;

namespace Pkc.CSharp;

internal sealed class CSharpObservableConstructionEnricher
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
                    var extraction = ExtractEndpointEvidence(
                        endpoint,
                        projectPath,
                        method,
                        semanticModel,
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
                // Product-value construction proof is optional and conservative.
                // If target-project semantics cannot be re-established, retain the
                // existing evidence without promoting initializer state.
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

    private static ExtractionResult ExtractEndpointEvidence(
        EvidenceFact endpoint,
        string projectPath,
        MethodDeclarationSyntax method,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        var facts = new List<EvidenceFact>();
        var relations = new List<EvidenceRelation>();

        foreach (var variable in method.Body!.DescendantNodes().OfType<VariableDeclaratorSyntax>())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (variable.Initializer?.Value is not BaseObjectCreationExpressionSyntax creation ||
                creation.Initializer is null ||
                !creation.Initializer.IsKind(SyntaxKind.ObjectInitializerExpression) ||
                semanticModel.GetDeclaredSymbol(variable, cancellationToken) is not ILocalSymbol local ||
                local.Type is not INamedTypeSymbol constructedType)
            {
                continue;
            }

            var downstreamUse = FindWholeObjectInvocationUse(
                method,
                creation,
                local,
                semanticModel,
                insideReturn: false,
                cancellationToken);
            var returnUse = FindWholeObjectInvocationUse(
                method,
                creation,
                local,
                semanticModel,
                insideReturn: true,
                cancellationToken);

            if (downstreamUse is null || returnUse is null)
            {
                continue;
            }

            foreach (var assignment in creation.Initializer.Expressions
                         .OfType<AssignmentExpressionSyntax>()
                         .Where(candidate => candidate.IsKind(SyntaxKind.SimpleAssignmentExpression)))
            {
                var member = semanticModel.GetSymbolInfo(assignment.Left, cancellationToken).Symbol;
                if (member is not IPropertySymbol and not IFieldSymbol ||
                    member.ContainingType is null ||
                    !IsMemberOnConstructedType(member.ContainingType, constructedType))
                {
                    continue;
                }

                var memberName = member.Name;
                var target = $"{local.Name}.{memberName}";
                var location = GetLocation(assignment, endpoint.Source.Path);
                var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["knowledgeClass"] = "observable-construction-state",
                    ["scopeFactId"] = endpoint.Id,
                    ["scopeName"] = endpoint.Name,
                    ["semanticProject"] = projectPath,
                    ["analysisMode"] = "project-semantic",
                    ["analysisConfidence"] = "high",
                    ["proof"] = "project-semantic-local-object-whole-argument-downstream-and-return-path",
                    ["mutationContext"] = "observable-object-construction",
                    ["stateMutationCandidate"] = "true",
                    ["target"] = target,
                    ["value"] = assignment.Right.ToString(),
                    ["operator"] = assignment.OperatorToken.ValueText,
                    ["targetSymbolKind"] = member.Kind.ToString(),
                    ["targetSymbol"] = member.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                    ["constructedType"] = constructedType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                    ["constructionLocal"] = local.Name,
                    ["downstreamOperation"] = DescribeInvocation(downstreamUse, semanticModel, cancellationToken),
                    ["returnPathOperation"] = DescribeInvocation(returnUse, semanticModel, cancellationToken)
                };

                var fact = new EvidenceFact(
                    $"cs-construction:{endpoint.Source.Path}:{assignment.SpanStart}:mutation:{memberName}",
                    "mutation",
                    target,
                    endpoint.Container,
                    location,
                    [],
                    metadata);

                facts.Add(fact);
                relations.Add(new EvidenceRelation(endpoint.Id, "mutates", fact.Id, fact.Source));
            }
        }

        return new ExtractionResult(facts, relations);
    }

    private static InvocationExpressionSyntax? FindWholeObjectInvocationUse(
        MethodDeclarationSyntax method,
        BaseObjectCreationExpressionSyntax creation,
        ILocalSymbol local,
        SemanticModel semanticModel,
        bool insideReturn,
        CancellationToken cancellationToken)
    {
        foreach (var invocation in method.Body!.DescendantNodes()
                     .OfType<InvocationExpressionSyntax>()
                     .Where(candidate => candidate.SpanStart > creation.Span.End)
                     .OrderBy(candidate => candidate.SpanStart))
        {
            var returnStatement = invocation.Ancestors().OfType<ReturnStatementSyntax>().FirstOrDefault();
            if (insideReturn != (returnStatement is not null))
            {
                continue;
            }

            var hasExactLocalArgument = invocation.ArgumentList.Arguments.Any(argument =>
                argument.Expression is IdentifierNameSyntax identifier &&
                SymbolEqualityComparer.Default.Equals(
                    semanticModel.GetSymbolInfo(identifier, cancellationToken).Symbol,
                    local));
            if (hasExactLocalArgument)
            {
                return invocation;
            }
        }

        return null;
    }

    private static bool IsMemberOnConstructedType(
        INamedTypeSymbol memberOwner,
        INamedTypeSymbol constructedType)
    {
        for (INamedTypeSymbol? current = constructedType; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, memberOwner) ||
                SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, memberOwner.OriginalDefinition))
            {
                return true;
            }
        }

        return false;
    }

    private static string DescribeInvocation(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(invocation, cancellationToken);
        var method = symbolInfo.Symbol as IMethodSymbol ??
                     symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();
        if (method is null)
        {
            return invocation.Expression.ToString();
        }

        var type = method.ContainingType?.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
        return string.IsNullOrWhiteSpace(type) ? method.Name : $"{type}.{method.Name}";
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
        IReadOnlyList<EvidenceRelation> Relations);
}
