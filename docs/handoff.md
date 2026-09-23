# PKC Handoff

Last updated: 2026-09-23

This handoff is for the next PKC coding or audit session.

## Read first

1. root `CLAUDE.md`
2. `AGENTS.md`
3. `docs/status.md`
4. this handoff
5. `docs/milestones.md`
6. `docs/product-knowledge-contract.md`
7. `docs/v0.4.7-acceptance-plan.md`
8. `docs/benchmarks/product-value-benchmark-protocol.md`
9. `docs/benchmarks/2026-09-23-fix1-jin12-di-product-value-benchmark.md`
10. `docs/reviews/2026-09-23-ai-workspace-preview-company-audit-request.md`
11. `docs/reviews/2026-09-23-v0.4.7-d-r7.10-rereview-17-request.md` only when explicitly assigned the formal R7.10/D independent-review role

Then inspect current `main`, recent commits, working tree state, production code and relevant regressions. Do not reset to a historical SHA merely because this handoff names one.

## Formal acceptance remains separate

```text
A/B/C                    PASS / COMPLETE
R7.9                     PASS / COMPLETE
mutation-causality       PASS / CLOSED
R7.10                    REPAIRED / ALL GATES PASS / PENDING REREVIEW #17
V0.4.7-D                 PENDING INDEPENDENT REREVIEW #17
V0.4.7-E                 LOCKED
R7.14                    NOT PASS / REQUIRED FOR E
```

Formal R7.10 candidate:

```text
96205a9a643864facaf9642a3b390ddcdbed59d9
fix: tolerate duplicate Angular import aliases
```

Do not self-certify that gate from a product-value implementation thread.

## Product-value Fix #1 checkpoint — PASS in bounded scope

Exact production SHA under validation:

```text
1401d42291f5da4ce59ba79887ac53106b978dd7
fix: prove DI dispatch host authority
```

Observed exact validation:

```text
primary CI / WorkPlay / PokeTrade   PASS
Loren pinned                        PASS
Loren-main canary                   PASS
Jellyfin parity                     PASS
Release build                       0 warnings / 0 errors
C# tests                            274 / 274 PASS
Frontend tests                      13 / 13 PASS
```

Keep the primary CI `.NET 8` + `.NET 10` setup. PKC targets `net10.0`; many semantic fixtures remain `net8.0`.

Fix #1 now proves this bounded path:

```text
controller / endpoint interface call
→ exact interface method symbol
→ one authoritative direct `builder.Services.AddScoped/AddTransient/AddSingleton<I,T>` registration
→ exact concrete implementation method
→ concrete guards / mutations / downstream calls
```

Safety boundaries are part of the feature, not incidental limitations. Keep fail-closed behavior for:

- ambiguous/multiple registrations;
- conditional registrations;
- dead/uninvoked helper registrations;
- unrelated service collections;
- builders that are never built;
- unrelated hosts/projects;
- assembly/type-name collisions;
- factory delegates;
- decorators;
- assembly scanning;
- keyed services;
- helper/extension registration indirection;
- any other DI shape without a concrete regression proving a safe generic path.

Relevant regressions:

- `tests/Pkc.CSharp.Tests/MinimalApiEndpointScannerTests.cs`
- `tests/Pkc.CSharp.Tests/DirectDiDispatchAuthorityScopeRegressionTests.cs`
- `tests/Pkc.CSharp.Tests/DirectDiDispatchCrossProjectCollisionRegressionTests.cs`

## Level-1 Jin12 result

Pinned benchmark:

```text
benchmark branch    benchmark/fix1-jin12-1401
wrapper commit      ed498f63578444e4a13ea5a31957d2597890f2f8
workflow run        35862459255
target              jin12-xyz/CRM
target SHA          00493af54d4d9e146d1c6eb75f5dc8f3898f09ec
artifact            .pkc/workspace only for phase 1
```

Phase 1 was frozen before source access:

```text
Q1  100%
Q2  100%
Q4  100%
repo 100%
```

Baseline:

```text
Q1   70%
Q2    0%
Q4   50%
repo 40%
```

Delta: **+60 percentage points**.

Phase 2 source cross-check found no mismatch for those questions. The generated workspace recovered:

- authorization for Contacts Update;
- the concrete contact-existence precondition;
- successful mutations to `FirstName`, `LastName`, `Email`, `Phone`, `JobTitle`, `CompanyId`, and `UpdatedAt`;
- persistence through the contact repository;
- a traceable controller/interface → DI registration → concrete `ContactService.UpdateAsync` proof path.

No proprietary source snippets were copied into the report.

Report:

`docs/benchmarks/2026-09-23-fix1-jin12-di-product-value-benchmark.md`

Decision: **Fix #1 is complete only for the bounded direct-DI proof path above. Fix #2 is now unlocked.**

The full Agentic + Jin12 + Kesetovic benchmark has not been rerun. Keep the historical 63.9% full-corpus baseline until Level 2.

## Product-value Fix #2 checkpoint — PASS / COMPLETE for bounded paths

Exact production HEAD:

`b4dcb5187ef996cf266a584a9df8c5bec534a94a`

Production commits:

```text
b69c01e15ea24d76b25621e9e81fcf297c4af10c — fix: resolve Angular service URL expressions
b4dcb5187ef996cf266a584a9df8c5bec534a94a — fix: bridge Angular output events to API calls
```

Exact-SHA validation:

```text
primary CI / WorkPlay / PokeTrade   35875287680 PASS
Loren pinned                        35875287748 PASS
Loren-main canary                   35875287700 PASS
Jellyfin parity                     35875287900 PASS
```

Targeted Kesetovic validation:

```text
target repository: kesetovic/crm-system
target SHA: 8e3b74bec4fdcd0144bd65f0c1b49c8e801bd2f7
URL-only run: 35873077699
final Fix #2 run: 35874940385 PASS
```

Bounded URL path:

`TypeScript URL expression → normalized route template → backend route match`

Bounded Output-event path:

`child external-template UI action → exact child handler → exact @Output → literal event discriminator → exact imported child selector → parent template $event handler → exact switch branch → parent target method → inject(ServiceType) receiver → existing service/API call → backend endpoint`

Relevant regressions:

```text
tests/Pkc.Frontend.Tests/angular-url-expression-regression.cjs
tests/Pkc.Frontend.Tests/angular-output-event-bridge-regression.cjs
```

Kesetovic PackOrder targeted result: Q3 `0% → 25% → 50%`; Q4 `60% → 70% → 80%` (baseline → URL-only → final Fix #2). Ambiguous or unsupported shapes remain fail-closed. Q3 remains partial pending displayed-value lineage.

Formal R7.10 remains separate: `REPAIRED / ALL GATES PASS / PENDING INDEPENDENT REREVIEW #17`. Preserve that state and do not self-certify it from this product-value thread.

## Required next action — Fix #3

Target: Agentic `Users Update`.

Goal: prove displayed-value lineage from `MAT_DIALOG_DATA` through `this.data.email`, form-control / FormBuilder initialization, control `email`, and `formControlName="email"` to the displayed field.

Pinned target SHA: `22f2aab64617f4de7984370a5bd40e8c9535dbf5`

Initial bounded lineage:

`MAT_DIALOG_DATA → this.data.email → form-control / FormBuilder initialization → control email → formControlName="email" → displayed field`

Regression-first sequence:

1. freeze the current workspace-only Q3/Q4 answer before source inspection;
2. inspect only enough pinned source to reproduce the missing lineage;
3. create the smallest compile-valid Angular/TypeScript regression;
4. reuse existing response-binding, form-behavior, UI-field, rendered-value and lineage architecture;
5. require exact component/control/property identity at every promoted hop;
6. add ambiguity/collision negative regressions;
7. fail closed whenever identity or ownership is not proven;
8. run focused, related, then broader relevant local tests;
9. review the full diff and prefer one coherent implementation commit and push;
10. run the targeted Agentic Level-1 Q3/Q4 benchmark after deterministic validation.

Do not build an unconstrained generic JavaScript/Angular data-flow engine or add unrelated semantic areas without a directly blocking Fix #3 regression.

Fix #2 has reached its terminal checkpoint. Fix #3 is now UNLOCKED / ACTIVE.

Current Fix #3 deterministic checkpoint:

Pinned target `hackersandwizards/agentic-engineering-training-angular` SHA `22f2aab64617f4de7984370a5bd40e8c9535dbf5`; workspace-only freeze was Q3 `50%`, Q4 `90%` before source inspection.

Implemented bounded lineage:

`MAT_DIALOG_DATA.data.email → EditUserDialogComponent.form.email → EditUserDialogComponent.displayed.email`

Exact imported-token identity is required (`MAT_DIALOG_DATA` from `@angular/material/dialog`, `FormBuilder` from `@angular/forms`, `inject` from `@angular/core`). Component body, formGroup ownership, control/property identity and ambiguity boundaries are checked; unsupported or colliding shapes fail closed.

Focused evidence: `AngularFormBehaviorTests` 11/11 PASS, including direct-expression, nested-group, duplicate-control, import-identity, lexical-spoof and collision negatives; `AngularUiValueLineageKnowledgeTests` 3/3 PASS, including CrossStackFeatureCandidateBuilder exact-component attachment from an external HTML template and markdown rendering; regenerated pinned workspace contains the complete chain in `knowledge/workflows/users/update.md`.

Fix #3 remains ACTIVE / NOT COMPLETE pending related/broader verification and targeted Agentic Level-1 Q3/Q4 benchmark.

## Fix #3 — displayed-value lineage

Primary target: Agentic `Users Update`.

Initial bounded chain:

```text
MAT_DIALOG_DATA
→ this.data.email
→ form-control initialization
→ control `email`
→ formControlName="email"
→ displayed field
```

Reuse existing lineage architecture and keep ambiguity handling explicit.

## Workspace / privacy boundary

Preferred product flow:

```text
pkc run <TEAM_REPOSITORY_PATH>
cd <TEAM_REPOSITORY_PATH>/.pkc/workspace
```

- PRODUCT/TRACE: workspace only; no source fallback.
- Benchmark phase 1: workspace only and frozen before source.
- ENGINEERING/benchmark phase 2: source allowed only in the approved company environment.
- Reports use paths, symbols, line ranges and business descriptions, never raw proprietary source bodies.
- `.pkc/` is co-located and locally Git-excluded where supported; PKC does not rewrite tracked ignore rules or untrack existing files.

## Git / CI discipline

For each logical fix:

```text
inspect
→ reproduce with regression
→ implement minimum generic fix
→ focused tests
→ related tests
→ broader relevant suite
→ review diff
→ one coherent commit
→ one push
→ CI final verification
```

Do not use GitHub Actions as the edit-test loop. A failing command is evidence to investigate, not a terminal state.

## Terminal state

Keep working on the assigned checkpoint until:

1. it is PASS and locally verified;
2. an external review/gate is genuinely required and cannot be performed in-session; or
3. a genuinely external blocker is proven and documented.

Fix #2 is at terminal state (1). The next coding checkpoint is Fix #3.
