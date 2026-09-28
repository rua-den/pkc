# PKC Handoff

Last updated: 2026-09-28

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
11. `docs/benchmarks/2026-09-27-rd8-private-reprobe-runbook.md`

Then verify current `main`, the active topic branch, recent commits, production code and relevant regressions. Never reset to an older SHA merely because an older review names one.

## Current repository state

Current `main` before the requested RD8 consolidation:

```text
9e2397e83f56df2533fecc7184cadc80b74acddb
Continue
```

RD8 integration source branch:

```text
codex/rd8-b-same-line-conditional-state
```

Exact semantic/production candidate for the private product-value gate:

```text
8c4055decd56e4597b2a1aa03e24b5c1a70235d5
fix: fail closed on ambiguous mutation owners
```

Existing draft PR before consolidation: `#7 fix: harden conditional state effect branches`.

The approved private re-probe is complete. The user explicitly directed consolidating the current RD8 branch chain into `main`; this does not make RD8-B or E0 PASS. Keep later checkpoints locked until their own evidence closes.

## Current checkpoint

```text
V0.4.7-D / R7.10                    OPEN / REVIEW PAUSED / NOT ACCEPTED
V0.4.7-E0                           ACTIVE
RD1-RD7                             PASS / COMPLETE
RD8-A operability                   PASS
RD8-C runtime/plugin target proof   PASS
RD8-B product value                 NOT PASS / REPAIR (a) CLOSED; REPAIR (b) NEXT
E1 remaining semantic repairs       LOCKED behind E0
E2 final acceptance / R7.14         LOCKED behind E1
```

## Repair (a) candidate

The candidate chain remains inside the same conditional-state correctness boundary:

```text
7eab43d  preserve conditional state effect branches
82b5ffd  harden same-line mutation identity + mixed-depth rendering
01d88b0  preserve relation ownership across distinct method names
8c4055d  fail closed when the relation owner fact ID itself is ambiguous
```

`8c4055d` prevents false `mutates` ownership when different types define same-named methods whose owner IDs collide. Ambiguous ownership now fails closed instead of cross-linking mutations. No deferred-queue semantics, scheduler semantics, target-specific pattern or new business authority was added.

## Verification on `8c4055d`

```text
Release build                     PASS, 0 warnings / 0 errors
C# tests                          401 / 401
frontend tests                    23 / 23
full solution                     424 / 424
pack/install                      PASS
WorkPlay                          PASS
PokeTrade                         PASS
Loren pinned external trial       PASS
Jellyfin generalization trial     PASS
```

GitHub Actions:

```text
CI       36317759550 / #442  PASS
Loren    36317759562 / #341  PASS
Jellyfin 36317759608 / #188  PASS
```

CI #442's first PokeTrade job failed before Angular build because npm registry returned HTTP 404 for `@peculiar/asn1-x509-attr-2.10.0.tgz`. No code changed. Rerunning only that failed job passed Angular build, business acceptance and PKC knowledge verification.

## Private gate is now operationalized

Use:

`docs/benchmarks/2026-09-27-rd8-private-reprobe-runbook.md`

The runbook fixes the exact order and privacy boundary for the next approved target run:

```text
verify exact semantic candidate 8c4055d
-> fresh disposable target, no .pkc, no --resume
-> one fresh pkc run
-> record RD8-OBS-1 aggregate counts only
-> Phase 1 probe #1 from .pkc/workspace only
-> freeze answer
-> Phase 2 source-known cross-check in approved environment
-> sanitized result + decision on repair (a)
```

It also makes the repair-(a) closure rule explicit: the previous false combined proven effect must be gone, exclusive branches must not be merged, no unsupported unconditional replacement authority may appear, and remaining uncertainty must be honest.

The overall probe may still be PARTIAL/FAIL because the authentication-event access-gate gap is separate. Repair (a) can close once its calibration defect is proven fixed.

## RD8-OBS-1

On the fresh private run record only:

```text
mapped-field-rule facts
applies-mapped-field-rule relations
```

Classify before touching code:

```text
facts = 0                         -> grammar/yield issue
facts > 0 and relations = 0      -> projection/linking issue
relations > 0                    -> relation path active on this run
```

This observation does not authorize a parser/linker repair by itself.

## 2026-09-28 candidate run

Exact candidate `8c4055decd56e4597b2a1aa03e24b5c1a70235d5` built Release cleanly and `pkc run` completed with exit 0 in 46m 29s. The target had no `.pkc` at start and was run without `--resume`; disposable-copy status was not recorded.

Sanitized output: 25,303 files; 16,460/16,497 planned semantic files executed; 255,229 facts; 4,629,193 relations; 4,044 workflows; 658 product features; 4,520 workspace files. RD8-OBS-1: 455 mapped-field-rule facts and 4,298 applies-mapped-field-rule relations (active relation path).

The clean-context Phase 1 answer and approved Phase 2 source cross-check are recorded in `docs/reviews/2026-09-28-rd8-private-reprobe-phase1.md`. Probe #1 is PARTIAL: Phase 1 correctly represented mutually exclusive branches and did not invent a LostDate cutoff, but the workspace under-proved login eligibility on 24/09. The source cross-check confirms access while both current-customer and active flags remain on, with access ending when either flag turns off. This clears repair (a)'s branch-calibration blocker; close (a), keep RD8-B NOT PASS / E0 ACTIVE, and open repair (b) next.

## Next repair scope

Keep these repairs separate:

```text
(b) deferred command-queue producer -> handler linking — NEXT
(c) recurring background jobs as workflow triggers
```

Do not combine (b) with (c); keep recurring background jobs unopened. The repair-(b) implementation plan is `docs/superpowers/plans/2026-09-28-rd8-deferred-command-queue-linking.md`.

## Exact next action

Approved source-enabled/company environment only:

```text
1. On `main`, confirm the sanitized producer → queued identity → dispatcher → handler shape in the approved environment.
2. Execute the repair-(b) plan regression-first on `main`.
3. Keep RD8-B NOT PASS and E0 ACTIVE until remaining probes and gates close.
```

Preserve until that evidence exists:

```text
RD8-B: NOT PASS
E0: ACTIVE / NOT COMPLETE
E1: LOCKED
R7.10/D: OPEN / REVIEW PAUSED
```
