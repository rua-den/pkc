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
| D / R7.10 — Joint visibility | What backend condition and frontend visibility condition jointly control that same rendered value? | **REPAIRED / ALL GATES PASS / PENDING REREVIEW #15** |
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

### D / R7.10 — repaired, awaiting independent rereview #15

Independent rereview #14 **FAILED** production `3e6fa774...` because external Angular dependency components with attribute/class/combined selectors were absent from the product-only component selector inventory. A lexical interpolation under such a component host could be over-promoted even when content projection was not proven.

Result:

`docs/reviews/2026-09-23-v0.4.7-d-r7.10-independent-rereview-14.md`

Repair chain:

```text
8f667abc819f048b3dc85fc834677b7ca30f5518  fix: recognize external Angular component selectors
67c67fc25b6488dbc9f110a1b05c53a4bfee1a6c  fix: resolve nested Angular dependency selectors
47e098dbe910b7f6cfd933a0595370524bec1fb2  fix: correct resolved package root type
```

`8f667abc...` reads bounded imported-package Angular Ivy component declaration metadata so external component hosts become conservative projection boundaries while directives remain non-blocking. Continuation review found that root-only `node_modules` resolution missed normal nested Angular app dependency trees. `67c67fc...` resolves imported packages from the importing TypeScript file upward to repository root, selecting nearest `node_modules`, and adds a nested-app regression. `47e098db...` is the compile-correct final candidate after fixing the `ResolveLinkTarget()` result type.

Exact candidate gates:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35818350915 — PASS
pinned Loren                                35818350859 — PASS
Loren-main canary                           35818350997 — PASS
pinned Jellyfin + parity/provenance         35818350930 — PASS

Release build        0 warnings / 0 errors
C# tests             248 / 248 PASS
frontend tests       13 / 13 PASS
tool pack/install    PASS
WorkPlay             PASS
PokeTrade            PASS
```

Pinned Jellyfin remains 43,365 / 43,365 project-semantic with 43,365 facts, 195,316 relations, 386 workflows, 116 product features and 504 knowledge Markdown files; portable parity/no-leak PASS. Artifact `10732606989`, digest `sha256:1d2b38aadc18828a75625ea94651f2a1acfc3a3813a3cc9df0c57d9be4b1bde2`.

Repaired three-repository safety benchmark:

```text
base production  47e098dbe910b7f6cfd933a0595370524bec1fb2
wrapper          437d14a9b9ed36ce24e7fd8edfb2cef31eed7f6c
run              35818835753 — PASS, 3 / 3 jobs
```

All three unchanged repositories remain conservatively at zero current R7.9/R7.10 render/visibility authority. Agentic mutation-causality remains closed. This is safety evidence, not R7.14 positive-yield completion.

Fresh independent request:

`docs/reviews/2026-09-23-v0.4.7-d-r7.10-rereview-15-request.md`

### E — locked

E remains locked until independent rereview #15 accepts D. E is the final knowledge-only PO acceptance and portable transport/parity gate.

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
roadmap:      V0.4.7-D / R7.10 pending independent rereview #15
tool/package: RuaDen.Pkc.Tool 0.4.3-preview.2
```
