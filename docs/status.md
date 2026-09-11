# PKC Status

Last updated: 2026-09-11

## Current milestone

**V0.1.2 feature/workflow candidate synthesis — IN PROGRESS**

V0.1.1 behavior evidence is complete and green.

Candidate synthesis adds a deterministic grouping layer between raw facts and future LLM synthesis.

Current pipeline:

```text
C# source
  ↓
Roslyn evidence
  ↓
.pkc/facts.json
  ↓
endpoint-centered evidence grouping
  ↓
.pkc/feature-candidates.json
```

Each feature candidate starts from one HTTP endpoint and follows semantic `invokes` relations through related methods. It includes attached condition, throw, mutation and message-publication evidence.

## Path to first AI-testable Markdown

After V0.1.2 is green, **1 engineering step remains**:

**V0.2 — Knowledge synthesis + Markdown renderer**

- synthesize canonical product knowledge from one grounded feature candidate
- render `knowledge/features/.../*.md`
- validate by attaching the Markdown to an AI and asking:
  - “How do I change WorkPlay status?”
  - “What should I be careful about?”

Frontend/UI and Azure DevOps stay out of the first proof. They will enrich the knowledge after backend-derived Markdown works.
