using Pkc.Core;

namespace Pkc.CSharp;

public sealed class CSharpEvidenceScanner
{
    public async Task<FactDocument> ScanAsync(
        string repositoryPath,
        CancellationToken cancellationToken = default)
    {
        var baselineRaw = await new CSharpRepositoryScanner().ScanAsync(repositoryPath, cancellationToken);
        var baseline = new CSharpMvcControllerRouteEnricher().Enrich(baselineRaw);
        var supplemental = await new CSharpSupplementalScanner().ScanAsync(
            repositoryPath,
            baseline,
            cancellationToken);
        var validation = await new CSharpValidationEvidenceScanner().ScanAsync(
            repositoryPath,
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
            .Concat(validation.Facts)
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
            .Concat(validation.Relations)
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
        var semantic = await new CSharpProjectSemanticEnricher().EnrichAsync(
            repositoryPath,
            normalized,
            cancellationToken);
        var snapshotLineage = await new CSharpValueLineageEnricher().EnrichAsync(
            repositoryPath,
            semantic,
            cancellationToken);
        var dynamicLineage = await new CSharpDynamicValueLineageEnricher().EnrichAsync(
            repositoryPath,
            snapshotLineage,
            cancellationToken);
        var lineage = await new CSharpComputationCausalityEnricher().EnrichAsync(
            repositoryPath,
            dynamicLineage,
            cancellationToken);
        var apiProjectionLineage = await new CSharpApiProjectionLineageEnricher().EnrichAsync(
            repositoryPath,
            lineage,
            cancellationToken);
        var predicates = await new CSharpBusinessPredicateEnricher().EnrichAsync(
            repositoryPath,
            apiProjectionLineage,
            cancellationToken);
        return await new CSharpBusinessPredicateAuthorityFilter().FilterAsync(
            repositoryPath,
            predicates,
            cancellationToken);
    }
}
