# PKC Status

Last updated: 2026-09-12

## North star

PKC is a **Product/System Knowledge Compiler**.

The product is the generated portable knowledge pack that a Product Owner can give to an AI assistant to understand the system **without making that AI re-scan the source repository**.

The analyzer, fact graph, candidate builder and renderer are supporting compiler stages. They are not independent success criteria.

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
PORTABLE AI HANDOFF
  ↓
AI CAN EXPLAIN THE PRODUCT AT THE RIGHT ABSTRACTION LEVEL
```

## Current milestone

**V0.4.4 Loren Knowledge Readiness — IN PROGRESS**

V0.4.3 Analyzer Fidelity Hardening remains the last accepted packaged checkpoint:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

Current development on `main` contains V0.4.4 trial fixes, portable AI handoff work and UI validation/behavior correlation. The accepted package version must not be bumped until the milestone gate passes.

Detailed execution/exit plan:

```text
docs/real-project-trial.md
```

AI handoff contract:

```text
docs/ai-handoff.md
```

UI behavior contract:

```text
docs/ui-behavior-contract.md
```

## Current benchmark roles

### PokeTrade

Known-answer runnable regression benchmark. Protects behavior already proven by controlled acceptance tests.

### Loren pinned commit

Blocking real-project benchmark for V0.4.4. The repository is not changed to suit PKC.

### Loren main

Moving non-blocking canary that exposes new real-world source patterns while Loren evolves. It never silently replaces the pinned acceptance SHA.

## Proven V0.4.4 gaps already found and fixed

The Loren trial has exposed gaps that PokeTrade did not:

- Minimal API endpoint discovery and target-project semantic enrichment;
- product-source contamination from `tests/` and `spikes/`;
- conditional/development-only endpoint registration;
- Minimal API condition/failure/direct response semantics;
- multi-project `MSBuildWorkspace` loading and false fallback elimination;
- dictionary/object-initializer assignments promoted incorrectly to domain state changes;
- framework and primitive call noise leaking into workflow flow presentation;
- ASP.NET authentication `SignInAsync` / `SignOutAsync` side effects not surfaced as product behavior;
- fact-ID collision that could drop Minimal API response metadata inside extension methods.

The pinned Loren acceptance currently requires zero `loose-roslyn-fallback` facts for the selected production benchmark and rejects product evidence from `tests/` or `spikes/`.

## Current development work

### Layered product knowledge

Product feature rule promotion is being hardened so helper-level conditions/loops remain available in workflow/evidence detail without dominating capability-level pages.

The first regression target is Loren `Run Operations`, where helper string-processing and collection loops must not be promoted as product rules while externally meaningful failures, responses and conditional development behavior remain visible.

### Portable AI handoff

`pkc build` development output now targets:

```text
knowledge/
  AI_INSTRUCTIONS.md
  index.md
  features/**/*.md
  workflows/**/*.md

PKC_KNOWLEDGE.md
PKC_KNOWLEDGE.zip
```

Contract:

```text
knowledge/         canonical structured pack
PKC_KNOWLEDGE.md   one-file AI upload convenience
PKC_KNOWLEDGE.zip  archive transport/storage convenience
```

The ZIP must not include source code or raw `.pkc` evidence by default. Single-file and structured-pack forms must preserve the same critical meaning.

### UI validation + behavior completeness

A core acceptance requirement is explicit: PKC must not stop at route/button/API knowledge when source contains meaningful form/configuration behavior.

Current canonical evidence includes:

```text
ui-field
ui-field-option
ui-field-validation
ui-field-visibility
ui-field-enabled-state
ui-field-binding
backend-field-validation
ui-backend-validation
```

The current Angular/static form layer preserves:

```text
field existence
select/type options
required fields
conditional requiredness
conditional visibility/enabled state
field → request-field mapping when statically traceable
```

Backend validation evidence currently recognizes:

```text
[Required] / [RequiredAttribute]
missing-value guards such as IsNullOrWhiteSpace / IsNullOrEmpty / null checks
conditional requiredness when the missing-value guard is gated by another condition
```

Cross-stack validation correlation now joins UI binding + UI requiredness + backend requiredness and emits one of:

```text
consistent
possible-mismatch
unknown
```

The known-answer CSP fixture proves this full path:

```text
serviceType offers CSP/NCE
serviceType is required on UI + backend
msSubscriptionId is visible when serviceType == CSP
msSubscriptionId is required when serviceType == CSP
msSubscriptionId maps to microsoftSubscriptionId
backend MicrosoftSubscriptionId is required when ServiceType == CSP
UI/backend conditional requiredness normalizes to the same condition key
portable knowledge reports observed consistency
```

Negative classification regressions also require:

```text
backend required + UI required not observed
→ possible-mismatch

UI required + backend required not observed
→ unknown

different UI/backend requiredness conditions
→ possible-mismatch
```

The happy path is asserted through evidence → workflow knowledge → Markdown → `PKC_KNOWLEDGE.md`, so these semantics may not die inside `.pkc/facts.json`.

Latest verified development checkpoint:

```text
HEAD: 326f556517c27085ed027eecdeef7e4125c851a6
CI #195: test PASS + PokeTrade PASS
Loren external trial #94: PASS
Loren-main canary #75: PASS
```

This proves the controlled cross-stack validation contract and protects existing real-project benchmarks. It does **not** yet prove arbitrary real UI generalization; that remains part of the independent real-repository gate.

## Main remaining V0.4.4 risk

The primary V0.4.4 blocker is now **knowledge-only comprehension and abstraction quality**, not whether validation evidence can technically survive the compiler.

The compiler can now preserve the supported UI/backend validation behavior and classify observed agreement carefully. The next question is whether a reader given only the portable knowledge can answer the critical product questions correctly, efficiently and with honest unknowns.

Therefore V0.4.4 is **not complete**.

## Knowledge hierarchy that must hold

```text
AI instructions
  → tell the receiving AI how to consume authority/unknowns and navigate the pack

index
  → orient the AI around observed capabilities and boundaries

feature
  → explain a product/system capability at high signal

workflow
  → explain one operation with permissions, validations, failures, state, side effects and flow

raw evidence
  → preserve detailed implementation proof and analyzer provenance
```

Do not delete low-level evidence merely to reduce noise. Promote it only to the level where it is useful.

## Current next steps

Follow `docs/real-project-trial.md` in this order:

```text
1. keep layered knowledge + handoff + UI/backend validation gates green
2. regenerate the pinned Loren artifact
3. blind-review PKC_KNOWLEDGE.md first
4. cross-check the structured knowledge/ pack
5. reopen Loren source and compare every critical answer
6. fix/regression-lock only proven blockers
7. external review V0.4.4
8. V0.4.5 independent real-repo trial, preferably with a supported real form/configuration surface
```

The blind Loren review must answer from portable knowledge alone, including at least:

```text
what capabilities Loren exposes
how owner authentication works
what the main run operation does
how project + memory context participate
how project list/bootstrap behaves
how action proposal approve/cancel behaves
what is conditional/dev-only
important permissions/validations/failures/side effects
what remains explicitly unknown
```

A supported UI/form benchmark must additionally prove the pack can answer:

```text
which fields/options exist
which fields are required
which requirements are conditional
which fields are shown/hidden or enabled/disabled conditionally
where important values are sent
whether frontend/backend validation agree when both are observed
```

CI/grep success is necessary but not sufficient.

## V0.4.5 — independent real-repo generalization gate

Even after Loren passes, **V0.5 does not open immediately**.

V0.4.5 will run the same knowledge-readiness and handoff-parity process against a second genuine repository that was not created/modified for PKC and differs materially from PokeTrade/Loren.

Purpose: catch benchmark overfitting before adding another major evidence source.

The second repo must be selected for realism, not because current heuristics handle it easily. If a suitable supported frontend exists, prefer a repository that also exercises real form/configuration behavior so the UI-behavior contract is tested outside synthetic fixtures.

## V0.5 unlock condition

**V0.5 Azure DevOps remains locked until all are PASS:**

```text
PokeTrade known-answer regression                  PASS
Loren evidence/workflow correctness                PASS
Loren blind knowledge-only comprehension           PASS
Loren handoff parity                               PASS
UI validation/behavior known-answer benchmark      PASS
Loren external review                              PASS
second independent real-repo trial                 PASS
second knowledge-only comprehension                PASS
second handoff parity                              PASS
cross-benchmark regression after all fixes         PASS
known boundaries/unknowns documented honestly      PASS
no repository-specific compiler exceptions         PASS
```

If any gate is not PASS, remain in V0.4.x.

## Scope discipline

Until the unlock gate passes, do not start these merely because they are attractive roadmap work:

- Azure DevOps ingestion;
- Angular TypeScript `TypeChecker` migration unless a validation/binding case proves it necessary;
- React AST rewrite unless a real benchmark requires it;
- additional frontend frameworks;
- browser/runtime exploration;
- incremental compilation;
- generalized product insight/drift analysis;
- live MCP/connector delivery merely for convenience.

A new analyzer or delivery capability may enter V0.4.x only when a real knowledge review proves it is required to improve:

```text
accuracy
completeness
signal-to-noise
traceability
honest uncertainty
portable handoff reliability
```

## Current commands

```bash
pkc scan <repository-path>
pkc build <repository-path>
```

Current development output:

```text
.pkc/facts.json
.pkc/feature-candidates.json
.pkc/product-features.json
knowledge/AI_INSTRUCTIONS.md
knowledge/index.md
knowledge/features/**/*.md
knowledge/workflows/**/*.md
PKC_KNOWLEDGE.md
PKC_KNOWLEDGE.zip
```
