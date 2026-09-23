# PKC Status

Last updated: 2026-09-23

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                 PASS / COMPLETE
V0.4.5 real-repository generalization            PASS / COMPLETE
V0.4.6 business logic reconstruction             PASS / COMPLETE
V0.4.7-A origin and copy timing                  PASS / COMPLETE
V0.4.7-B computation and later change            PASS / COMPLETE
V0.4.7-C backend to API                          PASS / COMPLETE
V0.4.7-D API to UI / R7.9 binding                PASS / COMPLETE
V0.4.7-D mutation-causality blocker              PASS / CLOSED
V0.4.7-D API to UI / R7.10 joint visibility      REPAIRED / ALL GATES PASS / PENDING REREVIEW #17
V0.4.7-D overall                                 PENDING INDEPENDENT REREVIEW #17
V0.4.7-E product acceptance                      LOCKED behind D
R7.14 real-project positive yield                NOT PASS / REQUIRED FOR E
real-repo safety benchmark                       PASS
real-repo product-value benchmark                NOT PASS / PARTIAL USEFULNESS
V0.5 Azure DevOps input evidence                 LOCKED
AI workspace + continuous-update plan            PREPARED / NOT UNLOCKED
```

V0.4.7 acceptance is defined in `docs/v0.4.7-acceptance-plan.md`; the permanent product contract is `docs/product-knowledge-contract.md`.

## Exact production candidate under review

```text
96205a9a643864facaf9642a3b390ddcdbed59d9
fix: tolerate duplicate Angular import aliases
```

Rereview #16 targeted `37a711...`, but implementation review found a new compile-valid R7.10 bypass before #16 completed. That request is superseded. Do not self-certify `96205...`; the next external gate is rereview #17.

## Accepted predecessors

```text
V0.4.6     c310e893762997f34562a6b3a62dbab2b05c0c93
V0.4.7-A   09a0c7f2a058adfd7ddb7b4ac2feb2d17580a429
V0.4.7-B   17fd30b3a4b8178208adabc12c40dee060bedb54
V0.4.7-C   fbb64b9917da1f63362558355201ff7998384ba0
R7.9       fc4bfa7042f59620b2c7ba1c4f6700cf3b02172a
mutation   67624944da27ff1f1f5a1154018a255aae11d1fe
```

Keep these closed unless a new real regression is demonstrated.

## Latest R7.10 repair

The new counterexample used a standalone Angular component whose `imports` reached an unresolved external component indirectly through a local helper/barrel. Existing filtering handled direct unresolved imports and same-file const arrays but did not close the local module import/re-export graph.

Repair chain:

```text
10876a28d15767930e0bc6de95def4993bb7265f  fix: close indirect Angular component import authority
d81be9560c741296271376aca7481db3d9925e41  fix: fail closed unsupported Angular import indirection
96205a9a643864facaf9642a3b390ddcdbed59d9  fix: tolerate duplicate Angular import aliases
```

The implementation now propagates projection risk through local imports, named/default re-exports, export-all and const-array closure. Unsupported scalar alias/default-re-export shapes fail closed rather than promoting render authority. Duplicate alias names no longer create a runtime exception path.

Focused regressions:

```text
tests/Pkc.CSharp.Tests/AngularComponentImportClosureAuthorityRegressionTests.cs
tests/Pkc.CSharp.Tests/AngularUnsupportedComponentImportIndirectionAuthorityRegressionTests.cs
```

## Exact-SHA verification

All standard gates passed on exact production `96205a9a643864facaf9642a3b390ddcdbed59d9`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35832501567 — PASS
pinned Loren                                35832501543 — PASS
Loren-main canary                           35832501552 — PASS
pinned Jellyfin + parity/provenance         35832501534 — PASS
```

```text
Release build        0 warnings / 0 errors
C# tests             259 / 259 PASS
frontend tests       13 / 13 PASS
tool pack/install    PASS
WorkPlay             PASS
PokeTrade            PASS
```

Pinned Jellyfin remains stable:

```text
facts                 43,365
relations             195,316
workflow candidates   386
product features      116
knowledge Markdown    504 files
analysis modes        43,365 / 43,365 project-semantic
portable parity       PASS
artifact              10737494915
sha256:94e17f2cb595c34586409aa279ff61457e8fd98c9c5835ef87ed0a68511a7c6a
```

## Real-repository benchmark

A one-line branch-trigger wrapper based directly on production `96205...` ran the unchanged pinned repositories:

```text
production  96205a9a643864facaf9642a3b390ddcdbed59d9
wrapper     65c02df67938197d929d30793dc012dbc3878ca3
run         35833147258 — PASS, 3 / 3
```

Observed outputs remain at the established baseline sizes:

```text
agentic-angular  441 facts / 660 relations / 26 knowledge files
jin12-crm        415 facts / 1,580 relations / 25 knowledge files
kesetovic-crm    488 facts / 2,054 relations / 28 knowledge files
```

Artifacts:

```text
agentic-angular  10737653171  sha256:da8ad00c539eb3887765d0314113c1c7287b97734012c2a39e22892757091f48
jin12-crm        10738280766  sha256:38bb0ae71b02e2c0045ee84785095606babcf471d1031244a705b40915b4b88f
kesetovic-crm    10737827547  sha256:726da3adada8742448ab87bf1bd1b8f86de35f000e452f2eca8a0edb422e0611
```

Agentic and Kesetovic upstream builds still fail on dependency vulnerability warnings-as-errors; PKC itself exits 0 and emits the same established benchmark counts. The production changes are fail-closed authority filters only; they do not manufacture new product evidence.

## Benchmark interpretation

Do not collapse benchmark results into one green/red bit.

```text
Safety / fail-closed stability        PASS
Backend workflow usefulness           PARTIAL / PROMISING
Feature-summary fidelity              NOT PASS
Cross-method/interface reconstruction NOT PASS
Object defaults/computations          NOT PASS
Side-effect synthesis                 NOT PASS
UI → API usefulness                   PARTIAL / REPO-DEPENDENT
R7.14 positive cross-layer yield      NOT PASS
Overall product-value benchmark       NOT PASS
```

Detailed known-answer scorecard:

`docs/benchmarks/2026-09-23-real-repo-product-value-scorecard.md`

## Current external gate

Required next gate:

```text
independent rereview #17 of exact 96205a9a643864facaf9642a3b390ddcdbed59d9
```

Request: `docs/reviews/2026-09-23-v0.4.7-d-r7.10-rereview-17-request.md`.

If rereview #17 finds no new compile-valid/runtime-valid false-positive blocker, it may mark R7.10 and all of D PASS / COMPLETE and unlock only E.

E must then use the known-answer scorecard as product-value acceptance evidence. R7.14 remains required and NOT PASS. V0.5 and the prepared AI-workspace/update initiative remain locked until V0.4.7 completes.

## Prepared future productization packet

```text
docs/plans/2026-09-22-ai-workspace-continuous-update-plan.md
docs/reviews/2026-09-22-ai-workspace-continuous-update-plan-self-review.md
```

After V0.4.7 is explicitly PASS / COMPLETE, preferred sequencing remains AI workspace + `run/verify`, then semantic `update/diff`, then Azure DevOps evidence.

## Version semantics

```text
roadmap:             V0.4.7-D / R7.10 pending independent rereview #17
tool/package:        RuaDen.Pkc.Tool 0.4.3-preview.2
C# raw schema:       0.4.4-csharp-raw
merged facts schema: 0.4.4
cross-stack schema:  0.4.6
frontend schema:     0.4.3-frontend
```
