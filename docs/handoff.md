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
6. prior R7.10 rereview records through #13 request/history
7. `docs/benchmarks/2026-09-23-r7.10-html-projection-fail-closed-benchmark.md`
8. `docs/reviews/2026-09-23-v0.4.7-d-r7.10-rereview-14-request.md`

Then inspect current `main`, recent commits and repository status. Never reset to a historical SHA merely because this handoff names it.

## Current production checkpoint

Exact production SHA for fresh independent rereview:

```text
3e6fa7749eb8ef47be4eedb72d1c159cd502692f
fix: bound Angular direct text containers
```

Direct predecessor in the same repaired authority boundary:

```text
34182e221df6cf50eaa0ac362a5f575e80236a64
fix: reject unproven Angular content projection
```

A docs-only `[skip ci]` checkpoint may sit above production on `main`. Review production behavior at `3e6fa774...`; do not reset `main`.

## Current milestone state

```text
V0.4.7-A                                PASS / COMPLETE
V0.4.7-B                                PASS / COMPLETE
V0.4.7-C                                PASS / COMPLETE
V0.4.7-D / R7.9                         PASS / COMPLETE
V0.4.7-D mutation-causality blocker     PASS / CLOSED
V0.4.7-D / R7.10                        REPAIRED / ALL GATES PASS / PENDING REREVIEW #14
V0.4.7-D overall                        PENDING INDEPENDENT REREVIEW #14
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

## Why rereview #13 is stale

Rereview #13 targeted:

```text
7818c7ed646b30cb7b8505f053572783e075af6f
fix: fail closed on native SVG render authority
```

Before independent acceptance was recorded, implementation pre-challenge found two additional compile-valid/runtime-valid false-positive authority classes. #13 is therefore superseded; it must not be interpreted as PASS.

## Repair 1 — component content projection

Counterexample:

```ts
@Component({
  selector: 'app-shell',
  template: `<span>Shell</span>`
})
export class ShellComponent {}

@Component({
  imports: [ShellComponent],
  template: `
    @if (isAllowed) {
      <app-shell>{{ displayPrice }}</app-shell>
    }
  `
})
export class PriceComponent {}
```

Without matching `ng-content`, `displayPrice` is not projected/rendered by `app-shell`, but the predecessor could over-promote the lexical interpolation.

Repair:

```text
34182e221df6cf50eaa0ac362a5f575e80236a64
fix: reject unproven Angular content projection
```

`AngularComponentProjectionAuthorityFilter` now fails closed for custom-element/component-host projection boundaries. Product-source component selectors are recognized for element, attribute, class and combined selector forms. This is an authority downgrade only; lower-authority facts remain available.

Focused regression:

`tests/Pkc.CSharp.Tests/AngularComponentProjectionRenderAuthorityRegressionTests.cs`

## Repair 2 — direct HTML text authority

Compile-valid Angular templates can contain child text that is metadata, fallback content, or conditionally presented rather than directly page-visible:

```html
<title>{{ displayPrice }}</title>
<canvas>{{ displayPrice }}</canvas>
<dialog>{{ displayPrice }}</dialog>
<details>{{ displayPrice }}</details>
<object>{{ displayPrice }}</object>
<noscript>{{ displayPrice }}</noscript>
```

Repair:

```text
3e6fa7749eb8ef47be4eedb72d1c159cd502692f
fix: bound Angular direct text containers
```

`AngularHtmlDirectTextAuthorityFilter` rejects authoritative direct-text proof beneath the bounded unsupported container set covering metadata/raw/fallback/conditional browser surfaces. Ordinary HTML remains supported; a closed unsupported container does not suppress a later ordinary supported render.

Focused regression:

`tests/Pkc.CSharp.Tests/AngularHtmlDirectTextAuthorityRegressionTests.cs`

Native SVG direct-render authority remains fail-closed from `7818c7ed...`. HTML inside supported SVG `foreignObject` remains eligible subject to the ordinary HTML and projection boundaries.

## Exact-SHA verification

Exact production `3e6fa7749eb8ef47be4eedb72d1c159cd502692f`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35769202107 — PASS
pinned Loren                                35769202115 — PASS
Loren-main canary                           35769202043 — PASS
pinned Jellyfin + parity/provenance         35769202074 — PASS
```

Core CI:

```text
Release build        0 warnings / 0 errors
C# tests             241 / 241 PASS
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
artifact 10713378704
sha256:cca8adf6d31a4c0207eaea58e5935147d279a7e9e3db361ad6b32291a0807198
```

Local repository test execution is not claimed: the execution container could not resolve `github.com` while cloning and has no local .NET toolchain. Exact-SHA GitHub Actions are the validation evidence.

## Real-repository safety benchmark

Wrapper based exactly on production:

```text
branch:         benchmark/r710-html-projection-3e6fa774
wrapper commit: 28c9758ba5ae172bb0ee52e032a28fa24fb1f917
run:            35769939151 — PASS, 3 / 3 jobs
```

Compare confirms the wrapper differs from production by exactly one branch-trigger line in `.github/workflows/real-repo-benchmark.yml`.

Artifacts:

```text
jin12-xyz/CRM
artifact 10713667423
sha256:10bb68b7e10714d6c217bce776c09737dfe0330279775840b4bd8b45c7387f72

hackersandwizards/agentic-engineering-training-angular
artifact 10713841164
sha256:cbbbcc2b9dd42d1bf3b2978ed56e72d057a3452d9e7844cd4b6cc7e14a41863d

kesetovic/crm-system
artifact 10713209096
sha256:d4f24200afff9a532514731ce93f18f802bfece6117a8f0b9fc386cc4d1a1d39
```

Direct artifact inspection for every repository:

```text
ui-member-render:           0
ui-member-visibility:       0
renderAuthority markers:    0
selected API/R7.9 terminal: 0
joint/combined visibility:  0
```

Canonical benchmark outputs are byte-identical to the previous accepted safety benchmark except the generated nested ZIP archive bytes/timestamps. Agentic still has exactly two relevant raw `UpdatedAt` mutations with `runtime-pattern-variable / caller-object-unproven`, and their exact IDs occur zero times outside raw facts in candidates/features/generated Markdown.

This proves fail-closed stability only. It does not satisfy R7.14; positive real-project yield remains NOT PASS.

Detailed record:

`docs/benchmarks/2026-09-23-r7.10-html-projection-fail-closed-benchmark.md`

## Next action — independent rereview #14

Review exact production:

```text
3e6fa7749eb8ef47be4eedb72d1c159cd502692f
```

Request:

`docs/reviews/2026-09-23-v0.4.7-d-r7.10-rereview-14-request.md`

The reviewer must independently search for another compile-valid/runtime-valid false-positive rather than merely replaying the new projection/container regressions. High-value seams now include:

- projection across local/external component selectors, especially unresolved dependency selectors;
- HTML/SVG namespace transitions and `foreignObject`;
- exact control-block scope across unsupported containers/component hosts;
- malformed or ambiguous structures only where Angular/runtime still accepts the input and PKC can over-promote authority;
- exact fact-ID composition, observed-only backend authority, and mutation-causality closure.

If no blocker exists:

```text
mark R7.10 PASS / COMPLETE
→ mark all V0.4.7-D PASS / COMPLETE
→ unlock only V0.4.7-E
→ keep R7.14 NOT PASS and required for E
→ keep V0.5 and future W/U locked until V0.4.7 completes
```

If a blocker exists, keep E locked and require a regression-first minimum generic repair.

This implementation continuation must not self-certify `3e6fa774...`.

## Prepared future execution packet — planning only

```text
docs/plans/2026-09-22-ai-workspace-continuous-update-plan.md
docs/reviews/2026-09-22-ai-workspace-continuous-update-plan-self-review.md
```

When V0.4.7 is explicitly PASS / COMPLETE, re-baseline the roadmap before implementation. Preferred order remains AI workspace + `run/verify` → continuous update/diff → Azure DevOps intent/history evidence.
