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
V0.4.7-D API to UI / R7.10 joint visibility      IMPLEMENTED / GATES PASS / PENDING INDEPENDENT REVIEW
V0.4.7-D overall                                 PENDING INDEPENDENT REVIEW
V0.4.7-E product acceptance                      LOCKED behind D
V0.5 Azure DevOps input evidence                 LOCKED
```

V0.4.7 acceptance is defined in `docs/v0.4.7-acceptance-plan.md`.
The permanent product contract is `docs/product-knowledge-contract.md`.

## Current production implementation checkpoint

```text
eb64309263489a2b9bd658762b4526a4a32a8508
fix: bound Angular visibility to active template structure
```

This is the exact R7.10 code SHA submitted for independent D review. A docs-only `[skip ci]` commit may sit on top of it; do not reset current `main` to the implementation SHA.

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

Post-R7.9 mutation-causality blocker repair
67624944da27ff1f1f5a1154018a255aae11d1fe
fix: avoid capturing mutation receiver out parameter
```

## Permanent product-knowledge invariants

Keep these classes distinct and retain deterministic lower-authority evidence when stronger authority fails:

```text
business conditions
value lineage / provenance
mutation / causality
```

Unsupported inference fails closed. Same/similar names are never sufficient proof.

## V0.4.7-D / R7.9 — PASS / COMPLETE

R7.9 proves the bounded property-level chain:

```text
C-proven backend response property
→ explicit JsonPropertyName wire identity
→ typed frontend HTTP result member
→ exact resolved service/API method
→ exact subscribe result member assignment
→ component/view-model member
→ authoritative Angular interpolation
→ rendered UI value
```

Route identity scopes endpoint correlation only; it is not property-level proof. C evidence survives when D composition fails.

Accepted R7.9 exact-SHA gates on `fc4bfa7042f59620b2c7ba1c4f6700cf3b02172a`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35588410936 — PASS
pinned Loren                                35588410941 — PASS
Loren-main canary                           35588410930 — PASS
pinned Jellyfin                             35588410939 — PASS
```

## Post-R7.9 mutation-causality blocker — PASS / CLOSED

The pinned real-repository benchmark exposed a false transitive mutation promotion through runtime pattern-selected helper receivers. The accepted repair keeps raw mutation evidence but marks unproven caller-object causality and denies workflow promotion.

Accepted implementation:

```text
67624944da27ff1f1f5a1154018a255aae11d1fe
```

Exact-SHA standard gates:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35632901150 — PASS
pinned Loren                                35632901064 — PASS
Loren-main canary                           35632901146 — PASS
pinned Jellyfin + parity/provenance         35632901144 — PASS
```

Pinned blocker re-benchmark: `35633768883` — PASS, 3 / 3 jobs.

## V0.4.7-D / R7.10 — IMPLEMENTED / GATES PASS / PENDING INDEPENDENT REVIEW

R7.10 now has a bounded positive proof for the same exact R7.9 value path:

```text
Enumerable.Single/First(predicate)
→ exact selected reference local
→ exact scalar auto-property
→ direct API response property projection
→ explicit wire identity
→ exact R7.9 frontend result/member/state/render identity
→ one authoritative line-anchored Angular @if around that exact render
→ joint backend/frontend visibility evidence
```

The selected backend predicate is upgraded to observable only when its exact invocation span is the selection that produced the receiver of the exact API projection. R7.10 composition then reuses exact fact IDs:

```text
rendered terminal.frontendRenderFactId
= frontend visibility.renderFactId

rendered terminal.backendProjectionFactId
→ projection.selectionPredicateFactId
→ predicate.selectedApiProjectionFactId
```

No type/name/text similarity creates a joint edge.

### R7.10 authority boundary

- Backend business-condition authority stays independent from frontend visibility evidence.
- Frontend visibility cannot upgrade an `observed-only` backend condition.
- Multiple matching frontend visibility facts fail closed and remain independently retained.
- Nested visibility currently fails closed.
- Inert `@if` text inside HTML comments or HTML tags is rejected.
- Current authoritative frontend visibility grammar is deliberately narrow: active Angular `@if` must start a template line after whitespace. This is not a general Angular compiler or arbitrary template expression solver.
- Failure to compose R7.10 retains accepted R7.9/C evidence.

Focused regressions:

```text
tests/Pkc.CSharp.Tests/JointVisibilityRegressionTests.cs
tests/Pkc.CSharp.Tests/JointVisibilityTemplateAuthorityRegressionTests.cs
```

They cover the positive chain, unrelated same-text predicate isolation, ambiguous visibility retention, nested visibility fail-closed, observed-only authority retention, and same-line/multiline HTML-comment false positives.

### Exact-SHA R7.10 standard gates

All standard gates passed on `eb64309263489a2b9bd658762b4526a4a32a8508`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35646162832 — PASS
pinned Loren                                35646163109 — PASS
Loren-main canary                           35646163085 — PASS
pinned Jellyfin + parity/provenance         35646163035 — PASS
```

Core verification includes Release build, full C#/frontend tests, tool pack/install, WorkPlay and PokeTrade product-knowledge verification.

### R7.10 pinned three-repository benchmark

Benchmark source base:

```text
implementation:     eb64309263489a2b9bd658762b4526a4a32a8508
branch:             benchmark/r710-eb643092
wrapper commit:     0bd0e501cc2bfc1f04d7cee275f43e5f888b3d1c
run:                35646837448 — PASS, 3 / 3 matrix jobs
```

The wrapper changes only the benchmark workflow trigger. PKC source/test logic is byte-identical to the implementation SHA.

Artifact inspection:

```text
jin12-xyz/CRM
PKC exit 0 | 415 facts | 1580 relations | 25 knowledge files

hackersandwizards/agentic-engineering-training-angular
PKC exit 0 | 441 facts | 660 relations | 26 knowledge files

kesetovic/crm-system
PKC exit 0 | 488 facts | 2054 relations | 28 knowledge files
```

All three naturally emit:

```text
R7.9 rendered-value terminal: 0
selected API projection:      0
ui-member-visibility:         0
joint-visibility candidate:   0
combined visibility rule:     0
```

This is expected fail-closed behavior for unsupported real-world shapes, not positive R7.14 yield. The agentic benchmark still retains both raw `UpdatedAt` mutation facts with `runtime-pattern-variable` / `caller-object-unproven`, promotes them into 0 candidates, and retains the transitive-mutation uncertainty warning.

R7.14 therefore remains **NOT YET PASS**.

Detailed R7.10 benchmark evidence: `docs/benchmarks/2026-09-22-r7.10-real-repo-benchmark.md`.

## Current external gate

R7.10 implementation and required automated gates are complete, but all of V0.4.7-D is **not complete until independent rereview passes**.

Independent review request:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-review-request.md`

Do not start E while D is under review.

## Version semantics

Roadmap, package and serialized schema versions remain independent:

```text
roadmap:             V0.4.7-D / R7.10 pending independent review
tool/package:        RuaDen.Pkc.Tool 0.4.3-preview.2
C# raw schema:       0.4.4-csharp-raw
merged facts schema: 0.4.4
cross-stack schema:  0.4.6
frontend schema:     0.4.3-frontend
```

No package/schema bump is implied by this checkpoint.

## Carried non-blocking warnings

```text
W10.1
Observed-only predicate Evidence is separated from Rules but does not explicitly render the `observed-only` label outside the new joint-visibility evidence/unknown path.

W10.2
Queryable names remain in old safe-operation sets but are unreachable behind the Queryable fail-closed guard.
```

## Exact next action

Perform an **independent rereview of exact implementation SHA `eb64309263489a2b9bd658762b4526a4a32a8508`** against the R7.10 acceptance boundary and regressions.

If the independent review finds no compile-valid/runtime-valid counterexample, mark R7.10 and all of D PASS / COMPLETE and only then unlock E.

```text
V0.4.7-E LOCKED until independent D review passes
V0.5 LOCKED until V0.4.7 / V0.4.x PO-question-readiness exit gate passes
```
