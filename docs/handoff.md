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

**V0.4.4 remains open. Independent re-review 3 found one final B1 provenance gap. B2/B3/B4 remain PASS. V0.4.5 is locked.**

Read in order:

```text
docs/status.md
docs/handoff.md
docs/reviews/2026-09-13-v0.4.4-external-review.md
docs/reviews/2026-09-13-v0.4.4-external-rereview.md
docs/reviews/2026-09-13-v0.4.4-external-rereview-2.md
docs/reviews/2026-09-13-v0.4.4-external-rereview-3.md
```

Latest reviewed repository HEAD:

```text
84d70aa8d6f646ca9a0d72ea9645340c1f583fc1
```

Implementation checkpoint:

```text
ab3bdf32c4076dcd22c957b122aea6cc424b027c
```

## What passed

The latest implementation correctly preserves:

```text
- full condition-set comparison
- signed numeric semantics
- decimal semantics
- quoted punctuation
- meaningful member-path structure/case
- strict frontend === proof
- known Angular binding-form root proof
- exact-case backend endpoint roots
- rejection of unrelated endpoint parameters when parameterName is present
- rejection of generic cross-stack path/path coincidence
```

Known positive request/enum and bound Angular form cases remain supported.

B2 repeated-build parity, B3 capability-flow anti-overfit, and B4 frontend product-source scope remain independently PASS.

## Remaining B1 gap

`TryCanonicalizeFieldPath()` still permits a backend condition root when:

```text
root is any exact endpoint parameter
AND backend validation fact has no parameterName
```

That is not enough proof that the condition describes the object whose field is being validated.

This is reachable from `CSharpValidationEvidenceScanner`. `FindRootIdentifier()` returns null for a missing-value member access rooted in `this.*`.

Example:

```csharp
public IActionResult Create(CreateTargetRequest request, StateTracker tracker)
{
    if (tracker.Status == Status.Active && this._state.TargetId == null)
    {
        return BadRequest();
    }

    return Ok();
}
```

The scanner can emit:

```text
backend-field-validation
field: TargetId
method: Create
condition: tracker.Status == Status.Active
parameterName: absent
```

The current correlation layer may then strip `tracker` because it is an endpoint parameter. A UI condition `status === 'Active'` can become the same canonical condition and be incorrectly classified `consistent / high`, even though the missing-value guard is on `this._state.TargetId`, not the request model.

## Required fix

Regression-first, remain inside B1.

Add an end-to-end scanner + correlation regression with real C# source for the reachable case above.

Safety contract:

```text
conditional backend validation is eligible for proven cross-stack equivalence
only when its validated-object root is known.

parameterName must be non-empty
AND it must exactly match an endpoint parameter
AND the condition field root must exactly match parameterName.

otherwise
→ equivalence unproven
→ never consistent / high
```

Existing positive synthetic conditional-validation tests that omit `parameterName` should be updated to carry `parameterName: "request"` when they intend to model normal scanner-produced request guards.

A broader scanner cleanup for `this.*` validation scope may be useful later, but it is not required to close this gate if the correlation layer is conservative when provenance is absent.

## Verified gates on current implementation checkpoint

```text
CI / PokeTrade          PASS  run 34758617653
pinned Loren external   PASS  run 34758617664
Loren-main canary       PASS  run 34758617742
```

Pinned Loren artifact:

```text
artifact id:      10318151788
artifact digest:  sha256:e28b6310f2cabb102c472e75175c5791e6ab825778e1d30616c496497ab2436f
structured files: 23
bundle parity:     23 / 23
portable ZIP:      23 / 23
source/raw leak:    0
```

## Next action

Do not start V0.4.5 yet.

Fix only the remaining B1 provenance gap, rerun all current gates and handoff parity, update status/handoff, then request independent B1 re-review.

Warnings W1 (claim-level portable provenance) and W2 (durable blind-review evidence) remain non-blocking follow-up concerns.

## Bootstrap prompt for coding thread

```text
Continue PKC from current main HEAD.
Read docs/status.md, docs/handoff.md, and docs/reviews/2026-09-13-v0.4.4-external-rereview-3.md.
Fix only the remaining V0.4.4 B1 backend-provenance gap regression-first.
Add an end-to-end scanner+correlation regression proving a conditional backend validation with missing parameterName cannot become consistent/high merely because its condition root is another endpoint parameter.
Require proven validated-object provenance for conditional backend equivalence, preserve existing positive request/enum and bound-Angular cases, rerun full CI/PokeTrade/pinned Loren/Loren-main/parity, update status/handoff, and do not advance V0.4.5 until independent review passes.
```
