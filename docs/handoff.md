# PKC Handoff

Last updated: 2026-09-22

Use this file when continuing PKC in another coding/review thread.

## Read first

Read in this order before changing production code:

1. `docs/status.md`
2. this handoff
3. `docs/milestones.md`
4. `docs/product-knowledge-contract.md`
5. `docs/v0.4.7-acceptance-plan.md`
6. `docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-1.md`
7. `docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-2.md`
8. `docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-3.md`
9. `docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-4.md`
10. `docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-5.md`
11. `docs/benchmarks/2026-09-22-r7.10-real-repo-benchmark.md`
12. `docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-6-request.md`

Then inspect current `main`, recent commits and repository status. Never reset to a historical SHA merely because this handoff names it.

## Current production checkpoint

Exact production SHA for fresh independent rereview:

```text
e54b8444c3d14e647bcc0a9fe23d8f6e1905865b
fix: reject inert legacy template renders
```

A docs-only `[skip ci]` checkpoint may sit above this SHA on `main`. Review production behavior at `e54b8444...`; do not reset `main`.

## Current milestone state

```text
V0.4.7-A                                PASS / COMPLETE
V0.4.7-B                                PASS / COMPLETE
V0.4.7-C                                PASS / COMPLETE
V0.4.7-D / R7.9                         PASS / COMPLETE
V0.4.7-D mutation-causality blocker     PASS / CLOSED
V0.4.7-D / R7.10                        REPAIRED / ALL GATES PASS / PENDING REREVIEW #6
V0.4.7-D overall                        PENDING INDEPENDENT REREVIEW #6
V0.4.7-E                                LOCKED behind D
R7.14 real-project positive yield       NOT PASS / REQUIRED FOR E
V0.5 Azure DevOps                       LOCKED
```

Do not start E or V0.5 before the independent D outcome.

## Permanent contract

Keep business conditions, value lineage/provenance, mutation/causality, and presentation authority distinct. Unsupported inference fails closed. Same/similar names are not proof. If stronger composition fails, preserve independently proven lower-authority evidence.

## Accepted predecessors

```text
V0.4.6     c310e893762997f34562a6b3a62dbab2b05c0c93
V0.4.7-A   09a0c7f2a058adfd7ddb7b4ac2feb2d17580a429
V0.4.7-B   17fd30b3a4b8178208adabc12c40dee060bedb54
V0.4.7-C   fbb64b9917da1f63362558355201ff7998384ba0
R7.9       fc4bfa7042f59620b2c7ba1c4f6700cf3b02172a
mutation   67624944da27ff1f1f5a1154018a255aae11d1fe
```

Keep these closed unless a real regression is demonstrated.

## R7.10 bounded positive

R7.10 answers, for the same exact R7.9-proven value path:

> What backend selection condition and frontend visibility condition jointly determine whether this rendered value is visible?

Supported backend remains the narrow target-project-semantic `Enumerable.Single/First(predicate)` → exact selected local → direct response property projection path. R7.9 supplies explicit wire identity, typed result member, exact assignment and authoritative active rendered text interpolation. R7.10 accepts one supported enclosing Angular `@if` and composes only exact fact IDs.

Frontend evidence cannot upgrade an `observed-only` backend condition. Unsupported or ambiguous structure fails closed.

## Rereview #5 finding

Independent rereview #5 challenged exact production `1fc4d212...` and found a distinct inert-fragment boundary:

```html
<template>
  @if (displayPrice > 0) {
    <strong>{{ displayPrice }}</strong>
  }
</template>
```

Fixture configuration:

```json
{
  "angularCompilerOptions": {
    "enableLegacyTemplate": true
  }
}
```

With legacy template support enabled, `<template>` is an inert template fragment. The previous authority filter recognized only `<ng-template>`, so the interpolation could incorrectly retain authoritative `ui-member-render`; the enclosing `@if` could then feed false R7.9/R7.10 PO-facing authority.

Review record:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-5.md`

## Legacy-template repair completed

Production checkpoint:

```text
e54b8444c3d14e647bcc0a9fe23d8f6e1905865b
fix: reject inert legacy template renders
```

The repair stays at the R7.9 render-authority boundary. The inert template-fragment parser now treats both `<ng-template>` and legacy `<template>` as non-rendered fragment containers. It does not change backend predicate authority, R7.10 exact-ID composition, CSS support, or runtime DOM claims.

Regression file:

`tests/Pkc.CSharp.Tests/LegacyTemplateRenderAuthorityRegressionTests.cs`

Coverage proves:

- interpolation and `@if` inside a legacy `<template>` do not become authoritative render/visibility facts;
- a closed legacy `<template>` sibling does not incorrectly suppress a later active interpolation.

The fixture explicitly enables `angularCompilerOptions.enableLegacyTemplate=true`.

The connector briefly created a test-only commit while editing. Before final verification, `main` was rewritten so the final history contains one coherent implementation commit directly above the prior docs handoff. Compare from `c7011acb...` to `e54b8444...` shows exactly two files: production filter + regression test.

## Exact-SHA verification

Exact production `e54b8444c3d14e647bcc0a9fe23d8f6e1905865b`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35699298170 — PASS
pinned Loren                                35699298080 — PASS
Loren-main canary                           35699298091 — PASS
pinned Jellyfin + parity/provenance         35699298089 — PASS
```

Core CI:

```text
Release build        0 warnings / 0 errors
C# tests             169 / 169 PASS
frontend tests       13 / 13 PASS
tool pack/install    PASS
WorkPlay             PASS
PokeTrade            PASS
```

Pinned Jellyfin:

```text
source build          PASS, 0 warnings / 0 errors
43,365 facts | 195,316 relations | 386 workflow candidates
116 product features | 504 knowledge Markdown files
43,365 / 43,365 facts project-semantic
portable parity/no-leak PASS
artifact 10681845827
sha256:cd6ffdb41b6652777064a51cc3f76a16cd44aa948e1044fa5a0d49ce1420b5f1
```

## Repaired real-repository benchmark

Wrapper based exactly on `e54b8444...`:

```text
branch:         benchmark/r710-legacy-template-e54b8444
wrapper commit: cee6bef95e4e2b7a86b483057d0672d9be728e52
run:            35699541953 — PASS, 3 / 3 jobs
```

GitHub compare proves the wrapper changes only one branch-trigger line in `.github/workflows/real-repo-benchmark.yml`; production and tests are byte-identical to `e54b8444...`.

Artifacts:

```text
jin12-xyz/CRM
artifact 10681654421
sha256:b49d66ce422707fbe237125aaf7842c9e64556e6e99ac003805c8a9f402d5cfc

hackersandwizards/agentic-engineering-training-angular
artifact 10682330270
sha256:7b36dbccda49b210a68d9fee917ecbbd4b06489371944fed808da2b200b7dd12

kesetovic/crm-system
artifact 10681677291
sha256:6a1492b58809b5963500a4f9e29839bf8de86fc3822508d2dbb433f13f3d6b99
```

Direct artifact inspection for every repository:

```text
R7.9 rendered UI terminal: 0
selected API projection:  0
ui-member-visibility:     0
joint-visibility:         0
combined visibility rule: 0
```

Agentic still contains exactly two raw `UpdatedAt` mutations with `runtime-pattern-variable` / `caller-object-unproven`, and zero candidate mutation promotion. The mutation-causality blocker remains closed.

This benchmark is fail-closed stress evidence only and does not satisfy R7.14. R7.14 remains **NOT PASS**.

## Next action — independent rereview #6

Review exact production SHA:

```text
e54b8444c3d14e647bcc0a9fe23d8f6e1905865b
```

Request:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-6-request.md`

The reviewer must independently search for a new compile-valid/runtime-valid counterexample rather than merely re-confirming legacy `<template>`.

A feature being unsupported and failing closed is not a blocker. A blocker requires actual false or over-authoritative output from a compile-valid/runtime-valid shape.

If no blocker exists:

```text
mark R7.10 PASS / COMPLETE
→ mark all V0.4.7-D PASS / COMPLETE
→ unlock only V0.4.7-E
→ keep R7.14 NOT PASS and required for E
→ keep V0.5 locked until E completes
```

If a blocker exists, keep E locked and require regression-first minimum generic repair.

This implementation session must not self-certify `e54b8444...`.
