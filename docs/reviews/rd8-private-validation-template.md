# RD8 Private Validation Report Template

Status: TEMPLATE ONLY — NOT EVIDENCE / NOT ACCEPTANCE

Use this file as the structure for the sanitized RD8 review after executing `docs/plans/2026-09-24-rd8-current-main-validation-runbook.md` inside the approved source-enabled/company environment.

Do not commit proprietary repository names, paths, endpoints, source snippets, raw fact payloads, config values, secrets, credentials, or generated private `.pkc/` contents.

Copy this template to a dated review file, for example:

```text
docs/reviews/2026-09-24-rd8-private-validation.md
```

Then replace every placeholder with actual sanitized evidence.

---

## Decision

```text
RD8: <PASS | PARTIAL | FAIL>
E0:  <PASS only if RD8 closes all E0 bullets; otherwise ACTIVE>
```

Reason in one paragraph:

`<concise decision rationale>`

## Candidate identity

```text
PKC main HEAD:             <sha>
latest production ancestor:<sha if HEAD is docs-only, otherwise same SHA>
PKC build:                 Release / net10.0
private target identity:   <sanitized operator-local label only, no internal URL/name>
measurement method:        <how elapsed + peak process-tree memory were measured>
```

Working-state note:

`<clean/known local state; mention any approved local-only instrumentation that did not alter PKC behavior>`

## Phase A — fresh operability run

Freshness conditions:

```text
[ ] disposable target copy
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
| Light-index files | `<count>` |
| Excluded areas/files | `<count>` |
| UNKNOWN areas/files | `<count>` |

Operator plausibility assessment:

`<PASS/PARTIAL/FAIL + short sanitized rationale>`

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

`<PASS/PARTIAL/FAIL + why the behavior is or is not practical on the approved demo environment>`

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

Keep detailed Phase-1 answers and source-known cross-checks inside the approved company environment when they contain proprietary detail. The committed report should contain only sanitized behavior-level scoring and misses.

### Probe 1 — quantity-adjustment visibility

```text
Selected because: reruns the private probe that drove 48ce2f10
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
| Business correctness | `<PASS/PARTIAL/FAIL + score if useful>` |
| Material condition/effect completeness | `<PASS/PARTIAL/FAIL + score>` |
| Unsupported additions | `<none / sanitized miss>` |
| Uncertainty calibration | `<PASS/PARTIAL/FAIL>` |
| Business wording | `<PASS/PARTIAL/FAIL>` |
| Evidence/trace completeness | `<PASS/PARTIAL/FAIL + score>` |
| Overall | `<PASS/PARTIAL/FAIL>` |

Concrete sanitized miss, if any:

`<behavior-level description only>`

### Probe 3 — `<optional question id / sanitized label>`

Use only when needed to cover materially different behavior or when the first two probes reveal broader risk.

| Dimension | Result |
| --- | --- |
| Business correctness | `<PASS/PARTIAL/FAIL + score if useful>` |
| Material condition/effect completeness | `<PASS/PARTIAL/FAIL + score>` |
| Unsupported additions | `<none / sanitized miss>` |
| Uncertainty calibration | `<PASS/PARTIAL/FAIL>` |
| Business wording | `<PASS/PARTIAL/FAIL>` |
| Evidence/trace completeness | `<PASS/PARTIAL/FAIL + score>` |
| Overall | `<PASS/PARTIAL/FAIL>` |

Concrete sanitized miss, if any:

`<behavior-level description only>`

### Level-1 summary

| Probe | Correctness | Completeness | Uncertainty | Trace | Overall |
| --- | ---: | ---: | ---: | ---: | --- |
| Quantity adjustment | `<score>` | `<score>` | `<score>` | `<score>` | `<PASS/PARTIAL/FAIL>` |
| Probe 2 | `<score>` | `<score>` | `<score>` | `<score>` | `<PASS/PARTIAL/FAIL>` |
| Probe 3 | `<score/N/A>` | `<score/N/A>` | `<score/N/A>` | `<score/N/A>` | `<PASS/PARTIAL/FAIL/N/A>` |

Product-value disposition:

`<Are answers materially useful and correctly uncertain? Identify any blocker without source dump.>`

## Phase C — runtime/plugin applicability

```text
Disposition: <PASS | accepted N/A | PARTIAL | FAIL>
```

Sanitized basis:

`<state whether the approved target contains applicable runtime/plugin topology; if absent, say absence was source-checked; if present elsewhere, summarize real-corpus validation without internal names>`

Checks when applicable:

| Check | Result |
| --- | --- |
| loader identity provenance | `<PASS/PARTIAL/FAIL/N/A>` |
| build/copy delivery provenance | `<PASS/PARTIAL/FAIL/N/A>` |
| host ownership | `<PASS/PARTIAL/FAIL/N/A>` |
| test-only load isolation | `<PASS/PARTIAL/FAIL/N/A>` |
| ambiguous identity remains unresolved/UNKNOWN | `<PASS/PARTIAL/FAIL/N/A>` |

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
| runtime/plugin applicability disposition explicit | `<PASS/PARTIAL/FAIL>` | `<note>` |

## Concrete blockers / non-blocking gaps

### Blockers

- `<none, or sanitized blocker>`

### Non-blocking gaps / E1 candidates

- `<gap; do not promote to E1 work until E0 is explicitly PASS>`

## Required next action

If RD8 PASS:

```text
update status/handoff/milestones/acceptance plan
→ mark RD8 PASS
→ mark E0 PASS if all E0 bullets are satisfied
→ unlock only E1
```

If RD8 PARTIAL/FAIL:

```text
choose the highest-ROI concrete blocker
→ reproduce with a synthetic PKC regression fixture
→ minimum generic repair
→ focused/related/broader local verification
→ one coherent commit/push
→ rerun only the affected RD8 evidence
```

Do not use CI as the edit/test loop and do not start the next milestone while RD8/E0 remain open.
