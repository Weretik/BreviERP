# CAT-017 — PPE supplier price import («Імпорт цін»)

**Status:** stage 1 implemented 2026-11-04 (sources, discounts, Insight XML and СЕМТО PDF files). Stages 2–3 planned. **Date:** 2026-11-04. **Module:** Catalog.

The client's Apps Scripts updated PPE supplier prices in the sheet «ЗІЗ»: АВ-Центр, 3C-Маркет and Перчатка-центр weekly from their sites or Google CSV with a discount; Insight and Трєле daily from the dealer cabinet; Insight and СЕМТО also from files the manager uploads. This feature moves that work into Brevi as the tab «Довідники» → «Імпорт цін».

## Decisions (owner, 2026-11-04)

- One tab lists every supplier source with its update logic, schedule and discount.
- Only an administrator or the director changes a discount (`RequireAdminRole`; the director holds Admin).
- A file supplier is updated by uploading its files: Brevi first shows what changes, and writes after confirmation.
- Prices are stored as the supplier gives them (with VAT), like the scripts did. The discount comes off that price.
- Insight and Трєле cabinet logins are the existing dealer accounts; they become GCP secrets in stage 3.

## Rules

- **R-01:** Sources are fixed in code (`PpePriceSources`): key, title, method, schedule, logic text, default discount, file kind and count. A saved row in `catalog.PpePriceSources` overrides the discount and, when chosen, the «Постачальники» row; without it the supplier is found by name aliases (e.g. the sheet typo «3C-Макркет»).
- **R-02:** Default discounts: АВ-Центр 20%, 3C-Маркет 5%, Перчатка-центр 22%, Insight/Трєле/СЕМТО 0%. A discount is 0–90% with up to 2 decimals. New price = supplier price × (100 − discount) / 100, rounded to kopecks away from zero.
- **R-03:** A product matches a file item by the supplier link first (scheme, `www`, query, trailing slash and Insight's `/ru/`, `/ua/` prefix ignored), then by the supplier article (upper case, Latin lookalikes swapped to Cyrillic, spaces, `-`, `_`, `.` removed).
- **R-04:** Products with «Дозволити ручну ціну» or not active are skipped and listed with the reason. A product of the supplier not found in the file is a problem, not a change.
- **R-05:** Insight: three YML lists in any order. Every size of a product shares its link; one link must have one price in each file, and must be in all three. Sorted prices are dealer < wholesale < retail. Wholesale and retail must be at least dealer × 1.2, retail not below wholesale; otherwise the price is not changed.
- **R-06:** СЕМТО: one PDF with a text layer. The article is the left-column word (a wrapped second line such as «winter» belongs to it); the price ("651,72", "1 633,86") is in the price column of the same row. An article with two different prices is a problem.
- **R-07:** `dryRun=true` (default) returns every product of the supplier with old and new prices and changes nothing. `dryRun=false` saves each changed price on the card and as a new approved offer version (source «Імпорт цін: {постачальник}», Kyiv date), product by product; a product whose offer changed meanwhile is reported, not overwritten. The run (who, when, counts, problems) is recorded.
- **R-08:** The Admin gzips files before upload so three 17 MB Insight lists fit Cloud Run's 32 MiB request limit; the server unpacks a gzip file and reads any other file as is. Request limit 30 MB.

## Stages

1. Tab, discounts, Insight XML and СЕМТО PDF upload (this change).
2. Weekly АВ-Центр (site), 3C-Маркет (6 published Google CSV), Перчатка-центр (site) in the nightly `brevierp-sync` job on Mondays, plus «Оновити зараз».
3. Daily Insight and Трєле through the dealer cabinet with GCP secrets.

## API

[ppe-supplier-prices.openapi.yaml](../../../contracts/catalog/ppe-supplier-prices.openapi.yaml): `listPpePriceSources` (CatalogRead; `canChangeDiscounts` tells the Admin whether to offer the discount edit), `updatePpePriceSource` (RequireAdminRole), `importPpeSupplierPrices` (CanManageProducts, multipart `files`, `dryRun`).

## Data

Migration `AddPpePriceImport`: `catalog.PpePriceSources` (Key PK, SupplierId, DiscountPercent numeric(5,2), UpdatedAt, UpdatedBy) and `catalog.PpePriceImportRuns` (identity Id, SourceKey, Trigger, Actor, FinishedAt, counts, ProblemsJson jsonb; index (SourceKey, Id)). No seed rows. Additive.

## Tests

- `tests/UnitTests/PpeSupplierPriceImportTests.cs`: keys, supplier aliases, discount rounding, plan statuses, Insight checks, discount validator.
- `tests/UnitTests/PpeSupplierPricesContractTests.cs`: DTOs and policies match the contract.
- `tests/IntegrationTests/Pricing/Ppe/PpeSupplierFileReaderTests.cs`: Insight lists (gzip, missing, size conflict, not XML), СЕМТО PDF rows.
- `tests/IntegrationTests/Pricing/Ppe/PpeSupplierFileImportTests.cs`: preview then apply in PostgreSQL.
- `tests/IntegrationTests/Pricing/Ppe/PpePriceSourceStoreTests.cs`, `PpeSupplierPricesApiTests.cs`: storage and HTTP access.

Checked against the client's sample files (2026-11-04): 3 Insight lists, 5049 offers each, all 26 Insight products matched by link with prices equal to the sheet; СЕМТО September 2026 PDF, 17 articles including all 10 products.
