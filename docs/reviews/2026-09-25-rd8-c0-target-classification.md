# RD8-C Phase C0 — Real-Target Runtime-Plugin Classification and Repair Spec

Date: 2026-09-25
Checkpoint: V0.4.7-E0 / RD8-C
Status: PHASE C0 COMPLETE / REPAIR AUTHORIZED (regression-first) / RD8-C NOT PASS

Authoritative acceptance state remains `docs/status.md`, `docs/handoff.md` and `docs/v0.4.7-acceptance-plan.md`.
Decision authority: `docs/reviews/2026-09-25-rd8-runtime-plugin-support-matrix.md`.

## Privacy boundary

This record was produced by read-only inspection of the approved private target in the source-enabled company environment. It contains only sanitized shape classifications, counts and PKC-side code references. It contains no target project names, paths, assembly identities, source bodies or configuration values. Placeholders below (`<H>`, `<P>`, `<M>`) are sanitized stand-ins, not real names.

The implementer does **not** need, and must not request, target source. Everything required is the synthetic shape below.

## PKC candidate inspected

```text
main  ac1b8294d8f3ce183802dd6d4e36349fc67c2722  docs: lock RD8 runtime plugin repair authority [skip ci]
latest production ancestor  fa30e3eb140357da5453702090be307417abb4e2
```

Target discovery output inspected: the existing `.pkc/discovery/repository-profile.json` produced on 2026-09-24 after RD4 (`a37d936`) merged. A fresh current-main discover on a disposable copy has not been run yet; the zero result below is fully explained by current code (see "Root cause"), so the classification does not depend on that rerun.

## Phase C0 checklist result

```text
A. loader ownership
   [x] host own project            (one production WEB_HOST, loader in its own C# file)

B. enumeration / identity
   [x] folder scan + variable file path
       shape: enumerate subdirectories of <runtime-base>/<M>/,
              load <subdir>/<subdir-name>.dll for each existing file
       NOT the GetFiles(..., "*.dll") shape

C. load API family
   [x] Assembly.LoadFrom           (passed as a METHOD GROUP, no call parenthesis)

D. delivery syntax
   [x] MSBuild <Copy>              (inside a plugin-owned <Target AfterTargets="Build">)
       SourceFiles = @(Item), Item defined by an <ItemGroup> inside the same Target
                     with Include = $(TargetDir)\**\*.*
       DestinationFolder = $(ProjectDir)..\..\..\<H>\$(OutDir)<M>\$(ProjectName)\%(RecursiveDir)
       additional unconditional copies of the same output into several TEST projects' $(OutDir)<M>\$(ProjectName)\
       additional Condition="$(<external-dest>) != ''" copies to $(<external-dest>)\<M>\$(ProjectName)\
       2 plugin projects use this shape; neither declares <AssemblyName> (default = project file name)
       neither plugin is ProjectReference'd by the host

E. solution dependency
   [x] present and relevant        (.slnx <BuildDependency> entries, 4, host -> plugin-side projects)
       build provenance only; NOT required for the runtime edge; do not implement now

F. current PKC result
   runtime-plugin-load edges:               0
   runtime-loader-identity-unresolved:      0
   runtime-plugin-identity-not-found:       0
   runtime-plugin-identity-ambiguous:       0
   runtime-plugin-copy-unproven:            0
   plugin-copy-without-identified-loader:   0
   other unresolved:                        project-file-not-found 1 (unrelated)
   all 205 edges are project-reference
```

## Root cause of the all-zero result

Two independent gaps; each alone suppresses even the unresolved signal.

1. **Loader never observed.** `LoaderCall` requires `\s*\(` immediately after the API name (`src/Pkc.Core/Discovery/RepositoryDiscovery.Runtime.cs:24-26`). A method group such as `.Select(Assembly.LoadFrom)` does not match, so no `RuntimeLoaderFinding` is produced and `runtime-loader-identity-unresolved` (`src/Pkc.Core/Discovery/ComponentGraph.cs:405-410`) never fires.
2. **Delivery never observed.** `OutputDelivery` accepts a plugin `<Copy>` only when `SourceFiles` textually contains an own-output token such as `$(TargetDir)` (`src/Pkc.Core/Discovery/ComponentGraph.cs:373-374`, `:508-513`). `SourceFiles="@(Item)"` hides the token behind a target-local item, so `plugin-copy-without-identified-loader` (`:457-468`) never fires. `CopyTaskRecord` stores only raw `SourceFiles`/`DestinationFolder` (`:7`, `:82-84`); item definitions and `Condition` are not captured.

The destination literal prefix already resolves into the host tree: `ResolveLiteralPrefix` strips `$(ProjectDir)` and stops at `$(OutDir)` (`:530-549`), and `IntoHost` then holds (`:498-501`).

## Repair decision

Per the support-matrix decision table row "target is folder-scan loader and current main leaves identity unresolved": **regression-first minimum generic repair is authorized.**

All three parts below are required by the same real edge; this is not speculative feature bundling. `.slnx BuildDependency`, `PostBuildEvent`, `<Exec>`/xcopy, `GetFiles("*.dll")`, shared-library loaders and config-driven identities remain **out of scope**.

### R1 — observe method-group path loads

Also recognise `Assembly.LoadFrom` / `Assembly.LoadFile` / `Assembly.UnsafeLoadFrom` used as a method group (API name not followed by `(`), in the same probed files. A method group carries no literal identity: it yields a finding with `Identity = null` unless R2 composes one. Do not add `Assembly.Load` method groups (overload/identity semantics differ; not required by the target).

Staged expectation on the target shape with R1 alone: `runtime-loader-identity-unresolved = 1`.

### R2 — subdirectory-named folder-scan loader composition

Within one probed host C# file, recognise all of:

```text
1. runtime base:  Path.Combine(AppContext.BaseDirectory | AppDomain.CurrentDomain.BaseDirectory, "<M>")
                  with <M> a single literal relative segment (or literal relative path)
2. enumeration:   Directory.GetDirectories(<value from 1>)          (EnumerateDirectories is equivalent)
3. convention:    Path.Combine(<dir>, $"{Path.GetFileName(<dir>)}.dll")   (string.Concat / + ".dll" equivalents optional)
4. load:          supported path-load API, call or method group (R1)
```

Result: a loader finding carrying a deterministic scan directory `<M>` and convention `subdirectory-named-assembly`, instead of a literal identity. Missing any of 1-4, a non-literal `<M>`, or multiple distinct `<M>` in the file → keep `Identity = null` (existing `runtime-loader-identity-unresolved`).

Implementation latitude: the probe is line-based today. The implementer chooses the narrowest reliable composition (for example: all four patterns within one file, or within one type/member span). Whatever is chosen must be covered by the negative regressions below. Do not use data-flow guessing across files.

### R3 — target-local item indirection in `<Copy>`

When a `<Copy SourceFiles="@(X)">` sits inside a `<Target>`, and that same Target contains an `<ItemGroup>` defining `X` whose `Include` contains an own-output token (`OwnOutputTokens`), treat the copy source as the plugin's own output. Resolve only same-Target, same-file items. Items from other targets, imports, `Exclude`/`Remove` manipulation, or multiple conflicting definitions → not resolved (fail closed).

Capture `Condition` on the `<Copy>` (and its Target/ItemGroup ancestors). Conditional copies are never delivery authority.

Staged expectation on the target shape with R3 alone: `plugin-copy-without-identified-loader = 2` (the host only; test-project destinations are not host kinds).

### R4 — compose folder-scan loader with delivery (the HIGH edge)

For a host loader from R2 with scan directory `<M>` and a plugin `<P>`, emit `runtime-plugin-load` HIGH only when all hold:

```text
a. plugin has an unconditional R3 (or already-supported) own-output <Copy>;
b. its DestinationFolder resolves literally into the host project directory,
   followed by exactly $(OutDir), then literal <M> (normalized, case-insensitive), then $(ProjectName),
   optionally followed by %(RecursiveDir) and nothing else;
c. $(ProjectName) (plugin project file name) equals the plugin's literal AssemblyName,
   so <subdir>/<subdir>.dll is exactly the plugin's own output assembly;
d. that AssemblyName is unique across the repository (existing ambiguity rule);
e. neither host nor plugin sets a literal OutputPath/OutDir/BaseOutputPath that would make
   plugin-side $(OutDir) differ from the host runtime base (if either overrides it: unresolved);
f. loader is in a production host (test-project loaders stay test evidence);
g. evidence records the loader file, plugin manifest, and the copy with a distinct kind,
   e.g. "msbuild-copy-into-host-scan-directory" (proposed name, does not exist today).
```

If R2 loader and R3 delivery both exist but any of b-e fails → an unresolved reference with a distinct reason, e.g. `runtime-plugin-scan-directory-mismatch` (proposed name, does not exist today). Never promote from name similarity, folder proximity, copy-only, loader-only, or solution build dependency.

Expected result on the real target after R1-R4: `runtime-plugin-load = 2`, host → each of the two plugins; no new unresolved noise from the test-project copies.

## Required regressions

Add to `tests/Pkc.CSharp.Tests/RepositoryDiscoveryRuntimePluginRegressionTests.cs` (existing RD4 file), using a synthetic repository reproducing the sanitized shape above with invented names.

Write them red first and confirm current main fails the positive case for the intended reason.

Positive:

- exact sanitized shape (method-group `LoadFrom`, subdirectory-named convention, target-local item `<Copy>` into `<host>/$(OutDir)<M>/$(ProjectName)/%(RecursiveDir)`) → one HIGH `runtime-plugin-load` per plugin, host ownership, deterministic evidence order;
- same shape with a direct `LoadFrom(...)` call instead of a method group → same edge.

Negative (each must produce no edge):

- directory enumeration without any assembly load;
- method-group load without the folder-scan composition → `runtime-loader-identity-unresolved`;
- delivery without a loader → `plugin-copy-without-identified-loader`;
- delivery into `<host>/$(OutDir)<other>/` (different scan directory);
- destination missing `$(ProjectName)` or using a literal subfolder name;
- plugin `AssemblyName` ≠ project file name;
- duplicate assembly identity across two projects;
- `@(X)` defined outside the Copy's Target, or defined with a non-own-output Include;
- conditional `<Copy>` only;
- plugin or host overriding `OutputPath`/`OutDir`;
- loader only in a test project (test evidence, not production edge);
- non-literal `<M>` (variable/config) → unresolved;
- same-name/similar-name folders without copy proof.

Existing RD4 regressions must stay green unchanged.

## Verification before commit

```text
focused:  dotnet test tests/Pkc.CSharp.Tests --filter "FullyQualifiedName~RuntimePlugin"
related:  discovery / scan-plan / coverage test classes
full:     dotnet test (whole solution) + Release build
diff review; byte-stable discovery output for unchanged fixtures
one coherent implementation commit on a topic branch; do not push to main directly
```

## After the implementation lands

Approved-environment operator only (implementer does not touch the private target):

```text
build PKC Release at the new commit
pkc discover <disposable copy of approved target>
record sanitized counts for section F
expect runtime-plugin-load = 2 and no new unexplained unresolved reasons
```

RD8-C passes only on that real-target discovery proof. RD8-A (fresh full run, no `--resume`) and RD8-B (Level-1 probes) remain required afterwards.
