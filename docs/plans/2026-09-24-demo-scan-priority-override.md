# Demo Scan Priority Override

Date: 2026-09-24
Status: ACTIVE / USER-AUTHORIZED

This decision supersedes older ordering clauses that required V0.4.7-D/R7.10 independent acceptance before Repository Discovery implementation could begin.

It does **not** mark R7.10 or V0.4.7-D PASS. The formal R7.10 independent-review gate remains open and must be completed before V0.4.7 can be accepted.

The product priority changed because the normal `pkc run <large-repository>` path was not operationally usable on the intended demo repository class. A private mixed legacy repository first reached ~18.6 GB peak and failed before a usable workspace during the RD7-era run.

Repository Discovery, scoped execution and large-run repairs are now implemented. The active product risk has moved from “can we scope the repository at all?” to “does the fully integrated current main complete practically and produce useful, correctly uncertain answers?”

## Active checkpoint

```text
V0.4.7-E0 Repository Discovery + bounded run
ACTIVE AS BOUNDED PREWORK UNDER OPEN V0.4.7-D

RD1 inventory + safe exclusion                    PASS / COMPLETE
RD2 application boundaries + ownership            PASS / COMPLETE
RD3 vendor/custom frontend classification         PASS / COMPLETE
RD4 runtime/plugin provenance                     PASS / COMPLETE (deterministic/synthetic gate)
RD5 deterministic ScanPlan                        PASS / COMPLETE
RD6 scoped/bounded semantic execution             PASS / COMPLETE
RD7 coverage + observability                      PASS / COMPLETE
RD8 current-main private large-repository gate    ACTIVE / PARTIAL EVIDENCE / NOT PASS
```

Production order remains:

```text
RD8 current-main private validation
→ practical workspace + targeted Level-1 product-value sign-off
→ E0 PASS
→ E1 remaining semantic/product-value repairs
→ formal R7.10/D independent acceptance if still open
→ E2 final real-project PO/QC acceptance
```

## Repository-state reconciliation

Immediately before the docs reconciliation, remote `main` was:

```text
9ef914f57c558e3b2963b9ba4dd187a6ecd6603f
Add question in demo
```

Latest production-code commit beneath it:

```text
fa30e3eb140357da5453702090be307417abb4e2
fix: report the preserved workspace when it could not replace the current one
```

Current descendant HEAD completed all four observed GitHub Actions workflows successfully.

Therefore older `PUSH + CI PENDING` wording for RD1-RD7 is obsolete.

## Runtime constraint: zero LLM dependency

Repository Discovery and the normal PKC compilation path MUST remain deterministic/local product code.

E0 MUST NOT require:

- Claude API;
- OpenAI API;
- Gemini API;
- hosted model calls;
- API keys;
- user AI tokens;
- source upload to an LLM.

The target product cost model remains:

```text
PKC repository compilation
= local CPU + RAM + disk
= zero required AI tokens
```

An AI may consume the generated `.pkc/workspace` afterward. That is outside repository compilation.

## E0 architecture contract

The implemented target flow is:

```text
repository
→ shallow deterministic discovery
→ RepositoryProfile
→ application/source/runtime graph
→ deterministic ScanPlan
→ scoped semantic execution
→ cross-stack/product knowledge
→ AI workspace + coverage/run summary
```

Discovery uses structural evidence first. Unsupported or ambiguous areas remain visible/UNKNOWN rather than being silently dropped.

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
- Existing accepted semantic/fail-closed authority remains intact unless a separately accepted semantic repair changes it explicitly.

## RD8 current evidence

First private exercise, sanitized:

```text
discover: about 98 s
RD7 run: 41.5 min / ~18.6 GB / OOM at artifact write
resource-patched run: 29 min / ~7.5 GB / exit 0 / workspace generated
```

The resource-patched run generated about 703 product features and 4,709 workspace files.

Those resource repairs are now merged in `612fa998`, and later production work `48ce2f10` + `fa30e3eb` is also on main.

Because the successful private run predates the fully integrated current production state, RD8 remains open.

## RD8 authorization and exit

The next authorized product work is **not** another speculative semantic feature. It is the current-main RD8 gate:

```text
fresh pkc discover
→ fresh full pkc run without --resume
→ sanitized elapsed/peak-memory/coverage/workspace evidence
→ 2-3 targeted Level-1 known-answer probes
→ runtime/plugin applicability disposition
```

RD8 PASS requires all of:

1. practical current-main completion on the approved private repository;
2. honest plausible coverage/run-summary/workspace output;
3. useful, correctly uncertain targeted product answers under the benchmark protocol;
4. explicit disposition of runtime/plugin applicability (`N/A` only when absence is proven/accepted, otherwise another approved real corpus).

There is no universal numeric RAM threshold. If the fresh run remains around the prior ~7.5 GB observation, profile the retained-memory phase before optimizing.

## Formal D boundary

R7.10/D remains OPEN. No E0 work may claim D passed or weaken accepted fail-closed semantic authority.

E0 is infrastructure/operability prework authorized by explicit product-priority decision, not retroactive acceptance of D.

## Demo exit condition

E0 is not complete merely because discovery JSON or a workspace exists.

The demo-critical exit is:

```text
normal current-main PKC command
→ deterministic discovery + inspectable plan
→ scoped semantic execution
→ practical completion on approved private large repository
→ honest coverage + run summary
→ usable workspace
→ targeted known-answer benchmark demonstrates useful correctly-calibrated product answers
```
