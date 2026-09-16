# PKC Status

Last updated: 2026-09-16

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                 PASS / COMPLETE
V0.4.5 real-repository generalization            PASS / COMPLETE
V0.4.6 business logic reconstruction             PASS / COMPLETE
V0.4.7 cross-layer PO-question readiness         IN PROGRESS
V0.5 Azure DevOps input evidence                 LOCKED
```

V0.4.7 acceptance is defined in:

```text
docs/v0.4.7-acceptance-plan.md
```

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

## V0.4.7-A checkpoint state

The first **copy/snapshot** implementation slice is now implementation-green on current production code:

```text
82fac01e669bce0a35dafde80f54c8f0baa595e5
fix: compile lineage regressions
```

Implementation ancestry for this slice:

```text
350ba2468e0d1b011936695fc48c1a970650d647  feat: prove scalar snapshot value lineage
0bd5f823b7ebdfa8d0a54ff25d079d2056be4a81  fix: compile scalar lineage proof
82fac01e669bce0a35dafde80f54c8f0baa595e5  fix: compile lineage regressions
```

This checkpoint proves the bounded supported shape:

```text
ProductGroup.Price
→ Product.Price
→ Service.Price
```

as deterministic direct scalar stored-copy lineage with snapshot timing. Generated workflow knowledge can explain that changing `ProductGroup.Price` later does not retroactively change the already stored `Product.Price` / `Service.Price` copies.

The implementation keeps value lineage separate from mutation semantics and renders a dedicated `Value lineage` section instead of promoting lineage into business Rules.

Focused regression coverage includes:

- executable snapshot behavior matching generated knowledge;
- exact source/target occurrence and semantic identity metadata;
- two-edge chain composition through the same stored location;
- distinct-receiver collision fail-closed behavior;
- intervening overwrite breaks prior lineage composition;
- incompatible branch shapes fail closed;
- custom setter effects fail closed;
- shared mutable-reference content is not labeled a scalar snapshot;
- unresolved/no-project semantic context does not produce proven lineage;
- same display members in distinct projects retain project/assembly-qualified identity;
- generated candidate → synthesis → Markdown delivery;
- lineage is absent from authoritative business Rules.

### Important: A is NOT complete

Do **not** advance to V0.4.7-B yet.

Checkpoint A still requires its separate positive proof for:

```text
reference / dynamic-read semantics
```

The acceptance plan requires both snapshot and dynamic/reference positives before A can be marked complete. Current state is therefore:

```text
V0.4.7-A snapshot slice             GREEN
V0.4.7-A dynamic/reference slice    NOT IMPLEMENTED / NEXT
V0.4.7-B/C/D/E                      LOCKED behind A
V0.5                                LOCKED
```

## Exact-SHA verification for snapshot slice

All existing final verification gates passed on exact code SHA `82fac01e669bce0a35dafde80f54c8f0baa595e5`:

```text
CI + PKC tests + WorkPlay + PokeTrade   35113764215 — PASS
pinned Loren                            35113764207 — PASS
Loren-main canary                       35113764095 — PASS
pinned Jellyfin                         35113764184 — PASS
```

Core CI evidence:

```text
Release build:       0 warnings / 0 errors
C# tests:            92 / 92 PASS
frontend tests:      13 / 13 PASS
tool pack/install:   PASS
WorkPlay:            PASS
PokeTrade:           PASS
```

Pinned Jellyfin evidence:

```text
commit:                  1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
source build:            0 warnings / 0 errors
facts:                   43,365
relations:               195,316
workflow candidates:     386
product features:        116
canonical Markdown:      504
project-semantic:        43,365 / 43,365
portable bundle parity:  PASS
ZIP file-set parity:     PASS
ZIP byte parity:         PASS
raw .pkc leak:           none
src/ source-tree leak:   none
artifact id:             10454295890
artifact digest:         sha256:2ac8778a3366767e344c0bf4bdc2403037b900eddb338a89c03d0cf5d77554cc
artifact size:           9,162,455 bytes
```

The small Jellyfin fact/relation increase versus accepted V0.4.6 is expected from the new deterministic value-transfer evidence; benchmark portability and source-build gates remain green.

## Version semantics

These version domains are independent:

```text
roadmap milestone version
tool/package version
evidence/schema version
```

Current examples:

```text
roadmap:             V0.4.7 IN PROGRESS
tool/package:        RuaDen.Pkc.Tool 0.4.3-preview.2
C# raw schema:       0.4.4-csharp-raw
merged facts schema: 0.4.4
cross-stack candidate schema: 0.4.6
frontend schema:     0.4.3-frontend
```

Do not mechanically bump package or schema versions because the roadmap milestone advances. Schema versions change only when that serialized contract/schema changes.

## Exact next action

Stay in **V0.4.7-A**.

Implement the separate dynamic/reference positive regression-first. It must deterministically distinguish a live/reference read from a stored scalar snapshot and render the PO-facing answer through scanner → candidate → synthesis → workflow Markdown.

Keep the current snapshot proof and all its fail-closed negatives green. Do not broaden into general alias/path analysis. Do not begin V0.4.7-B/C/D until A's snapshot + dynamic/reference + portable delivery gates are all green.

Astra/reviewer should review the snapshot checkpoint at `82fac01e...` as a bounded V0.4.7-A slice, **not** as completion of checkpoint A or V0.4.7.

```text
V0.4.7 IN PROGRESS
V0.5 LOCKED until the V0.4.7 / V0.4.x PO-question-readiness exit gate passes
```
