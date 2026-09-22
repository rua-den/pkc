# Self-Review — AI Workspace + Continuous Update Plan

Date: 2026-09-22
Reviewed plan: `docs/plans/2026-09-22-ai-workspace-continuous-update-plan.md`
Verdict: READY AS A FUTURE EXECUTION PLAN / NOT CURRENTLY UNLOCKED

## Review question

Does the plan produce a usable PKC product for QC/PO without breaking current evidence authority, milestone discipline, repository instructions, or update correctness?

## Findings

### 1. Current milestone state must remain authoritative — PASS WITH REQUIRED GATE

The plan originally risks sounding like permission to immediately implement `run/update` even though current repository docs still have V0.4.7-D under independent review and E locked.

Required correction is now explicit:

```text
future `start`
→ inspect current main/status/handoff
→ finish current V0.4.7 checkpoint(s)
→ only then open the next productization milestone
```

The plan itself must never be treated as an unlock signal.

### 2. Current HEAD/documented production mismatch — BLOCKER TO FUTURE EXECUTION UNTIL RECONCILED

During this planning session `main` advanced while the plan was being prepared. The final observed HEAD before the planning commit was:

```text
7dd00c1a960b8e85232d67b779e7906b4206cce6
fix: require SVG text ancestor
```

Current `docs/status.md` / handoff still describe the independent-review candidate:

```text
f9b20c27820a7ea9ac911222c613c2f9cdfb696f
fix: reject SVG resource renders
```

Therefore a future execution session must not assume either SHA is the accepted production state merely from this plan. It must reconcile current review/status history first and must not reset `main` to the older documented candidate.

This is intentionally not repaired by this planning commit because the current production/review checkpoint requires its own evidence and review discipline.

### 3. Writing agent files into target source root — REJECTED

Generating `CLAUDE.md` or `AGENTS.md` into a customer's repository root could overwrite or conflict with real team instructions.

Accepted design:

```text
source repository
  .pkc/
    workspace/
      CLAUDE.md
      AGENTS.md
      knowledge/...
```

The workspace is isolated and `.pkc/` is already an ignored internal boundary in PKC's own repository model.

Portable export may place those bootloaders at archive root after extraction, because that extracted directory is the generated product workspace, not the source repository.

### 4. "No code by default" as prompt-only safety — REJECTED

Instruction wording alone is not a sufficient boundary.

Accepted design has two layers:

1. answer contract says PRODUCT is default and code is explicit-only;
2. portable QC/PO workspace does not contain source code.

Therefore accidental code dumping is materially reduced. ENGINEERING mode requires a source-enabled context and must not fabricate unavailable code.

### 5. PO and QA split into separate packs — REJECTED

Duplicated PO/QA packs would drift.

Accepted design:

```text
one canonical knowledge graph
+ audience lens / answer policy
```

QA can derive scenarios from proven behavior while PO receives business explanation from the same source.

### 6. Generic "GPT" bootstrap semantics — NEEDS PRECISE PRODUCT CLAIM

`AGENTS.md` is appropriate for Codex/agent workflows that honor repository instructions. It must not be documented as proof that every ChatGPT surface automatically consumes `AGENTS.md`.

Initial support language should be precise:

- Claude Code workspace: `CLAUDE.md` bootloader;
- Codex-compatible source/workspace agent: `AGENTS.md` bootloader;
- ChatGPT upload/manual workspace surfaces: retain portable bundle/archive path unless that surface explicitly supports project instruction discovery.

Do not overclaim cross-client auto-bootstrap behavior.

### 7. Always building every detected frontend/project — REJECTED

A mixed real repository may contain old apps, non-buildable samples, generated projects or frontend packages that are unnecessary for current source analysis.

Accepted design:

```text
adapter declares prerequisites
→ prerequisite planner
→ only required preparation
```

.NET target-project semantic authority must not silently degrade. Existing Angular bounded source analysis does not require an npm build merely because package metadata exists.

### 8. Silent semantic fallback in `run` — REJECTED

Current scanner fallback can be useful evidence, but a product command must not call a degraded result READY without telling the user.

Accepted states:

```text
READY
PARTIAL
FAILED
```

`--allow-partial` is explicit. Coverage records exact downgrade reasons.

### 9. Fine-grained incremental invalidation as first implementation — REJECTED

File/fact-level incremental compilation is attractive but too risky before persistent semantic identity and dependency coverage are proven.

Accepted sequence:

```text
stable semantic identity
→ coarse project/application invalidation
→ full rebuild fallback whenever closure is uncertain
→ optimize only after equivalence evidence
```

Correctness beats update speed.

### 10. Persistent identity based on fact ID/source line — REJECTED

Source line movement would cause false delete/add churn and make semantic update untrustworthy.

Stable canonical knowledge identity is a hard prerequisite for incremental merge.

A regression must prove that harmless line insertion/formatting does not change semantic knowledge identity.

### 11. Markdown merge as update mechanism — REJECTED

Generated Markdown is not the merge source of truth.

Accepted architecture:

```text
source/evidence
→ canonical knowledge graph
→ semantic diff/merge
→ Markdown projection
```

Generated files may be deleted and rendered again without losing authoritative state.

### 12. Git-only baseline — ACCEPTED ONLY WITH SAFE FALLBACK

Git revision ancestry is the preferred update baseline for team/CI workflows, but PKC must not corrupt knowledge when Git is absent, shallow, rewritten or divergent.

Accepted behavior:

- compatible ancestor baseline => incremental candidate;
- incompatible/divergent/unknown history => full rebuild;
- non-Git repository => fingerprint/full-rebuild path until stronger support is proven.

### 13. Updating baseline before validation — REJECTED

The accepted baseline may advance only after workspace verification passes.

Failed/partial attempts must not overwrite the last known-good baseline unless an explicit partial-baseline contract is designed later.

### 14. Source change equals product change — REJECTED

`pkc diff` must report semantic product knowledge change, not just changed files.

A large code refactor may have zero product changes. A one-line condition change may have significant QA impact.

### 15. Human edits to generated knowledge — REJECTED

Human/product annotations must be a separate input source with explicit authority/provenance. Generated Markdown remains replaceable output.

This preserves future Azure DevOps integration and requirement-vs-code comparison.

### 16. Roadmap ordering — RECOMMEND CHANGE AFTER V0.4.7 ONLY

Current roadmap places Azure DevOps before incremental compilation. The new product goal depends first on a usable consumer workspace and then on reliable continuous updates.

Recommended future priority:

```text
close V0.4.7
→ AI workspace + run/verify
→ update/diff/change report
→ Azure DevOps intent/history
```

Reason: adding more evidence before stabilizing how QC/PO consume and refresh the knowledge increases output volume without solving the primary user experience.

Do not rewrite active milestone docs while V0.4.7 is still open. Re-baseline once it is explicitly complete.

## Acceptance holes checked

The reviewed plan now explicitly covers:

- existing source `CLAUDE.md`/`AGENTS.md` collision;
- mixed MVC/Angular/JS repositories;
- semantic build/preflight failure;
- partial analysis disclosure;
- no-change idempotence;
- add/change/delete source behavior;
- stale knowledge removal;
- project/config topology changes;
- history divergence;
- tool/schema incompatibility;
- source-code leakage;
- agent answer-code leakage;
- semantic routing/context explosion;
- baseline advancement after failure;
- full-run versus incremental-update parity;
- product-oriented change reports;
- CI/main as accepted team baseline.

## Deferred by design

The plan intentionally does not solve yet:

- perfect fine-grained dependency invalidation;
- arbitrary TypeScript/JavaScript semantics;
- runtime browser truth;
- automatic approved-intent inference;
- code embedding for engineering users;
- bidirectional manual editing of generated knowledge;
- LLM-dependent source analysis.

These are not blockers for the first productization/update implementation.

## Execution readiness

The design is sufficiently concrete to begin implementation without asking the user to restate requirements once the current milestone is unlocked.

Future agent instruction:

```text
If user says `start`:
1. reconcile current main/status/handoff and finish the current PKC checkpoint;
2. once V0.4.7 is explicitly complete, open the next roadmap checkpoint according to the execution plan;
3. implement regression-first in the W slices;
4. do not begin U until W passes;
5. update status/handoff after each accepted checkpoint;
6. prefer one coherent implementation commit/push per logical checkpoint after local validation.
```
