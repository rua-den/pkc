using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Operations;
using Pkc.Core;

namespace Pkc.CSharp;

internal sealed class CSharpComputationCausalityEnricher
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
                    var scopeFacts = facts.Values
                        .Where(fact => fact.Metadata.TryGetValue("scopeFactId", out var scopeFactId) &&
                                       string.Equals(scopeFactId, endpoint.Id, StringComparison.Ordinal))
                        .ToArray();
                    var extraction = ExtractEndpointEvidence(
                        endpoint,
                        projectPath,
                        method,
                        semanticModel,
                        scopeFacts,
                        cancellationToken);

                    foreach (var fact in extraction.Facts)
                    {
                        facts[fact.Id] = fact;
                    }

                    relations.AddRange(extraction.Relations);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                // B proof is optional and conservative. If exact target-project semantics
                // cannot be re-established, retain existing evidence without stronger claims.
            }
        }

        return document with
        {
            Facts = facts.Values.OrderBy(fact => fact.Id, StringComparer.Ordinal).ToArray(),
            Relations = relations
                .GroupBy(RelationKey, StringComparer.Ordinal)
                .Select(group => group.First())
                .OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
                .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
                .ThenBy(relation => relation.Target, StringComparer.Ordinal)
                .ToArray()
        };
    }

    private static ExtractionResult ExtractEndpointEvidence(
        EvidenceFact endpoint,
        string projectPath,
        MethodDeclarationSyntax method,
        SemanticModel semanticModel,
        IReadOnlyList<EvidenceFact> scopeFacts,
        CancellationToken cancellationToken)
    {
        var body = method.Body!;
        if (HasUnsupportedControlFlow(body) ||
            body.DescendantNodes().OfType<InvocationExpressionSyntax>().Any() ||
            method.ParameterList.Parameters
                .Select(parameter => semanticModel.GetDeclaredSymbol(parameter, cancellationToken))
                .OfType<IParameterSymbol>()
                .Any(parameter => parameter.Type.IsReferenceType) ||
            HasReferenceAmbiguity(body, semanticModel, cancellationToken) ||
            HasUnsupportedScalarWrite(body, semanticModel, projectPath, cancellationToken))
        {
            return ExtractionResult.Empty;
        }

        var knownFacts = scopeFacts.ToDictionary(fact => fact.Id, StringComparer.Ordinal);
        var currentValues = new Dictionary<string, ProvenValue>(StringComparer.Ordinal);
        var emittedFacts = new List<EvidenceFact>();
        var relations = new List<EvidenceRelation>();

        foreach (var statement in body.Statements)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (statement is ReturnStatementSyntax returnStatement)
            {
                if (returnStatement.Expression is not null &&
                    TryResolveSlot(
                        returnStatement.Expression,
                        semanticModel,
                        projectPath,
                        cancellationToken,
                        out var returnedSlot) &&
                    currentValues.TryGetValue(returnedSlot.Key, out var terminalValue))
                {
                    var terminal = CreateTerminalFact(
                        endpoint,
                        projectPath,
                        returnStatement,
                        returnedSlot,
                        terminalValue,
                        knownFacts);
                    emittedFacts.Add(terminal);
                    relations.Add(new EvidenceRelation(endpoint.Id, "mutates", terminal.Id, terminal.Source));
                    knownFacts[terminal.Id] = terminal;
                }

                continue;
            }

            if (statement is not ExpressionStatementSyntax expressionStatement ||
                expressionStatement.Expression is not AssignmentExpressionSyntax assignment ||
                !assignment.IsKind(SyntaxKind.SimpleAssignmentExpression) ||
                !TryResolveSlot(
                    assignment.Left,
                    semanticModel,
                    projectPath,
                    cancellationToken,
                    out var target) ||
                !IsSupportedStoredScalarAutoProperty(target.Property))
            {
                continue;
            }

            var directCopy = FindMatchingDirectCopy(endpoint, assignment, target, scopeFacts);
            if (directCopy is not null)
            {
                currentValues[target.Key] = new ProvenValue(
                    directCopy.Id,
                    directCopy.Metadata.GetValueOrDefault("mechanism") ?? "copy");
                knownFacts[directCopy.Id] = directCopy;
                continue;
            }

            if (TryResolveDerivation(
                    assignment.Right,
                    target,
                    semanticModel,
                    projectPath,
                    cancellationToken,
                    out var derivation))
            {
                var derivationFact = CreateDerivationFact(
                    endpoint,
                    projectPath,
                    assignment,
                    target,
                    derivation);
                emittedFacts.Add(derivationFact);
                relations.Add(new EvidenceRelation(endpoint.Id, "mutates", derivationFact.Id, derivationFact.Source));
                knownFacts[derivationFact.Id] = derivationFact;
                currentValues[target.Key] = new ProvenValue(derivationFact.Id, "derivation");
                continue;
            }

            if (currentValues.TryGetValue(target.Key, out var priorValue) &&
                semanticModel.GetConstantValue(assignment.Right, cancellationToken) is { HasValue: true })
            {
                var causalityFact = CreateOverrideFact(
                    endpoint,
                    projectPath,
                    assignment,
                    target,
                    priorValue,
                    knownFacts);
                emittedFacts.Add(causalityFact);
                relations.Add(new EvidenceRelation(endpoint.Id, "mutates", causalityFact.Id, causalityFact.Source));
                knownFacts[causalityFact.Id] = causalityFact;
                currentValues[target.Key] = new ProvenValue(causalityFact.Id, "override");
                continue;
            }

            currentValues.Remove(target.Key);
        }

        return new ExtractionResult(emittedFacts, relations);
    }

    private static EvidenceFact? FindMatchingDirectCopy(
        EvidenceFact endpoint,
        AssignmentExpressionSyntax assignment,
        MemberSlot target,
        IReadOnlyList<EvidenceFact> scopeFacts)
    {
        var sourceOccurrence = UnwrapParentheses(assignment.Right).ToString();
        var startLine = StartLine(assignment);
        return scopeFacts.FirstOrDefault(fact =>
            fact.Kind == "value-transfer" &&
            fact.Metadata.TryGetValue("scopeFactId", out var scopeFactId) &&
            string.Equals(scopeFactId, endpoint.Id, StringComparison.Ordinal) &&
            fact.Metadata.TryGetValue("mechanism", out var mechanism) &&
            string.Equals(mechanism, "copy", StringComparison.Ordinal) &&
            fact.Metadata.TryGetValue("targetOccurrence", out var targetOccurrence) &&
            string.Equals(targetOccurrence, target.Occurrence, StringComparison.Ordinal) &&
            fact.Metadata.TryGetValue("sourceOccurrence", out var observedSource) &&
            string.Equals(observedSource, sourceOccurrence, StringComparison.Ordinal) &&
            fact.Source.StartLine == startLine);
    }

    private static bool TryResolveDerivation(
        ExpressionSyntax expression,
        MemberSlot target,
        SemanticModel semanticModel,
        string projectPath,
        CancellationToken cancellationToken,
        out DerivationProof proof)
    {
        proof = default!;
        var operation = semanticModel.GetOperation(expression, cancellationToken);
        if (operation is null)
        {
            return false;
        }

        var inputs = new List<MemberSlot>();
        if (!TryCollectDerivationInputs(
                operation,
                semanticModel,
                projectPath,
                cancellationToken,
                inputs))
        {
            return false;
        }

        var distinctInputs = inputs.DistinctBy(input => input.Key, StringComparer.Ordinal).ToArray();
        if (distinctInputs.Length < 2 ||
            distinctInputs.Any(input =>
                !IsSupportedStoredScalarAutoProperty(input.Property) ||
                !SymbolEqualityComparer.Default.Equals(input.Property.Type, target.Property.Type)))
        {
            return false;
        }

        proof = new DerivationProof(expression.ToString(), distinctInputs);
        return true;
    }

    private static bool TryCollectDerivationInputs(
        IOperation operation,
        SemanticModel semanticModel,
        string projectPath,
        CancellationToken cancellationToken,
        ICollection<MemberSlot> inputs)
    {
        switch (operation)
        {
            case IParenthesizedOperation parenthesized:
                return TryCollectDerivationInputs(
                    parenthesized.Operand,
                    semanticModel,
                    projectPath,
                    cancellationToken,
                    inputs);

            case IConversionOperation conversion when conversion.OperatorMethod is null:
                return TryCollectDerivationInputs(
                    conversion.Operand,
                    semanticModel,
                    projectPath,
                    cancellationToken,
                    inputs);

            case IBinaryOperation binary when IsSupportedArithmetic(binary):
                return TryCollectDerivationInputs(
                           binary.LeftOperand,
                           semanticModel,
                           projectPath,
                           cancellationToken,
                           inputs) &&
                       TryCollectDerivationInputs(
                           binary.RightOperand,
                           semanticModel,
                           projectPath,
                           cancellationToken,
                           inputs);

            case IPropertyReferenceOperation propertyReference
                when propertyReference.Syntax is ExpressionSyntax propertyExpression &&
                     TryResolveSlot(
                         propertyExpression,
                         semanticModel,
                         projectPath,
                         cancellationToken,
                         out var input):
                inputs.Add(input);
                return true;

            case ILiteralOperation:
                return true;

            default:
                return false;
        }
    }

    private static bool IsSupportedArithmetic(IBinaryOperation operation)
    {
        if (operation.OperatorKind is not (
            BinaryOperatorKind.Add or
            BinaryOperatorKind.Subtract or
            BinaryOperatorKind.Multiply or
            BinaryOperatorKind.Divide or
            BinaryOperatorKind.Remainder))
        {
            return false;
        }

        return operation.OperatorMethod is null ||
               operation.OperatorMethod.ContainingType.SpecialType == SpecialType.System_Decimal;
    }

    private static EvidenceFact CreateDerivationFact(
        EvidenceFact endpoint,
        string projectPath,
        AssignmentExpressionSyntax assignment,
        MemberSlot target,
        DerivationProof derivation)
    {
        var location = GetLocation(assignment, endpoint.Source.Path);
        var id = $"cs-lineage:{endpoint.Source.Path}:{assignment.SpanStart}:derivation";
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["knowledgeClass"] = "value-lineage",
            ["scopeFactId"] = endpoint.Id,
            ["scopeName"] = endpoint.Name,
            ["semanticProject"] = projectPath,
            ["analysisMode"] = "project-semantic",
            ["analysisConfidence"] = "high",
            ["proof"] = "straight-line-project-semantic-scalar-arithmetic-derivation",
            ["mechanism"] = "derivation",
            ["temporalSemantics"] = "snapshot",
            ["compositionStatus"] = "derived-standalone",
            ["expression"] = derivation.Expression,
            ["inputOccurrences"] = string.Join(";", derivation.Inputs.Select(input => input.Occurrence)),
            ["inputMemberIdentities"] = string.Join(";", derivation.Inputs.Select(input => input.MemberIdentity)),
            ["targetOccurrence"] = target.Occurrence,
            ["targetReceiver"] = target.ReceiverDisplay,
            ["targetReceiverIdentity"] = target.ReceiverIdentity,
            ["targetMember"] = target.MemberDisplay,
            ["targetMemberIdentity"] = target.MemberIdentity
        };

        return new EvidenceFact(
            id,
            "value-transfer",
            $"{derivation.Expression} -> {target.Occurrence}",
            endpoint.Container,
            location,
            [],
            metadata);
    }

    private static EvidenceFact CreateOverrideFact(
        EvidenceFact endpoint,
        string projectPath,
        AssignmentExpressionSyntax assignment,
        MemberSlot target,
        ProvenValue priorValue,
        IReadOnlyDictionary<string, EvidenceFact> knownFacts)
    {
        var location = GetLocation(assignment, endpoint.Source.Path);
        var id = $"cs-causality:{endpoint.Source.Path}:{assignment.SpanStart}:override";
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["knowledgeClass"] = "mutation-causality",
            ["scopeFactId"] = endpoint.Id,
            ["scopeName"] = endpoint.Name,
            ["semanticProject"] = projectPath,
            ["analysisMode"] = "project-semantic",
            ["analysisConfidence"] = "high",
            ["proof"] = "straight-line-project-semantic-scalar-override",
            ["causalRole"] = "override",
            ["targetOccurrence"] = target.Occurrence,
            ["targetReceiverIdentity"] = target.ReceiverIdentity,
            ["targetMemberIdentity"] = target.MemberIdentity,
            ["valueExpression"] = assignment.Right.ToString(),
            ["priorValueFactId"] = priorValue.FactId,
            ["priorMechanism"] = priorValue.Mechanism
        };

        if (knownFacts.TryGetValue(priorValue.FactId, out var priorFact))
        {
            if (priorFact.Metadata.TryGetValue("sourceOccurrence", out var sourceOccurrence))
            {
                metadata["priorSourceOccurrence"] = sourceOccurrence;
            }
            else if (priorFact.Metadata.TryGetValue("inputOccurrences", out var inputOccurrences))
            {
                metadata["priorSourceOccurrence"] = inputOccurrences;
            }
        }

        return new EvidenceFact(
            id,
            "value-causality",
            $"override {target.Occurrence}",
            endpoint.Container,
            location,
            [],
            metadata);
    }

    private static EvidenceFact CreateTerminalFact(
        EvidenceFact endpoint,
        string projectPath,
        ReturnStatementSyntax returnStatement,
        MemberSlot returnedSlot,
        ProvenValue terminalValue,
        IReadOnlyDictionary<string, EvidenceFact> knownFacts)
    {
        var location = GetLocation(returnStatement, endpoint.Source.Path);
        var id = $"cs-lineage:{endpoint.Source.Path}:{returnStatement.SpanStart}:terminal-source";
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["knowledgeClass"] = "value-lineage",
            ["scopeFactId"] = endpoint.Id,
            ["scopeName"] = endpoint.Name,
            ["semanticProject"] = projectPath,
            ["analysisMode"] = "project-semantic",
            ["analysisConfidence"] = "high",
            ["proof"] = "straight-line-project-semantic-return-terminal-source",
            ["boundary"] = "return",
            ["returnedOccurrence"] = returnedSlot.Occurrence,
            ["returnedMemberIdentity"] = returnedSlot.MemberIdentity,
            ["sourceFactId"] = terminalValue.FactId,
            ["sourceMechanism"] = terminalValue.Mechanism
        };

        if (knownFacts.TryGetValue(terminalValue.FactId, out var sourceFact))
        {
            metadata["sourceFactLocation"] = $"{sourceFact.Source.Path}:L{sourceFact.Source.StartLine}";
            if (sourceFact.Metadata.TryGetValue("sourceOccurrence", out var sourceOccurrence))
            {
                metadata["sourceOccurrence"] = sourceOccurrence;
            }
            else if (sourceFact.Metadata.TryGetValue("expression", out var expression))
            {
                metadata["sourceOccurrence"] = expression;
            }
            else if (sourceFact.Metadata.TryGetValue("valueExpression", out var valueExpression))
            {
                metadata["sourceOccurrence"] = valueExpression;
            }
        }

        return new EvidenceFact(
            id,
            "value-terminal-source",
            $"last source before return: {returnedSlot.Occurrence}",
            endpoint.Container,
            location,
            [],
            metadata);
    }

    private static bool HasReferenceAmbiguity(
        BlockSyntax body,
        SemanticModel model,
        CancellationToken cancellationToken)
    {
        foreach (var variable in body.DescendantNodes().OfType<VariableDeclaratorSyntax>())
        {
            if (model.GetDeclaredSymbol(variable, cancellationToken) is not ILocalSymbol local ||
                !local.Type.IsReferenceType ||
                variable.Initializer is null)
            {
                continue;
            }

            var contextualConversion = model.GetConversion(variable.Initializer.Value, cancellationToken);
            if (contextualConversion.IsUserDefined ||
                !IsFreshReferenceAllocation(model.GetOperation(variable.Initializer.Value, cancellationToken)))
            {
                return true;
            }
        }

        return body.DescendantNodes()
            .OfType<AssignmentExpressionSyntax>()
            .Any(assignment =>
            {
                var symbol = model.GetSymbolInfo(assignment.Left, cancellationToken).Symbol;
                return symbol is ILocalSymbol or IParameterSymbol &&
                       model.GetTypeInfo(assignment.Left, cancellationToken).Type is { IsReferenceType: true };
            });
    }

    private static bool HasUnsupportedScalarWrite(
        BlockSyntax body,
        SemanticModel model,
        string projectPath,
        CancellationToken cancellationToken)
    {
        foreach (var assignment in body.DescendantNodes().OfType<AssignmentExpressionSyntax>())
        {
            if (assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
            {
                continue;
            }

            if (TryResolveSlot(assignment.Left, model, projectPath, cancellationToken, out var slot) &&
                IsSupportedStoredScalarAutoProperty(slot.Property))
            {
                return true;
            }
        }

        foreach (var expression in body.DescendantNodes().OfType<ExpressionSyntax>())
        {
            ExpressionSyntax? operand = expression switch
            {
                PrefixUnaryExpressionSyntax prefix
                    when prefix.IsKind(SyntaxKind.PreIncrementExpression) ||
                         prefix.IsKind(SyntaxKind.PreDecrementExpression) => prefix.Operand,
                PostfixUnaryExpressionSyntax postfix
                    when postfix.IsKind(SyntaxKind.PostIncrementExpression) ||
                         postfix.IsKind(SyntaxKind.PostDecrementExpression) => postfix.Operand,
                _ => null
            };

            if (operand is not null &&
                TryResolveSlot(operand, model, projectPath, cancellationToken, out var slot) &&
                IsSupportedStoredScalarAutoProperty(slot.Property))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsFreshReferenceAllocation(IOperation? operation)
    {
        while (operation is IParenthesizedOperation or IConversionOperation)
        {
            if (operation is IConversionOperation conversion && conversion.OperatorMethod is not null)
            {
                return false;
            }

            operation = operation switch
            {
                IParenthesizedOperation parenthesized => parenthesized.Operand,
                IConversionOperation conversion => conversion.Operand,
                _ => operation
            };
        }

        return operation is IObjectCreationOperation;
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

    private sealed record DerivationProof(string Expression, IReadOnlyList<MemberSlot> Inputs);

    private sealed record ProvenValue(string FactId, string Mechanism);

    private sealed record ExtractionResult(
        IReadOnlyList<EvidenceFact> Facts,
        IReadOnlyList<EvidenceRelation> Relations)
    {
        public static ExtractionResult Empty { get; } = new([], []);
    }
}
