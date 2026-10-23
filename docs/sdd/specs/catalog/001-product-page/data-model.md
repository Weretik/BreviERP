# Product page — data model

## Accepted storage strategy

The composition model is used: a shared Product table with a `Type` discriminator and separate detail tables for each type. These are not two independent product entities or one wide table with many nullable fields.

```text
catalog.Products
├── Id, Name, RuName, Slug, Type, DescriptionUk, DescriptionRu, CreatedAt, UpdatedAt, IsDeleted
├── ProductPhotos
└── ProductCategoryLinks

catalog.ProductInformationBlocks
├── Id, ProductId, TitleUk, TitleRu, TextUk, TextRu, SortOrder

catalog.ProductCharacteristicTables
├── Id, ProductId, TitleUk, TitleRu, SortOrder
└── ProductCharacteristicRows
    └── Id, TableId, LabelUk, LabelRu, ValueUk, ValueRu, SortOrder

catalog.SewingProductDetails             only Type = Sewing
├── ProductId (PK + FK → Products)
└── MetersPerProduct

catalog.SewingProductFabrics
├── ProductId
├── FabricId
├── IsPrimary                              no more than two `true` per Product
└── SortOrder

catalog.SewingProductAccessories
├── ProductId
├── GarmentAccessoryId
├── Quantity
└── SortOrder

catalog.SewingProductOperations
├── ProductId
└── GarmentPartOperationId

catalog.PpeProductDetails                only Type = Ppe
├── ProductId (PK + FK → Products)
├── SupplierId
├── BasePrice
├── RetailCoefficientSource              Reference | Custom
├── RetailAdditionalReferenceId?         only Reference
├── RetailCustomPercent?                 only Custom
├── WholesaleCoefficientSource           Reference | Custom
├── WholesaleAdditionalReferenceId?      only Reference
└── WholesaleCustomPercent?              only Custom
```

Sewing prices are a calculated read model for each `Product — Fabric` pair and are not stored in separate tables: the formula uses current Reference prices and `AdditionalReference`. Price and production-standard history is not stored. `Products`, `ProductPhotos`, and `ProductCategoryLinks` already exist; all other structures in the diagram are target structures and have not yet been implemented.

### Integrity rules

- `Products.Type` determines which detail model is allowed.
- Sewing has exactly one `SewingProductDetails` and no `PpeProductDetails`.
- Ppe has exactly one `PpeProductDetails` and no Sewing details or collections.
- `SewingProductDetails.ProductId` and `PpeProductDetails.ProductId` are both PK and FK: no more than one detail record can exist per product.
- Every Product-owned relationship has an explicit CLR FK (`ProductId`, and `TableId` for characteristic rows). EF Core shadow properties are not used for this model. Sewing link tables have a composite primary key of `ProductId` and the reference ID.
- `Products.Slug` is unique within Catalog.
- `Products.Name` and `Products.RuName` are each unique within Catalog.
- Product inherits the `CreatedAt`, optional `UpdatedAt`, and `IsDeleted` audit fields from `BaseAuditableEntity<ProductId>`. Time is supplied to Domain by Application/the clock; Domain does not call `DateTimeOffset.UtcNow`. `CreatedAt` is set by the server at creation, and `UpdatedAt` at each successful change. Dates are used to sort the admin list; Product is currently physically deleted, so `IsDeleted` is not set in this delivery.
- No more than two `SewingProductFabrics.IsPrimary = true` per Product; the read model orders by descending `IsPrimary`, then ascending `SortOrder`.
- `ProductInformationBlocks` and `ProductCharacteristicTables` belong to one Product; `ProductCharacteristicRows` belongs to one table. `SortOrder` determines order in each collection.
- `Type` and mutually exclusive field checks remain in Domain/Application; the database enforces keys, FKs, and relationship uniqueness.
