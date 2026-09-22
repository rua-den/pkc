# PKC Milestones

Last updated: 2026-09-22

PKC is a Product/System Knowledge Compiler. Milestones are accepted only when generated portable knowledge answers named Product Owner questions with deterministic evidence and appropriate uncertainty.

## Permanent delivery rules

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

Do not advance while a predecessor checkpoint is red or under review. Keep these knowledge classes distinct:

```text
business conditions
value lineage / provenance
mutation / causality
```

Unsupported inference fails closed and lower-authority deterministic evidence must survive stronger composition failure.

## Accepted V0.4.x checkpoints

```text
V0.4.4 Loren knowledge readiness          PASS / COMPLETE
V0.4.5 real-repository generalization     PASS / COMPLETE
V0.4.6 business logic reconstruction      PASS / COMPLETE
```

Accepted V0.4.6 production:

```text
c310e893762997f34562a6b3a62dbab2b05c0c93
fix: fail closed on Queryable authority
```

## V0.4.7 — cross-layer PO-question readiness — CURRENT

| Checkpoint | PO question | Current state |
| --- | --- | --- |
| A — Origin and copy timing | Where did this value come from? Does an upstream change alter this existing value? | **PASS / COMPLETE** |
| B — Computation and later change | Was it calculated? What can overwrite it? What was the last proven source before output? | **PASS / COMPLETE** |
| C — Backend to API | What exact backend value supplies this response field? | **PASS / COMPLETE** |
| D / R7.9 — API to rendered value | What exact API field feeds the displayed value? | **PASS / COMPLETE** |
| D / R7.10 — Joint visibility | What backend condition and frontend visibility condition jointly control that same rendered value? | **REPAIRED / ALL GATES PASS / PENDING REREVIEW #2** |
| E — Product acceptance | Can an AI answer agreed PO questions from the portable knowledge pack alone? | **LOCKED behind D** |

### A — accepted

```text
09a0c7f2a058adfd7ddb7b4ac2feb2d17580a429
feat: prove bounded reference dynamic lineage
```

A proves bounded snapshot vs reference/dynamic origin semantics with exact target-project identity.

### B — accepted

```text
17fd30b3a4b8178208adabc12c40dee060bedb54
fix: fail closed after opaque terminal effects
```

B proves supported derivation, original-origin retention, later override/mutation causality and bounded direct-return terminal source.

### C — accepted

```text
fbb64b9917da1f63362558355201ff7998384ba0
feat: prove backend API projection lineage
```

C proves exact backend entity/domain → DTO/projection → API response lineage using semantic project/assembly/type/member identity. Name matching alone is never proof.

### D / R7.9 — accepted

```text
fc4bfa7042f59620b2c7ba1c4f6700cf3b02172a
test: target frontend casing collision
```

R7.9 proves the bounded chain:

```text
C-proven API response property
→ explicit wire identity
→ typed frontend HTTP result member
→ exact resolved frontend service/result assignment
→ exact component/view-model member
→ authoritative Angular interpolation
→ rendered UI value
```

### Post-R7.9 mutation-causality repair — accepted

```text
67624944da27ff1f1f5a1154018a255aae11d1fe
fix: avoid capturing mutation receiver out parameter
```

Runtime pattern-selected helper mutations stay as raw evidence with `caller-object-unproven` and are not promoted into workflow state changes without proven causality.

### D / R7.10 — repaired, awaiting independent rereview #2

Original R7.10 candidate:

```text
eb64309263489a2b9bd658762b4526a4a32a8508
fix: bound Angular visibility to active template structure
```

Independent rereview #1 found compile-valid false rendered-value authority for:

- interpolation under a bare/inert `<ng-template>`;
- interpolation inside an HTML comment;
- interpolation inside an HTML tag/attribute.

Regression-first repairs:

```text
05ad6cb937010702a3fd01d5ef756e31bc4e8d9b
099fabfcaeecbaf009b75052d20075745ee02437
da5d23771ef8c9d58d0333d1f949e8d742210043
```

Current candidate:

```text
da5d23771ef8c9d58d0333d1f949e8d742210043
fix: require visible text interpolation
```

R7.10 composes only exact R7.9 identity with a bounded backend `Enumerable.Single/First(predicate)` selection proof and one supported active Angular `@if`. Frontend evidence cannot upgrade observed-only backend authority; ambiguous/nested/inert shapes fail closed.

Exact repaired-candidate gates:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35650084914 — PASS
pinned Loren                                35650084917 — PASS
Loren-main canary                           35650084926 — PASS
pinned Jellyfin + parity/provenance         35650084759 — PASS
```

Repaired three-repository benchmark:

```text
run 35650761753 — PASS, 3 / 3 jobs
```

The benchmark confirms no false R7.9/R7.10 positive on the unchanged pinned repos, but also confirms R7.14 positive real-project yield is still **NOT YET PASS**.

Rereview #1 cannot self-certify its repairs. Astra must perform independent rereview #2 on exact `da5d2377...` before D closes.

Review request:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-2-request.md`

### E — locked

E is the final knowledge-only PO acceptance and portable transport/parity gate. Before E can close, V0.4.7 must demonstrate positive usefulness on at least one unchanged real repository. Do not weaken proof authority or modify benchmarks merely to manufacture the shape.

## V0.5 — Azure DevOps input evidence — LOCKED

V0.5 starts only after V0.4.7 and the V0.4.x PO-question-readiness exit gate pass. ADO intent/history evidence must coexist with implementation-observed behavior without silently overwriting it.

## Later milestones

- V0.6 — incremental compilation and knowledge diffs.
- V0.7 — runtime UI exploration/confirmation.
- V0.8 — product insight, gaps and requirement-vs-implementation drift.

## Version semantics

```text
roadmap:      V0.4.7-D / R7.10 pending independent rereview #2
tool/package: RuaDen.Pkc.Tool 0.4.3-preview.2
```

Roadmap, package and schema versions are independent.
