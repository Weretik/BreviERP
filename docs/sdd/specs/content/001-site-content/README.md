# CNT-001 — «Контент сайту»: site content module (plan)

**Status:** draft, planned only; not implemented. **Date:** 2026-11-03. **Module:** new module `Content`, own schema `content`, own migrations history table (see [module rules](../../../standards/module-rules.md)).

This document keeps the idea for a later AI or developer. Decided by the product owner on 2026-11-03 while moving «Товари» from the Calculator sheet ([CAT-006](../../catalog/006-products-page/README.md)).

## Idea

Catalog «Товари» keep only what the business works with: product core, composition, prices and a short description for a quote. Everything the public site shows moves to a separate module «Контент сайту» that the Admin edits on two languages (uk, ru) and the new Next.js site reads.

## Scope

- **Product pages:** per product and language: name, slug, SEO title and description, long description, information blocks, characteristic tables, photo gallery with order, alt text and main photo.
- **Category pages:** site categories built from Reference «Типи і розділи» sections ([REF-005](../../reference/005-product-sections-sheet/README.md)): name, slug, SEO texts, description and photo per language; a product belongs to categories through its section and optional extra links.
- **Articles** later, if the site needs them.
- **Photos:** move from the current public site links (temporary `ProductCatalogProfiles.WebsiteUrl`, used by quotes) to our CDN storage; then quotes take photos from Content.

## Data to move from Catalog

These exist today in schema `catalog` and are hidden on the «Товари» page; the Content module takes them over with a data migration, then Catalog drops them:

| Catalog today | Content |
| --- | --- |
| `Products.RuName`, `Products.Slug`, `Products.DescriptionUk`, `Products.DescriptionRu` | product page texts per language |
| `ProductInformationBlocks`, `ProductCharacteristicTables` / rows | product page blocks |
| `ProductPhotos` | gallery (photo ids from the Media library) |
| `ProductCategories`, `ProductCategoryLinks` | site categories (rebuilt from sections) |

Content refers to products by Catalog product ID and to sections by Reference section ID, without cross-module foreign keys.

## Open questions for the specification

- Who writes texts: manual in Admin only, or a one-time import from the current site?
- Do category slugs follow the current site URLs (301 redirects are planned in brevi-web site migration)?
- ~~Does Content own media files?~~ Decided 2026-11-03: photos live in the separate Media module ([MED-001](../../media/001-media-library/README.md)); Content stores photo ids only.

## What to implement (when the owner starts it)

1. `$backend-feature-spec` for the module: schema, aggregates, API, contract.
2. Data migration from Catalog tables above; verify; then remove the fields from Catalog.
3. Admin pages in brevi-web («Контент сайту»: products, categories, media).
4. Public read API for the Next.js site.
