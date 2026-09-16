# Vision

PKC is a **Product/System Knowledge Compiler**.

Its goal is to turn implementation and delivery evidence into a portable knowledge pack that a Product Owner can give to any AI assistant.

The expensive work of understanding the system should happen during compilation, not every time somebody asks a product question.

## Product goal

Long-term inputs:

- backend/source code,
- frontend/UI implementation and eventually runtime UI evidence,
- eventually Azure DevOps Epics, Features, PBIs, Sprints and history,

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

## Success and current boundaries

The intended daily experience is: run `pkc build <repository>`, give the generated knowledge to an AI, and ask a product question. The answer must be useful, accurate within the supported scope, and traceable without rescanning source. Fact counts and new analyzer capabilities are intermediate work, not the measure of product success.

Keep business conditions, value lineage/provenance, and mutation/causality distinct. Missing authority must not delete independently known evidence. Unsupported behavior must remain explicitly unproven; a supported positive example cannot pass acceptance by answering every question with unknown.

Accepted scope through V0.4.6 includes C# and supported static frontend evidence, conservative business rules, and portable workflow/feature knowledge. V0.4.7 is the next implementation milestone: useful explanations of value origin, change, and cross-layer outcomes. It has an acceptance plan, but its generalized lineage behavior is not yet implemented. See [current status](status.md) for the authoritative checkpoint and [milestones](milestones.md) for delivery order.

Azure DevOps history, incremental compilation, runtime UI confirmation, and product insights are future capabilities. Do not describe them as available today. In particular, delivery-history answers in the example below require the future ADO input.

## Example

A PO should be able to ask:

> How do I change the status of a WorkPlay? What should I be careful about?

The generated knowledge should make it possible for an AI to answer with user steps, valid transitions, validation rules, permissions, side effects, delivery history and relevant caveats without scanning the source repository again.

## Core principles

1. **Portable output** — Markdown/YAML is the product interface. No specific AI vendor is required.
2. **Evidence before prose** — source is first converted into deterministic facts; an LLM must not directly rewrite an entire repository into documentation.
3. **Traceability** — important knowledge must point back to source evidence.
4. **Incremental compilation as a future efficiency goal** — unchanged source should not be re-analyzed unnecessarily; implementation is planned for V0.6.
5. **Current behavior is not automatically business intent** — implementation evidence and product intent must stay distinguishable.
6. **Cheap daily use** — PO questions should use compiled knowledge, not re-scan the repository.

## Long-term compiler shape

```text
Backend / Source ──┐
Frontend / UI ─────┼──> Evidence model ──> Feature synthesis ──> Knowledge model ──> Markdown
Azure DevOps ──────┘
```

V0.1 began with C# evidence. Subsequent delivery must keep working toward the portable PO experience above, one usable checkpoint at a time. Each checkpoint needs a question, a generated answer artifact, a correctness gate, and a stop condition; it must not become an open-ended analyzer project.
