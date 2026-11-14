# Fabric stock «Склад тканин» (Warehouse module)

Metres of each fabric of «Тканини» (by FAB- code) in each colour. A pair is a fabric (canonical code; FAB-067 is FAB-023 and FAB-077 is FAB-078) in a normalised colour; its lines keep their own codes. Balances change only through movements: opening (once per pair, zero allowed), receipt of the offer picked, issue without an order (may go below zero), issue to an order item, «Уточнити» to a counted total with a reason, and reversal. Openings, receipts, corrections and issues without an order can be edited (date, metres, supplier/document or purpose, the offer and colour of a receipt, the pair of an issue), voided («Скасувати», kept with no effect) and restored to the state before the last change; each change is a revision with values before and after. Each movement keeps before / change / after, the operation date apart from the record time, and the person; a line's chain runs by operation date. Every form sends an operation id, so a repeat never moves the fabric twice. Open orders of «Швейний цех» reserve their plan until issues cover it. Fabric goes to an item by actual cutting («Видача на завдання» in «Рух тканини» or the «Тканина» tab): the products cut and the real pairs; each planned part is compared with P × n / N, lines are «планова», «заміна» or «додаткова»; the item plan is saved at the first issue and changes by «Оновити план»; order-issue movements can be edited and voided too; «Рух тканини» shows «На 1 виріб»; it searches every text of a movement, filters by operation-date period and by kind chips with counts (receipts, to items, one-off, adjustments, openings, cancelled) and shows «Залишок пари» after each movement; «Залишки» shows per pair what open plans book («У броні»), «Вільно» (may go below zero), what to order, the tasks behind the booking and the latest movement. Four read-only reports, each its own page of «Права доступу»: «План і факт» (main fabric of fully issued items by model, weighted per product, by the period of the last issue), «Бронь і що замовити» (stock handed to open plans by order date, what to order, the fabric supplier and «Потрібно до»), «Вартість складу» (positive balances at the lowest group price, owner only) and «Фурнітура до замовлення» (accessory need of open orders by model composition, with the nearest due date). Everyone with access reads «Залишки», «Рух тканини» and «Початкові залишки»; the head of the workshop and Admin write. «Перенесення з v133» replaces the whole stock with the old «Цех» from its D1 export (dry run first; records entered in Brevi are removed only on confirmation; the same export again changes nothing). Own module and schema since [WH-001](../../specs/warehouse/001-warehouse-module/README.md) (first in [PRD-004](../../specs/production/004-fabric-stock/README.md)); ledger rules [WH-002](../../specs/warehouse/002-ledger-rules/README.md); edits [WH-003](../../specs/warehouse/003-movement-revisions/README.md); issue by cutting [WH-004](../../specs/warehouse/004-issue-by-cutting/README.md); reports [WH-005](../../specs/warehouse/005-reports/README.md); transfer from v133 [WH-006](../../specs/warehouse/006-v133-transfer/README.md); journal search [WH-007](../../specs/warehouse/007-movement-journal/README.md); stock booking columns [WH-008](../../specs/warehouse/008-stock-booking-columns/README.md); to-order dates [WH-009](../../specs/warehouse/009-to-order-dates/README.md); report periods and issues to tasks / one-off in «Витрата за період» [WH-010](../../specs/warehouse/010-report-periods/README.md); [contract](../../contracts/warehouse/fabric-stock.openapi.yaml), [tables](../../architecture/data-model/warehouse.md).

## Code structure

```text
src/Modules/Warehouse/
  Warehouse.Domain/Warehouse.Domain/Stock/
    FabricStock.cs               # line + its movements: Move, Reverse, Transfer, Restate (chain by date); version
    FabricMovement.cs            # kinds, details, before/change/after, destination, operation id, reversal mark
    FabricAliases.cs             # reviewed alias groups FAB-023/067, FAB-078/077 and their identity
    StockPair.cs                 # pair of a group and colour: Open, Receive, Issue, Adjust; spending order; version
    FabricMovementRevision.cs    # edit / void / restore with the state before and after
    StockOperation.cs            # form submission by operation id (repeat → saved result)
    StockTransfer.cs             # journal of v133 transfers (export hash, counts, who, when)
    FabricIssue.cs               # «Видача» package by cutting + lines over movements (Planned/Replacement/Additional), reversal
    FabricItemPlan.cs            # saved plan of an item (parts, layers), comparison plan P × n / N
    StockColor.cs                # colour text and NFC key
  Warehouse.Application/Warehouse.Application/
    Abstractions/                # IWarehouseDbContext; ports IWarehouseProductionReader, IWarehouseReferenceReader, IWarehouseStaffReader
    Features/Stock/
      Commands/                  # OpenFabricStock, ReceiveFabric, IssueFabric, AdjustFabricStock (the pair commands);
                                 # EditFabricMovement, VoidFabricMovement, RestoreFabricMovement; ReverseFabricMovement («Сторно»);
                                 # CreateFabricIssue (issue by cutting), RefreshItemFabricPlan («Оновити план»), ReverseFabricIssue;
                                 # TransferStock (V133Source reads and checks the export, V133Writer writes and replays it)
      Queries/                   # GetFabricStock («Залишки»), GetFabricStockChoices, GetFabricMovements («Рух тканини»),
                                 # GetFabricMovementHistory («Історія правок»), GetFabricIssueItems, GetProductionItemFabric,
                                 # GetFabricConsumption (period summary), GetFabricPlanFact, GetFabricBooking, GetFabricValuation, GetHardwareOrder
      Shared/
        WarehouseActor.cs        # CanMoveStock, SeesStockValue, CanImport, CanIssueFabric(item)
        WarehouseColors.cs       # a new line takes a colour of «Кольори» (REF-006)
        StockReservations.cs     # plans of «Нове» items (from Production) minus what issues covered
        StockChanges.cs          # actor, offer check, line lookup or creation, save with 409
        StockPairs.cs            # load a pair, restate touched ledgers, save with the operation
        StockOperations.cs       # request hash and replay
        StockRules.cs, StockList.cs, StockMessages.cs, MovementRevisions.cs, ItemPlans.cs, ItemFabricCards.cs, FabricBookingCalculator.cs
        Facts/StockFacts.cs      # issues of a period and balances at a date for «Звіти цеху»
  Warehouse.Infrastructure/Warehouse.Infrastructure/Persistence/  WarehouseDbContext (schema warehouse), FabricStockConfiguration, migrations
  Warehouse.Api/Warehouse.Api/Endpoints/   # one <UseCase>/ folder per operation, mapped by its group
    FabricStock/      # /api/warehouse/fabric-stock (policy access:fabric-stock)
    OrderItemFabric/  # /api/warehouse/orders/{id}/items/{itemId}/fabric… (policy access:production-orders)
    Reports/          # /api/warehouse/reports/… (a policy per report: access:fabric-plan-fact, access:fabric-booking, access:fabric-valuation, access:hardware-order)
src/Modules/Production/Production.Domain/Production.Domain/FabricPlans/  # FabricPlanParts, PlanColor
src/Modules/Production/Production.Application/Production.Application/Features/FabricPlans/Shared/  # ProductionFabricPlans and ProductionAssemblies for the stock
src/Modules/Warehouse/Warehouse.Contracts/Warehouse.Contracts/Stock/IStockFacts.cs  # issues and balances for other modules
src/Modules/Warehouse/Warehouse.Infrastructure/Warehouse.Infrastructure/Adapters/
  Reference/WarehouseReferenceReader.cs    # Reference.Contracts fabrics with price, suppliers, colours
  Identity/WarehouseStaffReader.cs         # Identity.Contracts IStaffDirectory
src/Modules/Production/Production.Infrastructure/Production.Infrastructure/Adapters/Warehouse/ProductionStockReader.cs  # Warehouse.Contracts IStockFacts for «Звіти цеху»
src/Modules/Warehouse/Warehouse.Infrastructure/Warehouse.Infrastructure/Adapters/Production/WarehouseProductionReader.cs  # Production.Contracts IOrderFabricPlans (ProductionFabricPlans, ProductionAssemblies)
```

## Structure diagram

```mermaid
flowchart LR
  Admin[Brevi Admin «Склад тканин» and tab «Тканина»] --> Endpoints[Warehouse endpoints]
  Endpoints --> UseCases[Stock use cases]
  UseCases --> Db[(warehouse.FabricStocks / FabricMovements / FabricIssues / FabricIssueLines / FabricItemPlans)]
  UseCases --> Plan[IWarehouseProductionReader] -.host.-> Production[Production plans, orders, history]
  UseCases --> Reference[IWarehouseReferenceReader] -.Reference.Contracts.-> Ref[Reference fabrics, suppliers, colours]
  Reports[Production «Звіти цеху»] --> Port[IProductionStockReader] -.Warehouse.Contracts.-> Facts[IStockFacts]
```

## Tests

- `tests/UnitTests/Warehouse/WarehouseLedgerTests.cs` — WH-002: opening once per pair, receipt by offer, issue order and shortage, «Уточнити» limits, operation id repeat, chain by date, paging and filters, alias identity, colour key.
- `tests/UnitTests/Warehouse/WarehouseMovementJournalTests.cs` — WH-007: pair balance across alias codes, kind counts under search and period, period order.
- `tests/UnitTests/Warehouse/WarehouseRevisionTests.cs` — WH-003: edit and restatement, date and pair change, void, restore one change at a time, revision guard, signs, access.
- `tests/UnitTests/Warehouse/WarehouseIssueByCuttingTests.cs` — WH-004: plan of a cut once per part, coverage over lines, saved plan and refresh, operation repeat, products limit, edit and void of order-issue movements, split lines keep their pair.
- `tests/UnitTests/Warehouse/WarehouseReportTests.cs` — WH-005: plan and fact (full issue, comparability, weighted totals), booking allocation and queue, valuation by group price with shortages, accessory need by code class, «На 1 виріб».
- `tests/UnitTests/Warehouse/WarehouseStockHandlerTests.cs` — receipt and reversal, access and prices, plan by colour, issue purposes, reversal of an issue, colours of a new line, stock facts.
- `tests/UnitTests/Warehouse/WarehouseTransferTests.cs` — WH-006: dry run, apply and repeat, refused export (broken, truncated, order missing in Brevi), confirmation of own records, Admin only.
- `tests/UnitTests/Warehouse/FabricStockContractTests.cs` — DTOs and operations against the contract.
- `tests/ArchitectureTests/WarehouseDependencyTests.cs` — layers and schema.

Focused run: `dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~UnitTests.Warehouse`.

**Data:** [warehouse schema](../../architecture/data-model/warehouse.md)
