# PKC Handoff

Use this file when continuing PKC in another chat/session.

## Product idea

PKC means **Product/System Knowledge Compiler**.

The target user is a Product Owner. A PO should be able to take PKC's generated `knowledge/` folder and attach it to any capable AI — ChatGPT, Claude, Gemini, Copilot, etc. — then ask questions about the product without making that AI re-read or grep the source repository.

Example target question:

> How do I change the status of a WorkPlay? What should I be careful about?

The mature knowledge pack should combine:

- how users perform actions in the UI
- business behavior and status transitions
- validations and permissions
- side effects and integrations
- Feature/PBI/Sprint history
- current delivery state
- gaps and improvement opportunities
- evidence supporting important claims

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

## Frontend architecture decision

Frontend technology must stay outside the compiler core.

```text
React / Angular / future MVC-Razor / Blazor / Vue
                    ↓
              IFrontendAdapter
                    ↓
          canonical `ui-*` evidence
                    ↓
             FrontendScanner
                    ↓
          generic relation linker
                    ↓
          shared evidence pipeline
```

Current canonical frontend facts:

- `ui-screen`
- `ui-route`
- `ui-action`
- `ui-api-call`

Current common relations:

- `renders`
- `triggers-handler`
- `triggers-api`
- `calls-endpoint`

Framework names are provenance metadata only. Feature/workflow synthesis must not branch on React, Angular, Blazor, MVC, etc.

## Current verified state

**V0.4.1 frontend adapter architecture is complete and green.**

Implementation: `e6fa94a1c6e55f6019fcad463493ac07483d64f1`
Test fix: `8ace3741a61f74096287a4cd33391ebb8ab1071a`
Frontend-adapter CI: `34598118760` — success.

An external review then triggered a narrow pre-V0.5 hardening pass. CI run `34602582122` is green and verifies:

- PKC solution builds and tests pass
- WorkPlay React end-to-end knowledge regression passes
- PokeTrade .NET 10 backend builds
- PokeTrade Angular 22 frontend builds
- PokeTrade Order → WorkPlay → Delivery business smoke passes
- PKC compiles PokeTrade knowledge through the framework-agnostic `FrontendScanner`
- `Pkc.Cli` packs as `.NET tool` package `RuaDen.Pkc.Tool` version `0.4.2-preview.1`
- CI installs that local `.nupkg` with `dotnet tool install` and successfully runs `pkc scan`

Commands:

```bash
pkc scan <repository-path>
pkc build <repository-path>
```

Current source-based local-tool quickstart:

```bash
dotnet pack src/Pkc.Cli/Pkc.Cli.csproj -c Release -o ./artifacts/tool
dotnet tool install --tool-path ./.pkc-tool --add-source ./artifacts/tool RuaDen.Pkc.Tool --version 0.4.2-preview.1
./.pkc-tool/pkc build <repository-path>
```

Public NuGet/release publishing is intentionally deferred until package/version/license decisions are made.

Outputs:

```text
.pkc/facts.json
.pkc/feature-candidates.json
.pkc/product-features.json
knowledge/index.md
knowledge/features/**/*.md
knowledge/workflows/**/*.md
```

`docs/status.md` is the current-state source of truth. `docs/golden-output.md` documents generated-output/golden maintenance policy.

## Runnable benchmark

`samples/PokeTradeSystem` is intentionally a small real application used to compare generated knowledge with actual behavior.

Business flow:

```text
Customer places card order
  ↓
stock sufficient? ── yes → reserve → ReadyForDelivery → Delivery
  ↓ no
AwaitingStock
  ↓
auto-create PurchaseStock WorkPlay
  ↓
Staff Start → Complete with purchased quantity
  ↓
inventory increases + waiting orders rechecked
  ↓
ReadyForDelivery → Dispatch → Delivered
```

This benchmark exists to expose compiler gaps, not to become a large demo product.

## Supported frontend adapters today

- React/TypeScript static
- Angular static

Do not implement MVC/Razor, Blazor or Vue inside core. When needed, add one adapter that emits the same canonical facts.

## Deferred review items

Do not broaden scope just because they were mentioned in review. Still deferred:

- public NuGet.org release / GitHub release
- LICENSE choice
- CONTRIBUTING and GitHub repository topics
- extra language/framework adapters
- Azure DevOps, Playwright/runtime UI and incremental compilation

## Next engineering target — keep narrow

**V0.4.2 PokeTrade knowledge review.**

Generate/review PokeTrade Markdown against the runnable application and fix only incorrect or missing knowledge exposed by the benchmark. Do not jump to Azure DevOps, Playwright, incremental builds or more frontend frameworks before this benchmark is clean enough for an external real-project trial.
