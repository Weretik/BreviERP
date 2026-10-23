# Product page — Infrastructure design

**Status:** implemented in phase 02\
**Depends on:** [Domain design](domain.md)\
**Blocks:** Application product create and update scenarios

## Outcome

Shared Product data is stored in `catalog.Products`, and type-specific data is stored in detail tables using the composition model, without nullable fields for the other product type.

## Target tables

| Table | Owner | Purpose |
| --- | --- | --- |
| `Products` | Catalog | shared properties and `Type` discriminator |
| `ProductPhotos` | Catalog | existing owned photo collection |
| `ProductCategoryLinks` | Catalog | existing owned category collection |
| `SewingProductDetails` | Catalog | `MetersPerProduct`, one record per Sewing product |
| `SewingProductFabrics` | Catalog | fabrics, primary flag, order |
| `SewingProductAccessories` | Catalog | accessories, quantity, order |
| `SewingProductOperations` | Catalog | references to labor-time operations |
| `PpeProductDetails` | Catalog | supplier, manual base price, and separate modes/values for retail and wholesale percentages |

## EF Core rules

- `SewingProductDetails` and `PpeProductDetails` are mapped as detail records in one-to-one relationships, where `ProductId` is both the primary and foreign key.
- All FKs in the Product model are CLR entity properties; string-based `HasForeignKey("ProductId")`, shadow `ProductId`, and shadow surrogate `Id` are disallowed. Sewing link tables use composite PKs, not artificial EF keys.
- `RetailPricePercent` and `WholesalePricePercent` are mapped as EF Core complex types in `PpeProductDetails`; they are not separate tables and do not duplicate one another. Each percentage has one state: `Source`, `AdditionalReferenceId`, or `CustomPercent`.
- Sewing collections have composite unique keys: `(ProductId, FabricId)`, `(ProductId, GarmentAccessoryId)`, `(ProductId, GarmentPartOperationId)`.
- `Products.Slug` has a unique index within the `catalog` schema.
- Reference IDs are stored as typed value objects/scalars. EF navigation properties to Reference entities are not added across DbContext contexts for different modules.
- Reference links do not have physical cascading FKs. Before deleting a `GarmentPartOperation`, `Fabric`, or `GarmentAccessory`, Reference checks whether Catalog uses it and rejects deletion if any link exists.
- A detail table cannot replace the Domain/Application check that it matches `Product.Type`.

## Read projections

The admin list uses `GetAdminProductsSpec : Specification<Product, ProductListItem>` with
`AsNoTracking`, SQL-side filters/sorting/pagination, and a `Select` into `ProductListItem`.
The repository materializes only list-row fields and required owned collection values
(`CategoryIds`, primary photo ID), not the full `Product` aggregate. `COUNT` uses the same
parameterized specification before pagination. The list use case returns
`Ardalis.Result.PagedResult<IReadOnlyList<ProductListItem>>` with metadata in `PagedInfo`.
For non-unique sort fields (`Name`, `CreatedAt`, `UpdatedAt`), the specification adds
`Id` as a secondary key in the same direction so `Skip`/`Take` pages have a stable order.

## Migration

A new approved EF migration is required after implementing the Domain model and EF configurations. Do not edit applied migrations. Rollback must remove only the new tables until production data exists.

## Acceptance criteria

- [x] The schema has no Sewing-specific columns in `PpeProductDetails` or PPE-specific columns in `SewingProductDetails`.
- [x] A Product cannot have two detail records of the same type.
- [x] The database enforces uniqueness of link tables.
- [x] The migration applies to an empty database and does not change existing tables outside the task scope.
- [x] Domain/Application does not depend on Infrastructure or Reference EF entities.

## Verification

- Integration test `CatalogProductPersistenceTests` verifies EF mapping, PK/FK, unique constraints, and aggregate lifecycle.
- The test creates an empty temporary PostgreSQL database, applies migrations, and verifies target tables.
- The test rolls Catalog back to `20260727004045_InitialCatalog`, verifies that the new tables are gone, and removes the temporary database.
- `GetAdminProductsSpecTests` verifies SQL filters, sorting, stable secondary sort by `Id`, pagination, and projection in a temporary PostgreSQL database.
