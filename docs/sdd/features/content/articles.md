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
    Features/Articles/
      Queries/GetContentArticles/      # every article, the newest first, with the cover
      Queries/GetContentArticle/       # editor with the photos the article uses
      Commands/SaveContentArticle/     # create or replace; slug, revision and photo checks
      Commands/SetContentArticleVisibility/  # «Показувати на сайті» from the list
      Commands/DeleteContentArticle/
      Commands/ImportOldSiteArticles/  # old-site transfer: file shape, allowed picture addresses, handler
      Queries/GetPublicArticles/, GetPublicArticle/  # public list and page, language fallback
      Shared/                          # responses, mapping, free slug, photo read, messages, public source kept for a minute
  Content.Infrastructure/Content.Infrastructure/
    Persistence/Configurations/Pages/ArticleConfiguration.cs
  Content.Api/Content.Api/
    Endpoints/Articles/ContentArticlesEndpoints.cs   # /api/content/articles, policy access:content-articles
    Endpoints/PublicArticles/PublicArticlesEndpoints.cs # /api/public/articles, anonymous, read-only
src/Modules/Content/Content.Infrastructure/Content.Infrastructure/Adapters/Media/
  ContentMediaReader.cs   # IContentMediaReader over Media.Contracts IMediaFileReader
  ContentPhotoImporter.cs # IContentPhotoImporter over Media.Contracts IMediaUrlImporter
```

## Tests

```text
tests/UnitTests/Content/Articles/ArticlesTests.cs           # dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~Articles"
tests/UnitTests/Content/Articles/ArticlesContractTests.cs
tests/UnitTests/Content/Articles/ArticlesOldSiteImportTests.cs
tests/UnitTests/Content/Articles/PublicArticlesTests.cs
```

## Data

Schema `content`: Articles, ArticleTexts. Photo ids of the Media library are kept without a database link.
