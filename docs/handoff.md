# PKC Handoff

Use this file when continuing PKC in another coding or review thread.

Last updated: 2026-09-21

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

Then inspect current `main`, recent commits and repository status. Never reset to a historical SHA merely because this handoff names it.

## Current production checkpoint

The exact R7.9 code/test checkpoint that passed all required gates is:

```text
fc4bfa7042f59620b2c7ba1c4f6700cf3b02172a
test: target frontend casing collision
```

A docs-only `[skip ci]` handoff commit may sit on top of that SHA. Verify current `main` first and do not reset.

## Current milestone state

```text
V0.4.7-A          PASS / COMPLETE
V0.4.7-B          PASS / COMPLETE
V0.4.7-C          PASS / COMPLETE
V0.4.7-D / R7.9   PASS / COMPLETE
V0.4.7-D / R7.10  CURRENT / UNLOCKED
V0.4.7-E          LOCKED behind D
V0.5 Azure DevOps LOCKED
```

Do not start E or V0.5.

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
```

C final review: `docs/reviews/2026-09-19-v0.4.7-c-final-rereview.md`.

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

## V0.4.7-C — accepted boundary

C proves the bounded target-project-semantic chain:

```text
scalar backend entity/domain auto-property
→ explicit semantic DTO/response property assignment
→ direct final API response object initializer
```

Equivalent accepted example:

```text
ProductEntity.Price
→ PriceResponse.DisplayPrice
→ API response
```

C retains exact project/assembly/type/member identity plus source-member, target-member, projection and response locations. C emits separate `api-projection` and `API response field` facts. Later D failure must not remove them.

## V0.4.7-D / R7.9 — PASS / COMPLETE

R7.9 now answers:

> What exact frontend state/display does this proven API response field feed?

First supported proof shape:

```text
C-proven backend response property
→ explicit [JsonPropertyName("displayPrice")]
→ API response field
→ typed frontend HTTP result member `displayPrice`
→ exact resolved frontend service/API method
→ exact subscribe result receiver/member
→ component/view-model assignment
→ exact authoritative Angular interpolation
→ rendered UI value
```

### Proof boundary

Property-level composition requires all of the following:

- accepted C API-response terminal identity;
- semantic `System.Text.Json.Serialization.JsonPropertyNameAttribute` with exactly one explicit string wire name;
- exact frontend typed HTTP response contract/member;
- exact resolved service identity and API method;
- exact result-member access on the subscribe result parameter;
- exact component/view-model assignment target;
- exact authoritative rendered member in the same component.

Route identity may scope endpoint ↔ API-call correlation only. It is never sufficient to create the property-level edge.

No normalized-name, casing similarity, textual similarity, route-label matching, DTO naming convention or camelCase/PascalCase guessing may create R7.9 lineage.

### Main implementation seams

```text
src/Pkc.CSharp/CSharpJsonWireContractEnricher.cs
src/Pkc.Frontend/AngularResponseBindingScanner.cs
src/Pkc.Frontend/AngularRenderedMemberAuthorityFilter.cs
src/Pkc.Knowledge/ApiFrontendBindingCandidateEnricher.cs
```

Pipeline integration also touches:

```text
src/Pkc.CSharp/CSharpEvidenceScanner.cs
src/Pkc.Frontend/AngularFrontendAdapter.cs
src/Pkc.Knowledge/CrossStackFeatureCandidateBuilder.cs
```

Focused regressions:

```text
tests/Pkc.CSharp.Tests/ApiResponseFrontendBindingRegressionTests.cs
tests/Pkc.CSharp.Tests/ApiResponseFrontendBindingIsolationRegressionTests.cs
```

### R7.9 regressions locked

Positive proof retains deterministic evidence locations for:

- backend projection;
- API response boundary;
- explicit JsonPropertyName declaration;
- frontend typed API call;
- frontend response-contract member;
- subscribe result-member use and component assignment;
- rendered UI member.

Fail-closed coverage includes:

1. `DisplayPrice` / `displayPrice` same-name/casing collision without explicit wire proof;
2. unrelated resolved service/result exposing the identical member name;
3. unresolved or ambiguous service receiver;
4. wrong/unresolved subscribe result receiver/RHS;
5. untyped/fallback-only HTTP evidence;
6. missing `JsonPropertyName` with no naming-convention fallback;
7. non-authoritative render evidence;
8. C evidence retention when D composition fails.

PokeTrade was not modified merely to fit the fixture.

### Important implementation repairs discovered during validation

The original candidate was not accepted blindly. Clean-environment verification exposed and fixed these generic defects:

```text
1. Roslyn declared-property symbol type was inferred as ISymbol.
   Fix: preserve IPropertySymbol before wire-attribute inspection.

2. C targetMemberLocation is based on the property identifier/source symbol line,
   while an attributed PropertyDeclarationSyntax starts at the attribute line.
   Fix: match the property identifier token line, preserving C's location contract.

3. Accepted C API-response terminal scopes itself with scopeFactId,
   not endpointFactId.
   Fix: compose R7.9 from the accepted C scope identity.

4. One collision regression incorrectly rejected backend `PriceResponse.DisplayPrice`
   anywhere in the full lineage string.
   Fix: assert specifically that frontend `result.DisplayPrice` is absent.
```

None of these repairs added naming heuristics or weakened the proof boundary.

### Exact-SHA R7.9 gates

All passed on exact SHA `fc4bfa7042f59620b2c7ba1c4f6700cf3b02172a`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35588410936 — PASS
pinned Loren                                35588410941 — PASS
Loren-main canary                           35588410930 — PASS
pinned Jellyfin                             35588410939 — PASS
```

Core verification:

```text
Release build:       0 warnings / 0 errors
C# tests:            142 / 142 PASS
frontend tests:      13 / 13 PASS
tool pack/install:   PASS
WorkPlay:            PASS
PokeTrade:           PASS
```

Pinned Jellyfin completed normal source build, PKC compile, portable handoff parity/provenance verification and artifact upload successfully.

The current agent environment did not provide local `.NET` execution. Do not rewrite history to claim local .NET validation; exact-SHA clean-environment gates are the recorded validation evidence.

## V0.4.7-D / R7.10 — exact next implementation

D remains open because joint visibility has not been implemented yet.

R7.10 must answer, for the **same already-proven R7.9 item/dataflow path**:

> What backend selection/eligibility conditions and frontend visibility/filter conditions jointly determine whether this item/value is visible?

Target composition:

```text
R7.9-proven API → frontend item/value identity
+
backend business-condition evidence
+
frontend visibility/filter evidence
→ one PO-facing visibility explanation
```

### R7.10 authority rules

- Reuse the exact R7.9 identity/dataflow as the composition key; do not correlate predicates by names alone.
- Backend business-condition authority and frontend visibility evidence remain distinct evidence classes.
- Frontend evidence must never upgrade an observed-only/lower-authority backend condition into an authoritative business rule.
- Unrelated backend/frontend predicates must remain disconnected.
- If joint composition fails, retain R7.9 lineage and all independently proven backend/frontend evidence.
- Production logic remains generic; do not modify PokeTrade merely to manufacture the fixture.

### R7.10 regression-first next action

```text
1. Inspect existing backend predicate and frontend visibility/filter facts already available in the R7.9 candidate path.
2. Add a focused positive fixture that attaches both sides to the same proven R7.9 item/dataflow identity.
3. Add unrelated-predicate, ambiguous-item and authority-downgrade negatives.
4. Assert candidate → synthesis → PO-facing Markdown delivery.
5. Run focused tests, related tests and full relevant verification.
6. Review the complete diff before one coherent implementation push.
7. Run exact-SHA PokeTrade, Loren pinned, Loren-main and Jellyfin/parity gates.
8. Obtain the required D review before marking all of V0.4.7-D complete.
```

Do not start E until R7.10 and the remaining D gates/review pass.

## Version semantics

Roadmap, package and schema versions remain independent:

```text
roadmap:             V0.4.7-D / R7.10 current
tool/package:        RuaDen.Pkc.Tool 0.4.3-preview.2
C# raw schema:       0.4.4-csharp-raw
merged facts schema: 0.4.4
cross-stack schema:  0.4.6
frontend schema:     0.4.3-frontend
```

Do not bump package/schema versions merely because R7.9 passed.

## If another session takes over

Verify current `main` and working state first. The last validated production/test SHA is:

```text
fc4bfa7042f59620b2c7ba1c4f6700cf3b02172a
```

If a docs-only `[skip ci]` commit is on top, continue from current main; do not reset. R7.9 is closed. Continue R7.10 only. E and V0.5 remain locked.
