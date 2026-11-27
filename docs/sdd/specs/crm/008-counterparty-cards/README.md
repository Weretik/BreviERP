# CARD-001 — client and supplier cards: links, period and pages

**Status:** backend implemented 2026-11-27; Admin cards in brevi-web. **Date:** 2026-11-27. **Modules:** Crm (КП, tasks, «Ціна вишивки»), Production (orders), Warehouse (fabric movements), Catalog (PPE products); one new column in `crm.Quotes`. Contract: [OpenAPI](../../../contracts/openapi.json).

## Why

The owner found the client card broken and unlike the supplier view (thread «Redesign client and supplier cards», 2026-11-27). The card's «КП» and «Завдання в цех» tabs sent the client's public id turned into a number (`NaN`), so the API answered 400 and the tables never stopped loading. Behind that, a КП never stored the client picked in «Кому»: only the text, so nothing could be listed under the client even with a correct id. The owner chose concept Б «Огляд і вкладки» ([concepts](https://claude.ai/artifact/VWCfNehMV2zJLXma4ifRPR)): one page for a client and for a supplier, a period for every tab, lists by pages of 20.

## Rules

- **R-01 Public ids.** HTTP identifies a client by its `publicId` (Guid), as tasks and «Ціна вишивки» already do ([ID strategy](../../../standards/identifier-strategy.md)). Internal customer ids stay inside Crm and in modules that copied them.
- **R-02 КП link.** A КП stores the chosen client as `QuoteCustomer` (internal `CustomerId` plus `CustomerPublicId`), set together from an active, not merged customer. `SaveQuoteInput` and `SaveQuoteIntroductionInput` take `customerPublicId`; an unknown or archived one is a 400 «Клієнта не знайдено…»; null (a name typed by hand) clears the link. Responses return `customerPublicId`. Old КП moved from the calculator keep their link: the migration copies the public id from `Customers`.
- **R-03 КП by client.** `GET /api/crm/quotes` takes `customerPublicId`, `from` and `to` (Kyiv days of creation, inclusive) next to the access rule (a Manager sees only own КП).
- **R-04 Orders by client.** `GET /api/production/orders` takes `customerPublicId` instead of the internal `customerId`; Production resolves it through `ICustomerDirectory.FindInternalIdAsync` (Crm.Contracts), and an unknown client lists nothing. Orders keep the internal id they copy from the КП.
- **R-05 Tasks and calculations by client.** `GET /api/crm/tasks` takes `customerPublicId`, `from`, `to` and the new filter `All` (open and done); `GET /api/crm/embroidery/calculations` takes `customerPublicId`, `from`, `to`.
- **R-06 Period.** Every `from`/`to` pair is inclusive and `to` may not be before `from` (400 with a Ukrainian message). Production orders and purchase invoices keep their existing period parameters (`from`/`to`, `documentFrom`/`documentTo`).
- **R-07 Paging.** The card asks for 20 rows per page; every list already pages on the server.
- **R-08 Supplier lists.** `GET /api/warehouse/fabric-stock/movements` takes `supplierId`: movements of that supplier, or receipts whose typed supplier name equals its name without case (receipts typed by hand keep only the name). `GET /api/catalog/products` takes `supplierId`: PPE products whose supplier or catalog offer is that supplier. Fabrics and accessories already carry `supplierId` in their lists; purchase invoices already take `supplierId`.
- **R-09 Rate limits.** No new endpoint: every change adds query parameters to cheap signed-in reads, so the global limit stays enough ([rate limit rules](../../../standards/rate-limit-rules.md)).
- **R-10 Access.** No new page or action on the server: each list keeps its own policy (КП, tasks, «Ціна вишивки», workshop orders, warehouse, catalog), so the card shows a tab only to those who may read it.

## Scenarios

- **SC-01** A manager picks «ТОВ «Агро»» in «Кому» and saves → the КП has the client's public id; the client card «КП» tab lists it.
- **SC-02** A typed name without picking from the list → the КП has no client; an unknown public id → 400.
- **SC-03** The client card asks for its КП, tasks, calculations and orders for November → only that client's rows created in November, 20 per page.
- **SC-04** An order made from the client's КП appears under «Завдання в цех»; an unknown public id lists nothing.
- **SC-05** The supplier card asks for movements of «Черкаси ВВ» → receipts linked by id and receipts typed «черкаси вв».
- **SC-06** The supplier card asks for PPE products of a supplier → only products with that supplier.

## Design

| Layer | Files |
| --- | --- |
| Domain | `Crm.Domain/Quotes/ValueObjects/QuoteCustomer.cs`; `Quote.CustomerId` is now the typed `CustomerId?`, plus `CustomerPublicId`; `QuoteHeader.Customer`; `QuoteSnapshot.CustomerPublicId` |
| Application | `Quotes/Shared/Building/QuoteCustomerResolver.cs`; `IQuoteStore.FindQuoteCustomerAsync`; `GetQuotes`, `GetTasks` (`TaskListFilter.All`), `GetEmbroideryCalculations` filters; `Customers/Queries/GetCustomerInternalId/`; Production `IProductionCustomerReader` and `GetProductionOrdersQuery.CustomerPublicId`; Warehouse `GetFabricMovementsQuery.SupplierId`; Catalog `GetAdminProductsQuery.SupplierId` |
| Contracts | `ICustomerDirectory.FindInternalIdAsync` (Crm.Contracts); `QuoteDirectory` resolves the internal id for `IQuoteReader` |
| Infrastructure | `QuoteConfiguration` (typed id conversion, index `CustomerPublicId, CreatedAt`), `ManagerTaskConfiguration` (index `CustomerPublicId, CreatedAt`), migration `LinkQuotesToCustomerPublicId` with the backfill; Warehouse index `SupplierId, OperationDate`, migration `IndexFabricMovementsBySupplier`; `Production.Infrastructure/Adapters/Crm/ProductionCustomerReader.cs` |
| Api | Query parameters on `getCrmQuotes`, `getCrmTasks`, `getCrmEmbroideryCalculations`, `getProductionOrders`, `getFabricMovements`, `getAdminProducts` |

Rollout: the migrations add a nullable column, a backfill and three indexes; old Admin builds keep working because new parameters are optional, except the Production `customerId` parameter, which only the broken card tab used. Rollback: the column stays unused.

Residual: a merged-away client's КП stay under the merged-away record (the merge does not move КП, as before); a receipt typed with another spelling of the supplier is not found.

## Tasks and evidence

- **TS-01** Domain and validators — `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~UnitTests.Crm.Quotes"` (header link, copy, import, period validator).
- **TS-02** КП and tasks by client and period on PostgreSQL — `QuotePersistenceTests.ClientCardListsOnlyThatClientsQuotesWithinTheActorsAccess`, `UnknownClientIsRejectedAndATypedNameStaysUnlinked`, `ManagerTaskPersistenceTests.ClientCardListsOpenAndDoneTasksOfThatClientInThePeriod` (the migration with its backfill runs in the fixture).
- **TS-03** Orders by public id — `ProductionOrderHandlerTests.Client_card_lists_only_orders_made_from_that_clients_quotes`.
- **TS-04** Supplier lists — `WarehouseMovementJournalTests.The_supplier_card_finds_receipts_by_the_supplier_or_its_typed_name`, `ProductAdminListReadModelTests.Supplier_filter_*`.
- **TS-05** Contract — regenerated `openapi.json`; `OpenApiDocumentRulesTests`, `ArchitectureTests`, `check_module_maps.py`.
- **TS-06** brevi-web: one card page for a client and a supplier, period, tabs by 20, the client picker of «Кому» sends `customerPublicId`.
