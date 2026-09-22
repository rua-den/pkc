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
9. `docs/benchmarks/2026-09-22-r7.10-real-repo-benchmark.md`
10. `docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-4-request.md`

Then inspect current `main`, recent commits and repository status. Never reset to a historical SHA merely because this handoff names it.

## Current production checkpoint

Exact production SHA for fresh independent rereview:

```text
dc69e44206942ffb0012e1994d6d39249d1db4be
fix: reject static display none renders
```

A docs-only `[skip ci]` checkpoint may sit above this SHA on `main`. Review production behavior at `dc69e442...`; do not reset `main`.

## Current milestone state

```text
V0.4.7-A                                PASS / COMPLETE
V0.4.7-B                                PASS / COMPLETE
V0.4.7-C                                PASS / COMPLETE
V0.4.7-D / R7.9                         PASS / COMPLETE
V0.4.7-D mutation-causality blocker     PASS / CLOSED
V0.4.7-D / R7.10                        REPAIRED / ALL GATES PASS / PENDING REREVIEW #4
V0.4.7-D overall                        PENDING INDEPENDENT REREVIEW #4
V0.4.7-E                                LOCKED behind D
R7.14 real-project positive yield       NOT PASS / REQUIRED FOR E
V0.5 Azure DevOps                       LOCKED
```

Do not start E or V0.5 before the independent D outcome.

## Permanent contract

Keep business conditions, value lineage/provenance, and mutation/causality distinct. Unsupported inference fails closed. Same/similar names are not proof. If stronger composition fails, preserve independently proven lower-authority evidence.

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

Supported backend remains the narrow target-project-semantic `Enumerable.Single/First(predicate)` → exact selected local → direct response property projection path. R7.9 supplies explicit wire identity, typed result member, exact assignment and authoritative active visible text interpolation. R7.10 accepts one supported enclosing Angular `@if` and composes only exact fact IDs.

Frontend evidence cannot upgrade an `observed-only` backend condition. Unsupported or ambiguous structure fails closed.

## Rereview history

Rereview #1 found and repaired:

```text
inert <ng-template>
HTML-comment interpolation
HTML tag/attribute interpolation
```

Rereview #2 found and repaired static HTML `hidden` ancestry at production SHA `97161baa2d0aff9131a7acf9db752393ae913d64`.

Rereview #3 challenged that repair and found a distinct static presentation blocker:

```html
<section style="display: none">
  @if (displayPrice > 0) {
    <strong>{{ displayPrice }}</strong>
  }
</section>
```

`display:none` makes the subtree non-presented to the user, but the previous authority filter could still treat the interpolation as visible. That would allow false R7.9 rendered-terminal and R7.10 combined visibility claims.

Review record:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-3.md`

## Static display-none repair completed

Production checkpoint:

```text
dc69e44206942ffb0012e1994d6d39249d1db4be
fix: reject static display none renders
```

The repair stays at the R7.9 render-authority boundary. The bounded HTML-ancestor parser now treats a static inline `style` declaration containing `display:none` or `display:none !important` as presentation suppression. It is case-insensitive and preserves existing authority for static `display:block`.

Deliberate non-claims:

- Angular `[style]` and `[style.display]` bindings;
- interpolated/dynamic style values;
- class-based or stylesheet CSS;
- computed CSS / browser cascade;
- other CSS properties such as `visibility`;
- signals, outlets, structural directives or general runtime DOM semantics.

Regression file:

`tests/Pkc.CSharp.Tests/StaticCssRenderAuthorityRegressionTests.cs`

It includes frontend negative, end-to-end R7.9/R7.10 negative, and a `display:block` positive safeguard.

Local .NET execution was unavailable in the implementation environment. The complete two-file implementation/test diff was reviewed before one production push. Clean exact-SHA gates below provide executable verification.

## Exact-SHA verification

Exact `dc69e44206942ffb0012e1994d6d39249d1db4be`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35687831689 — PASS
pinned Loren                                35687831632 — PASS
Loren-main canary                           35687831587 — PASS
pinned Jellyfin + parity/provenance         35687831537 — PASS
```

Core CI:

```text
Release build        0 warnings / 0 errors
C# tests             164 / 164 PASS
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
all facts project-semantic
portable parity/no-leak PASS
artifact 10677695966
sha256:088353b7a1e30bf2aa68dce408246c2f88de50b843e88a7c57bb8fac595bdb60
```

## Repaired real-repository benchmark

Wrapper based exactly on `dc69e442...`:

```text
branch:         benchmark/r710-css-dc69e442
wrapper commit: b3b0c5b40c0d34cafe3e7a60c28341ea777dd2cf
run:            35687951314 — PASS, 3 / 3 jobs
```

Artifacts:

```text
jin12-xyz/CRM
artifact 10676974702
sha256:640cfe616ac523f6e868446b31ce467644b82920b7ba20044340cfd5b652ffcc

hackersandwizards/agentic-engineering-training-angular
artifact 10676969751
sha256:c9f3106843d14f368abb37da9d0b61e391d943567f2c45511f61ae1150ce4723

kesetovic/crm-system
artifact 10677306261
sha256:ef3e57e3138492b87d0e8c12c8204e63e977487f46a3ce3c20a6869f05ca1408
```

Every repository still emits zero supported R7.9 rendered terminal, selected API projection, UI-member visibility, joint visibility, and combined visibility rule. Agentic retains exactly two raw `UpdatedAt` mutations and zero candidate mutation promotion. R7.14 remains NOT PASS.

Detailed record:

`docs/benchmarks/2026-09-22-r7.10-real-repo-benchmark.md`

## Next action — independent rereview #4

Review exact production SHA:

```text
dc69e44206942ffb0012e1994d6d39249d1db4be
```

Request:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-4-request.md`

The reviewer must independently search for another compile-valid/runtime-valid counterexample rather than merely re-confirm the display-none regression.

If no blocker exists:

```text
mark R7.10 PASS / COMPLETE
→ mark all V0.4.7-D PASS / COMPLETE
→ unlock only V0.4.7-E
→ keep R7.14 NOT PASS and required for E
→ keep V0.5 locked
```

If a blocker exists, keep E locked and require regression-first minimum generic repair.

This implementation session must not self-certify `dc69e442...`.
