# PKC Milestones

Last updated: 2026-09-21

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
| D — API to UI | What feeds the displayed value and controls its visibility? | **R7.9 PASS / R7.10 CURRENT** | R7.9 proves bounded API result → frontend state → rendered value; R7.10 adds joint backend/frontend visibility with separate authorities. |
| E — Product acceptance | Can an AI answer the agreed questions using only the knowledge pack? | **LOCKED behind D** | Blind knowledge-only review, real-project positive-yield gate, portable folder/bundle/ZIP parity/no-leak and exact-SHA cross-benchmark gates. |

### Checkpoint A — accepted

Accepted code:

```text
09a0c7f2a058adfd7ddb7b4ac2feb2d17580a429
feat: prove bounded reference dynamic lineage
```

A proves both stored scalar snapshot and bounded reference/dynamic read-time dependency with exact target-project identity.

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

Permanent C fail-closed regressions include unrelated same-name members, namespace collisions, same full type/member identity across assemblies, custom accessors, user-defined conversions, alias ambiguity, opaque/custom effects, unsupported control flow/path shapes and missing project semantics.

PokeTrade remained unmodified and served as the known-answer real-project regression gate.

Final exact-main C gates on `fbb64b9917da1f63362558355201ff7998384ba0`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35416169067 — PASS
pinned Loren                                35416169091 — PASS
Loren-main canary                           35416169051 — PASS
pinned Jellyfin                             35416169039 — PASS
```

### Checkpoint D / R7.9 — PASS / COMPLETE

Exact implementation/test checkpoint:

```text
fc4bfa7042f59620b2c7ba1c4f6700cf3b02172a
test: target frontend casing collision
```

R7.9 proves the first supported property-level chain only through explicit wire identity plus bounded frontend dataflow:

```text
C-proven backend response property
→ [JsonPropertyName("displayPrice")]
→ API response field
→ typed frontend HTTP result contract member
→ exact resolved service/API method inside the supported scanner boundary
→ exact subscribe result receiver/member assignment
→ component/view-model member
→ authoritative simple Angular interpolation
→ rendered UI value
```

No normalized-name, casing, route-label, DTO naming convention or camelCase/PascalCase guess may create the property-level edge. Route identity may scope endpoint ↔ API-call correlation only.

R7.9 retains backend response, wire-contract, frontend API/result member, assignment and render source locations. Failure to compose R7.9 retains accepted C evidence.

Permanent R7.9 negatives cover same/equivalent-name collisions without wire proof, unrelated services/results with the same member, unresolved/ambiguous service or result receivers, untyped/fallback-only HTTP evidence, missing explicit wire identity and non-authoritative render evidence.

All required exact-SHA gates passed on `fc4bfa7042f59620b2c7ba1c4f6700cf3b02172a`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35588410936 — PASS
pinned Loren                                35588410941 — PASS
Loren-main canary                           35588410930 — PASS
pinned Jellyfin                             35588410939 — PASS

Release build        0 warnings / 0 errors
C# tests             142 / 142 PASS
frontend tests       13 / 13 PASS
tool pack/install    PASS
```

### Checkpoint D / R7.10 — CURRENT / regression-first

R7.10 must answer, for the same already-proven R7.9 item/dataflow path:

> What backend selection/eligibility conditions and frontend visibility/filter conditions jointly determine whether this item/value is visible?

Target composition:

```text
R7.9-proven API → frontend item/value identity
+
backend business-condition evidence
+
frontend visibility/filter evidence
→ one PO-facing visibility explanation
```

Authority remains separated. Frontend evidence must never upgrade observed-only/lower-authority backend conditions, and unrelated predicates must remain disconnected. If joint composition fails, R7.9 and independently proven backend/frontend evidence remain available.

D completion requires focused R7.10 regressions, full relevant C#/frontend suites, Release build, PokeTrade, pinned Loren, Loren-main, pinned Jellyfin, portable parity/no-leak and independent rereview on the exact code SHA.

### Checkpoint E — locked

E is the final knowledge-only PO acceptance and portable transport/parity gate for V0.4.7.

Before E can close, V0.4.7 must also demonstrate **positive usefulness on an unchanged real repository**. At least one real-repository benchmark must naturally emit a supported cross-layer V0.4.7 answer from the generated knowledge pack; do not modify a benchmark merely to manufacture the accepted shape. If the existing benchmarks do not naturally exercise it, add a separate representative real project rather than weakening authority.

## V0.5 — Azure DevOps input evidence — LOCKED

Azure DevOps will add requirement intent, Epic/Feature/PBI history, status and traceability as another compiler input. It must not compensate for missing code-derived behavior.

Before V0.5 implementation, define the evidence-origin/authority and conflict contract so implementation-observed behavior, declared product intent and delivery history can coexist without one silently overwriting another.

V0.5 starts only after V0.4.7 and the V0.4.x PO-question-readiness exit gate pass.

## Later milestones

- V0.6 — incremental compilation and knowledge diffs.
- V0.7 — runtime UI exploration/confirmation.
- V0.8 — product insight, gaps and requirement-vs-implementation drift.

Cross-repository/system composition and a stable query/MCP projection remain important post-V0.5 product directions, but they are not allowed to distract the current V0.4.7 release checkpoint.

## Version semantics

Roadmap, package and serialized schema versions are independent. Current package remains:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

Do not mechanically bump package or schema identifiers when a roadmap checkpoint advances.