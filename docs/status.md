# PKC Status

Last updated: 2026-09-11

## Current milestone

**V0.1.1 behavior evidence — COMPLETE**

Verified implementation commit: `59473783fbd03d17ad5510dfa46d4bb77b163e44`

GitHub Actions run `34576273132` passed all checks:

- `dotnet build PKC.sln --configuration Release`
- unit tests
- end-to-end `pkc scan samples/WorkPlaySample`
- output schema is `0.1.1`
- sample output contains condition, throw, mutation, full-route, authorization-policy and publication-candidate evidence

## Evidence currently extracted

Structural evidence:

- class / interface / struct / record
- enum and enum members
- methods / constructors / properties
- attributes
- HTTP endpoint method
- exact source file and line ranges

Behavior evidence added in V0.1.1:

- `if` / guard conditions
- thrown exceptions
- assignments and state-mutation candidates
- semantic method-call targets when Roslyn can resolve them
- syntactic call fallback
- combined controller + action route
- authorization policies/roles
- message/event publication candidates

Output remains deterministic:

```text
.pkc/facts.json
```

Schema version: `0.1.1`.

## Path to first AI-testable Markdown

**2 engineering steps remaining.**

1. **V0.1.2 — Feature/Workflow candidate synthesis model**
   - group related evidence around a user-facing behavior such as WorkPlay status management
   - output a compact grounded intermediate payload that does not require the LLM to crawl source

2. **V0.2 — Knowledge synthesis + Markdown renderer**
   - LLM turns grounded feature/workflow evidence into the canonical knowledge model
   - deterministic renderer emits `knowledge/features/.../*.md`
   - validate using: “How do I change WorkPlay status? What should I be careful about?”

Frontend/UI and Azure DevOps intentionally come after the first backend-derived Markdown proof.

## Active next step

**V0.1.2 — Feature/Workflow candidate synthesis model.**
