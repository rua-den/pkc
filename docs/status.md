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

V0.4.6 is **not complete**. The implementation checkpoint is green across all current automated gates, but independent adversarial review found one remaining false-product-claim path in B6.4.

Reviewed implementation checkpoint:

```text
9a0817b21075a3d810072a310ed8fd3314625cd8
fix: enforce observable authority boundaries
```

Latest independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-3.md
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

Focused regression remains:

```text
Same_line_custom_Where_cannot_borrow_real_Linq_Where_semantic_authority
```

Do not reopen B6.1 without a new concrete contradiction.

### B6.2 — PASS / keep closed

Configured-item ownership remains conservative for the reviewed forms. Nested property object initializers are not promoted as direct collection items.

Do not reopen B6.2 without a new concrete contradiction.

### B6.3 — PASS / independently accepted

Module-qualified Angular service ownership now requires lexically active relative import evidence. Inactive line comments, block comments, strings and template literals have zero import authority; conflicting active local-name imports are treated as ambiguous; unresolved ownership is omitted rather than guessed.

Focused regression:

```text
Inactive_import_like_text_does_not_override_active_service_module
```

Independent re-review found no remaining false ownership-authority path in V0.4.6 scope. Conservative false negatives remain acceptable at this milestone.

### B6.4 — BLOCK / FIX REQUIRED

The previous polarity/return-containment defects are fixed, but the current `Where` return-pipeline allowlist treats arbitrary LINQ `Select` as preserving the filtered output-item semantics.

That is unsound.

Counterexample:

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

Runtime returns the unpublished `_cards[0]`, but the current authority path can still promote the `Where` predicate and synthesize:

```text
Includes items from `_cards` only when `card.IsPublished`.
```

That claim is false.

Required authority boundary:

```text
Where predicate
+ every outer operation proven to preserve the predicate's output-item semantics
→ authoritative inclusion rule

otherwise
→ observed-only / lower authority / no product-level inclusion claim
```

The minimum safe V0.4.6 direction is to stop treating arbitrary `Select` as preserving inclusion semantics unless its selector is deterministically proven identity-preserving.

Required focused regression before fixing:

```text
Where(...).Select(nonIdentity).ToArray()
```

The existing identity projection positive regression may remain authoritative only if identity preservation is explicitly proven.

## Exact implementation-checkpoint automation

All push-triggered gates for implementation checkpoint `9a0817b21075a3d810072a310ed8fd3314625cd8` are green:

```text
CI + PKC tests + WorkPlay + PokeTrade   34920522723 — PASS
pinned Loren                            34920522961 — PASS
Loren-main canary                       34920522799 — PASS
pinned Jellyfin                         34920522831 — PASS
portable parity / provenance / no-leak  PASS inside Jellyfin run
```

Exact CI evidence:

```text
PKC build:       0 warnings / 0 errors
C# tests:        72 / 72 PASS
frontend tests:  13 / 13 PASS
tool pack/install: PASS
WorkPlay:          PASS
PokeTrade backend: PASS
PokeTrade Angular: PASS
PokeTrade live business branches: PASS
PokeTrade generated-knowledge assertions: PASS
```

Real-repository gates:

```text
pinned Loren build + PKC compile:        PASS
current Loren-main build + PKC compile:  PASS
pinned Jellyfin source build:            PASS, 0 warnings / 0 errors
Jellyfin PKC facts:                      43,363
Jellyfin relations:                      195,314
Jellyfin workflow candidates:            386
Jellyfin product features:               116
canonical Markdown files:                504
analysis mode:                           project-semantic 43,363 / 43,363
```

Portable Jellyfin verification:

```text
every canonical Markdown file appears verbatim in PKC_KNOWLEDGE.md: PASS
ZIP file set equals canonical Markdown file set:                       PASS
ZIP bytes equal canonical Markdown bytes:                             PASS
raw .pkc leak in portable ZIP:                                        none
src/ source-tree leak in portable ZIP:                                none
```

Current Jellyfin artifact:

```text
artifact id:     10377409728
artifact digest: sha256:74ded0b0e5271b1731599b3490d7013ad07dde74445e893868320e2e657f714d
```

Green automation does not override the semantic blocker.

## Benchmark-special-case check

The reviewed production changes are generic analyzer/authority logic. No PokeTrade, Mewtwo, Loren or Jellyfin-specific production exception was found.

## Exact next action

Stay in V0.4.6.

Fix only B6.4 regression-first:

```text
1. add a focused red regression for Where(...).Select(nonIdentity).ToArray()
2. prove the current false authoritative inclusion rule
3. implement the minimum generic item-semantics-preservation fix
4. keep identity/safe pipeline positive regressions green
5. run focused + related + full relevant tests locally
6. review the complete diff
7. commit regression + fix together as one coherent checkpoint
8. push once
9. rerun PokeTrade, pinned Loren, Loren-main, pinned Jellyfin and portable parity/no-leak
10. request another independent V0.4.6 re-review
```

Do not start V0.4.7.
Do not start Azure DevOps ingestion.
