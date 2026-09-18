# PKC Status

Last updated: 2026-09-18

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                 PASS / COMPLETE
V0.4.5 real-repository generalization            PASS / COMPLETE
V0.4.6 business logic reconstruction             PASS / COMPLETE
V0.4.7-A origin and copy timing                  PASS / COMPLETE
V0.4.7-B computation and later change            REVIEW BLOCKER FIXED LOCALLY / REMOTE GATES REQUIRED
V0.4.7-C backend to API                          LOCKED behind repaired B acceptance
V0.4.7-D API to UI                               LOCKED behind C
V0.4.7-E product acceptance                      LOCKED behind D
V0.5 Azure DevOps input evidence                 LOCKED
```

V0.4.7 acceptance is defined in `docs/v0.4.7-acceptance-plan.md`.

## Closed V0.4.6 baseline

Accepted V0.4.6 production remains exactly:

```text
c310e893762997f34562a6b3a62dbab2b05c0c93
fix: fail closed on Queryable authority
```

Final independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-10.md
PASS / COMPLETE
```

Do not reopen V0.4.6 without a new compile-valid and behavior-valid contradiction.

Permanent product contract: `docs/product-knowledge-contract.md`.

Keep distinct and retain:

```text
business conditions
value lineage / provenance
mutation / causality
```

Conservative authority downgrade must not erase deterministic lower-authority evidence.

## V0.4.7-A — PASS / COMPLETE

Accepted A implementation checkpoint:

```text
09a0c7f2a058adfd7ddb7b4ac2feb2d17580a429
feat: prove bounded reference dynamic lineage
```

A proves both required temporal shapes through scanner → candidate → synthesis → workflow Markdown:

```text
stored scalar snapshot
ProductGroup.Price → Product.Price → Service.Price

reference / dynamic read-time dependency
ProductGroup.Price → Service.CurrentGroupPrice
```

The snapshot boundary fails closed for receiver reassignment, opaque mutation, alias writes, cast/`as` aliases and user-defined conversion aliases. The dynamic fixture proves an upstream `100 → 120` change is observed by a later downstream read without another scalar copy.

Exact-main A gates:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35232224202 — PASS
pinned Loren                                35232224014 — PASS
Loren-main canary                           35232224029 — PASS
pinned Jellyfin                             35232223958 — PASS
```

A is closed. Do not reopen it without a new compile-valid and behavior-valid contradiction.

## V0.4.7-B — review blocker fixed locally, acceptance pending

Exact implementation checkpoint on `main`:

```text
e5b0d47c82b6db99f5184292730919421a6d2a06
feat: prove V0.4.7-B computation and causality
```

Delivery/review note:

```text
docs/reviews/2026-09-17-v0.4.7-b-computation-causality-delivery.md
docs/reviews/2026-09-18-v0.4.7-b-independent-review.md
```

B currently proves the bounded straight-line target-project-semantic shape required by R7.5–R7.7:

```text
multi-input stored scalar derivation
service.Price + service.Discount → service.NetPrice

original origin retained
ProductGroup.Price → Product.Price → Service.Price

later constant write after a proven value
→ separate mutation/causality fact with causalRole=override

supported direct return
→ last proven source immediately before return
```

Generated PO-facing knowledge keeps the classes separate:

- derivation and terminal-source proof render in `Value lineage`;
- later override renders in `State changes`;
- original origin facts remain intact after the override;
- none of the B facts become authoritative business Rules merely because the implementation proof is strong.

### B fail-closed boundary

The implementation rejects stronger B claims for unsupported shapes including:

- branch/loop/try/conditional control flow;
- opaque invocations;
- reference-type parameters;
- non-fresh aliases or reference reassignment;
- custom/non-auto scalar accessors;
- unsupported expression operations;
- unsupported compound writes such as `+=` to modeled scalar state;
- unary scalar writes such as `++` / `--` after a proven value;
- unresolved target-project semantic context.

The compound/unary negatives permanently guard against stale terminal-source authority after an unmodeled write.

This is intentionally not a general expression, alias, effect or dataflow solver.

### Exact-main verification for B

All final gates passed on exact code SHA `e5b0d47c82b6db99f5184292730919421a6d2a06`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35339292475 — PASS
pinned Loren                                35339292546 — PASS
Loren-main canary                           35339292501 — PASS
pinned Jellyfin                             35339292495 — PASS
```

Core CI:

```text
Release build:       0 warnings / 0 errors
C# tests:            114 / 114 PASS
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
artifact id:             10544800985
artifact digest:         sha256:c9884964e48daae5f3b511daf361dd2d0ceb25a06ba6504ca53b041c276106cc
artifact size:           9,162,486 bytes
```

Independent review found a compile-valid/runtime-valid stale terminal-source contradiction after nested and deconstruction writes. The bounded repair is committed locally at `30e87df`, followed by point-in-time derivation coverage at `76d9bb1`; Release build, 117 C# tests and 13 frontend tests pass locally. The earlier remote results below apply to `e5b0d47...`, not to the repaired branch. B remains open and C remains locked until the repair is integrated and exact-SHA remote gates pass.

## Independent review result for B

Astra reviewed exact current `main` and recorded the result in `docs/reviews/2026-09-18-v0.4.7-b-independent-review.md`.

Primary review targets:

1. false derivation from unsupported/custom operators or accessors;
2. whether original origin survives a later override without being rewritten;
3. stale terminal-source state after an unmodeled write/effect;
4. receiver/member/project identity collisions;
5. mutation/causality accidentally promoted into business Rules;
6. terminal-source wording overclaiming persistence/general dataflow beyond the supported direct-return shape;
7. any compile-valid, runtime-valid counterexample inside the documented straight-line proof boundary.

Verdict: **FAIL on `e5b0d47...`; repaired branch through `76d9bb1` is locally green.** Integrate both local commits, run exact-SHA CI/cross-benchmark/portable gates, then rereview only the repaired boundary. If those gates pass without a new contradiction, close B and unlock C.

## Version semantics

These are independent domains:

```text
roadmap milestone
tool/package version
evidence/schema version
```

Current values remain:

```text
roadmap:             V0.4.7-B review pending
tool/package:        RuaDen.Pkc.Tool 0.4.3-preview.2
C# raw schema:       0.4.4-csharp-raw
merged facts schema: 0.4.4
cross-stack candidate schema: 0.4.6
frontend schema:     0.4.3-frontend
```

No package or schema bump is implied by B.

## V0.4.6 warnings carried forward

```text
W10.1
Observed-only predicate Evidence is separated from Rules but does not explicitly render the `observed-only` label.

W10.2
Queryable names remain in old safe-operation sets but are unreachable behind the Queryable fail-closed guard.
```

These remain non-blocking unless touched scope makes a regression-safe cleanup coherent.

## Exact next action

**Do not code V0.4.7-C yet.** Integrate repair commits `30e87df` and `76d9bb1`, then run exact-SHA CI, cross-benchmark and portable parity/no-leak gates. Rereview only the repaired terminal-source boundary. Close B and start C regression-first only after those gates pass without a new contradiction.

```text
V0.4.7-C/D/E LOCKED until their predecessor checkpoint passes review
V0.5 LOCKED until the V0.4.7 / V0.4.x PO-question-readiness exit gate passes
```
