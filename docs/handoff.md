# PKC Handoff

Last updated: 2026-09-23

This handoff is for the next Claude Code / Opus coding or audit session.

## Read first

1. root `CLAUDE.md`
2. `AGENTS.md`
3. `docs/status.md`
4. this handoff
5. `docs/milestones.md`
6. `docs/product-knowledge-contract.md`
7. `docs/v0.4.7-acceptance-plan.md`
8. `docs/plans/2026-09-22-ai-workspace-continuous-update-plan.md`
9. `docs/reviews/2026-09-22-ai-workspace-continuous-update-plan-self-review.md`
10. `docs/benchmarks/product-value-benchmark-protocol.md`
11. `docs/benchmarks/2026-09-23-ai-question-answerability-benchmark.md`
12. `docs/reviews/2026-09-23-ai-workspace-preview-company-audit-request.md`
13. `docs/reviews/2026-09-23-v0.4.7-d-r7.10-rereview-17-request.md` only when working on the formal R7.10/D gate

Then inspect current `main`, recent commits, working tree state, production code and relevant regressions. The repository is the source of truth; never reset to a historical SHA merely because this handoff names it.

## Keep these two states separate

### Formal V0.4.7 state

```text
A/B/C                    PASS / COMPLETE
R7.9                     PASS / COMPLETE
mutation-causality       PASS / CLOSED
R7.10                    REPAIRED / ALL GATES PASS / PENDING REREVIEW #17
V0.4.7-D                 PENDING INDEPENDENT REREVIEW #17
V0.4.7-E                 LOCKED
R7.14                    NOT PASS / REQUIRED FOR E
```

Formal R7.10 candidate:

```text
96205a9a643864facaf9642a3b390ddcdbed59d9
fix: tolerate duplicate Angular import aliases
```

Do not self-certify this candidate from its implementation continuation.

### User-authorized AI workspace preview

The user explicitly required same-day company-repository testing, so the isolated workspace preview is allowed before formal W unlock.

Current exact workspace/privacy production source:

```text
bf0ef686f2591841b85f36583a5fec7afb049060
feat: enforce workspace privacy and benchmark cadence
```

Predecessors:

```text
6e845dc16718e74adac38e4aa49ee75023f99fa5  feat: generate AI product workspace
b76427f67b78ab8964284c1a6c43ec89e5656375  fix: isolate pkc run workspace
```

This preview does not mark formal W complete.

## Preferred user flow

```text
pkc run <repository-path>
cd <repository-path>/.pkc/workspace
claude
```

Direct source-checkout flow:

```text
dotnet build PKC.sln --configuration Release
dotnet run --project src/Pkc.Cli/Pkc.Cli.csproj --configuration Release --no-build -- run <repository-path>
cd <repository-path>/.pkc/workspace
claude
```

The user should not have to open or upload `PKC_KNOWLEDGE.md` for the preferred UX.

## Generated workspace contract

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

Required boundaries:

- target root `CLAUDE.md` untouched;
- target root `AGENTS.md` untouched;
- no root legacy knowledge artifacts from `pkc run`;
- stale workspace files removed on regeneration;
- output paths confined to `.pkc/workspace`;
- PRODUCT default reads generated workspace only;
- TRACE is explicit and may cite paths/symbols without code dumps;
- ENGINEERING is explicit and requires approved source-enabled context;
- generated workspace contains product knowledge, not source code/raw facts;
- manifest remains `PREVIEW` until `pkc verify` exists.

## Company-source privacy boundary

Treat proprietary target source as confidential.

- Source inspection/editing occurs only in the approved company Claude Code / enterprise source-enabled environment.
- Never copy proprietary source-code bodies, source files, secrets, credentials, or raw fact payloads into PKC repo docs, public issues, generated workspaces or benchmark reports.
- Benchmark reports may contain business behavior, scores, endpoint names, source paths, symbol names and concise evidence descriptions.
- Do not commit the target company repository or its `.pkc/` output into PKC.
- PKC controls generated workspace content and routing, not provider/network retention policy. Use the company-approved Claude environment for private source.

## Benchmark cadence

Protocol:

`docs/benchmarks/product-value-benchmark-protocol.md`

Do **not** run full AI benchmark after every edit.

### Level 0 — deterministic, default

For every change run the appropriate focused/related/full tests and deterministic gates. Examples include build, regression, authority/no-leak checks, workspace isolation, expected files, output diff and idempotence.

No AI reread is needed when the change cannot alter product answers.

### Level 1 — targeted AI product-value

Use when a change can alter answers. Select only the affected pinned repo/workflow/questions.

Two-phase rule:

```text
phase 1: open only generated .pkc/workspace → answer + record
phase 2: inspect minimum required pinned/company source locally in approved environment → known answer → score
```

Never paste source code into the report.

Typical Level-1 triggers:

- interface → implementation traversal;
- permissions/preconditions;
- state/default/computation extraction;
- side effects;
- frontend/API linkage;
- displayed-value lineage;
- feature/workflow synthesis;
- authority change that affects answerability.

### Level 2 — full AI product-value

Full Agentic/Jin12/Kesetovic corpus only for acceptance/release/demo checkpoints, major semantic/routing changes, broad regression risk, or explicit request.

Safety PASS is never automatically product-value PASS.

## Why `bf0ef686...` does not require full AI Q&A

This checkpoint changes workspace privacy/routing instructions, manifest declarations, benchmark cadence and their regressions. It does not change extracted business semantics or canonical knowledge.

Use Level 0 deterministic validation for this checkpoint. Future semantic fixes should use Level 1 targeted AI benchmark first.

## Exact validation for `bf0ef686...`

Required exact-SHA runs:

```text
CI / full tests / WorkPlay / PokeTrade  35848727746
pinned Loren                            35848727772
Loren-main canary                       35848727756
pinned Jellyfin                         35848727793
```

The docs handoff should only claim this checkpoint validated when all four are PASS.

Previous isolated end-to-end `pkc run` smoke remains relevant for unchanged CLI/writer mechanics:

```text
branch   benchmark/workspace-b76427
commit   1ee7b0d523f29dcbb7f45494d2c9461e9d1027ce
run      35845652352 — PASS
```

Current privacy/cadence content is covered by `AiWorkspaceRendererTests`.

## Company Claude next action

Primary request:

`docs/reviews/2026-09-23-ai-workspace-preview-company-audit-request.md`

Claude Opus / Claude Code should:

1. verify current `main` and exact workspace/privacy source;
2. audit generated routing, privacy/no-code-dump behavior and benchmark cadence;
3. reproduce any concrete defect regression-first;
4. implement the minimum generic fix;
5. run focused → related → broader deterministic validation;
6. select benchmark Level 0/1/2 based on semantic impact rather than habit;
7. for Level 1/2, record workspace-only answer before source inspection;
8. source-cross-check only inside approved company environment;
9. report behavior/scores/evidence references, not proprietary code;
10. use one coherent implementation commit/push when possible;
11. update status/handoff after a meaningful verified checkpoint.

If the workspace/privacy boundary is sound, continue to the next highest-value product gap only within the user-authorized preview scope while formal D/E/W gates remain separate.

## Real company repository test

```text
pkc run <TEAM_REPOSITORY_PATH>
cd <TEAM_REPOSITORY_PATH>/.pkc/workspace
claude
```

Normal PO/QA use should stay in the workspace.

If benchmarking the team repository:

```text
1. ask from workspace only and record answer
2. source-cross-check locally in approved company Claude Code
3. score match
4. report misses without code dump
```

Do not commit `.pkc/` unless the target repository explicitly wants generated PKC artifacts under version control.

## Known product-value gaps

Current known-answer corpus points to:

1. interface → concrete implementation traversal;
2. frontend event/service URL-expression linkage;
3. displayed-value lineage;
4. construction/default/computation state;
5. integration side-effect synthesis;
6. feature-summary fidelity;
7. R7.14 positive real-project yield without weakening authority.

Use targeted benchmark evidence to select one high-ROI repair at a time.

## Terminal state

Keep moving on the assigned checkpoint until one of:

1. PASS and verified;
2. a required external review/gate cannot be performed in-session;
3. a genuinely external blocker is proven and documented.

Test/build/tool failures and first unsuccessful approaches are not terminal states.
