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
6. R7.10 independent rereview records through #9
7. `docs/benchmarks/2026-09-22-r7.10-real-repo-benchmark.md`
8. `docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-10-request.md`

Then inspect current `main`, recent commits and repository status. Never reset to a historical SHA merely because this handoff names it.

## Current production checkpoint

Exact production SHA for fresh independent rereview:

```text
1bc67e62f1099e4d580072682566c2d305e4db07
fix: ignore Angular style raw text
```

A docs-only `[skip ci]` checkpoint may sit above this SHA on `main`. Review production behavior at `1bc67e62...`; do not reset `main`.

## Current milestone state

```text
V0.4.7-A                                PASS / COMPLETE
V0.4.7-B                                PASS / COMPLETE
V0.4.7-C                                PASS / COMPLETE
V0.4.7-D / R7.9                         PASS / COMPLETE
V0.4.7-D mutation-causality blocker     PASS / CLOSED
V0.4.7-D / R7.10                        REPAIRED / ALL GATES PASS / PENDING REREVIEW #10
V0.4.7-D overall                        PENDING INDEPENDENT REREVIEW #10
V0.4.7-E                                LOCKED behind D
R7.14 real-project positive yield       NOT PASS / REQUIRED FOR E
V0.5 Azure DevOps                       LOCKED
```

Do not start E or V0.5 before the independent D outcome.

## Permanent contract

Keep business conditions, value lineage/provenance, mutation/causality, render authority and visibility authority distinct. Unsupported inference fails closed. Same/similar names are not proof. Stronger composition failure must preserve independently proven lower-authority evidence.

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

## R7.10 bounded positive

R7.10 answers, for the same exact R7.9-proven value path:

> What backend selection condition and frontend visibility condition jointly determine whether this rendered value is visible?

Supported backend remains target-project-semantic exact `System.Linq.Enumerable.Single/First(predicate)` → exact selected local → direct response property projection. R7.9 supplies explicit wire identity, typed result member, exact assignment and authoritative active rendered text interpolation. R7.10 accepts exactly one supported enclosing Angular `@if` and composes only exact fact IDs. Unsupported raw-text, nested-control or structural-directive visibility paths fail closed.

Frontend evidence cannot upgrade an `observed-only` backend condition.

## Rereview #9 finding

Independent rereview #9 challenged exact predecessor `cd3b1d4b64168c4e95284299c5715b3db0e4da4b` and found a distinct Angular raw-text boundary defect.

Compile-valid/runtime-valid examples showed that `<style>` may appear in a component template while interpolation-looking text inside it is not an Angular binding. The old scanner/filter could therefore promote CSS text such as `{{ displayPrice }}` as rendered member authority. CSS braces or `@if`-looking text inside the style region could also corrupt the bounded visibility matcher and falsely associate a later render with an `@if`.

Review record:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-9.md`

## Repair completed

Production checkpoint:

```text
1bc67e62f1099e4d580072682566c2d305e4db07
fix: ignore Angular style raw text
```

Repair scope is deliberately narrow:

- `AngularRenderedMemberAuthorityFilter` treats `<style>` content as non-authoritative raw text;
- `AngularRenderedMemberVisibilityEnricher` excludes control-block matches inside `<style>`;
- brace matching skips complete `<style>...</style>` regions and fails closed if the style close is missing;
- a closed style element before a genuine supported `@if` remains a positive path.

Regression coverage:

`tests/Pkc.CSharp.Tests/AngularStyleElementAuthorityRegressionTests.cs`

The implementation commit changes exactly two production frontend files and one regression file. No backend, R7.9 identity-composition or mutation-causality logic was changed.

## Exact-SHA verification

Exact production `1bc67e62f1099e4d580072682566c2d305e4db07`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35717515912 — PASS
pinned Loren                                35717515881 — PASS
Loren-main canary                           35717515909 — PASS
pinned Jellyfin + parity/provenance         35717515901 — PASS
```

Core CI:

```text
Release build        0 warnings / 0 errors
C# tests             198 / 198 PASS
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
artifact 10690353622
sha256:5b82eb6b43aa4dedd223b6a0956367e2fe2b15bd03a887087143bee7c0ee112e
```

## Real-repository safety benchmark

Wrapper based exactly on production `1bc67e62...`:

```text
branch:         benchmark/r710-style-1bc67e62
wrapper commit: f16ffb91ca986f2230f9e6f232263210b6a0a6d5
run:            35717582575 — PASS, 3 / 3 jobs
```

GitHub compare confirms the wrapper differs from production by exactly one workflow branch-trigger line.

Artifacts:

```text
jin12-xyz/CRM
artifact 10690217835
sha256:c0b09267fe7c001c0c9b8e83f4956c73a8daeef91d0951bb94c54dac07b86eb1

hackersandwizards/agentic-engineering-training-angular
artifact 10689343222
sha256:ce34dc9f24131790927dd8fbd048a4b1ac30059a83641c15304515bf234d1b75

kesetovic/crm-system
artifact 10689968027
sha256:4c84b4c2a4e02f9972c88064b5872c7953689173cf0d9fce7d2ed2a4f38ee652
```

Direct artifact inspection for every repository:

```text
R7.9 rendered UI terminal: 0
selected API projection:  0
ui-member-visibility:     0
joint-visibility:         0
combined visibility rule: 0
```

Agentic retains exactly two raw `UpdatedAt` mutations with `runtime-pattern-variable / caller-object-unproven`, with zero feature-candidate or Markdown promotion. Mutation-causality remains closed.

The benchmark proves fail-closed safety only. It does not satisfy R7.14; positive real-project yield remains NOT PASS.

## Next action — independent rereview #10

Review exact production:

```text
1bc67e62f1099e4d580072682566c2d305e4db07
```

Request:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-10-request.md`

The reviewer must independently seek a new compile-valid/runtime-valid false-positive rather than merely re-confirming `<style>` handling.

If no blocker exists:

```text
mark R7.10 PASS / COMPLETE
→ mark all V0.4.7-D PASS / COMPLETE
→ unlock only V0.4.7-E
→ keep R7.14 NOT PASS and required for E
→ keep V0.5 locked until E completes
```

If a blocker exists, keep E locked and require a regression-first minimum generic repair.

This implementation session found and repaired rereview #9 and must not self-certify `1bc67e62...`.
