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
product-value Fix #2 frontend URL + Output bridge PASS / COMPLETE FOR BOUNDED PATHS
product-value Fix #3 displayed-value lineage     UNLOCKED / ACTIVE NEXT CHECKPOINT
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

### Product-value Fix #2 — PASS / COMPLETE for bounded paths

Exact production HEAD:

`b4dcb5187ef996cf266a584a9df8c5bec534a94a`

Production commits:

```text
b69c01e15ea24d76b25621e9e81fcf297c4af10c — fix: resolve Angular service URL expressions
b4dcb5187ef996cf266a584a9df8c5bec534a94a — fix: bridge Angular output events to API calls
```

Exact-SHA validation for `b4dcb5187ef996cf266a584a9df8c5bec534a94a`:

```text
primary CI / WorkPlay / PokeTrade   35875287680 PASS
Loren pinned                        35875287748 PASS
Loren-main canary                   35875287700 PASS
Jellyfin parity                     35875287900 PASS
```

Targeted Kesetovic validation (`kesetovic/crm-system`, SHA `8e3b74bec4fdcd0144bd65f0c1b49c8e801bd2f7`):

```text
URL-only run  35873077699
final Fix #2  35874940385 PASS
```

The bounded URL path is:

`TypeScript URL expression → normalized route template → backend route match`

The bounded Output-event path is:

`child external-template UI action → exact child handler → exact @Output → literal discriminator → exact imported child selector → parent template $event handler → exact switch branch → parent target method → inject(ServiceType) receiver → existing service/API call → backend endpoint`

Ambiguous or unsupported URL/event/data-flow shapes remain fail-closed.

Relevant regressions:

```text
tests/Pkc.Frontend.Tests/angular-url-expression-regression.cjs
tests/Pkc.Frontend.Tests/angular-output-event-bridge-regression.cjs
```

Kesetovic PackOrder targeted result:

```text
                         baseline   URL-only   final Fix #2
Q3 UI/API/value lineage      0%        25%          50%
Q4 evidence trace           60%        70%          80%
```

Q3 remains partial because displayed-value origin is not yet proven.

### Next checkpoint — Fix #3 displayed-value lineage

Primary target: Agentic `Users Update`.

Initial bounded lineage:

`MAT_DIALOG_DATA → this.data.email → form-control / FormBuilder initialization → control email → formControlName="email" → displayed field`

Reuse existing response-binding, form-behavior, UI-field, rendered-value and lineage architecture. Require exact component/control/property identity at every promoted hop, add ambiguity/collision regressions, and fail closed whenever identity or ownership is not proven.

Fix #2 is complete for the bounded paths above. Fix #3 is now active for Agentic `Users Update` displayed-value lineage.

### Fix #3 implementation checkpoint — deterministic evidence

Pinned target: `hackersandwizards/agentic-engineering-training-angular` at `22f2aab64617f4de7984370a5bd40e8c9535dbf5`.

The workspace-only freeze before source inspection was Q3 `50%`, Q4 `90%`; the missing answer was the displayed-value lineage for `Users Update`.

The bounded implementation now proves, with exact component, form, control, property and imported-token identity:

`MAT_DIALOG_DATA.data.email → EditUserDialogComponent.form.email → EditUserDialogComponent.displayed.email`

It requires exact named imports for `MAT_DIALOG_DATA` from `@angular/material/dialog`, `FormBuilder` from `@angular/forms`, and `inject` from `@angular/core`; ambiguous injections, sibling/mismatched forms, duplicate ownership and same-file component collisions fail closed.

Deterministic verification:

```text
AngularFormBehaviorTests        11 / 11 PASS
AngularUiValueLineageKnowledge  3 / 3 PASS
CrossStackFeatureCandidateBuilder exact-component + external-template regression PASS
Users Update workspace render   full chain present in knowledge/workflows/users/update.md
```

Fix #3 remains ACTIVE / NOT COMPLETE pending related and broader verification plus the targeted Agentic Level-1 Q3/Q4 benchmark.

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
Fix #2:               PASS / COMPLETE FOR BOUNDED URL + OUTPUT PATHS
Fix #3:               UNLOCKED / ACTIVE NEXT CHECKPOINT
workspace privacy:    PRODUCT/TRACE workspace-only; ENGINEERING explicit
benchmark policy:     Level 0 deterministic / Level 1 targeted / Level 2 full checkpoint
tool/package:         RuaDen.Pkc.Tool 0.4.3-preview.2
C# raw schema:        0.4.4-csharp-raw
merged facts schema:  0.4.4
cross-stack schema:   0.4.6
frontend schema:      0.4.3-frontend
```
