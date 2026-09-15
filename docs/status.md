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

V0.4.6 is **not complete**. Independent re-review 4 of production checkpoint `a636172ea8575f46d51e503d1ba7ad6d861650fb` found one remaining blocker-class B6.4 authority defect.

Latest independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-4.md
reviewed production: a636172ea8575f46d51e503d1ba7ad6d861650fb
verdict: FAIL / FIX REQUIRED
```

V0.4.3 remains the last accepted tool package:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

## Product contract governing V0.4.x

PKC must generate portable knowledge rich enough for an AI to answer practical Product Owner questions about observable behavior, business conditions and cross-layer outcomes without re-reading source code.

Representative question:

> When is entity X sellable/visible on the web, and what exact conditions must be true for it to appear?

Required evidence chain where source can prove it:

```text
configured/static values
→ business predicates and boolean semantics
→ backend selection/eligibility
→ API/result flow
→ DTO/computed transformation when relevant
→ frontend visibility/list/filter behavior
→ observable product outcome
```

No deterministic proof means no authoritative product claim. Runtime database/config/external values remain unknown unless grounded by another input.

## V0.4.6 blocker state

### B6.1 — PASS / keep closed

Exact C# predicate authority is re-resolved from the exact invocation syntax `SpanStart` in the target project semantic model. Same-line custom and genuine LINQ invocations cannot share authority merely because they occupy the same source line/range.

Regression:

```text
Same_line_custom_Where_cannot_borrow_real_Linq_Where_semantic_authority
```

Do not reopen without a new concrete contradiction.

### B6.2 — PASS / keep closed

Configured-item ownership remains conservative for the reviewed forms. Nested property object initializers are not promoted as direct collection items.

Do not reopen without a new concrete contradiction.

### B6.3 — PASS / keep closed

Module-qualified Angular service ownership requires lexically active relative import evidence. Inactive comments, strings and template literals have zero import authority; conflicting active local-name imports are ambiguous; unresolved ownership is omitted rather than guessed.

Regression:

```text
Inactive_import_like_text_does_not_override_active_service_module
```

Independent re-review 3 accepted B6.3.

### B6.4 — BLOCK / fix required

The re-review-3 arbitrary-`Select` defect is fixed: arbitrary projection is no longer unconditionally treated as preserving a returned `Where` predicate, and direct identity `Select` is proven by Roslyn symbol identity.

Production checkpoint `a636172...` additionally supports a narrow same-type method-group projection for the PokeTrade-style defensive clone shape. It proves that every **discovered direct predicate member** is copied input-member → output-member.

The remaining defect is that discovery is incomplete. `GetWherePredicateMembers` only records direct field/property member accesses on the predicate parameter. It does not prove that the parameter has no other semantic uses, such as being passed whole into a helper.

Concrete blocker:

```csharp
public IReadOnlyList<Card> GetCards() =>
    _cards
        .Where(card => card.IsPublished && IsAllowed(card))
        .Select(CloneCard)
        .ToArray();

private static bool IsAllowed(Card card) => !card.Blocked;

private static Card CloneCard(Card card) => new()
{
    IsPublished = card.IsPublished,
    Blocked = true
};
```

Current proof can collect only `IsPublished`, prove that member copied, ignore `IsAllowed(card)`, and keep the full predicate authoritative. The returned clone no longer satisfies `IsAllowed`, yet knowledge can claim:

```text
Includes items from `_cards` only when `card.IsPublished && IsAllowed(card)`.
```

Required boundary:

```text
all semantic dependencies of the predicate source parameter are proven and preserved
→ authoritative returned-item rule

otherwise
→ observed-only / no product-level inclusion claim
```

For V0.4.6, conservatively reject same-type projection preservation when the predicate parameter has unmodeled whole-item uses such as helper calls, instance methods, reference/object identity checks, custom/operator semantics, or other unsupported dependency paths.

Required focused regression before the fix:

```text
Where(card => card.IsPublished && IsAllowed(card))
→ Select(CloneCard)
```

where the helper depends on a member changed by the clone.

## Exact automation for reviewed production checkpoint

All final gates for `a636172ea8575f46d51e503d1ba7ad6d861650fb` are green, but green automation does not override the semantic blocker:

```text
CI + PKC tests + WorkPlay + PokeTrade   34931116584 — PASS
pinned Loren                            34931116570 — PASS
Loren-main canary                       34931116599 — PASS
pinned Jellyfin                         34931116503 — PASS
portable parity / provenance / no-leak  PASS inside Jellyfin run
```

Exact CI evidence:

```text
PKC build:                         0 warnings / 0 errors
C# tests:                          75 / 75 PASS
frontend tests:                    13 / 13 PASS
tool pack/install:                 PASS
WorkPlay:                          PASS
PokeTrade backend build:           PASS
PokeTrade Angular build:           PASS
PokeTrade live business branches:  PASS
PokeTrade generated knowledge:     PASS
```

Pinned Jellyfin evidence:

```text
repository:              jellyfin/jellyfin @ 1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
source build:            PASS, 0 warnings / 0 errors
facts:                   43,363
relations:               195,314
workflow candidates:     386
product features:        116
canonical Markdown:      504 files
analysis mode:           project-semantic 43,363 / 43,363
bundle canonical parity: PASS
ZIP exact file-set:      PASS
ZIP byte parity:         PASS
raw .pkc leak:           none
src/ source-tree leak:   none
artifact id:             10381642948
artifact digest:         sha256:1e4df6f7c0b77ce7a72488462c209b4b5a783647f88b1fb7c9c737e8a7ba507f
```

## Exact next action

Stay in V0.4.6 and fix only the remaining B6.4 dependency-completeness blocker regression-first.

Required process:

```text
focused red regression
→ generic conservative fix
→ focused/related/full local validation where possible
→ review complete diff
→ one coherent implementation checkpoint/push
→ exact-head PokeTrade/Loren/Loren-main/Jellyfin/parity gates
→ independent re-review
```

Do not mark V0.4.6 complete until that independent re-review returns PASS.
Do not start V0.4.7.
Do not start Azure DevOps ingestion.
