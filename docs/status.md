# PKC Status

Last updated: 2026-09-24

## Current priority state

```text
V0.4.4 Loren knowledge readiness                   PASS / COMPLETE
V0.4.5 real-repository generalization              PASS / COMPLETE
V0.4.6 business logic reconstruction               PASS / COMPLETE
V0.4.7-A origin and copy timing                    PASS / COMPLETE
V0.4.7-B computation and later change              PASS / COMPLETE
V0.4.7-C backend to API                            PASS / COMPLETE
V0.4.7-D / R7.9 API to rendered value              PASS / COMPLETE
V0.4.7-D / R7.10 joint visibility                  OPEN / REVIEW PAUSED / NOT ACCEPTED
V0.4.7-E0 repository discovery + bounded run       ACTIVE / USER-AUTHORIZED BOUNDED PREWORK
V0.4.7-E0 / RD1 inventory + safe exclusion         LOCAL PASS at fd3428f / PUSH + CI PENDING
V0.4.7-E0 / RD2 application boundaries + ownership LOCAL PASS at 05eadb1 / PUSH + CI PENDING
V0.4.7-E0 / RD3 vendor/custom frontend            LOCAL PASS at ac3efd9 / PUSH + CI PENDING
V0.4.7-E0 / RD4 runtime/plugin provenance         LOCAL PASS at a37d936 / PUSH + CI PENDING
V0.4.7-E0 / RD5 deterministic ScanPlan            LOCAL PASS at c440eea / PUSH + CI PENDING
V0.4.7-E0 / RD6 scoped/bounded semantic execution LOCAL PASS at 280450c / PUSH + CI PENDING
V0.4.7-E0 / RD7 coverage + observability          LOCAL PASS at d0c1017 / PUSH + CI PENDING
V0.4.7-E0 current sub-checkpoint                   RD8 private large-repository validation (operator-run; external gate)
V0.4.7-E1 remaining product-value repairs          LOCKED behind E0
V0.4.7-E2 final product acceptance / R7.14         LOCKED behind E1
continuous update/diff                             LOCKED
V0.5 Azure DevOps input evidence                   LOCKED
```

## Explicit priority override

The user explicitly reprioritized the demo-critical path because the current whole-root `pkc run` is not operationally usable on the intended large mixed legacy repository.

The private observation of roughly 9 GB RAM before useful completion is evidence of a product-operability defect, not a universal numeric threshold.

Repository Discovery / bounded execution is therefore authorized now as bounded infrastructure prework even though formal R7.10/D independent acceptance remains open.

This does NOT mark D PASS and does NOT allow E0 to weaken or redefine existing semantic authority.

Authoritative override:

`docs/plans/2026-09-24-demo-scan-priority-override.md`

## Current main checkpoint before E0 implementation

At authorization time `main` was:

```text
087fa6368ebfc028f91ea9d6b34ecd39a82fdb70
fix: require proven Angular component import bindings
```

All five GitHub checks visible for that HEAD were green, including test, PokeTrade, Jellyfin, Loren and Loren-main. Formal R7.10 independent acceptance remains separate.

## RD1 — LOCAL PASS

```text
fd3428f06bae7089a749f59b3d802d5a4e36b160
feat: discover repository shape before semantic scans
```

Evidence: `docs/reviews/2026-09-24-rd1-inventory-safe-exclusion-evidence.md`.

- Release build 0 warnings; `Pkc.CSharp.Tests` 302/302, `Pkc.Frontend.Tests` 23/23; focused RD1 + CLI ordering tests 11/11.
- `pkc scan|build|run` now discovers and persists `.pkc/discovery/repository-profile.json` before any semantic scanner stage (`DiscoveryFirstScanPipeline`).
- `SAFE_AUTO_EXCLUDE` only with structural proof; name-only exclusion mutation is caught.
- Sample `pkc run` artifacts (facts, candidates, product features, workspace) byte-identical to `ab24363`.
- Push/CI pending: the implementing environment could not push (SSH key rejected; HTTPS push not permitted by session tool policy). Operator must push `main`; CI on that push is the final verification.
- RD1 does not yet reduce semantic scope or memory; scanner-internal name scopes are unchanged and must be reconciled in RD6.

## RD2 — LOCAL PASS

```text
05eadb1e44152f23bcf06134adfc89ae70572e47
feat: model application boundaries in repository discovery
```

Evidence: `docs/reviews/2026-09-24-rd2-application-boundaries-evidence.md`.

- Profile schema `0.2.0-discovery` adds manifest-derived components, `project-reference` edges, unresolved references and solution membership.
- Shared modules list every production host that reaches them through unconditional resolved references; tests are `TEST_ONLY` and never own; similar names never link; ambiguous identity/references stay UNKNOWN.
- Release build 0 warnings; `Pkc.CSharp.Tests` 308/308, `Pkc.Frontend.Tests` 23/23; RD1 + RD2 + CLI focused tests 17/17; test-ownership mutation caught.
- Sample `.pkc` semantic artifacts still byte-identical to `ab24363`.

## RD3 — LOCAL PASS

```text
ac3efd9f2b3f26bd99ab6cf141b8716325165578
feat: classify vendor and generated frontend files in discovery
```

Evidence: `docs/reviews/2026-09-24-rd3-vendor-frontend-classification-evidence.md`.

- Profile schema `0.3.0-discovery`: `THIRD_PARTY_RUNTIME` / `THIRD_PARTY_MODIFIED` / `THIRD_PARTY_UNKNOWN`, `RUNTIME_DEPENDENCY_INDEX`, generated artifacts with input provenance, byte-comparison list.
- Vendor authority only from restored NuGet package content (byte comparison) and LibMan file lists; generated authority only from `bundleconfig.json` and resolvable source maps; names/banners/copies never classify.
- Release build 0 warnings; `Pkc.CSharp.Tests` 313/313, `Pkc.Frontend.Tests` 23/23; focused discovery tests 22/22; modified-vendor mutation caught.
- Sample `.pkc` semantic artifacts still byte-identical to `ab24363`.

## RD4 — LOCAL PASS

```text
a37d936fc68c82e599f23da8b64bcb6ea85ef9fb
feat: prove runtime plugin edges in repository discovery
```

Evidence: `docs/reviews/2026-09-24-rd4-runtime-plugin-provenance-evidence.md`.

- Profile schema `0.4.0-discovery`: `runtime-plugin-load` edges require a host loader call, a unique literal assembly identity and build/copy delivery into the host tree; everything less is reported as unresolved; test loads are test references only.
- Bounded composition probe reads only host/test project C# files and records per-component counts.
- Release build 0 warnings; `Pkc.CSharp.Tests` 317/317, `Pkc.Frontend.Tests` 23/23; focused discovery tests 26/26; delivery-proof mutation caught.
- Sample `.pkc` semantic artifacts still byte-identical to `ab24363`.

## RD5 — LOCAL PASS

```text
c440eea592c0360e0b9cde3456b1dcbf39727cb0
feat: build a deterministic scan plan before semantic scans
```

Evidence: `docs/reviews/2026-09-24-rd5-deterministic-scan-plan-evidence.md`.

- Plan schema `0.1.0-scan-plan` persisted to `.pkc/discovery/scan-plan.json` before any semantic stage: every enumerated file in exactly one scope with role, scan mode, scanners, coverage and evidence; explicit exclusions and UNKNOWN areas; host/unowned/test/unattributed waves; input fingerprint over discovery reads.
- Release build 0 warnings; `Pkc.CSharp.Tests` 322/322, `Pkc.Frontend.Tests` 23/23; focused discovery/plan/CLI tests 31/31; test-scope-isolation mutation caught.
- Sample `.pkc` semantic artifacts still byte-identical to `ab24363`; scanners still receive the whole root.

## RD6 — LOCAL PASS

```text
280450cdaa77491b8a5ab6a45ae7f0cb8f0caa0d
feat: run semantic scanners inside the scan plan scope
```

Evidence: `docs/reviews/2026-09-24-rd6-scoped-semantic-execution-evidence.md`.

- `pkc run` executes the existing C# and frontend scanners inside a plan-derived `SemanticSourceScope`: safely excluded, generated/light-indexed, runtime-dependency and test-evidence files never reach deep scanners (including `MSBuildWorkspace` project sets and node TypeScript walkers); DEEP_SCAN and UNKNOWN stay visible; accepted name scopes remain as an extra filter and planned files they withhold are reported per stage.
- `scan` / `build` keep their whole-root scope.
- Release build 0 warnings; `Pkc.CSharp.Tests` 327/327, `Pkc.Frontend.Tests` 23/23; focused RD6 tests 4/4 + CLI source test; test-evidence and scope-entry mutations caught.
- Sample `.pkc` semantic artifacts still byte-identical to `ab24363`.
- Known gap: one bounded pass over the planned union (per-wave release deferred until RD8 measures).

## RD7 — LOCAL PASS

```text
d0c1017bd83d0d4b732facb5ddeddc5facfe48ca
feat: add plan-only discovery and persisted scan coverage
```

Evidence: `docs/reviews/2026-09-24-rd7-coverage-observability-evidence.md`.

- `pkc discover <repo>` is the plan-only inspection contract: discovery + plan persisted, no semantic scanner, no facts/workspace.
- Executing commands persist `.pkc/discovery/scan-coverage.json` and print `[pkc:coverage]`; `pkc run` adds a count-only `_meta/coverage.json` to the workspace and one answer-contract line.
- Release build 0 warnings; `Pkc.CSharp.Tests` 331/331, `Pkc.Frontend.Tests` 23/23; focused RD7 tests 4/4 (two via the real CLI); stale-coverage and privacy mutations caught.
- Sample semantic artifacts and knowledge byte-identical to `ab24363`; intended additions only (coverage files, one contract line).

## Active checkpoint — RD8 (external gate)

RD8 private large-repository validation is unlocked by RD7 local PASS. It requires the approved private repository inside the approved source-enabled/company environment, which the implementing session does not have. RD8 is therefore an operator-run gate, not something a PKC-source session can self-certify.

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

## RD1 acceptance record

RD1 was the production implementation checkpoint before RD2.

Goal:

```text
cheap deterministic repository inventory
+ safe exclusion only when strongly proven
+ discovery/plan state before expensive semantic scans
```

Regression-first mixed-repository fixture must include at least:

- multiple .NET applications/libraries/tests;
- Angular workspace;
- legacy JavaScript;
- generated/restorable directories;
- infrastructure/configuration;
- first-party files under ambiguous vendor/legacy-looking directory names.

Required negative rule:

```text
legacy / vendor / plugins / themes / Scripts / Content / old / packages
```

must never become exclusion authority by name alone.

Required structural rule: the new `pkc run` architecture must establish discovery/plan state before expensive C# or frontend semantic scanning begins.

RD1–RD7 are locally PASS and documented; RD8 is unlocked and waits on an operator run in the approved private environment.

## E0 runtime constraint — zero required AI tokens

Repository Discovery and normal PKC compilation must be deterministic/local product code.

E0 MUST NOT require Claude/OpenAI/Gemini APIs, hosted model calls, API keys, user tokens, or source upload to an LLM.

Claude/LLM reconnaissance is research/oracle evidence only.

Target cost model:

```text
PKC compilation = local CPU + RAM + disk = zero required AI tokens
```

An AI may optionally consume `.pkc/workspace` after PKC generates it.

## E0 sequence

Execute sequentially:

```text
RD1 inventory + safe exclusion
→ RD2 application boundaries + ownership
→ RD3 vendor/custom frontend classification
→ RD4 runtime/plugin provenance
→ RD5 deterministic ScanPlan
→ RD6 scoped/bounded semantic execution
→ RD7 coverage + observability + plan-only inspection
→ RD8 private large-repository validation
→ prove normal pkc run practically produces .pkc/workspace
```

No parallel production checkpoints.

## E0 invariants

- discovery precedes expensive semantic scanning;
- repository is a graph, not one homogeneous root;
- source role and scan mode remain separate;
- only strongly proven generated/restorable areas auto-exclude;
- third-party runtime remains visible without default deep scan;
- modified vendor receives narrow deterministic first-party carve-outs;
- UNKNOWN is valid and never silently discarded;
- tests are evidence, not production authority;
- similar names never create dependency edges;
- runtime/plugin edges require deterministic provenance;
- discovery stays shallow and bounded;
- private source/config values never leak into portable output;
- accepted semantic fail-closed boundaries remain unchanged inside selected scope.

## Parked work

Fix #4 remains parked off-main:

```text
branch  fix/product-value-construction-state
commit  35c8e5c5f856e15568aa963bb2d76268008c5570
```

Do not merge during E0.

## Formal D state

R7.10/D remains OPEN and not accepted. The prior rereview lane is paused by product-priority decision, not passed.

Before final V0.4.7 acceptance, a fresh independent review must still accept the then-current R7.10 production state.

## Exact next action

```text
operator: push main (RD1 fd3428f, RD2 05eadb1, RD3 ac3efd9, RD4 a37d936, RD5 c440eea, RD6 280450c, RD7 d0c1017 + docs) and confirm CI green
operator: run RD8 in the approved private environment (procedure above) and record sanitized measurements
→ if RD8 exposes scope/resource gaps, fix them regression-first on the PKC source with synthetic fixtures
→ only after RD8 PASS: practical workspace generation sign-off and E0 PASS
```
