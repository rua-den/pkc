# PKC Handoff

Use this file when continuing PKC in another chat/session.

## Product idea — do not drift from this

PKC means **Product/System Knowledge Compiler**.

The target user is a Product Owner. A PO should be able to give PKC's generated portable knowledge to an AI assistant and ask product/system questions **without making that AI re-read or grep the source repository**.

The analyzer/fact graph exists to make that knowledge trustworthy. Analyzer sophistication is not the final product.

Final acceptance question:

> If the source repository is hidden and an AI receives only the generated portable knowledge, can it explain the product accurately, at the right abstraction level, while preserving important unknowns and evidence boundaries?

## Non-negotiable compiler architecture

Do not implement `source → LLM → Markdown` directly.

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
DETERMINISTIC PORTABLE RENDERING
  ↓
AI-CONSUMABLE PRODUCT KNOWLEDGE
```

## Knowledge hierarchy

```text
AI_INSTRUCTIONS = tell the receiving AI how to consume the pack
index            = orient the AI
feature          = explain a capability at high signal
workflow         = explain one operation in grounded detail
raw evidence     = prove implementation detail/provenance
```

Do not delete raw evidence merely to reduce noise. Do not promote every helper condition/call into product-level knowledge.

## Portable handoff contract

Current `pkc build` development output:

```text
knowledge/
  AI_INSTRUCTIONS.md
  index.md
  features/**/*.md
  workflows/**/*.md

PKC_KNOWLEDGE.md
PKC_KNOWLEDGE.zip
```

Roles:

```text
knowledge/
→ canonical structured portable knowledge

PKC_KNOWLEDGE.md
→ one-file convenience handoff for PO → AI

PKC_KNOWLEDGE.zip
→ archive/transport containing knowledge/ only
```

The structured pack, single-file bundle, and ZIP transport must not disagree on current product behavior.

## Current milestone

**V0.4.4 Loren Knowledge Readiness — EXTERNAL REVIEW FAILED / FIX REQUIRED.**

V0.4.3 remains the last accepted packaged checkpoint:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

Do not bump the accepted package version.

External review inspected HEAD:

```text
34f77c036206d48bbf9495ea73e5debcea9f0eb3
```

Full review record:

```text
docs/reviews/2026-09-13-v0.4.4-external-review.md
```

Read that review before changing code.

## Existing benchmark evidence

Implementation/acceptance checkpoint before the docs-only external-review handoff:

```text
4f7f75e76a1f158a880e8f2d1d64ea0bea0d36e7
```

Previously verified:

```text
CI #205
  test                  PASS
  PokeTrade             PASS

Loren external #104     PASS
Loren-main canary #86   PASS
```

Pinned Loren SHA:

```text
e9e81651d380d7d40998f235cfdc7f119fe67af8
```

These passes are regression evidence only. They do not override the external-review blockers.

## Current blocking findings

### B1 — UI/backend validation equivalence can false-positive

Current normalization can collapse a compound condition to one equality and then mark UI/backend requiredness `consistent` even when the actual conditions differ.

Example that must not become `consistent`:

```text
UI:      serviceType == CSP && region == US
backend: serviceType == CSP
```

Required behavior:

```text
full supported condition equivalence proven
→ consistent

not provably equivalent
→ possible-mismatch or unknown
```

Required regression coverage:

- compound `&&` conditions;
- reversed equality operands;
- conditional vs unconditional requiredness;
- multiple requiredness facts;
- no high-confidence consistency from lossy normalization.

### B2 — repeated builds can leave stale canonical knowledge

`pkc build` currently writes current generated files but does not prove obsolete generated files are removed from `knowledge/` after source capabilities disappear.

Required regression:

```text
build #1: endpoint/capability A exists
→ A knowledge generated

change source: remove A

build #2
→ A absent from knowledge/
→ A absent from PKC_KNOWLEDGE.md
→ A absent from PKC_KNOWLEDGE.zip
```

Fix must not delete unrelated user-owned files without an explicit generated-file ownership rule.

### B3 — capability-flow scoring needs repo-neutral generalization

Current product-flow scoring contains Loren-shaped lexical preferences such as `IBrain`, `IMemory`, `IProject`, `Brain`, `Loop`, and `Catalog`.

Because ranking happens before the feature flow cap, vocabulary can determine which application edges survive.

Required direction:

- prefer graph/semantic role over repository vocabulary;
- use endpoint distance, interface/cross-component boundaries, project/application boundaries, or other deterministic structural signals;
- if lexical hints remain, make them generic;
- add neutral regression fixtures that do not reuse Loren terminology.

Example neutral names for tests:

```text
ExecutionCoordinator
ContextProvider
ConversationRepository
UseCase
Orchestrator
```

### B4 — frontend product-source contamination

C# product scanning excludes conventional `test`, `tests`, and `spikes` source. Frontend scanning does not yet apply an equivalent scope consistently and can ingest test/spec source.

Required direction:

- create a shared/equivalent frontend product-source scope;
- exclude conventional test/spike directories;
- exclude conventional test files where appropriate, including `*.spec.ts` / `*.test.ts` patterns;
- add a regression where test code calls the same production endpoint and prove it does not enter portable product knowledge.

## Review warnings

Not release blockers by themselves, but preserve them during fixes:

```text
W1. Portable KnowledgeEvidence does not preserve per-fact analyzer mode/confidence/caveat.
W2. Blind-review records should become more reproducible/durable: artifact hash, run id, frozen question/answer transcript, source cross-check per question.
```

Do not accidentally worsen provenance while fixing blockers.

## Required coding workflow

**Do not implement another roadmap feature now.**

For each blocker:

```text
1. reproduce the finding
2. add a regression that fails before the fix
3. implement a generic compiler fix
4. make the regression pass
5. run affected unit/integration tests
6. commit the blocker independently where practical
```

Recommended order:

```text
B1 validation correctness
→ B2 canonical rebuild/parity
→ B3 capability-flow generalization
→ B4 frontend source scope
```

The order may change only if implementation dependencies make another order safer. Do not combine the fixes into repository-specific special cases.

## Completion gate after blocker fixes

After all four blockers are resolved, run:

```text
full PKC unit/integration tests
PokeTrade runnable + known-answer regression
pinned Loren external trial
Loren-main canary
UI/backend validation benchmark
repeated-build structured/bundle/ZIP parity
blind knowledge-only review where compiler semantics changed
```

Then update:

```text
docs/status.md
docs/handoff.md
```

with exact runs/commits/results and request an independent re-review of the new HEAD.

Do **not** mark V0.4.4 PASS yourself solely because CI is green.

## Benchmark roles

```text
PokeTrade
→ known-answer runnable regression

Loren pinned commit
→ deterministic blocking real-project V0.4.4 benchmark

Loren main
→ moving non-blocking canary

second independent real repo
→ V0.4.5 anti-overfit/generalization gate, currently LOCKED
```

## V0.4.5 is locked

Do not start the second independent real-repository milestone until the current external review is re-run and returns PASS.

## V0.5 remains locked

Do not start Azure DevOps ingestion until all required V0.4 gates pass, including:

```text
external re-review of V0.4.4
second independent real-repo trial
second blind comprehension
second handoff parity
cross-benchmark regression after all fixes
```

## Scope rule

Do not start these merely because they are unfinished:

- Azure DevOps;
- Angular TypeScript TypeChecker migration unless a blocker proves it necessary;
- React AST rewrite unless a blocker proves it necessary;
- MVC/Razor/Blazor/Vue expansion;
- browser/runtime exploration;
- incremental compilation;
- generalized drift/insight analysis;
- live MCP/connector delivery.

Only add analyzer/delivery capability when a proven blocker requires it for accuracy, completeness, signal-to-noise, traceability, honest uncertainty, or handoff reliability.

## Coding-thread bootstrap

A new coding thread can start with:

```text
Continue PKC from current main.
Read docs/status.md, docs/handoff.md, and docs/reviews/2026-09-13-v0.4.4-external-review.md.
Fix the V0.4.4 external-review blockers only, regression-first, commit incrementally, rerun all current gates, and do not advance V0.4.5 until independent re-review passes.
```
