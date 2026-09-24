# RD8 Private Validation Report Template

Status: TEMPLATE ONLY — NOT EVIDENCE / NOT ACCEPTANCE

Use this structure after executing `docs/plans/2026-09-24-rd8-current-main-validation-runbook.md` inside the approved source-enabled/company environment.

Do not commit proprietary repository names, paths, endpoints, source snippets, raw fact payloads, config values, secrets, credentials, or generated private `.pkc/` contents.

Copy to a dated review file and replace every placeholder with actual sanitized evidence.

---

## Decision

```text
RD8: <PASS | PARTIAL | FAIL>
E0:  <PASS only if RD8 closes every E0 bullet; otherwise ACTIVE>
```

Reason:

`<concise decision rationale>`

## Candidate identity

```text
PKC main HEAD:              <sha>
latest production ancestor: <sha if docs-only HEAD; otherwise same SHA>
PKC build:                  Release / net10.0
private target identity:    <sanitized operator-local label only>
measurement method:         <elapsed + peak process-tree memory method>
```

Working-state note:

`<clean/known local state; approved local-only instrumentation if any>`

## Phase C0 — runtime-plugin exact-shape inspection

The intended target is already known, from sanitized reconnaissance, to contain reflection-loaded plugins delivered by a custom post-build copy without a host project reference. `N/A` is not a valid disposition for this target.

Record only sanitized syntax classes:

```text
loader syntax family:               <sanitized actual shape>
build/copy syntax family:           <sanitized actual shape>
solution build-dependency involved: <yes/no + sanitized role>
current-main discovery result:      <proven edge / unresolved / missing>
```

Do not write guessed implementation names. Examples such as `Directory.GetFiles`, `Assembly.LoadFrom`, `PostBuildEvent`, `xcopy`, `Target`, or `.sln ProjectDependencies` belong here only when actually observed.

Current-main capability assessment:

`<SUPPORTED / MISSING SHAPE / AMBIGUOUS + short rationale>`

If a repair was required:

```text
synthetic regression: <test/fixture identifier>
pre-fix result:       <RED / exact reason>
implementation:       <generic behavior only>
negative regressions: <what remains fail-closed>
local verification:   <focused / related / broader results>
implementation SHA:   <sha>
```

## Phase A — fresh operability run

Freshness conditions:

```text
[ ] approved disposable target copy
[ ] previous .pkc removed before acceptance run
[ ] no --resume
[ ] no product-source edits made to help PKC pass
[ ] no editor/session locking .pkc/workspace during the run
```

### Discovery

| Metric | Result |
| --- | ---: |
| Exit code | `<n>` |
| Elapsed | `<time>` |
| Repository files | `<count>` |
| Production hosts | `<count>` |
| Planned semantic files | `<count>` |
| Test-evidence files | `<count>` |
| Light/runtime-index files | `<count>` |
| Excluded areas/files | `<count>` |
| UNKNOWN areas/files | `<count>` |
| Runtime-plugin authoritative edges | `<count>` |
| Runtime-plugin unresolved findings | `<count>` |

Repository-boundary plausibility:

`<PASS/PARTIAL/FAIL + sanitized rationale>`

Known runtime-plugin topology disposition:

`<PASS/PARTIAL/FAIL + whether intended plugin path is represented deterministically>`

### Full `pkc run`

| Metric | Current-main result | Historical context |
| --- | ---: | ---: |
| Exit code | `<n>` | run 1 failed / run 2 exit 0 |
| Elapsed | `<time>` | ~41.5 min / ~29 min |
| Peak process-tree memory | `<value>` | ~18.6 GB / ~7.5 GB |
| Facts | `<count>` | ~263.6k before later semantics |
| Relations | `<count>` | ~2.04M before later semantics |
| Workflow candidates | `<count>` | ~4,186 before later semantics |
| Product features | `<count>` | ~703 in patched run 2 |
| Workspace files | `<count>` | ~4,709 in patched run 2 |
| Final workspace path | `<.pkc/workspace or .pkc/workspace.new>` | n/a |

Artifact failures:

`<none, or sanitized list>`

Operability assessment:

`<PASS/PARTIAL/FAIL + why behavior is/is not practical>`

## Artifact and coverage plausibility

| Check | Result | Sanitized note |
| --- | --- | --- |
| `RUN_SUMMARY.md` agrees with persisted counts | `<PASS/PARTIAL/FAIL>` | `<note>` |
| `run-summary.json` agrees with Markdown/console | `<PASS/PARTIAL/FAIL>` | `<note>` |
| scan plan accounts for enumerated files | `<PASS/PARTIAL/FAIL>` | `<note>` |
| coverage reports withheld/unsupported areas honestly | `<PASS/PARTIAL/FAIL>` | `<note>` |
| UNKNOWN remains explicit | `<PASS/PARTIAL/FAIL>` | `<note>` |
| tests remain non-production evidence | `<PASS/PARTIAL/FAIL>` | `<note>` |
| workspace path reported correctly | `<PASS/PARTIAL/FAIL>` | `<note>` |
| portable workspace contains no source/config bodies | `<PASS/PARTIAL/FAIL>` | `<note>` |
| failed artifact writes are visible | `<PASS/PARTIAL/FAIL/N/A>` | `<note>` |

## Phase B — targeted Level-1 benchmark

Protocol: `docs/benchmarks/product-value-benchmark-protocol.md`.

Keep detailed Phase-1 answers and source-known cross-checks inside the approved company environment when proprietary. Commit only sanitized behavior-level scoring and misses.

### Probe 1 — quantity-adjustment visibility

```text
Phase-1 workspace answer recorded before source inspection: <YES/NO>
```

| Dimension | Result |
| --- | --- |
| Business correctness | `<PASS/PARTIAL/FAIL + score if useful>` |
| Material condition completeness | `<PASS/PARTIAL/FAIL + score>` |
| Unsupported additions | `<none / sanitized miss>` |
| Uncertainty calibration | `<PASS/PARTIAL/FAIL>` |
| Business wording | `<PASS/PARTIAL/FAIL>` |
| Evidence/trace completeness | `<PASS/PARTIAL/FAIL + score>` |
| Overall | `<PASS/PARTIAL/FAIL>` |

Concrete sanitized miss, if any:

`<behavior-level description only>`

### Probe 2 — `<question id / sanitized label>`

Selection rationale:

`<why this probes a different semantic path>`

| Dimension | Result |
| --- | --- |
| Business correctness | `<PASS/PARTIAL/FAIL + score>` |
| Material condition/effect completeness | `<PASS/PARTIAL/FAIL + score>` |
| Unsupported additions | `<none / sanitized miss>` |
| Uncertainty calibration | `<PASS/PARTIAL/FAIL>` |
| Business wording | `<PASS/PARTIAL/FAIL>` |
| Evidence/trace completeness | `<PASS/PARTIAL/FAIL + score>` |
| Overall | `<PASS/PARTIAL/FAIL>` |

Concrete sanitized miss, if any:

`<behavior-level description only>`

### Probe 3 — `<optional question id / sanitized label>`

Use only when needed for materially different behavior.

| Dimension | Result |
| --- | --- |
| Business correctness | `<PASS/PARTIAL/FAIL + score>` |
| Material condition/effect completeness | `<PASS/PARTIAL/FAIL + score>` |
| Unsupported additions | `<none / sanitized miss>` |
| Uncertainty calibration | `<PASS/PARTIAL/FAIL>` |
| Business wording | `<PASS/PARTIAL/FAIL>` |
| Evidence/trace completeness | `<PASS/PARTIAL/FAIL + score>` |
| Overall | `<PASS/PARTIAL/FAIL>` |

### Level-1 summary

| Probe | Correctness | Completeness | Uncertainty | Trace | Overall |
| --- | ---: | ---: | ---: | ---: | --- |
| Quantity adjustment | `<score>` | `<score>` | `<score>` | `<score>` | `<PASS/PARTIAL/FAIL>` |
| Probe 2 | `<score>` | `<score>` | `<score>` | `<score>` | `<PASS/PARTIAL/FAIL>` |
| Probe 3 | `<score/N/A>` | `<score/N/A>` | `<score/N/A>` | `<score/N/A>` | `<PASS/PARTIAL/FAIL/N/A>` |

Product-value disposition:

`<Are answers materially useful and correctly uncertain?>`

## Phase C2 — runtime/plugin real-target proof

```text
Disposition: <PASS | PARTIAL | FAIL>
```

`accepted N/A` is intentionally absent because target applicability is already proven.

Sanitized basis:

`<how current-main discovery represents the known reflection-loader + build-delivery plugin topology>`

| Check | Result |
| --- | --- |
| loader provenance | `<PASS/PARTIAL/FAIL>` |
| plugin identity provenance | `<PASS/PARTIAL/FAIL>` |
| build/copy delivery provenance | `<PASS/PARTIAL/FAIL>` |
| correct host ownership | `<PASS/PARTIAL/FAIL>` |
| test-only load isolation | `<PASS/PARTIAL/FAIL/N/A>` |
| ambiguous/incomplete identity remains unresolved/UNKNOWN | `<PASS/PARTIAL/FAIL>` |
| no name/proximity-only authoritative linking | `<PASS/PARTIAL/FAIL>` |

## RD8 acceptance matrix

| Required property | Result | Evidence summary |
| --- | --- | --- |
| current-main fresh run completes practically | `<PASS/PARTIAL/FAIL>` | `<note>` |
| material resource improvement vs original failure | `<PASS/PARTIAL/FAIL>` | `<note>` |
| repository boundaries plausible | `<PASS/PARTIAL/FAIL>` | `<note>` |
| generated/restorable exclusion plausible | `<PASS/PARTIAL/FAIL>` | `<note>` |
| vendor/first-party separation plausible | `<PASS/PARTIAL/FAIL/N/A>` | `<note>` |
| shared ownership plausible | `<PASS/PARTIAL/FAIL/N/A>` | `<note>` |
| test isolation correct | `<PASS/PARTIAL/FAIL>` | `<note>` |
| UNKNOWN preservation honest | `<PASS/PARTIAL/FAIL>` | `<note>` |
| semantic scope reduction observable | `<PASS/PARTIAL/FAIL>` | `<note>` |
| workspace generated and usable | `<PASS/PARTIAL/FAIL>` | `<note>` |
| summaries/coverage internally plausible | `<PASS/PARTIAL/FAIL>` | `<note>` |
| Level-1 answers useful + correctly uncertain | `<PASS/PARTIAL/FAIL>` | `<note>` |
| intended runtime-plugin topology deterministically represented | `<PASS/PARTIAL/FAIL>` | `<note>` |

## Concrete blockers / non-blocking gaps

### Blockers

- `<none, or sanitized blocker>`

### Non-blocking gaps / E1 candidates

- `<gap; do not promote to E1 work until E0 PASS>`

## Required next action

If RD8 PASS:

```text
update status/handoff/milestones/acceptance plan
-> mark RD8 PASS
-> mark E0 PASS only if all E0 bullets are satisfied
-> unlock only E1
```

If RD8 PARTIAL/FAIL:

```text
choose highest-ROI concrete blocker
-> synthetic regression
-> minimum generic repair
-> focused/related/broader local verification
-> one coherent commit/push
-> rerun only affected RD8 evidence
```

Do not use CI as the edit/test loop and do not start E1 while RD8/E0 remain open.