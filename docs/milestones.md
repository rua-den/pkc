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
| D / R7.10 — Joint visibility | What backend condition and frontend visibility condition jointly control that same rendered value? | **REREVIEW #14 FAIL / REPAIRED LOCALLY / PUSH + GATES PENDING** |
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

### D / R7.10 — rereview #14 failed; local repair awaiting push and gates

Rereview #13 targeted native-SVG fail-closed candidate `7818c7ed...`. Before independent acceptance was recorded, pre-challenge found two additional Angular render-authority false-positive classes, so #13 is superseded rather than passed.

Current repaired production is:

```text
34182e221df6cf50eaa0ac362a5f575e80236a64  fix: reject unproven Angular content projection
3e6fa7749eb8ef47be4eedb72d1c159cd502692f  fix: bound Angular direct text containers
8f667abc819f048b3dc85fc834677b7ca30f5518  fix: recognize external Angular component selectors (local, not pushed)
```

The first repair prevents lexical children of component hosts from being treated as directly rendered when content projection is not proven. The second rejects metadata/fallback/conditional HTML containers such as `title`, `canvas`, `dialog`, `details`, `object` and `noscript` from direct-text authority. Native SVG direct authority remains fail-closed from `7818c7ed...`; ordinary HTML remains supported, including eligible HTML under SVG `foreignObject`.

Exact candidate gates:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35769202107 — PASS
pinned Loren                                35769202115 — PASS
Loren-main canary                           35769202043 — PASS
pinned Jellyfin + parity/provenance         35769202074 — PASS

Release build        0 warnings / 0 errors
C# tests             241 / 241 PASS
frontend tests       13 / 13 PASS
tool pack/install    PASS
```

Pinned Jellyfin remains 43,365 / 43,365 project-semantic with 43,365 facts, 195,316 relations, 386 workflows, 116 product features and 504 knowledge Markdown files; portable parity/no-leak PASS.

Repaired three-repository benchmark:

```text
base production  3e6fa7749eb8ef47be4eedb72d1c159cd502692f
wrapper          28c9758ba5ae172bb0ee52e032a28fa24fb1f917
run              35769939151 — PASS, 3 / 3 jobs
```

Direct artifact inspection shows all three unchanged repositories remain at zero current R7.9/R7.10 render/visibility authority promotion. Canonical benchmark outputs are byte-identical to the prior safety benchmark except generated nested ZIP bytes/timestamps. Mutation-causality remains closed.

Rereview #14 result:

`docs/reviews/2026-09-23-v0.4.7-d-r7.10-independent-rereview-14.md`

The new repair recognizes external Angular component selectors from bounded imported-package Ivy declaration metadata. Local focused and related authority tests pass, and the Release solution build is clean. The push is approved but blocked by repository authentication: SSH has no accepted key and the configured HTTPS identity lacks write permission. Exact-SHA gates and a fresh independent rereview #15 remain pending.

### E — locked

E remains locked until the repaired exact SHA passes all gates and independent rereview #15 accepts D. E is the final knowledge-only PO acceptance and portable transport/parity gate.

R7.14 positive real-project yield is **REQUIRED for E completion and remains NOT PASS**. Do not weaken authority or modify benchmarks merely to manufacture a positive shape.

## V0.5 — Azure DevOps input evidence — LOCKED

The active roadmap still starts V0.5 only after V0.4.7 and the V0.4.x PO-question-readiness exit gate pass. A prepared future packet recommends formally re-baselining the post-V0.4.7 order to productize AI workspace/run/verify and continuous update/diff before adding Azure DevOps evidence. That recommendation is not an unlock while V0.4.7 remains open.

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
roadmap:      V0.4.7-D / R7.10 repaired locally; push/gates/rereview #15 pending
tool/package: RuaDen.Pkc.Tool 0.4.3-preview.2
```
