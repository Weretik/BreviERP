# Product page — Domain design

**Status:** under design\
**Depends on:** accepted decision about Sewing rules\
**Blocks:** persistence, Application, and API for operations

## Outcome

`Product` can represent shared data for two product types and a set of operations only for `ProductType.Sewing`, without violating the Catalog and Reference boundaries.

## Existing baseline

`Product` is already an aggregate root. It owns names, slug, type, `ProductPhoto`, and `ProductCategoryReference`. `ProductPhoto` refers to `MediaFileId`, and the aggregate checks that the media file is ready.

`MediaFile` used by at least one `ProductPhoto` is not deleted from Media. Media checks Catalog usage before deletion and rejects the operation if a link exists; Product is not changed automatically.

The current technical baseline and Reference integration rules are described in [implementation-baseline.md](implementation-baseline.md).

## Proposed design

```text
Catalog.Product (aggregate root)
├── shared properties: Name, RuName, Slug, Type, Photos, Categories
├── Description: Markdown in Ukrainian and Russian
├── InformationBlocks[]: localized heading, text, and order
├── CharacteristicTables[]: localized tables and their rows
└── SewingOperations[]                                             (only for Type = Sewing)
    └── GarmentPartOperationId ──→ Reference.GarmentPartOperation
                                      └── Min
```

`SewingOperations` is a child collection of Product and contains only the typed ID of a reference operation. Product does not contain the `GarmentPartOperation` entity and does not depend on Reference Infrastructure. Application checks that the ID exists through a Reference reader abstraction.

## Accepted calculation

```text
totalOperationMinutes = Σ operation.Min
piecesPerShift = 480 / (totalOperationMinutes × 1.25)
```

`480` is the established shift duration in minutes, and `1.25` is a fixed coefficient applied to the total operation duration. `piecesPerShift` is the only metric for pieces per shift: it is passed as the full fractional numeric value without rounding. Separate values for completed or theoretical pieces and remaining minutes are not calculated.

A Sewing product may exist as a draft without operations; its metrics are not calculated. Calculation is also skipped when the sum of `Min` is zero. `Min` is always read from the current `GarmentPartOperation` in Reference; Product does not store a snapshot, so a reference change affects the next calculation.

## Reference data deletion restrictions

`GarmentPartOperation`, `Fabric`, and `GarmentAccessory` cannot be deleted from Reference if any Product contains a corresponding link. Reference checks Catalog for links before deletion; if one exists, deletion is rejected and Product is unchanged.

Automatic link cleanup, cross-module events, retries, and eventual consistency are not used for this.

## Acceptance criteria for implementation

- [ ] `Ppe` cannot have Sewing operations.
- [ ] An operation cannot be added twice to one Product.
- [ ] An invalid or missing operation ID does not enter the aggregate.
- [ ] A Sewing draft without operations is allowed; its labor time is not calculated.
- [ ] Labor time does not divide by zero; `piecesPerShift` is passed as the full fractional value without rounding.
- [ ] Changing `Min` in Reference changes the next calculation without changing Product.
- [ ] Deletion of an operation, fabric, or accessory used by Product is rejected without changing Product.
- [ ] Catalog does not depend on Reference Infrastructure or its entities.

## Fabrics: Domain model extension

`Fabrics` will be a child collection of Product with `FabricId`, `IsPrimary`, and `SortOrder` fields. It does not contain the `Fabric` entity, its `Price`, or `ProviderId`. Product enforces fabric uniqueness and no more than two primary fabrics; if the list contains one fabric, it is primary. The read model displays primary fabrics first, then applies `SortOrder`. Application checks fabric existence through the Reference abstraction.

`MetersPerProduct` is one manual positive field on the Sewing product itself: the number of meters of fabric per product. It does not belong to `Fabrics[]`, is not duplicated for each fabric, and does not apply to `Ppe`.

Consumption is defined by the product-level `MetersPerProduct`. Rules for calculating three price tiers, 1–10 / 11–39 / 40+ units, for each selected fabric are defined in [requirements/pricing.md](../requirements/pricing.md). The result is a calculated read model, not a Product snapshot: it uses current Reference prices and `AdditionalReference`. A Fabric linked to Product cannot be deleted.

Ppe requires a mandatory product-level `BasePrice` entered by an administrator. Retail and wholesale percentages are applied to it separately; for each tier, the administrator selects an `AdditionalReference` or enters a custom percentage. Formulas and ranges are defined in [requirements/pricing.md](../requirements/pricing.md).

## Accessories: Domain model extension

`Accessories` will be a child collection of Product with `GarmentAccessoryId`, `Quantity`, and `SortOrder` fields. It does not contain the `GarmentAccessory` entity, its `Price`, or `SupplierId`. Product enforces accessory uniqueness and positive quantity; Application checks that the ID exists through the Reference abstraction.

Cost calculation uses the current Reference price and quantity per product as defined in [requirements/pricing.md](../requirements/pricing.md); fractional results are not rounded. The pure formula is implemented in the Domain service `SewingPricing`: it accepts only Product values and prepared numeric inputs. Application reads current Reference data, builds the input, and maps the result into the read model; Domain does not depend on Reference. An accessory linked to Product cannot be deleted.

## Product type boundary

`Fabrics`, `Accessories`, `MetersPerProduct`, and `SewingOperations` are available only for `ProductType.Sewing`. A Product of type `Ppe` cannot create, store, or change this data.

`ProductType.Ppe` has exactly one required `SupplierId` referring to `Reference.Supplier` and a required positive `BasePrice`. Product does not contain the Supplier entity; Application checks that the ID exists through the Reference abstraction. A Sewing product cannot have `SupplierId` or `BasePrice` at Product level.

Ppe has two value objects, `RetailPricePercent` and `WholesalePricePercent`, each with two mutually exclusive states: `Reference(AdditionalReferenceId)` or `Custom(decimal value)`. In the first case, Application reads the current Reference value with unit `%`; in the second, the value belongs to Product and is non-negative. Product does not create an AdditionalReference automatically. A Sewing product cannot have these percentages.

## Verification

- Unit tests: add, remove, and replace operations; duplicate ID; type boundary; zero total; calculation without rounding.
- Integration tests: verify Reference operation existence, reject deletion of a used operation/fabric/accessory, and verify the product page read model.
- Risks: cross-module consistency and a change to `Min` in the reference after linking.
