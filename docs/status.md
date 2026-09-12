# PKC Status

Last updated: 2026-09-12

## North star

PKC is a **Product/System Knowledge Compiler**.

The product is the generated portable `knowledge/` pack that a Product Owner can give to an AI assistant to understand the system **without making that AI re-scan the source repository**.

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
PORTABLE MARKDOWN
  ↓
AI CAN EXPLAIN THE PRODUCT AT THE RIGHT ABSTRACTION LEVEL
```

## Current milestone

**V0.4.4 Loren Knowledge Readiness — IN PROGRESS**

V0.4.3 Analyzer Fidelity Hardening remains the last accepted packaged checkpoint:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

Current development on `main` contains V0.4.4 trial fixes, but the accepted package version must not be bumped until the milestone gate passes.

Detailed execution/exit plan:

```text
docs/real-project-trial.md
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

## Main remaining V0.4.4 risk

The main blocker discovered by full artifact review is now **knowledge abstraction/comprehension**, not analyzer breadth.

The generated knowledge is substantially correct and traceable, but product-level output such as `Run Operations` still promotes too many transitive helper conditions/loops and duplicates shared behavior across production/dev entry points.

This is valid implementation evidence at the wrong knowledge layer.

Therefore V0.4.4 is **not complete**.

## Knowledge hierarchy that must hold

```text
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
1. enforce layered knowledge abstraction
2. harden product-feature signal using Loren output
3. improve index/system orientation without inventing intent
4. regenerate pinned Loren artifact
5. perform blind knowledge-only comprehension review
6. reopen Loren source and compare every critical answer
7. fix/regression-lock only proven blockers
8. external review V0.4.4
```

The blind review must answer from `knowledge/` alone, including at least:

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

CI/grep success is necessary but not sufficient.

## V0.4.5 — independent real-repo generalization gate

Even after Loren passes, **V0.5 does not open immediately**.

V0.4.5 will run the same knowledge-readiness process against a second genuine repository that was not created/modified for PKC and differs materially from PokeTrade/Loren.

Purpose: catch benchmark overfitting before adding another major evidence source.

The second repo must be selected for realism, not because current heuristics handle it easily.

## V0.5 unlock condition

**V0.5 Azure DevOps remains locked until all are PASS:**

```text
PokeTrade known-answer regression                  PASS
Loren evidence/workflow correctness                PASS
Loren blind knowledge-only comprehension           PASS
Loren external review                              PASS
second independent real-repo trial                 PASS
second knowledge-only comprehension                PASS
cross-benchmark regression after all fixes         PASS
known boundaries/unknowns documented honestly      PASS
no repository-specific compiler exceptions         PASS
```

If any gate is not PASS, remain in V0.4.x.

## Scope discipline

Until the unlock gate passes, do not start these merely because they are attractive roadmap work:

- Azure DevOps ingestion;
- Angular TypeScript `TypeChecker` migration;
- React AST rewrite;
- additional frontend frameworks;
- browser/runtime exploration;
- incremental compilation;
- generalized product insight/drift analysis.

A new analyzer capability may enter V0.4.x only when a real knowledge review proves it is required to improve:

```text
accuracy
completeness
signal-to-noise
traceability
honest uncertainty
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
knowledge/index.md
knowledge/features/**/*.md
knowledge/workflows/**/*.md
```
