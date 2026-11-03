# CAT-014 — No review or check marks

**Status:** backend implemented 2026-11-03. **Date:** 2026-11-03. **Module:** Catalog, Reference.

The client's feedback on the Calculator port (items 8, 11, 12, 17, 18): nobody reviews the composition lines or checks products, so the marks «Перевірено», «Обов’язковий» and the director's check date are noise. Worse, unreviewed lines blocked the price (`UnreviewedMaterial`, `UnreviewedOperation`). The product owner asked to remove them everywhere and to clear the note «Спільна складова для всіх артикулів моделі» that the sheet repeats on every shared line.

## Scope

- In: the review fields of composition lines, the material «required» flag, the product check and its date, the PPE offer `sourceReviewed` flag, the operation norm check date; the readiness and pricing gates on them; the sheet import and the jobs that wrote them; the repeated note.
- Out: Admin screens (brevi-web spec 015); approval of pricing versions (`IsApproved`), which is a separate workflow.

## Rules

- **R-01:** A composition line has no review or required state. Readiness and pricing never block on review.
- **R-02:** A product has no check flag or check date. The sewing suggestion and the introduction quote no longer filter on it; an active product is enough.
- **R-03:** A new PPE offer version from the sheet import is dated the import day (Kyiv); the sheet check date is not read.
- **R-04:** The migration clears the note «Спільна складова для всіх артикулів моделі» on every line, and the sheet import drops it.

## API

- [sewing-composition.openapi.yaml](../../../contracts/catalog/sewing-composition.openapi.yaml): `required`, `reviewed`, `reviewedBy`, `reviewedAt` removed from the material and operation schemas.
- [product-overview.openapi.yaml](../../../contracts/catalog/product-overview.openapi.yaml): `isVerified`, `verifiedOn` removed from the item, the update and create requests and the sheet rows; `isRequired` removed from the sheet material row.
- [ppe-pricing.openapi.yaml](../../../contracts/catalog/ppe-pricing.openapi.yaml): `sourceReviewed` removed from `PpeOfferValues`.
- [sewing-pricing.openapi.yaml](../../../contracts/catalog/sewing-pricing.openapi.yaml): `isRequired`, `isReviewed` removed from the pricing rows.
- [garment-part-operations.openapi.yaml](../../../contracts/reference/garment-part-operations.openapi.yaml): `normCheckedAt` removed.

Clients that still send the removed fields are not rejected: the fields are ignored.

## Data

- Catalog migration `RemoveReviewMarks`: clears the repeated note, then drops `IsRequired`, `IsReviewed`, `ReviewedBy`, `ReviewedAt` and their check constraints from `SewingMaterialLines` and `SewingOperationLines`, and `IsSourceReviewed`, `SourceReviewDate` from `ProductCatalogProfiles`.
- Reference migration `RemoveOperationNormCheckDate`: drops `GarmentPartOperations.NormCheckedAt`.
- The dropped values are lost. `Down` restores the columns empty.

## Jobs

- `SewingPricing/Activation`: the composition confirmation only checks that the values and version match the plan; it writes nothing.
- `SewingPricing/Review`, `Verification` and `CatalogImport` no longer read or compare review state.

## Verification

Unit, architecture and HostApi tests; integration `CatalogSheetImport*` (with the transfer files), `Sewing*`, `Pricing*`, `PpePricing*`, `SewingSuggestion*`, `GarmentPart*`; `dotnet ef migrations has-pending-model-changes` for both contexts. Results are in the PR description.
