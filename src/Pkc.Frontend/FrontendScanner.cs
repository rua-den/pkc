using Pkc.Core;

namespace Pkc.Frontend;

public sealed class FrontendScanner
{
    private readonly IReadOnlyList<IFrontendAdapter> _adapters;

    public FrontendScanner(IEnumerable<IFrontendAdapter>? adapters = null)
    {
        _adapters = (adapters ?? DefaultAdapters()).ToArray();
    }

    public async Task<FactDocument> ScanAsync(
        string repositoryPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryPath);

        var rootPath = Path.GetFullPath(repositoryPath);
        if (!Directory.Exists(rootPath))
        {
            throw new DirectoryNotFoundException($"Repository path does not exist: {rootPath}");
        }

        var documents = new List<FactDocument>();
        foreach (var adapter in _adapters)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!adapter.CanHandle(rootPath))
            {
                continue;
            }

            documents.Add(await adapter.ScanAsync(rootPath, cancellationToken));
        }

        var merged = Merge(documents);
        return FrontendRelationLinker.Link(merged);
    }

    private static IEnumerable<IFrontendAdapter> DefaultAdapters()
    {
        yield return new ReactFrontendAdapter();
        yield return new AngularRepositoryScanner();
    }

    private static FactDocument Merge(IReadOnlyList<FactDocument> documents)
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

        return new FactDocument("0.4.1-frontend", facts, relations);
    }
}
