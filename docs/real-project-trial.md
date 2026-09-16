# V0.4 Knowledge Readiness Exit Plan

Last reconciled: 2026-09-16

This document preserves the historical V0.4.4/V0.4.5 knowledge-readiness gates and records the current V0.4.7 exit state before V0.5 Azure DevOps work may begin.

## Current state

```text
V0.4.4 Loren knowledge readiness                  PASS / COMPLETE
V0.4.5 independent real-repository generalization PASS / COMPLETE
V0.4.6 business logic reconstruction              PASS / COMPLETE
V0.4.7 cross-layer PO-question readiness          CURRENT / NEXT MILESTONE
V0.5 Azure DevOps input evidence                  LOCKED
```

Accepted V0.4.6 production:

```text
c310e893762997f34562a6b3a62dbab2b05c0c93
fix: fail closed on Queryable authority
```

Final V0.4.6 review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-10.md
PASS / COMPLETE
```

Current V0.4.7 acceptance contract:

```text
docs/v0.4.7-acceptance-plan.md
```

The V0.4.4 and V0.4.5 sections below are historical acceptance evidence. Their original execution requirements remain useful as regression context, but they are not the current next action.

V0.5 remains locked until V0.4.7 and the overall V0.4.x PO-question-readiness exit gate pass.

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

## Permanent product-knowledge contract

PKC must preserve:

```text
business conditions
value lineage / provenance
mutation / causality
```

These classes may be connected in explanations, but they must remain distinct.

Observed implementation is not automatically approved business intent. Conservative authority downgrade must not erase deterministic lower-authority lineage, mutation or causal evidence.

See `docs/product-knowledge-contract.md`.

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

### Loren pinned commit — accepted V0.4.4 real-project benchmark

The pinned Loren commit is the deterministic real-repository knowledge-readiness acceptance target.

It must not be modified to suit PKC.

### Loren main — moving canary

Current Loren `main` is a non-blocking canary. It exposes newly introduced source patterns as Loren evolves, but it never silently replaces the pinned acceptance SHA.

### Jellyfin pinned commit — accepted V0.4.5 anti-overfit benchmark

Accepted repository:

```text
jellyfin/jellyfin
1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
```

The second genuine repository gate exists specifically to catch overfitting.

### V0.4.7 focused fixtures — current semantic acceptance surface

Focused fixtures now cover the new questions that cannot be proven merely by rerunning older repositories:

```text
cross-entity value origin
copy/snapshot vs dynamic/reference semantics
derivation
later mutation/override
DTO/API/frontend value composition
joint backend/frontend visibility
authority downgrade with evidence retention
same-name collision negatives
```

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

# Historical V0.4.4 — Loren Knowledge Readiness — COMPLETE / EXTERNAL REVIEW PASS

## Final status

**COMPLETE.**

Final independent review:

```text
docs/reviews/2026-09-14-v0.4.4-external-rereview-4.md
```

The former `IN PROGRESS` wording in this plan was historical and is superseded by the accepted result above.

The Loren trial exposed and regression-locked fixes for:

- Minimal API discovery and semantic enrichment;
- source contamination from `tests/` and `spikes/`;
- conditional/development-only endpoint availability;
- Minimal API failure and direct response semantics;
- multi-project MSBuild semantic loading;
- initializer assignments falsely promoted to domain mutations;
- framework/primitive call-flow noise;
- ASP.NET sign-in/sign-out side effects;
- response metadata collision for endpoints declared inside extension methods;
- explicit zero-fallback acceptance for the pinned production benchmark;
- layered knowledge abstraction/comprehension;
- statically observable UI validation/configuration behavior for supported patterns;
- portable single-file/structured-pack parity.

The historical V0.4.4 execution plan is preserved below because it defines accepted behavior that later milestones must not regress.

## Historical Step 1 — Freeze the output hierarchy

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
- product pages do not behave like transitive call-graph dumps;
- production vs conditional/dev behavior remains distinguishable.

## Historical Step 2 — Product-feature signal hardening

For Loren `Run Operations`, feature-level knowledge needed to retain product-impacting behavior such as:

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

## Historical Step 3 — Index/system orientation

`knowledge/index.md` must let an AI identify the observed capability surface and navigate relationships between important areas without inventing product intent.

For Loren, a knowledge-only reader needed to orient around concepts such as:

```text
owner access/authentication
projects/context
run/agent execution
action proposals and approval/cancel
health/basic surface
conditional development behavior
```

The goal is orientation, not speculative product design.

## Historical Step 3.5 — Portable AI handoff parity

Accepted artifacts:

```text
knowledge/AI_INSTRUCTIONS.md
knowledge/...
PKC_KNOWLEDGE.md
PKC_KNOWLEDGE.zip
```

Rules retained from the original gate:

- `knowledge/` remains canonical;
- `PKC_KNOWLEDGE.md` embeds every canonical knowledge file with explicit file boundaries;
- the bundle places AI instructions/index before feature/workflow detail;
- the single-file bundle preserves the same authority, unknowns and important behavior as the structured pack;
- `PKC_KNOWLEDGE.zip` contains only portable knowledge by default, not source code or `.pkc/facts.json`;
- ZIP parsing is not required for an AI consumer.

Acceptance:

- the same fixed product questions are answerable from `PKC_KNOWLEDGE.md` and the structured pack without semantic disagreement;
- a packaging difference that changes or hides a critical answer is a blocker.

## Historical Step 3.75 — UI validation and configuration behavior

This gate established that portable knowledge must preserve important statically observable UI behavior instead of stopping at route/action/API structure.

The supported benchmark required:

```text
a selectable type/option
an always-required field
a conditionally-required field
a conditionally-visible or enabled field
field → request/API mapping
backend validation for at least one corresponding value
```

Accepted target questions:

```text
What fields/options exist?
Which fields are required?
Which requirement is conditional, and on what condition?
Which field is shown/hidden or enabled/disabled conditionally?
Where is the important field sent in the request/API?
Does backend validation agree with the UI requirement when both are observed?
```

Historical example:

> For a CSP service, is Microsoft Subscription Id required on the UI, under what condition, where is it sent, and does backend validation agree?

No CSP/repository-specific compiler special case was allowed.

Canonical evidence included supported concepts such as:

```text
ui-field
ui-field-option
ui-field-validation
ui-field-visibility
ui-field-enabled-state
ui-field-binding
```

A custom validator whose meaning cannot be proven remains a validator reference/unknown rather than invented business semantics.

## Historical Step 4 — Blind knowledge-only comprehension review

The V0.4.4 gate required:

1. generate a fresh pinned Loren artifact and UI-behavior regression artifact;
2. hide source repositories and `.pkc` raw files for the first pass;
3. first give the reviewer only `PKC_KNOWLEDGE.md`;
4. ask fixed benchmark questions and record answers;
5. provide the structured `knowledge/` pack for deeper navigation;
6. treat semantic disagreement between bundle and structured pack as a packaging blocker;
7. reopen source/known behavior only after knowledge-only answers are recorded;
8. compare critical answers against source/known behavior;
9. classify mismatches using the finding taxonomy.

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

Pass/fail rubric retained:

### Accuracy

- zero blocker-class false statements in fixed-question answers;
- no implementation observation presented as approved business intent.

### Coverage

- every critical question is answerable or explicitly unknown;
- important known behavior does not disappear merely because it was filtered as noise;
- supported UI validation/configuration behavior does not disappear merely because route/action/API linkage exists.

### Abstraction

- product-level answers do not depend on helper-level string/collection mechanics;
- feature pages expose capability-level rules before implementation details.

### Traceability

- important answers trace through workflow evidence to source locations;
- reducing product noise does not destroy proof.

### Honest uncertainty

- missing frontend/runtime/delivery/product-intent evidence remains explicit;
- the pack prefers unknown over invention.

### Handoff parity

- `PKC_KNOWLEDGE.md` and canonical `knowledge/` do not disagree on critical answers;
- archive transport preserves canonical files intact.

A green CI/grep suite was necessary but could not pass this gate by itself.

## Historical Step 5 — External review

The reviewer challenged:

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

V0.4.4 closed only after blocker findings were fixed, regression-locked and independently accepted.

---

# Historical V0.4.5 — Independent Real-Repository Generalization Gate — COMPLETE

## Repository selection requirement

The second repository had to:

- be genuine and not authored/modified for PKC;
- fit at least the supported C# backend surface;
- contain non-trivial product/system behavior;
- differ materially from PokeTrade and Loren;
- be pinned at the reviewed commit;
- not be selected merely because it was easy for current heuristics.

Accepted repository:

```text
jellyfin/jellyfin
1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
```

## Historical trial procedure

1. Build/run the target normally enough to establish valid pinned source.
2. Run PKC without changing the target repo to help the compiler.
3. Review analyzer fallback/provenance only to detect knowledge risk.
4. Review `knowledge/` using the layered-output contract.
5. Validate single-file vs structured-pack handoff parity.
6. Perform a blind knowledge-only comprehension review with product-adapted questions.
7. Include UI behavior questions when a supported UI surface exists.
8. Reopen source and compare answers.
9. Fix only proven generic gaps and add regression coverage.
10. Re-run PokeTrade + Loren + the independent repo after blocker fixes.

## Historical V0.4.5 pass condition

- no blocker wrong claims in critical product questions;
- important behavior answerable or explicitly unknown;
- product-level pages high-signal;
- evidence traceable;
- portable handoff forms preserve the same critical knowledge;
- supported UI validation/configuration behavior preserved when present;
- no benchmark-specific hardcoding;
- existing PokeTrade and Loren acceptance remain green;
- independent external review finds no unresolved blocker.

V0.4.5 is accepted and remains closed.

---

# Historical V0.4.6 — Business Logic Reconstruction — COMPLETE

Accepted production:

```text
c310e893762997f34562a6b3a62dbab2b05c0c93
```

Final independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-10.md
PASS / COMPLETE
```

Accepted exact-SHA gates:

```text
CI + PKC tests + WorkPlay + PokeTrade   34990080620 — PASS
pinned Loren                            34990080707 — PASS
Loren-main canary                       34990080551 — PASS
pinned Jellyfin                         34990080546 — PASS
```

V0.4.6 established conservative business-predicate authority, including fail-closed Queryable/provider behavior, while retaining downgraded deterministic predicate evidence.

Do not reopen B6.1-B6.4 without a new compile-valid and behavior-valid contradiction.

Non-blocking carry-forward warnings:

```text
W10.1
Rendered Evidence does not explicitly print `observed-only`, although authority separation is preserved.

W10.2
Queryable names remain in old safe-operation sets but are unreachable behind the Queryable fail-closed guard.
```

These are V0.4.7 maintenance considerations, not V0.4.6 blockers.

---

# Current V0.4.7 — Cross-layer PO-question Readiness

V0.4.7 is the remaining V0.4.x gate before Azure DevOps may be considered.

Acceptance contract:

```text
docs/v0.4.7-acceptance-plan.md
```

Required PO questions include:

```text
Where did this value originally come from?
If the upstream value changes later, does the existing downstream value change automatically?
What code path can change this value after creation?
Was this value directly copied or computed?
What was the last observed source before the value was persisted or returned?
What backend conditions and frontend conditions jointly determine whether an item is visible?
How did the value move through backend → DTO/projection → API → frontend composition?
If authority is incomplete, what lineage or causal evidence is still deterministically known?
```

Canonical lineage:

```text
ProductGroup.Price → Product.Price → Service.Price
```

Required semantics:

```text
copy
snapshot
derivation
reference/dynamic
override
mutation
```

Blocking collision regression:

```text
Product.Price
Service.Price
Dto.Price
Component.price
```

Matching names alone must never create lineage.

Current implementation sequence:

```text
V0.4.7-A origin/copy timing + snapshot/dynamic, rendered in workflow knowledge
V0.4.7-B computation + terminal source + later change, rendered with provenance
V0.4.7-C DTO/projection + API output, rendered with exact mapping
V0.4.7-D frontend binding + joint visibility, rendered with distinct authorities
V0.4.7-E final knowledge-only review + portable parity/no-leak + exact-SHA gates
```

No V0.4.7 compiler behavior is implemented by the planning checkpoint itself.

The next deliverable is the first A copy/snapshot slice through scanner, candidate, synthesis, and workflow Markdown. Apply the [acceptance plan](v0.4.7-acceptance-plan.md)'s receiver/path/storage/semantic negatives, then complete the dynamic-read positive before closing A. Rendering is part of every checkpoint; E validates the completed pack. Preserve pinned acceptance targets and the separate Loren-main canary role. Existing V0.4.6 results are historical baseline evidence, not proof that these new V0.4.7 answers work.

---

# V0.5 Unlock Gate — CURRENT FRAMING

**V0.5 Azure DevOps remains locked.**

Historical prerequisites are accepted:

```text
PokeTrade known-answer regression                    PASS
Loren pinned evidence/workflow correctness           PASS
Loren blind knowledge-only comprehension             PASS
Loren single-file/structured handoff parity          PASS
UI validation/behavior knowledge benchmark           PASS
Loren external review with no blocker                PASS
Second independent real-repo trial                   PASS
Second blind knowledge-only comprehension            PASS
Second handoff parity                                PASS
Cross-benchmark regression after accepted fixes      PASS
Known boundaries/unknowns documented honestly        PASS
No repository-specific compiler exceptions          PASS
V0.4.6 business-logic independent review             PASS
```

Those historical checks are necessary but no longer sufficient by themselves.

The current remaining unlock requirement is:

```text
V0.4.7 cross-layer PO-question readiness            REQUIRED / CURRENT
V0.4.x final PO-question-readiness exit review      REQUIRED
```

Only after V0.4.7 and the overall V0.4.x PO-question-readiness exit gate pass may V0.5 Azure DevOps ingestion begin.

If the V0.4.7 gate is not PASS, stay in V0.4.x.

## Explicit non-goals before the unlock gate

Do not start these merely because they are on the roadmap:

- Angular TypeScript `TypeChecker` migration unless a proven V0.4.7 acceptance gap requires it;
- React AST rewrite unless a real acceptance gap requires it;
- MVC/Razor/Blazor/Vue expansion;
- runtime browser exploration;
- incremental compilation;
- Azure DevOps ingestion;
- generalized product insight/drift analysis;
- live MCP/connector delivery merely for convenience.

They are allowed only when a real acceptance finding proves one is required, or after the relevant roadmap gate is satisfied.

## Version semantics before any package bump

Do not conflate:

```text
roadmap milestone version
tool/package version
evidence/schema version
```

Current examples:

```text
roadmap:             V0.4.7
tool/package:        RuaDen.Pkc.Tool 0.4.3-preview.2
C# raw schema:       0.4.4-csharp-raw
merged facts schema: 0.4.4
cross-stack schema:  0.4.6
frontend schema:     0.4.3-frontend
```

A milestone change does not automatically change package or schema versions.

Schema versions change only when the serialized contract/semantics change and the compatibility change is explicit and regression-covered.

Do not mechanically change `0.4.4`, `0.4.4-csharp-raw`, `0.4.6` or other schema strings because V0.4.7 is current.

## Current next action

```text
V0.4.7-A
→ add executable endpoint-backed ProductGroup.Price → Product.Price → Service.Price regression
→ require both scoped copy/snapshot edges in generated workflow Markdown
→ apply the acceptance plan's identity/path/storage/project-semantic negatives
→ prove meaningful semantic/output RED
→ implement the minimum proof, retention, synthesis and rendering for the supported slice
→ run focused + full relevant local tests/build
→ review diff and generated PO answers
→ save a coherent commit; publish when authorized and verify checkpoint gates
```

The first snapshot slice does not complete A without its separate dynamic positive. Do not begin B/C/D until A's identity, semantics and portable delivery gates are green.

Do not bump the accepted tool package as part of the planning checkpoint.

Do not start Azure DevOps ingestion.
