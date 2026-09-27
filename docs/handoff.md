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

Then verify current `main`, topic branch, recent commits, production code and relevant regressions. Never reset to an older SHA merely because an older review names one.

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

Exact production candidate awaiting the approved private product-value gate:

```text
8c4055decd56e4597b2a1aa03e24b5c1a70235d5
fix: fail closed on ambiguous mutation owners
```

Draft PR: `#7 fix: harden conditional state effect branches`.

Do not merge to `main` merely because repository CI is green. RD8-B still requires the approved private re-probe.

## Current checkpoint

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

## Accepted private baseline

Sanitized source of truth:

`docs/reviews/2026-09-25-rd8-private-validation-result.md`

RD8-A passed at ~31.1 min / ~5.42 GB peak process-tree memory with 265,120 facts, 1,206,340 relations, 4,186 workflow candidates and 703 product features.

RD8-C passed with two HIGH runtime-plugin edges.

RD8-B did not pass:

```text
probe #7 scheduled updates / invoice period   FAIL    ~27%
probe #1 LostDate / CustomerWeb access        FAIL    ~38%
probe #10 Worklog                             PARTIAL ~55%
```

Probe #1 contained the active calibration blocker: mutually exclusive state-effect branches were merged into one proven effect.

## Repair (a) implementation state

Spec:

`docs/plans/2026-09-25-exclusive-branch-state-effects-spec.md`

The candidate chain remains inside the same conditional-state repair:

```text
7eab43d  preserve conditional state effect branches
82b5ffd  harden same-line mutation identity + mixed-depth rendering
01d88b0  preserve relation ownership across distinct method names
8c4055d  fail closed when the relation owner fact ID itself is ambiguous
```

The latest rereview found a valid counterexample not covered by the prior `First`/`Second` regression: two different types can define same-named methods on the same physical line. Their method fact IDs collide, and their mutation `Container` values are also the same method name, so the prior fallback could still cross-link both mutations to the ambiguous owner.

`8c4055d` records ambiguous owner IDs and emits no guessed `mutates` relation for a collision whose owner cannot be distinguished. The new regression keeps both mutation facts distinct and requires zero cross-linked ownership relations. Unique-owner collisions still preserve all mutations for that owner.

No queue semantics, scheduler semantics, target-specific pattern or new business authority was added.

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

The first PokeTrade job in CI #442 failed during npm install because the npm registry returned HTTP 404 for the `@peculiar/asn1-x509-attr-2.10.0.tgz` tarball. No code changed. Rerunning only that failed job passed Angular build, PokeTrade business acceptance and PKC knowledge verification.

## RD8-OBS-1

The prior private target run emitted zero `applies-mapped-field-rule` relations. On the next approved run record:

```text
mapped-field-rule facts
applies-mapped-field-rule relations
```

Classify before touching code:

```text
facts = 0                 -> grammar/yield issue
facts > 0, relations = 0  -> projection/linking issue
```

## Do not start yet

Keep these separate from repair (a):

```text
(b) deferred command-queue producer -> handler linking
(c) recurring background jobs as workflow triggers
```

Do not implement either before probe #1 proves repair (a) cleared the calibration blocker. Otherwise the next private run changes multiple variables and loses causal attribution.

## Exact next action

Approved source-enabled/company environment only:

```text
1. Build exact production candidate 8c4055d.
2. Run fresh `pkc run <approved-disposable-target-copy>`.
3. Re-score RD8-B probe #1 first, workspace-only before source cross-check.
4. Verify exclusive branches are alternatives and no false combined proven effect remains.
5. Record sanitized score + concrete remaining miss.
6. Record the two RD8-OBS-1 fact/relation counts.
```

If probe #1 clears the blocker, close repair (a) and start repair (b) regression-first. If it does not, use the concrete probe miss for the next minimum generic correction inside repair (a).

Preserve until then:

```text
RD8-B: NOT PASS
E0: ACTIVE / NOT COMPLETE
E1: LOCKED
R7.10/D: OPEN / REVIEW PAUSED
```
