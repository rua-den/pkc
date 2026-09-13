# PKC Handoff

Use this file when continuing PKC in another chat/session.

## Product idea — do not drift from this

PKC means **Product/System Knowledge Compiler**. The target user is a Product Owner. A PO should be able to give PKC's generated portable knowledge to an AI assistant and ask product/system questions without making that AI re-read or grep the source repository.

Final acceptance question:

> If the source repository is hidden and an AI receives only the generated portable knowledge, can it explain the product accurately, at the right abstraction level, while preserving important unknowns and evidence boundaries?

Do not implement `source → LLM → Markdown` directly. Keep the compiler pipeline:

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

## Current state

**V0.4.4 blocker fixes are implemented and automated gates are green. Independent re-review is still required. V0.4.5 is locked.**

Original external review target:

```text
34f77c036206d48bbf9495ea73e5debcea9f0eb3
```

Review record:

```text
docs/reviews/2026-09-13-v0.4.4-external-review.md
```

Implementation checkpoint with all four fixes:

```text
8b01d4b5ba84112f36e46b234426f754be38c8f6
```

V0.4.3 remains the last accepted package:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

Do not bump that package and do not start V0.4.5 until an independent reviewer passes the resulting V0.4.4 HEAD.

Pinned Loren benchmark SHA:

```text
e9e81651d380d7d40998f235cfdc7f119fe67af8
```

## External-review blockers fixed regression-first

### B1 — validation-condition equivalence

```text
RED: 9dac656daf66eb0e3da7fedff65830caf4c926a5
FIX: b0f42ce87d36427ee39fcd09c8cd69e6f11de035
```

`ValidationConsistencyCandidateEnricher` no longer treats one matching lossy condition key as proof of equivalence. It canonicalizes the complete supported equality-conjunction condition, handles reversed equality operands, compares complete requiredness-condition sets, distinguishes unconditional requiredness, and degrades unsupported/unprovable semantics to conservative mismatch/unknown behavior instead of high-confidence `consistent`.

Regression coverage: compound `&&`, reversed operands, conditional/unconditional, and multiple requiredness facts.

### B2 — repeated-build canonical parity

```text
RED: b343c17d4e263103b3096463584a7c7dcc8ec1b2
FIX: b45a953c7020db283e53c7df4b66efc6edda9574
```

`pkc build` now reconciles stale PKC-owned generated Markdown before writing the current canonical pack. Ownership rule: only Markdown under `knowledge/` whose YAML frontmatter contains `generated: true` is eligible for stale deletion. Unowned user files are preserved.

End-to-end test performs two builds on one temp repository and proves a removed endpoint disappears from the structured pack, `PKC_KNOWLEDGE.md`, and `PKC_KNOWLEDGE.zip`, while `knowledge/user-notes.md` survives unchanged.

### B3 — capability-flow anti-overfit

```text
RED: cd29aea57aa7b201022ac1f525f176b4e564d347
FIX: 81701f53666fdee402c8e7347a6a85af653a870f
```

Removed Loren-shaped lexical scoring. Product-flow ranking now uses generic graph structure: HTTP entry-point proximity, BFS distance, cross-component transitions, intermediate-node role, and graph convergence. Neutral fixtures use `ExecutionCoordinator`, `ContextProvider`, `ConversationRepository`, `UseCase`, plus many `HelperServiceXX` decoys.

### B4 — frontend source contamination

```text
RED: e4d563478023e2bf712e41695c88eea7383b6adb
FIX: 8b01d4b5ba84112f36e46b234426f754be38c8f6
```

Added a shared `FrontendSourceScope` enforced by `FrontendScanner` before adapter evidence is merged. It excludes conventional test/spike directories and `*.spec.*` / `*.test.*` files for both React and Angular evidence. The regression-only commit failed on both adapters; the fix makes the same 8 frontend tests pass.

## Gate evidence on implementation checkpoint

For `8b01d4b5ba84112f36e46b234426f754be38c8f6`:

```text
full PKC test/build gate                 PASS
  Pkc.CSharp.Tests                       40 / 40
  Pkc.Frontend.Tests                      8 / 8
  build                                  0 warnings / 0 errors
PokeTrade runnable + known-answer        PASS
pinned Loren external trial              PASS
Loren-main canary                        PASS
```

The test suite now includes the UI/backend validation benchmark regressions and repeated-build structured/bundle/ZIP parity regression required by the review.

Every blocker has explicit pre-fix red evidence:

```text
B1 9dac656 → test FAIL
B2 b343c17 → test FAIL
B3 cd29aea → test FAIL
B4 e4d5634 → test FAIL
```

## Portable handoff contract

Current `pkc build` output:

```text
knowledge/
  AI_INSTRUCTIONS.md
  index.md
  features/**/*.md
  workflows/**/*.md
PKC_KNOWLEDGE.md
PKC_KNOWLEDGE.zip
```

The structured pack, single-file bundle, and ZIP must agree on the current product state. Do not weaken the `generated: true` ownership boundary when changing rebuild cleanup.

## Review warnings still open

These were non-blocking warnings in the external review and were intentionally not broadened into unrelated V0.4.4 changes:

```text
W1. Portable KnowledgeEvidence does not preserve all per-fact analyzer confidence/provenance.
W2. Blind-review records need stronger durable reproducibility evidence: artifact hash/run id/frozen transcript/source cross-check.
```

## What to do next

Do **not** implement more compiler features now.

The next action is:

```text
1. read docs/status.md
2. read docs/handoff.md
3. read docs/reviews/2026-09-13-v0.4.4-external-review.md
4. independently review the new final HEAD against every blocker and warning boundary
5. rerun/inspect blind knowledge-only checks where changed compiler semantics affect the generated artifact
6. return PASS or concrete findings
```

The implementation author must not mark V0.4.4 PASS merely because CI is green.

If independent re-review returns PASS, then and only then update status/handoff to close V0.4.4 and unlock V0.4.5. If it finds a blocker, stay in V0.4.4 and fix regression-first.

## Scope locks

Until independent V0.4.4 re-review passes, do not start:

- second independent real-repository V0.4.5 work;
- Azure DevOps ingestion;
- Angular TypeChecker migration unless a blocker requires it;
- React AST rewrite unless a blocker requires it;
- additional frontend frameworks;
- browser/runtime exploration;
- incremental compilation;
- generalized insight/drift analysis;
- live MCP/connector delivery.

## Bootstrap for the independent re-review thread

```text
Review PKC V0.4.4 independently from current main HEAD.
Read docs/status.md, docs/handoff.md, then docs/reviews/2026-09-13-v0.4.4-external-review.md.
Verify all four original blockers against the implementation and regressions, inspect the current gate evidence, and perform knowledge-only/blind checks where compiler semantics changed. Do not accept the implementation author's conclusions without source/test evidence. Return PASS or concrete blocking findings. Do not advance V0.4.5 unless V0.4.4 passes.
```
