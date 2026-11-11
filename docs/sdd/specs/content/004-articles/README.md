# CNT-004 — «Контент → Статті»: articles of the public site (Content module)

**Status:** accepted by the owner 2026-11-11 (all articles of the old site moved, articles added and edited in the Admin with SEO, pictures kept on our CDN); stage 1 (module code, admin API) implemented 2026-11-11. **Date:** 2026-11-11. **Module:** `Content`, schema `content`. Continues [CNT-002](../002-product-pages/README.md) and [CNT-003](../003-site-categories/README.md).

## Why

The new site shows 26 articles that were copied by hand into its code (`brevi-web`, `libs/site/articles`): staff cannot add or correct one, the Russian texts and SEO of the old site are not there, and only two pictures were kept. The owner asked on 2026-11-11 for articles managed in the Admin like products and categories, moved from the old site as a whole, with pictures on our CDN.

## Decision

Articles are owned by Content next to product pages and site categories: the same public site, the same two languages, the same SEO fields and visual editor. A separate module would repeat all of that for two tables. Pictures are not stored by Content: the cover and the pictures of the text are photos of the «Фото» library (Media, CDN) named by id, without a database link, read through the host port `IContentMediaReader`. The alternative (links to pictures typed as text) would let pictures stay on the old site and break when it is switched off.

## Rules

- R-01: An article has `Slug` (unique among articles, same shape as a product page), `PublishedOn` (the date readers see), `ShowOnSite`, `CoverPhotoId` (optional) and per language (`uk`, `ru`) `Title` (≤ 200), `Excerpt` (≤ 500, the short text in lists), `SeoTitle` (≤ 200), `SeoDescription` (≤ 320) and `Body`: at most 300 blocks and 60 000 characters. A block is a paragraph (with an optional bold lead), a heading, a list or an image (`PhotoId` of the library and `Alt` ≤ 200).
- R-02: The cover and every image of the bodies must be in the «Фото» library when the article is saved; otherwise the save is refused and nothing is written. A photo deleted from the library later is left out of the answers; the article is not changed.
- R-03: Staff add, change, hide and delete articles. Save replaces an article as a whole with a revision check; a taken slug is a 409; the Ukrainian title is required and gives the slug when none is typed (a repeated title gets a number). A language without any text keeps no row. Deleting removes the article and its texts, never the photos. At most 2 000 articles.
- R-04: A new article is hidden until staff mark «Показувати на сайті». The list shows the newest date first.
- R-05: Access: page key `content-articles` («Статті», group «Контент»), actions View, Create, Edit, Delete; defaults Director and Admin all, Manager view.
- R-06: Long texts are edited in the visual editor of the Admin; nothing is typed as markup.
- R-07 (stage 4): the public site reads only articles with `ShowOnSite`; an article without a text in the asked language is shown in Ukrainian with a canonical link to the Ukrainian page.

## Scenarios

- SC-01: Staff open «Контент → Статті» and see every article, the newest first, with the cover, the «на сайті» mark and what is missing (no Russian text, no SEO).
- SC-02: Staff add or edit an article in the side panel: address, date, mark, cover, titles, excerpts, SEO and text with pictures from «Фото» in both languages.
- SC-03: Staff switch «Показувати на сайті» from the list.
- SC-04: Staff delete an article.
- SC-05: A save with a photo that is not in the library, a taken address or a stale revision is refused with a Ukrainian message.
- SC-06: A one-time transfer brings every article of the old site with both languages, SEO and pictures uploaded to the CDN: preview first, then apply.
- SC-07: The site shows the list and the pages of articles from the Admin; the articles kept in the site code are removed.

## Stages (one PR each)

1. Domain, schema, admin API of articles (this repository).
2. Admin page «Контент → Статті» (brevi-web).
3. Old-site transfer with pictures to the CDN.
4. Public API of articles and the site reading it (brevi-web).

## Stage 1 design (implemented)

Admin API `/api/content/articles` ([contract](../../../contracts/content/content-articles.openapi.yaml)), policy `access:content-articles`: `getContentArticles`, `getContentArticle`, `createContentArticle`, `saveContentArticle`, `setContentArticleVisibility`, `deleteContentArticle`. Tables `Articles` and `ArticleTexts` in schema `content` (migration `AddArticles`). Validation (FluentValidation): Ukrainian title, slug shape, date between 2000 and 2100, lengths of title, excerpt and SEO, block count and characters, a photo in every image block, alt length. The slug, the revision, the article limit and the photos in the library are checked in the handler and answer 409 with a Ukrainian message. Errors: 400 validation, 404 unknown article, 409 as above.

| Path | Role |
| --- | --- |
| `Content.Domain/Articles/` | `Article` (slug, date, mark, cover, revision, `Replace`), `ArticleText`, `ArticleBlock` with its cleaner, `ArticleRules` |
| `Content.Application/Abstractions/IContentMediaReader.cs` | port to the «Фото» library |
| `Content.Application/Articles/{GetList,GetOne,Save,SetVisibility,Delete,Shared}` | one use case per folder; mapping, messages, free slug, photo read |
| `Content.Application/Contracts/ArticleResponses.cs` | responses |
| `Content.Infrastructure/Persistence/Configurations/ArticleConfiguration.cs` | tables and indexes |
| `Content.Api/Articles/` | controller and request bodies |
| `Host.Api/Content/ContentMediaReader.cs` | the port over the Media module |
| `Identity.Domain/Authorization/AccessCatalog.cs` | page `content-articles` in «Права доступу» |
| `tests/UnitTests/Content/ArticlesTests.cs`, `ArticlesContractTests.cs` | scenarios SC-01..SC-05 on SQLite; contract and «Права доступу» |

Rollout: the migration only adds two tables, so the running code keeps working; rollback is the migration `Down`. The site keeps its own articles until stage 4.

## Traceability

| Rule / scenario | Tests |
| --- | --- |
| R-01, R-03, SC-02 | `New_article_is_hidden_gets_an_address_from_its_title_and_clean_blocks`, `Save_replaces_the_article_keeps_the_address_and_needs_the_shown_revision`, `Same_title_gets_a_numbered_address_and_a_taken_address_is_refused`, `Validator_needs_a_ukrainian_title_a_valid_address_and_a_photo_in_picture_blocks` |
| R-02, SC-05 | `Cover_and_body_photos_must_be_in_the_library`, `Editor_leaves_out_a_photo_deleted_from_the_library_and_the_row_falls_back_to_the_full_photo` |
| R-04, SC-01, SC-03 | `List_shows_the_newest_first`, `Visibility_switch_raises_the_revision_only_when_it_changes`, `Seo_is_ready_only_when_both_languages_have_title_and_description` |
| SC-04 | `Delete_removes_the_article_with_its_texts` |
| R-05 | `ArticlesContractTests` |
