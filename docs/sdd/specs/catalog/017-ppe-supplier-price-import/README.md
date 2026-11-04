# CAT-017 — PPE supplier price import («Імпорт цін»)

**Status:** stages 1–3 implemented 2026-11-04 (sources, discounts, Insight XML and СЕМТО PDF files; weekly АВ-Центр, 3C-Маркет, Перчатка-центр and «Оновити зараз»; daily Insight and Трєле cabinets). **Date:** 2026-11-04. **Module:** Catalog.

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
- **R-09 (stage 2):** АВ-Центр and Перчатка-центр: each active, non-manual product's supplier link is opened once (sizes sharing a link are read once, 4 pages at a time, 30 s each) with the script's user agent, and the price is found with the script's patterns in its order (`extractAvCenterPrice_`, `perchatkaCentrWebsitePrice_`). A link off the supplier's site (`avcentr.com.ua`, `perchatka-centr.com.ua`, with or without `www.`), an HTTP error or a page without a price is a problem of that product only.
- **R-10 (stage 2):** 3C-Маркет: the six published CSV lists of the script are read; the article is the third column when it is all digits (leading zeros dropped, on both sides), the price is the last number in the row after it. The lists are joined by article instead of by the product's price group; an article with different prices in two lists is a problem. If any list does not load, nothing changes and the reason is returned (or recorded for the schedule).
- **R-11 (stage 2):** The daily `brevierp-sync` job (07:00 Kyiv) runs step `ppe-supplier-prices`: on Kyiv Monday every connected website or sheet source is read and its changed prices are saved, run trigger «Розклад», actor `nightly-sync`. The script ran every two weeks on Monday at 08:00; Brevi runs weekly, which only refreshes prices more often. A source that cannot be read records a run with the reason as its problem and the step fails after the other sources ran. Other days the step does nothing.
- **R-13 (stage 3):** Insight and Трєле are read daily (and on «Оновити зараз») through the dealer cabinet like the script's last `loginToAuthenticatedSupplier_` and `loginToTrele_`: cookies are kept by hand, no automatic redirects during sign-in. Insight: open `/authorization/`, post the login as JSON and as a form with `userEmail/userPassword` and `email/password`, accept the session only when `/profile/` shows «Вийти»/«Кабінет». Трєле: open `/login/`, post `login`, `password`, `remember=1`, follow one redirect; the session is the returned cookies (the script did not verify it either). One sign-in per run, then each product link once, 2 pages at a time; the dealer price is Insight `data-offer-mod-price0` (or the price0 block value) and Трєле `add2cart__price-default` `data-price`. The cabinet gives only the dealer price: wholesale and retail stay as they are; the 20% checks of R-05 apply only to the XML files. Missing credentials, a refused sign-in or a cabinet that does not answer fail the whole read, so nothing changes. Credentials: `SupplierCabinets:{Insight,Trele}:{Login,Password}` from Secret Manager, never logged.
- **R-12 (stage 2):** «Оновити зараз» (`refreshPpeSupplierPrices`, trigger «Оновити зараз») does the same for one source: `dryRun=true` previews, `dryRun=false` reads again and saves. File sources and sources not yet connected answer 400.

## Stages

1. Tab, discounts, Insight XML and СЕМТО PDF upload.
2. Done: weekly АВ-Центр (site), 3C-Маркет (6 published Google CSV), Перчатка-центр (site) in the nightly `brevierp-sync` job on Mondays, plus «Оновити зараз».
3. Done: daily Insight and Трєле through the dealer cabinet with GCP secrets (R-13).

## API

[ppe-supplier-prices.openapi.yaml](../../../contracts/catalog/ppe-supplier-prices.openapi.yaml): `listPpePriceSources` (CatalogRead; `canChangeDiscounts` tells the Admin whether to offer the discount edit), `updatePpePriceSource` (RequireAdminRole), `importPpeSupplierPrices` (CanManageProducts, multipart `files`, `dryRun`), `refreshPpeSupplierPrices` (CanManageProducts, `dryRun`; stage 2).

## Data

Migration `AddPpePriceImport`: `catalog.PpePriceSources` (Key PK, SupplierId, DiscountPercent numeric(5,2), UpdatedAt, UpdatedBy) and `catalog.PpePriceImportRuns` (identity Id, SourceKey, Trigger, Actor, FinishedAt, counts, ProblemsJson jsonb; index (SourceKey, Id)). No seed rows. Additive. Stage 2 adds no schema.

## Tests

- `tests/UnitTests/PpeSupplierPriceImportTests.cs`: keys, supplier aliases, discount rounding, plan statuses, Insight checks, discount validator.
- `tests/UnitTests/PpeSupplierPricesContractTests.cs`: DTOs and policies match the contract.
- `tests/IntegrationTests/Pricing/Ppe/PpeSupplierFileReaderTests.cs`: Insight lists (gzip, missing, size conflict, not XML), СЕМТО PDF rows.
- `tests/IntegrationTests/Pricing/Ppe/PpeSupplierFileImportTests.cs`: preview then apply in PostgreSQL.
- `tests/IntegrationTests/Pricing/Ppe/PpePriceSourceStoreTests.cs`, `PpeSupplierPricesApiTests.cs`: storage and HTTP access, including «Оновити зараз».
- `tests/IntegrationTests/Pricing/Ppe/PpeSupplierOnlinePriceReaderTests.cs`: АВ-Центр and Перчатка-центр page patterns, 3C CSV, one request per link, a failed 3C list (fake web, no network).
- `tests/IntegrationTests/Pricing/Ppe/PpeScheduledPriceRefreshTests.cs`: the Monday step in PostgreSQL, daily cabinets on other days, job DI.
- `PpeSupplierOnlinePriceReaderTests.cs` (stage 3): cabinet price patterns, Insight sign-in once with the session cookie, missing and refused logins.

Checked against the client's sample files (2026-11-04): 3 Insight lists, 5049 offers each, all 26 Insight products matched by link with prices equal to the sheet; СЕМТО September 2026 PDF, 17 articles including all 10 products.

Stages 2–3 could not be checked against the live supplier sites, cabinets and Google lists from the development container (network blocked); check each source with «Оновити зараз» → preview in production before applying.
