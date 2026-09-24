# Demo-Critical Sequential Execution Plan

Date: 2026-09-24
Status: ACTIVE — RD8 CURRENT-MAIN VALIDATION

This plan is superseded where necessary by:

`docs/plans/2026-09-24-demo-scan-priority-override.md`

Formal R7.10 / V0.4.7-D remains OPEN and NOT ACCEPTED. Repository Discovery / bounded execution is authorized as bounded infrastructure prework under that open gate. No E0 work may claim D PASS or weaken existing semantic authority.

## Product decision

The demo path is credible only if the normal PKC command can practically produce a trustworthy AI workspace on the intended repository class **and** that workspace can answer selected known product questions usefully and honestly.

The original private-repository failure mode was:

```text
whole-root style expensive analysis
→ large retained semantic state
→ post-scan artifact write pressure
→ ~18.6 GB peak on first RD7 private run
→ OOM before a usable workspace
```

Repository discovery, scoped execution and large-run repairs materially changed that architecture. The active question is now whether the **integrated current main** passes the real private-repository gate.

## Runtime product constraint

PKC Repository Discovery and repository compilation must be local/deterministic and require zero AI tokens.

Do not add runtime calls to Claude/OpenAI/Gemini or any hosted LLM. Do not require API keys. Do not upload source to an LLM.

LLM use is allowed only after compilation for product/QA consumption or benchmark/oracle work in an approved environment.

## Authoritative active order

```text
E0 Repository Discovery + bounded run prework — ACTIVE
        ↓
RD1 inventory + safe exclusion — PASS / COMPLETE
        ↓
RD2 application boundaries + ownership — PASS / COMPLETE
        ↓
RD3 vendor/custom frontend classification — PASS / COMPLETE
        ↓
RD4 runtime/plugin provenance — PASS / COMPLETE (deterministic/synthetic gate)
        ↓
RD5 deterministic ScanPlan — PASS / COMPLETE
        ↓
RD6 scoped/bounded semantic execution — PASS / COMPLETE
        ↓
RD7 coverage + observability + plan-only inspection — PASS / COMPLETE
        ↓
RD8 current-main private large-repository validation — ACTIVE
        ↓
practical workspace + targeted product-value sign-off
        ↓
E0 PASS
        ↓
E1 remaining semantic/product-value gaps
        ↓
formal R7.10/D independent acceptance if still open
        ↓
E2 real-project PO/QC product acceptance
```

Production checkpoints remain sequential.

## Repository state entering RD8

Immediately before the docs reconciliation, remote `main` was:

```text
9ef914f57c558e3b2963b9ba4dd187a6ecd6603f
Add question in demo
```

Latest production-code commit beneath it:

```text
fa30e3eb140357da5453702090be307417abb4e2
fix: report the preserved workspace when it could not replace the current one
```

Current descendant HEAD completed all four observed GitHub Actions workflows successfully.

Key merged current-main production work:

```text
612fa998  large-run streaming/atomic writes + shared workspace + resume + run summary
48ce2f10  sole-implementation + mapped-field/gate/guard/custom-auth rule surface
fa30e3eb  accurate workspace.new summary/reporting fallback
```

## RD1-RD7 reconciliation

RD1-RD7 no longer have pending push/CI gates. Their implementation commits and evidence are on `main`, and later descendant CI is green.

The detailed historical acceptance evidence remains in the per-RD review files. This plan records only the current execution state.

## RD8 — current-main private large-repository validation — ACTIVE

### Existing supporting evidence

Sanitized first private exercise:

```text
repository: ~26.9k files / 18 hosts
planned semantic files: ~16.8k
facts: ~263.6k
relations: ~2.04M
workflow candidates: 4,186

discover: ~98 s
run 1 (RD7): 41.5 min / ~18.6 GB / OOM at write
run 2 (resource-patched): 29 min / ~7.5 GB / exit 0 / workspace generated
```

Run 2 generated about 703 product features and 4,709 workspace files.

The resource repairs used by run 2 are now merged in `612fa998`, but the successful run predates the fully integrated current production state. It is therefore supporting evidence only.

### RD8-A — fresh current-main operability

Run only in the approved source-enabled/company environment against an approved repository copy:

```text
build PKC Release from current main
pkc discover <private-repo-copy>
pkc run <private-repo-copy>          # no --resume for acceptance measurement
```

Capture sanitized:

- discovery elapsed time;
- full-run elapsed time;
- peak working set;
- application/plan/coverage counts;
- planned/executed/withheld/UNKNOWN semantic coverage;
- facts, relations, workflows/product features;
- artifact write status;
- generated workspace path and file count.

Confirm:

- `RUN_SUMMARY.md` / `run-summary.json` are plausible;
- coverage remains honest;
- the workspace is usable;
- blocked workspace replacement, if encountered, reports `.pkc/workspace.new` correctly.

There is no universal hard RAM threshold. PASS means the normal product path materially improves the original failure mode and completes practically on the approved demo environment.

If the fresh run remains memory-heavy near the prior ~7.5 GB observation, profile the actual retained-memory phase before optimizing. Do not perform speculative Roslyn/memory refactors.

### RD8-B — targeted Level-1 product-value benchmark

After a fresh workspace is produced, run 2-3 known-answer probes under:

`docs/benchmarks/product-value-benchmark-protocol.md`

Preferred inputs:

- selected questions in `docs/question-trainning.md`;
- the quantity-adjustment probe that drove `48ce2f10`.

For each probe:

```text
workspace-only answer first
→ source-enabled known-answer cross-check second
→ score third
```

Score:

- correctness;
- completeness of material conditions/business effects;
- unsupported additions;
- uncertainty calibration;
- business-language quality;
- evidence/trace completeness.

Do not accept a benchmark merely because the agent returned text or because PKC generated a workspace.

### RD8-C — runtime/plugin real-repository applicability

The first private repository did not exercise runtime-dependency-index or runtime-plugin cases.

Before RD8 PASS:

- record `N/A` only if the repository is demonstrably free of those shapes and the reviewer explicitly accepts that absence; or
- validate another approved real repository/corpus that contains them.

RD4 synthetic/deterministic coverage remains PASS but is not a substitute for claiming those shapes were exercised in the first private repository.

## RD8 exit

RD8 is PASS only when RD8-A, RD8-B and RD8-C are explicitly closed.

## E0 completion rule

E0 is PASS only when:

```text
RD1-RD7 complete
+ RD8 PASS
+ normal product path practically produces the intended workspace
+ coverage is honest
+ selected demo answers are useful and correctly uncertain
```

JSON output alone is insufficient.

## E1 — remaining semantic/product-value gaps

Only after E0 PASS.

Parked construction candidate:

```text
35c8e5c5f856e15568aa963bb2d76268008c5570
fix: prove observable constructed state
```

Candidate gaps observed in the first private workspace include mediator dispatch, legacy-UI linkage and remaining rule-language cleanup. Do not implement them speculatively during RD8; let the fresh benchmark choose the next actual repair.

## Formal D boundary

R7.10/D remains OPEN. The independent gate is paused, not passed. Resume and complete a fresh independent review before final V0.4.7 acceptance.

## Explicitly deferred

Unless RD8 exposes a direct blocker, do not mix in:

- continuous update/diff;
- Azure DevOps evidence;
- generic JavaScript dataflow;
- arbitrary runtime instrumentation;
- automatic dead-code deletion;
- unrelated source cleanup recommendations;
- LLM-dependent repository classification;
- parked construction/default/computation work.

## Execution discipline

When RD8 exposes a concrete implementation blocker:

```text
inspect evidence
→ focused synthetic regression
→ confirm regression represents the real defect
→ minimum generic fix
→ focused tests
→ related tests
→ broader relevant verification
→ diff review
→ one coherent commit
→ one push
→ CI final verification
→ status/handoff update
```

Do not use GitHub Actions as the edit-test loop.

## Immediate next action

```text
RUN RD8-A ON CURRENT MAIN NOW
→ fresh discover
→ fresh full run without --resume
→ sanitized resource/coverage/workspace evidence
→ RD8-B targeted Level-1 benchmark
→ RD8-C applicability disposition
```
