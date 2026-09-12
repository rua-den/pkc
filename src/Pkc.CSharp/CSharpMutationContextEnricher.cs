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
                if (assignment is null ||
                    !assignment.Ancestors().OfType<InitializerExpressionSyntax>().Any())
                {
                    continue;
                }

                var metadata = new Dictionary<string, string>(fact.Metadata, StringComparer.Ordinal)
                {
                    ["mutationContext"] = "initializer",
                    ["stateMutationCandidate"] = "false",
                    ["analysisCaveat"] = "assignment-builds-an-object-or-collection-value-not-observed-domain-state"
                };

                facts[fact.Id] = fact with
                {
                    Kind = "initializer-assignment",
                    Metadata = metadata
                };
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

    private static int StartLine(SyntaxNode node) =>
        node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
}
