# CAT-015 — Fabric variants

**Status:** backend implemented 2026-11-03. **Date:** 2026-11-03. **Module:** Catalog.

The client's feedback on the Calculator port (items 4, 5, 7, 9, 10): «Артикули» of a sewing model are really its variants by fabric, and the composition should be one for the whole model. The product owner decided: materials and labour are shared by the model; the fabric is set in the variant itself and added last, so a variant's price is the shared part plus its fabric. The client's sheet confirms it: in all 123 models with variant fabrics every variant has exactly one fabric line, and both variants of a model use the same metres. One model (MOD-174 «Флекс») has one fabric shared by both variants.

## Scope

- In: a variant (Article) with its one fabric and metres per product; creating, changing and removing a variant; the sheet import and the existing data of a shared-only fabric.
- Out: Admin screens (brevi-web spec 016); the pricing formula, which already adds the variant's lines to the shared ones.

## Rules

- **R-01:** An Article-scoped material line can only be a Fabric, and an Article has at most one. Accessories and other costs are shared by the model.
- **R-02:** Creating a variant takes its sale SKU, activity, order, fabric and metres; the Article and its fabric line are written in one transaction.
- **R-03:** Changing a variant may change its sale SKU (still globally unique), activity, fabric and metres.
- **R-04:** A variant without saved prices can be removed with its fabric line. A variant with price history cannot be removed (409); it is switched off instead.
- **R-05:** The sheet import gives a fabric shared by the whole model to every variant that has no own fabric; the migration does the same for stored data.

## API

- [sewing-composition.openapi.yaml](../../../contracts/catalog/sewing-composition.openapi.yaml):
  - `SewingArticle` gains `fabricId` and `fabricQuantity` (null when the variant has no fabric yet).
  - `createSewingArticle` requires `fabricId` and `fabricQuantity`.
  - New `PUT /api/v1/products/{productId}/articles/{articleId}` (`updateSewingArticle`) and `DELETE …?expectedVersion=` (`deleteSewingArticle`).
  - `PUT /api/v1/products/{productId}/articles/{articleId}/composition` (`replaceSewingArticleComposition`) is removed.

## Data

- Catalog migration `MoveSharedFabricToVariants` (data only): for a model whose only fabric line is shared and whose variants have no own fabric, the line is copied to every variant and the shared one is removed; the model's composition version goes up. In the transferred sheet data this is MOD-174. Prices do not change.

## Verification

See the PR description for the commands and results.
