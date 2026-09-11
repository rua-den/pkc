namespace Pkc.Knowledge;

public sealed record ProductFeatureDocument(
    string SchemaVersion,
    IReadOnlyList<ProductFeature> Features);

public sealed record ProductFeature(
    string Id,
    string Title,
    string Area,
    string Category,
    string Authority,
    IReadOnlyList<string> Coverage,
    string Summary,
    IReadOnlyList<ProductWorkflowReference> Workflows,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<string> Rules,
    IReadOnlyList<string> Unknowns);

public sealed record ProductWorkflowReference(
    string Id,
    string Title,
    string MarkdownPath,
    IReadOnlyList<string> Coverage,
    IReadOnlyList<string> UiSteps,
    IReadOnlyList<string> EntryPoints,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<string> Rules,
    IReadOnlyList<string> StateChanges,
    IReadOnlyList<string> SideEffects,
    IReadOnlyList<string> Unknowns);
