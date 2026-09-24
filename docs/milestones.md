# PKC Milestones

Last updated: 2026-09-24

PKC is a Product/System Knowledge Compiler. Acceptance requires deterministic portable product knowledge, honest uncertainty, and a normal product flow that can practically produce the workspace on the intended repository class.

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

## V0.4.7 — CURRENT PRIORITY

| Checkpoint | Acceptance question | Current state |
| --- | --- | --- |
| D / R7.10 | What backend + frontend conditions jointly control that exact rendered value? | **OPEN / REVIEW PAUSED / NOT ACCEPTED** |
| E0 | Can PKC discover, plan and boundedly scan a large mixed repository well enough to produce the AI workspace? | **ACTIVE AS USER-AUTHORIZED BOUNDED PREWORK** |
| E0 / RD1 | Can PKC inventory repository shape and safely exclude only proven generated/restorable areas before expensive semantic scanning? | **LOCAL PASS at `fd3428f` / push + CI pending** |
| E0 / RD2 | Can PKC model application boundaries and shared ownership from deterministic evidence without tests or similar names creating production edges? | **LOCAL PASS at `05eadb1` / push + CI pending** |
| E0 / RD3 | Can PKC separate first-party, third-party runtime, generated/minified/bundled, wrapper and locally modified vendor frontend code without name-only authority? | **LOCAL PASS at `ac3efd9` / push + CI pending** |
| E0 / RD4 | Can PKC prove runtime/plugin edges only from deterministic loader + build/copy + identity provenance? | **LOCAL PASS at `a37d936` / push + CI pending** |
| E0 / RD5 | Can PKC turn the repository graph into a stable, inspectable, privacy-safe ScanPlan? | **LOCAL PASS at `c440eea` / push + CI pending** |
| E0 / RD6 | Can semantic scanners execute only planned scopes/waves without changing accepted semantics inside scope? | **LOCAL PASS at `280450c` / push + CI pending** |
| E0 / RD7 | Can users inspect detection, planned scope and honest coverage before and after expensive execution? | **LOCAL PASS at `d0c1017` / push + CI pending** |
| E0 / RD8 | Does the normal product path run practically on the approved private large repository with honest coverage? | **ACTIVE — operator-run external gate** |
| E1 | Are remaining high-value product behaviors represented? | **LOCKED behind E0** |
| E2 | Can an AI answer agreed PO/QC questions from the portable pack alone on unchanged real repositories? | **LOCKED behind E1** |

## Priority override

The user explicitly authorized E0 before formal D acceptance because scan operability is a prerequisite for a credible demo.

This is not a D PASS. It is bounded infrastructure prework under an open formal semantic gate.

Authoritative decision:

`docs/plans/2026-09-24-demo-scan-priority-override.md`

## E0 runtime constraint

Repository compilation must be local/deterministic and require zero AI tokens.

E0 must not call Claude/OpenAI/Gemini or any hosted LLM, must not require API keys, and must not upload proprietary source to an LLM.

LLM reconnaissance is research/oracle evidence only.

## Demo-critical E0 sequence

Execute sequentially:

```text
RD1 inventory + safe exclusion
→ RD2 application boundaries + ownership
→ RD3 vendor/custom frontend classification
→ RD4 runtime/plugin provenance
→ RD5 deterministic ScanPlan
→ RD6 scoped/bounded semantic execution
→ RD7 coverage + observability + plan-only inspection
→ RD8 private large-repository validation
→ prove normal pkc run practically produces .pkc/workspace
```

Do not overlap production checkpoints.

## RD1 acceptance

RD1 must prove with deterministic regression fixtures:

- cheap repository inventory;
- discovery before expensive semantic scanning in the new run architecture;
- strongly proven generated/restorable areas may become `SAFE_AUTO_EXCLUDE`;
- names alone never exclude `legacy`, `vendor`, `plugins`, `themes`, `Scripts`, `Content`, `old`, `packages`, or similar directories;
- ambiguous source remains included or UNKNOWN;
- tests remain non-production evidence;
- deterministic output ordering.

Do not start RD2 until RD1 is explicitly PASS and documented.

## E0 completion

E0 is PASS only when RD1–RD8 complete sequentially and the approved private large mixed repository demonstrates materially improved bounded behavior and successful `.pkc/workspace` generation without source/config leakage.

The earlier roughly 9 GB observation is a baseline symptom, not a portable hard threshold.

## E1

Only after E0 PASS, re-evaluate remaining semantic richness including the parked construction/default/computation candidate:

```text
branch  fix/product-value-construction-state
commit  35c8e5c5f856e15568aa963bb2d76268008c5570
```

Do not merge it during E0.

## E2 / formal acceptance

Before V0.4.7 can close, formal R7.10/D independent acceptance must still be completed if it remains open, followed by E2 product acceptance requirements including R7.14 positive unchanged-real-project yield and workspace-only PO/QC validation.

## Sequential execution rule

```text
finish current RD checkpoint
→ verify locally
→ review diff
→ coherent commit/push
→ CI final verification
→ update status/handoff
→ explicitly move to next RD checkpoint
```

## Current exact action

```text
RD8 private large-repository validation — ACTIVE, operator-run external gate (RD1–RD7 local PASS; push + CI pending)
```

## Version semantics

```text
roadmap: V0.4.7-E0 bounded prework active; RD1–RD7 local PASS; RD8 operator gate; R7.10/D still open
package: RuaDen.Pkc.Tool 0.4.3-preview.2
```
