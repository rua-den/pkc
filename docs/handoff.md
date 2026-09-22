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
8. `docs/benchmarks/2026-09-22-r7.10-real-repo-benchmark.md`
9. `docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-3-request.md`

Then inspect current `main`, recent commits and repository status. Never reset to a historical SHA merely because this handoff names it.

## Current production checkpoint

Exact production SHA for fresh independent rereview:

```text
97161baa2d0aff9131a7acf9db752393ae913d64
fix: reject statically hidden rendered text
```

A docs-only `[skip ci]` checkpoint may sit above this SHA on `main`. Review production behavior at `97161baa...`; do not reset `main`.

## Current milestone state

```text
V0.4.7-A                                PASS / COMPLETE
V0.4.7-B                                PASS / COMPLETE
V0.4.7-C                                PASS / COMPLETE
V0.4.7-D / R7.9                         PASS / COMPLETE
V0.4.7-D mutation-causality blocker     PASS / CLOSED
V0.4.7-D / R7.10                        REPAIRED / ALL GATES PASS / PENDING REREVIEW #3
V0.4.7-D overall                        PENDING INDEPENDENT REREVIEW #3
V0.4.7-E                                LOCKED behind D
R7.14 real-project positive yield       NOT PASS / REQUIRED FOR E
V0.5 Azure DevOps                       LOCKED
```

Do not start E or V0.5 before the independent D outcome.

## Permanent contract

PKC compiles deterministic implementation evidence into portable PO-facing knowledge. Keep these evidence classes distinct:

```text
business conditions
value lineage / provenance
mutation / causality
```

Unsupported inference fails closed. Same/similar names are not proof. If stronger composition fails, preserve independently proven lower-authority evidence.

## Accepted predecessors

```text
V0.4.6
c310e893762997f34562a6b3a62dbab2b05c0c93
fix: fail closed on Queryable authority

V0.4.7-A
09a0c7f2a058adfd7ddb7b4ac2feb2d17580a429
feat: prove bounded reference dynamic lineage

V0.4.7-B
17fd30b3a4b8178208adabc12c40dee060bedb54
fix: fail closed after opaque terminal effects

V0.4.7-C
fbb64b9917da1f63362558355201ff7998384ba0
feat: prove backend API projection lineage

V0.4.7-D / R7.9 baseline
fc4bfa7042f59620b2c7ba1c4f6700cf3b02172a
test: target frontend casing collision

Mutation-causality repair
67624944da27ff1f1f5a1154018a255aae11d1fe
fix: avoid capturing mutation receiver out parameter
```

Keep these closed unless a real regression is demonstrated.

## R7.10 bounded positive

R7.10 answers, for the same exact R7.9-proven value path:

> What backend selection condition and frontend visibility condition jointly determine whether this rendered value is visible?

Supported backend shape remains intentionally narrow:

```csharp
var item = source.Single(item => predicate);
// or Enumerable.First(predicate)
return new Response
{
    DisplayValue = item.Value
};
```

Requirements include target-project Roslyn semantics, exact `System.Linq.Enumerable.Single/First`, exact selected reference local, scalar auto-properties, direct final response initializer, no user-defined conversion, exact selection invocation span and exact projection fact identity.

R7.9 supplies exact wire identity, typed frontend response member, exact service/result/state assignment and an authoritative active visible rendered-member fact. R7.10 frontend visibility is one supported enclosing Angular `@if` around that exact render.

Exact composition identity:

```text
renderedTerminal.frontendRenderFactId
= visibility.renderFactId

renderedTerminal.backendProjectionFactId
→ projection.selectionPredicateFactId
→ predicate.selectedApiProjectionFactId
```

No textual similarity may substitute for these IDs. Frontend evidence cannot upgrade an `observed-only` backend condition.

## Rereview #1 and #2 history

Rereview #1 repaired false rendered authority for:

```text
bare/inert <ng-template>
HTML comments
HTML tag/attribute interpolation
```

Rereview #2 then found static hidden ancestry remained over-authoritative:

```html
<section hidden>
  @if (displayPrice > 0) {
    <strong>{{ displayPrice }}</strong>
  }
</section>
```

Full review record:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-2.md`

## Static-hidden repair completed

Production checkpoint:

```text
97161baa2d0aff9131a7acf9db752393ae913d64
fix: reject statically hidden rendered text
```

The repair stays at the R7.9 render-authority boundary rather than patching R7.10 composition. It tracks bounded HTML element ancestry before a simple interpolation and rejects authority when any active ancestor contains the static HTML boolean `hidden` attribute.

Boundary details:

- `hidden` rejects authority;
- `hidden="false"` also rejects authority because HTML boolean attributes are presence-based;
- closed hidden siblings do not leak hidden state to later text;
- standard void/self-closing elements are not pushed as ancestors;
- tags inside HTML comments are ignored;
- ambiguous/mismatched close structure fails closed;
- dynamic Angular `[hidden]` is not treated as the static HTML attribute and is not solved by this repair;
- arbitrary CSS visibility, outlets, signals, structural directives and general runtime DOM semantics remain unsupported.

Regression files:

```text
tests/Pkc.CSharp.Tests/JointVisibilityTemplateAuthorityRegressionTests.cs
tests/Pkc.CSharp.Tests/JointVisibilityRegressionTests.cs
```

The end-to-end regression proves that a static hidden ancestor removes authoritative render/visibility and therefore prevents both the R7.9 rendered terminal and R7.10 joint fact.

Local .NET execution was unavailable in the implementation environment. The complete three-file implementation/test diff was reviewed before a single production push. Clean exact-SHA CI then provided executable verification.

## Exact-SHA verification

Exact `97161baa2d0aff9131a7acf9db752393ae913d64`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35686749488 — PASS
pinned Loren                                35686749493 — PASS
Loren-main canary                           35686749523 — PASS
pinned Jellyfin + parity/provenance         35686749575 — PASS
```

Core CI:

```text
Release build        0 warnings / 0 errors
C# tests             161 / 161 PASS
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
all facts project-semantic
portable parity/no-leak PASS
artifact 10676304879
sha256:7dcfed74eadb0146ab422c497953d8054ed01d5fddb6b6e6013a37b7729fd28b
```

## Repaired real-repository benchmark

Temporary wrapper is based exactly on `97161baa...`:

```text
branch:         benchmark/r710-hidden-97161baa
wrapper commit: 850eea0b64f464bdc1a5ea536adb39d4b912c650
run:            35687153720 — PASS, 3 / 3 jobs
```

Compare confirms the wrapper changes only the branch trigger line in `.github/workflows/real-repo-benchmark.yml`; production/test source is byte-identical.

Pinned targets and outputs:

```text
jin12-xyz/CRM @ 00493af54d4d9e146d1c6eb75f5dc8f3898f09ec
Target build success
PKC exit 0 | 415 facts | 1580 relations | 25 knowledge files
artifact 10677178181
sha256:ff0da4dc8cf0cafcd52a63bca3fc557d3a3159493d3c2b853461b8c529f89713

hackersandwizards/agentic-engineering-training-angular @ 22f2aab64617f4de7984370a5bd40e8c9535dbf5
Target build failure
PKC exit 0 | 441 facts | 660 relations | 26 knowledge files
artifact 10677435079
sha256:364d5b8a567ae7ba40ba12a564b6019eec2e0629c5a117a6c9aca63f0220bccb

kesetovic/crm-system @ 8e3b74bec4fdcd0144bd65f0c1b49c8e801bd2f7
Target build failure
PKC exit 0 | 488 facts | 2054 relations | 28 knowledge files
artifact 10676444714
sha256:b77758005534bfb292c965aec52fbda1b146667655db0279561b20a6a558d09d
```

Artifact inspection for every repository:

```text
R7.9 rendered UI terminal: 0
selected API projection:  0
ui-member-visibility:     0
joint-visibility:         0
combined visibility rule: 0
```

Agentic mutation cross-check remains closed:

```text
raw UpdatedAt mutation facts:        2
candidate UpdatedAt mutation facts:  0
runtime-pattern-variable:            retained
caller-object-unproven:              retained
transitive mutation warning:         retained
false PO-facing UpdatedAt claims:    0
```

Detailed record:

`docs/benchmarks/2026-09-22-r7.10-real-repo-benchmark.md`

This is fail-closed stress evidence. It does not satisfy R7.14 positive real-project yield. R7.14 remains **NOT PASS** and required for E.

## Next action — independent rereview #3

Review exact production SHA:

```text
97161baa2d0aff9131a7acf9db752393ae913d64
```

Request:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-3-request.md`

The reviewer must search for a new compile-valid/runtime-valid counterexample rather than merely re-confirm the static hidden fixtures.

If no blocker exists:

```text
mark R7.10 PASS / COMPLETE
→ mark all V0.4.7-D PASS / COMPLETE
→ unlock only V0.4.7-E
→ keep R7.14 NOT PASS and required for E
→ keep V0.5 locked
```

If a blocker exists, keep E locked and require regression-first minimum generic repair.

Do not start E in the implementation session that produced `97161baa...`.
