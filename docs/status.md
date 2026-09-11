# PKC Status

Last updated: 2026-09-11

## Current milestone

**V0.4.2 PokeTrade real-system knowledge benchmark — COMPLETE**

Final verification commit: `659384ec4ba9e6e6bfbe5b381e9ac5ffd176752d`
GitHub Actions run: `34604840944` — SUCCESS

Verified end to end:

- PKC solution builds and all tests pass
- `RuaDen.Pkc.Tool` packs, installs from a local `.nupkg`, and runs as `pkc`
- WorkPlay React regression remains green
- PokeTrade .NET 10 backend builds
- PokeTrade Angular 22 frontend builds
- live Order → WorkPlay → Delivery business smoke passes
- PKC compiles the same PokeTrade source into portable Markdown
- CI verifies the generated Markdown against the live business behavior

## Correctness gaps fixed by the PokeTrade benchmark

The benchmark exposed real compiler/knowledge bugs rather than sample-specific issues. V0.4.2 fixed them in shared PKC code:

- guard conditions are paired only with their own contained throw instead of every throw in the method
- compound mutations preserve semantics (`+=`, `-=`, increment/decrement) instead of being rendered as simple assignment
- object-initializer and internal `_next...` counter noise is removed from PO-facing state changes
- `DispatchDelivery` is no longer presented as a message-publication side effect
- Angular service methods with object-shaped parameters can link component actions to HTTP calls
- Angular redirect routes no longer swallow the following component route (`/catalog` is preserved)
- lifecycle actions such as Start/Complete/Dispatch/Delivered group into Status Management while reads such as Get Deliveries stay Discovery
- member assignments remain available as state evidence when semantic binding is incomplete, allowing transitive transitions such as `order.Status = ReadyForDelivery` to survive analysis

Final CI explicitly verifies, among other things:

```text
/catalog → Place order → POST /api/orders
invalid order input → matching validation message
insufficient stock → OrderStatus.AwaitingStock
reserve stock → -= quantity
Complete WorkPlay → += purchased quantity
Complete WorkPlay → waiting Order becomes ReadyForDelivery
Dispatch → Pending/ReadyForDelivery guards
no false DispatchDelivery publication side effect
Deliveries Discovery != Deliveries Status Management
```

## Current commands

```bash
pkc scan <repository-path>
pkc build <repository-path>
```

From source, PKC can be packed and installed locally:

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

## Frontend support today

Implemented adapters:

- React/TypeScript static
- Angular static

Architecture-ready but not implemented yet:

- ASP.NET MVC / Razor Pages
- Blazor
- Vue
- other UI stacks

New UI technologies must be added as `IFrontendAdapter` implementations; compiler/knowledge core must remain framework-agnostic.

## Known non-blocking limitations

These are deliberately deferred until evidence from a real external repository says they matter:

- passive page-load/read flows may have an API-call fact without a complete screen/user-path chain
- cross-domain business journeys such as Order → WorkPlay → Delivery are still represented as deterministic area/workflow knowledge rather than one inferred product journey
- Azure DevOps intent/history is not implemented
- runtime UI confirmation is not implemented
- incremental compilation is not implemented
- public NuGet.org/GitHub release, LICENSE and wider OSS packaging remain deferred

## Countdown to external real-project trial

**0 steps remaining.**

PKC is ready to be run against a real external repository using the local .NET tool package.

## Next target — narrow

**External real-project trial.**

Run PKC on one genuine repository and review only:

1. `.pkc/product-features.json`
2. `knowledge/index.md`
3. the generated `knowledge/features/` and `knowledge/workflows/`

Classify findings as **wrong claim**, **missing important behavior**, **noise**, or **unsupported stack/pattern**. Fix compiler abstractions only when the real repository demonstrates the need.

Do not start V0.5 Azure DevOps until this trial is reviewed.
