# PKC Handoff

Use this file when continuing PKC in another coding or independent-review thread.

## Product contract

PKC is a deterministic Product/System Knowledge Compiler. A Product Owner should be able to hand generated portable knowledge to an AI and ask practical product/system questions without the AI re-reading source code.

Keep the architecture:

```text
source inputs
→ deterministic analyzers/adapters
→ evidence/facts
→ feature/workflow/business-decision candidates
→ knowledge synthesis
→ canonical model
→ portable rendering
```

Do not add direct source-to-freeform-AI generation.

Permanent product-knowledge guardrail: `docs/product-knowledge-contract.md`.

Keep distinct and retain:

```text
business conditions
value lineage / provenance
mutation / causality
```

Conservative downgrade of product-rule authority must not erase deterministic lower-authority evidence that can explain where a value came from or what code path changed it.

## Current state

```text
V0.4.4  Loren knowledge readiness              PASS / COMPLETE
V0.4.5  Jellyfin generalization               PASS / COMPLETE
V0.4.6  business logic reconstruction         INDEPENDENT RE-REVIEW FAIL / 1 BLOCKER
V0.4.7  cross-layer PO-question readiness     LOCKED
V0.5    Azure DevOps input evidence           LOCKED
```

Latest reviewed production checkpoint:

```text
18a1f1d1ef551833d859f23ea2e92dd548a6a81d
fix: fail closed on unsafe same-type projector effects
```

Latest independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-6.md
verdict: FAIL / FIX REQUIRED
```

Current disposition:

```text
B6.1 PASS / keep closed
B6.2 PASS / keep closed
B6.3 PASS / keep closed
B6.4 BLOCK / callback-bearing Where pipeline preservation remains unsafe
```

Do not start V0.4.7 or Azure DevOps until V0.4.6 independently passes.

## Closed scope

### B6.1 — PASS

Exact C# predicate authority is tied to the exact invocation syntax/semantic identity. Same-line custom and genuine LINQ calls cannot borrow authority.

### B6.2 — PASS

Configured-item ownership remains conservative; nested property object initializers are not promoted as direct collection items.

### B6.3 — PASS

Angular service correlation requires lexically active module-qualified relative import evidence. Ambiguous/unresolved ownership is omitted rather than guessed.

## B6.4 — remaining blocker

Re-review 5's same-type projector custom-setter gap is closed by `18a1f1d1...`.

The same-type method-group clone path now requires every supported initializer assignment to be a direct same-member input→output copy targeting a direct stored non-static field or auto-property. Custom setters and unmodeled initializer effects therefore fail closed.

Focused regression that remains green:

```text
Later_custom_setter_that_invalidates_predicate_state_downgrades_projection_authority
```

However, the broader `Where` pipeline still contains delegate-bearing operations that are accepted solely from their LINQ semantic target.

Current unconditional allowlist includes:

```text
OrderBy / OrderByDescending
ThenBy / ThenByDescending
```

and their Enumerable/Queryable targets. `IsAllowedWherePipelineInvocation()` returns true for a target in `WherePipelineTargets` without proving that the operation's selector/comparer cannot mutate predicate-relevant item state.

### Compile-valid counterexample

```csharp
public sealed class Card
{
    public bool IsPublished { get; set; }
}

private readonly List<Card> _cards =
[
    new() { IsPublished = true }
];

public IReadOnlyList<Card> GetCards() =>
    _cards
        .Where(card => card.IsPublished)
        .OrderBy(card => card.IsPublished = false)
        .ToArray();
```

Runtime:

```text
Where sees IsPublished == true
→ item passes
→ OrderBy key selector sets IsPublished = false
→ ToArray returns the same Card instance
→ returned item has IsPublished == false
```

Current authority logic may still emit:

```text
Includes items from `_cards` only when `card.IsPublished`.
```

That Product Owner claim is false for the returned item state.

## Required coding fix

Fix only this B6.4 boundary, regression-first.

Required generic property:

```text
exact LINQ operation proven
+ exact overload/argument shape proven
+ every callback/comparer that can execute during enumeration proven unable to invalidate predicate semantics
→ pipeline operation may preserve authoritative Where semantics

otherwise
→ observed-only / no authoritative inclusion rule
```

A conservative V0.4.6 fix is acceptable. Callback-bearing `OrderBy*` / `ThenBy*` can simply stop preserving authority unless callback safety is deterministically proven.

Audit the remaining `WherePipelineTargets` overloads for user callbacks/comparers rather than special-casing only the literal `OrderBy` name.

Required focused regression:

```csharp
_cards
    .Where(card => card.IsPublished)
    .OrderBy(card => card.IsPublished = false)
    .ToArray();
```

must not produce an authoritative inclusion rule.

Preserve the accepted positive direct-return / safe materialization cases where preservation is truly deterministic.

## Exact reviewed checkpoint gates

All current automated gates are green on exact production SHA `18a1f1d1ef551833d859f23ea2e92dd548a6a81d`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   34954590262 — PASS
pinned Loren                                34954590188 — PASS
Loren-main canary                           34954590239 — PASS
pinned Jellyfin                             34954590155 — PASS
```

Core evidence:

```text
PKC build:                 0 warnings / 0 errors
C# tests:                  77 / 77 PASS
frontend tests:            13 / 13 PASS
WorkPlay build:            PASS
PokeTrade backend build:   PASS
PokeTrade Angular build:   PASS
PokeTrade acceptance:      PASS
PokeTrade knowledge check: PASS
```

Pinned Jellyfin:

```text
jellyfin/jellyfin @ 1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
source build:            PASS, 0 warnings / 0 errors
facts:                   43,363
relations:               195,314
workflow candidates:     386
product features:        116
canonical Markdown:      504
analysis mode:           project-semantic 43,363 / 43,363
bundle parity:           PASS
ZIP file-set parity:     PASS
ZIP byte parity:         PASS
raw .pkc leak:           none
src/ source-tree leak:   none
artifact id:             10390258392
artifact digest:         sha256:6ba875d99cf64141267bb13811485cc025eabc87723beaa9f3a46a985860b497
```

Green CI does not override the semantic blocker.

## Coding-thread workflow

1. Read `docs/status.md` and this handoff first.
2. Inspect current `main` HEAD and this latest review.
3. Add one focused regression for the side-effecting `OrderBy` callback.
4. Confirm the defect locally if tooling permits.
5. Implement the minimum generic fail-closed fix and audit callback/comparer-bearing allowlist entries.
6. Run focused and related tests locally, then the broader relevant suite.
7. Review the full diff.
8. Commit the regression + fix together in one coherent implementation commit.
9. Push once.
10. Record exact production SHA and final gate evidence.
11. Stop and request another independent V0.4.6 review.

Do not use GitHub Actions as the normal edit/test loop.

Until independent PASS:

```text
V0.4.7 LOCKED
V0.5 LOCKED
```
