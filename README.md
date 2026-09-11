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

- C#/.NET backend evidence via Roslyn;
- behavior evidence such as endpoints, permissions, guards, throws, mutations and call relations;
- React/TypeScript static UI adapter;
- Angular static UI adapter;
- framework-agnostic frontend adapter boundary (`IFrontendAdapter`);
- UI action → API call → backend endpoint linkage;
- workflow Markdown;
- product-feature Markdown;
- `knowledge/index.md` entry point;
- real-system validation with the runnable .NET 10 + Angular 22 PokeTrade sample.

Current frontend adapters are deliberately static analyzers. Runtime browser confirmation is not implemented yet.

## Quickstart from source

Until a public NuGet package/release is published, PKC can be packed and installed locally as a .NET tool:

```bash
dotnet pack src/Pkc.Cli/Pkc.Cli.csproj -c Release -o ./artifacts/tool
dotnet tool install --tool-path ./.pkc-tool --add-source ./artifacts/tool RuaDen.Pkc.Tool --version 0.4.2-preview.1
./.pkc-tool/pkc build /path/to/your/repository
```

For normal development inside this repository you can also run:

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

The generated Markdown is currently marked from code-observed/static evidence. Requirement intent and delivery history remain unknown until those sources are explicitly added.

## Real-system benchmark

`samples/PokeTradeSystem` is an independently runnable benchmark:

- backend: .NET 10;
- frontend: Angular 22;
- business flow: Pokemon card order → insufficient stock → PurchaseStock WorkPlay → inventory replenishment → delivery → delivered order.

CI builds the backend and frontend, executes the business flow through the live API, and then runs PKC against the same source tree.

## Planned, not implemented yet

- ASP.NET MVC / Razor Pages adapters;
- Blazor and Vue adapters;
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
