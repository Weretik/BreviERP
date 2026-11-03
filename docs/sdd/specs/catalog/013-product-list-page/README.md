# CAT-013 — «Товари» list in one server request

**Status:** backend implemented 2026-11-03. **Date:** 2026-11-03. **Module:** Catalog. No migrations, no new tables.

After [CAT-011](../011-product-list-prices/README.md) the «Товари» page loaded the rows, then a second request (`GET /api/v1/products/overview/prices`) recalculated the live price of every fabric variant of every model with several queries each. On Cloud Run the rows stayed «—» without prices and without the expand arrow for up to a minute. With automatic publication ([CAT-012](../012-sewing-price-auto-publication/README.md)) the published price is always current, so the list does not need to calculate anything. The product owner asked for the usual design: one request per page, with search, filters, sort and paging on the server.

## Scope

- In: `GET /api/v1/products/overview` becomes a page with prices and fabric variants; new `GET /api/v1/products/{id}/overview` for the product card; removal of `getProductOverviewPrices`.
- Out: the sewing card tabs, the calculation and publication operations (later steps of the client feedback); the Admin changes (brevi-web).

## API

[product-overview.openapi.yaml](../../../contracts/catalog/product-overview.openapi.yaml): `getProductOverview` (query `type`, `search`, `section`, `withoutPrice`, `sort`, `direction`, `page`, `pageSize`; returns `ProductOverviewPage`), `getProductOverviewItem` (`ProductOverviewItem`). `getProductOverviewPrices` and the schemas `ProductOverviewRow`, `ProductOverviewPrices`, `ProductOverviewArticlePrices` are removed. Both reads need `CatalogRead`.

## Rules

- **R-01:** Prices are read, never recalculated. A sewing variant's price is its latest publication; a model's price is the lowest of each tier among its active variants. PPE prices come from the current offer and rule (unchanged).
- **R-02:** A row has `problems` only when a price is missing: the calculation codes of each active variant without a price (`Unpublished` when it is calculable but not yet published), `NoActiveArticle` for a model without an active variant, the PPE blocker codes. Only those sewing rows on the returned page are evaluated, to give the reasons.
- **R-03:** Search covers code, name, quote description, variant SKU and fabric name, case-insensitive. `section` is a section ID or `none`. `withoutPrice` keeps rows without a price.
- **R-04:** Sort `code` is natural (MOD-2 before MOD-10, article 9 before 17; rows without a code last), `name`, `price` (1–9 pcs; rows without a price last in both directions) or `updated`; ties go by name. Page size is 1–100.
- **R-05:** `sections` and `withoutPrice` count every row of the type, for the filter chips; `total` counts the rows after search and filters.

## Design

- Catalog.Application `Features/Product/Overview/GetList/`: `GetProductOverviewPageQuery` (+ validator), `ProductOverviewListing` (filter, sort, page, counts) and DTOs; `GetOne/` for one row; `Shared/Items/ProductOverviewItemReader` builds rows from a constant number of queries.
- Catalog.Infrastructure `Products/Overview/ProductOverviewVariantReader.cs`: one SQL query for every variant with its fabric and latest publication.
- Host.Api `CatalogFabricNameReader` gives fabric names through the Reference query (`IFabricNameReader`).
- The server filters and sorts in memory after these reads: a type has a few hundred rows, and PPE prices are not stored. When a type grows to thousands of rows, move filter and sort into SQL.

Cost on the Calculator data (124 models, 248 variants, 119 PPE, local PostgreSQL): one page about 20–40 ms, instead of 2–3 s for the old prices request.

## Acceptance

- **AC-01:** After the sheet import every model has a price and every variant its fabric name; the list does not change until a publication (R-01).
- **AC-02:** A PPE without a rule has no price and has problems; the others have none (R-02).
- **AC-03:** Search by SKU and by fabric name, `withoutPrice`, sort by price and page size 1 work; an unknown sort is 400 (R-03, R-04, R-05).

## Tasks

- **TS-01 (done):** backend, contract, docs, tests: `tests/UnitTests/ProductOverviewListingTests.cs`, `tests/IntegrationTests/CatalogSheetImportStoreTests.cs` (`Sewing_prices_are_published_automatically_after_an_import_and_after_a_fabric_price_change`), `tests/IntegrationTests/CatalogSheetImportRealFileTests.cs` (list timing).
- **TS-02:** brevi-web: the list asks the server for each page; the card reads `getProductOverviewItem`.

## Verification 2026-11-03

- `dotnet test tests/UnitTests/UnitTests.csproj`: 583 passed; `tests/ArchitectureTests`: 7 passed; `tests/HostApiTests`: 16 passed.
- `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter CatalogSheetImport` (local PostgreSQL 16, with the Calculator transfer files): 5 passed; sewing page 34 ms, PPE page 41 ms.
