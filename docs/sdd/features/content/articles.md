# Articles «Контент → Статті»

Articles of the public site. Each has an address, the date readers see, the «Показувати на сайті» mark, a cover and, in Ukrainian and Russian, a title, a short text for lists, SEO title and description and a body of paragraphs, headings, lists and pictures. Pictures are photos of the «Фото» library (CDN) named by id; a save with a photo that is not in the library is refused. Staff add, change, hide and delete articles; an article is saved as a whole with a revision check, and a taken address or a stale revision is refused with a Ukrainian message. Admin and the Director edit, Manager reads. [SDD](../../specs/content/004-articles/README.md), [contract](../../contracts/content/content-articles.openapi.yaml), [tables](../../architecture/data-model/content.md).

Not yet built: the Admin page (stage 2, brevi-web), the old-site transfer (stage 3), the public API and the site reading it (stage 4).

## Code structure

```text
src/Modules/Content/
  Content.Domain/Content.Domain/
    Articles/          # Article (slug, date, mark, cover, revision, Replace), ArticleText,
                       # ArticleBlock (+ cleaner), ArticleRules (limits)
  Content.Application/Content.Application/
    Abstractions/IContentMediaReader.cs   # photos of the «Фото» library by id
    Contracts/ArticleResponses.cs
    Articles/GetList/        # every article, the newest first, with the cover
    Articles/GetOne/         # editor with the photos the article uses
    Articles/Save/           # create or replace; slug, revision and photo checks
    Articles/SetVisibility/  # «Показувати на сайті» from the list
    Articles/Delete/
    Articles/Shared/         # mapping, free slug, photo read, messages
  Content.Infrastructure/Content.Infrastructure/
    Persistence/Configurations/ArticleConfiguration.cs
  Content.Api/Content.Api/
    Articles/ContentArticlesController.cs   # /api/content/articles, policy access:content-articles
src/Bootstrapper/Host.Api/Content/ContentMediaReader.cs   # IContentMediaReader over the Media module
```

## Tests

```text
tests/UnitTests/Content/ArticlesTests.cs           # dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~Articles"
tests/UnitTests/Content/ArticlesContractTests.cs
```

## Data

Schema `content`: Articles, ArticleTexts. Photo ids of the Media library are kept without a database link.
