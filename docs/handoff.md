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

## Current state

```text
V0.4.4  Loren knowledge readiness              PASS / COMPLETE
V0.4.5  Jellyfin generalization               PASS / COMPLETE
V0.4.6  business logic reconstruction         IMPLEMENTATION GREEN / INDEPENDENT RE-REVIEW PENDING
V0.4.7  cross-layer PO-question readiness     LOCKED
V0.5    Azure DevOps input evidence           LOCKED
```

Production checkpoint awaiting independent review:

```text
7f652c717c17f40f99a08b126b889a27a84c6376
fix: require complete predicate dependency proof
```

Latest completed independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-4.md
verdict: FAIL / FIX REQUIRED
```

Next independent review request:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-5-request.md
```

Current disposition:

```text
B6.1 PASS / keep closed
B6.2 PASS / keep closed
B6.3 PASS / keep closed
B6.4 IMPLEMENTATION GREEN / independent acceptance pending
```

Do not start V0.4.7 or Azure DevOps until V0.4.6 independently passes.

## Closed scope

### B6.1 — PASS

The final C# predicate authority stage re-resolves exact invocation identity using source path + syntax `SpanStart` in the target Roslyn project semantic model. Same-line custom and genuine LINQ calls cannot borrow one another's authority.

### B6.2 — PASS

Configured-item ownership remains conservative; nested property object initializers are not promoted as direct configured collection items.

### B6.3 — PASS

Angular service correlation requires lexically active module-qualified relative import evidence. Comment/string/template import-like text has zero authority, conflicting active ownership is ambiguous, and unresolved ownership is omitted.

## B6.4 implementation checkpoint

Independent re-review 4 found one remaining dependency-completeness defect in same-type projection authority.

Unsafe reviewed shape:

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

Before the fix, direct-member discovery could collect only `card.IsPublished`, ignore whole-item helper use `IsAllowed(card)`, prove `IsPublished` copied, and retain the full predicate as an authoritative returned-item rule.

Checkpoint `7f652c717c17f40f99a08b126b889a27a84c6376` changes the proof to fail closed.

Current generic rule:

```text
same-type Select projector preserves Where authority only when
  every semantic reference to the predicate source parameter
  is a supported direct field/property read
AND
  every required stored member is directly copied input-member → output-member

any unsupported whole-item/unmodeled use
→ no projection preservation proof
→ observed-only / no authoritative product-level inclusion rule
```

Unsupported shapes include at least:

```text
helper(card)
card.SomeMethod()
ReferenceEquals(card, ...)
whole-item custom/operator semantics
other unsupported parameter-use paths
```

Focused regression:

```text
Whole_item_helper_dependency_downgrades_same_type_projection_authority
```

Positive regression retained:

```text
Predicate_preserving_same_type_method_group_projection_remains_authoritative
```

Arbitrary non-identity projection remains conservative:

```text
Non_identity_projection_does_not_preserve_returned_filter_authority
```

## Exact checkpoint verification

All current gates are green on exact production SHA `7f652c717c17f40f99a08b126b889a27a84c6376`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   34933533049 — PASS
pinned Loren                                34933533044 — PASS
Loren-main canary                           34933533104 — PASS
pinned Jellyfin                             34933533050 — PASS
Jellyfin portable parity/provenance         PASS
```

Core evidence:

```text
PKC build:                 0 warnings / 0 errors
C# tests:                  76 / 76 PASS
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
source build:            PASS, 0 warnings / 0 errors
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
artifact id:             10382099834
artifact digest:         sha256:f46bf8b748bb1e044b61c74f73139b23a12d1b2b0e37fcaf17d520dd89a43113
```

## Validation environment note

The coding sandbox had no local `dotnet` executable, so local red/green execution was unavailable. The regression was added before the production proof change, the complete two-file implementation diff was reviewed before one fast-forward push, and final validation used the exact-head repository gates above. Do not claim local execution occurred.

## Next thread / reviewer read order

1. `docs/status.md`
2. `docs/handoff.md`
3. `docs/milestones.md`
4. `docs/reviews/2026-09-15-v0.4.6-independent-rereview-4.md`
5. `docs/reviews/2026-09-15-v0.4.6-independent-rereview-5-request.md`
6. inspect current `main`

Independent reviewer must review exact production checkpoint `7f652c717c17f40f99a08b126b889a27a84c6376`, not a later docs-only handoff SHA.

If independent review returns PASS, mark V0.4.6 complete and unlock V0.4.7. Keep V0.5 locked.

Until then, do not advance milestones and do not modify production without a new concrete blocker.
