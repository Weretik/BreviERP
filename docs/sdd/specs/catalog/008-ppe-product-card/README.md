# CAT-008 — PPE product card

**Status:** backend implemented 2026-11-03; Admin card in brevi-web. **Date:** 2026-11-03. **Module:** Catalog, schema `catalog`, no new module, no migration.

Stage 4 of the products transfer (plan: «Brevi: перенос «Товари» — анализ и этапы», 2026-11-03). Follows [CAT-006](../006-products-page/README.md) and [CAT-007](../007-sewing-product-card/README.md). Reuses the PPE pricing engine and offer versions of [CAT-003](../003-sheet-pricing-parity/README.md); no formula changes.

## Goal

One card per PPE product shows what the sheet tab «ЗІЗ» holds for it, plus the three prices from «Прайс на сайт»: supplier, supplier product link and article, supplier price, «Дозволити ручну ціну», pricing rule with its wholesale note, unit, and the status «Готово до розрахунку» or the reasons it cannot be calculated.

## Source columns («ЗІЗ»)

| Column | Field |
| --- | --- |
| A `Артикул сайту`, B, C, E, I, L, M, P | Product code, name, section, current site link, active, «Перевірено» and date, quote description: the «Товари» page fields of CAT-006 |
| D `Одиниця виміру` | `Unit` |
| F `Постачальник` | `SupplierId` (Reference supplier, no database relation) |
| G `Посилання на товар постачальника` | `SupplierUrl` |
| H `Ціна постачальника, грн` | `SupplierPrice` |
| K `Дозволити ручну ціну` | `AllowManualPrice` (the stage 7 supplier price job will not overwrite the price) |
| N `Правило ціноутворення` | `RuleCode` = PPE markup rule name from «Параметри ціноутворення» |
| O `Артикул постачальника` | `SupplierSku` |
| Q, R `Insight опт/роздріб XML` | `InsightWholesale`, `InsightRetail` of the current offer version (read only; uploaded in stage 7) |

The wholesale note (column M of «Прайс на сайт») is the rule's «Примітка для прайсу» in Reference; the Admin reads it from the rule list.

## Rules

- **R-01:** The supplier fields live in the product's catalog profile. A supplier is required; the link must be http(s); the price is positive with at most 2 decimals; the rule must exist in Catalog PPE rule versions.
- **R-02:** When the supplier price or the rule changes, the save appends an approved offer version (source «Картка ЗІЗ», today in Kyiv, server actor) that keeps the Insight prices, in the same transaction as the profile. Otherwise only the profile is saved.
- **R-03:** The save names the current offer version; a different current version returns 409 so a stale card cannot overwrite a newer price.
- **R-04:** The card read returns the fields, the supplier name, the current offer version and its time, and the live three-tier calculation with blocker codes. Nothing is saved on read.
- **R-05:** The quote description is not touched by the supplier save.

## Design

- Domain: `Product.SetPpeSupplier(PpeSupplierDetails)` (`Product.PpeSupplier.cs`), `ProductCatalogProfile.SetSupplier`.
- Application: `Features/Pricing/Ppe/SupplierOffer/` with `Get/` (`GetPpeSupplierOfferQuery`), `Update/` (`UpdatePpeSupplierOfferCommand`, FluentValidation validator) and `Shared/` (`PpeSupplierOffer` view, `PpeProductWithProfileSpec`, `PpeSupplierOfferReader`).
- API: `GET` (`CatalogRead`) and `PUT` (`CanManageProducts`) `/api/v1/products/{productId}/ppe-offer`. Contract: [ppe-pricing.openapi.yaml](../../../contracts/catalog/ppe-pricing.openapi.yaml).

## Tasks

- **TS-01 (done):** card read and supplier save. Tests: `tests/UnitTests/PpeSupplierOfferTests.cs`, `PpePricingContractTests`.
- **TS-02:** Admin PPE card (brevi-web).
- **TS-03 (stage 5):** fill the 119 sheet rows.
- **TS-04 (stage 7):** supplier price job and Insight XML upload respect `AllowManualPrice`.

## Verification 2026-11-03

- `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~PpeSupplierOffer|FullyQualifiedName~PpePricingContract"`: 30 passed.
- `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj`: 7 passed.
