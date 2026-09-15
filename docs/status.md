# PKC Status

Last updated: 2026-09-15

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                 PASS / COMPLETE
V0.4.5 real-repository generalization            PASS / COMPLETE
V0.4.6 business logic reconstruction             PASS / COMPLETE
V0.4.7 cross-layer PO-question readiness         CURRENT / NEXT MILESTONE
V0.5 Azure DevOps input evidence                 LOCKED
```

Accepted V0.4.6 production checkpoint:

```text
c310e893762997f34562a6b3a62dbab2b05c0c93
fix: fail closed on Queryable authority
```

Final independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-10.md
verdict: PASS / COMPLETE
reviewed production: c310e893762997f34562a6b3a62dbab2b05c0c93
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

## V0.4.6 final disposition

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

Rereview 10 independently challenged direct Queryable predicates, static/extension syntax, semantic candidate fallback, mixed Enumerable/Queryable chains, multiple Queryable hops, projection after downgrade, downstream synthesis/product aggregation, and portable rendering. No compile-valid/behavior-valid false-authority bypass was found.

## Non-blocking warnings

```text
W10.1 rendered Evidence says `Business predicate: ...` without explicitly printing `observed-only`; authority separation is currently conveyed by Evidence placement and AI instructions.
W10.2 old Queryable names remain in internal safe-operation sets but are unreachable because the Queryable guard rejects them first.
```

Neither warning produced an authoritative Product Owner rule in the reviewed pipeline.

## Exact automation for accepted production

All exact-SHA gates are green on `c310e893762997f34562a6b3a62dbab2b05c0c93`:

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

PokeTrade:

```text
.NET 10 backend build:      PASS, 0 warnings / 0 errors
Angular 22 build:           PASS
live business acceptance:   PASS
known-answer knowledge:     PASS
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

V0.4.6 is closed. V0.4.7 is the current / next milestone.

Before implementation, define concrete V0.4.7 acceptance questions and regression gates for cross-layer PO-question readiness, especially value lineage, copy/snapshot vs dynamic semantics, later mutation/override causality, frontend visibility, and backend→frontend composition.

Preserve all accepted V0.4.6 authority/evidence-retention boundaries. Do not begin Azure DevOps ingestion yet.

```text
V0.4.7 CURRENT / NEXT MILESTONE
V0.5 LOCKED
```
