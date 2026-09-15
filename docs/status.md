# PKC Status

Last updated: 2026-09-16

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                 PASS / COMPLETE
V0.4.5 real-repository generalization            PASS / COMPLETE
V0.4.6 business logic reconstruction             PASS / COMPLETE
V0.4.7 cross-layer PO-question readiness         CURRENT / NEXT MILESTONE
V0.5 Azure DevOps input evidence                 LOCKED
```

V0.4.7 planning/acceptance is now defined in:

```text
docs/v0.4.7-acceptance-plan.md
```

No V0.4.7 analyzer/compiler behavior has been implemented by this planning checkpoint.

Accepted V0.4.6 production remains:

```text
c310e893762997f34562a6b3a62dbab2b05c0c93
fix: fail closed on Queryable authority
```

Final independent V0.4.6 review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-10.md
verdict: PASS / COMPLETE
reviewed production: c310e893762997f34562a6b3a62dbab2b05c0c93
```

V0.4.6 is closed. Do not reopen B6.1-B6.4 without a new compile-valid and behavior-valid contradiction.

## Product contract governing V0.4.x

Permanent guardrail: `docs/product-knowledge-contract.md`.

Keep distinct and retain:

```text
business conditions
value lineage / provenance
mutation / causality
```

No deterministic proof means no authoritative product claim. Conservative authority downgrade must not erase deterministic lower-authority causal/value-origin evidence.

## V0.4.6 accepted boundary

```text
B6.1 PASS — exact C# invocation semantic identity; keep closed
B6.2 PASS — conservative configured-item ownership; keep closed
B6.3 PASS — module-qualified Angular service ownership; keep closed
B6.4 PASS — Queryable/provider-mediated authority fails closed without provider-semantics proof; keep closed
```

Accepted B6.4 hardening includes:

- discarded/local predicate downgrade;
- transformed/polarity-changing return-context fail-closed behavior;
- arbitrary `Select` rejection and identity projection proof;
- complete supported predicate-dependency proof;
- safe direct defensive-clone member copies;
- custom setter / initializer effect fail-closed behavior;
- callback/comparer-bearing ordering/equality pipeline fail-closed behavior;
- exact-shape callback-free Enumerable pipeline preservation;
- inert-constructor requirement for same-type defensive clones;
- Queryable predicates are observed-only unless provider semantics are proven;
- an Enumerable `Where` path loses returned-item authority when it crosses any Queryable pipeline hop;
- downgraded predicates remain deterministic Evidence through `observes-predicate` rather than being deleted.

## V0.4.7 acceptance scope

V0.4.7 must make portable knowledge answer practical PO questions across value flow and layers, including:

```text
Where did this value originally come from?
If the upstream value changes later, does the existing downstream value change automatically?
What code path can change this value after creation?
Was this value directly copied or computed?
What was the last observed source before the value was persisted or returned?
What backend conditions and frontend conditions jointly determine whether an item is visible?
How did the value move through backend → DTO/projection → API → frontend composition?
If authority is incomplete, what lineage or causal evidence is still deterministically known?
```

Canonical lineage acceptance includes:

```text
ProductGroup.Price → Product.Price → Service.Price
```

with each edge classified from proof as applicable:

```text
copy
snapshot
derivation
reference/dynamic
override
mutation
```

A blocking collision regression must prove that unrelated same-name members such as:

```text
Product.Price
Service.Price
Dto.Price
Component.price
```

are never connected merely because names match.

See `docs/v0.4.7-acceptance-plan.md` for the complete regression matrix and implementation sequence.

## V0.4.6 warnings carried forward as non-blocking V0.4.7 considerations

```text
W10.1
Observed-only predicate evidence is separated from Rules but rendered Evidence does not explicitly print `observed-only`.

W10.2
Queryable names remain in old safe-operation sets but are currently unreachable behind the Queryable fail-closed guard.
```

Do not reopen V0.4.6 for these warnings.

## Version semantics

These version domains are independent:

```text
roadmap milestone version
tool/package version
evidence/schema version
```

Current examples:

```text
roadmap:             V0.4.7 CURRENT
tool/package:        RuaDen.Pkc.Tool 0.4.3-preview.2
C# raw schema:       0.4.4-csharp-raw
merged facts schema: 0.4.4
cross-stack candidate schema: 0.4.6
frontend schema:     0.4.3-frontend
```

Do not mechanically bump package or schema versions because the roadmap milestone advances. Schema versions change only when that serialized contract/schema changes.

This planning checkpoint does not change package or schema versions.

## Exact accepted-production automation

All exact-SHA gates remain accepted on `c310e893762997f34562a6b3a62dbab2b05c0c93`:

```text
CI + PKC tests + WorkPlay + PokeTrade   34990080620 — PASS
pinned Loren                            34990080707 — PASS
Loren-main canary                       34990080551 — PASS
pinned Jellyfin                         34990080546 — PASS
```

Core CI:

```text
Release build:       0 warnings / 0 errors
C# tests:            88 / 88 PASS
frontend tests:      13 / 13 PASS
tool pack/install:   PASS
WorkPlay:            PASS
```

Pinned Jellyfin:

```text
commit:                  1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
source build:            0 warnings / 0 errors
facts:                   43,363
relations:               195,314
workflow candidates:     386
product features:        116
canonical Markdown:      504
project-semantic:        43,363 / 43,363
portable bundle parity:  PASS
ZIP file-set parity:     PASS
ZIP byte parity:         PASS
raw .pkc leak:           none
src/ source-tree leak:   none
artifact id:             10405810551
artifact digest:         sha256:8664945310d5fd0da3a0c838b001cc5fa343174410dc1ac6d05b1335e41b0257
artifact size:           9,159,880 bytes
```

## Exact next action

Stay in V0.4.7. Do not start Azure DevOps ingestion.

Start the first V0.4.7 implementation checkpoint regression-first:

```text
ProductGroup.Price
→ Product.Price
→ Service.Price
```

Require exact symbol/dataflow-backed direct-copy snapshot edges, plus a same-name collision negative covering `Product.Price`, `Service.Price`, `Dto.Price` and `Component.price`.

Only after that regression is red and compile-valid should implementation add the minimum generic backend lineage evidence/model needed to pass it.

Do not begin DTO/frontend composition until the backend lineage identity/collision gate is green.

```text
V0.4.7 CURRENT / NEXT MILESTONE
V0.5 LOCKED until the V0.4.7 / V0.4.x PO-question-readiness exit gate passes
```
