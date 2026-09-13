# PKC Status

Last updated: 2026-09-13

## Current milestone

**V0.4.4 Loren Knowledge Readiness — B1 FIX IMPLEMENTED / INDEPENDENT RE-REVIEW REQUIRED.**

Review lineage:

```text
original review HEAD:      34f77c036206d48bbf9495ea73e5debcea9f0eb3
all-four-fix checkpoint:   8b01d4b5ba84112f36e46b234426f754be38c8f6
first re-review HEAD:       e022dbf581846c4699b724df063cd68b47dff332
B1 implementation HEAD:    5fba7fd08e62e721e96e5a93bc65ab153d663f66
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

Do **not** mark V0.4.4 PASS and do **not** advance V0.4.5 until an independent re-review of the new HEAD passes.

## Re-review disposition before the B1 follow-up

```text
B1 validation consistency semantics                 BLOCK
B2 repeated-build canonical parity                 PASS
B3 capability-flow anti-overfit                    PASS
B4 frontend product-source scope                   PASS
```

B2/B3/B4 were not reopened by this change.

## B1 regression-first closure candidate

The re-review found that condition canonicalization could erase semantically meaningful operand structure. The follow-up was implemented regression-first.

Primary red regression commits:

```text
fb182596c6b64dd484e65e5a8c209c0bbed5f89d  test: reproduce semantic operand equivalence blocker
3da99f9cb0e7238559cd1df710688107bcafe146  test: lock decimal operand structure
```

The final red run before the typed-operand fix, workflow `34742346887` on `3da99f9...`, proved all four requested blocker classes still failed:

```text
signed numeric:       retryCount === -1 vs request.RetryCount == 1        FAIL as expected
decimal structure:    threshold === 1.2 vs request.Threshold == 2         FAIL as expected
quoted punctuation:   code === 'A-B' vs request.Code == "AB"              FAIL as expected
member path:          primary.status vs request.Secondary.Status           FAIL as expected
```

Typed-operand implementation:

```text
9bdb0da9e635c5591edecb730054de590ff50be5  fix: preserve validation operand semantics
```

A follow-up safety review of that implementation found one remaining lossy edge: multi-segment paths were still case-folded. That was also fixed regression-first:

```text
1192ac6ec56fde3367a74f6b55720feda67d0d56  test: preserve member path case semantics
5fba7fd08e62e721e96e5a93bc65ab153d663f66  fix: preserve multi-segment path case
```

On `1192ac6...`, workflow `34742645961` failed exactly the new regression `Multi_segment_member_path_case_is_not_erased`; the other 45 C# tests and all 8 frontend tests remained green.

## B1 implementation contract

The canonicalizer now:

- parses equality operands into typed string-literal, numeric-literal, and member-path forms;
- preserves numeric sign and decimal semantics with invariant numeric parsing;
- preserves quoted literal punctuation instead of stripping non-alphanumeric characters;
- preserves meaningful member-path segments instead of reducing every path to its terminal token;
- preserves case for multi-segment member paths rather than proving equality via blanket case-folding;
- case-normalizes only single-segment field names after deterministic context stripping, preserving the known camelCase/PascalCase cross-stack convenience;
- removes only context prefixes that are deterministically known, including the actual backend endpoint parameter root and recognized Angular form-value scaffolding;
- keeps the existing deterministic enum/string equivalence needed for cases such as UI `'CSP'` versus backend `ServiceType.CSP`;
- treats unsupported or unprovable operand semantics conservatively, producing a non-`consistent/high` result.

The complete-condition-set comparison from the first B1 fix remains in place.

## Current gate evidence

For B1 implementation HEAD `5fba7fd08e62e721e96e5a93bc65ab153d663f66`:

```text
full CI / WorkPlay          PASS  workflow run 34742742074
  C# tests                  46 / 46 PASS
  frontend tests             8 / 8 PASS
PokeTrade real system       PASS  workflow run 34742742074
pinned Loren external       PASS  workflow run 34742742056
Loren-main canary           PASS  workflow run 34742742039
```

Pinned Loren artifact from run `34742742056`:

```text
artifact id:      10312494140
artifact digest:  sha256:907b0d2de95933012e33992236894af229690a84666bee02433834dcfa192a06
structured files: 23
bundle parity:     23 / 23
ZIP parity:        23 / 23
missing content:    0
source/raw leak:     0
```

`PKC_KNOWLEDGE.md` contains every structured Markdown file verbatim, and `PKC_KNOWLEDGE.zip` contains the same 23 `knowledge/` files with no `.pkc/` or source entries.

## Acceptance position

Current implementation evidence is green, but V0.4.4 is **not self-certified PASS**.

Exact next action:

```text
independent re-review B1 on current main HEAD
→ if PASS, close V0.4.4
→ only then unlock V0.4.5
```

Warnings W1 (per-fact portable provenance) and W2 (durable blind-review evidence) remain non-blocking and outside this B1-only coding scope.
