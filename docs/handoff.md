# PKC Handoff

Last updated: 2026-09-24

This handoff authorizes demo-critical Repository Discovery work immediately.

## Latest checkpoint state

```text
RD1 inventory + safe exclusion   LOCAL PASS
implementation                   fd3428f06bae7089a749f59b3d802d5a4e36b160
                                 feat: discover repository shape before semantic scans
evidence                         docs/reviews/2026-09-24-rd1-inventory-safe-exclusion-evidence.md

RD2 application boundaries       LOCAL PASS
implementation                   05eadb1e44152f23bcf06134adfc89ae70572e47
                                 feat: model application boundaries in repository discovery
evidence                         docs/reviews/2026-09-24-rd2-application-boundaries-evidence.md

RD3 vendor/custom frontend       LOCAL PASS
implementation                   ac3efd9f2b3f26bd99ab6cf141b8716325165578
                                 feat: classify vendor and generated frontend files in discovery
evidence                         docs/reviews/2026-09-24-rd3-vendor-frontend-classification-evidence.md

RD4 runtime/plugin provenance    LOCAL PASS
implementation                   a37d936fc68c82e599f23da8b64bcb6ea85ef9fb
                                 feat: prove runtime plugin edges in repository discovery
evidence                         docs/reviews/2026-09-24-rd4-runtime-plugin-provenance-evidence.md

RD5 deterministic ScanPlan       LOCAL PASS
implementation                   c440eea592c0360e0b9cde3456b1dcbf39727cb0
                                 feat: build a deterministic scan plan before semantic scans
evidence                         docs/reviews/2026-09-24-rd5-deterministic-scan-plan-evidence.md

RD6 scoped semantic execution    LOCAL PASS
implementation                   280450cdaa77491b8a5ab6a45ae7f0cb8f0caa0d
                                 feat: run semantic scanners inside the scan plan scope
evidence                         docs/reviews/2026-09-24-rd6-scoped-semantic-execution-evidence.md

push / CI                        PENDING — operator must push main
RD7                              ACTIVE
```

The implementing session could not push: the SSH remote rejected its key and HTTPS push was not permitted by that session's tool policy. Before relying on RD1–RD6 remotely, verify with `git ls-remote origin` that `main` contains `280450c` (and its docs successor), then confirm the CI run for that push is green.

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
LOCAL PASS at 05eadb1 / push + CI pending

RD3 vendor/custom frontend classification
LOCAL PASS at ac3efd9 / push + CI pending

RD4 runtime/plugin provenance
LOCAL PASS at a37d936 / push + CI pending

RD5 deterministic ScanPlan
LOCAL PASS at c440eea / push + CI pending

RD6 scoped/bounded semantic execution
LOCAL PASS at 280450c / push + CI pending

RD7 coverage + observability + plan-only inspection
ACTIVE
```

## RD7 scope

Let users inspect detection and planned scope before expensive execution, and see honest coverage afterwards:

- a plan-only entry point (for example `pkc discover <repo>` or `pkc run --plan-only <repo>`; choose one contract) that runs discovery + plan, persists `.pkc/discovery/*`, prints the summary and starts no semantic scanner;
- persist a local execution/coverage record under `.pkc/discovery/` (per stage: planned, executed, withheld by plan, withheld by scanner name scope, UNKNOWN) without source bodies or configuration values;
- surface a bounded, privacy-safe coverage summary at the generated workspace/verification boundary so product answers can state what was not analyzed;
- progress messages must reflect actual phases and counts.

Do not start RD8 until RD7 is explicitly PASS.

## RD6 scope (completed)

Make the existing semantic scanners consume the persisted plan in `pkc run` instead of the whole root:

- SAFE_AUTO_EXCLUDE areas and RUNTIME_DEPENDENCY_INDEX internals never reach deep semantic scanners;
- targeted shared code is analyzed from owning application scope (plan waves), not by whole-root sweep;
- test evidence stays separate and never gains production authority;
- UNKNOWN areas are not silently discarded: they stay in scope (fail-open for visibility) or are reported with coverage effect;
- accepted semantic/fail-closed behavior is unchanged inside the selected scope (sample `.pkc` semantic artifacts stay byte-identical where the plan selects the same files);
- scanner-internal name scopes (`CSharpSourceScope`, `FrontendSourceScope`) are reconciled with the plan without widening production authority;
- heavy analysis state is released between bounded waves when safe;
- `scan` / `build` keep their current whole-root behavior until an explicit migration decision.

RD6 is locally PASS (see evidence above). Known gap: one bounded pass over the planned union; per-wave state release is deferred until RD8 measures memory.

## RD5 scope (completed)

Convert the discovery profile (areas, overrides, components, edges, generated artifacts) into a stable, inspectable plan:

- every enumerated file accounted for exactly once with source role, scan mode, scanner set, evidence reason, confidence and coverage effect;
- explicit exclusions (with evidence) and UNKNOWN areas listed, never dropped;
- application waves derived from host ownership (hosts + owned components; shared nodes listed once per wave that owns them; unowned UNKNOWN components kept in a separate wave);
- stable ordering, no timestamps, no absolute paths, no source/config values;
- staleness diagnosable without source bodies (e.g., input fingerprint from sorted paths + sizes of manifests actually read).

RD5 produces and persists the plan; it must not yet change what the semantic scanners receive (RD6).

RD5 is locally PASS (see evidence above).

## RD4 scope (completed)

Add provenance-bearing runtime/plugin edges to the same profile component graph:

- a plugin project absent from host project references, copied by deterministic build metadata into a host-loaded location and loaded by deterministic identity → `runtime-plugin-load` edge with loader, copy and identity evidence;
- copy without loader → no authoritative runtime edge;
- loader without resolvable plugin identity → UNKNOWN;
- test-only plugin load → test evidence, not a production edge.

Loader evidence is Tier-2 bounded composition evidence: inspect only the minimum files needed, never recursive business semantics. Unsupported loader shapes stay UNKNOWN.

RD4 is locally PASS (see evidence above).

## RD3 scope (completed)

Extend the same `RepositoryProfile` (areas + file-pattern overrides) so a legacy frontend tree can be separated into:

- first-party JavaScript;
- third-party runtime distributions (indexed, not deep-scanned);
- minified/generated output;
- tracked bundles and their declared inputs;
- first-party wrappers/adapters;
- locally added or modified files inside a vendor tree (narrow first-party carve-out).

Evidence must be deterministic and manifest/layout/banner based (package/manifest membership, license/version banners, minified/source-map relationships, bundle configuration, restore manifests). Directory names alone never classify. Vendor internals never enter DEEP_SCAN merely because the vendor is runtime-used. Unclear files stay UNKNOWN/included.

Do not implement runtime plugin edges (RD4) or scoped scanner execution (RD6).

RD3 is locally PASS (see evidence above).

## RD2 scope (completed)

Extend the RD1 `RepositoryProfile` (do not create a competing model) with deterministic application/component boundaries and ownership:

- API/composition host, MVC app, Angular app, worker, shared library, test host;
- a shared module referenced by two production hosts is one node with multiple owners;
- project-reference edges come from MSBuild `ProjectReference` resolution only;
- tests attach as test evidence and never create production ownership or edges;
- similarly named applications/modules never become linked;
- ambiguous host/application identity stays UNKNOWN.

Discovery must stay shallow (manifests first). Do not implement vendor/frontend classification (RD3), runtime plugin edges (RD4) or scoped scanner execution (RD6).

RD2 is locally PASS (see evidence above).

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
operator: push main and confirm CI green for fd3428f, 05eadb1, ac3efd9, a37d936, c440eea and 280450c
implementation: RD7 coverage + observability + plan-only inspection, regression-first
```

After RD7 is PASS, document exact HEAD, tests, remaining gaps, then unlock RD8 only.
