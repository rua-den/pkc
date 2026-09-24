# PKC Handoff

Last updated: 2026-09-24

This handoff defines the next safe execution order for coding/review sessions after R7.10 independent rereview #17.

## Read first

1. root `CLAUDE.md`
2. `AGENTS.md`
3. `docs/status.md`
4. this handoff
5. `docs/milestones.md`
6. `docs/product-knowledge-contract.md`
7. `docs/v0.4.7-acceptance-plan.md`
8. `docs/reviews/2026-09-24-v0.4.7-d-r7.10-independent-rereview-17.md`
9. `docs/plans/2026-09-24-demo-critical-sequential-execution-plan.md`
10. `docs/plans/2026-09-24-repository-discovery-scan-planning-plan.md`
11. `docs/reviews/2026-09-24-repository-discovery-scan-planning-self-review.md`
12. `docs/benchmarks/product-value-benchmark-protocol.md`

Then inspect current `main`, recent commits, production code and relevant regressions. Never reset to an older SHA merely because this handoff names one.

## Current formal state

```text
A/B/C                    PASS / COMPLETE
R7.9                     PASS / COMPLETE
mutation-causality       PASS / CLOSED
R7.10                    REREVIEW #17 FAIL / REPAIR REQUIRED
V0.4.7-D                 OPEN
V0.4.7-E0                LOCKED behind D
E1 semantic richness     LOCKED behind E0
E2 product acceptance    LOCKED behind E1
R7.14                    NOT PASS / REQUIRED FOR E2
V0.5                     LOCKED
```

Rereview #17 reviewed exact production:

```text
96205a9a643864facaf9642a3b390ddcdbed59d9
fix: tolerate duplicate Angular import aliases
```

The review decision is recorded in:

```text
docs/reviews/2026-09-24-v0.4.7-d-r7.10-independent-rereview-17.md
```

Do not start Repository Discovery production code while D is open. Do not merge the construction-state candidate. Do not start E1/E2.

## Rereview #17 blocker

A semantically equivalent TypeScript spelling bypasses the scalar-alias fail-closed boundary.

Covered regression spelling:

```ts
const IMPORTS = SHARED_IMPORTS;
```

Compile-valid missed spelling:

```ts
const IMPORTS = SHARED_IMPORTS

@Component({
  imports: [IMPORTS],
  template: `<div ext-shell>{{ displayPrice }}</div>`
})
export class PriceComponent {}
```

`AngularUnsupportedComponentImportIndirectionAuthorityFilter.ScalarAliasRegex` requires a literal trailing semicolon. The preceding component-import closure propagates risk through local imports and constant arrays but not through scalar aliases. In the existing indirect external-package fixture shape, the component source has no direct external import for `AngularUnresolvedExternalComponentImportAuthorityFilter` to reject, and the rejected outside-root package link cannot provide external selector authority to the component projection filter.

Result: `IMPORTS` can avoid the intended final fail-closed rejection, allowing an authoritative render/joint-visibility path that is not proven.

Independent syntax verification during rereview:

```text
TypeScript 5.8.3 semicolonless noEmit compile  PASS
semicolon/ASI emitted alias+decorator JS        equivalent
```

This is a compile-valid false-positive authority path and therefore an R7.10 acceptance blocker.

## Existing regression gap

Current focused regression:

```text
tests/Pkc.CSharp.Tests/AngularUnsupportedComponentImportIndirectionAuthorityRegressionTests.cs
Scalar_alias_of_indirect_external_component_imports_fails_closed
```

uses the semicolon form and therefore does not protect the ASI spelling.

The generic repair must not merely add one exact text special case. It must recognize supported scalar alias declarations independently of optional semicolon spelling while preserving fail-closed behavior and avoiding authority creation from ambiguous expressions/scopes.

## Exact-SHA gate evidence

The reviewed production SHA has green push workflows:

```text
CI                         35832501567 PASS
Loren pinned/external      35832501543 PASS
Loren-main canary          35832501552 PASS
Jellyfin parity            35832501534 PASS
```

These remain useful safety evidence, but the independent counterexample keeps D open.

## Local review environment

The shell in the rereview environment could not resolve `github.com`, so there was no repository-local clone for .NET build/test execution. The review used exact-SHA files and Actions records through the authenticated GitHub connector plus local TypeScript compiler verification.

Do not rewrite this as local PKC test evidence. The implementation session must perform the normal regression-first local verification where a working checkout is available.

## Required next implementation checkpoint

Work only on the R7.10 blocker until it is repaired and externally rereviewed.

```text
inspect current main
→ add focused semicolonless scalar-alias regression
→ confirm RED
→ implement minimum generic semicolon-independent scalar-alias fail-close
→ run focused R7.10 tests
→ run related frontend/render-authority tests
→ run broader relevant suite/build
→ review full diff
→ update status/handoff and create rereview #18 request
→ one coherent implementation commit
→ one push
→ exact-SHA CI/Loren/Jellyfin gates
→ independent rereview #18
```

Do not use CI as the edit/test loop and do not push speculative red fixes.

## Demo-critical order after D eventually passes

Only after a fresh independent PASS closes D:

```text
E0 — Repository Discovery + bounded run readiness

RD1 inventory + safe exclusion
→ RD2 application boundaries + ownership
→ RD3 vendor/custom frontend classification
→ RD4 runtime/plugin provenance
→ RD5 deterministic ScanPlan
→ RD6 scoped/bounded semantic execution
→ RD7 coverage + observability + plan-only inspection
→ RD8 private large-repo validation
→ prove pkc run produces the intended .pkc/workspace practically

E1 — Remaining product-value repairs

construction/default/computation
→ semantic integration side effects
→ feature-summary rule fidelity

E2 — Product acceptance

R7.14 positive real-project yield
→ Level-2 known-answer benchmark
→ portable workspace acceptance
```

Production checkpoints remain sequential. Independent reading, source inspection, and non-conflicting verification may be parallelized within the active checkpoint when useful.

## E0 technical principles — prepared, still locked

- Discovery MUST precede expensive semantic scans.
- The repository is an application/component graph, not a homogeneous root.
- Whole-repository discovery coverage is required even if semantic execution occurs in bounded waves.
- Third-party runtime assets remain visible without deep-scanning their internals.
- Locally modified vendor code receives narrow first-party carve-outs when deterministic evidence supports them.
- Tests are evidence, not production authority.
- Runtime/plugin edges require provenance; naming similarity is never sufficient.
- UNKNOWN is valid and must remain explicit.
- Generated/restorable areas may be auto-excluded only with strong evidence.
- Discovery itself must stay shallow/bounded; it must not become a second whole-source semantic scan.
- Scan decisions must be machine-readable, stable and inspectable.
- A plan-only/discovery inspection path must exist before expensive semantic execution.
- Coverage must remain honest: omitted/unsupported required areas prevent a false READY claim.
- Private validation must sanitize all reported evidence.

Primary design packet:

```text
docs/plans/2026-09-24-repository-discovery-scan-planning-plan.md
docs/reviews/2026-09-24-repository-discovery-scan-planning-self-review.md
docs/plans/2026-09-24-demo-critical-sequential-execution-plan.md
```

## Fix #4 candidate — keep parked

Validated candidate remains:

```text
branch  fix/product-value-construction-state
commit  35c8e5c5f856e15568aa963bb2d76268008c5570
        fix: prove observable constructed state
```

Prior validation:

```text
Release build          PASS
focused regressions    8 / 8 PASS
C# full suite          282 / 282 PASS
frontend full suite     23 / 23 PASS
real Kesetovic retry   35902984101 PASS
```

Do NOT merge this before D passes and E0 completes. It adds another semantic pass while RD6 is expected to change scanner orchestration/scope.

## Workspace / privacy boundary

Preferred product flow:

```text
pkc run <TEAM_REPOSITORY_PATH>
cd <TEAM_REPOSITORY_PATH>/.pkc/workspace
```

- PRODUCT/TRACE: workspace only, no source fallback.
- Benchmark phase 1: workspace only; freeze answer before source inspection.
- Phase 2: minimum source cross-check only in approved environment.
- Private-repo research/benchmarks: aliases, counts, classifications, resource deltas and failure categories only; no proprietary source bodies/internal identifiers/config values.

## Exact next action

```text
R7.10 regression-first semicolonless scalar-alias repair
→ local focused/related/broader verification
→ one implementation commit/push
→ exact-SHA gates
→ independent rereview #18
```

Until that rereview passes, the terminal state is an **external acceptance gate still open**, not permission to advance RD1.
