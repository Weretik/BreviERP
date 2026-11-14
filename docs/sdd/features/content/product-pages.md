# Product pages «Контент → Товари»

The site page of each Catalog product: the «Показувати на сайті» mark, the address after `/product/`, and texts in Ukrainian and Russian (name, SEO title and description, short facts next to the photo, description blocks, characteristics by garment part). The list shows every Catalog product of both types with what its page still misses and its site categories, and narrows to one site category with its subcategories or to products in none (`categoryId`, 0 = none); a page is saved as a whole with a revision check and can be deleted without touching the product. The site shows a product only when the mark is set and the Catalog product is active. Name, code, section, photo, fabrics and prices are read from Catalog and Reference and cannot be changed here. Admin and the Director edit, Manager reads. [SDD](../../specs/content/002-product-pages/README.md), [contract](../../contracts/content/content-products.openapi.yaml), [tables](../../architecture/data-model/content.md).

`POST /api/content/products/old-site-import` is the one-time transfer of old-site texts and SEO: it matches pages to products by the old-site link of the Catalog card, counts first (`dryRun`), keeps pages that already have texts unless asked to overwrite.

`GET /api/public/catalog/*` is the catalog of the public site, open without sign-in and read-only ([contract](../../contracts/content/content-public-catalog.openapi.yaml)): the tree (two types and their active sections that have products on the site), a page of cards with «від» and the names of the active fabrics, a product by its address (texts in the asked language or Ukrainian when there are no Russian ones, photos, active fabrics with prices) and all addresses for the sitemap. It answers from a snapshot that is rebuilt at most once a minute, and every answer may be cached for five minutes.

## Code structure

```text
src/Modules/Content/
  Content.Domain/Content.Domain/
    ProductPages/      # ProductPage (mark, slug, revision, Replace), ProductPageText (cleans one language),
                       # ProductPageContent (facts, blocks, spec groups), ProductPageRules (limits, slug rule),
                       # ProductTextBlocks (block cleaner, shared with site categories)
  Content.Application/Content.Application/
    Abstractions/      # IContentDbContext, IContentCatalogReader (products of other modules)
    Contracts/         # responses of the admin API and of the public catalog
    ProductPages/GetList/        # every Catalog product joined with its page; search and filters
    ProductPages/GetOne/         # editor; offered slug for a product without a page
    ProductPages/Save/           # create or replace as a whole; revision and slug conflicts
    ProductPages/SetVisibility/  # «Показувати на сайті» from the list
    ProductPages/Delete/
    ProductPages/ImportOldSite/  # one-time transfer: link matching, dry run, overwrite
    ProductPages/Shared/         # mapping, free slug, messages
    PublicCatalog/     # snapshot of what the site shows (kept for a minute), tree, site categories, cards, product, addresses
  Content.Infrastructure/Content.Infrastructure/
    Persistence/       # ContentDbContext (schema content), configurations (jsonb lists), migrations
    DependencyInjection/
  Content.Api/Content.Api/
    ProductPages/ContentProductsController.cs   # /api/content/products, policy access:content-products
    PublicCatalog/PublicCatalogController.cs    # /api/public/catalog, anonymous, read-only
src/Modules/Content/Content.Infrastructure/Content.Infrastructure/Adapters/Catalog/ContentCatalogReader.cs  # Catalog.Contracts products and photos + Reference.Contracts sections and fabrics
```

## Tests

```text
tests/UnitTests/Content/                          # dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~UnitTests.Content"
tests/ArchitectureTests/ContentDependencyTests.cs
```

## Data

Schema `content`: ProductPages, ProductPageTexts (and the [site category](site-categories.md) tables). Catalog product id is kept without a database link.
