# AI Workspace Preview — Company Claude Audit Request — 2026-09-23

## Purpose

Audit and continue the user-authorized demo-critical AI workspace preview without falsely advancing the formal V0.4.7 milestone.

This audit is specifically for the new product UX:

```text
pkc run <repository-path>
→ <repository>/.pkc/workspace/
→ Claude/Codex opens the generated workspace
→ asks Product Owner / QA questions without manually selecting PKC_KNOWLEDGE.md
```

## Exact implementation to audit

```text
b76427f67b78ab8964284c1a6c43ec89e5656375
fix: isolate pkc run workspace
```

Parent feature commit:

```text
6e845dc16718e74adac38e4aa49ee75023f99fa5
feat: generate AI product workspace
```

Do not reset `main` to these SHAs if a later docs-only handoff commit sits above them. Inspect the exact production behavior and current `main` history.

## Formal milestone boundary

The workspace preview was explicitly authorized by the user for same-day real-team testing. It is a productization spike, **not** evidence that the formal roadmap gate was completed.

At the time of this request:

```text
R7.10 / V0.4.7-D   pending independent rereview #17
V0.4.7-E           locked
R7.14               not pass
formal W acceptance not unlocked / not complete
```

Do not mark D, E, W, U, or V0.5 complete merely because the preview works.

## Required read order

1. root `CLAUDE.md`
2. `AGENTS.md`
3. `docs/status.md`
4. `docs/handoff.md`
5. `docs/product-knowledge-contract.md`
6. `docs/v0.4.7-acceptance-plan.md`
7. `docs/plans/2026-09-22-ai-workspace-continuous-update-plan.md`
8. `docs/reviews/2026-09-22-ai-workspace-continuous-update-plan-self-review.md`
9. `docs/benchmarks/product-value-benchmark-protocol.md`
10. `docs/benchmarks/2026-09-23-ai-question-answerability-benchmark.md`
11. this request

Then inspect current `main`, recent commits, production code and regressions.

## Implemented preview contract

`pkc run <repo>` must generate only the product workspace and PKC metadata under `.pkc`:

```text
<repo>/.pkc/
  facts.json
  feature-candidates.json
  product-features.json
  workspace/
    CLAUDE.md
    AGENTS.md
    knowledge/
      START_HERE.md
      index.md
      features/...
      workflows/...
    _policy/
      answer-contract.md
    _meta/
      manifest.json
      catalog.json
```

Required properties:

- do not overwrite source-root `CLAUDE.md`;
- do not overwrite source-root `AGENTS.md`;
- `pkc run` must not generate root `knowledge/`, `PKC_KNOWLEDGE.md`, or `PKC_KNOWLEDGE.zip`;
- legacy `pkc build` retains those compatibility outputs;
- stale generated workspace files are replaced by the next run;
- workspace path traversal is rejected;
- generated bootloaders route the AI through START_HERE + answer contract + catalog;
- PRODUCT is the default response mode;
- TRACE and ENGINEERING require explicit user intent;
- unsupported behavior must remain unknown rather than invented;
- the manifest must say `PREVIEW`, not `READY`, because `pkc verify` has not been implemented.

## Existing regressions

```text
tests/Pkc.CSharp.Tests/AiWorkspaceRendererTests.cs
```

Coverage includes:

- bootstrap/policy/catalog/manifest generation;
- source-root CLAUDE/AGENTS preservation;
- stale generated workspace cleanup;
- path traversal rejection.

## Isolated end-to-end smoke evidence

Validation wrapper branch:

```text
benchmark/workspace-b76427
```

Wrapper commit:

```text
1ee7b0d523f29dcbb7f45494d2c9461e9d1027ce
test: run isolated AI workspace smoke
```

Workflow run:

```text
35845652352 — PASS
```

The wrapper adds only a validation workflow. It copies WorkPlay to a temporary target, creates fake team root `CLAUDE.md` and `AGENTS.md`, runs exact production `pkc run`, and verifies:

- root team agent files are unchanged;
- root legacy knowledge artifacts are absent;
- `.pkc/workspace` contains bootloaders, routing, policy, catalog, manifest and canonical knowledge;
- generated knowledge still contains expected WorkPlay product behavior.

Do not merge the wrapper workflow into production merely because it exists; decide separately whether this smoke should become a permanent CI gate.

## Audit questions

Independently review at least:

1. Can `pkc run` modify anything outside `.pkc` on a clean target repository?
2. Can a crafted canonical path escape `.pkc/workspace`?
3. Can regeneration leave stale generated workspace knowledge that may mislead an AI?
4. Do root team `CLAUDE.md` / `AGENTS.md` remain byte-for-byte untouched?
5. Does the generated `CLAUDE.md` correctly route Claude to product knowledge without requiring manual `PKC_KNOWLEDGE.md` selection?
6. Does `AGENTS.md` provide equivalent routing for compatible coding agents?
7. Does PRODUCT mode avoid default source/code dumping?
8. Is TRACE explicit and bounded?
9. Is ENGINEERING honest about source availability?
10. Does the manifest avoid a false READY claim?
11. Does `pkc build` remain backward compatible?
12. Does running `pkc run` twice produce the same semantic workspace for unchanged source, ignoring genuinely non-semantic metadata?
13. Does the workspace remain usable on a repository that already has `.pkc` metadata from a prior scan/build?
14. Are filesystem/symlink/reparse-point boundaries sufficiently fail-closed for company repositories?

If a concrete defect exists, reproduce it regression-first and implement the minimum generic repair.

## Mandatory benchmark after product changes

After any fix that can affect generated knowledge or workspace routing, follow:

`docs/benchmarks/product-value-benchmark-protocol.md`

The required sequence is:

```text
pkc run pinned repo
→ open only .pkc/workspace as AI context
→ answer standard PO/QC questions
→ record answer
→ inspect pinned source known answer
→ score percentages + PASS/PARTIAL/FAIL
→ report concrete misses
```

Do not report only fact counts, workflow counts, process exit code, or CI status.

## Baseline product-value scores

Current reference baseline before workspace-specific product improvements:

```text
Agentic Users Update       80.8%
Jin12 Contacts Update      40.0%
Kesetovic PackOrder        65.0%

backend PO/QC core         72.6%
overall applicable         63.9%
```

Recompute rather than copying these numbers after a material knowledge change.

## Highest-value known gaps after workspace audit

Do not broaden scope until the workspace boundary itself is sound. Once it is, the existing product benchmark identifies these high-value knowledge gaps:

1. interface → concrete implementation traversal;
2. frontend event/service URL-expression linkage;
3. displayed-value lineage;
4. construction/default/computation state;
5. integration side-effect synthesis;
6. feature-summary fidelity;
7. real-repository R7.14 positive yield without weakening authority.

Formal E remains locked until D is independently accepted. Continue productization work only under the explicit user-authorized preview scope recorded in the handoff.
