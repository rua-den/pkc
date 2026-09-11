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

### V0.4.2 — PokeTrade real-system benchmark — COMPLETE

The `.NET 10 + Angular 22` PokeTrade application is independently runnable and its Order → WorkPlay → Delivery flow is verified in CI. PKC compiles the same source tree and CI checks generated Markdown against the live business behavior.

Benchmark-driven fixes include:

- correct guard → throw pairing
- correct compound mutation semantics
- removal of PO-facing object-initializer/internal-counter noise
- conservative publication-side-effect classification
- Angular object-shaped service method linking
- correct Angular redirect/component route extraction
- lifecycle-vs-discovery grouping without substring collisions
- state-mutation fallback when semantic binding is incomplete

Final verification: commit `659384ec4ba9e6e6bfbe5b381e9ac5ffd176752d`, CI run `34604840944` — SUCCESS.

### V0.4.3 — External real-project trial — CURRENT

Run the packaged PKC tool against one genuine external repository before widening the product surface.

Review only the generated evidence/knowledge:

```text
.pkc/product-features.json
knowledge/index.md
knowledge/features/
knowledge/workflows/
```

Classify real-project findings as wrong claim, missing important behavior, noise, or unsupported stack/pattern. Make only compiler fixes justified by those findings.

## V0.5 — Azure DevOps evidence

Ingest Epic/Feature/PBI/Sprint/history and link product/delivery evidence through PRs/commits where possible.

Do not start this milestone until V0.4.3 external real-project trial is reviewed.

## V0.6 — Incremental compilation

Use file/symbol hashes and dependency impact to rebuild only affected evidence and knowledge. Add PR/CI knowledge diffs.

## V0.7 — Runtime UI exploration

Use browser automation to confirm actual user-visible flows and states.

## V0.8 — Product insight

Surface gaps, inconsistencies, requirement-vs-implementation drift and improvement opportunities with evidence.
