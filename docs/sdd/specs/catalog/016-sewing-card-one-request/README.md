# CAT-016 — Sewing card in one request

**Status:** backend implemented 2026-11-04. **Date:** 2026-11-04. **Module:** Catalog.

Client feedback items 2, 12 and 14 (2026-11-03). The sewing card made about ten requests (access, product, Articles, shared and per-Article composition, price summary, full fabric, accessory and operation lists), and its labour time was computed in the browser from the reference list, so in some cards it showed 0 while the «Товари» side panel had the server value.

## Rules

- **R-01:** `GET /api/v1/products/{productId}/articles/pricing-summary` (`getSewingArticleSummary`) answers everything the card shows: every fabric variant with its fabric id, current fabric name and price per metre, metres, live costs, three prices, blockers and the latest published prices; plus the shared composition lines and operations.
- **R-02:** A shared line carries the current reference name and unit price (null for other costs and unknown references). An operation carries the reference name and norm minutes (null when unknown). Names are read through `IProductReferenceReader` in the same request.
- **R-03:** Labour minutes and pay of the card come from the server calculation (`costs`), the same source as the «Товари» side panel.
- **R-04:** Full reference lists are needed only for choosing a value in a side panel; the Admin loads them when a panel opens (brevi-web spec 017).

## API

- [sewing-pricing.openapi.yaml](../../../contracts/catalog/sewing-pricing.openapi.yaml): `SewingArticleSummaryRow` gains `fabricId`, `fabricName`, `fabricPrice`, `fabricQuantity`; `SewingArticleSummary` gains `materials` (`SewingCardMaterial`) and `operations` (`SewingCardOperation`). Additive; no route change.

## Tests

- `tests/UnitTests/SewingArticleSummaryTests.cs`: fabric names and prices, shared lines with names, unknown references.
- `tests/UnitTests/SewingPricingContractTests.cs`: the new DTOs match the contract.
