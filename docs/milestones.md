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

### V0.4.1 — Frontend adapter architecture — CURRENT

Narrow refactor only:

- add `IFrontendAdapter`
- make frontend orchestration framework-agnostic
- migrate React and Angular scanners behind the adapter boundary
- normalize both into the existing canonical `ui-*` facts
- link common `UI action → API call` relations outside framework adapters
- remove Angular-vs-React branching from the CLI

MVC/Razor, Blazor and Vue are **not** implemented in this milestone. They should be future adapters using the same contract.

### V0.4.2 — PokeTrade real-system benchmark

Use the runnable `.NET 10 + Angular` PokeTrade sample to validate that PKC knowledge matches an independently runnable application across Order, WorkPlay and Delivery flows.

## V0.5 — Azure DevOps evidence

Ingest Epic/Feature/PBI/Sprint/history and link product/delivery evidence through PRs/commits where possible.

## V0.6 — Incremental compilation

Use file/symbol hashes and dependency impact to rebuild only affected evidence and knowledge. Add PR/CI knowledge diffs.

## V0.7 — Runtime UI exploration

Use browser automation to confirm actual user-visible flows and states.

## V0.8 — Product insight

Surface gaps, inconsistencies, requirement-vs-implementation drift and improvement opportunities with evidence.
