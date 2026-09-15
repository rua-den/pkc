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

Production checkpoint awaiting independent review:

```text
eb0903ef93b2b85669ded0e2227ca1a950bc49c7
fix: fail closed on callback-bearing where pipelines
```

Latest completed independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-6.md
verdict: FAIL / FIX REQUIRED
```

Next review request:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-7-request.md
review production: eb0903ef93b2b85669ded0e2227ca1a950bc49c7
```

V0.4.3 remains the last accepted tool package:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

## Product contract governing V0.4.x

PKC must generate portable knowledge rich enough for an AI to answer practical Product Owner questions about observable behavior, business conditions, value origin, mutation causality and cross-layer outcomes without re-reading source code.

Permanent guardrail: `docs/product-knowledge-contract.md`.

Keep distinct and retain:

```text
business conditions
value lineage / provenance
mutation / causality
```

No deterministic proof means no authoritative product claim. Downgrading rule authority must not erase deterministic lower-authority causal/value-origin evidence.

## V0.4.6 disposition

```text
B6.1 PASS — exact C# invocation semantic identity; keep closed
B6.2 PASS — conservative configured-item ownership; keep closed
B6.3 PASS — module-qualified Angular service ownership; keep closed
B6.4 IMPLEMENTATION GREEN — callback/comparer-bearing Where pipeline preservation now fails closed unless the operation is in the audited callback-free safe subset
```

### B6.4 implementation checkpoint

Independent rereview 6 demonstrated that `WherePipelineTargets` incorrectly preserved authoritative predicate semantics across operations that execute user-controlled callbacks/comparers after `Where`, including a mutating `OrderBy` key selector.

Production checkpoint `eb0903ef...` replaces the unconditional pipeline allowlist with an exact-shape callback-free safe subset:

```text
zero-argument safe targets:
  Reverse
  AsEnumerable
  AsQueryable
  ToArray
  ToList

slice safe targets with exact supported scalar/range argument shape:
  Skip
  Take
```

The following no longer preserve authoritative `Where` semantics merely because their resolved LINQ target is known:

```text
OrderBy / OrderByDescending
ThenBy / ThenByDescending
Distinct
ToHashSet
```

This applies generically to Enumerable/Queryable variants where applicable and intentionally fails closed for callback/comparer/default-equality paths whose item-effect safety is not deterministically proven.

Focused regression coverage includes:

```text
Side_effecting_ordering_callback_downgrades_returned_filter_authority
Equality_comparer_pipeline_downgrades_returned_filter_authority
Default_item_equality_pipeline_downgrades_returned_filter_authority
Callback_free_enumerable_and_queryable_pipeline_remains_authoritative
```

Existing identity `Select(card => card)` and accepted same-type direct-member defensive clone proof remain unchanged. The prior custom-setter fix is not reopened.

The sandbox used for this coding pass did not provide `dotnet`, so the new regression could not be executed locally before the production change. The defect was confirmed by direct inspection of the pre-fix authority path: any resolved target in the old `WherePipelineTargets` returned `true` without inspecting callback/comparer effects. Runtime verification was then performed by the exact-SHA CI gates below.

## Exact automation for production checkpoint

All required gates for exact SHA `eb0903ef93b2b85669ded0e2227ca1a950bc49c7` are green:

```text
CI + PKC tests + WorkPlay + PokeTrade   34959028651 — PASS
pinned Loren                            34959028686 — PASS
Loren-main canary                       34959028633 — PASS
pinned Jellyfin                         34959028626 — PASS
```

Core CI:

```text
PKC build:       0 warnings / 0 errors
C# tests:        81 / 81 PASS
frontend tests:  13 / 13 PASS
WorkPlay:        PASS
PokeTrade:       PASS
```

PokeTrade validation:

```text
.NET 10 backend build:      PASS, 0 warnings / 0 errors
Angular 22 build:           PASS
live business acceptance:   PASS
known-answer knowledge:     PASS
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
bundle canonical parity: PASS
ZIP file-set parity:     PASS
ZIP byte parity:         PASS
raw .pkc leak:           none
src/ source-tree leak:   none
artifact id:             10391499835
artifact digest:         sha256:39aab5f88914a2e153646b75fa9c8b6cece8a03f3f2dbc821fae871dab820dc2
artifact size:           9,016,935 bytes
uploaded files:          509
```

Green CI is final verification evidence, not independent semantic approval.

## Exact next action

Stay in V0.4.6.

Have an independent reviewer inspect exact production SHA `eb0903ef93b2b85669ded0e2227ca1a950bc49c7` against rereview 6 and the new rereview 7 request. Do not make further production changes unless the independent review identifies a concrete contradiction.

Until independent PASS:

```text
V0.4.6 IMPLEMENTATION GREEN / INDEPENDENT RE-REVIEW PENDING
V0.4.7 LOCKED
V0.5 LOCKED
```
