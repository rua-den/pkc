# Milestones

## V0.1 — C# evidence compiler — COMPLETE

Deterministic Roslyn scanner producing `.pkc/facts.json` with symbols, endpoints, routes, permissions, call evidence and source locations.

### V0.1.1 — Behavior evidence — COMPLETE

Added conditions/guards, throws, state-mutation candidates, call targets, combined routes and publication candidates.

### V0.1.2 — Workflow candidate grouping — COMPLETE

Endpoint-centered traversal groups compact backend evidence into `.pkc/feature-candidates.json`.

## V0.2 — First portable Markdown proof — COMPLETE

`pkc build <repository-path>` turns grounded candidates into canonical knowledge and portable Markdown.

## V0.3 — Frontend static evidence — COMPLETE

React/TypeScript static evidence adds routes, screens, actions, permission guards and API calls and links them to backend behavior.

## V0.4 — Product feature/workflow synthesis — ACTIVE LINE

V0.4 is complete only when generated portable knowledge is sufficiently rich for an AI to answer practical Product Owner questions about observable behavior, business conditions, value origin, mutation causality and cross-layer outcomes without re-reading source code.

PKC must preserve three distinct knowledge classes: authoritative business conditions, lower-authority value lineage/provenance, and mutation/causality evidence. Conservative downgrade must prevent false business claims without deleting deterministic causal evidence. See `docs/product-knowledge-contract.md`.

Representative exit questions:

> When is entity X sellable/visible on the web, and what exact conditions must be true for it to appear?

> Where does field X on entity Y come from besides the UI, and what code path can later change it?

### V0.4.1 — Frontend adapter architecture — COMPLETE

Common frontend adapter architecture and canonical UI evidence are accepted.

### V0.4.2 — PokeTrade real-system benchmark — COMPLETE

The runnable `.NET 10 + Angular 22` PokeTrade application is the known-answer behavioral acceptance benchmark.

### V0.4.3 — Analyzer fidelity hardening — COMPLETE

Completed semantic/fallback provenance and frontend/backend analyzer fidelity hardening.

Last accepted tool package:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

### V0.4.4 — Loren knowledge readiness — COMPLETE / EXTERNAL REVIEW PASS

Final independent review:

```text
docs/reviews/2026-09-14-v0.4.4-external-rereview-4.md
```

### V0.4.5 — Independent real-repository generalization gate — COMPLETE / INDEPENDENT REVIEW PASS

Accepted benchmark:

```text
repository: jellyfin/jellyfin
pinned commit: 1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
```

V0.4.5 remains accepted with non-blocking warnings around duplicate HTTP verb extraction, feature-level promotion, and large-pack signal/noise.

### V0.4.6 — Business logic reconstruction — COMPLETE / INDEPENDENT REVIEW PASS

Purpose: compile deterministic business-decision evidence strongly enough that an AI can answer practical `when`, `why`, `which conditions` and `what makes this visible/eligible` questions from generated knowledge.

Accepted production checkpoint:

```text
c310e893762997f34562a6b3a62dbab2b05c0c93
fix: fail closed on Queryable authority
```

Final independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-10.md
reviewed production: c310e893762997f34562a6b3a62dbab2b05c0c93
verdict: PASS / COMPLETE
```

Final disposition:

```text
B6.1 PASS — exact C# invocation semantic identity; keep closed
B6.2 PASS — conservative configured-item ownership; keep closed
B6.3 PASS — active module-qualified Angular service ownership; keep closed
B6.4 PASS — observable predicate authority is conservative across projection, callback, constructor and Queryable-provider boundaries; keep closed
```

Accepted B6.4 hardening covers:

1. local/discarded predicates do not become observable rules merely because a LINQ call exists;
2. transformed/polarity-changing return contexts for `Any`, `All`, `First*`, `Single*` fail closed unless modeled;
3. arbitrary `Select` is not an unconditional preserving operation after `Where`;
4. direct identity `Select(card => card)` is proven by symbol identity;
5. unsupported whole-item/unmodeled predicate dependencies cause conservative downgrade;
6. same-type method-group projection requires a closed item type and direct safe same-member copies;
7. custom setter, nested initializer and rewritten output effects fail closed;
8. callback/comparer-bearing ordering/equality operations do not preserve authority merely from LINQ target identity;
9. callback-free Enumerable pipeline preservation is limited to an audited exact-shape subset;
10. same-type clone construction must itself be proven inert;
11. exact `System.Linq.Queryable` predicate targets are observed-only without provider-semantics proof;
12. an Enumerable `Where` authority path fails closed after any Queryable pipeline hop;
13. downgraded predicate evidence is retained through `observes-predicate` instead of being deleted.

Rereview 10 challenged direct and static Queryable shapes, mixed Enumerable/Queryable chains, semantic fallback, multiple Queryable hops, projections after downgrade, downstream rule synthesis, cross-stack enrichment, product aggregation and portable rendering. No compile-valid/behavior-valid false-authority bypass was found.

Non-blocking rereview-10 warnings:

```text
W10.1 rendered Evidence does not yet print an explicit `observed-only` label.
W10.2 unreachable Queryable names remain in old internal safe-operation sets behind the new guard.
```

Exact accepted-production gates:

```text
CI + PKC tests + WorkPlay + PokeTrade   34990080620 — PASS
pinned Loren                            34990080707 — PASS
Loren-main canary                       34990080551 — PASS
pinned Jellyfin                         34990080546 — PASS
```

Core evidence:

```text
Release build:       0 warnings / 0 errors
C# tests:            88 / 88 PASS
frontend tests:      13 / 13 PASS
WorkPlay:            PASS
PokeTrade:           PASS
```

Pinned Jellyfin evidence:

```text
source build:         0 warnings / 0 errors
facts:                43,363
relations:            195,314
workflow candidates:  386
product features:     116
canonical Markdown:   504
project-semantic:     43,363 / 43,363
portable parity:      PASS
portable ZIP parity:  PASS
raw .pkc leak:        NONE
src/ leak:            NONE
artifact id:          10405810551
digest:               sha256:8664945310d5fd0da3a0c838b001cc5fa343174410dc1ac6d05b1335e41b0257
size:                 9,159,880 bytes
```

V0.4.6 is closed by independent rereview 10.

### V0.4.7 — Cross-layer PO question readiness — CURRENT / NEXT MILESTONE

Purpose: broaden supported direct business-logic patterns into robust cross-layer product-behavior understanding.

Before coding, define concrete acceptance questions and regression gates for semantic chains such as:

```text
source value
→ copied / derived / snapshotted value
→ persisted or returned value
→ later mutation / override
→ backend condition / API output
→ frontend visibility / behavior
→ PO-facing explanation with provenance
```

Planned target coverage includes:

- frontend visibility/filter predicates that independently affect observable outcomes;
- DTO/projection/computed transformations that change observable state;
- cross-entity value lineage such as `ProductGroup.A → Product.A → Service.A` where source proves each copy/derivation step;
- snapshot/copy versus dynamic/reference-derived semantics;
- later overrides and mutation/causality paths that explain why a persisted or returned value changed;
- preservation of lower-authority causal evidence even when product-rule authority is downgraded;
- composition of backend conditions, data/value flow and frontend behavior into a PO-facing explanation;
- high-signal promotion so useful causal/value-origin knowledge is not buried in raw implementation noise.

V0.4.7 must preserve every accepted V0.4.6 authority and evidence-retention guardrail.

## V0.5 — Azure DevOps input evidence — LOCKED

Azure DevOps is planned as an additional compiler input for requirement intent, Epic/Feature/PBI history, status and traceability. ADO must not compensate for missing code-derived business logic.

Until ADO is integrated, its absence should be declared as a global knowledge boundary rather than repeated in every feature/workflow file.

V0.5 may start only after the V0.4.x PO-question-readiness exit gate independently passes.

## V0.6 — Incremental compilation

Use file/symbol hashes and dependency impact to rebuild only affected evidence and knowledge. Add PR/CI knowledge diffs.

## V0.7 — Runtime UI exploration

Use browser automation to confirm actual user-visible flows and states.

## V0.8 — Product insight

Surface gaps, inconsistencies, requirement-vs-implementation drift and improvement opportunities with evidence.
