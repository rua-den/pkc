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

Permanent guardrail: `docs/product-knowledge-contract.md`.

Keep distinct and retain:

```text
business conditions
value lineage / provenance
mutation / causality
```

Conservative downgrade of product-rule authority must not erase deterministic lower-authority evidence.

## Current state

```text
V0.4.4  Loren knowledge readiness              PASS / COMPLETE
V0.4.5  Jellyfin generalization               PASS / COMPLETE
V0.4.6  business logic reconstruction         IMPLEMENTATION GREEN / INDEPENDENT RE-REVIEW PENDING
V0.4.7  cross-layer PO-question readiness     LOCKED
V0.5    Azure DevOps input evidence           LOCKED
```

Production checkpoint for semantic review:

```text
868195eff5435cca1c98d4bf6ffd4b18018daf66
fix: require inert clone construction
```

Latest completed review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-7.md
FAIL / FIX REQUIRED on prior production eb0903ef...
```

Fresh review request:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-8-request.md
```

Semantic review must stay pinned to production `868195eff...` even if `main` is a later docs-only handoff commit.

## Closed scope

```text
B6.1 PASS / keep closed
B6.2 PASS / keep closed
B6.3 PASS / keep closed
```

The rereview-6 callback/comparer pipeline gap is closed. `OrderBy*`, `ThenBy*`, `Distinct`, and `ToHashSet` do not automatically borrow earlier `Where` authority. The audited callback-free subset remains accepted. Do not reopen absent a concrete contradiction.

Previously accepted B6.4 hardening also remains in force: observable return-path proof, polarity/return-context conservatism, arbitrary `Select` rejection, identity projection proof, complete supported predicate dependencies, safe direct clone members, custom-setter fail-closed behavior, and callback/comparer pipeline safety.

## Rereview-7 blocker and implementation

Rereview 7 showed that same-type projector preservation could accept:

```csharp
_cards
    .Where(card => card.IsPublished)
    .Select(CloneCard)
    .ToArray();

private static Card CloneCard(Card card) => new Card(card)
{
    IsPublished = card.IsPublished,
    Blocked = card.Blocked
};
```

when `Card(Card source)` mutates `source.IsPublished = false` before the initializer copies it.

Production `868195eff...` closes the generic construction gap conservatively. Same-type method-group projection can preserve authority only when construction is proven inert:

```text
zero constructor args
+ exact constructor symbol on projected type
+ compiler-generated implicit zero-arg constructor
+ no non-object base-constructor path
+ no instance field/event/property initializer code
+ existing safe direct same-member initializer proof
→ preservation may continue

anything unproven
→ fail closed / observed-only
```

Regressions added in `tests/Pkc.CSharp.Tests/ProjectionConstructionAuthorityRegressionTests.cs` cover:

1. source-mutating copy constructor;
2. user-defined parameterless constructor;
3. implicit constructor with instance initializer;
4. implicit constructor with effectful base constructor.

Existing positive implicit inert defensive-clone behavior remains green, including the PokeTrade known-answer path. No benchmark-specific exception was added.

Local environment limitation: the coding sandbox had no `dotnet`, so no local runtime result is claimed. The defect and implementation were inspected before the one production push; CI is the recorded runtime verification.

## Exact production gates

All gates are green on exact SHA `868195eff5435cca1c98d4bf6ffd4b18018daf66`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   34964195712 — PASS
pinned Loren                                34964195642 — PASS
Loren-main canary                           34964195717 — PASS
pinned Jellyfin                             34964195689 — PASS
```

Core CI:

```text
PKC build:                 0 warnings / 0 errors
C# tests:                  85 / 85 PASS
frontend tests:            13 / 13 PASS
WorkPlay:                  PASS
PokeTrade backend:         0 warnings / 0 errors
PokeTrade Angular build:   PASS
PokeTrade live acceptance: PASS
PokeTrade knowledge gate:  PASS
```

Pinned Jellyfin:

```text
commit:               1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
source build:          0 warnings / 0 errors
facts:                 43,363
relations:             195,314
workflow candidates:   386
product features:      116
canonical Markdown:    504
project-semantic:      43,363 / 43,363
artifact id:           10394456737
digest:                sha256:586863e971967be62ec6e0a7763cc90806903621d43ea896813e4cf3b3a2e414
size:                  9,016,935 bytes
```

Portable bundle and ZIP parity are exact; no raw `.pkc` and no `src/` source tree leaks into the portable ZIP.

## Next thread

This is now an independent-review handoff, not a coding task.

Read in order:

1. `docs/status.md`
2. `docs/handoff.md`
3. `docs/milestones.md`
4. `docs/product-knowledge-contract.md`
5. `docs/reviews/2026-09-15-v0.4.6-independent-rereview-7.md`
6. `docs/reviews/2026-09-15-v0.4.6-independent-rereview-8-request.md`

Then independently challenge exact production SHA `868195eff5435cca1c98d4bf6ffd4b18018daf66`.

If PASS: record the independent review, mark V0.4.6 complete, unlock V0.4.7 as next/current, keep V0.5 locked.

If FAIL: record the smallest compile-valid, behavior-valid contradiction; do not fix production code in the reviewer thread; keep V0.4.7 and V0.5 locked.

Do not self-approve V0.4.6.
