# PKC Claude Code Bootstrap

This file is for engineers/reviewers working on the **PKC source repository**. It is not the generated `CLAUDE.md` that `pkc run` creates for a target product repository.

Before auditing or changing PKC:

1. Read `AGENTS.md` completely and follow it as repository policy.
2. Read `docs/status.md`.
3. Read `docs/handoff.md`.
4. Read the milestone/acceptance/review documents named by the handoff.
5. Read `docs/benchmarks/product-value-benchmark-protocol.md` before changing product-knowledge or AI-workspace behavior.
6. Inspect current `main`, recent commits, working tree state and relevant regressions before editing.

The repository and current handoff are the source of truth. Do not trust stale chat SHAs or older requests over current repository state.

## Mandatory product-value loop

For changes affecting generated knowledge or the AI workspace, do not stop at green unit tests or a successful PKC process exit.

Run the benchmark protocol end-to-end:

```text
candidate code
→ pkc run pinned real repository
→ open only generated .pkc/workspace as product context
→ answer standard PO/QC questions
→ record generated answers
→ inspect pinned source known answers
→ score PASS/PARTIAL/FAIL + percentages
→ report concrete misses
→ choose next highest-ROI repair
```

A green safety benchmark is not the same as a product-value PASS.

## Milestone discipline

Respect the formal milestone state in `docs/status.md`. A user-authorized demo/productization spike may exist while an earlier acceptance gate remains open; such a spike must not be used to falsely mark the formal milestone complete.

Keep making forward progress on the active task until PASS, an external review/gate, or a proven external blocker is reached.
