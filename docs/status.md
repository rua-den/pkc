# PKC Status

Last updated: 2026-09-11

## Current milestone

**V0.4.2 PokeTrade real-system knowledge benchmark — COMPLETE**

Final acceptance commit: `b29bb0d6d7c56f0676dd0c9eddbe9faf8c9ddec7`

Final GitHub Actions run: `34620984359`

Both jobs are green:

- `test`: solution build, unit/regression tests, local `.NET tool` pack/install, WorkPlay end-to-end knowledge build
- `poketrade-real-system`: .NET 10 backend build, Angular 22 frontend build, runtime branch acceptance, PKC product-knowledge contract

PKC is now cleared for the **V0.4.3 external real-project trial**.

## What V0.4.2 now proves

The PokeTrade benchmark is no longer only a happy-path smoke test. CI now locks representative runtime behavior and matching generated knowledge for:

```text
stock sufficient → reserve immediately → ReadyForDelivery + Delivery
stock insufficient → AwaitingStock + PurchaseStock WorkPlay
WorkPlay QuantityToBuy = shortage + reorder level
invalid order validation → 400
invalid WorkPlay transition / quantity → 409
missing WorkPlay → 404
Complete WorkPlay → inventory increase + waiting-order re-evaluation
waiting orders processed in ascending order ID
Delivery Pending → Dispatched → Delivered
invalid Delivery transition → 409
missing Delivery → 404
```

The full PokeTrade backend/frontend source was reviewed against generated Markdown, and the blocking gaps found during that review were fixed and locked by regression/acceptance assertions.

## Compiler capabilities verified by PokeTrade

- deterministic C# fact extraction
- semantic call relations with conservative fallback
- guards / conditions / throws
- assignments and compound state mutations (`=`, `+=`, `-=`)
- computed domain properties such as `Order.Total`
- business-relevant object construction such as `PurchaseStock` WorkPlay creation
- collection / loop evidence needed for waiting-order fulfillment
- controller/action routes
- authorization policy names and observed policy definitions
- Angular routes, screens and actions
- UI permission + status visibility guards
- Angular component → helper/reload → service → HTTP linking
- passive page-load read flows such as route → `ngOnInit` → API
- controller error-response semantics such as 400 / 404 / 409 mappings
- product feature grouping and workflow Markdown generation
- portable `knowledge/index.md` + feature/workflow documents

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

## Known boundaries — not blockers for V0.4.3

These are current scope limits, not reasons to delay the external trial:

- frontend static adapters currently cover React/TypeScript and Angular; other frameworks are not implemented yet
- Azure DevOps intent/history is not compiled yet
- runtime browser/UI exploration is not implemented yet
- product intent is still separate from code-observed implementation
- analyzers are intentionally conservative and can leave unknowns instead of guessing
- minimal API endpoints such as `/health` are outside the current MVC endpoint path and are not core product-knowledge coverage

## Next target

**V0.4.3 — external real-project trial.**

Run PKC against a real repository and use failures/missing knowledge from that project as the next acceptance signal. Do not widen scope preemptively.

Only after the real-project trial is understood should V0.5 Azure DevOps/incremental work be reconsidered.
