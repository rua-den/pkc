# PKC Milestones

Last updated: 2026-09-24

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
| E0 / RD1 | Can PKC inventory repository shape and safely exclude only proven generated/restorable areas? | **PASS / COMPLETE** |
| E0 / RD2 | Can PKC model application boundaries and shared ownership from deterministic evidence? | **PASS / COMPLETE** |
| E0 / RD3 | Can PKC separate first-party, third-party runtime, generated/minified/bundled and modified-vendor frontend code? | **PASS / COMPLETE** |
| E0 / RD4 | Can PKC prove runtime/plugin edges only from deterministic loader + identity + delivery provenance? | **PASS / COMPLETE — deterministic/synthetic gate** |
| E0 / RD5 | Can PKC produce a stable, inspectable, privacy-safe ScanPlan? | **PASS / COMPLETE** |
| E0 / RD6 | Can semantic scanners execute only planned scopes without changing accepted authority? | **PASS / COMPLETE** |
| E0 / RD7 | Can users inspect detection, planned scope and honest coverage? | **PASS / COMPLETE** |
| E0 / RD8 | Does current-main run practically on the approved large repository with honest coverage, useful answers and correct runtime topology? | **ACTIVE / KNOWN RUNTIME-PLUGIN BLOCKER / NOT PASS** |
| E1 | Are remaining high-value product behaviors represented? | **LOCKED behind E0** |
| E2 | Can an AI answer agreed PO/QC questions from the portable pack alone on unchanged real repositories? | **LOCKED behind E1** |

## Priority override

The user explicitly authorized E0 before formal D acceptance because scan operability is a prerequisite for a credible demo.

This is not a D PASS. It is bounded infrastructure/operability prework under an open formal semantic gate.

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
-> RD8 current-main private validation             ACTIVE / BLOCKED
-> practical workspace + targeted answer sign-off
-> E0 PASS
```

## RD8 supporting resource evidence

Historical approved private exercise:

```text
RD7 baseline:   41.5 min, ~18.6 GB peak, OOM at artifact write
patched run:    29 min, ~7.5 GB peak, exit 0, workspace generated
```

The resource repair later merged in `612fa998`; later semantic/output repairs merged in `48ce2f10` and `fa30e3eb`. Therefore the old successful run is supporting evidence, not a current-main RD8 PASS.

## RD8 Gate A — fresh current-main run

Required in the approved source-enabled environment:

```text
pkc discover <approved-private-copy>
pkc run <approved-private-copy>     # fresh acceptance measurement; no --resume
```

Record sanitized elapsed time, peak memory, coverage, output counts, artifact status and workspace path/count. Confirm summaries and coverage are internally plausible.

## RD8 Gate B — targeted product value

Run 2-3 Level-1 known-answer probes from the generated workspace, workspace-only first, then approved source cross-check. Include the quantity-adjustment probe plus materially different questions when useful.

A green compiler exit alone is insufficient.

## RD8 Gate C — corrected target applicability

A recovered sanitized reconnaissance proves that the intended private repository class contains runtime-loaded plugin modules:

```text
reflection-based load from host runtime output
+ no host project reference to the plugin
+ custom post-build copy into that runtime output
```

It also identifies solution/build-dependency evidence plus post-build copy targets as necessary provenance for the real topology.

Therefore the old `accepted N/A if the private target lacks the shape` branch is no longer available for this target.

Current Phase C state:

```text
applicability:                         YES
current PKC proof on intended target: MISSING / NOT PROVEN
exact loader/copy defect syntax:      SOURCE-ENABLED INSPECTION REQUIRED
RD8-C:                                OPEN / KNOWN BLOCKER
```

Do not guess the parser shape. First obtain a sanitized exact loader + delivery syntax pattern from the approved environment. Then, if current main misses it, use regression-first synthetic coverage and a minimum generic fail-closed repair.

Review:

`docs/reviews/2026-09-24-rd8-runtime-plugin-target-applicability.md`

## E0 completion

E0 is PASS only when RD8 closes all of:

```text
A: fresh practical current-main run
B: useful correctly-calibrated Level-1 answers
C: deterministic proof of the target's applicable runtime-plugin topology
```

Coverage must remain honest and the normal product path must produce the workspace practically.

## E1

Only after E0 PASS, re-evaluate remaining semantic richness including the parked candidate:

```text
branch  fix/product-value-construction-state
commit  35c8e5c5f856e15568aa963bb2d76268008c5570
```

Mediator dispatch, legacy-UI linkage and rule-language cleanup remain candidate gaps, but fresh benchmark evidence chooses the next actual repair.

## E2 / formal acceptance

Only after E1 PASS.

Formal R7.10/D independent acceptance must still complete before V0.4.7 closes. E2 also requires broader product acceptance including R7.14 positive unchanged-real-project yield and Level-2 workspace-only PO/QC validation.

## Exact next action

```text
source-enabled RD8-C shape inspection
-> sanitize actual reflection-loader + build/copy provenance pattern
-> regression-first generic repair only if current main misses it
-> rerun affected discovery evidence

then fresh RD8-A without --resume
-> RD8-B Level-1 probes
-> RD8 PASS only if A+B+C close
```

## Version semantics

```text
roadmap: V0.4.7-E0 active; RD1-RD7 complete; RD8 blocked; R7.10/D still open
package: RuaDen.Pkc.Tool 0.4.3-preview.2
```