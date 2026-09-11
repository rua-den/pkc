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

**V0.4.2 PokeTrade benchmark is REOPENED after a full-source review.**

The earlier verification commit `659384ec4ba9e6e6bfbe5b381e9ac5ffd176752d` and CI run `34604840944` are green, but they prove a principal smoke/business slice rather than full source-to-knowledge coverage.

Still verified:

- PKC solution builds/tests pass
- local `.NET tool` packaging works
- WorkPlay React regression passes
- PokeTrade .NET 10 backend builds
- PokeTrade Angular 22 frontend builds
- principal live Order → WorkPlay → Delivery smoke passes
- previously fixed compiler correctness issues remain fixed

Do **not** tell the user PKC is ready for an external real repository yet.

## Full PokeTrade source reviewed

The mini project was reviewed file-by-file across:

### Backend

```text
Program.cs
Domain.cs
PokeTradeStore.cs
Controllers/CardsController.cs
Controllers/OrdersController.cs
Controllers/WorkPlaysController.cs
Controllers/DeliveriesController.cs
PokeTrade.Api.csproj
```

### Frontend

```text
src/main.ts
src/index.html
src/styles.css
src/app/app.config.ts
src/app/app.component.ts
src/app/app.routes.ts
src/app/api.service.ts
src/app/models.ts
src/app/pages/catalog.component.ts
src/app/pages/orders.component.ts
src/app/pages/workplays.component.ts
src/app/pages/deliveries.component.ts
package.json
angular.json
proxy.conf.json
```

## Important full-review findings

### High priority blockers

1. **Business-important object construction is being suppressed.**

`CreatePurchaseWorkPlays` constructs WorkPlays using:

```text
QuantityToBuy = shortage + card.ReorderLevel
Type = PurchaseStock
Reason = shortage explanation
```

Current PO-facing object-initializer filtering removes these semantics together with actual initializer noise.

2. **Computed business properties are not represented.**

`Order.Total` is:

```text
Lines.Sum(line => line.Quantity * line.UnitPrice)
```

Current property evidence does not preserve that expression.

3. **Waiting-order fulfillment semantics are incomplete.**

`CompleteWorkPlay` calls `FulfillWaitingOrders`, which scans all `AwaitingStock` orders ordered by ID, reserves any now-fulfillable order, sets it to `ReadyForDelivery`, and creates a delivery. Some transitive mutations survive, but the loop/collection semantics do not.

### Medium priority gaps

4. Angular action evidence records permission guards but not the surrounding status guard. Example:

```text
ManageWorkPlay + Open       → Start visible
ManageWorkPlay + InProgress → Complete visible
ManageDelivery + Pending    → Dispatch visible
ManageDelivery + Dispatched → Mark delivered visible
```

5. Two-hop read flows are incomplete. Example:

```text
Orders Refresh
  → component reload()
  → ApiService.getOrders()
  → GET /api/orders
```

Current action/API linking is strongest when component handler and service method names match.

6. Authorization implementation semantics are not analyzed. The sample's `ManageWorkPlay` and `ManageDelivery` policies intentionally use always-true assertions, while generated knowledge currently reports only the policy/guard names.

Additional lower-priority observations:

- the backend supports multiple order lines while the current Catalog UI creates one line at a time;
- WorkPlay completion accepts any positive purchased quantity, while the UI defaults the value to `QuantityToBuy`;
- API error status mapping (400/409/404) is not currently part of product knowledge;
- `/health` is a minimal API endpoint and is not part of the MVC endpoint scanner; this is operational rather than core product behavior.

## Runnable benchmark business flow

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
inventory increases + ALL waiting orders are re-evaluated
  ↓
fulfillable orders reserve stock → ReadyForDelivery → Delivery
  ↓
Dispatch → order Shipped
  ↓
Mark delivered → order Delivered
```

## Next engineering target — keep narrow

Finish V0.4.2 before any external trial.

Order of work:

1. preserve business-relevant object construction while still filtering implementation noise;
2. extract computed property/domain expressions such as `Order.Total`;
3. add deterministic collection/loop evidence needed for `FulfillWaitingOrders`;
4. capture Angular status predicates around actions;
5. improve two-hop component → service → HTTP linking for real read/refresh patterns;
6. expand PokeTrade CI to cover representative branches and compare generated knowledge;
7. review all generated PokeTrade Markdown again.

Only after that should V0.4.3 external real-project trial begin.

Do **not** start V0.5 Azure DevOps yet.
