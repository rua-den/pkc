# PKC Status

Last updated: 2026-09-13

## Current milestone

**V0.4.4 Loren Knowledge Readiness — FINAL B1 PROVENANCE GAP REMAINS / FIX REQUIRED.**

Latest independent review:

```text
docs/reviews/2026-09-13-v0.4.4-external-rereview-3.md
```

Reviewed HEAD:

```text
84d70aa8d6f646ca9a0d72ea9645340c1f583fc1
```

Implementation checkpoint under review:

```text
ab3bdf32c4076dcd22c957b122aea6cc424b027c
```

V0.4.3 remains the last accepted package:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

Do **not** advance V0.4.5 yet.

## Current blocker disposition

```text
B1 typed operand semantics                         PASS
B1 complete condition-set comparison              PASS
B1 Angular proven form root                        PASS
B1 backend validated-object provenance            BLOCK when parameterName is absent
B2 repeated-build canonical parity                 PASS
B3 capability-flow anti-overfit                    PASS
B4 frontend product-source scope                   PASS
```

The remaining B1 path is reachable from the real C# scanner. `CSharpValidationEvidenceScanner` can emit a conditional backend validation fact without `parameterName` when the missing-value member access is rooted in `this.*`. The correlation layer then falls back to accepting any endpoint parameter root and can incorrectly emit `consistent / high`.

Concrete reachable shape:

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

Current scanner/correlation path can treat the resulting `tracker.Status == Status.Active` condition as request-field evidence because `parameterName` is absent.

Required rule:

```text
conditional backend validation may participate in proven cross-stack equivalence
only when the validated-object root is known and proven.

missing parameterName
→ equivalence unproven
→ never consistent / high
```

Add an end-to-end scanner + correlation regression using real C# source, not only fabricated facts. Positive synthetic conditional-validation tests should include the `parameterName` provenance that normal supported request guards emit.

## Verified gates on `ab3bdf32...`

```text
CI / PokeTrade          PASS  run 34758617653
pinned Loren external   PASS  run 34758617664
Loren-main canary       PASS  run 34758617742
```

Pinned Loren handoff independently rechecked:

```text
structured files:      23
bundle parity:          23 / 23
portable ZIP parity:   23 / 23
source/raw leak:         0
```

Automation and parity remain green but do not override the reachable false-equivalence path.

## Exact next action

Stay in V0.4.4 and fix only the missing-backend-provenance B1 gap regression-first. Then rerun full PKC tests, PokeTrade, pinned Loren, Loren-main canary and handoff parity, and request independent B1 re-review.

Warnings W1 (claim-level portable provenance) and W2 (durable blind-review evidence) remain non-blocking follow-up concerns.
