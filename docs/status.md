# PKC Status

Last updated: 2026-09-11

## Current milestone

**V0.2 first AI-testable Markdown proof — COMPLETE**

Verified implementation/golden commit: `049dccd4374ccf2441e77d016ae3a093a84a3b3a`

GitHub Actions run `34582626084` passed all checks:

- solution build
- all tests
- end-to-end `pkc build samples/WorkPlaySample`
- generated facts
- generated feature candidate
- generated Markdown knowledge
- expected WorkPlay permission/rule/side-effect checks
- committed golden Markdown matches regenerated output exactly

A final quality fix filters endpoint setup noise such as `Id = id` and discard assignments from product-level state changes/evidence while preserving the raw facts in `.pkc/facts.json` for traceability.

## Current commands

Evidence + candidate only:

```bash
pkc scan <repository-path>
```

Full knowledge build:

```bash
pkc build <repository-path>
```

Current outputs:

```text
.pkc/facts.json
.pkc/feature-candidates.json
knowledge/features/**/*.md
```

## First portable knowledge file

```text
samples/WorkPlaySample/knowledge/features/workplay/complete.md
```

It contains:

- backend API entry point
- authorization policy
- observed guard/exception rule
- meaningful state mutation
- publication-like side effect
- backend call flow
- source evidence with line ranges
- explicit unknowns for UI and Azure DevOps

## Countdown to AI test

**0 steps remaining.**

The Markdown file can now be attached directly to another AI and tested with questions such as:

> How do I change WorkPlay status? What should I be careful about?

At this stage the correct answer should be honest about the missing UI path and delivery history, because those inputs have not been compiled yet.

## Architectural note

The first Markdown proof uses a deterministic grounded synthesizer rather than an external LLM. This proves the portable knowledge contract without provider cost or hallucination. `IKnowledgeSynthesizer` is already an abstraction point for later LLM-backed enrichment when business inference requires it.

## Next target

Add frontend static evidence so PKC can answer **how the user actually performs the action**, not only what the backend does.
