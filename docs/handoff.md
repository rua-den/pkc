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

PKC must keep distinct and retain:

```text
business conditions
value lineage / provenance
mutation / causality
```

Failing to prove an authoritative business rule must not erase deterministic causal/value-origin evidence. Example target question:

> Where does Service.A come from besides the UI, and what code path can later change it?

Until Azure DevOps is integrated, declare that missing evidence source once at the global knowledge boundary. Do not spam every feature/workflow Markdown file with ADO/product-intent unknown placeholders.

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
18a1f1d1ef551833d859f23ea2e92dd548a6a81d
fix: fail closed on unsafe same-type projector effects
```

Latest completed independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-5.md
verdict: FAIL / FIX REQUIRED
```

Next independent review request:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-6-request.md
```

Current disposition:

```text
B6.1 PASS / keep closed
B6.2 PASS / keep closed
B6.3 PASS / keep closed
B6.4 IMPLEMENTATION GREEN / independent acceptance pending
```

Do not start V0.4.7 or Azure DevOps until V0.4.6 independently passes.

## B6.4 implementation checkpoint

Re-review 5 found one remaining false-authority path: a same-type projector could copy a predicate member correctly and then another initializer assignment could invoke a custom setter that mutates the copied predicate state.

Checkpoint `18a1f1d1ef551833d859f23ea2e92dd548a6a81d` narrows the supported projector boundary instead of attempting general side-effect analysis.

Current generic rule:

```text
same-type Select method-group projector preserves Where authority only when
  predicate dependencies are completely proven
AND
  every initializer entry is a simple assignment
AND
  every output target is a direct non-static field or auto-property
AND
  every assignment is exact same-member input → output copy
AND
  every predicate-required member is copied

anything else
→ no projection-preservation proof
→ observed-only / no authoritative Product Owner inclusion rule
```

This rejects custom setters, nested/unmodeled effects, constants/rewrites/helpers in clone assignments and other output mutations that have not been proven harmless.

Focused regression:

```text
Later_custom_setter_that_invalidates_predicate_state_downgrades_projection_authority
```

Regression-fixture note: the re-review-5 example's source initializer order would itself run the mutating setter after `IsPublished = true`, leaving the source false before `Where`. The production regression uses behavior-valid ordering (`Blocked = false, IsPublished = true`) so the source passes `Where` and the returned clone is then invalidated by the later setter. The independent review document is intentionally left unchanged as an audit record.

Positive regression retained:

```text
Predicate_preserving_same_type_method_group_projection_remains_authoritative
```

Arbitrary non-identity projection remains conservative:

```text
Non_identity_projection_does_not_preserve_returned_filter_authority
```

The authority downgrade is not permission to delete deterministic assignment/mutation evidence. Future lineage/causality synthesis must be able to preserve facts such as `ProductGroup.A → Product.A → Service.A` and setter-driven changes while keeping product-intent authority separate.

## Exact checkpoint verification

All current gates are green on exact production SHA `18a1f1d1ef551833d859f23ea2e92dd548a6a81d`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   34954590262 — PASS
pinned Loren                                34954590188 — PASS
Loren-main canary                           34954590239 — PASS
pinned Jellyfin                             34954590155 — PASS
Jellyfin portable parity/provenance         PASS
```

Core evidence:

```text
PKC build:                 0 warnings / 0 errors
C# tests:                  77 / 77 PASS
frontend tests:            13 / 13 PASS
local tool pack/install:   PASS in CI runner
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
artifact id:             10390258392
artifact digest:         sha256:6ba875d99cf64141267bb13811485cc025eabc87723beaa9f3a46a985860b497
artifact size:           9,016,989 bytes
```

## Validation environment note

The coding sandbox had no local `dotnet` executable, so local runtime red/green execution was unavailable. The regression, generic fix and coherent diff were reviewed before a single production push. Exact-head GitHub Actions then provided final runtime validation. Do not reinterpret that as local execution evidence.

## Next thread / reviewer read order

1. `docs/status.md`
2. `docs/handoff.md`
3. `docs/milestones.md`
4. `docs/product-knowledge-contract.md`
5. `docs/reviews/2026-09-15-v0.4.6-independent-rereview-5.md`
6. `docs/reviews/2026-09-15-v0.4.6-independent-rereview-6-request.md`
7. inspect current `main`, but semantically review exact production checkpoint below

Independent reviewer must review exact production checkpoint:

```text
18a1f1d1ef551833d859f23ea2e92dd548a6a81d
```

not a later docs-only handoff SHA.

If independent review returns PASS, mark V0.4.6 complete and unlock V0.4.7. Keep V0.5 locked.

Until then, do not advance milestones and do not modify production without a new concrete blocker.
