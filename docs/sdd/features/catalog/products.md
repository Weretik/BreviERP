# Catalog Products

**Static code review:** 2026-10-16. **Module:** catalog.

Create, read, full replacement, and deletion of Sewing and PPE products.

The «Товари» page ([CAT-006](../../specs/catalog/006-products-page/README.md)) lists products per type with their working fields kept in the product catalog profile: code (model ID or PPE site article), section from Reference «Типи і розділи», season, active, the director's check and date, the link to the current public site (quotes take photos from it, temporary) and the short description for a quote. Site texts, SEO and photos stay in the database for the planned [Content module](../../specs/content/001-site-content/README.md).

The pages «Новий ЗІЗ» and «Нова модель» ([CAT-009](../../specs/catalog/009-product-create-pages/README.md)) create a product through `POST /api/v1/products/overview`: the server picks the ID (largest + 1), the slug and the next MOD-XXX for an empty sewing code; a PPE product also gets its supplier fields and first offer version (source «Створення ЗІЗ»). `GET /api/v1/products/overview/code-check` checks a code before saving. The legacy `POST /api/v1/products` stays until the Content module and is not used by these pages.

The one-time sheet import ([CAT-010](../../specs/catalog/010-sheet-product-import/README.md)) `POST /api/v1/products/sheet-import` brings all models and PPE of the Calculator sheet: it matches by code then name, creates the rest, replaces sewing articles (fabric variants) and composition, appends PPE price versions only on change and, with `deleteMissing`, deletes products that are not in the sheet. A preview (`dryRun`) writes nothing; any problem stops the write.

The «Товари» list is one server request per page ([CAT-013](../../specs/catalog/013-product-list-page/README.md)): `GET /api/v1/products/overview` searches, filters, sorts and pages the rows and returns them with their published prices, fabric variants (with fabric names) and, only when a price is missing, the reasons; `GET /api/v1/products/{id}/overview` returns one row for the card. Nothing is recalculated for the list; PPE rows get the live rule price. Sewing prices are published automatically after every change of their inputs ([CAT-012](../../specs/catalog/012-sewing-price-auto-publication/README.md), see [Sewing pricing](sewing-pricing.md)).

Product photos ([CAT-018](../../specs/catalog/018-product-photos-and-old-site-import/README.md)): `GET/PUT /api/v1/products/{id}/photos` list and save the card tab «Фото» (order, main photo; saving is Admin only); the «Товари» row carries `mainPhotoUrl`, and `active=true|false` filters active or switched-off rows. `POST /api/v1/products/old-site-photo-import` brings old-site photos into the Media folder «Старий сайт» by source key and creates missing sewing models switched off; a dry run first, then parts of 20.

Products and composition lines have no review, «required» or check marks ([CAT-014](../../specs/catalog/014-remove-review-marks/README.md)): nothing blocks a price on review.

The [Sewing Article/composition extension](sewing-composition.md) adds fabric variants (sale SKU + its fabric), shared material inputs and shared operation norms through separate protected operations. Legacy replacement preserves these children and rejects a second editable norm set once new composition is configured.

## Structure and Relationships

[ProductsEndpoints](../../../../src/Modules/Catalog/Catalog.Api/Catalog.Api/Endpoints/Product/ProductsEndpoints.cs) invokes the [create handler](../../../../src/Modules/Catalog/Catalog.Application/Catalog.Application/Features/Product/Commands/CreateProduct/CreateProductHandler.cs) and the replace/delete/read handlers. [Product](../../../../src/Modules/Catalog/Catalog.Domain/Catalog.Domain/Products/Product.cs) owns categories, photos, description, and either [Sewing](../../../../src/Modules/Catalog/Catalog.Domain/Catalog.Domain/Products/Sewing/SewingProductDetails.cs) or [PPE](../../../../src/Modules/Catalog/Catalog.Domain/Catalog.Domain/Products/Ppe/PpeProductDetails.cs) details. The handler validates IDs in Reference through the read boundary, and [CatalogDbContext](../../../../src/Modules/Catalog/Catalog.Infrastructure/Catalog.Infrastructure/Persistence/CatalogDbContext.cs) persists the aggregate.

## Code Structure

Verified entry points and key feature components are shown; the tree does not list every supporting file.

```text
src/
└── Modules/Catalog/
    ├── Catalog.Api/Catalog.Api/Endpoints/Product/ProductsEndpoints.cs  # HTTP entry point (Minimal API group, one <UseCase>/ folder per use case)
    ├── Catalog.Api/Catalog.Api/Endpoints/Overview/ProductOverviewEndpoints.cs  # «Товари» page API
    ├── Catalog.Application/Catalog.Application/Features/Product/
    │   ├── Commands/CreateProduct/CreateProductHandler.cs  # use case handler
    │   ├── Commands/DeleteProduct/DeleteProductHandler.cs  # use case handler
    │   ├── Queries/GetAdminProductDetail/GetAdminProductDetailHandler.cs  # use case handler
    │   ├── Queries/GetAdminProducts/GetAdminProductsHandler.cs  # use case handler
    │   ├── Queries/GetProductCodeCheck/GetProductCodeCheckHandler.cs  # free code and next MOD-XXX
    │   ├── Commands/CreateCatalogProduct/CreateCatalogProductHandler.cs  # «Новий ЗІЗ» / «Нова модель»
    │   ├── Queries/GetProductOverviewPage/GetProductOverviewPageHandler.cs  # «Товари» page (CAT-013)
    │   ├── Queries/GetProductOverviewPage/ProductOverviewListing.cs  # search, filters, sort, page
    │   ├── Queries/GetProductOverviewItem/GetProductOverviewItemHandler.cs  # one row for the card
    │   ├── Shared/Overview/Items/ProductOverviewItemReader.cs  # rows with published prices
    │   ├── Shared/Overview/SewingModelCodes.cs  # next MOD-XXX
    │   ├── Commands/UpdateProductOverview/UpdateProductOverviewHandler.cs  # «Товари» page fields
    │   ├── Shared/Overview/ProductSlugCandidates.cs  # slug choices for a new product
    │   ├── Commands/SetProductPhotos/SetProductPhotosHandler.cs  # tab «Фото» save (CAT-018)
    │   ├── Queries/GetProductSkuIndex/GetProductSkuIndexHandler.cs  # products by sale SKU for imports
    │   ├── Commands/ImportOldSitePhotos/ImportOldSitePhotosHandler.cs  # old-site photos and models (CAT-018)
    │   ├── Commands/ImportSheetProducts/ImportSheetProductsHandler.cs  # sheet import (CAT-010)
    │   ├── Commands/ImportSheetProducts/SheetImportPlanner.cs  # pure plan and report
    │   └── Commands/ReplaceProduct/ReplaceProductHandler.cs  # use case handler
    ├── Catalog.Domain/Catalog.Domain/Products/
    │   ├── Ppe/PpeProductDetails.cs  # domain model
    │   ├── Product.cs  # domain model
    │   ├── Product.Overview.cs  # code and «Товари» page fields
    │   ├── Product.PhotoList.cs  # photo order and main photo (CAT-018)
    │   ├── ProductCatalogProfile.cs  # sheet row fields, section ID
    │   └── Sewing/SewingProductDetails.cs  # domain model
    ├── Catalog.Infrastructure/Catalog.Infrastructure/Persistence/Repositories/Products/SheetProductImportStore.cs  # one-transaction import write
    ├── Catalog.Infrastructure/Catalog.Infrastructure/Persistence/Repositories/Products/ProductOverviewVariantReader.cs  # variants, fabrics, prices in one query
    └── Catalog.Infrastructure/Catalog.Infrastructure/Persistence/CatalogDbContext.cs  # data context
```

## Structure Diagram

```mermaid
flowchart LR
    A["ProductsEndpoints"] --> B["Create, replace, delete, and read handlers"]
    B --> C["Reference ID validation"]
    B --> D["Product"]
    D --> E["Categories, photos, and description"]
    D --> F["Sewing or PPE details"]
    B --> G["Catalog repository contracts"]
    G -.-> H["CatalogEfRepository and CatalogReadEfRepository"]
    H --> I["CatalogDbContext"]
    H --> D
```

## Tests

- `tests/UnitTests/Catalog/Products/Product*Tests.cs` (incl. `ProductOverviewTests.cs`, `ProductOverviewListingTests.cs`), `tests/UnitTests/Catalog/Products/CatalogProductCreation*Tests.cs`, `tests/UnitTests/Catalog/SheetImport/CatalogSheetImport*Tests.cs`, `tests/UnitTests/Catalog/Products/Overview/GetProductListRequestMapperTests.cs`, `tests/UnitTests/Catalog/Products/ProductDomainTests.cs` (ProductDomainTests), `tests/UnitTests/Catalog/Products/Photos/ProductPhotoListTests.cs`, `tests/UnitTests/Catalog/Products/Photos/OldSitePhotoImportTests.cs`
- `tests/IntegrationTests/Catalog/Products/ProductsApiTests.cs`, `tests/IntegrationTests/Catalog/Products/ProductDeletionTests.cs`, `tests/IntegrationTests/Catalog/Products/CatalogProductPersistenceTests.cs`, `tests/IntegrationTests/Catalog/Products/GetAdminProductsSpecTests.cs`, `tests/IntegrationTests/Catalog/SheetImport/CatalogSheetImportStoreTests.cs` (list page, prices and automatic publication), `tests/IntegrationTests/Catalog/Products/ProductPhotoPersistenceTests.cs`

Focused run: `dotnet test tests/UnitTests/UnitTests.csproj --filter Product` (covers `CatalogProductCreation`); `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter Product` (slow; needs `BREVIERP_CATALOG_TEST_CONNECTION`).

**Data:** [catalog schema](../../architecture/data-model/catalog.md)

## Related Sources

[OpenAPI](../../contracts/catalog/product-catalog.openapi.yaml) · [Overview OpenAPI](../../contracts/catalog/product-overview.openapi.yaml) · [Photos OpenAPI](../../contracts/catalog/product-photos.openapi.yaml) · [Current SDD](../../specs/catalog/001-product-page/README.md).
