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

Do not advance while a predecessor checkpoint is red or under review. Keep business conditions, value lineage/provenance, and mutation/causality distinct. Unsupported inference fails closed and lower-authority deterministic evidence must survive stronger composition failure.

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
| D / R7.10 — Joint visibility | What backend condition and frontend visibility condition jointly control that same rendered value? | **REPAIRED / ALL GATES PASS / PENDING REREVIEW #5** |
| E — Product acceptance | Can an AI answer agreed PO questions from the portable knowledge pack alone? | **LOCKED behind D** |

### Accepted A/B/C/R7.9 baselines

```text
A      09a0c7f2a058adfd7ddb7b4ac2feb2d17580a429
B      17fd30b3a4b8178208adabc12c40dee060bedb54
C      fbb64b9917da1f63362558355201ff7998384ba0
R7.9   fc4bfa7042f59620b2c7ba1c4f6700cf3b02172a
mutation-causality repair 67624944da27ff1f1f5a1154018a255aae11d1fe
```

Keep these closed unless a real regression is demonstrated.

### D / R7.10 — repaired, awaiting independent rereview #5

The render-authority rereview sequence has successively closed these compile-valid/runtime-valid false-positive classes:

1. inert `<ng-template>`, HTML comments, HTML tag/attribute interpolation;
2. static HTML `hidden` ancestry;
3. static inline `display:none` ancestry;
4. static inline `visibility:hidden` ancestry.

Independent rereview #4 found this distinct blocker on production `dc69e442...`:

```html
<section style="visibility: hidden">
  @if (displayPrice > 0) {
    <strong>{{ displayPrice }}</strong>
  }
</section>
```

Because `dc69e442...` only recognized `display:none`, the hidden text could still receive authoritative render identity and feed R7.9/R7.10 composition.

Regression-first production repair:

```text
1fc4d212b9c7add2f012f51adf3eef0c16f34dae
fix: reject static visibility hidden renders
```

The bounded static-inline-style parser now recognizes both:

```text
display:none / display:none !important
visibility:hidden / visibility:hidden !important
```

matching case-insensitively. Positive safeguards retain authority for `display:block` and `visibility:visible`. Dynamic bindings, class/stylesheet cascade, computed CSS and general runtime DOM semantics remain unsupported rather than guessed.

Regression coverage remains concentrated in `tests/Pkc.CSharp.Tests/StaticCssRenderAuthorityRegressionTests.cs`, including frontend authority checks and end-to-end R7.9/R7.10 negatives.

Exact repaired-candidate gates:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35696272391 — PASS
pinned Loren                                35696272323 — PASS
Loren-main canary                           35696272328 — PASS
pinned Jellyfin + parity/provenance         35696273519 — PASS

Release build        0 warnings / 0 errors
C# tests             167 / 167 PASS
frontend tests       13 / 13 PASS
tool pack/install    PASS
```

Repaired three-repository benchmark:

```text
base production  1fc4d212b9c7add2f012f51adf3eef0c16f34dae
wrapper           7101b4c911539821c7c368203e0b05d150cbea64
run               35696381103 — PASS, 3 / 3 jobs
```

All three unchanged repositories remain conservatively at zero supported current R7.9/R7.10 positives. The post-R7.9 mutation-causality blocker remains closed.

This implementation session cannot independently certify its own repair. Fresh request:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-5-request.md`

### E — locked

E remains locked until independent rereview #5 accepts D. E is the final knowledge-only PO acceptance and portable transport/parity gate.

R7.14 positive real-project yield is **REQUIRED for E completion and remains NOT PASS**. Do not weaken authority or modify benchmarks merely to manufacture a positive shape.

## V0.5 — Azure DevOps input evidence — LOCKED

V0.5 starts only after V0.4.7 and the V0.4.x PO-question-readiness exit gate pass. ADO intent/history evidence must coexist with implementation-observed behavior without silently overwriting it.

## Later milestones

- V0.6 — incremental compilation and knowledge diffs.
- V0.7 — runtime UI exploration/confirmation.
- V0.8 — product insight, gaps and requirement-vs-implementation drift.

## Version semantics

```text
roadmap:      V0.4.7-D / R7.10 pending independent rereview #5
tool/package: RuaDen.Pkc.Tool 0.4.3-preview.2
```
