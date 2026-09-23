# PKC Repository Discovery + Scan Planning Execution Plan

Date: 2026-09-24
Status: PREPARED / NOT YET UNLOCKED

This plan refines the existing AI-workspace `pkc run` roadmap with evidence from a sanitized reconnaissance of a large mixed legacy enterprise repository. It is intentionally a planning packet only. It does not change the current formal V0.4.7 gate, does not unlock V0.4.7-E, and does not authorize a new milestone while V0.4.7-D remains pending independent rereview.

Related documents:

- `docs/plans/2026-09-22-ai-workspace-continuous-update-plan.md`
- `docs/research/2026-09-24-large-legacy-repository-reconnaissance.md`
- `docs/reviews/2026-09-24-repository-discovery-scan-planning-self-review.md`

## Trigger contract

When a future session is told `start`, `continue`, or equivalent:

1. Read `docs/status.md` and `docs/handoff.md` first.
2. Verify current `main`, recent commits, working tree, current milestone, and exact acceptance gate.
3. Read the three related documents above.
4. Do not ask the user to restate this design.
5. If V0.4.7-D/R7.10 is still pending, do not start this plan as a new milestone. Complete the exact current gate first.
6. If D passes, follow the repository's updated roadmap. Do not skip V0.4.7-E if it is the newly unlocked checkpoint.
7. Only implement Repository Discovery when repository status/roadmap explicitly unlocks the relevant AI-workspace/run checkpoint or explicitly authorizes this work as bounded prework.
8. Once unlocked, execute regression-first and continue autonomously until the active checkpoint is locally PASS, reaches an external gate, or a proven external blocker remains.

## Why this plan exists

The current `pkc run <repository>` pipeline begins semantic C# and frontend scanning before it has a bounded model of repository applications, source ownership, vendor/runtime assets, tests, generated files, or runtime topology.

A sanitized real-project reconnaissance found a repository shape that makes that strategy unsafe and expensive:

- about 25k tracked files;
- about 65–70 .NET projects;
- about 18 likely runtime deployables plus tooling;
- multiple backend module generations;
- compile-time modules plus reflection-loaded runtime plugins;
- ASP.NET MVC + jQuery and multiple Angular generations coexisting;
- about 2.6k legacy JavaScript files, only about 350 first-party;
- commercial/vendor themes and libraries stored in source;
- locally modified vendor code;
- bundle outputs mixing first-party and third-party code;
- non-.NET production extensions;
- tests, generated artifacts, orphaned/stale areas, and runtime-injected configuration.

A manual product test of the current unbounded full-root run observed roughly 9 GB RAM consumption before useful completion. This is an operator observation, not a portable acceptance threshold, but it establishes that repository scope planning is a product and resource problem rather than only a progress-display problem.

The real-project research is design evidence, not source authority for PKC behavior and not an automated fixture by itself.

## Core product decision

`pkc run` must understand the repository shape before expensive semantic analysis.

Target pipeline:

```text
repository
  ↓
Repository Discovery
  ↓
Repository Profile + Application Graph + Source Classification
  ↓
Scan Planning
  ↓
Bounded Execution Waves
  ↓
Semantic Scanners
  ↓
Cross-stack composition
  ↓
Knowledge + verified workspace
```

Repository Discovery is deterministic product infrastructure. Claude/other LLM reconnaissance was used only to research the problem and produce a human-reviewed oracle. Runtime PKC must not require an LLM to classify the repository.

## Non-negotiable invariants

### 1. Discovery precedes semantic scanning

No expensive whole-repository C# or frontend semantic scan may begin before a repository profile and scan plan exist.

### 2. The repository is a graph, not one homogeneous source root

PKC must model application/component boundaries, ownership, project/workspace membership, shared nodes, and runtime/deployment evidence independently.

A shared module may have multiple owning hosts. Similar names are never sufficient to create an edge.

### 3. Discovery covers the whole repository; execution may be bounded

The scan plan must account for the whole repository even when semantic execution is divided into bounded waves.

A recommended first semantic slice is not permission to silently ignore the rest of the repository.

If a run intentionally analyzes only a subset, coverage must be explicit and the workspace must not claim full READY coverage for omitted applications.

### 4. Third-party runtime does not mean irrelevant

Third-party libraries/themes/plugins may be essential runtime dependencies while containing no product business logic.

Default handling:

```text
THIRD_PARTY_RUNTIME
→ index identity/ownership/runtime use
→ do not deep-scan internals
```

Locally modified/forked vendor areas require a narrow first-party carve-out rather than deep-scanning the entire vendor tree.

### 5. Unknown is a valid result

Uncertain ownership, runtime reachability, stale-looking areas, orphaned projects, dynamic loading, and vendor/custom boundaries must stay UNKNOWN until evidence proves more.

Do not turn `old`, `legacy`, `vendor`, `plugins`, `themes`, `Scripts`, `Content`, `packages`, or similar names into automatic exclusions.

### 6. Tests are evidence, not production authority

Tests may attach evidence to production nodes but must not inflate the production application/dependency graph or make a production path look live solely because a test hosts/references it.

### 7. Runtime edges require provenance

Reflection/plugin loading, build-copy targets, bundle membership, runtime configuration, and dynamic application wiring may produce runtime graph edges only when deterministic evidence supports them.

Do not call unsupported inferred links `synthetic` authority. Every promoted edge must carry its evidence kind and confidence.

### 8. Evaluated build/runtime truth beats misleading declarations

Declared globs/configuration are evidence, but where supported PKC should prefer evaluated project items and actual registration/load behavior. A declared exclusion that has no effect must not cause live source to be dropped.

### 9. Privacy remains strict

Configuration values, endpoints, credentials and source bodies must not leak into portable output. Discovery may index key names/types and topology evidence without copying sensitive values.

## Proposed deterministic model

Keep source role and scan mode separate so mixed areas can be represented precisely.

### Source role

Initial vocabulary:

```text
FIRST_PARTY_RUNTIME
FIRST_PARTY_SHARED
THIRD_PARTY_RUNTIME
THIRD_PARTY_MODIFIED
THIRD_PARTY_UNKNOWN
INFRASTRUCTURE
TEST_EVIDENCE
GENERATED_OR_RESTORABLE
UNKNOWN
```

### Scan mode

Initial vocabulary:

```text
DEEP_SCAN
TARGETED_SCAN
RUNTIME_DEPENDENCY_INDEX
LIGHT_INDEX
TEST_EVIDENCE
SAFE_AUTO_EXCLUDE
UNKNOWN
```

A physical directory is not required to have one classification. File/subtree overrides must be possible because legacy repositories can mix first-party scripts, vendor libraries, tracked bundles and locally modified vendor plugins in the same parent folder.

### Application node

At minimum:

```text
id
root(s)
role
technology stack
entry/composition evidence
project/workspace membership
owners
runtime/deployment evidence
confidence
```

### Dependency/runtime edge

At minimum:

```text
from
to
kind
confidence
evidence[]
```

Potential kinds include:

```text
project-reference
host-registration
runtime-plugin-load
build-copy
frontend-http
bundle-input
layout-load
shared-workspace-import
deployment-topology
```

Do not promote an edge from naming similarity alone.

### Scan decision

At minimum:

```text
area/application
sourceRole
scanMode
scanners
reason/evidence
confidence
explicit exclusions
coverage effect
```

## Discovery evidence hierarchy

Prefer cheap deterministic evidence before source semantics.

### Tier 0 — filesystem + VCS shape

- tracked/untracked distinction where Git is available;
- directory/file counts;
- known generated/restorable folders;
- extension/language inventory;
- source-control ignore rules as evidence, not sole authority.

### Tier 1 — manifests and build/workspace metadata

- `.sln` / `.slnx`;
- `.csproj` / project references;
- `Directory.Build.*` / central package config;
- `global.json`;
- `package.json` / lockfiles;
- `angular.json` / `tsconfig*`;
- legacy package/bundle manifests;
- Docker/build/pipeline metadata;
- known generated/restorable package roots.

### Tier 2 — bounded composition/runtime entry evidence

Only inspect a small set of high-value source/config files needed to resolve topology, such as:

- `Program`/startup/composition registration;
- DI/module registration;
- runtime plugin loaders;
- post-build copy/load targets;
- MVC route/bundle/layout registration;
- Angular bootstrap/workspace routing;
- runtime configuration key names without values.

Do not recursively enter business semantics during discovery.

### Tier 3 — targeted ambiguity resolution

Only when an important classification remains ambiguous, inspect the minimum evidence necessary to resolve it. If evidence stays insufficient, preserve UNKNOWN.

## Vendor and legacy JavaScript strategy

Directory-level rules are insufficient for the researched real project.

Discovery must be able to identify:

```text
first-party JS
third-party JS
minified/generated JS
bundled JS
wrapper/adapter JS
modified vendor JS
unknown JS
```

Deterministic signals may include:

- package/manifest membership;
- license/version banners;
- minified/source-map relationships;
- known distribution shapes;
- bundle input/output configuration;
- layout/script loading references;
- tracked custom plugin/patch files inside vendor trees;
- first-party wrapper relationships.

Do not require users to manually maintain a 300-file allowlist. PKC may produce an effective file-level scan set, but that set must be generated from evidence and remain inspectable.

Tracked bundle outputs are runtime evidence, not automatically deep-scan source. PKC should trace runtime bundle use back to classified inputs where possible.

## Runtime plugin strategy

Project-reference graphs are insufficient where modules are copied to an output folder and reflection-loaded.

Discovery should model the chain only when evidence supports it, for example:

```text
host loader
+ build/dependency/copy target
+ plugin assembly/project identity
→ runtime-plugin-load edge
```

The edge must preserve provenance and confidence. Missing or ambiguous plugin identity stays unresolved rather than being guessed.

## Scan-plan output

Preferred local output under the existing `.pkc` boundary:

```text
.pkc/
  discovery/
    repository-profile.json
    applications.json
    source-areas.json
    runtime-dependencies.json
    scan-plan.json
    coverage.json
```

The exact file split may change during implementation, but the machine-readable concepts must remain available.

Do not export raw config values or source bodies into the portable workspace. The workspace may receive a high-signal coverage summary sufficient to prevent false completeness claims.

## CLI behavior

Desired `pkc run .` UX before semantic analysis:

```text
PKC repository discovery
  projects ................. N
  application candidates ... N
  frontend workspaces ...... N
  runtime dependencies ..... N
  unknown areas ............ N

Scan plan
  deep ..................... N files/areas
  targeted ................. N
  runtime-index ............ N
  light-index .............. N
  tests .................... N
  excluded ................. N
  unknown .................. N

Starting bounded semantic analysis...
```

Numbers must come from the actual plan. Do not print guessed progress or coverage.

The existing progress-to-stderr behavior must remain compatible.

## Execution waves and memory

Discovery alone does not solve memory usage if all planned applications are then loaded simultaneously.

The executor should support bounded waves based on application ownership and shared dependencies:

```text
application wave
→ directly owned deep-scan roots
→ targeted closure into shared code
→ release/dispose analysis state when safe
→ next wave
```

Shared semantic contexts may be reused when this saves work without forcing the whole monorepo into memory. This must align with the existing future W6 shared-analysis-context plan rather than create a second competing architecture.

Correctness beats a fixed memory target. Initial acceptance should prove scope exclusion and bounded execution structurally, then record real peak memory/elapsed-time deltas on the private real repository. Do not invent an absolute RAM threshold before controlled measurement.

## Full-run completeness policy

The final product must distinguish:

```text
READY   all required planned areas supported and verified
PARTIAL some planned areas intentionally unsupported/failed/omitted with explicit coverage
FAILED  required semantic authority/preflight cannot be established
```

A bounded first wave may be useful for development/benchmarking, but it must not be presented as a complete repository pack unless the remaining plan is satisfied.

## Regression-first implementation slices

These slices refine the existing AI-workspace W5/W6 design. They do not create a separately unlocked roadmap milestone by themselves.

### RD1 — inventory + safe exclusion

Create a synthetic mixed repository fixture containing:

- multiple .NET apps/libraries/tests;
- one Angular workspace;
- legacy JS;
- generated/restorable folders;
- infrastructure/config files;
- ambiguous `legacy`/`vendor`-named first-party files.

Prove deterministic inventory and that only strongly proven generated/restorable areas become SAFE_AUTO_EXCLUDE.

Negative regression: a folder merely named `legacy`, `vendor`, `plugins`, `themes`, `Scripts`, or `Content` is never excluded by name alone.

### RD2 — application boundaries + ownership

Add fixtures for:

- API/composition host;
- MVC app;
- Angular app;
- worker;
- shared library;
- test host;
- shared module referenced by two production hosts.

Prove application identity, multi-owner shared nodes, and test isolation.

Negative regression: similarly named applications/modules do not become linked.

### RD3 — vendor/custom frontend classification

Add fixtures where one legacy scripts tree contains:

- first-party page JS;
- untouched vendor JS;
- minified output;
- a local wrapper;
- a custom plugin inside a vendor tree;
- a tracked bundle mixing first-party and vendor inputs.

Prove runtime dependency indexing, narrow modified-vendor carve-out, bundle provenance and generated file-level scan sets.

Negative regression: vendor internals never enter DEEP_SCAN merely because the vendor is runtime-used.

### RD4 — runtime/plugin edges

Add a compile-valid fixture where a plugin is absent from host project references but is copied by build metadata and reflection-loaded by deterministic identity.

Prove a provenance-bearing `runtime-plugin-load` edge.

Negative regressions:

- copy without loader => no authoritative runtime edge;
- loader without resolvable plugin identity => UNKNOWN;
- test-only plugin load => test evidence, not production edge.

### RD5 — deterministic scan plan

Given the above graph, produce a stable machine-readable plan assigning source role, scan mode, scanner set, evidence/confidence and coverage effect.

Prove stable ordering and no semantic dependence on wall-clock timestamps.

### RD6 — scoped scanner integration

Wire `pkc run` to execute existing scanners only against planned areas/application waves.

Keep `scan` / `build` compatibility until an explicit migration decision changes them.

Prove:

- SAFE_AUTO_EXCLUDE and RUNTIME_DEPENDENCY_INDEX internals never reach deep semantic scanners;
- TARGETED shared code is followed only from owning application scope;
- tests stay separate;
- UNKNOWN areas are not silently discarded;
- existing semantic authority/fail-closed behavior remains unchanged inside selected scope.

### RD7 — coverage + observability

Persist discovery/scan-plan artifacts and surface coverage to the generated workspace/verification boundary without leaking source bodies or config values.

Prove progress messages reflect actual phases and counts.

### RD8 — real-project validation

Use the sanitized research as the expected shape and the private source only in the approved environment.

Validate at least:

- application/deployable boundary quality;
- multiple frontend/workspace generations;
- compile-time and runtime plugin topology;
- multi-owner shared modules;
- first-party/vendor JS separation;
- modified-vendor carve-out;
- bundle-aware runtime mapping;
- test isolation;
- generated/restorable exclusion;
- UNKNOWN preservation;
- non-.NET production area visibility;
- semantic scope reduction;
- peak memory and elapsed time versus the observed unbounded baseline.

Do not copy proprietary source or exact internal names into PKC repository benchmark reports. Record only sanitized counts, classifications, deltas and failure categories.

## Acceptance matrix

Repository Discovery/Scan Planning is not accepted merely because it emits JSON.

At minimum:

| Property | Required outcome |
| --- | --- |
| deterministic inventory | PASS |
| application boundaries | supported fixture PASS |
| multi-owner shared modules | PASS |
| test isolation | PASS |
| safe exclusion | zero name-only exclusions |
| vendor runtime indexing | PASS |
| modified-vendor carve-out | PASS |
| bundle provenance | PASS for supported shape |
| runtime plugin provenance | PASS for supported shape |
| unsupported/ambiguous shapes | explicit UNKNOWN / fail-closed |
| scan-plan stability | PASS |
| deep scanner scope | excludes vendor/generated/test-only areas by plan |
| workspace completeness claims | READY/PARTIAL/FAILED honest |
| portable privacy | no source/config-value leakage |
| existing accepted semantic regressions | PASS |
| unchanged real-repo safety corpus | PASS when checkpoint requires it |
| private large-repo resource delta | measured and materially improved; exact threshold set from controlled rerun |

## Real-project oracle semantics

The sanitized reconnaissance is a design/acceptance oracle, not a source fixture.

It must not be hard-coded into PKC by application alias, product shape, folder name or specific technology combination.

A generic implementation should independently recover the supported structural properties from source/manifests/runtime wiring.

The private repository remains unchanged. Do not modify it to make PKC pass.

## Out of scope for this checkpoint

Unless a regression proves one is required for Repository Discovery itself, do not opportunistically implement:

- a generic JavaScript dataflow engine;
- full non-.NET ERP DSL business semantics;
- continuous update/diff;
- Azure DevOps evidence;
- arbitrary runtime instrumentation;
- automatic dead-code deletion;
- source cleanup recommendations;
- LLM-dependent repository classification;
- broad semantic refactors unrelated to scoped execution.

## Self-review corrections incorporated

This plan intentionally rejects several tempting but unsafe shortcuts:

1. **Do not deep-scan only the Claude-recommended first slice and call the repository complete.** The plan must cover all areas and express partial coverage honestly.
2. **Do not treat runtime plugin edges as unsupported synthetic guesses.** Require provenance-bearing deterministic edges.
3. **Do not require a user-maintained first-party JS allowlist.** Generate the effective scan set from evidence and keep UNKNOWN when unclear.
4. **Do not optimize Roslyn first.** Remove irrelevant semantic scope first; only then optimize reusable semantic contexts with regression protection.
5. **Do not auto-exclude vendor-looking or legacy-looking folders.** Mixed first-party/vendor repositories make path-name heuristics unsafe.
6. **Do not set a hardware-specific RAM acceptance number before controlled measurement.** Require bounded execution and measure the real delta.
7. **Do not let discovery become another full-source semantic scanner.** Use shallow, bounded evidence first.
8. **Do not create a second roadmap that bypasses V0.4.7.** This plan refines the prepared AI-workspace/run work and remains locked until repository status explicitly unlocks it.

## Required next action at plan creation time

At the time this plan was written, current `main` is:

```text
79a682fcb16683c0f5c7d70d2466c3349314e3c2
docs: hand off product-value checkpoint [skip ci]
```

Formal repository state remains:

```text
R7.10 / V0.4.7-D  pending independent rereview #17
V0.4.7-E          locked
AI-workspace W    not unlocked
```

Therefore the exact next formal action remains the independent rereview requested by current status/handoff. This plan may be implemented only after the active repository gate/roadmap explicitly allows it.
