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
V0.4.6  business logic reconstruction         IMPLEMENTATION GREEN / INDEPENDENT RE-REVIEW PENDING
V0.4.7  cross-layer PO-question readiness     LOCKED AGAIN
V0.5    Azure DevOps input evidence           LOCKED
```

Current reviewed production checkpoint:

```text
868195eff5435cca1c98d4bf6ffd4b18018daf66
fix: require inert clone construction
```

Latest independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-9.md
FAIL / REOPEN V0.4.6
```

Current implementation status: `IMPLEMENTATION GREEN / INDEPENDENT RE-REVIEW PENDING`. Queryable predicates remain observed evidence without Product Owner authority, and Enumerable `Where` authority fails closed across Queryable pipeline hops. V0.4.7 and V0.5 remain locked.

Local validation is green: focused authority regressions 7/7, full C# suite 88/88, frontend suite 13/13, and Release build 0 warnings/0 errors. TDD mutation RED removing both Queryable guards produced expected observed-only versus actual observable; the evidence-retention RED showed the downgraded predicate missing from portable Evidence before `observes-predicate` traversal was added. `DOTNET_ROLL_FORWARD=LatestMajor` was required because the net8 test host runs with only SDK 10 MSBuild discovery. Commit, push, exact-SHA gates, and fresh independent review remain outstanding.

Rereview 8 remains accepted for its constructor-effect analysis. V0.4.6 was reopened only because rereview 9 found a separate provider-semantics contradiction.

## Closed scope

Keep closed unless a new concrete contradiction appears:

```text
B6.1 PASS — exact C# invocation semantic identity
B6.2 PASS — conservative configured-item ownership
B6.3 PASS — module-qualified Angular service ownership
```

Previously fixed B6.4 sub-boundaries also remain accepted:

1. discarded/local predicates downgrade;
2. transformed/polarity-changing `Any`/`All`/`First*`/`Single*` contexts fail closed;
3. arbitrary `Select` is not preserving by default;
4. identity `Select(card => card)` uses symbol identity;
5. whole-item/unmodeled predicate dependencies fail closed;
6. direct defensive clones require stored same-member copies;
7. custom setters, nested writes and rewritten initializer effects fail closed;
8. ordering/equality callback/comparer paths do not preserve authority merely from LINQ target identity;
9. callback-free Enumerable pipeline preservation is exact-shape and conservative;
10. same-type defensive clone construction must be deterministically inert.

Do not reopen rereview 8's constructor fix without a separate contradiction.

## B6.4 — current blocker: Queryable provider semantics

Current business-predicate extraction and authority filtering accept `System.Linq.Queryable.<operation>` alongside `System.Linq.Enumerable.<operation>`.

The prior model was insufficient for authoritative product semantics because Queryable execution is provider-mediated.

`Queryable.Where` builds an expression tree and passes it to `source.Provider.CreateQuery(...)`. The provider decides how that expression is interpreted and how enumeration executes.

### Counterexample shape

Use a custom `IQueryable<T>` / `IQueryProvider` that stores the expression passed to `CreateQuery` but enumerates the underlying items without applying the expression.

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

Runtime:

```text
Queryable.Where delegates expression to Provider.CreateQuery
→ provider returns an IQueryable carrying the expression
→ ToList enumerates it
→ provider ignores the Where expression and yields the false item
→ returned Card has IsPublished == false
```

Current PKC can still preserve the `Where` as observable and emit:

```text
Includes items from `_cards` only when `card.IsPublished`.
```

That is false for the returned item.

## Implemented coding boundary — independent review pending

Fix only this provider-trust authority boundary, regression-first.

Required generic property:

```text
exact Queryable target
+ provider/source identity proven
+ semantics of that provider for the operation proven
+ observable execution path proven
→ authoritative product rule

otherwise
→ observed-only / no authoritative rule
```

A conservative V0.4.6 implementation may downgrade `Queryable` predicates unless their provider semantics are deterministically proven.

If a positive LINQ-to-Objects `AsQueryable()` path is retained, prove the provider/source identity rather than trusting the `Queryable` method name.

Do not special-case PokeTrade, Loren, Jellyfin, EF, `Card`, `IsPublished`, or the exact test fixture.

Required regression: a custom provider that ignores `Where` during enumeration must not lead to an authoritative returned-item inclusion rule.

Authority downgrade must continue to retain deterministic observed-only predicate/mutation/provenance evidence.

## Exact current-production gates

All automation is green on exact SHA `868195eff5435cca1c98d4bf6ffd4b18018daf66`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   34964195712 — PASS
pinned Loren                                34964195642 — PASS
Loren-main canary                           34964195717 — PASS
pinned Jellyfin                             34964195689 — PASS
```

Core evidence:

```text
PKC build:                 0 warnings / 0 errors
C# tests:                  85 / 85 PASS
frontend tests:            13 / 13 PASS
WorkPlay:                  PASS
PokeTrade:                 PASS
```

Pinned Jellyfin artifact:

```text
artifact id:   10394456737
digest:        sha256:586863e971967be62ec6e0a7763cc90806903621d43ea896813e4cf3b3a2e414
size:          9,016,935 bytes
head SHA:      868195eff5435cca1c98d4bf6ffd4b18018daf66
```

Green CI does not cover the new custom-provider contradiction.

## Coding-thread workflow

1. Read `docs/status.md`, this handoff, `docs/milestones.md`, `docs/product-knowledge-contract.md`, and rereview 9.
2. Inspect current `main` HEAD.
3. Add the focused custom-provider regression first.
4. Reproduce locally if tooling permits.
5. Implement the minimum generic provider-trust/fail-closed boundary.
6. Run focused and related C# tests, then the broader relevant suite/build locally.
7. Review the complete diff and remove debug code.
8. Update status/handoff/milestones to `IMPLEMENTATION GREEN / INDEPENDENT RE-REVIEW PENDING`.
9. Commit regression + fix together in one coherent implementation commit.
10. Push once.
11. Verify exact-SHA CI, WorkPlay, PokeTrade, pinned Loren, Loren-main, pinned Jellyfin, and portable parity/no-leak gates.
12. Stop and request another independent V0.4.6 review; do not claim acceptance or completion.

Do not use GitHub Actions as the normal edit/test loop.

Until independent PASS:

```text
V0.4.7 LOCKED
V0.5 LOCKED
```
