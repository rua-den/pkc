# RD8-B Private Re-probe Runbook

Date: 2026-09-27
Checkpoint: V0.4.7-E0 / RD8-B repair (a)
Semantic candidate: `8c4055decd56e4597b2a1aa03e24b5c1a70235d5`

This runbook exists to make the next approved private run deterministic and to avoid mixing later semantic repairs into the calibration check.

It does **not** replace `docs/benchmarks/product-value-benchmark-protocol.md`. The protocol remains authoritative.

## Purpose

The next private run has exactly two goals:

1. determine whether repair (a) removed probe #1's false combined state-effect authority;
2. classify RD8-OBS-1 by counting `mapped-field-rule` facts and `applies-mapped-field-rule` relations.

Do not evaluate repair (b) or (c) in this run and do not modify the target repository to improve PKC output.

## Privacy boundary

Use only the approved source-enabled/company environment.

Never commit or paste into PKC:

- target repository names or paths;
- source-code bodies;
- raw `.pkc/facts.json` content;
- proprietary file contents;
- credentials, configuration values, identities or secrets.

The final report may contain only sanitized behavior-level findings, scores, counts and concise evidence classifications allowed by the benchmark protocol.

## 1. Verify exact PKC candidate

From a clean PKC checkout:

```powershell
$Expected = "8c4055decd56e4597b2a1aa03e24b5c1a70235d5"
$Actual = (git rev-parse HEAD).Trim()
if ($Actual -ne $Expected) { throw "Wrong PKC SHA. Expected $Expected, got $Actual" }

git status --short
```

The working tree must be clean before build/run.

Build the exact candidate:

```powershell
dotnet build PKC.sln --configuration Release
if ($LASTEXITCODE -ne 0) { throw "PKC Release build failed" }
```

Use the built CLI from this checkout. Do not rebuild from another branch between verification and the target run.

## 2. Prepare a fresh disposable target copy

Set the approved disposable target path locally:

```powershell
$Target = "<approved-disposable-target-copy>"
```

Before running PKC:

- confirm this is a disposable copy, not the original checkout;
- confirm the original source remains read-only for phase-2 cross-check;
- remove any old `.pkc` from the disposable copy;
- do not use `--resume`.

Example guard:

```powershell
if (Test-Path (Join-Path $Target ".pkc")) {
    throw "Target already contains .pkc. Use a fresh disposable copy."
}
```

## 3. Run PKC once

Preferred product UX:

```powershell
pkc run $Target
if ($LASTEXITCODE -ne 0) { throw "pkc run failed" }
```

If the machine is intentionally using the Release DLL from the verified checkout instead of an installed tool, invoke that exact DLL. Record locally which invocation was used, but do not put target paths in the sanitized report.

After completion require:

```text
<target>/.pkc/facts.json
<target>/.pkc/workspace/
<target>/.pkc/workspace/knowledge/START_HERE.md
<target>/.pkc/workspace/_policy/answer-contract.md
<target>/.pkc/workspace/_meta/catalog.json
```

A successful compiler exit alone is not product-value acceptance.

## 4. Record RD8-OBS-1 counts before source inspection

From the fresh `.pkc/facts.json`, record only these aggregate counts:

```text
mapped-field-rule facts = ?
applies-mapped-field-rule relations = ?
```

Do not copy matching raw facts or relations into the report.

Classification:

```text
facts = 0                         -> grammar/yield miss
facts > 0 and relations = 0      -> projection/linking miss
relations > 0                    -> relation path is active on this run
```

This classification is observation-only. It does not authorize a parser/linker repair in this checkpoint.

## 5. Phase 1 — workspace-only probe #1

Before looking at target source, give the answering agent access only to:

```text
<target>/.pkc/workspace
```

The agent must read the generated instructions/policy/catalog first.

Ask exactly the pinned probe:

> On 01/09, Customer A reported that they stopped working with us, so LostDate = 01/09. Today is 24/09, but IsCustomer is still true. Can the contact of Customer A still log in to CustomerWeb? If yes, when will they lose access?

Record the workspace-only answer **before** any source inspection.

For repair (a), inspect the answer specifically for state-effect authority:

```text
A. Are mutually exclusive deactivation branches represented as alternatives?
B. Does the answer avoid claiming that both exclusive branch effects happen together?
C. Does it avoid inventing an unconditional state change from conditional evidence?
D. If access timing is still not provable, does it say NOT PROVEN instead of guessing?
```

Do not mark repair (a) successful merely because the overall answer sounds better.

## 6. Phase 2 — source-known cross-check

Only after the Phase-1 answer is frozen, use the read-only original source in the approved environment to establish the known behavior.

Inspect only enough source to answer the selected probe and calibrate the exclusive branch behavior.

Do not paste source code into the PKC report.

## 7. Decision rule for repair (a)

Close repair (a) only when all of the following are true:

```text
- the previous false combined proven effect is gone;
- exclusive branches are represented as alternatives or otherwise not merged;
- no new unsupported unconditional effect replaces the old error;
- remaining uncertainty is honestly marked rather than guessed.
```

The overall probe may still be PARTIAL/FAIL because the separate authentication-event access-gate gap is known. That does not keep repair (a) open if its calibration defect is genuinely gone.

Decision matrix:

```text
false combined effect still present
    -> repair (a) remains OPEN
    -> record the concrete generated misstatement
    -> make the next minimum generic correction inside (a)

exclusive-branch calibration clears, but other probe #1 behavior is missing
    -> close repair (a)
    -> keep RD8-B NOT PASS
    -> open repair (b) next, per current handoff

exclusive-branch calibration clears and probe improves materially
    -> close repair (a)
    -> keep only the independently verified score/result
    -> open repair (b) next
```

Do not start repair (b) before this decision is recorded.

## 8. Sanitized result template

Commit only a sanitized result shaped like this:

```markdown
# RD8-B Repair (a) Private Re-probe Result

Date: YYYY-MM-DD
Semantic candidate: 8c4055decd56e4597b2a1aa03e24b5c1a70235d5
Environment: approved source-enabled company environment
Target: approved private target, sanitized

## Fresh run

- `pkc run`: PASS / FAIL
- workspace generated: YES / NO
- fresh run, no resume: YES / NO

## RD8-OBS-1

- `mapped-field-rule` facts: N
- `applies-mapped-field-rule` relations: N
- classification: grammar/yield | projection/linking | active

## Probe #1

- selected dimensions: Q1 permission/preconditions, Q2 state/effect
- score: N%
- label: PASS / PARTIAL / FAIL
- exclusive branches represented as alternatives: YES / NO
- false combined proven effect remains: YES / NO
- unsupported replacement authority introduced: YES / NO
- concrete remaining miss: <sanitized behavior-level description>

## Repair (a) verdict

PASS / NOT PASS

## Next action

<close (a) and open (b), or exact minimum correction still required inside (a)>
```

No source names, source bodies, raw evidence payloads or target paths belong in this report.

## 9. State that must not move early

Until the sanitized probe result exists:

```text
RD8-B: NOT PASS
E0: ACTIVE / NOT COMPLETE
E1: LOCKED
R7.10/D: OPEN / REVIEW PAUSED
```

Repair (b) deferred command-queue producer -> handler linking and repair (c) recurring background-job triggers remain unopened.