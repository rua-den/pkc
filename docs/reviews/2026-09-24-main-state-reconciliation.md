# Main State Reconciliation Review

Date: 2026-09-24
Review type: repository/documentation state reconciliation
Scope: current `main`, recent production commits, CI state, E0/RD8 milestone documentation

## Decision

**PASS for documentation reconciliation.**

The repository state has moved beyond the older handoff wording. RD1-RD7 implementation commits are on remote `main`, the current descendant HEAD is green across all four observed GitHub Actions runs, and the large-run/rule-surface repairs previously described as uncommitted working-tree work are now merged production commits.

RD8 remains **OPEN / NOT PASS** because the successful private run was performed before the fully integrated current production state and because targeted product-value benchmarking plus runtime/plugin applicability disposition remain incomplete.

## Repository state reviewed

Remote `main` observed before this docs reconciliation:

```text
9ef914f57c558e3b2963b9ba4dd187a6ecd6603f
Add question in demo
```

The commit changes only `docs/question-trainning.md`.

Latest production-code commit beneath it:

```text
fa30e3eb140357da5453702090be307417abb4e2
fix: report the preserved workspace when it could not replace the current one
```

Recent production history relevant to the current checkpoint:

```text
612fa998da6983662a90eed2836714c9928243b5
feat: make large runs failure-safe, resumable and self-describing

48ce2f10c1e506b28e37aa3ff15ba5e241a3da0c
feat: follow sole implementations and surface mapped-field, gate and guard rules

fa30e3eb140357da5453702090be307417abb4e2
fix: report the preserved workspace when it could not replace the current one
```

## CI evidence

GitHub Actions query for head SHA `9ef914f57c558e3b2963b9ba4dd187a6ecd6603f` returned:

```text
total successful workflow runs: 4
failed workflow runs:            0
```

Observed successful workflows include the Loren external trial, Loren main canary and Jellyfin generalization trial; the query reports four successful runs total for the head SHA.

The prior docs statement that RD1-RD7 still required an operator push/CI confirmation is therefore obsolete.

## RD1-RD7 reconciliation

Previous state:

```text
LOCAL PASS / PUSH + CI PENDING
```

Reviewed state:

```text
PASS / COMPLETE
```

Reason:

1. each RD1-RD7 implementation commit is an ancestor of current remote `main`;
2. each checkpoint has a committed evidence document;
3. later current-main production commits depend on those checkpoints rather than replacing or bypassing them;
4. current descendant HEAD is green across all four observed workflows.

This change only closes the stale push/CI bookkeeping gate. It does not invent new semantic acceptance evidence.

## First private-run evidence reviewed

Sanitized existing evidence:

- about 26.9k files;
- 18 hosts;
- about 16.8k planned semantic files;
- about 263.6k facts;
- about 2.04M relations;
- 4,186 workflow candidates;
- `pkc discover` about 98 seconds.

Run observations:

| Run | State | Result | Elapsed | Peak memory |
| --- | --- | --- | --- | --- |
| 1 | RD7 (`cf20b29`) | OOM at artifact write | 41.5 min | ~18.6 GB |
| 2 | RD7 + then-uncommitted resource repairs | exit 0; workspace generated | 29 min | ~7.5 GB |

Run 2 produced about 703 product features and 4,709 workspace files.

## Production repair reconciliation

### `612fa998` — merged

The earlier handoff called these changes “working tree / not committed”. That is stale.

The commit contains the resource/operability repair used by the successful private run, including:

- streamed/atomic JSON artifact writes;
- shared `MSBuildWorkspace` per semantic-enrichment pass;
- best-effort post-scan artifact reporting;
- staged workspace replacement and preservation of prior/new workspaces;
- scan checkpoint and `--resume` support;
- run summary artifacts;
- answer-format contract.

### `48ce2f10` — merged

The earlier handoff also called the quantity-adjustment rule-surface work uncommitted. That is stale.

The commit contains:

- fail-closed sole implementation fallback for own non-generic interfaces when direct DI proof is unavailable;
- unresolved interface dead-end reporting;
- mapped-field/expression-factory rules;
- boolean gates;
- guard conditions;
- repository-specific `*Authorize` metadata;
- answer-contract guidance for comment/code disagreements.

### `fa30e3eb` — merged

The run summary now reports/counts `.pkc/workspace.new` when the new workspace could not replace the existing one.

## Why RD8 is not accepted yet

The successful 29-minute / ~7.5 GB run occurred before the fully integrated current production state.

A fresh current-main run is required because:

- `48ce2f10` changes semantic enrichment and may affect runtime/memory/output counts;
- `fa30e3eb` changes final workspace reporting behavior;
- current acceptance should measure the normal integrated product path, not reconstruct acceptance from an earlier patched checkout.

The acceptance measurement must be a **fresh full run without `--resume`**, because resume intentionally reuses an earlier scan and would not prove current-main scan operability.

## Benchmark review

The existing benchmark protocol correctly states that:

```text
green build / successful scanner / non-zero facts / generated workspace
!= product-value acceptance
```

The current RD8 documents previously treated a Level-1 benchmark as a follow-up proposal rather than an explicit exit gate.

That is too weak for the demo goal. RD8 is therefore clarified to require 2-3 targeted known-answer probes after the fresh current-main workspace is generated.

Required sequence:

```text
workspace-only answer
→ approved source-known cross-check
→ explicit score
```

Required dimensions:

- correctness;
- material-condition/business-effect completeness;
- unsupported additions;
- uncertainty calibration;
- business wording;
- evidence/trace completeness.

This strengthens demo acceptance without replacing E2's broader Level-2 product acceptance.

## Runtime/plugin applicability gap

The first private repository did not exercise runtime-dependency-index or runtime-plugin cases.

RD4 deterministic/synthetic evidence is still valid, but the private run must not be described as exercising those shapes.

Before RD8 PASS, require explicit disposition:

- accepted `N/A` if the approved repository demonstrably lacks those shapes; or
- validation on another approved real repository/corpus that contains them.

## Scope decision

Do **not** start another semantic feature merely because the candidate list exists.

Current authorized next sequence:

```text
fresh current-main private discover/run
→ measure actual current bottleneck
→ targeted Level-1 known-answer benchmark
→ resolve runtime/plugin applicability
→ only fix a blocker that this evidence actually demonstrates
```

The parked construction/default/computation candidate remains locked behind E0.

R7.10/D remains OPEN and requires fresh independent acceptance before final V0.4.7 completion.

## Documents updated by this reconciliation

```text
docs/status.md
docs/handoff.md
docs/milestones.md
docs/v0.4.7-acceptance-plan.md
docs/plans/2026-09-24-demo-critical-sequential-execution-plan.md
docs/plans/2026-09-24-demo-scan-priority-override.md
```

No production code is changed by this reconciliation.
