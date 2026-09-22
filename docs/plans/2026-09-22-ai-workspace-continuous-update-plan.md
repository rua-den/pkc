# PKC AI Workspace + Continuous Update Execution Plan

Date: 2026-09-22
Status: PREPARED / NOT YET UNLOCKED

This document is the execution packet for the next PKC productization work discussed on 2026-09-22. It is intentionally a plan only. It does not unlock work beyond the current milestone.

## Trigger contract

When the user says only `start`, `continue`, or equivalent in a future PKC session:

1. Read `docs/status.md` and `docs/handoff.md` first.
2. Inspect current `main`, recent commits, working state and the current milestone gate.
3. Read this plan and its self-review:
   - `docs/plans/2026-09-22-ai-workspace-continuous-update-plan.md`
   - `docs/reviews/2026-09-22-ai-workspace-continuous-update-plan-self-review.md`
4. Do not ask the user to restate this design.
5. If the current V0.4.7 checkpoint is still open, continue and close that checkpoint first. Do not start a later milestone merely because this plan exists.
6. Once V0.4.7 is explicitly PASS / COMPLETE, re-baseline the roadmap to place AI-workspace/run productization and continuous update/diff ahead of additional evidence-source expansion unless the repository already records a newer explicit decision.
7. Execute the next unlocked checkpoint regression-first and continue autonomously until PASS, an external review gate, or a proven external blocker.

## Product objective

PKC should feel like a compiler that turns a mixed source repository into a ready-to-use AI product workspace.

Target user experience:

```text
pkc run [repository]
    ↓
validated source/semantic preparation
    ↓
evidence + canonical product knowledge
    ↓
validated portable AI workspace
    ↓
QC / PO opens that workspace in Claude or GPT/Codex and asks questions normally
```

QC/PO must not need to know which Markdown file to read, which order to read it in, how Roslyn works, or which backend/frontend framework produced the evidence.

## Product principles

### 1. Product answers by default, code only when explicitly unlocked

The default audience is Product / QA, not engineering.

Default answers may contain:

- product behavior;
- business conditions;
- workflows;
- permissions;
- validations;
- outcomes and failures;
- visible behavior;
- state transitions;
- value lineage in business language;
- QA scenarios and expected results;
- uncertainty and unsupported boundaries.

Default answers must not dump:

- source code;
- code snippets;
- raw Roslyn facts;
- fact IDs;
- class/method walkthroughs;
- implementation detail that is unnecessary to answer the product question.

Use three disclosure levels:

```text
PRODUCT      default; behavior/business/QA language
TRACE        explicit request for where/how the behavior is implemented; paths/endpoints/evidence allowed, no code dump
ENGINEERING  explicit request for code/implementation details; requires source access outside the portable product workspace
```

Product and QA are answer lenses, not separate knowledge packs. There is one canonical knowledge source.

The portable QC/PO workspace must not contain source code. Therefore the no-code default is enforced both by instructions and by data minimization, not prompt wording alone.

### 2. One canonical knowledge model, multiple projections

Do not create separate PO knowledge and QA knowledge that can drift.

Canonical product knowledge should render into:

- product-friendly Markdown;
- QA-friendly answers through agent policy;
- semantic routing/index data;
- machine-readable catalog/manifest data;
- change reports;
- current single-file portable bundle for compatibility.

### 3. Agent instruction files are bootloaders, not the database

Claude and GPT/Codex entry files must stay small and route the agent into the generated knowledge.

Do not place generated `CLAUDE.md` or `AGENTS.md` in the target source root because a real repository may already own those files and team instructions.

Generate an isolated workspace under the existing ignored `.pkc/` boundary:

```text
.pkc/
  workspace/
    CLAUDE.md
    AGENTS.md
    knowledge/
      START_HERE.md
      index.md
      system/
      features/
      workflows/
      quality/
      changes/
    _policy/
      answer-contract.md
    _meta/
      manifest.json
      catalog.json
```

For sharing, export the workspace as a portable archive. When extracted, the archive root contains `CLAUDE.md`, `AGENTS.md` and `knowledge/`.

Keep current `PKC_KNOWLEDGE.md` / ZIP compatibility until an explicit migration removes them.

### 4. Minimal bootstrap context

`CLAUDE.md`, `AGENTS.md` and `knowledge/START_HERE.md` must be small routing documents. They must not preload the full knowledge pack.

Target behavior:

```text
agent bootstrap
→ START_HERE
→ semantic index/catalog
→ only relevant feature/workflow documents
→ answer
```

Large real repositories already generate hundreds of knowledge files; the product must not require reading all of them before every question.

### 5. Honest coverage is part of the product

Every workspace must expose a global quality report, for example:

```text
source revision
PKC version/schema
build/preflight status
projects discovered
projects loaded semantically
frontend applications discovered
analysis modes / downgrade counts
unsupported areas
portable parity/no-leak status
```

The agent must know when it cannot claim completeness.

## `pkc run` design

### CLI surface

Primary UX:

```text
pkc run
pkc run <repository-path>
```

Advanced overrides may include:

```text
--out <path>
--solution <path>
--configuration <name>
--allow-partial
--no-build
```

Do not require normal users to specify framework/backend/frontend/output format switches when discovery can determine them.

Retain `scan` and `build` as compatibility/developer commands until explicitly retired.

### Pipeline

```text
repository discovery
→ adapter prerequisite planning
→ source/semantic preflight
→ shared analysis context
→ evidence compilation
→ cross-stack composition
→ canonical knowledge synthesis
→ workspace rendering
→ workspace verification
→ READY / PARTIAL / FAILED
```

### Repository discovery

Treat the repository as a graph of applications/components, not one homogeneous project.

Discover at minimum:

- Git root/revision when available;
- `.sln` / `.slnx`;
- C# projects and project references;
- `Directory.Build.*` / package configuration relevant to semantic loading;
- `angular.json` and Angular app boundaries;
- `package.json` / `tsconfig*` boundaries;
- legacy/mixed frontend areas that existing adapters can claim;
- shared libraries.

Each adapter owns its prerequisites and supported scope.

### C#/.NET preparation

Current PKC opens C# projects with `MSBuildWorkspace` and can downgrade to loose Roslyn when project semantic loading fails. `run` must not silently present a degraded pack as fully ready.

Preferred architecture:

```text
RepositoryAnalysisContext
  repository root/revision
  discovered solution/project graph
  build/preflight report
  one reusable MSBuildWorkspace/project graph
  cached Compilation/SemanticModel per project
  frontend application contexts
  analysis coverage
```

Prefer solution-level loading when an authoritative solution exists. Otherwise load discovered projects through one reusable workspace/context rather than creating independent workspaces per analyzer or project.

Default `.NET` preparation for `run` should prove a usable semantic environment. Restore/build behavior belongs to adapter prerequisite planning rather than a universal hard-coded command.

Rules:

- build/restore failure that prevents target-project semantic authority => `FAILED` by default;
- `--allow-partial` may continue but must produce `PARTIAL` with explicit coverage/downgrade reporting;
- never silently convert a broken semantic target into a READY pack.

### Frontend preparation

Do not run `npm install` / `npm build` merely because `package.json` exists.

Existing bounded Angular/source adapters can analyze source without a full frontend build. Future TypeScript semantic adapters may declare package/TypeScript prerequisites when evidence proves they are needed.

## Workspace answer contract

Generate `_policy/answer-contract.md` and reference it from both bootloaders.

Required default behavior:

- answer the user's actual product/QA question first;
- translate implementation evidence into business language;
- do not volunteer code;
- do not volunteer class/method names unless trace is useful or requested;
- distinguish observed implementation behavior from approved intent when intent evidence is unavailable;
- preserve uncertainty;
- use only relevant knowledge documents;
- do not infer unsupported behavior from names or conventions;
- for QA questions, derive test scenarios only from proven behavior and clearly label inferred test ideas versus proven expected outcomes.

TRACE is unlocked only by an explicit implementation-location/technical-trace request.

ENGINEERING is unlocked only by an explicit code/implementation request. A portable QC/PO workspace contains no source, so it may point the user back to a source-enabled workspace rather than fabricate code.

## `pkc verify` design

`pkc run` may report READY only after workspace validation.

Minimum gates:

```text
semantic/preflight policy             PASS
canonical knowledge references        PASS
internal workspace links/catalog      PASS
provenance references                 PASS
unsupported-authority leak checks     PASS
source-code/raw-fact leakage          PASS
CLAUDE bootstrap routing              PASS
AGENTS bootstrap routing              PASS
portable workspace parity             PASS
```

`pkc verify` validates an existing generated workspace without rebuilding source evidence where possible.

## Continuous update model

### Core rule

`pkc update` is a semantic update of canonical evidence/knowledge, never a line-based merge of generated Markdown.

```text
source diff
→ invalidation set
→ evidence recomputation
→ canonical knowledge diff
→ render
→ verify
```

Generated Markdown is a projection and may be recreated at any time.

### Baseline manifest

Initial `run` stores a machine-readable baseline containing at least:

- source repository identity/fingerprint;
- Git source revision when available;
- PKC tool/schema versions;
- discovered component/application graph;
- project/config fingerprints;
- source file fingerprints needed for non-Git fallback/full rebuild decisions;
- canonical knowledge node identities;
- source/evidence dependencies for invalidation;
- workspace generation timestamp/version metadata that does not participate in semantic identity.

### Stable identity prerequisite

Incremental update must not use source line numbers or ephemeral fact IDs as the persistent identity of product knowledge.

Before incremental merge is allowed, define stable semantic identities for canonical knowledge nodes, based on proven semantic/project identities and workflow/feature identity rules.

A harmless line insertion must not appear as deletion + recreation of unrelated product knowledge.

### Safe update algorithm

```text
load baseline
→ verify compatibility
→ determine current revision/fingerprints
→ classify changed/added/deleted/config files
→ map files to owning components/projects/adapters
→ invalidate the minimum SAFE closure
→ recompute affected evidence
→ rebuild affected canonical knowledge
→ remove knowledge whose authoritative sources disappeared
→ calculate semantic knowledge diff
→ render affected projections
→ verify whole workspace
→ advance baseline only on PASS
```

Correctness beats speed. Initial implementation should use coarse project/application-level invalidation when dependency closure is uncertain.

If safety cannot be proven, perform a full rebuild instead of risking stale knowledge.

### Full-rebuild triggers

At minimum:

- incompatible PKC/schema version;
- baseline repository mismatch;
- Git history divergence where baseline ancestry cannot be established;
- solution/project topology changes whose impact cannot be bounded;
- global build/package/TypeScript configuration changes whose invalidation closure is uncertain;
- adapter discovery/ownership changes;
- missing/corrupt baseline;
- explicit `--full`.

### No-change idempotence

Running update with no semantic source change must produce zero semantic knowledge changes and must not churn generated files due only to timestamps/order/nonsemantic IDs.

## `pkc diff` design

Preview product impact without changing the accepted baseline/workspace.

Examples:

```text
pkc diff <base>..<head>
pkc diff
```

Output should be product/QA oriented:

```text
Added behavior
Changed behavior
Removed behavior
Affected workflows/features
Potential QA regression areas
Authority/coverage changes
Unknown/unproven impacts
```

Do not equate changed source files with changed product behavior.

## Change report

After successful update generate a knowledge-level change report, for example:

`knowledge/changes/latest.md`

It should answer:

- what product behavior changed;
- previous versus current proven behavior;
- which workflows/features are affected;
- what QA should retest;
- what evidence/authority became weaker or stronger;
- what was removed because its source disappeared.

The report should not default to code diff language.

## Multi-developer / CI model

For a repository with many contributors, the accepted knowledge baseline should be produced from an accepted branch/revision, normally CI after merge.

Preferred future flow:

```text
PR
→ pkc diff base..head
→ product-impact artifact for review/QA
→ merge
→ pkc update on accepted branch
→ verified versioned workspace
→ QC/PO consume accepted workspace
```

Do not allow every developer's local generated workspace to become an independent source of truth.

## Human-authored knowledge

Never ask users to edit generated Markdown because update will regenerate it.

Future human/product annotations must live in a separate authoritative input boundary, e.g. `overrides/` or an integrated external evidence source, and compile into canonical knowledge with explicit authority/provenance.

Azure DevOps evidence remains a separate authority source and must not silently overwrite code-observed behavior.

## Implementation sequencing

### Gate 0 — reconcile current milestone before any new work

At plan creation time, repository docs still describe V0.4.7-D/R7.10 around `f9b20c27...`, while current `main` is newer. A future execution session must reconcile current HEAD/status/review state first.

Do not start this plan while V0.4.7-D or E is still open.

### Gate 1 — close V0.4.7 completely

Finish the currently defined cross-layer PO-question readiness acceptance, including any independent review, R7.14 positive real-project yield, portable parity and E acceptance required by current repository docs.

Only after V0.4.7 is explicitly PASS / COMPLETE may roadmap ordering be changed.

### Recommended roadmap re-baseline after V0.4.7

Product usability should precede adding another major evidence source.

Preferred order:

```text
1. AI workspace + run/verify productization
2. continuous update/diff + change reports
3. Azure DevOps intent/history evidence
4. later runtime/product-insight work
```

Do not edit active milestone labels now merely because this is the preferred future order. Update `docs/milestones.md`, `docs/status.md`, `docs/handoff.md`, README and acceptance plans together only when V0.4.7 has closed and the next milestone is formally opened.

## Checkpoint W — AI workspace + run/verify

Regression-first acceptance slices:

### W1 — isolated portable workspace

Prove:

- generated workspace lives under `.pkc/workspace` (or an explicitly configured output), not target source root;
- existing source `CLAUDE.md` / `AGENTS.md` are untouched;
- workspace contains its own bootloaders;
- current portable bundle behavior is preserved.

### W2 — answer contract

Fixture/acceptance questions must prove:

- normal PO question answers contain business behavior but no code/class/method dump;
- QA question yields business-derived test scenarios;
- explicit trace request may expose source paths/endpoints/evidence references;
- explicit engineering request never fabricates source when the portable pack lacks it.

### W3 — semantic routing

Prove bootloaders route to a small START_HERE/index/catalog and relevant knowledge can be located without loading the whole pack.

### W4 — coverage + manifest

Prove READY/PARTIAL/FAILED and coverage metadata are deterministic and portable.

### W5 — `pkc run`

Prove mixed repository discovery, adapter prerequisites, build/semantic failure behavior, `--allow-partial`, and compatibility with current `scan/build` outputs.

### W6 — shared C# analysis context

Replace repeated project-semantic loading only behind regression coverage proving no accepted semantic authority is lost. Prefer one solution/project graph and reusable compilations. Do not refactor unrelated analyzers opportunistically.

### W7 — verify

Prove bootstrap, references, no-leak, parity and coverage gates. `run` reports READY only when verify passes.

### W acceptance

Run focused regressions, full relevant tests, Release build, known-answer projects, pinned real repositories and portable parity/no-leak. Add a knowledge-only QC/PO acceptance where an agent can open only the generated workspace and answer named questions without source access or manual file-order instructions.

## Checkpoint U — continuous update + diff

Do not start U until W passes.

Regression-first slices:

### U1 — baseline compatibility

Manifest repository/revision/tool/schema compatibility and safe full-rebuild fallback.

### U2 — stable canonical identity

Line movement/nonsemantic formatting must not recreate product knowledge identities.

### U3 — no-change idempotence

No semantic source change => zero semantic knowledge diff and no projection churn.

### U4 — change/add/delete

At least one known-answer fixture for each, including removal of stale knowledge after source deletion.

### U5 — coarse safe invalidation

Project/application-level invalidation first. Prove unaffected projects remain semantically identical. If the boundary is uncertain, full rebuild.

### U6 — config/topology invalidation

`csproj`, solution, `Directory.Build.*`, package/Angular/TypeScript config and adapter-boundary changes trigger the correct wider rebuild.

### U7 — history divergence

Non-ancestor baseline or incompatible repository state must not apply an unsafe incremental merge.

### U8 — `pkc diff`

Preview must not mutate baseline/workspace and must report semantic product changes rather than file counts.

### U9 — product change report

Changed rule/workflow should render previous/current behavior and QA impact without default code snippets.

### U10 — real-repository equivalence

For pinned repositories:

```text
full run at revision B
==
run at revision A + update to B
```

for canonical semantic knowledge and portable workspace, except explicitly nonsemantic metadata.

This parity gate is mandatory before update is trusted.

## Non-goals for the first implementation

Do not initially attempt:

- perfect file-level dependency invalidation;
- arbitrary JavaScript/TypeScript semantic solving beyond current adapter contracts;
- runtime browser/UI execution;
- source-code embedding in QC/PO workspace;
- user-editable generated Markdown merging;
- automatic conflict resolution between human intent and code-observed behavior;
- an LLM-dependent compiler core.

## Definition of done for this initiative

The initiative is complete only when:

1. a developer/CI can run one simple PKC command on a supported mixed repository;
2. broken semantic prerequisites cannot silently produce a READY pack;
3. the generated workspace can be opened directly by the supported Claude/GPT agent workflow without manual file-order prompting;
4. normal PO/QC questions produce product/QA answers without unsolicited code;
5. the workspace communicates its own authority and coverage limits;
6. update can safely move an accepted baseline forward without stale knowledge;
7. unsafe incremental states fall back to full rebuild;
8. diff reports product impact without mutating the accepted baseline;
9. full-run and run+update converge to equivalent canonical knowledge on real pinned repositories;
10. all existing accepted knowledge/provenance/fail-closed invariants remain intact.
