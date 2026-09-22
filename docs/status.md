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
V0.4.7-D API to UI / R7.10 joint visibility      REPAIRED / ALL GATES PASS / PENDING REREVIEW #5
V0.4.7-D overall                                 PENDING INDEPENDENT REREVIEW #5
V0.4.7-E product acceptance                      LOCKED behind D
R7.14 real-project positive yield                NOT PASS / REQUIRED FOR E
V0.5 Azure DevOps input evidence                 LOCKED
```

V0.4.7 acceptance is defined in `docs/v0.4.7-acceptance-plan.md`. The permanent product contract is `docs/product-knowledge-contract.md`.

## Exact production candidate under review

```text
1fc4d212b9c7add2f012f51adf3eef0c16f34dae
fix: reject static visibility hidden renders
```

A docs-only `[skip ci]` checkpoint may sit above this SHA on `main`. Review production behavior at `1fc4d212...`; do not reset `main`.

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

Keep business conditions, value lineage/provenance, and mutation/causality distinct. Unsupported inference fails closed. Same/similar names are never sufficient proof. Conservative authority downgrade must preserve independently proven lower-authority evidence.

## R7.10 intended bounded proof

For the same exact R7.9-proven value path:

```text
Enumerable.Single/First(predicate)
→ exact selected reference local
→ exact scalar auto-property
→ direct API response property projection
→ explicit wire identity
→ exact frontend result/member/state identity
→ authoritative active visible Angular text interpolation
→ one supported enclosing @if
→ joint backend/frontend visibility evidence
```

Frontend visibility cannot upgrade an `observed-only` backend predicate. Zero, multiple, nested or otherwise unsupported visibility paths fail closed.

## Rereview history

Rereview #1 repaired false render authority for inert `<ng-template>`, HTML comments, and HTML tag/attribute interpolation.

Rereview #2 repaired static HTML `hidden` ancestry at `97161baa2d0aff9131a7acf9db752393ae913d64`.

Rereview #3 found static inline `display:none` could still produce false visible-render authority; repaired at:

```text
dc69e44206942ffb0012e1994d6d39249d1db4be
fix: reject static display none renders
```

Rereview #4 independently challenged `dc69e442...` and found a distinct compile-valid/runtime-valid false-authority shape:

```html
<section style="visibility: hidden">
  @if (displayPrice > 0) {
    <strong>{{ displayPrice }}</strong>
  }
</section>
```

Static inline `visibility:hidden` makes the rendered text visually hidden, but `dc69e442...` only recognized `display:none`. The interpolation could therefore remain authoritative `ui-member-render`, receive the enclosing `@if`, and flow into false R7.9/R7.10 PO-facing authority.

Review record:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-4.md`

## Static visibility-hidden repair

Production repair:

```text
1fc4d212b9c7add2f012f51adf3eef0c16f34dae
fix: reject static visibility hidden renders
```

The existing bounded static-inline-style scanner now also rejects `visibility:hidden` and `visibility:hidden !important`, case-insensitively, on active HTML ancestors. Static `visibility:visible` remains authoritative. Existing `display:none` behavior is unchanged.

The repair deliberately does **not** claim dynamic `[style]` bindings, class/stylesheet cascade, computed browser CSS, signals, outlets, structural directives, opacity, or general runtime DOM semantics. Those remain unsupported unless a future compile-valid/runtime-valid false-positive is demonstrated and bounded generically.

Regression coverage is in:

`tests/Pkc.CSharp.Tests/StaticCssRenderAuthorityRegressionTests.cs`

Coverage includes frontend negative, end-to-end R7.9/R7.10 negative, and positive safeguards for both `display:block` and `visibility:visible`.

Local .NET execution was unavailable in the implementation environment. The complete production/test diff was reviewed before one implementation push. Exact-SHA clean-environment verification below is the executable evidence.

## Exact-SHA standard verification

All standard gates passed on exact production SHA `1fc4d212b9c7add2f012f51adf3eef0c16f34dae`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35696272391 — PASS
pinned Loren                                35696272323 — PASS
Loren-main canary                           35696272328 — PASS
pinned Jellyfin + parity/provenance         35696273519 — PASS
```

Core CI evidence:

```text
Release build        0 warnings / 0 errors
C# tests             167 / 167 PASS
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
artifact              10680339386
sha256:a31f2b97674c0a18fa4b3587a1d422dc1a7228e75ba67015f86791ca73467031
```

## Repaired pinned three-repository benchmark

Temporary wrapper branch based exactly on production SHA `1fc4d212...`:

```text
branch:         benchmark/r710-visibility-1fc4d212
wrapper commit: 7101b4c911539821c7c368203e0b05d150cbea64
run:            35696381103 — PASS, 3 / 3 jobs
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

The implementation session repaired the rereview #4 blocker and must not self-certify its own repair. Required next gate:

```text
independent rereview #5 of exact 1fc4d212b9c7add2f012f51adf3eef0c16f34dae
```

Request: `docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-5-request.md`.

If rereview #5 finds no new compile-valid/runtime-valid blocker, it may mark R7.10 and all of D PASS / COMPLETE and unlock only E. R7.14 remains required and NOT PASS; V0.5 remains locked. Do not start E before that independent outcome.

## Version semantics

```text
roadmap:             V0.4.7-D / R7.10 pending independent rereview #5
tool/package:        RuaDen.Pkc.Tool 0.4.3-preview.2
C# raw schema:       0.4.4-csharp-raw
merged facts schema: 0.4.4
cross-stack schema:  0.4.6
frontend schema:     0.4.3-frontend
```
