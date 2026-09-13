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

**V0.4.4 remains open. The final B1 backend-provenance gap from independent re-review 3 has been fixed regression-first and all automated gates on the implementation checkpoint are green. Independent re-review is still required. V0.4.5 is locked.**

Read in order:

```text
docs/status.md
docs/handoff.md
docs/reviews/2026-09-13-v0.4.4-external-review.md
docs/reviews/2026-09-13-v0.4.4-external-rereview.md
docs/reviews/2026-09-13-v0.4.4-external-rereview-2.md
docs/reviews/2026-09-13-v0.4.4-external-rereview-3.md
```

Latest independent review target:

```text
84d70aa8d6f646ca9a0d72ea9645340c1f583fc1
```

Current B1 implementation checkpoint:

```text
5a5fdcb5fdf1a9fb888857791a4757382b86c774
```

## Regression-first evidence

Red regression commit:

```text
82c28b4a4d2b2c8e9bdcdf2bb2e902d8caedf1c1
```

CI run `34762840775` reproduced the reachable scanner/correlation false positive with real C# source:

```text
public IActionResult Create(CreateTargetRequest request, StateTracker tracker)
{
    if (tracker.Status == Status.Active && this._state.TargetId == null)
    {
        return BadRequest();
    }

    return Ok();
}
```

`CSharpEvidenceScanner` emitted a conditional `backend-field-validation` for `TargetId` whose condition referenced `tracker.Status` and whose `parameterName` was absent. The new regression then observed the old correlation result as `consistent`, proving the blocker was reachable end-to-end.

Red run summary:

```text
build:          PASS, 0 warnings / 0 errors
frontend tests:  8 / 8 PASS
C# tests:       53 PASS / 1 FAIL
failing test:   BackendValidationProvenanceRegressionTests.Missing_validated_parameter_provenance_cannot_be_proven_by_another_endpoint_parameter
old result:     consistent
```

## Fix contract

Fix commit:

```text
5a5fdcb5fdf1a9fb888857791a4757382b86c774
```

For a conditional backend validation to participate in proven cross-stack equivalence, all three proofs are now mandatory:

```text
1. validated-object parameterName is present and non-empty;
2. parameterName exactly matches an endpoint parameter;
3. condition field root exactly matches parameterName.
```

No fallback to “any endpoint parameter root” remains. Missing or contradictory provenance causes requiredness-condition equivalence to be unproven, so the comparison cannot emit `consistent / high`.

Existing positive synthetic conditional-validation fixtures now carry realistic `parameterName: "request"` provenance where they model normal supported request guards.

## B1 behavior that must remain protected

The candidate preserves the previously hardened behavior:

```text
- full condition-set comparison
- signed numeric semantics
- decimal semantics
- quoted punctuation
- meaningful member-path structure and case
- strict frontend === proof
- known Angular binding-form root proof
- exact-case backend endpoint roots
- rejection of unrelated endpoint parameters
- rejection of generic cross-stack path/path coincidence
- known positive request/enum equivalence
- known positive bound-Angular form equivalence
- missing backend parameterName is now unproven
```

B2 repeated-build canonical parity, B3 capability-flow anti-overfit, and B4 frontend product-source scope remain PASS and were not reopened.

## Verified gates on implementation checkpoint

```text
full PKC CI / PokeTrade  PASS  run 34769999278
pinned Loren external   PASS  run 34769999298
Loren-main canary       PASS  run 34769999304
```

CI details:

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

## Independent re-review target

Do not declare V0.4.4 complete from this coding thread. An independent reviewer should inspect the current `main` candidate and specifically verify:

```text
- the real scanner regression remains reachable and now classifies conservatively;
- missing/blank parameterName can never prove conditional backend equivalence;
- parameterName must exactly belong to the endpoint parameters;
- condition root must exactly equal parameterName;
- prior positive request/enum and bound-Angular equivalence still works;
- prior typed-operand, member-path and condition-set protections remain intact;
- full CI, PokeTrade, pinned Loren, Loren-main canary and portable handoff parity remain green.
```

Warnings W1 (claim-level portable provenance) and W2 (durable blind-review evidence) remain non-blocking follow-up concerns and are outside this B1-only scope.

## Next action

Rerun all current automated gates on the final documentation HEAD and recheck pinned Loren handoff parity. Then request independent B1 re-review.

Do **not** start V0.4.5 or Azure DevOps work until independent review returns PASS.

## Bootstrap prompt for independent review thread

```text
Review current PKC main HEAD as the final V0.4.4 B1 candidate.
Read docs/status.md, docs/handoff.md, and docs/reviews/2026-09-13-v0.4.4-external-rereview-3.md.
Independently verify the backend validated-object provenance fix: conditional backend equivalence requires a non-empty parameterName, exact endpoint-parameter membership, and exact condition-root match to parameterName; missing provenance must never become consistent/high.
Re-run or inspect the real scanner+correlation regression and all current gates/parity. Do not advance V0.4.5 unless B1 passes independently.
```
