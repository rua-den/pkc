# PKC Handoff

Last updated: 2026-09-24

This handoff authorizes demo-critical Repository Discovery work immediately.

## Latest checkpoint state

```text
RD1 inventory + safe exclusion   LOCAL PASS
implementation                   fd3428f06bae7089a749f59b3d802d5a4e36b160
                                 feat: discover repository shape before semantic scans
evidence                         docs/reviews/2026-09-24-rd1-inventory-safe-exclusion-evidence.md
push / CI                        PENDING — operator must push main
RD2                              ACTIVE
```

The implementing session could not push: the SSH remote rejected its key and HTTPS push was not permitted by that session's tool policy. Before relying on RD1 remotely, verify with `git ls-remote origin` that `main` contains `fd3428f`, then confirm the CI run for that push is green.

## Read first

1. root `CLAUDE.md`
2. `AGENTS.md`
3. `docs/status.md`
4. this handoff
5. `docs/milestones.md`
6. `docs/product-knowledge-contract.md`
7. `docs/v0.4.7-acceptance-plan.md`
8. `docs/plans/2026-09-24-demo-scan-priority-override.md`
9. `docs/plans/2026-09-24-demo-critical-sequential-execution-plan.md`
10. `docs/plans/2026-09-24-repository-discovery-scan-planning-plan.md`
11. `docs/reviews/2026-09-24-repository-discovery-scan-planning-self-review.md`

Then verify current `main`, recent commits, production code and relevant regressions. Never reset to an older SHA merely because this handoff names one.

## Priority decision

Formal R7.10 / V0.4.7-D remains OPEN and NOT ACCEPTED.

However, the user explicitly authorized E0 Repository Discovery as bounded demo-critical infrastructure prework while D remains open because the current whole-root scan is operationally impractical on the intended large mixed legacy repository.

Do not continue spending the current implementation session on R7.10 review/repair unless E0 work exposes a direct regression dependency.

This authorization does not self-certify D and must not weaken existing fail-closed semantic authority.

## Current active checkpoint

```text
V0.4.7-E0 Repository Discovery + bounded run
ACTIVE AS BOUNDED PREWORK

RD1 inventory + safe exclusion
LOCAL PASS at fd3428f / push + CI pending

RD2 application boundaries + ownership
ACTIVE
```

## RD2 scope

Extend the RD1 `RepositoryProfile` (do not create a competing model) with deterministic application/component boundaries and ownership:

- API/composition host, MVC app, Angular app, worker, shared library, test host;
- a shared module referenced by two production hosts is one node with multiple owners;
- project-reference edges come from MSBuild `ProjectReference` resolution only;
- tests attach as test evidence and never create production ownership or edges;
- similarly named applications/modules never become linked;
- ambiguous host/application identity stays UNKNOWN.

Discovery must stay shallow (manifests first). Do not implement vendor/frontend classification (RD3), runtime plugin edges (RD4) or scoped scanner execution (RD6).

Do not start RD3 until RD2 is explicitly PASS.

## Why RD1 was first

Current `pkc run <repository>` starts whole-root C# and frontend semantic scanning before repository/application/vendor/test scope is planned.

A private large mixed legacy repository reached roughly 9 GB RAM before useful completion. The demo needs a practical normal product path that produces `.pkc/workspace`.

The first-order fix is scope/discovery, not another semantic pass and not Roslyn micro-optimization.

## Zero-token product constraint

E0 and normal PKC repository compilation must run locally and deterministically.

DO NOT introduce runtime dependencies on:

- Claude API;
- OpenAI API;
- Gemini API;
- any hosted LLM;
- API keys;
- user AI tokens;
- uploading source to an LLM.

Claude/LLM reconnaissance is research/oracle material only.

PKC generation target:

```text
local source
→ local deterministic analyzers
→ .pkc/workspace
```

An AI reads the generated workspace afterward; AI generation is not part of repository compilation.

## RD1 scope

Implement inventory + safe exclusion only.

Create a regression-first synthetic mixed repository fixture containing at least:

- multiple .NET applications/libraries/tests;
- one Angular workspace;
- legacy JavaScript;
- generated/restorable directories;
- infrastructure/configuration files;
- ambiguous first-party directories named like `legacy`, `vendor`, `plugins`, `themes`, `Scripts`, `Content`, `old`, or `packages`.

Prove:

1. deterministic inventory;
2. shallow discovery does not require whole-repository semantic analysis;
3. only strongly proven generated/restorable areas become `SAFE_AUTO_EXCLUDE`;
4. ambiguous/name-only areas remain included or UNKNOWN;
5. tests are identified as test evidence rather than production authority;
6. the new run architecture has discovery/plan state before expensive C# or frontend semantic scanners begin;
7. output ordering is deterministic.

Do not solve RD2 application ownership yet except for the minimum model needed by RD1.

## E0 architecture direction

Target flow:

```text
repository
→ deterministic shallow discovery
→ RepositoryProfile
→ application/source/runtime graph
→ deterministic ScanPlan
→ bounded execution waves
→ existing semantic scanners
→ cross-stack composition
→ knowledge/workspace
```

RD1 should establish the minimum generic model cleanly enough for later RD2–RD7 without prematurely implementing them.

Prefer structural evidence first:

- filesystem/VCS shape;
- `.sln` / `.slnx`;
- `.csproj` and project references;
- `Directory.Build.*` / central package metadata;
- `package.json` / locks;
- `angular.json` / `tsconfig*`;
- legacy package/bundle manifests;
- build/deployment metadata.

Do not deep-read business source during discovery.

## E0 sequential order

```text
RD1 inventory + safe exclusion
→ RD2 application boundaries + ownership
→ RD3 vendor/custom frontend classification
→ RD4 runtime/plugin provenance
→ RD5 deterministic ScanPlan
→ RD6 scoped/bounded semantic execution
→ RD7 coverage + observability + plan-only inspection
→ RD8 private large-repository validation
→ practical .pkc/workspace generation
```

One active production checkpoint at a time.

## Git / CI discipline

For RD1:

```text
inspect current architecture
→ write focused failing regression
→ confirm regression represents the defect
→ minimum generic implementation
→ focused tests
→ related tests
→ broader relevant verification
→ review full diff
→ one coherent implementation commit
→ one push
→ CI final verification
→ update docs/status.md + docs/handoff.md
```

Do not push speculative intermediate fixes. CI is the final verification layer.

## Anti-stall rule

Continue autonomously on RD1 until one of these states:

1. RD1 locally PASS;
2. a genuinely external gate is required;
3. a genuinely external blocker is proven and documented.

Test/build/tool failures are not terminal states. Inspect, narrow, fix and continue.

## Existing semantic safety

E0 is infrastructure/scope work. Existing accepted semantic authority and fail-closed behavior must remain unchanged for source that is selected for scanning.

Do not opportunistically merge Fix #4 or add unrelated product semantics.

Parked candidate:

```text
fix/product-value-construction-state
35c8e5c5f856e15568aa963bb2d76268008c5570
```

## Formal D boundary

R7.10/D remains OPEN. Do not mark it PASS from this thread.

The independent review must be resumed before final V0.4.7 acceptance, but it no longer blocks demo-critical E0 infrastructure implementation.

## Known gaps carried from RD1

- RD1 does not reduce semantic scope or memory yet; scanners still receive the whole root.
- Scanner-internal name scopes (`CSharpSourceScope`, `FrontendSourceScope`) are unchanged accepted semantic behavior; RD6 must reconcile them with the plan without silently dropping UNKNOWN areas.
- VCS tracked state, libman/bower destinations, minified/vendor distributions and tracked bundles are not classified yet.

## Exact next action

```text
operator: push main and confirm CI green for fd3428f
implementation: RD2 application boundaries + ownership, regression-first
```

After RD2 is PASS, document exact HEAD, tests, remaining gaps, then unlock RD3 only.
