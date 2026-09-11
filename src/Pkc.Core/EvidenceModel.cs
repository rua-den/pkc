namespace Pkc.Core;

public sealed record SourceLocation(
    string Path,
    int StartLine,
    int EndLine);

public sealed record EvidenceFact(
    string Id,
    string Kind,
    string Name,
    string? Container,
    SourceLocation Source,
    IReadOnlyList<string> Attributes,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record EvidenceRelation(
    string FromFactId,
    string Kind,
    string Target,
    SourceLocation Source);

public sealed record FactDocument(
    string SchemaVersion,
    IReadOnlyList<EvidenceFact> Facts,
    IReadOnlyList<EvidenceRelation> Relations);
