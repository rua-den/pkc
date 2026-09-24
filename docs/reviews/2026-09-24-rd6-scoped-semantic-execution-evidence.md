# RD6 Scoped/Bounded Semantic Execution — Local PASS Evidence

Date: 2026-09-24
Checkpoint: V0.4.7-E0 / RD6
Decision: **LOCAL PASS** (implementation self-verified; push/CI pending with RD1–RD5)

## Implementation

```text
280450cdaa77491b8a5ab6a45ae7f0cb8f0caa0d
feat: run semantic scanners inside the scan plan scope
```

Parent: `01c638a` (RD5 docs) → `c440eea` (RD5) → … → `ab24363`.

Code:

- `src/Pkc.Core/Discovery/SemanticSourceScope.cs` (new) — plan-derived boundary. A path is withheld only when discovery positively classified it `SAFE_AUTO_EXCLUDE`, `LIGHT_INDEX` (generated outputs, infrastructure files), `RUNTIME_DEPENDENCY_INDEX` (declared vendor content) or `TEST_EVIDENCE`. `DEEP_SCAN` and `UNKNOWN` stay visible. A path discovery never saw is withheld only when it sits in a safely excluded area; anything else fails open. Lookups are exact first, then case-insensitive, so legacy project `Compile` paths with different casing still resolve. The scope becomes ambient (`AsyncLocal`) through `Enter()`.
- `src/Pkc.Core/Discovery/DiscoveryFirstScanPipeline.cs` — `DiscoveryState.Scope`; a `scoped` option; each stage runs inside `Scope.Enter()` when scoped. Stages declare `Scanner` (the plan scanner id) and `InScannerSourceScope` (the scanner's accepted name scope). `SemanticStageExecution` records planned files, executable files, and the planned files the name scope still withholds.
- `src/Pkc.CSharp/CSharpSourceScope.cs` and the private `IsExcluded` checks in `CSharpRepositoryScanner`, `CSharpSupplementalScanner` and `MinimalApiEndpointScanner` — accepted name scope **or** ambient plan withholding. This covers `.cs` enumeration, the `.csproj` sets opened by `MSBuildWorkspace`, the syntax trees admitted into semantic models, and the final raw fact/relation filter.
- `src/Pkc.Frontend/FrontendSourceScope.cs` and the private `IsExcluded` checks in the Angular/React scanners — the same union. `FrontendScanner.ApplyProductSourceScope` therefore also drops any adapter fact whose source file the plan withholds.
- `src/Pkc.Frontend/NodeSemanticScope.cs` (new) plus the node TypeScript walkers (`AngularTypeScriptAstScanner`, `AngularUrlExpressionEnricher`, `AngularOutputEventBridge.cjs`). With an ambient scope, the walkers get a temporary `PKC_SEMANTIC_SCOPE` file listing excluded areas and withheld `.ts` files, and skip both. Without it, their behaviour is unchanged.
- `src/Pkc.Cli/Program.cs` — `pkc run` uses `scoped: true`; `scan` and `build` keep their whole-root scope (plan: "Keep `scan` / `build` compatibility until an explicit migration decision"). New `[pkc:scope:<stage>]` progress line.
- `CSharpEvidenceScanner.IsInSourceScope` and `FrontendScanner.IsInSourceScope` expose the accepted name scopes, independent of any plan.

## Requirement → proof

| RD6 requirement | Proof |
| --- | --- |
| SAFE_AUTO_EXCLUDE never reaches deep scanners | an Angular build `outputPath` file with an API call yields no fact under `run`, but does yield one under the whole-root scope; `node_modules` paths discovery never enumerated are withheld (`Scope_decisions_…`) |
| RUNTIME_DEPENDENCY_INDEX internals not deep-scanned | the declared LibMan file yields no fact under `run`; the undeclared local file in the same destination (carve-out) still does |
| targeted shared code follows ownership | shared/owned libraries are `DEEP_SCAN` and are analyzed once in the single bounded pass; the plan's waves attribute them to every owning host (RD5) |
| tests stay separate | a test project outside any `test`/`tests` directory yields endpoint facts under the whole-root scope and none under `run`; Angular spec files and test `.csproj` files are withheld |
| UNKNOWN not silently dropped | a LibMan destination with no file list (`THIRD_PARTY_UNKNOWN`) is still analyzed. Planned semantic files that a scanner's accepted name scope withholds (`knowledge/`, frontend `build/`) are listed in `SemanticStageExecution.WithheldByScannerScope` and counted in progress |
| accepted semantics unchanged inside the selected scope | scoped facts and relations equal whole-root facts and relations restricted to non-withheld paths, compared byte for byte as serialized JSON for both scanners (`Accepted_semantics_…`) |
| name-scope reconciliation | scanner name scopes stay as an extra filter, so production authority is never widened; the gap between plan and name scope is reported rather than hidden |
| heavy state released between bounded waves when safe | see known gaps: RD6 runs one bounded pass over the union of planned scopes, because per-wave passes would split cross-application linking |

Tests: `tests/Pkc.CSharp.Tests/ScopedSemanticExecutionRegressionTests.cs` (4) and `CliProgressOutputRegressionTests.Run_executes_semantic_stages_inside_the_plan_scope_while_scan_and_build_keep_whole_root`.

Mutations, each caught and reverted:

- dropping `TEST_EVIDENCE` from the withheld modes fails 2 tests;
- inverting scoped stage entry in the pipeline fails 3 tests.

## Verification

```text
dotnet build PKC.sln -c Release                      0 warnings, 0 errors
dotnet test PKC.sln -c Release --no-build            Pkc.CSharp.Tests 327/327, Pkc.Frontend.Tests 23/23
focused RD6 tests                                    4/4 (+1 CLI source test)
```

Sample parity: `pkc run` with the RD6 CLI on copies of both samples leaves `facts.json`, `feature-candidates.json`, `product-features.json` and `.pkc/workspace` byte-identical to `ab24363`. `.pkc/discovery/` is identical to the RD5 run. Progress output:

- WorkPlay: csharp 1/1 and frontend 1/1 planned files executable;
- PokeTrade: csharp 7/7 and frontend 11/11 planned files executable.

Neither sample has any withheld vendor, generated or test source inside scanner reach, so resource effect on the samples is nil by construction. Measurement belongs to RD8.

## Known gaps carried forward

- **Single bounded pass, not per-wave passes.** Running each host wave separately would duplicate shared-library analysis and split cross-application linking (frontend→API, shared DTOs), which would change accepted semantics. `CSharpProjectSemanticEnricher` still keeps all loaded projects' semantic models alive for its pass. Releasing per wave is a Roslyn-internals change deferred until RD8 measures memory after scope reduction.
- Scanners still walk directories and then filter; enumeration is not yet driven by the plan's file list. Walk cost over large restored trees is unchanged.
- Node-side walker gating is implemented but only runs where a local TypeScript runtime exists; the regression fixtures have none. The fact-level filter in `FrontendScanner` is what the tests exercise, and it guarantees withheld files emit no frontend facts.
- The execution record (`WithheldByScannerScope`) lives in memory and in progress output; persisting coverage artifacts is RD7.
- `scan` / `build` remain whole-root until an explicit migration decision.
