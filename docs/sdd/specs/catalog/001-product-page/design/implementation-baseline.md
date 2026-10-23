# Product page — current technical baseline

## Implemented

- Domain: shared Product model, mutually exclusive Sewing/PPE details, fabrics, accessories, operations, pricing, and invariant validation.
- Infrastructure: EF mapping for detail and link tables, Catalog migration and rollback, SQL read projections, and cross-module checks for Reference data usage.
- Application: CQRS scenarios for create, full replacement, list, detail, and delete; Reference validation and enriched admin read models.
- API: `/api/v1/products` CRUD, string enums at the write boundary, OpenAPI contract, and HTTP/API integration tests.

## Remaining verification

- Manual Swagger/API scenarios for Sewing/PPE create/update and Reference delete conflicts have not yet been run against a started Host API; see `tasks/verification/05.2-delivery-documentation.md`.

## Technical rules for Reference integration

- Before changing a product, Application checks the existence of `SupplierId`, `FabricId`, `GarmentAccessoryId`, `GarmentPartOperationId`, and selected `AdditionalReferenceId` values for retail/wholesale percentages; it also checks the `%` unit for each such value.
- Product stores only reference IDs and properties of its own links; it does not copy names, prices, contact details, or reference entities.
- Deletion of a `GarmentPartOperation`, `Fabric`, or `GarmentAccessory` is rejected if the entity is used by at least one Product. Product links are not automatically cleared and no cross-module events are published for this.
- Deletion of `MediaFile` is rejected if it is linked to Product as a photo. The link must first be removed from Product.
- Sewing calculation reads the current operation duration; historical standards would require a separate technical change.

## Open performance correction

During manual `GET /api/v1/products` on 2026-08-19, the following were recorded:
`MultipleCollectionIncludeWarning`, `GetAdminProductsQuery` at 7740 ms, and HTTP at 9142 ms.
Implementation and repeat measurement are described respectively in
[admin-list-query-performance.md](admin-list-query-performance.md),
`tasks/application/03.9-admin-list-query-performance.md`, and
`tasks/verification/05.3-admin-list-query-performance-verification.md`.
