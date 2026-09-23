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
V0.4.7-D API to UI / R7.10 joint visibility      REPAIRED / ALL GATES PASS / PENDING REREVIEW #16
V0.4.7-D overall                                 PENDING INDEPENDENT REREVIEW #16
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
37a71172c8c425219aef35f5843ac2109810ae9d
fix: preserve Angular infrastructure render evidence
```

Relevant continuation repair chain after the #15 request:

```text
46dcf8941c67a11d96e4416774d02e555a8f6aaf  fix: bound linked Angular package selectors
2ea361ba9d5e6e61041f38ca79e6587e12a1b745  fix: fail closed unresolved Angular component imports
37a71172c8c425219aef35f5843ac2109810ae9d  fix: preserve Angular infrastructure render evidence
```

A docs-only `[skip ci]` checkpoint may sit above production on `main`. Review production behavior at `37a711...`; do not reset `main`.

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

## Latest R7.10 repairs

Continuation review after the #15 request found three remaining package/import authority seams:

1. linked/workspace packages could resolve outside `node_modules` while still being valid application dependencies;
2. unresolved external symbols used in standalone component `imports` could hide an unknown component projection boundary;
3. fail-closed handling of unresolved packages could over-filter normal Angular infrastructure imports and const-array `imports`.

The repair chain above now accepts linked package metadata when its final target remains inside the repository, fails closed when a linked package escapes repository scope, fails closed when an unresolved external symbol is actually used in component `imports`, preserves known Angular framework infrastructure imports, and follows direct or bounded const-array component imports.

Focused regressions:

```text
tests/Pkc.CSharp.Tests/AngularNestedNodeModulesProjectionAuthorityRegressionTests.cs
tests/Pkc.CSharp.Tests/AngularUnresolvedExternalComponentImportAuthorityRegressionTests.cs
```

## Exact-SHA verification

All required standard gates passed on exact production `37a71172c8c425219aef35f5843ac2109810ae9d`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35823346084 — PASS
pinned Loren                                35823346060 — PASS
Loren-main canary                           35823346077 — PASS
pinned Jellyfin + parity/provenance         35823346046 — PASS
```

```text
Release build        0 warnings / 0 errors
C# tests             253 / 253 PASS
frontend tests       13 / 13 PASS
tool pack/install    PASS
WorkPlay             PASS
PokeTrade            PASS
```

Pinned Jellyfin:

```text
source build          PASS, 0 warnings / 0 errors
facts                 43,365
relations             195,316
workflow candidates   386
product features      116
knowledge Markdown    504 files
analysis modes        43,365 / 43,365 project-semantic
portable parity       PASS
artifact              10734541254
sha256:f071f9841ffc4d95edd78d0334ed703c0ff53e7c55271e68facea47ca59d45a6
```

## Real-repository benchmark

Exact production `37a711...` was benchmarked through a wrapper that changes only the branch trigger:

```text
production:      37a71172c8c425219aef35f5843ac2109810ae9d
wrapper:         f76c946e15cfe37d380745e0a2efb22cd3fc9123
run:             35823872896 — PASS, 3 / 3
```

Artifacts:

```text
agentic-angular  10734561505  sha256:b15120b5b17580ed9defe3600147ed0998f5cd9e121f8a6b6cc02297fec5daf4
jin12-crm        10734496662  sha256:36aa1772e18ebba7c3911429edbf51eb51c256f16065f0563b4c15d59b4ec8d1
kesetovic-crm    10733409752  sha256:d1f90d1cb1eb97d5ff6129134254bc11981faf28d78fd92fd9617a1a4f212fb0
```

Canonical evidence is byte-stable versus the preceding safety benchmark for all three repos. No new unsupported R7.9/R7.10 authority appears.

### Benchmark interpretation

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

A benchmark is not product-passing merely because jobs are green, output files exist, or authority positives are zero.

## Current external gate

Required next gate:

```text
independent rereview #16 of exact 37a71172c8c425219aef35f5843ac2109810ae9d
```

Request: `docs/reviews/2026-09-23-v0.4.7-d-r7.10-rereview-16-request.md`.

If rereview #16 finds no new compile-valid/runtime-valid false-positive blocker, it may mark R7.10 and all of D PASS / COMPLETE and unlock only E.

E must then use the known-answer scorecard as product-value acceptance evidence. R7.14 remains required and NOT PASS. V0.5 and the prepared AI-workspace/update initiative remain locked until V0.4.7 completes.

This implementation/benchmark continuation must not self-certify its own production candidate.

## Prepared future productization packet

```text
docs/plans/2026-09-22-ai-workspace-continuous-update-plan.md
docs/reviews/2026-09-22-ai-workspace-continuous-update-plan-self-review.md
```

After V0.4.7 is explicitly PASS / COMPLETE, preferred sequencing remains AI workspace + `run/verify`, then semantic `update/diff`, then Azure DevOps evidence.

## Version semantics

```text
roadmap:             V0.4.7-D / R7.10 pending independent rereview #16
tool/package:        RuaDen.Pkc.Tool 0.4.3-preview.2
C# raw schema:       0.4.4-csharp-raw
merged facts schema: 0.4.4
cross-stack schema:  0.4.6
frontend schema:     0.4.3-frontend
```
