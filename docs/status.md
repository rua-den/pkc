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
eb0903ef93b2b85669ded0e2227ca1a950bc49c7
fix: fail closed on callback-bearing where pipelines
```

Latest independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-7.md
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
B6.4 BLOCK — same-type projector proof does not prove constructor effects safe
```

### Rereview-6 blocker is closed

Production `eb0903ef...` correctly closes the callback/comparer-bearing `Where` pipeline gap. The broad unconditional pipeline allowlist was replaced with an exact-shape callback-free safe subset. `OrderBy*`, `ThenBy*`, `Distinct`, and `ToHashSet` no longer preserve authoritative `Where` semantics merely from method identity.

Focused regressions now cover:

```text
Side_effecting_ordering_callback_downgrades_returned_filter_authority
Equality_comparer_pipeline_downgrades_returned_filter_authority
Default_item_equality_pipeline_downgrades_returned_filter_authority
Callback_free_enumerable_and_queryable_pipeline_remains_authoritative
```

### Remaining B6.4 blocker

The same-type method-group defensive-clone proof validates the object initializer but does not validate constructor effects.

Compile-valid counterexample:

```csharp
public sealed class Card
{
    public bool IsPublished { get; set; }
    public bool Blocked { get; set; }

    public Card() { }
    public Card(Card source) => source.IsPublished = false;
}

public IReadOnlyList<Card> GetCards() =>
    _cards
        .Where(card => card.IsPublished)
        .Select(CloneCard)
        .ToArray();

private static Card CloneCard(Card card) => new Card(card)
{
    IsPublished = card.IsPublished,
    Blocked = card.Blocked
};
```

Runtime:

```text
Where sees IsPublished == true
→ item passes
→ constructor mutates source IsPublished = false
→ initializer copies the now-false value
→ returned clone has IsPublished == false
```

Current proof can still accept the projector because the type is closed/sealed, the predicate member is an auto-property, and every initializer assignment is an exact same-member copy. Constructor arguments/body are not part of the preservation proof.

Required generic boundary:

```text
complete predicate dependency proof
+ constructor/object-creation path proven effect-safe
+ every initializer write proven safe
+ every required predicate member copied
→ projection may preserve authoritative Where semantics

otherwise
→ observed-only / no authoritative inclusion rule
```

A conservative V0.4.6 fix may require an implicit/default inert constructor and fail closed for constructor arguments or user-defined constructor code unless effect safety is deterministically proven.

Required regression: the constructor-mutating-source example above must not produce `Includes items from _cards only when card.IsPublished` as an authoritative returned-item rule.

## Exact automation for reviewed production checkpoint

All existing gates for exact SHA `eb0903ef93b2b85669ded0e2227ca1a950bc49c7` are green:

```text
CI + PKC tests + WorkPlay + PokeTrade   34959028651 — PASS
pinned Loren                            34959028686 — PASS
Loren-main canary                       34959028633 — PASS
pinned Jellyfin                         34959028626 — PASS
```

Core CI:

```text
PKC build:       0 warnings / 0 errors
C# tests:        81 / 81 PASS
frontend tests:  13 / 13 PASS
WorkPlay:        PASS
PokeTrade:       PASS
```

Pinned Jellyfin artifact:

```text
artifact id:     10391499835
digest:          sha256:39aab5f88914a2e153646b75fa9c8b6cece8a03f3f2dbc821fae871dab820dc2
size:            9,016,935 bytes
```

Recorded Jellyfin validation remains: 43,363 facts, 195,314 relations, 386 workflow candidates, 116 product features, 504 canonical Markdown files, full project-semantic provenance, portable bundle/ZIP parity, no raw `.pkc` leak and no `src/` leak.

Green CI is final verification evidence, not proof that semantic authority is correct.

## Exact next action

Stay in V0.4.6.

Fix only the remaining B6.4 constructor-effect preservation gap regression-first, review the complete diff, make one coherent implementation commit and one push, then request another independent review of the new exact production SHA.

Until independent PASS:

```text
V0.4.7 LOCKED
V0.5 LOCKED
```
