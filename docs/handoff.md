# PKC Handoff

Use this file when continuing PKC in another chat/session.

## Product idea

PKC means **Product/System Knowledge Compiler**.

The target user is a Product Owner. The PO should be able to take PKC's generated knowledge folder and attach it to any capable AI — ChatGPT, Claude, Gemini, Copilot, etc. — then ask questions about the product without making that AI re-read or grep the source repository.

Example target question:

> How do I change the status of a WorkPlay? What should I be careful about?

A mature PKC knowledge pack should let the AI answer with:

- where/how the user performs the action in the UI
- valid status transitions
- validations and permissions
- side effects and integrations
- important caveats
- relevant Feature/PBI/Sprint history
- current gaps or improvement opportunities
- evidence supporting important claims

## Intended inputs

### 1. Backend/source code

Used to establish current implementation behavior: validation, state transitions, permissions, side effects, integrations, events, data relationships, etc.

### 2. Frontend/UI

Used to establish how users actually interact with the feature: screens, routes, buttons/actions, conditions, permission-driven visibility, forms, API calls and eventually runtime user flows.

### 3. Azure DevOps

Used to establish product/delivery context: Epic, Feature, PBI, Sprint, acceptance criteria, status/history, PR/commit links, original intent and later changes.

## Desired output

Portable Markdown/YAML product knowledge. Markdown is the interoperability layer so a PO is not locked to one AI vendor or one PKC runtime.

Conceptually:

```text
Backend ───────┐
Frontend/UI ───┼──> evidence/facts ──> feature/workflow synthesis ──> knowledge model ──> Markdown
Azure DevOps ──┘
```

The expensive reverse-engineering step happens during compilation. Day-to-day PO questions should consume the compiled knowledge rather than scan source again.

## Critical architecture rule

**Do not implement `source code -> LLM -> Markdown`.**

PKC must preserve an intermediate evidence layer:

```text
SOURCE
  ↓
DETERMINISTIC ANALYZER
  ↓
EVIDENCE / FACT MODEL
  ↓
FEATURE & WORKFLOW DISCOVERY
  ↓
LLM SYNTHESIS
  ↓
KNOWLEDGE MODEL
  ↓
MARKDOWN RENDERER
```

The LLM belongs between grounded facts and product knowledge. It should not be responsible for repository crawling, source citations, incremental builds or Markdown file mechanics.

## Current implementation

V0.1 baseline is complete and green.

Command:

```bash
dotnet run --project src/Pkc.Cli/Pkc.Cli.csproj -- scan <repository-path>
```

Output:

```text
<repository-path>/.pkc/facts.json
```

See `docs/status.md` for exact current capabilities and verified CI state.

## Next step

Stay in the C# evidence layer for the next slice. Add deterministic behavior evidence before adding an LLM:

- guards/conditions
- exceptions
- state mutations
- semantic call targets
- routes/permissions
- events/integration calls

Only once these facts are useful should PKC start synthesizing Feature/Workflow knowledge.

## Scope discipline

Do not jump early into:

- GraphRAG
- vector databases
- fine-tuning
- graph databases
- Azure DevOps ingestion
- Playwright/runtime UI exploration

Those are later stages. First prove that compact, traceable source evidence can support useful product knowledge generation.
