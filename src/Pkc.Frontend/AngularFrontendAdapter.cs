using Pkc.Core;

namespace Pkc.Frontend;

public sealed class AngularFrontendAdapter : IFrontendAdapter
{
    private readonly AngularRepositoryScanner _regexFallback = new();
    private readonly AngularTypeScriptAstScanner _astScanner = new();
    private readonly AngularUrlExpressionEnricher _urlExpressionEnricher = new();
    private readonly AngularOutputEventBridgeEnricher _outputEventBridgeEnricher = new();
    private readonly AngularFormBehaviorScanner _formBehaviorScanner = new();
    private readonly AngularListBehaviorScanner _listBehaviorScanner = new();
    private readonly AngularResponseBindingScanner _responseBindingScanner = new();
    private readonly AngularServiceIdentityEnricher _serviceIdentityEnricher = new();
    private readonly AngularRenderedMemberAuthorityFilter _renderedMemberAuthorityFilter = new();
    private readonly AngularHtmlDirectTextAuthorityFilter _htmlDirectTextAuthorityFilter = new();
    private readonly AngularComponentProjectionAuthorityFilter _componentProjectionAuthorityFilter = new();
    private readonly AngularUnresolvedExternalComponentImportAuthorityFilter _unresolvedExternalComponentImportAuthorityFilter = new();
    private readonly AngularComponentImportClosureAuthorityFilter _componentImportClosureAuthorityFilter = new();
    private readonly AngularUnsupportedComponentImportIndirectionAuthorityFilter _unsupportedComponentImportIndirectionAuthorityFilter = new();
    private readonly AngularSvgRenderedTextAuthorityFilter _svgRenderedTextAuthorityFilter = new();
    private readonly AngularSvgTextContentAuthorityFilter _svgTextContentAuthorityFilter = new();
    private readonly AngularRenderedMemberVisibilityEnricher _renderedMemberVisibilityEnricher = new();
    private readonly AngularRenderedMemberVisibilityAuthorityFilter _renderedMemberVisibilityAuthorityFilter = new();

    public string Id => "angular";

    public bool CanHandle(string repositoryPath) => _regexFallback.CanHandle(repositoryPath);

    public async Task<FactDocument> ScanAsync(
        string repositoryPath,
        CancellationToken cancellationToken = default)
    {
        var astAttempt = await _astScanner.TryScanAsync(repositoryPath, cancellationToken);
        var fallbackDocument = await _regexFallback.ScanAsync(repositoryPath, cancellationToken);
        var formBehaviorDocument = await _formBehaviorScanner.ScanAsync(repositoryPath, cancellationToken);
        var listBehaviorDocument = await _listBehaviorScanner.ScanAsync(repositoryPath, cancellationToken);
        var responseBindingDocument = await _responseBindingScanner.ScanAsync(repositoryPath, cancellationToken);

        FactDocument merged;
        if (astAttempt.Document is null)
        {
            var fallback = TagDocument(
                fallbackDocument,
                "regex-fallback",
                "low",
                astAttempt.FailureReason ?? "typescript-ast-unavailable");
            merged = Merge(
                [fallback, formBehaviorDocument, listBehaviorDocument, responseBindingDocument],
                "0.4.6-angular");
        }
        else
        {
            var templateFacts = fallbackDocument.Facts
                .Where(fact => fact.Kind == "ui-action")
                .Select(fact => TagFact(
                    fact,
                    "angular-template-regex-fallback",
                    "medium",
                    "angular-template-actions-not-yet-ast-backed"))
                .ToArray();

            var templateFactIds = templateFacts.Select(fact => fact.Id).ToHashSet(StringComparer.Ordinal);
            var templateRelations = fallbackDocument.Relations
                .Where(relation => templateFactIds.Contains(relation.FromFactId))
                .ToArray();

            var templateDocument = new FactDocument(
                "0.4.4-angular-template",
                templateFacts,
                templateRelations);

            merged = Merge(
                [astAttempt.Document, templateDocument, formBehaviorDocument, listBehaviorDocument, responseBindingDocument],
                "0.4.6-angular");
        }

        var urlResolved = await _urlExpressionEnricher.EnrichAsync(
            repositoryPath,
            merged,
            cancellationToken);
        var identified = await _serviceIdentityEnricher.EnrichAsync(
            repositoryPath,
            urlResolved,
            cancellationToken);
        var eventBridged = await _outputEventBridgeEnricher.EnrichAsync(
            repositoryPath,
            identified,
            cancellationToken);
        var authoritativeRenders = await _renderedMemberAuthorityFilter.FilterAsync(
            repositoryPath,
            eventBridged,
            cancellationToken);
        var htmlDirectTextAuthoritativeRenders = await _htmlDirectTextAuthorityFilter.FilterAsync(
            repositoryPath,
            authoritativeRenders,
            cancellationToken);
        var projectionAuthoritativeRenders = await _componentProjectionAuthorityFilter.FilterAsync(
            repositoryPath,
            htmlDirectTextAuthoritativeRenders,
            cancellationToken);
        var resolvableExternalComponentRenders = await _unresolvedExternalComponentImportAuthorityFilter.FilterAsync(
            repositoryPath,
            projectionAuthoritativeRenders,
            cancellationToken);
        var closedComponentImportRenders = await _componentImportClosureAuthorityFilter.FilterAsync(
            repositoryPath,
            resolvableExternalComponentRenders,
            cancellationToken);
        var supportedComponentImportRenders = await _unsupportedComponentImportIndirectionAuthorityFilter.FilterAsync(
            repositoryPath,
            closedComponentImportRenders,
            cancellationToken);
        var svgAuthoritativeRenders = await _svgRenderedTextAuthorityFilter.FilterAsync(
            repositoryPath,
            supportedComponentImportRenders,
            cancellationToken);
        var svgTextContentAuthoritativeRenders = await _svgTextContentAuthorityFilter.FilterAsync(
            repositoryPath,
            svgAuthoritativeRenders,
            cancellationToken);
        var renderedVisibility = await _renderedMemberVisibilityEnricher.EnrichAsync(
            repositoryPath,
            svgTextContentAuthoritativeRenders,
            cancellationToken);
        return await _renderedMemberVisibilityAuthorityFilter.FilterAsync(
            repositoryPath,
            renderedVisibility,
            cancellationToken);
    }

    private static FactDocument Merge(
        IReadOnlyList<FactDocument> documents,
        string schemaVersion)
    {
        var facts = documents
            .SelectMany(document => document.Facts)
            .GroupBy(fact => fact.Id, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(fact => fact.Id, StringComparer.Ordinal)
            .ToArray();

        var relations = documents
            .SelectMany(document => document.Relations)
            .GroupBy(
                relation => $"{relation.FromFactId}|{relation.Kind}|{relation.Target}|{relation.Source.Path}|{relation.Source.StartLine}",
                StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
            .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
            .ThenBy(relation => relation.Target, StringComparer.Ordinal)
            .ToArray();

        return new FactDocument(schemaVersion, facts, relations);
    }

    private static FactDocument TagDocument(
        FactDocument document,
        string mode,
        string confidence,
        string reason)
    {
        var facts = document.Facts
            .Select(fact => TagFact(fact, mode, confidence, reason))
            .ToArray();
        return document with { Facts = facts };
    }

    private static EvidenceFact TagFact(
        EvidenceFact fact,
        string mode,
        string confidence,
        string reason)
    {
        var metadata = new Dictionary<string, string>(fact.Metadata, StringComparer.Ordinal)
        {
            ["analysisMode"] = mode,
            ["analysisConfidence"] = confidence,
            ["analysisFallbackReason"] = reason
        };
        return fact with { Metadata = metadata };
    }
}
