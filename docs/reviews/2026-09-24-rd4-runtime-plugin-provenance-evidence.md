# RD4 Runtime/Plugin Provenance — Local PASS Evidence

Date: 2026-09-24
Checkpoint: V0.4.7-E0 / RD4
Decision: **LOCAL PASS** (implementation self-verified; push/CI pending with RD1–RD3)

## Implementation

```text
a37d936fc68c82e599f23da8b64bcb6ea85ef9fb
feat: prove runtime plugin edges in repository discovery
```

Parent chain: `047326e` (RD3 docs) → `ac3efd9` (RD3) → `2ebce42` → `05eadb1` (RD2) → `26a7f59` → `fd3428f` (RD1) → `ab24363`.

Code:

- `src/Pkc.Core/Discovery/RepositoryDiscovery.Runtime.cs` — bounded Tier-2 composition probe over production host and test project C# files; literal identity extraction.
- `src/Pkc.Core/Discovery/ComponentGraph.cs` — project records carry literal assembly name, output paths and `Copy` tasks; `runtime-plugin-load` edges, copy provenance, unresolved runtime findings; edges keyed by kind.
- `src/Pkc.Core/Discovery/RepositoryProfile.cs` — `CompositionProbe`; schema `0.4.0-discovery`.
- `src/Pkc.Cli/Program.cs` — runtime-plugin edge count in `[pkc:discover] Components`.

## Edge rule

`runtime-plugin-load` (HIGH) from host H to plugin P requires:

1. **Loader** — `Assembly.Load/LoadFrom/LoadFile/UnsafeLoadFrom` or `LoadFromAssemblyName/LoadFromAssemblyPath` call in one of H's own `.cs` files (not a nested project's, not an excluded area), comment lines skipped.
2. **Identity** — a string literal inside the call's own argument list: an assembly name (for `Load`/`LoadFromAssemblyName`, full names cut at the first comma) or a `*.dll` file name (path-based loads), matching exactly one project's literal assembly name (declared `AssemblyName` or file name; case-insensitive as the runtime loader is).
3. **Delivery** — P's `OutputPath`/`OutDir`/`BaseOutputPath` literal prefix resolves inside H's tree; or an MSBuild `Copy` in P whose `SourceFiles` references P's own output and whose destination literal prefix is inside H's tree; or a `Copy` in H from P's tree into H's tree; or an unconditional project reference from H to P.

Literal prefixes accept a leading `$(MSBuildProjectDirectory)`, `$(MSBuildThisFileDirectory)` or `$(ProjectDir)` and stop at the first segment needing evaluation.

Reported, never promoted (`unresolvedReferences`): `runtime-loader-identity-unresolved` (non-literal argument), `runtime-plugin-identity-not-found`, `runtime-plugin-identity-ambiguous`, `runtime-plugin-copy-unproven`, `plugin-copy-without-identified-loader`. Loads in test projects become `testReferences` on the plugin and never create edges or ownership.

Runtime edges participate in ownership, so a delivered and identified plugin becomes `OWNED` by its host.

## Bounded reads

The probe reads only production host and test project source files, 1 MiB per-file cap, line by line; libraries and plugins are never read. `compositionProbes` records the number of files probed per component. Loader contents are not stored; only file paths and literal assembly identities appear in the local profile.

## Regression coverage

`tests/Pkc.CSharp.Tests/RepositoryDiscoveryRuntimePluginRegressionTests.cs` — web host with a loader file; plugin with redirected `OutputPath` loaded by `.dll` path; plugin with custom `AssemblyName` copied by an MSBuild `Copy` task and loaded by name; plugin loaded by `new AssemblyName(...)` but never delivered; plugin delivered but only mentioned in a commented-out line; non-literal load; unknown identity; test project loading a plugin.

| RD4 requirement | Test |
| --- | --- |
| provenance-bearing runtime edge | `Plugins_loaded_by_identity_and_copied_into_the_host_get_provenance_bearing_runtime_edges` |
| copy without loader → no edge; loader without identity → UNKNOWN | `Missing_loader_identity_or_copy_provenance_never_creates_an_authoritative_edge` (mutation dropping delivery proof is caught by 2 tests) |
| test-only load → test evidence | `Test_only_plugin_loads_are_test_evidence_not_production_edges` |
| bounded probes, determinism, privacy | `Composition_probes_are_bounded_recorded_and_deterministic` |

## Verification

```text
dotnet build PKC.sln -c Release                      0 warnings, 0 errors
dotnet test PKC.sln -c Release --no-build            Pkc.CSharp.Tests 317/317, Pkc.Frontend.Tests 23/23
focused discovery + CLI ordering tests               26/26
mutation (delivery proof dropped)                    caught by 2 tests
```

Sample parity: `pkc run` on copies of both samples with the RD4 CLI produced `.pkc` output identical to `ab24363` except for `.pkc/discovery/`.

## Known gaps carried forward

- Folder-scan loaders (`GetFiles(dir, "*.dll")` + load each) have no literal identity and stay `runtime-loader-identity-unresolved`; the demo repository may rely on this shape and RD8 must measure it.
- Post-build `xcopy`/`copy` events, loaders in shared libraries, configuration-driven plugin lists and VB sources are not interpreted (fail-closed).
- Delivery into the host tree does not check the loader's runtime folder against the copy destination.
- Push/CI still pending from the implementing environment.
