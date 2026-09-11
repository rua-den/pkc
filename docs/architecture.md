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

PKC does not treat every extracted fact as equally reliable. Analyzer mode and confidence travel with evidence.

Current modes:

```text
project-semantic                  high
  C# loaded through the target project's MSBuildWorkspace compilation.

typescript-ast                    high
  Angular TypeScript structure parsed with the target project's TypeScript runtime.

loose-roslyn-fallback             medium
  C# Roslyn analysis without the target project's complete reference graph.

angular-template-regex-fallback   medium
  Conservative Angular template action/visibility extraction.

regex-fallback                    low
  Conservative text-pattern extraction, currently including React.
```

Rules:

1. Prefer the strongest available deterministic analyzer.
2. Fallback is allowed only when its provenance is explicit.
3. Downstream grouping/synthesis must not silently upgrade fallback evidence.
4. When fallback contributes materially to a workflow, generated knowledge surfaces a caveat under `Important unknowns`.
5. “Verified” means verified against an acceptance fixture/benchmark, not universal robustness across arbitrary repositories.

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

This path is used to enrich symbols and semantic call relations and to prove framework symbols such as ASP.NET Core controller base types and HTTP attributes.

If the target project cannot be loaded or a source file cannot be mapped to its project compilation, PKC retains conservative loose Roslyn evidence and tags it `loose-roslyn-fallback` rather than pretending the full target semantic context was available.

Existing syntax facts remain useful raw evidence; project-semantic enrichment adds stronger symbol context rather than replacing the whole extraction layer.

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

`FrontendScanner` may run zero, one or multiple adapters for a repository. The compiler core must not branch on a framework name.

Current canonical UI fact kinds are deliberately small:

- `ui-screen`
- `ui-route`
- `ui-action`
- `ui-api-call`

Current common relations include:

- route `renders` screen
- action `triggers-handler`
- action `triggers-api`
- API call `calls-endpoint` after backend matching

### Angular

Angular TypeScript structure currently prefers the project-local TypeScript compiler AST for components, routes, method structure and HTTP call expressions.

Angular template action/visibility extraction is still conservative fallback logic and is explicitly tagged `angular-template-regex-fallback`. This is an intentional current boundary, not hidden AST coverage.

If TypeScript/Node AST execution is unavailable, Angular can fall back to the legacy text analyzer with `regex-fallback` provenance.

### React

React currently uses the existing conservative text/regex adapter and is tagged `regex-fallback` / low confidence. React AST support is future work.

Framework identity such as `react-static` or `angular-static` remains metadata for provenance/debugging. Knowledge synthesis consumes canonical facts and relations rather than branching on framework identity.

## Boundaries

### Deterministic layer

Responsible for facts that can be proven from source syntax/semantics and their locations. It must preserve analyzer provenance and confidence.

### Inference layer

Responsible for grouping evidence into product concepts such as features, workflows and business explanations. It consumes canonical evidence, not framework-specific source constructs, and must respect evidence fidelity.

### Presentation layer

Renders the canonical knowledge model into portable Markdown/YAML. Markdown is output, not the internal source of truth for compilation. Fallback caveats are presentation-relevant because an AI/PO should know when a workflow contains lower-confidence evidence.

## Non-goal

PKC is not trying to implement every frontend framework inside the core. New UI technologies should be added as adapters that emit the same canonical UI evidence contract.
