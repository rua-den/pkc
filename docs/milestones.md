# PKC Milestones

Last updated: 2026-09-24

PKC is a Product/System Knowledge Compiler. A milestone is accepted only when portable generated knowledge answers named product questions from deterministic evidence with explicit uncertainty, and the normal product flow can practically produce that knowledge on the intended repository class.

## Permanent delivery rules

```text
question / acceptance boundary
→ regression-first fixture
→ deterministic implementation
→ focused verification
→ full relevant verification
→ diff review
→ coherent commit/push
→ exact-SHA gates
→ real-repository safety + product-value evidence
→ independent review where required
```

A green workflow is not by itself a product benchmark pass. Likewise, a semantically strong compiler is not demo-ready if `pkc run` cannot practically produce the workspace on the target repository.

## Accepted V0.4.x checkpoints

```text
V0.4.4 Loren knowledge readiness          PASS / COMPLETE
V0.4.5 real-repository generalization     PASS / COMPLETE
V0.4.6 business logic reconstruction      PASS / COMPLETE
V0.4.7-A origin and copy timing           PASS / COMPLETE
V0.4.7-B computation and later change     PASS / COMPLETE
V0.4.7-C backend to API                   PASS / COMPLETE
V0.4.7-D / R7.9 API to rendered value     PASS / COMPLETE
mutation-causality repair                 PASS / CLOSED
```

Accepted V0.4.6 production: `c310e893762997f34562a6b3a62dbab2b05c0c93`.

## V0.4.7 — cross-layer PO-question + demo readiness — CURRENT

| Checkpoint | Acceptance question | Current state |
| --- | --- | --- |
| A | Where did this value come from? | PASS / COMPLETE |
| B | Was it computed or later overwritten? | PASS / COMPLETE |
| C | What backend value supplies the response field? | PASS / COMPLETE |
| D / R7.9 | What API field feeds the rendered value? | PASS / COMPLETE |
| D / R7.10 | What backend + frontend conditions jointly control that exact rendered value? | **REPAIRED / ALL GATES PASS / PENDING REREVIEW #17** |
| E0 | Can PKC discover, plan and boundedly scan a large mixed repository well enough to produce the AI workspace? | **LOCKED behind D / PREPARED** |
| E1 | Are remaining high-value product behaviors represented? | **LOCKED behind E0** |
| E2 | Can an AI answer agreed PO/QC questions from the portable pack alone on unchanged real repositories? | **LOCKED behind E1** |

Exact R7.10 candidate:

```text
96205a9a643864facaf9642a3b390ddcdbed59d9
fix: tolerate duplicate Angular import aliases
```

Current external gate is independent rereview #17 of that exact SHA. Product/demo work does not self-certify it.

## Demo-critical E sequencing

When D passes, E executes sequentially.

### E0 — Repository Discovery + bounded run readiness

```text
RD1 inventory + safe exclusion
→ RD2 application boundaries + ownership
→ RD3 vendor/custom frontend classification
→ RD4 runtime/plugin provenance
→ RD5 deterministic ScanPlan
→ RD6 scoped/bounded semantic execution
→ RD7 coverage + observability + plan-only inspection
→ RD8 private large-repo validation
```

E0 exists because a private mixed legacy repository demonstrated that current full-root scanning can become operationally impractical before useful output. The observed ~9 GB run is design evidence, not a universal numeric acceptance threshold.

E0 acceptance requires structural scope reduction, bounded execution, honest coverage and successful practical workspace generation on the approved private validation repository. Discovery alone emitting JSON is not enough.

Technical packet:

```text
docs/plans/2026-09-24-repository-discovery-scan-planning-plan.md
docs/reviews/2026-09-24-repository-discovery-scan-planning-self-review.md
docs/plans/2026-09-24-demo-critical-sequential-execution-plan.md
```

### E1 — Remaining product-value repairs

Only after E0 PASS:

1. re-evaluate/adopt the validated construction/default/computation candidate;
2. close semantic integration-side-effect gaps;
3. preserve grounded workflow rules in feature summaries.

The existing Fix #4 candidate remains off `main` until E0 because it adds another semantic C# pass and RD6 changes scan orchestration/scope.

### E2 — Real-project product acceptance

Only after E1 PASS:

- R7.14 must produce positive unchanged-real-project cross-layer yield;
- Level-2 known-answer benchmark must be rerun at the acceptance checkpoint;
- workspace-only PO/QC answers must be useful, evidence-grounded and honest about unknowns;
- portable privacy/no-source-leak requirements remain mandatory.

## Benchmark acceptance semantics

Two independent properties remain necessary:

### Safety / authority

PASS requires unchanged pinned repos, honest execution result, no unsupported authority promotion, no portable source/raw leakage and preservation of accepted causality boundaries.

### Product value

Known answers are scored independently against pinned source for endpoint/capability discovery, permissions, business preconditions, state transitions, defaults/computations, side effects, implementation behavior, UI interaction, UI→API linkage, feature-summary fidelity and cross-layer proof.

Historical Level-2 selected-probe diagnostics after Fix #3:

```text
Agentic Users Update     ~94.5%
Jin12 Contacts Update    100.0%
Kesetovic PackOrder       82.5%
selected-probe aggregate ~91.6%
backend PO/QC core       ~95.3%
```

These are diagnostics, not acceptance thresholds.

## Sequential execution rule

Do not parallelize production implementation across E0 sub-checkpoints or between E0/E1/E2.

For demo-critical work:

```text
finish current checkpoint
→ verify locally
→ review diff
→ commit/push once
→ final CI
→ update status/handoff
→ only then start next checkpoint
```

This intentionally favors a working end-to-end `pkc run` path over accumulating additional semantic features while the scan path is not usable on the target repository.

## Current exact action

```text
independent rereview #17 of exact 96205a9a643864facaf9642a3b390ddcdbed59d9
→ PASS: close R7.10 + D, unlock E, start E0/RD1 only
→ FAIL: regression-first minimum generic R7.10 repair, rerun exact-SHA gates, rereview
```

## Post-V0.4.7 roadmap

After E0/E1/E2 complete and V0.4.7 closes:

1. formal AI workspace `run/verify` refinements not already absorbed by E0;
2. semantic `update/diff`;
3. Azure DevOps intent/history evidence;
4. later runtime/product insight work.

Prepared packets remain useful but cannot override the active status/handoff ordering.

## Version semantics

```text
roadmap:      V0.4.7-D / R7.10 pending independent rereview #17; E0 is first checkpoint after D
package:      RuaDen.Pkc.Tool 0.4.3-preview.2
```
