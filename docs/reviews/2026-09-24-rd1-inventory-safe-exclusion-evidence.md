# RD1 Inventory + Safe Exclusion — Local PASS Evidence

Date: 2026-09-24
Checkpoint: V0.4.7-E0 / RD1
Decision: **LOCAL PASS** (implementation self-verified; push/CI pending, see below)

## Implementation

```text
fd3428f06bae7089a749f59b3d802d5a4e36b160
feat: discover repository shape before semantic scans
```

Parent: `ab243630b5a4b7aa76bbf8dac3bd87aa5098899c`.

Code:

- `src/Pkc.Core/Discovery/RepositoryDiscovery.cs` — single filesystem walk, bounded manifest reads, proof rules.
- `src/Pkc.Core/Discovery/RepositoryProfile.cs` — `RepositoryProfile`, `SourceArea`, `SourceRole`, `ScanMode`, `DiscoveryEvidence`, `DiscoveryConfidence`, file-pattern overrides, `Classify`.
- `src/Pkc.Core/Discovery/DiscoveryPaths.cs` — repository-relative path/glob helpers.
- `src/Pkc.Core/Discovery/RepositoryProfileSerializer.cs` — byte-stable JSON; local artifact `.pkc/discovery/repository-profile.json`.
- `src/Pkc.Core/Discovery/DiscoveryFirstScanPipeline.cs` — semantic scanner stages can only run after discovery is established and persisted.
- `src/Pkc.Cli/Program.cs` — `scan`/`build`/`run` use the pipeline; `[pkc:discover]` progress reports real profile counts.

## Model decisions (minimum for RD1)

- Source role and scan mode are separate. RD1 roles: `UNKNOWN`, `TEST_EVIDENCE`, `GENERATED_OR_RESTORABLE`, `INFRASTRUCTURE`, `TOOL_STATE`. RD1 scan modes: `DEEP_SCAN`, `LIGHT_INDEX`, `TEST_EVIDENCE`, `SAFE_AUTO_EXCLUDE`, `UNKNOWN`.
- `DEEP_SCAN` + `UNKNOWN` role means "included, ownership not yet established" and preserves the existing whole-root semantic behavior. No first-party/third-party role is asserted in RD1 (RD2/RD3).
- Areas are directories (project/workspace/package roots, proven exclusions, unreadable/reparse points) or single infrastructure files; the root `.` always exists. Every enumerated file is attributed to exactly one nearest area.
- File-pattern overrides inside an area carry their own role/mode/evidence (used for Angular test-target includes; extensible for RD3 carve-outs).

## Exclusion authority rules

`SAFE_AUTO_EXCLUDE` requires a structural fact other than the directory name:

| Area | Required proof |
| --- | --- |
| `bin` / `obj` | sibling `.csproj`/`.vbproj`/`.fsproj` and no project or `Directory.Build.*` redirect that leaves the default folder (`UseArtifactsOutput`, `ArtifactsPath`, non-`bin/` output path, non-`obj/` intermediate path) |
| `node_modules` | sibling `package.json` |
| `.angular` | sibling `angular.json` |
| declared build output | `angular.json` `build.options.outputPath` inside the repository, not containing the workspace or `sourceRoot`, and not `deleteOutputPath: false` |
| restored NuGet package | `<dir>/<dir>.nupkg` or `<id>/<version>/<id>.<version>.nupkg` |
| tool state | `.git` containing `HEAD`; root `.pkc` (PKC output root) |

Excluded areas are not walked. Everything else stays enumerated and included.

Test evidence: `Microsoft.NET.Test.Sdk`/xunit/NUnit/MSTest/TUnit package references, legacy test framework assembly references, `IsTestProject`, `MSTest.Sdk`, test project type GUID, and Angular test-target `tsconfig` includes that select a dedicated test-file shape (`*.x.ts`, excluding `*.d.ts`). Project or folder names never create test classification.

## Regression coverage

`tests/Pkc.CSharp.Tests/RepositoryDiscoveryInventoryRegressionTests.cs` builds a synthetic mixed repository with API host, legacy non-SDK MVC app, shared library, redirected-output worker, xunit/MSTest/`IsTestProject` test projects, a production library under `tests/`, a project named like tests without test evidence, an Angular workspace, `node_modules`, declared and undeclared `dist`, `.angular`, restored NuGet package plus a first-party folder under `packages/`, loose legacy JS/C# under `legacy`/`vendor`/`plugins`/`themes`/`Scripts`/`Content`/`old`/`dist`/`build`/`tools/bin`, infrastructure files, runtime configuration containing a private value, and `.git`/`.pkc` state.

| RD1 requirement | Test |
| --- | --- |
| 1 deterministic inventory | `Inventory_is_deterministic_and_independent_of_checkout_location_and_creation_order` |
| 2 no whole-repository semantic analysis | `Discovery_has_no_semantic_analyzer_dependency`; `Infrastructure_and_manifests_are_inventoried_without_reading_source_or_config_values` (content reads limited to MSBuild/Angular/TypeScript manifests) |
| 3 only proven generated/restorable excluded | `Only_structurally_proven_generated_or_restorable_areas_are_safe_auto_excluded` (exact expected set; invalidated/absent proof stays included) |
| 4 names never create exclusion authority | `Directory_names_alone_never_create_exclusion_authority` (mutation with a `vendor` name rule fails two tests) |
| 5 ambiguous areas included/UNKNOWN | same, plus file attribution totals |
| 6 tests are evidence, not production authority | `Tests_are_identified_from_structural_evidence_and_never_create_production_authority` |
| 7 deterministic ordering | ordinal ordering assertions + byte-identical relocated rerun |
| 8 discovery/plan before semantic scanners | `Discovery_and_persisted_profile_exist_before_any_semantic_scanner_stage_begins`; `Semantic_stages_never_run_when_discovery_cannot_establish_a_profile`; `CliProgressOutputRegressionTests.Repository_discovery_runs_before_expensive_semantic_scanners` |
| privacy | serialized profile contains no absolute root, no source marker, no config value |

## Verification

```text
dotnet build PKC.sln -c Release                      0 warnings, 0 errors
dotnet test PKC.sln -c Release --no-build            Pkc.CSharp.Tests 302/302, Pkc.Frontend.Tests 23/23
focused RD1 + CLI ordering tests                     11/11
mutation (name-only `vendor` exclusion)              caught by 2 tests
```

Semantic-scope parity: the pre-change CLI (`ab24363`) and the candidate CLI were both run with `pkc run` on copies of `samples/WorkPlaySample` and `samples/PokeTradeSystem`. `facts.json`, `feature-candidates.json`, `product-features.json` and all `.pkc/workspace` files were byte-identical; the only difference was the new `.pkc/discovery/` directory. Stdout was identical except for the output paths.

## Push / CI state

The implementation commit is local only. From the implementing environment, the SSH remote rejected authentication and the HTTPS push was not permitted by the session's tool policy. The operator must push `main` (`fd3428f` plus the docs commit above it); CI on that push is the final verification. Remote `main` was still `ab24363` when checked.

## Known gaps carried forward (not RD1 blockers)

- Scanner semantic scope is unchanged: RD1 alone does not reduce memory. The accepted scanner-internal name scopes (`CSharpSourceScope`: `test`, `tests`, `spikes`, `knowledge`; `FrontendSourceScope`: `dist`, `build`, `coverage`, `test`, `tests`, `__tests__`, `spike(s)`, `node_modules`, spec/test suffixes) are still in force inside the semantic path. RD6 must reconcile them with the plan without silently dropping UNKNOWN areas.
- VCS tracked/untracked state is not yet used as evidence.
- `libman`/`bower` restore destinations, minified/vendor distributions and tracked bundles are not yet classified (RD3); they stay included.
- Application ownership and project-reference edges are not modeled (RD2).
- The per-file list is local in-process only (`RepositoryProfile.Files`); the persisted ScanPlan format belongs to RD5/RD7.
- An Angular build without a declared `outputPath` is not assumed to write `dist/<project>`.
