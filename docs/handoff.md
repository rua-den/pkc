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
V0.4.6  business logic reconstruction         IMPLEMENTATION GREEN / INDEPENDENT RE-REVIEW PENDING
V0.4.7  cross-layer PO-question readiness     LOCKED
V0.5    Azure DevOps input evidence           LOCKED
```

Production checkpoint to review:

```text
a636172ea8575f46d51e503d1ba7ad6d861650fb
fix: preserve proven same-type Select projections
```

Latest completed independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-3.md
```

Fresh review request:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-4-request.md
```

Current disposition:

```text
B6.1  PASS / keep closed
B6.2  PASS / keep closed
B6.3  PASS / keep closed
B6.4  IMPLEMENTED + GREEN / fresh independent acceptance pending
```

Do not mark V0.4.6 complete yet. Do not start V0.4.7 or Azure DevOps until a fresh independent V0.4.6 review returns PASS.

## Read first in the next review thread

```text
1. docs/status.md
2. docs/handoff.md
3. docs/milestones.md
4. docs/reviews/2026-09-15-v0.4.6-independent-rereview-4-request.md
5. docs/reviews/2026-09-15-v0.4.6-independent-rereview-3.md
```

Then inspect current remote `main`. The docs-only HEAD may be newer than the production checkpoint; review production behavior at `a636172ea8575f46d51e503d1ba7ad6d861650fb`.

## Accepted/closed scope

### B6.1 — PASS

The same-line semantic-authority collision is closed. Final C# predicate authority re-resolves the exact invocation by syntax `SpanStart` in the target project semantic model and accepts only the exact supported LINQ symbol.

Regression:

```text
Same_line_custom_Where_cannot_borrow_real_Linq_Where_semantic_authority
```

Do not reopen without a new concrete contradiction.

### B6.2 — PASS

Configured-item ownership remains conservative for reviewed supported initializer forms. Nested property objects are not promoted as direct collection items.

Do not reopen without a new concrete contradiction.

### B6.3 — PASS

Module-qualified Angular service ownership requires lexically active relative import evidence. Comment/string/template import-like text has no authority. Conflicting active local-name imports are ambiguous, and unresolved/non-relative ownership is omitted rather than guessed.

Regression:

```text
Inactive_import_like_text_does_not_override_active_service_module
```

Independent re-review 3 accepted B6.3. Do not reopen without a new concrete contradiction.

## B6.4 implementation checkpoint

Independent re-review 3 found one remaining false-authority path: arbitrary LINQ `Select` was treated as preserving a preceding `Where` predicate's observable item semantics.

Counterexample from the review:

```csharp
_cards
    .Where(card => card.IsPublished)
    .Select(_ => _cards[0])
    .ToArray();
```

The implementation now removes `Select` from the unconditional `Where` receiver-pipeline allowlist.

A `Select` is accepted only when item-semantics preservation is deterministically proven for supported forms.

### Supported proof 1 — direct identity lambda

```csharp
.Select(card => card)
```

The returned identifier must resolve by Roslyn symbol equality to the selector source parameter.

### Supported proof 2 — conservative same-type method-group copy

This exists because PokeTrade's genuine known-answer path uses:

```csharp
.Where(...)
.Select(CloneCard)
.ToArray();
```

PKC does **not** allow arbitrary method groups. Authority is kept only when all relevant proof succeeds:

```text
selector resolves uniquely to a source method
projector has exactly one source parameter
parameter type == return type
item type is sealed or a struct
projector source declaration is available
Where directly reads known stored fields/auto-properties from its source parameter
projector directly returns a same-type object initializer
for every member used by the Where predicate:
    returned.Member = input.Member
is proven by symbol equality
```

If a predicate member is rewritten, defaulted, transformed, routed through an arbitrary helper, or cannot be proven, the `Where` predicate is downgraded to observed-only.

Focused coverage:

```text
Non_identity_projection_does_not_preserve_returned_filter_authority
Returned_filter_through_supported_projection_pipeline_remains_authoritative
Predicate_preserving_same_type_method_group_projection_remains_authoritative
Same_type_method_group_that_changes_predicate_member_is_not_authoritative
```

The original B6.4 regressions for negated `Any`/`All`, transformed `First*`/`Single*`, arbitrary helper wrapping, local side-path predicates and throwing guards remain green.

## Implementation history and CI evidence

### First checkpoint

```text
ea9f423bdd6ab37658b632cdfa3fcd7c6f0c0d9f
fix: prove identity-preserving Select authority
```

This checkpoint correctly rejected the independent-review counterexample. C# tests were green at 73/73. Final CI then found one genuine integration regression:

```text
PokeTradePoQuestionReadinessTests.Mewtwo_catalog_visibility_is_answerable_from_compiled_knowledge
```

Source inspection showed PokeTrade uses `Select(CloneCard)` after its eligibility `Where`, and `CloneCard` copies the predicate-relevant auto-properties directly into a same-type sealed object. That is a valid supported semantic-preservation case, not a reason to restore arbitrary `Select` authority.

### Final implementation checkpoint

```text
a636172ea8575f46d51e503d1ba7ad6d861650fb
fix: preserve proven same-type Select projections
```

The final fix proves that narrow same-type member-copy shape and keeps non-identity projection rejected.

This required a second implementation push because the first final-validation CI exposed a concrete integration defect. It was investigated from exact source before the second change; there was no speculative push loop.

## Exact final automation evidence

All final gates for `a636172ea8575f46d51e503d1ba7ad6d861650fb` are green:

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
artifact id:     10381642948
artifact digest: sha256:1e4df6f7c0b77ce7a72488462c209b4b5a783647f88b1fb7c9c737e8a7ba507f
```

## Environment note

The implementation session's sandbox could not resolve GitHub for a local clone and did not contain the .NET SDK, so it could not honestly execute local red/green tests. The source diff was reviewed before each publish. Exact-SHA CI was used as final verification; the first run exposed a real integration regression, which was source-inspected and fixed generically before the final checkpoint.

## Exact next action

Independent review only.

Review production behavior at:

```text
a636172ea8575f46d51e503d1ba7ad6d861650fb
```

Challenge B6.4 adversarially, especially:

```text
arbitrary/non-identity Select must not preserve Where authority
identity Select may preserve it only with exact proof
same-type method-group projection must preserve every predicate-relevant stored member
member rewrite/default/transform/helper indirection must downgrade authority
no benchmark-specific production exception may exist
```

Keep B6.1/B6.2/B6.3 closed unless a new concrete contradiction appears.

If independent review returns PASS:

```text
mark V0.4.6 COMPLETE
unlock V0.4.7 as next/current milestone
keep V0.5 Azure DevOps locked
```

If review finds a concrete blocker:

```text
focused regression
→ generic minimum fix
→ focused/related/full validation
→ one coherent implementation checkpoint
→ final gates
→ independent re-review
```

Do not write more production code before such a blocker exists.
