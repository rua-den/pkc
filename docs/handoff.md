# PKC Handoff

Last updated: 2026-09-23

This handoff is for the next PKC coding or audit session.

## Read first

1. root `CLAUDE.md`
2. `AGENTS.md`
3. `docs/status.md`
4. this handoff
5. `docs/milestones.md`
6. `docs/product-knowledge-contract.md`
7. `docs/v0.4.7-acceptance-plan.md`
8. `docs/benchmarks/product-value-benchmark-protocol.md`
9. `docs/benchmarks/2026-09-23-product-value-level2-and-fix4-prework.md`
10. `docs/reviews/2026-09-23-v0.4.7-d-r7.10-rereview-17-request.md` only when explicitly assigned the formal independent-review role

Then inspect current `main`, recent commits, production code and relevant regressions. Do not reset to an old SHA merely because this handoff names one.

## Formal acceptance state

```text
A/B/C                    PASS / COMPLETE
R7.9                     PASS / COMPLETE
mutation-causality       PASS / CLOSED
R7.10                    REPAIRED / ALL GATES PASS / PENDING REREVIEW #17
V0.4.7-D                 PENDING INDEPENDENT REREVIEW #17
V0.4.7-E                 LOCKED
R7.14                    NOT PASS / REQUIRED FOR E
V0.5                     LOCKED
```

Formal R7.10 review target:

```text
96205a9a643864facaf9642a3b390ddcdbed59d9
fix: tolerate duplicate Angular import aliases
```

The product-value thread MUST NOT self-certify this gate. The request explicitly requires an independent rereview.

## Current main production code

Production code checkpoint below this docs-only handoff:

```text
79b0f9fec80a2afb87f43ec7a559a5d54cb87863
fix: surface pkc run progress
```

Exact-SHA gates are green:

```text
CI / full tests / WorkPlay / PokeTrade  35898219948 PASS
Loren pinned                            35898219784 PASS
Loren-main canary                       35898219957 PASS
Jellyfin                                35898219943 PASS
```

The CLI progress fix writes long-running phase progress to stderr and preserves stdout result/path behavior. Real PokeTrade CI showed C# scan, frontend scan, merge/link, synthesis and output phases visibly progressing.

## Product-value Fix #1

PASS / COMPLETE for the bounded direct-DI authority path:

```text
controller / endpoint interface call
→ exact interface method
→ one authoritative direct host registration
→ exact concrete implementation
→ concrete guards / mutations / downstream calls
```

Keep all existing fail-closed registration/host/collision boundaries.

Reference report:

`docs/benchmarks/2026-09-23-fix1-jin12-di-product-value-benchmark.md`

## Product-value Fix #2

PASS / COMPLETE for bounded Angular URL-expression resolution and child Output-event bridging.

Supported chain:

```text
child action
→ exact @Output + discriminator
→ parent template event handler
→ exact switch branch
→ parent method
→ exact injected service
→ URL expression normalization
→ API call
→ backend endpoint
```

Ambiguous/unsupported event, selector, receiver or URL shapes remain fail-closed.

## Product-value Fix #3

PASS / COMPLETE for the bounded Agentic email displayed-value path.

Pinned target:

```text
hackersandwizards/agentic-engineering-training-angular
22f2aab64617f4de7984370a5bd40e8c9535dbf5
```

Targeted Level-1 run:

```text
35899560821 PASS
```

Workspace-only Phase 1 proved:

```text
MAT_DIALOG_DATA.data.email
→ EditUserDialogComponent.form.email
→ EditUserDialogComponent.displayed.email
```

Targeted score moved Q3 `50% → 100%` and bounded Q4 `90% → 100%`. Phase 2 source cross-check found no mismatch for that chain.

Do not generalize this result to unrelated Agentic `UpdatedAt` or other unsupported displayed-value paths.

## Level-2 corpus checkpoint

Full benchmark:

```text
branch   benchmark/product-value-level2-79b0
wrapper  749e24406d050a2b738751e086f02320cd28cf86
run      35900111059 PASS, 3 / 3
```

Selected-probe diagnostic result:

```text
Agentic Users Update     ~94.5%
Jin12 Contacts Update    100.0%
Kesetovic PackOrder       82.5%
selected-probe aggregate ~91.6%
backend PO/QC core       ~95.3%
```

These scores are diagnostic only. Gate B remains NOT PASS because the corpus still has independent gaps:

- construction/default/computation state;
- semantic side effects (`OrderSignal` is the concrete known example);
- feature-summary rule fidelity;
- R7.14 positive unchanged-real-project cross-layer yield.

Detailed report:

`docs/benchmarks/2026-09-23-product-value-level2-and-fix4-prework.md`

## Fix #4 prework candidate — DO NOT MERGE YET

This was prepared because Level-2 exposed construction/default/computation as a high-value gap, but the formal acceptance plan still locks E behind D. Therefore this candidate remains off `main`.

Implementation:

```text
branch  fix/product-value-construction-state
commit  35c8e5c5f856e15568aa963bb2d76268008c5570
        fix: prove observable constructed state
```

Production-code delta is only:

```text
src/Pkc.CSharp/CSharpEvidenceScanner.cs
src/Pkc.CSharp/CSharpObservableConstructionEnricher.cs
tests/Pkc.CSharp.Tests/ObservableConstructionStateRegressionTests.cs
```

Authority rule:

Promote object-initializer member assignments only when exact target-project Roslyn identity proves the same local object is passed whole to both a downstream invocation and an invocation on the return path. Return-only construction and a downstream invocation involving another local fail closed.

Validation evidence for the unchanged implementation commit:

```text
Release build          PASS, 0 warnings / 0 errors
focused regressions    8 / 8 PASS
C# full suite          282 / 282 PASS
frontend full suite     23 / 23 PASS
```

First wrapper run `35901814917` reached all of the above PASS states, then a transient pinned-target restore failed on AutoMapper 12.0.1 NU1903 advisory before PKC generation.

A wrapper-only retry did not alter implementation:

```text
wrapper  3529c3a87597cb58ac0cd7f317c39d6311bddff3
run      35902984101 PASS
```

The retry target build succeeded and the generated Kesetovic `AddOrder` workflow contains the known construction behavior:

```text
newOrder.OrderStatus = OrderStatus.NEW
newOrder.BonusAwarded = orderDto.OrderPrice * 0.05
newOrder.CustomerName = orderDto.CustomerName
```

This proves the candidate technically works for the target gap. It is still NOT production and MUST stay unmerged while E is locked.

Potential performance note: the new semantic enricher adds another MSBuild semantic pass. Full C# suite on the candidate ran 6m08 versus roughly 4m45 on the preceding main CI. If E is unlocked and this candidate is adopted, consider prefiltering to endpoints that already own initializer-mutation candidates before opening project models; do not weaken authority to optimize runtime.

## Workspace / privacy boundary

Preferred product flow:

```text
pkc run <TEAM_REPOSITORY_PATH>
cd <TEAM_REPOSITORY_PATH>/.pkc/workspace
```

- PRODUCT/TRACE: workspace only, no source fallback.
- Benchmark phase 1: workspace only and freeze answer before source inspection.
- Phase 2/source cross-check: minimum necessary source only in the approved environment.
- Do not copy proprietary source bodies into benchmark reports.

## Git / CI discipline

For one logical repair:

```text
inspect
→ regression
→ generic fix
→ focused verification
→ related verification
→ broader verification
→ diff review
→ one coherent implementation commit/push
→ CI final verification
```

Do not use Actions as the ordinary edit-test loop.

## Exact next action

The current checkpoint has reached an external formal gate.

```text
independent rereview #17 of exact 96205a9a643864facaf9642a3b390ddcdbed59d9
```

Decision rule from the acceptance plan:

```text
PASS → mark R7.10 + V0.4.7-D PASS / COMPLETE; unlock only E
FAIL → regression-first minimum generic R7.10 repair; rerun exact-SHA gates and rereview
```

Do not merge Fix #4 or begin any other E-only work until that independent gate explicitly passes.
