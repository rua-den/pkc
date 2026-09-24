# PKC Handoff

Last updated: 2026-09-24

This handoff replaces the stale `PUSH + CI PENDING` / `working tree not committed` state from the earlier RD8 notes.

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
10. `docs/benchmarks/product-value-benchmark-protocol.md`
11. `docs/reviews/2026-09-24-main-state-reconciliation.md`

Then verify current `main`, recent commits, production code and relevant regressions. Never reset to an older SHA merely because this handoff names one.

## Repository state at handoff

Remote `main` immediately before the reconciliation commit:

```text
9ef914f57c558e3b2963b9ba4dd187a6ecd6603f
Add question in demo
```

Latest production-code commit beneath that docs-only HEAD:

```text
fa30e3eb140357da5453702090be307417abb4e2
fix: report the preserved workspace when it could not replace the current one
```

Current descendant HEAD `9ef914f` completed all four observed GitHub Actions workflows successfully.

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
RD8 private large-repository gate   ACTIVE / PARTIAL EVIDENCE / NOT PASS
E1 remaining semantic repairs       LOCKED behind E0
E2 final product acceptance         LOCKED behind E1
```

RD1-RD7 are no longer `push + CI pending`: their commits are on `main`, their evidence docs remain present, and a later descendant HEAD is green.

## Priority decision

Formal R7.10 / V0.4.7-D remains OPEN and NOT ACCEPTED.

The user explicitly authorized E0 Repository Discovery and bounded-run work before formal D acceptance because the intended large mixed legacy repository could not complete the normal product path practically.

This authorization does not self-certify D and must not weaken accepted fail-closed semantic authority.

## Merged production work since the first private run

### Large-run operability — merged

```text
612fa998da6983662a90eed2836714c9928243b5
feat: make large runs failure-safe, resumable and self-describing
```

Includes:

- streamed + atomic large JSON artifact writes;
- one shared `MSBuildWorkspace` per C# semantic-enrichment pass;
- best-effort post-scan artifact writes with explicit failure reporting;
- staged AI workspace replacement, preserving `.pkc/workspace.previous` and `.pkc/workspace.new` when needed;
- scan checkpoint and `pkc run|build <repo> --resume`;
- `.pkc/RUN_SUMMARY.md` and `.pkc/run-summary.json`;
- answer-format contract with user-language/business wording and proven/not-proven/inferred markers.

### Product-value rule-surface repair — merged

```text
48ce2f10c1e506b28e37aa3ff15ba5e241a3da0c
feat: follow sole implementations and surface mapped-field, gate and guard rules
```

Includes:

- fail-closed own-interface → sole concrete implementation fallback, explicitly labelled inferred rather than DI-proven;
- unresolved own-interface dead-ends surfaced as important unknowns;
- mapped-field rules through `ForMember` / `MapFrom` and supported `Expression<Func<...>>` factories;
- conjunct preservation with developer documentation kept as separately labelled evidence;
- boolean gates with local operand and setting-key recovery;
- supported guard-condition rules;
- repository-specific `*Authorize` attributes recorded as permission evidence;
- answer-contract rule that code condition wins when comments disagree.

### Workspace-summary fallback repair — merged

```text
fa30e3eb140357da5453702090be307417abb4e2
fix: report the preserved workspace when it could not replace the current one
```

When the current workspace cannot be replaced, summaries now report and count `.pkc/workspace.new` rather than accidentally describing the old workspace.

### Demo question corpus — docs only

```text
9ef914f57c558e3b2963b9ba4dd187a6ecd6603f
Add question in demo
```

Adds `docs/question-trainning.md`, which is suitable input for the next targeted Level-1 benchmark.

## RD8 evidence already collected

The first approved private large-repository exercise is supporting evidence, not final acceptance.

Sanitized repository scale:

- about 26.9k files;
- 18 hosts;
- about 16.8k planned semantic files;
- 1,982 test-evidence files and 41 light-index files withheld by the plan;
- 138 excluded areas;
- about 263.6k facts and 2.04M relations;
- 4,186 workflow candidates.

| Run | PKC state | Result | Elapsed | Peak memory |
| --- | --- | --- | --- | --- |
| 1 | `cf20b29` (RD7) | failed at `[pkc:write]` due OOM | 41.5 min | ~18.6 GB |
| 2 | `cf20b29` + then-uncommitted resource repairs | exit 0; workspace generated | 29 min | ~7.5 GB |

`pkc discover` alone took about 98 seconds. Run 2 generated about 703 product features and 4,709 workspace files.

The resource repairs used in run 2 are now merged in `612fa998`, but that successful run predates the fully integrated current production state (`48ce2f10` + `fa30e3eb`). Therefore it cannot by itself close RD8.

## RD8 required next execution

Run only inside the approved source-enabled/company environment, against an approved disposable copy of the private repository if `.pkc/` must not touch the original checkout.

### Phase A — fresh current-main operability run

```text
1. build PKC Release from current main
2. pkc discover <private-repo-copy>
3. pkc run <private-repo-copy>        # no --resume for the acceptance measurement
4. capture elapsed time + peak working set
5. inspect RUN_SUMMARY.md, run-summary.json and coverage
6. confirm the generated workspace path/file count is plausible and usable
```

Record only sanitized measurements:

- no proprietary names;
- no source snippets;
- no config values;
- no raw fact payloads.

There is no universal hard RAM threshold. The run must materially improve on the original failure mode and complete practically on the approved demo environment.

If memory is still around the prior ~7.5 GB observation, profile first. Do not implement speculative optimization without evidence of the retained-memory phase.

### Phase B — targeted Level-1 product-value benchmark

Use `docs/benchmarks/product-value-benchmark-protocol.md`.

Select 2-3 known-answer questions from `docs/question-trainning.md` and/or the quantity-adjustment probe.

For every probe:

```text
phase 1: generated workspace only → record the answer
phase 2: source-enabled approved environment → establish known answer
phase 3: compare and score
```

Score at least:

- business correctness;
- completeness of material conditions/effects;
- unsupported additions;
- uncertainty calibration;
- business wording quality;
- evidence/trace completeness.

A generated workspace alone is not enough to claim demo product value.

### Phase C — runtime/plugin applicability disposition

The first private repository did not exercise runtime-dependency-index or runtime-plugin cases.

Keep this explicit:

- RD4 deterministic/synthetic coverage remains PASS;
- the private RD8 run did not exercise those shapes;
- before RD8 PASS, either record an accepted `N/A` because the approved repository demonstrably lacks those shapes, or validate them on another approved real repository/corpus.

Do not silently promote synthetic evidence into real-repository exercise evidence.

## Known product-value gaps to keep visible

Do not start E1 yet, but preserve these as candidates revealed by the private workspace:

- generic mediator/request → handler traversal remains incomplete;
- UI linkage on the legacy frontend stacks remains weak/zero for many workflows;
- raw code expressions can still leak into product-language knowledge in some paths;
- source comments can contradict executable comparisons, so executable condition remains authoritative;
- some private-repository workflows may still end at unresolved interfaces when sole-implementation proof is unavailable.

The quantity-adjustment probe drove the already-merged `48ce2f10` repair; re-run that probe on the fresh workspace before deciding what semantic repair is actually next.

## E0 completion rule

Do not mark E0 PASS until all are true:

```text
RD1-RD7 PASS / COMPLETE
fresh current-main private RD8 run completes practically
generated workspace + summaries/coverage are plausible
targeted Level-1 answers are useful and correctly uncertain
runtime/plugin applicability has an explicit accepted disposition
```

Only then unlock E1.

## Parked work

Do not merge during RD8:

```text
branch  fix/product-value-construction-state
commit  35c8e5c5f856e15568aa963bb2d76268008c5570
```

## Formal D boundary

R7.10/D remains OPEN. The prior independent-review lane is paused, not passed.

Before final V0.4.7 acceptance, a fresh independent review must accept the then-current R7.10 production state.

## Exact next action

```text
operator:
  fresh `pkc discover` + full `pkc run` on current main, without --resume
  → sanitized time/memory/coverage/workspace evidence
  → 2-3 targeted Level-1 known-answer probes
  → explicit runtime/plugin applicability disposition

implementation:
  do not add a new semantic feature unless the fresh RD8 run/benchmark exposes a concrete blocker
  → reproduce that blocker regression-first with a synthetic fixture
  → minimum generic repair
  → local verification
  → one coherent commit/push
```
