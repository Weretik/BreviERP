# Site categories «Контент → Розділи»

The category tree of the public site, owned by the site and not tied to «Типи і розділи»: categories and their subcategories (two levels). Each has the «Показувати на сайті» mark, an address, a place among its neighbours, and a name, SEO title, SEO description and text in Ukrainian and Russian. Staff add, change, move to another category, reorder, hide and delete them. A subcategory is on the site only together with its category. A category is saved as a whole with a revision check; a taken address, a third level or a stale revision are refused with a Ukrainian message; a category with subcategories cannot be deleted. Deleting removes the texts and the product links, never the products. Admin and the Director edit, Manager reads. [SDD](../../specs/content/003-site-categories/README.md), [contract](../../contracts/content/content-categories.openapi.yaml), [tables](../../architecture/data-model/content.md).

Not yet built (later stages of the SDD): choosing the categories of a product, the old-site transfer, and the public catalog and site reading this tree.

## Code structure

```text
src/Modules/Content/
  Content.Domain/Content.Domain/
    SiteCategories/    # SiteCategory (tree place, mark, slug, order, revision, Replace), SiteCategoryText,
                       # SiteCategoryProduct (product link), SiteCategoryRules (limits)
  Content.Application/Content.Application/
    Contracts/SiteCategoryResponses.cs
    SiteCategories/GetList/        # the tree as a flat ordered list with product counts
    SiteCategories/GetOne/         # editor
    SiteCategories/Save/           # create or replace; parent, slug and revision checks
    SiteCategories/SetVisibility/  # «Показувати на сайті» from the tree
    SiteCategories/Reorder/        # order of one parent as a whole list
    SiteCategories/Delete/
    SiteCategories/Shared/         # mapping, free slug, next place, messages
  Content.Infrastructure/Content.Infrastructure/
    Persistence/Configurations/SiteCategoryConfiguration.cs
  Content.Api/Content.Api/
    SiteCategories/SiteCategoriesController.cs   # /api/content/categories, policy access:content-categories
```

## Tests

```text
tests/UnitTests/Content/SiteCategoriesTests.cs           # dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~SiteCategories"
tests/UnitTests/Content/SiteCategoriesContractTests.cs
```

## Data

Schema `content`: SiteCategories, SiteCategoryTexts, SiteCategoryProducts. Catalog product id is kept without a database link.
