# PKC Status

Last updated: 2026-09-28

## Repository state reviewed

Current reviewed `main` before this docs correction:

```text
7cbf640fab7a410bc71494e63cee8fcc10f9874b
docs: hand off RD8 repair-b source gate [skip ci]
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

Deferred command-queue indirection remains a shared material root cause for probe #7 scheduled updates and probe #10 work-log behavior.

Repair (a) is CLOSED by the 2026-09-28 private re-probe. Probe #1 remains PARTIAL only because of the separate authentication-event eligibility gap.

## 2026-09-28 candidate run

```text
candidate SHA                   8c4055decd56e4597b2a1aa03e24b5c1a70235d5
Release build                   PASS, 0 warnings / 0 errors
pkc run                         PASS, exit 0, 46m 29s
repository                      25,303 files / 17 apps / 45 owned libraries / 9 test projects
planned / executed semantic     16,497 / 16,460
facts / relations               255,229 / 4,629,193
workflow candidates             4,044
product features                658
AI workspace                    4,520 files
mapped-field-rule facts         455
applies-mapped-field-rule       4,298
```

No target source or raw facts are stored in this repository.

## Repair (b) — deferred command queue — NEXT

Primary spec:

`docs/plans/2026-09-28-rd8-repair-b-deferred-command-queue-spec.md`

Corrected implementation plan:

`docs/superpowers/plans/2026-09-28-rd8-deferred-command-queue-linking.md`

Private shape capture template:

`docs/reviews/2026-09-28-rd8-private-queue-shape.md`

The template remains:

```text
TEMPLATE / NOT EVIDENCE
source-enabled inspection performed: NO
capture reviewed:                  NO
implementation authorized:         NO
```

## 2026-09-28 source-independent plan rereview

A second production-path review found three important corrections that are now incorporated into the repair-(b) implementation plan.

### 1. Normal `pkc run` wrapper path is confirmed

The CLI uses:

```text
CrossStackFeatureCandidateBuilder
  -> FeatureCandidateBuilder

JointVisibilityKnowledgeSynthesizer
  -> EvidenceAwareKnowledgeSynthesizer
  -> GroundedKnowledgeSynthesizer

ProductFeatureBuilder
```

Therefore the originally selected inner classes are on the real production path, but regression coverage must exercise the wrappers too. No wrapper production change is currently required merely to expose the new relation.

### 2. Negative red/green semantics were corrected

The earlier plan incorrectly implied all fail-closed negative tests should already pass on current main. That is only true for the absence of a guessed concrete edge.

For a grounded producer identity with missing, mismatched, unsupported or ambiguous destination evidence, the spec requires future `unresolved-deferred-dispatch` evidence. Current main has no such relation kind, so those unresolved assertions must be **RED before implementation**.

Required negative coverage now includes:

```text
mismatched identity
ambiguous handler
missing consumer/dispatcher
unsupported resolver family
duplicate/non-unique callable fact
```

### 3. Product-flow parser needs explicit deferred-marker support

`GroundedKnowledgeSynthesizer.BuildFlow` uses `source → target` for ordinary invocation flow. `ProductFeatureBuilder.ParseFlow` treats everything after that Unicode separator as the target symbol and then runs method/owner/component scoring on it.

Therefore appending a human label directly to the target, for example `target (deferred queue)`, would corrupt symbol identity during product-flow ranking.

The corrected plan requires a narrowly recognized deferred marker contract in `ProductFeatureBuilder`: strip the marker before symbol scoring, preserve the clean source/target edge, then re-render the deferred label in product capability flow. Existing plain and DI flow behavior must remain unchanged.

### 4. Deferred candidate traversal must be bounded

The new `deferred-dispatch` traversal must respect the existing candidate call-depth policy. Do not alter existing synchronous/DI traversal semantics while adding the new edge.

## Repo-local implementation boundary

All source-independent design, wrapper tracing, pipeline preservation review, test-surface review, and parser-contract review are complete.

Production implementation remains intentionally blocked until the approved source-enabled environment proves the exact private queue chain:

```text
producer identity expression family
-> persisted identity member
-> dispatcher reads same identity
-> supported resolver mechanism
-> exactly one callable handler
```

Do not create a guessed scanner fixture before that capture is authorized.

## Exact next action

Approved source-enabled/company environment only:

```text
1. Fill `docs/reviews/2026-09-28-rd8-private-queue-shape.md` with sanitized structural proof.
2. Mark implementation AUTHORIZED only if every hop is deterministic.
3. Execute the corrected repair-(b) plan regression-first.
4. Verify both inner classes and normal `pkc run` wrappers.
5. Run full local tests/build, review the full diff, then one coherent implementation commit/push.
6. Re-probe affected Level-1 questions #7 and #10.
```

Preserve until that evidence exists:

```text
RD8-B: NOT PASS
E0: ACTIVE / NOT COMPLETE
repair (c): UNOPENED
E1: LOCKED
R7.10/D: OPEN / REVIEW PAUSED
```
