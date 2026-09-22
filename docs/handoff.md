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
10. `docs/benchmarks/2026-09-22-r7.10-real-repo-benchmark.md`
11. `docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-5-request.md`

Then inspect current `main`, recent commits and repository status. Never reset to a historical SHA merely because this handoff names it.

## Current production checkpoint

Exact production SHA for fresh independent rereview:

```text
1fc4d212b9c7add2f012f51adf3eef0c16f34dae
fix: reject static visibility hidden renders
```

A docs-only `[skip ci]` checkpoint may sit above this SHA on `main`. Review production behavior at `1fc4d212...`; do not reset `main`.

## Current milestone state

```text
V0.4.7-A                                PASS / COMPLETE
V0.4.7-B                                PASS / COMPLETE
V0.4.7-C                                PASS / COMPLETE
V0.4.7-D / R7.9                         PASS / COMPLETE
V0.4.7-D mutation-causality blocker     PASS / CLOSED
V0.4.7-D / R7.10                        REPAIRED / ALL GATES PASS / PENDING REREVIEW #5
V0.4.7-D overall                        PENDING INDEPENDENT REREVIEW #5
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

## Rereview history through #4

Rereview #1 repaired false render authority for inert `<ng-template>`, HTML comments and HTML tag/attribute interpolation.

Rereview #2 repaired static HTML `hidden` ancestry at `97161baa2d0aff9131a7acf9db752393ae913d64`.

Rereview #3 found static inline `display:none` remained over-authoritative and was repaired at:

```text
dc69e44206942ffb0012e1994d6d39249d1db4be
fix: reject static display none renders
```

Independent rereview #4 then found another static presentation-suppression case:

```html
<section style="visibility: hidden">
  @if (displayPrice > 0) {
    <strong>{{ displayPrice }}</strong>
  }
</section>
```

At `dc69e442...`, `AngularRenderedMemberAuthorityFilter` only recognized static `display:none`. Therefore the interpolation under `visibility:hidden` could retain `renderAuthority`; `AngularRenderedMemberVisibilityEnricher` could still attach the exact `@if`; downstream exact-ID composition could then create a false R7.9 rendered terminal and R7.10 joint visibility fact.

Full review record:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-4.md`

## Static visibility-hidden repair completed

Production checkpoint:

```text
1fc4d212b9c7add2f012f51adf3eef0c16f34dae
fix: reject static visibility hidden renders
```

The fix stays at the R7.9 render-authority boundary and extends the existing static inline declaration parser only:

- static `display:none` / `display:none !important` remains rejected;
- static `visibility:hidden` / `visibility:hidden !important` is now rejected;
- property/value matching is case-insensitive;
- static `display:block` and `visibility:visible` remain positive safeguards;
- dynamic/interpolated style values remain unsupported rather than guessed.

Deliberate non-claims remain:

- Angular `[style]`, `[style.visibility]`, `[style.display]`;
- class-based / stylesheet CSS and browser cascade;
- computed CSS, opacity and runtime script mutation;
- signals, outlets, structural directives and general DOM semantics.

Regression file:

`tests/Pkc.CSharp.Tests/StaticCssRenderAuthorityRegressionTests.cs`

It now includes frontend and end-to-end `visibility:hidden` negatives plus a `visibility:visible` positive safeguard, while retaining the prior `display:none` / `display:block` coverage.

Local .NET execution was unavailable in the implementation environment. The production/test diff was reviewed before one implementation push. Exact-SHA gates below provide clean executable verification.

## Exact-SHA verification

Exact production `1fc4d212b9c7add2f012f51adf3eef0c16f34dae`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35696272391 — PASS
pinned Loren                                35696272323 — PASS
Loren-main canary                           35696272328 — PASS
pinned Jellyfin + parity/provenance         35696273519 — PASS
```

Core CI:

```text
Release build        0 warnings / 0 errors
C# tests             167 / 167 PASS
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
artifact 10680339386
sha256:a31f2b97674c0a18fa4b3587a1d422dc1a7228e75ba67015f86791ca73467031
```

## Repaired real-repository benchmark

Wrapper based exactly on `1fc4d212...`:

```text
branch:         benchmark/r710-visibility-1fc4d212
wrapper commit: 7101b4c911539821c7c368203e0b05d150cbea64
run:            35696381103 — PASS, 3 / 3 jobs
```

GitHub compare proves the wrapper changes only one branch-trigger line in `.github/workflows/real-repo-benchmark.yml`; production and tests are byte-identical to `1fc4d212...`.

Artifacts:

```text
jin12-xyz/CRM
artifact 10679924411
sha256:723e597275aab0e116daa920ba62faec025f786c0572a2ca16176aa1498a98a4

hackersandwizards/agentic-engineering-training-angular
artifact 10680915908
sha256:f0477e13b6c91f88eed8f99485961fdc285922ff269a9fec4b79b29acb83fee5

kesetovic/crm-system
artifact 10680019528
sha256:0171e0091f4520adabbba43e3fa43add73bc07cf7e44f650615922a6e2fc79e0
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

## Next action — independent rereview #5

Review exact production SHA:

```text
1fc4d212b9c7add2f012f51adf3eef0c16f34dae
```

Request:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-5-request.md`

The reviewer must independently search for a new compile-valid/runtime-valid counterexample rather than merely re-confirming `visibility:hidden`.

If no blocker exists:

```text
mark R7.10 PASS / COMPLETE
→ mark all V0.4.7-D PASS / COMPLETE
→ unlock only V0.4.7-E
→ keep R7.14 NOT PASS and required for E
→ keep V0.5 locked
```

If a blocker exists, keep E locked and require regression-first minimum generic repair.

This implementation session must not self-certify `1fc4d212...`.
