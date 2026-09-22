# PKC Milestones

Last updated: 2026-09-23

PKC is a Product/System Knowledge Compiler. Milestones are accepted only when generated portable knowledge answers named Product Owner questions with deterministic evidence and appropriate uncertainty.

## Permanent delivery rules

```text
question / acceptance boundary
→ regression-first fixture
→ deterministic implementation
→ focused verification
→ full relevant verification
→ diff review
→ coherent commit/push
→ exact-SHA cross-benchmark gates
→ independent review
```

Do not advance while a predecessor checkpoint is red or under review. Unsupported inference fails closed and lower-authority deterministic evidence must survive stronger composition failure.

## Accepted V0.4.x checkpoints

```text
V0.4.4 Loren knowledge readiness          PASS / COMPLETE
V0.4.5 real-repository generalization     PASS / COMPLETE
V0.4.6 business logic reconstruction      PASS / COMPLETE
```

Accepted V0.4.6 production: `c310e893762997f34562a6b3a62dbab2b05c0c93`.

## V0.4.7 — cross-layer PO-question readiness — CURRENT

| Checkpoint | PO question | Current state |
| --- | --- | --- |
| A — Origin and copy timing | Where did this value come from? Does an upstream change alter this existing value? | **PASS / COMPLETE** |
| B — Computation and later change | Was it calculated? What can overwrite it? What was the last proven source before output? | **PASS / COMPLETE** |
| C — Backend to API | What exact backend value supplies this response field? | **PASS / COMPLETE** |
| D / R7.9 — API to rendered value | What exact API field feeds the displayed value? | **PASS / COMPLETE** |
| D / R7.10 — Joint visibility | What backend condition and frontend visibility condition jointly control that same rendered value? | **REPAIRED / ALL GATES PASS / PENDING REREVIEW #13** |
| E — Product acceptance | Can an AI answer agreed PO questions from the portable knowledge pack alone? | **LOCKED behind D** |

### Accepted baselines

```text
A      09a0c7f2a058adfd7ddb7b4ac2feb2d17580a429
B      17fd30b3a4b8178208adabc12c40dee060bedb54
C      fbb64b9917da1f63362558355201ff7998384ba0
R7.9   fc4bfa7042f59620b2c7ba1c4f6700cf3b02172a
mutation-causality repair 67624944da27ff1f1f5a1154018a255aae11d1fe
```

Keep these closed unless a real regression is demonstrated.

### D / R7.10 — repaired, awaiting independent rereview #13

The previous rereview chain closed many concrete false-positive classes across Angular HTML/SVG/template authority. After the rereview #12 request was written, implementation continued through additional SVG text, ancestry, opacity and content-model hardening without a recorded independent acceptance of the resulting production state.

A fresh reconciliation of `d4416c4a13a04db46091bbffff1c71566c0d5d0c` found that structurally supported native SVG text could still be non-visible through paint semantics, for example:

```html
<text fill="none" stroke="none">{{ displayPrice }}</text>
```

Rather than growing an incomplete SVG paint/layout solver inside V0.4.7, the current repair makes native SVG directly-visible render authority explicitly unsupported/fail-closed.

Current production candidate:

```text
7818c7ed646b30cb7b8505f053572783e075af6f
fix: fail closed on native SVG render authority
```

Ordinary HTML remains supported; HTML under SVG `foreignObject` remains a positive path. Native SVG can be reconsidered in a later milestone with stronger rendering evidence.

Exact candidate gates:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35765278584 — PASS
pinned Loren                                35765278607 — PASS
Loren-main canary                           35765278416 — PASS
pinned Jellyfin + parity/provenance         35765278447 — PASS

Release build        0 warnings / 0 errors
C# tests             231 / 231 PASS
frontend tests       13 / 13 PASS
tool pack/install    PASS
```

Pinned Jellyfin remains 43,365 / 43,365 project-semantic with 43,365 facts, 195,316 relations, 386 workflows, 116 product features and 504 knowledge Markdown files; portable parity/no-leak PASS.

Repaired three-repository benchmark:

```text
base production  7818c7ed646b30cb7b8505f053572783e075af6f
wrapper          87255ba6f5f012d82ee17f039d540db6bbdf01bf
run              35765659218 — PASS, 3 / 3 jobs
```

All three unchanged repositories remain conservatively at zero supported current R7.9/R7.10 positives. This is a safety PASS, not positive-yield completion. Mutation-causality remains closed.

Fresh independent request:

`docs/reviews/2026-09-23-v0.4.7-d-r7.10-rereview-13-request.md`

### E — locked

E remains locked until independent rereview #13 accepts D. E is the final knowledge-only PO acceptance and portable transport/parity gate.

R7.14 positive real-project yield is **REQUIRED for E completion and remains NOT PASS**. Do not weaken authority or modify benchmarks merely to manufacture a positive shape.

## V0.5 — Azure DevOps input evidence — LOCKED

The currently recorded roadmap still starts V0.5 only after V0.4.7 and the V0.4.x PO-question-readiness exit gate pass. ADO intent/history evidence must coexist with implementation-observed behavior without silently overwriting it.

A prepared future execution packet recommends formally re-baselining the post-V0.4.7 order to productize the AI workspace/run/verify flow and continuous update/diff before adding Azure DevOps evidence. That recommendation is **not yet an unlock** and must not change the active milestone while V0.4.7 is open.

Prepared packet:

```text
docs/plans/2026-09-22-ai-workspace-continuous-update-plan.md
docs/reviews/2026-09-22-ai-workspace-continuous-update-plan-self-review.md
```

## Later milestones

Current recorded roadmap, pending formal post-V0.4.7 re-baseline:

- V0.6 — incremental compilation and knowledge diffs.
- V0.7 — runtime UI exploration/confirmation.
- V0.8 — product insight, gaps and requirement-vs-implementation drift.

## Version semantics

```text
roadmap:      V0.4.7-D / R7.10 pending independent rereview #13
tool/package: RuaDen.Pkc.Tool 0.4.3-preview.2
```
