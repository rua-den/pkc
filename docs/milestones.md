# Milestones

## V0.1 — C# evidence compiler

Goal: prove that PKC can deterministically extract implementation evidence without an LLM.

Deliverables:

- .NET solution and CLI
- `pkc scan <repository-path>`
- Roslyn-based C# scanner
- `.pkc/facts.json`
- source locations for extracted facts
- sample project and automated tests

Exit criteria:

- scanner runs against a sample C# project
- facts are deterministic
- endpoints, symbols and basic call relations are visible in JSON

## V0.2 — Frontend static evidence

Extract routes, screens/components, visible actions, permission checks and API calls from supported frontend stacks.

## V0.3 — Feature/workflow synthesis

Combine backend and frontend evidence into feature and user-workflow candidates. Introduce LLM synthesis over compact evidence, not raw repositories.

## V0.4 — Azure DevOps evidence

Ingest Epic/Feature/PBI/Sprint/history and link delivery evidence to product features through PRs/commits where possible.

## V0.5 — Incremental compilation

Use file/symbol hashes and dependency impact to rebuild only affected evidence and knowledge.

## V0.6 — Runtime UI exploration

Use browser automation to confirm actual user-visible flows and states.

## V0.7 — Product insight

Surface gaps, inconsistencies, requirement-vs-implementation drift and improvement opportunities with evidence.
