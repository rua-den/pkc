# PKC Handoff

Use this file when continuing PKC in another coding or review thread.

Last updated: 2026-09-22

## Read first

Read in this exact order before changing production code:

1. `docs/status.md`
2. this handoff
3. `docs/milestones.md`
4. `docs/product-knowledge-contract.md`
5. `docs/v0.4.7-acceptance-plan.md`
6. `docs/reviews/2026-09-15-v0.4.6-independent-rereview-10.md`
7. `docs/reviews/2026-09-16-v0.4.7-plan-readiness-review.md`
8. `docs/reviews/2026-09-19-v0.4.7-c-final-rereview.md`
9. `docs/benchmarks/2026-09-21-real-repo-r7.9-benchmark.md`
10. `docs/benchmarks/2026-09-22-r7.10-real-repo-benchmark.md`
11. `docs/reviews/2026-09-22-v0.4.7-d-r7.10-review-request.md`

Then inspect current `main`, recent commits and repository status. Never reset to a historical SHA merely because this handoff names it.

## Current production checkpoint

The exact R7.10 implementation/test checkpoint submitted for independent D review is:

```text
eb64309263489a2b9bd658762b4526a4a32a8508
fix: bound Angular visibility to active template structure
```

A docs-only `[skip ci]` commit may sit on top. Continue from current `main`; do not reset.

## Current milestone state

```text
V0.4.7-A                                PASS / COMPLETE
V0.4.7-B                                PASS / COMPLETE
V0.4.7-C                                PASS / COMPLETE
V0.4.7-D / R7.9                         PASS / COMPLETE
V0.4.7-D mutation-causality blocker     PASS / CLOSED
V0.4.7-D / R7.10                        IMPLEMENTED / GATES PASS / PENDING INDEPENDENT REVIEW
V0.4.7-D overall                        PENDING INDEPENDENT REVIEW
V0.4.7-E                                LOCKED behind D
V0.5 Azure DevOps                       LOCKED
```

Do not start E or V0.5 while D is under independent review.

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

## Permanent product contract

PKC is a deterministic Product/System Knowledge Compiler:

```text
source inputs
→ deterministic analyzers/adapters
→ evidence/facts
→ feature/workflow/business-decision candidates
→ knowledge synthesis
→ canonical model
→ portable rendering
```

Keep these classes distinct:

```text
business conditions
value lineage / provenance
mutation / causality
```

Conservative authority downgrade must retain independently proven lower-authority evidence. Unsupported inference fails closed. Same/similar names are never sufficient proof.

## R7.9 accepted boundary

R7.9 proves:

```text
C-proven backend response property
→ explicit JsonPropertyName wire identity
→ typed frontend HTTP result member
→ exact resolved service/API method
→ exact subscribe result receiver/member
→ component/view-model assignment
→ authoritative Angular interpolation
→ rendered UI value
```

Route identity scopes endpoint correlation only. It does not prove the property edge.

Focused R7.9 regressions:

```text
tests/Pkc.CSharp.Tests/ApiResponseFrontendBindingRegressionTests.cs
tests/Pkc.CSharp.Tests/ApiResponseFrontendBindingIsolationRegressionTests.cs
```

## Mutation-causality benchmark blocker — PASS / CLOSED

The real-repository benchmark exposed false workflow promotion of runtime pattern-selected timestamp receivers. The repair keeps raw mutations but marks:

```text
mutationReceiverOrigin = runtime-pattern-variable
mutationCausalityBoundary = caller-object-unproven
```

and denies transitive workflow promotion unless causality is proven. Ordinary domain/local mutations remain supported.

Regression:

`tests/Pkc.CSharp.Tests/TransitiveMutationCausalityRegressionTests.cs`

Accepted exact-SHA gates on `67624944...`:

```text
core + PokeTrade   35632901150 — PASS
Loren pinned       35632901064 — PASS
Loren-main         35632901146 — PASS
Jellyfin           35632901144 — PASS
```

## R7.10 implemented boundary

R7.10 answers:

> For the same exact R7.9 value path, what backend selection condition and frontend visibility condition jointly control whether the value is rendered?

### Supported backend positive

The new D-specific selected-item proof does not weaken C. It supports only the bounded shape:

```text
var item = source.Single(item => predicate);
// or Enumerable.First(predicate)
return new Response
{
    DisplayValue = item.Value
};
```

Requirements include:

- target-project Roslyn semantics;
- `System.Linq.Enumerable.Single` or `First`, not Queryable;
- exact selected reference local;
- exact scalar auto-properties;
- direct final response object initializer;
- no user-defined conversion;
- exact selection invocation span and exact projection fact identity.

The existing business predicate becomes authoritative for this selected API item only when the predicate fact exactly matches the selection invocation and the exact projection:

```text
businessRuleAuthority = observable
observableContext = selected-api-response-item
selectedApiProjectionFactId = <exact projection fact>
```

### Supported frontend positive

R7.10 consumes an already-authoritative R7.9 `ui-member-render` and supports one enclosing active Angular `@if`:

```html
@if (displayPrice > 0) {
  <strong>{{ displayPrice }}</strong>
}
```

The authoritative visibility grammar is intentionally narrower than Angular itself:

- `@if` must start a template line after whitespace;
- only one enclosing supported `@if` is accepted;
- nested/multiple visibility fails closed;
- inert `@if` inside HTML comments is rejected, including multiline comments;
- `@if` inside an HTML tag/attribute is rejected;
- this is not Angular compiler semantics and does not claim `*ngIf`, arbitrary one-line control flow, list filtering or signal analysis.

### Exact R7.10 composition identity

The joint edge reuses existing fact identities instead of textual similarity:

```text
R7.9 rendered terminal.frontendRenderFactId
= ui-member-visibility.renderFactId

R7.9 rendered terminal.backendProjectionFactId
→ exact projection.selectionPredicateFactId
→ exact predicate.selectedApiProjectionFactId
```

If there are zero or multiple supported frontend visibility facts, no joint fact is created. Independent R7.9 and visibility evidence remain retained.

### Authority separation

A joint fact is `observable` only when the backend predicate is already `observable`. If backend authority is `observed-only`, frontend visibility cannot upgrade it; PKC retains evidence and emits uncertainty instead of an authoritative combined rule.

### R7.10 implementation files

```text
src/Pkc.CSharp/CSharpSelectedApiProjectionLineageEnricher.cs
src/Pkc.CSharp/CSharpSelectedApiPredicateAuthorityEnricher.cs
src/Pkc.CSharp/CSharpEvidenceScanner.cs
src/Pkc.Frontend/AngularRenderedMemberVisibilityEnricher.cs
src/Pkc.Frontend/AngularRenderedMemberVisibilityAuthorityFilter.cs
src/Pkc.Frontend/AngularFrontendAdapter.cs
src/Pkc.Knowledge/JointVisibilityCandidateEnricher.cs
src/Pkc.Knowledge/JointVisibilityKnowledgeSynthesizer.cs
src/Pkc.Cli/Program.cs
```

Focused regressions:

```text
tests/Pkc.CSharp.Tests/JointVisibilityRegressionTests.cs
tests/Pkc.CSharp.Tests/JointVisibilityTemplateAuthorityRegressionTests.cs
```

Coverage includes:

1. positive exact selected-item → R7.9 → exact `@if` joint visibility;
2. unrelated same-text predicate isolation;
3. ambiguous frontend visibility fail-closed while retaining R7.9 and visibility evidence;
4. nested visibility fail-closed;
5. observed-only backend authority cannot be upgraded;
6. same-line HTML-comment `@if` false positive rejected;
7. multiline HTML-comment `@if` false positive rejected.

## Exact-SHA automated verification

All required standard gates passed on `eb64309263489a2b9bd658762b4526a4a32a8508`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35646162832 — PASS
pinned Loren                                35646163109 — PASS
Loren-main canary                           35646163085 — PASS
pinned Jellyfin + parity/provenance         35646163035 — PASS
```

Core CI confirms Release build, full C#/frontend tests, tool pack/install, WorkPlay and PokeTrade product-knowledge verification.

## Pinned R7.10 real-repository benchmark

Because workflow dispatch is not exposed through the connector, the diagnostic matrix was rerun using a temporary branch based exactly on the implementation SHA:

```text
base implementation: eb64309263489a2b9bd658762b4526a4a32a8508
branch:              benchmark/r710-eb643092
wrapper commit:      0bd0e501cc2bfc1f04d7cee275f43e5f888b3d1c
benchmark run:       35646837448 — PASS, 3 / 3 jobs
```

The wrapper changes only `.github/workflows/real-repo-benchmark.yml` trigger scope. PKC source/test logic is byte-identical to main implementation.

Artifact summary:

```text
jin12-xyz/CRM
PKC exit 0 | 415 facts | 1580 relations | 25 knowledge files

hackersandwizards/agentic-engineering-training-angular
PKC exit 0 | 441 facts | 660 relations | 26 knowledge files

kesetovic/crm-system
PKC exit 0 | 488 facts | 2054 relations | 28 knowledge files
```

All three unchanged repositories produced zero current supported R7.9/R7.10 positives:

```text
rendered UI value terminal: 0 / repo
selected API projection:    0 / repo
ui-member-visibility:       0 / repo
joint-visibility:           0 / repo
combined rule:              0 / repo
```

That is expected fail-closed behavior. Do not treat zero-yield as a bug and do not broaden authority merely to pass R7.14.

Agentic-angular mutation regression remains closed:

```text
raw user.UpdatedAt mutation:       retained
raw contact.UpdatedAt mutation:    retained
receiver origin:                   runtime-pattern-variable
causality boundary:                caller-object-unproven
candidate inclusion count:         0
transitive-mutation warning:       retained
```

R7.14 real-project positive yield therefore remains **NOT YET PASS**.

Full benchmark record:

`docs/benchmarks/2026-09-22-r7.10-real-repo-benchmark.md`

## External gate now blocking D completion

The implementation thread has completed code, regression, standard exact-SHA verification and the pinned benchmark. It must not self-certify independence.

Independent review exact target:

```text
eb64309263489a2b9bd658762b4526a4a32a8508
```

Review request:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-review-request.md`

The reviewer should search for compile-valid/runtime-valid counterexamples to:

- selected API projection identity;
- exact predicate-to-projection authority;
- Angular visibility structure/lexical authority;
- exact R7.9 composition identity;
- lower-authority retention and non-upgrade;
- inert/commented template false positives;
- preservation of accepted C/R7.9 and mutation-causality behavior.

If no blocker exists, the reviewer may mark R7.10 and all of D PASS / COMPLETE. Only then may E unlock.

## Version semantics

```text
roadmap:             V0.4.7-D / R7.10 pending independent review
tool/package:        RuaDen.Pkc.Tool 0.4.3-preview.2
C# raw schema:       0.4.4-csharp-raw
merged facts schema: 0.4.4
cross-stack schema:  0.4.6
frontend schema:     0.4.3-frontend
```

No package/schema bump is implied by this checkpoint.

## If another session takes over

1. Verify current `main`; expect a docs-only `[skip ci]` commit on top of `eb643092...`.
2. Do **not** start E or V0.5.
3. Run an independent R7.10/D rereview against exact implementation SHA `eb643092...`.
4. If review PASS, update status/handoff/milestones/acceptance plan and unlock only E.
5. If review finds a blocker, reproduce it regression-first, make the minimum generic fix, run focused/full exact-SHA gates and re-review.
