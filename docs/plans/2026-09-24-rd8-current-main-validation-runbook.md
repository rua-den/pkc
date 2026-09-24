# RD8 Current-Main Private Validation Runbook

Date: 2026-09-24
Status: OPERATOR PACKET / NO ACCEPTANCE CLAIM

This runbook makes V0.4.7-E0 / RD8 reproducible inside the approved source-enabled/company environment. It does **not** mark RD8 or E0 PASS.

Do not export proprietary source, raw `.pkc/facts.json`, config values, internal repository names/endpoints, credentials or source snippets into the PKC repository. Commit only sanitized behavior-level evidence.

Authoritative state remains `docs/status.md`, `docs/handoff.md` and `docs/v0.4.7-acceptance-plan.md`.

## Goal

Close three distinct questions:

1. **Operability (A):** does fresh current-main PKC complete practically on the intended repository class?
2. **Product value (B):** can the generated workspace answer selected PO/QC questions correctly and with calibrated uncertainty?
3. **Runtime topology (C):** does PKC correctly recover the intended target's known runtime-plugin topology?

A successful process exit or generated workspace alone is not RD8 acceptance.

## Important Phase-C correction

A saved sanitized reconnaissance proves the intended target **does contain runtime plugins** loaded by reflection and delivered by a custom post-build copy, without a host project reference.

Therefore `runtime/plugin = N/A` is **not** an available disposition for this intended target.

Earlier private PKC output that did not show the runtime-plugin relationship must be treated as missing discovery yield, not proof that the architecture lacks it.

See `docs/reviews/2026-09-24-rd8-runtime-plugin-target-applicability.md`.

## Phase 0 — freeze the candidate

From the PKC repository:

```powershell
git status --short
git rev-parse HEAD
git log -1 --oneline
dotnet --info
```

Requirements:

- use current `main`; do not reset to an old production SHA;
- if `main` moved, reread status/handoff first;
- record exact PKC HEAD in the sanitized report;
- do not measure from a checkout containing unreviewed local PKC behavior changes.

Build once in Release:

```powershell
dotnet build PKC.sln -c Release
$PkcDll = (Resolve-Path '.\src\Pkc.Cli\bin\Release\net10.0\pkc.dll').Path
```

Use the built DLL directly:

```powershell
dotnet $PkcDll <command> <repository-path>
```

## Phase C0 — bounded runtime-plugin defect-shape inspection

Do this before implementing any Phase-C repair.

The sanitized architecture already proves this high-level shape:

```text
host
  -> reflection-based runtime load from an output subfolder
  -> plugin projects are not host project references

build delivery
  -> custom post-build copy into host runtime output
```

Inspect only enough approved private source/build metadata to classify the deterministic syntax shape.

Record locally:

```text
loader syntax family:               <sanitized shape>
build/copy syntax family:           <sanitized shape>
solution build-dependency involved: <yes/no + sanitized role>
current PKC discovery result:       <edge / unresolved / missing>
```

Examples such as `Directory.GetFiles`, `Assembly.LoadFrom`, `PostBuildEvent`, `xcopy`, an MSBuild `Target`, or a `.sln` `ProjectDependencies` section are **not assumptions**. Record them only if actually observed.

Do not copy proprietary names or literal paths. Replace them with neutral fixture names such as `Host`, `PluginA`, `PluginB`, `plugins/`.

### C0 decision

If current main already represents the exact target shape with deterministic loader identity + delivery provenance, retain evidence and proceed to C2.

If current main does not represent it:

```text
exact sanitized defect shape
-> create a focused synthetic regression fixture
-> verify current main fails that regression for the intended reason
-> implement the minimum generic repair
-> add negative regressions for ambiguous / incomplete evidence
-> run focused, related and full relevant local verification
-> one coherent implementation commit/push
```

Never create an authoritative runtime edge from names, folder proximity, copy-only evidence, loader-only evidence or ambiguous identity.

## Phase A — fresh operability validation

### A1. Prepare an approved disposable target copy

On the disposable target only:

- close editors/terminals locking `.pkc/workspace`;
- remove previous `.pkc` before the acceptance run;
- do not modify product source to help PKC pass;
- do not use `--resume`.

Example:

```powershell
$Target = 'C:\approved\disposable-target-copy'
if (Test-Path "$Target\.pkc") {
    Remove-Item "$Target\.pkc" -Recurse -Force
}
```

### A2. Discovery only

```powershell
$discover = [System.Diagnostics.Stopwatch]::StartNew()
dotnet $PkcDll discover $Target
$discoverExit = $LASTEXITCODE
$discover.Stop()
$discover.Elapsed
$discoverExit
```

Record:

- exit code and elapsed;
- repository files and components/hosts;
- planned semantic files;
- test-evidence/light-index/runtime-index counts;
- excluded and UNKNOWN counts;
- whether boundaries look plausible to an operator familiar with the target;
- **whether the known runtime-plugin topology is now represented or explicitly unresolved for a deterministic reason.**

Inspect locally:

```text
.pkc/discovery/repository-profile.json
.pkc/discovery/scan-plan.json
```

Do not publish proprietary paths or names.

### A3. Fresh full run

Run without `--resume`:

```powershell
$run = [System.Diagnostics.Stopwatch]::StartNew()
dotnet $PkcDll run $Target
$runExit = $LASTEXITCODE
$run.Stop()
$run.Elapsed
$runExit
```

Capture peak memory with an approved local process monitor/sampler. Measure the `dotnet pkc` process tree rather than only a wrapper console and record the method.

Required measurements:

- full-run elapsed time;
- peak working set / resident memory;
- exit code;
- facts and relations;
- workflow candidates and product features;
- workspace path/file count;
- artifact-write failures.

Historical context only:

```text
original failure: ~41.5 min / ~18.6 GB / OOM during write
patched pre-integration run: ~29 min / ~7.5 GB / exit 0
```

There is no universal RAM threshold. PASS requires materially improved behavior and practical completion on the approved demo environment.

If the fresh run remains near the previous ~7.5 GB peak, profile the retained-memory phase before changing code; do not optimize speculatively.

### A4. Artifact plausibility

Inspect locally:

```text
.pkc/RUN_SUMMARY.md
.pkc/run-summary.json
.pkc/discovery/repository-profile.json
.pkc/discovery/scan-plan.json
.pkc/discovery/scan-coverage.json
.pkc/checkpoint/scan-checkpoint.json
.pkc/workspace/_meta/coverage.json
```

If workspace replacement was blocked, use the actual path reported by the current summary, including `.pkc/workspace.new` when applicable.

Check:

- summary counts agree with persisted artifacts;
- coverage does not claim analyzed areas that were withheld/unsupported;
- UNKNOWN remains explicit;
- tests do not become production authority;
- final workspace exists at the path named by the summary;
- portable workspace contains no source/config bodies;
- artifact failures are visible.

## Phase B — targeted Level-1 product-value benchmark

Follow `docs/benchmarks/product-value-benchmark-protocol.md` exactly.

For each selected probe:

```text
Phase 1: generated .pkc/workspace only
         -> record answer before source inspection

Phase 2: approved source-enabled environment
         -> establish the source-known answer

Phase 3: compare and score
```

Never let source knowledge leak back into the recorded Phase-1 answer.

Recommended mix:

1. mandatory rerun: quantity-adjustment visibility probe that drove `48ce2f10`;
2. condition/temporal probe from `docs/question-trainning.md`;
3. workflow/permission/routing probe when a third question adds materially different coverage.

Score:

- business correctness;
- material condition/effect completeness;
- unsupported additions;
- uncertainty calibration;
- business wording;
- evidence/trace completeness.

Use `PASS`, `PARTIAL`, `FAIL`, `N/A` per the benchmark protocol. Any materially wrong or overconfident answer is a blocker.

Keep proprietary detailed answers/source-known cross-checks inside the approved environment. Commit only sanitized scoring and behavior-level misses.

## Phase C2 — runtime/plugin real-target proof

After any necessary repair, rerun `pkc discover` on a clean approved target copy and inspect the discovery metadata.

RD8-C PASS requires deterministic proof appropriate to the actual source shape:

- loader provenance is visible;
- plugin identity is proven rather than guessed;
- build/copy delivery provenance is visible;
- correct host ownership follows from the proven edge;
- test-only loads stay test evidence;
- ambiguous/non-literal/unproven alternatives stay unresolved/UNKNOWN;
- the intended runtime plugin modules are no longer falsely absent from topology merely because the host has no project reference.

The report may use aliases/counts only; do not publish real project names or paths.

## Decision

Create a sanitized review from `docs/reviews/rd8-private-validation-template.md`.

RD8 can be PASS only when:

```text
A: fresh current-main run completes practically
+ elapsed/memory/coverage/workspace evidence recorded
+ summaries/artifacts plausible

B: targeted Level-1 answers are useful and correctly uncertain
+ no materially misleading answer is accepted

C: known applicable runtime-plugin topology is deterministically represented on the intended target
+ incomplete/ambiguous variants remain fail-closed
```

If another concrete blocker appears:

```text
private observation
-> sanitize defect shape
-> reproduce with synthetic regression
-> minimum generic repair
-> focused + related + broader local verification
-> one coherent commit/push
-> rerun only affected RD8 evidence
```

Do not start E1 while RD8/E0 remain open.