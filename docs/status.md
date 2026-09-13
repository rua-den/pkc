# PKC Status

Last updated: 2026-09-13

## Current milestone

**V0.4.4 Loren Knowledge Readiness — ONE EXTERNAL-REVIEW BLOCKER REMAINS.**

Review lineage:

```text
original review HEAD:      34f77c036206d48bbf9495ea73e5debcea9f0eb3
implementation checkpoint: 8b01d4b5ba84112f36e46b234426f754be38c8f6
re-review HEAD:             e022dbf581846c4699b724df063cd68b47dff332
```

Review records:

```text
docs/reviews/2026-09-13-v0.4.4-external-review.md
docs/reviews/2026-09-13-v0.4.4-external-rereview.md
```

V0.4.3 remains the last accepted package:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

Do not advance V0.4.5 yet.

## Re-review result

```text
B1 validation consistency correctness              BLOCK
B2 repeated-build canonical parity                 PASS
B3 capability-flow anti-overfit                    PASS
B4 frontend product-source scope                   PASS
full PKC CI                                         PASS
PokeTrade runnable/known-answer                     PASS
pinned Loren external trial                         PASS
Loren-main canary                                   PASS
knowledge-only capability-flow recheck              PASS
structured/bundle/ZIP parity                        PASS
```

## Remaining blocker — B1

The first fix correctly moved from single lossy-key matching to complete requiredness-condition-set comparison, but operand normalization still removes meaningful characters and member-path structure.

Current counterexamples include:

```text
retryCount === -1     vs request.RetryCount == 1
threshold === 1.2     vs request.Threshold == 12
code === 'A-B'        vs request.Code == "AB"
primary.status        vs secondary.status
```

Those pairs can collapse to the same canonical operand and still allow false `consistent` / high-confidence output.

Required follow-up: preserve condition semantics explicitly. If equivalence cannot be proven deterministically, classify conservatively as `possible-mismatch` or `unknown`.

Required regressions at minimum:

```text
-1 != 1
1.2 != 12
"A-B" != "AB"
distinct member paths with the same terminal member are not automatically equivalent
```

Full details:

```text
docs/reviews/2026-09-13-v0.4.4-external-rereview.md
```

## Verified gate evidence

For implementation checkpoint `8b01d4b5ba84112f36e46b234426f754be38c8f6`:

```text
CI #219                    PASS
PokeTrade                  PASS
Loren external #118        PASS
Loren-main canary #101     PASS
```

Pinned Loren artifact:

```text
artifact id: 10311886470
artifact digest: sha256:e74aea2f657898d598d167c009966dac9dca5c4d297bb8bbdf89cfc182775b9c
structured files: 23
bundle parity:    23 / 23
ZIP parity:       23 / 23
source/raw leak:  0
```

## Exact next action

Stay in V0.4.4 and fix **B1 only**, regression-first:

```text
add red regressions
→ implement generic semantic-preserving canonicalization
→ run full tests/build
→ rerun PokeTrade
→ rerun pinned Loren
→ rerun Loren-main canary
→ recheck handoff parity
→ independent re-review B1
```

Warnings W1 (per-fact portable provenance) and W2 (durable blind-review evidence) remain non-blocking and out of the current coding scope.
