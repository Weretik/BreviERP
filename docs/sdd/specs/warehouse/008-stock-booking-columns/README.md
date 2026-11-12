# WH-008: «Залишки» with booking, free metres and the latest movement

**Status:** implemented 2026-11-12 (owner go «давай делай след етап» on the «Склад тканин» concepts, 2026-11-12). **Module:** Warehouse, schema `warehouse` (no schema change). Follows [WH-007](../007-movement-journal/README.md). Frontend: brevi-web «Склад тканин» UX stage 3.

## Scope

In: new response fields of `GET /api/warehouse/fabric-stock` ([contract](../../../contracts/warehouse/fabric-stock.openapi.yaml)). Read only; the page stays `fabric-stock` in «Права доступу».

Out: changes to the booking rules of [WH-005](../005-reports/README.md) (they are reused as they are), new filters on the server (search and problem chips stay on the page).

## Rules

| Rule | Behaviour |
| --- | --- |
| Booking | The booking of «Що замовити» (`FabricBookingCalculator`, shared with `GET .../reports/booking`) is computed once per request; each pair of «Залишки» gets the matching pair by canonical code and colour key. |
| `booked` | «У броні»: what open plans still need of the pair (plan − covered by issues, summed). |
| `free` | «Вільно»: `meters − booked`; may be below zero. |
| `toOrder` | What to buy, as «Що замовити» counts it (the shortage already there included). |
| `bookings` | The tasks behind `booked`, one row per order item, oldest order first, with what is left of its plan and what of it the stock does not cover. |
| `lastMovement` | The latest movement of the pair that is not voided, by ledger order (operation date, record time, id). |
| Fabric | `supplier` of the canonical offer in «Тканини»; `booked` and `free` summed over its colours. |
| List | `short`: pairs with `toOrder` above zero. A pair needed by plans but without a stock line stays only in «Що замовити». |

## Compatibility

Fields are added with defaults; an old client keeps working.

## Tests

- `tests/UnitTests/Warehouse/WarehouseReportTests.cs` — `Stock_shows_what_plans_book_of_each_pair_and_its_latest_movement`.
- `tests/UnitTests/Warehouse/FabricStockContractTests.cs` — the response matches the machine contract.
