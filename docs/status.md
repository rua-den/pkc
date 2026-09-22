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
V0.4.7-D mutation-causality blocker              PASS / CLOSED
V0.4.7-D API to UI / R7.10 joint visibility      REPAIRED / ALL GATES PASS / PENDING REREVIEW #11
V0.4.7-D overall                                 PENDING INDEPENDENT REREVIEW #11
V0.4.7-E product acceptance                      LOCKED behind D
R7.14 real-project positive yield                NOT PASS / REQUIRED FOR E
V0.5 Azure DevOps input evidence                 LOCKED
```

V0.4.7 acceptance is defined in `docs/v0.4.7-acceptance-plan.md`; the permanent product contract is `docs/product-knowledge-contract.md`.

## Exact production candidate under review

```text
a3334bc202ee5a9e2dc8cc6d7c176dde8e06c9f9
fix: reject SVG definition renders
```

A docs-only `[skip ci]` checkpoint may sit above this SHA on `main`. Review production behavior at `a3334bc...`; do not reset `main`.

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

## Permanent invariants

Keep business conditions, value lineage/provenance, mutation/causality, render authority and visibility authority distinct. Unsupported inference fails closed. Same/similar names are never sufficient proof. Stronger composition failure must preserve independently proven lower-authority evidence.

## R7.10 bounded proof

For the same exact R7.9-proven value path:

```text
Enumerable.Single/First(predicate)
→ exact selected reference local
→ exact scalar auto-property
→ direct API response property projection
→ explicit wire identity
→ exact frontend result/member/state identity
→ authoritative active directly rendered Angular text interpolation
→ exactly one supported enclosing @if
→ no unsupported raw-text / inert-definition / control / structural visibility authority
→ joint backend/frontend visibility evidence
```

Frontend visibility cannot upgrade an `observed-only` backend predicate. Zero, multiple, nested or unsupported visibility paths fail closed.

## Rereview #10 finding and repair

Independent rereview #10 of predecessor `1bc67e62f1099e4d580072682566c2d305e4db07` found a distinct SVG direct-render authority class.

Angular component templates can contain SVG and interpolation. However SVG definition containers such as `<defs>` and `<symbol>` do not directly render their child graphics/text to the user. The previous render-authority filter could still promote a simple interpolation under these containers as an authoritative `ui-member-render`, allowing false R7.9/R7.10 visible-render claims.

Regression-first repair:

```text
a3334bc202ee5a9e2dc8cc6d7c176dde8e06c9f9
fix: reject SVG definition renders
```

The bounded render-authority ancestor check now treats `style`, SVG `defs`, and SVG `symbol` as non-direct-render containers. A closed definition container does not suppress later genuine active text.

Regression coverage: `tests/Pkc.CSharp.Tests/AngularSvgDefinitionRenderAuthorityRegressionTests.cs`.

Review record: `docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-10.md`.

## Exact-SHA standard verification

All standard gates passed on exact production `a3334bc202ee5a9e2dc8cc6d7c176dde8e06c9f9`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35740454910 — PASS
pinned Loren                                35740454928 — PASS
Loren-main canary                           35740454970 — PASS
pinned Jellyfin + parity/provenance         35740454878 — PASS
```

```text
Release build        0 warnings / 0 errors
C# tests             201 / 201 PASS
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
analysis modes         43,365 / 43,365 project-semantic
portable parity       PASS
artifact              10698544024
sha256:dc679ba75f586ef7e7f0fd4b3ac8e0f79c70ab496132896b2c7bbdece9d50ef4
```

## Final pinned three-repository safety benchmark

```text
base production: a3334bc202ee5a9e2dc8cc6d7c176dde8e06c9f9
wrapper commit:  e01326c66fef67679b53af459e0bb9208b5dd3f4
run:             35740571925 — PASS, 3 / 3 jobs
```

The wrapper differs from production by exactly one workflow branch-trigger line. Direct artifact inspection for every pinned repository found:

```text
R7.9 rendered-value terminal: 0
selected API projection:      0
ui-member-visibility:         0
joint-visibility candidate:   0
combined visibility rule:     0
```

Interpretation:

```text
Safety / fail-closed: PASS — PKC does not manufacture unsupported cross-layer authority.
R7.14 positive real-project yield: NOT PASS — none of these three unchanged stress repos naturally contain the exact supported full shape.
```

Agentic mutation-causality remains closed: exactly two raw `UpdatedAt` mutations retain `runtime-pattern-variable / caller-object-unproven`, with zero feature-candidate, product-feature, or Markdown promotion.

## Current external gate

Required next gate:

```text
independent rereview #11 of exact a3334bc202ee5a9e2dc8cc6d7c176dde8e06c9f9
```

Request: `docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-11-request.md`.

If rereview #11 finds no new compile-valid/runtime-valid false-positive blocker, it may mark R7.10 and all of D PASS / COMPLETE and unlock only E. R7.14 remains required for E; V0.5 remains locked until E completes.

## Version semantics

```text
roadmap:             V0.4.7-D / R7.10 pending independent rereview #11
tool/package:        RuaDen.Pkc.Tool 0.4.3-preview.2
C# raw schema:       0.4.4-csharp-raw
merged facts schema: 0.4.4
cross-stack schema:  0.4.6
frontend schema:     0.4.3-frontend
```
