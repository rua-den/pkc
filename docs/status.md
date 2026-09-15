# PKC Status

Last updated: 2026-09-15

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                 PASS / COMPLETE
V0.4.5 real-repository generalization            PASS / COMPLETE
V0.4.6 business logic reconstruction             IMPLEMENTATION GREEN / INDEPENDENT RE-REVIEW PENDING
V0.4.7 cross-layer PO-question readiness         LOCKED
V0.5 Azure DevOps input evidence                 LOCKED
```

V0.4.6 is **not complete yet**. B6.1, B6.2 and B6.3 are independently accepted. The remaining B6.4 `Select` item-semantics blocker from independent re-review 3 has been implemented and all current automation is green, but the implementation still requires a fresh independent adversarial PASS.

Production implementation checkpoint to review:

```text
a636172ea8575f46d51e503d1ba7ad6d861650fb
fix: preserve proven same-type Select projections
```

Latest completed independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-3.md
verdict: FAIL / FIX REQUIRED against 9a0817b21075a3d810072a310ed8fd3314625cd8
```

Fresh review request:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-4-request.md
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

### B6.4 — IMPLEMENTED + GREEN / independent acceptance pending

Independent re-review 3 found that arbitrary LINQ `Select` was incorrectly treated as preserving `Where` output-item semantics.

Concrete false-authority shape:

```csharp
_cards
    .Where(card => card.IsPublished)
    .Select(_ => _cards[0])
    .ToArray();
```

That path could return an unpublished card while PKC claimed that returned items are included only when `card.IsPublished`.

Current implementation no longer allowlists arbitrary `Select`.

A `Select` may preserve `Where` authority only when PKC proves one of these supported conservative forms:

```text
1. direct identity projection
   card => card

2. same-type method-group projection where all are proven:
   - one source parameter
   - source type == return type
   - closed item type (sealed class or struct)
   - projector source is available in the same compilation
   - the Where predicate's directly-read stored members are known
   - each such member is copied directly from input to the returned same-type object initializer
   - no transform/default/helper substitutes any predicate member
```

If proof fails, the predicate is downgraded to observed-only and no authoritative product-level inclusion rule is emitted.

Focused regression coverage now includes:

```text
Non_identity_projection_does_not_preserve_returned_filter_authority
Returned_filter_through_supported_projection_pipeline_remains_authoritative
Predicate_preserving_same_type_method_group_projection_remains_authoritative
Same_type_method_group_that_changes_predicate_member_is_not_authoritative
```

Implementation history for this blocker:

```text
ea9f423bdd6ab37658b632cdfa3fcd7c6f0c0d9f
  - rejected arbitrary Select
  - C# regressions green
  - final CI exposed a genuine PokeTrade false-negative because the benchmark uses Select(CloneCard)

 a636172ea8575f46d51e503d1ba7ad6d861650fb
  - preserved only deterministically proven same-type predicate-member copies
  - retained rejection of arbitrary/non-identity projections
  - restored PokeTrade PO-readiness
  - all current gates green
```

The second push followed investigation of a genuine integration failure; it was not speculative CI-driven debugging.

## Exact implementation-checkpoint automation

All push-triggered gates for `a636172ea8575f46d51e503d1ba7ad6d861650fb` are green:

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
artifact id:     10381642948
artifact digest: sha256:1e4df6f7c0b77ce7a72488462c209b4b5a783647f88b1fb7c9c737e8a7ba507f
```

## Benchmark-special-case check

The B6.4 production logic is generic Roslyn semantic authority logic. No PokeTrade, Mewtwo, Loren or Jellyfin-specific production exception was added.

## Exact next action

Do not write more production code unless fresh independent review identifies a concrete remaining blocker.

Next step is independent adversarial V0.4.6 re-review of:

```text
a636172ea8575f46d51e503d1ba7ad6d861650fb
```

Review B6.4 specifically while keeping B6.1/B6.2/B6.3 closed unless a new contradiction is found.

If the independent review returns PASS:

```text
mark V0.4.6 PASS / COMPLETE
unlock V0.4.7 as the next milestone
keep V0.5 Azure DevOps locked
```

Until that PASS exists:

```text
V0.4.7 LOCKED
V0.5   LOCKED
```
