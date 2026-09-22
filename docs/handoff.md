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
6. `docs/benchmarks/2026-09-21-real-repo-r7.9-benchmark.md`
7. `docs/benchmarks/2026-09-22-r7.10-real-repo-benchmark.md`
8. `docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-1.md`
9. `docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-2-request.md`

Then inspect current `main`, recent commits and repository status. Never reset to a historical SHA merely because this handoff names it.

## Current production checkpoint

Exact R7.10 repair candidate for Astra review:

```text
da5d23771ef8c9d58d0333d1f949e8d742210043
fix: require visible text interpolation
```

A docs-only `[skip ci]` checkpoint may sit above this SHA on `main`. Review production behavior at `da5d2377...`; do not reset `main`.

## Current milestone state

```text
V0.4.7-A                                PASS / COMPLETE
V0.4.7-B                                PASS / COMPLETE
V0.4.7-C                                PASS / COMPLETE
V0.4.7-D / R7.9                         PASS / COMPLETE
V0.4.7-D mutation-causality blocker     PASS / CLOSED
V0.4.7-D / R7.10                        REPAIRED / ALL GATES PASS / PENDING REREVIEW #2
V0.4.7-D overall                        PENDING INDEPENDENT REREVIEW #2
V0.4.7-E                                LOCKED behind D
V0.5 Azure DevOps                       LOCKED
```

Do not start E or V0.5 until D is independently accepted.

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

Keep these closed unless Astra finds a real regression.

## R7.10 bounded positive

R7.10 answers, for the same exact R7.9-proven value path:

> What backend selection condition and frontend visibility condition jointly determine whether this rendered value is visible?

Supported backend shape is intentionally narrow:

```csharp
var item = source.Single(item => predicate);
// or Enumerable.First(predicate)
return new Response
{
    DisplayValue = item.Value
};
```

Requirements include target-project Roslyn semantics, `System.Linq.Enumerable.Single/First`, exact selected reference local, scalar auto-properties, direct final response initializer, no user-defined conversion, exact invocation span and exact projection fact identity.

R7.9 then supplies exact wire identity, typed frontend response member, exact service/result/state assignment and an authoritative rendered-member fact.

Supported R7.10 frontend visibility is one bounded active line-anchored Angular `@if` around that exact render:

```html
@if (displayPrice > 0) {
  <strong>{{ displayPrice }}</strong>
}
```

Exact composition identity:

```text
renderedTerminal.frontendRenderFactId
= visibility.renderFactId

renderedTerminal.backendProjectionFactId
→ projection.selectionPredicateFactId
→ predicate.selectedApiProjectionFactId
```

No property/type/predicate text similarity may substitute for those IDs. Frontend evidence cannot upgrade an `observed-only` backend condition.

## Rereview #1 findings and repair

The first independent rereview targeted the original candidate:

```text
eb64309263489a2b9bd658762b4526a4a32a8508
```

It found a real false-authority class at the R7.9 rendered-value boundary. PKC could classify interpolation text as rendered even when Angular would not visibly render that text.

Counterexamples:

```html
<ng-template>
  @if (displayPrice > 0) {
    {{ displayPrice }}
  }
</ng-template>

<!-- {{ displayPrice }} -->

<div data-price="{{ displayPrice }}"></div>
```

Regression-first repairs:

```text
05ad6cb937010702a3fd01d5ef756e31bc4e8d9b
fix: fail closed on inert ng-template renders

099fabfcaeecbaf009b75052d20075745ee02437
fix: reject inert commented renders

da5d23771ef8c9d58d0333d1f949e8d742210043
fix: require visible text interpolation
```

The resulting boundary intentionally prefers false negatives over false authority. Interpolation in HTML comments, tag/attribute context, or under an inert `<ng-template>` ancestor is not authoritative rendered UI text.

Focused regression file:

`tests/Pkc.CSharp.Tests/JointVisibilityTemplateAuthorityRegressionTests.cs`

Full review record:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-1.md`

Because rereview #1 also repaired its findings, it must not self-certify the repaired SHA. Astra is rereview #2.

## Exact-SHA verification on repaired candidate

Exact `da5d23771ef8c9d58d0333d1f949e8d742210043`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35650084914 — PASS
pinned Loren                                35650084917 — PASS
Loren-main canary                           35650084926 — PASS
pinned Jellyfin + parity/provenance         35650084759 — PASS
```

Core CI:

```text
Release build        0 warnings / 0 errors
C# tests             155 / 155 PASS
frontend tests       13 / 13 PASS
tool pack/install    PASS
WorkPlay             PASS
PokeTrade            PASS
```

## Repaired real-repository benchmark

Benchmark wrapper was based exactly on `da5d2377...`:

```text
branch:         benchmark/r710-rereview1-05ad6cb9
wrapper commit: d38b21a68e007c1a85a8ec8e906a08f0db440661
run:            35650761753 — PASS, 3 / 3 jobs
```

GitHub compare confirms the wrapper changes only one branch-trigger line in `.github/workflows/real-repo-benchmark.yml`.

Pinned targets and artifact results:

```text
jin12-xyz/CRM @ 00493af54d4d9e146d1c6eb75f5dc8f3898f09ec
Target build: success
PKC exit 0 | 415 facts | 1580 relations | 25 knowledge files
artifact 10662460505
sha256:f846b5384cc6472bf76ee469bfda888d5721664a440d2f18bf2e358d2f130753

hackersandwizards/agentic-engineering-training-angular @ 22f2aab64617f4de7984370a5bd40e8c9535dbf5
Target build: failure
PKC exit 0 | 441 facts | 660 relations | 26 knowledge files
artifact 10661878054
sha256:1a0e1eae1af7cba2c786dfb2c38050387f42dd6f2cc00708af4020ecd9673286

kesetovic/crm-system @ 8e3b74bec4fdcd0144bd65f0c1b49c8e801bd2f7
Target build: failure
PKC exit 0 | 488 facts | 2054 relations | 28 knowledge files
artifact 10662505428
sha256:48e3620862ec46aee4a1dbb119326be81fda60d38f8e44881208b1605bc1d399
```

All three unchanged repos emit:

```text
R7.9 rendered UI terminal: 0
selected API projection:  0
ui-member-visibility:     0
joint-visibility:         0
combined visibility rule: 0
```

That is a stress/fail-closed result, not positive product yield. R7.14 remains **NOT YET PASS**. Do not weaken authority merely to produce benchmark positives.

Agentic mutation cross-check:

```text
raw UpdatedAt mutation facts:        2
candidate UpdatedAt mutation facts:  0
runtime-pattern-variable:            retained
caller-object-unproven:              retained
transitive mutation warning:         retained
false PO-facing UpdatedAt claims:    0
```

Detailed evidence:

`docs/benchmarks/2026-09-22-r7.10-real-repo-benchmark.md`

## Astra / independent rereview #2

Review exact production SHA:

```text
da5d23771ef8c9d58d0333d1f949e8d742210043
```

Request:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-2-request.md`

Astra should independently challenge:

- exact selected API projection identity;
- exact predicate-to-projection authority;
- R7.9 rendered-value authority after the new text-render restrictions;
- Angular comment/tag/attribute/`ng-template` inert boundaries;
- exact R7.9 ↔ visibility composition IDs;
- ambiguous/nested visibility fail-closed behavior;
- observed-only non-upgrade;
- preservation of accepted C/R7.9 and mutation-causality behavior.

If Astra finds no compile-valid/runtime-valid counterexample, it may mark R7.10 and all of D PASS / COMPLETE and unlock only E. R7.14 stays required for E and V0.5 stays locked.

If a blocker exists, do not advance E. Record the exact counterexample and repair regression-first.

## Next action

```text
independent Astra rereview #2 of exact da5d2377...
→ PASS: close D, unlock only E
→ FAIL: regression-first repair, exact-SHA gates, rereview again
```
