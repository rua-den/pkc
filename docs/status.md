# PKC Status

Last updated: 2026-09-23

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                 PASS / COMPLETE
V0.4.5 real-repository generalization            PASS / COMPLETE
V0.4.6 business logic reconstruction             PASS / COMPLETE
V0.4.7-A origin and copy timing                  PASS / COMPLETE
V0.4.7-B computation and later change            PASS / COMPLETE
V0.4.7-C backend to API                          PASS / COMPLETE
V0.4.7-D API to UI / R7.9 binding                PASS / COMPLETE
V0.4.7-D mutation-causality blocker              PASS / CLOSED
V0.4.7-D API to UI / R7.10 joint visibility      REPAIRED / ALL GATES PASS / PENDING REREVIEW #15
V0.4.7-D overall                                 PENDING INDEPENDENT REREVIEW #15
V0.4.7-E product acceptance                      LOCKED behind D
R7.14 real-project positive yield                NOT PASS / REQUIRED FOR E
V0.5 Azure DevOps input evidence                 LOCKED
AI workspace + continuous-update plan            PREPARED / NOT UNLOCKED
```

V0.4.7 acceptance is defined in `docs/v0.4.7-acceptance-plan.md`; the permanent product contract is `docs/product-knowledge-contract.md`.

## Exact production candidate under review

```text
47e098dbe910b7f6cfd933a0595370524bec1fb2
fix: correct resolved package root type
```

The relevant repair chain is:

```text
8f667abc819f048b3dc85fc834677b7ca30f5518  fix: recognize external Angular component selectors
67c67fc25b6488dbc9f110a1b05c53a4bfee1a6c  fix: resolve nested Angular dependency selectors
47e098dbe910b7f6cfd933a0595370524bec1fb2  fix: correct resolved package root type
```

A docs-only `[skip ci]` checkpoint may sit above production on `main`. Review production behavior at `47e098...`; do not reset `main`.

## Accepted predecessors

```text
V0.4.6     c310e893762997f34562a6b3a62dbab2b05c0c93
V0.4.7-A   09a0c7f2a058adfd7ddb7b4ac2feb2d17580a429
V0.4.7-B   17fd30b3a4b8178208adabc12c40dee060bedb54
V0.4.7-C   fbb64b9917da1f63362558355201ff7998384ba0
R7.9       fc4bfa7042f59620b2c7ba1c4f6700cf3b02172a
mutation   67624944da27ff1f1f5a1154018a255aae11d1fe
```

Keep these closed unless a new real regression is demonstrated.

## Rereview #14 result and repaired boundary

Independent rereview #14 of `3e6fa774...` **FAILED**. The blocker was external Angular dependency components whose selectors are not visible in product-source `@Component` declarations. A native-looking host such as `div[ext-shell]` can be a component host from an imported package; without selector knowledge PKC could over-promote a lexical child interpolation even when that component does not project the child.

Result record:

`docs/reviews/2026-09-23-v0.4.7-d-r7.10-independent-rereview-14.md`

`8f667abc...` added bounded imported-package selector discovery from Angular Ivy `ɵɵComponentDeclaration` metadata. It intentionally reads dependency declaration metadata only for projection authority; dependency source remains excluded from product facts/knowledge, and directives do not become projection blockers merely because they have selectors.

A continuation review then found a second compile-valid layout hole: `8f667abc...` resolved packages only from `<repository-root>/node_modules`. A normal monorepo/nested Angular app can instead use:

```text
repo/
  frontend/
    package.json
    src/price.component.ts
    node_modules/@vendor/ui/index.d.ts
```

The app can resolve `@vendor/ui` correctly while PKC misses the selector if it only checks root `node_modules`.

`67c67fc...` repaired this generically by resolving each imported package from the importing TypeScript file directory upward toward repository root, choosing the nearest matching `node_modules` package. Scoped packages and package subpath imports remain supported; root-hoisted dependencies still resolve. Dependency package symlinks are accepted only when their resolved target remains inside the selected `node_modules` tree. A nested-app regression proves the authority downgrade.

The first CI attempt exposed one compile-only type mismatch around `ResolveLinkTarget()` (`FileSystemInfo` versus `DirectoryInfo`). `47e098db...` corrects that type without changing the resolver semantics and is the final candidate under review.

## Regression coverage

Relevant projection coverage now includes:

```text
tests/Pkc.CSharp.Tests/AngularComponentProjectionRenderAuthorityRegressionTests.cs
tests/Pkc.CSharp.Tests/AngularNestedNodeModulesProjectionAuthorityRegressionTests.cs
```

The nested regression constructs an Angular app under `frontend/` with its own `node_modules/@vendor/ui/index.d.ts`, imports the external component, uses an attribute selector host, and proves no authoritative `ui-member-render` / `ui-member-visibility` survives beneath the unproven projection boundary.

## Exact-SHA standard verification

All required standard gates passed on exact production `47e098dbe910b7f6cfd933a0595370524bec1fb2`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35818350915 — PASS
pinned Loren                                35818350859 — PASS
Loren-main canary                           35818350997 — PASS
pinned Jellyfin + parity/provenance         35818350930 — PASS
```

```text
Release build        0 warnings / 0 errors
C# tests             248 / 248 PASS
frontend tests       13 / 13 PASS
tool pack/install    PASS
WorkPlay             PASS
PokeTrade            PASS
```

Pinned Jellyfin:

```text
source build          PASS, 0 warnings / 0 errors
facts                 43,365
relations             195,316
workflow candidates   386
product features      116
knowledge Markdown    504 files
analysis modes        43,365 / 43,365 project-semantic
portable parity       PASS
artifact              10732606989
sha256:1d2b38aadc18828a75625ea94651f2a1acfc3a3813a3cc9df0c57d9be4b1bde2
```

Local repository execution is not claimed in this continuation because the available execution container could not resolve `github.com` and has no local .NET toolchain. Exact-SHA GitHub Actions are the completed verification evidence.

## Final pinned three-repository safety benchmark

A temporary wrapper branch based exactly on production `47e098...` changed only one workflow branch-trigger line:

```text
base production: 47e098dbe910b7f6cfd933a0595370524bec1fb2
wrapper commit:  437d14a9b9ed36ce24e7fd8edfb2cef31eed7f6c
run:             35818835753 — PASS, 3 / 3 jobs
```

Direct artifact inspection found, for every pinned repository:

```text
ui-member-render:           0
ui-member-visibility:       0
renderAuthority markers:    0
selected API/R7.9 terminal: 0
joint/combined visibility:  0
```

Counts remain stable:

```text
jin12-xyz/CRM                                      415 facts / 1,580 relations / 25 knowledge files
hackersandwizards/agentic-engineering-training-angular 441 facts /   660 relations / 26 knowledge files
kesetovic/crm-system                               488 facts / 2,054 relations / 28 knowledge files
```

Artifacts:

```text
jin12          10732532350  sha256:6f4e41e7b18357d5a06036bd14a39b15988ba437b013f78b0ecfee9af9b15f9f
agentic        10733025143  sha256:6c360ae70b72d9d8678b0d209f233d565f176a2aa878a5a20d62876638adb040
kesetovic      10732851433  sha256:16a95ee2bbb4155d33a1c3f6dfdf60767f036d389a83bff98e2954a86f348c54
```

Agentic mutation-causality remains closed: the two raw `UpdatedAt` mutations retain `runtime-pattern-variable / caller-object-unproven`, and their exact fact IDs occur zero times outside raw facts in candidates, product features and generated Markdown.

Interpretation:

```text
Safety / fail-closed: PASS
R7.14 positive real-project yield: NOT PASS
```

Detailed evidence: `docs/benchmarks/2026-09-23-r7.10-nested-dependency-selector-benchmark.md`.

## Current external gate

Required next gate:

```text
independent rereview #15 of exact 47e098dbe910b7f6cfd933a0595370524bec1fb2
```

Request: `docs/reviews/2026-09-23-v0.4.7-d-r7.10-rereview-15-request.md`.

If rereview #15 finds no new compile-valid/runtime-valid false-positive blocker, it may mark R7.10 and all of D PASS / COMPLETE and unlock only E. R7.14 remains required for E; V0.5 and the prepared AI-workspace/update initiative remain locked until V0.4.7 completes.

This implementation continuation repaired the blocker and must not self-certify its own production candidate.

## Prepared future productization packet

```text
docs/plans/2026-09-22-ai-workspace-continuous-update-plan.md
docs/reviews/2026-09-22-ai-workspace-continuous-update-plan-self-review.md
```

After V0.4.7 is explicitly PASS / COMPLETE, preferred sequencing is AI workspace + `run/verify`, then semantic `update/diff`, then Azure DevOps evidence.

## Version semantics

```text
roadmap:             V0.4.7-D / R7.10 pending independent rereview #15
tool/package:        RuaDen.Pkc.Tool 0.4.3-preview.2
C# raw schema:       0.4.4-csharp-raw
merged facts schema: 0.4.4
cross-stack schema:  0.4.6
frontend schema:     0.4.3-frontend
```
