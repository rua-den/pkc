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
10. `docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-5.md`
11. `docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-6.md`
12. `docs/benchmarks/2026-09-22-r7.10-real-repo-benchmark.md`
13. `docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-7-request.md`

Then inspect current `main`, recent commits and repository status. Never reset to a historical SHA merely because this handoff names it.

## Current production checkpoint

Exact production SHA for fresh independent rereview:

```text
5d43b180e09cc7026919a4dff2563f85e39e1b82
fix: fail closed on Angular hidden bindings
```

A docs-only `[skip ci]` checkpoint may sit above this SHA on `main`. Review production behavior at `5d43b180...`; do not reset `main`.

## Current milestone state

```text
V0.4.7-A                                PASS / COMPLETE
V0.4.7-B                                PASS / COMPLETE
V0.4.7-C                                PASS / COMPLETE
V0.4.7-D / R7.9                         PASS / COMPLETE
V0.4.7-D mutation-causality blocker     PASS / CLOSED
V0.4.7-D / R7.10                        REPAIRED / ALL GATES PASS / PENDING REREVIEW #7
V0.4.7-D overall                        PENDING INDEPENDENT REREVIEW #7
V0.4.7-E                                LOCKED behind D
R7.14 real-project positive yield       NOT PASS / REQUIRED FOR E
V0.5 Azure DevOps                       LOCKED
```

Do not start E or V0.5 before the independent D outcome.

## Permanent contract

Keep business conditions, value lineage/provenance, mutation/causality, and presentation authority distinct. Unsupported inference fails closed. Same/similar names are not proof. If stronger composition fails, preserve independently proven lower-authority evidence.

## R7.10 bounded positive

R7.10 answers, for the same exact R7.9-proven value path:

> What backend selection condition and frontend visibility condition jointly determine whether this rendered value is visible?

Supported backend remains the narrow target-project-semantic `Enumerable.Single/First(predicate)` → exact selected local → direct response property projection path. R7.9 supplies explicit wire identity, typed result member, exact assignment and authoritative active rendered text interpolation. R7.10 accepts one supported enclosing Angular `@if` and composes only exact fact IDs.

Frontend evidence cannot upgrade an `observed-only` backend condition. Unsupported or ambiguous structure fails closed.

## Rereview #6 finding

Independent rereview #6 challenged exact production:

```text
e54b8444c3d14e647bcc0a9fe23d8f6e1905865b
fix: reject inert legacy template renders
```

and found a distinct native-hidden binding boundary:

```html
<section [hidden]="true">
  @if (displayPrice > 0) {
    <strong>{{ displayPrice }}</strong>
  }
</section>
```

Angular sets the native element hidden property to true, suppressing presentation. The previous render-authority filter handled static `hidden` but not Angular hidden bindings. The interpolation could therefore remain an authoritative `ui-member-render`, receive the enclosing `@if`, and feed false R7.9/R7.10 output.

Review record:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-6.md`

## Native-hidden binding repair completed

Production checkpoint:

```text
5d43b180e09cc7026919a4dff2563f85e39e1b82
fix: fail closed on Angular hidden bindings
```

The fix stays at the R7.9 render-authority boundary and closes the native HTML `hidden` family only:

- `[hidden]="false"` and `bind-hidden="false"` preserve authority as exact lowercase literal positives;
- `[hidden]="true"`, `bind-hidden="true"`, and arbitrary property expressions fail closed;
- `[hidden]="False"` fails closed because Angular expressions are case-sensitive;
- `hidden="{{ ... }}"`, `[attr.hidden]`, and `bind-attr.hidden` fail closed;
- static `hidden`, `display:none`, `visibility:hidden`, inert templates, comments and tag/attribute interpolation retain their prior behavior.

The repair deliberately does not claim class/stylesheet CSS, computed browser styles, arbitrary dynamic property solving, signals, outlets, structural directives, or general runtime DOM behavior.

Regression file:

`tests/Pkc.CSharp.Tests/AngularHiddenBindingRenderAuthorityRegressionTests.cs`

The final history contains one coherent implementation commit directly above the prior docs handoff. Compare from `fda69aaf...` to `5d43b180...` shows exactly two files: production filter + regression test.

## Exact-SHA verification

Exact production `5d43b180e09cc7026919a4dff2563f85e39e1b82`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35702383031 — PASS
pinned Loren                                35702383035 — PASS
Loren-main canary                           35702383053 — PASS
pinned Jellyfin + parity/provenance         35702383040 — PASS
```

Core CI:

```text
Release build        0 warnings / 0 errors
C# tests             178 / 178 PASS
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
artifact 10683620529
sha256:992ebd2005ae40c3a587a0ec80515dfe4350ee8e490f635f07cc2950451dd178
```

## Repaired real-repository benchmark

Wrapper based exactly on `5d43b180...`:

```text
branch:         benchmark/r710-hidden-binding-5d43b180
wrapper commit: b22c846ecebb75c6f52533c280ba4acbbe453657
run:            35702503746 — PASS, 3 / 3 jobs
```

GitHub compare proves the wrapper changes only one branch-trigger line in `.github/workflows/real-repo-benchmark.yml`; production and tests are byte-identical to `5d43b180...`.

Artifacts:

```text
jin12-xyz/CRM
artifact 10681794667
sha256:fe4d3b5e7e1cb3870fd2fca00f05fd95215f579eaf3ef9b8d4d8cc5b6adb9353

hackersandwizards/agentic-engineering-training-angular
artifact 10682129190
sha256:9a5be3326eb49ac109ab7f34f0deeb4e16f14a4a16b9dfcfbe45e3c9e37e5d03

kesetovic/crm-system
artifact 10683555054
sha256:eeb8ddfb42592d38a24f8c98a73cca663d93b833e985081c6eb7cc1eb60eb1a9
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

## Next action — independent rereview #7

Review exact production SHA:

```text
5d43b180e09cc7026919a4dff2563f85e39e1b82
```

Request:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-7-request.md`

The reviewer must independently search for a new compile-valid/runtime-valid false-authority counterexample rather than merely replaying hidden-binding regressions.

A feature being unsupported and failing closed is not a blocker. A blocker requires actual false or over-authoritative output from a compile-valid/runtime-valid shape.

If no blocker exists:

```text
mark R7.10 PASS / COMPLETE
→ mark all V0.4.7-D PASS / COMPLETE
→ unlock only V0.4.7-E
→ keep R7.14 NOT PASS and required for E
→ keep V0.5 locked until E completes
```

If a blocker exists, keep E locked and require regression-first minimum generic repair.

This implementation session must not self-certify `5d43b180...`.
