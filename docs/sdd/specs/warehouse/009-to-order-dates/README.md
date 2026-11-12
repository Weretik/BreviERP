# WH-009: «Що замовити» due dates and supplier

**Status:** implemented 2026-11-12 (owner go «сливай и давай дальше» on the «Склад тканин» concepts, 2026-11-12). **Module:** Warehouse, schema `warehouse` (no schema change); Production passes the item due date to the accessory reader. Follows [WH-008](../008-stock-booking-columns/README.md). Frontend: brevi-web «Склад тканин» UX stage 4.

## Scope

In: new response fields of `GET /api/warehouse/fabric-stock/reports/booking` and `.../reports/hardware` ([contract](../../../contracts/warehouse/fabric-stock.openapi.yaml)). Read only; access pages unchanged (`fabric-booking`, `hardware-order`).

Out: changes to how the booking or the accessory need is counted ([WH-005](../005-reports/README.md)); accessory stock (the owner decided not to track it).

## Rules

| Rule | Behaviour |
| --- | --- |
| Fabric supplier | `supplier` of a booking pair is the supplier of the canonical offer in «Тканини» (another code of the group when the canonical one is missing); null when empty. |
| «Потрібно до» | `needBy` of a booking pair is the nearest due date of the items still short of it; when none is short, of the items that still need it; null when none has a date. |
| Accessory dates | Each share of an accessory need carries its item's `dueDate`; the need carries the nearest one. |

## Compatibility

Fields are added with defaults; an old client keeps working.

## Tests

- `tests/UnitTests/Warehouse/WarehouseReportTests.cs` — supplier of the booking pair, `Need_by_is_the_nearest_date_of_the_items_still_short_or_else_of_those_that_need_it`, `Hardware_need_keeps_the_nearest_due_date_of_its_tasks`.
- `tests/UnitTests/Warehouse/FabricStockContractTests.cs` — the responses match the machine contract.
