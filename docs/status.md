# PKC Status

Last updated: 2026-09-27

## Repository state reviewed

Current `main`:

```text
9e2397e83f56df2533fecc7184cadc80b74acddb
Continue
```

Active topic branch:

```text
codex/rd8-b-same-line-conditional-state
```

Exact production candidate awaiting the approved private product-value gate:

```text
8c4055decd56e4597b2a1aa03e24b5c1a70235d5
fix: fail closed on ambiguous mutation owners
```

Draft PR: `#7 fix: harden conditional state effect branches`.

`main` is untouched.

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

## Accepted private evidence

Authoritative sanitized baseline:

`docs/reviews/2026-09-25-rd8-private-validation-result.md`

RD8-A PASS on the approved large target: ~31.1 min, ~5.42 GB peak process-tree RAM, 265,120 facts, 1,206,340 relations, 4,186 workflow candidates and 703 product features.

RD8-C PASS: two deterministic HIGH `runtime-plugin-load` edges on the real target.

RD8-B remains NOT PASS:

```text
#7 scheduled updates / invoice period   FAIL    ~27%
#1 LostDate / CustomerWeb access        FAIL    ~38%  (calibration blocker)
#10 Worklog                             PARTIAL ~55%
```

Probe #1's blocker was false authority from mutually exclusive state-effect branches being rendered together as one proven effect.

## Repair (a) — repo-local implementation complete, private proof pending

Primary spec:

`docs/plans/2026-09-25-exclusive-branch-state-effects-spec.md`

The repair chain now covers five compile-valid correctness defects inside the same conditional-state boundary:

1. exclusive if/else, else-if and switch mutations were flattened into one proven effect;
2. same-line same-target mutations could share a fact ID and be dropped or mapped to the wrong syntax branch;
3. direct plus nested mutations in one branch arm could crash rendering through an out-of-range branch path;
4. same-line same-target mutations in different method names could cross-link each method to every colliding replacement fact;
5. two different types with the same method name on the same physical line could also collide at the owner fact ID, allowing the previous same-container fallback to cross-link ambiguous ownership.

Current candidate `8c4055d` fixes (5) fail-closed: if a `mutates` relation's owner fact ID is ambiguous, PKC emits no guessed ownership relation. Unique-owner same-method collisions retain all owned mutations. No queue, scheduler, target-specific or new business authority was added.

## Exact candidate verification

```text
8c4055decd56e4597b2a1aa03e24b5c1a70235d5
fix: fail closed on ambiguous mutation owners
```

Clean-environment gates:

```text
Release build:                    PASS, 0 warnings / 0 errors
C# tests:                         401 / 401 PASS
frontend tests:                   23 / 23 PASS
full solution total:              424 / 424 PASS
pack + local tool install:        PASS
WorkPlay end-to-end knowledge:    PASS
PokeTrade business + knowledge:   PASS
Loren external trial:             PASS
Jellyfin generalization trial:    PASS
```

GitHub Actions:

```text
CI       36317759550 / #442  PASS
Loren    36317759562 / #341  PASS
Jellyfin 36317759608 / #188  PASS
```

CI #442's first PokeTrade attempt failed during `npm install` because npm registry returned HTTP 404 for `@peculiar/asn1-x509-attr-2.10.0.tgz`, before Angular build. No code changed; rerunning only that failed job passed Angular build, PokeTrade business acceptance and PKC knowledge verification.

## RD8-OBS-1 — observation only

The approved real target previously emitted zero `applies-mapped-field-rule` relations. On the next approved target run record:

```text
mapped-field-rule fact count
applies-mapped-field-rule relation count
```

Interpretation:

```text
facts = 0                 -> rule grammar/yield miss
facts > 0, relations = 0  -> projection/linking miss
```

Do not add speculative parser support before this classification.

## Exact next action

The checkpoint is at an external approved-environment gate:

```text
1. Build/run exact production candidate 8c4055d on a fresh disposable copy of the approved target.
2. Run fresh `pkc run <target>`; do not stack repair (b) or (c) into this run.
3. Re-score RD8-B probe #1 first using the product-value benchmark protocol.
4. Confirm mutually exclusive state branches are alternatives and no false combined proven effect remains.
5. Record only sanitized score + concrete remaining miss.
6. Record the two RD8-OBS-1 counts above.
```

If and only if probe #1 clears the calibration blocker, close repair (a) and open repair (b) as its own regression-first task:

```text
(b) deferred command-queue producer -> handler linking
```

Repair (c), recurring background jobs as workflow triggers, remains later and separate.

Preserve until that evidence exists:

```text
RD8-B: NOT PASS
E0: ACTIVE / NOT COMPLETE
E1: LOCKED
R7.10/D: OPEN / REVIEW PAUSED
```
