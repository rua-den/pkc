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

Current snapshot-slice code checkpoint:

```text
82fac01e669bce0a35dafde80f54c8f0baa595e5
fix: compile lineage regressions
```

Implementation ancestry:

```text
350ba2468e0d1b011936695fc48c1a970650d647  feat: prove scalar snapshot value lineage
0bd5f823b7ebdfa8d0a54ff25d079d2056be4a81  fix: compile scalar lineage proof
82fac01e669bce0a35dafde80f54c8f0baa595e5  fix: compile lineage regressions
```

This is a coherent **first snapshot slice within checkpoint A**. It is not A-complete and not V0.4.7-complete.

## What the snapshot slice now proves

Canonical supported fixture:

```text
ProductGroup.Price
→ Product.Price
→ Service.Price
```

For direct scalar auto-property assignment in a proven project semantic context, PKC now emits deterministic `value-transfer` evidence and renders PO-facing `Value lineage` explanations that distinguish stored snapshot copies from later upstream changes.

The supported explanation includes:

- `ProductGroup.Price → Product.Price` as a stored scalar snapshot copy;
- `Product.Price → Service.Price` as a stored scalar snapshot copy;
- proven chain composition only when the second edge reads the exact stored target identity from the first edge;
- source traceability into generated Markdown;
- an explicit explanation that changing `ProductGroup.Price` later does not retroactively update the existing stored downstream scalar copies.

Value lineage remains separate from business Rules and mutation semantics.

## Regression boundary already covered

Keep all of these green while continuing A:

1. positive executable runtime fixture matches generated snapshot knowledge;
2. unrelated same-name members do not create lineage by name matching;
3. distinct receiver identities do not compose merely because member names match;
4. intervening writes break prior chain composition;
5. incompatible branch shapes fail closed;
6. custom setter effects fail closed;
7. shared mutable-reference content is not claimed to be a scalar snapshot;
8. unresolved/no-project semantic context does not create proven lineage;
9. same display members in distinct projects retain project/assembly-qualified identity;
10. candidate → synthesis → workflow Markdown delivery is asserted;
11. lineage is not promoted into authoritative business Rules.

Do not weaken any accepted V0.4.6 authority/filtering behavior while extending lineage.

## Exact-SHA final verification

All final verification gates passed on exact code SHA `82fac01e669bce0a35dafde80f54c8f0baa595e5`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35113764215 — PASS
pinned Loren                                35113764207 — PASS
Loren-main canary                           35113764095 — PASS
pinned Jellyfin                             35113764184 — PASS
```

Core verification:

```text
Release build:       0 warnings / 0 errors
C# tests:            92 / 92 PASS
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
artifact id:             10454295890
artifact digest:         sha256:2ac8778a3366767e344c0bf4bdc2403037b900eddb338a89c03d0cf5d77554cc
artifact size:           9,162,455 bytes
```

## Current checkpoint disposition

```text
V0.4.7-A snapshot slice             GREEN
V0.4.7-A dynamic/reference slice    NEXT / NOT IMPLEMENTED
V0.4.7-A overall                    IN PROGRESS
V0.4.7-B/C/D/E                      LOCKED behind A
V0.5                                LOCKED
```

Do not call checkpoint A complete from the snapshot slice alone.

The acceptance plan requires a separate positive proof for **reference/dynamic-read semantics** before A can close.

## Exact next implementation action

Stay in V0.4.7-A and implement the dynamic/reference positive regression-first.

Required shape:

- compile-valid and executable;
- demonstrates a value whose later upstream/source mutation is observed dynamically rather than preserved as a stored scalar snapshot;
- proves receiver/member identity and the observable read path deterministically;
- survives scanner → candidate → synthesis → workflow Markdown;
- generated PO answer must clearly distinguish dynamic/reference behavior from the already supported snapshot behavior;
- unsupported aliasing, setter effects, branch ambiguity or unresolved semantic context must fail closed rather than guessing.

Keep scope bounded. Do not introduce a general alias/path solver merely to broaden the fixture.

Do not begin B/C/D until both A positives — snapshot and dynamic/reference — plus A's portable delivery/identity negatives are green.

## Review instruction for Astra

If Astra is acting as reviewer, review exact code checkpoint:

```text
82fac01e669bce0a35dafde80f54c8f0baa595e5
```

Review this as **V0.4.7-A snapshot-slice readiness**, not as acceptance of all A or all V0.4.7.

Primary review questions:

1. Is the emitted snapshot claim deterministically justified by the assignment/storage proof?
2. Can any compile-valid counterexample make the current `copy` / `snapshot` / chain-composition claim false?
3. Are receiver/project/member identities strong enough to prevent same-name or cross-project false joins?
4. Are custom setters, shared mutable references, control-flow ambiguity and intervening writes conservatively rejected?
5. Does the generated portable Markdown answer the PO question without conflating value lineage with business Rules or mutation causality?
6. Does any new evidence leak benchmark-specific special casing?

If a concrete blocker is found, add a focused regression first and fix the generic boundary. Otherwise leave the snapshot slice closed and continue only with A's dynamic/reference slice.

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

These remain non-blocking V0.4.7 considerations:

```text
W10.1
Observed-only predicate Evidence is separated from Rules but does not explicitly render the `observed-only` label.

W10.2
Queryable names remain in old safe-operation sets but are unreachable behind the Queryable fail-closed guard.
```

Only address them when the touched V0.4.7 scope makes doing so coherent and regression-safe.

Do not start Azure DevOps ingestion:

```text
V0.5 LOCKED until the V0.4.7 / V0.4.x PO-question-readiness exit gate passes
```
