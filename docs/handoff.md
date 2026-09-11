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

Current frontend adapters:

- React/TypeScript static
- Angular static

New frameworks must be adapters that emit the same canonical facts; do not add framework branches to feature/knowledge compilation.

## Current verified state

**V0.4.2 PokeTrade real-system knowledge benchmark is COMPLETE and green.**

Final verification commit: `659384ec4ba9e6e6bfbe5b381e9ac5ffd176752d`
Final CI run: `34604840944` — success.

The final CI proves:

- PKC solution builds and tests pass
- local `.NET tool` package `RuaDen.Pkc.Tool` can be packed, installed and executed
- WorkPlay React regression passes
- PokeTrade .NET 10 backend builds
- PokeTrade Angular 22 frontend builds
- live Order → WorkPlay → Delivery business smoke passes
- PKC-generated Markdown is checked against concrete PokeTrade behavior

## Important benchmark fixes now in shared PKC code

PokeTrade exposed these compiler/knowledge issues and they are fixed:

- conditions no longer pair with unrelated throws
- compound assignments retain `+=` / `-=` semantics
- object-construction/internal counter noise is suppressed from PO state changes
- ordinary business methods named `Dispatch...` are not reported as message publication side effects
- Angular object-shaped service parameters can be parsed/link actions to API calls
- Angular redirect routes no longer steal the next component's route
- lifecycle action grouping uses word-level semantics instead of substring matches
- syntactic member-assignment fallback preserves state transitions when Roslyn semantic binding is incomplete

A key verified cross-method behavior is now retained:

```text
Complete WorkPlay
  ↓
stock += purchased quantity
  ↓
FulfillWaitingOrders
  ↓
order.Status = ReadyForDelivery
```

## Commands

```bash
pkc scan <repository-path>
pkc build <repository-path>
```

Local tool quickstart from this source repository:

```bash
dotnet pack src/Pkc.Cli/Pkc.Cli.csproj -c Release -o ./artifacts/tool
dotnet tool install --tool-path ./.pkc-tool --add-source ./artifacts/tool RuaDen.Pkc.Tool --version 0.4.2-preview.1
./.pkc-tool/pkc build <repository-path>
```

Outputs:

```text
.pkc/facts.json
.pkc/feature-candidates.json
.pkc/product-features.json
knowledge/index.md
knowledge/features/**/*.md
knowledge/workflows/**/*.md
```

`knowledge/index.md` is the preferred entry point for an AI assistant.

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

The sample exists to expose compiler gaps, not to grow into a large demo product.

## Known limitations that are not blockers for a real trial

- passive GET/page-load flows can lack a complete route/screen chain
- deterministic feature grouping does not yet synthesize one cross-domain journey across Order/WorkPlay/Delivery
- Azure DevOps evidence is not implemented
- runtime UI confirmation is not implemented
- incremental compilation is not implemented
- public package/release and OSS governance are intentionally deferred

## Next engineering target — keep narrow

**External real-project trial before V0.5.**

Run PKC on one genuine repository. Review:

```text
.pkc/product-features.json
knowledge/index.md
knowledge/features/
knowledge/workflows/
```

Tag each problem as one of:

- wrong claim
- missing important behavior
- noise
- unsupported stack/pattern

Use those findings to make only evidence-backed compiler fixes.

Do **not** begin Azure DevOps work until the real-project trial has been reviewed.
