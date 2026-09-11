# PKC — Product Knowledge Compiler

PKC compiles implementation evidence from a software system into portable, AI-readable product knowledge.

The target user is a Product Owner who should be able to attach the generated knowledge pack to any capable AI and ask questions such as:

> How do I change the status of a WorkPlay? What should I be careful about?

PKC is intentionally designed as a compiler pipeline, not as a chatbot and not as a repository-wide prompt pack.

```text
Source code
   ↓
Deterministic analyzers
   ↓
Evidence / facts
   ↓
Feature & workflow synthesis
   ↓
Knowledge model
   ↓
Portable Markdown
```

## V0.1

The first milestone is deliberately small:

```bash
pkc scan <repository-path>
```

It scans C# source using Roslyn and writes deterministic implementation facts to:

```text
.pkc/facts.json
```

V0.1 does **not** use an LLM yet. It proves the evidence layer first.

## Planned inputs

- Backend/source code — behavior, validation, state changes, permissions, integrations and side effects.
- Frontend/UI — screens, user actions, routes, conditional UI and API usage.
- Azure DevOps — Epics, Features, PBIs, Sprints, delivery history and intent.

See `docs/vision.md`, `docs/architecture.md` and `docs/milestones.md`.
