# PKC Handoff

Last updated: 2026-09-24

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
11. `docs/benchmarks/product-value-benchmark-protocol.md`
12. `docs/reviews/2026-09-24-rd8-runtime-plugin-real-repo-evidence-audit.md`
13. `docs/reviews/2026-09-24-rd8-runtime-plugin-target-applicability.md`

Then verify current `main`, recent commits, production code and relevant regressions. Never reset to an older SHA merely because a handoff names one.

## Repository state at this handoff

Current `main` immediately before this documentation checkpoint:

```text
3f1b481c1d909953b657b4cf0b142f894a0e0121
docs: audit RD8 runtime plugin real-repo evidence [skip ci]
```

Latest production-code ancestor:

```text
fa30e3eb140357da5453702090be307417abb4e2
fix: report the preserved workspace when it could not replace the current one
```

This handoff update is docs/state only.

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
RD8 private large-repository gate   ACTIVE / KNOWN BLOCKER / NOT PASS
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

The first approved private exercise remains supporting evidence only:

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

## Important correction — the intended target DOES contain runtime plugins

A saved sanitized reconnaissance of the intended large mixed repository was recovered after the earlier real-repository artifact audit.

It proves the target architecture includes runtime plugin modules with this sanitized shape:

```text
WEB_API_A
  -> [runtime reflection load; no project reference]
       SHARED_MODULE_P1
       SHARED_MODULE_P2

plugin delivery
  -> copied into a runtime output subfolder by a custom post-build step
```

The reconnaissance explicitly states that a project-reference-only graph would miss these plugins and recommends combining host loader evidence with solution build-dependency declarations and post-build copy targets.

This changes RD8-C materially:

```text
old interpretation:
  private target may have no applicable runtime/plugin topology -> N/A could be possible

corrected interpretation:
  private target applicability = YES
  previous PKC discovery did not recover the applicable topology
  N/A is not valid for this intended target
```

The previous statement that the private repository “did not exercise runtime-plugin cases” referred to generated PKC evidence, not the real architecture.

Review:

`docs/reviews/2026-09-24-rd8-runtime-plugin-target-applicability.md`

## Why production code is not being changed yet

The saved sanitized reconnaissance proves the architectural shape but intentionally does not include proprietary source bodies or enough exact syntax to identify which unsupported RD4 form is present.

Do **not** guess the implementation from likely patterns such as `Directory.GetFiles`, `Assembly.LoadFrom`, `PostBuildEvent`, `xcopy`, or a particular MSBuild target.

The next source-enabled inspection needs only the deterministic syntax shape:

1. loader syntax family;
2. build/copy syntax family;
3. whether solution-level build dependencies contribute to plugin identity/delivery;
4. sanitized evidence sufficient to construct a synthetic regression.

If current main misses that exact deterministic shape:

```text
private observation
-> sanitize defect shape
-> add focused red synthetic regression
-> minimum generic repair
-> add negative fail-closed regressions
-> focused + related + full relevant local verification
-> one coherent commit/push
-> rerun affected RD8-C discovery evidence
```

Never link plugins by similar names, nearby folders, or copy/load hints without deterministic identity + delivery provenance.

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

## RD8-C — now a known blocker

Current state:

```text
applicable topology exists                 YES
current-main proof on intended target      MISSING / NOT PROVEN
accepted N/A path for intended target      CLOSED
exact deterministic defect shape           EXTERNAL SOURCE-ENABLED INSPECTION REQUIRED
```

Jellyfin and Loren remain useful evidence that their pinned sources do not exercise this shape; they cannot substitute for the intended target. Do not spend a GitHub runner on the existing manual benchmark corpus merely to reproduce another no-applicable-shape result.

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
  inspect only the actual runtime loader + build/copy provenance shape
  -> sanitize exact syntax/evidence pattern

implementation, only if current main misses that pattern:
  regression-first generic RD8-C repair
  -> local verification
  -> one coherent commit/push

then:
  fresh RD8-A run without --resume
  -> RD8-B Level-1 probes
  -> close RD8 only when A+B+C all pass
```
