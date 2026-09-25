# PKC Status

Last updated: 2026-09-25

## Repository state reviewed

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

Important merged RD8 production checkpoints beneath this docs HEAD:

```text
612fa998da6983662a90eed2836714c9928243b5
feat: make large runs failure-safe, resumable and self-describing

48ce2f10c1e506b28e37aa3ff15ba5e241a3da0c
feat: follow sole implementations and surface mapped-field, gate and guard rules

fa30e3eb140357da5453702090be307417abb4e2
fix: report the preserved workspace when it could not replace the current one
```

This checkpoint is documentation/audit only. It does not change production behavior or runtime authority.

## Current priority state

```text
V0.4.4 Loren knowledge readiness                    PASS / COMPLETE
V0.4.5 real-repository generalization               PASS / COMPLETE
V0.4.6 business logic reconstruction                PASS / COMPLETE
V0.4.7-A origin and copy timing                     PASS / COMPLETE
V0.4.7-B computation and later change               PASS / COMPLETE
V0.4.7-C backend to API                             PASS / COMPLETE
V0.4.7-D / R7.9 API to rendered value               PASS / COMPLETE
V0.4.7-D / R7.10 joint visibility                   OPEN / REVIEW PAUSED / NOT ACCEPTED
V0.4.7-E0 repository discovery + bounded run        ACTIVE / USER-AUTHORIZED BOUNDED PREWORK
V0.4.7-E0 / RD1 inventory + safe exclusion          PASS / COMPLETE
V0.4.7-E0 / RD2 application boundaries + ownership PASS / COMPLETE
V0.4.7-E0 / RD3 vendor/custom frontend              PASS / COMPLETE
V0.4.7-E0 / RD4 runtime/plugin provenance           PASS / COMPLETE (deterministic/synthetic gate)
V0.4.7-E0 / RD5 deterministic ScanPlan              PASS / COMPLETE
V0.4.7-E0 / RD6 scoped/bounded semantic execution   PASS / COMPLETE
V0.4.7-E0 / RD7 coverage + observability            PASS / COMPLETE
V0.4.7-E0 / RD8 private large-repository validation ACTIVE / RD8-C REPAIR CANDIDATE VERIFIED (UNCOMMITTED) / NOT PASS
V0.4.7-E1 remaining product-value repairs           LOCKED behind E0
V0.4.7-E2 final product acceptance / R7.14          LOCKED behind E1
continuous update/diff                              LOCKED
V0.5 Azure DevOps input evidence                    LOCKED
```

The E0 priority override remains active because practical scan operability is required for the demo. It does not self-certify R7.10/D and does not permit weaker semantic/runtime authority.

## RD1-RD7

RD1-RD7 remain `PASS / COMPLETE`.

RD4 specifically accepted the currently supported fail-closed runtime-plugin rule. It did not prove that every real runtime loader/copy topology is supported.

Current RD4 authority is:

```text
production host loader
+ unique literal assembly identity
+ deterministic build/copy delivery into the host tree
-> HIGH runtime-plugin-load edge
```

Unsupported or ambiguous shapes remain unresolved/UNKNOWN.

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

The old successful run predates the fully integrated current production state and cannot close RD8-A or RD8-B.

## RD8 Gate A — fresh current-main operability

Still required in the approved source-enabled/company environment:

```text
build PKC Release from current main
pkc discover <approved-private-copy>
pkc run <approved-private-copy>      # fresh acceptance run; no --resume
```

Record sanitized elapsed time, peak process-tree memory, coverage, evidence counts, artifact status, final workspace path and file count. Confirm run summaries, discovery/coverage/checkpoint metadata and workspace output are internally plausible.

## RD8 Gate B — targeted Level-1 product value

After the fresh workspace exists, run 2-3 known-answer probes using `docs/benchmarks/product-value-benchmark-protocol.md`:

```text
phase 1: workspace-only answer
phase 2: approved source-enabled known answer
phase 3: compare and score
```

Prefer the quantity-adjustment probe plus materially different questions from `docs/question-trainning.md`.

A compiler exit or generated workspace alone is not product-value acceptance.

## RD8 Gate C — runtime/plugin target blocker

The intended large-repository class is now known to contain runtime-loaded plugins:

```text
host
  -> reflection-based plugin load from runtime output
  -> plugin projects are not host project references

build delivery
  -> custom post-build copy into runtime output
```

Therefore target applicability is `YES`; the old `N/A` path is invalid. Earlier PKC output without runtime-plugin edges is missing discovery yield, not proof that the topology is absent.

### Repo-local audit completed on 2026-09-25

Current production support was re-audited against RD4 regressions and code.

Supported loader identity shapes include direct string-literal assembly names and direct literal `.dll` names in supported reflection load APIs. Supported delivery shapes are literal `OutputPath`/`OutDir`/`BaseOutputPath`, supported MSBuild `<Copy>` forms, or the existing unconditional project-reference fallback.

Current production intentionally fails closed for important candidate target shapes including:

- folder enumeration such as `GetFiles(..., "*.dll")` followed by a variable-path assembly load;
- post-build `copy` / `xcopy` / `<Exec>` command delivery;
- loaders implemented in shared libraries rather than the production host's own project;
- configuration-driven plugin identities;
- solution-level `ProjectDependencies` as dependency provenance.

The `.sln` parser currently records solution project membership only; it does not model `ProjectDependencies`.

This is not a newly introduced defect. RD4's own acceptance record explicitly carried folder-scan loaders and post-build shell copy commands as known fail-closed gaps for RD8 to measure.

Authoritative audit/decision aid:

`docs/reviews/2026-09-25-rd8-runtime-plugin-support-matrix.md`

### Phase C0 complete on 2026-09-25 — repair authorized

Read-only source-enabled inspection classified the real target (sanitized; no names, paths or source):

```text
loader ownership:     production web host, own project
identity shape:       folder scan of <runtime-base>/<M>/ subdirectories, load <subdir>/<subdir-name>.dll
load API:             Assembly.LoadFrom passed as a method group (no call parenthesis)
delivery:             plugin-owned MSBuild <Copy> in an AfterTargets=Build target,
                      SourceFiles via a target-local item = $(TargetDir)**,
                      destination <host>/$(OutDir)<M>/$(ProjectName)/%(RecursiveDir)
                      (2 plugins; also copies into test projects and a conditional $(<external-dest>) copy)
solution dependency:  .slnx BuildDependency present and relevant; build provenance only, not needed
current PKC result:   0 runtime-plugin-load, 0 for every runtime unresolved reason
```

Root cause: the loader regex requires a call parenthesis, so the method-group load is never observed; the copy check requires an own-output token literally in `SourceFiles`, so item indirection hides delivery. Both gaps suppress even the unresolved signal.

Decision: support-matrix row "folder-scan loader" applies. Regression-first minimum generic repair R1-R4 is authorized and fully specified in:

`docs/reviews/2026-09-25-rd8-c0-target-classification.md`

Out of scope for this repair: `.slnx`/`.sln` dependency parsing, `PostBuildEvent`, `<Exec>`/xcopy, `GetFiles("*.dll")`, shared-library loaders, config-driven identities.

HIGH authority still requires deterministic composition of loader scan directory, actual assembly load, unique plugin identity, and unconditional delivery of that exact plugin output into the same runtime directory. Solution build dependency alone is never runtime-use authority.

## E0 completion rule

E0 is PASS only when:

```text
RD1-RD7 PASS / COMPLETE
+ RD8-A fresh current-main run closes
+ RD8-B targeted Level-1 product value closes
+ RD8-C applicable runtime-plugin topology is deterministically represented on the real target
+ normal product path practically produces the workspace
+ coverage remains honest
```

Only then may E1 unlock.

## Parked work

Construction/default/computation remains parked until E0 PASS:

```text
branch  fix/product-value-construction-state
commit  35c8e5c5f856e15568aa963bb2d76268008c5570
```

Do not merge it during RD8.

## Formal D state

R7.10/D remains OPEN and not accepted. The prior independent-review lane is paused by product priority, not passed. A fresh independent review is still required before final V0.4.7 acceptance.

## Exact next action

Update 2026-09-25: R1-R4 candidate is implemented and verified in the working tree of `codex/rd8-c-runtime-plugin` (408/408 tests, Release clean, real target 2 HIGH `runtime-plugin-load` edges). Remaining: commit, reproduce the count from the committed build, then RD8-A and RD8-B. Details: `docs/reviews/2026-09-25-rd8-c0-target-classification.md` section "Implementation candidate result".

```text
implementer (no private-target access needed):
  implement R1-R4 from docs/reviews/2026-09-25-rd8-c0-target-classification.md
  -> red synthetic regressions first (positive + listed negatives)
  -> minimum generic fail-closed repair
  -> focused / related / full local verification + Release build
  -> one coherent implementation commit on a topic branch

approved-environment operator, after it lands:
  pkc discover on a disposable copy of the target
  -> expect runtime-plugin-load = 2, no unexplained unresolved reasons
  -> record sanitized counts = RD8-C proof

then:
  RD8-A fresh discover + full run without --resume
  -> RD8-B 2-3 Level-1 probes

Do not start E1 until RD8 and E0 are explicitly PASS.
```
