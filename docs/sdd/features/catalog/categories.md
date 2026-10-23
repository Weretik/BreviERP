# Product Categories

**Static code review:** 2026-10-16. **Module:** catalog.

Admin CRUD and list for Storefront.

## Structure and relationships

[ProductCategoriesController](../../../../src/Modules/Catalog/Catalog.Api/Catalog.Api/Controllers/ProductCategoriesController.cs) routes requests to the [admin query](../../../../src/Modules/Catalog/Catalog.Application/Catalog.Application/Features/ProductCategory/GetAdminList/GetAdminProductCategoriesQueryHandler.cs), store query, and command handlers. [ProductCategory](../../../../src/Modules/Catalog/Catalog.Domain/Catalog.Domain/ProductCategories/Entities/ProductCategory.cs) is persisted through [CatalogEfRepository](../../../../src/Modules/Catalog/Catalog.Infrastructure/Catalog.Infrastructure/Repositories/CatalogEfRepository.cs). The category belongs to Catalog even though the API route starts with `/api/reference/`.

## Code structure

The following are verified entry points and key feature components; the tree does not list every supporting file.

```text
src/
└── Modules/Catalog/
    ├── Catalog.Api/Catalog.Api/Controllers/ProductCategoriesController.cs  # HTTP entry point
    ├── Catalog.Application/Catalog.Application/Features/ProductCategory/
    │   ├── Create/CreateProductCategoryCommandHandler.cs  # use-case handler
    │   ├── Delete/DeleteProductCategoryCommandHandler.cs  # use-case handler
    │   ├── GetAdminList/GetAdminProductCategoriesQueryHandler.cs  # use-case handler
    │   ├── GetStoreList/GetStoreProductCategoriesQueryHandler.cs  # use-case handler
    │   └── Update/UpdateProductCategoryCommandHandler.cs  # use-case handler
    ├── Catalog.Domain/Catalog.Domain/ProductCategories/Entities/ProductCategory.cs  # domain model
    └── Catalog.Infrastructure/Catalog.Infrastructure/Repositories/CatalogEfRepository.cs  # data access
```

## Structure diagram

```mermaid
flowchart LR
    A["ProductCategoriesController"] --> B["Admin and store query handlers"]
    A --> C["Command handlers"]
    B --> D["ProductCategory"]
    C --> D
    B --> E["Catalog repository contracts"]
    C --> E
    E -.-> F["CatalogEfRepository and CatalogReadEfRepository"]
    F --> D
```

## Related sources

[OpenAPI](../../contracts/catalog/product-dependencies.openapi.yaml) · [Admin product list SDD](../../specs/catalog/admin-product-list/README.md).
