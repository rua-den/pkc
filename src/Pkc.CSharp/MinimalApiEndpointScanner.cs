using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pkc.Core;

namespace Pkc.CSharp;

internal sealed class MinimalApiEndpointScanner
{
    private static readonly HashSet<string> ExcludedDirectoryNames =
        new(StringComparer.OrdinalIgnoreCase) { ".git", ".pkc", "bin", "obj", "knowledge" };

    private static readonly IReadOnlyDictionary<string, string> MapMethods =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["MapGet"] = "GET",
            ["MapPost"] = "POST",
            ["MapPut"] = "PUT",
            ["MapPatch"] = "PATCH",
            ["MapDelete"] = "DELETE"
        };

    public async Task<FactDocument> ScanAsync(
        string repositoryPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryPath);

        var rootPath = Path.GetFullPath(repositoryPath);
        var facts = new List<EvidenceFact>();
        var relations = new List<EvidenceRelation>();

        var files = Directory.EnumerateFiles(rootPath, "*.cs", SearchOption.AllDirectories)
            .Where(path => !IsExcluded(rootPath, path))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var relativePath = NormalizePath(Path.GetRelativePath(rootPath, file));
            var sourceText = await File.ReadAllTextAsync(file, cancellationToken);
            var tree = CSharpSyntaxTree.ParseText(sourceText, path: relativePath, cancellationToken: cancellationToken);
            var root = await tree.GetRootAsync(cancellationToken);

            ExtractEndpoints(root, relativePath, facts, relations);
        }

        return new FactDocument(
            "0.4.4-minimal-api",
            facts.OrderBy(fact => fact.Id, StringComparer.Ordinal).ToArray(),
            relations
                .OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
                .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
                .ThenBy(relation => relation.Target, StringComparer.Ordinal)
                .ToArray());
    }

    private static void ExtractEndpoints(
        SyntaxNode root,
        string relativePath,
        ICollection<EvidenceFact> facts,
        ICollection<EvidenceRelation> relations)
    {
        foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            var mapMethod = GetInvocationName(invocation.Expression);
            if (!MapMethods.TryGetValue(mapMethod, out var httpMethod) ||
                invocation.ArgumentList.Arguments.Count < 2)
            {
                continue;
            }

            var route = GetStringValue(invocation.ArgumentList.Arguments[0].Expression);
            if (string.IsNullOrWhiteSpace(route))
            {
                continue;
            }

            var handler = invocation.ArgumentList.Arguments[1].Expression;
            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["endpointStyle"] = "minimal-api",
                ["mapMethod"] = mapMethod,
                ["httpMethod"] = httpMethod,
                ["routeTemplate"] = route,
                ["fullRoute"] = route,
                ["handlerExpression"] = handler is AnonymousFunctionExpressionSyntax
                    ? "anonymous-function"
                    : handler.ToString()
            };

            if (HasFluentCall(invocation, "RequireAuthorization"))
            {
                metadata["authorization"] = "RequireAuthorization";
            }

            if (HasFluentCall(invocation, "AllowAnonymous"))
            {
                metadata["allowAnonymous"] = "true";
            }

            var endpoint = CreateFact(
                invocation,
                relativePath,
                "endpoint",
                $"{httpMethod} {route}",
                InferArea(route),
                metadata);
            facts.Add(endpoint);

            if (handler is AnonymousFunctionExpressionSyntax anonymousHandler)
            {
                ExtractHandlerSemantics(anonymousHandler, endpoint, relativePath, facts, relations);
            }
            else
            {
                relations.Add(new EvidenceRelation(
                    endpoint.Id,
                    "invokes-syntax",
                    handler.ToString(),
                    GetLocation(handler, relativePath)));
            }
        }
    }

    private static void ExtractHandlerSemantics(
        AnonymousFunctionExpressionSyntax handler,
        EvidenceFact endpoint,
        string relativePath,
        ICollection<EvidenceFact> facts,
        ICollection<EvidenceRelation> relations)
    {
        foreach (var condition in handler.DescendantNodes().OfType<IfStatementSyntax>())
        {
            var fact = CreateBehaviorFact(
                condition,
                relativePath,
                "condition",
                "if",
                endpoint.Name,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["expression"] = condition.Condition.ToString(),
                    ["sourceKind"] = "minimal-api-handler-condition"
                });
            facts.Add(fact);
            relations.Add(new EvidenceRelation(endpoint.Id, "contains-condition", fact.Id, fact.Source));
        }

        foreach (var switchExpression in handler.DescendantNodes().OfType<SwitchExpressionSyntax>())
        {
            foreach (var arm in switchExpression.Arms)
            {
                var expression = $"{switchExpression.GoverningExpression} switch {arm.Pattern} => {arm.Expression}";
                var fact = CreateBehaviorFact(
                    arm,
                    relativePath,
                    "condition",
                    expression,
                    endpoint.Name,
                    new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["expression"] = expression,
                        ["sourceKind"] = "minimal-api-switch-response"
                    });
                facts.Add(fact);
                relations.Add(new EvidenceRelation(endpoint.Id, "contains-condition", fact.Id, fact.Source));
            }
        }

        foreach (var throwStatement in handler.DescendantNodes().OfType<ThrowStatementSyntax>())
        {
            var expression = throwStatement.Expression?.ToString() ?? "throw";
            var fact = CreateBehaviorFact(
                throwStatement,
                relativePath,
                "throw",
                expression,
                endpoint.Name,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["exceptionType"] = throwStatement.Expression is ObjectCreationExpressionSyntax creation
                        ? creation.Type.ToString()
                        : "unknown",
                    ["expression"] = expression
                });
            facts.Add(fact);
            relations.Add(new EvidenceRelation(endpoint.Id, "throws", fact.Id, fact.Source));
        }

        foreach (var catchClause in handler.DescendantNodes().OfType<CatchClauseSyntax>())
        {
            var returned = catchClause.Block.DescendantNodes().OfType<ReturnStatementSyntax>()
                .FirstOrDefault(statement => statement.Expression is not null);
            if (returned?.Expression is null)
            {
                continue;
            }

            var caughtType = catchClause.Declaration?.Type.ToString() ?? "exception";
            var filter = catchClause.Filter?.FilterExpression is { } filterExpression
                ? $" when {filterExpression}"
                : string.Empty;
            var expression = $"catch {caughtType}{filter} => return {returned.Expression}";
            var fact = CreateBehaviorFact(
                catchClause,
                relativePath,
                "condition",
                expression,
                endpoint.Name,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["expression"] = expression,
                    ["sourceKind"] = "exception-response"
                });
            facts.Add(fact);
            relations.Add(new EvidenceRelation(endpoint.Id, "contains-condition", fact.Id, fact.Source));
        }

        foreach (var assignment in handler.DescendantNodes().OfType<AssignmentExpressionSyntax>())
        {
            var target = assignment.Left.ToString();
            var fact = CreateBehaviorFact(
                assignment,
                relativePath,
                "mutation",
                target,
                endpoint.Name,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["target"] = target,
                    ["value"] = assignment.Right.ToString(),
                    ["operator"] = assignment.OperatorToken.ValueText,
                    ["stateMutationCandidate"] = (assignment.Left is MemberAccessExpressionSyntax or ElementAccessExpressionSyntax)
                        .ToString().ToLowerInvariant()
                });
            facts.Add(fact);
            relations.Add(new EvidenceRelation(endpoint.Id, "mutates", fact.Id, fact.Source));
        }

        foreach (var nestedInvocation in handler.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            relations.Add(new EvidenceRelation(
                endpoint.Id,
                "invokes-syntax",
                nestedInvocation.Expression.ToString(),
                GetLocation(nestedInvocation, relativePath)));
        }
    }

    private static bool HasFluentCall(InvocationExpressionSyntax endpointInvocation, string methodName)
    {
        for (SyntaxNode? current = endpointInvocation.Parent;
             current is not null && current is not StatementSyntax;
             current = current.Parent)
        {
            if (current is InvocationExpressionSyntax invocation &&
                string.Equals(GetInvocationName(invocation.Expression), methodName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static string InferArea(string route)
    {
        var segments = route.Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Where(segment => !segment.StartsWith('{'))
            .Where(segment => segment is not "api" and not "internal" and not "dev")
            .ToArray();

        var area = segments.FirstOrDefault() ?? "Root";
        return string.Concat(area
            .Split(['-', '_'], StringSplitOptions.RemoveEmptyEntries)
            .Select(part => char.ToUpperInvariant(part[0]) + part[1..]));
    }

    private static string? GetStringValue(ExpressionSyntax expression) => expression switch
    {
        LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.StringLiteralExpression) =>
            literal.Token.ValueText,
        _ => null
    };

    private static string GetInvocationName(ExpressionSyntax expression) => expression switch
    {
        IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
        MemberAccessExpressionSyntax member => member.Name.Identifier.ValueText,
        MemberBindingExpressionSyntax binding => binding.Name.Identifier.ValueText,
        _ => expression.ToString().Split('.').LastOrDefault() ?? expression.ToString()
    };

    private static EvidenceFact CreateFact(
        SyntaxNode node,
        string relativePath,
        string kind,
        string name,
        string? container,
        IReadOnlyDictionary<string, string> metadata)
    {
        var location = GetLocation(node, relativePath);
        return new EvidenceFact(
            $"cs:{relativePath}:{location.StartLine}:{kind}:{name}",
            kind,
            name,
            container,
            location,
            [],
            metadata);
    }

    private static EvidenceFact CreateBehaviorFact(
        SyntaxNode node,
        string relativePath,
        string kind,
        string name,
        string container,
        IReadOnlyDictionary<string, string> metadata) =>
        CreateFact(node, relativePath, kind, name, container, metadata);

    private static SourceLocation GetLocation(SyntaxNode node, string relativePath)
    {
        var span = node.GetLocation().GetLineSpan();
        return new SourceLocation(
            relativePath,
            span.StartLinePosition.Line + 1,
            span.EndLinePosition.Line + 1);
    }

    private static bool IsExcluded(string rootPath, string path)
    {
        var relative = Path.GetRelativePath(rootPath, path);
        return relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(segment => ExcludedDirectoryNames.Contains(segment));
    }

    private static string NormalizePath(string path) => path.Replace('\\', '/');
}
