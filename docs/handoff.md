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

Permanent product-knowledge guardrail: `docs/product-knowledge-contract.md`.

Keep distinct and retain:

```text
business conditions
value lineage / provenance
mutation / causality
```

Conservative downgrade of product-rule authority must not erase deterministic lower-authority evidence that can explain where a value came from or what code path changed it.

## Current state

```text
V0.4.4  Loren knowledge readiness              PASS / COMPLETE
V0.4.5  Jellyfin generalization               PASS / COMPLETE
V0.4.6  business logic reconstruction         IMPLEMENTATION GREEN / INDEPENDENT RE-REVIEW PENDING
V0.4.7  cross-layer PO-question readiness     LOCKED
V0.5    Azure DevOps input evidence           LOCKED
```

Production checkpoint awaiting review:

```text
eb0903ef93b2b85669ded0e2227ca1a950bc49c7
fix: fail closed on callback-bearing where pipelines
```

Latest completed independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-6.md
verdict: FAIL / FIX REQUIRED
```

Fresh review request:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-7-request.md
review exact production SHA eb0903ef93b2b85669ded0e2227ca1a950bc49c7
```

Current disposition:

```text
B6.1 PASS / keep closed
B6.2 PASS / keep closed
B6.3 PASS / keep closed
B6.4 IMPLEMENTATION GREEN / independent review required
```

Do not start V0.4.7 or Azure DevOps until V0.4.6 independently passes.

## Closed scope

### B6.1 — PASS

Exact C# predicate authority is tied to exact invocation syntax/semantic identity. Same-line custom and genuine LINQ calls cannot borrow authority.

### B6.2 — PASS

Configured-item ownership remains conservative; nested property object initializers are not promoted as direct collection items.

### B6.3 — PASS

Angular service correlation requires lexically active module-qualified relative import evidence. Ambiguous/unresolved ownership is omitted rather than guessed.

## B6.4 — new implementation checkpoint

Rereview 6 proved that the old `WherePipelineTargets` allowlist was too broad: delegate/comparer-bearing operations could mutate predicate-relevant state after `Where`, while authority was retained solely from the semantic method target.

The compile-valid blocker was:

```csharp
_cards
    .Where(card => card.IsPublished)
    .OrderBy(card => card.IsPublished = false)
    .ToArray();
```

Runtime returns the same item after the key selector changes `IsPublished` to `false`, so the claim `Includes items from _cards only when card.IsPublished` is false for returned state.

Production `eb0903ef...` implements a generic fail-closed boundary instead of special-casing `OrderBy`:

```text
callback-free zero-argument safe subset:
  Reverse
  AsEnumerable
  AsQueryable
  ToArray
  ToList

callback-free exact slice shapes:
  Skip(int / supported Range shape)
  Take(int / supported Range shape)

not automatically preserving:
  OrderBy / OrderByDescending
  ThenBy / ThenByDescending
  Distinct
  ToHashSet
```

The authority filter now proves both the exact LINQ target and the supported argument shape for the safe subset. Callback/comparer/default-equality paths fail closed unless a future deterministic proof is added.

This change does not reopen the accepted `Select`/same-type-projector/custom-setter work. Identity `Select(card => card)` and the previously accepted defensive-clone proof remain in place.

Focused new regressions:

```text
Side_effecting_ordering_callback_downgrades_returned_filter_authority
Equality_comparer_pipeline_downgrades_returned_filter_authority
Default_item_equality_pipeline_downgrades_returned_filter_authority
Callback_free_enumerable_and_queryable_pipeline_remains_authoritative
```

All prior negative regressions remain covered by the full C# suite.

## Local execution limitation

The available coding sandbox did not contain `dotnet`, so the focused regression could not be run locally before changing production code. The defect was nevertheless confirmed directly against the pre-fix implementation: `IsAllowedWherePipelineInvocation()` returned `true` immediately for any resolved method target in the old `WherePipelineTargets`, without checking selector/comparer effects.

Do not claim local runtime validation for this checkpoint. Exact-SHA GitHub Actions below is the runtime evidence.

## Exact production gates

Exact production SHA:

```text
eb0903ef93b2b85669ded0e2227ca1a950bc49c7
```

All required automation is green:

```text
CI + full PKC tests + WorkPlay + PokeTrade   34959028651 — PASS
pinned Loren                                34959028686 — PASS
Loren-main canary                           34959028633 — PASS
pinned Jellyfin                             34959028626 — PASS
```

Core evidence:

```text
PKC build:                 0 warnings / 0 errors
C# tests:                  81 / 81 PASS
frontend tests:            13 / 13 PASS
WorkPlay build:            PASS
PokeTrade backend build:   PASS, 0 warnings / 0 errors
PokeTrade Angular build:   PASS
PokeTrade live acceptance: PASS
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
canonical Markdown:      504
analysis mode:           project-semantic 43,363 / 43,363
bundle parity:           PASS
ZIP file-set parity:     PASS
ZIP byte parity:         PASS
raw .pkc leak:           none
src/ source-tree leak:   none
artifact id:             10391499835
artifact digest:         sha256:39aab5f88914a2e153646b75fa9c8b6cece8a03f3f2dbc821fae871dab820dc2
artifact size:           9,016,935 bytes
uploaded files:          509
```

## Next action — independent review only

Review exact production SHA `eb0903ef93b2b85669ded0e2227ca1a950bc49c7` using:

1. `docs/status.md`
2. `docs/handoff.md`
3. `docs/milestones.md`
4. `docs/product-knowledge-contract.md`
5. `docs/reviews/2026-09-15-v0.4.6-independent-rereview-6.md`
6. `docs/reviews/2026-09-15-v0.4.6-independent-rereview-7-request.md`

Preserve reviewer/coder separation. No production edits in the review thread.

If the independent review passes, then and only then mark V0.4.6 complete and unlock V0.4.7 as next/current. Keep V0.5 locked.

Until independent PASS:

```text
V0.4.6 IMPLEMENTATION GREEN / INDEPENDENT RE-REVIEW PENDING
V0.4.7 LOCKED
V0.5 LOCKED
```
