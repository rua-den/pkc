# Architecture

PKC is designed as a compiler pipeline.

```text
SOURCE
  ↓
DETERMINISTIC ANALYZERS / ADAPTERS
  ↓
EVIDENCE / FACT MODEL
  ↓
FEATURE & WORKFLOW DISCOVERY
  ↓
KNOWLEDGE SYNTHESIS
  ↓
KNOWLEDGE MODEL
  ↓
MARKDOWN RENDERER
  ↓
PORTABLE PRODUCT KNOWLEDGE
```

## Why not source → Markdown directly?

Direct source-to-Markdown generation makes traceability, incremental rebuilds, testing and hallucination control difficult. PKC therefore keeps a machine-oriented evidence layer separate from human/AI-oriented knowledge.

## Backend evidence

C# source is analyzed with Roslyn and normalized into `Pkc.Core` facts and relations. Downstream feature/workflow code consumes the evidence model, not Roslyn syntax directly.

## Frontend adapter boundary

Frontend frameworks are source adapters, not product-knowledge concepts.

```text
React ───────┐
Angular ─────┤
MVC/Razor ───┤  future adapters
Blazor ──────┤  future adapters
Vue ─────────┘
      ↓
IFrontendAdapter
      ↓
canonical UI facts
      ↓
FrontendScanner + generic relation linker
      ↓
shared evidence model
      ↓
feature/workflow compiler
```

`FrontendScanner` may run zero, one or multiple adapters for a repository. The compiler core must not branch on a framework name.

Current canonical UI fact kinds are deliberately small:

- `ui-screen`
- `ui-route`
- `ui-action`
- `ui-api-call`

Current common relations include:

- route `renders` screen
- action `triggers-handler`
- action `triggers-api`
- API call `calls-endpoint` after backend matching

Framework identity such as `react-static` or `angular-static` is metadata for provenance/debugging. Knowledge synthesis must not require it to understand the workflow.

The generic linker currently uses a conservative rule: an action handler is linked to an API call when there is exactly one same-framework API-call method with the same normalized handler name. Ambiguous matches are left unlinked rather than guessed.

## Boundaries

### Deterministic layer

Responsible for facts that can be proven from source syntax/semantics and their locations.

### Inference layer

Responsible for grouping evidence into product concepts such as features, workflows and business explanations. It consumes canonical evidence, not framework-specific source constructs.

### Presentation layer

Renders the canonical knowledge model into portable Markdown/YAML. Markdown is output, not the internal source of truth for compilation.

## Non-goal

PKC is not trying to implement every frontend framework inside the core. New UI technologies should be added as adapters that emit the same canonical UI evidence contract.
