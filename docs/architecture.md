# Architecture

PKC is designed as a compiler pipeline.

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
  ↓
PORTABLE PRODUCT KNOWLEDGE
```

## Why not source → Markdown directly?

Direct source-to-Markdown generation makes traceability, incremental rebuilds, testing and hallucination control difficult. PKC therefore keeps a machine-oriented evidence layer separate from human/AI-oriented knowledge.

## V0.1 architecture

```text
C# repository
    ↓
Roslyn analyzer
    ↓
Pkc.Core fact records
    ↓
JSON writer
    ↓
.pkc/facts.json
```

No LLM is used in V0.1.

## Initial fact types

- source file
- namespace
- class / interface / enum / record
- method
- property
- attribute
- controller endpoint
- method invocation relation

Later versions can add:

- validators and conditions
- entity writes
- events and handlers
- authorization rules
- EF relationships
- frontend routes/components/actions/API calls
- runtime UI evidence
- Azure DevOps work-item evidence

## Boundaries

### Deterministic layer

Responsible for facts that can be proven from source syntax/semantics and their locations.

### Inference layer

Responsible for grouping evidence into product concepts such as features, workflows and business explanations. This layer arrives after the evidence model is trustworthy.

### Presentation layer

Renders the canonical knowledge model into portable Markdown/YAML. Markdown is output, not the internal source of truth for compilation.
