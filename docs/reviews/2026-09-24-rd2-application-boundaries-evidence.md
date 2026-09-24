# RD2 Application Boundaries + Ownership — Local PASS Evidence

Date: 2026-09-24
Checkpoint: V0.4.7-E0 / RD2
Decision: **LOCAL PASS** (implementation self-verified; push/CI pending with RD1)

## Implementation

```text
05eadb1e44152f23bcf06134adfc89ae70572e47
feat: model application boundaries in repository discovery
```

Parent chain: `26a7f59` (RD1 docs) → `fd3428f` (RD1) → `ab24363`.

Code:

- `src/Pkc.Core/Discovery/ComponentGraph.cs` — `DotnetProjectIdentity` (manifest-only project identity) and `ComponentGraph` (edges, unresolved references, ownership).
- `src/Pkc.Core/Discovery/RepositoryProfile.cs` — `RepositoryComponent`, `ComponentKind`, `OwnershipStatus`, `ComponentEdge`, `UnresolvedReference`; profile schema `0.2.0-discovery`.
- `src/Pkc.Core/Discovery/RepositoryDiscovery.cs` — collects project/solution/Angular project records during the same single walk; `.sln`/`.slnx` added to the bounded manifest read set.
- `src/Pkc.Cli/Program.cs` — `[pkc:discover] Components: ...` progress line from real profile counts.

## Model

- Components: one per `.csproj`/`.vbproj`/`.fsproj` (`dotnet:<manifest>`) and per `angular.json` project (`angular:<manifest>#<project>`), with area path, kind, confidence, evidence, solution membership, ownership, owners and test references.
- Kinds: `WEB_HOST`, `WORKER_HOST`, `EXECUTABLE_HOST`, `LIBRARY`, `TEST_PROJECT`, `ANGULAR_APPLICATION`, `ANGULAR_LIBRARY`, `UNKNOWN`.
- Ownership: `HOST` (owns itself), `OWNED` (reached by at least one production host), `TEST_ONLY`, `UNKNOWN` (no production host provably reaches it — may be dead, runtime-loaded, or conditionally referenced; RD4 decides runtime loading).
- Area roles from RD1 are unchanged; RD2 does not assert first-party/third-party roles.

## Evidence rules

| Kind | Proof |
| --- | --- |
| `WEB_HOST` | `Microsoft.NET.Sdk.Web` with Exe output (default), or legacy ASP.NET web application / WCF service project type GUID |
| `WORKER_HOST` | `Microsoft.NET.Sdk.Worker` with Exe output (default) |
| `EXECUTABLE_HOST` | declared `OutputType` `Exe`/`WinExe` |
| `LIBRARY` | declared or default `Library` output |
| `TEST_PROJECT` | RD1 test evidence (wins over web SDK) |
| `ANGULAR_APPLICATION` / `ANGULAR_LIBRARY` | `projectType`, or, when absent, an official `@angular-devkit/build-angular:` / `@angular/build:` application or ng-packagr builder; an absent or empty project `root` means the workspace root |
| `UNKNOWN` | OutputType inherited from an unevaluated `Directory.Build.*`, conflicting or property-based OutputType, unsupported output type, custom Angular builder, undeclared project type, unresolvable project root |

Edges: `project-reference` only from resolved `ProjectReference` items; exact path match or a unique case-insensitive match. Conditional references (`Condition` on the item or an ancestor, `Choose/When/Otherwise`) are kept with `UNKNOWN` confidence and excluded from ownership. Property/item-transform includes, wildcards, out-of-repository paths and missing files become `unresolvedReferences`.

Ownership: from each production host, traverse `HIGH` edges whose source and target are not test projects. Test projects record themselves in `testReferences` of what they reference and never appear in `owners`.

## Regression coverage

`tests/Pkc.CSharp.Tests/RepositoryDiscoveryApplicationBoundaryRegressionTests.cs` — synthetic platform with SDK web API, legacy MVC (GUID), worker SDK, console tool, shared domain referenced by three hosts (one transitively), infrastructure referenced by two hosts, similarly named `Reporting`/`Reporting.Api` with no reference, conditional reference, broken references (missing and property-based), inherited OutputType, xunit test project, web-SDK test host, Angular application/library/undeclared/builder-only/custom-builder projects, `.sln` (including a solution folder and a ghost entry) and `.slnx`.

| RD2 requirement | Test |
| --- | --- |
| application identity | `Application_and_component_identity_comes_from_build_manifests` |
| multi-owner shared nodes | `Shared_modules_have_every_production_host_that_references_them_as_owner` (exact edge list) |
| test isolation | `Tests_attach_as_evidence_and_never_create_production_ownership` (mutation letting tests own is caught by 2 tests) |
| similar names never link | `Similar_names_never_create_edges_or_ownership` |
| ambiguous stays UNKNOWN | `Unresolvable_references_are_reported_instead_of_guessed`; UNKNOWN kinds in identity test |
| determinism + manifest-only reads + privacy | `Application_graph_is_deterministic_and_reads_only_manifests` |

The RD1 regression's content-read allowlist was widened to include `dotnet-solution` (still manifests only).

## Verification

```text
dotnet build PKC.sln -c Release                      0 warnings, 0 errors
dotnet test PKC.sln -c Release --no-build            Pkc.CSharp.Tests 308/308, Pkc.Frontend.Tests 23/23
RD1 + RD2 + CLI ordering focused tests               17/17
mutation (test projects as owners)                   caught by 2 tests
```

Sample parity: `pkc run` on copies of `samples/WorkPlaySample` and `samples/PokeTradeSystem` with the final RD2 CLI produced `.pkc` output identical to the pre-E0 baseline (`ab24363`) except for `.pkc/discovery/`. PokeTrade discovers 2 hosts: the API (`WEB_HOST`, web SDK) and the Angular project (`ANGULAR_APPLICATION`, official application builder, `root: ""` resolved to the workspace root). An earlier RD2 candidate left that Angular project `UNKNOWN` because the empty root was treated as unresolvable; the regression fixture now covers `root: ""`.

## Known gaps carried forward

- Frontend↔backend hosting links (SPA root, build-copy into `wwwroot`) and Angular library consumption are not modeled; Angular libraries stay `UNKNOWN` ownership.
- `Directory.Build.*`-injected `ProjectReference` items and MSBuild evaluation are not performed; such references are invisible (fail-closed: less ownership, never false ownership).
- Runtime/plugin loading (RD4) can later turn some `UNKNOWN` libraries into runtime-owned nodes.
- Push/CI still pending from the implementing environment.
