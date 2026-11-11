# Site pages «Контент → Сторінки сайту»

Search texts of the 16 fixed pages of the public site (home, catalog, wholesale, contacts and others): title and description in Ukrainian and Russian, saved per page with a revision check. The list of pages is fixed in code; the migration seeds the texts the site showed before. Admin and the Director edit, Manager reads. [SDD](../../specs/content/007-site-pages/README.md), [contract](../../contracts/content/content-site-pages.openapi.yaml), [tables](../../architecture/data-model/content.md).

The public site reads them without sign-in from `/api/public/site-pages` ([contract](../../contracts/content/content-public-site-pages.openapi.yaml)), rebuilt at most once a minute; an empty text keeps the site's own text.

## Code structure

```text
src/Modules/Content/
  Content.Domain/Content.Domain/SitePages/                          # SitePage (texts, limits), SitePageCatalog (fixed pages)
  Content.Application/Content.Application/
    Contracts/SitePageResponses.cs
    SitePages/GetList/, SitePages/Save/, SitePages/Shared/          # admin list and save, mapping, messages
    PublicSitePages/                                                # public read kept a minute
  Content.Infrastructure/.../Configurations/SitePageConfiguration.cs
  Content.Infrastructure/.../Migrations/*_AddSitePages.cs           # table and the seed of the current texts
  Content.Api/Content.Api/
    SitePages/SitePagesController.cs                                # /api/content/site-pages, policy access:content-site-pages
    PublicSitePages/PublicSitePagesController.cs                    # /api/public/site-pages, anonymous
```

## Tests

```text
tests/UnitTests/Content/SitePagesTests.cs     # dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~SitePages"
tests/UnitTests/Content/SitePagesContractTests.cs
```

## Data

Schema `content`: SitePages (one row per fixed page).
