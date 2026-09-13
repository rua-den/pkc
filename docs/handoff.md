# PKC Handoff

Use this file when continuing PKC in another coding/review thread.

## Product contract

PKC is a Product/System Knowledge Compiler. A Product Owner should be able to hand the generated portable knowledge to an AI assistant and ask product/system questions without requiring a source-code rescan.

Keep the compiler architecture deterministic:

```text
source
→ analyzers/adapters
→ evidence/facts
→ feature/workflow candidates
→ knowledge synthesis
→ canonical model
→ portable rendering
```

Do not implement direct source-to-freeform-AI generation.

## Current state

**V0.4.4 remains open. One blocker remains after independent re-review. V0.4.5 is locked.**

Read in order:

```text
docs/status.md
docs/handoff.md
docs/reviews/2026-09-13-v0.4.4-external-review.md
docs/reviews/2026-09-13-v0.4.4-external-rereview.md
```

Review lineage:

```text
original review HEAD:      34f77c036206d48bbf9495ea73e5debcea9f0eb3
all-four-fix checkpoint:   8b01d4b5ba84112f36e46b234426f754be38c8f6
first re-review HEAD:       e022dbf581846c4699b724df063cd68b47dff332
```

## Re-review disposition

```text
B1 validation consistency semantics                 BLOCK
B2 repeated-build canonical parity                 PASS
B3 capability-flow anti-overfit                    PASS
B4 frontend product-source scope                   PASS
CI / PokeTrade / Loren pinned / Loren-main          PASS
knowledge-only capability-flow recheck              PASS
structured/bundle/ZIP parity                        PASS
```

Do not reopen B2/B3/B4 unless a new regression proves a problem.

## Exact remaining blocker — B1

`ValidationConsistencyCandidateEnricher` now compares complete supported requiredness-condition sets, but its operand canonicalization is still lossy.

`CanonicalOperand()` eventually strips non-alphanumeric characters and reduces member access to the terminal token. This can collapse semantically different conditions and still emit `consistent` with high confidence.

Regression cases to add before the fix:

```text
UI:      retryCount === -1
Backend: request.RetryCount == 1
Expected: NOT consistent

UI:      threshold === 1.2
Backend: request.Threshold == 12
Expected: NOT consistent

UI:      code === 'A-B'
Backend: request.Code == "AB"
Expected: NOT consistent

UI:      primary.status === 'active'
Backend: request.Secondary.Status == Status.Active
Expected: NOT consistent merely because both paths end in `status`
```

The fix must be generic and semantic-preserving. Cross-language convenience matching is allowed only when equivalence is explicit and deterministic. If PKC cannot prove equivalence, return `possible-mismatch` or `unknown`, never `consistent/high`.

Suggested implementation direction, not a required design:

```text
parse each equality into typed operands
→ preserve literal value/sign/decimal/punctuation
→ normalize identifiers/member paths without erasing meaningful path segments
→ use explicit rules for known frontend/backend context prefixes
→ canonicalize conjunction ordering
→ compare complete condition sets
```

Do not merely patch the four literal examples.

## Regression-first requirement

Use the same discipline as the previous blocker fixes:

```text
1. add failing B1 regressions
2. prove they fail on current implementation
3. implement generic fix
4. make regressions pass
5. run affected tests
6. commit incrementally
```

Then run all current gates:

```text
full PKC build/tests
PokeTrade runnable + known-answer
pinned Loren external trial
Loren-main canary
handoff structured/bundle/ZIP parity
```

After those are green, update status/handoff and request another independent review. Do not mark V0.4.4 PASS yourself.

## Current verified evidence before B1 follow-up

```text
implementation checkpoint: 8b01d4b5ba84112f36e46b234426f754be38c8f6
CI #219:                 PASS
Loren external #118:     PASS
Loren-main canary #101:  PASS
Loren artifact id:       10311886470
artifact digest:         sha256:e74aea2f657898d598d167c009966dac9dca5c4d297bb8bbdf89cfc182775b9c
structured files:        23
bundle parity:           23 / 23
ZIP parity:              23 / 23
source/raw leak:         0
```

## Scope locks

Until B1 independently passes, do not start:

- V0.4.5 second real-repository work;
- Azure DevOps ingestion;
- unrelated frontend/framework rewrites;
- browser/runtime exploration;
- generalized drift/insight work;
- other roadmap expansion.

Warnings W1 (claim-level portable provenance) and W2 (durable blind-review evidence) remain non-blocking follow-up concerns.

## Bootstrap prompt for coding thread

```text
Continue PKC from current main HEAD.
Read docs/status.md, docs/handoff.md, docs/reviews/2026-09-13-v0.4.4-external-review.md, then docs/reviews/2026-09-13-v0.4.4-external-rereview.md.
Fix only the remaining V0.4.4 B1 blocker, regression-first. Preserve signed numeric, decimal, quoted punctuation, and meaningful member-path semantics in condition equivalence. If equivalence cannot be proven, classify conservatively. Commit incrementally, rerun all current gates, update status/handoff, and do not advance V0.4.5 until independent re-review passes.
```
