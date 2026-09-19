# PKC Status

Last updated: 2026-09-19

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                 PASS / COMPLETE
V0.4.5 real-repository generalization            PASS / COMPLETE
V0.4.6 business logic reconstruction             PASS / COMPLETE
V0.4.7-A origin and copy timing                  PASS / COMPLETE
V0.4.7-B computation and later change            PASS / COMPLETE
V0.4.7-C backend to API                          PASS / COMPLETE
V0.4.7-D API to UI                               CURRENT / UNLOCKED
V0.4.7-E product acceptance                      LOCKED behind D
V0.5 Azure DevOps input evidence                 LOCKED
```

V0.4.7 acceptance is defined in `docs/v0.4.7-acceptance-plan.md`.
The permanent product contract is `docs/product-knowledge-contract.md`.

## Closed baselines

Accepted V0.4.6 production remains:

```text
c310e893762997f34562a6b3a62dbab2b05c0c93
fix: fail closed on Queryable authority
```

Accepted V0.4.7-A code:

```text
09a0c7f2a058adfd7ddb7b4ac2feb2d17580a429
feat: prove bounded reference dynamic lineage
```

Accepted V0.4.7-B code:

```text
17fd30b3a4b8178208adabc12c40dee060bedb54
fix: fail closed after opaque terminal effects
```

Accepted V0.4.7-C production code:

```text
fbb64b9917da1f63362558355201ff7998384ba0
feat: prove backend API projection lineage
```

Final C rereview: `docs/reviews/2026-09-19-v0.4.7-c-final-rereview.md`.

## Permanent product-knowledge invariants

Keep these knowledge classes distinct and retain deterministic lower-authority evidence when stronger authority fails:

```text
business conditions
value lineage / provenance
mutation / causality
```

Unsupported inference must fail closed rather than produce a false PO-facing claim.

## V0.4.7-C — PASS / COMPLETE

C answers, inside its documented bounded target-project-semantic shape:

> What exact backend entity/domain property supplies this DTO/projection property and API response field?

Accepted proof chain:

```text
backend entity/domain scalar auto-property
→ explicit DTO/response scalar auto-property assignment
→ direct API response object initializer
```

The focused positive proves a renamed mapping equivalent to:

```text
ProductEntity.Price
→ PriceResponse.DisplayPrice
→ API response
```

The mapping is emitted only from exact semantic assignment evidence. Name similarity is never sufficient.

### C evidence retained

The accepted C evidence retains at least:

- target semantic project identity;
- source and target assembly/type/member identity;
- source receiver and occurrence;
- source member declaration location;
- target DTO member declaration location;
- projection assignment location;
- endpoint/API response location;
- `analysisMode=project-semantic` and explicit proof boundary;
- candidate retention, knowledge synthesis and PO-facing workflow Markdown traceability.

C emits separate `value-transfer` projection evidence and `value-terminal-source` API-response-field evidence. Failure to prove C composition leaves existing lower-authority lineage/causality evidence intact.

### Permanent C fail-closed regressions

Coverage now includes:

- unrelated same-name DTO/entity members without an explicit mapping;
- same-name namespace collisions;
- **same full type/member name across different assemblies**, compile-valid through `extern alias`;
- custom/non-auto source accessors;
- custom/non-auto target accessors;
- user-defined conversion in the projection path;
- reference alias ambiguity;
- opaque invocation/effect before the response;
- custom response constructor effects;
- unsupported control-flow/path shapes through the generic C preflight;
- missing target-project semantic context.

PokeTrade was not modified to manufacture a DTO shape; the explicit renamed-property proof lives in a focused fixture and PokeTrade remains a real-project regression gate.

### Exact-main verification for final C

All required exact-main gates passed on `fbb64b9917da1f63362558355201ff7998384ba0`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35416169067 — PASS
pinned Loren                                35416169091 — PASS
Loren-main canary                           35416169051 — PASS
pinned Jellyfin                             35416169039 — PASS
```

Core CI:

```text
Release build:       0 warnings / 0 errors
C# tests:            136 / 136 PASS
frontend tests:      13 / 13 PASS
tool pack/install:   PASS
WorkPlay:            PASS
PokeTrade:           PASS
```

Pinned Jellyfin:

```text
source commit:           1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
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
artifact id:             10575663250
artifact digest:         sha256:857d33020332f4a68a69177b6809136ebe24b9ea86b5576efc8dbc69d8266347
artifact size:           9,162,486 bytes
```

Independent rereview found no remaining compile-valid/runtime-valid contradiction inside C's documented supported boundary. C is closed.

## V0.4.7-D — CURRENT / UNLOCKED

D owns the next segment of the cross-layer chain:

```text
API response/result
→ frontend HTTP result/data
→ component/view-model state
→ rendered/displayed value
```

D must prove exact backend/frontend linkage and source locations, not normalized-name similarity. It also owns the supported joint backend/frontend visibility explanation for the **same proven item/dataflow path**, while keeping backend business-condition authority and frontend visibility evidence separate.

Start D regression-first. Do not start E until D passes focused/full/cross-benchmark gates and independent rereview.

## Version semantics

Roadmap, package and serialized schema versions remain independent:

```text
roadmap:             V0.4.7-D current
tool/package:        RuaDen.Pkc.Tool 0.4.3-preview.2
C# raw schema:       0.4.4-csharp-raw
merged facts schema: 0.4.4
cross-stack schema:  0.4.6
frontend schema:     0.4.3-frontend
```

No package or schema bump is implied by closing C.

## V0.4.6 warnings carried forward

```text
W10.1
Observed-only predicate Evidence is separated from Rules but does not explicitly render the `observed-only` label.

W10.2
Queryable names remain in old safe-operation sets but are unreachable behind the Queryable fail-closed guard.
```

These remain non-blocking unless touched scope makes a regression-safe cleanup coherent.

## Exact next action

Implement **V0.4.7-D only**, regression-first, beginning with a focused API response/result → frontend assignment/binding fixture with exact endpoint/result/component identities and source locations. Add collision/ambiguity/fallback negatives before advancing to joint visibility composition.

```text
V0.4.7-E LOCKED until D passes
V0.5 LOCKED until the V0.4.7 / V0.4.x PO-question-readiness exit gate passes
```
