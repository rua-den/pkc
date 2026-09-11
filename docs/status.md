# PKC Status

Last updated: 2026-09-11

## Current milestone

**V0.4.2 PokeTrade real-system knowledge benchmark — REOPENED**

The previous verification commit `659384ec4ba9e6e6bfbe5b381e9ac5ffd176752d` and GitHub Actions run `34604840944` are still green, but they verified a smoke/business slice, not a full source-to-knowledge review.

A subsequent file-by-file review of the entire PokeTrade backend and frontend source found additional knowledge-coverage gaps. Therefore PKC is **not yet cleared for the external real-project trial**.

## What is still verified

- PKC solution builds and all current tests pass
- `RuaDen.Pkc.Tool` packs, installs from a local `.nupkg`, and runs as `pkc`
- WorkPlay React regression remains green
- PokeTrade .NET 10 backend builds
- PokeTrade Angular 22 frontend builds
- live happy-path Order → WorkPlay → Delivery smoke passes
- generated Markdown correctly covers the already-asserted validations, permissions, state mutations and UI actions
- previously fixed correctness bugs remain fixed: guard/throw pairing, compound assignments, false publication classification, redirect route parsing, lifecycle grouping, Angular object-shaped service signatures, and syntactic member-mutation fallback

## Full-source review findings that block external trial

### High priority

1. **Business-important object construction is over-filtered as noise.**
   `CreatePurchaseWorkPlays` creates a `PurchaseStock` WorkPlay with:

   ```text
   QuantityToBuy = shortage + card.ReorderLevel
   Type = PurchaseStock
   Reason = Order shortage explanation
   ```

   The current PO-facing mutation filter suppresses object-initializer assignments broadly, so these business semantics are not represented strongly enough in generated knowledge.

2. **Computed domain rule is missing.**
   `Order.Total` is defined as:

   ```text
   Sum(line.Quantity * line.UnitPrice)
   ```

   Current C# property evidence records the property/type but not expression-bodied computed semantics, so the product rule is absent from knowledge.

3. **Waiting-order fulfillment semantics are under-described.**
   Completing one WorkPlay calls `FulfillWaitingOrders`, which iterates all `AwaitingStock` orders in ascending order ID, reserves any now-fulfillable order, moves it to `ReadyForDelivery`, and creates a delivery. Current knowledge retains some transitive mutations/calls but does not represent the quantified/ordered loop semantics clearly enough.

### Medium priority

4. **UI status visibility guards are missing as UI evidence.**
   WorkPlay buttons depend on both permission and status (`Open` / `InProgress`); Delivery buttons depend on permission and status (`Pending` / `Dispatched`). The Angular adapter currently records the permission guard but not the surrounding status predicate.

5. **Read/refresh UI chains are incomplete.**
   Example: Orders `Refresh` calls component `reload()`, which calls `ApiService.getOrders()`, which performs `GET /api/orders`. Current static linking handles same-name handler/service patterns better than this two-hop differently-named chain.

6. **Configured policy semantics are not analyzed.**
   The sample registers `ManageWorkPlay` and `ManageDelivery` with always-true assertions for demo purposes. PKC reports the policy attributes/guards but not the effective policy implementation. This is not a wrong claim, but it is incomplete authorization context.

## Benchmark quality gap

The live CI smoke currently proves one principal business path. Before V0.4.2 can close again, the benchmark must also lock representative branches such as:

```text
stock sufficient → reserve immediately → ReadyForDelivery + Delivery
stock insufficient → AwaitingStock + PurchaseStock WorkPlay
WorkPlay QuantityToBuy uses shortage + reorder level
invalid order validations
invalid WorkPlay transitions / purchased quantity
Complete WorkPlay with replenishment → waiting-order re-evaluation
Delivery Pending → Dispatched → Delivered
missing IDs / invalid transitions return the expected API behavior
```

The goal is not exhaustive application testing. The goal is enough behavioral coverage to compare source, running behavior and generated knowledge without declaring readiness from one happy path.

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

## Next target — narrow

**Finish V0.4.2 full-source benchmark.**

Work in this order:

1. preserve business-important object-construction facts without reintroducing initializer noise;
2. extract useful computed property/domain-expression evidence such as `Order.Total`;
3. represent important loop/collection business semantics needed for `FulfillWaitingOrders`;
4. improve Angular UI status-condition and two-hop component → service → HTTP linking where the PokeTrade sample proves the need;
5. expand PokeTrade CI with branch-level behavioral assertions and matching knowledge assertions;
6. review all generated PokeTrade feature/workflow Markdown again.

Only after that review is clean should V0.4.3 external real-project trial begin.

Do not start V0.5 Azure DevOps yet.
