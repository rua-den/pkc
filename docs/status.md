# PKC Status

Last updated: 2026-09-12

## North star

PKC is a **Product/System Knowledge Compiler**.

The product is not the analyzer, the fact graph, or the Markdown renderer by themselves. The product is the generated `knowledge/` pack that a Product Owner can attach to an AI assistant and use to understand the system **without making that AI re-scan the source repository**.

The compiler pipeline remains:

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
PO / AI CAN UNDERSTAND THE PRODUCT
```

Analyzer fidelity is necessary for trustworthy knowledge, but it is a means rather than the final acceptance target.

## Current milestone

**V0.4.4 External real-project trial — IN PROGRESS**

V0.4.3 Analyzer Fidelity Hardening remains the last completed release checkpoint (`0.4.3-preview.2`).

The real-project trial uses `rua-den/loren` in two forms:

- a pinned Loren commit for deterministic blocking acceptance;
- current Loren `main` as a moving non-blocking canary for newly introduced real-world patterns.

## Proven V0.4.4 gaps already found and fixed

The Loren trial has already exposed gaps that WorkPlay/PokeTrade did not:

- Minimal API endpoint discovery and target-project semantic enrichment;
- product-source contamination from `tests/` and `spikes/`;
- conditional endpoint registration such as the development-only run endpoint;
- Minimal API condition → response semantics;
- direct/success Minimal API response semantics;
- multi-project `MSBuildWorkspace` loading that previously caused false loose-Roslyn fallback in referenced projects;
- dictionary/object-initializer assignments being promoted incorrectly to domain state changes;
- framework and primitive call noise leaking into workflow flow presentation;
- ASP.NET authentication `SignInAsync` / `SignOutAsync` side effects not being surfaced as product behavior;
- fact-ID collision that could drop Minimal API response metadata for endpoints declared inside extension methods.

The current Loren acceptance requires zero `loose-roslyn-fallback` facts for the pinned production benchmark and rejects product evidence from `tests/` or `spikes/`.

PokeTrade remains green as the known-answer behavioral regression system while Loren is the real-repository trial.

## Important finding from full Loren output review

The analyzer/evidence layer is no longer the main blocker discovered by the current artifact review.

The generated knowledge is substantially correct and traceable, but some **product-level Markdown is still too implementation-oriented**. In particular, `Run Operations` currently promotes many helper-level conditions and loops into the product-feature summary and duplicates most of them across `/api/run` and `/internal/dev/run`.

That is valid implementation evidence, but it is the wrong abstraction level for a Product Owner or an AI answering product questions.

Therefore V0.4.4 is **not complete** yet.

## V0.4.4 acceptance contract

V0.4.4 passes only when all of the following are true.

### 1. Evidence correctness

Important generated claims must remain grounded in source evidence and provenance/confidence must not overclaim analyzer fidelity.

### 2. Workflow correctness

A workflow file must preserve the important behavior of one user/system operation:

- entry point / UI path when known;
- permission;
- meaningful validation and failure paths;
- meaningful state changes;
- side effects/integrations;
- relevant backend flow;
- source evidence and explicit unknowns.

### 3. Product-level signal

Feature files and `knowledge/index.md` must summarize the product rather than copy implementation internals.

Helper-level string processing, collection loops, plumbing calls and duplicate rules may remain in evidence/workflow detail when useful for traceability, but must not dominate product-feature summaries.

### 4. Blind knowledge comprehension

A reviewer must be able to hide the source repository, use only generated `knowledge/`, and answer the important product questions correctly.

For the Loren trial this includes at least:

```text
What is this system's observable product surface?
How does owner authentication work?
What does the main run operation do?
How are projects listed/bootstraped and what can fail?
What are action proposals and how are approve/cancel handled?
Which endpoints/flows are conditional or development-only?
What important failure paths exist?
What is still unknown because the evidence source has not been compiled yet?
```

The answers are then checked against source/known behavior. A CI grep passing is not sufficient by itself.

### 5. No blocker-class review findings

No unresolved:

```text
wrong claim
missing important behavior
product-level noise that prevents comprehension
unexpected fallback affecting important behavior
unsupported pattern required by the selected real-repository trial
```

Every compiler bug proven by the external trial receives a regression test or acceptance assertion.

## Scope discipline

Until the V0.4.4 knowledge-comprehension gate passes, do **not** start work merely because these capabilities are attractive:

- Azure DevOps ingestion;
- Angular TypeScript `TypeChecker`;
- React AST rewrite;
- additional frontend frameworks;
- browser/runtime exploration;
- incremental compilation;
- generalized product insight/drift analysis.

They become work only if the current real-project acceptance proves one is the next blocker, or after the current milestone is accepted according to the roadmap.

## Current commands

```bash
pkc scan <repository-path>
pkc build <repository-path>
```

Last accepted packaged tool version:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

Current development output remains:

```text
.pkc/facts.json
.pkc/feature-candidates.json
.pkc/product-features.json
knowledge/index.md
knowledge/features/**/*.md
knowledge/workflows/**/*.md
```

## Next engineering focus

Improve **knowledge synthesis/presentation only where the Loren output proves it is needed**:

1. keep detailed evidence/workflow traceability;
2. prevent helper-level implementation conditions and loops from dominating product-feature summaries;
3. collapse duplicate product rules shared by equivalent production/dev workflows where doing so does not lose important distinctions;
4. re-run the blind `knowledge/`-only comprehension review;
5. compare those answers with Loren source/known behavior;
6. fix only proven remaining gaps.

**V0.5 remains locked.**
