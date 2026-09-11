using Pkc.Core;

namespace Pkc.Frontend;

public sealed class AngularFrontendAdapter : IFrontendAdapter
{
    private readonly AngularRepositoryScanner _regexFallback = new();
    private readonly AngularTypeScriptAstScanner _astScanner = new();

    public string Id => "angular";

    public bool CanHandle(string repositoryPath) => _regexFallback.CanHandle(repositoryPath);

    public async Task<FactDocument> ScanAsync(
        string repositoryPath,
        CancellationToken cancellationToken = default)
    {
        var astAttempt = await _astScanner.TryScanAsync(repositoryPath, cancellationToken);
        var fallbackDocument = await _regexFallback.ScanAsync(repositoryPath, cancellationToken);

        if (astAttempt.Document is null)
        {
            return TagDocument(
                fallbackDocument,
                "regex-fallback",
                "low",
                astAttempt.FailureReason ?? "typescript-ast-unavailable");
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

        var facts = astDocument.Facts
            .Concat(templateFacts)
            .GroupBy(fact => fact.Id, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(fact => fact.Id, StringComparer.Ordinal)
            .ToArray();

        var relations = astDocument.Relations
            .Concat(templateRelations)
            .GroupBy(
                relation => $"{relation.FromFactId}|{relation.Kind}|{relation.Target}|{relation.Source.Path}|{relation.Source.StartLine}",
                StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
            .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
            .ThenBy(relation => relation.Target, StringComparer.Ordinal)
            .ToArray();

        return new FactDocument("0.4.3-angular", facts, relations);
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
