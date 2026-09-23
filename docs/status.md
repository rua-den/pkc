# PKC Status

Last updated: 2026-09-23

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                 PASS / COMPLETE
V0.4.5 real-repository generalization            PASS / COMPLETE
V0.4.6 business logic reconstruction             PASS / COMPLETE
V0.4.7-A origin and copy timing                  PASS / COMPLETE
V0.4.7-B computation and later change            PASS / COMPLETE
V0.4.7-C backend to API                          PASS / COMPLETE
V0.4.7-D API to UI / R7.9 binding                PASS / COMPLETE
V0.4.7-D mutation-causality blocker              PASS / CLOSED
V0.4.7-D API to UI / R7.10 joint visibility      REPAIRED / ALL GATES PASS / PENDING REREVIEW #17
V0.4.7-D overall                                 PENDING INDEPENDENT REREVIEW #17
V0.4.7-E product acceptance                      LOCKED behind D
R7.14 real-project positive yield                NOT PASS / REQUIRED FOR E
real-repo safety benchmark                       PASS
real-repo product-value benchmark                NOT PASS / PARTIAL USEFULNESS
AI workspace preview spike                       IMPLEMENTED / VALIDATED PREVIEW / USER-AUTHORIZED
formal AI-workspace W acceptance                 NOT UNLOCKED / NOT COMPLETE
continuous update/diff                           LOCKED
V0.5 Azure DevOps input evidence                 LOCKED
```

The user explicitly authorized the AI-workspace preview before formal W unlock because same-day testing on a real company repository is required. This is a productization spike only. It must **not** be used to claim V0.4.7-D, E, W, U, or V0.5 complete.

V0.4.7 acceptance remains defined by `docs/v0.4.7-acceptance-plan.md`; the permanent product contract remains `docs/product-knowledge-contract.md`.

## Formal R7.10 candidate still under independent review

```text
96205a9a643864facaf9642a3b390ddcdbed59d9
fix: tolerate duplicate Angular import aliases
```

Formal R7.10/D acceptance still requires independent rereview #17:

`docs/reviews/2026-09-23-v0.4.7-d-r7.10-rereview-17-request.md`

Do not self-certify this candidate from an implementation continuation.

## User-authorized AI workspace preview

Exact production source:

```text
6e845dc16718e74adac38e4aa49ee75023f99fa5  feat: generate AI product workspace
b76427f67b78ab8964284c1a6c43ec89e5656375  fix: isolate pkc run workspace
```

The new preferred product command is:

```text
pkc run <repository-path>
```

It compiles the existing PKC knowledge model and generates an isolated AI workspace:

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

### Workspace behavior

- source-root `CLAUDE.md` is never overwritten by `pkc run`;
- source-root `AGENTS.md` is never overwritten by `pkc run`;
- `pkc run` does not generate root `knowledge/`, `PKC_KNOWLEDGE.md`, or `PKC_KNOWLEDGE.zip`;
- legacy `pkc build` continues to generate those compatibility outputs;
- repeated `pkc run` replaces the generated `.pkc/workspace` so stale generated files do not survive;
- workspace paths are constrained to the generated workspace root;
- `CLAUDE.md` / `AGENTS.md` route the AI through `knowledge/START_HERE.md`, `_policy/answer-contract.md`, and `_meta/catalog.json`;
- PRODUCT is the default answer mode;
- TRACE and ENGINEERING require explicit user intent;
- unsupported behavior must remain unknown rather than invented;
- manifest status is deliberately `PREVIEW` because `pkc verify` and READY/PARTIAL/FAILED are not implemented yet.

Implementation regressions:

```text
tests/Pkc.CSharp.Tests/AiWorkspaceRendererTests.cs
```

They cover bootstrap/policy/catalog/manifest generation, root-agent-file preservation, stale workspace replacement, and path traversal rejection.

## Exact-SHA verification for workspace preview

All standard gates passed on exact source `b76427f67b78ab8964284c1a6c43ec89e5656375`:

```text
CI / full tests / WorkPlay / PokeTrade  35845593687 — PASS
pinned Loren                            35845593592 — PASS
Loren-main canary                       35845593715 — PASS
pinned Jellyfin                         35845593692 — PASS
```

Core:

```text
Release build        0 warnings / 0 errors
C# tests             262 / 262 PASS
frontend tests       13 / 13 PASS
tool pack/install    PASS
WorkPlay legacy build PASS
PokeTrade            PASS
```

Pinned Jellyfin remains stable:

```text
facts                 43,365
relations             195,316
workflow candidates   386
product features      116
knowledge Markdown    504 files
analysis modes        43,365 / 43,365 project-semantic
portable parity       PASS
artifact              10743487151
sha256                5c2f06b03236aae82dabc92eb674befcfe866a08afab0674921ee94faf088fa3
```

## Isolated `pkc run` smoke gate

Validation-only branch:

```text
benchmark/workspace-b76427
```

Wrapper commit / run:

```text
1ee7b0d523f29dcbb7f45494d2c9461e9d1027ce  test: run isolated AI workspace smoke
35845652352 — PASS
```

The wrapper adds only a validation workflow. It copied WorkPlay into a temporary target, created pre-existing root team `CLAUDE.md` and `AGENTS.md`, invoked `pkc run`, and verified:

```text
root CLAUDE.md unchanged                     PASS
root AGENTS.md unchanged                     PASS
no root knowledge/                           PASS
no root PKC_KNOWLEDGE.md                     PASS
no root PKC_KNOWLEDGE.zip                    PASS
workspace CLAUDE.md / AGENTS.md              PASS
workspace START_HERE / policy / catalog      PASS
workspace manifest PREVIEW/isolation fields  PASS
WorkPlay feature/workflow product rules      PASS
```

Observed run output:

```text
PKC scan complete: 34 facts, 30 relations, 3 workflow candidates
PKC run complete: 3 workflows, 1 product features
PKC AI workspace generated: <repo>/.pkc/workspace
Workspace status: PREVIEW
```

## Product-value benchmark contract

Product changes must not be accepted merely because CI is green or PKC exits 0.

Mandatory protocol:

`docs/benchmarks/product-value-benchmark-protocol.md`

Required loop:

```text
candidate
→ pkc run pinned real repository
→ open only generated .pkc/workspace as AI product context
→ answer standard PO/QC questions
→ record generated answers
→ inspect pinned source known answers
→ score PASS/PARTIAL/FAIL + percentages
→ report concrete misses
→ choose next highest-ROI repair
```

Standard questions cover:

1. permission + business preconditions;
2. state changes/defaults/computations;
3. UI → API → displayed-value lineage;
4. evidence-trace completeness.

Current pre-workspace product baseline remains approximately:

```text
Agentic Users Update       80.8%
Jin12 Contacts Update      40.0%
Kesetovic PackOrder        65.0%
backend PO/QC core         72.6%
overall applicable         63.9%
```

Detailed baseline:

`docs/benchmarks/2026-09-23-ai-question-answerability-benchmark.md`

These numbers must be recomputed after a material knowledge change; do not copy them forward as a PASS claim.

## Company Claude continuation

Root `CLAUDE.md` now bootstraps Claude into repository policy, current status/handoff, and the mandatory product benchmark protocol.

Primary audit request:

`docs/reviews/2026-09-23-ai-workspace-preview-company-audit-request.md`

Claude should independently audit `b76427...`, reproduce any defect regression-first, and use the benchmark protocol after product/workspace changes.

Formal D review #17 remains a separate gate unless the new Claude session is explicitly assigned that independent rereview role.

## Real team-repository test

The preview is now ready for real local testing on a company repository:

```text
pkc run <TEAM_REPOSITORY_PATH>
cd <TEAM_REPOSITORY_PATH>/.pkc/workspace
claude
```

For source checkout execution rather than installed tool:

```text
dotnet run --project src/Pkc.Cli/Pkc.Cli.csproj --configuration Release --no-build -- run <TEAM_REPOSITORY_PATH>
```

Do not commit `.pkc/` into the company repository unless that repository intentionally wants generated PKC artifacts under version control.

## Known highest-value product gaps

After the workspace boundary is independently audited, the existing known-answer benchmark still exposes:

1. interface → concrete implementation traversal;
2. frontend event/service URL-expression linkage;
3. displayed-value lineage;
4. construction/default/computation state;
5. integration side-effect synthesis;
6. feature-summary fidelity;
7. R7.14 positive real-project yield without weakening evidence authority.

Formal E remains locked behind independent D acceptance. Productization work may continue only inside the user-authorized preview scope until formal roadmap gates are reconciled.

## Version semantics

```text
formal roadmap:       V0.4.7-D / R7.10 pending independent rereview #17
workspace preview:    implemented + validated PREVIEW at b76427f67b78ab8964284c1a6c43ec89e5656375
tool/package:         RuaDen.Pkc.Tool 0.4.3-preview.2
C# raw schema:        0.4.4-csharp-raw
merged facts schema:  0.4.4
cross-stack schema:   0.4.6
frontend schema:      0.4.3-frontend
