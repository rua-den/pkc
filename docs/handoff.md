# PKC Handoff

Use this file when continuing PKC in another coding/review thread.

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
V0.4.6  business logic reconstruction         INDEPENDENT RE-REVIEW FAIL / 3 BLOCKERS
V0.4.7  cross-layer PO-question readiness     LOCKED
V0.5    Azure DevOps input evidence           LOCKED
```

Latest independent re-review:

```text
docs/reviews/2026-09-14-v0.4.6-independent-rereview.md
```

Reviewed implementation checkpoint:

```text
478e92343154083c3987f07e4fbad66a042c25e8
```

Reviewed code HEAD before review-documentation commits:

```text
45b2d9b6e215f26c395843c77f1e59887774e21b
```

## Read first

```text
1. docs/status.md
2. docs/handoff.md
3. docs/milestones.md
4. docs/reviews/2026-09-14-v0.4.6-independent-rereview.md
5. docs/reviews/2026-09-14-v0.4.5-v0.4.6-independent-review.md
6. docs/reviews/2026-09-14-v0.4.6-independent-rereview-request.md
```

## Accepted baseline

V0.4.4 and V0.4.5 are independently accepted PASS.

V0.4.5 benchmark remains:

```text
jellyfin/jellyfin
1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
```

Do not reopen accepted gates without concrete contradictory evidence or a new regression caused by later compiler changes.

## V0.4.6 re-review disposition

The coding thread genuinely reproduced the original B6.1/B6.2/B6.3 defects regression-first and improved all three areas.

Independent re-review result:

```text
B6.1  BLOCK
B6.2  PASS
B6.3  BLOCK
B6.4  BLOCK
```

### B6.1 — exact invocation identity still missing

Current predicate authority correctly requires a semantic target on:

```text
System.Linq.Enumerable
System.Linq.Queryable
```

but associates that proof with an invocation using only:

```text
owner + path + StartLine + EndLine
```

`SourceLocation` has no column/span identity.

Reachable counterexample:

```csharp
_ = _bucket.Where(card => card.IsPublished); return _cards.Where(card => card.IsPublished).ToArray();
```

where `_bucket.Where` is custom and `_cards.Where` is genuine LINQ. Both are on the same source line. The custom invocation can borrow the LINQ relation from the real invocation and become a high-confidence business predicate.

Required coding-thread work:

```text
RED: same-line custom Where + real LINQ Where
FIX: exact invocation semantic identity, not line-range identity
```

Prefer exact syntax span/column provenance or direct semantic resolution of the same invocation.

Apply the authority rule generically to the supported operation table, not only `Where`.

### B6.2 — accepted PASS

Current direct configured-item extraction is sufficiently conservative for the reviewed scope. Nested property object initializers are not promoted as additional collection items.

Do not change B6.2 unless required by a new concrete regression.

### B6.3 — service identity still ambiguous across modules

The current Angular correlation requires:

```text
binding.apiMethod == apiCall.Container
binding.serviceType == apiCall.ownerClass
```

but both identities are simple class names.

Reachable counterexample:

```text
catalog/cards-api.ts → export class CardsApi → GET /api/cards
admin/cards-api.ts   → export class CardsApi → GET /api/admin/cards
CatalogComponent imports only ./catalog/cards-api and renders its result
```

Both API facts have `ownerClass = CardsApi`, so the admin endpoint can still inherit the Catalog list flow.

Required coding-thread work:

```text
RED: two modules export same class + method name with different routes
FIX: deterministic module/import-qualified service identity
```

If module identity cannot be proven, omit cross-stack list correlation.

### B6.4 — operation existence is stronger than observable behavior proof

This is the new blocker found during re-review.

Current code can prove that a LINQ operation exists, but then promotes it into endpoint business wording without proving the result affects the endpoint's observable outcome.

Reachable counterexample:

```csharp
public IReadOnlyList<Card> GetCards()
{
    var published = _cards.Where(card => card.IsPublished).ToArray();
    Audit(published.Length);
    return _cards;
}
```

The LINQ `Where` is real, yet the endpoint returns unfiltered `_cards`. Current synthesis can still claim:

```text
Includes items from `_cards` only when `card.IsPublished`.
```

Required coding-thread work:

```text
RED: real LINQ predicate computed but not used for observable endpoint outcome
FIX: distinguish local observed predicate from proven observable business rule
```

Only promote authoritative PO wording when deterministic control/data-flow evidence proves the predicate participates in the return, guard, mutation or other observable result.

Also guard context/polarity for `Any`/`All`/selection wording. For example, `if (items.Any(x => x.Blocked)) throw ...` must not become a positive requirement that blocked items exist.

## Regression history already verified

The original red runs are genuine:

```text
B6.1  f09658a8f5f097abd4003b15e5261a5388691488 / 34806787160 — FAIL expected
B6.2  bec7ff74d48dc7b56b27850f3060d690e1bb311a / 34807054449 — FAIL expected
B6.3  aee74969d26627ba7e446de3e28bed5ce6276110 / 34807296605 — FAIL expected
```

## Exact reviewed-head verification

For code HEAD `45b2d9b6e215f26c395843c77f1e59887774e21b`:

```text
CI + PokeTrade       34807855630 — PASS
pinned Loren         34807855596 — PASS
Loren-main push      34807855650 — PASS
Loren-main schedule  34807965935 — PASS
pinned Jellyfin      34807855635 — PASS
```

CI:

```text
build           0 warnings / 0 errors
C# tests        59 / 59 PASS
frontend tests  11 / 11 PASS
tool pack       PASS
WorkPlay        PASS
PokeTrade       PASS
```

Current-head Jellyfin portable artifact independently rechecked:

```text
artifact id          10334150660
artifact digest      sha256:17b546f3119f4350dfffa5fc22833a80872a76c677005ca0c2c66810c6f79f82
structured Markdown  504
bundle parity        504 / 504
ZIP set parity       PASS
ZIP byte parity      504 / 504
raw/source leak      0
```

## Exact next action

Stay in V0.4.6 and fix only:

```text
B6.1 exact invocation identity
B6.3 module-qualified Angular service identity
B6.4 observable predicate-effect authority
```

For each:

```text
focused red regression
→ verify expected failure
→ generic fix
→ focused green
```

Then rerun:

```text
full PKC tests
PokeTrade known-answer/live acceptance
pinned Loren
Loren-main canary
pinned Jellyfin
portable parity/no-leak
```

Update status/handoff and request independent V0.4.6 re-review.

## Scope locks

Do not start V0.4.7.
Do not start Azure DevOps ingestion.

V0.5 remains an additional future input-evidence source for Epic/Feature/PBI, acceptance intent, history/status and PR/commit linkage. It must not compensate for missing code-derived business logic.

## Coding-thread bootstrap

```text
Continue PKC from current main HEAD.

Read:
1. docs/status.md
2. docs/handoff.md
3. docs/reviews/2026-09-14-v0.4.6-independent-rereview.md

Stay in V0.4.6. Fix only B6.1, B6.3 and B6.4 regression-first.

B6.1: semantic proof must identify the exact invocation; same-line custom and genuine LINQ calls must not share authority.
B6.3: Angular service identity must distinguish same-named classes from different modules/imports.
B6.4: a real LINQ predicate must not become an endpoint business rule unless deterministic evidence proves it affects the observable outcome.

Keep B6.2 closed.

Rerun all current cross-benchmark and portable-integrity gates, update status/handoff, and request independent re-review.

Do not start V0.4.7 or Azure DevOps.
```
