# Demo-Critical Sequential Execution Plan

Date: 2026-09-24
Status: ACTIVE — SCAN OPERABILITY PREWORK AUTHORIZED NOW

This plan is superseded where necessary by:

`docs/plans/2026-09-24-demo-scan-priority-override.md`

The current product priority is to make the normal PKC scan/run path practical on the intended large mixed repository before investing in more semantic richness.

Formal R7.10 / V0.4.7-D remains OPEN and NOT ACCEPTED. Repository Discovery is authorized as bounded infrastructure prework under that open gate. No E0 work may claim D PASS or weaken existing semantic authority.

## Product decision

The demo path is credible only if the normal product command can actually produce a trustworthy AI workspace on the intended repository class.

A private mixed legacy enterprise repository exposed the current structural problem:

```text
pkc run <large-repository>
→ expensive whole-root C# scan starts immediately
→ whole-root frontend scan follows
→ repository/application/vendor/test boundaries are not planned first
→ observed resource use reached roughly 9 GB RAM before useful completion
```

Visible progress alone does not solve this. Adding semantic passes first makes it worse.

Therefore:

> Make repository discovery and bounded execution operational first. Improve semantic richness second. Complete formal/product acceptance after that.

The roughly 9 GB observation is a private operator baseline, not a universal hard limit.

## Runtime product constraint

PKC Repository Discovery and repository compilation must be local/deterministic and require zero AI tokens.

Do not add runtime calls to Claude/OpenAI/Gemini or any hosted LLM. Do not require API keys. Do not upload source to an LLM.

LLM reconnaissance is research/oracle evidence only.

## Authoritative active order

```text
E0 Repository Discovery + bounded run prework — ACTIVE
        ↓
RD1 inventory + safe exclusion — CURRENT
        ↓
RD2 application boundaries + ownership
        ↓
RD3 vendor/custom frontend classification
        ↓
RD4 runtime/plugin provenance
        ↓
RD5 deterministic ScanPlan
        ↓
RD6 scoped/bounded semantic execution
        ↓
RD7 coverage + observability + plan-only inspection
        ↓
RD8 private large-repository validation
        ↓
prove normal pkc run produces .pkc/workspace practically
        ↓
E1 remaining semantic/product-value gaps
        ↓
formal R7.10/D independent acceptance if still open
        ↓
E2 real-project PO/QC product acceptance
```

Production checkpoints remain sequential.

## RD1 — deterministic inventory + safe exclusion — ACTIVE

Goal:

- inventory repository shape cheaply;
- identify manifests/workspaces/projects/languages;
- classify only strongly proven generated/restorable areas as safe exclusions;
- preserve ambiguous legacy/vendor-looking areas;
- establish discovery/plan state before expensive semantic scanning in the new run architecture.

Required mixed-repository regression fixture includes:

- multiple .NET apps/libraries/tests;
- Angular workspace;
- legacy JavaScript;
- generated/restorable directories;
- infrastructure/configuration;
- first-party source in ambiguous vendor/legacy-looking folder names.

Required negative behavior:

`legacy`, `vendor`, `plugins`, `themes`, `Scripts`, `Content`, `old`, `packages` or similar names never become exclusion authority by themselves.

Checkpoint exit:

- focused regression PASS;
- related tests PASS;
- deterministic ordering/output proven;
- no deep semantic scan required to obtain inventory;
- orchestration proves discovery/plan exists before expensive semantic scanner execution;
- diff reviewed;
- one coherent commit/push;
- status/handoff updated.

Do not start RD2 before RD1 PASS.

## RD2 — application boundaries + ownership

After RD1 PASS only.

Model runtime/application nodes and shared ownership from deterministic evidence. Tests cannot create production authority. Similar names cannot create edges.

## RD3 — vendor/custom frontend classification

After RD2 PASS only.

Separate first-party JS, third-party runtime, generated/minified/bundled output, wrappers/adapters and locally modified vendor code. Runtime dependency identity does not imply deep-scan internals.

## RD4 — runtime/plugin provenance

After RD3 PASS only.

Authoritative runtime plugin edges require deterministic loader + build/copy/dependency + resolvable identity provenance. Unsupported identity remains UNKNOWN.

## RD5 — deterministic ScanPlan

After RD4 PASS only.

Convert repository/application/source graph into an inspectable stable plan carrying source role, scan mode, scanner set, evidence/reason, confidence, exclusions and coverage effect.

Plan metadata should make staleness diagnosable where possible without leaking source bodies.

## RD6 — scoped/bounded semantic execution

After RD5 PASS only.

Make existing semantic scanners consume planned areas/application waves rather than the whole repository indiscriminately.

Required:

- SAFE_AUTO_EXCLUDE never reaches deep scanners;
- RUNTIME_DEPENDENCY_INDEX internals never deep-scan by default;
- targeted shared code follows supported ownership/reachability;
- tests stay separate;
- UNKNOWN is not silently dropped;
- accepted semantic/fail-closed behavior is unchanged inside selected scope;
- heavy analysis state can be released/disposed between bounded waves when safe.

Optimize semantic internals only after scope reduction evidence exists.

## RD7 — coverage + observability + plan-only inspection

After RD6 PASS only.

Users must be able to inspect detection and planned scope before expensive execution. The exact CLI surface may be `pkc discover`, `pkc run --plan-only`, or another equivalent contract selected during implementation.

Persist local discovery/plan/coverage artifacts under `.pkc` without exporting source bodies or sensitive config values.

## RD8 — private large-repository validation

After RD7 PASS only.

Validate the approved private mixed legacy repository using sanitized measurements only:

- application/deployable boundaries;
- frontend generations;
- first-party/vendor separation;
- modified-vendor carve-outs;
- runtime/plugin topology;
- shared ownership;
- test isolation;
- generated/restorable exclusion;
- UNKNOWN preservation;
- semantic scope reduction;
- elapsed time and peak memory delta;
- successful `.pkc/workspace` generation sufficient for the demo flow.

Do not publish proprietary names, paths, endpoints, source snippets or configuration values.

## E0 completion rule

E0 is PASS only when RD1–RD8 complete sequentially and the normal PKC product path produces the intended workspace practically on the approved private large repository with honest coverage.

JSON output alone is insufficient.

## E1 — remaining semantic/product-value gaps

Only after E0 PASS.

The validated construction candidate remains parked during E0:

```text
35c8e5c5f856e15568aa963bb2d76268008c5570
fix: prove observable constructed state
```

## Formal D boundary

R7.10/D remains OPEN. The independent gate is paused, not passed. Resume and complete a fresh independent review before final V0.4.7 acceptance.

## Explicitly deferred

Unless an E0 regression proves a dependency is unavoidable, do not mix in:

- remaining product-value semantic features;
- continuous update/diff;
- Azure DevOps evidence;
- generic JavaScript dataflow;
- arbitrary runtime instrumentation;
- automatic dead-code deletion;
- source cleanup recommendations;
- LLM-dependent repository classification.

## Execution discipline

```text
inspect
→ regression
→ minimum generic fix
→ focused tests
→ related tests
→ broader relevant verification
→ diff review
→ one coherent commit
→ one push
→ CI final verification
→ status/handoff
→ next RD checkpoint only after explicit PASS
```

## Immediate next action

```text
START RD1 NOW.
```
