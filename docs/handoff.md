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

RD7 coverage + observability     LOCAL PASS
implementation                   d0c1017bd83d0d4b732facb5ddeddc5facfe48ca
                                 feat: add plan-only discovery and persisted scan coverage
evidence                         docs/reviews/2026-09-24-rd7-coverage-observability-evidence.md

push / CI                        PENDING — operator must push main
RD8                              ACTIVE — operator-run external gate
```

The implementing session could not push: the SSH remote rejected its key and HTTPS push was not permitted by that session's tool policy. Before relying on RD1–RD7 remotely, verify with `git ls-remote origin` that `main` contains `d0c1017` (and its docs successor), then confirm the CI run for that push is green.

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
LOCAL PASS at d0c1017 / push + CI pending

RD8 private large-repository validation
ACTIVE — operator-run external gate
```

## RD8 scope

Validate on the approved private mixed legacy repository, using sanitized measurements only, every bullet in `docs/plans/2026-09-24-demo-critical-sequential-execution-plan.md` § RD8 (boundaries, frontend generations, vendor separation and carve-outs, runtime/plugin topology, shared ownership, test isolation, generated/restorable exclusion, UNKNOWN preservation, semantic scope reduction, elapsed time and peak memory against the unbounded baseline, and `.pkc/workspace` generation).

Run only inside the approved source-enabled/company environment, against the approved private repository, without modifying it (work on a disposable copy if `.pkc/` must not be written into the original checkout):

```text
1. build PKC at the current main (Release)
2. pkc discover <private-repo-copy>
   → review [pkc:discover]/[pkc:plan] counts and .pkc/discovery/scan-plan.json locally
     (hosts/components, exclusions, test evidence, vendor/runtime-index, UNKNOWN areas)
3. pkc run <private-repo-copy>, measuring elapsed time and peak working set
   (e.g. PowerShell: Measure-Command + Get-Process peak WorkingSet64 sampling)
4. compare with the recorded unbounded baseline (~9 GB RAM before useful completion)
5. confirm .pkc/workspace is generated and _meta/coverage.json is plausible
6. record sanitized measurements only in docs/reviews/<date>-rd8-private-validation.md:
   counts, ratios, elapsed/peak memory, PASS/PARTIAL/FAIL per RD8 bullet —
   no proprietary names, paths, endpoints, source snippets or configuration values
```

A PKC-source session without that environment must not claim RD8. If the operator's run exposes gaps, reproduce them with synthetic fixtures and fix them regression-first here.

## First private-repository run — findings and follow-ups (2026-09-24)

Operator-authorized run in the approved company environment. Sanitized measurements only; RD8 is **not** yet formally PASS (see open items).

Repository scale:

- about 26.9k files;
- 18 hosts;
- about 16.8k planned semantic files;
- 1,982 test-evidence files and 41 light-index files withheld by the plan;
- 138 excluded areas;
- about 263.6k facts and 2.04M relations;
- 4,186 workflow candidates.

| Run | PKC state | Result | Elapsed | Peak memory |
| --- | --- | --- | --- | --- |
| 1 | `cf20b29` (RD7) | **failed** at `[pkc:write]` (out of memory) | 41.5 min | ~18.6 GB |
| 2 | `cf20b29` + working-tree fixes below | **exit 0**, workspace generated (703 product features, 4,709 files) | 29 min | ~7.5 GB |

`pkc discover` alone took about 98 s. Fact and relation counts were identical in runs 1 and 2.

Fixes, in the working tree and **not committed yet**:

- `src/Pkc.Core/JsonArtifactFile.cs` — `facts.json`, `feature-candidates.json` and `product-features.json` are streamed to disk. `facts.json` reached about 1.2 GB, which cannot be materialized as one string. The bytes are identical to the previous output (regression test `JsonArtifactFileRegressionTests`).
- `src/Pkc.CSharp/CSharpProjectSemanticEnricher.cs` — one shared `MSBuildWorkspace` per pass. Previously each project opened its own workspace and recompiled its whole reference closure, and every copy was kept alive by its semantic models; that was the source of the ~18 GB peak.
- Verification: `Pkc.CSharp.Tests` 333/333, `Pkc.Frontend.Tests` 23/23; sample `.pkc` output identical to the RD7 run.

Implemented after run 2, in the working tree and **not committed yet**:

- **Answer format** — `AiWorkspaceRenderer.RenderAnswerContract` now has an `## Answer format` section:
  - answer in the user's language;
  - translate code conditions into business language;
  - certainty markers ✅ proven / ⚠️ not proven / 💡 inferred;
  - PRODUCT template: short answer, a When/Then rules table, permissions, errors, not proven, links;
  - QA table `ID | Scenario | Preconditions | Steps | Expected result | Basis`;
  - TRACE evidence list without code bodies.

  This replaces the hand-added demo section. The private run's patched workspace is kept locally as `.pkc/legacy/2026-09-24-workspace` in the target repository, not in this repository.
- **Run summary** — `src/Pkc.Knowledge/RunSummary.cs`. `pkc run` prints an ASCII summary block and writes `.pkc/RUN_SUMMARY.md` and `.pkc/run-summary.json`. They record:
  - repository shape;
  - what was analyzed and skipped;
  - facts and relations;
  - knowledge counts;
  - grounded share per aspect (rules, permissions, state changes, side effects, UI);
  - largest areas;
  - phase timings;
  - per-artifact write status.

  Paths are repository-relative. `knowledge/START_HERE.md` gets a count-only "What this workspace contains" section with answer caveats derived from the grounding shares.
- **Failure-safe writes and resume:**
  - JSON artifacts are written to a temporary sibling file and then atomically replaced;
  - post-scan artifact writes are best-effort: failures are listed in the summary and the exit code is 1;
  - the AI workspace is written to `.pkc/workspace.staging` and swapped in. The replaced one is kept as `.pkc/workspace.previous`. If the swap is blocked (for example a session open inside the workspace), the new one is kept as `.pkc/workspace.new`;
  - after `facts.json` is written, `.pkc/checkpoint/scan-checkpoint.json` records the plan fingerprint, scanner build identity, scope, facts hash and coverage;
  - `pkc run|build <repo> --resume` reuses that scan only while all of these still match; otherwise it reports why and runs a full scan;
  - `--resume` does not see source edits made after the checkpoint, and the CLI says so.
- Verification: 367/367 tests (`ResumableRunRegressionTests` adds 11). Sample `.pkc` output is identical to before except the answer contract, START_HERE and the new summary/checkpoint files.

Follow-ups (proposals, not in code):

1. **Commit the fixes above** and write `docs/reviews/<date>-rd8-private-validation.md` from these measurements after the operator's next full private run.
2. The answer format is only a contract for the assistant; a Level 1 benchmark must confirm answers follow it.
3. The run summary is written for `pkc run` only; `build`/`scan` still print the older console lines.
4. **Remaining resource work:**
   - the ~7.5 GB peak now occurs after scanning, while the full fact document is held in memory;
   - the other C# enrichers still open a workspace per project group, which costs time;
   - discovery takes about 98 s on about 27k files.

   Profile before optimizing.
5. **Open RD8 items:**
   - the product-value quality of answers from this workspace has not been benchmarked; run a Level 1 targeted benchmark with 2–3 known questions;
   - the private repository exercised no runtime-dependency-index or runtime-plugin cases, so those RD8 bullets remain unproven on it.
6. **Knowledge gaps seen in the generated private workspace.** These are E1 semantic candidates, to be prioritized after E0 and reproduced with synthetic fixtures first:
   - **Mediator dispatch.** About 186 of about 4,000 workflows stop at a generic in-process mediator (`IMediator.InvokeAsync`-style), so the command/query handler holding the real permissions, rules and state changes is never reached, and those workflows show "No grounded information". This needs proven request-type → handler traversal, fail-closed when the handler is ambiguous.
   - **Custom authorization attributes.** Permissions show only the framework `[Authorize]`. Organization-specific attributes that carry claim/role/module requirements are not extracted, so permission answers are under-reported.
   - **UI linkage.** 0 workflows have `UI to backend` or `How to do it in the UI` evidence on this repository; screen-level answers are therefore weak. Needs frontend coverage for the legacy UI stacks actually present (per `_meta/coverage.json`, many files are not analyzable).
   - **Raw code expressions in knowledge.** Business rules and state changes are rendered as literal code conditions and initializers (sometimes multi-line). The workspace holds no source files, but it does hold these fragments. Either render them in business language or fence them clearly as evidence; until then, product wording must say "no source files, only short rule conditions".
   - Observed during source cross-checking: chained comparison expressions can mean something different from their apparent intent. PKC currently reports such conditions verbatim; it must never paraphrase them as simple intent without proof.

## RD7 scope (completed)

Let users inspect detection and planned scope before expensive execution, and see honest coverage afterwards:

- a plan-only entry point (for example `pkc discover <repo>` or `pkc run --plan-only <repo>`; choose one contract) that runs discovery + plan, persists `.pkc/discovery/*`, prints the summary and starts no semantic scanner;
- persist a local execution/coverage record under `.pkc/discovery/` (per stage: planned, executed, withheld by plan, withheld by scanner name scope, UNKNOWN) without source bodies or configuration values;
- surface a bounded, privacy-safe coverage summary at the generated workspace/verification boundary so product answers can state what was not analyzed;
- progress messages must reflect actual phases and counts.

RD7 is locally PASS (see evidence above).

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
operator: push main and confirm CI green for fd3428f, 05eadb1, ac3efd9, a37d936, c440eea, 280450c and d0c1017
operator: next full private `pkc run` with the working-tree changes (legacy workspace kept locally)
implementation: commit the working-tree changes (see "First private-repository run"), then
                Level 1 benchmark on the private workspace → remaining resource work
```

After RD8, fix any exposed gaps regression-first, then record E0 PASS only when the normal product path generates the workspace practically with honest coverage.
