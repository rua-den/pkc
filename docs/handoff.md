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
8. `docs/reviews/2026-09-23-v0.4.7-d-r7.10-rereview-16-request.md`

Then inspect current `main`, recent commits and repository status. Never reset to a historical SHA merely because this handoff names it.

## Exact production under review

```text
37a71172c8c425219aef35f5843ac2109810ae9d
fix: preserve Angular infrastructure render evidence
```

Latest repair chain:

```text
46dcf8941c67a11d96e4416774d02e555a8f6aaf  fix: bound linked Angular package selectors
2ea361ba9d5e6e61041f38ca79e6587e12a1b745  fix: fail closed unresolved Angular component imports
37a71172c8c425219aef35f5843ac2109810ae9d  fix: preserve Angular infrastructure render evidence
```

A docs-only `[skip ci]` checkpoint may sit above production. Review production behavior at `37a711...`; do not reset `main`.

## Current state

```text
A/B/C                                  PASS / COMPLETE
R7.9                                   PASS / COMPLETE
mutation-causality                     PASS / CLOSED
R7.10                                  REPAIRED / ALL GATES PASS / PENDING REREVIEW #16
V0.4.7-D                               PENDING INDEPENDENT REREVIEW #16
V0.4.7-E                               LOCKED
R7.14                                  NOT PASS / REQUIRED FOR E
real-repo safety benchmark             PASS
real-repo product-value benchmark      NOT PASS / PARTIAL USEFULNESS
V0.5 / future W-U work                 LOCKED
```

Do not start E or later milestones until D is independently accepted.

## What changed after rereview #15 was requested

Continuation review found remaining external dependency authority seams rather than accepting `47e098...`:

- workspace / linked packages;
- unresolved external component imports used in standalone component `imports`;
- over-filtering of normal Angular framework imports;
- bounded const-array component imports.

The current candidate closes those shapes fail-closed without admitting dependency source into product knowledge.

## Exact-SHA gates

```text
CI / full tests / WorkPlay / PokeTrade  35823346084 — PASS
pinned Loren                            35823346060 — PASS
Loren-main canary                       35823346077 — PASS
pinned Jellyfin                         35823346046 — PASS
```

```text
Release build      0 warnings / 0 errors
C#                 253 / 253 PASS
frontend           13 / 13 PASS
tool pack/install  PASS
WorkPlay           PASS
PokeTrade          PASS
```

Jellyfin remains 43,365/43,365 project-semantic with 43,365 facts, 195,316 relations, 386 workflows, 116 product features and 504 knowledge Markdown files; portable parity/no-leak PASS.

## Real-repository benchmark

Wrapper based directly on `37a711...`:

```text
wrapper  f76c946e15cfe37d380745e0a2efb22cd3fc9123
run      35823872896 — PASS, 3 / 3
```

The wrapper changes only the benchmark branch trigger. Canonical output is unchanged versus the preceding safety benchmark.

### Safety result

PASS. No unsupported R7.9/R7.10 authority was introduced and accepted mutation-causality boundaries remain closed.

### Product-value result

**NOT PASS.**

Known-answer inspection shows PKC is already useful for direct backend questions but still misses material PO/QC behavior:

- Agentic login/signup validation rules are reconstructed well and UI→API linkage is often present.
- Kesetovic order Pack/Complete/Cancel permissions, preconditions and direct status transitions are reconstructed well.
- Kesetovic AddOrder misses initial `NEW` status and `OrderPrice * 0.05` bonus computation.
- SignalR `SendAsync("OrderSignal")` is visible in backend flow but Side effects remains empty.
- Jin12 controller → interface → concrete service behavior is not reconstructed deeply enough; user-scoped contact filtering and update mutations are lost.
- feature documents may report no business rules even when linked workflow documents contain grounded rules.
- all three pinned repos still yield zero authoritative R7.9/R7.10 positive cross-layer answers.

Use `docs/benchmarks/2026-09-23-real-repo-product-value-scorecard.md` as the benchmark truth. A green workflow alone is not product acceptance.

## Next action

Perform **independent rereview #16** of exact production `37a711...`.

Request: `docs/reviews/2026-09-23-v0.4.7-d-r7.10-rereview-16-request.md`.

If no new R7.10 false-positive blocker exists:

```text
mark R7.10 PASS / COMPLETE
→ mark D PASS / COMPLETE
→ unlock only E
→ keep R7.14 NOT PASS
→ use the product-value known-answer scorecard to drive E
```

If a blocker exists, record the exact counterexample and convert to regression-first implementation work.

The implementation thread that authored the repairs must not self-certify the candidate.

## Today / manual testing

The current candidate is suitable for **manual pre-acceptance product-value testing** because all standard gates and the safety benchmark pass.

Current CLI remains:

```text
pkc build <repository-path>
```

Inspect the generated `PKC_KNOWLEDGE.md` / `knowledge/` pack and test with PO/QC questions. Treat gaps documented in the scorecard as expected known failures, not proof that the run crashed.

## Future packet

After V0.4.7 is explicitly complete:

```text
docs/plans/2026-09-22-ai-workspace-continuous-update-plan.md
docs/reviews/2026-09-22-ai-workspace-continuous-update-plan-self-review.md
```

Preferred sequence remains AI workspace + `run/verify` → update/diff → Azure DevOps evidence.
