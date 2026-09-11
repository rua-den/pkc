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

V0.1 baseline and **V0.1.1 behavior evidence are complete and green**.

Command:

```bash
dotnet run --project src/Pkc.Cli/Pkc.Cli.csproj -- scan <repository-path>
```

Output:

```text
<repository-path>/.pkc/facts.json
```

Current schema: `0.1.1`.

Current facts include source-grounded:

- symbols / methods / endpoints / properties / enums
- `if` / guard conditions
- thrown exceptions
- assignment/state-mutation candidates
- semantic call targets where Roslyn can resolve them
- combined controller/action routes
- authorization policies/roles
- message/event publication candidates
- exact source line ranges

Verified implementation commit: `59473783fbd03d17ad5510dfa46d4bb77b163e44`
Verified CI run: `34576273132`

## Shortest path to first AI-testable Markdown

**2 engineering steps remain from the V0.1.1 checkpoint:**

1. **V0.1.2 — Feature/Workflow candidate synthesis model** — ACTIVE NEXT
   - group related evidence around one behavior such as WorkPlay status management
   - emit a compact grounded payload for synthesis

2. **V0.2 — Knowledge synthesis + Markdown renderer**
   - synthesize a canonical product-knowledge model from the grounded payload
   - deterministically render `knowledge/features/.../*.md`
   - test it with: “How do I change WorkPlay status? What should I be careful about?”

The first Markdown proof should use backend evidence only. Frontend/UI and Azure DevOps are added after that proof works.

## Future change/update model

Long term PKC should support incremental compilation:

```text
changed source
  ↓
changed hashes/facts
  ↓
affected feature/workflow
  ↓
regenerate only affected Markdown
```

Expected modes later:

- manual full build
- incremental build
- PR/CI diff
- merge-time publication of updated knowledge

## Scope discipline

Do not jump early into:

- GraphRAG
- vector databases
- fine-tuning
- graph databases
- Azure DevOps ingestion
- Playwright/runtime UI exploration

First prove that compact, traceable source evidence can support useful product knowledge generation.
