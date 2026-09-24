# RD8 Current-Main Private Validation Runbook

Date: 2026-09-24
Status: OPERATOR PACKET / NO ACCEPTANCE CLAIM

This runbook makes the active V0.4.7-E0 / RD8 external gate reproducible. It does **not** mark RD8 or E0 PASS.

Use it only inside the approved source-enabled/company environment against an approved disposable copy of the private target repository. Do not export proprietary source, raw `.pkc/facts.json`, config values, internal repository names, endpoints, or source snippets into the PKC repository.

Authoritative state remains `docs/status.md` and `docs/handoff.md`.

## Goal

Produce enough sanitized evidence to decide RD8 without confusing three different questions:

1. **Operability:** does fresh current-main PKC complete practically on the intended repository class?
2. **Product value:** can the generated workspace answer selected PO/QC questions correctly and with calibrated uncertainty?
3. **Applicability:** were runtime/plugin shapes actually exercised, or are they legitimately absent/N/A for this repository?

A successful process exit or generated workspace alone is not RD8 acceptance.

## Phase 0 — freeze the candidate

From the PKC repository:

```powershell
git status --short
git rev-parse HEAD
git log -1 --oneline
dotnet --info
```

Requirements:

- use current `main`; do not reset to an older production SHA named by historical notes;
- if `main` moved after this runbook was written, reread `docs/status.md` and `docs/handoff.md` before running;
- record the exact PKC HEAD used in the sanitized review report;
- build from a clean enough checkout that local PKC edits cannot contaminate the measurement.

The CLI is `src/Pkc.Cli/Pkc.Cli.csproj`, assembly/tool name `pkc`, target framework `net10.0`.

Build once in Release:

```powershell
dotnet build PKC.sln -c Release
$PkcDll = (Resolve-Path '.\src\Pkc.Cli\bin\Release\net10.0\pkc.dll').Path
```

Use the built DLL directly for the measurement so the measured command is not a package/tool wrapper:

```powershell
dotnet $PkcDll <command> <repository-path>
```

## Phase A — fresh operability validation

### A1. Prepare a disposable target copy

Use an approved disposable copy of the private repository. On that copy only:

- close editors/terminals whose current directory is inside `.pkc/workspace`;
- remove any previous `.pkc` directory before the acceptance run;
- do not modify product source to help PKC pass;
- do not use `--resume` for the acceptance measurement.

Example:

```powershell
$Target = 'C:\approved\disposable-target-copy'
if (Test-Path "$Target\.pkc") {
    Remove-Item "$Target\.pkc" -Recurse -Force
}
```

### A2. Run discovery only

```powershell
$discover = [System.Diagnostics.Stopwatch]::StartNew()
dotnet $PkcDll discover $Target
$discover.Stop()
$discover.Elapsed
```

Record:

- exit code;
- elapsed time;
- discovered hosts/components;
- planned semantic files;
- excluded/test/light-index/UNKNOWN counts;
- whether repository shape looks plausible to an operator familiar with the target.

Inspect locally:

```text
.pkc/discovery/repository-profile.json
.pkc/discovery/scan-plan.json
```

Do not copy proprietary paths or names into the public/sanitized report.

### A3. Run a fresh full scan

Run without `--resume`:

```powershell
$run = [System.Diagnostics.Stopwatch]::StartNew()
dotnet $PkcDll run $Target
$exitCode = $LASTEXITCODE
$run.Stop()
$run.Elapsed
$exitCode
```

Capture peak memory with an approved local process monitor/sampler. Measure the `dotnet pkc` process tree rather than only a console wrapper. Record the measurement method in the report so later numbers are comparable.

Required measurements:

- full-run elapsed time;
- peak working set / peak resident memory;
- exit code;
- facts;
- relations;
- workflow candidates;
- product features;
- workspace path and file count;
- artifact-write failures, if any.

Compare with the historical supporting evidence only as context:

```text
original failure: ~41.5 min / ~18.6 GB / OOM during write
patched pre-integration run: ~29 min / ~7.5 GB / exit 0
```

There is no universal numeric RAM threshold. RD8 requires materially improved behavior and practical completion on the approved demo environment.

If the fresh run still peaks near the prior ~7.5 GB observation, profile the retained-memory phase before implementing another optimization. Do not optimize speculatively.

### A4. Inspect final artifacts

Inspect locally after the run:

```text
.pkc/RUN_SUMMARY.md
.pkc/run-summary.json
.pkc/discovery/repository-profile.json
.pkc/discovery/scan-plan.json
.pkc/discovery/scan-coverage.json
.pkc/checkpoint/scan-checkpoint.json
.pkc/workspace/_meta/coverage.json
```

If workspace replacement was blocked, the summary may correctly point at `.pkc/workspace.new`; use the path actually reported by the current run.

Plausibility checks:

- summary counts agree with persisted artifacts;
- coverage does not claim analyzed areas that were withheld/unsupported;
- UNKNOWN remains explicit rather than disappearing;
- test evidence does not become production authority;
- final workspace exists at the path named by the summary;
- no source/config bodies are exported into the portable workspace;
- no artifact failure is hidden behind a successful-looking summary.

## Phase B — targeted Level-1 product-value benchmark

Follow `docs/benchmarks/product-value-benchmark-protocol.md` exactly.

For every selected probe:

```text
Phase 1: generated .pkc/workspace only
         → record answer before any source inspection

Phase 2: approved source-enabled environment
         → establish the source-known answer

Phase 3: compare and score
```

Never let source knowledge leak back into the recorded Phase-1 answer.

Recommended probe mix:

1. **Mandatory rerun:** the quantity-adjustment visibility probe that drove `48ce2f10`.
2. **Condition/temporal probe:** one of the scheduled-update / bundle-pricing questions from `docs/question-trainning.md`.
3. **Workflow/permission/routing probe:** one of CustomerWeb access, inbound-email WO routing, or Worklog lifecycle from `docs/question-trainning.md`.

Two probes are enough if they cover the changed semantics well; use three when the first results expose materially different behavior.

For each probe score:

- correctness of the business answer;
- completeness of material conditions/effects;
- unsupported additions / hallucinations;
- uncertainty calibration;
- business wording rather than raw-code restatement;
- evidence/trace completeness.

Use protocol labels `PASS`, `PARTIAL`, `FAIL`, `N/A`.

Interpretation for RD8:

- any materially wrong or overconfident answer is a blocker;
- an honestly incomplete answer may remain `PARTIAL`, but the missing behavior must be explicit and must receive an acceptance disposition;
- do not weaken evidence authority merely to improve benchmark yield.

Keep the Phase-1 answer and source-known cross-check in the approved company environment if they contain proprietary detail. Commit only sanitized behavior-level scoring and misses.

## Phase C — runtime/plugin applicability disposition

The first private repository did not exercise runtime-dependency-index or runtime-plugin cases.

RD4 deterministic/synthetic coverage remains valid, but RD8 needs an explicit real-repository disposition:

### Option 1 — accepted N/A

Use only when the approved target is source-checked and demonstrably contains no applicable runtime/plugin shape.

Record sanitized evidence such as:

```text
runtime/plugin applicability: N/A
reason: approved target has no supported runtime-loaded plugin topology after source-enabled inspection
```

Do not publish internal plugin/project names.

### Option 2 — validate on another approved real corpus

If applicable runtime/plugin shapes exist elsewhere, validate:

- loader identity provenance;
- build/copy delivery provenance;
- correct host ownership;
- test-only loads remaining test evidence;
- ambiguous/non-literal identities staying unresolved/UNKNOWN.

Do not promote deterministic synthetic evidence into a claim that the private target exercised the same shape.

## Phase D — decision

Create a sanitized review from `docs/reviews/rd8-private-validation-template.md`.

RD8 can be marked PASS only when all current acceptance bullets have an explicit disposition:

```text
fresh current-main run completes practically
+ elapsed/memory/coverage/workspace evidence recorded
+ summaries/artifacts plausible
+ targeted Level-1 answers useful and correctly uncertain
+ no materially misleading answer accepted
+ runtime/plugin applicability explicitly PASS or accepted N/A
```

If a concrete blocker appears:

```text
private observation
→ sanitize the defect shape
→ reproduce with a synthetic PKC regression fixture
→ minimum generic repair
→ focused + related + broader local verification
→ one coherent commit/push
→ rerun only the affected RD8 evidence
```

Do not start E1 merely because RD8 found an interesting semantic gap. E1 remains locked until RD8 and E0 are explicitly PASS.
