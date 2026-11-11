# WH-001 — «Склад тканин» as its own module (Warehouse), stage 1

**Status:** accepted by the product owner 2026-11-11; stage 1 implemented 2026-11-11. **Date:** 2026-11-11. **Module:** new module `Warehouse`, schema `warehouse`, own migration history `warehouse.__EFMigrationsHistory` ([module rules](../../../standards/module-rules.md)). Takes over [PRD-004](../../production/004-fabric-stock/README.md) from «Швейний цех».

## Why

The owner decided on 2026-11-11 that «Склад тканин» is a separate bounded context with its own database schema, not a part of «Швейний цех», and that it is rebuilt to the specification «Brevi — склад тканин» v1.0 of the old «Цех» (app version 133). The first step moves what exists without changing behaviour; the next stages add the rules of that specification.

## Owner decisions (2026-11-11)

- A new module with its own schema and clean architecture; the whole current stock moves there.
- The stock stays linked to «Швейний цех»: orders and their fabric plan come from Production, issues go to order items.
- In the Admin it is a new menu block «Склад тканин» right under «Швейний цех», one menu item per page.
- At the data transfer (stage 6) the stock is replaced by the full export of the old «Цех» v133.

## Stages (owner plan, one backend + Admin PR pair each)

1. **Module** (this document): code and data move from Production; same API behaviour under `/api/warehouse/...`; menu block.
2. [WH-002](../002-ledger-rules/README.md) ledger rules of the specification: hundredths of a metre, operation date apart from the record time, idempotent `operationId`, fabric alias groups (FAB-023/067, FAB-078/077), colour normalisation, negative balance on issue, «Уточнити» to a target balance with a reason; pages «Рух тканини», «Залишки», «Початкові залишки» with Excel.
3. Edit, void and restore of movements with revisions.
4. Issue by actual cutting: issued quantity, plan key, layers, replacement and additional lines, preview, plan versions.
5. Reports «План і факт», «Бронь і що замовити», «Вартість складу» (Admin and Director), «Фурнітура до замовлення».
6. Transfer of the v133 data with a dry run, an import journal and the reconciliation; needs the full untruncated export.

## Rules of stage 1

- The domain (`FabricStock`, `FabricMovement`, `FabricIssue`) and every use case move unchanged into Warehouse. Responses and inputs keep their shapes and operation ids; only the paths change from `/api/production/...` to `/api/warehouse/...`.
- Warehouse references no other module. What it needs comes through ports implemented in the host:
  - `IWarehouseProductionReader` — items of «Нове» orders with their plan by colour (Production computes it from the norms), one item with its order version and whether it is open, orders moved from the old «Цех», and the line «Тканина» in the item history.
  - `IWarehouseReferenceReader` — «Тканини» with price, active «Постачальники», active «Кольори».
  - `IWarehouseStaffReader` — the person's name for «хто змінив».
- Production keeps the plan by colour (`FabricPlanParts`, `PlanColor`) and asks the stock for «Звіти цеху» through its port `IProductionStockReader` (issues of a period, balances at a date), implemented by the host over Warehouse `StockFacts`.
- `OrderId` and `ItemId` of an issue point to Production by value; the old foreign keys to `production.Orders` and `production.Items` are gone. An issue whose order was deleted shows «Завдання не знайдено».
- An issue is saved in the stock first and then written to the order history; the two are separate transactions now. If the history write fails, the issue stays and the history misses one line.
- The «Права доступу» key stays `fabric-stock`, so saved rights keep working; its group becomes «Склад тканин». The «Тканина» tab of an order item keeps the `production-orders` policy.

## Data move

- `warehouse.InitialWarehouse` creates the four tables and, when `production."FabricStocks"` exists, copies FabricStocks, FabricMovements, FabricIssues and FabricIssueLines with their ids, moves the identity sequences past the copied ids and drops the production tables in the same transaction.
- `production.MoveFabricStockToWarehouse` drops the production tables only when `warehouse."FabricStocks"` exists, so the rows are never dropped before they are copied, whichever migrator runs first. Host.Api and Host.Seed register Warehouse before Production, so the normal order is copy, then nothing to drop.
- Checked 2026-11-11 on PostgreSQL 16 in three orders: existing data (copy, ids and reversal links kept, the next id continues), new database Production first, new database Warehouse first. All end with only the `warehouse` tables.

## Contract

[warehouse/fabric-stock.openapi.yaml](../../../contracts/warehouse/fabric-stock.openapi.yaml): `/api/warehouse/fabric-stock` (list, choices, line, movements, correction, reversal, consumption, import) and `/api/warehouse/orders/{id}/items/{itemId}/fabric`, `.../fabric-issues`, `/api/warehouse/orders/{id}/fabric-issues/{issueId}/reverse`. Breaking for the Admin: the paths change in the same release as brevi-web.

## File map

```text
src/Modules/Warehouse/
  Warehouse.Domain/…/Stock/            FabricStock, FabricMovement, FabricIssue, StockColor; WarehouseText
  Warehouse.Application/…/
    Abstractions/                       IWarehouseDbContext, IWarehouseProductionReader, IWarehouseReferenceReader, IWarehouseStaffReader
    Access/WarehouseActor.cs            who moves the stock, sees the value, imports
    Colors/WarehouseColors.cs           colours of «Кольори» for a new line
    Contracts/FabricStockContracts.cs
    Stock/…                             use cases moved from Production; Facts/StockFacts.cs for «Звіти цеху»
  Warehouse.Infrastructure/…/           WarehouseDbContext (schema warehouse), configurations, InitialWarehouse, DI
  Warehouse.Api/…/                      FabricStockController, OrderItemFabricController, WarehouseAccess
src/Modules/Production/
  Production.Domain/…/FabricPlans/      FabricPlanParts, PlanColor (stay with the norms)
  Production.Application/…/FabricPlans/ProductionFabricPlans.cs   plans, legacy orders, item history for the stock
  Production.Application/…/Abstractions/IProductionStockReader.cs  issues and balances for «Звіти цеху»
src/Bootstrapper/Host.Api/
  Warehouse/                            WarehouseProductionReader, WarehouseReferenceReader, WarehouseStaffReader
  Production/ProductionStockReader.cs
```

## Tests

- `tests/UnitTests/Warehouse/WarehouseStockHandlerTests.cs` — the PRD-004 scenarios on the new module with the real Production plan; colours of a new line; stock facts for the report.
- `tests/UnitTests/Warehouse/FabricStockContractTests.cs` — DTOs and operations against the moved contract.
- `tests/UnitTests/Production/WorkshopReportHandlerTests.cs` — the report reads issues and stock from Warehouse through the port.
- `tests/ArchitectureTests/WarehouseDependencyTests.cs` — layers and schema; every other module list includes Warehouse.

## Structure audit (stage 1)

- Four layers, Domain on BuildingBlocks only, Application without Infrastructure or Api, Api without Infrastructure: checked by the architecture tests.
- No cross-module project reference or foreign key; the host owns the adapters.
- Production no longer has stock code, DbSets, configuration or the supplier port; its remaining stock terms are the plan by colour, which belongs to the norms.
