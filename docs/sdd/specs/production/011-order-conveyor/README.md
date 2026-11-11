# PRD-011 «Виробничі завдання»: stage conveyor, sorting and item lines

Status: implemented 2026-11-11. Changes [PRD-010](../010-order-filters-model-summary/README.md). Admin: brevi-web spec `docs/specs/admin/production/011-order-conveyor`.

## Goal

The owner chose concept B «Конвеєр етапів» (2026-11-11) for the list of «Виробничі завдання»: over the table a row of the workshop stages with how many open orders stand at each, a strip only when orders are overdue or due soon, the list sorted by «Готово до», and in each row the models with their stages and how much is shipped. Every existing function stays.

## Decisions

| ID | Decision |
| --- | --- |
| PRD-D68 | `getProductionOrders` takes `sort`: `number`, `due`, `created`, each with a leading `-` for largest first; default `-number` as before. `due` is the nearest item due date, orders without one last, then the number. Other values are refused with «Невідоме сортування.». |
| PRD-D69 | Each row adds `shippedQuantity` (pieces of the order's shipments; a cancelled shipment is removed, so it does not count) and `items`: model name, pieces and current stage of every item in card order. |
| PRD-D70 | The page adds `stages`: every stage in workshop order with the «Нове» orders whose least advanced item is at it and how many of those are overdue; and `attention`: overdue «Нове» orders (nearest due date before today) and due soon (today to today + 3 days). Both use every filter except status and stage, so the conveyor stays whole while it filters the list; «today» is the Kyiv workshop day (`WorkshopDay`). Open orders are few, so they are counted in memory. |

## Acceptance

| ID | Check |
| --- | --- |
| PRD-011-A1 | `sort=due` puts the nearest due date first; ties go by number. |
| PRD-011-A2 | A row lists its items with stage and the shipped pieces. |
| PRD-011-A3 | The conveyor counts only «Нове» orders, ignores the stage filter, and has all 9 stages. |
| PRD-011-A4 | Attention counts overdue and due within 3 days; other filters narrow it. |
| PRD-011-A5 | An unknown sort is refused. |

## File-role map

| Role | Files |
| --- | --- |
| Use cases | `Orders/GetOrders/GetProductionOrdersQuery.cs`, `GetProductionOrdersQueryHandler.cs`, `GetProductionOrdersQueryValidator.cs`, `ProductionOrderFiltering.cs` |
| Contracts | `Contracts/ProductionOrderResponses.cs` (`ProductionOrderSummaryItem`, `ProductionStageCount`, `ProductionOrderAttention`) |
| API | `Production.Api/Orders/ProductionOrdersController.cs` (`sort`) |
| Contract | `docs/sdd/contracts/production/production-orders.openapi.yaml` |
| Tests | `tests/UnitTests/Production/ProductionOrderFilterTests.cs`, `ProductionOrdersContractTests.cs` |

## Evidence

See the PR description.
