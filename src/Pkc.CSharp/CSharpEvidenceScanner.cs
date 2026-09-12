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
        var minimalApiRaw = await new MinimalApiEndpointScanner().ScanAsync(
            repositoryPath,
            cancellationToken);
        var minimalApi = await new MinimalApiContextEnricher().EnrichAsync(
            repositoryPath,
            minimalApiRaw,
            cancellationToken);

        var rawFacts = baseline.Facts
            .Concat(supplemental.Facts)
            .Concat(minimalApi.Facts)
            .Where(fact => !CSharpSourceScope.IsExcludedRelativePath(fact.Source.Path))
            .GroupBy(fact => fact.Id, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(fact => fact.Id, StringComparer.Ordinal)
            .ToArray();

        var rawFactIds = rawFacts
            .Select(fact => fact.Id)
            .ToHashSet(StringComparer.Ordinal);

        var rawRelations = baseline.Relations
            .Concat(supplemental.Relations)
            .Concat(minimalApi.Relations)
            .Where(relation => !CSharpSourceScope.IsExcludedRelativePath(relation.Source.Path))
            .Where(relation => rawFactIds.Contains(relation.FromFactId))
            .Where(relation => !relation.Target.StartsWith("cs:", StringComparison.Ordinal) || rawFactIds.Contains(relation.Target))
            .GroupBy(
                relation => $"{relation.FromFactId}|{relation.Kind}|{relation.Target}|{relation.Source.Path}|{relation.Source.StartLine}",
                StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
            .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
            .ThenBy(relation => relation.Target, StringComparer.Ordinal)
            .ToArray();

        var raw = new FactDocument("0.4.4-csharp-raw", rawFacts, rawRelations);
        var normalized = await new CSharpMutationContextEnricher().EnrichAsync(
            repositoryPath,
            raw,
            cancellationToken);
        return await new CSharpProjectSemanticEnricher().EnrichAsync(
            repositoryPath,
            normalized,
            cancellationToken);
    }
}
