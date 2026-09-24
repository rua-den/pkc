# PKC Milestones

Last updated: 2026-09-24

PKC is a Product/System Knowledge Compiler. Acceptance requires deterministic portable product knowledge, honest uncertainty and a normal product flow that can practically produce the workspace on the intended repository class.

## Accepted checkpoints

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

## V0.4.7 — CURRENT

| Checkpoint | Acceptance question | Current state |
| --- | --- | --- |
| A | Where did this value come from? | PASS / COMPLETE |
| B | Was it computed or later overwritten? | PASS / COMPLETE |
| C | What backend value supplies the response field? | PASS / COMPLETE |
| D / R7.9 | What API field feeds the rendered value? | PASS / COMPLETE |
| D / R7.10 | What backend + frontend conditions jointly control that exact rendered value? | **REPAIRED / EXACT-SHA GATES PASS / PENDING REREVIEW #18** |
| E0 | Can PKC discover, plan and boundedly scan a large mixed repository well enough to produce the AI workspace? | **LOCKED behind D / PREPARED** |
| E1 | Are remaining high-value product behaviors represented? | **LOCKED behind E0** |
| E2 | Can an AI answer agreed PO/QC questions from the portable pack alone on unchanged real repositories? | **LOCKED behind E1** |

Exact R7.10 production candidate:

```text
a67abfa4caded980bd8abec317598afd0ea16a42
fix: fail closed unsupported Angular alias expressions
```

Rereview #17 blocker is repaired regression-first. The final filter no longer depends on semicolon/ASI terminator spelling; unsupported alias/expression indirection beginning with an identifier conservatively fails closed. Exact-SHA gates are green, but R7.10/D remain open until fresh independent rereview #18 accepts this SHA.

Review request:

```text
docs/reviews/2026-09-24-v0.4.7-d-r7.10-rereview-18-request.md
```

Exact-SHA gates:

```text
CI / full tests / WorkPlay / PokeTrade   35939834957 PASS
Loren pinned/external                    35939834993 PASS
Loren-main canary                        35939834966 PASS
Jellyfin parity                          35939834845 PASS
```

Implementation environment lacked a local .NET SDK; Actions evidence must not be rewritten as local execution. Local TypeScript 5.8.3 compile checks prove LF/CR/U+2028/U+2029 and block-comment-contained line-terminator ASI variants are compile-valid.

## Current exact action

```text
fresh independent rereview #18 of exact
a67abfa4caded980bd8abec317598afd0ea16a42
```

Do not start E0/RD1, merge Fix #4, or start E1/E2 before independent PASS explicitly closes D.

## Demo-critical E sequence

Once D passes, execute sequentially:

```text
RD1 inventory + safe exclusion
→ RD2 application boundaries + ownership
→ RD3 vendor/custom frontend classification
→ RD4 runtime/plugin provenance
→ RD5 deterministic ScanPlan
→ RD6 scoped/bounded semantic execution
→ RD7 coverage + observability + plan-only inspection
→ RD8 private large-repo validation
→ prove pkc run practically produces .pkc/workspace
→ E1 semantic richness
→ E2 product acceptance
```

The private mixed legacy repository observation (~9 GB RAM before useful completion) remains design evidence, not a universal threshold. E0 must solve structural scope/discovery first.

RD1, once unlocked, is inventory + safe exclusion only. Name-only heuristics such as `legacy`, `vendor`, `plugins`, `themes`, `Scripts`, `Content`, `old` or `packages` never create exclusion authority. UNKNOWN remains valid, discovery precedes expensive semantic scans, and tests remain evidence rather than production authority.

## E1

Only after E0 PASS, re-evaluate remaining semantic richness including the parked construction/default/computation candidate:

```text
branch  fix/product-value-construction-state
commit  35c8e5c5f856e15568aa963bb2d76268008c5570
```

Do not merge it during D or E0.

## E2

Only after E1 PASS: R7.14 positive unchanged-real-project yield, Level-2 known-answer benchmark, useful workspace-only PO/QC answers, grounded feature/workflow fidelity and portable parity/no-source-leak.

## Sequential execution rule

```text
finish current checkpoint
→ verify
→ review diff
→ coherent commit/push
→ exact-SHA gates
→ independent gate where required
→ update status/handoff
→ only then start next checkpoint
```

Independent inspection and non-conflicting verification may be parallelized inside the active checkpoint; production checkpoints may not overlap.

## Version semantics

```text
roadmap: V0.4.7-D / R7.10 repaired; exact-SHA gates PASS; independent rereview #18 pending
package: RuaDen.Pkc.Tool 0.4.3-preview.2
```
