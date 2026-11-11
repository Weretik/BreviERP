# WH-003 — Edit, void and restore of movements with «Історія правок» (stage 3)

**Status:** implemented 2026-11-11 (owner go «давай след етап», 2026-11-11; same PRs as stage 2). **Module:** Warehouse, schema `warehouse`. **Source:** TZ «Brevi — склад тканин» v1.0 of the old «Цех» v133, section 8, journal of edits in section 13; scenarios A28–A32. Follows [WH-002](../002-ledger-rules/README.md).

## Scope

In: edit, «Скасувати» (void) and «Скасувати останню правку» (restore) of openings, receipts, corrections and issues without an order, each saved as a revision with the values before and after, and the history of a movement. Out: movements of an issue to an order item (the TZ links them to the primary issue line and plan coverage; they move to the pair rules in stage 4 and are edited there), reversals and reversed movements (they stay as they are; «Сторно» remains for them).

## Rules

| Rule | Behaviour |
| --- | --- |
| Who | WorkshopChief and Admin (`CanMoveStock`). Rows carry `canEdit`. |
| What may change | Every kind: operation date and metres. Receipt: supplier, document, and another offer and colour of «Тканини» (colour from «Кольори»). Issue without an order: «Куди використано» and another pair the stock holds (its current line if it is in that pair, else the first line). Opening and correction keep their line. |
| Signs | Amount entered without sign: receipt and issue > 0, opening ≥ 0; a correction keeps its own sign and is ≠ 0. A void sets the change to 0. |
| Restatement | The chain of every line the movement left or joined is restated by operation date, record time and id: a changed amount or date updates all later before/after and the balance; a change of material moves the effect from the old line to the new one (A28–A30). |
| Void | The movement stays in the history, marked «Скасовано», change 0; reason 3…500 characters. A voided movement cannot be edited, voided again or reversed; a voided opening drops out of «Внесені початкові залишки». |
| Restore | Takes back the newest edit or void not taken back yet, and only while the movement still is what that revision left; otherwise 409 and the newer data stays (A31, A32). Repeated restores go back one change at a time. |
| Revision guard | Each movement has a revision number (concurrency token). Every form sends the number it shows (`expectedRevision`); another one is 409 «Рух уже змінили». |
| Operation id | Every edit, void and restore sends a new operation id, as in WH-002; a repeat returns the saved result. |
| Journal | Table `FabricMovementRevisions`: number, action (Edit/Void/Restore), line, date, change, supplier, document, destination and void mark before and after, reason, author, time, operation id, the revision a restore took back, and whether it was taken back. |

## Pages (Admin, «Рух тканини»)

Per row (for those who may move stock and only where `canEdit`): an edit icon opens the side panel with the fields the kind allows and shows the effect on the balance before saving; «Скасувати» asks for a reason and shows the effect; «Історія правок» lists the revisions with values before and after, author and time, and offers «Скасувати останню правку» when the server allows it. Voided rows show «Скасовано» and no amount.

## Contract

[warehouse/fabric-stock.openapi.yaml](../../../contracts/warehouse/fabric-stock.openapi.yaml), additive:

- `PUT /api/warehouse/fabric-stock/movements/{movementId}` (`EditFabricMovementInput`).
- `POST …/movements/{movementId}/void` (`VoidFabricMovementInput`), `POST …/movements/{movementId}/restore` (`RestoreFabricMovementInput`); all three return `FabricStockOperationResponse` (the pair the movement is on after the change).
- `GET …/movements/{movementId}/revisions` → `FabricMovementHistoryResponse`.
- `FabricMovementRowResponse` gains `isVoided`, `revision`, `canEdit`. `ReverseFabricMovementInput.reason` is now checked as the contract said (3…500).

## Data

Migration `WarehouseMovementRevisions`: `FabricMovements.IsVoided` (false), `FabricMovements.Revision` (0, concurrency token), table `FabricMovementRevisions` (unique MovementId + Number; FKs to the movement, both lines and the restored revision, all restrict). Checked on PostgreSQL 16 on the stage-2 test database: applies.

## Acceptance

| TZ | Test (`WarehouseRevisionTests`) |
| --- | --- |
| A28 | `Editing_a_receipt_restates_the_chain_and_keeps_before_and_after` |
| A29, A30 | `A_back_dated_edit_moves_the_movement_in_the_chain_and_an_issue_may_change_pair`, `A_receipt_moved_to_another_colour_restates_both_lines` |
| A31 | `Void_keeps_the_row_without_effect_and_restore_brings_it_back`, `Restores_go_back_one_change_at_a_time` |
| A32 | stale revision in `Editing_a_receipt…`; «nothing left» in `Void_keeps…` |
| §8 limits | `Edits_check_the_sign_the_kind_and_who_may_move_stock`, `A_reversal_and_a_reversed_movement_cannot_be_edited` |

## File map

```text
src/Modules/Warehouse/
  Warehouse.Domain/…/Stock/   FabricMovementRevision (+ action, state); FabricMovement.IsVoided/Revision/CanRevise/Revise; FabricStock.Revise (sign rules)
  Warehouse.Application/…/Stock/Revisions/
    MovementRevisions.cs      load with the revision guard; restate both lines, record the operation, save
    EditFabricMovementCommand.cs, VoidFabricMovementCommand.cs, RestoreFabricMovementCommand.cs
    GetFabricMovementHistoryQuery.cs
  Warehouse.Infrastructure/…/ FabricMovementRevisionConfiguration, migration WarehouseMovementRevisions
  Warehouse.Api/…/Stock/FabricStockController.cs   four endpoints
tests/UnitTests/Warehouse/WarehouseRevisionTests.cs
```
