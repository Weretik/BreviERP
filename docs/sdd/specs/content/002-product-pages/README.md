# CNT-002 — «Контент → Товари»: product pages of the public site (Content module)

**Status:** accepted by the product owner 2026-11-11; stage 1 implemented 2026-11-11. **Date:** 2026-11-11. **Module:** new module `Content`, schema `content`, own migration history `content.__EFMigrationsHistory` ([module rules](../../../standards/module-rules.md)). Continues the plan [CNT-001](../001-site-content/README.md).

## Why

The new public site ([brevi-web site catalog](https://github.com/Weretik/brevi-web/blob/master/docs/features/site/catalog.md)) shows a copy of the old `brevi.com.ua` catalog. The owner decided on 2026-11-11 that the site must show the products of the Admin instead: a product of the site is a model with all its fabrics and their prices, staff choose which models are on the site, and the texts the old site shows for a product (short facts, description, characteristics, SEO) are kept and edited in the Admin.

## Owner decisions (2026-11-11)

- The site product is a Catalog model with its fabrics (two, or as many as the model has). PPE products of the Admin are on the site too.
- A model has a «Показувати на сайті» mark in the Admin. Only marked models reach the site.
- Prices are shown as on the old site: «від» in listings, and on the product page the price of the chosen fabric.
- The site catalog tree is built from «Типи і розділи» of the Admin, not from the categories of the old site.
- Texts and SEO of the old site are transferred in full and keep the old structure: short facts («Підходить для професій», «Стать», «Сезон», «Тканина на фото», «Особливості»), description, characteristics grouped by garment part, SEO title and description, in Ukrainian and Russian.
- The Admin gets the page «Контент» → «Товари», where this content and SEO can be changed or deleted.
- Content is a separate bounded context with its own database schema and clean architecture; the site reads it through a public API.

## Architecture decision: new module `Content`

- **Owns:** the site page of a product (visibility, slug, texts per language, SEO). Later: section pages and articles (CNT-001).
- **Does not own:** the product core, fabrics, composition and prices (Catalog), sections (Reference), photos (Media; the order and main photo stay on the Catalog card tab «Фото»).
- **Reads other modules** only through the port `IContentCatalogReader`, implemented in `Host.Api` with Mediator queries of Catalog and Reference (like `QuoteCatalogReader`). No project reference to another module, no cross-schema foreign keys: `ProductId` is stored as a value.
- **Alternative rejected:** keep the texts in Catalog (`Products.DescriptionUk`, `ProductInformationBlocks`, `ProductCharacteristicTables`). These legacy fields stay untouched in this work and are dropped by a later decision; they hold no real data (the sheet import filled only the name copy and slug).

## Rules

- R-01: A product page belongs to one Catalog product (`ProductId`). A product has at most one page. A product without a page is not on the site.
- R-02: The site shows a product only when its page has `ShowOnSite = true` and the Catalog product is active. Switching the mark never changes the Catalog product.
- R-03: `Slug` is lower-case Latin letters, digits and `-`, 3–120 characters, unique among pages, the same for both languages. An empty slug on save is built from the Ukrainian name. A slug that is already taken is a 409.
- R-04: Texts exist per language (`uk`, `ru`): `Name` (required to show on the site in that language; the Ukrainian name falls back to the Catalog name), `SeoTitle` (≤ 120), `SeoDescription` (≤ 320), `Summary` (ordered facts: label ≤ 80 and text ≤ 1000, at most 20), `Description` (ordered blocks: heading, paragraph with an optional bold lead, list; at most 200 blocks, 20 000 characters in total), `Specs` (ordered groups with a title ≤ 120 and rows «name ≤ 120 — value ≤ 500»; at most 20 groups and 60 rows per group).
- R-05: Saving a page replaces all its texts (full replacement with a revision check: a stale `revision` is a 409).
- R-06: Deleting a page removes its texts and takes the product off the site. The Catalog product stays.
- R-07: The public API is anonymous, read-only, and returns nothing internal: no cost, supplier, problems of calculation, staff names or unpublished products.
- R-08: Public prices: a sewing variant shows the three published tier prices of its Article; a PPE product shows its three rule prices. «Від» is the lowest large-wholesale price among the active variants. A variant or product without a price is shown without it («ціну уточнюйте»).
- R-09: The catalog tree of the site: two types (`Sewing`, `Ppe`) and under them the active sections of «Типи і розділи» that have at least one product on the site, in the display order of the reference.
- R-10: Access: page key `content-products`, group «Контент», actions View, Create, Edit, Delete (Create covers only the old-site transfer of stage 3, the one `POST`); defaults Director and Admin all, Manager view. A role made on «Права доступу» has exactly the rights it was given; there are no per-owner rules.

## Scenarios

- SC-01: Staff open «Контент → Товари» and see every Catalog product (both types) with code, name, section, the «на сайті» mark, and what is missing (no page, no Ukrainian or Russian text, no SEO, no photo, no price). Search by code or name; filters by type, section, and «на сайті».
- SC-02: Staff open a product, edit the slug, the texts and SEO in both languages, and save. A stale revision or a taken slug is refused with a message.
- SC-03: Staff switch «Показувати на сайті» in the list without opening the product.
- SC-04: Staff delete the page of a product; it disappears from the site.
- SC-05: A one-time transfer brings the texts and SEO of the old site: preview first (how many pages are created, updated, not matched and why), then apply. Products are matched by the old-site link of the Catalog card (`CurrentSiteUrl`), then by model code. Transferred pages get `ShowOnSite = true` only when the request says so.
- SC-06: The site reads the tree, a page of product cards of a section (photo, name, «від»), and a product by slug (texts, photos, model code, fabrics with prices). An unknown or hidden slug is 404.

## Data

Schema `content` ([data model](../../../architecture/data-model/content.md), added in stage 1):

| Table | Key | Data |
| --- | --- | --- |
| `ProductPages` | `ProductId` (Catalog product id, no FK) | `ShowOnSite`, unique `Slug`, `Revision` (concurrency), `UpdatedAtUtc`, `UpdatedBy` |
| `ProductPageTexts` | `ProductId` + `Language` (`uk`, `ru`), FK to `ProductPages` with cascade delete | `Name`, `SeoTitle`, `SeoDescription`, `Summary` jsonb, `Description` jsonb, `Specs` jsonb |

The first migration `InitialContent` creates the schema and both tables; it copies nothing. Rollback: deploy the previous image; the schema is unused by other modules.

## Contract

Admin, [content-products.openapi.yaml](../../../contracts/content/content-products.openapi.yaml) (stage 1), every operation requires `access:content-products`:

| Operation | Route | Notes |
| --- | --- | --- |
| `getContentProducts` | `GET /api/content/products` | `search`, `type`, `sectionId`, `onSite`, `page`, `pageSize` (≤ 100); rows with missing-content flags |
| `getContentProduct` | `GET /api/content/products/{productId}` | page with texts, plus read-only Catalog facts (code, section, variants with prices, photos) |
| `saveContentProduct` | `PUT /api/content/products/{productId}` | create or replace; `revision` for an existing page |
| `setContentProductVisibility` | `PATCH /api/content/products/{productId}/visibility` | `{ showOnSite }`; creates an empty page when there is none |
| `deleteContentProduct` | `DELETE /api/content/products/{productId}` | 204; 404 when there is no page |
| `importOldSiteProductPages` | `POST /api/content/products/old-site-import` | stage 3; `dryRun` true by default; edit action |

Public (stage 4), `content-public-catalog.openapi.yaml`, anonymous, `Cache-Control: public, max-age=300`:

| Operation | Route |
| --- | --- |
| `getPublicCatalogTree` | `GET /api/public/catalog/tree` |
| `getPublicCatalogProducts` | `GET /api/public/catalog/products?type=&sectionId=&page=&pageSize=` |
| `getPublicCatalogProduct` | `GET /api/public/catalog/products/{slug}` |
| `getPublicCatalogProductSlugs` | `GET /api/public/catalog/product-slugs` (for the sitemap and static paths) |

Validators (FluentValidation, one per command or query with client input): `GetContentProductsQueryValidator` (paging, lengths), `SaveContentProductCommandValidator` (R-03, R-04 shapes), `SetContentProductVisibilityCommandValidator`, `ImportOldSiteProductPagesCommandValidator`, `GetPublicCatalogProductsQueryValidator`, `GetPublicCatalogProductQueryValidator`. Uniqueness of the slug and the revision check stay in the handler and the domain.

## Planned file map

```text
src/Modules/Content/
├── Content.Domain/Content.Domain/ProductPages/        # ProductPage aggregate, ProductPageText, value lists, slug rule, errors (planned)
├── Content.Application/Content.Application/
│   ├── Abstractions/IContentCatalogReader.cs          # products, variants, prices, photos, sections from other modules (planned)
│   ├── Abstractions/IProductPageRepository.cs         # persistence port (planned)
│   └── ProductPages/{GetList,GetOne,Save,SetVisibility,Delete,OldSiteImport,Public}/  # use cases with validators (planned)
├── Content.Infrastructure/Content.Infrastructure/
│   ├── Persistence/ContentDbContext.cs, Configurations/, Migrations/  # schema `content` (planned)
│   └── DependencyInjection/ContentInfrastructureExtensions.cs
└── Content.Api/Content.Api/
    ├── Access/ContentAccess.cs                         # access:content-products
    ├── ProductPages/ContentProductsController.cs       # admin API
    └── Public/PublicCatalogController.cs               # anonymous read API (stage 4)
src/Bootstrapper/Host.Api/Content/ContentCatalogReader.cs   # port adapter over Catalog and Reference queries (planned)
src/Modules/Identity/Identity.Domain/Authorization/AccessCatalog.cs  # existing; add the area `content-products`
tests/ArchitectureTests/ContentDependencyTests.cs       # module isolation (planned)
tests/UnitTests/Content*Tests.cs                        # domain and handlers (planned)
```

## Stages

| Stage | Scope | Where | Status |
| --- | --- | --- | --- |
| 1 | Module scaffold, `ProductPages`, admin API (list, read, save, visibility, delete), access area, contract | BreviERP | implemented 2026-11-11 |
| 2 | Admin page «Контент → Товари»: list with the «на сайті» switch and missing-content marks, editor in the side panel | brevi-web | planned |
| 3 | Transfer of old-site texts and SEO (SC-05); the import file is built from the site snapshot | both | planned; applied on production only after the owner's «да» |
| 4 | Public API: tree, cards with «від», product with fabrics and prices | BreviERP | planned |
| 5 | Site reads the public API: tree from types and sections, prices, fabric choice; the old-site snapshot is removed | brevi-web | planned |

## Tasks of stage 1

- EN-01: scaffold the four projects with `dotnet new classlib`, add them to the solution, register `AddContent…` in `Host.Api`, add `ContentDependencyTests`.
- TS-01 (R-01, R-03, R-04): `ProductPage` aggregate with slug and text rules; unit tests first.
- TS-02: `ContentDbContext`, configurations, migration `InitialContent`; check on an empty database.
- TS-03 (SC-01): list query over `IContentCatalogReader` rows joined with pages; missing-content flags.
- TS-04 (SC-02, SC-03, SC-04; R-05, R-06): save, visibility, delete handlers with validators.
- TS-05 (R-10): `AccessCatalog` area, `ContentAccess`, policy on every endpoint, `ApiAccessTests` and `AccessRightsTests` updated.
- TS-06: OpenAPI contract, feature card `docs/sdd/features/content/product-pages.md`, feature map, module map, data model page.

Evidence of stage 1: `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~UnitTests.Content"` (45 tests: slug and text rules, list, editor, save, stale revision, taken address, visibility, delete, validators, contract and access defaults) and `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj` (`ContentDependencyTests`). The migration was generated with `dotnet ef migrations add InitialContent`; it runs on deploy and was not applied to a live database here.

## Risks and open points

- The list joins rows of two modules in memory (about 250 products today). When the catalog passes a few thousand products, Catalog should publish a paged read for Content.
- Section pages (Russian names of sections, their slugs and SEO texts) are not in this specification. Until they exist, the public tree gives the Ukrainian section name for both languages and a slug built from it. `[NEEDS CLARIFICATION: Russian names and SEO texts of sections — edit in «Контент → Розділи» as the next specification]`.
- Old-site products that the Admin does not have are not transferred; the preview of SC-05 lists them.
