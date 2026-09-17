# PKC Status

Last updated: 2026-09-17

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                 PASS / COMPLETE
V0.4.5 real-repository generalization            PASS / COMPLETE
V0.4.6 business logic reconstruction             PASS / COMPLETE
V0.4.7-A origin and copy timing                  PASS / COMPLETE
V0.4.7-B computation and later change            CURRENT / NEXT CHECKPOINT
V0.4.7-C backend to API                          LOCKED behind B
V0.4.7-D API to UI                               LOCKED behind C
V0.4.7-E product acceptance                      LOCKED behind D
V0.5 Azure DevOps input evidence                 LOCKED
```

V0.4.7 acceptance is defined in `docs/v0.4.7-acceptance-plan.md`.

## Closed V0.4.6 baseline

Accepted V0.4.6 production remains:

```text
c310e893762997f34562a6b3a62dbab2b05c0c93
fix: fail closed on Queryable authority
```

Final independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-10.md
PASS / COMPLETE
```

Do not reopen B6.1-B6.4 without a new compile-valid and behavior-valid contradiction.

Permanent product contract: `docs/product-knowledge-contract.md`.

Keep distinct and retain:

```text
business conditions
value lineage / provenance
mutation / causality
```

## V0.4.7-A — PASS / COMPLETE

Accepted A implementation checkpoint on `main`:

```text
09a0c7f2a058adfd7ddb7b4ac2feb2d17580a429
feat: prove bounded reference dynamic lineage
```

A now proves both required temporal shapes through scanner → candidate → synthesis → workflow Markdown:

```text
stored scalar snapshot
ProductGroup.Price → Product.Price → Service.Price

reference / dynamic read-time dependency
ProductGroup.Price → Service.CurrentGroupPrice
```

The dynamic fixture executes downstream `100`, changes upstream `100 → 120`, then executes a later downstream read returning `120` without another scalar copy.

The snapshot boundary remains conservative for receiver reassignment, opaque helper mutation, alias writes, cast/`as` aliasing and user-defined conversion alias construction. Unsupported shapes fail closed without deleting independently valid direct-transfer evidence. Same-name/property-name similarity is never lineage proof.

Relevant review/history:

```text
docs/reviews/2026-09-16-v0.4.7-snapshot-checkpoint-review.md
docs/reviews/2026-09-17-v0.4.7-alias-composition-review.md
docs/reviews/2026-09-17-v0.4.7-reference-dynamic-delivery.md
```

### Exact-main final verification for A

All required gates passed on exact code SHA `09a0c7f2a058adfd7ddb7b4ac2feb2d17580a429`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35232224202 — PASS
pinned Loren                                35232224014 — PASS
Loren-main canary                           35232224029 — PASS
pinned Jellyfin                             35232223958 — PASS
```

Core CI:

```text
Release build:       0 warnings / 0 errors
C# tests:            105 / 105 PASS
frontend tests:      13 / 13 PASS
tool pack/install:   PASS
WorkPlay:            PASS
PokeTrade:           PASS
```

Pinned Jellyfin portable gate:

```text
portable parity/no-leak: PASS
artifact id:            10501038912
artifact digest:        sha256:41d90fbe3951d85474eb37121d2e5d04442fdb447aba5db56db7da755d66c129
artifact size:          9,162,486 bytes
```

Checkpoint A is closed. Do not reopen it without a new compile-valid and behavior-valid contradiction.

## Current checkpoint — V0.4.7-B

PO questions:

```text
Was this value directly copied or computed?
What code path can change it after creation?
What was the last deterministically observed source before return/persistence?
```

Acceptance gates are R7.5–R7.7 plus applicable evidence-retention and workflow-rendering checks.

The first bounded supported shape should prove:

```text
Service.Price + Service.Discount → Service.NetPrice
mechanism: derivation

initial Product.Price → Service.Price origin
later write → Service.Price
causal role: mutation / override

return boundary
→ retain the last proven source/derivation immediately before return
```

Required behavior:

- retain every supported derivation input and exact source location;
- keep original origin distinct from later override/mutation history;
- a later override must not rewrite history as though it were the original source;
- terminal-source proof must distinguish earliest origin from the last source before return/persistence;
- keep value lineage separate from mutation/causality and business Rules;
- fail closed on unsupported operators, alias/effect ambiguity, custom accessors, control-flow ambiguity or unresolved project semantics;
- deliver the answer through generated workflow Markdown, not raw facts only;
- preserve all V0.4.6 and V0.4.7-A regressions.

Do not broaden the first B slice into a general expression/dataflow solver.

## Version semantics

Do not conflate roadmap, package and schema versions.

```text
roadmap:             V0.4.7-B CURRENT
tool/package:        RuaDen.Pkc.Tool 0.4.3-preview.2
C# raw schema:       0.4.4-csharp-raw
merged facts schema: 0.4.4
cross-stack candidate schema: 0.4.6
frontend schema:     0.4.3-frontend
```

No package/schema bump is implied by starting B.

## V0.4.6 warnings carried forward

```text
W10.1
Observed-only predicate Evidence is separated from Rules but does not explicitly render the `observed-only` label.

W10.2
Queryable names remain in old safe-operation sets but are unreachable behind the Queryable fail-closed guard.
```

These remain non-blocking unless touched scope makes a regression-safe cleanup coherent.

## Exact next action

Start **V0.4.7-B regression-first**. Implement the minimum generic backend proof for R7.5–R7.7: multi-input scalar derivation, later override/mutation causality that preserves the original origin, and exact terminal source before a supported return boundary. Render all three at PO-readable workflow level and keep unsupported shapes fail-closed.

Do not begin C/D/E or V0.5 until B passes its focused/full/cross-benchmark gates and review.
