# SEO settings «Контент → Налаштування SEO сайту»

Site-wide SEO settings of the public site: Google Analytics 4 and Google Tag Manager ids, the verification codes of Google Search Console and Bing Webmaster Tools, and the default sharing picture from the «Фото» library. One row, saved as a whole with a revision check; a code of the wrong shape or a missing photo is refused with a Ukrainian message. Admin and the Director edit, Manager reads. [SDD](../../specs/content/006-seo-settings/README.md), [contract](../../contracts/content/content-seo-settings.openapi.yaml), [tables](../../architecture/data-model/content.md).

The public site reads them without sign-in from `/api/public/site-settings` ([contract](../../contracts/content/content-public-site-settings.openapi.yaml)), rebuilt at most once a minute.

## Code structure

```text
src/Modules/Content/
  Content.Domain/Content.Domain/SiteSettings/SiteSeoSettings.cs      # entity, formats of the codes
  Content.Application/Content.Application/
    Features/SiteSettings/
      Queries/GetSiteSeoSettings/, Commands/SaveSiteSeoSettings/    # admin read and save
      Queries/GetPublicSiteSettings/                                # public read kept a minute
      Shared/                                                       # responses, picture, messages
  Content.Infrastructure/.../Persistence/SiteSettings/SiteSeoSettingsConfiguration.cs
  Content.Api/Content.Api/
    SiteSettings/SiteSeoSettingsController.cs                       # /api/content/seo-settings, policy access:content-seo-settings
    PublicSiteSettings/PublicSiteSettingsController.cs              # /api/public/site-settings, anonymous
```

## Tests

```text
tests/UnitTests/Content/SiteSeoSettingsTests.cs     # dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~SiteSeoSettings"
tests/UnitTests/Content/SiteSeoSettingsContractTests.cs
```

## Data

Schema `content`: SiteSeoSettings (one row).
