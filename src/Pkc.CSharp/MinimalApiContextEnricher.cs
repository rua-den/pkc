using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pkc.Core;

namespace Pkc.CSharp;

internal sealed class MinimalApiContextEnricher
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
        var relations = document.Relations.ToList();
        var relationKeys = relations.Select(RelationKey).ToHashSet(StringComparer.Ordinal);

        foreach (var group in document.Facts
                     .Where(IsMinimalEndpoint)
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

            foreach (var endpoint in group)
            {
                var registration = FindRegistration(root, endpoint);
                if (registration is null)
                {
                    continue;
                }

                AddRegistrationContext(endpoint, registration, facts, relations, relationKeys);
                AddHandlerResponses(endpoint, registration, facts, relations, relationKeys);
            }
        }

        return document with
        {
            Facts = facts.Values.OrderBy(fact => fact.Id, StringComparer.Ordinal).ToArray(),
            Relations = relations
                .OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
                .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
                .ThenBy(relation => relation.Target, StringComparer.Ordinal)
                .ToArray()
        };
    }

    private static void AddRegistrationContext(
        EvidenceFact endpoint,
        InvocationExpressionSyntax registration,
        IDictionary<string, EvidenceFact> facts,
        ICollection<EvidenceRelation> relations,
        ISet<string> relationKeys)
    {
        var guards = registration.Ancestors()
            .OfType<IfStatementSyntax>()
            .Reverse()
            .ToArray();

        if (guards.Length == 0)
        {
            return;
        }

        var expressions = guards.Select(guard => guard.Condition.ToString()).ToArray();
        var endpointMetadata = new Dictionary<string, string>(endpoint.Metadata, StringComparer.Ordinal)
        {
            ["availabilityCondition"] = string.Join(" && ", expressions)
        };
        facts[endpoint.Id] = endpoint with { Metadata = endpointMetadata };

        foreach (var guard in guards)
        {
            var location = GetLocation(guard, endpoint.Source.Path);
            var rawExpression = guard.Condition.ToString();
            var expression = $"endpoint is registered only when {rawExpression}";
            var id = $"cs:{endpoint.Source.Path}:{location.StartLine}:condition:endpoint-availability:{endpoint.Name}";
            var condition = new EvidenceFact(
                id,
                "condition",
                "endpoint-availability",
                endpoint.Name,
                location,
                [],
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["expression"] = expression,
                    ["conditionExpression"] = rawExpression,
                    ["sourceKind"] = "endpoint-registration-condition",
                    ["endpointAvailability"] = "conditional"
                });

            facts[id] = condition;
            AddRelation(
                new EvidenceRelation(endpoint.Id, "contains-condition", id, location),
                relations,
                relationKeys);
        }
    }

    private static void AddHandlerResponses(
        EvidenceFact endpoint,
        InvocationExpressionSyntax registration,
        IDictionary<string, EvidenceFact> facts,
        List<EvidenceRelation> relations,
        ISet<string> relationKeys)
    {
        var handler = registration.ArgumentList.Arguments
            .Skip(1)
            .Select(argument => argument.Expression)
            .OfType<AnonymousFunctionExpressionSyntax>()
            .FirstOrDefault();
        if (handler is null)
        {
            return;
        }

        foreach (var condition in handler.DescendantNodes().OfType<IfStatementSyntax>())
        {
            var returned = condition.Statement
                .DescendantNodesAndSelf()
                .OfType<ReturnStatementSyntax>()
                .FirstOrDefault(statement => statement.Expression is not null);
            if (returned?.Expression is null)
            {
                continue;
            }

            var existing = facts.Values.FirstOrDefault(fact =>
                fact.Kind == "condition" &&
                string.Equals(fact.Container, endpoint.Name, StringComparison.Ordinal) &&
                string.Equals(fact.Source.Path, endpoint.Source.Path, StringComparison.Ordinal) &&
                fact.Source.StartLine == GetLocation(condition, endpoint.Source.Path).StartLine &&
                fact.Metadata.TryGetValue("sourceKind", out var sourceKind) &&
                string.Equals(sourceKind, "minimal-api-handler-condition", StringComparison.Ordinal));
            if (existing is null)
            {
                continue;
            }

            existing.Metadata.TryGetValue("expression", out var rawExpression);
            rawExpression ??= condition.Condition.ToString();
            var response = returned.Expression.ToString();
            var location = GetLocation(condition, endpoint.Source.Path);
            var responseFactId = $"cs:{endpoint.Source.Path}:{location.StartLine}:condition:minimal-api-response:{endpoint.Name}";
            var metadata = new Dictionary<string, string>(existing.Metadata, StringComparer.Ordinal)
            {
                ["conditionExpression"] = rawExpression,
                ["expression"] = $"{rawExpression} => return {response}",
                ["response"] = response,
                ["responseKind"] = "minimal-api-return"
            };
            var responseFact = new EvidenceFact(
                responseFactId,
                "condition",
                "minimal-api-response",
                endpoint.Name,
                location,
                [],
                metadata);

            facts[responseFactId] = responseFact;

            var oldRelations = relations
                .Where(relation =>
                    relation.FromFactId == endpoint.Id &&
                    relation.Kind == "contains-condition" &&
                    relation.Target == existing.Id)
                .ToArray();
            foreach (var oldRelation in oldRelations)
            {
                relations.Remove(oldRelation);
                relationKeys.Remove(RelationKey(oldRelation));
            }

            AddRelation(
                new EvidenceRelation(endpoint.Id, "contains-condition", responseFactId, location),
                relations,
                relationKeys);
        }
    }

    private static InvocationExpressionSyntax? FindRegistration(SyntaxNode root, EvidenceFact endpoint)
    {
        if (!endpoint.Metadata.TryGetValue("mapMethod", out var mapMethod) ||
            !endpoint.Metadata.TryGetValue("fullRoute", out var route))
        {
            return null;
        }

        return root.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .FirstOrDefault(invocation =>
                StartLine(invocation) == endpoint.Source.StartLine &&
                string.Equals(GetInvocationName(invocation.Expression), mapMethod, StringComparison.Ordinal) &&
                invocation.ArgumentList.Arguments.Count > 0 &&
                invocation.ArgumentList.Arguments[0].Expression is LiteralExpressionSyntax literal &&
                literal.IsKind(SyntaxKind.StringLiteralExpression) &&
                string.Equals(literal.Token.ValueText, route, StringComparison.Ordinal));
    }

    private static bool IsMinimalEndpoint(EvidenceFact fact) =>
        fact.Kind == "endpoint" &&
        fact.Metadata.TryGetValue("endpointStyle", out var style) &&
        string.Equals(style, "minimal-api", StringComparison.Ordinal);

    private static int StartLine(SyntaxNode node) =>
        node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

    private static string GetInvocationName(ExpressionSyntax expression) => expression switch
    {
        IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
        MemberAccessExpressionSyntax member => member.Name.Identifier.ValueText,
        MemberBindingExpressionSyntax binding => binding.Name.Identifier.ValueText,
        _ => expression.ToString().Split('.').LastOrDefault() ?? expression.ToString()
    };

    private static SourceLocation GetLocation(SyntaxNode node, string relativePath)
    {
        var span = node.GetLocation().GetLineSpan();
        return new SourceLocation(
            relativePath,
            span.StartLinePosition.Line + 1,
            span.EndLinePosition.Line + 1);
    }

    private static void AddRelation(
        EvidenceRelation relation,
        ICollection<EvidenceRelation> relations,
        ISet<string> relationKeys)
    {
        if (relationKeys.Add(RelationKey(relation)))
        {
            relations.Add(relation);
        }
    }

    private static string RelationKey(EvidenceRelation relation) =>
        $"{relation.FromFactId}|{relation.Kind}|{relation.Target}|{relation.Source.Path}|{relation.Source.StartLine}";
}
