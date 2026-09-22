# PKC Milestones

Last updated: 2026-09-22

PKC is a Product/System Knowledge Compiler. Milestones are accepted only when generated portable knowledge answers named Product Owner questions with deterministic evidence and appropriate uncertainty.

## Permanent delivery rules

```text
question / acceptance boundary
→ regression-first fixture
→ deterministic implementation
→ focused verification
→ full relevant verification
→ diff review
→ coherent commit/push
→ exact-SHA cross-benchmark gates
→ independent review
```

Do not advance while a predecessor checkpoint is red or under review. Unsupported inference fails closed and lower-authority deterministic evidence must survive stronger composition failure.

## Accepted V0.4.x checkpoints

```text
V0.4.4 Loren knowledge readiness          PASS / COMPLETE
V0.4.5 real-repository generalization     PASS / COMPLETE
V0.4.6 business logic reconstruction      PASS / COMPLETE
```

Accepted V0.4.6 production: `c310e893762997f34562a6b3a62dbab2b05c0c93`.

## V0.4.7 — cross-layer PO-question readiness — CURRENT

| Checkpoint | PO question | Current state |
| --- | --- | --- |
| A — Origin and copy timing | Where did this value come from? Does an upstream change alter this existing value? | **PASS / COMPLETE** |
| B — Computation and later change | Was it calculated? What can overwrite it? What was the last proven source before output? | **PASS / COMPLETE** |
| C — Backend to API | What exact backend value supplies this response field? | **PASS / COMPLETE** |
| D / R7.9 — API to rendered value | What exact API field feeds the displayed value? | **PASS / COMPLETE** |
| D / R7.10 — Joint visibility | What backend condition and frontend visibility condition jointly control that same rendered value? | **REPAIRED / ALL GATES PASS / PENDING REREVIEW #9** |
| E — Product acceptance | Can an AI answer agreed PO questions from the portable knowledge pack alone? | **LOCKED behind D** |

### Accepted baselines

```text
A      09a0c7f2a058adfd7ddb7b4ac2feb2d17580a429
B      17fd30b3a4b8178208adabc12c40dee060bedb54
C      fbb64b9917da1f63362558355201ff7998384ba0
R7.9   fc4bfa7042f59620b2c7ba1c4f6700cf3b02172a
mutation-causality repair 67624944da27ff1f1f5a1154018a255aae11d1fe
```

Keep these closed unless a real regression is demonstrated.

### D / R7.10 — repaired, awaiting independent rereview #9

Rereviews #1–#7 repaired inert render containers, comments/tags/attributes, static and Angular hidden forms, static display/visibility suppression, legacy template fragments and comment-brace scope corruption.

Rereview #8 then found two further false-positive classes:

- `ngNonBindable` could leave literal `{{ member }}` text while PKC claimed a rendered member value;
- `>` inside quoted attributes could defeat the old tag-boundary heuristic and make attribute interpolation look like visible text.

The repair session also hardened visibility authority before handoff:

- quote-aware tag/attribute parsing for render and visibility boundaries;
- fake `@if` inside tags/attributes rejected;
- nested/extra Angular control blocks fail closed;
- plain HTML text quotes no longer corrupt brace scope;
- any `*structuralDirective` ancestor fails closed for R7.10 while preserving R7.9.

Final production candidate:

```text
cd3b1d4b64168c4e95284299c5715b3db0e4da4b
fix: fail closed on structural visibility directives
```

Exact final-candidate gates:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35714413695 — PASS
pinned Loren                                35714413680 — PASS
Loren-main canary                           35714413696 — PASS
pinned Jellyfin + parity/provenance         35714413688 — PASS

Release build        0 warnings / 0 errors
C# tests             194 / 194 PASS
frontend tests       13 / 13 PASS
tool pack/install    PASS
```

Final three-repository safety benchmark:

```text
base production  cd3b1d4b64168c4e95284299c5715b3db0e4da4b
wrapper           41c73a4c6a16065c49981bbd59b3e6dbc022b3cc
run               35714494308 — PASS, 3 / 3 jobs
```

All three unchanged repositories remain conservatively at zero supported current R7.9/R7.10 full positives. This passes the **safety/fail-closed benchmark**; it does not satisfy R7.14 positive yield.

Fresh independent request:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-9-request.md`

### E — locked

E remains locked until independent rereview #9 accepts D. E is the final knowledge-only PO acceptance and portable transport/parity gate.

R7.14 positive real-project yield is **REQUIRED for E completion and remains NOT PASS**. At least one unchanged real repository must naturally emit a supported positive V0.4.7 cross-layer answer. Do not weaken authority or modify a benchmark merely to manufacture yield.

## V0.5 — Azure DevOps input evidence — LOCKED

V0.5 starts only after V0.4.7 and the V0.4.x PO-question-readiness exit gate pass. ADO intent/history evidence must coexist with implementation-observed behavior without silently overwriting it.

## Later milestones

- V0.6 — incremental compilation and knowledge diffs.
- V0.7 — runtime UI exploration/confirmation.
- V0.8 — product insight, gaps and requirement-vs-implementation drift.

## Version semantics

```text
roadmap:      V0.4.7-D / R7.10 pending independent rereview #9
tool/package: RuaDen.Pkc.Tool 0.4.3-preview.2
```
