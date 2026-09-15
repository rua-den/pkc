# External Review — PKC Product Knowledge Compiler

Date captured: 2026-09-11
Status: **Reference only / deferred**

This note records external feedback for later review. It is intentionally not an implementation plan and does not change the current narrow execution scope.

## Strengths to preserve

1. **Clear and disciplined architecture.**
   - Keeping deterministic evidence separate from generated product knowledge is the right foundation.
   - The `evidence before prose` rule and traceability directly reduce hallucination risk.

2. **Real-system validation instead of shallow unit-only validation.**
   - The `poketrade-real-system` CI job builds a real .NET 10 backend and Angular frontend, runs an end-to-end business flow, then runs PKC against the same system and validates generated Markdown.

3. **Reasonable module boundaries.**
   - `Core / CSharp / Frontend / Knowledge / Cli` are separated and independently testable.

## Feedback to consider later

### 1. Documentation drift

Observed concerns:

- milestone/status/readme content can drift from actual implementation state;
- README examples can become stale when generated output structure changes;
- current-state documentation should have one explicit source of truth.

Suggested direction:

- keep `docs/status.md` as the source of truth for current implementation state;
- update `docs/milestones.md` whenever milestone status changes;
- ensure README examples match what CI verifies.

### 2. Distribution / quick trial experience

Current usage still assumes cloning the repository and running the CLI project directly.

Possible future improvements:

- publish PKC as a .NET global tool / NuGet package;
- optionally publish release binaries;
- add a very small quickstart for running PKC against another repository.

### 3. Clarify current scope vs long-term vision

PKC's vision includes more than what is implemented today.

Current implementation is still intentionally narrow compared with the mature target:

- backend support is centered on C#/.NET;
- frontend static adapters currently cover React and Angular;
- Azure DevOps evidence is not implemented yet;
- runtime UI confirmation is not implemented yet;
- optional LLM-backed synthesis is not yet the primary synthesis path.

README should eventually distinguish clearly between:

- **works today**;
- **planned / future**.

### 4. Basic OSS project hygiene

If PKC is intended for broader public use later, consider:

- `LICENSE`;
- `CONTRIBUTING.md`;
- repository description/topics;
- documented contribution expectations.

## Questions worth revisiting later

- When should optional LLM-backed synthesis be introduced relative to Azure DevOps evidence?
- How broad should language/framework support become, versus staying deep on C#/.NET first?
- Should golden-output maintenance be documented so Markdown format changes do not create confusing CI churn?

## Review summary

The project is viewed as a technically solid proof-of-concept, especially because of its evidence-first anti-hallucination architecture and real-system validation. The main concerns are product maturity rather than the core direction: scope is still narrow, documentation can drift, distribution is not yet convenient, and public OSS packaging is incomplete.

## Decision for now

**Do not expand scope based on this review yet.**

Continue the current narrow milestone first. Revisit this feedback after the PokeTrade benchmark and real-project validation expose which items actually matter next.
