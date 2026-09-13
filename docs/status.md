# PKC Status

Last updated: 2026-09-13

## North star

PKC is a **Product/System Knowledge Compiler**. The product is a portable, AI-readable knowledge pack that a Product Owner can hand to an AI assistant and use for product/system questions without making that AI rescan the source repository.

```text
SOURCE
  ↓
DETERMINISTIC ANALYZERS / ADAPTERS
  ↓
EVIDENCE / FACT MODEL
  ↓
FEATURE / WORKFLOW DISCOVERY
  ↓
KNOWLEDGE SYNTHESIS
  ↓
CANONICAL KNOWLEDGE MODEL
  ↓
PORTABLE AI HANDOFF
```

Do not replace this with `source → LLM → Markdown`.

## Current milestone

**V0.4.4 Loren Knowledge Readiness — BLOCKER FIXES IMPLEMENTED / INDEPENDENT RE-REVIEW REQUIRED.**

The independent external review of `34f77c036206d48bbf9495ea73e5debcea9f0eb3` found four release blockers. All four now have regression-first fixes on `main`, and the implementation checkpoint below passed all current automated gates:

```text
8b01d4b5ba84112f36e46b234426f754be38c8f6
```

V0.4.4 is **not self-certified PASS**. It remains open until an independent re-review of the resulting HEAD returns PASS.

V0.4.3 remains the last accepted packaged checkpoint:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

Do not bump the accepted package version and do not advance V0.4.5 while re-review is pending.

External review record:

```text
docs/reviews/2026-09-13-v0.4.4-external-review.md
```

Pinned Loren benchmark SHA:

```text
e9e81651d380d7d40998f235cfdc7f119fe67af8
```

## External-review blocker closure evidence

### B1 — validation consistency false positives

Regression-first pair:

```text
9dac656daf66eb0e3da7fedff65830caf4c926a5  test: reproduce compound validation equivalence blocker
b0f42ce87d36427ee39fcd09c8cd69e6f11de035  fix: prove full validation condition equivalence
```

The comparator now requires the complete supported requiredness-condition set to be provably equivalent before returning `consistent`. Supported equality conjunctions are normalized deterministically, including reversed equality operands. Conditional/unconditional mismatches, differing multi-fact sets, and unsupported/unprovable condition semantics are conservative rather than high-confidence consistent.

Regression coverage includes:

```text
compound && condition mismatch
reversed equality operands
conditional vs unconditional requiredness
multiple requiredness facts
```

### B2 — stale generated knowledge after repeated build

Regression-first pair:

```text
b343c17d4e263103b3096463584a7c7dcc8ec1b2  test: reproduce stale generated knowledge blocker
b45a953c7020db283e53c7df4b66efc6edda9574  fix: reconcile owned knowledge output on rebuild
```

`pkc build` now reconciles generated Markdown owned by PKC before publishing the current canonical pack. Ownership is explicit: only Markdown under `knowledge/` whose YAML frontmatter contains `generated: true` is eligible for stale-file deletion. Unowned user files are preserved.

The end-to-end regression builds the same temp repository twice, removes an endpoint between builds, and proves obsolete knowledge disappears from:

```text
knowledge/
PKC_KNOWLEDGE.md
PKC_KNOWLEDGE.zip
```

while a user-created `knowledge/user-notes.md` survives unchanged.

### B3 — Loren-shaped capability-flow ranking

Regression-first pair:

```text
cd29aea57aa7b201022ac1f525f176b4e564d347  test: reproduce capability flow lexical overfit blocker
81701f53666fdee402c8e7347a6a85af653a870f  fix: rank capability flow by graph structure
```

Repository-specific lexical preferences were removed from product-flow ranking. Ranking now uses deterministic structural signals such as HTTP entry-point proximity, graph distance, cross-component transitions, intermediate graph role, and convergence.

Neutral anti-overfit fixtures use vocabulary materially different from Loren, including:

```text
ExecutionCoordinator
ContextProvider
ConversationRepository
UseCase
```

plus `HelperServiceXX` decoys to prove type-name suffixes do not decide which important path survives the feature-level cap.

### B4 — frontend test/spec/spike contamination

Regression-first pair:

```text
e4d563478023e2bf712e41695c88eea7383b6adb  test: reproduce frontend test source contamination blocker
8b01d4b5ba84112f36e46b234426f754be38c8f6  fix: enforce frontend product source scope
```

A shared frontend product-source boundary is enforced in `FrontendScanner` before adapter output is merged into product evidence. Conventional test/spike directories and `*.spec.*` / `*.test.*` files are rejected across React and Angular adapter results.

The pre-fix regression failed on both React and Angular because facts from test/spec/spike source entered evidence. The fixed implementation passes the same tests while preserving production source.

## Automated gate results on implementation checkpoint

Implementation checkpoint:

```text
8b01d4b5ba84112f36e46b234426f754be38c8f6
```

Results:

```text
full PKC test/build gate                 PASS
  Pkc.CSharp.Tests                       40 / 40
  Pkc.Frontend.Tests                      8 / 8
  Release build                          0 warnings / 0 errors

PokeTrade runnable + known-answer        PASS
pinned Loren external trial              PASS
Loren-main moving canary                 PASS
```

The full test gate includes the new validation-equivalence, repeated-build parity, capability-flow generalization, and frontend product-source regressions. The WorkPlay tool-package/install/build smoke checks also pass using the accepted `0.4.3-preview.2` package version.

## Regression-first proof

Each blocker was reproduced before its fix:

```text
B1 regression-only commit 9dac656  → test FAIL
B2 regression-only commit b343c17  → test FAIL
B3 regression-only commit cd29aea  → test FAIL
B4 regression-only commit e4d5634  → test FAIL
```

The corresponding fix commits above make those regressions pass.

## Review warnings intentionally left in scope

The external review also recorded two non-blocking warnings. They remain open follow-up concerns and were not expanded into unrelated V0.4.4 work:

```text
W1. portable KnowledgeEvidence does not preserve all per-fact analyzer confidence/provenance
W2. blind-review records should preserve stronger durable reproducibility evidence
```

Do not silently regress these boundaries.

## Portable handoff contract

Current development output remains:

```text
.pkc/facts.json
.pkc/feature-candidates.json
.pkc/product-features.json
knowledge/AI_INSTRUCTIONS.md
knowledge/index.md
knowledge/features/**/*.md
knowledge/workflows/**/*.md
PKC_KNOWLEDGE.md
PKC_KNOWLEDGE.zip
```

The structured pack, one-file bundle, and ZIP must represent the same current source state. The repeated-build regression now locks this contract against stale PKC-owned Markdown.

## Acceptance position

Current position after blocker fixes:

```text
B1 validation equivalence regression              PASS
B2 repeated-build structured/bundle/ZIP parity    PASS
B3 neutral capability-flow generalization         PASS
B4 frontend product-source scope                  PASS
full PKC tests                                     PASS on 8b01d4b
PokeTrade                                          PASS on 8b01d4b
pinned Loren                                       PASS on 8b01d4b
Loren-main canary                                  PASS on 8b01d4b
independent V0.4.4 re-review                       PENDING
```

Therefore V0.4.4 is **ready for independent re-review but not complete**.

The next permitted action is an independent review of the new final HEAD against `docs/reviews/2026-09-13-v0.4.4-external-review.md`, including knowledge-only/blind checks where changed semantics materially affect the generated pack. The implementation author must not convert green CI into a self-issued review PASS.

## V0.4.5 — locked

The second independent real-repository milestone remains locked until V0.4.4 independent re-review returns PASS.

Do not start Azure DevOps, another frontend rewrite/framework, browser/runtime exploration, incremental compilation, generalized drift analysis, or other roadmap work while this gate is open.
