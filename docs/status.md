# PKC Status

Last updated: 2026-09-24

## Repository state reviewed

Current `main` immediately before this state-correction checkpoint:

```text
3f1b481c1d909953b657b4cf0b142f894a0e0121
docs: audit RD8 runtime plugin real-repo evidence [skip ci]
```

Latest production-code ancestor remains:

```text
fa30e3eb140357da5453702090be307417abb4e2
fix: report the preserved workspace when it could not replace the current one
```

Important merged production checkpoints beneath current docs HEAD:

```text
612fa998da6983662a90eed2836714c9928243b5
feat: make large runs failure-safe, resumable and self-describing

48ce2f10c1e506b28e37aa3ff15ba5e241a3da0c
feat: follow sole implementations and surface mapped-field, gate and guard rules

fa30e3eb140357da5453702090be307417abb4e2
fix: report the preserved workspace when it could not replace the current one
```

This update is documentation/state correction only. It does not change production behavior.

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
V0.4.7-E0 / RD4 runtime/plugin provenance           PASS / COMPLETE (deterministic/synthetic coverage)
V0.4.7-E0 / RD5 deterministic ScanPlan              PASS / COMPLETE
V0.4.7-E0 / RD6 scoped/bounded semantic execution   PASS / COMPLETE
V0.4.7-E0 / RD7 coverage + observability            PASS / COMPLETE
V0.4.7-E0 / RD8 private large-repository validation ACTIVE / KNOWN BLOCKER / NOT PASS
V0.4.7-E1 remaining product-value repairs           LOCKED behind E0
V0.4.7-E2 final product acceptance / R7.14          LOCKED behind E1
continuous update/diff                              LOCKED
V0.5 Azure DevOps input evidence                    LOCKED
```

## Priority boundary

The user explicitly authorized E0 before formal R7.10/D acceptance because scan operability is required for a credible demo.

That override remains active, but it does **not** mark R7.10/D PASS and does not allow E0 work to weaken accepted semantic/fail-closed authority.

Authoritative decision:

`docs/plans/2026-09-24-demo-scan-priority-override.md`

## RD1-RD7

RD1-RD7 are `PASS / COMPLETE`. Their implementation commits are ancestors of current `main`, their evidence documents remain in the repository, and descendant production-equivalent workflow runs were green.

RD4 is specifically a deterministic/synthetic acceptance of the supported runtime-plugin provenance rule. It was never proof that the intended private repository's runtime topology had been recovered.

## RD8 supporting evidence

The first approved private large-repository exercise produced useful but incomplete evidence.

Sanitized scale:

- about 26.9k files;
- 18 hosts;
- about 16.8k planned semantic files;
- 1,982 test-evidence files and 41 light-index files withheld by plan;
- 138 excluded areas;
- about 263.6k facts and 2.04M relations;
- 4,186 workflow candidates.

Observed runs:

| Run | PKC state | Result | Elapsed | Peak memory |
| --- | --- | --- | --- | --- |
| 1 | `cf20b29` (RD7) | failed at artifact write (OOM) | 41.5 min | ~18.6 GB |
| 2 | `cf20b29` + then-working resource repairs | exit 0; workspace generated | 29 min | ~7.5 GB |

`pkc discover` alone took about 98 seconds. Run 2 generated about 703 product features and 4,709 workspace files.

The resource repairs used by run 2 are now merged in `612fa998`. Later semantic/output fixes `48ce2f10` and `fa30e3eb` are also merged. Therefore the old successful run remains supporting evidence, not current-main RD8 acceptance.

## RD8 Gate A — fresh current-main operability

Still required in the approved source-enabled/company environment:

```text
build PKC Release at current main
pkc discover <approved-private-copy>
pkc run <approved-private-copy>      # fresh acceptance run; no --resume
```

Record sanitized elapsed time, peak process-tree memory, coverage, evidence counts, artifact status, final workspace path and file count. Confirm `RUN_SUMMARY.md`, `run-summary.json`, discovery/coverage/checkpoint metadata and the generated workspace are internally plausible.

No universal hard RAM threshold exists. The run must materially improve the original failure mode and complete practically on the approved demo environment.

## RD8 Gate B — targeted Level-1 product value

After the fresh workspace exists, run 2-3 known-answer probes using `docs/benchmarks/product-value-benchmark-protocol.md`:

```text
phase 1: workspace-only answer
phase 2: approved source-enabled known answer
phase 3: compare and score
```

Prefer the quantity-adjustment probe plus materially different questions from `docs/question-trainning.md`.

A compiler exit, non-zero facts or generated workspace alone is not product-value acceptance.

## RD8 Gate C — runtime/plugin target blocker

A recovered sanitized reconnaissance corrects the previous applicability assumption.

The intended large-repository class **does contain runtime-loaded plugin modules**:

```text
host
  -> reflection-based plugin load from an output subfolder
  -> plugin modules not project-referenced by the host

build delivery
  -> custom post-build copy into the runtime output
```

The sanitized reconnaissance also identifies solution/build dependency evidence and post-build copy targets as the evidence needed to model those runtime edges.

Therefore:

```text
runtime/plugin applicability on intended target: YES
N/A disposition for intended target:             NOT VALID
previous PKC private-profile runtime edge yield: MISSING / NOT PROVEN
RD8-C:                                           OPEN / KNOWN BLOCKER
```

The earlier statement that the private repository “did not exercise” runtime-plugin cases is now interpreted as **PKC did not recover the applicable topology**, not that the topology was absent.

The safe summary does not expose enough exact loader/copy syntax to implement a generic parser without guessing. Phase C therefore requires a narrow source-enabled inspection of the actual loader + delivery syntax, sanitized into a regression shape. If current main cannot represent that deterministic provenance:

```text
exact sanitized defect shape
-> red synthetic regression
-> minimum generic fix
-> negative fail-closed regressions
-> focused / related / full relevant local verification
-> one coherent commit/push
-> rerun affected RD8-C discovery evidence
```

Do not create runtime authority from similar names, folder proximity, or copy/load hints that do not jointly prove identity and delivery.

Evidence:

- `docs/reviews/2026-09-24-rd4-runtime-plugin-provenance-evidence.md`
- `docs/reviews/2026-09-24-rd8-runtime-plugin-real-repo-evidence-audit.md`
- `docs/reviews/2026-09-24-rd8-runtime-plugin-target-applicability.md`

## E0 completion rule

E0 is PASS only when:

```text
RD1-RD7 PASS / COMPLETE
+ RD8-A fresh current-main run closes
+ RD8-B targeted Level-1 product value closes
+ RD8-C applicable runtime-plugin topology is correctly handled or explicitly accepted with deterministic evidence
+ normal product path practically produces the workspace
+ coverage remains honest
```

Only then may E1 unlock.

## Parked work

Construction/default/computation candidate remains parked until E0 PASS:

```text
branch  fix/product-value-construction-state
commit  35c8e5c5f856e15568aa963bb2d76268008c5570
```

Do not merge it during RD8.

## Formal D state

R7.10/D remains OPEN and not accepted. The prior rereview lane is paused by product priority, not passed. A fresh independent review is still required before final V0.4.7 acceptance.

## Exact next action

```text
RD8-C first:
  approved source-enabled inspection of the target's actual reflection loader + build/copy syntax
  -> sanitize the exact deterministic defect shape
  -> if current main misses it, regression-first generic repair
  -> rerun discovery and prove the applicable runtime-plugin edge/fail-closed behavior

Then / in the same approved environment:
  RD8-A fresh discover + full run without --resume
  -> sanitized resource/coverage/workspace evidence
  RD8-B 2-3 Level-1 known-answer probes

Do not start E1 until RD8 and E0 are explicitly PASS.
```