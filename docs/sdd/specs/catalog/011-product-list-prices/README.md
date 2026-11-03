# CAT-011 — Prices on the «Товари» list and one-button recalculation

**Status:** backend implemented 2026-11-03; recalculation (R-04, R-05, `recalculateSewingPrices`) replaced by automatic publication in [CAT-012](../012-sewing-price-auto-publication/README.md). **Date:** 2026-11-03. **Module:** Catalog (prices, recalculation) and Reference (first input versions). One Reference migration (backfill), no new tables.

Stage 6 of moving the client's Calculator into Brevi. On the old site the price list showed every article with its three prices. The product owner chose list design B (one row per model with «від …» prices, an arrow opens the articles) with the side panel of design C, and «Одной кнопкой» recalculation: «Перерахувати всі» shows what is stale and by how much, then recalculates and publishes; the same button sits in a model's panel. PPE prices stay live.

## Scope

- In: a prices read for a whole product type; recalculation and publication of Sewing prices for every model or chosen ones; the first pricing input version of every fabric, accessory and operation.
- Out: the Admin list itself (brevi-web), price exports, PPE price versions (unchanged, [PPE pricing](../../../features/catalog/ppe-pricing.md)).

## API

Contracts: [product-overview.openapi.yaml](../../../contracts/catalog/product-overview.openapi.yaml), [sewing-pricing.openapi.yaml](../../../contracts/catalog/sewing-pricing.openapi.yaml).

| Operation | Endpoint | Policy |
| --- | --- | --- |
| `getProductOverviewPrices` | `GET /api/v1/products/overview/prices?type=Sewing\|Ppe` → 200 `ProductOverviewPrices[]` | `CatalogRead` |
| `recalculateSewingPrices` | `POST /api/v1/pricing/sewing/recalculation` → 200 `SewingRecalculationReport` | `CanManageProducts` + actor claim |

## Rules

- **R-01:** A Sewing Article shows its published (fixed) prices and its live prices from today's inputs; it is stale when both exist and differ. The model's «від …» is the lowest published price of each tier among active Articles. Tiers are 1–9, 10–40 and from 41 pieces (`SewingPriceTiers.ForQuantity`).
- **R-02:** A model's status is Blocked when it has no active Article or no active Article has a published or calculable price; else Stale when an Article is stale; else Unpublished when a calculable active Article has no publication; else Ready. Blockers are the calculation codes of its active Articles.
- **R-03:** A PPE row shows the live price from the current offer and rule, dated by the offer; no offer or rule means Blocked.
- **R-04:** Recalculation takes every active Article whose live price is calculable and either unpublished or different from the published one. For each it saves a calculation and publishes it, one transaction per model. A dry run (the default) only reports the changes (before → after), the unchanged count and the blocked Articles. A blocked Article keeps its published price.
- **R-05:** If an input changes during the run so that an Article is no longer calculable, the run stops with 409; models already done stay published.
- **R-06:** Every fabric, accessory and operation has pricing input versions. A new row gets revision 1 (approved, source `reference:<Kind>:<id>`) in the same save; a change of price, minutes, accessory unit or activity appends the next revision; a deleted row loses its versions. Accessory unit «м» is priced per metre, «шт.» and «послуга» per piece. The migration `BackfillReferencePricingInputVersions` gives every existing row without a version its revision 1 (source `reference-backfill:<Kind>:<id>`).
- **R-07:** The legacy baseline bootstrap (`PricingBootstrapJob`) replaces these automatic first versions with its frozen baseline; it still refuses when a settings version or another baseline exists.

Before R-06, rows imported from the sheet ([REF-002](../../reference/002-fabrics-accessories-sheet/README.md), [REF-003](../../reference/003-operations-sheet/README.md)) had no versions, so every Sewing Article was blocked (MissingMaterialCost, MissingOperationTime).

## Design

- Catalog.Application `Features/Pricing/Sewing/Recalculation/`: `SewingPriceStatusReader` (live and published price per Article, shared by both operations), `RecalculateSewingPricesCommand` + handler + `Validators/`.
- Catalog.Application `Features/Product/Overview/Prices/`: query, handler, `DTOs/ProductOverviewPrices.cs`, `Specifications/ProductIdsByTypeSpec.cs`.
- Catalog.Api: `ProductOverviewController.GetPrices`; `Controllers/Pricing/Sewing/SewingPriceRecalculationController.cs` with `Contracts/Pricing/Sewing/Recalculation/RecalculateSewingPricesRequest.cs`.
- Reference.Infrastructure `Pricing/InputVersions/Changes/`: `TrackedReferencePricingChanges` (added, modified, deleted rows), `ReferencePricingChangeAppender` (first, next, removed versions); migration `20261103184127_BackfillReferencePricingInputVersions`.

## Acceptance

- **AC-01:** After the sheet import and the backfill, prices read lists every model as Unpublished with live prices; a dry run writes nothing; applying it makes every calculable model Ready and a repeat has no changes (R-01, R-02, R-04).
- **AC-02:** Raising a fabric price makes its models Stale with the new live price, the published price unchanged (R-01, R-06).
- **AC-03:** PPE rows show the live rule price, or Blocked without an offer (R-03).
- **AC-04:** A new fabric or accessory gets revision 1, a price change revision 2, a delete removes both (R-06).

## Tasks

- **TS-01 (done):** backend, contracts, tests: `tests/IntegrationTests/CatalogSheetImportStoreTests.cs` (`The_price_list_shows_live_prices_and_recalculation_publishes_and_marks_stale_ones`), `tests/IntegrationTests/Pricing/Sewing/ReferencePricingFirstVersionTests.cs`, `tests/UnitTests/SewingPriceRecalculationValidatorTests.cs`.
- **TS-02:** brevi-web «Товари» list B with the side panel and «Перерахувати всі».
- **TS-03:** after deploy the owner runs «Перерахувати всі» on production: preview, then apply on the owner's «да».

## Verification 2026-11-03

- `dotnet test tests/IntegrationTests/IntegrationTests.csproj` (local PostgreSQL 16, UTF8): 263 passed, 110 skipped (need other services), 0 failed.
- `dotnet test tests/UnitTests/UnitTests.csproj`: all passed; `tests/ArchitectureTests`: 7 passed.
- `dotnet ef migrations has-pending-model-changes` for ReferenceDbContext and CatalogDbContext (Host.Seed startup, Production): no changes.
