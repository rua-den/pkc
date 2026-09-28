# PKC Status

Last updated: 2026-09-28

## Repository state reviewed

Latest completed repair-(b) planning/status checkpoint before this handoff update:

```text
d5d1be99878ae4298b986e55c2afc6bd16e8f23d
docs: align RD8 repair-b checkpoint [skip ci]
```

That checkpoint aligned `docs/milestones.md` and `docs/v0.4.7-acceptance-plan.md` with accepted RD8 evidence and added the private queue-shape capture template. It changed no production behavior.

RD8 integration commit on `main`:

```text
343ac18ee1b29d2c3d169dda80ec49f9fb371b18
Merge RD8 repair-a hardening and reprobe plan
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

The 2026-09-25 private validation identified deferred command-queue indirection as a shared material root cause for probe #7 scheduled updates and probe #10 work-log behavior. Producers persist a handler identity into a queue record; a later dispatcher runs the handler; current PKC does not link the producer to that handler.

## Repair (a) — CLOSED

Primary spec:

`docs/plans/2026-09-25-exclusive-branch-state-effects-spec.md`

The accepted repair chain fixed exclusive branch flattening plus related same-line/ownership defects. The 2026-09-28 workspace-only/source-known re-probe confirmed that mutually exclusive deactivation effects are represented as alternatives rather than one combined proven effect.

Source-known comparison remains PARTIAL overall because the workspace under-proved a separate CustomerWeb authentication-event eligibility rule. That remaining access-gate gap is not repair (b).

Authoritative re-probe:

`docs/reviews/2026-09-28-rd8-private-reprobe-phase1.md`

## 2026-09-28 candidate run

The exact semantic candidate built Release cleanly and completed `pkc run` successfully without `--resume`:

```text
candidate SHA                   8c4055decd56e4597b2a1aa03e24b5c1a70235d5
Release build                   PASS, 0 warnings / 0 errors
pkc run                         PASS, exit 0, 46m 29s
discovery / scan / link         54s / 43m 48s / 1m 32s
repository                      25,303 files / 17 apps / 45 owned libraries / 9 test projects
planned / executed semantic     16,497 / 16,460
not analyzable / unknown        6,990 / 0
facts / relations               255,229 / 4,629,193
workflow candidates             4,044
product features                658
AI workspace                    4,520 files
mapped-field-rule facts         455
applies-mapped-field-rule       4,298 (relation path active)
```

The target had no `.pkc` directory at start. Its disposable-copy status was not recorded. No target source or raw facts are stored in this repository.

## Repair (b) — deferred command queue — NEXT

Primary spec:

`docs/plans/2026-09-28-rd8-repair-b-deferred-command-queue-spec.md`

Implementation plan:

`docs/superpowers/plans/2026-09-28-rd8-deferred-command-queue-linking.md`

Private shape capture template:

`docs/reviews/2026-09-28-rd8-private-queue-shape.md`

The template is deliberately marked:

```text
TEMPLATE / NOT EVIDENCE
source-enabled inspection performed: NO
capture reviewed:                  NO
implementation authorized:         NO
```

Do not implement a guessed queue/resolver grammar while those values remain `NO`.

## Repo-local pre-implementation findings

Current-main review has already closed the source-independent pipeline questions:

1. `CSharpProjectSemanticEnricher` already uses assembly-qualified symbol/type keys and requires unique callable facts for existing dispatch authority. Repair (b) should reuse those exact-symbol/fail-closed foundations rather than introduce name matching.
2. `FeatureCandidateBuilder` currently transports `dispatches`, `dispatches-sole-implementation`, and `unresolved-dispatch`; without an explicit new branch, a `deferred-dispatch` edge would be dropped before synthesis.
3. `GroundedKnowledgeSynthesizer` currently renders only synchronous `invokes`, DI `dispatches`, and sole-implementation dispatch. Deferred routing needs distinct wording plus separate unresolved-deferred uncertainty.
4. `ProductFeatureBuilder` parses workflow flow from the literal `source → target` shape. Deferred wording must remain parseable so the producer-to-handler edge survives into product capability flow.
5. Downstream C# semantic/authority enrichers preserve unrelated relation kinds. `CSharpBusinessPredicateAuthorityFilter` only removes relations targeting rejected business-predicate facts; a deferred edge targets a handler callable. `CSharpSelectedApiPredicateAuthorityEnricher` also preserves the existing relation set while adding/upgrading predicate evidence.

Therefore the remaining unknown is the private target's exact producer identity / persisted member / dispatcher resolver / unique-handler syntax-mechanism chain, not the PKC insertion path.

## Exact next action

Approved source-enabled/company environment only:

```text
1. Inspect the narrow producer -> queued identity -> dispatcher -> handler path read-only.
2. Fill `docs/reviews/2026-09-28-rd8-private-queue-shape.md` with sanitized mechanism families only.
3. Mark implementation AUTHORIZED only if the full identity chain and unique handler are proven.
4. Then execute repair (b) regression-first:
   raw deferred relation
   -> candidate transport
   -> deferred workflow wording
   -> product-flow retention
   -> fail-closed negative cases
5. Run focused, related and full local gates before one implementation commit/push.
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
