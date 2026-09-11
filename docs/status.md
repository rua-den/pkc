# PKC Status

Last updated: 2026-09-11

## Current milestone

**V0.1.1 behavior evidence — IN PROGRESS**

Implementation commit prepared: `a43c59c12330a76944272b6f011ab182813c85a9`

This slice enriches the deterministic C# evidence layer before any LLM or Markdown generation.

## Target evidence in V0.1.1

- `if` / guard conditions
- thrown exceptions
- assignments and state-mutation candidates
- semantic method-call targets when Roslyn can resolve them
- combined controller + action routes
- authorization policy/role metadata
- message/event publication candidates
- exact source file + line ranges for all evidence

Output remains:

```text
.pkc/facts.json
```

Schema version target: `0.1.1`.

## Path to first AI-testable Markdown

After V0.1.1 is green, the shortest path to the first useful Markdown test has **2 engineering steps remaining**:

1. **V0.1.2 — Feature/Workflow candidate synthesis model**
   - deterministically group related evidence around a user-facing behavior (for example WorkPlay status change)
   - produce a compact intermediate feature/workflow payload for an LLM

2. **V0.2 — Knowledge synthesis + Markdown renderer**
   - LLM turns grounded feature/workflow evidence into the canonical knowledge model
   - deterministic renderer emits `knowledge/features/.../*.md`
   - validate with the target PO question: “How do I change WorkPlay status? What should I be careful about?”

Frontend/UI and Azure DevOps are intentionally later; the first Markdown test should prove backend-derived product knowledge first.
