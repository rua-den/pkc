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

Reviewed `main` before this docs correction:

```text
7cbf640fab7a410bc71494e63cee8fcc10f9874b
docs: hand off RD8 repair-b source gate [skip ci]
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

## Repair (a)

Repair (a) is CLOSED. The 2026-09-28 private re-probe confirmed exclusive state-effect branches are alternatives rather than a false combined effect.

Probe #1 remains PARTIAL for a separate authentication-event eligibility gap. Do not reopen repair (a) while working repair (b).

## Repair (b) — current work

Spec:

`docs/plans/2026-09-28-rd8-repair-b-deferred-command-queue-spec.md`

Corrected implementation plan:

`docs/superpowers/plans/2026-09-28-rd8-deferred-command-queue-linking.md`

Private shape capture:

`docs/reviews/2026-09-28-rd8-private-queue-shape.md`

The capture file remains deliberately blocked:

```text
Status: TEMPLATE / NOT EVIDENCE
source-enabled inspection performed: NO
capture reviewed:                  NO
implementation authorized:         NO
```

Do not create a source-shape regression from guessed framework conventions.

## Production path verified

The normal CLI path is now explicitly traced:

```text
Pkc.Cli Program
  -> CrossStackFeatureCandidateBuilder.Build(facts)
      -> FeatureCandidateBuilder.Build(document)
  -> ValidationConsistencyCandidateEnricher
  -> JointVisibilityCandidateEnricher
  -> JointVisibilityKnowledgeSynthesizer
      -> EvidenceAwareKnowledgeSynthesizer
          -> GroundedKnowledgeSynthesizer
  -> ProductFeatureBuilder
```

Consequences:

1. `FeatureCandidateBuilder` is the correct place to transport/traverse `deferred-dispatch` and retain `unresolved-deferred-dispatch`.
2. `GroundedKnowledgeSynthesizer` is the correct inner renderer for the deferred flow/unknown.
3. No wrapper production patch is currently needed solely for the new relation because the wrappers delegate/preserve the inner candidate/knowledge; nevertheless tests must exercise the wrapper path used by `pkc run`.

## Candidate transport contract

Current `FeatureCandidateBuilder` retains:

```text
unresolved-dispatch                       non-traversing
dispatches                                traversing target fact
dispatches-sole-implementation            traversing target fact
```

Repair (b) must add:

```text
unresolved-deferred-dispatch              retained, non-traversing
deferred-dispatch                         retained, traversing unique handler fact
```

The new deferred traversal must respect the existing `MaxCallDepth` policy. Do not change existing `invokes`/DI traversal semantics as collateral work.

## Corrected regression semantics

Create after source-shape authorization:

`tests/Pkc.CSharp.Tests/DeferredCommandQueueDispatchRegressionTests.cs`

Positive regression:

```text
compile-valid invented fixture matching only the authorized mechanism family
-> current main RED because deferred-dispatch is absent
-> implementation turns exact producer -> handler relation green
```

Negative regressions:

```text
mismatched identity
ambiguous handler
missing consumer/dispatcher
unsupported resolver family
duplicate/non-unique callable fact
```

Important red/green rule:

- before implementation, assertions that no guessed concrete edge exists may already be green;
- however, when producer identity is grounded, the spec also requires `unresolved-deferred-dispatch` for an unproven destination;
- current main has no unresolved-deferred relation, so those unresolved assertions are expected RED before implementation.

Do not declare Task 3 green merely because current code emits nothing.

## Flow-rendering/parser contract

`GroundedKnowledgeSynthesizer.BuildFlow` currently uses the Unicode separator:

```text
source → target
```

`ProductFeatureBuilder.ParseFlow` splits at that separator and treats everything to the right as the target symbol. It then calls `MethodName`, `SymbolOwner`, component-boundary and graph-distance logic on that value.

Therefore this is **not safe**:

```text
source → target (deferred queue)
```

because `(deferred queue)` becomes part of the target symbol.

Repair (b) must introduce one narrowly recognized deferred marker contract:

```text
workflow flow: clean source + clean target + explicit deferred marker
product parser: remove marker before symbol scoring
product render: preserve/re-render deferred marker
```

The final workflow and product capability flow must visibly distinguish queued/deferred routing from synchronous calls while keeping source/target symbols clean. Do not opportunistically rewrite the existing DI flow format.

## Required verification layers after authorization

The positive fixture must prove all of these:

```text
1. CSharpEvidenceScanner / semantic output
   -> exactly one deferred-dispatch producer -> handler relation

2. FeatureCandidateBuilder
   -> relation retained
   -> handler fact retained
   -> traversal bounded

3. CrossStackFeatureCandidateBuilder
   -> same relation/handler survive normal CLI candidate enrichment

4. GroundedKnowledgeSynthesizer
   -> route visibly deferred
   -> no runtime execution claim

5. JointVisibilityKnowledgeSynthesizer
   -> deferred flow/unknown survive normal CLI synthesis wrappers

6. ProductFeatureBuilder
   -> source/target remain symbol-clean
   -> deferred label preserved
   -> capability edge retained
```

Fail-closed fixtures must never create a guessed handler edge.

## Verification sequence

After the source shape is authorized:

```text
dotnet test tests/Pkc.CSharp.Tests/Pkc.CSharp.Tests.csproj --filter FullyQualifiedName~DeferredCommandQueueDispatchRegressionTests
-> related project-semantic/direct-DI/candidate/wrapper/product-flow tests
-> dotnet test PKC.sln
-> dotnet build PKC.sln --configuration Release
-> review complete diff
-> one coherent implementation commit/push
-> CI final verification
-> re-probe #7 and #10 workspace-only first, then source-known comparison
```

CI is not the edit/test loop.

## Source-enabled gate

The remaining implementation blocker is genuinely external to this web session. Prior context intentionally did not retain the private repository locator/name. No approved private checkout is available here.

The approved source-enabled/company environment must fill only sanitized mechanism evidence for:

```text
producer identity expression family
persisted identity member
proof dispatcher reads same identity
resolver mechanism family
exact unique callable-handler selection
```

If any hop is ambiguous, keep implementation `NOT AUTHORIZED` and record the unproven hop rather than weakening authority.

## Exact next action

```text
approved private source inspection
-> fill queue-shape template
-> authorize only with complete deterministic chain
-> execute corrected repair-(b) regression plan
-> local full verification
-> one implementation commit/push
-> re-probe #7 and #10
```

Preserve until then:

```text
RD8-B: NOT PASS
E0: ACTIVE / NOT COMPLETE
repair (c): UNOPENED
E1: LOCKED
R7.10/D: OPEN / REVIEW PAUSED
```
