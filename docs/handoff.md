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
6. `docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-1.md` through `...-7.md`
7. `docs/benchmarks/2026-09-22-r7.10-real-repo-benchmark.md`
8. `docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-8-request.md`

Then inspect current `main`, recent commits and repository status. Never reset to a historical SHA merely because this handoff names it.

## Current production checkpoint

Exact production SHA for fresh independent rereview:

```text
d785807dfa053a5abd1e3b6500a2fc1bd729d38a
fix: ignore comment braces in Angular visibility scope
```

A docs-only `[skip ci]` checkpoint may sit above this SHA on `main`. Review production behavior at `d785807d...`; do not reset `main`.

## Current milestone state

```text
V0.4.7-A                                PASS / COMPLETE
V0.4.7-B                                PASS / COMPLETE
V0.4.7-C                                PASS / COMPLETE
V0.4.7-D / R7.9                         PASS / COMPLETE
V0.4.7-D mutation-causality blocker     PASS / CLOSED
V0.4.7-D / R7.10                        REPAIRED / ALL GATES PASS / PENDING REREVIEW #8
V0.4.7-D overall                        PENDING INDEPENDENT REREVIEW #8
V0.4.7-E                                LOCKED behind D
R7.14 real-project positive yield       NOT PASS / REQUIRED FOR E
V0.5 Azure DevOps                       LOCKED
```

Do not start E or V0.5 before the independent D outcome.

## Permanent contract

Keep business conditions, value lineage/provenance, mutation/causality, and presentation authority distinct. Unsupported inference fails closed. Same/similar names are not proof. Stronger composition failure must preserve independently proven lower-authority evidence.

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

Supported backend remains target-project-semantic exact `System.Linq.Enumerable.Single/First(predicate)` → exact selected local → direct response property projection. R7.9 supplies explicit wire identity, typed result member, exact assignment and authoritative active rendered text interpolation. R7.10 accepts exactly one supported enclosing Angular `@if` and composes only exact fact IDs.

Frontend evidence cannot upgrade an `observed-only` backend condition. Unsupported or ambiguous structure fails closed.

## Rereview #7 finding

Independent rereview #7 challenged exact production `5d43b180e09cc7026919a4dff2563f85e39e1b82` and found a control-flow scope bug:

```html
@if (isAllowed) {
  <!-- { -->
}
<strong>{{ displayPrice }}</strong>
<!-- } -->
```

HTML-comment braces are inert at Angular runtime, so the interpolation is outside the real `@if`. The previous `FindMatchingBrace` counted those braces and could falsely attach `isAllowed` as `ui-member-visibility`, contaminating downstream R7.10 authority.

Review record:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-7.md`

## Repair completed

Production checkpoint:

```text
d785807dfa053a5abd1e3b6500a2fc1bd729d38a
fix: ignore comment braces in Angular visibility scope
```

The fix is limited to `AngularRenderedMemberVisibilityEnricher.FindMatchingBrace`: complete HTML comment regions are skipped while matching braces; an unterminated comment fails closed. Regression coverage is in:

`tests/Pkc.CSharp.Tests/AngularIfCommentBraceVisibilityRegressionTests.cs`

Coverage includes both a negative commented-opening-brace case and a positive commented-closing-brace case inside a genuinely enclosing `@if`.

Final implementation compare from docs HEAD `e831bab74ebff50c68055996a46bc5bcd7f7ec62` contains exactly two changed files: the visibility enricher and this regression file.

## Exact-SHA verification

Exact production `d785807dfa053a5abd1e3b6500a2fc1bd729d38a`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35703893704 — PASS
pinned Loren                                35703893679 — PASS
Loren-main canary                           35703893712 — PASS
pinned Jellyfin + parity/provenance         35703893678 — PASS
```

Core CI:

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
43,365 facts | 195,316 relations | 386 workflow candidates
116 product features | 504 knowledge Markdown files
43,365 / 43,365 facts project-semantic
portable parity/no-leak PASS
artifact 10683608495
sha256:3cee80dc7d827ce948cda1f6f23e9282bd8ac51debdbf671238c03792bea31fa
```

## Real-repository benchmark

Wrapper based exactly on `d785807d...`:

```text
branch:         benchmark/r710-comment-brace-d785807d
wrapper commit: d38ec071e3be6b26de5598e3a54ab06010f28a77
run:            35704017705 — PASS, 3 / 3 jobs
```

Artifacts:

```text
jin12-xyz/CRM
artifact 10684026181
sha256:8b313a95e3ed134b99ef1a144916ee6e50304c99d2931f7f8c1d183c14f1423c

hackersandwizards/agentic-engineering-training-angular
artifact 10684040991
sha256:3c5f2cb85c7119ca2283ef08f9194f86fde8a45146a633cde4996318e34cacb5

kesetovic/crm-system
artifact 10683736502
sha256:739086f68b806ac40c665c0876d15e70f7bfa4b02a5d3fb6ce6bbdf9a18b6610
```

Direct artifact inspection for every repository:

```text
R7.9 rendered UI terminal: 0
selected API projection:  0
ui-member-visibility:     0
joint-visibility:         0
combined visibility rule: 0
```

Agentic retains exactly two raw `UpdatedAt` mutations with `runtime-pattern-variable` / `caller-object-unproven` and zero candidate mutation promotion. Mutation-causality remains closed.

The benchmark is fail-closed stress evidence only and does not satisfy R7.14.

## Next action — independent rereview #8

Review exact production:

```text
d785807dfa053a5abd1e3b6500a2fc1bd729d38a
```

Request:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-8-request.md`

The reviewer must independently seek a new compile-valid/runtime-valid false-positive rather than merely re-confirming comment-brace handling.

If no blocker exists:

```text
mark R7.10 PASS / COMPLETE
→ mark all V0.4.7-D PASS / COMPLETE
→ unlock only V0.4.7-E
→ keep R7.14 NOT PASS and required for E
→ keep V0.5 locked until E completes
```

If a blocker exists, keep E locked and require a regression-first minimum generic repair.

This implementation session must not self-certify `d785807d...`.
