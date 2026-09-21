using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pkc.Core;

namespace Pkc.CSharp;

internal sealed class CSharpMutationContextEnricher
{
    public async Task<FactDocument> EnrichAsync(
        string repositoryPath,
        FactDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryPath);
        ArgumentNullException.ThrowIfNull(document);

        var rootPath = Path.GetFullPath(repositoryPath);
        var facts = document.Facts.ToDictionary(fact => fact.Id, StringComparer.Ordinal);

        foreach (var group in document.Facts
                     .Where(fact => fact.Kind == "mutation")
                     .GroupBy(fact => fact.Source.Path, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var fullPath = Path.Combine(rootPath, group.Key.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(fullPath))
            {
                continue;
            }

            var text = await File.ReadAllTextAsync(fullPath, cancellationToken);
            var tree = CSharpSyntaxTree.ParseText(text, path: group.Key, cancellationToken: cancellationToken);
            var root = await tree.GetRootAsync(cancellationToken);

            foreach (var fact in group)
            {
                var assignment = FindAssignment(root, fact);
                if (assignment is null)
                {
                    continue;
                }

                if (assignment.Ancestors().OfType<InitializerExpressionSyntax>().Any())
                {
                    var initializerMetadata = new Dictionary<string, string>(fact.Metadata, StringComparer.Ordinal)
                    {
                        ["mutationContext"] = "initializer",
                        ["stateMutationCandidate"] = "false",
                        ["analysisCaveat"] = "assignment-builds-an-object-or-collection-value-not-observed-domain-state"
                    };

                    facts[fact.Id] = fact with
                    {
                        Kind = "initializer-assignment",
                        Metadata = initializerMetadata
                    };
                    continue;
                }

                if (!TryGetRuntimePatternReceiver(assignment, out var receiver))
                {
                    continue;
                }

                var metadata = new Dictionary<string, string>(fact.Metadata, StringComparer.Ordinal)
                {
                    ["mutationReceiver"] = receiver,
                    ["mutationReceiverOrigin"] = "runtime-pattern-variable",
                    ["mutationCausalityBoundary"] = "caller-object-unproven",
                    ["analysisCaveat"] = "runtime-pattern-selected-receiver-requires-caller-object-proof"
                };

                facts[fact.Id] = fact with { Metadata = metadata };
            }
        }

        return document with
        {
            Facts = facts.Values.OrderBy(fact => fact.Id, StringComparer.Ordinal).ToArray()
        };
    }

    private static AssignmentExpressionSyntax? FindAssignment(SyntaxNode root, EvidenceFact fact)
    {
        fact.Metadata.TryGetValue("target", out var target);
        return root.DescendantNodes()
            .OfType<AssignmentExpressionSyntax>()
            .FirstOrDefault(assignment =>
                StartLine(assignment) == fact.Source.StartLine &&
                (string.IsNullOrWhiteSpace(target) ||
                 string.Equals(assignment.Left.ToString(), target, StringComparison.Ordinal)));
    }

    private static bool TryGetRuntimePatternReceiver(
        AssignmentExpressionSyntax assignment,
        out string receiver)
    {
        receiver = GetRootReceiverIdentifier(assignment.Left) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(receiver) || string.Equals(receiver, "this", StringComparison.Ordinal))
        {
            return false;
        }

        var callable = assignment.Ancestors().FirstOrDefault(node =>
            node is BaseMethodDeclarationSyntax or
                LocalFunctionStatementSyntax or
                AnonymousFunctionExpressionSyntax);
        if (callable is null)
        {
            return false;
        }

        var receiverName = receiver;
        return callable.DescendantNodes()
            .OfType<SingleVariableDesignationSyntax>()
            .Where(designation => designation.SpanStart < assignment.SpanStart)
            .Where(designation => string.Equals(
                designation.Identifier.ValueText,
                receiverName,
                StringComparison.Ordinal))
            .Any(designation => designation.Ancestors().Any(ancestor => ancestor is PatternSyntax));
    }

    private static string? GetRootReceiverIdentifier(ExpressionSyntax expression)
    {
        ExpressionSyntax current = expression;

        while (true)
        {
            switch (current)
            {
                case MemberAccessExpressionSyntax memberAccess:
                    current = memberAccess.Expression;
                    continue;
                case ElementAccessExpressionSyntax elementAccess:
                    current = elementAccess.Expression;
                    continue;
                case ParenthesizedExpressionSyntax parenthesized:
                    current = parenthesized.Expression;
                    continue;
                case IdentifierNameSyntax identifier:
                    return identifier.Identifier.ValueText;
                case ThisExpressionSyntax:
                    return "this";
                default:
                    return null;
            }
        }
    }

    private static int StartLine(SyntaxNode node) =>
        node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
}
