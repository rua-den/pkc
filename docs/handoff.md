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

Then inspect current `main`, recent commits and repository status. Never reset to a historical SHA merely because this handoff names it.

## Current production checkpoint

The latest production implementation/test checkpoint validated in clean CI is:

```text
67624944da27ff1f1f5a1154018a255aae11d1fe
fix: avoid capturing mutation receiver out parameter
```

A docs-only `[skip ci]` checkpoint may sit on top of that SHA. Continue from current `main`; do not reset.

## Current milestone state

```text
V0.4.7-A                                PASS / COMPLETE
V0.4.7-B                                PASS / COMPLETE
V0.4.7-C                                PASS / COMPLETE
V0.4.7-D / R7.9                         PASS / COMPLETE
V0.4.7-D mutation-causality blocker     PASS / CLOSED
V0.4.7-D / R7.10                        CURRENT / UNLOCKED
V0.4.7-E                                LOCKED behind D
V0.5 Azure DevOps                       LOCKED
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

V0.4.7-D / R7.9
fc4bfa7042f59620b2c7ba1c4f6700cf3b02172a
test: target frontend casing collision

Post-R7.9 mutation-causality blocker repair
67624944da27ff1f1f5a1154018a255aae11d1fe
fix: avoid capturing mutation receiver out parameter
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

Conservative authority downgrade must not erase independently proven lower-authority evidence. Unsupported inference fails closed. Same/similar names are never sufficient proof.

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

C retains exact project/assembly/type/member identity plus source-member, target-member, projection and response locations. Later D failure must not remove independently proven C evidence.

## V0.4.7-D / R7.9 — PASS / COMPLETE

R7.9 answers:

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

Property-level composition requires accepted C API-response identity, explicit semantic wire identity, exact frontend typed response member, exact resolved service/API method, exact subscribe result-member use, exact state assignment, and exact authoritative render member. Route matching may scope endpoint correlation only; it cannot prove the property edge. No casing/name/convention fallback is allowed.

Focused regressions:

```text
tests/Pkc.CSharp.Tests/ApiResponseFrontendBindingRegressionTests.cs
tests/Pkc.CSharp.Tests/ApiResponseFrontendBindingIsolationRegressionTests.cs
```

Accepted R7.9 exact-SHA gates on `fc4bfa7042f59620b2c7ba1c4f6700cf3b02172a`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35588410936 — PASS
pinned Loren                                35588410941 — PASS
Loren-main canary                           35588410930 — PASS
pinned Jellyfin                             35588410939 — PASS
```

## Post-R7.9 mutation-causality release blocker — PASS / CLOSED

A pinned real-repository benchmark found a wrong PO-facing causality promotion in the modern Angular CRM:

```text
SaveChangesAsync()
→ SetTimestamps()
→ ChangeTracker.Entries().Where(State == Modified)
→ entry.Entity is User user / Contact contact
→ user.UpdatedAt / contact.UpdatedAt
```

Unrelated workflows such as Signup and Contact Store were incorrectly claiming both timestamp mutations merely because the helper was transitively reachable.

The repaired generic boundary is:

```text
runtime pattern-selected receiver
→ raw mutation evidence retained
→ metadata: mutationReceiverOrigin = runtime-pattern-variable
→ metadata: mutationCausalityBoundary = caller-object-unproven
→ transitive workflow promotion denied unless causality is proven
```

Ordinary local/domain-object transitive mutations and self-owned domain mutations remain eligible. This prevents over-filtering legitimate PokeTrade stock/status mutations.

Regression file:

```text
tests/Pkc.CSharp.Tests/TransitiveMutationCausalityRegressionTests.cs
```

It locks:

1. runtime pattern-selected receiver is retained raw but not promoted;
2. ordinary local/domain transitive mutation remains promoted;
3. self-owned domain mutation remains promoted.

Exact-SHA standard gates on `67624944da27ff1f1f5a1154018a255aae11d1fe`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35632901150 — PASS
pinned Loren                                35632901064 — PASS
Loren-main canary                           35632901146 — PASS
pinned Jellyfin + parity/provenance         35632901144 — PASS
```

Core verification:

```text
Release build:       0 warnings / 0 errors
C# tests:            145 / 145 PASS
frontend tests:      13 / 13 PASS
tool pack/install:   PASS
WorkPlay:            PASS
PokeTrade:           PASS, including product-knowledge verification
```

## Pinned real-repository blocker re-benchmark

The GitHub connector did not expose workflow dispatch, so the existing diagnostic matrix was rerun through a temporary evidence branch based exactly on the accepted implementation SHA:

```text
base implementation: 67624944da27ff1f1f5a1154018a255aae11d1fe
branch:              benchmark/p0-mutation-causality-67624944
wrapper commit:      ea727bea808d79526f786d6092810ec368338c20
benchmark run:       35633768883 — PASS, 3 / 3 jobs
```

The wrapper changes one workflow trigger line only. PKC source/test logic is byte-identical to `67624944...`.

Rerun summary:

```text
jin12-xyz/CRM
PKC exit 0 | 415 facts | 1580 relations | 25 knowledge files

hackersandwizards/agentic-engineering-training-angular
PKC exit 0 | 441 facts | 660 relations | 26 knowledge files

kesetovic/crm-system
PKC exit 0 | 488 facts | 2054 relations | 28 knowledge files
```

Agentic-angular artifact cross-check:

```text
raw user.UpdatedAt mutation:       retained
raw contact.UpdatedAt mutation:    retained
receiver origin:                   runtime-pattern-variable
causality boundary:                caller-object-unproven
candidate inclusion count:         0 for both timestamp mutations
PO-facing UpdatedAt false claims:  0
```

Affected workflows receive the transitive-mutation warning instead of fabricated state changes.

The benchmark still yields `0 / 3` R7.9 `rendered UI value` chains. Therefore R7.14 real-project positive yield remains **NOT YET PASS**. Do not broaden syntax support pre-demo merely to force a positive benchmark.

Full evidence: `docs/benchmarks/2026-09-21-real-repo-r7.9-benchmark.md`.

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

Authority rules:

- Reuse the exact R7.9 identity/dataflow as the composition key; do not correlate predicates by names alone.
- Backend business-condition authority and frontend visibility evidence remain distinct evidence classes.
- Frontend evidence must never upgrade an observed-only/lower-authority backend condition into an authoritative business rule.
- Unrelated backend/frontend predicates must remain disconnected.
- If joint composition fails, retain R7.9 lineage and all independently proven backend/frontend evidence.
- Production logic remains generic; do not modify PokeTrade merely to manufacture the fixture.

Regression-first next action:

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

Do not bump package/schema versions merely because R7.9 or the benchmark blocker repair passed.

## If another session takes over

Verify current `main` first. The latest clean-environment validated implementation SHA is:

```text
67624944da27ff1f1f5a1154018a255aae11d1fe
```

If a docs-only `[skip ci]` commit is on top, continue from current main. The mutation-causality release blocker is closed. Continue R7.10 regression-first only. E and V0.5 remain locked.
