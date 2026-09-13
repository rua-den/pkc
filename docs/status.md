# PKC Status

Last updated: 2026-09-13

## Current milestone

**V0.4.4 Loren Knowledge Readiness — ONE B1 CONTEXT-PROOF BLOCKER REMAINS.**

Independent re-review of current candidate `f98f05a624fe4262615db992fbe3f9ea3bd43bce` confirms the typed-operand fix closed the previously reported numeric/string/member-path collisions, but found one remaining false-equivalence path in context stripping.

Latest review:

```text
docs/reviews/2026-09-13-v0.4.4-external-rereview-2.md
```

Review lineage:

```text
original review HEAD:      34f77c036206d48bbf9495ea73e5debcea9f0eb3
all-four-fix checkpoint:   8b01d4b5ba84112f36e46b234426f754be38c8f6
first re-review HEAD:       e022dbf581846c4699b724df063cd68b47dff332
B1 typed-operand HEAD:      5fba7fd08e62e721e96e5a93bc65ab153d663f66
second re-review HEAD:      f98f05a624fe4262615db992fbe3f9ea3bd43bce
```

V0.4.3 remains the last accepted package:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

Do **not** mark V0.4.4 PASS and do **not** advance V0.4.5 until the remaining B1 gap is fixed and independently re-reviewed.

## Current blocker disposition

```text
B1 typed operand semantics                         PASS
B1 complete condition-set comparison              PASS
B1 proven context stripping                        BLOCK
B2 repeated-build canonical parity                 PASS
B3 capability-flow anti-overfit                    PASS
B4 frontend product-source scope                   PASS
```

## Remaining B1 gap

`ValidationConsistencyCandidateEnricher` still strips path roots more broadly than PKC can prove.

Angular currently reduces:

```text
<root>.value.<field>
<root>.controls.<field>.value
```

for any Angular validation fact, without proving `<root>` is the form associated with the current binding.

Therefore this can false-match:

```text
UI:      this.domain.value.status === 'Active'
Backend: request.Status == Status.Active
```

Likewise the backend currently strips a root if it matches **any** endpoint parameter name. That can false-match a guard on an unrelated context/service parameter, for example:

```text
CreateRequest request, StateTracker tracker
tracker.Status == Status.Active && request.TargetId == null
```

The condition on `tracker.Status` must not be treated as a request-field condition merely because `tracker` is an endpoint parameter.

Required deterministic rule:

```text
Angular:
strip form scaffolding only when the root is proven to be the relevant form root.

Backend:
strip only the proven validated/request-object root for that validation fact
(for example its `parameterName`), not every endpoint parameter.

No proof:
preserve the path or return conservative non-consistent classification.
```

Add regression tests for both false-equivalence cases before the fix.

## Automated evidence on typed-operand candidate

For `5fba7fd08e62e721e96e5a93bc65ab153d663f66`:

```text
CI / WorkPlay + PokeTrade    PASS  run 34742742074
pinned Loren                 PASS  run 34742742056
Loren-main canary            PASS  run 34742742039
C# tests                     46 / 46 PASS
frontend tests                8 / 8 PASS
```

These gates remain valuable regression evidence but do not override the remaining deterministic false-equivalence path.

## Exact next action

Stay in V0.4.4 and fix **only** B1 context-root proof, regression-first.

Then rerun full tests, PokeTrade, pinned Loren, Loren-main canary and handoff parity, update status/handoff, and request final independent B1 re-review.

B2/B3/B4 remain closed unless a new regression proves otherwise.

Warnings W1 (claim-level portable provenance) and W2 (durable blind-review evidence) remain non-blocking follow-up concerns.
