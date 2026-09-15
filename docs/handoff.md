# PKC Handoff

Use this file when continuing PKC in another coding or independent-review thread.

## Read first

1. `docs/status.md`
2. this handoff
3. `docs/milestones.md`
4. `docs/product-knowledge-contract.md`
5. `docs/reviews/2026-09-15-v0.4.6-independent-rereview-10.md`

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
V0.4.6  business logic reconstruction         PASS / COMPLETE
V0.4.7  cross-layer PO-question readiness     CURRENT / NEXT MILESTONE
V0.5    Azure DevOps input evidence           LOCKED
```

Accepted V0.4.6 production:

```text
c310e893762997f34562a6b3a62dbab2b05c0c93
fix: fail closed on Queryable authority
```

Final independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-10.md
PASS / COMPLETE on production c310e893762997f34562a6b3a62dbab2b05c0c93
```

V0.4.6 is closed. Do not reopen accepted boundaries without a new concrete compile-valid/behavior-valid contradiction.

## Accepted V0.4.6 scope

```text
B6.1 PASS — exact C# invocation semantic identity
B6.2 PASS — conservative configured-item ownership
B6.3 PASS — module-qualified Angular service ownership
B6.4 PASS — observable business-predicate authority is conservative and provider-aware
```

Accepted B6.4 hardening includes:

1. discarded/local predicates downgrade;
2. transformed/polarity-changing `Any`/`All`/`First*`/`Single*` contexts fail closed;
3. arbitrary `Select` is not preserving by default;
4. identity `Select(card => card)` uses symbol identity;
5. whole-item/unmodeled predicate dependencies fail closed;
6. direct defensive clones require safe stored same-member copies;
7. custom setters, nested writes and rewritten initializer effects fail closed;
8. ordering/equality callback/comparer paths do not preserve authority merely from LINQ target identity;
9. callback-free Enumerable pipeline preservation is exact-shape and conservative;
10. same-type defensive clone construction must be deterministically inert;
11. exact `System.Linq.Queryable` predicate targets are observed-only unless provider semantics are independently proven;
12. an Enumerable `Where` path loses returned-item authority when it crosses any `System.Linq.Queryable.*` pipeline hop;
13. authority downgrade retains deterministic predicate evidence through `observes-predicate` instead of deleting it.

## Rereview-10 acceptance rationale

Rereview 9 found that a custom `IQueryable<T>` / `IQueryProvider` can retain a `Where` expression while enumeration ignores it. That made exact Queryable method identity insufficient for a Product Owner claim.

Production `c310e893...` closes the gap generically:

```text
Queryable predicate
→ deterministic predicate Evidence retained
→ businessRuleAuthority = observed-only
→ no authoritative returned-item/business rule
```

and:

```text
Enumerable Where
→ any later Queryable pipeline hop
→ returned-item authority fails closed
```

Fresh independent review challenged direct Queryable predicates, static/extension syntax, ambiguous semantic resolution, mixed Enumerable/Queryable chains, multiple Queryable hops, projection after downgrade, workflow synthesis, cross-stack enrichment, product aggregation and portable rendering. No false-authority bypass was found.

Downstream authority separation is important:

- `GroundedKnowledgeSynthesizer.BuildRules()` promotes business predicates only through `contains-condition`;
- downgraded facts use `observes-predicate` and remain in Evidence;
- `EvidenceAwareKnowledgeSynthesizer` does not re-promote them;
- `ProductFeatureBuilder` aggregates workflow Rules, not Evidence;
- portable Markdown separates `Observed business rules` from `Evidence` and AI instructions prohibit silent authority upgrades.

## Non-blocking warnings

```text
W10.1 Evidence text currently says `Business predicate: ...` without printing an explicit `observed-only` label. It remains separated from Rules and did not produce a false claim.
W10.2 Queryable operation names remain in old internal safe-operation sets, but the new Queryable guard rejects them before those sets are consulted.
```

These are maintenance/clarity follow-ups, not V0.4.6 blockers.

## Exact accepted-production gates

All gates are green on exact SHA `c310e893762997f34562a6b3a62dbab2b05c0c93`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   34990080620 — PASS
pinned Loren                                34990080707 — PASS
Loren-main canary                           34990080551 — PASS
pinned Jellyfin                             34990080546 — PASS
```

Core CI:

```text
Release build:             0 warnings / 0 errors
C# tests:                  88 / 88 PASS
frontend tests:            13 / 13 PASS
tool pack/install:         PASS
WorkPlay:                  PASS
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
digest:                  sha256:8664945310d5fd0da3a0c838b001cc5fa343174410dc1ac6d05b1335e41b0257
size:                    9,159,880 bytes
```

## Next thread

V0.4.7 is now current. Before coding, define its concrete PO-question-readiness acceptance questions and regression gates.

The planned focus is cross-layer semantic chaining, especially:

```text
source value
→ copy / derivation / snapshot
→ persisted or returned value
→ later mutation / override
→ backend condition / API result
→ frontend visibility / behavior
→ PO-facing explanation with provenance
```

Cover cross-entity value lineage, snapshot vs dynamic/reference semantics, later overrides/mutation causality, frontend visibility predicates and backend→frontend composition. Preserve every accepted V0.4.6 authority/evidence-retention guardrail.

Do not begin Azure DevOps ingestion yet:

```text
V0.4.7 CURRENT / NEXT MILESTONE
V0.5 LOCKED
```
