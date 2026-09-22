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
| D / R7.10 — Joint visibility | What backend condition and frontend visibility condition jointly control that same rendered value? | **REREVIEW #2 FAIL — STATIC HIDDEN ANCESTOR BLOCKER** |
| E — Product acceptance | Can an AI answer agreed PO questions from the portable knowledge pack alone? | **LOCKED behind D** |

### A — accepted

```text
09a0c7f2a058adfd7ddb7b4ac2feb2d17580a429
feat: prove bounded reference dynamic lineage
```

### B — accepted

```text
17fd30b3a4b8178208adabc12c40dee060bedb54
fix: fail closed after opaque terminal effects
```

### C — accepted

```text
fbb64b9917da1f63362558355201ff7998384ba0
feat: prove backend API projection lineage
```

C proves exact target-project-semantic domain/entity property → DTO/projection → API response identity and fails closed on unsupported or ambiguous semantics.

### D / R7.9 — accepted predecessor

```text
fc4bfa7042f59620b2c7ba1c4f6700cf3b02172a
test: target frontend casing collision
```

R7.9 proves the bounded chain:

```text
C-proven API response property
→ explicit wire identity
→ typed frontend HTTP result member
→ exact resolved service/result assignment
→ exact component/view-model member
→ authoritative rendered interpolation
→ rendered UI value
```

R7.9 remains accepted as a predecessor, but the current R7.10 rereview exposed a new over-authority case in the rendered-value boundary that must be repaired before D can close.

### Post-R7.9 mutation-causality repair — accepted

```text
67624944da27ff1f1f5a1154018a255aae11d1fe
fix: avoid capturing mutation receiver out parameter
```

Runtime pattern-selected helper mutations stay as raw evidence with `caller-object-unproven` and are not promoted into workflow state changes without proven causality.

### D / R7.10 — blocked after independent rereview #2

Current production SHA reviewed:

```text
da5d23771ef8c9d58d0333d1f949e8d742210043
fix: require visible text interpolation
```

Rereview #1 had already repaired false rendered-value authority for interpolation under inert `<ng-template>`, inside HTML comments, and inside HTML tags/attributes.

Independent rereview #2 found a new compile-valid/runtime-valid counterexample:

```html
<section hidden>
  @if (displayPrice > 0) {
    <strong>{{ displayPrice }}</strong>
  }
</section>
```

The standard HTML `hidden` attribute prevents the subtree from being presented to the user, but current render authority still treats the simple interpolation as authoritative because it is not in a comment, attribute, or inert `<ng-template>`. That can produce a false R7.9 `rendered UI value` terminal and a false R7.10 observable joint-visibility rule.

Review record:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-2.md`

Required next implementation is regression-first and minimum-generic: reject interpolation under a statically hidden HTML ancestor, then rerun all local and exact-SHA gates and request a fresh independent rereview.

Do not start E while this blocker is open.

### E — locked

E is the final knowledge-only PO acceptance and portable transport/parity gate. Before E can close, V0.4.7 must also demonstrate positive usefulness on at least one unchanged real repository.

R7.14 positive real-project yield remains **NOT PASS**. Do not weaken proof authority or modify benchmarks merely to manufacture the shape.

## V0.5 — Azure DevOps input evidence — LOCKED

V0.5 starts only after V0.4.7 and the V0.4.x PO-question-readiness exit gate pass. ADO intent/history evidence must coexist with implementation-observed behavior without silently overwriting it.

## Later milestones

- V0.6 — incremental compilation and knowledge diffs.
- V0.7 — runtime UI exploration/confirmation.
- V0.8 — product insight, gaps and requirement-vs-implementation drift.

## Version semantics

```text
roadmap:      V0.4.7-D / R7.10 BLOCKED after independent rereview #2
tool/package: RuaDen.Pkc.Tool 0.4.3-preview.2
```

Roadmap, package and schema versions are independent.