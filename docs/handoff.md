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
7. `docs/benchmarks/2026-09-22-r7.10-real-repo-benchmark.md`
8. `docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-2.md`

Then inspect current `main`, recent commits and repository status. Never reset to a historical SHA merely because this handoff names it.

## Current production checkpoint

Exact production SHA independently rereviewed and rejected:

```text
da5d23771ef8c9d58d0333d1f949e8d742210043
fix: require visible text interpolation
```

A docs-only `[skip ci]` checkpoint may sit above this SHA on `main`. Review or repair production behavior from current `main`; do not reset `main` to `da5d2377...`.

## Current milestone state

```text
V0.4.7-A                                PASS / COMPLETE
V0.4.7-B                                PASS / COMPLETE
V0.4.7-C                                PASS / COMPLETE
V0.4.7-D / R7.9                         PASS / COMPLETE
V0.4.7-D mutation-causality blocker     PASS / CLOSED
V0.4.7-D / R7.10                        REREVIEW #2 FAIL — STATIC HIDDEN ANCESTOR BLOCKER
V0.4.7-D overall                        OPEN / BLOCKED
V0.4.7-E                                LOCKED behind D
V0.5 Azure DevOps                       LOCKED
```

Do not start E or V0.5.

## Permanent contract

PKC compiles deterministic implementation evidence into portable PO-facing knowledge. Keep these evidence classes distinct:

```text
business conditions
value lineage / provenance
mutation / causality
```

Unsupported inference fails closed. Same/similar names are not proof. If stronger composition fails, preserve deterministic lower-authority evidence.

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

V0.4.7-D / R7.9
fc4bfa7042f59620b2c7ba1c4f6700cf3b02172a
test: target frontend casing collision

Mutation-causality repair
67624944da27ff1f1f5a1154018a255aae11d1fe
fix: avoid capturing mutation receiver out parameter
```

Keep these closed unless a real regression is demonstrated.

## R7.10 intended bounded positive

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

Requirements include target-project Roslyn semantics, exact `System.Linq.Enumerable.Single/First`, exact selected reference local, scalar auto-properties, direct final response initializer, no user-defined conversion, exact invocation span and exact projection fact identity.

R7.9 then supplies exact wire identity, typed frontend response member, exact service/result/state assignment and an authoritative rendered-member fact.

Supported frontend visibility is one bounded active line-anchored Angular `@if` around that exact render.

Exact composition identity remains:

```text
renderedTerminal.frontendRenderFactId
= visibility.renderFactId

renderedTerminal.backendProjectionFactId
→ projection.selectionPredicateFactId
→ predicate.selectedApiProjectionFactId
```

No property/type/predicate text similarity may substitute for those IDs. Frontend evidence cannot upgrade an `observed-only` backend condition.

## Rereview #1 repairs already present

The first independent rereview found false rendered-value authority for:

```text
bare/inert <ng-template>
HTML comments
HTML tag/attribute interpolation
```

Regression-first repairs were:

```text
05ad6cb937010702a3fd01d5ef756e31bc4e8d9b
099fabfcaeecbaf009b75052d20075745ee02437
da5d23771ef8c9d58d0333d1f949e8d742210043
```

Focused regression file:

`tests/Pkc.CSharp.Tests/JointVisibilityTemplateAuthorityRegressionTests.cs`

## Independent rereview #2 blocker

Rereview #2 independently challenged the repaired SHA and found another compile-valid/runtime-valid frontend authority counterexample:

```html
<section hidden>
  @if (displayPrice > 0) {
    <strong>{{ displayPrice }}</strong>
  }
</section>
```

The standard HTML `hidden` attribute prevents the subtree from being presented/rendered to the user. Current PKC still accepts the simple interpolation because it is not inside a comment, tag/attribute, or inert `<ng-template>`.

Current path to false authority:

```text
simple interpolation under hidden ancestor
→ ui-member-render survives authority filter
→ renderAuthority added
→ one enclosing @if creates ui-member-visibility
→ exact R7.9 render fact becomes rendered UI value terminal
→ exact R7.10 composition creates observable joint-visibility
→ PO-facing rule says visible/rendered when @if condition is true
```

That claim is false because static hidden ancestry prevents user-visible rendering regardless of the `@if` result.

The blocker is at the **R7.9 rendered-value / user-visible text authority boundary** consumed by R7.10. Exact R7.10 fact IDs are not the problem; the exact render fact is already over-authoritative.

Full review record:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-2.md`

## Required regression-first repair

Do not patch production first.

1. Add a focused negative regression for simple interpolation under a statically hidden HTML ancestor.
2. Confirm exact current production behavior fails that regression.
3. Implement the minimum generic repair in the render-authority boundary:
   - reject interpolation under a statically hidden HTML ancestor;
   - handle the standard boolean `hidden` attribute generically across enclosing elements;
   - fail closed on ambiguous ancestor structure;
   - do not broaden into arbitrary CSS visibility, dynamic `[hidden]`, outlets, signals, structural directives or a general Angular/DOM solver without separate proof.
4. Run the focused regression locally.
5. Run related frontend/R7.9/R7.10 tests locally.
6. Run the full relevant local suite/build.
7. Review the complete diff.
8. Commit the regression and generic repair together.
9. Push once.
10. Run exact-SHA standard gates and repaired real-repository benchmark.
11. Request a fresh independent rereview.

Expected negative result for the regression:

```text
no authoritative ui-member-render
no ui-member-visibility
no rendered UI value terminal
no joint-visibility fact
```

## Existing exact-SHA gates

Previously green gates on `da5d2377...` remain regression-stability evidence only:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35650084914 — PASS
pinned Loren                                35650084917 — PASS
Loren-main canary                           35650084926 — PASS
pinned Jellyfin + parity/provenance         35650084759 — PASS
```

The repaired three-repository benchmark also passed (`35650761753`) with zero current R7.9/R7.10 positives. That does not cover the new static-hidden counterexample and does not satisfy R7.14.

R7.14 remains **NOT PASS** and required for E.

## Next action

```text
current checkpoint remains V0.4.7-D / R7.10
→ regression-first static-hidden-ancestor repair
→ local verification
→ one coherent production commit/push
→ exact-SHA gates + benchmark
→ new independent rereview
```

Do not start V0.4.7-E until the repaired R7.10 SHA independently passes. Keep V0.5 locked.