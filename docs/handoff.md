# PKC Handoff

Last updated: 2026-09-30

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
10. `docs/reviews/2026-09-30-rd8-private-queue-shape-reconciliation.md`
11. `docs/reviews/2026-09-28-rd8-private-queue-shape.md`
12. `docs/plans/2026-09-28-rd8-repair-b-deferred-command-queue-spec.md`
13. `docs/superpowers/plans/2026-09-28-rd8-deferred-command-queue-linking.md`

Then verify current `main`, recent commits, production code and relevant regressions. Never reset to an older SHA merely because an older review names one.

## Current repository checkpoint

Reviewed base before this reconciliation:

```text
832831cf7de0cc2f74c876ceecdc7592ba04e0a2
docs: harden RD8 repair-b execution plan [skip ci]
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

## Repair (b) — accepted evidence reconciliation

The previous handoff overstated how much of the queue shape remained unknown. The accepted 2026-09-25 private validation already records this sanitized source-known behavior:

```text
PRODUCER
  -> inserts a queue row
  -> queue row names its handler by a type-name string
  -> DISPATCHER later runs that handler
```

Carry these forward as accepted evidence:

```text
queue persistence exists:             PROVEN
producer inserts queue row:           PROVEN
handler identity representation:      type-name string — PROVEN
later dispatcher exists:              PROVEN
```

Do not waste another private-source pass rediscovering those four facts.

The remaining source gate is narrower:

```text
exact type-name expression / encoding
-> exact persisted member
-> dispatcher reads same member
-> resolver mechanism
-> callable handler entry point
-> exactly one callable fact
```

Examples such as `FullName`, `AssemblyQualifiedName`, `Type.GetType`, assembly scanning, DI resolution, dictionary/factory lookup or reflection invocation are not evidence until actually observed.

Authoritative reconciliation:

`docs/reviews/2026-09-30-rd8-private-queue-shape-reconciliation.md`

Updated capture:

`docs/reviews/2026-09-28-rd8-private-queue-shape.md`

Current authorization remains:

```text
PARTIAL SANITIZED EVIDENCE
IMPLEMENTATION NOT AUTHORIZED
```

## Production path verified

Normal CLI path:

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

1. `CSharpProjectSemanticEnricher` is the intended semantic correlation surface once resolver shape is authorized.
2. `FeatureCandidateBuilder` must transport/traverse `deferred-dispatch` and retain `unresolved-deferred-dispatch`.
3. `CrossStackFeatureCandidateBuilder` needs wrapper-path regression coverage even though no production change is currently expected there.
4. `GroundedKnowledgeSynthesizer` must render deferred routing separately from synchronous calls.
5. `JointVisibilityKnowledgeSynthesizer` wrapper-path tests must prove deferred flow/unknowns survive normal synthesis.
6. `ProductFeatureBuilder` requires a narrow deferred-marker parser contract so target symbols remain clean during ranking.

## Regression contract once authorized

Create:

`tests/Pkc.CSharp.Tests/DeferredCommandQueueDispatchRegressionTests.cs`

Positive layers:

```text
CSharpEvidenceScanner / semantic output
-> FeatureCandidateBuilder
-> CrossStackFeatureCandidateBuilder
-> GroundedKnowledgeSynthesizer
-> JointVisibilityKnowledgeSynthesizer
-> ProductFeatureBuilder
```

Negative matrix:

```text
mismatched persisted identity
missing consumer/dispatcher
unsupported resolver family
ambiguous handler identity
duplicate/non-unique callable fact
```

Red/green rule:

- no false concrete edge may already be green on current main;
- grounded producer identity with unproven destination must eventually emit `unresolved-deferred-dispatch`;
- current main does not implement that relation, so unresolved assertions must be RED before the fix.

Deferred traversal must respect existing `MaxCallDepth` and must not broaden ordinary `invokes`/DI dispatch authority.

## Flow parser contract

Do not render a deferred relation as plain:

```text
source → target (deferred queue)
```

and then feed it unchanged into current `ProductFeatureBuilder.ParseFlow`, because everything after ` → ` is currently treated as the target symbol.

Repair (b) must use one narrowly recognized deferred marker contract:

```text
workflow flow: clean source + clean target + explicit deferred marker
product parser: remove marker before symbol scoring
product output: preserve/re-render deferred marker
```

Do not opportunistically rewrite unrelated DI flow formatting.

## Environment proof

On 2026-09-30 the installed GitHub repository connection was enumerated to exhaustion. It exposes only connected `rua-den/*` repositories and no approved company target. The private target locator/name is intentionally absent from sanitized PKC records.

This web session therefore cannot inspect the missing resolver/member/callable hops without guessing, which is forbidden by the repair spec.

## Branch cleanup state

Keep:

```text
main
fix/product-value-construction-state
```

The latter is the explicitly parked E1 candidate. Historical branches remain cleanup candidates, but the current connector still has no delete-ref action and shell GitHub networking is unavailable. Do not force-move refs to simulate deletion.

## Exact next action

Approved source-enabled/company environment only:

```text
1. Read only the narrow queue path needed for the remaining hops.
2. Confirm exact type-name expression/encoding and the queue-record member.
3. Prove dispatcher reads that same member.
4. Record the actual resolver mechanism family.
5. Prove the handler entry point and exact unique callable selection.
6. Update the queue-shape capture and mark AUTHORIZED only if the full chain is deterministic.
7. Execute the corrected repair-(b) plan regression-first.
8. Run focused -> related -> full local tests/build, review diff, then one implementation commit/push.
9. CI final verification, then re-probe #7 and #10.
```

Preserve until then:

```text
RD8-B: NOT PASS
E0: ACTIVE / NOT COMPLETE
repair (c): UNOPENED
E1: LOCKED
R7.10/D: OPEN / REVIEW PAUSED
```
