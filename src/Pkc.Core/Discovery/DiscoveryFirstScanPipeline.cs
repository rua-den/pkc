namespace Pkc.Core.Discovery;

/// <summary>Discovery output that exists, persisted, before any semantic scanner stage starts.</summary>
public sealed record DiscoveryState(
    RepositoryProfile Profile,
    string ProfilePath,
    ScanPlan Plan,
    string PlanPath)
{
    /// <summary>Plan-derived boundary made ambient while semantic stages run.</summary>
    public required SemanticSourceScope Scope { get; init; }
}

/// <summary>An expensive semantic scanner invocation. It can only be started with an established discovery state.</summary>
public sealed record SemanticScanStage(
    string Name,
    Func<DiscoveryState, CancellationToken, Task<FactDocument>> ScanAsync)
{
    /// <summary>Plan scanner id (for example <see cref="ScanPlanner.CSharpScanner"/>) whose semantic scopes this stage executes.</summary>
    public string? Scanner { get; init; }

    /// <summary>The scanner's own accepted source scope; planned files outside it are reported, never silently dropped.</summary>
    public Func<string, bool>? InScannerSourceScope { get; init; }
}

/// <summary>What a stage was planned to analyze and which planned files its accepted source scope still withholds.</summary>
public sealed record SemanticStageExecution(
    string Name,
    string? Scanner,
    bool Scoped,
    int PlannedFiles,
    int ExecutableFiles,
    IReadOnlyList<string> WithheldByScannerScope);

public sealed record DiscoveryFirstScanResult(
    DiscoveryState State,
    IReadOnlyList<FactDocument> Documents)
{
    public RepositoryProfile Profile => State.Profile;

    public IReadOnlyList<SemanticStageExecution> Executions { get; init; } = [];
}

/// <summary>
/// Orchestrates <c>pkc</c> scanning so repository discovery and the scan plan are completed and persisted before
/// any semantic scanner stage begins. With scoped execution, stages run inside the plan-derived
/// <see cref="SemanticSourceScope"/>, so safely excluded, generated/light-indexed, runtime-dependency and
/// test-evidence files never reach deep semantic scanners. If discovery fails, no semantic stage runs.
/// </summary>
public sealed class DiscoveryFirstScanPipeline
{
    private readonly Action<DiscoveryState>? _onDiscovered;
    private readonly Action<SemanticStageExecution>? _onStageStarting;
    private readonly bool _scoped;

    public DiscoveryFirstScanPipeline(
        Action<DiscoveryState>? onDiscovered = null,
        Action<SemanticStageExecution>? onStageStarting = null,
        bool scoped = true)
    {
        _onDiscovered = onDiscovered;
        _onStageStarting = onStageStarting;
        _scoped = scoped;
    }

    public async Task<DiscoveryFirstScanResult> RunAsync(
        string repositoryPath,
        IReadOnlyList<SemanticScanStage> stages,
        CancellationToken cancellationToken = default)
    {
        var profile = new RepositoryDiscovery().Discover(repositoryPath, cancellationToken);
        var plan = ScanPlanner.Build(repositoryPath, profile);
        var (profilePath, planPath) = await RepositoryDiscoveryArtifacts.WriteAsync(repositoryPath, profile, plan, cancellationToken);
        var state = new DiscoveryState(profile, profilePath, plan, planPath)
        {
            Scope = SemanticSourceScope.FromProfile(repositoryPath, profile)
        };
        _onDiscovered?.Invoke(state);

        var documents = new List<FactDocument>(stages.Count);
        var executions = new List<SemanticStageExecution>(stages.Count);
        foreach (var stage in stages)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var execution = Execution(plan, stage, _scoped);
            executions.Add(execution);
            _onStageStarting?.Invoke(execution);

            using (_scoped ? state.Scope.Enter() : null)
            {
                documents.Add(await stage.ScanAsync(state, cancellationToken));
            }
        }

        return new DiscoveryFirstScanResult(state, documents) { Executions = executions };
    }

    private static SemanticStageExecution Execution(ScanPlan plan, SemanticScanStage stage, bool scoped)
    {
        var planned = stage.Scanner is null
            ? []
            : plan.Scopes
                .Where(scope => scope.Coverage == PlanCoverage.Semantic && scope.Scanners.Contains(stage.Scanner))
                .SelectMany(scope => scope.Files)
                .OrderBy(file => file, StringComparer.Ordinal)
                .ToArray();
        var withheld = stage.InScannerSourceScope is null
            ? []
            : planned.Where(file => !stage.InScannerSourceScope(file)).ToArray();
        return new SemanticStageExecution(stage.Name, stage.Scanner, scoped, planned.Length, planned.Length - withheld.Length, withheld);
    }
}
