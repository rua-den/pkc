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
co-located workspace Git isolation               IMPLEMENTED / VALIDATED
formal AI-workspace W acceptance                 NOT UNLOCKED / NOT COMPLETE
continuous update/diff                           LOCKED
V0.5 Azure DevOps input evidence                 LOCKED
```

The user explicitly authorized the AI-workspace preview before formal W unlock for real company-repository testing. This remains a productization spike and must not be used to claim V0.4.7-D, E, W, U, or V0.5 complete.

## Formal R7.10 candidate — separate gate

```text
96205a9a643864facaf9642a3b390ddcdbed59d9
fix: tolerate duplicate Angular import aliases
```

Formal R7.10/D acceptance still requires independent rereview #17:

`docs/reviews/2026-09-23-v0.4.7-d-r7.10-rereview-17-request.md`

Do not self-certify this candidate from an implementation continuation.

## Current AI workspace preview source

Latest exact workspace source:

```text
c45eb24809f181d48eac53abc64e8b5e816c57cc
feat: isolate colocated PKC workspace from Git
```

Relevant predecessors:

```text
6e845dc16718e74adac38e4aa49ee75023f99fa5  feat: generate AI product workspace
b76427f67b78ab8964284c1a6c43ec89e5656375  fix: isolate pkc run workspace
bf0ef686f2591841b85f36583a5fec7afb049060  feat: enforce workspace privacy and benchmark cadence
```

Preferred product command:

```text
pkc run <repository-path>
```

Generated product workspace is co-located with the target source under `.pkc/`:

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
      git-isolation.json
      source-context.json
```

## Co-location / privacy / Git boundary

The source repository and generated PKC workspace intentionally share one repository root, but they do not share the same evidence authority.

- PRODUCT is the default and reads generated `.pkc/workspace` only.
- TRACE may cite generated evidence paths/symbols but does not read or dump source bodies.
- ENGINEERING requires explicit intent plus an approved company/source-enabled context.
- benchmark phase 1 is workspace-only; benchmark phase 2 may inspect the minimum required source in the approved company environment.
- `_meta/source-context.json` records the source root as relative `../..`; it does not persist the absolute company checkout path.
- generated workspace remains source-free and must not contain proprietary source bodies, secrets, credentials, or raw facts.
- PKC controls generated workspace content/routing, not provider/network retention policy.

`pkc run` now protects co-located generated output from ordinary Git staging without mutating the team's tracked ignore rules:

- normal Git checkout: use `.git/info/exclude`;
- linked worktree/separate gitdir: follow `.git` `gitdir:` and `commondir`, then use the common Git `info/exclude`;
- add `/.pkc/` idempotently;
- never modify tracked `.gitignore`;
- record the result in `.pkc/workspace/_meta/git-isolation.json`;
- unsupported/non-Git/read-only layouts do not fail workspace generation; status is recorded instead.

Important limitation: Git ignore rules do not untrack files that are already tracked. If a team previously committed `.pkc/`, PKC does not rewrite the index or repository history; that must be handled explicitly by the team.

## Workspace behavior retained

- source-root `CLAUDE.md` is never overwritten;
- source-root `AGENTS.md` is never overwritten;
- `pkc run` does not generate root `knowledge/`, `PKC_KNOWLEDGE.md`, or `PKC_KNOWLEDGE.zip`;
- legacy `pkc build` retains those compatibility outputs;
- repeated `pkc run` replaces generated `.pkc/workspace` so stale generated files do not survive;
- generated paths may not escape `.pkc/workspace`;
- generated `CLAUDE.md` / `AGENTS.md` route agents through `knowledge/START_HERE.md`, `_policy/answer-contract.md`, and `_meta/catalog.json`;
- manifest remains `PREVIEW`; `pkc verify` / READY-PARTIAL-FAILED is still not implemented.

## Exact-SHA validation for co-located workspace checkpoint

Exact source:

```text
c45eb24809f181d48eac53abc64e8b5e816c57cc
```

Workflow runs:

```text
CI / full tests / WorkPlay / PokeTrade  35852404008  PASS
pinned Loren                            35852403984  PASS
Loren-main canary                       35852404005  PASS
pinned Jellyfin                         35852404020  PASS
```

Core verification on exact SHA:

```text
Release build       PASS — 0 warnings / 0 errors
C# tests            PASS — 265 / 265
Frontend tests      PASS — 13 / 13
local tool pack     PASS
WorkPlay            PASS
PokeTrade           PASS
Loren pinned        PASS
Loren main          PASS
Jellyfin parity     PASS
```

Focused regression:

`tests/Pkc.CSharp.Tests/PkcWorkspaceColocationTests.cs`

It covers normal checkout local exclusion, idempotence, tracked `.gitignore` preservation, worktree/common-git exclusion, source-context metadata, and non-Git graceful behavior.

This checkpoint is benchmark Level 0: it changes workspace/Git isolation, not extracted business semantics. Do not spend AI benchmark context on Agentic/Jin12/Kesetovic for this change.

## Real company/team repository flow

From an installed tool:

```text
pkc run <TEAM_REPOSITORY_PATH>
cd <TEAM_REPOSITORY_PATH>/.pkc/workspace
claude
```

Or from a PKC source checkout:

```text
dotnet build PKC.sln --configuration Release
dotnet run --project src/Pkc.Cli/Pkc.Cli.csproj --configuration Release --no-build -- run <TEAM_REPOSITORY_PATH>
cd <TEAM_REPOSITORY_PATH>/.pkc/workspace
claude
```

After the first run, `git status --short` in the team repository should not list newly generated `.pkc/` files when the checkout uses a supported Git layout and `.pkc/` was not already tracked. Inspect `_meta/git-isolation.json` if that expectation is not met.

## Product-value baseline / next semantic repairs

Reference known-answer baseline:

```text
Agentic Users Update       80.8%
Jin12 Contacts Update      40.0%
Kesetovic PackOrder        65.0%
backend PO/QC core         72.6%
overall applicable         63.9%
```

Safety/fail-closed remains PASS; product-value acceptance remains NOT PASS.

Highest-ROI semantic repair order remains:

1. interface → concrete implementation traversal;
2. frontend URL-expression resolution, then Angular output/event bridging if targeted evidence proves it remains necessary;
3. displayed-value lineage;
4. construction/default/computation state;
5. integration side-effect synthesis;
6. feature-summary fidelity;
7. R7.14 positive real-project yield without weakening authority.

For Fix #1, prefer project-semantic Roslyn proof:

```text
endpoint invocation
→ exact interface method symbol
→ proven DI registration
→ exact concrete implementing method
→ existing concrete guards / mutations / downstream calls
```

Do not match implementations by method name alone. Direct `AddScoped/AddTransient/AddSingleton<I,T>` registrations are the first bounded authoritative target. Ambiguous/multiple implementations must downgrade to uncertain/not grounded. Factory, assembly scanning, decorators, keyed/conditional registrations should be added only with deterministic proof and regressions.

## Benchmark cadence

Protocol:

`docs/benchmarks/product-value-benchmark-protocol.md`

```text
Level 0 — deterministic default
Level 1 — targeted AI for product-answer semantic changes
Level 2 — full AI only for acceptance/release/demo checkpoints, major semantic/routing change, broad regression risk, or explicit request
```

For Level 1/2, record the workspace-only answer before any source inspection. Source cross-check occurs only in the approved company Claude Code environment and reports behavior/evidence references, not proprietary source bodies.

## Company Claude / Opus continuation

Root `CLAUDE.md` is the bootstrap for Claude Code working on PKC source.

Read in order:

1. `CLAUDE.md`
2. `AGENTS.md`
3. `docs/status.md`
4. `docs/handoff.md`
5. documents named by the handoff
6. `docs/benchmarks/product-value-benchmark-protocol.md`
7. `docs/reviews/2026-09-23-ai-workspace-preview-company-audit-request.md`

The next semantic coding priority is Fix #1 interface→concrete implementation traversal, regression-first, with a targeted Jin12 Level-1 benchmark after deterministic validation. Formal R7.10 rereview #17 remains a separate gate unless the new session is explicitly assigned that independent reviewer role.

## Version semantics

```text
formal roadmap:       V0.4.7-D / R7.10 pending independent rereview #17
workspace preview:    implemented + validated PREVIEW
workspace placement:  co-located under target .pkc/, locally Git-excluded when supported
workspace privacy:    PRODUCT/TRACE workspace-only; approved ENGINEERING/source cross-check explicit
benchmark policy:     Level 0 deterministic / Level 1 targeted AI / Level 2 full checkpoint AI
tool/package:         RuaDen.Pkc.Tool 0.4.3-preview.2
C# raw schema:        0.4.4-csharp-raw
merged facts schema:  0.4.4
cross-stack schema:   0.4.6
frontend schema:      0.4.3-frontend
```
