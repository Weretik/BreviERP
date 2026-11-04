# PRD-002 «Швейний цех»: production orders from КП

Status: stage 2 implemented 2026-11-04. Follows [PRD-001](../001-workshop-references/README.md). Stages, shipments, files, Telegram, closing and cancelling come with stage 3.

## Goal

A manager passes their sent КП to the workshop as a «Виробниче завдання»; the head of the workshop sees what to sew, for whom and by when, with the sewing rate and planned fabric; the owner keeps the КП price for reports. The 29 orders of the old «Цех» move in with the same numbers.

## Decisions

| ID | Decision |
| --- | --- |
| PRD-D8 | Policy `CanUseProductionOrders` = Manager, WorkshopChief, Admin (the director also holds Admin) for `/api/production/orders`. Inside it: Admin does everything; a Manager creates from their own sent КП «під запит», edits orders whose manager they are, never sees the sewing rate or the report price; WorkshopChief sees and edits every order with the rate, without КП prices, and cannot create. Only Admin deletes and moves old orders. |
| PRD-D9 | Creation copies a snapshot: КП number, client (the manager may type another company name), the КП responsible as manager, model name and code (MOD-), fabric name and code (FAB-), the seamstress labour of the Catalog variant as «розцінка пошиття», and one КП price per item for reports. Later changes of the КП, Catalog or Reference do not change the order. |
| PRD-D10 | The report price is the tier the manager picks for the order: «Роздріб», «Опт» (default) or «Великий опт». The old «Цех» never captured a price (all `unavailable`), so moved orders have none. |
| PRD-D11 | Planned fabric = workshop norm (PRD-001) for 1, 2 or 3 colours (main + additional, at most two) × quantity, rounded to 0.01 m; recalculated when quantity or colours change. Null when the model has no norm. |
| PRD-D12 | Idempotency: the form sends a new `requestId`; a repeat returns the same order. Order numbers continue after the largest number (old ones are kept); a unique index stops a race. |
| PRD-D13 | Every changed field of the order or an item is a line of «Історія змін» with display values before and after, the person's name and time. Saves carry `version`; a stale save is 409. |
| PRD-D14 | «Видалити» is soft: the order leaves lists and detail, its rows and history stay. «Копіювати» makes a new «Нове» order dated today with the same items, owned by the person who copies. |
| PRD-D15 | Colours are free text for now; the colour list of a fabric comes with the stock in stage 4. «Нанесення» is a list of kind (вишивка, друк), place, description and colour. |
| PRD-D16 | Old orders move through `POST /api/production/orders/import` with a preview first; an order already moved (same old id) stays unchanged; any wrong order saves nothing. The manager is matched by email. Item stages move with stage 3. |

## Acceptance

| ID | Check |
| --- | --- |
| PRD-002-A1 | An order from a sent КП copies model, fabric, rate and the chosen tier price, and plans fabric from the norm. |
| PRD-002-A2 | A repeated press with the same request id returns the same order. |
| PRD-002-A3 | A manager cannot use another manager's КП (404); WorkshopChief cannot create (403); a draft or archived КП is 409. |
| PRD-002-A4 | A manager never receives the sewing rate or report price; WorkshopChief receives the rate but not the price. |
| PRD-002-A5 | A save writes one history line per changed field and replans fabric; a stale version is 409; another manager's order is 403; a manager's rate change is ignored. |
| PRD-002-A6 | Only Admin deletes; a deleted order is gone from the list and detail. |
| PRD-002-A7 | The list filters by status, manager, text and due date; status counts ignore the status filter. |
| PRD-002-A8 | Import preview saves nothing; a repeat keeps moved orders; a taken number saves nothing. |

## File-role map

| Role | Files |
| --- | --- |
| Domain | `Production.Domain/Orders/` — ProductionOrder (aggregate, history), ProductionItem (+ ItemSource, ItemDetails), ProductionOrderChange, ItemApplication, FabricPlan, OrderText, enums |
| Use cases | `Production.Application/Orders/` — GetOrders, GetOrder, GetHistory, QuoteSources (list, draft, QuoteItemResolver), CreateOrder, UpdateOrder, CopyOrder, DeleteOrder, ImportOrders; Access/ProductionActor; Validation; shared OrderMapping, OrderNorms, OrderNumbers, OrderActors, OrderMessages |
| Ports | `Abstractions/` — IProductionQuoteReader (Crm), IProductionFabricReader (Reference), IProductionStaffReader (Identity), IProductionModelReader.ReadCostsAsync (Catalog) |
| Persistence | `Persistence/Configurations/ProductionOrderConfiguration.cs`, `JsonColumn.cs`; migration `ProductionOrders` (Orders, Items, OrderChanges) |
| API | `Production.Api/Orders/ProductionOrdersController.cs`; `Access/ProductionAccess.Actor` |
| Host | `Host.Api/Production/ProductionQuoteReader.cs`, `ProductionFabricReader.cs`, `ProductionStaffReader.cs`, `ProductionModelReader.ReadCostsAsync`; policy in `AuthorizationPoliciesRegistrationExtensions` |
| Contract | `docs/sdd/contracts/production/production-orders.openapi.yaml` |

## Evidence

- Unit: `dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~UnitTests.Production` — 61 passed (2026-11-04), including `ProductionOrderHandlerTests` (13) and `ProductionOrdersContractTests`.
- Architecture: `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj` — 13 passed.
