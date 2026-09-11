# External Real-Project Trial

This is the gate between the PokeTrade benchmark and V0.5 Azure DevOps work.

The purpose is not to prove PKC understands every stack. The purpose is to expose the next compiler gaps using a genuine codebase.

## 1. Build the local PKC tool

From the PKC repository:

```bash
dotnet pack src/Pkc.Cli/Pkc.Cli.csproj -c Release -o ./artifacts/tool
```

Install it into a disposable local tool directory:

```bash
dotnet tool install \
  --tool-path ./.pkc-tool \
  --add-source ./artifacts/tool \
  RuaDen.Pkc.Tool \
  --version 0.4.2-preview.1
```

## 2. Run PKC on the real repository

```bash
./.pkc-tool/pkc build /path/to/real-repository
```

On Windows PowerShell, use the generated executable under `.pkc-tool`:

```powershell
.\.pkc-tool\pkc.exe build D:\path\to\real-repository
```

PKC writes generated files into the target repository:

```text
.pkc/
  facts.json
  feature-candidates.json
  product-features.json

knowledge/
  index.md
  features/
  workflows/
```

Do not commit those files during the first trial unless you intentionally want to preserve the result.

## 3. Review in this order

### A. Product feature map

Open:

```text
.pkc/product-features.json
```

Check whether the discovered product areas/features are recognizable and whether unrelated actions were grouped together.

### B. AI entry point

Open:

```text
knowledge/index.md
```

Ask whether this is a useful map for another AI to choose the right feature file.

### C. Feature Markdown

Review a few high-value files under:

```text
knowledge/features/
```

Prefer features that have state transitions, permissions or non-trivial business rules.

### D. Workflow Markdown

For the same features, inspect the linked files under:

```text
knowledge/workflows/
```

Compare every important claim with the actual source/system behavior.

## 4. Classify every finding

Use exactly one primary category per finding.

### Wrong claim

PKC states something that is not true.

Examples:

```text
- wrong validation message attached to a guard
- += described as assignment
- unrelated UI action linked to an endpoint
- fake side effect inferred from a method name
```

Wrong claims have the highest priority.

### Missing important behavior

The behavior exists in source but PKC fails to carry it into knowledge.

Examples:

```text
- status transition missing
- permission missing
- important validation omitted
- UI action → API link missing
- transitive service/domain effect missing
```

### Noise

The claim is technically observable but not useful as product knowledge.

Examples:

```text
- object initializer fields
- internal ID counters
- infrastructure calls presented as business behavior
```

### Unsupported stack / pattern

PKC does not yet have an adapter or parser for the pattern.

Examples:

```text
- MVC/Razor UI
- Blazor UI
- Vue UI
- custom generated API client pattern not recognized by an existing adapter
```

Do not immediately implement every unsupported pattern. Record frequency and product impact first.

## 5. Minimal review report

For each finding, capture:

```text
Category: Wrong claim | Missing important behavior | Noise | Unsupported stack/pattern
Generated file: knowledge/...
Observed output: ...
Expected behavior: ...
Evidence: source file + method/component/route
Impact: High | Medium | Low
```

## 6. Exit criterion

The external trial is good enough to proceed when:

- no known high-impact wrong claims remain in the reviewed workflows;
- core product state transitions and permissions are represented for the selected features;
- remaining misses are understood limitations rather than silent incorrect claims;
- fixes are general compiler/adapter improvements, not hard-coded exceptions for the trial repository.

Only after this review should PKC move to V0.5 Azure DevOps evidence.

## Known limitations before the trial

These are already known and should not automatically be treated as regressions:

- React and Angular are the only frontend adapters implemented today;
- passive page-load/read flows may not have a complete UI navigation chain;
- cross-domain journeys are not yet synthesized into one inferred product journey;
- Azure DevOps intent/history is not present;
- runtime UI has not been confirmed by browser automation;
- incremental rebuild is not implemented.
