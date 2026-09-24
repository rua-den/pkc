namespace Pkc.Core.Discovery;

/// <summary>Discovery output that exists, persisted, before any semantic scanner stage starts.</summary>
public sealed record DiscoveryState(
    RepositoryProfile Profile,
    string ProfilePath,
    ScanPlan Plan,
    string PlanPath);

/// <summary>An expensive semantic scanner invocation. It can only be started with an established discovery state.</summary>
public sealed record SemanticScanStage(
    string Name,
    Func<DiscoveryState, CancellationToken, Task<FactDocument>> ScanAsync);

public sealed record DiscoveryFirstScanResult(
    DiscoveryState State,
    IReadOnlyList<FactDocument> Documents)
{
    public RepositoryProfile Profile => State.Profile;
}

/// <summary>
/// Orchestrates <c>pkc</c> scanning so repository discovery and the scan plan are completed and persisted before
/// any semantic scanner stage begins. If discovery fails, no semantic stage runs.
/// </summary>
public sealed class DiscoveryFirstScanPipeline
{
    private readonly Action<DiscoveryState>? _onDiscovered;

    public DiscoveryFirstScanPipeline(Action<DiscoveryState>? onDiscovered = null)
    {
        _onDiscovered = onDiscovered;
    }

    public async Task<DiscoveryFirstScanResult> RunAsync(
        string repositoryPath,
        IReadOnlyList<SemanticScanStage> stages,
        CancellationToken cancellationToken = default)
    {
        var profile = new RepositoryDiscovery().Discover(repositoryPath, cancellationToken);
        var plan = ScanPlanner.Build(repositoryPath, profile);
        var (profilePath, planPath) = await RepositoryDiscoveryArtifacts.WriteAsync(repositoryPath, profile, plan, cancellationToken);
        var state = new DiscoveryState(profile, profilePath, plan, planPath);
        _onDiscovered?.Invoke(state);

        var documents = new List<FactDocument>(stages.Count);
        foreach (var stage in stages)
        {
            cancellationToken.ThrowIfCancellationRequested();
            documents.Add(await stage.ScanAsync(state, cancellationToken));
        }

        return new DiscoveryFirstScanResult(state, documents);
    }
}
