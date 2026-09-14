# PKC Handoff

Use this file when continuing PKC in another coding or review thread.

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
V0.4.6  business logic reconstruction         FAIL / FIX REQUIRED
V0.4.7  cross-layer PO-question readiness     LOCKED
V0.5    Azure DevOps input evidence           LOCKED
```

Latest independent V0.4.6 review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-2.md
```

Reviewed implementation checkpoint:

```text
34c3bf00233ac4c0c4df717f7f7c5ae55fffe07b
```

Independent disposition:

```text
B6.1  PASS
B6.2  PASS
B6.3  BLOCK
B6.4  BLOCK
```

Do not mark V0.4.6 complete. Do not start V0.4.7 or Azure DevOps.

## Read first in the next coding thread

```text
1. docs/status.md
2. docs/handoff.md
3. docs/reviews/2026-09-15-v0.4.6-independent-rereview-2.md
4. docs/milestones.md
5. docs/reviews/2026-09-14-v0.4.6-independent-rereview.md
```

Then inspect current remote `main` before changing anything.

## Accepted/closed scope

### B6.1 — PASS

The same-line semantic-authority collision is closed for the reviewed scope.

The final C# business-predicate authority stage re-resolves the exact invocation using its syntax `SpanStart` in the actual project semantic model and requires an exact supported LINQ symbol. A custom same-named call cannot borrow authority from another invocation on the same line.

Focused regression:

```text
Same_line_custom_Where_cannot_borrow_real_Linq_Where_semantic_authority
```

Do not change B6.1 unless a new concrete counterexample is found.

### B6.2 — PASS

Configured-item ownership remains conservative for the reviewed forms. Nested property object initializers are not promoted as direct items of the source collection.

Do not change B6.2 unless a new concrete counterexample is found.

## Open blocker B6.3 — Angular ownership proof is still trivia-sensitive

The ordinary same-class-name/different-module case was improved by module-qualified identity, but service import ownership is currently derived using a raw-text regular expression over the entire TypeScript file.

That allows inactive commented text to become authoritative ownership evidence.

Reproducer to add as a focused regression before the fix:

```ts
import { Component, inject } from '@angular/core';
import { CardsApi } from './catalog/cards-api';

// Not active TypeScript; historical note only:
// import { CardsApi } from './admin/cards-api';

@Component({
  selector: 'app-catalog',
  template: `
    @for (card of cards; track card.id) {
      <article>{{ card.name }}</article>
    }
  `
})
export class CatalogComponent {
  private readonly api = inject(CardsApi);
  cards = [];

  reload() {
    this.api.getCards().subscribe(cards => this.cards = cards);
  }
}
```

Use two modules:

```text
catalog/cards-api.ts → class CardsApi → GET /api/cards
admin/cards-api.ts   → class CardsApi → GET /api/admin/cards
```

Current risk:

```text
real import mapping                  catalog/cards-api.ts#CardsApi
later commented import-like mapping  admin/cards-api.ts#CardsApi
raw regex dictionary overwrite       admin/cards-api.ts#CardsApi
cross-stack equality                 admin endpoint falsely feeds CatalogComponent list
```

Required generic property:

```text
only active TypeScript import declarations may establish service ownership
comments / strings / templates have zero import authority
ambiguous or unresolved ownership must omit the authoritative correlation
```

AST-backed import declarations are an acceptable implementation direction. Do not hardcode fixture paths/classes/routes.

## Open blocker B6.4 — return containment still overclaims observable semantics

The current filter correctly downgrades predicates that are not syntactically within return/yield-return expressions, but it treats any supported invocation contained anywhere inside the returned expression as observable.

That does not preserve polarity, result type or value flow.

### Regression A — negated Any

```csharp
public bool HasNoBlockedCards()
    => !_cards.Any(card => card.Blocked);
```

Current false wording can be:

```text
Returns whether at least one item from `_cards` satisfies `card.Blocked`.
```

Actual behavior is the opposite.

### Regression B — selection converted to boolean

```csharp
public bool HasNoPublishedCard()
    => _cards.FirstOrDefault(card => card.IsPublished) is null;
```

Current false wording can describe returning a selected item even though the method returns `bool`.

Cover equivalent `SingleOrDefault` shape too.

### Regression C — helper discards returned predicate value

```csharp
public IReadOnlyList<Card> GetCards()
    => ReturnAll(_cards.Where(card => card.IsPublished).ToArray());

private IReadOnlyList<Card> ReturnAll(IReadOnlyList<Card> ignored)
    => _cards;
```

Current syntax containment can promote the `Where` into:

```text
Includes items from `_cards` only when `card.IsPublished`.
```

but the helper returns the unfiltered collection.

Required generic property:

```text
supported predicate invocation
+ deterministic proof that enclosing syntax/data flow preserves the relevant value and polarity to the observable result
→ authoritative PO rule

otherwise
→ observed-only / lower authority / omit the product-level rule
```

At minimum test:

```text
!Any(...)
!All(...)
FirstOrDefault(...) is null
SingleOrDefault(...) is not null
arbitrary helper wrapping that discards/transforms the predicate result
positive direct Any/All returns
positive returned Where pipeline
throwing/rejecting guards
```

Do not solve only the literal examples. The rule must be generic and conservative.

## Exact reviewed automation evidence

All gates for implementation checkpoint `34c3bf00233ac4c0c4df717f7f7c5ae55fffe07b` were inspected and are green:

```text
CI + full PKC tests + WorkPlay + PokeTrade   34841796973 — PASS
pinned Loren                                34841796981 — PASS
Loren-main canary                           34841796980 — PASS
pinned Jellyfin                             34841797032 — PASS
Jellyfin portable parity/provenance         PASS
```

Exact CI counts:

```text
build:          0 warnings / 0 errors
C# tests:       65 / 65 PASS
frontend tests: 12 / 12 PASS
```

PokeTrade live behavior and generated knowledge assertions pass.

Pinned Jellyfin:

```text
jellyfin/jellyfin @ 1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
source build:                         PASS
workflow candidates:                  386
product features:                     116
facts:                                43,363
canonical Markdown files:             504
bundle verbatim parity:               PASS
ZIP exact file-set parity:            PASS
ZIP byte parity:                      PASS
raw .pkc leak:                        none
src/ source-tree leak:                none
artifact id:                          10346636986
artifact digest:                      sha256:5a643ec5d2ed1f84af2a70d2b0cd108a2ca13500b6c064f8a615d4481f49b987
```

Green automation does not close B6.3/B6.4 because both are semantic false-authority paths not represented by the current regressions.

## Benchmark-special-case check

No PokeTrade, Mewtwo, Loren or Jellyfin-specific production exception was found in the production paths reviewed for these blockers. Benchmark names remain in tests/samples/acceptance material, where they are expected.

## Exact next coding action

Stay in V0.4.6. Fix **only B6.3 and B6.4** regression-first.

Preferred work unit:

```text
inspect current main
→ add focused B6.3/B6.4 adversarial regressions locally
→ verify they fail for the intended authority reasons
→ implement generic fixes locally
→ run focused tests
→ run full relevant tests/build locally
→ review complete diff
→ one coherent implementation commit/push when possible
→ let CI run once as final verification
```

After the implementation checkpoint is green across:

```text
full PKC tests
PokeTrade known-answer/live acceptance
pinned Loren
Loren-main canary
pinned Jellyfin
portable parity/no-leak
```

request another independent V0.4.6 re-review.

Do not reopen B6.1/B6.2 without new evidence.
Do not start V0.4.7.
Do not start Azure DevOps ingestion.

## Copy/paste bootstrap for the next coding thread

```text
Continue PKC from current remote main HEAD.

Read in order:
1. docs/status.md
2. docs/handoff.md
3. docs/reviews/2026-09-15-v0.4.6-independent-rereview-2.md
4. docs/milestones.md

Stay in V0.4.6.
B6.1 PASS; keep closed unless new contradiction appears.
B6.2 PASS; keep closed unless new contradiction appears.

Fix only B6.3 and B6.4 regression-first:
- B6.3: commented/string import-like TypeScript text must never establish Angular service ownership.
- B6.4: return-expression containment alone is not observable-effect proof; preserve polarity/result semantics for negated Any/All, First*/Single* transformations and arbitrary helper wrapping.

Batch the related work locally and prefer one coherent implementation commit/push after local validation.
Rerun all current gates, then request independent V0.4.6 re-review.
Do not start V0.4.7 or Azure DevOps.
```
