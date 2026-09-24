# PKC Handoff

Last updated: 2026-09-24

This handoff defines the next safe execution order for coding/review sessions.

## Read first

1. root `CLAUDE.md`
2. `AGENTS.md`
3. `docs/status.md`
4. this handoff
5. `docs/milestones.md`
6. `docs/product-knowledge-contract.md`
7. `docs/v0.4.7-acceptance-plan.md`
8. `docs/plans/2026-09-24-demo-critical-sequential-execution-plan.md`
9. `docs/plans/2026-09-24-repository-discovery-scan-planning-plan.md`
10. `docs/reviews/2026-09-24-repository-discovery-scan-planning-self-review.md`
11. `docs/benchmarks/product-value-benchmark-protocol.md`
12. `docs/reviews/2026-09-23-v0.4.7-d-r7.10-rereview-17-request.md` only when explicitly assigned the independent-review role

Then inspect current `main`, recent commits, production code and relevant regressions. Never reset to an older SHA merely because this handoff names one.

## Current formal state

```text
A/B/C                    PASS / COMPLETE
R7.9                     PASS / COMPLETE
mutation-causality       PASS / CLOSED
R7.10                    REPAIRED / ALL GATES PASS / PENDING REREVIEW #17
V0.4.7-D                 PENDING INDEPENDENT REREVIEW #17
V0.4.7-E                 LOCKED behind D
E0 repository discovery PREPARED / FIRST E CHECKPOINT WHEN D PASSES
E1 semantic richness     LOCKED behind E0
E2 product acceptance    LOCKED behind E1
R7.14                    NOT PASS / REQUIRED FOR E2
V0.5                     LOCKED
```

Formal R7.10 review target:

```text
96205a9a643864facaf9642a3b390ddcdbed59d9
fix: tolerate duplicate Angular import aliases
```

The product-value/demo thread MUST NOT self-certify this gate.

## Current validated production code

Latest production-code checkpoint below docs-only planning commits:

```text
79b0f9fec80a2afb87f43ec7a559a5d54cb87863
fix: surface pkc run progress
```

Exact-SHA gates:

```text
CI / full tests / WorkPlay / PokeTrade  35898219948 PASS
Loren pinned                            35898219784 PASS
Loren-main canary                       35898219957 PASS
Jellyfin                                35898219943 PASS
```

The progress fix reports long-running phases to stderr while preserving stdout result/path behavior.

## Why the execution order changed

A private mixed legacy enterprise repository exposed that visible progress is insufficient: the current full-root `pkc run` still begins expensive semantic scanning before it has a bounded repository model. One manual run was observed around 9 GB RAM before useful completion.

For the demo, semantic correctness is irrelevant if the normal product path cannot practically produce `.pkc/workspace` on the target repository.

Therefore, once D passes, V0.4.7-E is intentionally ordered around run operability first.

## Mandatory sequential E order

Do not split these into parallel production branches.

```text
E0 — Repository Discovery + bounded run readiness

RD1 inventory + safe exclusion
→ RD2 application boundaries + ownership
→ RD3 vendor/custom frontend classification
→ RD4 runtime/plugin provenance
→ RD5 deterministic ScanPlan
→ RD6 scoped/bounded semantic execution
→ RD7 coverage + observability + plan-only inspection
→ RD8 private large-repo validation

E1 — Remaining product-value repairs

construction/default/computation
→ semantic integration side effects
→ feature-summary rule fidelity

E2 — Product acceptance

R7.14 positive real-project yield
→ Level-2 known-answer benchmark
→ portable workspace acceptance
```

Finish the active checkpoint and its focused/related verification before starting the next one.

## E0 technical principles

- Discovery MUST precede expensive semantic scans.
- The repository is an application/component graph, not a homogeneous root.
- Whole-repository discovery coverage is required even if semantic execution occurs in bounded waves.
- Third-party runtime assets remain visible without deep-scanning their internals.
- Locally modified vendor code receives narrow first-party carve-outs when deterministic evidence supports them.
- Tests are evidence, not production authority.
- Runtime/plugin edges require provenance; naming similarity is never sufficient.
- UNKNOWN is valid and must remain explicit.
- Generated/restorable areas may be auto-excluded only with strong evidence.
- Discovery itself must stay shallow/bounded; it must not become a second whole-source semantic scan.
- Scan decisions must be machine-readable, stable and inspectable.
- A plan-only/discovery inspection path should allow users to see what PKC intends to scan before expensive execution.
- Coverage must remain honest: omitted/unsupported required areas prevent a false READY claim.
- Private validation must sanitize all reported evidence.

Primary design packet:

`docs/plans/2026-09-24-repository-discovery-scan-planning-plan.md`

Ordering/acceptance override:

`docs/plans/2026-09-24-demo-critical-sequential-execution-plan.md`

## Existing product-value evidence

Already complete for bounded paths:

```text
Fix #1 direct interface→concrete DI
Fix #2 Angular URL + Output-event bridge
Fix #3 Agentic displayed-value lineage
```

Level-2 diagnostic checkpoint:

```text
Agentic Users Update     ~94.5%
Jin12 Contacts Update    100.0%
Kesetovic PackOrder       82.5%
selected-probe aggregate ~91.6%
backend PO/QC core       ~95.3%
```

These numbers are diagnostic, not acceptance thresholds.

## Fix #4 candidate — keep off main until E0 PASS

Validated candidate remains:

```text
branch  fix/product-value-construction-state
commit  35c8e5c5f856e15568aa963bb2d76268008c5570
        fix: prove observable constructed state
```

Validation already obtained:

```text
Release build          PASS
focused regressions    8 / 8 PASS
C# full suite          282 / 282 PASS
frontend full suite     23 / 23 PASS
real Kesetovic retry   35902984101 PASS
```

Do NOT merge this before E0. It adds another semantic pass to `CSharpEvidenceScanner`, while RD6 is expected to change semantic scan orchestration/scope. Resolve scan architecture first, then re-evaluate/adopt the candidate without weakening its authority rules.

## Workspace / privacy boundary

Preferred product flow:

```text
pkc run <TEAM_REPOSITORY_PATH>
cd <TEAM_REPOSITORY_PATH>/.pkc/workspace
```

- PRODUCT/TRACE: workspace only, no source fallback.
- Benchmark phase 1: workspace only; freeze answer before source inspection.
- Phase 2: minimum source cross-check only in approved environment.
- Private-repo research/benchmarks: aliases, counts, classifications, resource deltas and failure categories only; no proprietary source bodies/internal identifiers/config values.

## Git / CI discipline

For one logical checkpoint:

```text
inspect
→ focused regression
→ minimum generic implementation
→ focused verification
→ related verification
→ broader relevant verification
→ diff review
→ one coherent commit
→ one push
→ CI as final verification
```

Do not use GitHub Actions as the edit/test loop.

## Exact next action

Right now:

```text
independent rereview #17 of exact 96205a9a643864facaf9642a3b390ddcdbed59d9
```

If PASS:

```text
1. mark R7.10 + V0.4.7-D PASS / COMPLETE
2. unlock V0.4.7-E
3. start E0/RD1 only
4. continue RD1 → RD8 sequentially
5. require private large-repo `pkc run` usability/workspace proof
6. only then start E1
7. only after E1 start E2/R7.14/product acceptance
```

If FAIL:

```text
reproduce exact counterexample
→ minimum generic R7.10 repair
→ local verification
→ exact-SHA gates
→ independent rereview again
```

Do not start Repository Discovery production code while D is still pending. Do not start E1/Fix #4 while E0 is incomplete.
