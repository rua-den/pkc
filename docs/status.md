# PKC Status

Last updated: 2026-09-14

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                 PASS / COMPLETE
V0.4.5 real-repository generalization            PASS / COMPLETE
V0.4.6 business logic reconstruction             INDEPENDENT REVIEW FAILED / FIX REQUIRED
V0.4.7 cross-layer PO-question readiness         LOCKED
V0.5 Azure DevOps input evidence                 LOCKED
```

Independent review record:

```text
docs/reviews/2026-09-14-v0.4.5-v0.4.6-independent-review.md
```

Reviewed repository HEAD:

```text
c293fc157783ce416af9e5730b6b4de005b26b6b
```

Last V0.4.6 code checkpoint before review/handoff docs:

```text
cbfaf6fb9107fdb233045358a2c0e7b689a647ce
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

## V0.4.5 — PASS / COMPLETE

Pinned benchmark:

```text
jellyfin/jellyfin
1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
```

Records:

```text
docs/trials/2026-09-14-v0.4.5-jellyfin.md
docs/trials/2026-09-14-v0.4.5-jellyfin-crosscheck.md
```

The independent review accepted the generalization gate.

Why it passed:

- genuine second repository materially different from PokeTrade/Loren;
- blind knowledge-only answers frozen before source inspection;
- source cross-check found a real inherited MVC controller-route compiler defect;
- defect was reproduced red first and fixed generically;
- explicit derived `[Route("")]` override semantics are regression-locked;
- no Jellyfin/repository-specific production exception was observed;
- exact-head PokeTrade, pinned Loren, Loren-main and pinned Jellyfin all remain green;
- Jellyfin portable artifact retains exact structured/bundle/ZIP parity and no portable source/raw leak.

Accepted warnings remain open:

```text
W3 duplicate HTTP verb extraction
W4 feature-level rule promotion
W5 large-pack signal/noise
W1 claim-level portable provenance
W2 durable blind-review reproducibility
```

W3/W4/W5 are not blockers for V0.4.5 but remain quality targets for later V0.4.x work.

## V0.4.6 — FAIL / FIX REQUIRED

The known PokeTrade Mewtwo path works, but independent review found three generic false-claim paths.

### B6.1 — lexical `Where/Any/First...` matching can invent LINQ semantics

`CSharpBusinessPredicateEnricher` recognizes predicate operations by method name + lambda syntax without proving the resolved method is a supported LINQ operation.

A user-defined custom `Where` method can therefore be rendered as:

```text
Includes items from ... only when ...
```

even when that custom method does not filter by the lambda.

Required: semantic operation proof or conservative non-authoritative fallback.

### B6.2 — nested initializer objects can be mislabeled as source collection items

Configured-object extraction walks every descendant object creation under a predicate-source field initializer, then synthesis renders each as:

```text
Configured item in `<source>`: ...
```

Nested metadata/configuration objects are not necessarily items of the collection.

Required: prove direct collection-item/value ownership before emitting configured-item evidence.

### B6.3 — Angular list/result flow cross-links by API method name alone

`CrossStackFeatureCandidateBuilder` correlates `ui-result-binding` to `ui-api-call` using only:

```text
binding.apiMethod == apiCall.Container
```

Two different services that both expose `getCards()` can therefore attach one component's list rendering to the wrong backend endpoint.

Required: service/API ownership proof; method-name equality alone is insufficient.

Full details and counterexamples:

```text
docs/reviews/2026-09-14-v0.4.5-v0.4.6-independent-review.md
```

## Reviewed exact-head automation

For reviewed HEAD `c293fc157783ce416af9e5730b6b4de005b26b6b`:

```text
CI / PokeTrade             34805328962 — PASS
pinned Loren               34805329030 — PASS
Loren-main canary          34805329006 — PASS
pinned Jellyfin            34805328945 — PASS
```

The Jellyfin workflow built the pinned repository normally before PKC compilation and verified portable parity.

Green automation is preserved regression evidence but does not override B6.1–B6.3 because those are reachable semantic false-claim paths not covered by the current controlled fixtures.

## Required next sequence

Stay in V0.4.6.

Fix only generic correctness gaps, regression-first:

```text
1. B6.1 semantic proof for supported business-predicate operations
2. B6.2 direct configured-item ownership
3. B6.3 service-aware Angular result-flow correlation
```

For each blocker:

```text
focused red regression
→ generic fix
→ focused regression green
```

Then rerun:

```text
full PKC tests
PokeTrade known-answer / live acceptance
pinned Loren
Loren-main canary
pinned Jellyfin
portable parity / no source leak
```

Request independent V0.4.6 re-review only after all three are fixed.

Do not start V0.4.7 or Azure DevOps yet.

## Azure DevOps scope lock

V0.5 remains an additional input-evidence source for Epic/Feature/PBI, acceptance intent, history/status and PR/commit linkage. It must not compensate for missing code-derived business logic.

V0.5 stays locked until V0.4.x PO-question readiness passes independently.
