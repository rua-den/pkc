# PKC Status

Last updated: 2026-09-24

## Repository state reviewed

Remote `main` observed immediately before this documentation reconciliation:

```text
9ef914f57c558e3b2963b9ba4dd187a6ecd6603f
Add question in demo
```

That HEAD is documentation/training-question only. The latest production-code commit beneath it is:

```text
fa30e3eb140357da5453702090be307417abb4e2
fix: report the preserved workspace when it could not replace the current one
```

Important merged production checkpoints now present on `main`:

```text
612fa998da6983662a90eed2836714c9928243b5
feat: make large runs failure-safe, resumable and self-describing

48ce2f10c1e506b28e37aa3ff15ba5e241a3da0c
feat: follow sole implementations and surface mapped-field, gate and guard rules

fa30e3eb140357da5453702090be307417abb4e2
fix: report the preserved workspace when it could not replace the current one
```

GitHub Actions for `9ef914f` show four workflow runs for that head SHA, all completed successfully. The previous `PUSH + CI PENDING` wording for RD1-RD7 is therefore stale and is retired by this update.

This reconciliation is docs-only and does not change production behavior.

## Current priority state

```text
V0.4.4 Loren knowledge readiness                   PASS / COMPLETE
V0.4.5 real-repository generalization              PASS / COMPLETE
V0.4.6 business logic reconstruction               PASS / COMPLETE
V0.4.7-A origin and copy timing                    PASS / COMPLETE
V0.4.7-B computation and later change              PASS / COMPLETE
V0.4.7-C backend to API                            PASS / COMPLETE
V0.4.7-D / R7.9 API to rendered value              PASS / COMPLETE
V0.4.7-D / R7.10 joint visibility                  OPEN / REVIEW PAUSED / NOT ACCEPTED
V0.4.7-E0 repository discovery + bounded run       ACTIVE / USER-AUTHORIZED BOUNDED PREWORK
V0.4.7-E0 / RD1 inventory + safe exclusion         PASS / COMPLETE
V0.4.7-E0 / RD2 application boundaries + ownership PASS / COMPLETE
V0.4.7-E0 / RD3 vendor/custom frontend             PASS / COMPLETE
V0.4.7-E0 / RD4 runtime/plugin provenance          PASS / COMPLETE (synthetic/deterministic coverage)
V0.4.7-E0 / RD5 deterministic ScanPlan             PASS / COMPLETE
V0.4.7-E0 / RD6 scoped/bounded semantic execution  PASS / COMPLETE
V0.4.7-E0 / RD7 coverage + observability           PASS / COMPLETE
V0.4.7-E0 / RD8 private large-repository validation ACTIVE / PARTIAL EVIDENCE / NOT PASS
V0.4.7-E1 remaining product-value repairs          LOCKED behind E0
V0.4.7-E2 final product acceptance / R7.14         LOCKED behind E1
continuous update/diff                             LOCKED
V0.5 Azure DevOps input evidence                   LOCKED
```

## Priority boundary

The user explicitly authorized E0 before formal R7.10/D acceptance because scan operability is required for a credible demo. That priority override remains active.

It does **not** mark R7.10/D PASS and does not allow E0 work to weaken accepted semantic/fail-closed authority.

Authoritative decision:

`docs/plans/2026-09-24-demo-scan-priority-override.md`

## RD1-RD7 reconciliation

RD1-RD7 were previously documented as `LOCAL PASS / PUSH + CI PENDING` because the implementing environment could not push.

That blocker no longer exists in repository state:

- every RD1-RD7 implementation commit is an ancestor of current `main`;
- their evidence documents remain in the repository;
- current descendant HEAD `9ef914f` completed all four observed GitHub Actions workflows successfully;
- later production commits built on top of those checkpoints rather than bypassing them.

Therefore RD1-RD7 are now recorded as `PASS / COMPLETE`. This is a reconciliation of already-merged, already-validated work, not a new implementation acceptance claim.

Evidence remains in:

```text
docs/reviews/2026-09-24-rd1-inventory-safe-exclusion-evidence.md
docs/reviews/2026-09-24-rd2-application-boundaries-evidence.md
docs/reviews/2026-09-24-rd3-vendor-frontend-classification-evidence.md
docs/reviews/2026-09-24-rd4-runtime-plugin-provenance-evidence.md
docs/reviews/2026-09-24-rd5-deterministic-scan-plan-evidence.md
docs/reviews/2026-09-24-rd6-scoped-semantic-execution-evidence.md
docs/reviews/2026-09-24-rd7-coverage-observability-evidence.md
```

## RD8 — current evidence

The first approved private large-repository exercise produced useful but incomplete RD8 evidence.

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
| 2 | `cf20b29` + then-uncommitted resource repairs | exit 0; workspace generated | 29 min | ~7.5 GB |

`pkc discover` alone took about 98 seconds. Run 2 generated about 703 product features and 4,709 workspace files.

The resource repairs used by run 2 are now merged in `612fa998`:

- streamed/atomic large JSON writes;
- one shared `MSBuildWorkspace` per semantic-enrichment pass;
- failure-safe artifact reporting;
- staged workspace swap with `.previous` / `.new` preservation;
- scan checkpoint + `--resume`;
- run summary and answer-format contract.

Subsequent production work also merged:

- `48ce2f10`: fail-closed sole-implementation traversal, unresolved interface dead-ends, mapped-field/expression rules, boolean gates, guard rules, custom `*Authorize` metadata;
- `fa30e3eb`: correct run-summary path/count when a new workspace is preserved as `.pkc/workspace.new`.

Because the successful private run predated the fully integrated current production state, it is **supporting evidence**, not final current-main RD8 acceptance.

## RD8 remaining acceptance work

### 1. Fresh full private run on current main

Run in the approved source-enabled/company environment against the approved private repository copy:

```text
build PKC Release at current main
pkc discover <private-repo-copy>
pkc run <private-repo-copy>          # fresh run; do not use --resume for this acceptance measurement
```

Record sanitized:

- discovery elapsed time;
- full-run elapsed time;
- peak working set;
- planned/executed/withheld/UNKNOWN coverage;
- facts, relations, workflows/product features;
- final workspace path and file count;
- artifact-write failures, if any.

Confirm `.pkc/workspace` (or the correctly reported `.pkc/workspace.new` fallback) is usable and that `RUN_SUMMARY.md`, `run-summary.json`, coverage and checkpoint metadata are internally plausible.

There is no universal numeric RAM threshold. Acceptance is based on materially improved behavior **and** practical completion on the approved demo environment. If the fresh run still peaks around the prior ~7.5 GB observation, profile the actual retained-memory phase before implementing another optimization.

### 2. Targeted Level-1 product-value benchmark

After the fresh workspace exists, run 2-3 known-answer probes, preferably from `docs/question-trainning.md` and the quantity-adjustment probe already used during source cross-checking.

Follow `docs/benchmarks/product-value-benchmark-protocol.md`:

```text
phase 1: workspace-only answer
phase 2: approved source-enabled known-answer cross-check
```

For each probe record at least:

- correctness;
- completeness of material conditions/business effects;
- unsupported additions / hallucinations;
- uncertainty calibration;
- business wording rather than raw-code restatement;
- evidence/trace completeness.

A compiler exit code, non-zero fact count, or generated workspace alone is not product-value acceptance.

### 3. Runtime/plugin real-repository evidence gap

The first private repository did not exercise runtime-dependency-index or runtime-plugin cases. RD4 deterministic/synthetic coverage remains valid, but RD8 must not claim those shapes were exercised on that private repository.

Before RD8 PASS, record an explicit acceptance disposition:

- `N/A` only if the approved repository is proven not to contain those shapes and the reviewer accepts absence as the correct result; or
- validate them on another approved real repository/corpus.

Do not silently convert synthetic coverage into private-repository exercise evidence.

## E0 completion rule

E0 is PASS only when:

```text
RD1-RD7 PASS / COMPLETE
+ fresh current-main RD8 run completes practically
+ honest coverage is recorded
+ targeted Level-1 demo questions are useful and correctly uncertain
+ remaining RD8 applicability gaps have an explicit disposition
```

Only then may E1 unlock.

## E0 invariants

- repository compilation is deterministic/local and requires zero AI tokens;
- discovery precedes expensive semantic scanning;
- repository is modeled as a graph, not one homogeneous root;
- source role and scan mode remain separate;
- only strongly proven generated/restorable areas auto-exclude;
- third-party runtime remains visible without default deep scan;
- modified vendor receives narrow deterministic first-party carve-outs;
- UNKNOWN is valid and never silently discarded;
- tests are evidence, not production authority;
- similar names never create dependency edges;
- runtime/plugin edges require deterministic provenance;
- private source/config values never leak into portable output;
- accepted semantic fail-closed boundaries remain unchanged inside selected scope.

## Parked work

Construction/default/computation candidate remains parked until E0 PASS:

```text
branch  fix/product-value-construction-state
commit  35c8e5c5f856e15568aa963bb2d76268008c5570
```

Do not merge it during RD8.

## Formal D state

R7.10/D remains OPEN and not accepted. The prior rereview lane is paused by product priority, not passed.

Before final V0.4.7 acceptance, a fresh independent review must accept the then-current R7.10 production state.

## Exact next action

```text
operator: run a fresh full RD8 validation on current main without --resume
→ record sanitized elapsed/peak-memory/coverage/workspace evidence
→ run 2-3 Level-1 known-answer probes from the generated workspace
→ resolve the runtime/plugin applicability evidence gap
→ if a concrete blocker appears, reproduce it regression-first with a synthetic fixture and fix only that blocker
→ if all RD8 bullets pass, record RD8 PASS and E0 PASS
```
