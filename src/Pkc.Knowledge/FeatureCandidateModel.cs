using Pkc.Core;

namespace Pkc.Knowledge;

public sealed record FeatureCandidateDocument(
    string SchemaVersion,
    IReadOnlyList<FeatureCandidate> Candidates);

public sealed record FeatureCandidate(
    string Id,
    string Name,
    string Area,
    string SeedFactId,
    IReadOnlyList<string> Coverage,
    IReadOnlyList<string> Unknowns,
    IReadOnlyList<EvidenceFact> Facts,
    IReadOnlyList<EvidenceRelation> Relations);
