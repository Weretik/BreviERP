# WH-002 — Ledger rules and the pages «Рух тканини», «Залишки», «Початкові залишки» (stage 2)

**Status:** implemented 2026-11-11 (owner go «давай етап 2», 2026-11-11). **Module:** Warehouse, schema `warehouse`. **Source:** TZ «Brevi — склад тканин» v1.0 of the old «Цех» v133, sections 4, 6.1–6.3, 9, 10, 16, 17; scenarios A01–A13, A22–A23. Follows [WH-001](../001-warehouse-module/README.md).

## Scope

In: the ledger rules every later stage builds on, and the three pages that use only them. Out (later stages): edit, void and restore with revisions (3), issue by actual cutting with layers and plan versions (4), «План і факт», «Бронь і що замовити», «Вартість складу», «Фурнітура до замовлення» (5), the v133 data transfer (6). Until stage 3 a wrong opening, receipt, correction or issue without an order is taken back by «Сторно», as before.

## Rules

| Rule | Behaviour |
| --- | --- |
| Precision | Metres are `numeric(12,2)`: exact hundredths, never binary float. Input 0,01…1 000 000 with at most two decimals; an opening may be 0. |
| Colour key | Unicode NFC → trim → inner spaces collapsed → lower case uk-UA (`StockColor.Key`). «  Білий » and «білий» meet; different shades are never joined. The colour typed is kept for display. |
| Alias groups | Reviewed manifest `greta-023-067-sintepon-077-078-v2` (`FabricAliases`): FAB-067 → FAB-023, FAB-077 → FAB-078. No other code is joined by name, supplier or price. Lines keep their own code; the pair (canonical code + colour key) sums them. |
| Alias identity | A code of a group must still have the reviewed composition and density in «Тканини»; otherwise it is not offered and an opening or receipt with it is refused («Характеристики вибраної позиції змінилися…»). |
| Spending order | An issue or a decreasing «Уточнити» spends positive parts of the lines in order: the canonical code first, then by code and id. What is left is taken from the first line, below zero if needed. |
| Operation date | Every form sends the day the fabric really came or went (1900-01-01…2100-12-31, default today in Kyiv); the record time is kept apart. Each line's chain runs by operation date, record time, id: a back-dated movement restates before/after of the later ones. Migration `WarehouseLedgerRules` restates existing chains the same way (balances unchanged). |
| Operation id | Every stock form sends a new `operationId` (UUID). The same id with the same person and data returns the saved result (`replayed: true`) and moves nothing; with other data it is 409. Table `StockOperations` keeps the id, request hash (SHA-256 without the version guard), pair and total after. |
| Pair version | A short hash of the alias revision, code, colour key and every line's id, code, balance and row version. «Уточнити» must send it; «Разова витрата» may. A change in between is 409 «Склад уже змінили». |
| Opening | Once per pair: refused if any line of the group already has that colour, even at zero. Creates the line of the exact code picked and an Opening movement. |
| Receipt | Grows (or creates) the line of the exact offer picked; repeated receipts are allowed. Supplier is a name typed or picked (≤160), document ≤160; no counterparty id is required. |
| Issue without an order | «Разова витрата»: a pair the stock holds, metres, «Куди використано» (≤160). The balance may go below zero: the shortage is a fact, not a refusal. No order, no plan coverage. |
| «Уточнити» | The counted total of the pair with a reason of 3…500 characters → one adjustment of the difference (15,00 → 12,25 is −2,75). The new total may not be below min(0, current total): −10 → −5 is allowed, −10 → −12 is not. Same total is refused. |
| Delete | `DELETE /api/warehouse/fabric-stock/{id}` is always 410: lines keep their history; «Уточнити» brings a balance to zero. |
| Access | Policy `access:fabric-stock` reads all three pages; WorkshopChief and Admin write (`CanMoveStock`). Prices of «Тканини» on the forms only for those who see the value. |

## Pages (Admin, block «Склад тканин»)

- **«Рух тканини»** `/warehouse/movements`: «Прихід» (fabric with offer choice when a group has several, colour of «Кольори», supplier, document, date, metres) and «Разова витрата» (pair from the stock, date, metres, purpose). History 50 per page newest first with date and record time, kind, fabric/colour, supplier/document, order or purpose, receipt, issue, line balance after, «Сторно». Filters: fabric, colour, «Усі / Прихід / Витрата». «Вивантажити рух в Excel» reads every page of the current filter.
- **«Залишки»** `/warehouse/stock`: fabric (name, composition, density, canonical code), colours with metres, total; negative highlighted, zero kept. Search by fabric or colour on the page. «Уточнити» per colour. «Завантажити всі залишки Excel» exports every pair regardless of the search.
- **«Початкові залишки»** `/warehouse/opening`: the form (fabric, colour, metres from 0) and «Внесені початкові залишки» (openings not taken back, 50 per page).

## Contract

[warehouse/fabric-stock.openapi.yaml](../../../contracts/warehouse/fabric-stock.openapi.yaml). Breaking for the Admin, released with brevi-web:

- `GET /api/warehouse/fabric-stock` returns fabrics with colour pairs (no reservation or value; those return on «Бронь» and «Вартість складу» in stage 5); the `search` parameter is gone.
- New: `GET …/movements`, `POST …/openings`, `POST …/receipts`, `POST …/issues`, `POST …/adjustments`, `DELETE …/{id}` (410).
- Changed: `POST …/movements/{movementId}/reverse` (no line id or version; returns the pair).
- Removed: `GET …/{id}` (line card), `POST …/movements` (opening or receipt), `POST …/{id}/correction`.
- `choices` lists active offers with `groupCode`, composition, density, supplier and price (null for those who do not see it); suppliers are names.

## Data

Migration `WarehouseLedgerRules`: `FabricMovements.Destination` (160), `FabricMovements.OperationId` (uuid, indexed), table `StockOperations`, and the restatement of existing chains. Checked on PostgreSQL 16 on the stage-1 test database: applies, balances unchanged.

## Acceptance

| TZ | Test |
| --- | --- |
| A01–A03, A12 | `WarehouseLedgerTests.Opening_is_once_per_pair_under_any_code_of_its_group_and_may_be_zero` |
| A04, A05, A12 | `…Receipt_keeps_the_offer_picked_and_the_pair_sums_its_lines` |
| A06, A07, A13 | `…One_off_issue_spends_lines_in_order_and_may_go_below_zero` |
| A08, A09 | `…Adjustment_sets_the_counted_total_but_never_deepens_a_shortage` |
| A10 | validators (`StockRules`, `OpenFabricStockCommandValidator`, …) |
| A11 | `…Colour_key_is_nfc_trimmed_collapsed_and_lower_case`, `…Movements_are_paged_by_50_and_filtered_by_fabric_and_colour` |
| A22, A23 | `…The_same_operation_id_repeats_the_result_and_other_data_is_refused` |
| §6.3 chain | `…A_back_dated_movement_restates_the_chain_by_operation_date` |
| §5.1 identity | `…A_changed_alias_offer_cannot_be_received_as_the_same_fabric` |

## Known limits

- Two people opening the same new pair under different codes of one group at the same moment can both pass; the unique index guards only one code. Rare; stage 3 adds a pair lock with revisions.
- Order issues («Тканина» of an item) still spend the exact line picked; stage 4 moves them to the pair rules.

## File map

```text
src/Modules/Warehouse/
  Warehouse.Domain/…/Stock/         FabricAliases, StockPair (+ StockPairResult), StockOperation; FabricStock.Restate, StockColor.Key (NFC, uk-UA)
  Warehouse.Application/…/Stock/
    StockPairs.cs                   load a pair, restate touched ledgers, save with the operation, Kyiv today
    StockOperations.cs              request hash, replay
    StockRules.cs, StockList.cs     shared field rules, permissions
    Opening/, Receipts/, OneOff/, Adjustments/   the four pair commands
    Movements/                      GetFabricMovementsQuery (+ filter), ReverseFabricMovementCommand
    GetStock/, GetChoices/          «Залишки», form choices
  Warehouse.Infrastructure/…/       StockOperationConfiguration, migration WarehouseLedgerRules
  Warehouse.Api/…/Stock/FabricStockController.cs
src/Bootstrapper/Host.Api/Warehouse/WarehouseReferenceReader.cs   composition, density, supplier, active
tests/UnitTests/Warehouse/WarehouseLedgerTests.cs
```
