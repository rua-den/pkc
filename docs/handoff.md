# PKC Handoff

Use this file when continuing PKC in another chat/session or external review.

## Product idea — do not drift from this

PKC means **Product/System Knowledge Compiler**.

The target user is a Product Owner. A PO should be able to take PKC's generated `knowledge/` folder and attach it to any capable AI — ChatGPT, Claude, Gemini, Copilot, etc. — then ask questions about the product **without making that AI re-read or grep the source repository**.

The analyzer/fact graph exists to make that knowledge trustworthy. Analyzer sophistication is not the final product by itself.

The final acceptance question is:

> If the source repository is hidden and an AI receives only `knowledge/`, can it explain the product accurately, at the right abstraction level, with important unknowns and evidence boundaries preserved?

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
  ↓
PORTABLE PRODUCT KNOWLEDGE
```

## Current milestone

**V0.4.4 External real-project knowledge trial — IN PROGRESS.**

V0.4.3 Analyzer Fidelity Hardening remains the last completed packaged checkpoint:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

Do not call V0.4.4 complete merely because analyzers/CI are green.

## Real-project trial

Current real repository: `rua-den/loren`.

Use two benchmark modes:

```text
Pinned Loren commit
→ deterministic blocking regression/acceptance

Loren main
→ moving canary
→ catches new repository patterns as Loren evolves
→ must not silently replace the pinned acceptance SHA
```

PokeTrade remains the known-answer runnable regression system.

## Proven external-trial fixes so far

The Loren trial has already forced fixes for:

- Minimal API endpoint discovery + target-project semantic enrichment;
- exclusion of `tests/` and `spikes/` from product evidence;
- conditional endpoint availability;
- Minimal API condition/exception/direct response semantics;
- multi-project MSBuild semantic loading;
- dictionary/object-initializer mutation noise;
- framework/primitive call-flow presentation noise;
- ASP.NET sign-in/sign-out product side effects;
- Minimal API response metadata collision when endpoints are declared inside methods/extensions.

Pinned Loren currently verifies that important production evidence does not fall back to loose Roslyn analysis.

## What the latest output review says

The main remaining issue is **knowledge abstraction**, not missing parser breadth.

Example: Loren `Run Operations` currently promotes helper-level conditions and loops such as string truncation, candidate matching and character normalization into the product-feature summary, and repeats many rules across `/api/run` and `/internal/dev/run`.

Those details may be legitimate evidence/workflow detail. They should not dominate the product-level feature file a PO/AI reads first.

Therefore the next work must focus on knowledge synthesis/presentation using proven Loren output problems.

## V0.4.4 acceptance gate

### Evidence layer

- important claims grounded in source;
- provenance/confidence honest;
- no silent unsupported fallback for benchmark-critical behavior.

### Workflow layer

Each workflow should expose meaningful:

- entry point / UI path when known;
- permission;
- validation/failure paths;
- state changes;
- side effects/integrations;
- application-oriented flow;
- source evidence;
- unknowns.

### Product layer

`knowledge/index.md` and feature files must let a PO/AI understand product capabilities quickly.

Do not dump every helper guard/loop into product-level rules simply because it was observed in the transitive call graph.

### Blind comprehension review

Hide Loren source and give the reviewer only generated `knowledge/`.

The knowledge pack must let the reviewer answer, at minimum:

```text
What observable product/system surface exists?
How does owner authentication work?
What does the main run operation do?
How are projects listed and bootstrapped?
How are action proposals approved/cancelled?
Which behavior is conditional/development-only?
What important failure paths exist?
What remains unknown because its evidence source has not been compiled?
```

Then compare those answers against source/known behavior.

A green grep/test suite is necessary but **not sufficient**.

## Scope rule — very important

A new analyzer capability is allowed into V0.4.4 only when the real-project knowledge review proves it is needed to improve one of:

```text
accuracy
completeness
signal-to-noise
traceability
honest uncertainty
```

Do not start TypeScript TypeChecker work, React AST, MVC/Blazor/Vue expansion, browser exploration, incremental compilation or Azure DevOps merely because they are unfinished roadmap items.

## V0.5 gate

**V0.5 Azure DevOps is locked.**

Open it only after V0.4.4 passes the knowledge-only comprehension review with no blocker-class findings.
