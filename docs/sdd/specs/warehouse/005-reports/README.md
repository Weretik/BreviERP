# WH-005: The four stock reports (stage 5)

**Status:** implemented 2026-11-11 (owner go «сливай и давай след етап», 2026-11-11). **Module:** Warehouse, schema `warehouse` (no schema change). **Source:** TZ «Brevi — склад тканин» v1.0 of the old «Цех» v133, sections 11 («План і факт»), 12 («Бронь і що замовити»), 13 («Вартість складу»), 14 («Фурнітура до замовлення») and 6.3 («На 1 виріб»). Follows [WH-004](../004-issue-by-cutting/README.md).

## Scope

In:
- Four read-only reports, each its own page of «Права доступу».
- The «На 1 виріб» value of «Рух тканини».

Out:
- The v133 data transfer (stage 6).
- Season and category in the «План і факт» grouping: production items keep a model code, not a season or category. Models group by code, or by name when there is no code.
- A saved accessory composition per item. Production items do not keep the composition of the КП. «Фурнітура до замовлення» reads today's composition of the item's Catalog model (its shared lines and the lines of its fabric variant). The TZ asks for the saved snapshot; stage 6 may bring it with the v133 data.

## Rules

### «План і факт» (TZ §11)

| Rule | Behaviour |
| --- | --- |
| Items | Items with at least one active (not reversed) issue. Items of deleted orders are left out. |
| Full issue | The `issuedQuantity` of the item's issues must add up to its quantity (of the saved plan). Less is «in progress»: left out and counted in `inProgressItems`. An issue without a count, or more products than the item has, makes the item «Потрібна перевірка». |
| Plan | The saved plan's main parts (layer Upper), for the whole quantity. No main part is «Без плану». |
| Fact | Active (not voided) movements of the lines that cover a main part (planned or replacement), plus additional lines given the main role. Lining, insulation and fleece show only in the details. |
| Comparable | Every main part has at least one line; no additional line without a role; no line covers a part that is no longer in the plan. Otherwise «Потрібна перевірка» with the reason, and the difference is null. |
| Result | Fact − plan < 0 «Економія», > 0 «Перевитрата», 0 «За планом». |
| Model and total | Orders are unique order ids. Each item counts its products once. Per product is weighted: Σ plan / Σ products (110 m for 30 products = 3,667). One item that cannot be compared makes the difference of the model and of the total unknown; the number of such items is shown. |
| Filters | Model (name or code), client, fabric (planned or issued code, name or colour). |

### «Бронь і що замовити» (TZ §12)

| Rule | Behaviour |
| --- | --- |
| Items | Items of «Нове» orders (any stage), with the saved plan when there is one. |
| Unknown | An item without a fabric, norm or metres, or a part whose fabric is neither in «Тканини» nor on the stock, goes to a separate table with the reason. It is not counted as zero. |
| Pairs | Canonical fabric and colour key. S is the sum of the pair's stock lines. |
| Queue | By order creation time, then order number, then item id. |
| Allocation | In whole hundredths: R = max(0, plan − covered); A starts at max(0, S); B = min(A, R); M = R − B. Free = max(0, S − ΣB); to order = max(0, −S) + ΣM. |
| Rows | Pairs with a need or a non-zero balance, sorted by «замовити» descending, then fabric and colour. The totals are sums of the rows. |
| Effect | None: the report books nothing and moves nothing. |

### «Вартість складу» (TZ §13)

| Rule | Behaviour |
| --- | --- |
| Price | The lowest positive price of the active offers of the fabric's alias group. An offer of a group whose composition or density no longer matches the reviewed one is skipped. Other codes take their own offer. |
| Value | Kopecks = round(hundredths × UAH per metre), half away from zero. A fabric's value is the sum of its colours' values; the total is the sum of the priced pairs. |
| No price | Price and value are «—»; the pairs and metres «Без ціни» are counted; the total is marked partial. |
| Negative balance | A separate shortage table with the positive shortage and its value. It does not lower the value of the stock. |
| Zero balance | Not valued. |
| Freshness | Each request reads balances and prices anew. The answer carries the time of the calculation and the price source. |

### «Фурнітура до замовлення» (TZ §14)

| Rule | Behaviour |
| --- | --- |
| Items | Every item of every «Нове» order. The stage does not matter. |
| Codes | FUR-001…020, 025, 027, 031, 033 and 035…039 count. FUR-021…024, 026 and 029 (textile) and FUR-032 (service) are left out. Any other FUR code, or an accessory without a code, goes to «Потреба неповна» with «не визначено». |
| Need | Norm × products for each line, summed and rounded to 6 decimals. Stock, purchases and issues are not taken off. |
| Grouping | Code, name (NFC, spaces squeezed), note and unit. Different units are never added together. |
| Unknown quantity | No composition, a norm ≤ 0, or no products: «Потреба неповна», never zero. |
| Name | A line whose reference name is gone shows its code and is marked «Потрібна перевірка назв»; its quantity still counts. |

### «На 1 виріб» (TZ §6.3)

A movement of an issue by cutting shows |metres| / the products cut for its issue, to thousandths. Other movements have no count and show «—».

## Access

New pages of «Права доступу» (group «Склад тканин»), view only. Each has its own policy:

| Page | Policy | Default |
| --- | --- | --- |
| «План і факт» | `access:fabric-plan-fact` | Admin, Manager, WorkshopChief |
| «Бронь і що замовити» | `access:fabric-booking` | Admin, Manager, WorkshopChief |
| «Вартість складу» | `access:fabric-valuation` | Admin (the owner) |
| «Фурнітура до замовлення» | `access:hardware-order` | Admin, WorkshopChief |

## Contract

[warehouse/fabric-stock.openapi.yaml](../../../contracts/warehouse/fabric-stock.openapi.yaml), additive:
- `GET /api/warehouse/reports/plan-fact?model&client&fabric` returns `FabricPlanFactResponse`.
- `GET /api/warehouse/reports/booking` returns `FabricBookingResponse`.
- `GET /api/warehouse/reports/valuation` returns `FabricValuationResponse`.
- `GET /api/warehouse/reports/hardware` returns `HardwareOrderResponse`.
- `FabricMovementRowResponse` gains `perProduct`.

## Acceptance

| TZ | Test (`tests/UnitTests/Warehouse/WarehouseReportTests.cs`) |
| --- | --- |
| §11.1, §11.3, §6.3 | `Plan_and_fact_compares_main_fabric_of_items_issued_for_all_products` |
| §11.2 | `One_item_that_cannot_be_compared_makes_the_difference_unknown` |
| §11.3 weighted example | `Per_product_figures_are_weighted_by_products` |
| §12.1 examples | `Booking_hands_the_stock_to_the_queue_and_orders_the_rest` |
| §12 queue, coverage, read only | `Booking_reads_open_plans_in_order_and_books_nothing` |
| §13 | `Valuation_takes_the_lowest_price_of_the_group_and_lists_shortages_apart`, `Value_is_kopecks_of_hundredths_times_price` |
| §14 | `Hardware_counts_physical_accessories_of_open_orders` |
| Access | `FabricStockContractTests.Every_report_is_in_the_contract_and_is_its_own_page_of_access_rights` |

## File map

```text
src/Modules/Warehouse/
  Warehouse.Application/…/Contracts/StockReportContracts.cs
  Warehouse.Application/…/Stock/Reports/
    GetFabricPlanFactQuery.cs   FabricPlanFact: item evaluation and weighted totals
    GetFabricBookingQuery.cs    FabricBooking: allocation in hundredths
    GetFabricValuationQuery.cs  FabricValuation: group price and kopecks
    GetHardwareOrderQuery.cs    HardwareNeeds: code classes and grouping
  Warehouse.Application/…/Stock/Movements/GetFabricMovementsQuery.cs   perProduct
  Warehouse.Api/…/Stock/StockReportsController.cs   /api/warehouse/reports (a policy per action)
src/Modules/Production/…/FabricPlans/
  ProductionFabricPlans.cs      items by id; model code, order creation and due date on a plan item
  ProductionAssemblies.cs       open items with the non-fabric composition lines of their model
src/Modules/Catalog/…/Sewing/ListCompositions/   lines carry their variant's article id
src/Modules/Identity/Identity.Domain/…/Authorization/AccessCatalog.cs   four pages
tests/UnitTests/Warehouse/WarehouseReportTests.cs
```
