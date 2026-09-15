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

V0.4.6 is **not complete yet**. The only blocker from independent re-review 4 has been fixed generically and every current automated gate is green on the exact production checkpoint below. A fresh independent review is still required before V0.4.6 may be marked complete.

Production checkpoint for independent review:

```text
7f652c717c17f40f99a08b126b889a27a84c6376
fix: require complete predicate dependency proof
```

Latest completed independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-4.md
verdict: FAIL / FIX REQUIRED
```

Next review request:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-5-request.md
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

## V0.4.6 disposition

### B6.1 — PASS / keep closed

Exact C# predicate authority is re-resolved from the exact invocation syntax `SpanStart` in the target project semantic model. Same-line custom and genuine LINQ invocations cannot share authority merely because they occupy the same source line/range.

Regression:

```text
Same_line_custom_Where_cannot_borrow_real_Linq_Where_semantic_authority
```

### B6.2 — PASS / keep closed

Configured-item ownership remains conservative for reviewed forms. Nested property object initializers are not promoted as direct collection items.

### B6.3 — PASS / keep closed

Module-qualified Angular service ownership requires lexically active relative import evidence. Inactive comments, strings and template literals have zero import authority; conflicting active local-name imports are ambiguous; unresolved ownership is omitted rather than guessed.

Regression:

```text
Inactive_import_like_text_does_not_override_active_service_module
```

### B6.4 — IMPLEMENTATION GREEN / independent acceptance pending

Independent re-review 4 found that the same-type `Select(CloneCard)` authority proof collected direct predicate members but did not prove that those members were the complete semantic dependency set. A predicate such as:

```csharp
.Where(card => card.IsPublished && IsAllowed(card))
.Select(CloneCard)
```

could therefore ignore the whole-item helper dependency `IsAllowed(card)` and preserve a false full-predicate inclusion claim.

Checkpoint `7f652c717c17f40f99a08b126b889a27a84c6376` fixes that boundary conservatively.

The projector path now requires:

```text
all semantic references to the predicate source parameter
must be supported direct field/property reads
AND every required stored member must be directly preserved by the projection
→ same-type projection may retain authoritative Where semantics

otherwise
→ downgrade / no authoritative product-level inclusion rule
```

Unsupported whole-item or unmodeled uses fail closed, including shapes such as:

```text
helper(card)
card.SomeMethod()
ReferenceEquals(card, ...)
custom/operator or other whole-item semantics involving card
```

Focused regression added:

```text
Whole_item_helper_dependency_downgrades_same_type_projection_authority
```

The existing positive same-type direct-member clone regression remains green.

## Exact automation for production checkpoint

All push-triggered gates for `7f652c717c17f40f99a08b126b889a27a84c6376` are green:

```text
CI + PKC tests + WorkPlay + PokeTrade   34933533049 — PASS
pinned Loren                            34933533044 — PASS
Loren-main canary                       34933533104 — PASS
pinned Jellyfin                         34933533050 — PASS
portable parity / provenance / no-leak  PASS inside Jellyfin run
```

Exact CI evidence:

```text
PKC build:                         0 warnings / 0 errors
C# tests:                          76 / 76 PASS
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

Jellyfin artifact:

```text
artifact id:     10382099834
artifact digest: sha256:f46bf8b748bb1e044b61c74f73139b23a12d1b2b0e37fcaf17d520dd89a43113
```

## Validation note

The current coding sandbox did not have a local `dotnet` executable, so it could not truthfully execute the requested local red/green loop. The focused regression and generic fix were reviewed before one implementation push, then validated on the exact pushed SHA by the full repository gates above. Do not reinterpret this as local execution evidence.

## Exact next action

Do not change production code unless fresh review finds a concrete contradiction.

Request an independent V0.4.6 re-review of exact production checkpoint:

```text
7f652c717c17f40f99a08b126b889a27a84c6376
```

If and only if that independent review returns PASS:

```text
mark V0.4.6 COMPLETE
unlock V0.4.7 as next/current milestone
keep V0.5 Azure DevOps locked
```

Until then:

```text
V0.4.7 LOCKED
V0.5 LOCKED
```
