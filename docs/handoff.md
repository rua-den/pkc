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
6. R7.10 independent rereview records #1 through #11
7. `docs/benchmarks/2026-09-22-r7.10-real-repo-benchmark.md`
8. `docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-12-request.md`

Then inspect current `main`, recent commits and repository status. Never reset to a historical SHA merely because this handoff names it.

## Current production checkpoint

Exact production SHA for fresh independent rereview:

```text
f9b20c27820a7ea9ac911222c613c2f9cdfb696f
fix: reject SVG resource renders
```

A docs-only `[skip ci]` checkpoint may sit above this SHA on `main`. Review production behavior at `f9b20c27...`; do not reset `main`.

## Current milestone state

```text
V0.4.7-A                                PASS / COMPLETE
V0.4.7-B                                PASS / COMPLETE
V0.4.7-C                                PASS / COMPLETE
V0.4.7-D / R7.9                         PASS / COMPLETE
V0.4.7-D mutation-causality blocker     PASS / CLOSED
V0.4.7-D / R7.10                        REPAIRED / ALL GATES PASS / PENDING REREVIEW #12
V0.4.7-D overall                        PENDING INDEPENDENT REREVIEW #12
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

Supported backend remains target-project-semantic exact `System.Linq.Enumerable.Single/First(predicate)` → exact selected local → direct response property projection. R7.9 supplies explicit wire identity, typed result member, exact assignment and authoritative active directly-rendered text interpolation. R7.10 accepts exactly one supported enclosing Angular `@if` and composes only exact fact IDs. Unsupported raw-text, SVG resource/definition, nested-control or structural-directive visibility paths fail closed.

Frontend evidence cannot upgrade an `observed-only` backend condition.

## Rereview #11 finding

Independent rereview #11 challenged exact predecessor:

```text
a3334bc202ee5a9e2dc8cc6d7c176dde8e06c9f9
fix: reject SVG definition renders
```

A new SVG resource direct-render authority defect was found. Angular templates can contain resource elements such as `<clipPath>`, `<mask>`, `<marker>` and `<pattern>`. Their child interpolation may participate in resource definitions but is not directly presented at the declaration site. The predecessor could still promote `{{ displayPrice }}` under those containers to authoritative `ui-member-render`, after which R7.9/R7.10 could overstate that occurrence as visibly rendered.

Review record:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-11.md`

## Repair completed

Production checkpoint:

```text
f9b20c27820a7ea9ac911222c613c2f9cdfb696f
fix: reject SVG resource renders
```

Repair scope is deliberately bounded:

- `style`, `defs`, `symbol`, `clipPath`, `mask`, `marker` and `pattern` are treated as non-direct-render ancestors;
- a closed resource container before a later genuine supported render does not over-filter;
- no SVG reference graph, CSS/browser engine or runtime DOM solver is claimed.

Regression coverage:

`tests/Pkc.CSharp.Tests/AngularSvgDefinitionRenderAuthorityRegressionTests.cs`

The implementation commit changes exactly one production frontend file and one regression file. No backend selected-projection identity, R7.9 exact API/frontend identity, R7.10 exact-ID composition or mutation-causality code changed.

Regression-first test-only commit object, never attached to a branch:

```text
33b21245cf5dae4cd8fbb38d61d27e16670617db
```

## Exact-SHA verification

Exact production `f9b20c27820a7ea9ac911222c613c2f9cdfb696f`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35743258917 — PASS
pinned Loren                                35743258977 — PASS
Loren-main canary                           35743259239 — PASS
pinned Jellyfin + parity/provenance         35743258815 — PASS
```

Core CI:

```text
Release build        0 warnings / 0 errors
C# tests             206 / 206 PASS
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
artifact 10702535277
sha256:fa8a47d770be0d93d78142e116b89858750cfb04d0d884f77b2b3ce00bc710bf
```

## Real-repository safety benchmark

Wrapper based exactly on production `f9b20c27...`:

```text
branch:         benchmark/r710-svg-resource-f9b20c27
wrapper commit: 461a281bd9c6d6e3a9d5b9b95c68562ff41638a9
run:            35743389433 — PASS, 3 / 3 jobs
```

Compare confirms the wrapper differs from production by exactly one workflow branch-trigger line.

Artifacts:

```text
jin12-xyz/CRM
artifact 10701386099
sha256:327e3e8673b158dcc678402c506188d13df69b45321c44126f8aab0137be3ae8

hackersandwizards/agentic-engineering-training-angular
artifact 10701296056
sha256:97285af533f15e7bf89d72f33736576f3c552656ae28a6ec376e5a6858756277

kesetovic/crm-system
artifact 10701136220
sha256:e40dffbef269b3b83530f4e9eece03e1e85ea582a1ff87bd7c90f16f70cb7a92
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

## Next action — independent rereview #12

Review exact production:

```text
f9b20c27820a7ea9ac911222c613c2f9cdfb696f
```

Request:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-12-request.md`

The reviewer must independently seek a new compile-valid/runtime-valid false-positive rather than merely replaying SVG `clipPath` / `mask` / `marker` / `pattern` handling.

If no blocker exists:

```text
mark R7.10 PASS / COMPLETE
→ mark all V0.4.7-D PASS / COMPLETE
→ unlock only V0.4.7-E
→ keep R7.14 NOT PASS and required for E
→ keep V0.5 locked until E completes
```

If a blocker exists, keep E locked and require a regression-first minimum generic repair.

This implementation continuation found and repaired rereview #11 and must not self-certify `f9b20c27...`.

## Prepared future execution packet — planning only

A future productization/update execution packet is now prepared:

```text
docs/plans/2026-09-22-ai-workspace-continuous-update-plan.md
docs/reviews/2026-09-22-ai-workspace-continuous-update-plan-self-review.md
```

It covers:

- `pkc run` as the simple product command;
- isolated Claude/Codex AI workspace bootstrap;
- product/QA answer contract with no unsolicited code;
- READY / PARTIAL / FAILED coverage semantics;
- shared repository/project analysis context;
- mixed MVC / Angular / JS repository discovery;
- `pkc verify`;
- safe semantic `pkc update`;
- `pkc diff` product-impact preview;
- canonical knowledge merge rather than Markdown merge;
- CI/main as the accepted team baseline;
- full-run versus incremental-update parity gates.

This packet is **not an unlock**. Current V0.4.7 work remains authoritative and must finish first.

### Current-main reconciliation warning

During preparation of this packet, `main` advanced through new production commits after the current status/review handoff. The final observed HEAD before this planning commit was:

```text
7dd00c1a960b8e85232d67b779e7906b4206cce6
fix: require SVG text ancestor
```

That is newer production code than the `f9b20c27...` candidate described by the current status/review handoff above. Therefore the next execution session must inspect and reconcile current HEAD, review records and gates before treating either SHA as accepted production or opening later work.

Do not reset `main` to the older documented candidate.

### Trigger behavior for future sessions

If the user says only `start`, `continue`, or equivalent:

```text
read status + handoff + current main
→ reconcile/finish the current V0.4.7 checkpoint
→ do not ask the user to restate the prepared design
→ once V0.4.7 is explicitly PASS / COMPLETE, read the execution plan + self-review
→ formally open the next roadmap checkpoint
→ implement regression-first
→ keep moving until PASS, an external review gate, or a proven external blocker
```

The preferred future priority after V0.4.7 is complete is:

```text
AI workspace + run/verify
→ continuous update/diff
→ Azure DevOps intent/history evidence
```

Do not rewrite active milestone ordering before V0.4.7 closes.
