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
V0.4.7-D API to UI / R7.10 joint visibility      REPAIRED / ALL GATES PASS / PENDING REREVIEW #6
V0.4.7-D overall                                 PENDING INDEPENDENT REREVIEW #6
V0.4.7-E product acceptance                      LOCKED behind D
R7.14 real-project positive yield                NOT PASS / REQUIRED FOR E
V0.5 Azure DevOps input evidence                 LOCKED
```

V0.4.7 acceptance is defined in `docs/v0.4.7-acceptance-plan.md`. The permanent product contract is `docs/product-knowledge-contract.md`.

## Exact production candidate under review

```text
e54b8444c3d14e647bcc0a9fe23d8f6e1905865b
fix: reject inert legacy template renders
```

A docs-only `[skip ci]` checkpoint may sit above this SHA on `main`. Review production behavior at `e54b8444...`; do not reset `main`.

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

## R7.10 intended bounded proof

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

Frontend visibility cannot upgrade an `observed-only` backend predicate. Zero, multiple, nested or otherwise unsupported visibility paths fail closed.

## Rereview history

Rereview #1 repaired false render authority for inert `<ng-template>`, HTML comments, and HTML tag/attribute interpolation.

Rereview #2 repaired static HTML `hidden` ancestry at `97161baa2d0aff9131a7acf9db752393ae913d64`.

Rereview #3 repaired static inline `display:none` at `dc69e44206942ffb0012e1994d6d39249d1db4be`.

Rereview #4 repaired static inline `visibility:hidden` at `1fc4d212b9c7add2f012f51adf3eef0c16f34dae`.

Rereview #5 independently found a distinct compile-valid/runtime-valid inert-fragment case when Angular legacy template support is enabled:

```html
<template>
  @if (displayPrice > 0) {
    <strong>{{ displayPrice }}</strong>
  }
</template>
```

With `angularCompilerOptions.enableLegacyTemplate=true`, legacy `<template>` is an inert template fragment equivalent to the already-recognized `<ng-template>` boundary. `1fc4d212...` recognized only `<ng-template>`, so interpolation inside legacy `<template>` could receive authoritative `ui-member-render` and feed false R7.9/R7.10 authority.

Review record:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-5.md`

## Legacy-template repair

Production repair:

```text
e54b8444c3d14e647bcc0a9fe23d8f6e1905865b
fix: reject inert legacy template renders
```

The render-authority filter now treats both `<ng-template>` and legacy `<template>` as inert template-fragment boundaries. The repair is intentionally narrow: it does not broaden CSS/runtime claims or change R7.10 identity composition.

Regression coverage:

`tests/Pkc.CSharp.Tests/LegacyTemplateRenderAuthorityRegressionTests.cs`

It proves:

```text
legacy <template> containing @if + interpolation
→ no authoritative ui-member-render
→ no ui-member-visibility

closed legacy <template> sibling
→ does not suppress a later active @if interpolation
```

The test fixture explicitly enables `angularCompilerOptions.enableLegacyTemplate=true`.

Local .NET execution was unavailable in the implementation environment. An accidental temporary red-only test commit was removed from `main`; final history contains one coherent implementation commit from the prior docs handoff, with exactly the production filter and regression file changed.

## Exact-SHA standard verification

All standard gates passed on exact production SHA `e54b8444c3d14e647bcc0a9fe23d8f6e1905865b`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35699298170 — PASS
pinned Loren                                35699298080 — PASS
Loren-main canary                           35699298091 — PASS
pinned Jellyfin + parity/provenance         35699298089 — PASS
```

Core CI evidence:

```text
Release build        0 warnings / 0 errors
C# tests             169 / 169 PASS
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
artifact              10681845827
sha256:cd6ffdb41b6652777064a51cc3f76a16cd44aa948e1044fa5a0d49ce1420b5f1
```

## Repaired pinned three-repository benchmark

Temporary wrapper branch based exactly on production SHA `e54b8444...`:

```text
branch:         benchmark/r710-legacy-template-e54b8444
wrapper commit: cee6bef95e4e2b7a86b483057d0672d9be728e52
run:            35699541953 — PASS, 3 / 3 jobs
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

This benchmark is fail-closed stability evidence only. R7.14 remains **NOT PASS**.

## Current external gate

The implementation session repaired the rereview #5 blocker and must not self-certify its own repair. Required next gate:

```text
independent rereview #6 of exact e54b8444c3d14e647bcc0a9fe23d8f6e1905865b
```

Request: `docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-6-request.md`.

If rereview #6 finds no new compile-valid/runtime-valid false-positive blocker, it may mark R7.10 and all of D PASS / COMPLETE and unlock only E. R7.14 remains required and NOT PASS; V0.5 remains locked until E completes.

## Version semantics

```text
roadmap:             V0.4.7-D / R7.10 pending independent rereview #6
tool/package:        RuaDen.Pkc.Tool 0.4.3-preview.2
C# raw schema:       0.4.4-csharp-raw
merged facts schema: 0.4.4
cross-stack schema:  0.4.6
frontend schema:     0.4.3-frontend
```
