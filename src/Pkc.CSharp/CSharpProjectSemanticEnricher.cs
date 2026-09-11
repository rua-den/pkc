using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Pkc.Core;

namespace Pkc.CSharp;

public sealed class CSharpProjectSemanticEnricher
{
    private static readonly object RegistrationGate = new();
    private static readonly HashSet<string> ExcludedDirectoryNames =
        new(StringComparer.OrdinalIgnoreCase) { ".git", ".pkc", "bin", "obj" };

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

                if (!string.IsNullOrWhiteSpace(load.FallbackReason))
                {
                    metadata["analysisFallbackReason"] = load.FallbackReason;
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

            var method = await FindMethodAsync(fact, source, cancellationToken);
            if (method is null)
            {
                continue;
            }

            foreach (var invocation in method.DescendantNodes().OfType<InvocationExpressionSyntax>())
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
            "0.4.3-csharp",
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

        var node = await FindFactNodeAsync(fact, source, cancellationToken);
        if (node is null)
        {
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

    private static async Task<SyntaxNode?> FindFactNodeAsync(
        EvidenceFact fact,
        ProjectSemanticSource source,
        CancellationToken cancellationToken)
    {
        var root = await source.Tree.GetRootAsync(cancellationToken);
        return root.DescendantNodes().FirstOrDefault(node =>
            StartLine(node) == fact.Source.StartLine && MatchesFact(node, fact));
    }

    private static async Task<BaseMethodDeclarationSyntax?> FindMethodAsync(
        EvidenceFact fact,
        ProjectSemanticSource source,
        CancellationToken cancellationToken)
    {
        var node = await FindFactNodeAsync(fact, source, cancellationToken);
        return node as BaseMethodDeclarationSyntax;
    }

    private static bool MatchesFact(SyntaxNode node, EvidenceFact fact) => fact.Kind switch
    {
        "endpoint" or "method" => node is MethodDeclarationSyntax method &&
                                    method.Identifier.ValueText == fact.Name,
        "constructor" => node is ConstructorDeclarationSyntax constructor &&
                           constructor.Identifier.ValueText == fact.Name,
        "class" or "interface" or "struct" or "record" or "enum" =>
            node is BaseTypeDeclarationSyntax type && type.Identifier.ValueText == fact.Name,
        "property" => node is PropertyDeclarationSyntax property && property.Identifier.ValueText == fact.Name,
        "enum-member" => node is EnumMemberDeclarationSyntax member && member.Identifier.ValueText == fact.Name,
        _ => true
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
            .Where(path => !IsExcluded(rootPath, path))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (projectFiles.Length == 0)
        {
            return new ProjectSemanticLoadResult(
                new Dictionary<string, ProjectSemanticSource>(StringComparer.OrdinalIgnoreCase),
                "no-csproj-found");
        }

        try
        {
            EnsureMsBuildRegistered();

            using var workspace = MSBuildWorkspace.Create();
            var diagnostics = new List<string>();
            workspace.WorkspaceFailed += (_, args) => diagnostics.Add(args.Diagnostic.Message);

            var models = new Dictionary<string, ProjectSemanticSource>(StringComparer.OrdinalIgnoreCase);

            foreach (var projectFile in projectFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Project project;
                try
                {
                    project = await workspace.OpenProjectAsync(projectFile, cancellationToken: cancellationToken);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    diagnostics.Add($"{Path.GetFileName(projectFile)}: {exception.Message}");
                    continue;
                }

                var compilation = await project.GetCompilationAsync(cancellationToken);
                if (compilation is null)
                {
                    diagnostics.Add($"{Path.GetFileName(projectFile)}: compilation unavailable");
                    continue;
                }

                var projectPath = NormalizePath(Path.GetRelativePath(rootPath, projectFile));
                foreach (var tree in compilation.SyntaxTrees)
                {
                    if (string.IsNullOrWhiteSpace(tree.FilePath))
                    {
                        continue;
                    }

                    var fullPath = Path.GetFullPath(tree.FilePath);
                    if (!File.Exists(fullPath) || !IsUnderRoot(rootPath, fullPath))
                    {
                        continue;
                    }

                    models.TryAdd(
                        fullPath,
                        new ProjectSemanticSource(tree, compilation.GetSemanticModel(tree, ignoreAccessibility: true), projectPath));
                }
            }

            var fallbackReason = models.Count == 0
                ? diagnostics.FirstOrDefault() ?? "msbuild-project-load-produced-no-source-models"
                : null;

            return new ProjectSemanticLoadResult(models, fallbackReason);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return new ProjectSemanticLoadResult(
                new Dictionary<string, ProjectSemanticSource>(StringComparer.OrdinalIgnoreCase),
                $"msbuild-workspace-unavailable: {exception.Message}");
        }
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

    private static bool IsExcluded(string rootPath, string path)
    {
        var relative = Path.GetRelativePath(rootPath, path);
        return relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(segment => ExcludedDirectoryNames.Contains(segment));
    }

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
        string? FallbackReason);
}
