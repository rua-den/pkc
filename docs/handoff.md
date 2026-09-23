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

## Required next action — Fix #2

Target: Kesetovic `PackOrder`.

Goal: prove frontend service URL expressions well enough to connect the relevant frontend request to the correct backend route.

Initial bounded scope:

```text
literal
+ binary string concatenation
+ template literal / template spans
+ known local constants
+ proven this.property values
→ normalized route template
→ backend route match
```

Regression-first sequence:

1. inspect the current Kesetovic PackOrder workspace miss and the exact source expression only after freezing the workspace answer;
2. create the smallest compile-valid TypeScript/Angular regression reproducing the URL expression shape;
3. implement the minimum bounded evaluator in the existing frontend scanning/linking architecture;
4. add ambiguity/fail-closed cases;
5. run focused frontend tests;
6. run related C#/cross-stack tests if route binding changes affect them;
7. run the full relevant local suite;
8. review the complete diff;
9. create one coherent implementation commit and one push;
10. use CI as final verification;
11. run only the targeted Kesetovic Level-1 benchmark.

Do **not** automatically add `HttpParams`, `HttpHeaders`, Angular `@Output` propagation or broad dataflow. If the URL proof is correct but the benchmark still misses action→API linkage, freeze that as a separate blocker and reproduce it independently.

Do not start Fix #3 until Fix #2 reaches its terminal checkpoint.

## Fix #3 — later

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

Fix #1 is at terminal state (1). The next coding checkpoint is Fix #2.
