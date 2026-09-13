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

### V0.4.5 — Independent real-repository generalization gate — CURRENT

Purpose: prove PKC did not simply overfit PokeTrade + Loren before adding another major evidence source.

Select a second genuine repository that:

- was not authored/modified for PKC;
- fits the currently supported C# surface;
- has non-trivial product/system behavior;
- differs materially from PokeTrade/Loren;
- is not chosen merely because current heuristics handle it easily.

Run the same layered-output and blind knowledge-only comprehension process.

V0.4.5 passes only when:

- critical product questions have no blocker false claims;
- important behavior is answerable or explicitly unknown;
- product-level output is high-signal;
- evidence remains traceable;
- fixes remain generic and do not contain repository-specific exceptions;
- PokeTrade + Loren still pass after any new fixes;
- independent external review has no unresolved blocker.

Detailed procedure: `docs/real-project-trial.md`.

## V0.5 — Azure DevOps evidence — LOCKED

Ingest Epic/Feature/PBI/Sprint/history and link product/delivery evidence through PRs/commits where possible.

V0.5 may start only after **both V0.4.4 and V0.4.5 pass**.

The unlock decision is based on knowledge readiness, not merely analyzer/CI success:

```text
PokeTrade known-answer regression                 PASS
Loren blind knowledge-only comprehension          PASS
Loren external review                             PASS
second independent real-repo comprehension        PASS
cross-benchmark regression                        PASS
known boundaries documented honestly              PASS
```

If any gate is not PASS, remain in V0.4.x.

## V0.6 — Incremental compilation

Use file/symbol hashes and dependency impact to rebuild only affected evidence and knowledge. Add PR/CI knowledge diffs.

## V0.7 — Runtime UI exploration

Use browser automation to confirm actual user-visible flows and states.

## V0.8 — Product insight

Surface gaps, inconsistencies, requirement-vs-implementation drift and improvement opportunities with evidence.
