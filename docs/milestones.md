# Milestones

## V0.1 — C# evidence compiler — COMPLETE

Deterministic Roslyn scanner producing `.pkc/facts.json` with symbols, endpoints, routes, permissions, semantic calls and source locations.

### V0.1.1 — Behavior evidence — COMPLETE

Added conditions/guards, throws, state-mutation candidates, semantic call targets, combined routes and publication candidates.

### V0.1.2 — Feature candidate grouping — COMPLETE

Endpoint-centered traversal groups compact backend evidence into `.pkc/feature-candidates.json`.

## V0.2 — First portable Markdown proof — COMPLETE

`pkc build <repository-path>` turns grounded candidates into a canonical knowledge model and renders `knowledge/features/**/*.md`.

The first sample is `WorkPlay Complete`. It is suitable for attaching to another AI today.

## V0.3 — Frontend static evidence — NEXT

Extract routes, screens/components, visible actions, permission checks and API calls. Link frontend interactions to backend features.

## V0.4 — Cross-stack feature/workflow synthesis

Combine backend + frontend evidence into stronger user-facing feature/workflow knowledge. Add optional LLM-backed synthesis over compact evidence where deterministic logic is insufficient.

## V0.5 — Azure DevOps evidence

Ingest Epic/Feature/PBI/Sprint/history and link product/delivery evidence through PRs/commits where possible.

## V0.6 — Incremental compilation

Use file/symbol hashes and dependency impact to rebuild only affected evidence and knowledge. Add PR/CI knowledge diffs.

## V0.7 — Runtime UI exploration

Use browser automation to confirm actual user-visible flows and states.

## V0.8 — Product insight

Surface gaps, inconsistencies, requirement-vs-implementation drift and improvement opportunities with evidence.
