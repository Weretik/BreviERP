# Content schema

Code model reviewed 2026-11-11. `Content.Infrastructure.Persistence.ContentDbContext` owns schema `content`; migration history is `content.__EFMigrationsHistory`. Migration `InitialContent` creates both tables; `ExtendProductPageSeoTitle` widens SeoTitle from 120 to 200 ([CNT-002](../../specs/content/002-product-pages/README.md)); `AddSiteCategories` adds the three category tables ([CNT-003](../../specs/content/003-site-categories/README.md)).

| Table | Identity / relationships / purpose |
| --- | --- |
| ProductPages | PK ProductId = Catalog product id (never generated here, no FK to `catalog`). ShowOnSite (indexed), Slug (unique, 3–120), Revision (concurrency token, grows with every change), UpdatedAtUtc, UpdatedBy (e-mail) |
| ProductPageTexts | PK (ProductId, Language); ProductId FK → ProductPages (cascade). Language `uk` or `ru`; Name, SeoTitle, SeoDescription; Summary, Description and Specs are jsonb lists of facts, text blocks and characteristic groups |
| SiteCategories | PK Id (identity). ParentId FK → SiteCategories (restrict; null for a category, set for a subcategory, two levels only). ShowOnSite, Slug (unique, 3–120), SortOrder (index with ParentId), Revision (concurrency token), UpdatedAtUtc, UpdatedBy |
| SiteCategoryTexts | PK (CategoryId, Language); CategoryId FK → SiteCategories (cascade). Name, SeoTitle, SeoDescription; Text is a jsonb list of text blocks |
| SiteCategoryProducts | PK (CategoryId, ProductId); CategoryId FK → SiteCategories (cascade). ProductId = Catalog product id (indexed, no FK to `catalog`); SortOrder inside the category |

A product without a row in ProductPages has no site page. A language without any text has no row in ProductPageTexts. Deleting a Catalog product does not delete its page here; the site and the admin list read Catalog first, so such a page is never shown.
