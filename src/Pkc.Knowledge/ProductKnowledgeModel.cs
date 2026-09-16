using Pkc.Core;

namespace Pkc.Knowledge;

public sealed record FeatureKnowledge(
    string Id,
    string Title,
    string Area,
    string Authority,
    IReadOnlyList<string> Coverage,
    string Summary,
    IReadOnlyList<string> UiSteps,
    IReadOnlyList<string> UiToBackend,
    IReadOnlyList<string> EntryPoints,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<string> Rules,
    IReadOnlyList<string> StateChanges,
    IReadOnlyList<string> SideEffects,
    IReadOnlyList<string> Flow,
    IReadOnlyList<string> Unknowns,
    IReadOnlyList<KnowledgeEvidence> Evidence)
{
    public IReadOnlyList<string> ValueLineage { get; init; } = [];
}

public sealed record KnowledgeEvidence(
    string FactId,
    string Kind,
    string Description,
    SourceLocation Source);

public interface IKnowledgeSynthesizer
{
    ValueTask<FeatureKnowledge> SynthesizeAsync(
        FeatureCandidate candidate,
        CancellationToken cancellationToken = default);
}
