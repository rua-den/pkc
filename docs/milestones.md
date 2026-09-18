# PKC Milestones

Last updated: 2026-09-19

PKC is a Product/System Knowledge Compiler. Milestones are accepted only when the generated portable knowledge answers the named Product Owner questions with deterministic evidence and appropriate uncertainty.

## Permanent delivery rules

Every milestone follows:

```text
question / acceptance boundary
→ regression-first fixture
→ deterministic implementation
→ focused verification
→ full relevant local/clean verification
→ diff review
→ coherent commit/push
→ exact-SHA cross-benchmark gates
→ independent review
```

Do not use CI as the normal edit/test loop. Do not advance while a predecessor checkpoint is red or under review.

Keep these knowledge classes distinct:

```text
business conditions
value lineage / provenance
mutation / causality
```

Conservative authority downgrade must retain independently proven lower-authority evidence.

## Accepted V0.4.x checkpoints

### V0.4.4 — Loren knowledge readiness — PASS / COMPLETE

Established usable PO-facing knowledge on Loren and the knowledge-only handoff/readiness bar.

### V0.4.5 — real-repository generalization — PASS / COMPLETE

Established independent generalization and portable parity/no-leak gates, including pinned Jellyfin.

### V0.4.6 — business logic reconstruction — PASS / COMPLETE

Accepted production:

```text
c310e893762997f34562a6b3a62dbab2b05c0c93
fix: fail closed on Queryable authority
```

Final review: `docs/reviews/2026-09-15-v0.4.6-independent-rereview-10.md`.

## V0.4.7 — cross-layer PO-question readiness — CURRENT

V0.4.7 extends deterministic product knowledge through value origin, temporal semantics, computation, later change and cross-layer delivery.

Canonical value-lineage fixture begins with:

```text
ProductGroup.Price
→ Product.Price
→ Service.Price
```

No lineage may be inferred merely from matching names such as:

```text
Product.Price
Service.Price
Dto.Price
Component.price
```

### Implementation checkpoints

| Checkpoint | PO question | Current state | Completion boundary |
| --- | --- | --- | --- |
| A — Origin and copy timing | Where did this value come from? Does an upstream change alter this existing value? | **PASS / COMPLETE** | Snapshot + dynamic positives, exact identity/path/storage negatives, source traceability and PO-facing Markdown. |
| B — Computation and later change | Was it calculated? What can overwrite it? What was the last source before output? | **PASS / COMPLETE** | Portable derivation, retained original origin, later override/causality, supported direct-return terminal source and comprehensive fail-closed negatives. |
| C — Backend to API | What exact backend value supplies this response field? | **CURRENT / UNLOCKED** | Proven entity/domain → DTO/projection → API mapping with exact identities, locations and PO-facing Markdown. |
| D — API to UI | What feeds the displayed value and controls its visibility? | **LOCKED behind C** | Proven frontend binding/composition and joint backend/frontend explanation with separate authorities. |
| E — Product acceptance | Can an AI answer the agreed questions using only the knowledge pack? | **LOCKED behind D** | Blind knowledge-only review, portable folder/bundle/ZIP parity/no-leak and exact-SHA cross-benchmark gates. |

### Checkpoint A — accepted

Accepted code:

```text
09a0c7f2a058adfd7ddb7b4ac2feb2d17580a429
feat: prove bounded reference dynamic lineage
```

A proves both stored scalar snapshot and bounded reference/dynamic read-time dependency with exact project/member/receiver identity.

Exact-main A gates:

```text
CI + WorkPlay + PokeTrade   35232224202 — PASS
pinned Loren                35232224014 — PASS
Loren-main                  35232224029 — PASS
pinned Jellyfin             35232223958 — PASS
```

### Checkpoint B — accepted

Accepted final code:

```text
17fd30b3a4b8178208adabc12c40dee060bedb54
fix: fail closed after opaque terminal effects
```

B proves, inside its bounded straight-line target-project-semantic shape:

```text
multi-input stored scalar derivation
original origin retained after later change
later compile-time-constant override as separate mutation/causality
last proven source before a supported direct return
```

Knowledge-class separation is required and implemented:

```text
Value lineage → origin / derivation / terminal source
State changes  → later override / causality
Rules          → no automatic promotion from B evidence
```

B is not a general expression, alias, effect, path or persistence solver. Unsupported effects invalidate stronger suffix authority while historical deterministic evidence remains retained.

Permanent B regressions include:

- runtime derivation and runtime override;
- point-in-time snapshot behavior after input changes;
- nested assignment and deconstruction member writes;
- deconstruction reference aliases;
- compound/unary writes;
- invocation and branch/control-flow negatives;
- custom getter/setter/constructor effects;
- user-defined operator/conversion effects;
- alias/reassignment/reference-parameter negatives;
- `goto`/label/throw control-flow negatives;
- missing project-semantic context;
- candidate/synthesis/Markdown delivery.

Final exact-main B gates on `17fd30b3a4b8178208adabc12c40dee060bedb54`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35379750213 — PASS
pinned Loren                                35379750276 — PASS
Loren-main canary                           35379750231 — PASS
pinned Jellyfin                             35379750251 — PASS
```

Core:

```text
Release build:       0 warnings / 0 errors
C# tests:            124 / 124 PASS
frontend tests:      13 / 13 PASS
```

Pinned Jellyfin portable gate:

```text
facts:               43,365
relations:           195,316
workflow candidates: 386
product features:    116
Markdown files:      504
project-semantic:    43,365 / 43,365
portable parity:     PASS
raw .pkc/src leak:   none
artifact id:         10561683704
artifact digest:     sha256:7d148704af898f82ecfd23d23e3ceeca6936a36da074a225aa4bafd95c042827
artifact size:       9,162,486 bytes
```

Final review: `docs/reviews/2026-09-19-v0.4.7-b-final-rereview.md` — PASS / COMPLETE.

### Checkpoint C — CURRENT / regression-first

C must answer:

> What backend value supplies this response field, and how did it travel through domain/entity state → DTO/projection → API output?

First supported shape must prove exact target-project semantic lineage equivalent to:

```text
DomainEntity.Price
→ ResponseDto.DisplayPrice
→ API response
```

Renamed properties are valid only when explicit assignment/projection semantics prove the mapping.

Required evidence/provenance:

```text
source project/type/member identity
source occurrence + source location
target DTO/projection type/member identity
target occurrence + projection location
endpoint/API response identity + return location
project-semantic analysis mode
```

Required output:

```text
scanner facts
→ endpoint candidate retention
→ knowledge synthesis
→ PO-facing workflow Markdown with source traceability
```

Blocking negatives:

- unrelated same-name entity/DTO members;
- cross-project/assembly same-name collision;
- custom/non-auto source or target accessors without proven semantics;
- user-defined conversions;
- reference alias/reassignment ambiguity;
- opaque effects between projection and response;
- unsupported control flow/path ambiguity;
- missing target-project semantic context.

PokeTrade is the known-answer real project. Do not change PokeTrade merely to fit the analyzer. Use focused fixtures for explicit DTO and renamed-property cases when the known-answer project does not naturally contain them.

C completion requires focused tests, full relevant suite, Release build, PokeTrade, pinned Loren, Loren-main, pinned Jellyfin, portable parity/no-leak and independent rereview on the exact code SHA.

### Checkpoint D — locked

D proves API response → frontend result/binding/composition and composes backend/frontend conditions only for the same proven item path. Authorities remain separate.

### Checkpoint E — locked

E is the final knowledge-only PO acceptance and portable transport/parity gate for V0.4.7.

## V0.5 — Azure DevOps input evidence — LOCKED

Azure DevOps will add requirement intent, Epic/Feature/PBI history, status and traceability as another compiler input. It must not compensate for missing code-derived behavior.

V0.5 starts only after V0.4.7 and the V0.4.x PO-question-readiness exit gate pass.

## V0.6 — Incremental compilation

Use file/symbol hashes and dependency impact to rebuild only affected evidence and knowledge. Add PR/CI knowledge diffs.

## V0.7 — Runtime UI exploration

Use browser automation to confirm actual user-visible flows and states.

## V0.8 — Product insight

Surface gaps, inconsistencies, requirement-vs-implementation drift and improvement opportunities with evidence.

## Version semantics

Roadmap, package and serialized schema versions are independent. Current package remains:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

Do not mechanically bump package or schema identifiers when a roadmap checkpoint advances.
