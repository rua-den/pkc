# PKC Status

Last updated: 2026-09-15

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                 PASS / COMPLETE
V0.4.5 real-repository generalization            PASS / COMPLETE
V0.4.6 business logic reconstruction             IMPLEMENTATION GREEN / INDEPENDENT RE-REVIEW PENDING
V0.4.7 cross-layer PO-question readiness         LOCKED AGAIN
V0.5 Azure DevOps input evidence                 LOCKED
```

Current reviewed production checkpoint:

```text
868195eff5435cca1c98d4bf6ffd4b18018daf66
fix: require inert clone construction
```

Latest independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-9.md
verdict: FAIL / REOPEN V0.4.6
```

Implementation status: `IMPLEMENTATION GREEN / INDEPENDENT RE-REVIEW PENDING`. Queryable predicates remain observed evidence without Product Owner authority, and Enumerable `Where` authority fails closed across Queryable pipeline hops. V0.4.7 and V0.5 remain locked.

Local implementation validation:

```text
TDD RED — removing both Queryable guards produced expected observed-only, actual observable.
TDD RED — before evidence retention, the downgraded predicate was absent from portable Evidence.
Focused WherePipelineCallbackAuthorityRegressionTests: 7 / 7 PASS
Full C# suite: 88 / 88 PASS
Frontend suite: 13 / 13 PASS
Release build: 0 warnings / 0 errors
Environment: DOTNET_ROLL_FORWARD=LatestMajor required for net8 test host with SDK 10 MSBuild discovery.
```

Rereview 8 remains valid for the constructor-effect boundary it accepted, but its milestone-complete disposition is superseded by rereview 9's newly discovered Queryable provider-authority contradiction.

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
B6.4 BLOCK — Queryable predicate authority assumes provider semantics that are not proven
```

### Prior B6.4 hardening remains accepted

Keep closed absent a concrete contradiction:

- discarded/local predicate downgrade;
- transformed/polarity-changing return-context fail-closed behavior;
- arbitrary `Select` rejection and identity projection proof;
- whole-item predicate dependency completeness;
- safe direct clone-member copies;
- custom setter / initializer effect fail-closed behavior;
- callback/comparer-bearing ordering/equality pipeline fail-closed behavior;
- exact-shape callback-free Enumerable pipeline preservation;
- inert-constructor requirement for same-type defensive clones.

Rereview 8's constructor analysis is not being reversed.

### Remediated boundary — `Queryable` provider trust (independent review pending)

The semantic model retains exact `System.Linq.Queryable.<operation>` targets as deterministic evidence, but no Queryable operation establishes Product Owner authority without a proven provider boundary.

For `Queryable.Where`, resolving the exact BCL method only proves that an expression tree is created and handed to `source.Provider.CreateQuery(...)`. Runtime query behavior depends on the provider implementation.

Compile-valid behavior-valid counterexample shape:

```csharp
private readonly IQueryable<Card> _cards =
    new IgnoringQuery<Card>(
    [
        new Card { IsPublished = false }
    ]);

public IReadOnlyList<Card> GetCards() =>
    _cards
        .Where(card => card.IsPublished)
        .ToList();
```

where `IgnoringQuery<T>` implements `IQueryable<T>` / `IQueryProvider`, retains the expression passed to `CreateQuery`, but enumerates the underlying items without interpreting the `Where` expression.

Observable result:

```text
underlying item IsPublished == false
→ Queryable.Where delegates expression to Provider.CreateQuery
→ provider returns query carrying the expression
→ ToList enumerates provider-backed query
→ provider yields the false item
→ returned item does not satisfy card.IsPublished
```

Current PKC can still promote:

```text
Includes items from `_cards` only when `card.IsPublished`.
```

That Product Owner statement is false.

Required generic boundary:

```text
exact Queryable operation
+ source/provider identity proven
+ provider semantics for that operation proven/trusted
+ observable execution path proven
→ authoritative Product Owner rule

otherwise
→ observed-only / no authoritative product rule
```

A conservative V0.4.6 implementation downgrades Queryable business predicates and rejects Enumerable authority when the returned path crosses any Queryable hop. It does not preserve outputs through benchmark/provider-name special cases.

Required focused regression: custom `IQueryable<T>` / `IQueryProvider` that ignores the `Where` expression during enumeration must not yield an authoritative inclusion rule.

## Exact automation for current production

All existing gates remain green on exact SHA `868195eff5435cca1c98d4bf6ffd4b18018daf66`:

```text
CI + PKC tests + WorkPlay + PokeTrade   34964195712 — PASS
pinned Loren                            34964195642 — PASS
Loren-main canary                       34964195717 — PASS
pinned Jellyfin                         34964195689 — PASS
```

Core CI:

```text
PKC build:       0 warnings / 0 errors
C# tests:        85 / 85 PASS
frontend tests:  13 / 13 PASS
WorkPlay:        PASS
PokeTrade:       PASS
```

Pinned Jellyfin artifact:

```text
artifact id:     10394456737
digest:          sha256:586863e971967be62ec6e0a7763cc90806903621d43ea896813e4cf3b3a2e414
size:            9,016,935 bytes
head SHA:        868195eff5435cca1c98d4bf6ffd4b18018daf66
```

Green automation does not exercise the custom-provider contradiction and does not override the semantic blocker.

## Exact next action

Stay in V0.4.6.

Review the complete implementation diff, make the implementation commit and push, verify exact-SHA CI and external gates, then request fresh independent review. Do not claim V0.4.6 accepted or complete before that review passes.

Until that review passes:

```text
V0.4.7 LOCKED
V0.5 LOCKED
```
