# PKC Handoff

Use this file when continuing PKC in another chat/session.

## Product idea

PKC means **Product/System Knowledge Compiler**.

The target user is a Product Owner. A PO should be able to take PKC's generated `knowledge/` folder and attach it to any capable AI — ChatGPT, Claude, Gemini, Copilot, etc. — then ask questions about the product without making that AI re-read or grep the source repository.

Example target question:

> How do I change the status of a WorkPlay? What should I be careful about?

The mature knowledge pack should combine UI operation, business behavior/state transitions, validations/permissions, side effects/integrations, delivery history and traceable evidence.

## Non-negotiable compiler architecture

Do not implement `source -> LLM -> Markdown` directly.

```text
SOURCE
  ↓
DETERMINISTIC ANALYZERS / ADAPTERS
  ↓
EVIDENCE / FACT MODEL
  ↓
FEATURE / WORKFLOW CANDIDATES
  ↓
KNOWLEDGE SYNTHESIS
  ↓
CANONICAL KNOWLEDGE MODEL
  ↓
DETERMINISTIC MARKDOWN RENDERER
```

## Current verified state

**V0.4.3 Analyzer Fidelity Hardening is COMPLETE.**

Verified acceptance commit:

```text
8f69d6c931e917ce7538b9a991a05211e624cb45
```

Verified CI run:

```text
34627169975  (#85)
```

Both `test` and `poketrade-real-system` are green.

Tool package version:

```text
RuaDen.Pkc.Tool 0.4.3-preview.1
```

## Why V0.4.3 exists

External review correctly found that the previous implementation could sound stronger than its analyzer fidelity actually was:

- C# used Roslyn but built a loose compilation from repository source + runtime trusted platform assemblies instead of the target project's real build graph;
- React and Angular frontend extraction were primarily regex/text scanners.

V0.4.3 does not change the evidence-first architecture. It strengthens the analyzer layer and makes fallback provenance explicit.

## C# analyzer fidelity

Preferred path:

```text
.csproj
  ↓
MSBuildWorkspace
  ↓
target project compilation/references
  ↓
Roslyn SemanticModel
  ↓
analysisMode = project-semantic
analysisConfidence = high
```

PokeTrade acceptance proves:

- `Microsoft.AspNetCore.Mvc.ControllerBase` resolves semantically;
- ASP.NET Core HTTP attributes resolve semantically;
- project-backed semantic call relations are rebuilt from the target compilation.

If project loading/source mapping fails, PKC keeps conservative loose Roslyn evidence and marks it:

```text
analysisMode = loose-roslyn-fallback
analysisConfidence = medium
semanticContext = runtime-platform-assemblies-only
analysisFallbackReason = ...
```

Never hide this fallback.

## Frontend analyzer fidelity

Frontend technology remains outside compiler core:

```text
framework adapter
  ↓
canonical ui-* evidence
  ↓
generic linker
  ↓
shared workflow/feature compiler
```

### Angular

TypeScript source prefers project-local TypeScript AST:

```text
analysisMode = typescript-ast
analysisConfidence = high
```

Currently AST-backed:

- component class detection
- application routes
- method structure needed for passive load linkage
- HTTP call expressions

Angular template action/visibility extraction is **still fallback**, explicitly tagged:

```text
analysisMode = angular-template-regex-fallback
analysisConfidence = medium
```

If the TypeScript AST path is unavailable, the Angular adapter can fall back further to:

```text
analysisMode = regex-fallback
analysisConfidence = low
```

### React

React remains a conservative regex/text adapter for now:

```text
analysisMode = regex-fallback
analysisConfidence = low
```

Do not describe React as AST-backed.

## Fallback propagation into knowledge

`analysisMode` and `analysisConfidence` are stored in evidence metadata.

If fallback facts materially contribute to a workflow, `CrossStackFeatureCandidateBuilder` adds an explicit warning to the candidate `Unknowns`, which is rendered into Markdown under `Important unknowns`.

The rule is:

```text
fallback may contribute evidence
          ↓
provenance remains visible
          ↓
knowledge must not silently upgrade confidence
```

## PokeTrade fidelity acceptance

PokeTrade remains the runnable `.NET 10 + Angular 22` acceptance benchmark. Its business behavior is unchanged, but frontend source was deliberately restyled so the old Angular regex-only scanner would miss important facts:

- multiline/chained `HttpClient.post(...)` for Create Order;
- quoted `path` / `component` object keys for the Orders route.

CI still generates the expected knowledge and explicitly requires:

```text
project-semantic
target-project semantic context
Microsoft.AspNetCore.Mvc.ControllerBase
semantic HTTP attribute resolution
typescript-ast
angular-template-regex-fallback
```

and rejects a full Angular `regex-fallback` path for this benchmark.

The installed CLI also verifies `project-semantic` scanning on WorkPlay.

## Important interpretation of “verified”

WorkPlay and PokeTrade prove the current acceptance contracts. They do **not** prove arbitrary real-world repositories are already robustly supported.

That distinction is important for external reviews and user-facing claims.

## Current commands

```bash
pkc scan <repository-path>
pkc build <repository-path>
```

Main outputs:

```text
.pkc/facts.json
.pkc/feature-candidates.json
.pkc/product-features.json
knowledge/index.md
knowledge/features/**/*.md
knowledge/workflows/**/*.md
```

## Next engineering target — keep narrow

**V0.4.4 external real-project trial.**

Do not add speculative capability first.

Use one genuine repository:

```text
real project builds/runs normally
    ↓
pkc build <real-project>
    ↓
inspect analyzer modes/fallbacks
    ↓
review generated knowledge against source + known behavior
    ↓
classify failures:
  wrong claim
  missing important behavior
  noise
  unsupported stack/pattern
  unexpected fallback
    ↓
fix only proven gaps + add regression fixture
```

Do not start V0.5 Azure DevOps, React AST, MVC/Blazor/Vue adapters, Playwright, or incremental architecture unless the external trial proves they are the next blocker.
