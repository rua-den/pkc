# PKC — Product Knowledge Compiler

PKC turns implementation evidence into portable product knowledge that a Product Owner can attach to ChatGPT, Claude, Gemini, Copilot or another capable AI.

PKC follows an evidence-first compiler model:

```text
source
  ↓
deterministic analyzers / adapters
  ↓
.pkc/facts.json
  ↓
workflow candidates
  ↓
canonical knowledge
  ↓
portable Markdown
```

Important claims are grounded in source evidence. PKC does not silently invent UI behavior, delivery history or requirements that were not analyzed.

## What works today

Current verified scope:

- C#/.NET evidence with Roslyn, preferring the target project's real `MSBuildWorkspace` compilation;
- explicit C# fallback when the target project cannot be loaded;
- behavior evidence such as endpoints, permissions, guards, throws, mutations and call relations;
- Angular TypeScript structure/routes/HTTP-call shapes through the project-local TypeScript syntactic AST when available;
- Angular template actions through an explicit conservative template-regex fallback;
- React/TypeScript through an explicit conservative regex fallback;
- framework-agnostic frontend adapter boundary (`IFrontendAdapter`);
- UI action → API call → backend endpoint linkage;
- workflow Markdown, product-feature Markdown and `knowledge/index.md`;
- real-system validation with the runnable .NET 10 + Angular 22 PokeTrade benchmark.

Runtime browser confirmation is not implemented yet.

## Analyzer fidelity

Analyzer fidelity is part of the evidence and is not hidden behind a generic "static analysis" label.

Current modes include:

```text
project-semantic                  high/medium
  C# target-project MSBuildWorkspace context. Declaration node matches are high confidence;
  if the project loads but the original fact cannot be matched back to a syntax node, the fact is
  explicitly marked semanticNodeMatch=failed and confidence is reduced to medium.

typescript-ast-syntactic          high/medium
  Angular uses the target repo's local TypeScript parser via ts.createSourceFile.
  Structural ui-screen/ui-route evidence is high confidence.
  ui-api-call evidence is medium because receiver type is not checked by a TypeChecker.

loose-roslyn-fallback             medium
  C# source analyzed without the full target-project reference graph.

angular-template-regex-fallback   medium
  Angular template action/visibility extraction.

regex-fallback                    low
  conservative text-pattern fallback, currently including React and Angular when the TS AST path is unavailable.
```

Angular `ui-api-call` facts additionally carry:

```text
typescriptSemanticContext: syntax-only-no-type-checker
httpReceiverResolution: syntactic-unverified
```

This is intentional: a property-access call named `.get/.post/.put/.patch/.delete` can be detected syntactically, but PKC does not currently prove that its receiver is Angular `HttpClient`.

### Angular AST runtime precondition

The higher-fidelity Angular TypeScript path currently requires:

1. `node` available on `PATH`;
2. a project-local `node_modules/typescript/lib/typescript.js` in or above the Angular project directory, normally produced by `npm install`, `npm ci`, `pnpm install`, or the repository's equivalent dependency-install step.

If those prerequisites are unavailable, PKC does not hide the downgrade: the Angular adapter falls back to `regex-fallback` / `low` and records the fallback reason.

Facts carry `analysisMode` and `analysisConfidence`. When fallback evidence contributes to a workflow, generated Markdown surfaces that limitation under `Important unknowns` instead of silently upgrading it to high-confidence evidence.

The PokeTrade benchmark proves these analyzer paths and generated knowledge against that benchmark. It does **not** claim that arbitrary external repositories are already robustly supported; that is the purpose of the next external real-project trial.

## Quickstart from source

Until a public NuGet package/release is published, PKC can be packed and installed locally as a .NET tool:

```bash
dotnet pack src/Pkc.Cli/Pkc.Cli.csproj -c Release -o ./artifacts/tool
dotnet tool install --tool-path ./.pkc-tool --add-source ./artifacts/tool RuaDen.Pkc.Tool --version 0.4.3-preview.2
./.pkc-tool/pkc build /path/to/your/repository
```

For normal development inside this repository:

```bash
dotnet run --project src/Pkc.Cli/Pkc.Cli.csproj -- build <repository-path>
```

Use `scan` when you only want machine evidence/candidates:

```bash
pkc scan <repository-path>
```

## Output

`pkc build <repository-path>` produces:

```text
.pkc/
  facts.json
  feature-candidates.json
  product-features.json

knowledge/
  index.md
  features/
    <area>/
      <feature>.md
  workflows/
    <area>/
      <workflow>.md
```

Generated knowledge is currently `code-observed`. Requirement intent and delivery history remain unknown until those sources are explicitly added.

## Real-system benchmark

`samples/PokeTradeSystem` is an independently runnable benchmark:

- backend: .NET 10;
- frontend: Angular 22;
- business flow: Pokémon card order → insufficient stock → PurchaseStock WorkPlay → inventory replenishment → delivery → delivered order.

CI builds both applications, executes representative runtime branches, runs PKC against the same source tree, and asserts analyzer provenance plus generated product knowledge. The benchmark deliberately contains TypeScript formatting/property styles that the previous regex-only Angular scanner would miss.

## Planned, not implemented yet

- TypeScript `Program` / `TypeChecker` semantic analysis for Angular receiver/type resolution;
- React AST-backed analysis;
- full Angular template AST/compiler analysis;
- ASP.NET MVC / Razor Pages, Blazor and Vue frontend adapters;
- Azure DevOps Epic / Feature / PBI / Sprint evidence;
- incremental compilation and PR knowledge diffs;
- runtime UI confirmation;
- product gap/drift analysis;
- optional LLM-assisted synthesis where deterministic grouping is insufficient.

Framework expansion must happen through adapters; the knowledge compiler core should remain framework-agnostic.

## Project status

`docs/status.md` is the source of truth for the current verified milestone. `docs/milestones.md` describes the roadmap.

See also:

- `docs/vision.md`
- `docs/architecture.md`
- `docs/status.md`
- `docs/handoff.md`
- `docs/milestones.md`
- `docs/golden-output.md`
