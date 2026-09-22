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
| D / R7.10 — Joint visibility | What backend condition and frontend visibility condition jointly control that same rendered value? | **REPAIRED / ALL GATES PASS / PENDING REREVIEW #4** |
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

### D / R7.10 — repaired, awaiting independent rereview #4

Rereview #1 repaired false rendered authority for inert `<ng-template>`, HTML comments, and HTML tag/attribute interpolation.

Rereview #2 repaired static HTML `hidden` ancestry at:

```text
97161baa2d0aff9131a7acf9db752393ae913d64
fix: reject statically hidden rendered text
```

Rereview #3 found another compile-valid/runtime-valid false-authority shape:

```html
<section style="display: none">
  @if (displayPrice > 0) {
    <strong>{{ displayPrice }}</strong>
  }
</section>
```

The subtree is statically non-presented but the previous R7.9 render-authority boundary could still promote it and feed R7.10.

Regression-first repair:

```text
dc69e44206942ffb0012e1994d6d39249d1db4be
fix: reject static display none renders
```

The repair rejects simple interpolation beneath static inline `display:none` / `display:none !important` ancestry and retains authority for static `display:block`. Dynamic style bindings, class stylesheets, computed CSS and general DOM/runtime semantics remain unsupported rather than guessed.

Regression coverage is in `tests/Pkc.CSharp.Tests/StaticCssRenderAuthorityRegressionTests.cs`, including an end-to-end R7.9/R7.10 negative.

Exact repaired-candidate gates:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35687831689 — PASS
pinned Loren                                35687831632 — PASS
Loren-main canary                           35687831587 — PASS
pinned Jellyfin + parity/provenance         35687831537 — PASS

Release build        0 warnings / 0 errors
C# tests             164 / 164 PASS
frontend tests       13 / 13 PASS
tool pack/install    PASS
```

Repaired three-repository benchmark:

```text
run 35687951314 — PASS, 3 / 3 jobs
```

All three unchanged repositories remain conservatively at zero supported current R7.9/R7.10 positives. The post-R7.9 mutation-causality blocker remains closed.

Rereview #3 cannot self-certify its own repair. Fresh request:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-4-request.md`

### E — locked

E remains locked until independent rereview #4 accepts D. E is the final knowledge-only PO acceptance and portable transport/parity gate.

R7.14 positive real-project yield is **REQUIRED for E completion and remains NOT PASS**. Do not weaken authority or modify benchmarks merely to manufacture a positive shape.

## V0.5 — Azure DevOps input evidence — LOCKED

V0.5 starts only after V0.4.7 and the V0.4.x PO-question-readiness exit gate pass. ADO intent/history evidence must coexist with implementation-observed behavior without silently overwriting it.

## Later milestones

- V0.6 — incremental compilation and knowledge diffs.
- V0.7 — runtime UI exploration/confirmation.
- V0.8 — product insight, gaps and requirement-vs-implementation drift.

## Version semantics

```text
roadmap:      V0.4.7-D / R7.10 pending independent rereview #4
tool/package: RuaDen.Pkc.Tool 0.4.3-preview.2
```
