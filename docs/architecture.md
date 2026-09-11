# Architecture

PKC is designed as a compiler pipeline.

```text
SOURCE
  ↓
DETERMINISTIC ANALYZERS / ADAPTERS
  ↓
EVIDENCE / FACT MODEL
  ↓
FEATURE & WORKFLOW DISCOVERY
  ↓
KNOWLEDGE SYNTHESIS
  ↓
KNOWLEDGE MODEL
  ↓
MARKDOWN RENDERER
  ↓
PORTABLE PRODUCT KNOWLEDGE
```

## Why not source → Markdown directly?

Direct source-to-Markdown generation makes traceability, incremental rebuilds, testing and hallucination control difficult. PKC therefore keeps a machine-oriented evidence layer separate from human/AI-oriented knowledge.

## Analyzer fidelity is evidence

PKC does not treat every extracted fact as equally reliable. Analyzer mode, confidence and caveats travel with evidence.

Current modes:

```text
project-semantic                  high/medium
  C# target-project MSBuildWorkspace context. A declaration fact is high confidence when its
  syntax node is matched; a project-loaded-but-node-match-failed declaration is reduced to medium.

typescript-ast-syntactic          high/medium
  Angular TypeScript parsed with the target repo's local TypeScript parser using ts.createSourceFile.
  Structural screen/route evidence is high; HTTP-call evidence is medium because receiver types are not checked.

loose-roslyn-fallback             medium
  C# Roslyn analysis without the target project's complete reference graph.

angular-template-regex-fallback   medium
  Conservative Angular template action/visibility extraction.

regex-fallback                    low
  Conservative text-pattern extraction, currently including React and Angular when the AST path is unavailable.
```

Rules:

1. Prefer the strongest available deterministic analyzer.
2. Fallback is allowed only when its provenance is explicit.
3. A loaded semantic environment and a successfully resolved/matched symbol are separate claims.
4. Syntactic AST evidence must not be labeled as type-checked semantic evidence.
5. Downstream grouping/synthesis must not silently upgrade fallback evidence.
6. “Verified” means verified against an acceptance fixture/benchmark, not universal robustness across arbitrary repositories.

## Backend evidence

C# extraction remains Roslyn-based and normalized into `Pkc.Core` facts and relations.

The preferred semantic path is:

```text
.csproj
  ↓
MSBuildWorkspace
  ↓
target framework / project / package references
  ↓
Roslyn Compilation + SemanticModel
  ↓
project-semantic evidence
```

For declaration-like facts, PKC records whether the original evidence fact matched a declaration node in the MSBuild-loaded syntax tree:

```text
semanticNodeMatch = matched | failed
```

A failed match does not discard the fact, but it lowers confidence and adds an explicit caveat. This prevents “project loaded successfully” from being conflated with “this declaration was semantically enriched successfully”.

If the target project cannot be loaded or a source file cannot be mapped to its project compilation, PKC retains conservative loose Roslyn evidence and tags it `loose-roslyn-fallback` rather than pretending the full target semantic context was available.

## Frontend adapter boundary

Frontend frameworks are source adapters, not product-knowledge concepts.

```text
React ───────┐
Angular ─────┤
MVC/Razor ───┤  future adapters
Blazor ──────┤  future adapters
Vue ─────────┘
      ↓
IFrontendAdapter
      ↓
canonical UI facts
      ↓
FrontendScanner + generic relation linker
      ↓
shared evidence model
      ↓
feature/workflow compiler
```

Current canonical UI fact kinds are deliberately small:

- `ui-screen`
- `ui-route`
- `ui-action`
- `ui-api-call`

### Angular

Angular TypeScript currently uses a project-local **syntactic** TypeScript AST. The Node helper loads the target repo's own `node_modules/typescript/lib/typescript.js` and parses with `ts.createSourceFile`.

It does not yet use `ts.createProgram()` or a `TypeChecker`.

Therefore:

- component/screen and route structure can be high-confidence syntactic evidence;
- `ui-api-call` is only medium-confidence because method names and URL shapes are recognized without proving the receiver type;
- method-call graph helpers are name-based and remain syntactic rather than symbol-resolved.

Angular `ui-api-call` carries `httpReceiverResolution=syntactic-unverified` so downstream consumers can distinguish it from future TypeChecker-backed HTTP evidence.

Runtime prerequisites for this path are explicit:

```text
node available on PATH
+ target repo dependencies installed
+ local node_modules/typescript/lib/typescript.js present
```

If those prerequisites are unavailable, Angular falls back to the legacy text analyzer with `regex-fallback` provenance.

Angular template action/visibility extraction remains conservative fallback logic and is explicitly tagged `angular-template-regex-fallback`.

### React

React currently uses the existing conservative text/regex adapter and is tagged `regex-fallback` / low confidence. React AST support is future work.

Framework identity such as `react-static` or `angular-static` remains metadata for provenance/debugging. Knowledge synthesis consumes canonical facts and relations rather than branching on framework identity.

## Boundaries

### Deterministic layer

Responsible for facts that can be proven from source syntax/semantics and their locations. It must preserve analyzer provenance, confidence and caveats.

### Inference layer

Responsible for grouping evidence into product concepts such as features, workflows and business explanations. It consumes canonical evidence, not framework-specific source constructs, and must respect evidence fidelity.

### Presentation layer

Renders the canonical knowledge model into portable Markdown/YAML. Markdown is output, not the internal source of truth for compilation.

## Non-goal

PKC is not trying to implement every frontend framework inside the core. New UI technologies should be added as adapters that emit the same canonical UI evidence contract.
