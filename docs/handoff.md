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

The V0.4.x exit standard is business-logic and PO-question readiness, not merely endpoint/workflow enumeration.

## Current state

```text
V0.4.4  Loren knowledge readiness              PASS / COMPLETE
V0.4.5  Jellyfin generalization               PASS / COMPLETE
V0.4.6  business logic reconstruction         INDEPENDENT RE-REVIEW FAIL / 1 BLOCKER
V0.4.7  cross-layer PO-question readiness     LOCKED
V0.5    Azure DevOps input evidence           LOCKED
```

Latest reviewed implementation checkpoint:

```text
9a0817b21075a3d810072a310ed8fd3314625cd8
fix: enforce observable authority boundaries
```

Latest independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-3.md
```

Current disposition:

```text
B6.1  PASS / keep closed
B6.2  PASS / keep closed
B6.3  PASS / independently accepted
B6.4  BLOCK / one remaining authority defect
```

Do not start V0.4.7 until B6.4 is fixed and V0.4.6 receives a fresh independent PASS.

## Accepted/closed scope

### B6.1 — PASS

The same-line semantic-authority collision remains closed. The final C# predicate authority stage re-resolves the exact invocation by syntax `SpanStart` in the target project semantic model and accepts only the exact supported LINQ symbol.

Regression:

```text
Same_line_custom_Where_cannot_borrow_real_Linq_Where_semantic_authority
```

Do not reopen without a new concrete contradiction.

### B6.2 — PASS

Configured-item ownership remains conservative for reviewed supported initializer forms. Nested property objects are not promoted as direct configured items of the source collection.

Do not reopen without a new concrete contradiction.

### B6.3 — PASS

The inactive-import authority blocker is closed.

Current generic behavior:

```text
API owner identity:
  normalized TypeScript source module path + exported class

component service identity:
  resolved relative active import module + exported class

inactive import-like text:
  line comments     → no authority
  block comments    → no authority
  quoted strings    → no authority
  template literals → no authority

conflicting active local-name imports:
  ambiguous → no deterministic identity

unresolved/non-relative module:
  no authoritative endpoint → result/list correlation
```

Regression:

```text
Inactive_import_like_text_does_not_override_active_service_module
```

Independent review found conservative false-negative edges but no remaining false ownership-authority path in V0.4.6 scope.

## Remaining blocker: B6.4 — arbitrary `Select` is not item-semantics-preserving

The previous B6.4 problems are fixed for negation, null/pattern transformations, arbitrary helper wrapping, discarded local queries and throwing guards.

The remaining problem is the `Where` receiver-chain allowlist.

Current production code treats both:

```text
System.Linq.Enumerable.Select
System.Linq.Queryable.Select
```

as operations that preserve the original `Where` predicate's observable inclusion semantics.

That is not true for arbitrary projections.

Concrete counterexample:

```csharp
private readonly List<Card> _cards =
[
    new Card(IsPublished: false),
    new Card(IsPublished: true)
];

public IReadOnlyList<Card> GetCards() =>
    _cards
        .Where(card => card.IsPublished)
        .Select(_ => _cards[0])
        .ToArray();
```

Actual behavior:

```text
Where keeps the published card
Select replaces it with unpublished _cards[0]
returned list contains an unpublished source card
```

Current PKC authority path can still promote the predicate as observable and synthesize:

```text
Includes items from `_cards` only when `card.IsPublished`.
```

That claim is false.

The existing positive regression only proves this safe identity projection:

```csharp
.Select(card => card)
```

It does not justify accepting arbitrary `Select` in production.

### Required generic fix property

```text
Where predicate
+ every outer receiver-chain operation proven to preserve output-item semantics
→ authoritative inclusion rule

otherwise
→ businessRuleAuthority=observed-only
→ observes-predicate
→ no product-level inclusion rule
```

For the current V0.4.6 scope, the safest minimal fix is either:

- prove that `Select` is identity-preserving before keeping authority; or
- treat arbitrary `Select` as non-preserving and downgrade it.

Do not create a selector-specific special case for the fixture. The rule must be generic.

### Regression-first coding sequence

Add a focused regression with:

```csharp
_cards
    .Where(card => card.IsPublished)
    .Select(_ => _cards[0])
    .ToArray();
```

Before the fix, confirm that generated product knowledge incorrectly contains:

```text
Includes items from `_cards` only when `card.IsPublished`.
```

After the generic fix, that authoritative rule must disappear.

Keep the existing identity-projection positive case green if identity preservation is intentionally modeled.

Use the repository workflow:

```text
focused red
→ generic fix
→ focused green
→ related tests
→ full relevant local suite
→ review complete diff
→ one coherent implementation commit
→ one push
→ final CI verification
```

Do not use GitHub Actions as the edit-test loop.

## Exact reviewed-checkpoint gates

All final gates for `9a0817b21075a3d810072a310ed8fd3314625cd8` were green:

```text
CI + PKC tests + WorkPlay + PokeTrade   34920522723 — PASS
pinned Loren                            34920522961 — PASS
Loren-main canary                       34920522799 — PASS
pinned Jellyfin                         34920522831 — PASS
Jellyfin portable parity/provenance     PASS
```

Exact CI counts/results:

```text
PKC build:                 0 warnings / 0 errors
C# tests:                  72 / 72 PASS
frontend tests:            13 / 13 PASS
local tool pack/install:   PASS
WorkPlay build:            PASS
PokeTrade backend build:   PASS
PokeTrade Angular build:   PASS
PokeTrade live branches:   PASS
PokeTrade knowledge check: PASS
```

Real-project gates:

```text
pinned Loren build + compile:       PASS
current Loren-main build + compile: PASS
pinned Jellyfin source build:       PASS, 0 warnings / 0 errors
Jellyfin PKC facts:                 43,363
Jellyfin relations:                 195,314
workflow candidates:                386
product features:                   116
canonical Markdown files:           504
analysis mode:                      project-semantic 43,363 / 43,363
```

Portable verification:

```text
canonical Markdown preserved verbatim in single-file bundle: PASS
ZIP exact file-set parity:                                  PASS
ZIP byte parity:                                            PASS
raw .pkc leak:                                              none
src/ source-tree leak:                                      none
```

Jellyfin artifact:

```text
artifact id:     10377409728
artifact digest: sha256:74ded0b0e5271b1731599b3490d7013ad07dde74445e893868320e2e657f714d
```

Green automation is supporting evidence only and does not override B6.4.

## Exact next action

Coding thread only:

1. read `docs/status.md`;
2. read `docs/handoff.md`;
3. read `docs/reviews/2026-09-15-v0.4.6-independent-rereview-3.md`;
4. stay in V0.4.6;
5. fix only the remaining B6.4 `Select` item-semantics authority blocker regression-first;
6. validate locally before pushing;
7. make one coherent implementation commit/push;
8. rerun all current gates;
9. update status/handoff with the new exact checkpoint;
10. request another independent re-review.

Do not start V0.4.7.
Do not start Azure DevOps ingestion.
