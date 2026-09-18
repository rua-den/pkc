# Milestones

## V0.1 — C# evidence compiler — COMPLETE

Deterministic Roslyn scanner producing `.pkc/facts.json` with symbols, endpoints, routes, permissions, call evidence and source locations.

### V0.1.1 — Behavior evidence — COMPLETE

Added conditions/guards, throws, state-mutation candidates, call targets, combined routes and publication candidates.

### V0.1.2 — Workflow candidate grouping — COMPLETE

Endpoint-centered traversal groups compact backend evidence into `.pkc/feature-candidates.json`.

## V0.2 — First portable Markdown proof — COMPLETE

`pkc build <repository-path>` turns grounded candidates into canonical knowledge and portable Markdown.

## V0.3 — Frontend static evidence — COMPLETE

React/TypeScript static evidence adds routes, screens, actions, permission guards and API calls and links them to backend behavior.

## V0.4 — Product feature/workflow synthesis — ACTIVE LINE

V0.4 is complete only when generated portable knowledge is sufficiently rich for an AI to answer practical Product Owner questions about observable behavior, business conditions, value origin, mutation causality and cross-layer outcomes without re-reading source code.

PKC must keep these knowledge classes distinct:

```text
business conditions
value lineage / provenance
mutation / causality
```

Conservative downgrade must prevent false business claims without deleting deterministic lower-authority evidence. See `docs/product-knowledge-contract.md`.

### V0.4.1 — Frontend adapter architecture — COMPLETE

Common frontend adapter architecture and canonical UI evidence are accepted.

### V0.4.2 — PokeTrade real-system benchmark — COMPLETE

The runnable `.NET 10 + Angular 22` PokeTrade application is the known-answer behavioral acceptance benchmark.

### V0.4.3 — Analyzer fidelity hardening — COMPLETE

Completed semantic/fallback provenance and frontend/backend analyzer fidelity hardening.

Last accepted tool package remains:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

Package version is independent of roadmap milestone state.

### V0.4.4 — Loren knowledge readiness — COMPLETE / EXTERNAL REVIEW PASS

Final independent review:

```text
docs/reviews/2026-09-14-v0.4.4-external-rereview-4.md
```

### V0.4.5 — Independent real-repository generalization — COMPLETE / INDEPENDENT REVIEW PASS

Accepted benchmark:

```text
repository: jellyfin/jellyfin
pinned commit: 1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
```

V0.4.5 remains accepted with historical non-blocking warnings around duplicate HTTP verb extraction, feature-level promotion and large-pack signal/noise.

### V0.4.6 — Business logic reconstruction — COMPLETE / INDEPENDENT REVIEW PASS

Accepted production checkpoint:

```text
c310e893762997f34562a6b3a62dbab2b05c0c93
fix: fail closed on Queryable authority
```

Final independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-10.md
PASS / COMPLETE
```

Accepted boundary remains closed:

```text
B6.1 PASS — exact C# invocation semantic identity
B6.2 PASS — conservative configured-item ownership
B6.3 PASS — module-qualified Angular service ownership
B6.4 PASS — observable predicate authority conservative across projection/callback/constructor/Queryable boundaries
```

Do not reopen V0.4.6 without a new compile-valid and behavior-valid contradiction.

Historical exact-production gates:

```text
CI + PKC tests + WorkPlay + PokeTrade   34990080620 — PASS
pinned Loren                            34990080707 — PASS
Loren-main canary                       34990080551 — PASS
pinned Jellyfin                         34990080546 — PASS
```

Non-blocking rereview-10 warnings carried into V0.4.7:

```text
W10.1 rendered Evidence does not explicitly print `observed-only`.
W10.2 unreachable Queryable names remain in old safe-operation sets behind the fail-closed guard.
```

### V0.4.7 — Cross-layer PO question readiness — CURRENT / IN PROGRESS

Purpose: compile cross-entity and cross-layer evidence strongly enough that portable knowledge can answer where values came from, how they change, how they reach the UI, and what backend/frontend conditions jointly determine observable outcomes.

Acceptance contract:

```text
docs/v0.4.7-acceptance-plan.md
```

Required Product Owner questions include:

```text
Where did this value originally come from?
If the upstream value changes later, does the existing downstream value change automatically?
What code path can change this value after creation?
Was this value directly copied or computed?
What was the last observed source before the value was persisted or returned?
What backend conditions and frontend conditions jointly determine whether an item is visible?
How did the value move through backend → DTO/projection → API → frontend composition?
If authority is incomplete, what lineage or causal evidence is still deterministically known?
```

Canonical value-lineage fixture begins with:

```text
ProductGroup.Price
→ Product.Price
→ Service.Price
```

No lineage may be inferred merely from matching names such as:

```text
Product.Price
Service.Price
Dto.Price
Component.price
```

#### V0.4.7 implementation checkpoints

| Checkpoint | PO question | Current state | Completion boundary |
| --- | --- | --- | --- |
| A — Origin and copy timing | Where did this value come from? Does an upstream change alter this existing value? | **PASS / COMPLETE** | Snapshot + dynamic positives, exact identity/path/storage negatives, source traceability and PO-facing Markdown. |
| B — Computation and later change | Was it calculated? What can overwrite it? What was the last source before return/persistence? | **IMPLEMENTATION GREEN / INDEPENDENT REVIEW REQUIRED** | Portable derivation, original origin, later mutation/override and supported terminal-source proof, plus fail-closed negatives. |
| C — Backend to API | What backend value supplies this response field? | **LOCKED behind B review** | Proven entity/domain → DTO/projection → API mapping rendered with source locations. |
| D — API to UI | What feeds the displayed value and controls its visibility? | **LOCKED** | Proven frontend binding/composition and joint backend/frontend explanation with separate authorities. |
| E — Product acceptance | Can an AI answer the agreed questions using only the knowledge pack? | **LOCKED** | Blind knowledge-only review, portable folder/bundle/ZIP parity/no-leak, and exact-SHA cross-benchmark gates. |

#### Checkpoint A — accepted

Accepted A code checkpoint:

```text
09a0c7f2a058adfd7ddb7b4ac2feb2d17580a429
feat: prove bounded reference dynamic lineage
```

A proves both:

```text
stored scalar snapshot
ProductGroup.Price → Product.Price → Service.Price

reference / dynamic read-time dependency
ProductGroup.Price → Service.CurrentGroupPrice
```

It retains deterministic lower-authority evidence when composition authority is lost and fails closed across the accepted receiver/alias/effect/control-flow hazards.

Exact-main A gates all passed:

```text
CI + WorkPlay + PokeTrade   35232224202
pinned Loren                35232224014
Loren-main                  35232224029
pinned Jellyfin             35232223958
```

A is closed.

#### Checkpoint B — implementation green, review pending

Exact B implementation checkpoint on `main`:

```text
e5b0d47c82b6db99f5184292730919421a6d2a06
feat: prove V0.4.7-B computation and causality
```

Delivery/review note:

```text
docs/reviews/2026-09-17-v0.4.7-b-computation-causality-delivery.md
```

The bounded supported proof now covers:

```text
multi-input scalar arithmetic derivation
later constant override after a proven current value
original origin retained separately from override causality
last proven source before a supported direct return
```

Knowledge-class separation is required and currently implemented:

```text
Value lineage → derivation/origin/terminal source
State changes  → later override/causality
Rules          → no automatic B promotion
```

B fails closed on branch/loop/try/conditional shapes, opaque invocation, reference parameters, non-fresh alias/reassignment, custom/non-auto scalar accessors, unsupported expression operations, compound/unary scalar writes, and unresolved target-project semantics. This is not a general expression/dataflow/alias solver.

Permanent B regressions include runtime derivation (`100 - 10 = 90`), runtime override (`120`), candidate/synthesis/Markdown delivery, custom getter/invocation/branch/alias negatives, stale-terminal compound/unary negatives, and no-project-semantic fail-closed behavior.

Exact-main B gates all passed:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35339292475 — PASS
pinned Loren                                35339292546 — PASS
Loren-main canary                           35339292501 — PASS
pinned Jellyfin                             35339292495 — PASS
```

Core:

```text
Release build:       0 warnings / 0 errors
C# tests:            114 / 114 PASS
frontend tests:      13 / 13 PASS
tool pack/install:   PASS
WorkPlay:            PASS
PokeTrade:           PASS
```

Pinned Jellyfin portable gate remains green:

```text
facts:               43,365
relations:           195,316
workflow candidates: 386
product features:    116
Markdown files:      504
project-semantic:    43,365 / 43,365
portable parity:     PASS
raw .pkc/src leak:   none
artifact id:         10544800985
artifact digest:     sha256:c9884964e48daae5f3b511daf361dd2d0ceb25a06ba6504ca53b041c276106cc
artifact size:       9,162,486 bytes
```

**B is not COMPLETE until independent review passes.** Astra must review the exact current main checkpoint before C begins. A review blocker must be reproduced regression-first and fixed generically; a PASS permits docs to close B and unlock C.

#### Next only after B PASS — checkpoint C

C must answer:

> What backend value supplies this response field, and how did it travel through domain/entity state → DTO/projection → API output?

Start C regression-first only after B is explicitly marked PASS / COMPLETE. Require deterministic target-project identity, source locations, portable Markdown delivery and fail-closed behavior for unsupported projection/alias/conversion/custom-accessor/effect shapes.

Do not advance past an earlier checkpoint while its review or regression gate is red.

#### Delivery discipline

Each change must serve a named PO question and an assertion on generated knowledge. Supported positive cases must produce useful PO-facing answers; all-unknown output is not acceptance. Unsupported cases may retain observations without stronger claims.

Do not opportunistically refactor unrelated areas or build a general solver. Use local focused/full validation when available and CI as the clean-environment final gate for coherent checkpoints.

#### Version semantics during V0.4.7

Roadmap, package and serialized schema versions are independent. The accepted package remains:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

Current schema examples remain:

```text
0.4.4-csharp-raw
0.4.4
0.4.6
0.4.3-frontend
```

Do not mechanically bump package or schema versions because a roadmap checkpoint advances.

### V0.5 — Azure DevOps input evidence — LOCKED

Azure DevOps is planned as an additional compiler input for requirement intent, Epic/Feature/PBI history, status and traceability. It must not compensate for missing code-derived business logic.

V0.5 may start only after the V0.4.7 / V0.4.x PO-question-readiness exit gate independently passes.

## V0.6 — Incremental compilation

Use file/symbol hashes and dependency impact to rebuild only affected evidence and knowledge. Add PR/CI knowledge diffs.

## V0.7 — Runtime UI exploration

Use browser automation to confirm actual user-visible flows and states.

## V0.8 — Product insight

Surface gaps, inconsistencies, requirement-vs-implementation drift and improvement opportunities with evidence.
