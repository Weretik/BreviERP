# Articles «Контент → Статті»

Articles of the public site. Each has an address, the date readers see, the «Показувати на сайті» mark, a cover and, in Ukrainian and Russian, a title, a short text for lists, SEO title and description and a body of paragraphs, headings, lists and pictures. Pictures are photos of the «Фото» library (CDN) named by id; a save with a photo that is not in the library is refused. Staff add, change, hide and delete articles; an article is saved as a whole with a revision check, and a taken address or a stale revision is refused with a Ukrainian message. Admin and the Director edit, Manager reads. [SDD](../../specs/content/004-articles/README.md), [contract](../../contracts/content/content-articles.openapi.yaml), [tables](../../architecture/data-model/content.md).

A one-time transfer brings the articles of the old site from a prepared file: preview first, then apply; pictures are downloaded from `brevi.com.ua` into «Фото», folder «Старий сайт / Статті», an existing article is kept unless «перезаписати» is asked, and a picture that failed is named in the report.

The public site reads the shown articles without sign-in from `/api/public/articles` ([contract](../../contracts/content/content-public-articles.openapi.yaml)): the list, newest first, and a page by address with SEO, text and CDN pictures; an article without a Russian text comes back in Ukrainian. Answers are rebuilt at most once a minute.

## Code structure

```text
src/Modules/Content/
  Content.Domain/Content.Domain/
    Articles/          # Article (slug, date, mark, cover, revision, Replace), ArticleText,
                       # ArticleBlock (+ cleaner), ArticleRules (limits)
  Content.Application/Content.Application/
    Abstractions/IContentMediaReader.cs   # photos of the «Фото» library by id
    Abstractions/IContentPhotoImporter.cs # download an old-site picture into «Фото»
    Contracts/ArticleResponses.cs
    Articles/GetList/        # every article, the newest first, with the cover
    Articles/GetOne/         # editor with the photos the article uses
    Articles/Save/           # create or replace; slug, revision and photo checks
    Articles/SetVisibility/  # «Показувати на сайті» from the list
    Articles/Delete/
    Articles/ImportOldSite/  # old-site transfer: file shape, allowed picture addresses, handler
    Articles/Shared/         # mapping, free slug, photo read, messages
    PublicArticles/          # public list and page, language fallback, source kept for a minute
  Content.Infrastructure/Content.Infrastructure/
    Persistence/Configurations/ArticleConfiguration.cs
  Content.Api/Content.Api/
    Articles/ContentArticlesController.cs   # /api/content/articles, policy access:content-articles
    PublicArticles/PublicArticlesController.cs # /api/public/articles, anonymous, read-only
src/Bootstrapper/Host.Api/Content/ContentMediaReader.cs   # IContentMediaReader over the Media module
src/Bootstrapper/Host.Api/Content/ContentPhotoImporter.cs # IContentPhotoImporter over the Catalog photo downloader
```

## Tests

```text
tests/UnitTests/Content/ArticlesTests.cs           # dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~Articles"
tests/UnitTests/Content/ArticlesContractTests.cs
tests/UnitTests/Content/ArticlesOldSiteImportTests.cs
tests/UnitTests/Content/PublicArticlesTests.cs
```

## Data

Schema `content`: Articles, ArticleTexts. Photo ids of the Media library are kept without a database link.
