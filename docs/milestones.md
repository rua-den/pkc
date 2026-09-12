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

### V0.4.4 — External real-project knowledge trial — CURRENT

The purpose of this milestone is **not** to make every analyzer perfect. It is to prove that PKC's generated `knowledge/` is useful as portable product/system knowledge on a genuine repository.

Current real-project benchmark: `rua-den/loren`.

Two tracks are used:

```text
pinned Loren commit → deterministic blocking acceptance
current Loren main   → moving canary for new real-world patterns
```

The trial has already proven and regression-locked fixes for Minimal API support, source-scope contamination, conditional endpoints, response semantics, multi-project semantic loading, mutation noise, flow noise and authentication side effects.

The remaining acceptance focus is the final product abstraction, not analyzer breadth.

V0.4.4 passes only when:

1. important claims are source-grounded and confidence/provenance is honest;
2. workflow Markdown preserves meaningful operation behavior and failure paths;
3. feature/index Markdown is product-oriented rather than a dump of helper internals;
4. a reviewer using **only `knowledge/`**, with the source repository hidden, can correctly explain the selected system's important product behavior;
5. those knowledge-only answers are checked against source/known behavior;
6. there are no unresolved blocker-class wrong claims, missing important behavior, comprehension-breaking noise, important unexpected fallbacks or required unsupported patterns;
7. every external-repo compiler bug has a regression fixture or acceptance assertion.

Do not broaden this milestone into TypeScript TypeChecker work, React AST, new frameworks, browser automation, incremental compilation or Azure DevOps unless the real-project trial proves that capability is required to pass the knowledge acceptance contract.

## V0.5 — Azure DevOps evidence — LOCKED

Ingest Epic/Feature/PBI/Sprint/history and link product/delivery evidence through PRs/commits where possible.

Do not start V0.5 until V0.4.4 passes the **knowledge-only comprehension gate**, not merely CI/analyzer assertions.

## V0.6 — Incremental compilation

Use file/symbol hashes and dependency impact to rebuild only affected evidence and knowledge. Add PR/CI knowledge diffs.

## V0.7 — Runtime UI exploration

Use browser automation to confirm actual user-visible flows and states.

## V0.8 — Product insight

Surface gaps, inconsistencies, requirement-vs-implementation drift and improvement opportunities with evidence.
