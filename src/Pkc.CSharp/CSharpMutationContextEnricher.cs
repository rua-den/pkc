using System.Globalization;
using System.Text.Json;
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
                var mutation = FindMutation(root, fact);
                if (mutation is null)
                {
                    continue;
                }

                var branchPath = BranchPathFor(mutation);
                var branchMetadata = new Dictionary<string, string>(fact.Metadata, StringComparer.Ordinal);
                branchMetadata["branchPath"] = JsonSerializer.Serialize(branchPath);
                branchMetadata["branchConditional"] = (branchPath.Count > 0).ToString().ToLowerInvariant();

                if (mutation is AssignmentExpressionSyntax assignment && assignment.Ancestors().OfType<InitializerExpressionSyntax>().Any())
                {
                    branchMetadata = new Dictionary<string, string>(branchMetadata, StringComparer.Ordinal)
                    {
                        ["mutationContext"] = "initializer",
                        ["stateMutationCandidate"] = "false",
                        ["analysisCaveat"] = "assignment-builds-an-object-or-collection-value-not-observed-domain-state"
                    };

                    facts[fact.Id] = fact with
                    {
                        Kind = "initializer-assignment",
                        Metadata = branchMetadata
                    };
                    continue;
                }

                if (mutation is not AssignmentExpressionSyntax runtimeAssignment ||
                    !TryGetRuntimePatternReceiver(runtimeAssignment, out var receiver))
                {
                    facts[fact.Id] = fact with { Metadata = branchMetadata };
                    continue;
                }

                var metadata = new Dictionary<string, string>(branchMetadata, StringComparer.Ordinal)
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

    private static SyntaxNode? FindMutation(SyntaxNode root, EvidenceFact fact)
    {
        fact.Metadata.TryGetValue("target", out var target);
        var candidates = root.DescendantNodes()
            .Where(node => node is AssignmentExpressionSyntax or PrefixUnaryExpressionSyntax or PostfixUnaryExpressionSyntax)
            .Where(node =>
            {
                var candidate = MutationTarget(node);
                return StartLine(node) == fact.Source.StartLine &&
                       (string.IsNullOrWhiteSpace(target) || string.Equals(candidate, target, StringComparison.Ordinal)) &&
                       MatchesMutationFingerprint(node, fact);
            })
            .ToArray();

        if (fact.Metadata.TryGetValue(CSharpBehaviorFactCollisionDisambiguator.MutationCollisionOrdinalMetadata, out var ordinalText) &&
            int.TryParse(ordinalText, NumberStyles.None, CultureInfo.InvariantCulture, out var ordinal) &&
            ordinal >= 0)
        {
            return candidates.ElementAtOrDefault(ordinal);
        }

        return candidates.FirstOrDefault();
    }

    private static string MutationTarget(SyntaxNode node) =>
        node switch
        {
            AssignmentExpressionSyntax assignment => assignment.Left.ToString(),
            PrefixUnaryExpressionSyntax prefix => prefix.Operand.ToString(),
            PostfixUnaryExpressionSyntax postfix => postfix.Operand.ToString(),
            _ => string.Empty
        };

    private static bool MatchesMutationFingerprint(SyntaxNode node, EvidenceFact fact)
    {
        fact.Metadata.TryGetValue("operator", out var expectedOperator);
        var actualOperator = node switch
        {
            AssignmentExpressionSyntax assignment => assignment.OperatorToken.ValueText,
            PrefixUnaryExpressionSyntax prefix => prefix.Kind().ToString(),
            PostfixUnaryExpressionSyntax postfix => postfix.Kind().ToString(),
            _ => string.Empty
        };

        if (!string.IsNullOrWhiteSpace(expectedOperator) &&
            !string.Equals(actualOperator, expectedOperator, StringComparison.Ordinal))
        {
            return false;
        }

        if (!fact.Metadata.TryGetValue("value", out var expectedValue))
        {
            return true;
        }

        var actualValue = node is AssignmentExpressionSyntax assignmentNode
            ? assignmentNode.Right.ToString()
            : null;
        return string.Equals(actualValue, expectedValue, StringComparison.Ordinal);
    }

    private static IReadOnlyList<BranchStep> BranchPathFor(SyntaxNode mutation)
    {
        var sourcePath = mutation.SyntaxTree?.FilePath ?? string.Empty;
        var callable = mutation.Ancestors().FirstOrDefault(node =>
            node is BaseMethodDeclarationSyntax or LocalFunctionStatementSyntax or AnonymousFunctionExpressionSyntax);
        if (callable is null)
        {
            return [];
        }

        var result = new List<(int Position, BranchStep Step)>();
        foreach (var ancestor in mutation.Ancestors())
        {
            if (ReferenceEquals(ancestor, callable))
            {
                break;
            }

            if (ancestor is SwitchSectionSyntax section)
            {
                var switchStatement = section.Ancestors().OfType<SwitchStatementSyntax>().FirstOrDefault();
                var constructStart = switchStatement?.SpanStart ?? ancestor.SpanStart;
                var constructEnd = switchStatement?.Span.End ?? ancestor.Span.End;
                var labels = section.Labels.Select(label => label.ToString()).ToArray();
                var caseValues = labels
                    .Where(label => label.StartsWith("case ", StringComparison.Ordinal))
                    .Select(label => label[5..].TrimEnd(':').Trim())
                    .ToArray();
                var arm = caseValues.Length > 0
                    ? $"case:{string.Join("|", caseValues)}"
                    : "default";
                var condition = caseValues.Length > 0 ? string.Join(" or ", caseValues) : null;
                result.Add((ancestor.SpanStart, new BranchStep($"{sourcePath}:switch@{constructStart}-{constructEnd}", arm, condition)));
            }
            else if (ancestor is IfStatementSyntax condition && !IsElseIfChild(condition))
            {
                var (arm, text) = GetIfArm(condition, mutation);
                result.Add((ancestor.SpanStart, new BranchStep($"{sourcePath}:if@{ancestor.SpanStart}-{ancestor.Span.End}", arm, text)));
            }
        }

        return result.OrderBy(item => item.Position).Select(item => item.Step).ToArray();
    }

    private static bool IsElseIfChild(IfStatementSyntax condition) =>
        condition.Parent is ElseClauseSyntax;

    private static (string Arm, string? Condition) GetIfArm(IfStatementSyntax root, SyntaxNode mutation)
    {
        if (root.Statement.Span.Contains(mutation.Span))
        {
            return ("then", RenderableCondition(root.Condition));
        }

        var current = root;
        var index = 1;
        while (current.Else?.Statement is IfStatementSyntax next)
        {
            if (next.Statement.Span.Contains(mutation.Span))
            {
                var condition = RenderableCondition(next.Condition);
                return ($"else-if#{index}", condition is null ? null : $"{condition} after earlier branches do not match");
            }

            current = next;
            index++;
        }

        return ("else", index > 1 ? "all earlier branches do not match" : RenderableCondition(root.Condition));
    }

    private static string? RenderableCondition(ExpressionSyntax condition) =>
        condition is IdentifierNameSyntax or MemberAccessExpressionSyntax or BinaryExpressionSyntax or PrefixUnaryExpressionSyntax or ParenthesizedExpressionSyntax
            ? condition.ToString()
            : null;

    private sealed record BranchStep(string Id, string Arm, string? Condition);

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
