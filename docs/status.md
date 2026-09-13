# PKC Status

Last updated: 2026-09-14

## Current milestone

**V0.4.4 Loren Knowledge Readiness — PASS / COMPLETE.**

**V0.4.5 Independent Real-Repository Generalization Gate — UNLOCKED / NEXT.**

Final independent review:

```text
docs/reviews/2026-09-14-v0.4.4-external-rereview-4.md
```

Reviewed implementation state:

```text
final B1 implementation checkpoint: 5a5fdcb5fdf1a9fb888857791a4757382b86c774
independently reviewed HEAD:         cd7c4139f0bb2d17deb883a1639fd343c1b68d67
```

The commits after `5a5fdcb5...` and through the reviewed HEAD are documentation-only. The final external re-review found no remaining V0.4.4 acceptance blocker.

Last accepted published tool package remains:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

V0.4.4 acceptance does not by itself imply that a new package version has been published.

## Final V0.4.4 disposition

```text
B1 validation-condition equivalence correctness   PASS
B2 repeated-build canonical parity                 PASS
B3 capability-flow anti-overfit                    PASS
B4 frontend product-source scope                   PASS
PokeTrade known-answer regression                  PASS
Loren blind knowledge-only comprehension           PASS
Loren pinned external trial                        PASS
Loren-main canary                                  PASS
portable handoff parity                            PASS
independent external review                        PASS
```

### B1 final provenance closure

The last blocker was a reachable cross-stack false-positive when a conditional backend validation had no validated-object `parameterName` provenance.

Regression commit:

```text
82c28b4a4d2b2c8e9bdcdf2bb2e902d8caedf1c1
```

Red CI run:

```text
34762840775 — FAIL as expected before fix
```

Final fix:

```text
5a5fdcb5fdf1a9fb888857791a4757382b86c774
```

Conditional backend equivalence now requires:

```text
non-empty parameterName
AND exact endpoint-parameter membership
AND exact condition-root == parameterName
```

Missing or contradictory provenance is conservative and cannot emit `consistent / high`.

## Exact-HEAD verification

For reviewed HEAD `cd7c4139f0bb2d17deb883a1639fd343c1b68d67`:

```text
CI / PokeTrade       PASS  run 34770333701
pinned Loren trial   PASS  run 34770333766
Loren-main canary    PASS  run 34770333731
```

CI details:

```text
build:             PASS, 0 warnings / 0 errors
C# tests:          54 / 54 PASS
frontend tests:     8 / 8 PASS
tool pack/install: PASS
WorkPlay build:    PASS
PokeTrade:         PASS
```

Pinned Loren exact-HEAD artifact:

```text
artifact id:      10322236370
artifact digest:  sha256:365245f600c0ad0cf43ca6a89b5fa73066e517b2c157b018a361d8e2b9b228c4
structured files: 23
bundle parity:     23 / 23 verbatim
portable ZIP:      23 / 23 exact set + byte parity
source/raw leak:    0
bundle sha256:     2ea7037c5c4c8954b5fa1abcb4248963ad9299a3ce53e10591e5ce492dc17186
inner ZIP sha256:  663415a19781d5e93a814abd71fc547c94b884b18ac5c39300a2dda55e2d59bc
```

## Next milestone

Proceed to **V0.4.5 — Independent Real-Repository Generalization Gate** according to `docs/milestones.md` and `docs/real-project-trial.md`.

The next benchmark must be a second genuine repository that was not authored or modified for PKC, differs materially from PokeTrade/Loren, and is not selected merely because current heuristics handle it easily.

Run the same layered-output, blind knowledge-only comprehension, source cross-check, regression discipline and independent external review process.

## Scope lock

**V0.5 Azure DevOps evidence remains locked.**

Do not start V0.5 until both V0.4.4 and V0.4.5 pass.

Warnings W1 (claim-level portable provenance) and W2 (durable blind-review evidence) remain non-blocking follow-up concerns.