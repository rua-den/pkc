# PKC Handoff

Last updated: 2026-09-27

## Read first

1. root `CLAUDE.md`
2. `AGENTS.md`
3. `docs/status.md`
4. this handoff
5. `docs/milestones.md`
6. `docs/product-knowledge-contract.md`
7. `docs/v0.4.7-acceptance-plan.md`
8. `docs/reviews/2026-09-25-rd8-private-validation-result.md`
9. `docs/plans/2026-09-25-exclusive-branch-state-effects-spec.md`
10. `docs/benchmarks/product-value-benchmark-protocol.md`

Then verify current `main`, active topic branch, recent commits, production code and relevant regressions. Never reset to an older SHA merely because an older review names one.

## Current repository state

Current `main`:

```text
9e2397e83f56df2533fecc7184cadc80b74acddb
Continue
```

Active branch:

```text
codex/rd8-b-same-line-conditional-state
```

Exact production candidate awaiting the private product-value gate:

```text
82b5ffd7aac7db491042873b594ef2c5e987b55b
fix: harden conditional state effect branches
```

Its parent repair is:

```text
7eab43dd14de114f1328aa94f2959a3b3aa0fcac
fix: preserve conditional state effect branches
```

Draft PR: `#7 fix: harden conditional state effect branches`.

Do not merge to `main` merely because repository CI is green. RD8-B still requires the approved private re-probe.

## Current checkpoint state

```text
V0.4.7-D / R7.10                    OPEN / REVIEW PAUSED / NOT ACCEPTED
V0.4.7-E0                           ACTIVE
RD1-RD7                             PASS / COMPLETE
RD8-A operability                   PASS
RD8-C runtime/plugin target proof   PASS
RD8-B product value                 NOT PASS / REPAIR (a) AWAITING PRIVATE RE-PROBE
E1 remaining semantic repairs       LOCKED behind E0
E2 final acceptance / R7.14         LOCKED behind E1
```

## What the approved private run already proved

Sanitized source of truth:

`docs/reviews/2026-09-25-rd8-private-validation-result.md`

RD8-A passed at approximately 31.1 minutes and 5.42 GB peak process-tree memory, producing 265,120 facts, 1,206,340 relations, 4,186 workflow candidates and 703 product features on the approved large target.

RD8-C passed with two HIGH runtime-plugin edges on the real target.

RD8-B did not pass:

```text
probe #7 scheduled updates / invoice period   FAIL    ~27%
probe #1 LostDate / CustomerWeb access        FAIL    ~38%
probe #10 Worklog                             PARTIAL ~55%
```

Probe #1 contained a calibration blocker: mutually exclusive state-effect branches were merged into one proven effect.

## Repair (a) implementation state

Spec:

`docs/plans/2026-09-25-exclusive-branch-state-effects-spec.md`

`7eab43d` added branch context to mutation evidence and rendered if/else, else-if, switch and nested state effects as conditional alternatives instead of flattening them into one effect list.

Review of that implementation found two additional generic defects in the same boundary:

1. same-line same-target mutations could share the same fact ID and one could be dropped or mapped to the wrong syntax branch;
2. direct and nested mutations in the same branch arm could crash branch rendering through an out-of-range branch-path access.

`82b5ffd` fixes both regression-first. It also normalizes one regression assertion for cross-platform newline handling. No new business authority, queue semantics, scheduler semantics or target-specific source pattern was added.

## Verification on `82b5ffd`

Clean repository gates are green:

```text
Release build                     PASS, 0 warnings / 0 errors
C# tests                          399 / 399
frontend tests                    23 / 23
full solution                     422 / 422
pack/install                      PASS
WorkPlay                          PASS
PokeTrade                         PASS
Loren pinned external trial       PASS
Jellyfin generalization trial     PASS
```

GitHub Actions evidence:

```text
CI       36258951904 / #439  PASS
Loren    36258951922 / #338  PASS
Jellyfin 36258951931 / #185  PASS
```

The first PokeTrade attempt inside CI #439 failed during `npm install` because the npm registry returned HTTP 404 for a package tarball. No PKC code changed; rerunning failed jobs succeeded through Angular build, business acceptance and PKC knowledge verification.

Jellyfin is an important regression proof: the earlier candidate crashed while synthesizing a real Jellyfin workflow with mixed-depth branch mutations; `82b5ffd` passes the complete Jellyfin gate.

## RD8-OBS-1

The prior private target run emitted zero `applies-mapped-field-rule` relations.

Repo-local review confirms the mapped-field relation path is wired. Do not speculate on parser changes. On the next approved target run, record only:

```text
mapped-field-rule facts
applies-mapped-field-rule relations
```

Use the counts to classify the miss before touching code:

```text
facts = 0                 -> grammar/yield issue
facts > 0, relations = 0  -> projection/linking issue
```

## Do not start yet

These remain separate later tasks and are not part of repair (a):

```text
(b) deferred command-queue producer -> handler linking
(c) recurring background jobs as workflow triggers
```

Do not implement either before the private re-probe proves repair (a) solved probe #1's calibration defect. Otherwise the next target run would change multiple variables and would not tell us which repair improved the answer.

## Exact next action

This checkpoint is now blocked only by an external approved-environment gate.

In the source-enabled/company environment:

```text
1. Build production candidate 82b5ffd.
2. Run a fresh `pkc run <approved-disposable-target-copy>`.
3. Re-score RD8-B probe #1 first, workspace-only before source cross-check.
4. Verify exclusive branches are represented as alternatives and no false combined proven effect remains.
5. Record sanitized score + concrete remaining miss.
6. Record the RD8-OBS-1 fact/relation counts.
```

If probe #1 clears the calibration blocker, mark repair (a) closed and only then start repair (b) regression-first. If it does not clear, use the concrete probe miss to drive the next minimum generic correction inside repair (a).

Preserve these states until that evidence exists:

```text
RD8-B: NOT PASS
E0: ACTIVE / NOT COMPLETE
E1: LOCKED
R7.10/D: OPEN / REVIEW PAUSED
```
