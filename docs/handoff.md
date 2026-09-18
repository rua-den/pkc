# PKC Handoff

Use this file when continuing PKC in another coding or review thread.

Last updated: 2026-09-19

## Read first

Read in this exact order before changing production code:

1. `docs/status.md`
2. this handoff
3. `docs/milestones.md`
4. `docs/product-knowledge-contract.md`
5. `docs/v0.4.7-acceptance-plan.md`
6. `docs/reviews/2026-09-15-v0.4.6-independent-rereview-10.md`
7. `docs/reviews/2026-09-16-v0.4.7-plan-readiness-review.md`
8. `docs/reviews/2026-09-16-v0.4.7-snapshot-checkpoint-review.md`
9. `docs/reviews/2026-09-17-v0.4.7-alias-composition-review.md`
10. `docs/reviews/2026-09-17-v0.4.7-reference-dynamic-delivery.md`
11. `docs/reviews/2026-09-17-v0.4.7-b-computation-causality-delivery.md`
12. `docs/reviews/2026-09-18-v0.4.7-b-independent-review.md`
13. `docs/reviews/2026-09-19-v0.4.7-b-final-rereview.md`

Then inspect current `main`, recent commits and repository status before touching production code. Never reset to an older SHA because a handoff names a historical production checkpoint.

## Current accepted production-code checkpoint

V0.4.7-B is accepted on exact production code:

```text
17fd30b3a4b8178208adabc12c40dee060bedb54
fix: fail closed after opaque terminal effects
```

The current `main` may also contain the docs-only B closure checkpoint that includes this handoff. Verify HEAD before work.

## Current milestone state

```text
V0.4.7-A          PASS / COMPLETE
V0.4.7-B          PASS / COMPLETE
V0.4.7-C          CURRENT / UNLOCKED
V0.4.7-D          LOCKED behind C
V0.4.7-E          LOCKED behind D
V0.5 Azure DevOps LOCKED
```

Do not start D, E or V0.5.

## Closed production baseline

Accepted V0.4.6 remains:

```text
c310e893762997f34562a6b3a62dbab2b05c0c93
fix: fail closed on Queryable authority
```

Final V0.4.6 review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-10.md
PASS / COMPLETE
```

V0.4.6 is closed unless a new compile-valid and behavior-valid contradiction is found.

## Permanent product contract

PKC is a deterministic Product/System Knowledge Compiler:

```text
source inputs
→ deterministic analyzers/adapters
→ evidence/facts
→ feature/workflow/business-decision candidates
→ knowledge synthesis
→ canonical model
→ portable rendering
```

Keep these knowledge classes distinct:

```text
business conditions
value lineage / provenance
mutation / causality
```

Conservative authority downgrade must not erase independently proven lower-authority evidence.

## V0.4.7-A — closed

Accepted A checkpoint:

```text
09a0c7f2a058adfd7ddb7b4ac2feb2d17580a429
feat: prove bounded reference dynamic lineage
```

A proves both stored scalar snapshot semantics and bounded reference/dynamic read-time dependency with exact target-project identities and PO-facing Markdown.

Exact A gates:

```text
CI + WorkPlay + PokeTrade   35232224202 — PASS
pinned Loren                35232224014 — PASS
Loren-main                  35232224029 — PASS
pinned Jellyfin             35232223958 — PASS
```

## V0.4.7-B — PASS / COMPLETE

Final accepted B checkpoint:

```text
17fd30b3a4b8178208adabc12c40dee060bedb54
fix: fail closed after opaque terminal effects
```

B answers, for a bounded supported straight-line endpoint shape:

```text
Was the value copied or calculated?
What was its original observed origin?
What later supported write overrode it?
What was the last proven source before a supported direct return?
```

B emits/retains separate evidence for:

```text
stored derivation
original origin
later override / mutation causality
terminal source before direct return
```

The derivation is explicitly a snapshot, not a dynamic dependency. A later input change does not rewrite an already stored derived value.

### B blocker history and final boundary

Independent review first found stale terminal authority after nested/deconstruction writes. Subsequent rereview found deconstruction reference-alias reassignment, then opaque/custom effects and `goto` control flow. The fixes are generic fail-closed boundaries, not fixture special cases.

Permanent B negatives cover:

- nested and deconstruction member writes;
- deconstruction reference reassignment/declaration aliases;
- compound/unary writes;
- opaque invocation;
- custom getter/setter/constructor effects;
- user-defined operator/conversion effects;
- reference aliases/reassignment/reference parameters;
- branch/loop/try/conditional and `goto`/label/throw control flow;
- unresolved project semantics.

When an unsupported effect appears after a current value proof, current terminal authority is cleared for the remaining suffix while already emitted historical lineage/causality is retained.

B deliberately does not claim arbitrary persistence analysis or a general alias/effect/dataflow solver. The accepted terminal boundary is a supported direct return.

### Final exact-main B gates

All passed on exact code SHA `17fd30b3a4b8178208adabc12c40dee060bedb54`:

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
src/ source leak:        none
artifact id:             10561683704
artifact digest:         sha256:7d148704af898f82ecfd23d23e3ceeca6936a36da074a225aa4bafd95c042827
artifact size:           9,162,486 bytes
```

Final rereview found no remaining compile-valid/runtime-valid contradiction inside the documented B proof boundary. B is closed.

## V0.4.7-C — exact next implementation

C answers:

> What exact backend value supplies this response field, and how did it travel through entity/domain state → DTO/projection → API output?

Start regression-first from current main.

### Required first positive

Prove a bounded explicit chain equivalent to:

```text
DomainEntity.Price
→ ResponseDto.DisplayPrice
→ API response
```

The DTO property may be renamed; the edge must come from semantic assignment/projection evidence, never name matching.

Require metadata sufficient to prove:

```text
source project identity
source type/member identity
source occurrence and source location
target DTO/projection type/member identity
target occurrence and projection location
endpoint/API response identity and return location
analysisMode=project-semantic
```

Deliver the proven chain through scanner → candidate → synthesis → PO-facing workflow Markdown.

### Required negatives

C must fail closed for at least:

- unrelated same-name entity/DTO properties;
- same-name types/members across project/assembly context;
- custom/non-auto source or target accessors where storage/value semantics are not proven;
- user-defined conversion in the mapping path;
- reference alias ambiguity/reassignment;
- opaque invocation/effect between proven mapping and response;
- unsupported control flow/path ambiguity;
- missing target-project semantic context.

Preserve independently proven lower-authority facts when C composition is unavailable.

### PokeTrade role

Use `samples/PokeTradeSystem` as the known-answer real project. Do **not** modify PokeTrade merely to manufacture a convenient analyzer shape. If PokeTrade lacks an explicit DTO or renamed projection needed for a blocking regression, create a focused fixture instead. Once the generic implementation is green, PokeTrade remains a cross-project regression gate.

### C delivery discipline

```text
reproduce RED with focused fixture
→ implement minimum generic semantic proof
→ run focused C tests
→ run related value-lineage tests
→ run full C# tests + Release build
→ review complete diff
→ one coherent C implementation commit/push
→ exact-SHA CI + PokeTrade + pinned Loren + Loren-main + Jellyfin
→ independent C rereview
```

Do not use GitHub Actions as the edit/test loop.

## What remains locked

D will prove API → frontend binding/composition and joint backend/frontend explanations. E is final knowledge-only product acceptance and portable parity. Neither starts until its predecessor is explicitly PASS / COMPLETE.

V0.5 Azure DevOps remains locked until the complete V0.4.7 / V0.4.x PO-question-readiness exit gate passes.

## Version semantics

Roadmap, package and schema versions remain independent. Current package is still:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

Do not bump package/schema versions merely because B closed or C started.
