# PKC Status

Last updated: 2026-09-11

## Current milestone

**V0.4.1 frontend adapter architecture — COMPLETE**

Implementation commit: `e6fa94a1c6e55f6019fcad463493ac07483d64f1`
Test-fix commit: `8ace3741a61f74096287a4cd33391ebb8ab1071a`
GitHub Actions run: `34598118760` — SUCCESS

Verified:

- `IFrontendAdapter` is the framework adapter contract
- `FrontendScanner` owns 0..N adapter orchestration
- CLI no longer branches on Angular vs React
- React and Angular emit the same canonical `ui-screen`, `ui-route`, `ui-action`, `ui-api-call` facts
- generic frontend linker creates conservative `ui-action -> triggers-api -> ui-api-call` relations
- Angular component action can link to an HTTP call in a separate service file
- `CrossStackFeatureCandidateBuilder` consumes canonical frontend relations instead of framework-specific source structure
- WorkPlay React regression remains green
- PokeTrade .NET 10 backend builds
- PokeTrade Angular 22 frontend builds
- PokeTrade Order → WorkPlay → Delivery smoke flow passes
- PKC successfully compiles PokeTrade product knowledge after the adapter refactor

## Current commands

```bash
pkc scan <repository-path>
pkc build <repository-path>
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

## Frontend support

Implemented adapters:

- React/TypeScript static adapter
- Angular static adapter

Architecture-ready but not implemented yet:

- ASP.NET MVC / Razor Pages
- Blazor
- Vue
- other UI stacks

Those should be added only as `IFrontendAdapter` implementations. Core knowledge compilation must not add framework branches.

## Real-system benchmark

`samples/PokeTradeSystem` is the current benchmark:

- backend: .NET 10
- frontend: Angular 22
- business: Pokemon card catalog, customer orders, purchase-stock WorkPlay tasks, inventory replenishment and delivery lifecycle
- independently runnable for behavior-vs-knowledge comparison

CI validates the live API business flow and then runs PKC against the same source tree.

## Countdown to AI-testable Markdown

**0 steps remaining.**

Both the WorkPlay sample and PokeTrade benchmark can generate portable Markdown today.

## Next target — narrow

**V0.4.2 PokeTrade knowledge review.**

Review generated PokeTrade Markdown against the runnable app and fix only correctness/coverage gaps exposed by that benchmark. Do not add Azure DevOps, Playwright, MVC/Blazor/Vue adapters or incremental compilation until this benchmark is clean enough to take to a real external project.
