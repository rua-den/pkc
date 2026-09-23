# PKC Milestones

Last updated: 2026-09-23

PKC is a Product/System Knowledge Compiler. A milestone is accepted only when portable generated knowledge answers named product questions from deterministic evidence with explicit uncertainty.

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
→ independent review
```

A green workflow is not by itself a product benchmark pass.

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

## V0.4.7 — cross-layer PO-question readiness — CURRENT

| Checkpoint | PO question | Current state |
| --- | --- | --- |
| A | Where did this value come from? | PASS / COMPLETE |
| B | Was it computed or later overwritten? | PASS / COMPLETE |
| C | What backend value supplies the response field? | PASS / COMPLETE |
| D / R7.9 | What API field feeds the rendered value? | PASS / COMPLETE |
| D / R7.10 | What backend + frontend conditions jointly control that exact rendered value? | **REPAIRED / ALL GATES PASS / PENDING REREVIEW #17** |
| E | Can an AI answer agreed PO/QC questions from the portable pack alone? | **LOCKED behind D** |

Exact R7.10 candidate:

```text
96205a9a643864facaf9642a3b390ddcdbed59d9
fix: tolerate duplicate Angular import aliases
```

Latest repair closes local Angular import/re-export closure around unresolved external component projection risk and fails closed on unsupported scalar/default indirection. Exact gates are all PASS: CI `35832501567`, pinned Loren `35832501543`, Loren-main `35832501552`, Jellyfin `35832501534`; Release 0 warnings/errors; C# 259/259; frontend 13/13.

Safety benchmark wrapper `65c02df...`, run `35833147258`, passes 3/3 pinned repositories with established output counts unchanged.

Rereview #16 targeted older candidate `37a711...` and is superseded. Current external gate is rereview #17 of `96205...`.

## Benchmark acceptance semantics

Real-repository benchmarking has two independent gates.

### Safety / regression gate

PASS requires unchanged pinned repos, honest execution result, no unsupported authority promotion, no portable source/raw leakage, and preservation of accepted causality boundaries.

Current state: **PASS**.

### Product-value / known-answer gate

Must compare generated knowledge against product behavior verified independently from source. Score endpoint/capability, permissions, business preconditions, state transitions, defaults/computations, side effects, cross-method behavior, UI interaction, UI→API linkage, feature-summary fidelity and cross-layer proof.

Current state: **NOT PASS / PARTIAL USEFULNESS**.

Current known gaps:

- feature summaries can lose child-workflow rules;
- controller → interface → implementation behavior can be lost;
- object construction/default/computed business state can be lost;
- integration side effects can be observed in flow but omitted from Side effects;
- R7.14 cross-layer positive real-project yield remains zero.

Evidence: `docs/benchmarks/2026-09-23-real-repo-product-value-scorecard.md`.

These gaps do not reopen accepted predecessor checkpoints unless a real regression is demonstrated. They are product-acceptance evidence for E after D is independently accepted.

## R7.14 — real-project positive yield

**REQUIRED for E / NOT PASS.**

At least one unchanged real repository must naturally emit a supported positive V0.4.7 cross-layer answer. Do not modify a benchmark or weaken fail-closed authority to manufacture yield.

## Current exact action

```text
independent rereview #17 of exact 96205a9a643864facaf9642a3b390ddcdbed59d9
→ PASS: close R7.10 + D and unlock only E
→ FAIL: regression-first minimum generic repair
```

Request: `docs/reviews/2026-09-23-v0.4.7-d-r7.10-rereview-17-request.md`.

## Post-V0.4.7 roadmap

V0.5 and later work remain locked while V0.4.7 is open. A prepared packet recommends re-baselining after V0.4.7 to:

1. AI workspace + `run/verify`
2. semantic `update/diff`
3. Azure DevOps intent/history evidence
4. later runtime/product insight work

Prepared packet:

```text
docs/plans/2026-09-22-ai-workspace-continuous-update-plan.md
docs/reviews/2026-09-22-ai-workspace-continuous-update-plan-self-review.md
```

## Version semantics

```text
roadmap:      V0.4.7-D / R7.10 pending independent rereview #17
tool/package: RuaDen.Pkc.Tool 0.4.3-preview.2
```
