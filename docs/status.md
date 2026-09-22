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
V0.4.7-D API to UI / R7.10 joint visibility      REPAIRED / ALL GATES PASS / PENDING REREVIEW #8
V0.4.7-D overall                                 PENDING INDEPENDENT REREVIEW #8
V0.4.7-E product acceptance                      LOCKED behind D
R7.14 real-project positive yield                NOT PASS / REQUIRED FOR E
V0.5 Azure DevOps input evidence                 LOCKED
```

V0.4.7 acceptance is defined in `docs/v0.4.7-acceptance-plan.md`. The permanent product contract is `docs/product-knowledge-contract.md`.

## Exact production candidate under review

```text
d785807dfa053a5abd1e3b6500a2fc1bd729d38a
fix: ignore comment braces in Angular visibility scope
```

A docs-only `[skip ci]` checkpoint may sit above this SHA on `main`. Review production behavior at `d785807d...`; do not reset `main`.

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

Frontend visibility cannot upgrade an `observed-only` backend predicate. Zero, multiple, nested or unsupported visibility paths fail closed.

## Rereview history through #7

Previous adversarial rereviews successively repaired:

1. inert `<ng-template>`, HTML comments, tag/attribute interpolation;
2. static HTML `hidden` ancestry;
3. static inline `display:none` ancestry;
4. static inline `visibility:hidden` ancestry;
5. legacy inert `<template>` with `enableLegacyTemplate=true`;
6. Angular native hidden bindings / ambiguous dynamic hidden forms;
7. HTML-comment braces corrupting bounded Angular `@if` scope matching.

Rereview #7 found this distinct compile-valid/runtime-valid control-flow shape on production `5d43b180e09cc7026919a4dff2563f85e39e1b82`:

```html
@if (isAllowed) {
  <!-- { -->
}
<strong>{{ displayPrice }}</strong>
<!-- } -->
```

The interpolation is outside the actual `@if`, but the old brace matcher counted braces inside HTML comments and could emit false `ui-member-visibility(isAllowed)` authority.

Review record:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-7.md`

## Comment-brace scope repair

Production repair:

```text
d785807dfa053a5abd1e3b6500a2fc1bd729d38a
fix: ignore comment braces in Angular visibility scope
```

`AngularRenderedMemberVisibilityEnricher.FindMatchingBrace` now skips complete `<!-- ... -->` regions while counting control-flow braces and fails closed on an unterminated comment. Regression coverage is in `tests/Pkc.CSharp.Tests/AngularIfCommentBraceVisibilityRegressionTests.cs`, including both false-scope and real-scope positive safeguards.

## Exact-SHA standard verification

All standard gates passed on exact production `d785807dfa053a5abd1e3b6500a2fc1bd729d38a`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35703893704 — PASS
pinned Loren                                35703893679 — PASS
Loren-main canary                           35703893712 — PASS
pinned Jellyfin + parity/provenance         35703893678 — PASS
```

```text
Release build        0 warnings / 0 errors
C# tests             180 / 180 PASS
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
artifact              10683608495
sha256:3cee80dc7d827ce948cda1f6f23e9282bd8ac51debdbf671238c03792bea31fa
```

## Repaired pinned three-repository benchmark

```text
branch:         benchmark/r710-comment-brace-d785807d
wrapper commit: d38ec071e3be6b26de5598e3a54ab06010f28a77
run:            35704017705 — PASS, 3 / 3 jobs
```

Direct artifact inspection for every pinned repository:

```text
R7.9 rendered-value terminal: 0
selected API projection:      0
ui-member-visibility:         0
joint-visibility candidate:   0
combined visibility rule:     0
```

Agentic mutation-causality remains closed: exactly 2 raw `UpdatedAt` mutations, 0 candidate promotion, with `runtime-pattern-variable` and `caller-object-unproven` retained.

This remains fail-closed stability evidence only. R7.14 remains NOT PASS.

## Current external gate

The implementation session found and repaired the rereview #7 blocker and must not self-certify its own repair.

Required next gate:

```text
independent rereview #8 of exact d785807dfa053a5abd1e3b6500a2fc1bd729d38a
```

Request: `docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-8-request.md`.

If rereview #8 finds no new compile-valid/runtime-valid false-positive blocker, it may mark R7.10 and all of D PASS / COMPLETE and unlock only E. R7.14 remains required for E; V0.5 remains locked until E completes.

## Version semantics

```text
roadmap:             V0.4.7-D / R7.10 pending independent rereview #8
tool/package:        RuaDen.Pkc.Tool 0.4.3-preview.2
C# raw schema:       0.4.4-csharp-raw
merged facts schema: 0.4.4
cross-stack schema:  0.4.6
frontend schema:     0.4.3-frontend
```
