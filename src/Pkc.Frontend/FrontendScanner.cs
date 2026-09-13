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

            var document = await adapter.ScanAsync(rootPath, cancellationToken);
            documents.Add(EnsureAnalyzerMetadata(ApplyProductSourceScope(document), adapter.Id));
        }

        var merged = Merge(documents);
        return FrontendRelationLinker.Link(merged);
    }

    private static IEnumerable<IFrontendAdapter> DefaultAdapters()
    {
        yield return new ReactFrontendAdapter();
        yield return new AngularFrontendAdapter();
    }

    private static FactDocument ApplyProductSourceScope(FactDocument document)
    {
        var facts = document.Facts
            .Where(fact => FrontendSourceScope.IsProductSource(fact.Source.Path))
            .ToArray();
        var factIds = facts.Select(fact => fact.Id).ToHashSet(StringComparer.Ordinal);
        var relations = document.Relations
            .Where(relation => factIds.Contains(relation.FromFactId))
            .Where(relation => FrontendSourceScope.IsProductSource(relation.Source.Path))
            .ToArray();

        return document with
        {
            Facts = facts,
            Relations = relations
        };
    }

    private static FactDocument EnsureAnalyzerMetadata(FactDocument document, string adapterId)
    {
        var facts = document.Facts.Select(fact =>
        {
            if (fact.Metadata.ContainsKey("analysisMode"))
            {
                return fact;
            }

            var metadata = new Dictionary<string, string>(fact.Metadata, StringComparer.Ordinal);
            if (string.Equals(adapterId, "react-static", StringComparison.Ordinal))
            {
                metadata["analysisMode"] = "regex-fallback";
                metadata["analysisConfidence"] = "low";
                metadata["analysisFallbackReason"] = "react-adapter-not-yet-ast-backed";
            }
            else
            {
                metadata["analysisMode"] = "static-adapter";
                metadata["analysisConfidence"] = "medium";
            }

            return fact with { Metadata = metadata };
        }).ToArray();

        return document with { Facts = facts };
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

        return new FactDocument("0.4.3-frontend", facts, relations);
    }
}
