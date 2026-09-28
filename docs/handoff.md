# PKC Handoff

Last updated: 2026-09-28

## Read first

1. root `CLAUDE.md`
2. `AGENTS.md`
3. `docs/status.md`
4. this handoff
5. `docs/milestones.md`
6. `docs/product-knowledge-contract.md`
7. `docs/v0.4.7-acceptance-plan.md`
8. `docs/reviews/2026-09-25-rd8-private-validation-result.md`
9. `docs/reviews/2026-09-28-rd8-private-reprobe-phase1.md`
10. `docs/plans/2026-09-28-rd8-repair-b-deferred-command-queue-spec.md`
11. `docs/superpowers/plans/2026-09-28-rd8-deferred-command-queue-linking.md`
12. `docs/reviews/2026-09-28-rd8-private-queue-shape.md`

Then verify current `main`, recent commits, production code and relevant regressions. Never reset to an older SHA merely because an older review names one.

## Current repository checkpoint

Latest completed docs checkpoint before this handoff update:

```text
d5d1be99878ae4298b986e55c2afc6bd16e8f23d
docs: align RD8 repair-b checkpoint [skip ci]
```

It aligned stale milestone/acceptance state and added the queue-shape capture template. Production behavior was unchanged.

Previous repair-(b) planning correction:

```text
65a4449751a915261e8e90edd7062c6232174885
docs: preserve deferred dispatch through candidate graph [skip ci]
```

RD8 integration commit:

```text
343ac18ee1b29d2c3d169dda80ec49f9fb371b18
Merge RD8 repair-a hardening and reprobe plan
```

Exact semantic candidate used by the private product-value gate:

```text
8c4055decd56e4597b2a1aa03e24b5c1a70235d5
fix: fail closed on ambiguous mutation owners
```

## Current checkpoint

```text
V0.4.7-D / R7.10                    OPEN / REVIEW PAUSED / NOT ACCEPTED
V0.4.7-E0                           ACTIVE
RD1-RD7                             PASS / COMPLETE
RD8-A operability                   PASS
RD8-C runtime/plugin target proof   PASS
RD8-B product value                 NOT PASS / REPAIR (a) CLOSED; REPAIR (b) NEXT
repair (c) recurring jobs           UNOPENED / KEEP SEPARATE
E1 remaining semantic repairs       LOCKED behind E0
E2 final acceptance / R7.14         LOCKED behind E1
```

## Accepted evidence

Authoritative private validation:

`docs/reviews/2026-09-25-rd8-private-validation-result.md`

Accepted high-level result:

```text
RD8-A  PASS
RD8-C  PASS
RD8-B  NOT PASS
```

Deferred command-queue indirection is a shared material cause of missing behavior in:

```text
#7 scheduled updates / invoice period
#10 work log
```

The source-known baseline establishes only the sanitized behavior shape: producers persist a handler identity in a queued record and a dispatcher later executes the handler. The exact source syntax/mechanism chain was intentionally not stored and must be re-confirmed in the approved source-enabled environment.

## Repair (a)

Repair (a) is CLOSED. The 2026-09-28 re-probe confirmed exclusive state-effect branches are represented as alternatives instead of a false combined effect.

Probe #1 remains PARTIAL for a separate reason: the workspace under-proved a CustomerWeb authentication-event eligibility rule. Source establishes access while both current-customer and active flags remain on, with access ending when either flag turns off; LostDate itself is not the cutoff rule.

Do not reopen repair (a) while working repair (b).

## Repair (b) — current work

Spec:

`docs/plans/2026-09-28-rd8-repair-b-deferred-command-queue-spec.md`

Implementation plan:

`docs/superpowers/plans/2026-09-28-rd8-deferred-command-queue-linking.md`

Private shape capture:

`docs/reviews/2026-09-28-rd8-private-queue-shape.md`

The capture file currently says:

```text
Status: TEMPLATE / NOT EVIDENCE
source-enabled inspection performed: NO
capture reviewed:                  NO
implementation authorized:         NO
```

That is an intentional implementation gate. Do not create a regression based on guessed reflection/DI/queue APIs.

## Source-independent production review already complete

### `CSharpProjectSemanticEnricher`

Current dispatch authority already provides the right generic foundation:

- callable facts are indexed by exact compiler symbols;
- symbol keys include assembly identity plus compiler display identity;
- direct DI resolution requires exactly one proven registration and exactly one callable fact;
- sole-implementation resolution requires exactly one concrete implementation and exactly one callable fact;
- unsupported own-interface calls remain unresolved instead of being guessed.

Repair (b) should extend this semantic relation pass once the target shape is known. Do not create a second fuzzy/name-based linker.

### Downstream scanner pipeline

`CSharpEvidenceScanner` runs project-semantic enrichment before value-lineage, causality, API projection, wire-contract and predicate-authority enrichers.

Review of the final authority stages found no generic relation-kind whitelist that would discard a handler-directed deferred edge:

- `CSharpBusinessPredicateAuthorityFilter` only removes/rewrites relations whose target is a rejected/observed business-predicate fact;
- `CSharpSelectedApiPredicateAuthorityEnricher` starts from the existing relation set and preserves it while adding/upgrading selected predicate evidence.

A `deferred-dispatch` relation targeting a callable handler therefore does not need a separate late-stage preservation hook under current code.

### `FeatureCandidateBuilder`

Current traversal explicitly retains:

```text
unresolved-dispatch                       non-traversing
dispatches                                traversing target fact
dispatches-sole-implementation            traversing target fact
```

It does not know the proposed deferred relation kinds. Repair (b) must add:

```text
unresolved-deferred-dispatch              retained, non-traversing
deferred-dispatch                         retained, traversing unique handler fact
```

This was the missing plan hop found in the web review. Without it, semantic enrichment could succeed while product knowledge silently loses the edge.

### `GroundedKnowledgeSynthesizer`

`BuildFlow` currently renders only `invokes`, `dispatches`, and `dispatches-sole-implementation`. `BuildUnresolvedDispatchUnknowns` only handles synchronous/interface unresolved dispatch.

Repair (b) must render deferred routing distinctly and add a separate unresolved-deferred unknown. The wording must not claim runtime execution, schedule timing, delivery guarantees or retries.

### `ProductFeatureBuilder`

`BuildProductFlow` parses workflow strings using the literal separator:

```text
source → target
```

Deferred flow rendering must preserve a parseable source/target edge so the route survives into product capability flow. Regression coverage must assert this; scanner-only coverage is insufficient.

## Regression shape once authorized

Create:

`tests/Pkc.CSharp.Tests/DeferredCommandQueueDispatchRegressionTests.cs`

Use existing temp-project patterns from:

```text
CSharpProjectSemanticEnricherTests.cs
DirectDiDispatchAuthorityScopeRegressionTests.cs
DirectDiDispatchCrossProjectCollisionRegressionTests.cs
FeatureCandidateBuilderTests.cs
ProductFeatureCapabilityFlowTests.cs
```

Required layers in the positive regression:

```text
1. raw FactDocument contains exactly one deferred-dispatch producer -> handler relation
2. FeatureCandidateBuilder retains the relation and handler fact
3. GroundedKnowledgeSynthesizer labels the route deferred/queued, not synchronous
4. ProductFeatureBuilder retains a usable producer -> handler capability-flow edge
```

Required fail-closed negatives:

```text
mismatched persisted identity
missing consumer/dispatcher
ambiguous handler identity
unsupported resolver shape
multiple callable facts for the resolved handler
```

When producer-side queued identity is proven but the destination cannot be proven, retain `unresolved-deferred-dispatch`; otherwise emit no speculative relation.

## Verification after implementation

Follow progressive local verification:

```text
dotnet test tests/Pkc.CSharp.Tests/Pkc.CSharp.Tests.csproj --filter FullyQualifiedName~DeferredCommandQueueDispatchRegressionTests
-> related CSharpProjectSemanticEnricher / DirectDiDispatch / FeatureCandidateBuilder / product-flow tests
-> dotnet test PKC.sln
-> dotnet build PKC.sln --configuration Release
-> complete diff review
-> one coherent implementation commit/push
-> CI final verification
-> re-probe #7 and #10
```

Do not use CI as the edit/test loop.

## Branch cleanup state

Keep:

```text
main
fix/product-value-construction-state
```

The second branch remains the explicitly parked E1 candidate and must not be deleted while E1 is locked.

Historical `benchmark/*`, old `codex/*`, `docs/*`, `notes/*`, `scratch/*`, `sol/*`, `web/*`, and `work/*` branches remain cleanup candidates. This web environment still cannot delete them: the connected GitHub actions have no delete-ref operation; local shell cannot resolve `github.com`; a zero-object `update_ref` attempt was rejected with HTTP 422 and changed nothing. Do not force-move refs to simulate deletion.

## Exact next action

Approved source-enabled/company environment only:

```text
1. Read the narrow private queue path read-only.
2. Fill the sanitized capture template with:
   producer identity expression family
   persisted member proof
   dispatcher read proof
   resolver mechanism family
   unique callable handler proof
3. Mark AUTHORIZED only if every hop is deterministic.
4. Execute the already-reviewed regression-first repair-(b) plan on current main.
5. Keep repair (c), E1 and R7.10/D locked.
```

Terminal state for the current web environment: the repo-local design/pipeline/test-surface review is complete; production implementation requires the source-enabled gate above.
