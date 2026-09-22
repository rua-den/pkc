# PKC Status

Last updated: 2026-09-23

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                 PASS / COMPLETE
V0.4.5 real-repository generalization            PASS / COMPLETE
V0.4.6 business logic reconstruction             PASS / COMPLETE
V0.4.7-A origin and copy timing                  PASS / COMPLETE
V0.4.7-B computation and later change            PASS / COMPLETE
V0.4.7-C backend to API                          PASS / COMPLETE
V0.4.7-D API to UI / R7.9 binding                PASS / COMPLETE
V0.4.7-D mutation-causality blocker              PASS / CLOSED
V0.4.7-D API to UI / R7.10 joint visibility      REPAIRED / ALL GATES PASS / PENDING REREVIEW #14
V0.4.7-D overall                                 PENDING INDEPENDENT REREVIEW #14
V0.4.7-E product acceptance                      LOCKED behind D
R7.14 real-project positive yield                NOT PASS / REQUIRED FOR E
V0.5 Azure DevOps input evidence                 LOCKED
AI workspace + continuous-update plan            PREPARED / NOT UNLOCKED
```

V0.4.7 acceptance is defined in `docs/v0.4.7-acceptance-plan.md`; the permanent product contract is `docs/product-knowledge-contract.md`.

## Exact production candidate under review

```text
3e6fa7749eb8ef47be4eedb72d1c159cd502692f
fix: bound Angular direct text containers
```

Its direct implementation predecessor is:

```text
34182e221df6cf50eaa0ac362a5f575e80236a64
fix: reject unproven Angular content projection
```

A docs-only `[skip ci]` checkpoint may sit above production on `main`. Review production behavior at `3e6fa774...`; do not reset `main`.

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

## Why rereview #13 is superseded

Rereview #13 requested an independent review of native-SVG fail-closed production `7818c7ed...`. Before an independent acceptance record was committed, this continuation pre-challenged that candidate and found two new compile-valid/runtime-valid Angular render-authority classes. Therefore #13 is historical/stale and must not be treated as PASS.

### Blocker 1 — unproven component content projection

A child interpolation is not directly rendered merely because it is lexically inside a component host:

```html
@if (isAllowed) {
  <app-shell>{{ displayPrice }}</app-shell>
}
```

If `app-shell` does not project that child through a matching `ng-content`, the interpolation never becomes rendered DOM. The predecessor could still emit authoritative `ui-member-render` / `ui-member-visibility`.

Generic repair `34182e22...` adds an authority filter that fails closed beneath component/custom-element projection boundaries unless direct rendering is independently established. Local product-source component selectors are recognized for element, attribute, class and combined selector forms; custom-element hosts also fail closed conservatively.

### Blocker 2 — non-direct / conditional HTML text containers

Ordinary HTML syntax is not sufficient proof that child text is directly page-visible. Examples include metadata, fallback and conditionally-presented containers such as:

```html
<title>{{ displayPrice }}</title>
<canvas>{{ displayPrice }}</canvas>
<dialog>{{ displayPrice }}</dialog>
<details>{{ displayPrice }}</details>
<object>{{ displayPrice }}</object>
<noscript>{{ displayPrice }}</noscript>
```

Generic repair `3e6fa774...` rejects authoritative direct-text proof under the bounded unsupported container set while preserving later ordinary HTML positives after a closed unsupported container. This is deliberately a fail-closed authority boundary, not a browser/CSS/layout engine.

Native SVG direct render remains unsupported from `7818c7ed...`; HTML under supported `foreignObject` namespace transition remains eligible subject to the new HTML/projection boundaries.

## Regression coverage added

```text
tests/Pkc.CSharp.Tests/AngularComponentProjectionRenderAuthorityRegressionTests.cs
tests/Pkc.CSharp.Tests/AngularHtmlDirectTextAuthorityRegressionTests.cs
```

Coverage includes element-selector projection, attribute-selector projection, non-direct HTML containers, and positive later ordinary-HTML rendering after the unsupported boundary closes.

## Exact-SHA standard verification

All required standard gates passed on exact production `3e6fa7749eb8ef47be4eedb72d1c159cd502692f`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35769202107 — PASS
pinned Loren                                35769202115 — PASS
Loren-main canary                           35769202043 — PASS
pinned Jellyfin + parity/provenance         35769202074 — PASS
```

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
facts                 43,365
relations             195,316
workflow candidates   386
product features      116
knowledge Markdown    504 files
analysis modes        43,365 / 43,365 project-semantic
portable parity       PASS
artifact              10713378704
sha256:cca8adf6d31a4c0207eaea58e5935147d279a7e9e3db361ad6b32291a0807198
```

Local repository execution is not claimed because the execution container could not resolve `github.com` when cloning and has no local .NET toolchain. Exact-SHA GitHub gates are the validation evidence.

## Final pinned three-repository safety benchmark

A temporary wrapper branch based exactly on production `3e6fa774...` changed only the benchmark workflow branch-trigger line:

```text
base production: 3e6fa7749eb8ef47be4eedb72d1c159cd502692f
wrapper commit:  28c9758ba5ae172bb0ee52e032a28fa24fb1f917
run:             35769939151 — PASS, 3 / 3 jobs
```

Direct artifact inspection found, for every pinned repository:

```text
ui-member-render:           0
ui-member-visibility:       0
renderAuthority markers:    0
selected API/R7.9 terminal: 0
joint/combined visibility:  0
```

The canonical benchmark evidence is byte-identical to the preceding safety benchmark for all three repositories except the nested generated ZIP archive bytes/timestamps. Counts remain:

```text
jin12-xyz/CRM                                      415 facts / 1,580 relations / 25 knowledge files
hackersandwizards/agentic-engineering-training-angular 441 facts /   660 relations / 26 knowledge files
kesetovic/crm-system                               488 facts / 2,054 relations / 28 knowledge files
```

Benchmark artifacts:

```text
jin12          10713667423  sha256:10bb68b7e10714d6c217bce776c09737dfe0330279775840b4bd8b45c7387f72
agentic        10713841164  sha256:cbbbcc2b9dd42d1bf3b2978ed56e72d057a3452d9e7844cd4b6cc7e14a41863d
kesetovic      10713209096  sha256:d4f24200afff9a532514731ce93f18f802bfece6117a8f0b9fc386cc4d1a1d39
```

Agentic mutation-causality remains closed: exactly two raw `UpdatedAt` mutations retain `runtime-pattern-variable / caller-object-unproven`, and their exact fact IDs occur zero times outside raw facts in feature candidates, product features and generated Markdown.

Interpretation:

```text
Safety / fail-closed: PASS
R7.14 positive real-project yield: NOT PASS
```

Detailed evidence: `docs/benchmarks/2026-09-23-r7.10-html-projection-fail-closed-benchmark.md`.

## Current external gate

Required next gate:

```text
independent rereview #14 of exact 3e6fa7749eb8ef47be4eedb72d1c159cd502692f
```

Request: `docs/reviews/2026-09-23-v0.4.7-d-r7.10-rereview-14-request.md`.

If rereview #14 finds no new compile-valid/runtime-valid false-positive blocker, it may mark R7.10 and all of D PASS / COMPLETE and unlock only E. R7.14 remains required for E; V0.5 and the prepared AI-workspace/update initiative remain locked until V0.4.7 completes.

This implementation continuation repaired the blockers and must not self-certify its own production candidate.

## Prepared future productization packet

```text
docs/plans/2026-09-22-ai-workspace-continuous-update-plan.md
docs/reviews/2026-09-22-ai-workspace-continuous-update-plan-self-review.md
```

After V0.4.7 is explicitly PASS / COMPLETE, preferred sequencing is AI workspace + `run/verify`, then semantic `update/diff`, then Azure DevOps evidence. Do not open that work while D/E remain incomplete.

## Version semantics

```text
roadmap:             V0.4.7-D / R7.10 pending independent rereview #14
tool/package:        RuaDen.Pkc.Tool 0.4.3-preview.2
C# raw schema:       0.4.4-csharp-raw
merged facts schema: 0.4.4
cross-stack schema:  0.4.6
frontend schema:     0.4.3-frontend
```
