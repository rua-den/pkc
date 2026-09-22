# PKC Status

Last updated: 2026-09-22

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                 PASS / COMPLETE
V0.4.5 real-repository generalization            PASS / COMPLETE
V0.4.6 business logic reconstruction             PASS / COMPLETE
V0.4.7-A origin and copy timing                  PASS / COMPLETE
V0.4.7-B computation and later change            PASS / COMPLETE
V0.4.7-C backend to API                          PASS / COMPLETE
V0.4.7-D API to UI / R7.9 binding                PASS / COMPLETE
V0.4.7-D mutation-causality benchmark blocker    PASS / CLOSED
V0.4.7-D API to UI / R7.10 joint visibility      REPAIRED / ALL GATES PASS / PENDING REREVIEW #7
V0.4.7-D overall                                 PENDING INDEPENDENT REREVIEW #7
V0.4.7-E product acceptance                      LOCKED behind D
R7.14 real-project positive yield                NOT PASS / REQUIRED FOR E
V0.5 Azure DevOps input evidence                 LOCKED
```

V0.4.7 acceptance is defined in `docs/v0.4.7-acceptance-plan.md`. The permanent product contract is `docs/product-knowledge-contract.md`.

## Exact production candidate under review

```text
5d43b180e09cc7026919a4dff2563f85e39e1b82
fix: fail closed on Angular hidden bindings
```

A docs-only `[skip ci]` checkpoint may sit above this SHA on `main`. Review production behavior at `5d43b180...`; do not reset `main`.

## Accepted predecessors

```text
V0.4.6  c310e893762997f34562a6b3a62dbab2b05c0c93
V0.4.7-A 09a0c7f2a058adfd7ddb7b4ac2feb2d17580a429
V0.4.7-B 17fd30b3a4b8178208adabc12c40dee060bedb54
V0.4.7-C fbb64b9917da1f63362558355201ff7998384ba0
V0.4.7-D / R7.9 baseline fc4bfa7042f59620b2c7ba1c4f6700cf3b02172a
mutation-causality repair 67624944da27ff1f1f5a1154018a255aae11d1fe
```

Keep these closed unless a real regression is demonstrated.

## Permanent invariants

Keep business conditions, value lineage/provenance, mutation/causality, and presentation authority distinct. Unsupported inference fails closed. Same/similar names are never sufficient proof. Stronger composition failure must preserve independently proven lower-authority evidence.

## R7.10 bounded proof

For the same exact R7.9-proven value path:

```text
Enumerable.Single/First(predicate)
→ exact selected reference local
→ exact scalar auto-property
→ direct API response property projection
→ explicit wire identity
→ exact frontend result/member/state identity
→ authoritative active rendered Angular text interpolation
→ one supported enclosing @if
→ joint backend/frontend visibility evidence
```

Frontend visibility cannot upgrade an `observed-only` backend predicate. Unsupported or ambiguous presentation/control-flow structure fails closed.

## Rereview history through #6

Rereviews #1–#5 successively repaired:

1. inert `<ng-template>`, HTML comments, and tag/attribute interpolation;
2. static HTML `hidden` ancestry;
3. static inline `display:none` ancestry;
4. static inline `visibility:hidden` ancestry;
5. legacy inert `<template>` fragments when `enableLegacyTemplate=true`.

Independent rereview #6 challenged exact production `e54b8444c3d14e647bcc0a9fe23d8f6e1905865b` and found a new compile-valid/runtime-valid false-authority class:

```html
<section [hidden]="true">
  @if (displayPrice > 0) {
    <strong>{{ displayPrice }}</strong>
  }
</section>
```

Angular sets the native element `hidden` property to true, so the subtree is not presented. `e54b8444...` modeled static `hidden` but not Angular hidden bindings, allowing the interpolation to retain `renderAuthority`, receive `ui-member-visibility`, and potentially feed false R7.9/R7.10 authority.

Review record:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-6.md`

## Angular hidden-binding repair

Production repair:

```text
5d43b180e09cc7026919a4dff2563f85e39e1b82
fix: fail closed on Angular hidden bindings
```

The render-authority boundary now treats the native `hidden` family conservatively:

```text
[hidden]="false" / bind-hidden="false"   → supported positive safeguard
[hidden]="true"                          → reject render authority
bind-hidden="true"                       → reject render authority
[hidden]="expression"                    → fail closed
[hidden]="False"                         → fail closed; Angular expressions are case-sensitive
hidden="{{ expression }}"                → fail closed
[attr.hidden]="expression"               → fail closed
bind-attr.hidden="expression"            → fail closed
```

The repair does not broaden into stylesheet/class CSS, computed browser styles, arbitrary property bindings, signals, outlets, or general runtime DOM solving.

Regression coverage:

`tests/Pkc.CSharp.Tests/AngularHiddenBindingRenderAuthorityRegressionTests.cs`

The final production history contains one coherent implementation commit directly above the prior docs handoff, with exactly the production filter and regression test changed.

## Exact-SHA standard verification

All standard gates passed on exact production SHA `5d43b180e09cc7026919a4dff2563f85e39e1b82`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35702383031 — PASS
pinned Loren                                35702383035 — PASS
Loren-main canary                           35702383053 — PASS
pinned Jellyfin + parity/provenance         35702383040 — PASS
```

Core CI evidence:

```text
Release build        0 warnings / 0 errors
C# tests             178 / 178 PASS
frontend tests       13 / 13 PASS
tool pack/install    PASS
WorkPlay             PASS
PokeTrade            PASS
```

Pinned Jellyfin evidence:

```text
source build          PASS, 0 warnings / 0 errors
facts                 43,365
relations             195,316
workflow candidates   386
product features      116
knowledge Markdown    504 files
analysis modes         43,365 / 43,365 project-semantic
portable parity       PASS
artifact              10683620529
sha256:992ebd2005ae40c3a587a0ec80515dfe4350ee8e490f635f07cc2950451dd178
```

## Repaired pinned three-repository benchmark

Temporary wrapper branch based exactly on production SHA `5d43b180...`:

```text
branch:         benchmark/r710-hidden-binding-5d43b180
wrapper commit: b22c846ecebb75c6f52533c280ba4acbbe453657
run:            35702503746 — PASS, 3 / 3 jobs
```

GitHub compare confirms the wrapper differs only by the benchmark workflow branch-trigger line. Direct artifact inspection for every pinned repository found:

```text
R7.9 rendered-value terminal: 0
selected API projection:      0
ui-member-visibility:         0
joint-visibility candidate:   0
combined visibility rule:     0
```

Agentic mutation-causality remains closed: exactly 2 raw `UpdatedAt` mutations, 0 candidate mutation promotion, with `runtime-pattern-variable` and `caller-object-unproven` retained.

Detailed benchmark record: `docs/benchmarks/2026-09-22-r7.10-real-repo-benchmark.md`.

This benchmark remains fail-closed stability evidence only. R7.14 remains **NOT PASS**.

## Current external gate

The implementation session repaired the rereview #6 blocker and must not self-certify its own repair. Required next gate:

```text
independent rereview #7 of exact 5d43b180e09cc7026919a4dff2563f85e39e1b82
```

Request: `docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-7-request.md`.

If rereview #7 finds no new compile-valid/runtime-valid false-positive blocker, it may mark R7.10 and all of D PASS / COMPLETE and unlock only E. R7.14 remains required and NOT PASS; V0.5 remains locked until E completes.

## Version semantics

```text
roadmap:             V0.4.7-D / R7.10 pending independent rereview #7
tool/package:        RuaDen.Pkc.Tool 0.4.3-preview.2
C# raw schema:       0.4.4-csharp-raw
merged facts schema: 0.4.4
cross-stack schema:  0.4.6
frontend schema:     0.4.3-frontend
```
