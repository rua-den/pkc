using System.Text;
using System.Text.Json;

namespace Pkc.Knowledge;

public sealed class AiWorkspaceRenderer
{
    public const string ClaudeRelativePath = "CLAUDE.md";
    public const string AgentsRelativePath = "AGENTS.md";
    public const string StartHereRelativePath = "knowledge/START_HERE.md";
    public const string AnswerContractRelativePath = "_policy/answer-contract.md";
    public const string ManifestRelativePath = "_meta/manifest.json";
    public const string CatalogRelativePath = "_meta/catalog.json";

    public IReadOnlyDictionary<string, string> Render(
        IReadOnlyDictionary<string, string> canonicalKnowledgeFiles,
        string? sourceRepositoryLabel = null)
    {
        ArgumentNullException.ThrowIfNull(canonicalKnowledgeFiles);

        var files = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [ClaudeRelativePath] = RenderBootstrap("Claude Code"),
            [AgentsRelativePath] = RenderBootstrap("Codex-compatible agents"),
            [StartHereRelativePath] = RenderStartHere(sourceRepositoryLabel),
            [AnswerContractRelativePath] = RenderAnswerContract(),
            [ManifestRelativePath] = RenderManifest(canonicalKnowledgeFiles, sourceRepositoryLabel),
            [CatalogRelativePath] = RenderCatalog(canonicalKnowledgeFiles)
        };

        foreach (var pair in canonicalKnowledgeFiles
                     .Where(pair => pair.Key.StartsWith("knowledge/", StringComparison.Ordinal))
                     .OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            files[pair.Key] = pair.Value;
        }

        return files;
    }

    private static string RenderBootstrap(string consumer)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# PKC Product Workspace");
        builder.AppendLine();
        builder.AppendLine($"This generated workspace is the product/QA knowledge entry point for {consumer}.");
        builder.AppendLine("Do not start by reading the source repository or every generated file.");
        builder.AppendLine();
        builder.AppendLine("## Required bootstrap");
        builder.AppendLine();
        builder.AppendLine("1. Read `knowledge/START_HERE.md`.");
        builder.AppendLine("2. Read `_policy/answer-contract.md`.");
        builder.AppendLine("3. Use `_meta/catalog.json` to locate only the relevant feature/workflow documents.");
        builder.AppendLine("4. Answer from generated knowledge first. Do not inspect parent/source directories unless the user explicitly requests ENGINEERING/source analysis.");
        builder.AppendLine();
        builder.AppendLine("Default mode is PRODUCT: explain behavior, rules, permissions, validations, state changes, outcomes and uncertainty in business language.");
        builder.AppendLine("TRACE is allowed only when the user asks where/how a behavior is grounded; use evidence paths/endpoints without dumping source.");
        builder.AppendLine("ENGINEERING requires an explicit code/implementation request and source-enabled context. Never fabricate unavailable source.");
        builder.AppendLine();
        builder.AppendLine("Prefer an explicit unknown over an unsupported inference.");
        return builder.ToString();
    }

    private static string RenderStartHere(string? sourceRepositoryLabel)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Start here");
        builder.AppendLine();
        builder.AppendLine("This is a generated PKC product workspace. Use it to answer Product Owner and QA questions without manually selecting PKC files.");
        if (!string.IsNullOrWhiteSpace(sourceRepositoryLabel))
        {
            builder.AppendLine($"Source repository label: `{sourceRepositoryLabel}` (identifier only, not approved product naming).");
        }

        builder.AppendLine();
        builder.AppendLine("## Routing");
        builder.AppendLine();
        builder.AppendLine("- Start with `index.md` for the observed capability surface.");
        builder.AppendLine("- Capability/product questions: open the relevant document under `features/`.");
        builder.AppendLine("- Operation/validation/permission/state questions: open the linked document under `workflows/`.");
        builder.AppendLine("- Use `_meta/catalog.json` to find candidate files instead of loading the full workspace.");
        builder.AppendLine("- Read only the files needed for the current question.");
        builder.AppendLine();
        builder.AppendLine("## Answer boundary");
        builder.AppendLine();
        builder.AppendLine("- Treat generated behavior as code-observed implementation evidence, not approved intent unless a stronger authority is explicitly present.");
        builder.AppendLine("- Preserve `Important unknowns` and unsupported boundaries.");
        builder.AppendLine("- Do not infer UI behavior, side effects, requirements, delivery history or value lineage that is not grounded.");
        builder.AppendLine("- For ordinary Product/QA questions, do not inspect the source repository outside this workspace.");
        builder.AppendLine("- If the user explicitly asks for implementation/code, explain that ENGINEERING mode may require a source-enabled context.");
        builder.AppendLine();
        builder.AppendLine("## Workspace status");
        builder.AppendLine();
        builder.AppendLine("This first `pkc run` workspace is a PREVIEW productization slice. `pkc verify` / READY-PARTIAL-FAILED verification is not implemented yet.");
        return builder.ToString();
    }

    private static string RenderAnswerContract()
    {
        return """
# PKC answer contract

## PRODUCT — default

- Answer the user's product or QA question first.
- Use business language: behavior, conditions, permissions, validations, outcomes, failures, state transitions and visible behavior.
- Do not volunteer source code, raw facts, fact IDs, classes or method walkthroughs.
- Distinguish proven/observed behavior from unknown or unsupported behavior.
- Never infer behavior from names or conventions alone.

## QA lens

- Derive expected outcomes only from grounded behavior.
- Clearly label inferred test ideas separately from proven expected outcomes.
- Include negative/precondition cases when the knowledge proves them.

## TRACE — explicit request only

- May name endpoints, source paths and evidence references.
- Keep the answer focused on traceability; do not dump code.

## ENGINEERING — explicit request only

- Requires source-enabled context for code-level analysis.
- This portable workspace intentionally contains generated knowledge rather than source code.
- If source is unavailable, say so instead of fabricating implementation details.

## Uncertainty

- Prefer `unknown`, `not proven`, or a bounded partial answer over an unsupported claim.
- Never let frontend evidence upgrade weaker backend authority, or vice versa.
- Do not treat code-observed behavior as approved product intent unless explicit intent evidence exists.
""";
    }

    private static string RenderManifest(
        IReadOnlyDictionary<string, string> canonicalKnowledgeFiles,
        string? sourceRepositoryLabel)
    {
        var payload = new
        {
            schemaVersion = "0.1-preview",
            workspaceStatus = "PREVIEW",
            sourceRepositoryLabel,
            knowledgeFileCount = canonicalKnowledgeFiles.Count(pair =>
                pair.Key.StartsWith("knowledge/", StringComparison.Ordinal)),
            compatibilityArtifacts = new[]
            {
                PortableKnowledgePackRenderer.BundleFileName,
                PortableKnowledgePackRenderer.ArchiveFileName
            },
            verification = new
            {
                workspaceVerifyImplemented = false,
                note = "PREVIEW does not imply READY. pkc verify is a later checkpoint."
            }
        };

        return JsonSerializer.Serialize(payload, JsonOptions());
    }

    private static string RenderCatalog(IReadOnlyDictionary<string, string> canonicalKnowledgeFiles)
    {
        var entries = canonicalKnowledgeFiles
            .Where(pair => pair.Key.StartsWith("knowledge/", StringComparison.Ordinal))
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => new
            {
                path = pair.Key,
                kind = Classify(pair.Key)
            })
            .ToArray();

        return JsonSerializer.Serialize(
            new
            {
                schemaVersion = "0.1-preview",
                entries
            },
            JsonOptions());
    }

    private static string Classify(string path)
    {
        if (string.Equals(path, "knowledge/index.md", StringComparison.Ordinal)) return "index";
        if (string.Equals(path, PortableKnowledgePackRenderer.InstructionsRelativePath, StringComparison.Ordinal)) return "instructions";
        if (path.StartsWith("knowledge/features/", StringComparison.Ordinal)) return "feature";
        if (path.StartsWith("knowledge/workflows/", StringComparison.Ordinal)) return "workflow";
        return "knowledge";
    }

    private static JsonSerializerOptions JsonOptions() => new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}
