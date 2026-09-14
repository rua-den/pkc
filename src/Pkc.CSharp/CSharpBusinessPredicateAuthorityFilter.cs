using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Pkc.Core;

namespace Pkc.CSharp;

internal sealed class CSharpBusinessPredicateAuthorityFilter
{
    private static readonly object RegistrationGate = new();

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
                metadata["observableEffectResolution"] = "direct-return-syntax";
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
                            GetObservableContext(invocation)));
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

    private static string? GetObservableContext(InvocationExpressionSyntax invocation)
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

        var returnStatement = invocation.Ancestors().OfType<ReturnStatementSyntax>().FirstOrDefault();
        if (returnStatement?.Expression is not null &&
            ContainsNode(returnStatement.Expression, invocation))
        {
            return "return";
        }

        var arrow = invocation.Ancestors().OfType<ArrowExpressionClauseSyntax>().FirstOrDefault();
        if (arrow?.Parent == containingMethod && ContainsNode(arrow.Expression, invocation))
        {
            return "return";
        }

        var yieldReturn = invocation.Ancestors().OfType<YieldStatementSyntax>()
            .FirstOrDefault(statement => statement.ReturnOrBreakKeyword.ValueText == "return");
        if (yieldReturn?.Expression is not null && ContainsNode(yieldReturn.Expression, invocation))
        {
            return "yield-return";
        }

        return null;
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
