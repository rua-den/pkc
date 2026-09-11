# Milestones

## V0.1 — C# evidence compiler — COMPLETE

Deterministic Roslyn scanner producing `.pkc/facts.json` with symbols, endpoints, routes, permissions, semantic calls and source locations.

### V0.1.1 — Behavior evidence — COMPLETE

Added conditions/guards, throws, state-mutation candidates, semantic call targets, combined routes and publication candidates.

### V0.1.2 — Workflow candidate grouping — COMPLETE

Endpoint-centered traversal groups compact backend evidence into `.pkc/feature-candidates.json`.

## V0.2 — First portable Markdown proof — COMPLETE

`pkc build <repository-path>` turns grounded candidates into canonical knowledge and portable Markdown.

## V0.3 — Frontend static evidence — COMPLETE

React/TypeScript static evidence adds routes, screens, actions, permission guards and API calls and links them to backend behavior.

## V0.4 — Product feature/workflow synthesis — COMPLETE

Actions are rendered as workflow Markdown and grouped into product-level feature Markdown plus `knowledge/index.md` and `.pkc/product-features.json`.

### V0.4.1 — Frontend adapter architecture — COMPLETE

- `IFrontendAdapter` contract
- framework-agnostic `FrontendScanner`
- React and Angular adapters emit the same canonical `ui-*` facts
- generic `UI action → API call` relation linker
- CLI has no Angular-vs-React branch
- cross-file Angular component → service API pattern verified

MVC/Razor, Blazor and Vue remain future adapters; they are not part of this milestone.

### V0.4.2 — PokeTrade real-system benchmark — REOPENED

The `.NET 10 + Angular 22` PokeTrade application builds, runs and has a green principal Order → WorkPlay → Delivery smoke path. PKC also compiles the same source tree and prior benchmark work fixed several real correctness bugs.

However, a later file-by-file review of the complete sample source showed that the benchmark had been declared complete too early.

Remaining benchmark blockers:

- business-important object construction such as `WorkPlay.QuantityToBuy = shortage + ReorderLevel` is filtered too aggressively;
- computed domain expressions such as `Order.Total = Sum(quantity * unit price)` are not carried into knowledge;
- collection/loop semantics in `FulfillWaitingOrders` are under-described;
- Angular status-based button visibility is not captured as UI evidence;
- two-hop component → service → HTTP read/refresh chains are incomplete;
- authorization policy implementation semantics are not analyzed;
- CI needs representative branch coverage, not only the principal happy path.

V0.4.2 closes only after source, live behavior and generated Markdown have been reviewed together for the whole mini application at an appropriate business-behavior level.

### V0.4.3 — External real-project trial — BLOCKED

Do not run the external real-project trial as the next engineering step until V0.4.2 is clean again.

When unblocked, run the packaged PKC tool against one genuine external repository and classify findings as wrong claim, missing important behavior, noise, or unsupported stack/pattern.

## V0.5 — Azure DevOps evidence

Ingest Epic/Feature/PBI/Sprint/history and link product/delivery evidence through PRs/commits where possible.

Do not start this milestone until the external real-project trial has been reviewed.

## V0.6 — Incremental compilation

Use file/symbol hashes and dependency impact to rebuild only affected evidence and knowledge. Add PR/CI knowledge diffs.

## V0.7 — Runtime UI exploration

Use browser automation to confirm actual user-visible flows and states.

## V0.8 — Product insight

Surface gaps, inconsistencies, requirement-vs-implementation drift and improvement opportunities with evidence.
