# Site categories «Контент → Розділи»

The category tree of the public site, owned by the site and not tied to «Типи і розділи»: categories and their subcategories (two levels). Each has the «Показувати на сайті» mark, an address, a place among its neighbours, and a name, SEO title, SEO description and text in Ukrainian and Russian. Staff add, change, move to another category, reorder, hide and delete them. A subcategory is on the site only together with its category. A category is saved as a whole with a revision check; a taken address, a third level or a stale revision are refused with a Ukrainian message; a category with subcategories cannot be deleted. Deleting removes the texts and the product links, never the products. Admin and the Director edit, Manager reads. [SDD](../../specs/content/003-site-categories/README.md), [contract](../../contracts/content/content-categories.openapi.yaml), [tables](../../architecture/data-model/content.md).

A product is put into categories from its page in «Контент → Товари» (`categoryIds` of the product editor); the list of products shows how many categories each is in. `POST /api/content/categories/old-site-import` is the one-time transfer of the old-site tree with SEO, texts and products: it counts first (`dryRun`), keeps existing categories unless asked to overwrite, and only adds products.

The public catalog serves this tree (`/api/public/catalog/categories`, `categories/{slug}`, `category-slugs`, `products?category=`, `categories` of a product; code in `Content.Application/Features/Catalog/`, tests `PublicCatalogCategoriesTests.cs`). The site reads it since stage 5 (brevi-web).

## Code structure

```text
src/Modules/Content/
  Content.Domain/Content.Domain/
    SiteCategories/    # SiteCategory (tree place, mark, slug, order, revision, Replace), SiteCategoryText,
                       # SiteCategoryProduct (product link), SiteCategoryRules (limits)
  Content.Application/Content.Application/
    Features/SiteCategories/
      Queries/GetSiteCategories/              # the tree as a flat ordered list with product counts
      Queries/GetSiteCategory/                # editor
      Commands/SaveSiteCategory/              # create or replace; parent, slug and revision checks
      Commands/SetSiteCategoryVisibility/     # «Показувати на сайті» from the tree
      Commands/ReorderSiteCategories/         # order of one parent as a whole list
      Commands/DeleteSiteCategory/
      Commands/ImportOldSiteCategories/       # one-time transfer: tree, texts, products by old-site link, dry run
      Shared/                                 # responses, mapping, free slug, next place, messages
    Features/ProductPages/Shared/ProductCategoryLinks.cs   # categories of one product
  Content.Infrastructure/Content.Infrastructure/
    Persistence/Configurations/SiteCategoryConfiguration.cs
  Content.Api/Content.Api/
    SiteCategories/SiteCategoriesController.cs   # /api/content/categories, policy access:content-categories
```

## Tests

```text
tests/UnitTests/Content/SiteCategoriesTests.cs           # dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~SiteCategories"
tests/UnitTests/Content/SiteCategoriesContractTests.cs
tests/UnitTests/Content/SiteCategoryProductsTests.cs     # product categories and the old-site transfer
```

## Data

Schema `content`: SiteCategories, SiteCategoryTexts, SiteCategoryProducts. Catalog product id is kept without a database link.
