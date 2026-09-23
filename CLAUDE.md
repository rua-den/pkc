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

## Company-source privacy boundary

PKC may be run against proprietary company repositories. Treat those sources as confidential unless the current company policy explicitly says otherwise.

- Keep proprietary source inspection inside the approved source-enabled/company Claude Code environment.
- Never copy company source files, code snippets, secrets, credentials, raw `.pkc/facts.json`, or proprietary file contents into PKC repository docs, benchmark reports, public issues, or generated product workspaces.
- Benchmark reports should record business behavior, score, source paths/symbol names when useful, and bounded evidence descriptions — not source-code bodies.
- Do not commit a company's target repository or generated `.pkc/` output into the PKC repository.
- Editing source locally as part of an explicitly authorized ENGINEERING task is allowed; exporting or dumping proprietary source outside the approved environment is not.
- PKC workspace isolation and these instructions reduce accidental code exposure, but PKC itself does not control Claude/network retention policy. Use only the company-approved Claude Code/enterprise environment for proprietary source.

## Benchmark cadence — do not waste AI context

A green safety benchmark is not the same as a product-value PASS, but the full AI benchmark is **not** required after every edit.

Use three levels:

### Level 0 — deterministic verification, default for every change

Run focused tests, related tests, relevant build/smoke gates, deterministic regression assertions, workspace isolation/no-leak checks, and output-diff/idempotence checks where applicable.

This level does **not** require an AI to reread generated knowledge.

### Level 1 — targeted AI product-value benchmark

Run this only when a change can materially alter product answers, for example:

- interface → implementation traversal;
- permissions/preconditions;
- state/default/computation extraction;
- side effects;
- frontend → API linking;
- displayed-value lineage;
- feature/workflow synthesis;
- evidence authority that changes what may be stated.

Choose only the affected pinned repo/workflow and only the relevant standard questions. Usually one or two probes are enough.

Use two phases:

```text
phase 1: generated .pkc/workspace only → answer the question
phase 2: source-enabled local cross-check → establish known answer → score
```

Do not paste source code into the benchmark report.

### Level 2 — full AI product-value benchmark

Run the full pinned corpus only for:

- milestone/product-acceptance checkpoints;
- release candidates;
- important demos;
- major canonical-knowledge or routing-model changes;
- a targeted benchmark that reveals broad regression risk;
- an explicit user/reviewer request.

Do not run Level 2 merely because a small implementation or documentation change landed.

## Product-value loop when Level 1 or Level 2 is required

```text
candidate code
→ pkc run selected pinned real repository
→ open only generated .pkc/workspace as product context
→ answer selected PO/QC questions
→ record generated answers
→ inspect pinned source in approved source-enabled context
→ compare with source-known answer
→ score PASS/PARTIAL/FAIL + percentages
→ report concrete misses without source-code dumps
→ choose next highest-ROI repair
```

## Milestone discipline

Respect the formal milestone state in `docs/status.md`. A user-authorized demo/productization spike may exist while an earlier acceptance gate remains open; such a spike must not be used to falsely mark the formal milestone complete.

Keep making forward progress on the active task until PASS, an external review/gate, or a proven external blocker is reached.
