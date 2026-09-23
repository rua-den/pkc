# PKC Status

Last updated: 2026-09-23

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                 PASS / COMPLETE
V0.4.5 real-repository generalization            PASS / COMPLETE
V0.4.6 business logic reconstruction             PASS / COMPLETE
V0.4.7-A origin and copy timing                  PASS / COMPLETE
V0.4.7-B computation and later change            PASS / COMPLETE
V0.4.7-C backend to API                          PASS / COMPLETE
V0.4.7-D API to UI / R7.9 binding                PASS / COMPLETE
V0.4.7-D mutation-causality blocker              PASS / CLOSED
V0.4.7-D API to UI / R7.10 joint visibility      REPAIRED / ALL GATES PASS / PENDING REREVIEW #17
V0.4.7-D overall                                 PENDING INDEPENDENT REREVIEW #17
V0.4.7-E product acceptance                      LOCKED behind D
R7.14 real-project positive yield                NOT PASS / REQUIRED FOR E
real-repo safety benchmark                       PASS
real-repo product-value benchmark                NOT PASS / PARTIAL USEFULNESS
product-value Fix #1 interface→concrete DI       PASS / COMPLETE FOR BOUNDED DIRECT-DI PATH
product-value Fix #2 frontend URL resolution     UNLOCKED / NEXT
product-value Fix #3 displayed-value lineage     LOCKED behind Fix #2 checkpoint
AI workspace preview spike                       IMPLEMENTED / VALIDATED PREVIEW / USER-AUTHORIZED
co-located workspace Git isolation               IMPLEMENTED / VALIDATED
formal AI-workspace W acceptance                 NOT UNLOCKED / NOT COMPLETE
continuous update/diff                           LOCKED
V0.5 Azure DevOps input evidence                 LOCKED
```

Formal R7.10/D acceptance remains a separate independent-review gate. Completion of product-value Fix #1 does not self-certify R7.10, V0.4.7-D, E, W, U, or V0.5.

## Formal R7.10 candidate — separate gate

```text
96205a9a643864facaf9642a3b390ddcdbed59d9
fix: tolerate duplicate Angular import aliases
```

Required independent review:

`docs/reviews/2026-09-23-v0.4.7-d-r7.10-rereview-17-request.md`

## Product-value Fix #1 — bounded interface → concrete traversal

Exact production SHA:

```text
1401d42291f5da4ce59ba79887ac53106b978dd7
fix: prove DI dispatch host authority
```

Standard exact-SHA validation is PASS:

```text
primary CI / WorkPlay / PokeTrade   PASS
Loren pinned                        PASS
Loren-main canary                   PASS
Jellyfin parity                     PASS

Release build                       0 warnings / 0 errors
C# tests                            274 / 274 PASS
Frontend tests                      13 / 13 PASS
```

Primary CI intentionally installs both `.NET 8` and `.NET 10`: PKC targets `net10.0`, while semantic regression fixtures still include `net8.0`. Do not collapse this compatibility matrix.

The proven dispatch path is deliberately bounded:

```text
controller / endpoint interface call
→ exact Roslyn interface method
→ one authoritative direct host DI registration
→ exact concrete implementation method
→ existing concrete guards / mutations / downstream calls
```

Authority constraints retained:

- assembly-qualified semantic identities;
- project-scoped registration identity;
- exact interface-member implementation and overload matching;
- exactly one matching direct registration;
- top-level supported startup registration only;
- registration must be on the supported host builder's `.Services`;
- the same builder must later be `.Build()`-ed;
- dead helpers, unrelated `IServiceCollection` instances, unrelated hosts/projects, ambiguous registrations and conditional registrations do not dispatch;
- factory delegates, decorators, assembly scanning, keyed services, helper registrations and other unsupported DI shapes remain fail-closed.

Relevant regression coverage:

- `tests/Pkc.CSharp.Tests/MinimalApiEndpointScannerTests.cs`
- `tests/Pkc.CSharp.Tests/DirectDiDispatchAuthorityScopeRegressionTests.cs`
- `tests/Pkc.CSharp.Tests/DirectDiDispatchCrossProjectCollisionRegressionTests.cs`

## Fix #1 Level-1 Jin12 benchmark

Benchmark report:

`docs/benchmarks/2026-09-23-fix1-jin12-di-product-value-benchmark.md`

Pinned inputs:

```text
PKC production      1401d42291f5da4ce59ba79887ac53106b978dd7
benchmark wrapper   ed498f63578444e4a13ea5a31957d2597890f2f8
workflow run        35862459255
target repository   jin12-xyz/CRM
target SHA          00493af54d4d9e146d1c6eb75f5dc8f3898f09ec
```

Phase 1 was frozen from `.pkc/workspace` only before source inspection. Phase 2 then cross-checked the minimum pinned source.

```text
                         baseline   Fix #1
Q1 permissions/preconds     70%      100%
Q2 successful state          0%      100%
Q4 evidence trace           50%      100%
repo score                  40%      100%
delta                                  +60 percentage points
```

Recovered product facts include authorization, the concrete contact-existence precondition, all update mutations (`FirstName`, `LastName`, `Email`, `Phone`, `JobTitle`, `CompanyId`, `UpdatedAt`), and persistence through the contact repository. The workspace also exposes the proof edge from controller/interface call through the direct DI registration to `ContactService.UpdateAsync`.

No new source mismatch or overclaim was found for Q1/Q2/Q4. Fix #1 is therefore complete only for the bounded direct-DI proof path above. Unsupported DI shapes remain intentionally unknown/fail-closed.

The full three-repository product-value score is **not recomputed** by this targeted Level-1 run. The historical full-corpus baseline remains 63.9% until the next Level-2 checkpoint.

## Current workspace / privacy boundary

Preferred product UX:

```text
pkc run <repository-path>
cd <repository-path>/.pkc/workspace
```

- PRODUCT and TRACE remain workspace-only; do not fall back to source.
- Benchmark phase 1 is workspace-only and must be frozen before source inspection.
- ENGINEERING / benchmark phase 2 may inspect only the minimum required source in the approved company environment.
- Reports record paths, symbols, line ranges, business descriptions and scores; do not copy proprietary source bodies.
- `.pkc/` remains co-located and locally Git-excluded where supported; PKC does not rewrite tracked ignore policy or untrack existing files.

## Product-value baseline / next repairs

Reference full-corpus baseline before the three major repairs:

```text
Agentic Users Update       80.8%
Jin12 Contacts Update      40.0%
Kesetovic PackOrder        65.0%
backend PO/QC core         72.6%
overall applicable         63.9%
```

Fix #1 now has targeted Jin12 evidence at 100% for Q1/Q2/Q4, but product-value acceptance overall remains NOT PASS until the remaining major semantic gaps are repaired and a Level-2 benchmark is run.

### Next checkpoint — Fix #2 frontend service URL expression resolution

Primary target: Kesetovic `PackOrder`.

Initial bounded scope:

- string concatenation;
- template literals;
- known local constants;
- proven `this.property` values;
- normalized route templates;
- backend route matching.

Do not automatically include `HttpParams`, `HttpHeaders`, Angular event propagation, or other UI plumbing. If URL matching lands but PackOrder still lacks action→API proof, reproduce the remaining edge separately and keep it regression-first.

After Fix #2 is locally verified, run only the targeted Kesetovic Level-1 questions affected by the change. Fix #3 remains displayed-value lineage for Agentic `Users Update`.

## Benchmark cadence

Protocol:

`docs/benchmarks/product-value-benchmark-protocol.md`

```text
Level 0 — deterministic tests/build/smoke
Level 1 — targeted AI benchmark for one semantic workflow
Level 2 — full Agentic + Jin12 + Kesetovic at milestone/release/demo or after the three major fixes
```

Do not use the full AI corpus as the normal edit-test loop.

## Version semantics

```text
formal roadmap:       V0.4.7-D / R7.10 pending independent rereview #17
workspace preview:    implemented + validated PREVIEW
Fix #1:               bounded direct-DI traversal PASS
Fix #2:               UNLOCKED / NEXT
Fix #3:               locked behind current semantic checkpoint
workspace privacy:    PRODUCT/TRACE workspace-only; ENGINEERING explicit
benchmark policy:     Level 0 deterministic / Level 1 targeted / Level 2 full checkpoint
tool/package:         RuaDen.Pkc.Tool 0.4.3-preview.2
C# raw schema:        0.4.4-csharp-raw
merged facts schema:  0.4.4
cross-stack schema:   0.4.6
frontend schema:      0.4.3-frontend
```
