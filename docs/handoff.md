# PKC Handoff

Use this file when continuing PKC in another chat/session or when handing the current checkpoint to an independent reviewer.

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

Do not make ZIP extraction the primary interface. For small/medium packs, the default PO flow is to upload `PKC_KNOWLEDGE.md`.

## Current milestone

**V0.4.4 Loren Knowledge Readiness — READY FOR INDEPENDENT EXTERNAL REVIEW.**

V0.4.3 remains the last accepted packaged checkpoint:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

Do not bump the accepted package version yet.

Implementation/acceptance commit before docs-only updates:

```text
4f7f75e76a1f158a880e8f2d1d64ea0bea0d36e7
```

Verified runs:

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

## Benchmark roles

```text
PokeTrade
→ known-answer runnable regression

Loren pinned commit
→ deterministic blocking real-project V0.4.4 benchmark

Loren main
→ moving non-blocking canary

second independent real repo
→ V0.4.5 anti-overfit/generalization gate
```

## Important proven V0.4.4 fixes

The Loren trial has already forced generic fixes for:

- Minimal API endpoint discovery + semantic enrichment;
- test/spike source contamination;
- conditional/dev-only endpoint registration;
- Minimal API failures/direct responses;
- multi-project MSBuild loading;
- false state-change noise from dictionary/object initializers;
- framework/primitive call-flow noise;
- ASP.NET sign-in/sign-out side effects;
- Minimal API response fact-ID collision;
- layered product-rule promotion;
- portable AI handoff;
- high-signal product capability flow.

UI/backend work also now covers:

```text
ui-field
ui-field-option
ui-field-validation
ui-field-visibility
ui-field-enabled-state
ui-field-binding
backend-field-validation
ui-backend-validation
```

and conservative classification:

```text
consistent
possible-mismatch
unknown
```

## Loren blind-review result

The first knowledge-only pass found two serious comprehension gaps:

```text
A. Run feature did not expose project → memory → agent/brain flow clearly enough.
B. Action proposal purpose was only inferable from deep implementation names.
```

Source cross-check confirmed both were real gaps.

PKC now promotes selective `Observed capability flow` to product feature pages while filtering plumbing/self-helper noise.

Second blind pass used **only `PKC_KNOWLEDGE.md`** from commit `4f7f75e...` and can now recover the important Run path:

```text
POST /api/run
→ LorenRunService.RunAsync
→ LorenProjectContextBuilder.BuildAsync
→ LorenMemoryContextBuilder.BuildAsync
→ IMemoryStore.ListCurrentForProjectAsync

LorenRunService.RunAsync
→ AgentLoop.RunAsync
→ IBrain.ThinkAsync
→ IActionGateway.ExecuteAsync
```

and proposal approval:

```text
approve endpoint
→ ApproveProposalAndCreateBranchAsync
→ ICreateBranchProposalStore
→ IProjectCatalog
→ ActionIntentFingerprint
→ IActionGateway.ExecuteAsync
```

Current assessment:

```text
blind knowledge-only comprehension   PASS
Run abstraction gap                  RESOLVED
proposal-purpose gap                 RESOLVED
workflow low-level detail            NON-BLOCKING; preserve for traceability
integration presentation             RESOLVED via capability-flow abstraction
```

Full record:

```text
docs/benchmarks/2026-09-12-loren-blind-review.md
```

## Handoff parity result

For the pinned Loren artifact generated from `4f7f75e...`:

```text
knowledge Markdown files                 23
embedded verbatim in PKC_KNOWLEDGE.md    23 / 23
missing markers/content                  0
PKC_KNOWLEDGE.zip knowledge files        23
raw .pkc entries in handoff ZIP          0
source .cs/.ts entries in handoff ZIP    0
```

The workflow artifact used by CI may contain raw `.pkc` diagnostics for benchmark review; the PO-facing `PKC_KNOWLEDGE.zip` must not.

## Exact next action

**Do not implement another roadmap feature now.**

The current checkpoint should go to an independent reviewer.

Reviewer should inspect at least:

```text
1. compiler architecture / no source→LLM shortcut
2. provenance and fallback honesty
3. PokeTrade regression integrity
4. pinned Loren knowledge correctness
5. Run capability abstraction
6. action-proposal semantics
7. UI/backend validation classification
8. single-file vs structured-pack parity
9. whether any claims overstate observed code
10. whether any repo-specific exception was introduced
```

If reviewer finds a serious blocker:

```text
classify finding
→ fix only generic proven gap
→ add regression
→ rerun PokeTrade + Loren pinned + Loren-main + blind/parity checks
→ review again
```

If reviewer passes V0.4.4, move to **V0.4.5 second independent real-repository trial**.

## V0.4.5 requirements

The second repository must be genuine and materially different from PokeTrade/Loren. Do not choose it because current heuristics make it easy.

Run the same acceptance process:

```text
normal project build
→ pkc build
→ inspect provenance/fallbacks
→ blind knowledge-only review
→ structured/single-file parity
→ source cross-check
→ fix only generic defects
→ regression-lock
→ independent review
```

Prefer a supported real frontend/configuration surface if one exists so UI validation/form behavior is exercised outside synthetic fixtures.

## V0.5 remains locked

Do not start Azure DevOps ingestion until all are PASS:

```text
PokeTrade known-answer regression                  PASS
Loren evidence/workflow correctness                PASS
Loren blind knowledge-only comprehension           PASS
Loren handoff parity                               PASS
UI validation/behavior known-answer benchmark      PASS
Loren independent external review                  PENDING
second independent real-repo trial                 PENDING
second blind comprehension                         PENDING
second handoff parity                              PENDING
cross-benchmark regression after all fixes         PENDING
known boundaries/unknowns documented honestly      PASS so far
no repository-specific compiler exceptions         PASS so far
```

If any gate is not PASS, remain in V0.4.x.

## Scope rule

Do not start these merely because they are unfinished:

- Azure DevOps;
- Angular TypeScript TypeChecker migration;
- React AST rewrite;
- MVC/Razor/Blazor/Vue expansion;
- browser/runtime exploration;
- incremental compilation;
- generalized drift/insight analysis;
- live MCP/connector delivery.

Only add one of them if a real benchmark/reviewer proves it is necessary for accuracy, completeness, signal-to-noise, traceability, honest uncertainty or handoff reliability.
