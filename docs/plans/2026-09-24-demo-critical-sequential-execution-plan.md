# Demo-Critical Sequential Execution Plan

Date: 2026-09-24
Status: AUTHORITATIVE ORDERING FOR V0.4.7-E AFTER D PASSES

This document changes execution order only. It does not self-certify V0.4.7-D/R7.10 and does not authorize production work while D remains pending independent rereview #17.

It supersedes any earlier informal or prepared ordering that would begin additional semantic product-value work before repository scan operability is proven.

Technical Repository Discovery design remains in:

- `docs/plans/2026-09-24-repository-discovery-scan-planning-plan.md`
- `docs/reviews/2026-09-24-repository-discovery-scan-planning-self-review.md`

## Product decision

The demo path is only credible if the normal product command can actually produce a trustworthy AI workspace on the intended repository class.

A private mixed legacy enterprise repository exposed the current ordering problem:

```text
pkc run <large-repository>
→ expensive whole-root semantic analysis starts immediately
→ repository/application/vendor/test boundaries are not planned first
→ observed resource use reached roughly 9 GB RAM before useful completion
```

Visible progress does not solve this. Adding more semantic passes does not solve this. The structural problem is that expensive scanners receive an unbounded repository before PKC has decided what the repository contains and what each scanner should analyze.

Therefore:

> Make the run path operational first. Improve remaining semantic richness second. Run final PO/QC acceptance third.

The ~9 GB observation is a private operator baseline, not a portable hard limit.

## Formal gate remains first

Current formal action remains independent rereview #17 of:

```text
96205a9a643864facaf9642a3b390ddcdbed59d9
fix: tolerate duplicate Angular import aliases
```

If the rereview fails, repair D and rereview. Do not start E0 production implementation while D is unresolved.

If the rereview passes, close D and unlock V0.4.7-E with E0 as its first checkpoint.

## Authoritative sequential order

```text
D independent acceptance
        ↓
E0 Repository Discovery + bounded run readiness
        ↓
E1 Remaining semantic/product-value gaps
        ↓
E2 Real-project PO/QC product acceptance
        ↓
V0.4.7 complete
```

No parallel production implementation across these stages.

## E0 — Repository Discovery + bounded run readiness

Execute strictly in order.

### RD1 — deterministic inventory + safe exclusion

Goal:

- inventory repository shape cheaply;
- identify manifests/workspaces/projects/languages;
- classify only strongly proven generated/restorable areas as safe exclusions;
- preserve ambiguous legacy/vendor-looking areas.

Required negative behavior:

`legacy`, `vendor`, `plugins`, `themes`, `Scripts`, `Content`, `old`, `packages` or similar names never become exclusion authority by themselves.

Checkpoint exit:

- focused regression PASS;
- related tests PASS;
- deterministic ordering/output proven;
- no deep semantic scanner required to obtain inventory.

### RD2 — application boundaries + ownership

Goal:

Model runtime/application nodes and shared ownership from deterministic evidence.

Must cover supported fixtures for:

- API/composition host;
- MVC application;
- Angular application;
- worker/service;
- shared library/module;
- test host;
- shared module owned by multiple production hosts.

Tests cannot create production authority. Similar names cannot create edges.

### RD3 — vendor/custom frontend classification

Goal:

Separate runtime dependency identity from business-source semantics in mixed legacy frontend trees.

Supported fixture should include:

- first-party JS;
- untouched vendor JS;
- minified/generated output;
- first-party wrapper/adapter;
- custom code inside a vendor tree;
- tracked bundle with mixed inputs.

Third-party runtime stays visible but vendor internals do not enter DEEP_SCAN merely because they are runtime-used.

### RD4 — runtime/plugin provenance

Goal:

Represent supported runtime loading that project references alone cannot prove.

Authoritative runtime plugin edge requires deterministic provenance such as:

```text
loader evidence
+ build/copy/dependency evidence
+ resolvable plugin identity
→ runtime-plugin-load edge
```

Copy without loader is insufficient. Loader without resolvable identity remains UNKNOWN. Test-only loading remains test evidence.

### RD5 — deterministic ScanPlan

Goal:

Convert repository/application/source-area graph into an inspectable machine-readable plan.

At minimum every decision carries:

```text
area/application
source role
scan mode
scanner set
evidence/reason
confidence
explicit exclusions
coverage effect
```

The plan must be stable and must not depend on timestamps or nondeterministic traversal order.

Plan metadata should make staleness diagnosable. Where available include schema/version identity, repository snapshot/HEAD identity and dirty-state/fingerprint information without leaking source bodies.

### RD6 — scoped/bounded semantic execution

Goal:

Make existing semantic scanners consume the ScanPlan instead of the whole repository indiscriminately.

Required:

- SAFE_AUTO_EXCLUDE never reaches deep semantic scanners;
- RUNTIME_DEPENDENCY_INDEX internals never reach deep scanners by default;
- TARGETED shared code follows supported application ownership/reachability;
- tests remain separate;
- UNKNOWN is never silently dropped;
- accepted semantic/fail-closed authority remains unchanged inside selected scope;
- execution can occur in bounded application waves and release/dispose heavy analysis state when safe.

Optimize semantic internals only after scope reduction evidence exists. Do not weaken authority to reduce memory.

### RD7 — coverage + observability + plan-only inspection

Goal:

Make scan decisions visible before and during expensive execution.

The product needs an inspectable preflight path, for example an eventual contract such as:

```text
pkc discover <repository>
```

or:

```text
pkc run <repository> --plan-only
```

The exact CLI surface is an implementation decision, but the capability is required: users must be able to inspect what PKC detected, intends to deep-scan, indexes lightly, excludes and leaves UNKNOWN before committing to expensive semantic execution.

Persist local discovery/plan/coverage artifacts under `.pkc` without exporting source bodies or sensitive config values.

Progress and coverage counts must come from actual plan state.

### RD8 — private large-repository validation

Goal:

Prove E0 against the approved private mixed legacy repository without leaking it into PKC artifacts/reports.

Validate sanitized properties only:

- application/deployable boundary quality;
- multiple frontend generations;
- first-party/vendor JS separation;
- modified-vendor carve-out;
- runtime/plugin topology;
- multi-owner shared modules;
- test isolation;
- generated/restorable exclusion;
- UNKNOWN preservation;
- non-.NET production visibility where supported;
- semantic scope reduction;
- elapsed time and peak memory delta from the unbounded baseline;
- successful generation of the intended `.pkc/workspace` far enough to exercise the demo flow.

Do not publish exact internal names, paths, endpoints, source snippets or configuration values.

## E0 completion rule

E0 is PASS only when:

1. RD1–RD8 are complete in sequence;
2. focused and related regressions are green;
3. broader relevant suite is green;
4. existing accepted authority/safety behavior remains green;
5. private validation demonstrates practical bounded behavior and workspace generation;
6. coverage semantics are honest about unsupported/omitted/UNKNOWN areas;
7. status/handoff are updated with the exact accepted implementation HEAD and resource observations.

Do not declare PASS merely because `repository-profile.json` or `scan-plan.json` exists.

## E1 — remaining semantic/product-value gaps

Only after E0 PASS.

Current expected order:

```text
construction/default/computation state
→ semantic integration side effects
→ feature-summary rule fidelity
```

The validated construction candidate:

```text
35c8e5c5f856e15568aa963bb2d76268008c5570
fix: prove observable constructed state
```

must remain off `main` through E0. It adds another semantic pass and should be re-evaluated on top of the new scoped execution architecture rather than merged first.

Each E1 repair remains regression-first and fail-closed.

## E2 — final product acceptance

Only after E1 PASS.

Required:

```text
R7.14 positive unchanged-real-project yield
→ Level-2 Agentic + Jin12 + Kesetovic benchmark
→ workspace-only PO/QC answer review
→ portable privacy/parity verification
```

Historical selected-probe diagnostics are context, not an acceptance threshold.

## Explicitly deferred until after V0.4.7

Unless an E0 regression proves a dependency is unavoidable, do not mix in:

- continuous update/diff;
- Azure DevOps evidence;
- generic JavaScript dataflow;
- arbitrary runtime instrumentation;
- automatic dead-code deletion;
- source cleanup recommendations;
- LLM-dependent runtime repository classification;
- broad semantic refactors unrelated to planned scope.

## Execution discipline

One active production checkpoint at a time.

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
→ next checkpoint
```

Do not use CI as the edit/test loop. Do not push red speculative commits to `main`.

## Immediate next action

Until independent rereview #17 resolves D, the only production-gating action is that review (or its regression-first repair if it fails).

After D PASS, start `E0 / RD1` and nothing else.
