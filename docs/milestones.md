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

That requires deterministic evidence for relevant predicates, configured/static values, data flow and frontend presentation where source can prove them, plus explicit unknowns where runtime state cannot be proven statically.

### V0.4.1 — Frontend adapter architecture — COMPLETE

- `IFrontendAdapter` contract
- framework-agnostic `FrontendScanner`
- React and Angular adapters emit the same canonical `ui-*` facts
- generic `UI action → API call` relation linker
- CLI has no Angular-vs-React branch

### V0.4.2 — PokeTrade real-system benchmark — COMPLETE

The runnable `.NET 10 + Angular 22` PokeTrade application is the known-answer behavioral acceptance benchmark.

### V0.4.3 — Analyzer fidelity hardening — COMPLETE

Completed semantic/fallback provenance and frontend/backend analyzer fidelity hardening.

Last accepted tool package:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

### V0.4.4 — Loren knowledge readiness — COMPLETE / EXTERNAL REVIEW PASS

Purpose: prove that the generated portable pack can explain a genuine product/system at the correct abstraction level without source access.

Final independent review:

```text
docs/reviews/2026-09-14-v0.4.4-external-rereview-4.md
```

Accepted baseline includes PokeTrade, pinned Loren, Loren-main and portable handoff parity, with conservative cross-stack semantic authority.

### V0.4.5 — Independent real-repository generalization gate — COMPLETE / INDEPENDENT REVIEW PASS

Purpose: prove PKC did not simply overfit PokeTrade + Loren before deepening business-logic reconstruction.

Accepted benchmark:

```text
repository: jellyfin/jellyfin
pinned commit: 1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
```

Records:

```text
docs/trials/2026-09-14-v0.4.5-jellyfin.md
docs/trials/2026-09-14-v0.4.5-jellyfin-crosscheck.md
docs/reviews/2026-09-14-v0.4.5-v0.4.6-independent-review.md
```

The blind pass was frozen before source inspection. Source cross-check exposed a generic inherited ASP.NET Core controller-route defect. The issue was reproduced regression-first and fixed generically while preserving explicit derived `[Route("")]` override behavior.

V0.4.5 is accepted with non-blocking warnings:

```text
W3 duplicate HTTP verb extraction
W4 feature-level rule promotion
W5 large-pack signal/noise
```

PokeTrade + Loren + Jellyfin regressions remained green on the reviewed candidate.

### V0.4.6 — Business logic reconstruction — CURRENT / INDEPENDENT REVIEW FAILED

Purpose: compile deterministic business-decision evidence strongly enough that an AI can answer practical `when`, `why`, `which conditions` and `what makes this visible/eligible` questions from generated knowledge.

The current increment successfully demonstrates the intended known-answer Mewtwo path, including:

- preservation of raw boolean predicate expression structure;
- declarative configured values for the supported sample shape;
- Angular API-result assignment and rendered-list evidence;
- a cross-stack PO-question regression.

However independent review found three generic false-claim paths that must be fixed before V0.4.6 can pass.

Review record:

```text
docs/reviews/2026-09-14-v0.4.5-v0.4.6-independent-review.md
```

Blocking requirements:

```text
B6.1 prove supported business-predicate operation semantics
     - lexical method-name matching is insufficient
     - custom Where/Any/First-style methods must not become LINQ rules

B6.2 prove configured-item ownership
     - nested object creations under a collection initializer must not be
       mislabeled as additional configured items of that collection

B6.3 prove Angular API-service ownership for result/list flow
     - same method name across different services must not cross-link one
       component's list flow to the wrong backend endpoint
```

V0.4.6 passes only when all three are regression-locked and fixed generically, then exact gates pass again:

```text
PKC tests
PokeTrade known-answer/live acceptance
pinned Loren
Loren-main canary
pinned Jellyfin
portable parity / no source leak
independent re-review
```

### V0.4.7 — Cross-layer PO question readiness — LOCKED UNTIL V0.4.6 PASSES

Purpose: broaden the V0.4.6 proof from supported direct patterns into robust cross-layer product-behavior understanding.

Target coverage includes:

- frontend visibility/filter predicates that independently hide or include an item;
- DTO/projection/computed transformations that change observable eligibility/display state;
- richer stores/RxJS/state data-flow where deterministic proof is possible;
- composition of backend predicate + API/DTO transformation + frontend predicate into observable outcome;
- explicit boundaries around DB/remote configuration/feature flags/external state;
- high-signal feature-level promotion so PO-relevant rules are not buried.

Do not start V0.4.7 while V0.4.6 blockers remain.

## V0.5 — Azure DevOps input evidence — LOCKED

Azure DevOps is planned as an additional compiler input describing requirement intent and product/work-item context around code: Epic/Feature/PBI, acceptance intent, sprint/history/status and links through PRs/commits where possible.

ADO must not compensate for missing code-derived business logic.

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
