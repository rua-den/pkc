# PKC Status

Last updated: 2026-09-23

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                   PASS / COMPLETE
V0.4.5 real-repository generalization              PASS / COMPLETE
V0.4.6 business logic reconstruction               PASS / COMPLETE
V0.4.7-A origin and copy timing                    PASS / COMPLETE
V0.4.7-B computation and later change              PASS / COMPLETE
V0.4.7-C backend to API                            PASS / COMPLETE
V0.4.7-D API to UI / R7.9 binding                  PASS / COMPLETE
V0.4.7-D mutation-causality blocker                PASS / CLOSED
V0.4.7-D API to UI / R7.10 joint visibility        REPAIRED / ALL GATES PASS / PENDING REREVIEW #17
V0.4.7-D overall                                   PENDING INDEPENDENT REREVIEW #17
V0.4.7-E product acceptance                        LOCKED behind D
R7.14 real-project positive yield                  NOT PASS / REQUIRED FOR E
real-repo safety benchmark                         PASS
real-repo product-value benchmark                  NOT PASS / MATERIAL GAPS REMAIN
product-value Fix #1 interface→concrete DI         PASS / COMPLETE FOR BOUNDED DIRECT-DI PATH
product-value Fix #2 frontend URL + Output bridge  PASS / COMPLETE FOR BOUNDED PATHS
product-value Fix #3 displayed-value lineage       PASS / COMPLETE FOR BOUNDED AGENTIC EMAIL PATH
product-value Fix #4 construction/default state    VALIDATED FUTURE CANDIDATE / NOT PRODUCTION / E LOCKED
AI workspace preview spike                         IMPLEMENTED / VALIDATED PREVIEW / USER-AUTHORIZED
co-located workspace Git isolation                 IMPLEMENTED / VALIDATED
formal AI-workspace W acceptance                   NOT UNLOCKED / NOT COMPLETE
continuous update/diff                             LOCKED
V0.5 Azure DevOps input evidence                   LOCKED
```

Formal D acceptance remains a separate independent-review gate. Product-value work must not self-certify R7.10/D or unlock E.

## Current main production code checkpoint

Production code on `main` before this docs-only checkpoint:

```text
79b0f9fec80a2afb87f43ec7a559a5d54cb87863
fix: surface pkc run progress
```

The CLI now reports long-running phases to stderr while preserving result/path output on stdout. Exact-SHA validation for that code checkpoint is green:

```text
CI / full tests / WorkPlay / PokeTrade  35898219948 — PASS
Loren pinned                            35898219784 — PASS
Loren-main canary                       35898219957 — PASS
Jellyfin parity                         35898219943 — PASS
```

Runtime PokeTrade evidence confirms visible progress across C# scan, frontend scan, merge/link, synthesis and knowledge generation.

## Formal R7.10/D gate — unchanged

Independent-review target:

```text
96205a9a643864facaf9642a3b390ddcdbed59d9
fix: tolerate duplicate Angular import aliases
```

Required request:

`docs/reviews/2026-09-23-v0.4.7-d-r7.10-rereview-17-request.md`

The next formal action is independent rereview #17. This implementation/product-value session cannot mark that review PASS itself.

## Product-value repair state

### Fix #1 — bounded interface → concrete DI

PASS / COMPLETE for the direct, single-authority DI path. Jin12 targeted Level-1 recovered controller/interface → exact direct registration → concrete service behavior, including existence guard, seven contact mutations and persistence.

Report:

`docs/benchmarks/2026-09-23-fix1-jin12-di-product-value-benchmark.md`

### Fix #2 — bounded frontend URL + Output-event bridge

PASS / COMPLETE for the supported path:

```text
child UI action
→ exact @Output event/discriminator
→ parent handler/switch branch
→ exact injected service
→ normalized URL expression
→ API call
→ backend endpoint
```

Unsupported or ambiguous shapes remain fail-closed.

### Fix #3 — bounded Agentic displayed-value lineage

Targeted Agentic Level-1 is complete.

Pinned target:

```text
hackersandwizards/agentic-engineering-training-angular
22f2aab64617f4de7984370a5bd40e8c9535dbf5
```

Run:

```text
35899560821 — PASS
```

Bounded proven chain:

```text
MAT_DIALOG_DATA.data.email
→ EditUserDialogComponent.form.email
→ EditUserDialogComponent.displayed.email
```

Targeted score moved from Q3 50% / Q4 90% to Q3 100% / Q4 100% for this exact email displayed-value probe. Source cross-check found no mismatch. This does not solve unrelated Agentic behavior such as DbContext-managed `UpdatedAt`.

## Level-2 product-value checkpoint

Full Agentic + Jin12 + Kesetovic execution:

```text
wrapper  749e24406d050a2b738751e086f02320cd28cf86
run      35900111059 — PASS, 3 / 3
```

Selected-probe diagnostic scores after workspace-only freeze and minimum source cross-check:

```text
Agentic Users Update     ~94.5%
Jin12 Contacts Update    100.0%
Kesetovic PackOrder       82.5%
selected-probe aggregate ~91.6%
backend PO/QC core       ~95.3%
```

These are diagnostic measurements, not an acceptance threshold. The broad product-value gate remains NOT PASS because independent gaps still exist:

1. construction/default/computation state;
2. semantic integration side effects such as SignalR `OrderSignal`;
3. feature-summary preservation of grounded workflow rules;
4. positive unchanged-real-project R7.14 cross-layer yield.

Detailed evidence:

`docs/benchmarks/2026-09-23-product-value-level2-and-fix4-prework.md`

## Fix #4 prework — validated future candidate only

Formal E is still locked, so this candidate is intentionally NOT merged to `main`.

```text
branch  fix/product-value-construction-state
commit  35c8e5c5f856e15568aa963bb2d76268008c5570
        fix: prove observable constructed state
```

Bounded authority requires exact project-semantic local-object identity plus both a downstream whole-object invocation and a return-path whole-object invocation before object-initializer member assignments are promoted to state changes.

Validation:

```text
Release build          PASS, 0 warnings / 0 errors
focused regressions    8 / 8 PASS
C# full suite          282 / 282 PASS
frontend full suite     23 / 23 PASS
real Kesetovic retry   35902984101 — PASS
```

Real-repository generation now reports Kesetovic AddOrder construction behavior including `OrderStatus.NEW` and `OrderPrice * 0.05` bonus computation.

Do not merge this candidate until D is independently accepted and E is formally unlocked.

## Workspace / privacy boundary

Preferred product UX:

```text
pkc run <repository-path>
cd <repository-path>/.pkc/workspace
```

- PRODUCT/TRACE: workspace only; no source fallback.
- Benchmark phase 1: workspace only and frozen before source inspection.
- ENGINEERING/benchmark phase 2: source allowed only in the approved environment and only as needed for cross-check.
- Reports use paths, symbols, line ranges and business descriptions; do not copy proprietary source bodies.

## Benchmark cadence

Protocol:

`docs/benchmarks/product-value-benchmark-protocol.md`

```text
Level 0 — deterministic tests/build/smoke
Level 1 — targeted AI/product-value benchmark for one semantic workflow
Level 2 — full Agentic + Jin12 + Kesetovic at checkpoint/release/demo
```

## Exact next action

```text
independent rereview #17 of exact 96205a9a643864facaf9642a3b390ddcdbed59d9
→ PASS: mark R7.10 + V0.4.7-D PASS / COMPLETE; unlock only E
→ FAIL: regression-first minimum generic R7.10 repair; rerun exact-SHA gates and rereview
```

No new milestone may start before that external gate is resolved.
