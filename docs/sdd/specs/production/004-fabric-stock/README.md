# PRD-004 «Швейний цех»: fabric stock

Status: stage 4 implemented 2026-11-04. Follows [PRD-003](../003-order-tracking/README.md).

## Goal

The workshop knows how many metres of each fabric and colour lie on its shelves, how much of it the open orders need, and what was issued to which item. Every change is a movement with who, when and why, so the balance can always be explained. The stock of the old «Цех» (179 balances, 275 movements, 25 issues) comes with its history.

## Decisions

| ID | Decision |
| --- | --- |
| PRD-D25 | A stock line is one fabric of «Тканини» (by its FAB- code, which it must have) in one colour (free text; the key ignores case and extra spaces). Metres keep two decimals. The line is created by its first movement. |
| PRD-D26 | The balance changes only through movements: «Початковий залишок» (first one), «Прихід» (supplier from «Постачальники», document, date), «Видача» (to an item), «Коригування» (counted amount with a reason) and «Сторно». Each keeps before / change / after, the operation date, the person and a note. A movement is never edited: a wrong one is reversed by «Сторно» with a reason and recorded again. A stock line has a version so two people cannot overwrite each other (409). |
| PRD-D27 | The balance may go below zero (the old «Цех» allowed it: fabric is cut before the receipt is typed in); such a line is marked as a problem «Мінус» until a receipt or correction fixes it. |
| PRD-D28 | Plan of an item = the current workshop norm (PRD-001) for its number of colours, split by colour: one colour takes «1 колір»; two take main and additional; three take main and both additional; × quantity, 0.01 m. It is computed, not stored, so a corrected norm moves the reservation too. |
| PRD-D29 | «Бронь» of a stock line = sum over items of «Нове» orders of (plan part for that fabric and colour − what issues already covered of it), never below zero. «Доступно» = balance − бронь. Changing quantity or colours changes the reservation, never what was issued. |
| PRD-D30 | «Видача» is one package for one item with lines: «Планова» (the planned fabric and colour; covers the plan up to what was issued), «Заміна» (another fabric or colour instead of a planned part; closes that part) and «Додаткова» (beyond the plan; covers nothing). The card shows plan, covered, issued and the difference per part. A package can be reversed whole («Сторно видачі», with a reason): every line gets its reversal movement and the plan is reserved again. |
| PRD-D31 | Access: the stock list, cards and the «Тканина» tab of an order are for everyone who sees production orders (managers too); movements and issues are for WorkshopChief and Admin on a «Нове» order; the value in UAH (metres × price of «Тканини») only for Admin (Director holds Admin) and WorkshopChief. |
| PRD-D32 | Old stock moves through `POST /api/production/fabric-stock/import` (Admin), preview first: lines by FAB- code and colour, movements in their order, issues matched to moved orders by old id and position. It checks that the movements of every line add up to its balance and that each before + change = after; nothing is saved unless every row is fine; a repeat changes nothing (old ids are kept). The old reservation table is not moved: it is recomputed by PRD-D29, and the preview shows how many lines differ. |
| PRD-D33 | The forms pick fabrics (with a FAB- code) and active suppliers from `GET /api/production/fabric-stock/choices`, so the head of the workshop needs no access to «Довідники». |

## Acceptance

| ID | Check |
| --- | --- |
| PRD-004-A1 | Receipt, correction and reversal keep before / change / after; a stale version is 409; a movement without a reason where one is needed is 400. |
| PRD-004-A2 | Plan is split by colours from the norm; reservation falls by what an issue covered and comes back after «Сторно видачі». |
| PRD-004-A3 | «Додаткова» covers nothing, «Заміна» closes its part, «Планова» covers up to the issued metres. |
| PRD-004-A4 | Managers see stock without value; they cannot move it. |
| PRD-004-A5 | Import: a line whose movements do not add up is an error and nothing is saved; a preview saves nothing; a repeat is unchanged. |

## File-role map

| Role | Files |
| --- | --- |
| Domain | `Stock/FabricStock.cs` (line + movements), `Stock/FabricMovement.cs`, `Stock/FabricIssue.cs` (package + lines), `Stock/FabricPlanParts.cs` (plan by colour), `Stock/StockColor.cs` (colour key) |
| Use cases | `Stock/` — `GetStock` (list with reservation and value), `GetStockLine` (card with movements), `Movements` (opening, receipt, correction, reversal), `Issues` (item tab, issue, reversal), `Consumption` (period summary), `ImportStock`; `Stock/StockReservations.cs` computes бронь |
| Ports | `Abstractions/IProductionFabricReader.cs` gains price; `IProductionSupplierReader.cs` («Постачальники») |
| Persistence | `Configurations/FabricStockConfiguration.cs`; migration `FabricStock` |
| API | `Production.Api/Stock/FabricStockController.cs`, `Orders/ProductionOrderFabricController.cs` |
| Host | `Host.Api/Production/ProductionSupplierReader.cs`; `ProductionFabricReader` maps price |
| Contract | `docs/sdd/contracts/production/fabric-stock.openapi.yaml` |

## Evidence

- Unit: `dotnet test tests/UnitTests/UnitTests.csproj` — 690 passed (2026-11-04), including `ProductionStockHandlerTests` (5) and `FabricStockContractTests`.
- Architecture: `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj` — 13 passed.
- Transfer: the file of 179 lines, 275 movements and 25 issues was run through the import against the 28 moved orders on SQLite: no errors, a repeat changed nothing. The recomputed reservation of «Нове» items with a norm equals the old one (1150.63 m); the old app also kept reservations of closed orders and of items without a workshop norm, which PRD-D28/D29 do not reserve.
