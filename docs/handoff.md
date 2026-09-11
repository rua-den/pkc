# PKC Handoff

Use this file when continuing PKC in another chat/session or external review.

## Product idea

PKC means **Product/System Knowledge Compiler**.

The target user is a Product Owner. A PO should be able to take PKC's generated `knowledge/` folder and attach it to any capable AI — ChatGPT, Claude, Gemini, Copilot, etc. — then ask questions about the product without making that AI re-read or grep the source repository.

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
DETERMINISTIC MARKDOWN RENDERER
```

## Current verified state

**V0.4.3 Analyzer Fidelity Hardening is COMPLETE, including follow-up review fixes.**

Verified acceptance commit:

```text
5c457111d072ad5f7b93bf3cff49d27960ac79fb
```

Verified CI run:

```text
34630303904  (#95)
```

Both `test` and `poketrade-real-system` are green.

Tool package version:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

## C# analyzer fidelity

Preferred path:

```text
.csproj
  ↓
MSBuildWorkspace
  ↓
target project compilation/references
  ↓
Roslyn SemanticModel
  ↓
analysisMode = project-semantic
```

Declaration facts now distinguish project load from actual node matching:

```text
semanticNodeMatch = matched   → confidence high
semanticNodeMatch = failed    → confidence medium + analysisCaveat
```

If project loading/source mapping fails, PKC keeps conservative loose Roslyn evidence:

```text
analysisMode = loose-roslyn-fallback
analysisConfidence = medium
semanticContext = runtime-platform-assemblies-only
analysisFallbackReason = ...
```

## Frontend analyzer fidelity

Frontend technology remains outside compiler core and emits canonical `ui-*` evidence.

### Angular TypeScript

Current TypeScript analysis is **syntactic AST**, not TypeChecker-backed semantic analysis:

```text
analysisMode = typescript-ast-syntactic
typescriptSemanticContext = syntax-only-no-type-checker
```

Confidence:

```text
ui-screen / ui-route = high
ui-api-call          = medium
```

`ui-api-call` carries:

```text
httpReceiverResolution = syntactic-unverified
analysisCaveat = http-method-name-and-url-shape-detected-without-receiver-type-checking
```

Do not describe Angular HTTP evidence as type-resolved yet. The Node helper currently uses `ts.createSourceFile`, not `ts.createProgram()` + `TypeChecker`.

Angular AST runtime preconditions:

```text
node on PATH
+ target repo dependencies installed
+ local node_modules/typescript/lib/typescript.js available
```

When unavailable, Angular falls back to `regex-fallback` / low with an explicit fallback reason.

### Angular templates

Template action/visibility extraction remains:

```text
analysisMode = angular-template-regex-fallback
analysisConfidence = medium
```

### React

React remains:

```text
analysisMode = regex-fallback
analysisConfidence = low
```

## CI fidelity acceptance

PokeTrade CI deliberately exercises the AST path after `npm install` and asserts:

```text
project-semantic
semanticNodeMatch = matched
semanticBaseType = Microsoft.AspNetCore.Mvc.ControllerBase
semantic HTTP attribute resolution
typescript-ast-syntactic
syntax-only-no-type-checker
ui-screen/ui-route high confidence
ui-api-call medium confidence
httpReceiverResolution = syntactic-unverified
angular-template-regex-fallback
no full Angular regex-fallback
```

A dedicated C# regression test also forces a declaration StartLine mismatch and verifies `semanticNodeMatch=failed` + confidence downgrade.

## Important interpretation of “verified”

WorkPlay and PokeTrade prove the current acceptance contracts. They do **not** prove arbitrary real-world repositories are already robustly supported.

## Current commands

```bash
pkc scan <repository-path>
pkc build <repository-path>
```

Main outputs:

```text
.pkc/facts.json
.pkc/feature-candidates.json
.pkc/product-features.json
knowledge/index.md
knowledge/features/**/*.md
knowledge/workflows/**/*.md
```

## Next engineering target — keep narrow

**V0.4.4 external real-project trial.**

Do not add speculative capability first. Run the current compiler against a genuine repository, inspect analyzer modes/fallbacks, review generated knowledge against source + known behavior, then classify findings as wrong claim, missing important behavior, noise, unsupported stack/pattern or unexpected fallback.

Fix only proven gaps and add a regression fixture for each fix. Do not start V0.5 Azure DevOps, TypeScript TypeChecker work, React AST, MVC/Blazor/Vue adapters, Playwright, or incremental architecture unless the external trial proves they are the next blocker.
