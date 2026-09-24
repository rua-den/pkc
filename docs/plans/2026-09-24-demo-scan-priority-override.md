# Demo Scan Priority Override

Date: 2026-09-24
Status: ACTIVE / USER-AUTHORIZED

This decision supersedes older ordering clauses that required V0.4.7-D/R7.10 independent acceptance before Repository Discovery implementation could begin.

It does **not** mark R7.10 or V0.4.7-D PASS. The formal R7.10 independent-review gate remains open and must be completed before V0.4.7 can be accepted.

The product priority changed because the current normal `pkc run <large-repository>` path is not operationally usable on the intended demo repository class: a private mixed legacy repository reached roughly 9 GB RAM before useful completion while whole-root C# and frontend semantic scanners ran before repository scope planning.

The immediate product risk is therefore scan operability, not additional semantic richness.

## Active checkpoint

```text
V0.4.7-E0 Repository Discovery + bounded run
ACTIVE AS BOUNDED PREWORK UNDER OPEN V0.4.7-D

current sub-checkpoint: RD1 inventory + safe exclusion
```

Production work is sequential:

```text
RD1 inventory + safe exclusion
→ RD2 application boundaries + ownership
→ RD3 vendor/custom frontend classification
→ RD4 runtime/plugin provenance
→ RD5 deterministic ScanPlan
→ RD6 scoped/bounded semantic execution
→ RD7 coverage + observability + plan-only inspection
→ RD8 private large-repository validation
→ prove normal pkc run can practically produce .pkc/workspace
```

Only after E0 PASS should the project resume remaining semantic-richness work and then return to the still-open formal R7.10/D acceptance gate before final V0.4.7 acceptance if that gate has not already been completed independently.

## Runtime constraint: zero LLM dependency

Repository Discovery and the normal PKC compilation path MUST be deterministic/local product code.

E0 MUST NOT require:

- Claude API;
- OpenAI API;
- Gemini API;
- hosted model calls;
- API keys;
- user AI tokens;
- source upload to an LLM.

Claude/other LLM reconnaissance is research/oracle evidence only. It must never become a runtime prerequisite.

The target product cost model is:

```text
PKC repository compilation
= local CPU + RAM + disk
= zero required AI tokens
```

An AI may optionally consume the generated `.pkc/workspace` afterward. That is outside repository compilation itself.

## E0 architecture contract

Target flow:

```text
repository
→ shallow deterministic discovery
→ RepositoryProfile
→ application/source/runtime graph
→ deterministic ScanPlan
→ bounded scanner execution waves
→ existing semantic evidence
→ knowledge/workspace
```

Discovery should use cheap structural evidence first: filesystem/VCS shape, `.sln`/`.slnx`, `.csproj`, project references, package/workspace manifests, Angular/TypeScript config, bundle/layout/build/deployment metadata, and only bounded composition/runtime entry evidence when necessary.

Do not turn discovery into another full semantic scan.

## Non-negotiable safety rules

- Discovery precedes expensive semantic scanning.
- Name-only heuristics never create exclusion authority.
- `legacy`, `vendor`, `plugins`, `themes`, `Scripts`, `Content`, `old`, `packages` are not auto-excluded by name.
- Only strongly proven generated/restorable areas may become `SAFE_AUTO_EXCLUDE`.
- `THIRD_PARTY_RUNTIME` remains runtime-visible but is not deep-scanned by default.
- Locally modified vendor code requires a narrow deterministic first-party carve-out.
- `UNKNOWN` is valid and must not be silently dropped.
- Tests are evidence, not production authority.
- Similar names never create application/dependency edges.
- Runtime/plugin edges require deterministic provenance.
- Private source bodies, secrets and raw configuration values must not leak into portable output.

## RD1 authorization

RD1 is explicitly authorized now.

Regression-first fixture must prove deterministic inventory and safe exclusion on a mixed repository containing at least:

- multiple .NET apps/libraries/tests;
- Angular workspace;
- legacy JavaScript;
- generated/restorable directories;
- infrastructure/configuration;
- first-party files under ambiguous vendor/legacy-looking folder names.

Required negative regression: folder names alone never cause exclusion.

Required structural regression: expensive C#/frontend semantic scanning cannot begin before discovery/plan state exists in the new run architecture.

Do not implement RD2 until RD1 is explicitly locally PASS and the checkpoint state is updated.

## Git / CI discipline

For each RD checkpoint:

```text
inspect
→ focused regression
→ minimum generic implementation
→ focused tests
→ related tests
→ broader relevant verification
→ review full diff
→ one coherent implementation commit
→ one push
→ CI final verification
→ update status/handoff
→ next RD checkpoint
```

Do not use GitHub Actions as the normal edit-test loop.

## Formal D boundary

R7.10/D remains OPEN. No E0 work may claim that D passed or weaken accepted fail-closed semantic authority.

E0 is infrastructure/operability prework authorized by explicit product-priority decision, not a retroactive acceptance of D.

If E0 touches code that affects R7.10 semantics, existing R7.10 regressions and exact authority boundaries must remain green; do not opportunistically repair/redefine R7.10 while implementing scan scope.

## Demo exit condition

E0 is not complete merely because JSON discovery artifacts exist.

The demo-critical exit is:

```text
normal PKC command
→ deterministic discovery + inspectable plan
→ bounded semantic execution
→ materially improved resource behavior on the approved private large repository
→ successful .pkc/workspace generation
```

Coverage must remain honest (`READY` / `PARTIAL` / `FAILED` or equivalent) for unsupported or UNKNOWN areas.
