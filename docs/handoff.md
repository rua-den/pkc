# PKC Handoff

Use this file when continuing PKC in another chat/session.

## Product idea

PKC means **Product/System Knowledge Compiler**.

The target user is a Product Owner. The PO should be able to take PKC's generated knowledge folder and attach it to any capable AI — ChatGPT, Claude, Gemini, Copilot, etc. — then ask questions about the product without making that AI re-read or grep the source repository.

Example target question:

> How do I change the status of a WorkPlay? What should I be careful about?

A mature PKC knowledge pack should answer with user steps, valid transitions, validations, permissions, side effects, caveats, delivery history, gaps/improvements and evidence.

## Inputs

1. Backend/source code — current behavior, validation, state transitions, permissions, side effects, integrations, events, data relationships.
2. Frontend/UI — screens, routes, actions, conditional visibility, forms, API calls and later runtime user flows.
3. Azure DevOps — Epic/Feature/PBI/Sprint, acceptance criteria, status/history, PR/commit links, intent and later changes.

## Architecture rule

Never implement `source -> LLM -> Markdown` directly.

```text
SOURCE
  ↓
DETERMINISTIC ANALYZER
  ↓
EVIDENCE / FACT MODEL
  ↓
FEATURE / WORKFLOW CANDIDATES
  ↓
LLM SYNTHESIS
  ↓
KNOWLEDGE MODEL
  ↓
MARKDOWN RENDERER
```

## Current state

V0.1.1 behavior evidence is complete and green.

V0.1.2 is the active slice. It groups endpoint-centered evidence into:

```text
.pkc/feature-candidates.json
```

The grouping starts at an HTTP endpoint, follows semantic `invokes` relations through related backend methods, and includes attached condition, throw, mutation and publication evidence. This gives the next LLM a compact grounded payload instead of the full repository.

Command remains:

```bash
dotnet run --project src/Pkc.Cli/Pkc.Cli.csproj -- scan <repository-path>
```

Current intended outputs:

```text
<repository-path>/.pkc/facts.json
<repository-path>/.pkc/feature-candidates.json
```

## Countdown to first AI-testable Markdown

Once V0.1.2 is green, only **1 engineering step remains**:

### V0.2 — Knowledge synthesis + Markdown renderer

Take a grounded feature candidate, synthesize the canonical product-knowledge model, then deterministically render a Markdown file under `knowledge/features/...`.

First proof question:

> How do I change WorkPlay status? What should I be careful about?

The first Markdown proof is backend-only. Frontend/UI and Azure DevOps are added after this proof works.

## Future update model

Later PKC should support incremental compilation:

```text
changed source
  ↓
changed hashes/facts
  ↓
affected feature/workflow
  ↓
regenerate only affected Markdown
```

Expected modes later: manual full build, incremental build, PR/CI diff, merge-time knowledge publication.

## Scope discipline

Do not jump early into GraphRAG, vector databases, fine-tuning, graph databases, Azure DevOps ingestion or Playwright runtime exploration before the first Markdown proof works.
