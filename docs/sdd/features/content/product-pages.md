# Product pages «Контент → Товари»

The site page of each Catalog product: the «Показувати на сайті» mark, the address after `/product/`, and texts in Ukrainian and Russian (name, SEO title and description, short facts next to the photo, description blocks, characteristics by garment part). The list shows every Catalog product of both types with what its page still misses; a page is saved as a whole with a revision check and can be deleted without touching the product. The site shows a product only when the mark is set and the Catalog product is active. Name, code, section, photo, fabrics and prices are read from Catalog and Reference and cannot be changed here. Admin and the Director edit, Manager reads. [SDD](../../specs/content/002-product-pages/README.md), [contract](../../contracts/content/content-products.openapi.yaml), [tables](../../architecture/data-model/content.md).

Not built yet: the transfer of old-site texts (stage 3) and the public API for the site (stage 4).

## Code structure

```text
src/Modules/Content/
  Content.Domain/Content.Domain/
    ProductPages/      # ProductPage (mark, slug, revision, Replace), ProductPageText (cleans one language),
                       # ProductPageContent (facts, blocks, spec groups), ProductPageRules (limits, slug rule)
  Content.Application/Content.Application/
    Abstractions/      # IContentDbContext, IContentCatalogReader (products of other modules)
    Contracts/         # responses of the admin API
    ProductPages/GetList/        # every Catalog product joined with its page; search and filters
    ProductPages/GetOne/         # editor; offered slug for a product without a page
    ProductPages/Save/           # create or replace as a whole; revision and slug conflicts
    ProductPages/SetVisibility/  # «Показувати на сайті» from the list
    ProductPages/Delete/
    ProductPages/Shared/         # mapping, free slug, messages
  Content.Infrastructure/Content.Infrastructure/
    Persistence/       # ContentDbContext (schema content), configurations (jsonb lists), migrations
    DependencyInjection/
  Content.Api/Content.Api/
    ProductPages/ContentProductsController.cs   # /api/content/products, policy access:content-products
src/Bootstrapper/Host.Api/Content/ContentCatalogReader.cs   # Catalog overview + Reference sections and fabrics
```

## Tests

```text
tests/UnitTests/Content/                          # dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~UnitTests.Content"
tests/ArchitectureTests/ContentDependencyTests.cs
```

## Data

Schema `content`: ProductPages, ProductPageTexts. Catalog product id is kept without a database link.
