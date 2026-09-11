# PKC Handoff

Use this file when continuing PKC in another chat/session.

## Product idea

PKC means **Product/System Knowledge Compiler**.

The target user is a Product Owner. A PO should be able to take PKC's generated `knowledge/` folder and attach it to any capable AI — ChatGPT, Claude, Gemini, Copilot, etc. — then ask questions about the product without making that AI re-read or grep the source repository.

Example target question:

> How do I change the status of a WorkPlay? What should I be careful about?

The mature knowledge pack should combine:

- how users perform actions in the UI
- business behavior and status transitions
- validations and permissions
- side effects and integrations
- Feature/PBI/Sprint history
- current delivery state
- gaps and improvement opportunities
- evidence supporting important claims

## Intended inputs

1. Backend/source code — implementation behavior.
2. Frontend/UI — user-facing screens, actions, conditions and API flows.
3. Azure DevOps — Epic/Feature/PBI/Sprint/history, intent and delivery traceability.

## Non-negotiable architecture

Do not implement `source -> LLM -> Markdown` directly.

```text
SOURCE
  ↓
DETERMINISTIC ANALYZERS
  ↓
EVIDENCE / FACT MODEL
  ↓
FEATURE / WORKFLOW CANDIDATES
  ↓
KNOWLEDGE SYNTHESIZER
  ↓
CANONICAL KNOWLEDGE MODEL
  ↓
DETERMINISTIC MARKDOWN RENDERER
```

## Current verified state

V0.3 backend + frontend-static knowledge is complete and green.

Commands:

```bash
pkc scan <repository-path>
pkc build <repository-path>
```

Outputs:

```text
.pkc/facts.json
.pkc/feature-candidates.json
knowledge/features/**/*.md
```

Concrete sample:

```text
samples/WorkPlaySample/knowledge/features/workplay/complete.md
```

The sample Markdown now has coverage:

```yaml
coverage:
  - backend-code
  - frontend-static
```

It includes:

- UI route `/workplays/:id`
- `Complete` action on `WorkPlayDetailPage`
- UI permission guard `ManageWorkPlay`
- matching UI API call
- backend endpoint and authorization
- Completed-status guard/exception
- meaningful state mutation
- publication-like side effect
- backend call flow
- exact source evidence from TSX and C#
- explicit Azure DevOps unknown

Frontend evidence is attached to a backend feature only when HTTP method + normalized API route match. This conservative linkage is intentional.

## Verified checkpoint

Implementation: `26a7c381362dd3cf155974fc55883cbc72f8891a`
Golden output: `1ee1d7bc2f7e5aff8f5d808a5d14b891144abee6`
CI run: `34583992309` — success.

Countdown to an AI-testable Markdown with UI instructions is **0**.

## Frontend scope today

`Pkc.Frontend` is a deterministic React/TypeScript static analyzer for common patterns, including function components, React Router routes, buttons/direct handlers, common permission guards, `fetch`, and common HTTP client methods.

It is deliberately not yet a full TypeScript AST implementation and does not represent runtime-confirmed UI behavior.

## Next engineering target

V0.4 Azure DevOps evidence:

- Epic / Feature / PBI
- Sprint / iteration
- current work-item state
- description / acceptance criteria
- selected revisions/history
- PR/commit relationships
- map delivery evidence into the same feature knowledge

Target outcome: a PO can ask both “how does this feature work?” and “why/when was it built, what changed, and how far is delivery?” from the portable Markdown pack.
