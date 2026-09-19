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
| C — Backend to API | What exact backend value supplies this response field? | **PASS / COMPLETE** | Proven entity/domain → DTO/projection → API mapping with exact project/assembly/member identities, locations, PO-facing Markdown and collision/effect fail-closed coverage. |
| D — API to UI | What feeds the displayed value and controls its visibility? | **CURRENT / UNLOCKED** | Proven API result → frontend binding/composition and joint backend/frontend explanation with separate authorities. |
| E — Product acceptance | Can an AI answer the agreed questions using only the knowledge pack? | **LOCKED behind D** | Blind knowledge-only review, portable folder/bundle/ZIP parity/no-leak and exact-SHA cross-benchmark gates. |

### Checkpoint A — accepted

Accepted code:

```text
09a0c7f2a058adfd7ddb7b4ac2feb2d17580a429
feat: prove bounded reference dynamic lineage
```

A proves both stored scalar snapshot and bounded reference/dynamic read-time dependency with exact project/member/receiver identity.

### Checkpoint B — accepted

Accepted code:

```text
17fd30b3a4b8178208adabc12c40dee060bedb54
fix: fail closed after opaque terminal effects
```

B proves supported multi-input scalar derivation, original origin retention, later override/mutation causality and last proven source before a supported direct return. It is not a general expression, alias, effect, path or persistence solver.

### Checkpoint C — accepted

Accepted production code:

```text
fbb64b9917da1f63362558355201ff7998384ba0
feat: prove backend API projection lineage
```

C proves a bounded target-project-semantic chain equivalent to:

```text
DomainEntity.Price
→ ResponseDto.DisplayPrice
→ API response
```

Renamed properties are supported only when explicit semantic assignment proves the mapping.

Required provenance is retained:

```text
semantic project identity
source assembly/type/member identity
source receiver/occurrence + source declaration location
target DTO assembly/type/member identity
target occurrence + declaration/projection location
endpoint/API response identity + return location
analysis mode/confidence/proof boundary
```

The proven chain reaches scanner facts → endpoint candidate → knowledge synthesis → PO-facing workflow Markdown with source traceability.

Permanent C fail-closed regressions include:

- unrelated same-name members;
- same-name namespace collisions;
- same full type/member identity across different assemblies through a compile-valid `extern alias` fixture;
- custom source/target accessors;
- user-defined conversions;
- reference alias ambiguity;
- opaque invocation/effects;
- custom response construction;
- unsupported control-flow/path shapes;
- missing target-project semantic context.

PokeTrade remained unmodified and served as the known-answer real-project regression gate.

Final exact-main C gates on `fbb64b9917da1f63362558355201ff7998384ba0`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35416169067 — PASS
pinned Loren                                35416169091 — PASS
Loren-main canary                           35416169051 — PASS
pinned Jellyfin                             35416169039 — PASS
```

Core:

```text
Release build:       0 warnings / 0 errors
C# tests:            136 / 136 PASS
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
artifact id:         10575663250
artifact digest:     sha256:857d33020332f4a68a69177b6809136ebe24b9ea86b5576efc8dbc69d8266347
artifact size:       9,162,486 bytes
```

Final review: `docs/reviews/2026-09-19-v0.4.7-c-final-rereview.md` — PASS / COMPLETE.

### Checkpoint D — CURRENT / regression-first

D must answer:

> What frontend state/display does this proven API field feed, and what backend + frontend conditions jointly determine visibility for the same proven item path?

The first supported D shape must prove exact dataflow equivalent to:

```text
API response field
→ frontend HTTP/API result
→ component/view-model property
→ rendered/displayed value
```

Required properties:

- exact backend endpoint/response field identity from proven evidence;
- exact frontend call/result/assignment identity and source locations;
- no normalized-name or property-name join;
- candidate/synthesis/PO-facing Markdown delivery;
- conservative failure when receiver/result/binding identity is ambiguous or only fallback text is available;
- retain independently proven backend C lineage when D composition cannot be established.

After binding identity is proven, add supported joint visibility for the same proven item/dataflow path. Backend business-condition authority and frontend visibility/filter evidence remain separate classes.

D completion requires focused D regressions, full relevant C#/frontend suites, Release build, PokeTrade, pinned Loren, Loren-main, pinned Jellyfin, portable parity/no-leak and independent rereview on the exact code SHA.

### Checkpoint E — locked

E is the final knowledge-only PO acceptance and portable transport/parity gate for V0.4.7.

## V0.5 — Azure DevOps input evidence — LOCKED

Azure DevOps will add requirement intent, Epic/Feature/PBI history, status and traceability as another compiler input. It must not compensate for missing code-derived behavior.

V0.5 starts only after V0.4.7 and the V0.4.x PO-question-readiness exit gate pass.

## Later milestones

- V0.6 — incremental compilation and knowledge diffs.
- V0.7 — runtime UI exploration/confirmation.
- V0.8 — product insight, gaps and requirement-vs-implementation drift.

## Version semantics

Roadmap, package and serialized schema versions are independent. Current package remains:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

Do not mechanically bump package or schema identifiers when a roadmap checkpoint advances.
