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

Permanent guardrail: `docs/product-knowledge-contract.md`.

Keep distinct and retain:

```text
business conditions
value lineage / provenance
mutation / causality
```

Conservative downgrade of product-rule authority must not erase deterministic lower-authority evidence.

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
eb0903ef93b2b85669ded0e2227ca1a950bc49c7
fix: fail closed on callback-bearing where pipelines
```

Latest independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-7.md
verdict: FAIL / FIX REQUIRED
```

Current disposition:

```text
B6.1 PASS / keep closed
B6.2 PASS / keep closed
B6.3 PASS / keep closed
B6.4 BLOCK / same-type projector constructor effects remain unproven
```

Do not start V0.4.7 or Azure DevOps until V0.4.6 independently passes.

## Closed scope

### B6.1 — PASS

Exact C# predicate authority is tied to exact invocation syntax/semantic identity.

### B6.2 — PASS

Configured-item ownership remains conservative; nested property object initializers are not promoted as direct collection items.

### B6.3 — PASS

Angular service correlation requires lexically active module-qualified relative import evidence.

### Rereview-6 callback blocker — CLOSED

`eb0903ef...` replaces the broad unconditional `Where` pipeline allowlist with a callback-free exact-shape subset.

Allowed safe subset currently covers zero-argument `Reverse`, `AsEnumerable`, `AsQueryable`, `ToArray`, `ToList`, plus exact scalar/range `Skip` / `Take` shapes. Ordering, equality/comparer, `Distinct`, and `ToHashSet` paths do not retain authority without explicit proof.

New callback/comparer regressions are green in exact-SHA CI.

## B6.4 — remaining blocker

The same-type method-group projection proof validates initializer writes but does not validate object-constructor effects.

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

Execution:

```text
Where sees source IsPublished == true
→ item passes
→ CloneCard constructor mutates source IsPublished = false
→ initializer copies false into clone
→ returned clone IsPublished == false
```

Current proof still sees a sealed same-type projector with safe-looking direct same-member initializer assignments and can preserve the earlier `Where` predicate.

## Required coding fix

Fix only this constructor-effect authority gap, regression-first.

Required generic property:

```text
complete predicate dependency proof
+ same closed item type
+ object-construction path proven effect-safe
+ every initializer assignment proven side-effect-safe
+ every required predicate member copied
→ may preserve authoritative Where semantics

otherwise
→ fail closed / observed-only / no authoritative inclusion rule
```

A conservative V0.4.6 solution is acceptable. The supported defensive-clone path may require an implicit/default inert constructor and reject constructor arguments or user-defined constructor bodies unless their effects are deterministically proven safe.

Do not special-case `Card`, `IsPublished`, or the exact example.

Required focused regression: a source-mutating constructor invoked by the projector must downgrade the earlier `Where` authority.

Keep the existing positive defensive-clone regression green only where the constructor path is deterministically inert.

Do not reopen the callback-pipeline fix, custom-setter fix, whole-item predicate dependency fix, or B6.1–B6.3 unless a concrete contradiction requires it.

## Exact reviewed checkpoint gates

All automation is green on exact production SHA `eb0903ef93b2b85669ded0e2227ca1a950bc49c7`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   34959028651 — PASS
pinned Loren                                34959028686 — PASS
Loren-main canary                           34959028633 — PASS
pinned Jellyfin                             34959028626 — PASS
```

Core CI:

```text
PKC build:                 0 warnings / 0 errors
C# tests:                  81 / 81 PASS
frontend tests:            13 / 13 PASS
WorkPlay:                  PASS
PokeTrade:                 PASS
```

Pinned Jellyfin artifact:

```text
artifact id:     10391499835
digest:          sha256:39aab5f88914a2e153646b75fa9c8b6cece8a03f3f2dbc821fae871dab820dc2
size:            9,016,935 bytes
```

## Coding-thread workflow

1. Read `docs/status.md` and this handoff first.
2. Inspect current `main` HEAD and rereview 7.
3. Add one focused regression for constructor-side-effect invalidation.
4. Confirm the defect locally if tooling permits.
5. Implement the minimum generic fail-closed constructor boundary.
6. Run focused/related tests locally, then broader relevant tests/build.
7. Review the full diff.
8. Commit regression + fix together in one coherent implementation commit.
9. Push once.
10. Record the exact production SHA and exact-head gate evidence.
11. Stop and request another independent V0.4.6 review.

Do not use GitHub Actions as the normal edit/test loop.

Until independent PASS:

```text
V0.4.7 LOCKED
V0.5 LOCKED
```
