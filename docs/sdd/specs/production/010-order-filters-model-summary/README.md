# PRD-010 «Виробничі завдання»: filters and «Зведення за моделями»

Status: implemented 2026-11-11. Changes [PRD-002](../002-production-orders/README.md). Admin: brevi-web spec `docs/specs/admin/production/010-order-filters-model-summary`.

## Goal

The owner asked (2026-11-11) for the filters of the old «Цех» order list, made in Brevi's own design: «Замовник», «Період від / до», «Модель», «Тканина», «Колір», «Сезон», «Стадія замовлення», search by order or КП number and cancelled orders. Under the list, «Зведення за моделями» shows how many pieces of each model the filtered orders hold, with season and category, sorted on the page by several fields in priority order.

## Decisions

| ID | Decision |
| --- | --- |
| PRD-D64 | `getProductionOrders` takes `client`, `from`, `to`, `model`, `fabric`, `color`, `season`. Client is the exact name; the period bounds the order date; model and fabric are item codes (case ignored); colour matches the main or an additional colour of an item by `StockColor.Key` (case and outer spaces ignored), filtered in memory because additional colours are a JSON column; season is the Catalog season of the item's model, read through `IProductionModelReader`. Stage, search (number, КП number, client) and status already existed. |
| PRD-D65 | `getProductionModelSummary` takes the same filters without paging and returns every filtered item grouped by model code (by name for an item without code): season, category, ordered pieces, number of orders, and the total pieces. Quantity is the ordered pieces, not the shipped ones. |
| PRD-D66 | Category is read from the model name like the old «Цех» `product_type` (`ModelCategory.Of`): a suit («костюм», «+», « / ») is «Костюм куртка + напівкомбінезон» or «Костюм куртка + брюки»; otherwise Напівкомбінезон, Комбінезон, Куртка, Штани (also «брюки»), Жилет, Фліска; else «Інше». Catalog has no such category per model. On the old export 61 of 66 rows match; the rest differ only in Штани/Брюки naming and a plain «Костюм». |
| PRD-D67 | `getProductionOrderFilterOptions` lists the values the filters offer from orders that are not deleted: clients, models and fabrics (code and name), colours (one per key) and Catalog seasons of the models used. Sorting of the summary by priority is on the page, not in the API. Access stays `access:production-orders`. |

## Acceptance

| ID | Check |
| --- | --- |
| PRD-010-A1 | Each new filter narrows the list; an additional colour counts; model and season combine with AND. |
| PRD-010-A2 | The summary counts every filtered order, not only the page, and honours the status filter. |
| PRD-010-A3 | Filter options list only values used by orders. |
| PRD-010-A4 | «Період до» before «від» is rejected. |
| PRD-010-A5 | Category is read from the model name. |

## File-role map

| Role | Files |
| --- | --- |
| Domain | `Production.Domain/Orders/ModelCategory.cs` |
| Use cases | `Orders/GetOrders/ProductionOrderFilters.cs`, `ProductionOrderFiltering.cs`, `GetProductionOrdersQuery*.cs`, `GetProductionModelSummaryQuery.cs`, `GetProductionOrderFilterOptionsQuery.cs` |
| Contracts | `Contracts/ProductionOrderResponses.cs` (`ProductionModelSummaryRow`, `ProductionModelSummaryResponse`, `ProductionFilterOption`, `ProductionOrderFilterOptionsResponse`) |
| API | `Production.Api/Orders/ProductionOrdersController.cs` (`model-summary`, `filter-options`) |
| Contract | `docs/sdd/contracts/production/production-orders.openapi.yaml` |
| Tests | `tests/UnitTests/Production/ProductionOrderFilterTests.cs`, `ProductionOrdersContractTests.cs` |

## Evidence

See the PR description.
