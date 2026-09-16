# PKC Handoff

Use this file when continuing PKC in another coding or review thread.

## Read first

1. `docs/status.md`
2. this handoff
3. `docs/milestones.md`
4. `docs/product-knowledge-contract.md`
5. `docs/v0.4.7-acceptance-plan.md`
6. `docs/reviews/2026-09-15-v0.4.6-independent-rereview-10.md`
7. `docs/reviews/2026-09-16-v0.4.7-plan-readiness-review.md`
8. `docs/reviews/2026-09-16-v0.4.7-snapshot-checkpoint-review.md`

Before changing code, inspect `git status`, current `main` HEAD and recent commits. Work from current `main`; do not reset to an older planning or production SHA.

## Closed production baseline

Accepted V0.4.6 production remains exactly:

```text
c310e893762997f34562a6b3a62dbab2b05c0c93
fix: fail closed on Queryable authority
```

Final independent V0.4.6 review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-10.md
verdict: PASS / COMPLETE
```

Do not reopen B6.1-B6.4 without a new compile-valid and behavior-valid contradiction.

## Current milestone state

```text
V0.4.4  Loren knowledge readiness              PASS / COMPLETE
V0.4.5  Jellyfin generalization               PASS / COMPLETE
V0.4.6  business logic reconstruction         PASS / COMPLETE
V0.4.7  cross-layer PO-question readiness     IN PROGRESS
V0.5    Azure DevOps input evidence           LOCKED
```

## Permanent product contract

PKC is a deterministic Product/System Knowledge Compiler. Preserve this architecture:

```text
source inputs
→ deterministic analyzers/adapters
→ evidence/facts
→ feature/workflow/business-decision candidates
→ knowledge synthesis
→ canonical model
→ portable rendering
```

Keep distinct and retain:

```text
business conditions
value lineage / provenance
mutation / causality
```

Conservative business-rule downgrade must not erase deterministic lower-authority lineage, mutation or causal evidence.

## Current V0.4.7-A code checkpoint

Latest verified implementation checkpoint:

```text
bc938823b46802a4d2c32300a1b6de692f5866ad
fix: invalidate stale lineage composition
```

Relevant history:

```text
350ba2468e0d1b011936695fc48c1a970650d647  feat: prove scalar snapshot value lineage
0bd5f823b7ebdfa8d0a54ff25d079d2056be4a81  fix: compile scalar lineage proof
82fac01e669bce0a35dafde80f54c8f0baa595e5  fix: compile lineage regressions
fd5d8311c38b0463f54ad8f45d12222ea5d24a8b  docs: record stale lineage review blocker
f35901692060e511461a28fa94a1abe5400ce0bd  noop (zero-tree-diff connector commit)
bc938823b46802a4d2c32300a1b6de692f5866ad  fix: invalidate stale lineage composition
```

`f359016` is an accidental connector-created no-op commit with the same tree as its parent. It was intentionally left in history rather than force-resetting `main`.

Checkpoint A is **not complete**. The snapshot half is green; the dynamic/reference half is still pending.

## What the snapshot slice now proves

Canonical supported fixture:

```text
ProductGroup.Price
→ Product.Price
→ Service.Price
```

For direct scalar auto-property assignment in proven project-semantic context, PKC emits deterministic `value-transfer` evidence and PO-facing `Value lineage` explanations that distinguish stored snapshot copies from later upstream changes.

The supported explanation includes:

- `ProductGroup.Price → Product.Price` as a stored scalar snapshot copy;
- `Product.Price → Service.Price` as a stored scalar snapshot copy;
- chain composition only when the second edge reads the exact still-valid stored target/value version from the first edge;
- source traceability into generated Markdown;
- an explicit explanation that changing `ProductGroup.Price` later does not retroactively update existing stored downstream scalar copies.

Value lineage remains separate from business Rules and mutation semantics.

## Closed snapshot-review blocker

The snapshot review against `82fac01e...` found a runtime-proven false-chain blocker:

```text
receiver reassignment between copies
opaque helper mutation between copies
```

Review and repro:

```text
docs/reviews/2026-09-16-v0.4.7-snapshot-checkpoint-review.md
docs/reviews/2026-09-16-v0.4.7-snapshot-stale-lineage-repro.patch
```

The repro executed both fixtures and proved that runtime values contradicted the old composed origin chain.

`bc938823...` repairs only that generic composition boundary:

- relevant top-level statements are processed in execution order;
- receiver local/parameter reassignment invalidates tracked slots for that receiver;
- an opaque invocation conservatively invalidates predecessor composition state;
- tracked unary writes invalidate their slot;
- unsupported assignment targets create a conservative barrier;
- independently valid direct transfer facts remain available even when composition is blocked;
- no general alias/path or helper-body solver was introduced.

Permanent regression coverage now includes both stale-predecessor cases. Each asserts runtime behavior, retains the immediate later direct-copy evidence, marks composition blocked, removes the predecessor link, and forbids false `Proven stored lineage chain` prose.

## Snapshot regression boundary to keep green

1. positive executable canonical snapshot fixture;
2. unrelated same-name members do not create lineage;
3. distinct receiver identities do not compose;
4. intervening writes break composition;
5. receiver reassignment breaks stale composition;
6. opaque helper mutation breaks stale composition;
7. incompatible branch shapes fail closed;
8. custom setter effects fail closed;
9. shared mutable-reference content is not a scalar snapshot;
10. unresolved/no-project semantic context does not produce proven lineage;
11. same display members in distinct projects retain project/assembly-qualified identity;
12. candidate → synthesis → workflow Markdown delivery remains proven;
13. lineage is not promoted into authoritative business Rules.

Do not weaken accepted V0.4.6 authority/filtering behavior while extending lineage.

## Exact-SHA final verification

All current checkpoint gates passed on exact code SHA `bc938823b46802a4d2c32300a1b6de692f5866ad`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35128897392 — PASS
pinned Loren                                35128897399 — PASS
Loren-main canary                           35128897380 — PASS
pinned Jellyfin                             35128897797 — PASS
```

Core:

```text
Release build:       0 warnings / 0 errors
C# tests:            94 / 94 PASS
frontend tests:      13 / 13 PASS
tool pack/install:   PASS
WorkPlay:            PASS
PokeTrade:           PASS
```

Pinned Jellyfin:

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
artifact id:             10460043943
artifact digest:         sha256:3053a33b3abdb0acd16e66f85bc83f15c405295a0c9305965aeb4734497452c3
artifact size:           9,162,455 bytes
```

## Current checkpoint disposition

```text
V0.4.7-A snapshot composition       GREEN
V0.4.7-A dynamic/reference slice    NEXT / NOT IMPLEMENTED
V0.4.7-A overall                    IN PROGRESS
V0.4.7-B/C/D/E                      LOCKED behind A
V0.5                                LOCKED
```

Do not call checkpoint A complete until the separate reference/dynamic-read positive and its portable answer pass.

## Exact next implementation action

Stay in V0.4.7-A and implement the **reference/dynamic-read positive** regression-first.

Use a narrow compile-valid executable shape where a downstream property resolves an upstream property at read time, for example an expression-bodied/otherwise explicitly modeled getter equivalent to:

```text
Service.CurrentGroupPrice → reads ProductGroup.Price dynamically
```

The fixture must mutate the upstream value after establishing the reference and prove a later downstream read observes the new upstream value without another scalar-copy assignment.

Required proof:

- exact project semantic context;
- exact receiver/member identity for the dynamic dependency;
- executable before/after behavior;
- deterministic read path rather than name similarity;
- scanner → candidate → synthesis → workflow Markdown delivery;
- PO-facing wording that says the value is resolved dynamically/read-time and contrasts it with stored snapshot behavior.

Fail closed for unsupported aliasing, receiver reassignment, opaque effects, custom getter behavior outside the modeled shape, branch ambiguity, unresolved semantics, or any dependency the compiler cannot prove. Do not add a general alias/path solver merely to support more shapes.

Keep every snapshot positive and negative above green. Do not begin B/C/D until both A positives and A's identity/portable gates are green.

## Version semantics

Do not conflate:

```text
roadmap milestone version
tool/package version
evidence/schema version
```

Current examples:

```text
roadmap milestone:        V0.4.7 IN PROGRESS
tool/package:             RuaDen.Pkc.Tool 0.4.3-preview.2
raw C# evidence schema:   0.4.4-csharp-raw
merged facts schema:      0.4.4
cross-stack candidates:   0.4.6
frontend evidence schema: 0.4.3-frontend
```

Roadmap progress does not mechanically bump package or schema versions.

## V0.4.6 warnings carried forward

```text
W10.1
Observed-only predicate Evidence is separated from Rules but does not explicitly render the `observed-only` label.

W10.2
Queryable names remain in old safe-operation sets but are unreachable behind the Queryable fail-closed guard.
```

Only address them when touched V0.4.7 scope makes doing so coherent and regression-safe.

Do not start Azure DevOps ingestion:

```text
V0.5 LOCKED until the V0.4.7 / V0.4.x PO-question-readiness exit gate passes
```
