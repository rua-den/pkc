# PKC Handoff

Use this file when continuing PKC in another chat/session or external review.

## Product idea — do not drift from this

PKC means **Product/System Knowledge Compiler**.

The target user is a Product Owner. A PO should be able to take PKC's generated portable knowledge and give it to any capable AI — ChatGPT, Claude, Gemini, Copilot, etc. — then ask questions about the product **without making that AI re-read or grep the source repository**.

The analyzer/fact graph exists to make that knowledge trustworthy. Analyzer sophistication is not the final product.

The final acceptance question is:

> If the source repository is hidden and an AI receives only the generated portable knowledge, can it explain the product accurately, at the right abstraction level, while preserving important unknowns and evidence boundaries?

## Non-negotiable compiler architecture

Do not implement `source -> LLM -> Markdown` directly.

```text
SOURCE
  ↓
DETERMINISTIC ANALYZERS / ADAPTERS
  ↓
EVIDENCE / FACT MODEL
  ↓
FEATURE / WORKFLOW CANDIDATES
  ↓
KNOWLEDGE SYNTHESIS
  ↓
CANONICAL KNOWLEDGE MODEL
  ↓
DETERMINISTIC PORTABLE RENDERING
  ↓
AI-CONSUMABLE PRODUCT KNOWLEDGE
```

## Knowledge hierarchy — also non-negotiable

```text
AI_INSTRUCTIONS = tell the receiving AI how to consume the pack
index            = orient the AI
feature          = explain a capability
workflow         = explain an operation
raw evidence     = prove implementation detail
```

Do not delete raw evidence to make output cleaner. Do not promote every helper guard/loop/call into product-level knowledge either.

A detail belongs at product level only when it materially changes externally meaningful behavior, constraints, outcomes or safety.

The primary consumer is an AI, so optimize for reliable retrieval/reasoning and traceability rather than Markdown aesthetics alone.

## Portable AI handoff contract

Current development output from `pkc build` includes:

```text
knowledge/
  AI_INSTRUCTIONS.md
  index.md
  features/**/*.md
  workflows/**/*.md

PKC_KNOWLEDGE.md
PKC_KNOWLEDGE.zip
```

Roles:

```text
knowledge/
→ canonical structured portable knowledge

PKC_KNOWLEDGE.md
→ single-file convenience bundle for the simplest PO → AI handoff

PKC_KNOWLEDGE.zip
→ archive/storage/share transport of knowledge/ only
```

PO-facing default for small/medium packs should be: upload `PKC_KNOWLEDGE.md` and ask the question.

If the AI/workspace supports multiple-file knowledge/indexing, provide the `knowledge/` files and let the AI start from `AI_INSTRUCTIONS.md` then `index.md`.

Do not require ZIP extraction or ZIP parsing as the primary interface. Archive support differs by destination. The ZIP must not contain source code or raw `.pkc` evidence by default.

Detailed contract: `docs/ai-handoff.md`.

Live MCP/connector/workspace synchronization is a future delivery adapter, not a V0.4 requirement. Prove the portable knowledge first.

## Current milestone

**V0.4.4 Loren Knowledge Readiness — IN PROGRESS.**

V0.4.3 remains the last accepted packaged checkpoint:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

Current `main` contains V0.4.4 trial and portable-handoff development. Do not bump the accepted package version merely because current CI is green.

Primary execution plan:

```text
docs/real-project-trial.md
```

## Benchmark roles

```text
PokeTrade
→ known-answer runnable regression

Loren pinned commit
→ deterministic blocking real-project acceptance for V0.4.4

Loren main
→ moving non-blocking canary
→ catches new source patterns as Loren evolves

second independent real repo
→ V0.4.5 anti-overfit/generalization gate
```

## Proven Loren fixes so far

The real-project trial has already forced generic fixes for:

- Minimal API discovery + target-project semantic enrichment;
- exclusion of `tests/` and `spikes/` from product evidence;
- conditional/development-only endpoint availability;
- Minimal API failure/direct response semantics;
- multi-project MSBuild semantic loading;
- dictionary/object-initializer mutation noise;
- framework/primitive call-flow presentation noise;
- ASP.NET sign-in/sign-out product side effects;
- Minimal API response metadata collision for endpoints declared inside methods/extensions.

Pinned Loren also requires zero `loose-roslyn-fallback` facts for the selected production benchmark.

## Current knowledge-readiness work

The main remaining blocker is **knowledge abstraction/comprehension**, not parser breadth.

The latest layered-output change keeps full workflow/evidence detail while reducing helper-level rules promoted into product features. On the first Loren artifact after this change, `Run Operations` dropped from roughly 66 product rules to 10 while each Run workflow retained its full detailed rule set.

The next focus is index/system orientation and then the blind handoff/comprehension gate.

## Exact next steps

Do these in order. Do not skip ahead because another roadmap item looks attractive.

### Step 1 — Layered knowledge abstraction

Ensure synthesis/presentation obeys:

```text
index → feature → workflow → evidence
```

Preserve all proof while promoting only product-impacting behavior upward.

### Step 2 — Portable handoff integrity

Verify:

- `AI_INSTRUCTIONS.md` is generic and vendor-neutral;
- `PKC_KNOWLEDGE.md` embeds the complete canonical pack with explicit file boundaries;
- ZIP contains only canonical `knowledge/` files;
- bundle and structured pack preserve the same critical meaning.

### Step 3 — Loren feature + index signal

- keep product-impacting auth, input validation, project/context behavior, agent-loop outcomes, action limits/proposals, failures and dev-only availability;
- keep helper string/collection mechanics below feature level unless they affect product behavior;
- dedupe shared rules without hiding production/dev differences;
- make `knowledge/index.md` orient the AI using grounded capability/entry/permission/conditional evidence without inventing a user journey.

### Step 4 — Blind Loren comprehension

Hide Loren source and `.pkc` raw files.

First give the reviewer only `PKC_KNOWLEDGE.md` and record answers to the fixed questions in `docs/real-project-trial.md`.

Then provide the structured `knowledge/` pack and check whether deeper navigation changes any critical answer. Any semantic mismatch between handoff forms is a blocker.

Only after answers are recorded should the Loren source be reopened.

### Step 5 — Compare and fix

Classify mismatches as:

```text
wrong claim
missing important behavior
comprehension-breaking noise
unsupported required pattern
unexpected fallback
traceability gap
uncertainty/authority overclaim
handoff/packaging mismatch
```

Fix only proven generic gaps and regression-lock every compiler bug.

### Step 6 — External review V0.4.4

A separate reviewer checks the compiler + artifact + blind answers for blockers and benchmark gaming.

V0.4.4 does not close until blockers are fixed.

### Step 7 — V0.4.5 second real repo

After Loren passes, run the same layered-output, handoff-parity and blind-comprehension process on a second genuine repository that differs materially from PokeTrade/Loren and was not chosen to fit current heuristics.

This is required before V0.5.

## V0.5 unlock gate

**V0.5 Azure DevOps is locked.**

Open it only after all of these are PASS:

```text
PokeTrade known-answer regression                  PASS
Loren evidence/workflow correctness                PASS
Loren blind knowledge-only comprehension           PASS
Loren handoff parity                               PASS
Loren external review                              PASS
second independent real-repo trial                 PASS
second blind knowledge-only comprehension          PASS
second handoff parity                              PASS
cross-benchmark regression after all fixes         PASS
known boundaries/unknowns documented honestly      PASS
no repository-specific compiler exceptions         PASS
```

If any item is not PASS, remain in V0.4.x.

## Scope rule — very important

A new analyzer or delivery capability is allowed into V0.4.x only when a real knowledge review proves it is needed to improve one of:

```text
accuracy
completeness
signal-to-noise
traceability
honest uncertainty
portable handoff reliability
```

Do not start TypeScript TypeChecker work, React AST, MVC/Razor/Blazor/Vue expansion, browser exploration, incremental compilation, Azure DevOps or live MCP/connector delivery merely because those items are unfinished.
