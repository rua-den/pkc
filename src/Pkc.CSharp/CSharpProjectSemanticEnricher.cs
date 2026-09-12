using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Pkc.Core;

namespace Pkc.CSharp;

public sealed class CSharpProjectSemanticEnricher
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
        var load = await TryLoadProjectModelsAsync(rootPath, cancellationToken);

        var projectFactIds = new HashSet<string>(StringComparer.Ordinal);
        var facts = new List<EvidenceFact>(document.Facts.Count);

        foreach (var fact in document.Facts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var fullPath = Path.GetFullPath(Path.Combine(
                rootPath,
                fact.Source.Path.Replace('/', Path.DirectorySeparatorChar)));

            if (load.Models.TryGetValue(fullPath, out var source))
            {
                projectFactIds.Add(fact.Id);
                facts.Add(await EnrichFactAsync(fact, source, cancellationToken));
            }
            else
            {
                var metadata = new Dictionary<string, string>(fact.Metadata, StringComparer.Ordinal)
                {
                    ["analysisMode"] = "loose-roslyn-fallback",
                    ["analysisConfidence"] = "medium",
                    ["semanticContext"] = "runtime-platform-assemblies-only"
                };

                var fallbackReason = FindFallbackReason(fullPath, load);
                if (!string.IsNullOrWhiteSpace(fallbackReason))
                {
                    metadata["analysisFallbackReason"] = fallbackReason;
                }

                facts.Add(fact with { Metadata = metadata });
            }
        }

        var relations = document.Relations
            .Where(relation => relation.Kind != "invokes" || !projectFactIds.Contains(relation.FromFactId))
            .ToList();

        foreach (var fact in facts.Where(fact =>
                     projectFactIds.Contains(fact.Id) &&
                     fact.Kind is "method" or "endpoint" or "constructor"))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fullPath = Path.GetFullPath(Path.Combine(
                rootPath,
                fact.Source.Path.Replace('/', Path.DirectorySeparatorChar)));
            if (!load.Models.TryGetValue(fullPath, out var source))
            {
                continue;
            }

            var scope = await FindCallableScopeAsync(fact, source, cancellationToken);
            if (scope is null)
            {
                continue;
            }

            foreach (var invocation in scope.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                var symbolInfo = source.SemanticModel.GetSymbolInfo(invocation, cancellationToken);
                var methodSymbol = symbolInfo.Symbol as IMethodSymbol ??
                                   symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();
                if (methodSymbol is null)
                {
                    continue;
                }

                relations.Add(new EvidenceRelation(
                    fact.Id,
                    "invokes",
                    GetMethodTarget(methodSymbol),
                    GetLocation(invocation, fact.Source.Path)));
            }
        }

        return new FactDocument(
            "0.4.4-csharp",
            facts.OrderBy(fact => fact.Id, StringComparer.Ordinal).ToArray(),
            relations
                .GroupBy(RelationKey, StringComparer.Ordinal)
                .Select(group => group.First())
                .OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
                .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
                .ThenBy(relation => relation.Target, StringComparer.Ordinal)
                .ToArray());
    }

    private static async Task<EvidenceFact> EnrichFactAsync(
        EvidenceFact fact,
        ProjectSemanticSource source,
        CancellationToken cancellationToken)
    {
        var metadata = new Dictionary<string, string>(fact.Metadata, StringComparer.Ordinal)
        {
            ["analysisMode"] = "project-semantic",
            ["analysisConfidence"] = "high",
            ["semanticContext"] = "target-project",
            ["semanticProject"] = source.ProjectPath
        };

        if (!SupportsSemanticNodeEnrichment(fact))
        {
            metadata["semanticNodeMatch"] = "not-applicable";
            return fact with { Metadata = metadata };
        }

        var node = await FindFactNodeAsync(fact, source, cancellationToken);
        if (node is null)
        {
            metadata["analysisConfidence"] = "medium";
            metadata["semanticNodeMatch"] = "failed";
            metadata["analysisCaveat"] = "target-project-loaded-but-fact-node-match-failed";
            return fact with { Metadata = metadata };
        }

        metadata["semanticNodeMatch"] = "matched";

        if (IsMinimalApiEndpoint(fact) && node is InvocationExpressionSyntax endpointRegistration)
        {
            var symbolInfo = source.SemanticModel.GetSymbolInfo(endpointRegistration, cancellationToken);
            var methodSymbol = symbolInfo.Symbol as IMethodSymbol ??
                               symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();

            if (methodSymbol is null)
            {
                metadata["analysisConfidence"] = "medium";
                metadata["endpointRegistrationResolution"] = "unresolved";
                metadata["analysisCaveat"] = "minimal-api-registration-symbol-unresolved";
                return fact with { Metadata = metadata };
            }

            metadata["semanticSymbol"] = methodSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
            metadata["endpointRegistrationResolution"] = "semantic";
            metadata["endpointRegistrationSymbol"] = methodSymbol.OriginalDefinition
                .ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
            return fact with { Metadata = metadata };
        }

        var declared = source.SemanticModel.GetDeclaredSymbol(node, cancellationToken);
        if (declared is not null)
        {
            metadata["semanticSymbol"] = declared.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
        }

        if (declared is INamedTypeSymbol namedType && namedType.BaseType is not null)
        {
            metadata["semanticBaseType"] = namedType.BaseType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
        }

        if (node is BaseMethodDeclarationSyntax method)
        {
            foreach (var attribute in method.AttributeLists.SelectMany(list => list.Attributes))
            {
                var constructor = source.SemanticModel.GetSymbolInfo(attribute, cancellationToken).Symbol as IMethodSymbol;
                var attributeType = constructor?.ContainingType;
                if (attributeType is null)
                {
                    continue;
                }

                if (IsHttpMethodAttribute(attributeType))
                {
                    metadata["endpointAttributeResolution"] = "semantic";
                    metadata["endpointAttributeSymbol"] = attributeType.ToDisplayString();
                }

                if (IsAttribute(attributeType, "AuthorizeAttribute"))
                {
                    metadata["authorizationAttributeResolution"] = "semantic";
                    metadata["authorizationAttributeSymbol"] = attributeType.ToDisplayString();
                }
            }
        }

        return fact with { Metadata = metadata };
    }

    private static bool SupportsSemanticNodeEnrichment(EvidenceFact fact) =>
        IsMinimalApiEndpoint(fact) || fact.Kind is
            "endpoint" or "method" or "constructor" or
            "class" or "interface" or "struct" or "record" or "enum" or
            "property" or "enum-member";

    private static bool IsMinimalApiEndpoint(EvidenceFact fact) =>
        fact.Kind == "endpoint" &&
        fact.Metadata.TryGetValue("endpointStyle", out var style) &&
        string.Equals(style, "minimal-api", StringComparison.Ordinal);

    private static async Task<SyntaxNode?> FindFactNodeAsync(
        EvidenceFact fact,
        ProjectSemanticSource source,
        CancellationToken cancellationToken)
    {
        var root = await source.Tree.GetRootAsync(cancellationToken);
        return root.DescendantNodes().FirstOrDefault(node =>
            StartLine(node) == fact.Source.StartLine && MatchesFact(node, fact));
    }

    private static async Task<SyntaxNode?> FindCallableScopeAsync(
        EvidenceFact fact,
        ProjectSemanticSource source,
        CancellationToken cancellationToken)
    {
        var node = await FindFactNodeAsync(fact, source, cancellationToken);
        if (node is BaseMethodDeclarationSyntax method)
        {
            return method;
        }

        if (IsMinimalApiEndpoint(fact) && node is InvocationExpressionSyntax registration)
        {
            return registration.ArgumentList.Arguments
                .Skip(1)
                .Select(argument => argument.Expression)
                .OfType<AnonymousFunctionExpressionSyntax>()
                .FirstOrDefault();
        }

        return null;
    }

    private static bool MatchesFact(SyntaxNode node, EvidenceFact fact)
    {
        if (IsMinimalApiEndpoint(fact))
        {
            if (node is not InvocationExpressionSyntax invocation ||
                !fact.Metadata.TryGetValue("mapMethod", out var mapMethod) ||
                !string.Equals(GetInvocationName(invocation.Expression), mapMethod, StringComparison.Ordinal))
            {
                return false;
            }

            if (!fact.Metadata.TryGetValue("fullRoute", out var fullRoute) ||
                invocation.ArgumentList.Arguments.Count == 0)
            {
                return true;
            }

            return invocation.ArgumentList.Arguments[0].Expression is LiteralExpressionSyntax literal &&
                   string.Equals(literal.Token.ValueText, fullRoute, StringComparison.Ordinal);
        }

        return fact.Kind switch
        {
            "endpoint" or "method" => node is MethodDeclarationSyntax method &&
                                        method.Identifier.ValueText == fact.Name,
            "constructor" => node is ConstructorDeclarationSyntax constructor &&
                               constructor.Identifier.ValueText == fact.Name,
            "class" or "interface" or "struct" or "record" or "enum" =>
                node is BaseTypeDeclarationSyntax type && type.Identifier.ValueText == fact.Name,
            "property" => node is PropertyDeclarationSyntax property && property.Identifier.ValueText == fact.Name,
            "enum-member" => node is EnumMemberDeclarationSyntax member && member.Identifier.ValueText == fact.Name,
            _ => false
        };
    }

    private static string GetInvocationName(ExpressionSyntax expression) => expression switch
    {
        IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
        MemberAccessExpressionSyntax member => member.Name.Identifier.ValueText,
        MemberBindingExpressionSyntax binding => binding.Name.Identifier.ValueText,
        _ => expression.ToString().Split('.').LastOrDefault() ?? expression.ToString()
    };

    private static bool IsHttpMethodAttribute(INamedTypeSymbol type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (current.Name == "HttpMethodAttribute" &&
                current.ContainingNamespace.ToDisplayString().StartsWith("Microsoft.AspNetCore.Mvc", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsAttribute(INamedTypeSymbol type, string name) =>
        string.Equals(type.Name, name, StringComparison.Ordinal);

    private static async Task<ProjectSemanticLoadResult> TryLoadProjectModelsAsync(
        string rootPath,
        CancellationToken cancellationToken)
    {
        var projectFiles = Directory.EnumerateFiles(rootPath, "*.csproj", SearchOption.AllDirectories)
            .Where(path => !CSharpSourceScope.IsExcluded(rootPath, path))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (projectFiles.Length == 0)
        {
            return new ProjectSemanticLoadResult(
                new Dictionary<string, ProjectSemanticSource>(StringComparer.OrdinalIgnoreCase),
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                "no-csproj-found");
        }

        try
        {
            EnsureMsBuildRegistered();

            var models = new Dictionary<string, ProjectSemanticSource>(StringComparer.OrdinalIgnoreCase);
            var projectFailures = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var diagnostics = new List<string>();

            foreach (var projectFile in projectFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var projectPath = NormalizePath(Path.GetRelativePath(rootPath, projectFile));
                var projectDirectory = Path.GetDirectoryName(Path.GetFullPath(projectFile)) ?? rootPath;
                var projectDiagnostics = new List<string>();

                try
                {
                    using var workspace = MSBuildWorkspace.Create();
                    workspace.WorkspaceFailed += (_, args) => projectDiagnostics.Add(args.Diagnostic.Message);

                    var project = await workspace.OpenProjectAsync(projectFile, cancellationToken: cancellationToken);
                    var compilation = await project.GetCompilationAsync(cancellationToken);
                    if (compilation is null)
                    {
                        projectFailures[projectDirectory] = $"{projectPath}: compilation unavailable";
                        continue;
                    }

                    foreach (var tree in compilation.SyntaxTrees)
                    {
                        if (string.IsNullOrWhiteSpace(tree.FilePath))
                        {
                            continue;
                        }

                        var fullPath = Path.GetFullPath(tree.FilePath);
                        if (!File.Exists(fullPath) || !IsUnderRoot(rootPath, fullPath) ||
                            CSharpSourceScope.IsExcluded(rootPath, fullPath))
                        {
                            continue;
                        }

                        models.TryAdd(
                            fullPath,
                            new ProjectSemanticSource(
                                tree,
                                compilation.GetSemanticModel(tree, ignoreAccessibility: true),
                                projectPath));
                    }

                    if (projectDiagnostics.Count > 0)
                    {
                        diagnostics.AddRange(projectDiagnostics.Select(message => $"{projectPath}: {message}"));
                    }
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    var reason = $"{projectPath}: {exception.Message}";
                    projectFailures[projectDirectory] = reason;
                    diagnostics.Add(reason);
                }
            }

            var fallbackReason = models.Count == 0
                ? diagnostics.FirstOrDefault() ?? "msbuild-project-load-produced-no-source-models"
                : null;

            return new ProjectSemanticLoadResult(models, projectFailures, fallbackReason);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return new ProjectSemanticLoadResult(
                new Dictionary<string, ProjectSemanticSource>(StringComparer.OrdinalIgnoreCase),
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                $"msbuild-workspace-unavailable: {exception.Message}");
        }
    }

    private static string? FindFallbackReason(string fullPath, ProjectSemanticLoadResult load)
    {
        if (!string.IsNullOrWhiteSpace(load.FallbackReason))
        {
            return load.FallbackReason;
        }

        var directory = Path.GetDirectoryName(fullPath);
        while (!string.IsNullOrWhiteSpace(directory))
        {
            if (load.ProjectFailures.TryGetValue(directory, out var reason))
            {
                return reason;
            }

            directory = Path.GetDirectoryName(directory);
        }

        return "source-not-present-in-loaded-project-compilation";
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

    private static string GetMethodTarget(IMethodSymbol symbol)
    {
        var type = symbol.ContainingType?.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
        return string.IsNullOrWhiteSpace(type) ? symbol.Name : $"{type}.{symbol.Name}";
    }

    private static string RelationKey(EvidenceRelation relation) =>
        $"{relation.FromFactId}|{relation.Kind}|{relation.Target}|{relation.Source.Path}|{relation.Source.StartLine}";

    private static bool IsUnderRoot(string rootPath, string path)
    {
        var relative = Path.GetRelativePath(rootPath, path);
        return relative != ".." &&
               !relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal) &&
               !Path.IsPathRooted(relative);
    }

    private static string NormalizePath(string path) => path.Replace('\\', '/');

    private sealed record ProjectSemanticSource(
        SyntaxTree Tree,
        SemanticModel SemanticModel,
        string ProjectPath);

    private sealed record ProjectSemanticLoadResult(
        IReadOnlyDictionary<string, ProjectSemanticSource> Models,
        IReadOnlyDictionary<string, string> ProjectFailures,
        string? FallbackReason);
}
