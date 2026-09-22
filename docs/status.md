# PKC Status

Last updated: 2026-09-22

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                 PASS / COMPLETE
V0.4.5 real-repository generalization            PASS / COMPLETE
V0.4.6 business logic reconstruction             PASS / COMPLETE
V0.4.7-A origin and copy timing                  PASS / COMPLETE
V0.4.7-B computation and later change            PASS / COMPLETE
V0.4.7-C backend to API                          PASS / COMPLETE
V0.4.7-D API to UI / R7.9 binding                PASS / COMPLETE
V0.4.7-D mutation-causality benchmark blocker    PASS / CLOSED
V0.4.7-D API to UI / R7.10 joint visibility      REREVIEW #2 FAIL — STATIC HIDDEN ANCESTOR BLOCKER
V0.4.7-D overall                                 OPEN / BLOCKED
V0.4.7-E product acceptance                      LOCKED behind D
V0.5 Azure DevOps input evidence                 LOCKED
```

V0.4.7 acceptance is defined in `docs/v0.4.7-acceptance-plan.md`.
The permanent product contract is `docs/product-knowledge-contract.md`.

## Production SHA that failed independent rereview #2

```text
da5d23771ef8c9d58d0333d1f949e8d742210043
fix: require visible text interpolation
```

Do not reset `main` to this SHA. A docs-only checkpoint may sit above it.

## Closed production baselines

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

Post-R7.9 mutation-causality repair
67624944da27ff1f1f5a1154018a255aae11d1fe
fix: avoid capturing mutation receiver out parameter
```

Keep accepted predecessors closed unless a real regression is demonstrated.

## Permanent invariants

Keep these evidence classes distinct:

```text
business conditions
value lineage / provenance
mutation / causality
```

Unsupported inference fails closed. Same/similar names are never sufficient proof. Conservative authority downgrade must retain independently proven lower-authority evidence.

## R7.10 intended bounded positive

R7.10 composes only the same exact R7.9-proven value path:

```text
Enumerable.Single/First(predicate)
→ exact selected reference local
→ exact scalar auto-property
→ direct API response property projection
→ explicit wire identity
→ exact R7.9 frontend result/member/state/render identity
→ one authoritative Angular @if around that exact rendered text interpolation
→ joint backend/frontend visibility evidence
```

Frontend visibility cannot upgrade an `observed-only` backend predicate. Zero or multiple supported frontend visibility facts fail closed. Failure to compose R7.10 retains R7.9/C and independently proven evidence.

## Rereview #1 repairs already present

Rereview #1 found and repaired false rendered-value authority for:

```text
interpolation under inert <ng-template>
interpolation inside HTML comments
interpolation inside HTML tags/attributes
```

Repair sequence:

```text
05ad6cb937010702a3fd01d5ef756e31bc4e8d9b
099fabfcaeecbaf009b75052d20075745ee02437
da5d23771ef8c9d58d0333d1f949e8d742210043
```

Focused coverage remains in:

`tests/Pkc.CSharp.Tests/JointVisibilityTemplateAuthorityRegressionTests.cs`

## Independent rereview #2 — FAIL

A new compile-valid/runtime-valid Angular counterexample remains over-authoritative at exact `da5d2377...`:

```html
<section hidden>
  @if (displayPrice > 0) {
    <strong>{{ displayPrice }}</strong>
  }
</section>
```

The standard HTML `hidden` attribute prevents the subtree from being rendered to the user, but the current R7.9 authority filter does not inspect static hidden ancestry. The interpolation therefore can still receive `renderAuthority`, receive `ui-member-visibility` from the enclosing `@if`, compose into a `rendered UI value` terminal, and then produce an observable R7.10 joint-visibility rule.

That violates the required `active visible Angular text interpolation` proof boundary. Exact fact-ID composition does not save the result because the exact render fact itself is over-authoritative.

Review record:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-2.md`

## Existing exact-SHA verification

The previously recorded standard gates for `da5d2377...` remain PASS and are regression-stability evidence only:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35650084914 — PASS
pinned Loren                                35650084917 — PASS
Loren-main canary                           35650084926 — PASS
pinned Jellyfin + parity/provenance         35650084759 — PASS
```

Core evidence:

```text
Release build        0 warnings / 0 errors
C# tests             155 / 155 PASS
frontend tests       13 / 13 PASS
tool pack/install    PASS
WorkPlay build       PASS
PokeTrade            PASS
```

These do not override the semantic blocker.

## Repaired three-repository benchmark remains historical evidence

Benchmark run `35650761753` passed on a wrapper branch based exactly on `da5d2377...` and showed zero current R7.9/R7.10 positives across the three unchanged pinned repositories. That remains useful fail-closed stress evidence but does not cover the new static-hidden counterexample and does not satisfy R7.14.

R7.14 remains **NOT PASS** and is still required for E.

Detailed benchmark record:

`docs/benchmarks/2026-09-22-r7.10-real-repo-benchmark.md`

## Required next action

This review thread does not modify production code.

Required implementation sequence:

```text
add a regression for interpolation under a statically hidden HTML ancestor
→ confirm current production fails the regression
→ implement minimum generic static-hidden-ancestor rejection
→ focused + related + full local verification
→ review diff
→ one coherent production commit and one push
→ exact-SHA standard gates + real-repository benchmark
→ fresh independent rereview
```

Do not advance V0.4.7-E until the repaired SHA independently passes. Keep V0.5 locked.

## Version semantics

```text
roadmap:             V0.4.7-D / R7.10 BLOCKED after independent rereview #2
tool/package:        RuaDen.Pkc.Tool 0.4.3-preview.2
C# raw schema:       0.4.4-csharp-raw
merged facts schema: 0.4.4
cross-stack schema:  0.4.6
frontend schema:     0.4.3-frontend
```

No package/schema bump is implied by this review outcome.