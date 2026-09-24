# PKC Status

Last updated: 2026-09-24

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                   PASS / COMPLETE
V0.4.5 real-repository generalization              PASS / COMPLETE
V0.4.6 business logic reconstruction               PASS / COMPLETE
V0.4.7-A origin and copy timing                    PASS / COMPLETE
V0.4.7-B computation and later change              PASS / COMPLETE
V0.4.7-C backend to API                            PASS / COMPLETE
V0.4.7-D / R7.9 API to rendered value              PASS / COMPLETE
V0.4.7-D / R7.10 joint visibility                  REREVIEW #17 FAIL / REPAIR REQUIRED
V0.4.7-D overall                                   OPEN
V0.4.7-E0 repository discovery + bounded run       LOCKED behind D
V0.4.7-E1 remaining product-value repairs          LOCKED behind E0
V0.4.7-E2 final product acceptance / R7.14         LOCKED behind E1
continuous update/diff                             LOCKED
V0.5 Azure DevOps input evidence                   LOCKED
```

Formal D acceptance remains an independent-review gate. Rereview #17 found a new compile-valid scalar-alias authority bypass on exact production `96205a9a643864facaf9642a3b390ddcdbed59d9`. Product-value or demo-operability work must not self-certify or bypass this gate.

Independent review record:

```text
docs/reviews/2026-09-24-v0.4.7-d-r7.10-independent-rereview-17.md
Decision: FAIL
```

## Current main / reviewed checkpoint

Rereview #17 started from:

```text
ce041a9326362c07ab2c1201492335ab54bf7da2
docs: prioritize scan operability before demo [skip ci]
```

Reviewed exact R7.10 production:

```text
96205a9a643864facaf9642a3b390ddcdbed59d9
fix: tolerate duplicate Angular import aliases
```

Latest validated production-code checkpoint below the docs-only roadmap commits remains:

```text
79b0f9fec80a2afb87f43ec7a559a5d54cb87863
fix: surface pkc run progress
```

The exact R7.10 SHA itself has four green push workflows:

```text
CI                         35832501567 PASS
Loren pinned/external      35832501543 PASS
Loren-main canary          35832501552 PASS
Jellyfin parity            35832501534 PASS
```

Green gates do not close the new rereview blocker.

## R7.10 rereview #17 blocker

The unsupported scalar-alias fail-closed filter currently recognizes only aliases ending in a literal semicolon, for example:

```ts
const IMPORTS = SHARED_IMPORTS;
```

TypeScript automatic semicolon insertion also permits the equivalent compile-valid form:

```ts
const IMPORTS = SHARED_IMPORTS

@Component({
  imports: [IMPORTS],
  template: `<div ext-shell>{{ displayPrice }}</div>`
})
export class PriceComponent {}
```

The existing local-import closure does not propagate scalar aliases, while the final unsupported-indirection filter misses this semicolonless spelling. In the existing indirect external-component fixture shape this can let authoritative `ui-member-render` / downstream joint-visibility evidence survive without the required component-import proof.

Independent local syntax evidence during review:

```text
TypeScript 5.8.3 noEmit compile of semicolonless form  PASS
semicolon vs semicolonless emitted alias/decorator JS   equivalent
```

Repository-local .NET tests were not rerun in this review environment because the shell could not resolve `github.com` to obtain a working clone. No local PKC test claim is made. Exact-SHA production/regression sources and GitHub Actions evidence were reviewed through the authenticated connector.

## Demo-critical roadmap decision

A private mixed legacy enterprise repository exposed a first-order product blocker: an unbounded `pkc run` entered expensive semantic scanning before understanding repository shape and was observed consuming roughly 9 GB RAM before producing useful output.

That remains the first demo-operability priority **after D passes**. The ordering is unchanged:

```text
R7.10 repair
→ exact-SHA gates
→ independent rereview #18
→ close V0.4.7-D only if PASS
→ E0 Repository Discovery + bounded run readiness
   RD1 inventory + safe exclusion
→  RD2 application boundaries + ownership
→  RD3 vendor/custom frontend classification
→  RD4 runtime/plugin provenance
→  RD5 deterministic ScanPlan
→  RD6 scoped/bounded semantic execution
→  RD7 coverage + observability + plan-only inspection
→  RD8 private large-repo validation
→ prove pkc run can practically produce .pkc/workspace
→ E1 remaining semantic richness
→ E2 final product acceptance
```

No parallel production implementation is authorized across these checkpoints. Independent reading, diff inspection, and non-conflicting verification may be parallelized inside the active checkpoint, but milestone/gate order must remain sequential.

## E0 acceptance intent — still locked

E0 is not accepted merely because discovery JSON exists. Once unlocked it must prove that PKC understands repository shape before expensive semantic work and that planned execution is practically usable.

At minimum:

- discovery runs before deep C#/frontend semantic scanning;
- only strongly proven generated/restorable areas are auto-excluded;
- first-party, shared, vendor runtime, modified vendor, tests, infrastructure and UNKNOWN remain distinguishable;
- application ownership and runtime/plugin edges require deterministic provenance;
- `scan-plan` is stable, inspectable and privacy-safe;
- semantic scanners receive only planned scopes/application waves;
- UNKNOWN is explicit and cannot silently disappear;
- workspace coverage cannot claim READY when required areas are omitted/unsupported;
- a plan-only/discovery inspection path exists before expensive semantic execution;
- private large-repo validation records elapsed time and peak memory versus the observed unbounded baseline without leaking proprietary source;
- `pkc run` completes far enough to produce the intended AI workspace on the approved demo repository before E1/E2 are treated as demo-ready.

Do not invent a universal RAM threshold from the single ~9 GB observation. Prove structural boundedness first, then measure controlled before/after resource deltas.

Technical design packet remains prepared but locked:

- `docs/plans/2026-09-24-repository-discovery-scan-planning-plan.md`
- `docs/reviews/2026-09-24-repository-discovery-scan-planning-self-review.md`
- `docs/plans/2026-09-24-demo-critical-sequential-execution-plan.md`

## Product-value state already achieved

```text
Fix #1 interface→concrete direct DI       PASS / COMPLETE for bounded path
Fix #2 frontend URL + Output bridge       PASS / COMPLETE for bounded path
Fix #3 displayed-value lineage            PASS / COMPLETE for bounded Agentic email path
Fix #4 construction/default state         VALIDATED FUTURE CANDIDATE / NOT PRODUCTION
```

Fix #4 remains off `main` and must not be merged before D is accepted and E0 is complete:

```text
branch  fix/product-value-construction-state
commit  35c8e5c5f856e15568aa963bb2d76268008c5570
```

Latest Level-2 diagnostic checkpoint remains useful evidence, not acceptance:

```text
Agentic Users Update     ~94.5%
Jin12 Contacts Update    100.0%
Kesetovic PackOrder       82.5%
selected-probe aggregate ~91.6%
backend PO/QC core       ~95.3%
```

## Workspace / privacy boundary

Preferred product UX remains:

```text
pkc run <repository-path>
cd <repository-path>/.pkc/workspace
```

- PRODUCT/TRACE: workspace only; no source fallback.
- Benchmark phase 1: workspace only and frozen before source inspection.
- Phase 2/source cross-check: minimum necessary source only in the approved environment.
- Private-repository reports contain only sanitized counts, aliases, classifications, deltas and failure categories; no proprietary source bodies, internal names, endpoints or config values.

## Benchmark cadence

```text
Level 0 — deterministic tests/build/smoke
Level 1 — targeted semantic/product-value workflow
Level 2 — full Agentic + Jin12 + Kesetovic only at checkpoint/release/demo
```

Do not repeatedly run Level 2 during R7.10 repair or RD1–RD7. Use focused regressions and related suites locally; use CI as final verification for coherent checkpoints.

## Exact next action

```text
R7.10 regression-first repair on top of current main
→ add semicolonless scalar-alias negative regression
→ prove RED on current production
→ minimum generic semicolon-independent fail-closed repair
→ focused R7.10 tests
→ related frontend/render-authority tests
→ broader relevant suite/build
→ review full diff
→ one coherent implementation commit
→ one push
→ exact-SHA gates
→ independent rereview #18
```

Until rereview #18 passes:

```text
V0.4.7-D remains OPEN
E0/RD1 remains LOCKED
Fix #4 remains parked
E1/E2 remain LOCKED
```
