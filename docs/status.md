# PKC Status

Last updated: 2026-09-24

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                   PASS / COMPLETE
V0.4.5 real-repository generalization              PASS / COMPLETE
V0.4.6 business logic reconstruction               PASS / COMPLETE
V0.4.7-A origin and copy timing                    PASS / COMPLETE
V0.4.7-B computation and later change              PASS / COMPLETE
V0.4.7-C backend to API                            PASS / COMPLETE
V0.4.7-D / R7.9 API to rendered value              PASS / COMPLETE
V0.4.7-D / R7.10 joint visibility                  REPAIRED / ALL GATES PASS / PENDING REREVIEW #17
V0.4.7-D overall                                   PENDING INDEPENDENT REREVIEW #17
V0.4.7-E                                           LOCKED behind D
E0 repository discovery + bounded run readiness    PREPARED / FIRST E CHECKPOINT WHEN D PASSES
E1 remaining product-value repairs                 LOCKED behind E0
E2 real-project product acceptance / R7.14         LOCKED behind E1
continuous update/diff                             LOCKED
V0.5 Azure DevOps input evidence                   LOCKED
```

Formal D acceptance remains an independent-review gate. Product-value or demo-operability work must not self-certify R7.10/D.

## Current main

This status belongs to the docs-only roadmap checkpoint created after:

```text
ac328a197cbe3da755fbd051476947c67275e1e4
docs: plan repository discovery from real-project research [skip ci]
```

Latest validated production-code checkpoint below the docs-only planning commits:

```text
79b0f9fec80a2afb87f43ec7a559a5d54cb87863
fix: surface pkc run progress
```

Exact-SHA validation for that production code is green:

```text
CI / full tests / WorkPlay / PokeTrade  35898219948 — PASS
Loren pinned                            35898219784 — PASS
Loren-main canary                       35898219957 — PASS
Jellyfin parity                         35898219943 — PASS
```

## Demo-critical roadmap decision

A private mixed legacy enterprise repository exposed a first-order product blocker: an unbounded `pkc run` entered expensive semantic scanning before understanding repository shape and was observed consuming roughly 9 GB RAM before producing useful output.

The product cannot credibly demo PO/QC knowledge if the normal run path cannot practically produce a workspace on the target repository.

Therefore, once D is independently accepted, V0.4.7-E must execute sequentially in this order:

```text
E0 Repository Discovery + bounded run readiness
   RD1 inventory + safe exclusion
→  RD2 application boundaries + ownership
→  RD3 vendor/custom frontend classification
→  RD4 runtime/plugin provenance
→  RD5 deterministic ScanPlan
→  RD6 scoped/bounded semantic execution
→  RD7 coverage + observability + plan-only inspection
→  RD8 private large-repo validation

E1 Remaining product-value repairs
→ construction/default/computation candidate review/adoption
→ semantic side-effect synthesis
→ feature-summary rule fidelity

E2 Product acceptance
→ R7.14 positive unchanged-real-project yield
→ Level-2 known-answer benchmark
→ portable workspace acceptance
```

No parallel production implementation is authorized across these checkpoints. Finish and locally verify one checkpoint before starting the next.

Technical design packet:

- `docs/plans/2026-09-24-repository-discovery-scan-planning-plan.md`
- `docs/reviews/2026-09-24-repository-discovery-scan-planning-self-review.md`
- `docs/plans/2026-09-24-demo-critical-sequential-execution-plan.md`

## E0 acceptance intent

E0 is not accepted merely because discovery JSON exists. It must prove that PKC understands repository shape before expensive semantic work and that the planned execution is practically usable.

At minimum:

- discovery runs before deep C#/frontend semantic scanning;
- only strongly proven generated/restorable areas are auto-excluded;
- first-party, shared, vendor runtime, modified vendor, tests, infrastructure and UNKNOWN remain distinguishable;
- application ownership and runtime/plugin edges require deterministic provenance;
- `scan-plan` is stable, inspectable and privacy-safe;
- semantic scanners receive only planned scopes/application waves;
- UNKNOWN is explicit and cannot silently disappear;
- workspace coverage cannot claim READY when required areas are omitted/unsupported;
- a plan-only/discovery inspection path exists before expensive semantic execution;
- private large-repo validation records elapsed time and peak memory versus the observed unbounded baseline without leaking proprietary source;
- `pkc run` completes far enough to produce the intended AI workspace on the approved demo repository before E1/E2 are treated as demo-ready.

Do not invent a universal RAM threshold from the single ~9 GB observation. Prove structural boundedness first, then measure controlled before/after resource deltas.

## Product-value state already achieved

```text
Fix #1 interface→concrete direct DI       PASS / COMPLETE for bounded path
Fix #2 frontend URL + Output bridge       PASS / COMPLETE for bounded path
Fix #3 displayed-value lineage            PASS / COMPLETE for bounded Agentic email path
Fix #4 construction/default state         VALIDATED FUTURE CANDIDATE / NOT PRODUCTION
```

Fix #4 remains off `main` and must not be merged before D is accepted and E0 is complete. RD6 changes scanner orchestration, so merging the extra semantic pass first would create avoidable architecture/performance churn.

Latest Level-2 diagnostic checkpoint remains useful evidence, not acceptance:

```text
Agentic Users Update     ~94.5%
Jin12 Contacts Update    100.0%
Kesetovic PackOrder       82.5%
selected-probe aggregate ~91.6%
backend PO/QC core       ~95.3%
```

## Workspace / privacy boundary

Preferred product UX remains:

```text
pkc run <repository-path>
cd <repository-path>/.pkc/workspace
```

- PRODUCT/TRACE: workspace only; no source fallback.
- Benchmark phase 1: workspace only and frozen before source inspection.
- Phase 2/source cross-check: minimum necessary source only in the approved environment.
- Private-repository reports contain only sanitized counts, aliases, classifications, deltas and failure categories; no proprietary source bodies, internal names, endpoints or config values.

## Benchmark cadence

```text
Level 0 — deterministic tests/build/smoke
Level 1 — targeted semantic/product-value workflow
Level 2 — full Agentic + Jin12 + Kesetovic only at checkpoint/release/demo
```

Do not repeatedly run Level 2 during RD1–RD7. Use focused regressions and related suites locally; use CI as final verification for coherent checkpoints.

## Exact next action

Current formal action is still:

```text
independent rereview #17 of exact 96205a9a643864facaf9642a3b390ddcdbed59d9
```

Then:

```text
PASS
→ mark R7.10 + V0.4.7-D PASS / COMPLETE
→ unlock V0.4.7-E
→ start E0/RD1 only
→ proceed RD1 → RD2 → RD3 → RD4 → RD5 → RD6 → RD7 → RD8 sequentially
→ only after E0 PASS start E1

FAIL
→ regression-first minimum generic R7.10 repair
→ rerun exact-SHA gates
→ independent rereview again
```

The demo-critical ordering is deliberate: first make `pkc run` capable of producing trustworthy output on the target repository, then spend effort improving the remaining semantic richness of that output.
