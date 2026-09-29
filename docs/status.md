# PKC Status

Last updated: 2026-09-30

## Repository state reviewed

Reviewed base before this reconciliation:

```text
832831cf7de0cc2f74c876ceecdc7592ba04e0a2
docs: harden RD8 repair-b execution plan [skip ci]
```

Exact semantic candidate assessed by the approved private product-value gate:

```text
8c4055decd56e4597b2a1aa03e24b5c1a70235d5
fix: fail closed on ambiguous mutation owners
```

## Current priority state

```text
V0.4.4 Loren knowledge readiness                    PASS / COMPLETE
V0.4.5 real-repository generalization               PASS / COMPLETE
V0.4.6 business logic reconstruction                PASS / COMPLETE
V0.4.7-A origin and copy timing                     PASS / COMPLETE
V0.4.7-B computation and later change               PASS / COMPLETE
V0.4.7-C backend to API                             PASS / COMPLETE
V0.4.7-D / R7.9 API to rendered value               PASS / COMPLETE
V0.4.7-D / R7.10 joint visibility                   OPEN / REVIEW PAUSED / NOT ACCEPTED
V0.4.7-E0 repository discovery + bounded run        ACTIVE
V0.4.7-E0 / RD1-RD7                                 PASS / COMPLETE
V0.4.7-E0 / RD8-A operability                       PASS
V0.4.7-E0 / RD8-C runtime/plugin real-target proof  PASS
V0.4.7-E0 / RD8-B targeted product value            NOT PASS / REPAIR (a) CLOSED; REPAIR (b) NEXT
V0.4.7-E1 remaining product-value repairs           LOCKED behind E0
V0.4.7-E2 final product acceptance / R7.14          LOCKED behind E1
continuous update/diff                              LOCKED
V0.5 Azure DevOps input evidence                    LOCKED
```

Do not advance E1, open repair (c), or resume formal R7.10/D acceptance until the active RD8/E0 checkpoint explicitly permits it.

## Accepted private evidence

Authoritative sanitized baseline:

`docs/reviews/2026-09-25-rd8-private-validation-result.md`

Accepted state:

```text
RD8-A operability                         PASS
RD8-C runtime/plugin real-target proof    PASS
RD8-B product value                       NOT PASS
```

Repair (a) is CLOSED by the 2026-09-28 private re-probe. Probe #1 remains PARTIAL for the separate authentication-event eligibility gap.

## Repair (b) — deferred command queue — NEXT

Primary spec:

`docs/plans/2026-09-28-rd8-repair-b-deferred-command-queue-spec.md`

Execution plan:

`docs/superpowers/plans/2026-09-28-rd8-deferred-command-queue-linking.md`

Queue-shape capture:

`docs/reviews/2026-09-28-rd8-private-queue-shape.md`

Evidence reconciliation:

`docs/reviews/2026-09-30-rd8-private-queue-shape-reconciliation.md`

### Accepted repair-(b) baseline is narrower than previously recorded

The accepted 2026-09-25 private validation already proves, at sanitized behavior level:

```text
producer
  -> inserts a persisted queue row
  -> the row names its handler by a type-name string
  -> a dispatcher later runs the handler
```

Therefore these are no longer open questions:

```text
queue persistence exists                    PROVEN
producer creates/inserts queue row          PROVEN
handler identity representation is string   PROVEN: type-name string
later dispatcher exists                     PROVEN
```

The implementation gate is still **NOT AUTHORIZED** because the shareable record does not prove:

```text
exact type-name expression/encoding family
exact persisted member carrying that string
proof dispatcher reads that same persisted member
resolver mechanism mapping string -> runtime type/service/handler
callable entry-point selection
exactly one callable handler
```

Do not assume `FullName`, `AssemblyQualifiedName`, `Type.GetType`, assembly scanning, DI resolution, dictionary/factory lookup, reflection invocation, or any other mechanism.

## Source-independent implementation review complete

Production path is confirmed:

```text
Pkc.Cli
  -> CrossStackFeatureCandidateBuilder
      -> FeatureCandidateBuilder
  -> JointVisibilityKnowledgeSynthesizer
      -> EvidenceAwareKnowledgeSynthesizer
          -> GroundedKnowledgeSynthesizer
  -> ProductFeatureBuilder
```

Required repair behavior is already planned:

- `CSharpProjectSemanticEnricher`: exact semantic deferred correlation; reuse existing exact-symbol/fail-closed foundations.
- `FeatureCandidateBuilder`: carry `deferred-dispatch`; retain `unresolved-deferred-dispatch`; bounded traversal only.
- wrapper-path regressions: prove normal `pkc run` candidate/synthesis path retains the edge.
- `GroundedKnowledgeSynthesizer`: distinguish deferred routing from synchronous calls and report unresolved deferred identity honestly.
- `ProductFeatureBuilder`: preserve clean source/target symbols while carrying a deferred marker; do not append human annotation into the target symbol before parsing/scoring.

Negative regression matrix:

```text
mismatched identity
ambiguous handler
missing consumer/dispatcher
unsupported resolver family
duplicate/non-unique callable fact
```

Current main has no `unresolved-deferred-dispatch`, so unresolved assertions are expected RED before implementation even where no false concrete edge is already emitted.

## Environment proof

The GitHub installation available to this web session was enumerated completely on 2026-09-30. It exposes only the connected `rua-den/*` repositories and does not expose the approved company target. The target locator was intentionally not retained in sanitized PKC docs.

Therefore the remaining queue-shape inspection cannot be performed from this session without inventing private-source details.

## Exact next action

Approved source-enabled/company environment only:

```text
1. Inspect only the remaining missing queue hops; do not redo the already accepted high-level queue finding.
2. Confirm exact type-name expression/encoding and queue-record member.
3. Prove dispatcher reads that same member.
4. Record the actual resolver mechanism family and callable entry-point selection.
5. Prove exactly one handler callable or remain unresolved.
6. Mark the queue-shape capture AUTHORIZED only if the full chain is deterministic.
7. Execute repair (b) regression-first, then focused/related/full local verification.
8. One coherent implementation commit/push, CI final verification, then re-probe #7 and #10.
```

Preserve until that evidence exists:

```text
RD8-B: NOT PASS
E0: ACTIVE / NOT COMPLETE
repair (c): UNOPENED
E1: LOCKED
R7.10/D: OPEN / REVIEW PAUSED
```
