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
| D / R7.10 — Joint visibility | What backend condition and frontend visibility condition jointly control that same rendered value? | **REPAIRED / ALL GATES PASS / PENDING REREVIEW #3** |
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

R7.9 proves the bounded exact API response → frontend result → component member → authoritative rendered-value chain. Rereviews of R7.10 exposed additional render-authority exclusions that are now repaired on top of this accepted baseline.

### Post-R7.9 mutation-causality repair — accepted

```text
67624944da27ff1f1f5a1154018a255aae11d1fe
fix: avoid capturing mutation receiver out parameter
```

Runtime pattern-selected helper mutations stay as raw evidence with `caller-object-unproven` and are not promoted into workflow state changes without proven causality.

### D / R7.10 — repaired, awaiting independent rereview #3

Rereview #1 repaired false rendered-value authority for:

- inert `<ng-template>` content;
- HTML-comment interpolation;
- HTML tag/attribute interpolation.

Rereview #2 then found interpolation beneath a static HTML `hidden` ancestor could still be promoted as user-visible rendered text.

Regression-first production repair:

```text
97161baa2d0aff9131a7acf9db752393ae913d64
fix: reject statically hidden rendered text
```

The bounded render-authority scanner now rejects interpolation beneath active ancestors with a static `hidden` boolean attribute, honors presence semantics such as `hidden="false"`, pops closed hidden siblings, and fails closed on ambiguous ancestry. It does not claim dynamic `[hidden]`, CSS visibility, outlet, signal, structural-directive or general DOM semantics.

Regression coverage includes focused static-hidden cases and an end-to-end R7.9/R7.10 negative proving no rendered terminal or joint fact survives the hidden ancestry.

Exact repaired-candidate gates:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35686749488 — PASS
pinned Loren                                35686749493 — PASS
Loren-main canary                           35686749523 — PASS
pinned Jellyfin + parity/provenance         35686749575 — PASS

Release build        0 warnings / 0 errors
C# tests             161 / 161 PASS
frontend tests       13 / 13 PASS
tool pack/install    PASS
```

Repaired pinned three-repository benchmark:

```text
run 35687153720 — PASS, 3 / 3 jobs
```

Artifact inspection shows zero unsupported current R7.9/R7.10 positives on all three unchanged repositories, and the prior mutation-causality blocker remains closed.

This implementation session cannot independently certify its own repair. Fresh rereview request:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-3-request.md`

### E — locked

E remains locked until independent rereview #3 accepts D. E is the final knowledge-only PO acceptance and portable transport/parity gate.

R7.14 positive real-project yield is **REQUIRED for E completion and remains NOT PASS**. The current three-repository benchmark intentionally demonstrates conservative zero positive yield; do not weaken proof authority or modify a benchmark merely to manufacture the supported shape.

## V0.5 — Azure DevOps input evidence — LOCKED

V0.5 starts only after V0.4.7 and the V0.4.x PO-question-readiness exit gate pass. ADO intent/history evidence must coexist with implementation-observed behavior without silently overwriting it.

## Later milestones

- V0.6 — incremental compilation and knowledge diffs.
- V0.7 — runtime UI exploration/confirmation.
- V0.8 — product insight, gaps and requirement-vs-implementation drift.

## Version semantics

```text
roadmap:      V0.4.7-D / R7.10 pending independent rereview #3
tool/package: RuaDen.Pkc.Tool 0.4.3-preview.2
```

Roadmap, package and schema versions are independent.
