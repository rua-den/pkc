# RD3 Vendor/Custom Frontend Classification — Local PASS Evidence

Date: 2026-09-24
Checkpoint: V0.4.7-E0 / RD3
Decision: **LOCAL PASS** (implementation self-verified; push/CI pending with RD1/RD2)

## Implementation

```text
ac3efd9f2b3f26bd99ab6cf141b8716325165578
feat: classify vendor and generated frontend files in discovery
```

Parent chain: `2ebce42` (RD2 docs) → `05eadb1` (RD2) → `26a7f59` → `fd3428f` (RD1) → `ab24363`.

Code:

- `src/Pkc.Core/Discovery/RepositoryDiscovery.Frontend.cs` — NuGet content comparison, LibMan destinations, bundleconfig provenance, source-map provenance, file-level decisions with precedence.
- `src/Pkc.Core/Discovery/RepositoryProfile.cs` — roles `THIRD_PARTY_RUNTIME`, `THIRD_PARTY_MODIFIED`, `THIRD_PARTY_UNKNOWN`; scan mode `RUNTIME_DEPENDENCY_INDEX`; `GeneratedArtifact`; `byteComparisons`; schema `0.3.0-discovery`.
- `src/Pkc.Core/Discovery/RepositoryDiscovery.cs` — `*.map` manifest kind `source-map`; RD3 pass runs after the walk, before classification counts.
- `src/Pkc.Cli/Program.cs` — `runtime-index` count in `[pkc:discover] Scope`.

## Evidence rules

| Classification | Required proof |
| --- | --- |
| `THIRD_PARTY_RUNTIME` / `RUNTIME_DEPENDENCY_INDEX` (file) | `packages.config` package restored in exactly one RD1-proven package folder; project file at the package `Content/` relative path and byte-identical (size + SHA-256) |
| `THIRD_PARTY_MODIFIED` / `DEEP_SCAN` (file carve-out) | same declared content path, bytes differ |
| `THIRD_PARTY_RUNTIME` / `RUNTIME_DEPENDENCY_INDEX` (directory) | LibMan library `destination` (or `defaultDestination`) with a non-empty `files` list; destination exists and is not inside an excluded area |
| `UNKNOWN` / `DEEP_SCAN` (file carve-out) | file inside such a destination but not in any declared `files` list |
| `THIRD_PARTY_UNKNOWN` / `DEEP_SCAN` (directory) | LibMan destination without a declared file set — locally added files cannot be told apart |
| `GENERATED_OR_RESTORABLE` / `LIGHT_INDEX` + `bundle-output` artifact | `bundleconfig.json` `outputFileName`; inputs from `inputFiles` (globs, `!` exclusions); missing inputs recorded as unresolved |
| `GENERATED_OR_RESTORABLE` / `LIGHT_INDEX` + `source-map-output` artifact | sibling `<file>.map` / `<name>.map` whose `file` matches (if present) and at least one `sources` entry resolves to another repository file |

Precedence per file: carve-out (modified/undeclared) > declared vendor > generated. Package transforms (`.pp`, `.transform`, `.install.xdt`, `.uninstall.xdt`) are never compared. Unrestored or ambiguously restored packages and LibMan libraries without a destination become `unresolvedReferences`.

Non-authority: directory names, license/version banners, `.min.js` names without a resolvable map, and byte-identical copies at undeclared paths.

Bounded reads: `packages.config`, `libman.json`, `bundleconfig.json`, `*.map` are parsed as manifests (4 MiB cap). Byte comparison reads only files at declared NuGet content paths and their package counterparts (16 MiB cap), listed in `byteComparisons`. Only equality is recorded, never contents or hashes.

## Regression coverage

`tests/Pkc.CSharp.Tests/RepositoryDiscoveryFrontendClassificationRegressionTests.cs` — legacy MVC project with restored jQuery / jQuery.Validation / WebGrease packages and an unrestored Modernizr package; untouched and locally patched NuGet scripts; vendor minified file + map; first-party page script with minified output and map; wrapper script; look-alike copy, bannered vendor-looking file and `.min.js` without map; LibMan library with files list plus a locally added file, library without files list, library without destination; bundleconfig mixing vendor and first-party inputs with a glob, an exclusion and a missing input.

| RD3 requirement | Test |
| --- | --- |
| runtime dependency indexing | `Declared_restore_content_is_indexed_as_third_party_runtime_not_deep_scanned` |
| vendor internals never deep-scanned because runtime-used | same (bundle input stays `RUNTIME_DEPENDENCY_INDEX`) |
| narrow modified-vendor carve-out | `Locally_modified_or_added_vendor_files_get_a_narrow_first_party_carve_out` (mutation forcing "identical" is caught) |
| bundle + minified provenance, generated file-level scan set | `Generated_bundles_and_minified_outputs_carry_input_provenance` |
| no name/banner/copy authority | `Names_banners_and_copied_content_never_create_vendor_authority` |
| unresolved declarations, determinism, privacy, bounded reads | `Unrestorable_declarations_are_reported_and_classification_is_deterministic` |

The RD1 regression's content-read allowlist was widened to the new manifest kinds (still no source/config reads).

## Verification

```text
dotnet build PKC.sln -c Release                      0 warnings, 0 errors
dotnet test PKC.sln -c Release --no-build            Pkc.CSharp.Tests 313/313, Pkc.Frontend.Tests 23/23
RD1 + RD2 + RD3 + CLI ordering focused tests         22/22
mutation (NuGet content always identical)            caught
```

Sample parity: `pkc run` on copies of `samples/WorkPlaySample` and `samples/PokeTradeSystem` with the RD3 CLI produced `.pkc` output identical to `ab24363` except for `.pkc/discovery/`.

## Known gaps carried forward

- MVC `BundleConfig.cs` virtual bundles, gulp/grunt/webpack scripts, bower and vendored npm packages are not classified (they need composition-source or tool-specific evidence); affected files stay included.
- SDK-style `PackageReference` `contentFiles` are linked rather than copied and are not compared.
- Wrapper/adapter relationships are not modeled; wrappers stay `DEEP_SCAN` as first-party-included files.
- Push/CI still pending from the implementing environment.
