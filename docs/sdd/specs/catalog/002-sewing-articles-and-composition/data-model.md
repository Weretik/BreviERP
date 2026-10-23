# Data model and migration

## Current schema and limitation

`catalog.Products` has typed `ProductId` backed by `int`, type, names and slug. `SewingProductDetails` stores one `MetersPerProduct`. `SewingProductFabrics` has PK `(ProductId, FabricId)` with primary/order but no quantity. `SewingProductAccessories` has quantity but no unit or article applicability. `SewingProductOperations` has PK `(ProductId, GarmentPartOperationId)` and no quantity, manual time or review. Reference owns suppliers, fabrics, accessories and operations. The user confirmed that the current Product catalogue is empty; target database state must be checked again before import.

## Proposed additive model

| Record | Identity and fields | Integrity |
| --- | --- | --- |
| Existing `Product` | Preserve `ProductId`; optional unique `ModelCode` for Sewing, nullable during rollout | Do not reinterpret or renumber existing IDs before live inventory. |
| `SewingArticle` | New typed `ArticleId` over `long`; required `ProductId`, immutable unique `SaleSku`, active status, sort order | Parent must be Sewing; `(ProductId, SaleSku)` also unique for local reads. Sale SKU is string to preserve leading zeros. |
| `SewingMaterialLine` | Typed `LineId` over `long`; `ProductId`, nullable `ArticleId`, kind (`Fabric`, `Accessory`, `OtherCost`), typed reference or cost code, quantity, unit, required, order, review metadata | Article must belong to Product. Exactly one kind-specific reference. Positive quantity. Common/article conflicts rejected by rule R-002. |
| `SewingOperationLine` | Typed `LineId` over `long`; `ProductId`, nullable `ArticleId` only when a later scenario approves an article-specific operation, operation ID, quantity, optional manual minutes, order, review metadata | Positive quantity/time; Reference ID exists; repeated operation can be represented by quantity without silently dropping it. |

`OtherCost` is a non-inventory input. Its owning reference and versioned unit cost must be settled before price publication. The sample `FUR-032` uses unit `послуга`; current `AdditionalReferences` allowed-unit list cannot hold it as a normal percentage/amount setting. Do not insert it into an inventory-only accessory model by coercing the unit.

New Domain IDs and foreign keys follow the [identifier strategy](../../../standards/identifier-strategy.md). New tables use EF mappings in Catalog Infrastructure and the `catalog` schema. Cross-module Reference IDs follow the current contract approach; a cross-schema FK is not assumed. Index `SaleSku` uniquely and `ProductId`/ArticleId for composition reads. Add a `ProductId`/`ArticleId` consistency check in the write use case and database where representable.

## Compatibility, rollout and rollback

1. Add nullable model code and new tables without dropping columns or changing existing Product IDs. Existing product HTTP contracts and queries continue to work.
2. Add new read/write use cases and migrate one verified product/article slice in a non-production database. Do not synthesize Article rows from every existing fabric choice; current product-to-fabric links are not sale SKUs.
3. After verifying the selected target has no Product rows, import the frozen source catalogue as new model/Product and Article records, with a recorded source-to-Brevi ID map. Reconcile all source and target counts. Match existing Reference rows separately, and establish one authoritative composition write path before Pricing consumes new lines.
4. In a later compatible release, update old Product sewing read/write projections or retire legacy links once dependent clients move. Avoid two editable copies of BOM or operations.

Rollback of the additive release is application rollback while retaining new tables. Destructive schema rollback is prohibited after data has been written without an export and dependency audit. Verify the migration against an empty test database and an existing-schema test database per [database rules](../../../standards/database-rules.md).
