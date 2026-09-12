# PKC — Product Knowledge Compiler

PKC turns implementation evidence into portable product knowledge that a Product Owner can attach to ChatGPT, Claude, Gemini, Copilot or another capable AI.

The core success condition is not “the scanner found many facts”. It is:

> Can an AI receive only the generated `knowledge/` pack and explain the product/system accurately, at the right abstraction level, without re-reading the source repository?

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

## Knowledge hierarchy

PKC preserves detailed evidence while presenting product knowledge in layers:

```text
knowledge/index.md
  → orient the AI around observed capabilities and boundaries

knowledge/features/**/*.md
  → capability-level rules, permissions, outcomes and important failures

knowledge/workflows/**/*.md
  → operation-level behavior, validations, state changes, side effects, flow and evidence

.pkc/facts.json
  → detailed implementation evidence and analyzer provenance
```

A transitive helper guard/loop is not automatically a product rule. Detail should be promoted only when it materially affects observable behavior, constraints, outcomes or safety.

## What works today

Current verified/development scope includes:

- C#/.NET evidence with Roslyn, preferring the target project's real `MSBuildWorkspace` compilation;
- explicit C# fallback when target-project semantic context is unavailable;
- MVC-style and Minimal API backend evidence, including permissions, guards, throws, direct/failure responses, mutations and call relations;
- Angular TypeScript structure/routes/HTTP-call shapes through the project-local TypeScript syntactic AST when available;
- Angular template actions through an explicit conservative template-regex fallback;
- React/TypeScript through an explicit conservative regex fallback;
- framework-agnostic frontend adapter boundary (`IFrontendAdapter`);
- UI action → API call → backend endpoint linkage;
- workflow Markdown, product-feature Markdown and `knowledge/index.md`;
- PokeTrade known-answer runnable regression;
- Loren pinned real-project acceptance plus Loren-main moving canary during V0.4.4.

Runtime browser confirmation is not implemented yet.

## Analyzer fidelity

Analyzer fidelity is part of the evidence and is not hidden behind a generic "static analysis" label.

Current modes include:

```text
project-semantic                  high/medium
  C# target-project MSBuildWorkspace context. Declaration node matches are high confidence;
  project-loaded-but-node-match-failed evidence is reduced to medium.

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

### Angular AST runtime precondition

The higher-fidelity Angular TypeScript path currently requires:

1. `node` available on `PATH`;
2. a project-local `node_modules/typescript/lib/typescript.js`, normally produced by the repository dependency-install step.

If unavailable, PKC records an explicit fallback rather than hiding the downgrade.

## Quickstart from source

The last accepted packaged checkpoint remains:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

Current `main` contains V0.4.4 development fixes that have not yet passed the full knowledge-readiness exit gate.

Until a public package/release is published, PKC can be packed and installed locally:

```bash
dotnet pack src/Pkc.Cli/Pkc.Cli.csproj -c Release -o ./artifacts/tool
dotnet tool install --tool-path ./.pkc-tool --add-source ./artifacts/tool RuaDen.Pkc.Tool --version 0.4.3-preview.2
./.pkc-tool/pkc build /path/to/your/repository
```

For development from source:

```bash
dotnet run --project src/Pkc.Cli/Pkc.Cli.csproj -- build <repository-path>
```

Use `scan` when only machine evidence/candidates are needed:

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

Generated knowledge is currently `code-observed`. Requirement intent and delivery history remain unknown until those evidence sources are explicitly added.

## Validation strategy

PKC deliberately uses multiple benchmark roles:

```text
PokeTrade
→ known-answer runnable regression

Loren pinned commit
→ V0.4.4 blocking real-project knowledge-readiness benchmark

Loren main
→ moving non-blocking canary

second independent real repository
→ V0.4.5 anti-overfit/generalization gate
```

A CI/grep suite is necessary but does not by itself prove knowledge readiness. V0.4.4 includes a blind review where the source is hidden and a reviewer must answer fixed product/system questions using only generated `knowledge/`.

V0.5 Azure DevOps remains locked until both the Loren gate and the second independent real-repository gate pass.

Detailed exit plan: `docs/real-project-trial.md`.

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

These are roadmap items, not automatic V0.4 work. A new analyzer capability enters V0.4.x only when a real knowledge review proves it is required for accuracy, completeness, signal-to-noise, traceability or honest uncertainty.

## Project status

`docs/status.md` is the current source of truth. `docs/real-project-trial.md` is the V0.4 exit plan and `docs/milestones.md` defines the roadmap gates.

See also:

- `docs/vision.md`
- `docs/architecture.md`
- `docs/status.md`
- `docs/handoff.md`
- `docs/milestones.md`
- `docs/real-project-trial.md`
- `docs/golden-output.md`
