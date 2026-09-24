using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

        var callableSymbols = new Dictionary<string, EvidenceFact[]>(StringComparer.Ordinal);
        foreach (var fact in facts.Where(fact =>
                     projectFactIds.Contains(fact.Id) &&
                     fact.Kind is "method" or "endpoint" or "constructor"))
        {
            var fullPath = Path.GetFullPath(Path.Combine(
                rootPath,
                fact.Source.Path.Replace('/', Path.DirectorySeparatorChar)));
            if (!load.Models.TryGetValue(fullPath, out var source))
            {
                continue;
            }

            var node = await FindFactNodeAsync(fact, source, cancellationToken);
            var symbol = node is null
                ? null
                : source.SemanticModel.GetDeclaredSymbol(node, cancellationToken) as IMethodSymbol;
            if (symbol is null)
            {
                continue;
            }

            var key = GetSymbolKey(symbol);
            callableSymbols[key] = callableSymbols.TryGetValue(key, out var existing)
                ? existing.Append(fact).ToArray()
                : [fact];
        }

        var registrations = FindDirectDiRegistrations(rootPath, load.Models);

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

                if (TryResolveDirectDiDispatch(
                        methodSymbol,
                        source.ProjectPath,
                        registrations,
                        callableSymbols,
                        out var concrete,
                        out var registrationSource))
                {
                    relations.Add(new EvidenceRelation(
                        fact.Id,
                        "dispatches",
                        concrete.Id,
                        registrationSource));
                }
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

    /// <summary>A project already loaded into the shared workspace as another project's reference.</summary>
    private static Project? FindLoadedProject(Workspace workspace, string projectFile)
    {
        var fullPath = Path.GetFullPath(projectFile);
        return workspace.CurrentSolution.Projects.FirstOrDefault(project =>
            project.FilePath is not null &&
            string.Equals(Path.GetFullPath(project.FilePath), fullPath, StringComparison.OrdinalIgnoreCase));
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

            // One workspace for the whole pass: a project referenced by several others is loaded and compiled once
            // and shared, instead of once per referencing project with every copy kept alive by its semantic models.
            using var workspace = MSBuildWorkspace.Create();
            var projectDiagnostics = new List<string>();
            workspace.WorkspaceFailed += (_, args) => projectDiagnostics.Add(args.Diagnostic.Message);

            foreach (var projectFile in projectFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var projectPath = NormalizePath(Path.GetRelativePath(rootPath, projectFile));
                var projectDirectory = Path.GetDirectoryName(Path.GetFullPath(projectFile)) ?? rootPath;
                projectDiagnostics.Clear();

                try
                {
                    var project = FindLoadedProject(workspace, projectFile) ??
                                  await workspace.OpenProjectAsync(projectFile, cancellationToken: cancellationToken);
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

    private static IReadOnlyDictionary<string, DirectDiRegistration[]> FindDirectDiRegistrations(
        string rootPath,
        IReadOnlyDictionary<string, ProjectSemanticSource> models)
    {
        var registrations = new List<DirectDiRegistration>();
        foreach (var source in models.Values)
        {
            var root = source.Tree.GetRoot();
            foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (!IsProvenStartupRegistrationSite(invocation, source.SemanticModel) ||
                    IsConditionalRegistrationSite(invocation))
                {
                    continue;
                }

                var method = source.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
                if (method is null || method.ContainingNamespace.ToDisplayString() !=
                    "Microsoft.Extensions.DependencyInjection" ||
                    method.Name is not ("AddScoped" or "AddTransient" or "AddSingleton") ||
                    method.TypeArguments.Length != 2 ||
                    method.TypeArguments[0] is not INamedTypeSymbol serviceType ||
                    method.TypeArguments[1] is not INamedTypeSymbol implementationType ||
                    serviceType.TypeKind != TypeKind.Interface)
                {
                    continue;
                }

                registrations.Add(new DirectDiRegistration(
                    GetRegistrationKey(source.ProjectPath, serviceType),
                    serviceType,
                    implementationType,
                    GetLocation(
                        invocation,
                        NormalizePath(Path.GetRelativePath(rootPath, source.Tree.FilePath)))));
            }
        }

        return registrations
            .GroupBy(registration => registration.ServiceKey, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.ToArray(),
                StringComparer.Ordinal);
    }

    private static bool IsProvenStartupRegistrationSite(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel)
    {
        var ancestors = invocation.Ancestors().ToArray();
        if (!ancestors.OfType<GlobalStatementSyntax>().Any() ||
            ancestors.Any(ancestor => ancestor is AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax) ||
            invocation.Expression is not MemberAccessExpressionSyntax registrationAccess ||
            registrationAccess.Expression is not MemberAccessExpressionSyntax servicesAccess ||
            servicesAccess.Name.Identifier.ValueText != "Services" ||
            servicesAccess.Expression is not IdentifierNameSyntax builderIdentifier)
        {
            return false;
        }

        var servicesProperty = semanticModel.GetSymbolInfo(servicesAccess).Symbol as IPropertySymbol;
        var builderSymbol = semanticModel.GetSymbolInfo(builderIdentifier).Symbol;
        if (servicesProperty is null ||
            builderSymbol is null ||
            !IsSupportedHostBuilder(servicesProperty.ContainingType))
        {
            return false;
        }

        var root = invocation.SyntaxTree.GetRoot();
        foreach (var candidate in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (candidate.SpanStart <= invocation.SpanStart ||
                candidate.Expression is not MemberAccessExpressionSyntax buildAccess ||
                buildAccess.Name.Identifier.ValueText != "Build")
            {
                continue;
            }

            var candidateBuilder = semanticModel.GetSymbolInfo(buildAccess.Expression).Symbol;
            var buildMethod = semanticModel.GetSymbolInfo(candidate).Symbol as IMethodSymbol;
            if (SymbolEqualityComparer.Default.Equals(builderSymbol, candidateBuilder) &&
                buildMethod is not null &&
                IsSupportedHostBuilder(buildMethod.ContainingType))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsSupportedHostBuilder(INamedTypeSymbol? type) =>
        type?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) is
            "global::Microsoft.AspNetCore.Builder.WebApplicationBuilder" or
            "global::Microsoft.Extensions.Hosting.HostApplicationBuilder";

    private static bool IsConditionalRegistrationSite(InvocationExpressionSyntax invocation) =>
        invocation.Ancestors().Any(ancestor => ancestor switch
        {
            IfStatementSyntax or
            ElseClauseSyntax or
            SwitchStatementSyntax or
            SwitchSectionSyntax or
            SwitchExpressionSyntax or
            ConditionalAccessExpressionSyntax or
            ForStatementSyntax or
            ForEachStatementSyntax or
            WhileStatementSyntax or
            DoStatementSyntax or
            ConditionalExpressionSyntax => true,
            BinaryExpressionSyntax binary => binary.IsKind(SyntaxKind.LogicalAndExpression) ||
                                             binary.IsKind(SyntaxKind.LogicalOrExpression) ||
                                             binary.IsKind(SyntaxKind.CoalesceExpression),
            _ => false
        });

    private static bool TryResolveDirectDiDispatch(
        IMethodSymbol interfaceMethod,
        string projectPath,
        IReadOnlyDictionary<string, DirectDiRegistration[]> registrations,
        IReadOnlyDictionary<string, EvidenceFact[]> callableSymbols,
        out EvidenceFact concrete,
        out SourceLocation registrationSource)
    {
        concrete = null!;
        registrationSource = null!;
        if (interfaceMethod.ContainingType?.TypeKind != TypeKind.Interface ||
            !registrations.TryGetValue(
                GetRegistrationKey(projectPath, interfaceMethod.ContainingType),
                out var matches) ||
            matches.Length != 1)
        {
            return false;
        }

        var registration = matches[0];
        var interfaceMember = registration.ServiceType
            .GetMembers(interfaceMethod.Name)
            .OfType<IMethodSymbol>()
            .SingleOrDefault(candidate => HasSameSignature(candidate, interfaceMethod));
        if (interfaceMember is null)
        {
            return false;
        }

        var implementationMethod = registration.ImplementationType
            .FindImplementationForInterfaceMember(interfaceMember) as IMethodSymbol;
        if (implementationMethod is null ||
            !callableSymbols.TryGetValue(GetSymbolKey(implementationMethod), out var facts) ||
            facts.Length != 1)
        {
            return false;
        }

        concrete = facts[0];
        registrationSource = registration.Source;
        return true;
    }

    private static bool HasSameSignature(IMethodSymbol left, IMethodSymbol right) =>
        left.Name == right.Name &&
        left.Arity == right.Arity &&
        left.Parameters.Length == right.Parameters.Length &&
        left.Parameters.Zip(right.Parameters).All(pair =>
            pair.First.RefKind == pair.Second.RefKind &&
            GetTypeKey(pair.First.Type) == GetTypeKey(pair.Second.Type));

    private static string GetRegistrationKey(string projectPath, ITypeSymbol serviceType) =>
        $"{projectPath}::{GetTypeKey(serviceType)}";

    private static string GetSymbolKey(ISymbol symbol) =>
        $"{GetAssemblyKey(symbol.ContainingAssembly)}::{symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)}";

    private static string GetTypeKey(ITypeSymbol symbol) =>
        $"{GetAssemblyKey(symbol.ContainingAssembly)}::{symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}";

    private static string GetAssemblyKey(IAssemblySymbol? assembly) =>
        assembly?.Identity.ToString() ?? "<no-assembly>";

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

    private sealed record DirectDiRegistration(
        string ServiceKey,
        INamedTypeSymbol ServiceType,
        INamedTypeSymbol ImplementationType,
        SourceLocation Source);
}
