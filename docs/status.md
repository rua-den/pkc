# PKC Status

Last updated: 2026-09-13

## North star

PKC is a **Product/System Knowledge Compiler**.

The product is the generated portable knowledge pack that a Product Owner can hand to an AI assistant and use for product/system questions **without making that AI rescan the source repository**.

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
AI CAN EXPLAIN THE PRODUCT WITH HONEST BOUNDARIES
```

Do not replace this with `source → LLM → Markdown`.

## Current milestone

**V0.4.4 Loren Knowledge Readiness — READY FOR INDEPENDENT EXTERNAL REVIEW**

V0.4.3 remains the last accepted packaged checkpoint:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

Do **not** bump the accepted package version merely because V0.4.4 development gates are green. Version advancement is an acceptance decision.

## Current verified development checkpoint

Implementation/acceptance commit before documentation-only updates:

```text
4f7f75e76a1f158a880e8f2d1d64ea0bea0d36e7
```

Verified runs:

```text
CI #205
  test                         PASS
  PokeTrade real system        PASS

Loren external trial #104      PASS
Loren-main canary #86          PASS
```

Pinned Loren benchmark SHA:

```text
e9e81651d380d7d40998f235cfdc7f119fe67af8
```

## V0.4.4 work proven so far

The Loren real-project trial forced generic fixes for gaps that PokeTrade did not expose:

- Minimal API discovery + target-project semantic enrichment;
- product-source exclusion for `tests/` / `spikes/`;
- conditional/development-only endpoint registration;
- Minimal API failure/direct-response semantics;
- multi-project MSBuild semantic loading;
- dictionary/object-initializer state-change noise;
- framework/primitive call-flow noise;
- ASP.NET sign-in/sign-out side effects;
- Minimal API response fact-ID collision;
- feature-level product-rule filtering;
- portable AI handoff (`knowledge/`, `PKC_KNOWLEDGE.md`, `PKC_KNOWLEDGE.zip`);
- Angular/static UI field/options/validation/visibility/enabled-state/binding evidence;
- backend field validation evidence;
- UI ↔ backend validation correlation;
- high-signal product-level capability-flow promotion.

Pinned Loren still requires zero `loose-roslyn-fallback` facts for the selected production benchmark and rejects product evidence from test/spike source.

## UI/backend validation contract

Canonical evidence now includes:

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

Controlled classification contract:

```text
same requiredness + matching condition evidence
→ consistent

backend required + UI required not observed
→ possible-mismatch

UI required + backend required not observed
→ unknown

different observed requiredness conditions
→ possible-mismatch
```

The CSP regression proves the full portable path:

```text
serviceType offers CSP/NCE
serviceType required on UI + backend
msSubscriptionId visible when serviceType == CSP
msSubscriptionId required when serviceType == CSP
msSubscriptionId → microsoftSubscriptionId request binding
backend MicrosoftSubscriptionId required when ServiceType == CSP
normalized condition evidence matches
workflow Markdown reports observed consistency
PKC_KNOWLEDGE.md preserves the same answer
```

Negative tests lock `possible-mismatch` and `unknown` so missing evidence cannot silently become false consistency.

## Loren blind knowledge-only comprehension

First blind pass found two blocking abstraction gaps:

```text
A. Run feature did not expose project → memory → agent/brain collaboration clearly enough.
B. Action-proposal feature did not make current create-branch semantics obvious enough.
```

Source cross-check confirmed both were real product-semantic gaps.

Generic fix: product feature references now carry a selective `Observed capability flow` built from grounded workflow call evidence. It prioritizes application/capability boundaries and suppresses obvious plumbing/self-helper noise.

Second blind pass on the artifact produced by `4f7f75e...` used **only `PKC_KNOWLEDGE.md`** and can now recover:

```text
POST /api/run
→ LorenRunService.RunAsync
→ LorenProjectContextBuilder.BuildAsync
→ LorenMemoryContextBuilder.BuildAsync
→ IMemoryStore.ListCurrentForProjectAsync

LorenRunService.RunAsync
→ AgentLoop.RunAsync
→ IBrain.ThinkAsync
→ IActionGateway.ExecuteAsync
```

and for proposal approval:

```text
approve endpoint
→ ApproveProposalAndCreateBranchAsync
→ ICreateBranchProposalStore
→ IProjectCatalog
→ ActionIntentFingerprint
→ IActionGateway.ExecuteAsync
```

Results:

```text
Loren blind knowledge-only comprehension   PASS
Run abstraction finding A                  RESOLVED
Action-proposal finding B                  RESOLVED
workflow-detail noise finding C            NON-BLOCKING / preserved for traceability
integration presentation finding D         RESOLVED via capability-flow abstraction
```

Detailed benchmark record:

```text
docs/benchmarks/2026-09-12-loren-blind-review.md
```

## Portable handoff parity

Current contract:

```text
knowledge/         canonical structured knowledge pack
PKC_KNOWLEDGE.md   one-file AI upload convenience
PKC_KNOWLEDGE.zip  transport/archive of knowledge/ only
```

For the current pinned Loren artifact:

```text
structured Markdown files:            23
files embedded verbatim in bundle:    23 / 23
missing bundle markers/content:        0
PKC_KNOWLEDGE.zip knowledge files:     23
raw .pkc entries in handoff ZIP:       0
source .cs/.ts entries in handoff ZIP: 0
```

**Loren handoff parity: PASS.**

The CI artifact itself may contain `.pkc` diagnostic output for benchmark inspection; that is not the PO-facing `PKC_KNOWLEDGE.zip` contract.

## Knowledge hierarchy that must hold

```text
AI_INSTRUCTIONS
  → tell the receiving AI how to consume authority/unknowns

index
  → orient around observed capabilities and boundaries

feature
  → explain product/system capability at high signal

workflow
  → explain one operation with exact behavior and evidence

raw evidence
  → preserve implementation proof/provenance
```

Low-level evidence should not be deleted merely to make output cleaner, and should not dominate feature pages.

## V0.4.4 acceptance position

Current gate status:

```text
PokeTrade known-answer regression                  PASS
Loren evidence/workflow correctness                PASS
Loren blind knowledge-only comprehension           PASS
Loren handoff parity                               PASS
UI validation/behavior known-answer benchmark      PASS
Loren external independent review                  PENDING
```

Therefore V0.4.4 is **not self-declared complete**. The next action is an independent review of the current checkpoint.

If that review finds a serious blocker:

```text
classify finding
→ fix only proven generic gap
→ add regression
→ rerun all current gates
→ re-review
```

If the review passes, move to **V0.4.5 second independent real-repository generalization**.

## V0.4.5 — second real-repository gate

The second repository must be genuine, not created/modified for PKC, and materially different from PokeTrade/Loren.

Run the same process:

```text
build real project normally
→ pkc build
→ inspect fallbacks/provenance
→ blind knowledge-only comprehension
→ structured/single-file parity
→ source cross-check
→ fix only proven generic defects
→ regression-lock
→ independent review
```

Prefer a supported real frontend/configuration surface if available so UI/form semantics are tested outside synthetic fixtures.

## V0.5 unlock condition

**V0.5 Azure DevOps remains locked until all are PASS:**

```text
PokeTrade known-answer regression                  PASS
Loren evidence/workflow correctness                PASS
Loren blind knowledge-only comprehension           PASS
Loren handoff parity                               PASS
UI validation/behavior known-answer benchmark      PASS
Loren independent external review                  PENDING
second independent real-repo trial                 PENDING
second knowledge-only comprehension                PENDING
second handoff parity                              PENDING
cross-benchmark regression after all fixes         PENDING
known boundaries/unknowns documented honestly      PASS so far
no repository-specific compiler exceptions         PASS so far
```

If any required gate is not PASS, remain in V0.4.x.

## Scope discipline

Until the unlock gate passes, do not start these merely because they are attractive roadmap work:

- Azure DevOps ingestion;
- Angular TypeScript `TypeChecker` migration unless a benchmark proves it necessary;
- React AST rewrite unless a real benchmark requires it;
- additional frontend frameworks;
- browser/runtime exploration;
- incremental compilation;
- generalized insight/drift analysis;
- live MCP/connector delivery for convenience alone.

A new analyzer/delivery capability may enter V0.4.x only when a real knowledge review proves it improves accuracy, completeness, signal-to-noise, traceability, honest uncertainty, or portable handoff reliability.

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
