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

## V0.4 — Product feature/workflow synthesis — COMPLETE

Actions are rendered as workflow Markdown and grouped into product-level feature Markdown plus `knowledge/index.md` and `.pkc/product-features.json`.

### V0.4.1 — Frontend adapter architecture — COMPLETE

- `IFrontendAdapter` contract
- framework-agnostic `FrontendScanner`
- React and Angular adapters emit the same canonical `ui-*` facts
- generic `UI action → API call` relation linker
- CLI has no Angular-vs-React branch

### V0.4.2 — PokeTrade real-system benchmark — COMPLETE

The runnable `.NET 10 + Angular 22` PokeTrade application is the behavioral acceptance benchmark.

Completed hardening includes:

- full source review across backend and frontend;
- branch-level live API acceptance beyond the happy path;
- computed domain properties;
- business object construction;
- collection/loop semantics;
- permission + status visibility guards;
- multi-hop frontend read/action linkage;
- observed policy definitions;
- 400 / 404 / 409 response semantics;
- generated knowledge assertions against the running system.

### V0.4.3 — Analyzer fidelity hardening — COMPLETE

External review identified that previous analyzer internals were weaker than the architecture wording could imply. This milestone makes fidelity explicit and strengthens the analyzer layer without changing the knowledge architecture.

Completed:

- C# target-project semantic enrichment through `MSBuildWorkspace`;
- semantic framework resolution verified for ASP.NET Core controller base types and HTTP attributes;
- explicit `loose-roslyn-fallback` when target-project semantic context is unavailable;
- Angular TypeScript AST primary path for components/routes/method structure/HTTP calls;
- explicit `angular-template-regex-fallback` for template actions/visibility;
- explicit React `regex-fallback` until a React AST adapter is implemented;
- `analysisMode` / `analysisConfidence` provenance on facts;
- generated knowledge warns when fallback evidence contributes;
- PokeTrade deliberately reformatted so important route/HTTP facts defeat the old Angular regex-only patterns while the same business knowledge still passes.

Acceptance:

- commit `8f69d6c931e917ce7538b9a991a05211e624cb45`
- GitHub Actions run `34627169975` (#85), both jobs green
- packaged `0.4.3-preview.1` tool verifies project-semantic WorkPlay scanning
- PokeTrade verifies `project-semantic`, `typescript-ast`, semantic ASP.NET Core symbols, explicit template fallback and the existing product-knowledge contract

### V0.4.4 — External real-project trial — NEXT

Run the packaged V0.4.3 tool against one genuine external repository.

Classify findings as:

```text
wrong claim
missing important behavior
noise
unsupported stack/pattern
unexpected fallback
```

Fix only proven gaps and add regression fixtures. Passing WorkPlay/PokeTrade proves benchmark behavior; it is not treated as universal real-world robustness.

## V0.5 — Azure DevOps evidence

Ingest Epic/Feature/PBI/Sprint/history and link product/delivery evidence through PRs/commits where possible.

Do not start this milestone until the external real-project trial has been reviewed.

## V0.6 — Incremental compilation

Use file/symbol hashes and dependency impact to rebuild only affected evidence and knowledge. Add PR/CI knowledge diffs.

## V0.7 — Runtime UI exploration

Use browser automation to confirm actual user-visible flows and states.

## V0.8 — Product insight

Surface gaps, inconsistencies, requirement-vs-implementation drift and improvement opportunities with evidence.
