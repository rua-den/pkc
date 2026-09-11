using Pkc.Core;

namespace Pkc.CSharp;

public sealed class CSharpEvidenceScanner
{
    public async Task<FactDocument> ScanAsync(
        string repositoryPath,
        CancellationToken cancellationToken = default)
    {
        var baseline = await new CSharpRepositoryScanner().ScanAsync(repositoryPath, cancellationToken);
        var supplemental = await new CSharpSupplementalScanner().ScanAsync(
            repositoryPath,
            baseline,
            cancellationToken);

        var rawFacts = baseline.Facts
            .Concat(supplemental.Facts)
            .GroupBy(fact => fact.Id, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(fact => fact.Id, StringComparer.Ordinal)
            .ToArray();

        var rawRelations = baseline.Relations
            .Concat(supplemental.Relations)
            .GroupBy(
                relation => $"{relation.FromFactId}|{relation.Kind}|{relation.Target}|{relation.Source.Path}|{relation.Source.StartLine}",
                StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
            .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
            .ThenBy(relation => relation.Target, StringComparer.Ordinal)
            .ToArray();

        var raw = new FactDocument("0.4.3-csharp-raw", rawFacts, rawRelations);
        return await new CSharpProjectSemanticEnricher().EnrichAsync(
            repositoryPath,
            raw,
            cancellationToken);
    }
}
