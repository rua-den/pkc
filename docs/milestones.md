# PKC Milestones

Last updated: 2026-09-28

PKC is a Product/System Knowledge Compiler. Acceptance requires deterministic portable product knowledge, honest uncertainty, and a normal product path that can practically produce a useful workspace on the intended repository class.

## Accepted checkpoints

```text
V0.4.4 Loren knowledge readiness          PASS / COMPLETE
V0.4.5 real-repository generalization     PASS / COMPLETE
V0.4.6 business logic reconstruction      PASS / COMPLETE
V0.4.7-A origin and copy timing           PASS / COMPLETE
V0.4.7-B computation and later change     PASS / COMPLETE
V0.4.7-C backend to API                   PASS / COMPLETE
V0.4.7-D / R7.9 API to rendered value     PASS / COMPLETE
mutation-causality repair                 PASS / CLOSED
```

## V0.4.7 — current priority

| Checkpoint | Acceptance question | Current state |
| --- | --- | --- |
| D / R7.10 | What backend + frontend conditions jointly control that exact rendered value? | **OPEN / REVIEW PAUSED / NOT ACCEPTED** |
| E0 | Can PKC discover, plan and boundedly scan a large mixed repository well enough to produce a useful AI workspace practically? | **ACTIVE AS USER-AUTHORIZED BOUNDED PREWORK** |
| E0 / RD1-RD7 | Inventory, ownership, vendor classification, deterministic runtime provenance, scan planning, bounded execution and observability | **PASS / COMPLETE** |
| E0 / RD8-A | Does the normal product path complete practically on the approved large repository? | **PASS** |
| E0 / RD8-C | Does PKC deterministically recover the target's applicable runtime/plugin topology? | **PASS** |
| E0 / RD8-B | Are selected Level-1 PO/QC answers useful and correctly calibrated? | **NOT PASS — repair (a) CLOSED; repair (b) NEXT** |
| E1 | Are remaining high-value product behaviors represented? | **LOCKED behind E0** |
| E2 | Can an AI answer agreed PO/QC questions from the portable pack alone on unchanged real repositories? | **LOCKED behind E1** |

Do not resume formal R7.10/D acceptance or advance E1 until E0 explicitly passes.

## Priority override

The user explicitly authorized E0 before formal D acceptance because scan operability is a prerequisite for a credible demo. This does not make D PASS.

Authoritative decision:

`docs/plans/2026-09-24-demo-scan-priority-override.md`

## E0 sequence

```text
RD1 inventory + safe exclusion                    PASS
-> RD2 application boundaries + ownership         PASS
-> RD3 vendor/custom frontend classification      PASS
-> RD4 runtime/plugin deterministic provenance    PASS
-> RD5 deterministic ScanPlan                     PASS
-> RD6 scoped/bounded semantic execution          PASS
-> RD7 coverage + observability                   PASS
-> RD8-A current semantic candidate operability   PASS
-> RD8-C real-target runtime/plugin proof          PASS
-> RD8-B targeted product value                    NOT PASS
   -> repair (a) exclusive branch calibration      CLOSED
   -> repair (b) deferred queue producer->handler  NEXT
   -> later repairs chosen from fresh evidence
-> practical workspace + targeted answer sign-off
-> E0 PASS
```

## RD8-A — operability PASS

Approved source-enabled/company evidence shows a fresh no-`--resume` run completed successfully on the intended large repository class with practical resource use and all expected artifacts written.

Latest sanitized 2026-09-28 candidate run:

```text
Release build                   PASS
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

This closes the practical-run blocker. It does not make RD8-B PASS.

## RD8-C — runtime/plugin target proof PASS

Approved private validation proved the intended repository contains the applicable runtime-loaded plugin topology and PKC represents it deterministically:

```text
runtime-plugin-load: 2 HIGH edges
production host:     1
unresolved runtime-* reasons: none
```

The old `KNOWN RUNTIME-PLUGIN BLOCKER` wording is obsolete. Keep the accepted deterministic authority boundary: loader + identity + delivery provenance; do not infer runtime/plugin edges from name or directory proximity.

Authoritative sanitized evidence:

`docs/reviews/2026-09-25-rd8-private-validation-result.md`

## RD8-B — targeted product value NOT PASS

Baseline probes identified shared semantic gaps:

```text
#7 scheduled updates / invoice period   FAIL
#1 lost date / CustomerWeb access       PARTIAL after repair (a) re-probe
#10 work log                            PARTIAL
```

Repair (a) is CLOSED. The 2026-09-28 re-probe confirmed mutually exclusive branch effects are no longer rendered as one combined proven effect. The remaining #1 miss is an authentication-event eligibility path and is separate from repair (b).

Repair (b) is the current task because deferred command-queue indirection materially affects #7 and #10. Producers persist a handler identity and a later dispatcher invokes the handler; PKC must link that route only when the exact identity chain and unique handler are proven.

Primary spec:

`docs/plans/2026-09-28-rd8-repair-b-deferred-command-queue-spec.md`

Implementation plan:

`docs/superpowers/plans/2026-09-28-rd8-deferred-command-queue-linking.md`

Current external gate before implementation:

```text
approved source-enabled inspection
-> sanitize exact producer identity expression family
-> prove persisted identity member
-> prove dispatcher reads/resolves the same identity
-> prove unique callable handler selection
-> record behavior-only shape
-> red synthetic regression
-> minimum generic fail-closed repair
```

Do not guess queue syntax or resolver conventions. Repair (c), recurring background jobs as workflow triggers, remains unopened and separate.

## E0 completion

E0 is PASS only when RD1-RD7 remain accepted, RD8-A and RD8-C remain accepted, RD8-B selected product probes become useful and correctly calibrated, coverage stays honest, and the normal product path practically produces the portable workspace.

A green build or compiler exit alone is insufficient.

## E1

Only after E0 PASS, re-evaluate remaining semantic richness including the parked candidate:

```text
branch  fix/product-value-construction-state
commit  35c8e5c5f856e15568aa963bb2d76268008c5570
```

Mediator dispatch, legacy-UI linkage, entity-construction richness and rule-language cleanup remain candidate gaps, but fresh benchmark evidence chooses the next actual repair.

## E2 / formal acceptance

Only after E1 PASS.

Formal R7.10/D independent acceptance must still complete before V0.4.7 closes. E2 also requires broader product acceptance including R7.14 positive unchanged-real-project yield and Level-2 workspace-only PO/QC validation.

## Exact next action

```text
source-enabled repair-(b) shape inspection
-> fill sanitized queue-shape record
-> execute deferred command-queue linking plan regression-first
-> focused + related + full local verification
-> one coherent implementation commit/push
-> re-probe affected Level-1 questions
-> keep RD8-B NOT PASS until evidence closes it
```

## Version semantics

```text
roadmap: V0.4.7-E0 active; RD1-RD7, RD8-A and RD8-C pass; RD8-B repair (b) next; R7.10/D still open
package: RuaDen.Pkc.Tool 0.4.3-preview.2
```
