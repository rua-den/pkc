# PKC Handoff

Last updated: 2026-09-25

## Read first

1. root `CLAUDE.md`
2. `AGENTS.md`
3. `docs/status.md`
4. this handoff
5. `docs/milestones.md`
6. `docs/product-knowledge-contract.md`
7. `docs/v0.4.7-acceptance-plan.md`
8. `docs/plans/2026-09-24-demo-scan-priority-override.md`
9. `docs/plans/2026-09-24-demo-critical-sequential-execution-plan.md`
10. `docs/plans/2026-09-24-rd8-current-main-validation-runbook.md`
11. `docs/reviews/2026-09-25-rd8-runtime-plugin-support-matrix.md`
12. `docs/benchmarks/product-value-benchmark-protocol.md`
13. `docs/reviews/2026-09-24-rd8-runtime-plugin-real-repo-evidence-audit.md`
14. `docs/reviews/2026-09-24-rd8-runtime-plugin-target-applicability.md`
15. `docs/reviews/2026-09-25-rd8-c0-target-classification.md` — **implementation spec for the next task**

Then verify current `main`, recent commits, production code and relevant regressions. Never reset to an older SHA merely because a handoff names one.

## Repository state at this handoff

Current `main` immediately before this Phase-C0 checkpoint:

```text
ac1b8294d8f3ce183802dd6d4e36349fc67c2722
docs: lock RD8 runtime plugin repair authority [skip ci]
```

Latest production-code ancestor remains:

```text
fa30e3eb140357da5453702090be307417abb4e2
fix: report the preserved workspace when it could not replace the current one
```

This handoff update is docs/audit only. It does not change production behavior.

## Current checkpoint state

```text
V0.4.7-D / R7.10                    OPEN / REVIEW PAUSED / NOT ACCEPTED
V0.4.7-E0                           ACTIVE / USER-AUTHORIZED BOUNDED PREWORK
RD1 inventory + safe exclusion      PASS / COMPLETE
RD2 application boundaries          PASS / COMPLETE
RD3 vendor/custom frontend          PASS / COMPLETE
RD4 runtime/plugin provenance       PASS / COMPLETE (deterministic/synthetic gate)
RD5 deterministic ScanPlan          PASS / COMPLETE
RD6 scoped semantic execution       PASS / COMPLETE
RD7 coverage + observability        PASS / COMPLETE
RD8 private large-repository gate   ACTIVE / C0 CLASSIFIED / REPAIR AUTHORIZED / NOT PASS
E1 remaining semantic repairs       LOCKED behind E0
E2 final product acceptance         LOCKED behind E1
```

## Merged production work relevant to RD8

```text
612fa998da6983662a90eed2836714c9928243b5
feat: make large runs failure-safe, resumable and self-describing

48ce2f10c1e506b28e37aa3ff15ba5e241a3da0c
feat: follow sole implementations and surface mapped-field, gate and guard rules

fa30e3eb140357da5453702090be307417abb4e2
fix: report the preserved workspace when it could not replace the current one
```

These provide streamed/atomic writes, shared `MSBuildWorkspace` use, checkpoint/`--resume`, staged workspace replacement, run summaries, fail-closed sole-implementation traversal, mapped-field/gate/guard rules and correct `.pkc/workspace.new` reporting.

## RD8 supporting evidence

Historical approved private exercise remains supporting evidence only:

```text
repository scale: about 26.9k files / 18 hosts
planned semantic files: about 16.8k
facts / relations: about 263.6k / 2.04M
workflow candidates: 4,186

run 1: ~41.5 min / ~18.6 GB / OOM during artifact write
run 2 with resource repairs: ~29 min / ~7.5 GB / exit 0
pkc discover: about 98 s
run-2 workspace: about 703 product features / 4,709 files
```

The successful run predates the fully integrated current production state and cannot close RD8-A or RD8-B.

## Intended target runtime topology

A saved sanitized reconnaissance proves the intended large mixed repository has runtime plugin modules with this shape:

```text
production host
  -> reflection-based runtime load from an output subfolder
  -> plugin projects not project-referenced by the host

plugin delivery
  -> custom post-build copy into runtime output
```

The reconnaissance also names solution/build-dependency declarations and post-build copy targets as relevant evidence for the relationship.

Therefore:

```text
runtime/plugin applicability on intended target: YES
accepted N/A path:                            CLOSED
previous PKC runtime-edge yield:              MISSING / NOT PROVEN
RD8-C:                                        OPEN / KNOWN BLOCKER
```

## Repo-local RD8-C audit completed

Current production and RD4 regressions were re-audited before any repair.

Current support:

```text
loader:
  supported Assembly.Load / LoadFrom / LoadFile / UnsafeLoadFrom
  supported AssemblyLoadContext name/path load APIs
  identity promoted only from accepted direct string-literal name/.dll evidence

delivery:
  OutputPath / OutDir / BaseOutputPath literal prefix into host
  supported MSBuild <Copy> from plugin output or host copy from plugin tree
  unconditional host ProjectReference fallback

identity:
  unique literal AssemblyName or default project-file assembly name
  ambiguous/missing identity fails closed
```

Known fail-closed gaps carried by RD4 and relevant to the intended target class:

```text
folder scan GetFiles(..., "*.dll") -> variable-path load
PostBuildEvent / Exec copy / xcopy / robocopy
loader in shared library
configuration-driven identities
VB/custom loader shapes
solution-level ProjectDependencies provenance
```

The current solution parser records solution project membership only. It does not parse `.sln` `ProjectSection(ProjectDependencies)` into dependency evidence.

Full matrix and repair authority:

`docs/reviews/2026-09-25-rd8-runtime-plugin-support-matrix.md`

## Phase C0 result (2026-09-25) — repair now authorized

Read-only inspection in the approved source-enabled environment classified the real target. Sanitized result:

```text
A. loader ownership      production web host, own project
B. identity/enumeration  folder scan: subdirectories of <runtime-base>/<M>/, load <subdir>/<subdir-name>.dll
C. load API              Assembly.LoadFrom as a METHOD GROUP (no call parenthesis)
D. delivery              plugin <Copy> in AfterTargets=Build target; SourceFiles=@(target-local item = $(TargetDir)**);
                         DestinationFolder=<host>/$(OutDir)<M>/$(ProjectName)/%(RecursiveDir); 2 plugins
E. solution dependency   .slnx BuildDependency present/relevant; build provenance only; not needed now
F. current PKC           0 runtime-plugin-load; 0 for every runtime unresolved reason
```

Why zero: `LoaderCall` requires `(` after the API (`src/Pkc.Core/Discovery/RepositoryDiscovery.Runtime.cs:24-26`), and `OutputDelivery` needs an own-output token literally in `SourceFiles` (`src/Pkc.Core/Discovery/ComponentGraph.cs:508-513`). Both hide even the unresolved signal.

Full spec — R1 method-group loads, R2 folder-scan composition, R3 target-local item copies, R4 HIGH composition rule, positive/negative regressions, verification:

`docs/reviews/2026-09-25-rd8-c0-target-classification.md`

The implementer needs no private-target access and must not request target source; the synthetic shape in that spec is sufficient.

Never promote runtime authority from name similarity, folder proximity, solution build order, copy-only evidence or loader-only evidence. Do not add `.sln`/`.slnx` dependency parsing, `PostBuildEvent`, `<Exec>`/xcopy, `GetFiles("*.dll")`, shared-library or config-driven loader support in this repair.

## RD8-A — still required

Inside the approved source-enabled/company environment against an approved disposable target copy:

```text
build PKC Release at current main
pkc discover <target>
pkc run <target>             # fresh; no --resume
```

Capture sanitized elapsed time, peak process-tree memory, coverage, facts/relations/workflow/product counts, artifact failures, final workspace path and file count. Verify summaries, coverage, checkpoint and workspace plausibility.

## RD8-B — still required

After the fresh workspace exists, run 2-3 Level-1 probes using `docs/benchmarks/product-value-benchmark-protocol.md`:

```text
phase 1 workspace-only
phase 2 source-known cross-check
phase 3 compare and score
```

Include the quantity-adjustment probe and one or two materially different questions from `docs/question-trainning.md`.

## E0 completion rule

Do not mark E0 PASS until all are true:

```text
RD1-RD7 PASS / COMPLETE
RD8-A fresh current-main run completes practically
workspace + summaries/coverage are plausible
RD8-B targeted Level-1 answers are useful and correctly uncertain
RD8-C applicable runtime-plugin topology has deterministic real-target proof
```

Only then unlock E1.

## Parked work

Do not merge during RD8:

```text
branch  fix/product-value-construction-state
commit  35c8e5c5f856e15568aa963bb2d76268008c5570
```

## Formal D boundary

R7.10/D remains OPEN. The independent-review lane is paused, not passed. A fresh independent review is still required before final V0.4.7 acceptance.

## Exact next action

```text
implementer:
  read docs/reviews/2026-09-25-rd8-c0-target-classification.md
  -> red synthetic regressions in tests/Pkc.CSharp.Tests/RepositoryDiscoveryRuntimePluginRegressionTests.cs
  -> implement R1-R4 minimum generic fail-closed repair
  -> focused + related + full local verification, Release build
  -> one coherent implementation commit on a topic branch (never push main directly)

approved-environment operator, after it lands:
  pkc discover on a disposable copy of the target
  -> expect runtime-plugin-load = 2, record sanitized counts = RD8-C proof

then:
  fresh RD8-A run without --resume
  -> RD8-B Level-1 probes
  -> close RD8 only when A+B+C all pass
```
