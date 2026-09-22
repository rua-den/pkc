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
V0.4.7-D API to UI / R7.10 joint visibility      REPAIRED / ALL GATES PASS / PENDING REREVIEW #4
V0.4.7-D overall                                 PENDING INDEPENDENT REREVIEW #4
V0.4.7-E product acceptance                      LOCKED behind D
R7.14 real-project positive yield                NOT PASS / REQUIRED FOR E
V0.5 Azure DevOps input evidence                 LOCKED
```

V0.4.7 acceptance is defined in `docs/v0.4.7-acceptance-plan.md`. The permanent product contract is `docs/product-knowledge-contract.md`.

## Exact production candidate under review

```text
dc69e44206942ffb0012e1994d6d39249d1db4be
fix: reject static display none renders
```

A docs-only `[skip ci]` commit may sit above this SHA on `main`. Review production behavior at `dc69e442...`; do not reset `main`.

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

Rereview #1 found false rendered-value authority for inert `<ng-template>`, HTML comments, and HTML tag/attribute interpolation. Those were repaired through `da5d23771ef8c9d58d0333d1f949e8d742210043`.

Rereview #2 found static HTML `hidden` ancestry could still create false visible-render authority. That was repaired at:

```text
97161baa2d0aff9131a7acf9db752393ae913d64
fix: reject statically hidden rendered text
```

Rereview #3 found a distinct compile-valid/runtime-valid case not covered by the static-hidden repair:

```html
<section style="display: none">
  @if (displayPrice > 0) {
    <strong>{{ displayPrice }}</strong>
  }
</section>
```

Static inline `display:none` suppresses the subtree from user presentation, but `97161baa...` could still promote `{{ displayPrice }}` to authoritative `ui-member-render`, then R7.9 rendered terminal and R7.10 joint visibility. Review record:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-3.md`

## Static inline CSS repair

Production repair:

```text
dc69e44206942ffb0012e1994d6d39249d1db4be
fix: reject static display none renders
```

The bounded HTML-ancestry authority filter now rejects simple interpolation beneath an active ancestor whose static inline `style` contains `display: none` or `display: none !important`, case-insensitively. Static `display:block` remains authoritative. Interpolated/dynamic style values are not treated as static proof. The repair deliberately does not claim `[style]`, `[style.display]`, class stylesheets, computed CSS, signals, outlets, structural directives, or general runtime DOM semantics.

Regression coverage:

`tests/Pkc.CSharp.Tests/StaticCssRenderAuthorityRegressionTests.cs`

It proves:

```text
static display:none ancestor
→ no authoritative ui-member-render
→ no ui-member-visibility
→ no R7.9 rendered UI terminal
→ no R7.10 joint-visibility fact

static display:block ancestor
→ existing bounded authority remains available
```

Local .NET execution was unavailable in the implementation environment. The complete two-file implementation/test diff was reviewed before one production push. Clean exact-SHA CI supplies executable verification.

## Exact-SHA standard verification

All standard gates passed on exact production SHA `dc69e44206942ffb0012e1994d6d39249d1db4be`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35687831689 — PASS
pinned Loren                                35687831632 — PASS
Loren-main canary                           35687831587 — PASS
pinned Jellyfin + parity/provenance         35687831537 — PASS
```

Core CI evidence:

```text
Release build        0 warnings / 0 errors
C# tests             164 / 164 PASS
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
artifact              10677695966
sha256:088353b7a1e30bf2aa68dce408246c2f88de50b843e88a7c57bb8fac595bdb60
```

## Repaired pinned three-repository benchmark

Temporary wrapper branch based exactly on `dc69e442...`:

```text
branch:         benchmark/r710-css-dc69e442
wrapper commit: b3b0c5b40c0d34cafe3e7a60c28341ea777dd2cf
run:            35687951314 — PASS, 3 / 3 jobs
```

The wrapper changes only the benchmark branch-trigger line. Direct artifact inspection for every pinned repository found:

```text
R7.9 rendered-value terminal: 0
selected API projection:      0
ui-member-visibility:         0
joint-visibility candidate:   0
combined visibility rule:     0
```

Agentic mutation-causality remains closed: 2 raw `UpdatedAt` mutations, 0 candidate promotion, `runtime-pattern-variable` and `caller-object-unproven` retained, transitive-mutation warning retained.

Detailed benchmark record: `docs/benchmarks/2026-09-22-r7.10-real-repo-benchmark.md`.

This is fail-closed stability evidence only. R7.14 remains **NOT PASS**.

## Current external gate

The same session found and repaired the rereview #3 blocker, so it cannot self-certify the repaired SHA. Required next gate:

```text
independent rereview #4 of exact dc69e44206942ffb0012e1994d6d39249d1db4be
```

Request: `docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-4-request.md`.

If rereview #4 finds no new compile-valid/runtime-valid blocker, it may mark R7.10 and all of D PASS / COMPLETE and unlock only E. R7.14 remains required and NOT PASS; V0.5 remains locked. Do not start E before that independent outcome.

## Version semantics

```text
roadmap:             V0.4.7-D / R7.10 pending independent rereview #4
tool/package:        RuaDen.Pkc.Tool 0.4.3-preview.2
C# raw schema:       0.4.4-csharp-raw
merged facts schema: 0.4.4
cross-stack schema:  0.4.6
frontend schema:     0.4.3-frontend
```
