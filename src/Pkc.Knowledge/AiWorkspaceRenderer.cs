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
    public const string CoverageRelativePath = "_meta/coverage.json";

    public IReadOnlyDictionary<string, string> Render(
        IReadOnlyDictionary<string, string> canonicalKnowledgeFiles,
        string? sourceRepositoryLabel = null,
        WorkspaceOverview? overview = null)
    {
        ArgumentNullException.ThrowIfNull(canonicalKnowledgeFiles);

        var files = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [ClaudeRelativePath] = RenderBootstrap("Claude Code"),
            [AgentsRelativePath] = RenderBootstrap("Codex-compatible agents"),
            [StartHereRelativePath] = RenderStartHere(sourceRepositoryLabel, overview),
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
        builder.AppendLine("4. Answer from generated knowledge first. Do not inspect parent/source directories unless the user explicitly requests ENGINEERING/source analysis or an approved benchmark source cross-check.");
        builder.AppendLine();
        builder.AppendLine("Default mode is PRODUCT: explain behavior, rules, permissions, validations, state changes, outcomes and uncertainty in business language.");
        builder.AppendLine("TRACE is allowed only when the user asks where/how a behavior is grounded; use evidence paths/endpoints without dumping source.");
        builder.AppendLine("ENGINEERING requires an explicit code/implementation request and source-enabled context. Never fabricate unavailable source.");
        builder.AppendLine();
        builder.AppendLine("## Source/privacy boundary");
        builder.AppendLine();
        builder.AppendLine("- This workspace is intentionally source-free product context. Do not copy source-code bodies, secrets, credentials, raw facts, or proprietary file contents into PRODUCT/QA answers or benchmark reports.");
        builder.AppendLine("- Ordinary PRODUCT/QA usage stays inside this workspace and must not climb into parent/source directories.");
        builder.AppendLine("- ENGINEERING or benchmark source cross-check may inspect source only inside an approved source-enabled/company environment; keep source local and summarize behavior/evidence instead of dumping code.");
        builder.AppendLine("- PKC does not control provider/network retention policy. Proprietary source must be used only in the organization's approved Claude Code/enterprise environment.");
        builder.AppendLine();
        builder.AppendLine("## Benchmark mode");
        builder.AppendLine();
        builder.AppendLine("- Do not run a full AI benchmark after every edit. Deterministic tests/smoke are the default verification layer.");
        builder.AppendLine("- For a semantic change, benchmark only the affected workflow/questions unless a full checkpoint benchmark is explicitly required.");
        builder.AppendLine("- Benchmark phase 1: answer from this generated workspace only. Phase 2: source-enabled local cross-check, then score the answer without copying source code into the report.");
        builder.AppendLine("- Full-corpus AI benchmark is reserved for acceptance checkpoints, releases, important demos, major knowledge/routing changes, broad regression risk, or explicit request.");
        builder.AppendLine();
        builder.AppendLine("Prefer an explicit unknown over an unsupported inference.");
        return builder.ToString();
    }

    private static string RenderStartHere(string? sourceRepositoryLabel, WorkspaceOverview? overview)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Start here");
        builder.AppendLine();
        builder.AppendLine("This is a generated PKC product workspace. Use it to answer Product Owner and QA questions without manually selecting PKC files.");
        if (!string.IsNullOrWhiteSpace(sourceRepositoryLabel))
        {
            builder.AppendLine($"Source repository label: `{sourceRepositoryLabel}` (identifier only, not approved product naming).");
        }

        if (overview is not null)
        {
            RenderOverview(builder, overview);
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
        builder.AppendLine("- Do not copy proprietary source code, secrets, raw facts, or source-file bodies into PRODUCT/QA answers.");
        builder.AppendLine("- If the user explicitly asks for implementation/code, ENGINEERING mode may inspect source only in an approved source-enabled environment.");
        builder.AppendLine();
        builder.AppendLine("## Benchmark routing");
        builder.AppendLine();
        builder.AppendLine("- Default to deterministic verification when a change does not alter product semantics.");
        builder.AppendLine("- For semantic changes, benchmark only the affected feature/workflow/questions first.");
        builder.AppendLine("- Phase 1 is workspace-only. Phase 2 may cross-check source locally in the approved environment, but the report must summarize behavior/evidence without source-code dumps.");
        builder.AppendLine("- Full-corpus AI benchmark is a checkpoint/release/demo gate, not an every-edit loop.");
        builder.AppendLine();
        builder.AppendLine("## Workspace status");
        builder.AppendLine();
        builder.AppendLine("This first `pkc run` workspace is a PREVIEW productization slice. `pkc verify` / READY-PARTIAL-FAILED verification is not implemented yet.");
        return builder.ToString();
    }

    private static void RenderOverview(StringBuilder builder, WorkspaceOverview overview)
    {
        var g = overview.Grounding;
        builder.AppendLine();
        builder.AppendLine("## What this workspace contains");
        builder.AppendLine();
        builder.AppendLine($"- {RunSummary.N(overview.ProductFeatures)} product features, {RunSummary.N(overview.Workflows)} workflows, {RunSummary.N(overview.Areas)} areas.");
        builder.AppendLine($"- Built from {RunSummary.N(overview.AnalyzedFiles)} deeply analyzed files; {RunSummary.N(overview.TestEvidenceFiles)} test files were kept out of product knowledge; " +
                           $"{RunSummary.N(overview.NotAnalyzedFiles)} files were not analyzed; {RunSummary.N(overview.UnknownAreas)} unknown areas (details: `_meta/coverage.json`).");
        if (overview.TopAreas.Count > 0)
        {
            builder.AppendLine("- Largest areas: " + string.Join(", ", overview.TopAreas.Select(area => $"{area.Area} ({RunSummary.N(area.Workflows)})")) + ".");
        }

        builder.AppendLine();
        builder.AppendLine("| Grounded evidence in workflows | Share |");
        builder.AppendLine("| --- | ---: |");
        builder.AppendLine($"| Business rules / conditions | {RunSummary.Pct(g.WithRules, g.Workflows)} |");
        builder.AppendLine($"| Permissions | {RunSummary.Pct(g.WithPermissions, g.Workflows)} |");
        builder.AppendLine($"| State / data changes | {RunSummary.Pct(g.WithStateChanges, g.Workflows)} |");
        builder.AppendLine($"| Side effects | {RunSummary.Pct(g.WithSideEffects, g.Workflows)} |");
        builder.AppendLine($"| UI steps | {RunSummary.Pct(g.WithUiSteps, g.Workflows)} |");
        builder.AppendLine();
        builder.AppendLine("What this means for answers:");
        foreach (var limit in Limits(g))
        {
            builder.AppendLine($"- {limit}");
        }
    }

    private static IEnumerable<string> Limits(KnowledgeGrounding g)
    {
        static bool Rare(int count, int total) => total == 0 || count * 20 < total;

        if (Rare(g.WithUiSteps + g.WithUiToBackend, g.Workflows))
        {
            yield return "Screen/UI questions (where a button is, what a page shows) are mostly not grounded here; answer them as ⚠️ not proven.";
        }

        if (g.Workflows > 0 && g.WithPermissions * 2 < g.Workflows)
        {
            yield return "Permission evidence exists for only part of the workflows; when a workflow lists none, say permissions are ⚠️ not proven rather than \"no restriction\".";
        }

        if (Rare(g.WithSideEffects, g.Workflows))
        {
            yield return "Side effects (emails, notifications, integrations) are rarely grounded; do not assume they happen.";
        }

        yield return "Rules are strongest where a workflow lists conditions; translate them into business language and keep unclear ones as ⚠️.";
    }

    private static string RenderAnswerContract()
    {
        return """
# PKC answer contract

## PRODUCT — default

- Answer the user's product or QA question first.
- Use business language: behavior, conditions, permissions, validations, outcomes, failures, state transitions and visible behavior.
- Do not volunteer source code, raw facts, fact IDs, classes or method walkthroughs.
- Do not copy proprietary source-code bodies, secrets, credentials, raw fact payloads, or source-file contents into the answer.
- Distinguish proven/observed behavior from unknown or unsupported behavior.
- Never infer behavior from names or conventions alone.

## QA lens

- Derive expected outcomes only from grounded behavior.
- Clearly label inferred test ideas separately from proven expected outcomes.
- Include negative/precondition cases when the knowledge proves them.
- Stay in generated workspace context unless an explicit benchmark source cross-check is being performed in an approved source-enabled environment.

## TRACE — explicit request only

- May name endpoints, source paths, symbols and evidence references.
- Keep the answer focused on traceability; do not dump code or proprietary file contents.

## ENGINEERING — explicit request only

- Requires source-enabled context for code-level analysis.
- This portable workspace intentionally contains generated knowledge rather than source code.
- Source may be inspected/edited only inside the approved source-enabled/company environment.
- Do not export or reproduce proprietary source outside that environment.
- If source is unavailable, say so instead of fabricating implementation details.

## Benchmark — explicit benchmark/audit workflow

- Full AI benchmark is not an every-edit default; deterministic verification should handle non-semantic changes.
- For semantic changes, start with only the affected workflow/questions.
- Phase 1: answer from this generated workspace only and record the answer.
- Phase 2: cross-check only the minimum required source inside the approved source-enabled environment.
- Report behavior, evidence paths/symbols, score and misses; do not paste source-code bodies into the benchmark report.
- Run the full corpus only for acceptance/release/demo checkpoints, major semantic/routing changes, broad regression risk, or explicit request.

## Uncertainty

- Prefer `unknown`, `not proven`, or a bounded partial answer over an unsupported claim.
- Never let frontend evidence upgrade weaker backend authority, or vice versa.
- Do not treat code-observed behavior as approved product intent unless explicit intent evidence exists.
- `_meta/coverage.json` counts what PKC did not semantically analyze (test evidence, indexed generated/infrastructure/runtime-dependency files, not-analyzable and unknown areas); when an answer may depend on those areas, say it is not proven.

## Answer format

Every answer uses the same shape so readers can scan it. Follow these rules first:

- Answer in the user's language (Vietnamese question → Vietnamese answer). Keep product terms, field names and status names as they appear in the knowledge.
- Translate code conditions into business language ("the organization number is required when creating from the registry"), never paste raw expressions such as `x == null` in PRODUCT/QA answers. If a condition cannot be translated with certainty, say it is unclear instead of paraphrasing it.
- Mark certainty explicitly: ✅ proven by generated knowledge, ⚠️ not proven / unknown, 💡 inferred idea (QA only).
- When the knowledge has no grounded information for the question, say so in one line, name what is missing, and suggest who or what can confirm it (the team, or ENGINEERING mode in an approved environment). Do not fill the gap.
- Keep it short: no preamble, no restating the question, no filler. Prefer bullets and tables over paragraphs.

### PRODUCT answers

```
**Short answer:** <1–3 sentences that answer the question directly>

### Rules and conditions
| When | Then |
| --- | --- |
| <business condition> | <outcome / error message> |

### Permissions
- <who may do it> ✅ | ⚠️ not proven

### Errors and edge cases
- <proven failure paths only>

### ⚠️ Not proven / unknown
- <gaps, Important unknowns, coverage limits>

### Related
- [<feature or workflow title>](<workspace-relative path>)
```

Omit a section only when it has nothing grounded to say. Keep "⚠️ Not proven / unknown" whenever anything relevant is unproven.

### QA test cases

```
| ID | Scenario | Preconditions | Steps | Expected result | Basis |
| --- | --- | --- | --- | --- | --- |
| TC1 | <happy path> | ... | ... | ... | ✅ proven |
| TC2 | <negative / permission / validation case> | ... | ... | ... | ✅ proven |
| TC3 | <idea not grounded in knowledge> | ... | ... | ... | 💡 inferred |
```

Follow the table with "⚠️ Not covered" listing behavior the knowledge could not prove (for example UI steps or side effects).

### TRACE answers (explicit request only)

Give the short answer, then an "Evidence" list: backend entry point, workflow document, and source path with line range from the workflow's Evidence section. Never include code bodies.
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
            isolation = new
            {
                generatedRoot = ".pkc/workspace",
                sourceRootAgentFilesModified = false,
                legacyRootKnowledgeArtifactsGeneratedByRun = false
            },
            privacy = new
            {
                workspaceContainsSourceCode = false,
                rawFactFilesIncluded = false,
                productAnswersMayReadSourceByDefault = false,
                sourceCrossCheckRequiresApprovedContext = true,
                note = "PKC controls generated workspace content, not provider/network retention policy."
            },
            benchmark = new
            {
                defaultLevel = "deterministic",
                targetedAiForSemanticChanges = true,
                fullAiForCheckpointsOnly = true
            },
            legacyCompatibility = new
            {
                command = "pkc build <repository-path>",
                artifacts = new[]
                {
                    "knowledge/",
                    PortableKnowledgePackRenderer.BundleFileName,
                    PortableKnowledgePackRenderer.ArchiveFileName
                }
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
