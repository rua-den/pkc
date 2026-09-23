# AI Workspace Preview — Company Claude Audit / Coding Request — 2026-09-23

## Purpose

Audit and continue the user-authorized AI workspace preview for real company-repository use without falsely advancing the formal V0.4.7 roadmap.

Primary product UX:

```text
pkc run <repository-path>
→ <repository>/.pkc/workspace/
→ Claude Code / approved company agent opens the generated workspace
→ Product Owner / QA questions are answered without manually selecting PKC_KNOWLEDGE.md
```

## Exact workspace/privacy candidate

```text
bf0ef686f2591841b85f36583a5fec7afb049060
feat: enforce workspace privacy and benchmark cadence
```

Relevant predecessor commits:

```text
6e845dc16718e74adac38e4aa49ee75023f99fa5  feat: generate AI product workspace
b76427f67b78ab8964284c1a6c43ec89e5656375  fix: isolate pkc run workspace
```

Do not reset current `main` to these SHAs if a later docs-only `[skip ci]` handoff sits above them. Review the exact production behavior while keeping current repository history intact.

## Formal milestone boundary

The workspace preview is explicitly user-authorized for company testing. It is a productization spike, not evidence that formal roadmap gates are complete.

Keep this state separate:

```text
R7.10 / V0.4.7-D   pending independent rereview #17
V0.4.7-E           locked
R7.14               not pass
formal W acceptance not unlocked / not complete
```

Do not mark D, E, W, U, or V0.5 complete merely because the preview works.

## Required read order for Claude Opus / Claude Code

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

Then inspect current `main`, recent commits, working tree state, production code and regressions.

## Company-source privacy boundary

Company source is confidential unless the company explicitly says otherwise.

- Perform proprietary source inspection/editing only inside the approved source-enabled/company Claude Code or enterprise environment.
- Never copy company source files, code snippets, secrets, credentials, raw `.pkc/facts.json`, or proprietary file contents into PKC repo docs, benchmark reports, public issues, or generated product workspaces.
- Benchmark reports may contain business behavior, scores, endpoints, source paths, symbol names and bounded evidence descriptions; they must not contain source-code bodies.
- Never commit the company target repository or its generated `.pkc/` output into the PKC repository.
- PRODUCT/QA usage must stay inside generated `.pkc/workspace` by default.
- ENGINEERING/source inspection is explicit and local to the approved environment.
- PKC controls generated workspace content and routing. It does not control provider/network retention policy; proprietary source must therefore use the company-approved Claude environment.

## Implemented generated workspace contract

`pkc run <repo>` generates PKC metadata and the product workspace under `.pkc`:

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

- source-root `CLAUDE.md` is never overwritten;
- source-root `AGENTS.md` is never overwritten;
- `pkc run` emits no root `knowledge/`, `PKC_KNOWLEDGE.md`, or `PKC_KNOWLEDGE.zip`;
- legacy `pkc build` retains compatibility outputs;
- stale generated workspace files are replaced by the next run;
- path traversal outside `.pkc/workspace` is rejected;
- generated `CLAUDE.md` / `AGENTS.md` route through START_HERE + answer contract + catalog;
- PRODUCT is default and does not inspect source;
- TRACE is explicit and may identify paths/symbols without code dumps;
- ENGINEERING is explicit and requires approved source-enabled context;
- benchmark phase 1 is workspace-only;
- benchmark source cross-check is phase 2 and local to the approved environment;
- manifest remains `PREVIEW`, not `READY`;
- manifest states that generated workspace contains no source code/raw facts and source is not read by default.

## Primary implementation/regression files

```text
src/Pkc.Cli/Program.cs
src/Pkc.Knowledge/AiWorkspaceRenderer.cs
src/Pkc.Knowledge/AiWorkspaceWriter.cs
tests/Pkc.CSharp.Tests/AiWorkspaceRendererTests.cs
```

The renderer regression must protect at least:

- bootstrap/policy/catalog/manifest generation;
- root CLAUDE/AGENTS preservation;
- stale workspace cleanup;
- path traversal rejection;
- source-free/product-default privacy declarations;
- source cross-check requiring approved context;
- deterministic-default / targeted-AI / full-checkpoint benchmark cadence.

## Benchmark cadence — mandatory but proportional

Read and follow:

`docs/benchmarks/product-value-benchmark-protocol.md`

Do **not** run a full AI benchmark after every change.

### Level 0 — deterministic, default

Every change gets the appropriate focused/related/full tests, build/smoke, authority/no-leak, workspace isolation and output assertions.

No AI reread is required if the change cannot materially alter product answers.

### Level 1 — targeted AI product-value

When a change can alter product answers, select only the affected pinned repo/workflow/questions.

Typical triggers:

- interface → implementation;
- permission/precondition extraction;
- state/default/computation recovery;
- side effects;
- frontend/API linking;
- displayed-value lineage;
- feature/workflow synthesis;
- answer authority materially changing answerability.

Use two phases:

```text
phase 1: .pkc/workspace only → record generated answer
phase 2: local approved source cross-check → known answer → score
```

Do not put source code in the report.

### Level 2 — full AI product-value

Run all pinned probes only for acceptance/release/demo checkpoints, major semantic/routing changes, broad regression risk, or explicit request.

A green deterministic safety result never becomes a product-value PASS automatically.

## Standard product-value questions

1. Who may perform the action and what business preconditions apply?
2. What state/data/default/computation changes after success?
3. Which UI action calls which API and where does the displayed value come from?
4. What evidence trace supports the answer?

Level 1 asks only the affected questions. Level 2 asks the full set.

## Current product-value baseline

Reference only; recompute after semantic changes:

```text
Agentic Users Update       80.8%
Jin12 Contacts Update      40.0%
Kesetovic PackOrder        65.0%
backend PO/QC core         72.6%
overall applicable         63.9%
```

## Audit / coding questions

Independently review at least:

1. Can `pkc run` modify anything outside `.pkc` on a clean target repo?
2. Can crafted canonical paths escape `.pkc/workspace`?
3. Can regeneration leave stale generated knowledge?
4. Are target-root team `CLAUDE.md` / `AGENTS.md` byte-for-byte preserved?
5. Does generated `CLAUDE.md` reliably route Opus/Claude Code to START_HERE, policy and catalog?
6. Does `AGENTS.md` provide equivalent routing for compatible agents?
7. Does PRODUCT stay source-free and avoid code/raw-fact dumps?
8. Is TRACE bounded to evidence references rather than code dumps?
9. Is ENGINEERING explicit about approved source-enabled context?
10. Can benchmark phase 1 accidentally climb into source before recording the workspace-only answer?
11. Can benchmark reporting accidentally serialize/copy proprietary code or raw facts?
12. Does manifest truthfully advertise PREVIEW/privacy/cadence metadata?
13. Does `pkc build` remain backward compatible?
14. Is repeated `pkc run` semantically idempotent for unchanged source?
15. Are symlink/reparse/file-system boundaries sufficiently fail-closed for company repos?

If a concrete defect exists, reproduce it regression-first and implement the minimum generic repair.

## Coding discipline

- Make related changes locally first.
- Add/update regressions.
- Run focused tests, then related tests, then broader relevant verification.
- Review the complete diff.
- Prefer one coherent implementation commit + one push.
- Do not use GitHub Actions as the edit/test loop.
- Do not broaden into unrelated product gaps in the same patch.
- After a semantic product change, run Level 1 targeted benchmark before deciding whether Level 2 is justified.

## Highest-value known semantic gaps after workspace audit

1. interface → concrete implementation traversal;
2. frontend event/service URL-expression linkage;
3. displayed-value lineage;
4. construction/default/computation state;
5. integration side-effect synthesis;
6. feature-summary fidelity;
7. R7.14 positive real-project yield without weakening authority.

Let benchmark evidence select the next repair; do not attack all gaps in one patch.
