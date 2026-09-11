# Vision

PKC is a **Product/System Knowledge Compiler**.

Its goal is to turn implementation and delivery evidence into a portable knowledge pack that a Product Owner can give to any AI assistant.

The expensive work of understanding the system should happen during compilation, not every time somebody asks a product question.

## Product goal

Given:

- backend/source code,
- frontend/UI implementation and eventually runtime UI evidence,
- Azure DevOps Epics, Features, PBIs, Sprints and history,

PKC should generate knowledge that explains:

- what the system is,
- what features exist,
- what each feature does,
- how a user performs a workflow,
- business rules and validations,
- permissions and state transitions,
- side effects and integrations,
- when/why a feature was delivered,
- what changed later,
- known gaps, inconsistencies and possible improvements,
- evidence supporting each important statement.

## Example

A PO should be able to ask:

> How do I change the status of a WorkPlay? What should I be careful about?

The generated knowledge should make it possible for an AI to answer with user steps, valid transitions, validation rules, permissions, side effects, delivery history and relevant caveats without scanning the source repository again.

## Core principles

1. **Portable output** — Markdown/YAML is the product interface. No specific AI vendor is required.
2. **Evidence before prose** — source is first converted into deterministic facts; an LLM must not directly rewrite an entire repository into documentation.
3. **Traceability** — important knowledge must point back to source evidence.
4. **Incremental compilation** — unchanged source should not be re-analyzed unnecessarily.
5. **Current behavior is not automatically business intent** — implementation evidence and product intent must stay distinguishable.
6. **Cheap daily use** — PO questions should use compiled knowledge, not re-scan the repository.

## Long-term compiler shape

```text
Backend / Source ──┐
Frontend / UI ─────┼──> Evidence model ──> Feature synthesis ──> Knowledge model ──> Markdown
Azure DevOps ──────┘
```

V0.1 starts with C# source only and proves the evidence layer first.
