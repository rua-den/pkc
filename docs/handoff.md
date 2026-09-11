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
2. Frontend/UI — real user-facing screens, actions, conditions and API flows.
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

## Verified baseline before V0.3

V0.2 is green and already produces backend-only Markdown with exact evidence.

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

## Active slice: V0.3 frontend static evidence

The implementation adds a separate `Pkc.Frontend` analyzer for React/TypeScript-style source and keeps frontend extraction deterministic.

Current target facts:

- `ui-screen`
- `ui-route`
- `ui-action`
- `ui-api-call`

Initial patterns cover:

- React function components
- React Router `<Route ...>`
- `<button>` / `<Button>` actions with direct handlers
- common permission guards: `hasPermission`, `can`, `canAccess`
- `fetch(...)` and common client `.get/.post/.put/.patch/.delete(...)` calls

Frontend evidence is only attached to a backend feature when HTTP method + normalized API route match the backend endpoint. This is intentionally conservative to avoid assigning unrelated UI to a feature.

When matched, feature coverage becomes:

```yaml
coverage:
  - backend-code
  - frontend-static
```

and generated Markdown gains:

- `How to do it in the UI`
- `UI to backend`
- UI permission evidence
- UI source evidence

The old `frontend-ui-not-analyzed` unknown is removed only when a matching frontend flow is found. Azure DevOps remains unknown until the next major input slice.

## Countdown

After V0.3 CI is green, countdown to Markdown with grounded UI instructions is **0**.

Next major engineering target after that is Azure DevOps evidence: Feature/PBI/Sprint/history and delivery traceability.
