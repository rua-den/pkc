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

PKC must preserve three distinct knowledge classes:

```text
business conditions
value lineage / provenance
mutation / causality
```

Conservative downgrade must prevent false business claims without deleting deterministic lower-authority evidence. See `docs/product-knowledge-contract.md`.

Representative V0.4.x exit questions include:

> When is entity X sellable/visible on the web, and what exact backend/frontend conditions must be true for it to appear?

> Where did field X come from, was it copied or computed, and what can change it later?

> If an upstream value changes, does an existing downstream value update automatically or is it a stored snapshot?

### V0.4.1 — Frontend adapter architecture — COMPLETE

Common frontend adapter architecture and canonical UI evidence are accepted.

### V0.4.2 — PokeTrade real-system benchmark — COMPLETE

The runnable `.NET 10 + Angular 22` PokeTrade application is the known-answer behavioral acceptance benchmark.

### V0.4.3 — Analyzer fidelity hardening — COMPLETE

Completed semantic/fallback provenance and frontend/backend analyzer fidelity hardening.

Last accepted tool package:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

Package version is independent of roadmap milestone state.

### V0.4.4 — Loren knowledge readiness — COMPLETE / EXTERNAL REVIEW PASS

Final independent review:

```text
docs/reviews/2026-09-14-v0.4.4-external-rereview-4.md
```

The detailed Loren readiness execution steps remain preserved as historical evidence in `docs/real-project-trial.md`.

### V0.4.5 — Independent real-repository generalization gate — COMPLETE / INDEPENDENT REVIEW PASS

Accepted benchmark:

```text
repository: jellyfin/jellyfin
pinned commit: 1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
```

V0.4.5 remains accepted with non-blocking warnings around duplicate HTTP verb extraction, feature-level promotion and large-pack signal/noise.

### V0.4.6 — Business logic reconstruction — COMPLETE / INDEPENDENT REVIEW PASS

Purpose: compile deterministic business-decision evidence strongly enough that an AI can answer practical `when`, `why`, `which conditions` and `what makes this visible/eligible` questions from generated knowledge.

Accepted production checkpoint:

```text
c310e893762997f34562a6b3a62dbab2b05c0c93
fix: fail closed on Queryable authority
```

Final independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-10.md
reviewed production: c310e893762997f34562a6b3a62dbab2b05c0c93
verdict: PASS / COMPLETE
```

Final disposition:

```text
B6.1 PASS — exact C# invocation semantic identity; keep closed
B6.2 PASS — conservative configured-item ownership; keep closed
B6.3 PASS — active module-qualified Angular service ownership; keep closed
B6.4 PASS — observable predicate authority is conservative across projection, callback, constructor and Queryable-provider boundaries; keep closed
```

Accepted B6.4 hardening covers:

1. local/discarded predicates do not become observable rules merely because a LINQ call exists;
2. transformed/polarity-changing return contexts fail closed unless modeled;
3. arbitrary `Select` is not an unconditional preserving operation;
4. direct identity `Select(card => card)` is proven by symbol identity;
5. unsupported whole-item/unmodeled predicate dependencies cause conservative downgrade;
6. same-type method-group projection requires safe stored same-member copies;
7. custom setter, nested initializer and rewritten output effects fail closed;
8. callback/comparer-bearing ordering/equality operations do not preserve authority merely from LINQ target identity;
9. callback-free Enumerable pipeline preservation is limited to an audited exact-shape subset;
10. same-type clone construction must be proven inert;
11. exact `System.Linq.Queryable` predicate targets are observed-only without provider-semantics proof;
12. an Enumerable `Where` authority path fails closed after any Queryable pipeline hop;
13. downgraded predicate evidence is retained through `observes-predicate` instead of being deleted.

Non-blocking rereview-10 warnings carried into V0.4.7:

```text
W10.1 rendered Evidence does not yet print an explicit `observed-only` label.
W10.2 unreachable Queryable names remain in old internal safe-operation sets behind the fail-closed guard.
```

Do not reopen V0.4.6 solely for those warnings.

Accepted exact-production gates:

```text
CI + PKC tests + WorkPlay + PokeTrade   34990080620 — PASS
pinned Loren                            34990080707 — PASS
Loren-main canary                       34990080551 — PASS
pinned Jellyfin                         34990080546 — PASS
```

Pinned Jellyfin artifact:

```text
artifact id:     10405810551
digest:          sha256:8664945310d5fd0da3a0c838b001cc5fa343174410dc1ac6d05b1335e41b0257
size:            9,159,880 bytes
portable parity: PASS
raw .pkc leak:   NONE
src/ leak:       NONE
```

V0.4.6 is closed.

### V0.4.7 — Cross-layer PO question readiness — CURRENT / NEXT MILESTONE

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

Required semantic coverage includes:

```text
cross-entity value lineage
source/origin of values
copy/snapshot vs reference/dynamic semantics
derived/computed values
later mutation and override causality
DTO/projection and API output
frontend result binding/composition
frontend visibility/filter conditions
authority downgrade with evidence retention
```

Canonical lineage fixture:

```text
ProductGroup.Price
→ Product.Price
→ Service.Price
```

Each proven edge must distinguish as applicable:

```text
copy
snapshot
derivation
reference/dynamic
override
mutation
```

Blocking same-name collision regression:

```text
Product.Price
Service.Price
Dto.Price
Component.price
```

No lineage edge may be inferred merely from matching member/property names.

#### V0.4.7 implementation checkpoints

| Checkpoint | PO question | Deliverable and completion boundary |
| --- | --- | --- |
| A — Origin and copy timing | Where did this value come from? Does an upstream change alter this existing value? | Two proven backend edges in workflow Markdown, source traceability, snapshot and dynamic positives, and identity/path/storage negatives. |
| B — Computation and later change | Was it calculated? What can overwrite it? What was the last source before return/persistence? | Portable explanation of derivation, original origin, later mutation/override, and supported terminal-source proof. |
| C — Backend to API | What backend value supplies this response field? | Proven entity/DTO/API mapping rendered with source locations. |
| D — API to UI | What feeds the displayed value and controls its visibility? | Proven frontend binding/composition and joint backend/frontend explanation with separate authorities. |
| E — Product acceptance | Can an AI answer the agreed questions using only the knowledge pack? | Blind knowledge-only review, full folder/bundle/ZIP parity and no-leak checks, and exact-SHA cross-benchmark gates. |

Portable explanations start in A and grow with each checkpoint. E is final product verification, not the first rendering work. Detailed proof and regression requirements live in [the acceptance plan](v0.4.7-acceptance-plan.md), including the incorporated [readiness findings](reviews/2026-09-16-v0.4.7-plan-readiness-review.md).

Current next step is the first A copy/snapshot slice through scanner → candidate → synthesis → workflow Markdown. Completing that slice does not complete A until dynamic-read coverage also passes.

Do not advance past an earlier checkpoint while its regression gate is red.

V0.4.7 must preserve every accepted V0.4.6 authority and evidence-retention guardrail.

#### Delivery discipline

Each change must serve a named PO question and an assertion on generated knowledge. Supported positive cases must produce useful answers; all-unknown output is not acceptance. Unsupported cases may retain observations without stronger claims.

Limit analysis to proven supported shapes. Do not add unrelated refactoring, frameworks, evidence sources, or a general alias/path solver. Broaden a checkpoint only for its acceptance failure or a concrete counterexample. Run focused/local checks while editing; use the existing benchmark gates and independent review at coherent checkpoints, not as an endless edit loop. Preserve accepted history unless a new compile-valid, behavior-valid contradiction requires reopening it.

#### Version semantics during V0.4.7

Do not conflate:

```text
roadmap milestone version
tool/package version
evidence/schema version
```

Current accepted package remains:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

Current schema examples include:

```text
0.4.4-csharp-raw
0.4.4
0.4.6
0.4.3-frontend
```

Milestone progress does not mechanically bump package or schema versions. Schema versions change only when their serialized contract/semantics change and that change has explicit compatibility/regression coverage.

### V0.5 — Azure DevOps input evidence — LOCKED

Azure DevOps is planned as an additional compiler input for requirement intent, Epic/Feature/PBI history, status and traceability. ADO must not compensate for missing code-derived business logic.

Until ADO is integrated, its absence should be declared as a global knowledge boundary rather than repeated in every feature/workflow file.

V0.5 may start only after the V0.4.7 / V0.4.x PO-question-readiness exit gate independently passes.

The older V0.4.4/V0.4.5 real-project unlock checks are historical accepted prerequisites, not the complete current unlock condition. V0.4.7 cross-layer PO-question readiness is now the remaining V0.4.x gate.

## V0.6 — Incremental compilation

Use file/symbol hashes and dependency impact to rebuild only affected evidence and knowledge. Add PR/CI knowledge diffs.

## V0.7 — Runtime UI exploration

Use browser automation to confirm actual user-visible flows and states.

## V0.8 — Product insight

Surface gaps, inconsistencies, requirement-vs-implementation drift and improvement opportunities with evidence.
