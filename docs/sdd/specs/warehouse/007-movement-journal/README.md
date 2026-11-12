# WH-007: «Рух тканини» search, period, kind chips and pair balance

**Status:** implemented 2026-11-12 (owner go «окей делай» on the «Склад тканин» concepts, 2026-11-12). **Module:** Warehouse, schema `warehouse` (no schema change). Follows [WH-005](../005-reports/README.md). Frontend: brevi-web «Склад тканин» UX stage 2.

## Scope

In: new query parameters and response fields of `GET /api/warehouse/fabric-stock/movements` ([contract](../../../contracts/warehouse/fabric-stock.openapi.yaml)). Read only; no new access page (it stays `fabric-stock`).

Out: new movement kinds, changes to edit, void or reversal rules (the frontend joins «Скасувати» and «Сторно» into one dialog over the existing endpoints).

## Rules

| Rule | Behaviour |
| --- | --- |
| Search | `search` (≤ 200): every word, case-insensitive after NFC, must be found in the fabric code (or its canonical code), fabric name, colour, supplier, document, order number («Ц-14» or «14»), model, destination, note or the person who recorded it. |
| Period | `from` and `to` on the operation date, inclusive; `to` before `from` is refused (400). |
| Kinds | `All`, `Receipt`, `Issue` (both issues, kept for compatibility), `Opening` (not reversed, not voided), `ItemIssue` (to an order item), `OneOffIssue` (without an item), `Adjustment`, `Cancelled` (voided, reversed, and the reversals). |
| Counts | `counts` gives each chip under every filter except the kind; `total` is the rows of the chosen kind. |
| Pair balance | `pairMetersAfter`: the running sum of every movement of the merged pair (canonical fabric + colour key, as on «Залишки») in ledger order (operation date, record time, id); a voided movement adds nothing. `metersAfter` (the line's own) stays. |
| Paging | 50 per page, newest operation date first; `hasMore` as before. |

## Compatibility

New parameters are optional and new fields are added; an old client keeps working.

## Tests

- `tests/UnitTests/Warehouse/WarehouseMovementJournalTests.cs` — pair balance across alias codes, counts under search and period, every word must match, period order.
- `tests/UnitTests/Warehouse/FabricStockContractTests.cs` — the response matches the machine contract.
