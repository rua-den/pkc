# PKC Handoff

Last updated: 2026-09-23

Use this file when continuing PKC in another coding/review thread.

## Read first

Before changing production code, read:

1. `docs/status.md`
2. this handoff
3. `docs/milestones.md`
4. `docs/product-knowledge-contract.md`
5. `docs/v0.4.7-acceptance-plan.md`
6. R7.10 independent rereview records #1 through #11 plus the rereview #12 request/history
7. `docs/benchmarks/2026-09-23-r7.10-native-svg-fail-closed-benchmark.md`
8. `docs/reviews/2026-09-23-v0.4.7-d-r7.10-rereview-13-request.md`

Then inspect current `main`, recent commits and repository status. Never reset to a historical SHA merely because this handoff names it.

## Current production checkpoint

Exact production SHA for fresh independent rereview:

```text
7818c7ed646b30cb7b8505f053572783e075af6f
fix: fail closed on native SVG render authority
```

A docs-only `[skip ci]` checkpoint may sit above this SHA on `main`. Review production behavior at `7818c7ed...`; do not reset `main`.

## Current milestone state

```text
V0.4.7-A                                PASS / COMPLETE
V0.4.7-B                                PASS / COMPLETE
V0.4.7-C                                PASS / COMPLETE
V0.4.7-D / R7.9                         PASS / COMPLETE
V0.4.7-D mutation-causality blocker     PASS / CLOSED
V0.4.7-D / R7.10                        REPAIRED / ALL GATES PASS / PENDING REREVIEW #13
V0.4.7-D overall                        PENDING INDEPENDENT REREVIEW #13
V0.4.7-E                                LOCKED behind D
R7.14 real-project positive yield       NOT PASS / REQUIRED FOR E
V0.5 Azure DevOps                       LOCKED
future AI workspace/update packet       PREPARED / NOT UNLOCKED
```

Do not start E, V0.5, or the prepared W/U productization work before the applicable predecessor gates pass.

## Permanent contract

Keep business conditions, value lineage/provenance, mutation/causality, render authority and visibility authority distinct. Unsupported inference fails closed. Same/similar names are not proof. Stronger composition failure must preserve independently proven lower-authority evidence.

## R7.10 bounded positive

R7.10 answers, for the same exact R7.9-proven value path:

> What backend selection condition and frontend visibility condition jointly determine whether this rendered value is visible?

Supported backend remains target-project-semantic exact `System.Linq.Enumerable.Single/First(predicate)` → exact selected local → direct response property projection. R7.9 supplies explicit wire identity, typed result member, exact assignment and authoritative active directly-rendered text interpolation. R7.10 accepts exactly one supported enclosing Angular `@if` and composes only exact fact IDs. Unsupported raw-text, inert, structural, nested-control or unproven visibility/render paths fail closed.

Frontend evidence cannot upgrade an `observed-only` backend condition.

## Reconciliation of the stale rereview #12 handoff

The prior handoff requested independent rereview #12 of:

```text
f9b20c27820a7ea9ac911222c613c2f9cdfb696f
fix: reject SVG resource renders
```

No independent acceptance record for that exact candidate was committed before `main` advanced through additional R7.10 SVG authority changes:

```text
11b6b21315ac21f7b5f947c933b07bd9bb7a3298  fix: enforce SVG text render authority
7dd00c1a960b8e85232d67b779e7906b4206cce6  fix: require SVG text ancestor
3fe0d4452b49a6b4c2b16d422af3975afbda40a8  fix: reject transparent SVG renders
d4416c4a13a04db46091bbffff1c71566c0d5d0c  fix: bound SVG text content authority
```

This continuation therefore treated `d4416c4a...` as an implementation candidate needing fresh challenge rather than assuming rereview #12 had passed.

## New blocker found and repaired

Concrete counterexample:

```html
@if (isAllowed) {
  <svg>
    <text fill="none" stroke="none">{{ displayPrice }}</text>
  </svg>
}
```

This is valid native SVG text structure but paints neither fill nor stroke. The predecessor could still treat the interpolation as authoritative directly-visible output because the bounded filters had accumulated structural, display, visibility and opacity checks without proving the full native SVG rendering model.

The minimum generic repair is deliberately broader and simpler than adding another paint-property whitelist:

```text
7818c7ed646b30cb7b8505f053572783e075af6f
fix: fail closed on native SVG render authority
```

V0.4.7 now makes this authority boundary explicit:

- native SVG interpolation is unsupported for authoritative directly-visible render proof;
- no SVG paint/layout/browser-engine model is claimed;
- ordinary HTML rendering remains supported;
- HTML descendants inside SVG `foreignObject` remain supported through the HTML namespace path;
- lower-authority evidence survives the render-authority rejection.

Regression coverage is concentrated in:

```text
tests/Pkc.CSharp.Tests/AngularSvgRenderedTextAuthorityRegressionTests.cs
tests/Pkc.CSharp.Tests/AngularSvgTextChildPlacementRegressionTests.cs
tests/Pkc.CSharp.Tests/AngularSvgOpacityRenderAuthorityRegressionTests.cs
tests/Pkc.CSharp.Tests/AngularSvgTextContentModelAuthorityRegressionTests.cs
```

The explicit `fill="none" stroke="none"` regression is included, and `foreignObject` HTML remains a positive authority path.

## Exact-SHA verification

Exact production `7818c7ed646b30cb7b8505f053572783e075af6f`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35765278584 — PASS
pinned Loren                                35765278607 — PASS
Loren-main canary                           35765278416 — PASS
pinned Jellyfin + parity/provenance         35765278447 — PASS
```

Core CI:

```text
Release build        0 warnings / 0 errors
C# tests             231 / 231 PASS
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
artifact 10712181821
sha256:281403635a62680c71640f0a839fdbeffdbcaf72d59b38407789c65037c59adb
```

Local clone/test execution was unavailable because the execution container could not resolve `github.com`; no local PASS claim is made. Exact-SHA GitHub gates above are the completed verification evidence.

## Real-repository safety benchmark

Wrapper based exactly on production `7818c7ed...`:

```text
branch:         benchmark/r710-native-svg-7818c7ed
wrapper commit: 87255ba6f5f012d82ee17f039d540db6bbdf01bf
run:            35765659218 — PASS, 3 / 3 jobs
```

Compare confirms the wrapper differs from production by exactly one workflow branch-trigger line.

Artifacts:

```text
jin12-xyz/CRM
artifact 10711409458
sha256:15b9a85614984f05aef447bbfeb89cd08ad111d5ed1e531b869d809e481a1766

hackersandwizards/agentic-engineering-training-angular
artifact 10711259746
sha256:c3f2c77a4f3132431685d75bb5c56c8dbbfe5564f39ed10f795247c724907a74

kesetovic/crm-system
artifact 10711519386
sha256:4175f1171a61c9d9c95016c40f55a7c861b8c1b7762e4b9bcc44cf2b5b37224d
```

Direct artifact inspection for every repository:

```text
R7.9 rendered UI terminal: 0
selected API projection:  0
ui-member-visibility:     0
joint-visibility:         0
combined visibility rule: 0
```

Agentic retains exactly two raw `UpdatedAt` mutations with `runtime-pattern-variable / caller-object-unproven`; their exact fact IDs occur zero times in feature candidates, product features, `PKC_KNOWLEDGE.md` and canonical knowledge Markdown. Mutation-causality remains closed.

The benchmark proves fail-closed safety only. It does not satisfy R7.14; positive real-project yield remains NOT PASS.

Detailed record:

`docs/benchmarks/2026-09-23-r7.10-native-svg-fail-closed-benchmark.md`

## Next action — independent rereview #13

Review exact production:

```text
7818c7ed646b30cb7b8505f053572783e075af6f
```

Request:

`docs/reviews/2026-09-23-v0.4.7-d-r7.10-rereview-13-request.md`

The reviewer must independently search for another compile-valid/runtime-valid false-positive. Native SVG is now intentionally unsupported for authoritative directly-visible render proof, so lack of native SVG positives is not itself a blocker. High-value review seams include HTML/SVG namespace transitions, `foreignObject`, malformed/ambiguous lexical scope that the bounded scanner might still over-promote, exact identity composition, unsupported nested/structural visibility and backend observed-only authority.

If no blocker exists:

```text
mark R7.10 PASS / COMPLETE
→ mark all V0.4.7-D PASS / COMPLETE
→ unlock only V0.4.7-E
→ keep R7.14 NOT PASS and required for E
→ keep V0.5 and future W/U work locked until V0.4.7 completes
```

If a blocker exists, keep E locked and require a regression-first minimum generic repair.

This implementation continuation must not self-certify `7818c7ed...`.

## Prepared future execution packet — planning only

Prepared files:

```text
docs/plans/2026-09-22-ai-workspace-continuous-update-plan.md
docs/reviews/2026-09-22-ai-workspace-continuous-update-plan-self-review.md
```

When V0.4.7 is explicitly PASS / COMPLETE, re-baseline the roadmap before implementation. Preferred order remains:

```text
AI workspace + run/verify
→ continuous update/diff
→ Azure DevOps intent/history evidence
```

Do not start that future initiative while D/E remain open.
