# PKC Status

Last updated: 2026-09-17

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                 PASS / COMPLETE
V0.4.5 real-repository generalization            PASS / COMPLETE
V0.4.6 business logic reconstruction             PASS / COMPLETE
V0.4.7 cross-layer PO-question readiness         IN PROGRESS
V0.5 Azure DevOps input evidence                 LOCKED
```

V0.4.7 acceptance is defined in `docs/v0.4.7-acceptance-plan.md`.

Accepted V0.4.6 production remains:

```text
c310e893762997f34562a6b3a62dbab2b05c0c93
fix: fail closed on Queryable authority
```

Final independent V0.4.6 review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-10.md
verdict: PASS / COMPLETE
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

Accepted B6.4 hardening includes discarded/local predicate downgrade, transformed/polarity-changing return-context fail-closed behavior, arbitrary `Select` rejection and identity projection proof, complete supported predicate-dependency proof, safe direct defensive-clone member copies, custom setter/initializer effect fail-closed behavior, callback/comparer-bearing ordering/equality fail-closed behavior, audited callback-free Enumerable preservation, inert-constructor requirements, Queryable observed-only behavior without provider-semantics proof, Enumerable authority loss after a Queryable hop, and retained downgraded evidence through `observes-predicate`.

## V0.4.7-A snapshot checkpoint

The receiver-reassignment and opaque-helper repair passes its focused regressions. A 2026-09-17 independent review found a new runtime-valid **P1 alias-write composition blocker**; snapshot composition is not yet green for that boundary. See `docs/reviews/2026-09-17-v0.4.7-alias-composition-review.md` and its adjacent reproduction patch.

Reviewed code checkpoint (existing gates passed; new alias regression is RED):

```text
bc938823b46802a4d2c32300a1b6de692f5866ad
fix: invalidate stale lineage composition
```

Relevant implementation history:

```text
350ba2468e0d1b011936695fc48c1a970650d647  feat: prove scalar snapshot value lineage
0bd5f823b7ebdfa8d0a54ff25d079d2056be4a81  fix: compile scalar lineage proof
82fac01e669bce0a35dafde80f54c8f0baa595e5  fix: compile lineage regressions
fd5d8311c38b0463f54ad8f45d12222ea5d24a8b  docs: record stale lineage review blocker
f35901692060e511461a28fa94a1abe5400ce0bd  noop (zero-tree-diff connector commit)
bc938823b46802a4d2c32300a1b6de692f5866ad  fix: invalidate stale lineage composition
```

`f359016` changes no repository content; it is retained in history rather than force-resetting `main`.

The snapshot slice proves the bounded supported shape:

```text
ProductGroup.Price
→ Product.Price
→ Service.Price
```

as deterministic direct scalar auto-property copies with stored snapshot timing. Generated workflow knowledge can explain that changing `ProductGroup.Price` later does not retroactively change already stored `Product.Price` / `Service.Price` copies.

The implementation keeps value lineage separate from business Rules and mutation semantics and renders a dedicated `Value lineage` section.

### Closed stale-predecessor blocker

Review artifact:

```text
docs/reviews/2026-09-16-v0.4.7-snapshot-checkpoint-review.md
docs/reviews/2026-09-16-v0.4.7-snapshot-stale-lineage-repro.patch
```

The review proved two false-chain cases against `82fac01e...`:

```text
receiver reassignment between copies
opaque helper mutation between copies
```

`bc938823...` fixes the generic composition boundary by processing relevant top-level statements in execution order and conservatively invalidating predecessor state when a tracked receiver is reassigned, an opaque invocation may mutate tracked state, or a tracked unary write changes the value. Independently valid direct transfer evidence is retained; only unsupported chain composition is blocked. No general alias/path solver was added.

Permanent focused regressions now cover both runtime-valid counterexamples and require the later direct transfer to remain observable while the stale predecessor link and false proven-chain prose are absent.

Existing snapshot coverage remains green for:

- executable canonical snapshot behavior;
- exact source/target occurrence and semantic identity metadata;
- valid two-edge chain composition through the same stored value version;
- unrelated same-name collision fail-closed behavior;
- distinct receiver identities;
- intervening overwrite invalidation;
- incompatible branches;
- custom setters;
- shared mutable-reference content;
- unresolved/no-project semantic context;
- cross-project member identity;
- candidate → synthesis → Markdown delivery;
- lineage/business-rule separation.

### A is still NOT complete

The acceptance plan requires a separate positive proof for:

```text
reference / dynamic-read semantics
```

Current disposition:

```text
V0.4.7-A snapshot composition       BLOCKED: alias-write predecessor invalidation
V0.4.7-A dynamic/reference slice    NEXT FEATURE / after bounded repair
V0.4.7-A overall                    IN PROGRESS
V0.4.7-B/C/D/E                      LOCKED behind A
V0.5                                LOCKED
```

Do not begin B/C/D until A's snapshot + dynamic/reference + portable delivery gates are green.

## Exact-SHA verification for stale-composition repair

Previously recorded checkpoint gates passed on exact code SHA `bc938823b46802a4d2c32300a1b6de692f5866ad`. These gates did not include the new alias-write counterexample:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35128897392 — PASS
pinned Loren                                35128897399 — PASS
Loren-main canary                           35128897380 — PASS
pinned Jellyfin                             35128897797 — PASS
```

Core CI:

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

## V0.4.6 warnings carried forward

```text
W10.1
Observed-only predicate Evidence is separated from Rules but does not explicitly render the `observed-only` label.

W10.2
Queryable names remain in old safe-operation sets but are unreachable behind the Queryable fail-closed guard.
```

These remain non-blocking V0.4.7 considerations. Do not reopen V0.4.6 for them.

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

Do not mechanically bump package or schema versions because the roadmap milestone advances.

## Exact next action

Stay in **V0.4.7-A**. First close the reproduced alias-write composition gap described in the 2026-09-17 review, retaining valid immediate copies and the six existing lineage regressions. Then implement the already agreed **reference/dynamic-read positive** regression-first: upstream `100 → 120`, downstream read-time property observes `120`.

Use a compile-valid executable supported shape where a downstream property resolves an upstream property at read time, mutate the upstream value, then prove a later downstream read observes the new value without another scalar copy. The analysis must prove receiver/member identity and the observable dependency through scanner → candidate → synthesis → workflow Markdown.

The generated PO answer must clearly distinguish:

```text
stored scalar snapshot
vs
reference/dynamic read-time dependency
```

Unsupported aliasing, receiver reassignment, opaque effects, custom getter behavior outside the modeled shape, branch ambiguity, or unresolved semantic context must fail closed rather than guess. Preserve every snapshot regression, including the two stale-predecessor counterexamples now fixed.

Do not broaden into a general alias/path solver. Do not begin V0.4.7-B/C/D until both A positives and A's portable delivery/identity negatives are green.

```text
V0.4.7 IN PROGRESS
V0.5 LOCKED until the V0.4.7 / V0.4.x PO-question-readiness exit gate passes
```
