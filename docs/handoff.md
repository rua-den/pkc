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
6. `docs/reviews/2026-09-23-v0.4.7-d-r7.10-independent-rereview-14.md`
7. `docs/benchmarks/2026-09-23-r7.10-nested-dependency-selector-benchmark.md`
8. `docs/reviews/2026-09-23-v0.4.7-d-r7.10-rereview-15-request.md`

Then inspect current `main`, recent commits and repository status. Never reset to a historical SHA merely because this handoff names it.

## Current production checkpoint

Exact production SHA for fresh independent rereview:

```text
47e098dbe910b7f6cfd933a0595370524bec1fb2
fix: correct resolved package root type
```

Relevant repair chain:

```text
8f667abc819f048b3dc85fc834677b7ca30f5518  fix: recognize external Angular component selectors
67c67fc25b6488dbc9f110a1b05c53a4bfee1a6c  fix: resolve nested Angular dependency selectors
47e098dbe910b7f6cfd933a0595370524bec1fb2  fix: correct resolved package root type
```

A docs-only `[skip ci]` checkpoint may sit above production on `main`. Review production behavior at `47e098...`; do not reset `main`.

## Current milestone state

```text
V0.4.7-A                                PASS / COMPLETE
V0.4.7-B                                PASS / COMPLETE
V0.4.7-C                                PASS / COMPLETE
V0.4.7-D / R7.9                         PASS / COMPLETE
V0.4.7-D mutation-causality blocker     PASS / CLOSED
V0.4.7-D / R7.10                        REPAIRED / ALL GATES PASS / PENDING REREVIEW #15
V0.4.7-D overall                        PENDING INDEPENDENT REREVIEW #15
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

Supported backend remains target-project-semantic exact `System.Linq.Enumerable.Single/First(predicate)` → exact selected local → direct response property projection. R7.9 supplies explicit wire identity, typed result member, exact assignment and authoritative active directly-rendered text interpolation. R7.10 accepts exactly one supported enclosing Angular `@if` and composes only exact fact IDs.

Frontend evidence cannot upgrade an `observed-only` backend condition.

## Rereview #14 — FAIL

Independent rereview #14 of `3e6fa774...` found a real external-component projection blocker. Product-source selector inventory alone cannot prove whether native-looking hosts such as `div[ext-shell]`, `.external-shell`, or combined selectors are ordinary HTML or imported Angular components. A child interpolation under an external component host is not authoritative direct render unless projection is proven.

Record:

`docs/reviews/2026-09-23-v0.4.7-d-r7.10-independent-rereview-14.md`

The current continuation treats #14 as FAIL history, not as an accepted D checkpoint.

## Repair 1 — external dependency component selectors

```text
8f667abc819f048b3dc85fc834677b7ca30f5518
fix: recognize external Angular component selectors
```

The projection authority filter now identifies packages referenced by product TypeScript imports and reads bounded Angular Ivy declaration metadata (`ɵɵComponentDeclaration`) from those imported package declarations. Only component declarations supply projection boundaries; directive declarations do not become projection blockers. Dependency declarations are authority metadata only and remain excluded from product facts/knowledge.

This repair handles external element, attribute, class and combined component selectors and escaped selector strings within the bounded declaration parser.

## Repair 2 — nested/nearest `node_modules`

Continuation review found `8f667abc...` assumed imported packages lived under repository-root `node_modules`. That is insufficient for normal monorepo layouts where an Angular app has its own dependency tree:

```text
repo/
  frontend/
    package.json
    src/price.component.ts
    node_modules/@vendor/ui/index.d.ts
```

Repair:

```text
67c67fc25b6488dbc9f110a1b05c53a4bfee1a6c
fix: resolve nested Angular dependency selectors
```

For each external import, package resolution now starts at the importing TypeScript file directory and walks ancestors up to repository root, selecting the nearest matching `node_modules/<package>` directory. Scoped packages and subpath imports are reduced to the owning package name; root-hoisted dependencies continue to work. Package symlinks are resolved conservatively only when the final target stays within the selected `node_modules` tree.

Focused regression:

`tests/Pkc.CSharp.Tests/AngularNestedNodeModulesProjectionAuthorityRegressionTests.cs`

The regression constructs a nested `frontend/` Angular app with local `node_modules/@vendor/ui/index.d.ts` and proves an imported attribute-selector component cannot leave authoritative render/visibility evidence for an unprojected child interpolation.

The first CI attempt on `67c67fc...` exposed only a compile type mismatch because `DirectoryInfo.ResolveLinkTarget()` returns `FileSystemInfo`. Final compile correction:

```text
47e098dbe910b7f6cfd933a0595370524bec1fb2
fix: correct resolved package root type
```

No resolver semantics changed in that final correction.

## Exact-SHA verification

Exact production `47e098dbe910b7f6cfd933a0595370524bec1fb2`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35818350915 — PASS
pinned Loren                                35818350859 — PASS
Loren-main canary                           35818350997 — PASS
pinned Jellyfin + parity/provenance         35818350930 — PASS
```

Core CI:

```text
Release build        0 warnings / 0 errors
C# tests             248 / 248 PASS
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
artifact 10732606989
sha256:1d2b38aadc18828a75625ea94651f2a1acfc3a3813a3cc9df0c57d9be4b1bde2
```

Local repository execution is not claimed in this continuation because the available container could not resolve `github.com` and has no local .NET toolchain. Exact-SHA GitHub Actions are the verification evidence.

## Real-repository safety benchmark

Wrapper based exactly on production:

```text
branch:         benchmark/r710-nested-node-modules-47e098
wrapper commit: 437d14a9b9ed36ce24e7fd8edfb2cef31eed7f6c
run:            35818835753 — PASS, 3 / 3 jobs
```

Compare confirms the wrapper differs from production by exactly one branch-trigger line in `.github/workflows/real-repo-benchmark.yml`.

Artifacts:

```text
jin12-xyz/CRM
artifact 10732532350
sha256:6f4e41e7b18357d5a06036bd14a39b15988ba437b013f78b0ecfee9af9b15f9f

hackersandwizards/agentic-engineering-training-angular
artifact 10733025143
sha256:6c360ae70b72d9d8678b0d209f233d565f176a2aa878a5a20d62876638adb040

kesetovic/crm-system
artifact 10732851433
sha256:16a95ee2bbb4155d33a1c3f6dfdf60767f036d389a83bff98e2954a86f348c54
```

Direct artifact inspection for every repository:

```text
ui-member-render:           0
ui-member-visibility:       0
renderAuthority markers:    0
selected API/R7.9 terminal: 0
joint/combined visibility:  0
```

Counts remain the same safety baseline:

```text
jin12       415 facts / 1,580 relations / 25 knowledge files
agentic     441 facts /   660 relations / 26 knowledge files
kesetovic   488 facts / 2,054 relations / 28 knowledge files
```

Agentic retains the exact two relevant raw `UpdatedAt` mutations with `runtime-pattern-variable / caller-object-unproven`; their exact fact IDs occur zero times outside raw facts in feature candidates, product features and generated Markdown. Mutation-causality remains closed.

This benchmark proves fail-closed stability only. It does not satisfy R7.14; positive real-project yield remains NOT PASS.

Detailed record:

`docs/benchmarks/2026-09-23-r7.10-nested-dependency-selector-benchmark.md`

## Next action — independent rereview #15

Review exact production:

```text
47e098dbe910b7f6cfd933a0595370524bec1fb2
```

Request:

`docs/reviews/2026-09-23-v0.4.7-d-r7.10-rereview-15-request.md`

The reviewer must independently search for a new compile-valid/runtime-valid false-positive rather than replaying the external-selector or nested-node_modules regressions. High-value seams include nearest/hoisted dependency resolution, package subpaths/scoped packages, workspace/symlink packages, external component-vs-directive distinction, selector collisions, HTML/SVG `foreignObject` boundaries, exact control scope/fact-ID composition, observed-only backend authority, and mutation-causality closure.

If no blocker exists:

```text
mark R7.10 PASS / COMPLETE
→ mark all V0.4.7-D PASS / COMPLETE
→ unlock only V0.4.7-E
→ keep R7.14 NOT PASS and required for E
→ keep V0.5 and future W/U locked until V0.4.7 completes
```

If a blocker exists, keep E locked and require a regression-first minimum generic repair.

This implementation continuation must not self-certify `47e098...`.

## Prepared future execution packet — planning only

```text
docs/plans/2026-09-22-ai-workspace-continuous-update-plan.md
docs/reviews/2026-09-22-ai-workspace-continuous-update-plan-self-review.md
```

When V0.4.7 is explicitly PASS / COMPLETE, re-baseline the roadmap before implementation. Preferred order remains AI workspace + `run/verify` → continuous update/diff → Azure DevOps intent/history evidence.
