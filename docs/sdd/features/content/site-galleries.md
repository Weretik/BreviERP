# Site pictures «Контент → Наші роботи» and «Контент → Сертифікати»

The pictures of the site pages «Наші роботи» (photos of sewn clothing with descriptions) and «Сертифікати» (documents with titles, notes and standards). Each page is one list saved as a whole with a revision check; the migration seeds what the site showed, as files of the site, and «Перенести у бібліотеку фото» moves them into the «Фото» library in steps of 10. New pictures come from the library. Director, Admin and the SEO specialist edit, Manager reads. [SDD](../../specs/content/011-site-galleries/README.md), [contract](../../contracts/content/content-galleries.openapi.yaml), [tables](../../architecture/data-model/content.md).

The public site reads both lists without sign-in from `/api/public/galleries` ([contract](../../contracts/content/content-public-galleries.openapi.yaml)), rebuilt at most once a minute; with empty lists the site keeps its own.

## Code structure

```text
src/Modules/Content/
  Content.Domain/Content.Domain/SiteGalleries/                         # entity, item records, limits, site-file path
  Content.Application/Content.Application/
    Features/SiteGalleries/
      Queries/GetSiteGallery/, Commands/SaveSiteGallery/, Commands/TransferSiteGallery/  # admin read, save, transfer
      Queries/GetPublicSiteGalleries/                                   # public read kept a minute
      Shared/                                                           # responses, messages, addresses
  Content.Infrastructure/.../Persistence/SiteGalleries/SiteGalleryConfiguration.cs # jsonb list
  Content.Infrastructure/.../Migrations/*_AddSiteGalleries.cs           # table and the seed of the site lists
  Content.Api/Content.Api/
    SiteGalleries/                                                      # /api/content/works, /api/content/certificates
    PublicSiteGalleries/PublicSiteGalleriesController.cs                # /api/public/galleries, anonymous
```

## Tests

```text
tests/UnitTests/Content/SiteGalleriesTests.cs     # dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~SiteGalleries"
tests/UnitTests/Content/SiteGalleriesContractTests.cs
```

## Data

Schema `content`: SiteGalleries (rows `works` and `certificates`).
