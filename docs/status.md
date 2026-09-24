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
V0.4.7-E0 current sub-checkpoint                   RD5 deterministic ScanPlan
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

## Active implementation checkpoint — RD5

RD5 deterministic ScanPlan is unlocked by RD4 local PASS and is now the only production implementation checkpoint.

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

RD1–RD4 are locally PASS and documented; RD5 is unlocked.

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
operator: push main (RD1 fd3428f, RD2 05eadb1, RD3 ac3efd9, RD4 a37d936 + docs) and confirm CI green
implementation: RD5 deterministic ScanPlan, regression-first
→ finish and locally verify RD5
→ update status/handoff with exact implementation HEAD and verification
→ only then start RD6
```
