using Pkc.Core;

namespace Pkc.Frontend;

public sealed class AngularFrontendAdapter : IFrontendAdapter
{
    private static readonly HashSet<string> UiBehaviorKinds = new(StringComparer.Ordinal)
    {
        "ui-field",
        "ui-field-option",
        "ui-field-validation",
        "ui-field-visibility",
        "ui-field-enabled-state",
        "ui-field-binding"
    };

    private readonly AngularRepositoryScanner _regexFallback = new();
    private readonly AngularTypeScriptAstScanner _astScanner = new();
    private readonly AngularFormBehaviorScanner _formBehaviorScanner = new();

    public string Id => "angular";

    public bool CanHandle(string repositoryPath) => _regexFallback.CanHandle(repositoryPath);

    public async Task<FactDocument> ScanAsync(
        string repositoryPath,
        CancellationToken cancellationToken = default)
    {
        var astAttempt = await _astScanner.TryScanAsync(repositoryPath, cancellationToken);
        var fallbackDocument = await _regexFallback.ScanAsync(repositoryPath, cancellationToken);
        var formBehaviorDocument = await _formBehaviorScanner.ScanAsync(repositoryPath, cancellationToken);

        if (astAttempt.Document is null)
        {
            var fallback = TagDocument(
                fallbackDocument,
                "regex-fallback",
                "low",
                astAttempt.FailureReason ?? "typescript-ast-unavailable");
            return Merge(fallback, formBehaviorDocument, "0.4.4-angular");
        }

        var astDocument = astAttempt.Document;
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

        return Merge(astDocument, templateDocument, formBehaviorDocument, "0.4.4-angular");
    }

    private static FactDocument Merge(
        FactDocument first,
        FactDocument second,
        string schemaVersion) =>
        Merge(first, second, null, schemaVersion);

    private static FactDocument Merge(
        FactDocument first,
        FactDocument second,
        FactDocument? third,
        string schemaVersion)
    {
        var documents = third is null ? [first, second] : new[] { first, second, third };

        var facts = documents
            .SelectMany(document => document.Facts)
            .Where(fact => third is null || documentAllowsFact(fact))
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

        static bool documentAllowsFact(EvidenceFact fact) =>
            fact.Kind == "ui-action" ||
            fact.Kind is "ui-screen" or "ui-route" or "ui-api-call" ||
            UiBehaviorKinds.Contains(fact.Kind);
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
