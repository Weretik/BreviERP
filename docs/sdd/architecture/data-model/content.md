# Content schema

Code model reviewed 2026-11-11. `Content.Infrastructure.Persistence.ContentDbContext` owns schema `content`; migration history is `content.__EFMigrationsHistory`. Migration `InitialContent` creates both tables; `ExtendProductPageSeoTitle` widens SeoTitle from 120 to 200 ([CNT-002](../../specs/content/002-product-pages/README.md)).

| Table | Identity / relationships / purpose |
| --- | --- |
| ProductPages | PK ProductId = Catalog product id (never generated here, no FK to `catalog`). ShowOnSite (indexed), Slug (unique, 3–120), Revision (concurrency token, grows with every change), UpdatedAtUtc, UpdatedBy (e-mail) |
| ProductPageTexts | PK (ProductId, Language); ProductId FK → ProductPages (cascade). Language `uk` or `ru`; Name, SeoTitle, SeoDescription; Summary, Description and Specs are jsonb lists of facts, text blocks and characteristic groups |

A product without a row in ProductPages has no site page. A language without any text has no row in ProductPageTexts. Deleting a Catalog product does not delete its page here; the site and the admin list read Catalog first, so such a page is never shown.
