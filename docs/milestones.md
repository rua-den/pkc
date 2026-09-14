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

V0.4 is complete only when generated portable knowledge is sufficiently rich for an AI to answer practical Product Owner questions about observable behavior, business conditions and cross-layer outcomes without re-reading source code.

The exit standard is not merely endpoint/workflow coverage. PKC must preserve enough grounded logic to answer questions of this class:

> When is entity X sellable/visible on the web, and what exact conditions must be true for it to appear?

That requires deterministic evidence for the relevant predicates, configured/static values, data flow and frontend presentation where source can prove them, plus explicit unknowns where runtime state cannot be proven statically.

### V0.4.1 — Frontend adapter architecture — COMPLETE

- `IFrontendAdapter` contract
- framework-agnostic `FrontendScanner`
- React and Angular adapters emit the same canonical `ui-*` facts
- generic `UI action → API call` relation linker
- CLI has no Angular-vs-React branch

### V0.4.2 — PokeTrade real-system benchmark — COMPLETE

The runnable `.NET 10 + Angular 22` PokeTrade application is the known-answer behavioral acceptance benchmark.

Completed hardening includes full source review, branch-level live API acceptance, computed domain properties, business object construction, collection/loop semantics, permission/status guards, multi-hop frontend linkage, observed policy definitions, 400/404/409 response semantics and generated knowledge assertions.

### V0.4.3 — Analyzer fidelity hardening — COMPLETE

Completed:

- C# target-project semantic enrichment through `MSBuildWorkspace`;
- explicit semantic/fallback provenance and declaration node-match confidence;
- Angular TypeScript syntactic-AST primary path;
- explicit confidence split for structural facts vs syntactic HTTP-call evidence;
- explicit Angular template and React fallback provenance;
- analyzer fidelity locked against PokeTrade.

Last accepted tool package:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

### V0.4.4 — Loren knowledge readiness — COMPLETE / EXTERNAL REVIEW PASS

Purpose: prove that the generated `knowledge/` pack can explain a genuine product/system at the correct abstraction level without source access.

Benchmark roles:

```text
pinned Loren commit → deterministic blocking acceptance
current Loren main   → moving non-blocking canary
```

Completed hardening includes Minimal API support, source-scope contamination, conditional endpoints, failure/direct response semantics, multi-project semantic loading, mutation noise, flow noise, authentication side effects, fact-ID response collisions, canonical handoff reconciliation, repository-neutral capability-flow ranking, frontend product-source filtering and conservative cross-stack validation-condition equivalence.

V0.4.4 passed the layered-output + blind knowledge-only + source comparison + external-review gate on 2026-09-14.

Final independent review:

```text
docs/reviews/2026-09-14-v0.4.4-external-rereview-4.md
```

Accepted review state:

```text
PokeTrade known-answer regression                 PASS
Loren blind knowledge-only comprehension          PASS
Loren pinned external trial                       PASS
Loren-main canary                                 PASS
portable handoff parity                           PASS
independent external review                       PASS
```

The accepted validation-equivalence contract is conservative: if cross-stack condition semantics or validated-object provenance cannot be deterministically proven, PKC must not emit `consistent / high`.

Detailed execution/exit plan: `docs/real-project-trial.md`.

### V0.4.5 — Independent real-repository generalization gate — IMPLEMENTATION + INTERNAL TRIAL COMPLETE / INDEPENDENT REVIEW REQUIRED

Purpose: prove PKC did not simply overfit PokeTrade + Loren before deepening business-logic reconstruction.

Selected benchmark:

```text
repository: jellyfin/jellyfin
pinned commit: 1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
```

The blind knowledge-only review was frozen before source inspection. Source cross-check then exposed a generic ASP.NET Core inherited-controller-route gap. That blocker was reproduced first, fixed generically and followed by PokeTrade + Loren + Jellyfin regression runs.

Records:

```text
docs/trials/2026-09-14-v0.4.5-jellyfin.md
docs/trials/2026-09-14-v0.4.5-jellyfin-crosscheck.md
```

V0.4.5 passes only when an independent reviewer accepts that:

- critical product questions have no blocker false claims;
- important behavior is answerable or explicitly unknown;
- product-level output is sufficiently high-signal;
- evidence remains traceable;
- fixes remain generic and contain no repository-specific exception;
- PokeTrade + Loren still pass after compiler changes;
- Jellyfin warnings and residual limitations are classified correctly.

Coding-thread completion is not an independent PASS.

### V0.4.6 — Business logic reconstruction — CURRENT / REVIEW CANDIDATE

Purpose: compile deterministic business-decision evidence strongly enough that an AI can answer practical `when`, `why`, `which conditions` and `what makes this visible/eligible` questions from generated knowledge.

Current increment adds generic evidence for:

- common C# LINQ selection/inclusion predicates while preserving the actual boolean expression and AND/OR grouping;
- declaratively configured objects behind simple predicate-source collections;
- readable grounded business-rule synthesis from those predicates and configured values;
- Angular API-result-to-component-collection binding;
- Angular rendered-list evidence;
- cross-stack linkage from backend endpoint to API result to rendered list.

Known-answer acceptance is the PokeTrade Mewtwo question. Compiled knowledge must establish, without source re-reading by the eventual consumer:

```text
Mewtwo VSTAR configured values
+ backend list-inclusion predicate
+ sale window
+ publication/web-enabled flags
+ stock requirement
+ GET /api/cards frontend result binding
+ CatalogComponent list rendering
```

The boolean condition must remain semantically meaningful. A flattened bag of identifiers is insufficient.

V0.4.6 passes only when:

- the Mewtwo PO-question regression passes from compiled evidence/knowledge;
- the implementation contains no Mewtwo/PokeTrade/repository-specific production exception;
- runtime or externally sourced values that cannot be proven remain explicit unknowns;
- predicate/configuration evidence remains source-traceable;
- PokeTrade, Loren and Jellyfin regressions remain green;
- independent review has no unresolved blocker.

Review request:

```text
docs/reviews/2026-09-14-v0.4.6-business-logic-review-request.md
```

### V0.4.7 — Cross-layer PO question readiness — NEXT / LOCKED UNTIL V0.4.6 REVIEW

Purpose: broaden the V0.4.6 proof from a supported direct pattern into robust cross-layer product-behavior understanding.

Target coverage includes:

- frontend visibility/filter predicates that can independently hide or include an item;
- DTO/projection/computed transformations that change observable eligibility or display state;
- more complex frontend state/data-flow patterns such as stores and RxJS transformation chains where deterministic proof is possible;
- composition of backend predicate + API/DTO transformation + frontend predicate into an observable outcome;
- explicit boundaries when database values, remote configuration, feature flags or external-service state cannot be statically known;
- high-signal feature-level promotion so PO-relevant rules are not buried only in workflow/evidence layers.

V0.4.x exits only after a blind knowledge-only PO-question gate demonstrates that important `when/why/what conditions/what happens if` questions can be answered correctly from the portable package, with source cross-check and independent review.

## V0.5 — Azure DevOps input evidence — LOCKED

Azure DevOps is planned as an additional compiler input describing requirement intent and product/work-item context around code: Epic/Feature/PBI, acceptance intent, sprint/history/status, and links through PRs/commits where possible.

Its purpose is to help developers and AI understand not only what the code currently does, but also the requirement/context that led to the implementation. It is not merely a delivery-reporting layer.

ADO must not compensate for missing code-derived business logic. Code behavior still has to be reconstructed truthfully from source evidence first.

V0.5 may start only after the V0.4.x PO-question-readiness exit gate independently passes. At minimum:

```text
PokeTrade known-answer PO regression              PASS
Loren blind knowledge-only comprehension          PASS
Jellyfin independent real-repo generalization     PASS
business-logic reconstruction                     PASS
cross-layer PO-question readiness                 PASS
cross-benchmark regression                        PASS
known boundaries documented honestly              PASS
independent external review                       PASS
```

If any gate is not PASS, remain in V0.4.x.

## V0.6 — Incremental compilation

Use file/symbol hashes and dependency impact to rebuild only affected evidence and knowledge. Add PR/CI knowledge diffs.

## V0.7 — Runtime UI exploration

Use browser automation to confirm actual user-visible flows and states.

## V0.8 — Product insight

Surface gaps, inconsistencies, requirement-vs-implementation drift and improvement opportunities with evidence.
