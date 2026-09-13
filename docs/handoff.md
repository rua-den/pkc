# PKC Handoff

Use this file when continuing PKC in another coding/review thread.

## Product contract

PKC is a Product/System Knowledge Compiler. A Product Owner should be able to hand generated portable knowledge to an AI assistant and ask product/system questions without requiring a source-code rescan.

Keep the compiler architecture deterministic:

```text
source
→ analyzers/adapters
→ evidence/facts
→ feature/workflow candidates
→ knowledge synthesis
→ canonical model
→ portable rendering
```

Do not implement direct source-to-freeform-AI generation.

## Current state

**V0.4.4 Loren Knowledge Readiness has PASSED independent external review and is COMPLETE.**

**V0.4.5 Independent Real-Repository Generalization Gate is now unlocked and is the next milestone.**

V0.5 Azure DevOps remains locked until V0.4.5 also passes.

Read first:

```text
docs/status.md
docs/milestones.md
docs/real-project-trial.md
docs/reviews/2026-09-14-v0.4.4-external-rereview-4.md
```

For review history, the earlier blocker records remain under `docs/reviews/2026-09-13-v0.4.4-*`.

## V0.4.4 accepted checkpoint

```text
final B1 implementation: 5a5fdcb5fdf1a9fb888857791a4757382b86c774
reviewed HEAD:            cd7c4139f0bb2d17deb883a1639fd343c1b68d67
final verdict:            PASS
```

Final accepted gate state:

```text
B1 validation-condition equivalence correctness   PASS
B2 repeated-build canonical parity                 PASS
B3 capability-flow anti-overfit                    PASS
B4 frontend product-source scope                   PASS
PokeTrade                                           PASS
Loren pinned                                        PASS
Loren-main canary                                   PASS
portable handoff parity                             PASS
independent external review                         PASS
```

The final B1 regression proves through real C# + frontend scanning and cross-stack correlation that missing backend validated-object provenance cannot be promoted to `consistent / high` merely because another condition root is an endpoint parameter.

The accepted backend condition proof contract is:

```text
conditional backend equivalence requires:
1. non-empty validation fact parameterName;
2. exact endpoint-parameter membership;
3. exact condition field root == parameterName.

No proof → non-consistent conservative result.
```

Do not weaken this contract in later work.

## Exact reviewed automation

For reviewed HEAD `cd7c4139f0bb2d17deb883a1639fd343c1b68d67`:

```text
CI / PokeTrade       PASS  run 34770333701
pinned Loren trial   PASS  run 34770333766
Loren-main canary    PASS  run 34770333731
```

CI contained:

```text
build:          0 warnings / 0 errors
C# tests:       54 / 54 PASS
frontend tests:  8 / 8 PASS
WorkPlay build: PASS
PokeTrade:      PASS
```

Pinned Loren portable output was independently rechecked:

```text
structured Markdown: 23
bundle parity:        23 / 23 verbatim
portable ZIP:         exact same 23 files + byte parity
source/raw leak:      0
```

## Next milestone — V0.4.5

Purpose: prove PKC did not overfit PokeTrade + Loren before adding another major evidence source.

Select a **second genuine repository** that:

- was not authored or modified for PKC;
- fits the currently supported C# surface;
- has non-trivial product/system behavior;
- differs materially from PokeTrade/Loren;
- is not chosen merely because current heuristics handle it easily.

Then run the same acceptance discipline described in `docs/real-project-trial.md`:

```text
pin benchmark commit
→ compile layered knowledge output
→ inspect analyzer/fallback provenance
→ knowledge-only blind review
→ freeze answers
→ source cross-check
→ classify concrete gaps
→ fix compiler generically, regression-first if needed
→ rerun PokeTrade + Loren + new benchmark
→ independent external review
```

V0.4.5 passes only when:

- critical product questions have no blocker false claims;
- important behavior is answerable or explicitly unknown;
- product-level output is high-signal;
- evidence remains traceable;
- fixes are generic, with no repository-specific exceptions;
- PokeTrade + Loren remain green after any fixes;
- independent external review has no unresolved blocker.

## Scope locks

Do not start V0.5 Azure DevOps ingestion until V0.4.5 independently passes.

Also avoid unrelated roadmap expansion while V0.4.5 is being used as the anti-overfit/generalization gate.

Warnings W1 (claim-level portable provenance) and W2 (durable blind-review evidence) remain non-blocking follow-up concerns and should be tracked, but they do not reopen V0.4.4.

## Bootstrap prompt for the next coding thread

```text
Continue PKC from current main HEAD.

Read in order:
1. docs/status.md
2. docs/handoff.md
3. docs/milestones.md
4. docs/real-project-trial.md
5. docs/reviews/2026-09-14-v0.4.4-external-rereview-4.md

V0.4.4 has passed independent external review. Begin only V0.4.5: the independent real-repository generalization gate.

Select and pin a second genuine repository according to the milestone criteria, run the same layered-output + blind knowledge-only + source-cross-check process, keep all fixes generic and regression-first, and rerun PokeTrade + Loren after any compiler change.

Do not start V0.5 Azure DevOps until V0.4.5 independently passes.
```
