# PokeTrade System

A runnable PKC validation system for buying and selling Pokémon cards.

## Business flow

1. A customer creates an order for one or more cards.
2. If all requested stock is available, inventory is reserved immediately, the order becomes `ReadyForDelivery`, and a pending delivery is created.
3. If stock is missing, the order becomes `AwaitingStock` and the system creates `PurchaseStock` WorkPlays for staff.
4. Staff starts a WorkPlay, purchases stock, and completes the WorkPlay with the purchased quantity.
5. Completing a WorkPlay replenishes inventory and re-evaluates waiting orders. Any order that can now be fulfilled is reserved and moved to `ReadyForDelivery` with a delivery created.
6. Staff dispatches a delivery, moving the order to `Shipped`.
7. Staff marks the delivery delivered, moving the order to `Delivered`.

## Stack

- Backend: ASP.NET Core / .NET 10
- Frontend: Angular 22
- Storage: in-memory demo store seeded at startup

## Run locally

### Backend

```bash
cd backend/PokeTrade.Api
dotnet run
```

API: `http://localhost:5080`

### Frontend

Requires Node.js 24.15+.

```bash
cd frontend
npm install
npm start
```

Web: `http://localhost:4200`

Angular dev server proxies `/api` to `http://localhost:5080`.

## PKC validation

From the PKC repository root:

```bash
dotnet run --project src/Pkc.Cli/Pkc.Cli.csproj -- build samples/PokeTradeSystem
```

Inspect:

```text
samples/PokeTradeSystem/.pkc/
samples/PokeTradeSystem/knowledge/index.md
samples/PokeTradeSystem/knowledge/features/
samples/PokeTradeSystem/knowledge/workflows/
```

Compare the generated knowledge with the running application to identify scanner/synthesis gaps.
