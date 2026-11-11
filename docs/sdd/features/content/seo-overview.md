# SEO overview «Контент → SEO-огляд»

A read-only list of what the public site misses for search: pages shown on the site (products, categories, articles, fixed site pages) with an empty or too long SEO title or description, a title repeated by another page, or no Russian texts; plus site-wide settings that are not filled (analytics, Google and Bing verification, sharing picture). Only problems are listed. No table of its own: it reads the data of the other «Контент» sections. Admin, the Director and Manager view. [SDD](../../specs/content/008-seo-overview/README.md), [contract](../../contracts/content/content-seo-overview.openapi.yaml).

## Code structure

```text
src/Modules/Content/
  Content.Application/Content.Application/
    Contracts/SeoOverviewResponses.cs
    SeoOverview/SeoChecks.cs                    # per-page checks, repeated titles
    SeoOverview/GetSeoOverviewQuery.cs          # pages of the four areas, site problems
  Content.Api/Content.Api/SeoOverview/SeoOverviewController.cs   # /api/content/seo-overview, policy access:content-seo-overview
```

## Tests

```text
tests/UnitTests/Content/SeoOverviewTests.cs     # dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~SeoOverview"
tests/UnitTests/Content/SeoOverviewContractTests.cs
```
