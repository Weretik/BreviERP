# CAT-007 — Sewing product card

**Status:** backend implemented 2026-11-03; Admin card in brevi-web. **Date:** 2026-11-03. **Module:** Catalog, schema `catalog`, no new module.

Stage 3 of the products transfer (plan: «Brevi: перенос «Товари» — анализ и этапы», 2026-11-03). Follows [CAT-006](../006-products-page/README.md). Reuses the Article/composition model of [CAT-002](../002-sewing-articles-and-composition/README.md) and the pricing engine of [CAT-003](../003-sheet-pricing-parity/README.md); no formula changes.

## Goal

One card per sewing model shows what the Calculator sheet shows across «Товари», «Прайс на сайт», «Комплектація виробів», «Додаткові матеріали моделей» and «Трудовитрати виробів»: the model fields, its site articles with live cost and three prices, shared and article-specific materials (including lining and padding), the operations with time and cost, and the status «Готово до розрахунку» or the reasons it cannot be calculated.

## Source columns

| Sheet | Columns | Where |
| --- | --- | --- |
| «Прайс на сайт» | A `Артикул сайту`, K `Активна`, N–X costs and three prices, Y `Статус розрахунку` | Article rows: live summary |
| «Комплектація виробів» | B article (empty = shared), F type, G component ID, I norm, M `Обов’язкова`, N `Коментар` | Material lines (`ArticleId` null = shared) with `Comment` |
| «Додаткові матеріали моделей» | B shared/article, C `Підкладка`/`Синтепон`, D ID, F norm | Accessory material lines (FUR-021, FUR-023 …) |
| «Трудовитрати виробів» | F `ID операції`, H `Кількість`, I `Час вручну, хв`, J time, K cost, L `Коментар`, M `Статус перевірки` | Operation lines with `Comment`; time and cost from the summary |

Site names and texts of articles (columns B–E of «Прайс на сайт», Russian) belong to the future [Content module](../../content/001-site-content/README.md).

## Rules

- **R-01:** Material and operation lines keep an optional comment up to 500 characters (trimmed, empty = none), as in the sheet.
- **R-02:** The card summary calculates every Article of the model at server time with the approved settings and reference versions, without saving anything. A blocked Article returns its blocker codes and no prices. The Admin translates the codes into the sheet's reasons («Немає трудовитрат», «Операції не підтверджено», «Немає тканини» …).
- **R-03:** The summary also returns each Article's latest published prices, so the card shows when the live price differs from the published one. Publishing stays the existing explicit action.
- **R-04:** A model with no Articles returns an empty list; the Admin shows «Немає артикулу».

## Design

- Domain: `SewingMaterialLine.Comment`, `SewingOperationLine.Comment` (`CommentMaxLength` 500). Migration `AddSewingCompositionLineComments` adds two nullable columns; rollback is the migration `Down`.
- Application: `SewingMaterialInput`/`SewingOperationInput` take an optional `comment`; the views return it. `Features/Pricing/Sewing/ArticleSummary/` (`GetSewingArticleSummaryQuery`) reuses `SewingArticlePricingEvaluator` per Article and `ISewingPublishedPriceReader`.
- API: `GET /api/v1/products/{productId}/articles/pricing-summary` (`CatalogRead`). Contracts: [sewing-pricing.openapi.yaml](../../../contracts/catalog/sewing-pricing.openapi.yaml), [sewing-composition.openapi.yaml](../../../contracts/catalog/sewing-composition.openapi.yaml).

## Tasks

- **TS-01 (done):** line comments, card summary query and endpoint. Tests: `tests/UnitTests/SewingArticleSummaryTests.cs`.
- **TS-02:** Admin sewing card with tabs «Артикули», «Комплектація», «Трудовитрати», «Розрахунок» (brevi-web).
- **TS-03 (stage 5):** fill articles, materials, lining/padding, operations and comments from the sheet.
