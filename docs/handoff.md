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

Latest reviewed production checkpoint:

```text
a636172ea8575f46d51e503d1ba7ad6d861650fb
fix: preserve proven same-type Select projections
```

Latest independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-4.md
verdict: FAIL / FIX REQUIRED
```

Current disposition:

```text
B6.1  PASS / keep closed
B6.2  PASS / keep closed
B6.3  PASS / keep closed
B6.4  BLOCK — same-type projection proof does not prove complete predicate dependencies
```

Do not start V0.4.7 or Azure DevOps.

## Accepted/closed scope

### B6.1 — PASS

The same-line semantic-authority collision is closed. Final C# predicate authority re-resolves the exact invocation by syntax `SpanStart` in the target project semantic model and accepts only the exact supported LINQ symbol.

Regression:

```text
Same_line_custom_Where_cannot_borrow_real_Linq_Where_semantic_authority
```

Do not reopen without a new concrete contradiction.

### B6.2 — PASS

Configured-item ownership remains conservative for reviewed supported initializer forms. Nested property objects are not promoted as direct configured items of the source collection.

Do not reopen without a new concrete contradiction.

### B6.3 — PASS

Module-qualified Angular service ownership requires lexically active relative import evidence. Comment/string/template import-like text has no authority. Conflicting active local-name imports are ambiguous, and unresolved/non-relative ownership is omitted rather than guessed.

Regression:

```text
Inactive_import_like_text_does_not_override_active_service_module
```

Independent re-review 3 accepted B6.3. Do not reopen without a new concrete contradiction.

## B6.4 — remaining blocker

Independent re-review 3 found that arbitrary LINQ `Select` was incorrectly treated as preserving a preceding returned `Where` predicate.

That specific defect is now closed at `a636172...`:

- `Select` is no longer in the unconditional preserving allowlist;
- direct identity `card => card` is proven by Roslyn symbol equality;
- arbitrary projection such as `.Select(_ => _cards[0])` is rejected;
- a narrow same-type method-group projector may be accepted only after proving direct copies for every predicate member the implementation discovers.

The remaining defect is **dependency completeness**.

`GetWherePredicateMembers` only discovers direct field/property member accesses whose receiver is the predicate source parameter. It does not prove that those members are the complete semantic dependency set of the predicate.

A whole-item helper call can therefore be ignored while at least one direct member gives the projector enough evidence to pass.

Concrete counterexample:

```csharp
public sealed class Card
{
    public bool IsPublished { get; init; }
    public bool Blocked { get; init; }
}

private readonly List<Card> _cards =
[
    new() { IsPublished = true, Blocked = false }
];

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

Why current production can falsely promote it:

```text
predicate expression stored by PKC:
  card.IsPublished && IsAllowed(card)

member collector:
  sees card.IsPublished
  ignores whole-item use IsAllowed(card)
  requiredMembers = { IsPublished }

projector proof:
  Card -> Card
  sealed source type
  direct object initializer
  IsPublished = card.IsPublished proven
  Blocked rewrite is invisible to preservation proof

result:
  returned Where remains observable
  full predicate is rendered as authoritative inclusion rule
```

False possible knowledge claim:

```text
Includes items from `_cards` only when `card.IsPublished && IsAllowed(card)`.
```

But the returned clone has `Blocked=true`, so `IsAllowed(clone)` is false.

### Required generic fix property

Do not special-case helper names or this fixture.

The same-type projector path may retain authoritative `Where` semantics only when PKC proves that every semantic use of the predicate source parameter is covered by the preservation model.

A sufficient conservative V0.4.6 rule is:

```text
all source-parameter uses are supported direct stored-member reads
AND every required member is directly preserved by the projection
→ projection may preserve authoritative Where semantics

otherwise
→ observed-only / no product-level inclusion rule
```

Unsupported/unmodeled whole-item uses should downgrade, including shapes such as:

```text
helper(card)
card.SomeMethod()
ReferenceEquals(card, ...)
custom/operator semantics involving card
other helper indirection
```

### Required regression-first coding action

Add a focused regression reproducing the exact semantic defect before changing production code:

```text
Where(card => card.IsPublished && IsAllowed(card))
→ Select(CloneCard)
```

where `IsAllowed` depends on `Blocked` and the clone changes `Blocked` while copying `IsPublished`.

The generated knowledge must not contain the authoritative full-predicate inclusion rule.

Then implement the minimum generic dependency-completeness fix. Keep the existing direct-member safe same-type clone regression green if it remains deterministically provable.

Do not refactor unrelated areas.

## Exact reviewed gates

All final gates for production checkpoint `a636172ea8575f46d51e503d1ba7ad6d861650fb` are green:

```text
CI + full PKC tests + WorkPlay + PokeTrade   34931116584 — PASS
pinned Loren                                34931116570 — PASS
Loren-main canary                           34931116599 — PASS
pinned Jellyfin                             34931116503 — PASS
Jellyfin portable parity/provenance         PASS
```

Exact CI counts/results:

```text
PKC build:                 0 warnings / 0 errors
C# tests:                  75 / 75 PASS
frontend tests:            13 / 13 PASS
local tool pack/install:   PASS
WorkPlay build:            PASS
PokeTrade backend build:   PASS
PokeTrade Angular build:   PASS
PokeTrade live branches:   PASS
PokeTrade knowledge check: PASS
```

Pinned Jellyfin:

```text
jellyfin/jellyfin @ 1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
build:                   PASS, 0 warnings / 0 errors
facts:                   43,363
relations:               195,314
workflow candidates:     386
product features:        116
canonical Markdown:      504 files
analysis mode:           project-semantic 43,363 / 43,363
bundle parity:           PASS
ZIP file-set parity:     PASS
ZIP byte parity:         PASS
raw .pkc leak:           none
src/ source-tree leak:   none
artifact id:             10381642948
artifact digest:         sha256:1e4df6f7c0b77ce7a72488462c209b4b5a783647f88b1fb7c9c737e8a7ba507f
```

Green automation is supporting evidence only; it does not close the concrete false-product-claim path.

## Process note

The previous coding environment could not run the intended local red/green loop because the required local toolchain/network access was unavailable. The first checkpoint `ea9f423bdd6ab37658b632cdfa3fcd7c6f0c0d9f` reached final CI, which exposed a real PokeTrade false-negative; source inspection then produced the generic same-type proof in `a636172...`.

For the next fix, follow the normal local regression-first process wherever the environment permits. If local execution is genuinely unavailable, document that constraint explicitly rather than claiming local red/green evidence.

## Exact next action

Coding thread only:

```text
stay in V0.4.6
→ reproduce helper-dependent predicate gap with one focused regression
→ implement minimum generic dependency-completeness fix
→ focused related full validation
→ review diff
→ one coherent implementation push
→ rerun exact-head PokeTrade/Loren/Loren-main/Jellyfin/parity gates
→ independent re-review
```

Do not mark V0.4.6 complete yourself.
Do not start V0.4.7.
Do not start Azure DevOps ingestion.
