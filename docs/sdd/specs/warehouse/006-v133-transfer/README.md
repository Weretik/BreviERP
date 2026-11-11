# WH-006: «Перенесення з v133» (stage 6)

**Status:** implemented 2026-11-11 (owner go «сливай и давай след етап»; full replace chosen 2026-11-11). **Module:** Warehouse, schema `warehouse` (table `StockTransfers`). **Source:** TZ «Brevi — склад тканин» v1.0 of the old «Цех» v133, sections 18 («Состав и схема переноса»), 19 («Порядок миграции и сверки»), 19.1 and 20. Follows [WH-005](../005-reports/README.md).

## Scope

In:
- Replace the whole «Склад тканин» with the stock of the old «Цех» from its D1 export (JSONL tables), Admin only, dry run first.
- The orders and items of the export reach «Швейний цех» through its existing imports; `tools/transfer/workshop_v133.py` builds their files from the same export.

Out:
- The truncated columns of the export (TZ §20): `production_items.quote_snapshot_json`, the composition cards and their history, `production_audit`. Nothing is rebuilt from them, and they are not read.
- `fabric_stock_operations` and `fabric_issue_batches.result_json` (repeat results of the old forms): old operation ids are not replayed; a new form never repeats an old one.
- `fabric_stock_audit` and `fabric_colors`: the movements keep the history of each line; colours stay as the lines name them.
- Status and details of orders the workshop already moved (the orders import skips known orders; closing them stays a «Швейний цех» action).
- Binary attachments (`production_files` keeps Drive links only).

## Decisions

| ID | Decision |
| --- | --- |
| WH-D60 | The transfer replaces the stock: revisions, movements, issues with their lines, saved item plans, stock lines and the operation journal are emptied and written again in one transaction. A dry run does the same and rolls back, so it checks exactly what an apply would write. |
| WH-D61 | Records entered in Brevi itself (no old id) are listed before the transfer; an apply that would remove them needs `confirmReplace`. Without it the answer says so and nothing changes. |
| WH-D62 | The journal `StockTransfers` keeps each applied export by the SHA-256 of its rows. The same export again, with nothing entered in Brevi since, answers «Unchanged» and writes nothing. |
| WH-D63 | Every movement is written as it was first made (the «before» of its first revision), then each revision is applied in time order through the domain (move to another line, change of date or metres, void, restore), the chains are restated, and every movement and line is compared with the export: line, date, change, before, after, void mark, balance. Any difference rolls back. |
| WH-D64 | An old item is found in Brevi by its order's old id and its position: the order of the export rows within the order, as the orders import numbered them. Its model code (or name) must match. An item of an order deleted in the old «Цех» has no Brevi item: its movements stay as issues without an order («Видалене завдання Ц-…»), and its packages are not moved. |
| WH-D65 | The products cut of a package are read from the head of its `result_json` (before any truncation); a package without them is an older issue (no count). |
| WH-D66 | A planned or replacement line whose plan has no «Тканини» code is moved as additional with a note: a plan is never bound by a similar name (TZ §19). |
| WH-D67 | The saved plan of an item issued in the old «Цех» is its `fabric_reservations` rows (main fabric) plus the planned components its lines kept, for the item's quantity in Brevi; its issues take plan version 1. |
| WH-D68 | A fabric code missing in «Тканини» keeps its lines with the old name and no price (note). |

## Checks (TZ §19.1)

Refused with the table and row id (errors), nothing written:
- A value with «[truncated for model]» in a column the transfer reads.
- A duplicate id; a link without its parent (movement → line, line → package, component → movement and line, package → item, item → order, revision → movement and lines, reservation → item, `last_movement_id` → a movement of the line).
- before + change ≠ after; a void with a change; a broken chain by operation date, record time and id; a balance that differs from the sum of its movements; a balance with no movement.
- Two lines of one code and colour key.
- A package line whose active movements do not add up to its issued metres, whose void mark differs from its movements, or whose movement belongs to another item.
- An order of the export (not deleted) that Brevi does not have, or an item whose position or model differs.

## Contract

[warehouse/fabric-stock.openapi.yaml](../../../contracts/warehouse/fabric-stock.openapi.yaml): `POST /api/warehouse/fabric-stock/transfer?dryRun&confirmReplace` takes `StockTransferInput` (nine tables, camelCase columns) and returns `StockTransferResult` (source control numbers, what Brevi gets, what it holds now, errors, notes). It replaces `POST …/fabric-stock/import` (the 23.10 copy, PRD-D32), which is removed.

## Run order

1. Sync fork (migration `WarehouseStockTransfers`).
2. `python3 tools/transfer/workshop_v133.py <export>/d1 <out>`; in «Виробничі завдання» load `production-orders.json` with «Імпорт» and `production-progress.json` with «Імпорт етапів» (preview first; known orders are skipped).
3. «Склад тканин → Залишки → Перенесення з v133»: choose the JSONL files, «Перевірити», read the control numbers and notes, then «Перенести» (with the confirmation if Brevi records are listed).

## Acceptance

| TZ | Test (`tests/UnitTests/Warehouse/WarehouseTransferTests.cs`) |
| --- | --- |
| §19 dry run, revisions, issues, plan, repeat | `Dry_run_checks_everything_and_apply_brings_the_same_stock_once` |
| §19.1, §20 | `An_export_that_does_not_add_up_or_was_cut_short_changes_nothing` |
| Replace (WH-D61) | `Records_entered_in_Brevi_are_replaced_only_when_confirmed` |
| Access | `Only_an_admin_transfers` |

Checked on the export of 11.11.2026 (v133, SQLite): 191 lines, 190 pairs, 412 movements (164 openings, 54 receipts, 187 issues, 6 adjustments, 1 voided), 5 revisions, 57 packages with 187 lines, 56 saved plans, 6 969.57 m; 0 errors, a repeat unchanged. The orders file of the 23.10 export built by the script equals the one loaded on 04.11 row for row.

## File map

```text
src/Modules/Warehouse/
  Warehouse.Domain/…/Stock/StockTransfer.cs                 journal of transferred exports
  Warehouse.Domain/…/Stock/FabricStock.cs                   Import: a movement as first made
  Warehouse.Application/…/Contracts/StockTransferContracts.cs
  Warehouse.Application/…/Stock/Transfer/
    TransferStockCommand.cs   handler: hash, checks, Brevi items, replace in a transaction, journal
    V133Source.cs             reads and checks the rows (§19.1)
    V133Writer.cs             writes lines, movements, revisions, issues and plans, then compares
  Warehouse.Infrastructure/…/Persistence/  StockTransfers, transaction, migration WarehouseStockTransfers
  Warehouse.Api/…/Stock/FabricStockController.cs            POST fabric-stock/transfer
src/Modules/Production/…/FabricPlans/ProductionFabricPlans.cs   legacy items carry model code and quantity
tools/transfer/workshop_v133.py                             orders and progress files from the export
tests/UnitTests/Warehouse/WarehouseTransferTests.cs
```
