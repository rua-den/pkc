# PKC Status

Last updated: 2026-09-15

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                 PASS / COMPLETE
V0.4.5 real-repository generalization            PASS / COMPLETE
V0.4.6 business logic reconstruction             INDEPENDENT RE-REVIEW FAIL / 1 BLOCKER
V0.4.7 cross-layer PO-question readiness         LOCKED
V0.5 Azure DevOps input evidence                 LOCKED
```

Latest independently reviewed production checkpoint:

```text
18a1f1d1ef551833d859f23ea2e92dd548a6a81d
fix: fail closed on unsafe same-type projector effects
```

Latest independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-6.md
verdict: FAIL / FIX REQUIRED
```

V0.4.3 remains the last accepted tool package:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

## Product contract governing V0.4.x

PKC must generate portable knowledge rich enough for an AI to answer practical Product Owner questions about observable behavior, business conditions, value origin, mutation causality and cross-layer outcomes without re-reading source code.

Permanent guardrail: `docs/product-knowledge-contract.md`.

Keep distinct and retain:

```text
business conditions
value lineage / provenance
mutation / causality
```

No deterministic proof means no authoritative product claim. Downgrading rule authority must not erase deterministic lower-authority causal/value-origin evidence.

## V0.4.6 disposition

```text
B6.1 PASS — exact C# invocation semantic identity; keep closed
B6.2 PASS — conservative configured-item ownership; keep closed
B6.3 PASS — module-qualified Angular service ownership; keep closed
B6.4 BLOCK — delegate-bearing Where pipeline operations can invalidate predicate state after filtering
```

### Re-review-5 blocker is closed

Checkpoint `18a1f1d1...` correctly closes the prior same-type projector custom-setter gap by validating every supported clone initializer assignment, not only the predicate-required copies.

Focused regression remains green:

```text
Later_custom_setter_that_invalidates_predicate_state_downgrades_projection_authority
```

### Remaining B6.4 blocker

`WherePipelineTargets` still unconditionally treats known LINQ targets such as `OrderBy*` and `ThenBy*` as predicate-semantics-preserving. `IsAllowedWherePipelineInvocation()` returns `true` based on target membership without proving that the user-supplied selector/comparer cannot mutate predicate-relevant state.

Compile-valid counterexample:

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

Observable behavior:

```text
Where sees IsPublished == true
→ item passes
→ OrderBy key selector sets IsPublished = false
→ returned item has IsPublished == false
```

Current authority logic may still promote:

```text
Includes items from `_cards` only when `card.IsPublished`.
```

That Product Owner claim is false for the returned item state.

Required generic boundary:

```text
known pipeline operation
+ exact overload/argument shape proven
+ every callback/comparer that can affect item semantics proven safe
→ may preserve authoritative Where semantics

otherwise
→ observed-only / no authoritative inclusion rule
```

A conservative V0.4.6 solution may fail closed for callback-bearing `OrderBy*` / `ThenBy*` unless effect safety is deterministically proven. Audit the rest of the allowlist for callback/comparer overloads rather than special-casing one spelling.

Required focused regression:

```text
Where(card => card.IsPublished)
→ OrderBy(card => card.IsPublished = false)
→ ToArray()
```

must not produce an authoritative returned-item inclusion rule.

## Exact automation for reviewed production checkpoint

All existing gates for exact SHA `18a1f1d1ef551833d859f23ea2e92dd548a6a81d` are green:

```text
CI + PKC tests + WorkPlay + PokeTrade   34954590262 — PASS
pinned Loren                            34954590188 — PASS
Loren-main canary                       34954590239 — PASS
pinned Jellyfin                         34954590155 — PASS
```

Core CI:

```text
PKC build:       0 warnings / 0 errors
C# tests:        77 / 77 PASS
frontend tests:  13 / 13 PASS
WorkPlay:        PASS
PokeTrade:       PASS
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
bundle canonical parity: PASS
ZIP file-set parity:     PASS
ZIP byte parity:         PASS
raw .pkc leak:           none
src/ source-tree leak:   none
artifact id:             10390258392
artifact digest:         sha256:6ba875d99cf64141267bb13811485cc025eabc87723beaa9f3a46a985860b497
```

Green CI is final verification evidence, not proof that semantic authority is correct.

## Exact next action

Stay in V0.4.6.

Fix only the remaining B6.4 callback/pipeline preservation gap regression-first, review the complete local diff, make one coherent implementation commit and one push, then request another independent review of the new exact production SHA.

Until independent PASS:

```text
V0.4.7 LOCKED
V0.5 LOCKED
```
