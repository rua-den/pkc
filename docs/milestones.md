# PKC Milestones

Last updated: 2026-09-24

PKC is a Product/System Knowledge Compiler. Acceptance requires deterministic portable product knowledge, honest uncertainty, and a normal product path that can practically produce the workspace on the intended repository class.

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
| E0 / RD1 | Can PKC inventory repository shape and safely exclude only proven generated/restorable areas before expensive semantic scanning? | **PASS / COMPLETE** |
| E0 / RD2 | Can PKC model application boundaries and shared ownership from deterministic evidence without tests or similar names creating production edges? | **PASS / COMPLETE** |
| E0 / RD3 | Can PKC separate first-party, third-party runtime, generated/minified/bundled, wrapper and locally modified vendor frontend code without name-only authority? | **PASS / COMPLETE** |
| E0 / RD4 | Can PKC prove runtime/plugin edges only from deterministic loader + build/copy + identity provenance? | **PASS / COMPLETE — deterministic/synthetic gate** |
| E0 / RD5 | Can PKC turn the repository graph into a stable, inspectable, privacy-safe ScanPlan? | **PASS / COMPLETE** |
| E0 / RD6 | Can semantic scanners execute only planned scopes without changing accepted semantics inside selected scope? | **PASS / COMPLETE** |
| E0 / RD7 | Can users inspect detection, planned scope and honest coverage before and after expensive execution? | **PASS / COMPLETE** |
| E0 / RD8 | Does the normal current-main product path run practically on the approved private large repository with honest coverage and useful targeted answers? | **ACTIVE / PARTIAL EVIDENCE / NOT PASS** |
| E1 | Are remaining high-value product behaviors represented? | **LOCKED behind E0** |
| E2 | Can an AI answer agreed PO/QC questions from the portable pack alone on unchanged real repositories? | **LOCKED behind E1** |

## Repository reconciliation checkpoint

Before this documentation update, remote `main` was:

```text
9ef914f57c558e3b2963b9ba4dd187a6ecd6603f
Add question in demo
```

The latest production-code commit beneath it was `fa30e3eb`.

RD1-RD7 are now recorded as complete because their implementation commits and evidence are present on current `main`, and the current descendant head completed all four observed GitHub Actions workflows successfully.

The earlier `LOCAL PASS / PUSH + CI PENDING` labels are obsolete.

Review record:

`docs/reviews/2026-09-24-main-state-reconciliation.md`

## Priority override

The user explicitly authorized E0 before formal D acceptance because scan operability is a prerequisite for a credible demo.

This is not a D PASS. It is bounded infrastructure prework under an open formal semantic gate.

Authoritative decision:

`docs/plans/2026-09-24-demo-scan-priority-override.md`

## E0 runtime constraint

Repository compilation must be local/deterministic and require zero AI tokens.

E0 must not call Claude/OpenAI/Gemini or another hosted LLM, require API keys, or upload proprietary source to an LLM.

LLM reconnaissance is research/oracle evidence only.

## E0 sequence

```text
RD1 inventory + safe exclusion                    PASS
→ RD2 application boundaries + ownership          PASS
→ RD3 vendor/custom frontend classification       PASS
→ RD4 runtime/plugin provenance                   PASS
→ RD5 deterministic ScanPlan                      PASS
→ RD6 scoped/bounded semantic execution           PASS
→ RD7 coverage + observability                    PASS
→ RD8 current-main private validation             ACTIVE
→ practical workspace + targeted answer sign-off
→ E0 PASS
```

Production checkpoints remain sequential.

## RD8 current evidence

The first private exercise proved the original resource failure and a strong improvement after the resource repair:

```text
RD7 baseline:   41.5 min, ~18.6 GB peak, OOM at artifact write
patched run:    29 min, ~7.5 GB peak, exit 0, workspace generated
```

The patched resource work later merged in `612fa998`. Additional semantic and workspace-summary fixes then merged in `48ce2f10` and `fa30e3eb`.

Therefore the previous successful run is supporting evidence, not a current-main RD8 PASS.

## RD8 exit criteria

RD8 may close only after:

1. a fresh full `pkc run` from current `main` completes on the approved private repository without `--resume` for the acceptance measurement;
2. sanitized elapsed time, peak memory, coverage and final workspace path/count are recorded;
3. `RUN_SUMMARY.md`, `run-summary.json`, coverage and workspace output are plausible;
4. 2-3 targeted Level-1 known-answer probes are scored from workspace-only answer → approved source cross-check;
5. the runtime/plugin real-repository applicability gap receives an explicit accepted disposition (`N/A` only when absence is proven/accepted, otherwise another approved real corpus is required).

A successful compiler exit alone is insufficient.

There is no universal numeric memory threshold; the run must complete practically on the approved demo environment and materially improve the original failure mode.

## E0 completion

E0 is PASS only when RD8 closes and the normal product path demonstrates both:

```text
operability
+ honest coverage
+ useful correctly-uncertain targeted product answers
```

## E1

Only after E0 PASS, re-evaluate remaining semantic richness including the parked construction/default/computation candidate:

```text
branch  fix/product-value-construction-state
commit  35c8e5c5f856e15568aa963bb2d76268008c5570
```

Do not merge it during RD8.

Current observed high-ROI candidates include mediator dispatch, legacy-UI linkage and remaining rule-language cleanup, but the fresh RD8 benchmark decides the next actual repair. Do not implement from the candidate list speculatively.

## E2 / formal acceptance

Only after E1 PASS.

Before V0.4.7 can close, formal R7.10/D independent acceptance must still be completed if it remains open, followed by E2 product acceptance including R7.14 positive unchanged-real-project yield and broader workspace-only PO/QC validation.

## Current exact action

```text
RD8 current-main private validation
→ fresh discover + full run without --resume
→ sanitized resource/coverage/workspace evidence
→ 2-3 Level-1 known-answer probes
→ explicit runtime/plugin applicability disposition
→ RD8 PASS only if all required evidence closes
```

## Version semantics

```text
roadmap: V0.4.7-E0 active; RD1-RD7 complete; RD8 active; R7.10/D still open
package: RuaDen.Pkc.Tool 0.4.3-preview.2
```
