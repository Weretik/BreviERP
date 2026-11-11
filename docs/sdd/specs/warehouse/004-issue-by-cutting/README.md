# WH-004: Issuing fabric by actual cutting (stage 4)

**Status:** implemented 2026-11-11 (owner go «сливай и след етап давай», 2026-11-11). **Module:** Warehouse, schema `warehouse`. **Source:** TZ «Brevi — склад тканин» v1.0 of the old «Цех» v133, section 7 (issue by actual cutting), section 5.2 (saved plan), section 8 (edits of issue movements); scenarios A13–A27. Follows [WH-003](../003-movement-revisions/README.md).

## Scope

In:
- An issue to an item of an open order, recorded by the number of products cut now and the real pairs taken.
- Plan and fact for each planned part.
- The plan of an item, saved at its first issue, and «Оновити план».
- A search of items for «Видача на завдання» in «Рух тканини».
- Edit and void of the movements of an order issue.

Out:
- The four reports (stage 5).
- The v133 data transfer (stage 6).
- A «на виріб» column in «Рух тканини». The response gives the issue's number of products, and the column comes with the stage 5 reports.

## Rules

| Rule | Behaviour |
| --- | --- |
| Who | WorkshopChief and Admin (`CanMoveStock`), on an open order (status «Нове»). A closed order answers 409. |
| Products cut | `issuedQuantity` n, from 1 to the item quantity N. The products are not counted against each other: the TZ allows several cuts of one item. |
| Lines | From 1 to 20 lines, each a pair the stock holds (`fabricCode` + `color`), with metres > 0. The balance may go below zero. A line names the planned part it covers (`planFabricCode` + `planColorKey`), or no part for additional fabric. An additional line may carry a role (`layer`: Upper, Lining, Insulation or Fleece). |
| Purpose | The server decides the purpose of each line. A line on the planned pair (canonical code and colour key) is «планова». A line on another pair for a part is «заміна». A line with no part is «додаткова». |
| Comparison plan | The plan of a part for this cut is round(P × n / N) to hundredths, half up (A16: 30 m × 4 / 10 = 12 m). It is counted once per part, however many lines cover it. The fact is the sum of the part's lines; the difference is fact minus plan. Fact per product is shown to 3 decimals. |
| Coverage | Each part covers min(comparison plan, plan not yet covered). The coverage goes to the part's lines in order, each up to its metres, and the last line takes the rest. The reservation of a «Нове» item is its plan minus coverage. |
| Saved plan | The first issue saves the item's plan from today's norm: quantity, plus fabric, colour, layer and metres of each part. Later norm changes do not touch the saved plan. «Оновити план» takes today's norm, raises the plan version and writes the change to the item history; when the norm has not changed it answers «Норма не змінилася». Every issue and refresh sends the plan version it shows; another version is 409. |
| Pair guard | A line may send the pair version it shows (`expectedVersion`); another version is 409. Every pair is loaded and checked before anything is spent. Lines of one pair spend it one after another, line by line, in its spending order. |
| Operation id | One confirmed form is one package and one operation. A repeat with the same data returns the card; a repeat with other data is 409. The order and plan versions are left out of the comparison, so a repeat after a lost answer still matches. |
| Links | A line may spend a pair over several stock lines (the alias group), so each movement keeps the line's id (`FabricMovements.IssueLineId`). |
| Edits of issue movements | An order-issue movement may be edited (date, metres) and voided like other movements (WH-003). Its line is then restated: metres, the largest movement's pair, and «планова» or «заміна» against the part. A voided line covers nothing, so the reservation comes back. A line spent over two stock lines keeps its pair: a change of material is refused. «Сторно» of an order issue stays the reversal of the whole package. |
| History | Every issue writes «Видано X м на N шт.» to the item history of «Швейний цех». |

## Pages (Admin)

- «Рух тканини» gets «Видача на завдання». The storekeeper searches an open item by order number, client, model or fabric. Then they enter the products cut, the issue date and the lines, with planned parts offered from the plan. Before saving, a preview step shows the number of products, the metres, the fact per product, plan and fact by part, and the pair balances before and after. The issue is saved after the storekeeper confirms it.
- The «Тканина» tab of a production item opens the same panel.
- The card shows the plan version, «Оновити план», the reason when there is no plan, and plan and fact by part for each issue.

## Contract

[warehouse/fabric-stock.openapi.yaml](../../../contracts/warehouse/fabric-stock.openapi.yaml).

New operations, under the `access:fabric-stock` policy:
- `GET /api/warehouse/fabric-stock/issue-items?search` returns `FabricIssueItemsResponse` (at most 30, newest first).
- `GET …/issue-items/{orderId}/{itemId}` returns the card `ProductionItemFabricResponse`.
- `POST …/issue-items/{orderId}/{itemId}/issues` takes `CreateFabricIssueInput`.
- `POST …/issue-items/{orderId}/{itemId}/plan` takes `RefreshItemFabricPlanInput`.

Changed operations, a breaking change for the one consumer (brevi-web, same release):
- `POST /api/warehouse/orders/{id}/items/{itemId}/fabric-issues` takes the new `CreateFabricIssueInput`.
- `CreateFabricIssueInput` gains `operationId`, `planVersion` and `issuedQuantity`.
- `FabricIssueLineInput` names the pair by `fabricCode` and `color`, with `expectedVersion`, `planFabricCode`, `planColorKey`, `layer` and `note`. It no longer sends `purpose` or `fabricId`.
- `ProductionItemFabricResponse` gains `orderNumber`, `clientName`, `modelName`, `isOpen`, `planVersion` and `planMissing`.
- `FabricPlanPartResponse` gains `layer` and `missing`.
- `FabricIssueResponse` gains `issuedQuantity` and `comparison`.
- `FabricIssueLineResponse` gains `planFabricCode`, `layer`, `note` and `isVoided`.

## Data

Migration `WarehouseIssueByCutting`:
- `FabricMovements.IssueLineId` → `FabricIssueLines` (restrict, indexed). It is filled from the old `FabricIssueLines.MovementId`, and then that column is dropped.
- `FabricIssues` gets `IssuedQuantity`, `OperationId` (unique) and `PlanVersion`.
- `FabricIssueLines` gets `PlanCoverage` (copied from `CoveredMeters` for older lines), `Layer` and `Note`.
- New tables `FabricItemPlans` (unique ItemId, Version as the concurrency token) and `FabricItemPlanParts`.

Checked on local PostgreSQL 16 with two older issue lines: up, down and up again, with links and coverage kept.

Older issues have no saved plan: the first new issue of such an item saves today's plan. Their lines keep their coverage. They have no `issuedQuantity`, so they show no comparison.

## Acceptance

| TZ | Test |
| --- | --- |
| A13–A15, A19 | `WarehouseStockHandlerTests.Plan_is_split_by_colour_and_an_issue_covers_the_plan_of_the_products_cut`: planned, replacement and additional lines, coverage of 2 of 3 products, a part not in the plan, a pair the stock never held, below zero. |
| A16, A17 | `WarehouseIssueByCuttingTests.Two_lines_of_one_part_are_compared_with_its_plan_once`, `The_plan_of_a_cut_is_rounded_half_up_to_hundredths` |
| A18, §5.2 | `The_plan_is_saved_at_the_first_issue_and_changes_only_by_refresh` |
| A20, A21 | `A_repeat_moves_nothing_and_more_products_than_the_item_has_are_refused` |
| A22–A25 | `Editing_or_voiding_an_order_issue_restates_its_line`, `A_line_spent_over_two_offers_keeps_its_pair` |
| A26, A27 | `WarehouseStockHandlerTests.Reversing_an_issue_returns_the_fabric_and_the_reservation` |

## File map

```text
src/Modules/Warehouse/
  Warehouse.Domain/…/Stock/
    FabricItemPlan.cs         saved plan of an item with parts and layers; ComparisonPlan (P × n / N, half up)
    FabricIssue.cs            issued quantity, operation id, plan version; a line holds its movements and restates from them
    FabricMovement.cs         IssueLineId; order issues are revisable
  Warehouse.Application/…/Stock/Issues/
    FabricIssueCommands.cs    issue by cutting (checks, purposes, coverage, operation), reversal of a package
    ItemPlans.cs              saved or today's plan of an item
    ItemFabricCards.cs        card: plan by part over pairs, issues with plan and fact
    RefreshItemFabricPlanCommand.cs, GetFabricIssueItemsQuery.cs
  Warehouse.Application/…/Stock/Revisions/   MovementRevisions restates the issue line; Edit refuses a material change of a split line
  Warehouse.Infrastructure/…/  FabricItemPlanConfiguration, migration WarehouseIssueByCutting
  Warehouse.Api/…/Stock/FabricStockController.cs   four issue-items endpoints
src/Modules/Production/…/FabricPlans/ProductionFabricPlans.cs   client name; every open item when no fabric is given
tests/UnitTests/Warehouse/WarehouseIssueByCuttingTests.cs
```
