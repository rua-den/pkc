# PKC Handoff

Use this file when continuing PKC in another coding or review thread.

Last updated: 2026-09-20

## Read first

Read in this exact order before changing production code:

1. `docs/status.md`
2. this handoff
3. `docs/milestones.md`
4. `docs/product-knowledge-contract.md`
5. `docs/v0.4.7-acceptance-plan.md`
6. `docs/reviews/2026-09-15-v0.4.6-independent-rereview-10.md`
7. `docs/reviews/2026-09-16-v0.4.7-plan-readiness-review.md`
8. `docs/reviews/2026-09-16-v0.4.7-snapshot-checkpoint-review.md`
9. `docs/reviews/2026-09-17-v0.4.7-alias-composition-review.md`
10. `docs/reviews/2026-09-17-v0.4.7-reference-dynamic-delivery.md`
11. `docs/reviews/2026-09-17-v0.4.7-b-computation-causality-delivery.md`
12. `docs/reviews/2026-09-18-v0.4.7-b-independent-review.md`
13. `docs/reviews/2026-09-19-v0.4.7-b-final-rereview.md`
14. `docs/reviews/2026-09-19-v0.4.7-c-final-rereview.md`

Then inspect current `main`, recent commits and repository status. Never reset to an older SHA because a handoff names a historical checkpoint.

## Current accepted production-code checkpoint

V0.4.7-C is accepted on exact production code:

```text
fbb64b9917da1f63362558355201ff7998384ba0
feat: prove backend API projection lineage
```

The D pre-implementation analysis described below was performed from docs-only `main` parent:

```text
f3ee356339ff718835801a3b52b1366e86ccf61f
docs: close V0.4.7-C and open D [skip ci]
```

No D production implementation was committed in that analysis session. Verify current `main` before work; do not reset to either historical SHA.

## Current milestone state

```text
V0.4.7-A          PASS / COMPLETE
V0.4.7-B          PASS / COMPLETE
V0.4.7-C          PASS / COMPLETE
V0.4.7-D          CURRENT / UNLOCKED
V0.4.7-E          LOCKED behind D
V0.5 Azure DevOps LOCKED
```

Do not start E or V0.5.

## Closed production baseline

Accepted V0.4.6 remains:

```text
c310e893762997f34562a6b3a62dbab2b05c0c93
fix: fail closed on Queryable authority
```

Final V0.4.6 review: `docs/reviews/2026-09-15-v0.4.6-independent-rereview-10.md`.

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

Keep these knowledge classes distinct:

```text
business conditions
value lineage / provenance
mutation / causality
```

Conservative authority downgrade must not erase independently proven lower-authority evidence. Unsupported inference fails closed.

## Accepted A and B

A accepted code:

```text
09a0c7f2a058adfd7ddb7b4ac2feb2d17580a429
feat: prove bounded reference dynamic lineage
```

A proves bounded stored scalar snapshot and reference/dynamic read-time lineage with exact target-project identity.

B accepted code:

```text
17fd30b3a4b8178208adabc12c40dee060bedb54
fix: fail closed after opaque terminal effects
```

B proves supported multi-input derivation, retained original origin, later override/mutation causality and last proven source before a supported direct return. Unsupported effects/control flow invalidate stronger current authority while retaining deterministic historical evidence.

## V0.4.7-C — PASS / COMPLETE

C answers:

> What exact backend value supplies this response field, and how did it travel through entity/domain state → DTO/projection → API output?

Accepted supported shape:

```text
scalar auto-property on a fresh backend entity/domain local
→ explicit semantic assignment to a scalar response/DTO auto-property
→ direct final API response object initializer
```

The focused positive proves a renamed property chain equivalent to:

```text
ProductEntity.Price
→ PriceResponse.DisplayPrice
→ API response
```

C requires exact project/assembly/type/member semantics and records source member, target member, projection and response locations. It reaches endpoint candidate → knowledge synthesis → PO-facing workflow Markdown.

### C fail-closed boundary

Permanent regressions cover:

- same-name members without mapping;
- same-name namespace collisions;
- same **full type/member name across assemblies** using a compile-valid `extern alias` fixture;
- custom source/target accessors;
- user-defined conversion;
- reference alias ambiguity;
- opaque invocation/effect;
- custom response constructor;
- unsupported statement/control-flow shapes;
- missing target-project semantic context.

The implementation is intentionally conservative. It does not claim a general alias/effect/path solver or arbitrary MVC wrapper/helper mapping analysis.

PokeTrade was not modified merely to fit C; explicit DTO/rename coverage is supplied by focused fixtures.

### Exact-main C gates

All passed on exact code SHA `fbb64b9917da1f63362558355201ff7998384ba0`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35416169067 — PASS
pinned Loren                                35416169091 — PASS
Loren-main canary                           35416169051 — PASS
pinned Jellyfin                             35416169039 — PASS
```

Core:

```text
Release build:       0 warnings / 0 errors
C# tests:            136 / 136 PASS
frontend tests:      13 / 13 PASS
tool pack/install:   PASS
WorkPlay:            PASS
PokeTrade:           PASS
```

Pinned Jellyfin:

```text
source commit:           1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
source build:            0 warnings / 0 errors
facts:                   43,365
relations:               195,316
workflow candidates:     386
product features:        116
canonical Markdown:      504
project-semantic facts:  43,365 / 43,365
portable bundle parity:  PASS
ZIP file-set parity:     PASS
ZIP byte parity:         PASS
raw .pkc leak:           none
src/ source leak:        none
artifact id:             10575663250
artifact digest:         sha256:857d33020332f4a68a69177b6809136ebe24b9ea86b5576efc8dbc69d8266347
artifact size:           9,162,486 bytes
```

Final review: `docs/reviews/2026-09-19-v0.4.7-c-final-rereview.md` — PASS / COMPLETE.

## V0.4.7-D — exact next implementation

D owns:

```text
API response/result
→ frontend API result/data
→ component/view-model assignment
→ rendered/displayed value
```

D must prove exact linkage and source locations. No normalized-name, property-name, route-label or casing similarity may create an edge by itself.

D also owns the supported joint visibility answer for the **same proven item/dataflow path**:

```text
backend selection / eligibility conditions
+
frontend visibility / filtering conditions
```

Keep authorities separate. Frontend evidence must not upgrade an observed-only backend condition into an authoritative business rule.

### D regression-first starting point

Begin with a focused compile-valid backend/frontend fixture that has an exact API field → frontend result → component/view-model assignment. Add a collision with the same/equivalent frontend property name that must stay disconnected. Preserve both backend and frontend source locations in deterministic evidence and PO-facing Markdown.

Then add joint-visibility composition only after the binding identity is proven.

### D pre-implementation analysis handoff

The current frontend pipeline already proves service/API-method/list flow at a coarser level:

```text
ui-api-call
→ ui-result-binding
→ ui-list-render
```

with service ownership qualified by TypeScript source module + class identity. Relevant seams are:

```text
src/Pkc.Frontend/AngularTypeScriptAstScanner.cs
src/Pkc.Frontend/AngularListBehaviorScanner.cs
src/Pkc.Frontend/AngularServiceIdentityEnricher.cs
src/Pkc.Knowledge/CrossStackFeatureCandidateBuilder.cs
src/Pkc.Knowledge/GroundedKnowledgeSynthesizer.cs
```

Do not weaken the existing service-identity regressions in `AngularServiceAwareListCorrelationRegressionTests`.

The first D property-level proof must **not** connect backend `DisplayPrice` to frontend `displayPrice` merely because the names normalize or differ only by casing. That would violate R7.9's explicit no-name-similarity rule.

Use an explicit wire-contract identity as the first supported boundary. A suitable focused fixture is equivalent to:

```text
backend:
  PriceResponse.DisplayPrice
  [JsonPropertyName("displayPrice")]
  returned by exact GET endpoint already carrying C terminal evidence

frontend service:
  exact GET call for that endpoint
  typed result whose property is `displayPrice`

component:
  exact subscribe/result receiver
  explicit assignment such as `this.displayPrice = result.displayPrice`

render:
  exact Angular interpolation/binding that reads `displayPrice`
```

The cross-layer join should require explicit backend wire-name proof plus exact frontend receiver/member dataflow. Route matching may scope endpoint ↔ API-call correlation, but route/text/property similarity must never be sufficient to create the field-level edge.

Recommended regression-first order for Codex:

```text
1. Add a focused D R7.9 test that currently fails because no property-level API→UI lineage fact exists.
2. Positive fixture: C-proven response field with explicit JsonPropertyName wire identity → typed frontend response member → component assignment → rendered interpolation.
3. Negative: same/equivalent frontend property name on an unrelated result/service remains disconnected.
4. Negative: backend member and frontend member differ only by casing/name normalization but have no explicit wire contract; fail closed.
5. Negative: ambiguous/unresolved service/result receiver or fallback-only frontend evidence; fail closed.
6. Assert C `api-projection` and `API response field` facts remain present even when D composition fails.
7. Preserve backend response location, frontend API-call location, result/member assignment location and render location in deterministic metadata/evidence.
8. Deliver the proven chain through candidate → knowledge synthesis → PO-facing Markdown.
9. Only after this R7.9 binding identity is green should R7.10 joint backend/frontend visibility be implemented.
```

Prefer a new narrow D enricher/fact kind over extending route/name heuristics. Keep production logic generic; do not modify PokeTrade merely to manufacture the first supported property-level shape.

No production code from this pre-implementation analysis was committed, and no local test claim is recorded for D. The next coding session must reproduce the red regression and validate the implementation locally before any production commit/push.

Required D gates remain:

```text
focused D regressions
→ related frontend/cross-stack tests
→ full C# + frontend suite
→ Release build
→ PokeTrade
→ pinned Loren
→ Loren-main canary
→ pinned Jellyfin + parity/no-leak
→ independent rereview
```

Do not use CI as the normal edit/test loop.

## What remains locked

E is final knowledge-only V0.4.7 product acceptance and portable transport/parity review. It starts only after D is explicitly PASS / COMPLETE.

V0.5 Azure DevOps remains locked until V0.4.7 and the V0.4.x PO-question-readiness exit gate pass.

## Version semantics

Roadmap, package and schema versions remain independent. Current package remains:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

Do not bump package/schema versions merely because C closed or D started.
