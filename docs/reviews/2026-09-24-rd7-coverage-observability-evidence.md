# RD7 Coverage + Observability + Plan-Only Inspection — Local PASS Evidence

Date: 2026-09-24
Checkpoint: V0.4.7-E0 / RD7
Decision: **LOCAL PASS** (implementation self-verified; push/CI pending with RD1–RD6)

## Implementation

```text
d0c1017bd83d0d4b732facb5ddeddc5facfe48ca
feat: add plan-only discovery and persisted scan coverage
```

Parent: `6dcdf81` (RD6 docs) → `280450c` (RD6) → … → `ab24363`.

Contract chosen for plan-only inspection: **`pkc discover <repository-path>`**.

- `pkc discover` runs discovery and the scan plan and persists `.pkc/discovery/repository-profile.json` and `scan-plan.json`. It prints the discovery/plan summary and starts no semantic scanner. It writes no `facts.json` and no workspace, and it deletes any previous `scan-coverage.json` so old coverage cannot be read as the new plan's.
- Every executing command (`scan`, `build`, `run`) persists `.pkc/discovery/scan-coverage.json` (schema `0.1.0-scan-coverage`) containing:
  - the plan fingerprint and whether the plan scope was applied;
  - plan coverage counts;
  - files withheld by the plan, per scan mode;
  - the excluded-area count and the UNKNOWN areas;
  - each stage's planned files, executable files, and the planned files the scanner name scope still withholds.

  It is byte-stable and local-only, and contains no absolute paths, source bodies or configuration values.
- `[pkc:coverage]` progress reports the actual executed counts; `[pkc:scope:<stage>]` reports per-stage counts before each stage.
- `pkc run` adds a count-only `_meta/coverage.json` to `.pkc/workspace`, with no paths, names or values. The answer contract gains one line telling agents that answers depending on unanalyzed areas are not proven.

Code: `src/Pkc.Core/Discovery/ScanCoverage.cs` (new), `DiscoveryFirstScanPipeline.cs`, `RepositoryProfileSerializer.cs`, `src/Pkc.Knowledge/AiWorkspaceRenderer.cs`, `src/Pkc.Cli/Program.cs`.

## Requirement → proof

`tests/Pkc.CSharp.Tests/ScanCoverageRegressionTests.cs` (4; two run the real CLI process):

| RD7 requirement | Test |
| --- | --- |
| inspect detection and planned scope before expensive execution | `Discover_is_plan_only_and_starts_no_semantic_scanner`: CLI exits 0, plan progress printed, no `scan:`/`scope:` stage, profile and plan written, stale coverage removed, no facts or workspace |
| persist local coverage without source bodies/config values | `Coverage_record_reports_plan_and_scanner_scope_withholding_byte_stably_and_locally`: test-evidence 3, runtime-index 1, scanner-scope withholding listed per stage; byte-stable across warm runs; no absolute root; whole-root runs record `scoped: false` and no plan withholding |
| surface coverage at the workspace boundary privacy-safely | `Portable_workspace_coverage_is_count_only`: portable counts equal the local record; no enumerated path, name or root appears; answer contract references `_meta/coverage.json` |
| progress reflects actual phases and counts | `Run_progress_coverage_and_workspace_summary_reflect_the_actual_execution`: `pkc run` CLI; per-stage scope lines and the coverage line match the persisted record; phase order is plan → scope → scan → coverage → workspace; workspace `_meta/coverage.json` equals the portable projection of the local record |

Mutations, each caught and reverted:

- keeping stale coverage in plan-only mode fails the discover test;
- leaking withheld paths into the portable note fails the privacy test.

## Verification

```text
dotnet build PKC.sln -c Release                      0 warnings, 0 errors
dotnet test PKC.sln -c Release --no-build            Pkc.CSharp.Tests 331/331, Pkc.Frontend.Tests 23/23
focused RD7 tests                                    4/4
```

Sample parity (`pkc run`, RD7 CLI, copies of both samples, against `ab24363`):

- `facts.json`, `feature-candidates.json`, `product-features.json` and all knowledge files are byte-identical.
- The intended differences are:
  - new `.pkc/discovery/scan-coverage.json`;
  - new `.pkc/workspace/_meta/coverage.json`;
  - one added answer-contract line.
- `pkc discover` on PokeTrade: 26 files, 5 scopes, 2 host waves, 18 semantic files; no scanner started.

## Known gaps carried forward

- On a fresh repository the first run creates `.pkc/`. Later discoveries list it as an excluded tool-state area, so the first and later plan/coverage `excludedAreas` counts differ by one. The fingerprint and file lists are unaffected.
- Generated workspace text built from C# raw string literals inherits the build checkout's line endings (existing behaviour, unchanged here). Parity was checked with CRLF working files, matching the baseline build.
- Coverage is counts plus local paths. Area-level (rather than file-level) portable coverage for product answers is not attempted.
