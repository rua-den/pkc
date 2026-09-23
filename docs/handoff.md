# PKC Handoff

Last updated: 2026-09-23

Use this file when continuing PKC in another coding/review thread.

## Read first

Read in this order before changing production code:

1. `docs/status.md`
2. this handoff
3. `docs/milestones.md`
4. `docs/product-knowledge-contract.md`
5. `docs/v0.4.7-acceptance-plan.md`
6. `docs/reviews/2026-09-23-v0.4.7-d-r7.10-independent-rereview-14.md`
7. `docs/benchmarks/2026-09-23-real-repo-product-value-scorecard.md`
8. `docs/reviews/2026-09-23-v0.4.7-d-r7.10-rereview-17-request.md`

Then inspect current `main`, recent commits and repository status. Never reset to a historical SHA merely because this handoff names it.

## Exact production under review

```text
96205a9a643864facaf9642a3b390ddcdbed59d9
fix: tolerate duplicate Angular import aliases
```

Latest production repair chain:

```text
10876a28d15767930e0bc6de95def4993bb7265f  fix: close indirect Angular component import authority
d81be9560c741296271376aca7481db3d9925e41  fix: fail closed unsupported Angular import indirection
96205a9a643864facaf9642a3b390ddcdbed59d9  fix: tolerate duplicate Angular import aliases
```

Rereview #16 targeted the older `37a711...` candidate and is superseded. Do not review that SHA as the current candidate and do not self-certify `96205...` from the implementation thread.

## Current state

```text
A/B/C                                  PASS / COMPLETE
R7.9                                   PASS / COMPLETE
mutation-causality                     PASS / CLOSED
R7.10                                  REPAIRED / ALL GATES PASS / PENDING REREVIEW #17
V0.4.7-D                               PENDING INDEPENDENT REREVIEW #17
V0.4.7-E                               LOCKED
R7.14                                  NOT PASS / REQUIRED FOR E
real-repo safety benchmark             PASS
real-repo product-value benchmark      NOT PASS / PARTIAL USEFULNESS
V0.5 / future W-U work                 LOCKED
```

Do not start E or later milestones until D is independently accepted.

## What changed after rereview #16 was requested

A fresh implementation-side adversarial review found a compile-valid standalone Angular indirection bypass:

```text
external package symbol
→ local shared-imports helper
→ local barrel / re-export
→ component imports
→ rendered member
```

The previous boundary handled direct unresolved imports and bounded same-file arrays but did not close the local module symbol graph. The repair now propagates risk through local imports/re-exports/export-all/const-array closure. Unsupported scalar aliases and default re-export shapes fail closed. A duplicate-alias crash path discovered during static review was also removed.

Focused regressions:

```text
tests/Pkc.CSharp.Tests/AngularComponentImportClosureAuthorityRegressionTests.cs
tests/Pkc.CSharp.Tests/AngularUnsupportedComponentImportIndirectionAuthorityRegressionTests.cs
```

## Exact-SHA gates

```text
CI / full tests / WorkPlay / PokeTrade  35832501567 — PASS
pinned Loren                            35832501543 — PASS
Loren-main canary                       35832501552 — PASS
pinned Jellyfin                         35832501534 — PASS
```

```text
Release build      0 warnings / 0 errors
C#                 259 / 259 PASS
frontend           13 / 13 PASS
tool pack/install  PASS
WorkPlay           PASS
PokeTrade          PASS
```

Jellyfin remains 43,365/43,365 project-semantic with 43,365 facts, 195,316 relations, 386 workflows, 116 product features and 504 knowledge Markdown files; portable parity/no-leak PASS.

## Real-repository benchmark

Wrapper based directly on `96205...`:

```text
wrapper  65c02df67938197d929d30793dc012dbc3878ca3
run      35833147258 — PASS, 3 / 3
```

Established output sizes are unchanged:

```text
agentic-angular  441 facts / 660 relations / 26 knowledge files
jin12-crm        415 facts / 1,580 relations / 25 knowledge files
kesetovic-crm    488 facts / 2,054 relations / 28 knowledge files
```

Artifacts:

```text
agentic-angular  10737653171
jin12-crm        10738280766
kesetovic-crm    10737827547
```

Agentic and Kesetovic target builds remain blocked by dependency vulnerability warnings-as-errors, but PKC exits 0 and emits benchmark evidence. Do not confuse the 3/3 workflow result with product-value acceptance.

### Safety result

PASS. The new production code only removes or withholds unsupported render authority; it does not synthesize new product evidence.

### Product-value result

**NOT PASS.** The known-answer gaps are still the acceptance truth:

- feature documents can lose child-workflow rules;
- controller → interface → implementation behavior is not reconstructed deeply enough;
- construction-time/default/computed state can be lost;
- integration side effects can be observed but omitted from synthesized Side effects;
- R7.14 still has zero supported positive cross-layer yield on the pinned repos.

Use `docs/benchmarks/2026-09-23-real-repo-product-value-scorecard.md` as source of truth for E once D passes.

## Next action

Perform **independent rereview #17** of exact production `96205...`.

Request: `docs/reviews/2026-09-23-v0.4.7-d-r7.10-rereview-17-request.md`.

If no new R7.10 false-positive blocker exists:

```text
mark R7.10 PASS / COMPLETE
→ mark D PASS / COMPLETE
→ unlock only E
→ keep R7.14 NOT PASS
→ drive E from the known-answer product-value scorecard
```

If a blocker exists, record the exact counterexample and convert to regression-first implementation work.

## Future packet

Only after V0.4.7 is explicitly complete:

```text
docs/plans/2026-09-22-ai-workspace-continuous-update-plan.md
docs/reviews/2026-09-22-ai-workspace-continuous-update-plan-self-review.md
```

Preferred sequence remains AI workspace + `run/verify` → update/diff → Azure DevOps evidence.
