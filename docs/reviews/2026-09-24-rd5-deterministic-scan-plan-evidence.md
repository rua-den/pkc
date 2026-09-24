# RD5 Deterministic ScanPlan — Local PASS Evidence

Date: 2026-09-24
Checkpoint: V0.4.7-E0 / RD5
Decision: **LOCAL PASS** (implementation self-verified; push/CI pending with RD1–RD4)

## Implementation

```text
c440eea592c0360e0b9cde3456b1dcbf39727cb0
feat: build a deterministic scan plan before semantic scans
```

Parent chain: `b870380` (RD4 docs) → `a37d936` (RD4) → `047326e` → `ac3efd9` (RD3) → `2ebce42` → `05eadb1` (RD2) → `26a7f59` → `fd3428f` (RD1) → `ab24363`.

Code:

- `src/Pkc.Core/Discovery/ScanPlan.cs` — `ScanPlan`, `ScanScope`, `ScanExclusion`, `ScanWave`, `PlanCoverage`, `ScanPlanner` (schema `0.1.0-scan-plan`).
- `src/Pkc.Core/Discovery/RepositoryProfileSerializer.cs` — shared byte-stable JSON options; `ScanPlanSerializer`; artifacts now `.pkc/discovery/repository-profile.json` + `.pkc/discovery/scan-plan.json`.
- `src/Pkc.Core/Discovery/DiscoveryFirstScanPipeline.cs` — `DiscoveryState` (profile, plan, paths) built and persisted before any stage; stages receive it.
- `src/Pkc.Cli/Program.cs` — `[pkc:plan]` summary and plan path.

## Plan contract

- **Scopes**: every enumerated file in exactly one scope keyed by area (+ override pattern), attributed components (nearest component area) and scanner set. Each scope carries role, scan mode, confidence, scanners, coverage and evidence.
- **Scanners**: only for `DEEP_SCAN` — `csharp-semantic` for `.cs`, `frontend-semantic` for `.ts/.tsx/.js/.jsx/.mjs/.cjs/.html/.htm`.
- **Coverage**: `SEMANTIC`, `NOT_ANALYZABLE` (deep scan but no supported scanner), `INDEXED` (light/runtime index), `TEST_EVIDENCE`, `UNKNOWN`.
- **Exclusions / unknown areas**: listed explicitly with evidence.
- **Waves**: `host:<id>` per production host (host + owned components; shared components in every owning wave), then `unowned`, `test`, `unattributed`. Test-evidence scopes join only the test wave; every semantic scope is in at least one wave.
- **Fingerprint**: `sha256:` over profile schema, enumerated paths and the contents discovery actually read (manifests, byte-compared files, runtime loader files). Source bodies discovery never reads are intentionally not tracked.
- **Privacy/determinism**: no timestamps, absolute paths, source bodies or configuration values; ordinal ordering; `\n` line endings.

RD5 does not change what the semantic scanners receive.

## Regression coverage

`tests/Pkc.CSharp.Tests/ScanPlanRegressionTests.cs` — two hosts sharing a library, an unowned library, a test project, an Angular application with test-target spec files, loose legacy JS, docs, infrastructure, private config, restored `bin/obj`/`node_modules`, `.git`.

| RD5 requirement | Test |
| --- | --- |
| every file planned once with role/mode/scanners/coverage/evidence | `Every_enumerated_file_is_planned_exactly_once_with_role_mode_scanners_and_coverage` |
| explicit exclusions | `Exclusions_are_explicit_and_never_planned_for_scanning` |
| ownership waves, UNKNOWN/unattributed kept, tests isolated | `Application_waves_follow_ownership_and_keep_unowned_tests_and_unattributed_scopes` (mutation letting test scopes into production waves is caught) |
| byte-stable, privacy-safe, staleness fingerprint | `Plan_is_byte_stable_privacy_safe_and_its_fingerprint_detects_manifest_changes` |
| plan persisted before semantic stages | `Pipeline_persists_the_plan_before_semantic_stages_run` |

## Verification

```text
dotnet build PKC.sln -c Release                      0 warnings, 0 errors
dotnet test PKC.sln -c Release --no-build            Pkc.CSharp.Tests 322/322, Pkc.Frontend.Tests 23/23
focused discovery + plan + CLI ordering tests        31/31
mutation (test scopes in production waves)           caught
```

Sample parity: `pkc run` on copies of both samples with the RD5 CLI produced `.pkc` output identical to `ab24363` except for `.pkc/discovery/`. PokeTrade plan: 5 scopes, 2 host waves, 18 semantic files, 8 not-analyzable, 2 exclusions.

## Known gaps carried forward

- Scope lists include every file path; very large repositories produce a large local plan file (acceptable for local inspection; RD7 decides portable summaries).
- Component attribution is nearest-component-directory; Angular projects whose root is not a discovery area are attributed by path, not by angular.json membership of intermediate folders.
- Semantic scanners do not yet consume the plan (RD6).
