# CAT-006 — «Товари» page: two tables from the Calculator sheet

**Status:** backend implemented and verified 2026-11-03; Admin page in brevi-web; sheet data import is stage 5. **Date:** 2026-11-03. **Module:** Catalog, schema `catalog`, no new module.

Stage 2 of the products transfer (plan: «Brevi: перенос «Товари» — анализ и этапы», 2026-11-03). Depends on [REF-005 product sections](../../reference/005-product-sections-sheet/README.md).

## Product owner decisions (2026-11-03)

- The page has two tables: «Швейна продукція» (sheet tab «Товари», one row per **model**, not per article) and «ЗІЗ» (tab «ЗІЗ», one row per site article).
- «Товари» keep the product core, prices and composition plus a short description for a quote (КП). Site texts, SEO, photos and site categories on two languages move to the future [Content module](../../content/001-site-content/README.md); КП are built in Crm ([QTE-001](../../crm/003-quotes/README.md)).
- A product keeps the link to its page on the current public site, as the sheet does, so quotes can take photos from there. Temporary: photos move to our CDN later.
- «Перевірено» is set only by the director. While the API is anonymous this is an agreement, not a check.
- Old Brevi products are deleted when the sheet data is imported (stage 5).

## Source columns

| Sheet | Columns used here |
| --- | --- |
| «Товари» | A `ID моделі` (MOD-…), B `Назва моделі`, D `Категорія` (= section), E `Сезон` (Літо/Зима, derived from the category), F `Активна`, G `Перевірено`, H `Дата перевірки`, K `Посилання на товар на сайті` (HYPERLINK formula), Y `Короткий опис для КП` |
| «ЗІЗ» | A `Артикул сайту`, B `Назва товару`, C `Розділ / категорія`, E `Посилання на наш сайт`, I `Активний`, L `Перевірено`, M `Дата перевірки`, P `Короткий опис для КП`. Supplier, price, rule and manual lock belong to stage 4 (PPE card) and stage 7 (supplier prices). |

## Rules

- **R-01:** A row shows code, name, section, season (sewing), active, checked with date, current site link and quote description. Code is the model ID for sewing (`Products.ModelCode`) and the site article for PPE (`ProductCatalogProfiles.SaleSku`); unique within the type; PPE needs one.
- **R-02:** The section must exist in Reference «Типи і розділи» and have the product's type. Products refer to it by ID without a database relation.
- **R-03:** Season is «Літо», «Зима» or empty, sewing only.
- **R-04:** Checking a product without a date keeps its stored date or sets today (Kyiv, dd.MM.yyyy); unchecking clears the date.
- **R-05:** The current site link is an http(s) address up to 500 characters; the quote description is up to 2000 characters.
- **R-06:** The list returns all rows of a type at once (≈124 sewing, ≈119 PPE); the Admin filters by section and searches on the client.

## Design

- The fields live in the existing `ProductCatalogProfile` (it already held season, activity, review flag/date, website URL and short description from the catalogue import and is read by quotes and suggestions). New column `SectionId` (migration `AddProductCatalogProfileSection`, additive; rollback is the migration `Down`). The profile is created when a product without one is saved.
- Domain: `Product.Overview.cs` (`Code`, `SetCode`, `SetOverview`), `ProductOverviewDetails`, `ProductCatalogProfile.SetOverview`/`SetSaleSku`.
- Application: `Features/Product/Overview/` (`GetList`, `Update`, FluentValidation). The section is read through `IProductSectionReader`, implemented in Host.Api over the Reference query.
- API: `GET /api/v1/products/overview?type=Sewing|Ppe` (anonymous), `PUT /api/v1/products/{id}/overview` (`CanManageProducts`). Contract: [product-overview.openapi.yaml](../../../contracts/catalog/product-overview.openapi.yaml).
- Content fields (Russian name, slug, descriptions, information blocks, characteristic tables, photos, site categories) stay in the database for the Content module; the «Товари» page does not show them.
- Deleting a section in Reference does not check products yet; the Admin shows an unknown section ID as «—». The check comes with the sheet import (stage 5).

## Tasks

- **TS-01 (done):** profile section column, overview list and update. Tests: `tests/UnitTests/ProductOverviewTests.cs`.
- **TS-02:** Admin page «Товари» with tabs «Швейна продукція» / «ЗІЗ» (brevi-web).
- **TS-03 (stage 5):** import both sheet tabs, delete old products.

## Verification 2026-11-03

- `dotnet test tests/UnitTests/UnitTests.csproj --filter "Product"`: 88 passed (7 in `ProductOverviewTests`). `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj`: 7 passed.
- `dotnet ef migrations has-pending-model-changes` (CatalogDbContext, Host.Seed startup, Production): see PR.
