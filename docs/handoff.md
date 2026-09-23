# PKC Handoff

Last updated: 2026-09-23

Use this file when continuing PKC in another coding/review thread, especially the company Claude audit requested by the user.

## Read first

For Claude Code, root `CLAUDE.md` is the bootstrap. Then read in this order before changing production code:

1. `AGENTS.md`
2. `docs/status.md`
3. this handoff
4. `docs/milestones.md`
5. `docs/product-knowledge-contract.md`
6. `docs/v0.4.7-acceptance-plan.md`
7. `docs/plans/2026-09-22-ai-workspace-continuous-update-plan.md`
8. `docs/reviews/2026-09-22-ai-workspace-continuous-update-plan-self-review.md`
9. `docs/benchmarks/product-value-benchmark-protocol.md`
10. `docs/benchmarks/2026-09-23-ai-question-answerability-benchmark.md`
11. `docs/reviews/2026-09-23-ai-workspace-preview-company-audit-request.md`
12. `docs/reviews/2026-09-23-v0.4.7-d-r7.10-rereview-17-request.md` when working on the formal R7.10/D gate

Then inspect current `main`, recent commits, working tree state, production code and relevant regressions. Never reset to a historical SHA merely because this handoff names it.

## Two states must remain separate

### 1. Formal V0.4.7 acceptance state

```text
A/B/C                                  PASS / COMPLETE
R7.9                                   PASS / COMPLETE
mutation-causality                     PASS / CLOSED
R7.10                                  REPAIRED / ALL GATES PASS / PENDING REREVIEW #17
V0.4.7-D                               PENDING INDEPENDENT REREVIEW #17
V0.4.7-E                               LOCKED
R7.14                                  NOT PASS / REQUIRED FOR E
```

Formal R7.10 candidate under review:

```text
96205a9a643864facaf9642a3b390ddcdbed59d9
fix: tolerate duplicate Angular import aliases
```

The implementation thread that authored R7.10 repairs must not self-certify it. Rereview #17 remains an external formal gate.

### 2. User-authorized AI workspace preview spike

The user explicitly required a usable Claude/Codex workspace today for a real company repository, so this isolated productization slice was implemented before formal W unlock.

Exact source:

```text
6e845dc16718e74adac38e4aa49ee75023f99fa5  feat: generate AI product workspace
b76427f67b78ab8964284c1a6c43ec89e5656375  fix: isolate pkc run workspace
```

This preview is implemented and validated, but it does **not** mark formal W complete.

## Current preferred user flow

```text
pkc run <repository-path>
cd <repository-path>/.pkc/workspace
claude
```

For direct source checkout execution:

```text
dotnet build PKC.sln --configuration Release
dotnet run --project src/Pkc.Cli/Pkc.Cli.csproj --configuration Release --no-build -- run <repository-path>
cd <repository-path>/.pkc/workspace
claude
```

The target AI should start from generated `CLAUDE.md` and should not require the user to manually choose or upload `PKC_KNOWLEDGE.md`.

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

Important boundaries:

- target root `CLAUDE.md` untouched;
- target root `AGENTS.md` untouched;
- `pkc run` emits no root `knowledge/`, `PKC_KNOWLEDGE.md`, or `PKC_KNOWLEDGE.zip`;
- `pkc build` still emits legacy compatibility artifacts;
- generated workspace is replaced on the next run so stale generated files are removed;
- output paths may not escape `.pkc/workspace`;
- default answer mode is PRODUCT;
- TRACE is explicit;
- ENGINEERING is explicit and honest about source availability;
- manifest currently says `PREVIEW` because `pkc verify` / READY-PARTIAL-FAILED is not implemented.

Primary implementation files:

```text
src/Pkc.Cli/Program.cs
src/Pkc.Knowledge/AiWorkspaceRenderer.cs
src/Pkc.Knowledge/AiWorkspaceWriter.cs
tests/Pkc.CSharp.Tests/AiWorkspaceRendererTests.cs
```

## Exact validation on `b76427...`

All standard gates PASS:

```text
CI / full tests / WorkPlay / PokeTrade  35845593687 — PASS
pinned Loren                            35845593592 — PASS
Loren-main canary                       35845593715 — PASS
pinned Jellyfin                         35845593692 — PASS
```

```text
Release build        0 warnings / 0 errors
C#                   262 / 262 PASS
frontend             13 / 13 PASS
tool pack/install    PASS
WorkPlay             PASS
PokeTrade            PASS
```

Pinned Jellyfin remains:

```text
43,365 facts
195,316 relations
386 workflows
116 product features
504 knowledge Markdown files
43,365 / 43,365 project-semantic
portable parity PASS
artifact 10743487151
sha256:5c2f06b03236aae82dabc92eb674befcfe866a08afab0674921ee94faf088fa3
```

## Isolated workspace smoke

Validation-only wrapper:

```text
branch   benchmark/workspace-b76427
commit   1ee7b0d523f29dcbb7f45494d2c9461e9d1027ce
run      35845652352 — PASS
```

The wrapper adds one smoke workflow only; source under test is exact `b76427...`.

It runs `pkc run` on a temporary WorkPlay copy with pre-existing team root `CLAUDE.md`/`AGENTS.md` and verifies:

```text
root team instructions preserved    PASS
legacy root knowledge absent        PASS
workspace bootloaders present       PASS
START_HERE / policy / catalog       PASS
manifest PREVIEW/isolation fields   PASS
known WorkPlay product rules        PASS
```

Do not merge the wrapper workflow blindly. Decide during audit whether an equivalent workspace smoke belongs permanently in CI.

## Mandatory product-value benchmark

The user explicitly requires agents to benchmark the **actual generated AI workspace**, not merely count facts or read `PKC_KNOWLEDGE.md`.

Protocol:

`docs/benchmarks/product-value-benchmark-protocol.md`

After any product-knowledge/workspace change:

```text
candidate code
→ pkc run pinned real repo
→ AI opens only .pkc/workspace
→ ask standard PO/QC questions
→ record generated answer before source inspection
→ inspect pinned source known answer
→ compare + score
→ report percentages + concrete misses
→ choose next highest-ROI repair
```

Standard questions:

1. Who may perform the action and what preconditions apply?
2. What state/data changes after success?
3. Which UI action calls which API and where does the displayed value come from?
4. What evidence trace supports the answer?

Use PASS / PARTIAL / FAIL / N/A plus explicit percentage components. Do not turn a safety PASS into a product-value PASS.

Reference baseline:

```text
Agentic Users Update       80.8%
Jin12 Contacts Update      40.0%
Kesetovic PackOrder        65.0%
permission/preconditions   90.0%
state changes              61.0%
UI/API/value lineage       25.0%
evidence trace             66.7%
backend PO/QC core         72.6%
overall applicable         63.9%
```

These are baseline evidence only. Recompute them after a material knowledge change.

## Company Claude next action

Primary request:

`docs/reviews/2026-09-23-ai-workspace-preview-company-audit-request.md`

The company Claude session should:

1. independently audit exact workspace source `b76427...` against the isolation/routing/answer-contract boundary;
2. reproduce any concrete defect with a focused regression;
3. implement the minimum generic repair;
4. run focused/related/full validation locally when available;
5. push one coherent checkpoint rather than using CI as an edit loop;
6. run the mandatory workspace product-value benchmark after changes that affect output or routing;
7. record exact scores and concrete misses in the handoff;
8. keep formal D/E/W acceptance claims separate from the user-authorized preview.

If the audit finds the workspace boundary sound, the preview can be used immediately on the real company/team repository.

## Real team-repository validation

This is the next highest-value external/product check:

```text
pkc run <TEAM_REPOSITORY_PATH>
cd <TEAM_REPOSITORY_PATH>/.pkc/workspace
claude
```

Then benchmark the actual team workspace using the same product-value protocol. For company/private source, keep source and generated artifacts within the company-approved environment.

If `.pkc/` is not already ignored in the team repository, do not accidentally commit generated PKC output.

## Known product-value gaps after workspace boundary audit

The current known-answer corpus still points to:

1. interface → concrete implementation traversal;
2. frontend event/service URL-expression linkage;
3. displayed-value lineage;
4. construction/default/computation state;
5. integration side-effect synthesis;
6. feature-summary fidelity;
7. R7.14 positive real-project yield without weakening authority.

Do not opportunistically attack all of them in one patch. Let the mandatory benchmark identify the highest-value next repair.

## Terminal-state rule

For the workspace audit/code continuation, keep moving until one of these is reached:

1. current workspace checkpoint is PASS and locally/exact-SHA verified;
2. a required external review/gate cannot be performed in-session;
3. a genuinely external blocker is proven and documented.

Test failure, build failure, command failure, tool failure, a hanging process, ambiguity, or the first unsuccessful approach are not terminal states.
