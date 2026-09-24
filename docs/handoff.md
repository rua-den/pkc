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

Then verify current `main`, recent commits, production code and relevant regressions. Never reset to an older SHA merely because a handoff names one.

## Repository state at this handoff

Current `main` immediately before this support-matrix checkpoint:

```text
06f0f01f522650db7b2304a4a8295d2d5b118724
docs: correct RD8 runtime plugin target applicability [skip ci]
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
RD8 private large-repository gate   ACTIVE / KNOWN RUNTIME-PLUGIN BLOCKER / NOT PASS
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

## Why production code is still unchanged

The repo-local audit narrows the likely gap but does not prove which unsupported syntax the private target actually uses.

Do not guess `Directory.GetFiles`, `PostBuildEvent`, `xcopy`, `<Exec>`, or `.sln ProjectDependencies` merely because those forms are plausible.

The exact source-enabled inspection now has a bounded output contract and needs no proprietary source dump.

Return only:

```text
A. loader ownership
   host own project | shared library | other/unknown

B. identity/enumeration
   direct literal name | direct literal .dll | folder scan + variable path | config/list | other

C. load API family
   Assembly.* | AssemblyLoadContext.* | custom/other

D. delivery syntax
   OutputPath | MSBuild Copy | PostBuildEvent | Exec/copy/xcopy/robocopy | external script/other

E. solution dependency
   ProjectDependencies relevant | unrelated | absent

F. current PKC result
   runtime-plugin-load edge count
   runtime-loader-identity-unresolved count
   runtime-plugin-copy-unproven count
   plugin-copy-without-identified-loader count
   any other relevant unresolved reason + count
```

No real project names, paths, source snippets, assembly identities, config values or endpoints are needed.

## Repair authority if Phase C0 confirms a gap

Never promote runtime authority from name similarity, folder proximity, solution build order, copy-only evidence or loader-only evidence.

If the target is a folder-scan loader, the minimum generic HIGH rule must compose:

```text
host enumerates a deterministic runtime directory/pattern
+ enumerated file is passed to an assembly-load operation
+ plugin has unique deterministic assembly identity
+ build metadata delivers that exact plugin output to the same loader runtime directory
```

Ambiguous identity, unresolved directory, delivery to a different directory, broad dynamic command expansion or test-only loaders remain fail-closed.

Solution `ProjectDependencies` may contribute build provenance but is never runtime-use evidence by itself.

If Phase C0 confirms a currently unsupported shape:

```text
private observation
-> sanitize exact defect shape
-> focused red synthetic regression
-> minimum generic repair
-> negative fail-closed regressions
-> focused + related + full relevant local verification
-> one coherent implementation commit/push
-> rerun only affected RD8-C discovery evidence
```

Do not combine folder-loader, shell-copy and solution-dependency support speculatively. Implement only the real-target shape that the evidence requires.

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
approved source-enabled environment:
  fill the RD8-C Phase-C0 support-matrix checklist only
  -> loader ownership + identity/enumeration + load API
  -> delivery syntax
  -> solution ProjectDependencies role
  -> current PKC edge/unresolved counts

implementation, only if exact target shape is unsupported:
  regression-first minimum generic repair
  -> local verification
  -> one coherent commit/push

then:
  fresh RD8-A run without --resume
  -> RD8-B Level-1 probes
  -> close RD8 only when A+B+C all pass
```