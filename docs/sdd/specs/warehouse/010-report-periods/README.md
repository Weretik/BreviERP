# WH-010: «Звіти» periods and the split of issues

**Status:** implemented 2026-11-12 (owner go «начинай» on stage 5 of the «Склад тканин» concepts, 2026-11-12). **Module:** Warehouse, schema `warehouse` (no schema change). Follows [WH-009](../009-to-order-dates/README.md). Frontend: brevi-web «Склад тканин» UX stage 5.

## Scope

In: `GET /api/warehouse/reports/plan-fact` takes `from` and `to`, and each item carries `issuedOn`; `GET /api/warehouse/fabric-stock/consumption` splits what was issued ([contract](../../../contracts/warehouse/fabric-stock.openapi.yaml)). Read only; access pages unchanged.

Out: how plan and fact are compared ([WH-005](../005-reports/README.md)); «Вартість складу» (its per-fabric totals already exist).

## Rules

| Rule | Behaviour |
| --- | --- |
| Plan-fact period | `issuedOn` of an item is the day of its last active issue. With `from` / `to` only fully issued items with `issuedOn` in the period are compared and summed; items still in progress are counted as before. `to` before `from` is 400. |
| Issues split | `issuedToTasks` is what was issued to items of orders, `oneOff` what was issued without a task («Разова витрата»); `issued` stays their sum. A reversal counts against the movement it takes back. The response also sums `corrected`. |

## Compatibility

Fields and parameters are added with defaults; an old client keeps working.

## Tests

- `tests/UnitTests/Warehouse/WarehouseReportTests.cs` — `Plan_and_fact_keeps_the_items_whose_last_issue_is_in_the_period`, `Consumption_splits_what_went_to_tasks_from_one_off_issues`.
- `tests/UnitTests/Warehouse/FabricStockContractTests.cs` — the responses match the machine contract.
