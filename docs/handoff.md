# PKC Handoff

Last updated: 2026-09-22

Use this file when continuing PKC in another coding/review thread.

## Read first

Before changing production code, read:

1. `docs/status.md`
2. this handoff
3. `docs/milestones.md`
4. `docs/product-knowledge-contract.md`
5. `docs/v0.4.7-acceptance-plan.md`
6. R7.10 independent rereview records through #10
7. `docs/benchmarks/2026-09-22-r7.10-real-repo-benchmark.md`
8. `docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-11-request.md`

Then inspect current `main`, recent commits and repository status. Never reset to a historical SHA merely because this handoff names it.

## Current production checkpoint

Exact production SHA for fresh independent rereview:

```text
a3334bc202ee5a9e2dc8cc6d7c176dde8e06c9f9
fix: reject SVG definition renders
```

A docs-only `[skip ci]` checkpoint may sit above this SHA on `main`. Review production behavior at `a3334bc...`; do not reset `main`.

## Current milestone state

```text
V0.4.7-A                                PASS / COMPLETE
V0.4.7-B                                PASS / COMPLETE
V0.4.7-C                                PASS / COMPLETE
V0.4.7-D / R7.9                         PASS / COMPLETE
V0.4.7-D mutation-causality blocker     PASS / CLOSED
V0.4.7-D / R7.10                        REPAIRED / ALL GATES PASS / PENDING REREVIEW #11
V0.4.7-D overall                        PENDING INDEPENDENT REREVIEW #11
V0.4.7-E                                LOCKED behind D
R7.14 real-project positive yield       NOT PASS / REQUIRED FOR E
V0.5 Azure DevOps                       LOCKED
```

Do not start E or V0.5 before the independent D outcome.

## Permanent contract

Keep business conditions, value lineage/provenance, mutation/causality, render authority and visibility authority distinct. Unsupported inference fails closed. Same/similar names are not proof. Stronger composition failure must preserve independently proven lower-authority evidence.

## R7.10 bounded positive

R7.10 answers, for the same exact R7.9-proven value path:

> What backend selection condition and frontend visibility condition jointly determine whether this rendered value is visible?

Supported backend remains target-project-semantic exact `System.Linq.Enumerable.Single/First(predicate)` → exact selected local → direct response property projection. R7.9 supplies explicit wire identity, typed result member, exact assignment and authoritative active directly-rendered text interpolation. R7.10 accepts exactly one supported enclosing Angular `@if` and composes only exact fact IDs. Unsupported raw-text, SVG definition, nested-control or structural-directive visibility paths fail closed.

Frontend evidence cannot upgrade an `observed-only` backend condition.

## Rereview #10 finding

Independent rereview #10 challenged exact predecessor:

```text
1bc67e62f1099e4d580072682566c2d305e4db07
fix: ignore Angular style raw text
```

A new SVG direct-render authority defect was found. Angular templates can contain SVG interpolation, but child content under SVG `<defs>` or `<symbol>` is definition content and is not directly presented to the user. The predecessor could still promote `{{ displayPrice }}` under those containers to authoritative `ui-member-render`, after which R7.9/R7.10 could overstate that value as visibly rendered.

Review record:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-10.md`

## Repair completed

Production checkpoint:

```text
a3334bc202ee5a9e2dc8cc6d7c176dde8e06c9f9
fix: reject SVG definition renders
```

Repair scope is deliberately narrow:

- direct render authority is rejected beneath SVG `defs` and `symbol` ancestors;
- existing `<style>` raw-text suppression remains intact;
- a closed SVG definition container before a later genuine supported render does not over-filter;
- no general SVG `<use>` graph, CSS/browser engine or runtime DOM solver is claimed.

Regression coverage:

`tests/Pkc.CSharp.Tests/AngularSvgDefinitionRenderAuthorityRegressionTests.cs`

The implementation commit changes exactly one production frontend file and one regression file. No backend, R7.9 exact identity-composition, joint composition or mutation-causality code changed.

## Exact-SHA verification

Exact production `a3334bc202ee5a9e2dc8cc6d7c176dde8e06c9f9`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35740454910 — PASS
pinned Loren                                35740454928 — PASS
Loren-main canary                           35740454970 — PASS
pinned Jellyfin + parity/provenance         35740454878 — PASS
```

Core CI:

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
43,365 facts | 195,316 relations | 386 workflow candidates
116 product features | 504 knowledge Markdown files
43,365 / 43,365 facts project-semantic
portable parity/no-leak PASS
artifact 10698544024
sha256:dc679ba75f586ef7e7f0fd4b3ac8e0f79c70ab496132896b2c7bbdece9d50ef4
```

## Real-repository safety benchmark

Wrapper based exactly on production `a3334bc...`:

```text
branch:         benchmark/r710-svg-a3334bc2
wrapper commit: e01326c66fef67679b53af459e0bb9208b5dd3f4
run:            35740571925 — PASS, 3 / 3 jobs
```

Compare confirms the wrapper differs from production by exactly one workflow branch-trigger line.

Artifacts:

```text
jin12-xyz/CRM
artifact 10699379067
sha256:6f46ad1b02c0bbe71a0d3ca1a8b325cf104f8a7224e369d0618bfea7d48dff8d

hackersandwizards/agentic-engineering-training-angular
artifact 10699408849
sha256:639a4faa92653250a9d439461be261ec47a2efd68f9954c70278fd1aa3ed297d

kesetovic/crm-system
artifact 10698434908
sha256:8b98a64b12457087cd1ea403c1ba024ce5d65bb6b92a9fa7efac895fda287fc9
```

Direct artifact inspection for every repository:

```text
R7.9 rendered UI terminal: 0
selected API projection:  0
ui-member-visibility:     0
joint-visibility:         0
combined visibility rule: 0
```

Agentic retains exactly two raw `UpdatedAt` mutations with `runtime-pattern-variable / caller-object-unproven`; their exact fact IDs occur zero times in feature candidates, product features and generated Markdown. Mutation-causality remains closed.

The benchmark proves fail-closed safety only. It does not satisfy R7.14; positive real-project yield remains NOT PASS.

## Next action — independent rereview #11

Review exact production:

```text
a3334bc202ee5a9e2dc8cc6d7c176dde8e06c9f9
```

Request:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-11-request.md`

The reviewer must independently seek a new compile-valid/runtime-valid false-positive rather than merely re-confirming SVG `defs` / `symbol` handling.

If no blocker exists:

```text
mark R7.10 PASS / COMPLETE
→ mark all V0.4.7-D PASS / COMPLETE
→ unlock only V0.4.7-E
→ keep R7.14 NOT PASS and required for E
→ keep V0.5 locked until E completes
```

If a blocker exists, keep E locked and require a regression-first minimum generic repair.

This implementation continuation found and repaired rereview #10 and must not self-certify `a3334bc...`.
