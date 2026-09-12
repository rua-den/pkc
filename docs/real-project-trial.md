# V0.4 Knowledge Readiness Exit Plan

This document is the execution plan for finishing the V0.4 line safely before V0.5 Azure DevOps work begins.

## North star

PKC is a **Product/System Knowledge Compiler**.

The target product is the portable `knowledge/` pack, not the analyzer by itself. A Product Owner should be able to attach that pack to a capable AI and ask product/system questions without requiring the AI to re-read or grep the source repository.

The non-negotiable pipeline remains:

```text
SOURCE
  ↓
DETERMINISTIC ANALYZERS / ADAPTERS
  ↓
EVIDENCE / FACT MODEL
  ↓
FEATURE / WORKFLOW DISCOVERY
  ↓
KNOWLEDGE SYNTHESIS
  ↓
CANONICAL KNOWLEDGE MODEL
  ↓
PORTABLE MARKDOWN
  ↓
AI CAN EXPLAIN THE PRODUCT AT THE RIGHT ABSTRACTION LEVEL
```

Analyzer fidelity matters only because inaccurate evidence produces inaccurate knowledge. Analyzer breadth is **not** the V0.4 exit goal.

## Knowledge pack contract

PKC must preserve detail without forcing every consumer to read implementation internals first.

The intended hierarchy is:

```text
knowledge/AI_INSTRUCTIONS.md
  → vendor-neutral instructions for how an AI should consume the pack

knowledge/index.md
  → orientation, observed system surface, capability map, boundaries/unknowns

knowledge/features/**/*.md
  → product/system capabilities, shared rules, permissions, outcomes, important failures

knowledge/workflows/**/*.md
  → one operation in enough depth to answer how it behaves, including validations,
    state changes, side effects, integrations, application-oriented flow and evidence

.pkc/facts.json + workflow evidence sections
  → detailed implementation traceability and analyzer provenance
```

Portable handoff artifacts:

```text
PKC_KNOWLEDGE.md   → single-file convenience bundle for AI upload
PKC_KNOWLEDGE.zip  → archive transport of the canonical knowledge/ pack
```

Rules for abstraction:

1. **Do not delete evidence to make Markdown pretty.** Move detail to the correct layer.
2. **Do not promote every transitive helper guard/loop into a product rule.** A helper detail belongs at feature level only when it materially changes externally meaningful behavior, constraints, outcomes or safety.
3. **Feature pages summarize capabilities; workflow pages explain operations; evidence preserves proof.**
4. **Observed implementation is not business intent.** Keep `code-observed`, unknowns and future delivery/product evidence distinct.
5. **The primary consumer is an AI.** Markdown should be structured for reliable retrieval and reasoning, not optimized only for human prose aesthetics.
6. **Transport must not change meaning.** The structured folder and single-file bundle must preserve the same authority, unknowns and product behavior.
7. **UI behavior is product knowledge.** When the source statically expresses validation/configuration behavior, route/button/API extraction alone is not sufficient.

Detailed AI handoff contract: `docs/ai-handoff.md`.
Detailed UI behavior contract: `docs/ui-behavior-contract.md`.

## Benchmark roles

PKC uses different benchmarks for different purposes.

### PokeTrade — known-answer behavioral regression

PokeTrade remains the controlled runnable system where expected behavior is known in advance. It protects previously proven compiler behavior and prevents regressions.

It does **not** prove real-world generalization by itself.

### Loren pinned commit — V0.4.4 blocking real-project benchmark

A pinned Loren commit is the deterministic real-repository acceptance target.

It must not be modified to suit PKC.

### Loren main — moving canary

Current Loren `main` is a non-blocking canary. It exposes newly introduced source patterns as Loren evolves, but it never silently replaces the pinned acceptance SHA.

### Independent second real repository — V0.4.5 anti-overfit benchmark

After Loren passes, PKC must be tried against a second genuine repository that was not created for PKC and is meaningfully different from PokeTrade/Loren.

This exists specifically to catch overfitting before V0.5.

## Finding taxonomy

Every trial finding receives one primary category:

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

Priority order:

```text
wrong claim
  > missing important behavior
  > authority/confidence overclaim
  > traceability gap
  > handoff/packaging mismatch
  > comprehension-breaking noise
  > unsupported pattern with low product impact
```

A fix must be generic compiler behavior, never a repository-name-specific exception.

Every proven compiler bug gets a regression fixture or acceptance assertion.

---

# V0.4.4 — Loren Knowledge Readiness

## Current status

**IN PROGRESS.**

The Loren trial has already exposed and regression-locked fixes for:

- Minimal API discovery and semantic enrichment;
- source contamination from `tests/` and `spikes/`;
- conditional/development-only endpoint availability;
- Minimal API failure and direct response semantics;
- multi-project MSBuild semantic loading;
- initializer assignments falsely promoted to domain mutations;
- framework/primitive call-flow noise;
- ASP.NET sign-in/sign-out side effects;
- response metadata collision for endpoints declared inside extension methods;
- explicit zero-fallback acceptance for the pinned production benchmark.

Current artifact review shows the main remaining risks are **knowledge abstraction/comprehension and UI behavior completeness**. The compiler must not only produce accurate backend/workflow facts; it must preserve product-relevant configuration/validation behavior when that behavior is present in supported UI source.

## Step 1 — Freeze the output hierarchy

Before adding analyzer capability, make the knowledge layers obey the contract above.

Required result:

```text
index        = orient the AI
feature      = explain capability
workflow     = explain operation
raw evidence = prove detail
```

Acceptance:

- raw facts remain available;
- workflow traceability does not regress;
- product pages no longer behave like transitive call-graph dumps;
- production vs conditional/dev behavior remains distinguishable.

## Step 2 — Product-feature signal hardening

Use Loren output to fix only proven abstraction problems.

For `Run Operations`, feature-level knowledge should retain product-impacting behavior such as:

```text
requires authenticated owner
rejects empty requests
resolves/validates project context
uses project + memory context
runs the agent/brain loop
stops on final output
limits actions
can collect/propose actions
maps important failures
has a conditional development-only entry point
```

Implementation details such as string truncation mechanics, character normalization and low-level collection loops may remain in workflow/evidence when useful, but must not dominate the feature summary.

Acceptance:

- common product rules across equivalent production/dev workflows are not duplicated needlessly;
- dev-only differences are preserved;
- no meaningful validation, permission, state transition, side effect or externally relevant failure disappears.

## Step 3 — Index/system orientation

`knowledge/index.md` must do more than list files.

Without inventing product intent, it should let an AI identify the observed capability surface and navigate relationships between important areas.

For Loren, a knowledge-only reader should be able to orient around concepts such as:

```text
owner access/authentication
projects/context
run/agent execution
action proposals and approval/cancel
health/basic surface
conditional development behavior
```

Do not invent a journey that evidence does not support. The goal is orientation, not speculative product design.

## Step 3.5 — Portable AI handoff parity

The acceptance artifact must match how a Product Owner will actually give PKC knowledge to an AI.

Generate and validate:

```text
knowledge/AI_INSTRUCTIONS.md
knowledge/...
PKC_KNOWLEDGE.md
PKC_KNOWLEDGE.zip
```

Rules:

- `knowledge/` remains canonical;
- `PKC_KNOWLEDGE.md` must embed every canonical knowledge file with explicit file boundaries;
- the bundle must place AI instructions/index before feature/workflow detail;
- the single-file bundle must preserve the same authority, unknowns and important behavior as the structured pack;
- `PKC_KNOWLEDGE.zip` must contain only portable knowledge by default, not source code or `.pkc/facts.json`;
- ZIP parsing is not required for an AI consumer; it is a transport/storage convenience.

Acceptance:

- the same fixed product questions can be answered from `PKC_KNOWLEDGE.md` and from the structured `knowledge/` pack without semantic disagreement;
- a packaging difference that changes or hides a critical answer is a blocker.

## Step 3.75 — UI validation and configuration behavior

This is a required code-derived knowledge gate before V0.5.

PKC must prove, on a supported Angular/form benchmark, that portable knowledge preserves important statically observable UI behavior instead of stopping at navigation/action/API structure.

At minimum the benchmark must contain:

```text
a selectable type/option
an always-required field
a conditionally-required field
a conditionally-visible or enabled field
field → request/API mapping
backend validation for at least one corresponding value
```

Target questions must be answerable from portable knowledge without source access:

```text
What fields/options exist?
Which fields are required?
Which requirement is conditional, and on what condition?
Which field is shown/hidden or enabled/disabled conditionally?
Where is the important field sent in the request/API?
Does backend validation agree with the UI requirement when both are observed?
```

Example acceptance question:

> For a CSP service, is Microsoft Subscription Id required on the UI, under what condition, where is it sent, and does backend validation agree?

Do not hardcode CSP or repository-specific names. Implement canonical evidence such as `ui-field`, `ui-field-option`, `ui-field-validation`, `ui-field-visibility`, `ui-field-enabled-state` and `ui-field-binding` as needed.

A custom validator whose meaning cannot be proven should remain an explicit validator reference/unknown rather than being paraphrased into invented business semantics.

## Step 4 — Blind knowledge-only comprehension review

This is the primary V0.4.4 acceptance gate.

Procedure:

1. Generate a fresh pinned Loren artifact and the UI-behavior regression artifact.
2. Hide source repositories and `.pkc` raw files from the reviewer for the first pass.
3. First give the reviewer only `PKC_KNOWLEDGE.md` to exercise the simplest PO handoff path.
4. Ask the fixed benchmark questions below and record the answers.
5. Give the reviewer the structured `knowledge/` pack and repeat/check any answer that requires deeper navigation.
6. Record any semantic disagreement between the bundle and structured pack as a packaging blocker.
7. Reopen source/known behavior only after the knowledge-only answers are recorded.
8. Compare every critical answer against source/known behavior.
9. Classify every mismatch using the finding taxonomy.

Required Loren questions:

```text
1. What observable product/system capabilities does Loren expose?
2. How does owner authentication work, including important failure/success outcomes?
3. What does the main run operation do at a product/system level?
4. How are project context and memory involved in a run?
5. How are projects listed and bootstrapped, and what can fail?
6. What are action proposals and what happens when they are approved or cancelled?
7. Which behavior is conditional or development-only?
8. What important permissions, validations and failure paths exist?
9. What important side effects or integrations are visible?
10. What does PKC explicitly not know yet because that evidence source has not been compiled?
```

Required UI/form questions are the ones defined in Step 3.75 and `docs/ui-behavior-contract.md`.

Pass/fail rubric:

### Accuracy

- zero blocker-class false statements in answers to the fixed questions;
- no implementation observation presented as approved business intent.

### Coverage

- every critical question is answerable from the portable knowledge artifact or explicitly answered as unknown;
- an important known behavior may not disappear merely because it was filtered as noise;
- statically observable UI validation/configuration behavior required by the UI contract may not disappear merely because route/action/API linkage is already present.

### Abstraction

- product-level answers can be produced without relying on helper-level string/collection mechanics;
- feature pages expose capability-level rules before implementation details.

### Traceability

- after the blind pass, important answers can be traced through workflow evidence to source locations;
- reducing product noise must not destroy proof.

### Honest uncertainty

- missing frontend/runtime/delivery/product-intent evidence remains explicit;
- the pack prefers unknown over invention.

### Handoff parity

- `PKC_KNOWLEDGE.md` and the canonical `knowledge/` pack must not disagree on a critical answer;
- archive transport must preserve the canonical files intact.

A green CI/grep suite is necessary but **cannot pass this gate by itself**.

## Step 5 — External review of V0.4.4

After the blind review passes internally, hand the pinned commit + generated artifact + this acceptance plan to an independent review thread.

The reviewer should attempt to find:

```text
wrong claims
missing product behavior
missing UI validation/configuration behavior
bad abstraction
lost traceability
authority/confidence overclaim
handoff mismatch
benchmark gaming / repository-specific hardcoding
```

V0.4.4 closes only after blocker findings are fixed and regression-locked.

---

# V0.4.5 — Independent Real-Repository Generalization Gate

V0.4.5 exists because one real repository is not enough evidence to unlock a new evidence source such as Azure DevOps.

## Repository selection

Choose one genuine repository that:

- was not authored or modified for PKC;
- fits at least the currently supported C# backend surface;
- contains non-trivial product/system behavior;
- differs materially from PokeTrade and Loren in code organization/patterns;
- preferably includes a supported Angular or React frontend if a suitable repo is available, especially if it can exercise real form/configuration behavior without forcing unrelated framework expansion.

Do not pick a repository because it is easy for the current heuristics.

Pin the reviewed commit.

## Trial procedure

1. Build/run the target normally enough to establish that the pinned source is valid.
2. Run PKC without changing the target repo to help the compiler.
3. Review analyzer fallback/provenance only to detect knowledge risk.
4. Review `knowledge/` using the same layered-output contract.
5. Validate single-file vs structured-pack handoff parity.
6. Perform a blind knowledge-only comprehension review with questions adapted to that product.
7. If the repository has a supported UI form/configuration surface, include the UI behavior questions from `docs/ui-behavior-contract.md`.
8. Reopen source and compare answers.
9. Fix only proven generic gaps and add regression coverage.
10. Re-run PokeTrade + Loren + the independent repo after each blocker fix.

## V0.4.5 pass condition

- no blocker wrong claims in critical product questions;
- important behavior is answerable or explicitly unknown;
- product-level pages are high-signal;
- evidence remains traceable;
- portable handoff forms preserve the same critical knowledge;
- supported UI validation/configuration behavior is preserved when present;
- no benchmark-specific hardcoding;
- existing PokeTrade and Loren acceptance remain green;
- independent external review finds no unresolved blocker.

---

# V0.5 Unlock Gate

**V0.5 Azure DevOps remains locked until every item below is true.**

```text
PokeTrade known-answer regression                   PASS
Loren pinned evidence/workflow correctness          PASS
Loren blind knowledge-only comprehension            PASS
Loren single-file/structured handoff parity          PASS
UI validation/behavior knowledge benchmark          PASS
Loren external review with no blocker                PASS
Second independent real-repo trial                  PASS
Second blind knowledge-only comprehension           PASS
Second handoff parity                                PASS
Cross-benchmark regression after fixes              PASS
Known boundaries/unknowns documented honestly       PASS
No repository-specific compiler exceptions          PASS
```

Only then is the code-derived knowledge foundation stable enough to add a second major evidence source (Azure DevOps intent/history).

If any item is not PASS, stay in V0.4.x.

## Explicit non-goals before the unlock gate

Do not start these merely because they are on the roadmap:

- Angular TypeScript `TypeChecker` migration unless a proven validation/binding gap requires it;
- React AST rewrite unless a real benchmark requires it;
- MVC/Razor/Blazor/Vue expansion;
- runtime browser exploration;
- incremental compilation;
- Azure DevOps ingestion;
- generalized product insight/drift analysis;
- live MCP/connector delivery merely for convenience.

They are allowed only when a real acceptance finding proves one is required, or after the V0.5 unlock gate is satisfied.

## Current next action

```text
V0.4.4
→ finish layered knowledge + handoff checks
→ implement canonical UI field/validation/conditional behavior evidence
→ add frontend/backend validation regression fixture
→ regenerate artifacts
→ run blind knowledge-only review
```

Do not bump the accepted tool version until the milestone acceptance gate passes.
