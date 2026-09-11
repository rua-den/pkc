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

## Current verified state

The first backend-only Markdown proof is complete and green.

Verified product-knowledge checkpoint:

- implementation/golden commit: `049dccd4374ccf2441e77d016ae3a093a84a3b3a`
- GitHub Actions run: `34582626084`
- result: build, tests, E2E knowledge generation and golden Markdown diff all pass

Commands:

```bash
pkc scan <repository-path>
pkc build <repository-path>
```

`scan` emits:

```text
.pkc/facts.json
.pkc/feature-candidates.json
```

`build` additionally emits:

```text
knowledge/features/**/*.md
```

Concrete sample:

```text
samples/WorkPlaySample/knowledge/features/workplay/complete.md
```

The sample Markdown is `authority: code-observed` and includes the WorkPlay Complete endpoint, `ManageWorkPlay` permission, the Completed guard/exception, Status mutation, publication-like side effect, backend flow and exact source evidence.

Endpoint setup noise such as `Id = id` and discard assignments are deliberately excluded from product-level knowledge while raw facts remain preserved for traceability.

It explicitly says that frontend/UI and Azure DevOps have not yet been analyzed.

## Important implementation detail

The first knowledge synthesizer is deterministic and grounded. This was intentional: prove the portable Markdown contract before spending tokens or coupling PKC to an LLM vendor.

`IKnowledgeSynthesizer` exists so a later model-backed synthesizer can infer richer business meaning from compact evidence rather than raw source.

## AI-test checkpoint

Countdown is now **0**. Attach the sample Markdown to an AI and ask:

> How do I change WorkPlay status? What should I be careful about?

Expected behavior today: the AI should explain the backend Complete action and caveats, while admitting that the UI path and delivery history are unknown.

## Next engineering target

Frontend static evidence.

Extract supported frontend routes, pages/components, actions/buttons, permission conditions and API calls. Then connect those facts to backend feature candidates so the knowledge can answer the user-facing “how do I do it?” part.

Azure DevOps comes after the UI proof, then incremental compilation/change impact.
