# PKC Status

Last updated: 2026-09-27

## Repository state reviewed

Current `main`:

```text
9e2397e83f56df2533fecc7184cadc80b74acddb
Continue
```

Active RD8-B repair branch:

```text
codex/rd8-b-same-line-conditional-state
production candidate: 82b5ffd7aac7db491042873b594ef2c5e987b55b
fix: harden conditional state effect branches
parent repair: 7eab43dd14de114f1328aa94f2959a3b3aa0fcac
fix: preserve conditional state effect branches
```

Draft PR: `#7 fix: harden conditional state effect branches`.

`main` has not been modified by this checkpoint. The candidate remains on a topic branch pending the approved private RD8-B re-probe.

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
V0.4.7-E0 repository discovery + bounded run        ACTIVE
V0.4.7-E0 / RD1-RD7                                 PASS / COMPLETE
V0.4.7-E0 / RD8-A operability                       PASS
V0.4.7-E0 / RD8-C runtime/plugin real-target proof  PASS
V0.4.7-E0 / RD8-B targeted product value            NOT PASS / REPAIR (a) AWAITING PRIVATE RE-PROBE
V0.4.7-E1 remaining product-value repairs           LOCKED behind E0
V0.4.7-E2 final product acceptance / R7.14          LOCKED behind E1
continuous update/diff                              LOCKED
V0.5 Azure DevOps input evidence                    LOCKED
```

Do not advance E1 or resume formal R7.10/D acceptance until the active RD8/E0 checkpoint explicitly permits it.

## RD8 accepted evidence

Authoritative sanitized result:

`docs/reviews/2026-09-25-rd8-private-validation-result.md`

RD8-C is PASS: the approved large target produced two deterministic HIGH `runtime-plugin-load` edges after the bounded runtime-plugin repair.

RD8-A is PASS on the approved large target:

```text
fresh run:              ~31.1 min
peak process-tree RAM:  ~5.42 GB
repository files:       25,355
semantic planned:       16,712
semantic executed:      16,709
facts:                  265,120
relations:              1,206,340
workflow candidates:    4,186
product features:       703
workspace files:        ~4,711
```

These results prove practical integrated operation; they do not prove RD8-B product value.

## RD8-B baseline — still NOT PASS

Known-answer probes from the approved private run:

```text
#7 scheduled updates / invoice period   FAIL    ~27%
#1 LostDate / CustomerWeb access        FAIL    ~38%  (calibration blocker)
#10 Worklog                             PARTIAL ~55%
```

The #1 blocker was that mutually exclusive state-effect branches could be rendered together as one proven effect.

Separate later candidate gaps remain:

- (b) deferred command-queue producer -> handler linking when the handler is named by type-name string;
- (c) recurring background jobs as workflow triggers;
- static factory / bulk-insert entity creation and other benchmark-driven gaps only when a later probe proves they are next-highest ROI.

Do not combine (b) or (c) into repair (a).

## Repair (a) — repo-local implementation complete, private proof pending

Primary implementation spec:

`docs/plans/2026-09-25-exclusive-branch-state-effects-spec.md`

`7eab43d` implemented branch context and honest if/else, else-if, switch and nested state-effect rendering. Independent source review in this web checkpoint found two additional compile-valid generic defects inside that same repair boundary:

1. same-line mutations to the same target could collide on fact ID and be dropped or attached to the wrong branch;
2. a branch arm containing both direct and nested mutations could index past a shorter branch path during knowledge rendering.

Candidate `82b5ffd` closes both defects regression-first and also makes the existing flat-output regression newline-neutral across Windows/Linux. It does not broaden business authority or add queue/scheduler support.

### Exact candidate verification

Exact production candidate:

```text
82b5ffd7aac7db491042873b594ef2c5e987b55b
fix: harden conditional state effect branches
```

Clean-environment gates on that candidate / PR merge ref:

```text
Release build:                    PASS, 0 warnings / 0 errors
C# tests:                         399 / 399 PASS
frontend tests:                   23 / 23 PASS
full solution total:              422 / 422 PASS
pack + local tool install:        PASS
WorkPlay end-to-end knowledge:    PASS
PokeTrade business + knowledge:   PASS
Loren external trial:             PASS
Jellyfin generalization trial:    PASS
```

CI run: `36258951904` / #439 — PASS after one retry of the failed PokeTrade job. The first PokeTrade attempt failed before Angular build because npm registry returned HTTP 404 for the `webpack-sources-3.6.0.tgz` tarball; the retry passed Angular build, business acceptance, and PKC knowledge verification without any code change.

Loren run: `36258951922` / #338 — PASS.

Jellyfin run: `36258951931` / #185 — PASS. This is especially material because the prior candidate exposed the mixed-depth branch renderer crash during Jellyfin synthesis; the final candidate passes the same real-repository gate.

This web environment has no local .NET runtime, so the final candidate was source-reviewed here and verified by the repository clean-environment gates. The earlier `7eab43d` handoff already recorded its local Windows verification before this hardening pass.

## RD8-OBS-1 — observation only

The approved real target previously emitted zero `applies-mapped-field-rule` relations.

Repo-local wiring review confirms mapped-field analysis is not dead code: semantic enrichment scans loaded project models for `mapped-field-rule` facts and links them only when a callable projects the matching destination type.

Do not add speculative parser support yet. On the next approved target run, first record:

```text
mapped-field-rule fact count
applies-mapped-field-rule relation count
```

Interpretation:

```text
facts = 0                 -> rule grammar/yield miss
facts > 0, relations = 0  -> projection/linking miss
```

Only the resulting evidence may authorize a repair.

## Exact next action

The current checkpoint has reached an external gate.

Approved source-enabled/company environment only:

```text
1. Build/run production candidate 82b5ffd on a fresh disposable copy of the approved target.
2. Run fresh `pkc run <target>`; do not stack repair (b) or (c) into this run.
3. Re-score RD8-B probe #1 first using the product-value benchmark protocol.
4. Confirm the mutually exclusive state branches are no longer merged into a false proven claim.
5. Record only sanitized benchmark evidence.
6. In the same generated evidence, record the two RD8-OBS-1 counts above.
```

If and only if probe #1 clears the calibration blocker, close repair (a). Then open repair (b) as a separate regression-first task driven by the deferred-queue miss. Repair (c) remains later and separate.

RD8-B remains **NOT PASS** until the required product-value evidence closes it. E0 remains active. Do not merge the parked E1 work or claim V0.4.7 complete.
