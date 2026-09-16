using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Pkc.Core;

namespace Pkc.CSharp;

internal sealed class CSharpValueLineageEnricher
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
                    var extraction = ExtractEndpointTransfers(
                        endpoint,
                        projectPath,
                        method,
                        semanticModel,
                        cancellationToken);

                    foreach (var transfer in extraction.Facts)
                    {
                        facts[transfer.Id] = transfer;
                    }

                    relations.AddRange(extraction.Relations);
                }
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                // Lineage proof is optional and conservative. If exact target-project context
                // cannot be re-established, retain the existing evidence without adding a
                // stronger provenance claim.
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

    private static ExtractionResult ExtractEndpointTransfers(
        EvidenceFact endpoint,
        string projectPath,
        MethodDeclarationSyntax method,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        if (HasUnsupportedControlFlow(method.Body!))
        {
            return ExtractionResult.Empty;
        }

        var topLevelAssignments = method.Body!.Statements
            .OfType<ExpressionStatementSyntax>()
            .Select(statement => statement.Expression)
            .OfType<AssignmentExpressionSyntax>()
            .ToArray();
        var topLevelSet = topLevelAssignments.ToHashSet<SyntaxNode>();

        if (method.Body.DescendantNodes().OfType<AssignmentExpressionSyntax>()
            .Any(assignment => !topLevelSet.Contains(assignment)))
        {
            return ExtractionResult.Empty;
        }

        var versions = new Dictionary<string, int>(StringComparer.Ordinal);
        var currentWriter = new Dictionary<string, WriterInfo>(StringComparer.Ordinal);
        var hadProvenWriter = new HashSet<string>(StringComparer.Ordinal);
        var facts = new List<EvidenceFact>();
        var relations = new List<EvidenceRelation>();

        foreach (var assignment in topLevelAssignments.OrderBy(item => item.SpanStart))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!TryResolveSlot(
                    assignment.Left,
                    semanticModel,
                    projectPath,
                    cancellationToken,
                    out var target))
            {
                continue;
            }

            var targetVersion = versions.GetValueOrDefault(target.Key) + 1;
            versions[target.Key] = targetVersion;

            var sourceExpression = UnwrapParentheses(assignment.Right);
            var isSafeTransfer =
                assignment.IsKind(SyntaxKind.SimpleAssignmentExpression) &&
                TryResolveSlot(
                    sourceExpression,
                    semanticModel,
                    projectPath,
                    cancellationToken,
                    out var source) &&
                !string.Equals(source.Key, target.Key, StringComparison.Ordinal) &&
                IsSupportedStoredScalarAutoProperty(source.Property) &&
                IsSupportedStoredScalarAutoProperty(target.Property) &&
                SymbolEqualityComparer.Default.Equals(source.Property.Type, target.Property.Type);

            if (!isSafeTransfer)
            {
                currentWriter.Remove(target.Key);
                continue;
            }

            var sourceVersion = versions.GetValueOrDefault(source.Key);
            currentWriter.TryGetValue(source.Key, out var predecessor);
            var hasMatchingPredecessor =
                predecessor is not null &&
                predecessor.TargetVersion == sourceVersion;
            var compositionBlocked =
                !hasMatchingPredecessor &&
                hadProvenWriter.Contains(source.Key);

            var location = GetLocation(assignment, endpoint.Source.Path);
            var id = $"cs-lineage:{endpoint.Source.Path}:{assignment.SpanStart}:value-transfer";
            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["knowledgeClass"] = "value-lineage",
                ["scopeFactId"] = endpoint.Id,
                ["scopeName"] = endpoint.Name,
                ["semanticProject"] = projectPath,
                ["analysisMode"] = "project-semantic",
                ["analysisConfidence"] = "high",
                ["proof"] = "straight-line-project-semantic-auto-property-scalar",
                ["mechanism"] = "copy",
                ["temporalSemantics"] = "snapshot",
                ["sourceOccurrence"] = source.Occurrence,
                ["targetOccurrence"] = target.Occurrence,
                ["sourceReceiver"] = source.ReceiverDisplay,
                ["targetReceiver"] = target.ReceiverDisplay,
                ["sourceReceiverIdentity"] = source.ReceiverIdentity,
                ["targetReceiverIdentity"] = target.ReceiverIdentity,
                ["sourceMember"] = source.MemberDisplay,
                ["targetMember"] = target.MemberDisplay,
                ["sourceMemberIdentity"] = source.MemberIdentity,
                ["targetMemberIdentity"] = target.MemberIdentity,
                ["sourceValueVersion"] = sourceVersion.ToString(),
                ["targetValueVersion"] = targetVersion.ToString(),
                ["compositionStatus"] = hasMatchingPredecessor
                    ? "continues-proven-chain"
                    : compositionBlocked
                        ? "blocked-by-intervening-or-unproven-write"
                        : "root-or-standalone"
            };

            if (hasMatchingPredecessor)
            {
                metadata["predecessorTransferFactId"] = predecessor!.FactId;
            }

            var transfer = new EvidenceFact(
                id,
                "value-transfer",
                $"{source.Occurrence} -> {target.Occurrence}",
                endpoint.Container,
                location,
                [],
                metadata);
            facts.Add(transfer);

            // A direct value transfer is also a target write, so reusing the existing
            // method→mutation containment relation lets the current candidate builder retain
            // the evidence without promoting it into a business-condition relation. The
            // transfer remains a distinct value-lineage fact kind and renders separately.
            relations.Add(new EvidenceRelation(
                endpoint.Id,
                "mutates",
                transfer.Id,
                location));

            currentWriter[target.Key] = new WriterInfo(transfer.Id, targetVersion);
            hadProvenWriter.Add(target.Key);
        }

        return new ExtractionResult(facts, relations);
    }

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

    private static bool TryResolveSlot(
        ExpressionSyntax expression,
        SemanticModel semanticModel,
        string projectPath,
        CancellationToken cancellationToken,
        out MemberSlot slot)
    {
        expression = UnwrapParentheses(expression);
        if (expression is not MemberAccessExpressionSyntax memberAccess)
        {
            slot = default!;
            return false;
        }

        var memberSymbol = semanticModel.GetSymbolInfo(memberAccess, cancellationToken).Symbol as IPropertySymbol;
        var receiverSymbol = semanticModel.GetSymbolInfo(memberAccess.Expression, cancellationToken).Symbol;
        if (memberSymbol is null || receiverSymbol is not (ILocalSymbol or IParameterSymbol))
        {
            slot = default!;
            return false;
        }

        var receiverIdentity = BuildReceiverIdentity(receiverSymbol);
        var memberDisplay = $"{memberSymbol.ContainingType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)}.{memberSymbol.Name}";
        var assemblyIdentity = memberSymbol.ContainingAssembly?.Identity.ToString() ?? "unknown-assembly";
        var memberIdentity = $"{assemblyIdentity}|{memberDisplay}";
        var key = $"{projectPath}|{receiverIdentity}|{memberIdentity}";

        slot = new MemberSlot(
            key,
            memberAccess.ToString(),
            memberAccess.Expression.ToString(),
            receiverIdentity,
            memberDisplay,
            memberIdentity,
            memberSymbol);
        return true;
    }

    private static bool IsSupportedStoredScalarAutoProperty(IPropertySymbol property)
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
            property.DeclaringSyntaxReferences.Length != 1)
        {
            return false;
        }

        if (property.DeclaringSyntaxReferences[0].GetSyntax() is not PropertyDeclarationSyntax syntax ||
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

    private sealed record MemberSlot(
        string Key,
        string Occurrence,
        string ReceiverDisplay,
        string ReceiverIdentity,
        string MemberDisplay,
        string MemberIdentity,
        IPropertySymbol Property);

    private sealed record WriterInfo(string FactId, int TargetVersion);

    private sealed record ExtractionResult(
        IReadOnlyList<EvidenceFact> Facts,
        IReadOnlyList<EvidenceRelation> Relations)
    {
        public static ExtractionResult Empty { get; } = new([], []);
    }
}
