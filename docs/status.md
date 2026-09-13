# PKC Status

Last updated: 2026-09-13

## Current milestone

**V0.4.4 Loren Knowledge Readiness — FINAL B1 PROVENANCE FIX IMPLEMENTED / INDEPENDENT RE-REVIEW REQUIRED.**

Latest independent review:

```text
docs/reviews/2026-09-13-v0.4.4-external-rereview-3.md
```

That review found one remaining B1 backend-provenance gap. The gap has now been fixed regression-first, but V0.4.4 is **not** self-certified. V0.4.5 remains locked until an independent reviewer passes the current candidate.

V0.4.3 remains the last accepted package:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

## B1 provenance fix candidate

Regression commit:

```text
82c28b4a4d2b2c8e9bdcdf2bb2e902d8caedf1c1
```

Regression CI:

```text
run 34762840775
build:     PASS, 0 warnings / 0 errors
frontend:  8 / 8 PASS
C#:        53 PASS / 1 FAIL
failure:   BackendValidationProvenanceRegressionTests.Missing_validated_parameter_provenance_cannot_be_proven_by_another_endpoint_parameter
actual:    consistent
```

The regression uses real C# source through `CSharpEvidenceScanner` plus frontend scanning and cross-stack correlation. It proves the reachable case where a conditional `backend-field-validation` has no `parameterName`, while its condition root (`tracker`) is merely another endpoint parameter.

Fix commit:

```text
5a5fdcb5fdf1a9fb888857791a4757382b86c774
```

For conditional backend equivalence, `TryCanonicalizeFieldPath()` now requires all of the following before stripping the backend condition root:

```text
parameterName is present and non-empty
AND parameterName exactly matches an endpoint parameter
AND condition field root exactly matches parameterName
```

If any proof is missing, condition equivalence is unproven and the comparison cannot become `consistent / high`.

Positive synthetic conditional-backend fixtures were updated to carry realistic `parameterName: "request"` provenance where they are intended to model normal supported request guards.

## Current blocker disposition

```text
B1 typed operand semantics                         PASS candidate
B1 complete condition-set comparison              PASS candidate
B1 Angular proven form root                        PASS candidate
B1 backend validated-object provenance            FIX IMPLEMENTED / RE-REVIEW REQUIRED
B2 repeated-build canonical parity                 PASS
B3 capability-flow anti-overfit                    PASS
B4 frontend product-source scope                   PASS
```

## Verified gates on fix commit `5a5fdcb5...`

```text
full PKC CI / PokeTrade  PASS  run 34769999278
pinned Loren external   PASS  run 34769999298
Loren-main canary       PASS  run 34769999304
```

Full CI evidence:

```text
build:          PASS, 0 warnings / 0 errors
C# tests:       54 / 54 PASS
frontend tests:  8 / 8 PASS
tool install:   PASS
WorkPlay build: PASS
PokeTrade:      PASS
```

Pinned Loren artifact:

```text
artifact id:       10322215429
artifact digest:   sha256:c615a129f96992ed3662ff2368a1dbc8bfa639d4fbc86034c7b102b773ce99de
structured files:  23
bundle parity:      23 / 23, marker + verbatim content
portable ZIP:       23 / 23, exact file set + byte parity
source/raw leak:     0
bundle sha256:      2ea7037c5c4c8954b5fa1abcb4248963ad9299a3ce53e10591e5ce492dc17186
inner ZIP sha256:   1fc740bafef0411e9702fe4da2a4b8c2e5b9903ba252e07036d97315322e9258
```

## Exact next action

Keep V0.4.4 open. Rerun the current automated gates on the final documentation HEAD, verify pinned Loren handoff parity again, then request independent B1 re-review focused on the backend validated-object provenance contract.

Do **not** advance V0.4.5 until that independent review returns PASS.

Warnings W1 (claim-level portable provenance) and W2 (durable blind-review evidence) remain non-blocking follow-up concerns and are outside this B1-only fix.
