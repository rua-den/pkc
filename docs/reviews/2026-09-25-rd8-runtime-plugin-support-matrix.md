# RD8 Runtime-Plugin Support Matrix and Repair Authority

Date: 2026-09-25
Checkpoint: V0.4.7-E0 / RD8-C
Status: REPO-LOCAL AUDIT / OPERATOR DECISION AID / NO ACCEPTANCE CLAIM

## Purpose

This document freezes what current `main` can and cannot prove about runtime-loaded .NET plugins before any real-target repair is attempted.

It exists to prevent two failure modes:

1. guessing a private-repository implementation shape from a high-level architectural summary;
2. weakening RD4 fail-closed authority merely to make the intended demo repository produce a runtime edge.

Authoritative acceptance state remains `docs/status.md`, `docs/handoff.md` and `docs/v0.4.7-acceptance-plan.md`.

## Candidate audited

```text
06f0f01f522650db7b2304a4a8295d2d5b118724
docs: correct RD8 runtime plugin target applicability [skip ci]
```

Latest production-code ancestor remains:

```text
fa30e3eb140357da5453702090be307417abb4e2
fix: report the preserved workspace when it could not replace the current one
```

No production code was changed for this audit.

## Real-target fact already established

The saved sanitized reconnaissance proves the intended repository class has this topology:

```text
production host
  -> reflection-based runtime plugin loading from an output subfolder
  -> plugin projects are not project-referenced by the host

plugin delivery
  -> custom post-build copy into runtime output
```

The reconnaissance also calls out solution/build dependency declarations and post-build copy targets as evidence relevant to reconstructing the plugin relationship.

What is still missing is the exact syntax family used by the private target. The sanitized reconnaissance intentionally does not expose proprietary source bodies.

Therefore RD8-C applicability is `YES`, but current-main real-target proof is still `MISSING / NOT PROVEN`.

## Current-main support matrix

### Loader evidence

| Shape | Current main | Authority |
| --- | --- | --- |
| `Assembly.Load("Plugin.Identity")` | supported | may provide literal identity |
| `Assembly.LoadFrom(... "Plugin.dll" ...)` | supported when the call itself contains a literal `.dll` name | may provide literal identity |
| `Assembly.LoadFile` / `Assembly.UnsafeLoadFrom` with literal `.dll` | supported | may provide literal identity |
| `AssemblyLoadContext.LoadFromAssemblyName` with literal name | supported | may provide literal identity |
| `AssemblyLoadContext.LoadFromAssemblyPath` with literal `.dll` | supported | may provide literal identity |
| `Assembly.Load(new AssemblyName(...))` | loader call may be seen, but no direct string-literal identity is promoted from the nested object construction | unresolved unless another supported literal occurs in the call arguments in the accepted form |
| variable/configured path passed to a loader | observed loader, identity unresolved | fail closed |
| folder scan such as `GetFiles(..., "*.dll")` then load each variable path | not composed into plugin identities | fail closed |
| loader implemented in a shared library rather than the production host's own project | not probed by RD4 | fail closed |
| VB/config-driven/custom loader API | not interpreted | fail closed |

The runtime probe is intentionally bounded to own C# files of production hosts and test projects, with a per-file cap. Test-project loads remain test evidence and never create production ownership.

### Plugin identity evidence

| Shape | Current main | Authority |
| --- | --- | --- |
| unique literal project `AssemblyName` | supported | deterministic candidate identity |
| default assembly name from project file name | supported | deterministic candidate identity |
| two projects with same matching assembly identity | detected ambiguous | fail closed |
| loader identity has no matching project | detected missing | fail closed |
| assembly name depends on unevaluated MSBuild property | identity unresolved | fail closed |
| similar project/folder/module name | ignored | never authority |

### Delivery evidence

| Shape | Current main | Authority |
| --- | --- | --- |
| plugin `OutputPath` / `OutDir` / `BaseOutputPath` literal prefix resolves inside host tree | supported | delivery evidence |
| plugin MSBuild `<Copy>` from its own output into host tree | supported | delivery evidence |
| host MSBuild `<Copy>` from plugin tree into host tree | supported | delivery evidence |
| unconditional host `ProjectReference` to plugin | supported as output-delivery fallback | delivery evidence, although intended target is known not to use this topology |
| `PostBuildEvent` shell command | not interpreted | fail closed |
| MSBuild `<Exec Command="copy ...">` / `xcopy` / `robocopy` | not interpreted | fail closed |
| Cake/pipeline/script copy outside supported project XML metadata | not interpreted for RD4 edge authority | fail closed |
| destination that cannot be resolved beyond MSBuild properties/items | unresolved | fail closed |

### Solution evidence

Current `.sln` / `.slnx` discovery records which projects are members of the solution. It does **not** currently parse or model solution-level `ProjectDependencies` as dependency provenance.

Even if the real target uses `ProjectSection(ProjectDependencies)`, that relation must not by itself create runtime authority. At most it can prove build ordering/inclusion or help connect already-proven loader and delivery evidence.

## Current authoritative edge rule

Current RD4 production creates `runtime-plugin-load` HIGH only when all of these hold:

```text
production host loader
+ unique literal assembly identity
+ deterministic delivery of that plugin output into the host tree
```

Missing evidence is surfaced rather than guessed. Existing unresolved outcomes include:

```text
runtime-loader-identity-unresolved
runtime-plugin-identity-not-found
runtime-plugin-identity-ambiguous
runtime-plugin-copy-unproven
plugin-copy-without-identified-loader
```

This boundary remains accepted and must not be weakened while RD8-C investigates the target-specific gap.

## Minimum safe authority for a folder-scan repair

If source-enabled inspection confirms the intended target uses a folder-scan loader, a future generic repair must **not** infer plugin identity merely because a plugin project exists or is copied somewhere nearby.

A HIGH runtime edge should require deterministic composition equivalent to:

```text
1. host runtime code proves it enumerates a specific runtime directory / file pattern for assemblies;
2. host runtime code proves enumerated files are passed into a supported assembly-load operation;
3. plugin project has a unique deterministic assembly identity;
4. build metadata proves that exact plugin output is delivered into the loader's enumerated runtime directory;
5. loader directory and delivery destination can be normalized to the same deterministic location;
6. test-only loaders remain test evidence;
7. ambiguous identity, unresolved directory, broad/unknown copy, or directory mismatch remains unresolved/UNKNOWN.
```

A solution-level build dependency may strengthen build provenance, but it is not runtime-use evidence by itself.

## Minimum safe authority for post-build command support

If source-enabled inspection confirms delivery uses `PostBuildEvent` or `<Exec>` shell copy commands, a future parser must stay narrow:

- accept only deterministic literal copy source/destination shapes that can be normalized safely;
- require source to resolve to the candidate plugin's own output, not merely a same-named DLL somewhere in the repository;
- require destination to resolve into the proven loader runtime directory;
- do not execute shell commands;
- do not expand arbitrary environment variables, command substitution, wildcards or conditionals into authority;
- unsupported command syntax remains unresolved/UNKNOWN.

## Operator checklist for Phase C0

The approved source-enabled operator only needs to return a sanitized classification, not proprietary source:

```text
A. loader ownership
   [ ] host own project
   [ ] shared library
   [ ] other / unknown

B. enumeration / identity
   [ ] direct literal assembly name
   [ ] direct literal .dll path
   [ ] folder scan + variable file path
   [ ] config/list driven
   [ ] other / unknown

C. load API family
   [ ] Assembly.Load
   [ ] Assembly.LoadFrom / LoadFile / UnsafeLoadFrom
   [ ] AssemblyLoadContext path/name API
   [ ] custom wrapper / other

D. delivery syntax
   [ ] OutputPath / OutDir / BaseOutputPath
   [ ] MSBuild <Copy>
   [ ] PostBuildEvent
   [ ] MSBuild <Exec> copy/xcopy/robocopy
   [ ] external build script / other

E. solution dependency
   [ ] ProjectDependencies present and relevant
   [ ] present but unrelated
   [ ] absent

F. current PKC result
   runtime-plugin-load edges: <count>
   runtime-loader-identity-unresolved: <count>
   runtime-plugin-copy-unproven: <count>
   plugin-copy-without-identified-loader: <count>
   other relevant unresolved reason: <sanitized reason + count>
```

No project names, absolute paths, source snippets, config values or assembly names are required in the committed evidence.

## Regression decision table

After Phase C0, choose exactly one path:

| Observation | Action |
| --- | --- |
| exact target shape already falls inside current support matrix and PKC produces correct edge | no production change; capture real-target proof and continue RD8-C |
| target is folder-scan loader and current main leaves identity unresolved | synthetic red regression for exact sanitized folder-scan composition, then minimum generic repair |
| target delivery is unsupported post-build command | synthetic red regression for exact sanitized command form, then minimum generic repair |
| target requires solution `ProjectDependencies` only for build provenance | add only the minimum build-provenance parser needed by the exact shape; never treat solution dependency alone as runtime authority |
| exact target syntax remains ambiguous | no production change; keep RD8-C blocked and collect the smallest missing syntax fact |

Do not combine speculative folder-loader, shell-copy and solution-dependency features in one repair unless the real target demonstrably requires all of them for the same edge.

## Required regression properties for any repair

Positive regression:

- sanitized exact target shape produces the intended runtime edge and host ownership.

Negative regressions must prove at least:

- folder enumeration without assembly load -> no edge;
- assembly load without deterministic delivery -> no edge;
- delivery without proven loader -> no edge;
- same/similar names without identity proof -> no edge;
- ambiguous assembly identity -> no edge;
- delivery to a different runtime directory -> no edge;
- test-only loader -> test evidence, never production edge;
- unsupported dynamic command/config expansion -> unresolved/UNKNOWN.

Then run:

```text
focused runtime-plugin regressions
-> related discovery/scan-plan tests
-> full relevant local suite
-> diff review
-> one coherent implementation commit/push
-> CI final verification
-> rerun only affected RD8-C real-target evidence
```

## Decision

```text
RD4 deterministic/synthetic gate: PASS / remains closed
RD8-C intended-target applicability: YES
RD8-C real-target proof: MISSING / NOT PASS
production repair authorized now: NO — exact target syntax classification still required
E1: LOCKED
```

Exact next action is the small Phase-C0 operator checklist above. Once its sanitized result exists, the next implementation choice is deterministic rather than speculative.
