# Site pages «Контент → Сторінки сайту»

Texts of the 16 fixed pages of the public site (home, catalog, wholesale, contacts and others) in Ukrainian and Russian: search title and description, the main text at the bottom of the page (visual editor blocks) and frequent questions, saved per page with a revision check. The list of pages is fixed in code; the migration seeds the texts the site showed before. Admin and the Director edit, Manager reads. SDD: [search texts](../../specs/content/007-site-pages/README.md), [main text and questions](../../specs/content/010-site-page-texts/README.md); [contract](../../contracts/content/content-site-pages.openapi.yaml), [tables](../../architecture/data-model/content.md).

The public site reads them without sign-in from `/api/public/site-pages` ([contract](../../contracts/content/content-public-site-pages.openapi.yaml)), rebuilt at most once a minute; an empty text or list keeps the site's own text.

## Code structure

```text
src/Modules/Content/
  Content.Domain/Content.Domain/SitePages/                          # SitePage (texts, limits), SiteFaqItem, SitePageCatalog (fixed pages)
  Content.Application/Content.Application/
    Features/SitePages/
      Queries/GetSitePages/, Commands/SaveSitePage/                 # admin list and save
      Queries/GetPublicSitePages/                                   # public read kept a minute
      Shared/                                                       # responses, mapping, messages
  Content.Infrastructure/.../Persistence/Configurations/Pages/SitePageConfiguration.cs
  Content.Infrastructure/.../Migrations/*_AddSitePages.cs           # table and the seed of the current texts
  Content.Infrastructure/.../Migrations/*_AddSitePageTextAndFaq.cs  # main text and questions, seed of home and industries
  Content.Api/Content.Api/
    Endpoints/SitePages/SitePagesEndpoints.cs                                # /api/content/site-pages, policy access:content-site-pages
    Endpoints/PublicSitePages/PublicSitePagesEndpoints.cs                    # /api/public/site-pages, anonymous
```

## Tests

```text
tests/UnitTests/Content/Site/SitePagesTests.cs     # dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~SitePages"
tests/UnitTests/Content/Site/SitePagesContractTests.cs
```

## Data

Schema `content`: SitePages (one row per fixed page).
