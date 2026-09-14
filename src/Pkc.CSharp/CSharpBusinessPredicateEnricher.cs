using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pkc.Core;

namespace Pkc.CSharp;

internal sealed class CSharpBusinessPredicateEnricher
{
    private static readonly IReadOnlyDictionary<string, string> Effects =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Where"] = "inclusion",
            ["Any"] = "existence",
            ["All"] = "universal-requirement",
            ["First"] = "selection",
            ["FirstOrDefault"] = "selection",
            ["Single"] = "selection",
            ["SingleOrDefault"] = "selection"
        };

    public async Task<FactDocument> EnrichAsync(
        string repositoryPath,
        FactDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryPath);
        ArgumentNullException.ThrowIfNull(document);

        var root = Path.GetFullPath(repositoryPath);
        var facts = document.Facts.ToDictionary(fact => fact.Id, StringComparer.Ordinal);
        var relations = document.Relations.ToList();
        var relationKeys = relations
            .Select(relation => $"{relation.FromFactId}|{relation.Kind}|{relation.Target}")
            .ToHashSet(StringComparer.Ordinal);

        var callableFacts = document.Facts
            .Where(fact => fact.Kind is "method" or "endpoint" or "constructor")
            .Where(fact => !CSharpSourceScope.IsExcludedRelativePath(fact.Source.Path))
            .GroupBy(fact => fact.Source.Path, StringComparer.Ordinal);

        foreach (var fileGroup in callableFacts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(root, fileGroup.Key.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path))
            {
                continue;
            }

            var sourceText = await File.ReadAllTextAsync(path, cancellationToken);
            var tree = CSharpSyntaxTree.ParseText(sourceText, path: fileGroup.Key, cancellationToken: cancellationToken);
            var syntaxRoot = await tree.GetRootAsync(cancellationToken);

            foreach (var owner in fileGroup)
            {
                var method = FindMethod(syntaxRoot, owner);
                if (method is null)
                {
                    continue;
                }

                foreach (var invocation in method.DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    if (!TryDescribe(invocation, out var description))
                    {
                        continue;
                    }

                    var span = invocation.GetLocation().GetLineSpan();
                    var location = new SourceLocation(
                        owner.Source.Path,
                        span.StartLinePosition.Line + 1,
                        span.EndLinePosition.Line + 1);
                    var id = $"cs:{owner.Source.Path}:{location.StartLine}:business-predicate:{description.Operation}:{invocation.SpanStart}";
                    var fact = new EvidenceFact(
                        id,
                        "business-predicate",
                        description.Operation,
                        owner.Name,
                        location,
                        [],
                        new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            ["operation"] = description.Operation,
                            ["source"] = description.Source,
                            ["parameter"] = description.Parameter,
                            ["expression"] = description.Expression,
                            ["effect"] = description.Effect,
                            ["sourceKind"] = "query-predicate"
                        });

                    facts[id] = fact;
                    var relation = new EvidenceRelation(owner.Id, "contains-condition", id, location);
                    var relationKey = $"{relation.FromFactId}|{relation.Kind}|{relation.Target}";
                    if (relationKeys.Add(relationKey))
                    {
                        relations.Add(relation);
                    }
                }
            }
        }

        return document with
        {
            SchemaVersion = "0.4.6-csharp",
            Facts = facts.Values.OrderBy(fact => fact.Id, StringComparer.Ordinal).ToArray(),
            Relations = relations
                .OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
                .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
                .ThenBy(relation => relation.Target, StringComparer.Ordinal)
                .ToArray()
        };
    }

    private static BaseMethodDeclarationSyntax? FindMethod(SyntaxNode root, EvidenceFact owner) =>
        root.DescendantNodes()
            .OfType<BaseMethodDeclarationSyntax>()
            .FirstOrDefault(method =>
            {
                var span = method.GetLocation().GetLineSpan();
                if (span.StartLinePosition.Line + 1 != owner.Source.StartLine)
                {
                    return false;
                }

                var name = method switch
                {
                    MethodDeclarationSyntax declaration => declaration.Identifier.ValueText,
                    ConstructorDeclarationSyntax declaration => declaration.Identifier.ValueText,
                    _ => string.Empty
                };
                return string.Equals(name, owner.Name, StringComparison.Ordinal);
            });

    private static bool TryDescribe(InvocationExpressionSyntax invocation, out PredicateDescription description)
    {
        description = default!;
        if (invocation.Expression is not MemberAccessExpressionSyntax member)
        {
            return false;
        }

        var operation = member.Name.Identifier.ValueText;
        if (!Effects.TryGetValue(operation, out var effect) || invocation.ArgumentList.Arguments.Count == 0)
        {
            return false;
        }

        var first = invocation.ArgumentList.Arguments[0].Expression;
        string source;
        LambdaExpressionSyntax lambda;

        if (first is LambdaExpressionSyntax extensionLambda)
        {
            source = member.Expression.ToString();
            lambda = extensionLambda;
        }
        else if (string.Equals(member.Expression.ToString(), "Enumerable", StringComparison.Ordinal) &&
                 invocation.ArgumentList.Arguments.Count >= 2 &&
                 invocation.ArgumentList.Arguments[1].Expression is LambdaExpressionSyntax staticLambda)
        {
            source = first.ToString();
            lambda = staticLambda;
        }
        else
        {
            return false;
        }

        var parameter = lambda switch
        {
            SimpleLambdaExpressionSyntax simple => simple.Parameter.Identifier.ValueText,
            ParenthesizedLambdaExpressionSyntax parenthesized when parenthesized.ParameterList.Parameters.Count == 1 =>
                parenthesized.ParameterList.Parameters[0].Identifier.ValueText,
            _ => string.Empty
        };

        if (string.IsNullOrWhiteSpace(parameter) || lambda.Body is not ExpressionSyntax body)
        {
            return false;
        }

        description = new PredicateDescription(operation, source, parameter, body.ToString(), effect);
        return true;
    }

    private sealed record PredicateDescription(
        string Operation,
        string Source,
        string Parameter,
        string Expression,
        string Effect);
}
