# PKC Status

Last updated: 2026-09-19

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                 PASS / COMPLETE
V0.4.5 real-repository generalization            PASS / COMPLETE
V0.4.6 business logic reconstruction             PASS / COMPLETE
V0.4.7-A origin and copy timing                  PASS / COMPLETE
V0.4.7-B computation and later change            PASS / COMPLETE
V0.4.7-C backend to API                          CURRENT / UNLOCKED
V0.4.7-D API to UI                               LOCKED behind C
V0.4.7-E product acceptance                      LOCKED behind D
V0.5 Azure DevOps input evidence                 LOCKED
```

V0.4.7 acceptance is defined in `docs/v0.4.7-acceptance-plan.md`.
The permanent product contract is `docs/product-knowledge-contract.md`.

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

## Permanent product-knowledge invariants

Keep these knowledge classes distinct and retain deterministic lower-authority evidence when stronger authority fails:

```text
business conditions
value lineage / provenance
mutation / causality
```

Unsupported inference must fail closed rather than produce a false PO-facing claim.

## V0.4.7-A — PASS / COMPLETE

Accepted A implementation checkpoint:

```text
09a0c7f2a058adfd7ddb7b4ac2feb2d17580a429
feat: prove bounded reference dynamic lineage
```

A proves the bounded supported forms of:

```text
stored scalar snapshot
ProductGroup.Price → Product.Price → Service.Price

reference / dynamic read-time dependency
ProductGroup.Price → Service.CurrentGroupPrice
```

Exact-main A gates:

```text
CI + WorkPlay + PokeTrade   35232224202 — PASS
pinned Loren                35232224014 — PASS
Loren-main                  35232224029 — PASS
pinned Jellyfin             35232223958 — PASS
```

A is closed. Do not reopen it without a new concrete contradiction.

## V0.4.7-B — PASS / COMPLETE

Accepted final B production-code checkpoint:

```text
17fd30b3a4b8178208adabc12c40dee060bedb54
fix: fail closed after opaque terminal effects
```

Delivery/review trail:

```text
docs/reviews/2026-09-17-v0.4.7-b-computation-causality-delivery.md
docs/reviews/2026-09-18-v0.4.7-b-independent-review.md
docs/reviews/2026-09-19-v0.4.7-b-final-rereview.md
```

### What B proves

Within the documented bounded, straight-line, target-project-semantic shape, PKC distinguishes:

```text
direct copy
multi-input stored scalar derivation
later constant override / mutation causality
original origin retained separately from later change
last proven source before a supported direct return boundary
```

The generated PO-facing knowledge keeps the classes separate:

```text
Value lineage
  original origin
  stored derivation
  last proven source before return

State changes
  later override / mutation causality

Rules
  no automatic promotion from lineage/causality
```

A stored derivation is a point-in-time snapshot. Later changes to its inputs do not rewrite the stored value unless a separately proven mutation executes.

### Final B fail-closed boundary

Permanent regressions now cover stale terminal authority after or through:

- nested simple member assignment;
- top-level deconstruction member write;
- deconstruction reference reassignment and declaration aliases;
- compound and unary scalar writes;
- opaque invocation;
- custom getter/setter effects;
- custom constructor effects;
- user-defined operator effects;
- user-defined conversion effects;
- reference-type parameters, non-fresh aliases and reference reassignment;
- unsupported branch/loop/try/conditional control flow;
- `goto`, labels and throw-driven control flow;
- unresolved target-project semantic context.

The generic rule is conservative: once a current terminal value has been proven, an unsupported/opaque statement invalidates stronger terminal authority for the remaining suffix while already emitted historical derivation/origin evidence remains retained.

B intentionally does **not** claim a general expression solver, alias solver, effect solver, arbitrary path solver, or arbitrary persistence/API-call analysis. Its terminal-source acceptance is the supported direct-return boundary. Persistence beyond that remains unproven until a later explicit checkpoint requires it.

### Exact-main verification for final B

All required exact-main gates passed on `17fd30b3a4b8178208adabc12c40dee060bedb54`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35379750213 — PASS
pinned Loren                                35379750276 — PASS
Loren-main canary                           35379750231 — PASS
pinned Jellyfin                             35379750251 — PASS
```

Core CI:

```text
Release build:       0 warnings / 0 errors
C# tests:            124 / 124 PASS
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
project-semantic facts:  43,365 / 43,365
portable bundle parity:  PASS
ZIP file-set parity:     PASS
ZIP byte parity:         PASS
raw .pkc leak:           none
src/ source-tree leak:   none
artifact id:             10561683704
artifact digest:         sha256:7d148704af898f82ecfd23d23e3ceeca6936a36da074a225aa4bafd95c042827
artifact size:           9,162,486 bytes
```

Independent rereview found no remaining compile-valid/runtime-valid contradiction inside the documented B proof boundary. B is closed.

## V0.4.7-C — CURRENT / UNLOCKED

C answers:

> What exact backend entity/domain property supplies this DTO/projection property and API response field?

Start C regression-first. The first coherent C checkpoint must prove exact target-project semantic identity and source locations through:

```text
backend entity/domain property
→ explicit DTO/projection property
→ API response
```

Requirements:

- use exact symbol/project identity, not member-name similarity;
- preserve source and target locations in deterministic evidence;
- support an explicit renamed DTO property when assignment proves the edge;
- deliver the mapping through candidate/synthesis into PO-facing workflow Markdown;
- use PokeTrade as the known-answer project without modifying PokeTrade merely to fit the analyzer;
- use focused fixtures where an explicit DTO or renamed-property shape is required;
- fail closed for same-name collisions, custom accessors, user-defined conversion, alias ambiguity and opaque effects;
- retain independently proven lower-authority evidence when stronger C composition is unavailable.

Do not start D until C passes its focused/full/cross-benchmark gates and review.

## Version semantics

Roadmap, package and serialized schema versions are independent:

```text
roadmap:             V0.4.7-C current
tool/package:        RuaDen.Pkc.Tool 0.4.3-preview.2
C# raw schema:       0.4.4-csharp-raw
merged facts schema: 0.4.4
cross-stack schema:  0.4.6
frontend schema:     0.4.3-frontend
```

No package or schema bump is implied by closing B.

## V0.4.6 warnings carried forward

```text
W10.1
Observed-only predicate Evidence is separated from Rules but does not explicitly render the `observed-only` label.

W10.2
Queryable names remain in old safe-operation sets but are unreachable behind the Queryable fail-closed guard.
```

These remain non-blocking unless touched scope makes a regression-safe cleanup coherent.

## Exact next action

Implement **V0.4.7-C only**, regression-first, beginning with a focused exact entity/domain property → DTO/projection property → API response fixture and negative collisions/effects. Review and validate the coherent C checkpoint locally/clean-environment before one final push.

```text
V0.4.7-D/E LOCKED until their predecessor checkpoint passes
V0.5 LOCKED until the V0.4.7 / V0.4.x PO-question-readiness exit gate passes
```
