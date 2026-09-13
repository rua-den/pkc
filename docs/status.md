# PKC Status

Last updated: 2026-09-13

## North star

PKC is a **Product/System Knowledge Compiler**.

The product is the generated portable knowledge pack that a Product Owner can hand to an AI assistant and use for product/system questions **without making that AI rescan the source repository**.

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
  ↓
AI CAN EXPLAIN THE PRODUCT WITH HONEST BOUNDARIES
```

Do not replace this with `source → LLM → Markdown`.

## Current milestone

**V0.4.4 Loren Knowledge Readiness — EXTERNAL REVIEW FAILED / FIX REQUIRED**

The independent review of repository HEAD:

```text
34f77c036206d48bbf9495ea73e5debcea9f0eb3
```

found four blocker classes that must be fixed before V0.4.4 can close:

```text
1. UI/backend validation can false-positive `consistent` on lossy compound-condition normalization.
2. repeated `pkc build` can leave stale generated files in canonical `knowledge/`.
3. product capability-flow ranking contains Loren-shaped lexical preferences and needs a repo-neutral generalization proof.
4. frontend source scope can ingest tests/spikes/spec files into product evidence.
```

Detailed review:

```text
docs/reviews/2026-09-13-v0.4.4-external-review.md
```

V0.4.3 remains the last accepted packaged checkpoint:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

Do **not** bump the accepted package version. Do **not** advance to V0.4.5 until the external-review blockers are fixed, regression-locked, all current gates are rerun, and the resulting HEAD passes re-review.

## Current verified development checkpoint

Implementation/acceptance commit before documentation-only updates:

```text
4f7f75e76a1f158a880e8f2d1d64ea0bea0d36e7
```

External-review target that exposed the blockers:

```text
34f77c036206d48bbf9495ea73e5debcea9f0eb3
```

Verified runs before external review:

```text
CI #205
  test                         PASS
  PokeTrade real system        PASS

Loren external trial #104      PASS
Loren-main canary #86          PASS
```

Those green runs remain useful regression evidence, but they do not override the external-review findings.

Pinned Loren benchmark SHA:

```text
e9e81651d380d7d40998f235cfdc7f119fe67af8
```

## V0.4.4 work proven so far

The Loren real-project trial forced generic fixes for gaps that PokeTrade did not expose:

- Minimal API discovery + target-project semantic enrichment;
- backend/C# product-source exclusion for `tests/` / `spikes/`;
- conditional/development-only endpoint registration;
- Minimal API failure/direct-response semantics;
- multi-project MSBuild semantic loading;
- dictionary/object-initializer state-change noise;
- framework/primitive call-flow noise;
- ASP.NET sign-in/sign-out side effects;
- Minimal API response fact-ID collision;
- feature-level product-rule filtering;
- portable AI handoff (`knowledge/`, `PKC_KNOWLEDGE.md`, `PKC_KNOWLEDGE.zip`);
- Angular/static UI field/options/validation/visibility/enabled-state/binding evidence;
- backend field validation evidence;
- UI ↔ backend validation correlation;
- high-signal product-level capability-flow promotion.

Pinned Loren still requires zero `loose-roslyn-fallback` facts for the selected production benchmark and rejects product evidence from test/spike source for that benchmark.

External review proved that these successes do **not** yet establish the corresponding generic frontend source-scope, compound-condition validation, repeated-build parity, or capability-flow generalization contracts.

## External-review blockers

### B1 — validation consistency false-positive risk

Current condition-key normalization can discard parts of compound requiredness conditions and still classify UI/backend evidence as `consistent`.

Required direction:

```text
prove full supported condition equivalence
→ consistent

cannot prove equivalence
→ possible-mismatch or unknown
```

Add regressions for compound conditions, reversed operands, mixed conditional/unconditional requiredness, and multiple validation facts.

### B2 — stale canonical knowledge after rebuild

`pkc build` must reconcile generated knowledge from the current source state. A removed endpoint/capability must not leave an obsolete generated workflow/feature in `knowledge/` after a later build.

Regression must prove after a second build that obsolete knowledge disappears from:

```text
knowledge/
PKC_KNOWLEDGE.md
PKC_KNOWLEDGE.zip
```

without deleting unrelated user-owned files.

### B3 — capability-flow anti-overfit

Capability-flow promotion must not rely on Loren-shaped names to decide which important application edges survive the feature-level cap.

Prefer repository-neutral structural/semantic evidence. If lexical role hints remain, regression fixtures must use neutral vocabulary materially different from Loren.

### B4 — frontend source contamination

Angular/React product evidence needs an explicit product-source scope that rejects conventional test/spike source and test files where appropriate.

A test/spec component calling the same production endpoint must not contaminate portable product knowledge.

## Review warnings

Two non-blocking but important follow-ups were also recorded:

```text
portable KnowledgeEvidence loses per-fact analyzer confidence/provenance
blind-review record should preserve stronger durable reproducibility evidence
```

These are documented in the external review and should not be allowed to silently regress.

## UI/backend validation contract

Canonical evidence includes:

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

Intended classification contract remains:

```text
same requiredness + proven matching condition evidence
→ consistent

backend required + UI required not observed
→ possible-mismatch

UI required + backend required not observed
→ unknown

different observed requiredness conditions
→ possible-mismatch
```

The CSP regression proves one simple matching conditional path, but external review showed that it does **not** prove safe compound-condition equivalence. Until B1 is fixed, validation consistency correctness is blocked.

## Loren blind knowledge-only comprehension

First blind pass found two blocking abstraction gaps:

```text
A. Run feature did not expose project → memory → agent/brain collaboration clearly enough.
B. Action-proposal feature did not make current create-branch semantics obvious enough.
```

Source cross-check confirmed both were real product-semantic gaps.

The second blind pass on the artifact produced by `4f7f75e...` used **only `PKC_KNOWLEDGE.md`** and resolved those comprehension findings for pinned Loren.

Current interpretation after external review:

```text
Loren blind knowledge-only comprehension   PASS for the reviewed pinned artifact
Run abstraction finding A                  RESOLVED for Loren
Action-proposal finding B                  RESOLVED for Loren
workflow-detail noise finding C            NON-BLOCKING / preserved for traceability
integration presentation finding D         RESOLVED for Loren
capability-flow generalization             BLOCKED pending B3
```

Detailed internal benchmark record:

```text
docs/benchmarks/2026-09-12-loren-blind-review.md
```

## Portable handoff parity

For the reviewed pinned Loren artifact, single-build parity was:

```text
structured Markdown files:            23
files embedded verbatim in bundle:    23 / 23
missing bundle markers/content:        0
PKC_KNOWLEDGE.zip knowledge files:     23
raw .pkc entries in handoff ZIP:       0
source .cs/.ts entries in handoff ZIP: 0
```

That single clean-build check remains PASS for the recorded artifact.

External review found a different contract hole: repeated builds can leave stale files in canonical `knowledge/`, so **general canonical-pack/transport parity is BLOCKED until B2 is fixed**.

## Knowledge hierarchy that must hold

```text
AI_INSTRUCTIONS
  → tell the receiving AI how to consume authority/unknowns

index
  → orient around observed capabilities and boundaries

feature
  → explain product/system capability at high signal

workflow
  → explain one operation with exact behavior and evidence

raw evidence
  → preserve implementation proof/provenance
```

Low-level evidence should not be deleted merely to make output cleaner, and should not dominate feature pages.

## V0.4.4 acceptance position

Current gate status:

```text
PokeTrade known-answer regression                  PASS before blocker fixes
Loren evidence/workflow correctness                PASS before blocker fixes
Loren blind knowledge-only comprehension           PASS for pinned artifact
Loren single-build handoff parity                   PASS for pinned artifact
UI validation/behavior known-answer benchmark      PASS for simple fixture only
Loren independent external review                  FAIL / FIX REQUIRED
validation consistency general correctness          BLOCKED
repeated-build canonical handoff parity             BLOCKED
capability-flow repo-neutral generalization         BLOCKED
frontend product-source scope                       BLOCKED
```

Therefore V0.4.4 is **not complete**.

Exact next sequence:

```text
fix B1 generically + regression
→ fix B2 generically + repeated-build regression
→ fix B3 generically + neutral anti-overfit regression
→ fix B4 generically + frontend contamination regression
→ run full tests
→ rerun PokeTrade
→ rerun Loren pinned
→ rerun Loren-main canary
→ rerun UI validation benchmark
→ rerun handoff/parity checks
→ rerun blind review where semantics changed
→ independent re-review of new HEAD
```

Do not implement unrelated roadmap work while these blockers are open.

## V0.4.5 — locked

The second independent real-repository gate remains the next milestone **only after V0.4.4 external re-review passes**.

When unlocked, the second repository must be genuine, not created/modified for PKC, and materially different from PokeTrade/Loren.

## V0.5 unlock condition

**V0.5 Azure DevOps remains locked until all are PASS:**

```text
PokeTrade known-answer regression                  PASS; rerun required after fixes
Loren evidence/workflow correctness                PASS; rerun required after fixes
Loren blind knowledge-only comprehension           PASS; rerun required where semantics change
Loren handoff parity                               BLOCKED by repeated-build finding
UI validation/behavior known-answer benchmark      BLOCKED by compound-condition finding
Loren independent external review                  FAIL / FIX REQUIRED
second independent real-repo trial                 PENDING / LOCKED
second knowledge-only comprehension                PENDING / LOCKED
second handoff parity                              PENDING / LOCKED
cross-benchmark regression after all fixes         PENDING
known boundaries/unknowns documented honestly      PASS so far
no repository-specific compiler exceptions         NEEDS B3 GENERALIZATION PROOF
```

If any required gate is not PASS, remain in V0.4.x.

## Scope discipline

Until the unlock gate passes, do not start these merely because they are attractive roadmap work:

- Azure DevOps ingestion;
- Angular TypeScript `TypeChecker` migration unless a blocker proves it necessary;
- React AST rewrite unless a blocker proves it necessary;
- additional frontend frameworks;
- browser/runtime exploration;
- incremental compilation;
- generalized insight/drift analysis;
- live MCP/connector delivery for convenience alone.

A new analyzer/delivery capability may enter V0.4.x only when a real blocker proves it is necessary for accuracy, completeness, signal-to-noise, traceability, honest uncertainty, or portable handoff reliability.

## Current commands

```bash
pkc scan <repository-path>
pkc build <repository-path>
```

Current development output:

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
