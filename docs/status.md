# PKC Status

Last updated: 2026-09-15

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                 PASS / COMPLETE
V0.4.5 real-repository generalization            PASS / COMPLETE
V0.4.6 business logic reconstruction             FAIL / FIX REQUIRED
V0.4.7 cross-layer PO-question readiness         LOCKED
V0.5 Azure DevOps input evidence                 LOCKED
```

Latest independent V0.4.6 review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-2.md
```

Reviewed implementation checkpoint:

```text
34c3bf00233ac4c0c4df717f7f7c5ae55fffe07b
fix: preserve observable predicate return semantics
```

Independent disposition:

```text
B6.1  PASS
B6.2  PASS
B6.3  BLOCK
B6.4  BLOCK
```

V0.4.6 is not complete. Green automation is valid supporting evidence, but independent source inspection found concrete false-authority paths in B6.3 and B6.4.

V0.4.7 must remain locked until another independent V0.4.6 review returns PASS. V0.5 Azure DevOps remains locked.

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

## V0.4.6 blocker status

### B6.1 — PASS

The prior same-line collision is closed for the reviewed scope.

Current final authority logic re-resolves the exact predicate invocation from its syntax `SpanStart` in the real project semantic model and only accepts an exact supported symbol:

```text
System.Linq.Enumerable.<operation>
System.Linq.Queryable.<operation>
```

A custom same-named invocation on the same source line can no longer borrow LINQ authority from a different invocation.

Focused regression remains:

```text
Same_line_custom_Where_cannot_borrow_real_Linq_Where_semantic_authority
```

Do not reopen B6.1 without a new concrete contradiction.

### B6.2 — PASS

Configured-item extraction remains conservative for the reviewed forms. Nested property object initializers are not promoted as additional direct collection items.

Do not reopen B6.2 without a new concrete contradiction.

### B6.3 — BLOCK / FIX REQUIRED

The module-qualified identity format is correct in principle, but import ownership is currently derived from a raw-text regular expression over the full TypeScript file.

Because the import matcher is not comment/string aware, inactive text can become authoritative ownership evidence.

Concrete valid TypeScript counterexample:

```ts
import { CardsApi } from './catalog/cards-api';

// Historical note only — not an active import:
// import { CardsApi } from './admin/cards-api';
```

With both modules exporting the same `CardsApi.getCards()`, the later commented text can overwrite the active import mapping. A Catalog component can then be falsely correlated with `/api/admin/cards` and its rendered list.

Required property:

```text
only syntactically active imports establish service ownership
comment/string/template import-like text has zero authority
ambiguous or unresolved ownership → omit authoritative cross-stack correlation
```

Add the adversarial regression before implementing the generic fix.

### B6.4 — BLOCK / FIX REQUIRED

The implementation correctly downgrades predicates that are not in return/yield-return syntax, but current observable authority is still based on syntactic containment anywhere inside a return expression.

That is insufficient to prove value semantics or polarity.

Concrete polarity counterexample:

```csharp
public bool HasNoBlockedCards()
    => !_cards.Any(card => card.Blocked);
```

Current authority can mark `Any(...)` observable and synthesis can say:

```text
Returns whether at least one item from `_cards` satisfies `card.Blocked`.
```

Actual behavior is the opposite.

Concrete selection-context counterexample:

```csharp
public bool HasNoPublishedCard()
    => _cards.FirstOrDefault(card => card.IsPublished) is null;
```

Current synthesis can describe returning a selected item even though the method returns a boolean.

Concrete observable-effect counterexample:

```csharp
public IReadOnlyList<Card> GetCards()
    => ReturnAll(_cards.Where(card => card.IsPublished).ToArray());

private IReadOnlyList<Card> ReturnAll(IReadOnlyList<Card> ignored)
    => _cards;
```

The `Where(...)` is syntactically inside the returned expression but the helper discards it. PKC must not claim filtered endpoint output unless the value-preserving path is deterministically proven.

Required property:

```text
predicate invocation
+ proven value/polarity-preserving path to observable result
→ authoritative Product Owner rule

otherwise
→ observed-only / lower authority / omit product-level claim
```

Regression coverage must challenge negated `Any`/`All`, transformed `First*`/`Single*`, arbitrary helper wrapping, positive direct returns and throwing/rejecting guards.

## Exact implementation-checkpoint automation

The following gates for `34c3bf00233ac4c0c4df717f7f7c5ae55fffe07b` are verified green:

```text
CI + PKC tests + WorkPlay + PokeTrade   34841796973 — PASS
pinned Loren                            34841796981 — PASS
Loren-main canary                       34841796980 — PASS
pinned Jellyfin                         34841797032 — PASS
portable parity / no-leak               PASS inside Jellyfin run
```

Exact CI evidence:

```text
build:          0 warnings / 0 errors
C# tests:       65 / 65 PASS
frontend tests: 12 / 12 PASS
tool pack:      PASS
WorkPlay:       PASS
PokeTrade:      PASS
```

Jellyfin portable verification:

```text
canonical Markdown files: 504
bundle contains canonical files verbatim: PASS
ZIP file set equals canonical pack: PASS
ZIP byte parity: PASS
raw .pkc leak: none
src/ source-tree leak: none
facts: 43,363
analysis mode: project-semantic 43,363 / 43,363
```

Artifact:

```text
id:     10346636986
digest: sha256:5a643ec5d2ed1f84af2a70d2b0cd108a2ca13500b6c064f8a615d4481f49b987
```

## Benchmark-special-case check

No PokeTrade, Mewtwo, Loren or Jellyfin-specific exception was found in the production analyzer/enricher/synthesizer paths reviewed for B6.1-B6.4. Benchmark names remain appropriate in tests, samples and acceptance material.

## Exact next action

Stay in V0.4.6 and fix only the two independently reproduced authority blockers:

```text
B6.3 — syntax-aware Angular import/service ownership
B6.4 — value/polarity-preserving observable predicate authority
```

Coder workflow:

```text
focused regression reproducing the concrete blocker
→ verify red locally
→ generic fix
→ focused green
→ relevant/full local validation
→ review complete diff
→ one coherent implementation commit/push when possible
→ rerun PKC + PokeTrade + pinned Loren + Loren-main + pinned Jellyfin + portable parity
→ independent V0.4.6 re-review
```

Do not modify B6.1 or B6.2 without new contradictory evidence.
Do not start V0.4.7.
Do not start Azure DevOps ingestion.
