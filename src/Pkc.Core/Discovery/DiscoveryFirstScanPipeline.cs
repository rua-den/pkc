namespace Pkc.Core.Discovery;

/// <summary>An expensive semantic scanner invocation. It can only be started with an established profile.</summary>
public sealed record SemanticScanStage(
    string Name,
    Func<RepositoryProfile, CancellationToken, Task<FactDocument>> ScanAsync);

public sealed record DiscoveryFirstScanResult(
    RepositoryProfile Profile,
    string ProfilePath,
    IReadOnlyList<FactDocument> Documents);

/// <summary>
/// Orchestrates <c>pkc</c> scanning so repository discovery is completed and persisted before any
/// semantic scanner stage begins. If discovery fails, no semantic stage runs.
/// </summary>
public sealed class DiscoveryFirstScanPipeline
{
    private readonly Action<RepositoryProfile, string>? _onDiscovered;

    public DiscoveryFirstScanPipeline(Action<RepositoryProfile, string>? onDiscovered = null)
    {
        _onDiscovered = onDiscovered;
    }

    public async Task<DiscoveryFirstScanResult> RunAsync(
        string repositoryPath,
        IReadOnlyList<SemanticScanStage> stages,
        CancellationToken cancellationToken = default)
    {
        var profile = new RepositoryDiscovery().Discover(repositoryPath, cancellationToken);
        var profilePath = await RepositoryDiscoveryArtifacts.WriteAsync(repositoryPath, profile, cancellationToken);
        _onDiscovered?.Invoke(profile, profilePath);

        var documents = new List<FactDocument>(stages.Count);
        foreach (var stage in stages)
        {
            cancellationToken.ThrowIfCancellationRequested();
            documents.Add(await stage.ScanAsync(profile, cancellationToken));
        }

        return new DiscoveryFirstScanResult(profile, profilePath, documents);
    }
}
