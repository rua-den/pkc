using System.Net;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pkc.Core;

namespace Pkc.CSharp;

/// <summary>
/// Rules that decide what a workflow returns but do not live in the workflow's own methods:
/// <list type="bullet">
///   <item>mapping-profile member rules (<c>ForMember(d =&gt; d.X, o =&gt; o.MapFrom(rule))</c>), where the rule is an
///   inline lambda or a reusable <c>Expression&lt;Func&lt;…&gt;&gt;</c> factory. The factory's conjuncts, the developer's
///   inline comments and its XML summary are kept, each labelled by authority;</item>
///   <item>boolean gates: a member assigned from an <c>&amp;&amp;</c> conjunction, with local operands resolved to their
///   initializer and constant setting keys named.</item>
/// </list>
/// Everything is proven by the target project's semantic model; anything ambiguous is skipped rather than guessed.
/// </summary>
internal static partial class CSharpRuleSurfaceAnalyzer
{
    public const string MappedFieldRuleKind = "mapped-field-rule";
    public const string AppliesMappedFieldRuleRelation = "applies-mapped-field-rule";
    public const string BooleanGateKind = "boolean-gate";
    public const string ContainsBooleanGateRelation = "contains-boolean-gate";
    public const string GuardConditionKind = "guard-condition";
    public const string ContainsGuardRelation = "contains-guard";

    private const int MaxNoteLength = 400;
    private const int MaxDocumentationLength = 2000;

    internal sealed record RuleSource(SyntaxTree Tree, SemanticModel Model, string RelativePath);

    internal sealed record MappedFieldRule(EvidenceFact Fact, string DestinationTypeKey);

    public static IReadOnlyList<MappedFieldRule> FindMappedFieldRules(
        string rootPath,
        IEnumerable<RuleSource> sources,
        CancellationToken cancellationToken)
    {
        var rules = new Dictionary<string, MappedFieldRule>(StringComparer.Ordinal);
        foreach (var source in sources)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var root = source.Tree.GetRoot(cancellationToken);
            foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (TryDescribeMappedFieldRule(rootPath, source, invocation, cancellationToken) is { } rule)
                {
                    rules.TryAdd(rule.Fact.Id, rule);
                }
            }
        }

        return rules.Values.OrderBy(rule => rule.Fact.Id, StringComparer.Ordinal).ToArray();
    }

    /// <summary>Destination types a callable projects into: its return type and every generic type argument it uses.</summary>
    public static IReadOnlySet<string> FindProjectedTypeKeys(
        SyntaxNode scope,
        SemanticModel model,
        CancellationToken cancellationToken)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);
        if (model.GetDeclaredSymbol(scope, cancellationToken) is IMethodSymbol declared)
        {
            AddTypeKeys(declared.ReturnType, keys);
        }

        foreach (var generic in scope.DescendantNodes().OfType<GenericNameSyntax>())
        {
            switch (model.GetSymbolInfo(generic, cancellationToken).Symbol)
            {
                case INamedTypeSymbol type:
                    AddTypeKeys(type, keys);
                    break;
                case IMethodSymbol method:
                    foreach (var argument in method.TypeArguments)
                    {
                        AddTypeKeys(argument, keys);
                    }

                    break;
            }
        }

        return keys;
    }

    public static IEnumerable<EvidenceFact> FindBooleanGates(
        EvidenceFact owner,
        SyntaxNode scope,
        SemanticModel model,
        CancellationToken cancellationToken)
    {
        foreach (var assignment in scope.DescendantNodes().OfType<AssignmentExpressionSyntax>())
        {
            if (!assignment.IsKind(SyntaxKind.SimpleAssignmentExpression) ||
                Unwrap(assignment.Right) is not BinaryExpressionSyntax conjunction ||
                !conjunction.IsKind(SyntaxKind.LogicalAndExpression) ||
                model.GetSymbolInfo(assignment.Left, cancellationToken).Symbol is not (IPropertySymbol or IFieldSymbol))
            {
                continue;
            }

            var operands = FlattenConjunction(conjunction);
            var location = GetLocation(assignment, owner.Source.Path);
            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["target"] = Normalize(assignment.Left),
                ["operandCount"] = operands.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["analysisMode"] = "project-semantic",
                ["analysisConfidence"] = "high"
            };

            for (var index = 0; index < operands.Count; index++)
            {
                var operand = operands[index];
                metadata[$"operand.{index}"] = Normalize(operand);
                var origin = operand;
                if (Unwrap(operand) is IdentifierNameSyntax identifier &&
                    model.GetSymbolInfo(identifier, cancellationToken).Symbol is ILocalSymbol local &&
                    FindLocalInitializer(scope, model, local, cancellationToken) is { } initializer)
                {
                    metadata[$"operandSource.{index}"] = Normalize(initializer);
                    origin = initializer;
                }

                if (FindSettingKey(origin, model, cancellationToken) is { } settingKey)
                {
                    metadata[$"operandSetting.{index}"] = settingKey;
                }
            }

            yield return new EvidenceFact(
                $"cs:{owner.Source.Path}:{location.StartLine}:boolean-gate:{assignment.SpanStart}",
                BooleanGateKind,
                Normalize(assignment.Left),
                owner.Name,
                location,
                [],
                metadata);
        }
    }

    /// <summary>
    /// Guard helpers such as <c>Guard.Against&lt;TException&gt;(condition, message)</c>: a static void method on a
    /// throw/guard type whose first parameter is the rejecting condition.
    /// </summary>
    public static IEnumerable<EvidenceFact> FindGuardConditions(
        EvidenceFact owner,
        SyntaxNode scope,
        SemanticModel model,
        CancellationToken cancellationToken)
    {
        foreach (var invocation in scope.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (invocation.ArgumentList.Arguments.Count == 0 ||
                model.GetSymbolInfo(invocation, cancellationToken).Symbol is not IMethodSymbol
                {
                    IsStatic: true,
                    ReturnsVoid: true
                } method ||
                method.Parameters.Length == 0 ||
                method.Parameters[0].Type.SpecialType != SpecialType.System_Boolean ||
                !IsGuardMethod(method))
            {
                continue;
            }

            var location = GetLocation(invocation, owner.Source.Path);
            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["condition"] = Normalize(invocation.ArgumentList.Arguments[0].Expression),
                ["guard"] = $"{method.ContainingType.Name}.{method.Name}",
                ["analysisMode"] = "project-semantic",
                ["analysisConfidence"] = "high"
            };
            if (method.TypeArguments.FirstOrDefault() is { } exception)
            {
                metadata["exceptionType"] = exception.Name;
            }

            if (invocation.ArgumentList.Arguments.Count > 1)
            {
                metadata["message"] = Truncate(Normalize(invocation.ArgumentList.Arguments[1].Expression), MaxNoteLength);
            }

            yield return new EvidenceFact(
                $"cs:{owner.Source.Path}:{location.StartLine}:guard:{invocation.SpanStart}",
                GuardConditionKind,
                method.Name,
                owner.Name,
                location,
                [],
                metadata);
        }
    }

    private static bool IsGuardMethod(IMethodSymbol method)
    {
        var type = method.ContainingType.Name;
        return (type.Contains("Throw", StringComparison.Ordinal) || type.Contains("Guard", StringComparison.Ordinal)) &&
               (method.Name is "Against" or "If" or "When" or "IfTrue" || method.Name.StartsWith("ThrowIf", StringComparison.Ordinal));
    }

    public static string TypeKey(ITypeSymbol symbol) =>
        $"{symbol.ContainingAssembly?.Identity.ToString() ?? "<no-assembly>"}::{symbol.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}";

    private static MappedFieldRule? TryDescribeMappedFieldRule(
        string rootPath,
        RuleSource source,
        InvocationExpressionSyntax invocation,
        CancellationToken cancellationToken)
    {
        if (invocation.Expression is not MemberAccessExpressionSyntax { Name.Identifier.ValueText: "ForMember" } ||
            invocation.ArgumentList.Arguments.Count != 2 ||
            invocation.ArgumentList.Arguments[0].Expression is not LambdaExpressionSyntax { Body: MemberAccessExpressionSyntax destinationMember } ||
            invocation.ArgumentList.Arguments[1].Expression is not LambdaExpressionSyntax { Body: InvocationExpressionSyntax mapFrom } ||
            mapFrom.Expression is not MemberAccessExpressionSyntax { Name.Identifier.ValueText: "MapFrom" } ||
            mapFrom.ArgumentList.Arguments.Count != 1 ||
            source.Model.GetTypeInfo(destinationMember.Expression, cancellationToken).Type is not INamedTypeSymbol destinationType)
        {
            return null;
        }

        var ruleExpression = mapFrom.ArgumentList.Arguments[0].Expression;
        IMethodSymbol? factory = null;
        MethodDeclarationSyntax? factoryDeclaration = null;
        LambdaExpressionSyntax? lambda = ruleExpression as LambdaExpressionSyntax;
        if (lambda is null)
        {
            if (ruleExpression is not InvocationExpressionSyntax factoryCall ||
                source.Model.GetSymbolInfo(factoryCall, cancellationToken).Symbol is not IMethodSymbol method ||
                !IsExpressionTree(method.ReturnType) ||
                method.DeclaringSyntaxReferences.Length != 1 ||
                method.DeclaringSyntaxReferences[0].GetSyntax(cancellationToken) is not MethodDeclarationSyntax declaration ||
                FindReturnedLambda(declaration) is not { } returned)
            {
                return null;
            }

            factory = method;
            factoryDeclaration = declaration;
            lambda = returned;
        }

        if (lambda.Body is not ExpressionSyntax body || !IsDecision(Unwrap(body)))
        {
            return null;
        }

        var field = destinationMember.Name.Identifier.ValueText;
        var location = GetLocation(invocation, source.RelativePath);
        var destination = destinationType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
        var conjuncts = Unwrap(body) is BinaryExpressionSyntax binary && binary.IsKind(SyntaxKind.LogicalAndExpression)
            ? FlattenConjunction(binary)
            : [body];

        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["destinationType"] = destination,
            ["field"] = field,
            ["ruleSource"] = factory is null
                ? "inline-lambda"
                : factory.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
            ["conjunctCount"] = conjuncts.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["analysisMode"] = "project-semantic",
            ["analysisConfidence"] = "high"
        };

        var lambdaPath = RelativePath(rootPath, lambda.SyntaxTree.FilePath) ?? source.RelativePath;
        metadata["ruleLocation"] = $"{lambdaPath}:L{GetLocation(lambda, lambdaPath).StartLine}";

        for (var index = 0; index < conjuncts.Count; index++)
        {
            metadata[$"conjunct.{index}"] = Normalize(conjuncts[index]);
            if (DeveloperNote(conjuncts[index]) is { } note)
            {
                metadata[$"note.{index}"] = note;
            }
        }

        if (factoryDeclaration is not null && Summary(factoryDeclaration) is { } documentation)
        {
            metadata["documentation"] = documentation;
        }

        var fact = new EvidenceFact(
            $"cs:{source.RelativePath}:{location.StartLine}:mapped-field-rule:{destinationType.Name}.{field}",
            MappedFieldRuleKind,
            field,
            destination,
            location,
            [],
            metadata);
        return new MappedFieldRule(fact, TypeKey(destinationType));
    }

    private static bool IsExpressionTree(ITypeSymbol type) =>
        type is INamedTypeSymbol { Name: "Expression", IsGenericType: true } named &&
        named.ContainingNamespace.ToDisplayString() == "System.Linq.Expressions";

    /// <summary>The single lambda a factory returns; any other body shape is not proven and is skipped.</summary>
    private static LambdaExpressionSyntax? FindReturnedLambda(MethodDeclarationSyntax declaration)
    {
        if (declaration.ExpressionBody?.Expression is LambdaExpressionSyntax expressionBodied)
        {
            return expressionBodied;
        }

        if (declaration.Body is null)
        {
            return null;
        }

        var returns = declaration.Body.DescendantNodes(node => node is not AnonymousFunctionExpressionSyntax and not LocalFunctionStatementSyntax)
            .OfType<ReturnStatementSyntax>()
            .ToArray();
        return returns.Length == 1 ? returns[0].Expression as LambdaExpressionSyntax : null;
    }

    private static bool IsDecision(ExpressionSyntax body) =>
        body is BinaryExpressionSyntax or PrefixUnaryExpressionSyntax or ConditionalExpressionSyntax or IsPatternExpressionSyntax;

    private static List<ExpressionSyntax> FlattenConjunction(BinaryExpressionSyntax conjunction)
    {
        var operands = new List<ExpressionSyntax>();
        void Visit(ExpressionSyntax expression)
        {
            if (expression is BinaryExpressionSyntax binary && binary.IsKind(SyntaxKind.LogicalAndExpression))
            {
                Visit(binary.Left);
                Visit(binary.Right);
                return;
            }

            operands.Add(expression);
        }

        Visit(conjunction);
        return operands;
    }

    private static ExpressionSyntax Unwrap(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
        {
            expression = parenthesized.Expression;
        }

        return expression;
    }

    private static ExpressionSyntax? FindLocalInitializer(
        SyntaxNode scope,
        SemanticModel model,
        ILocalSymbol local,
        CancellationToken cancellationToken) =>
        scope.DescendantNodes()
            .OfType<VariableDeclaratorSyntax>()
            .FirstOrDefault(declarator => SymbolEqualityComparer.Default.Equals(
                model.GetDeclaredSymbol(declarator, cancellationToken), local))
            ?.Initializer?.Value;

    /// <summary>A constant string key passed to the call that produced the value, for example a settings key.</summary>
    private static string? FindSettingKey(ExpressionSyntax expression, SemanticModel model, CancellationToken cancellationToken)
    {
        var unwrapped = Unwrap(expression);
        if (unwrapped is AwaitExpressionSyntax awaited)
        {
            unwrapped = Unwrap(awaited.Expression);
        }

        if (unwrapped is not InvocationExpressionSyntax invocation)
        {
            return null;
        }

        foreach (var argument in invocation.ArgumentList.Arguments)
        {
            if (model.GetSymbolInfo(argument.Expression, cancellationToken).Symbol is IFieldSymbol { IsStatic: true } field &&
                field.Type.SpecialType == SpecialType.System_String &&
                (field.IsConst || HasLiteralInitializer(field, cancellationToken)))
            {
                return $"{field.ContainingType.Name}.{field.Name}";
            }
        }

        return null;
    }

    /// <summary>A static string field initialized with a literal is used as a key even when it is not declared const.</summary>
    private static bool HasLiteralInitializer(IFieldSymbol field, CancellationToken cancellationToken) =>
        field.DeclaringSyntaxReferences.Length == 1 &&
        field.DeclaringSyntaxReferences[0].GetSyntax(cancellationToken) is VariableDeclaratorSyntax
        {
            Initializer.Value: LiteralExpressionSyntax literal
        } &&
        literal.IsKind(SyntaxKind.StringLiteralExpression);

    private static string? DeveloperNote(SyntaxNode node)
    {
        var comments = node.GetLeadingTrivia()
            .Where(trivia => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            .Select(trivia => trivia.ToString().TrimStart('/').Trim())
            .Where(text => text.Length > 0)
            .ToArray();
        return comments.Length == 0 ? null : Truncate(string.Join(" ", comments), MaxNoteLength);
    }

    private static string? Summary(MethodDeclarationSyntax declaration)
    {
        var summary = declaration.GetLeadingTrivia()
            .Select(trivia => trivia.GetStructure())
            .OfType<DocumentationCommentTriviaSyntax>()
            .SelectMany(comment => comment.Content.OfType<XmlElementSyntax>())
            .FirstOrDefault(element => element.StartTag.Name.LocalName.ValueText == "summary");
        if (summary is null)
        {
            return null;
        }

        var text = summary.ToString().Replace("///", " ", StringComparison.Ordinal);
        text = CrefRegex().Replace(text, match => match.Groups["name"].Value.Split('.').Last());
        text = ParamRefRegex().Replace(text, match => match.Groups["name"].Value);
        text = TagRegex().Replace(text, " ");
        text = WebUtility.HtmlDecode(WhitespaceRegex().Replace(text, " ")).Trim();
        return text.Length == 0 ? null : Truncate(text, MaxDocumentationLength);
    }

    private static string Normalize(SyntaxNode node)
    {
        var withoutComments = node.ReplaceTrivia(
            node.DescendantTrivia(descendIntoTrivia: true),
            (original, _) => original.IsKind(SyntaxKind.SingleLineCommentTrivia) || original.IsKind(SyntaxKind.MultiLineCommentTrivia)
                ? SyntaxFactory.Space
                : original);
        var text = WhitespaceRegex().Replace(withoutComments.ToString(), " ").Trim();
        return text.Replace("( ", "(", StringComparison.Ordinal).Replace(" )", ")", StringComparison.Ordinal);
    }

    private static void AddTypeKeys(ITypeSymbol type, ISet<string> keys)
    {
        if (type is not INamedTypeSymbol named)
        {
            return;
        }

        keys.Add(TypeKey(named));
        foreach (var argument in named.TypeArguments)
        {
            AddTypeKeys(argument, keys);
        }
    }

    private static string? RelativePath(string rootPath, string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var relative = Path.GetRelativePath(rootPath, path);
        return relative.StartsWith("..", StringComparison.Ordinal) || Path.IsPathRooted(relative)
            ? null
            : relative.Replace('\\', '/');
    }

    private static SourceLocation GetLocation(SyntaxNode node, string relativePath)
    {
        var span = node.GetLocation().GetLineSpan();
        return new SourceLocation(relativePath, span.StartLinePosition.Line + 1, span.EndLinePosition.Line + 1);
    }

    private static string Truncate(string value, int length) =>
        value.Length <= length ? value : value[..length].TrimEnd() + "…";

    [GeneratedRegex(@"<(?:see|seealso)\s+(?:cref|langword)=""(?:[A-Z]:)?(?<name>[^""]+)""\s*/>")]
    private static partial Regex CrefRegex();

    [GeneratedRegex(@"<(?:paramref|typeparamref)\s+name=""(?<name>[^""]+)""\s*/>")]
    private static partial Regex ParamRefRegex();

    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex TagRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
