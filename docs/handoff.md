# PKC Handoff

Use this file when continuing PKC in another coding/review thread.

## Product contract

PKC is a Product/System Knowledge Compiler. A Product Owner should be able to hand generated portable knowledge to an AI assistant and ask product/system questions without requiring a source-code rescan.

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

**V0.4.4 remains open. The remaining B1 implementation blocker has a regression-first fix candidate and all current automated gates are green on the implementation HEAD. Independent re-review is required. V0.4.5 is locked.**

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
B1 implementation HEAD:    5fba7fd08e62e721e96e5a93bc65ab153d663f66
```

B2 repeated-build parity, B3 capability-flow anti-overfit, and B4 frontend product-source scope remain PASS from the prior independent re-review. Do not reopen them without a new regression.

## B1 regression-first evidence

Primary blocker regressions:

```text
fb182596c6b64dd484e65e5a8c209c0bbed5f89d  test: reproduce semantic operand equivalence blocker
3da99f9cb0e7238559cd1df710688107bcafe146  test: lock decimal operand structure
```

Workflow `34742346887` on `3da99f9...` failed exactly the requested semantic classes before the fix:

```text
retryCount === -1        vs request.RetryCount == 1
threshold === 1.2        vs request.Threshold == 2
code === 'A-B'           vs request.Code == "AB"
primary.status            vs request.Secondary.Status
```

Typed-operand implementation:

```text
9bdb0da9e635c5591edecb730054de590ff50be5  fix: preserve validation operand semantics
```

A follow-up review of that implementation found that blanket lowercasing could still erase case semantics inside a multi-segment path. That edge was also regression-first:

```text
1192ac6ec56fde3367a74f6b55720feda67d0d56  test: preserve member path case semantics
5fba7fd08e62e721e96e5a93bc65ab153d663f66  fix: preserve multi-segment path case
```

Workflow `34742645961` on `1192ac6...` failed only `Multi_segment_member_path_case_is_not_erased`; 45 other C# tests and all 8 frontend tests were green. The fix then brought the full suite to 46/46 C# and 8/8 frontend.

## B1 implementation contract

`ValidationConsistencyCandidateEnricher` now uses typed equality operands rather than stripping punctuation and collapsing member access to the final token.

The safety contract is:

```text
full supported condition-set equivalence + typed operand equivalence proven
→ consistent / high

semantics differ
→ possible-mismatch

semantics cannot be proven deterministically
→ conservative non-consistent result; never consistent/high
```

Important details:

- signed numeric values retain sign;
- decimal values are parsed invariantly and are not interpreted as dotted member paths;
- quoted string punctuation is retained;
- meaningful member-path segments are retained;
- multi-segment member-path case is retained;
- single-segment field names may be case-normalized only after deterministic context stripping so known camelCase/PascalCase cross-stack matching continues to work;
- endpoint request-parameter roots are stripped only when the root is an actual parsed endpoint parameter;
- Angular form scaffolding is stripped only for recognized Angular validation facts and known forms such as `<form>.value.<field>` or `<form>.controls.<field>.value`;
- enum/string convenience matching is narrow and deterministic, preserving known valid cases such as UI `'CSP'` versus backend `ServiceType.CSP`;
- unsupported constructs fall back to a conservative non-high-confidence result.

Do not weaken this into a generic token sanitizer again.

## Current verified gates

For implementation HEAD `5fba7fd08e62e721e96e5a93bc65ab153d663f66`:

```text
full CI / WorkPlay          PASS  run 34742742074
  C# tests                  46 / 46 PASS
  frontend tests             8 / 8 PASS
PokeTrade real system       PASS  run 34742742074
pinned Loren external       PASS  run 34742742056
Loren-main canary           PASS  run 34742742039
```

Pinned Loren output from run `34742742056`:

```text
artifact id:      10312494140
artifact digest:  sha256:907b0d2de95933012e33992236894af229690a84666bee02433834dcfa192a06
structured files: 23
bundle parity:     23 / 23
ZIP parity:        23 / 23
missing content:    0
source/raw leak:     0
```

The artifact's `PKC_KNOWLEDGE.md` contains all 23 structured Markdown files verbatim. Its `PKC_KNOWLEDGE.zip` contains the same 23 `knowledge/` files and no `.pkc/` or source entries.

## Next action

Do **not** start more implementation work merely because the automated gates are green.

The next action is an **independent B1 re-review of current main HEAD**. The reviewer should specifically verify:

```text
signed numeric semantics
true decimal semantics
quoted punctuation
member-path segment identity and case
conservative behavior for unsupported/unprovable expressions
preservation of valid cross-language single-field + enum/string equivalence
complete condition-set comparison
all current gates and portable handoff parity
```

Only if that independent review returns PASS may V0.4.4 close and V0.4.5 unlock.

## Scope locks

Until independent B1 re-review passes, do not start:

- V0.4.5 second real-repository work;
- Azure DevOps ingestion;
- unrelated frontend/framework rewrites;
- browser/runtime exploration;
- generalized drift/insight work;
- other roadmap expansion.

Warnings W1 (claim-level portable provenance) and W2 (durable blind-review evidence) remain non-blocking follow-up concerns.

## Bootstrap prompt for independent re-review

```text
Independently re-review PKC V0.4.4 B1 from current main HEAD.
Read docs/status.md, docs/handoff.md, docs/reviews/2026-09-13-v0.4.4-external-review.md, and docs/reviews/2026-09-13-v0.4.4-external-rereview.md.
Focus only on validation-condition equivalence safety. Verify signed numeric, decimal, quoted punctuation, meaningful member-path structure/case, conservative handling of unprovable conditions, and preservation of valid cross-language equivalence. Confirm all current gates and handoff parity. Do not advance V0.4.5 unless the independent re-review passes.
```
