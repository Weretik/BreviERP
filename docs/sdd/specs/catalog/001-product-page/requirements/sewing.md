# Product page — Sewing

### Sewing: operations and labor time

For `Sewing`, the user can add or remove sewing operations. Each operation has a name, a garment part, and a duration in minutes.

A Sewing product also has one manual `MetersPerProduct` field: the number of meters of fabric required for one product. This is a Product property, not a property of an individual fabric. The field is required for a Sewing product, must be greater than zero, and does not apply to `Ppe`.

The fabric list, accessory list, `MetersPerProduct`, and labor operations belong exclusively to `ProductType.Sewing`. `Ppe` cannot contain any of these collections or fields.

## Fabric list

Product will have an ordered list of references to `FabricId` values from the Reference module. Up to two fabrics are marked as **primary**; all others are additional. Primary fabrics are always displayed first, and `SortOrder` determines order within each group. The business model does not limit the number of additional fabrics: a product may have two, five, ten, or more fabrics.

```text
Product
└── Fabrics[]
    ├── FabricId → Reference.Fabric
    ├── IsPrimary
    └── SortOrder
```

`Fabric` belongs to Reference and currently contains `Name`, current `Price`, and `ProviderId`. Product stores only the ID and properties of its own link; it does not copy the fabric name or price.

### Accepted invariants

- The same `FabricId` cannot be added twice to one Product.
- A product with a non-empty fabric list has no more than two primary fabrics; if there is only one fabric, it is primary.
- Any number of additional fabrics is allowed.
- The read model sorts primary fabrics before additional fabrics, and by `SortOrder` within each group.
- The fabric list is available only for `Sewing`; `Ppe` cannot have any fabrics.

## Accessory list

Product will have an ordered list of references to `GarmentAccessoryId` values from the Reference module. For each selected accessory, the user sets the quantity required for one product.

```text
Product
└── Accessories[]
    ├── GarmentAccessoryId → Reference.GarmentAccessory
    ├── Quantity
    └── SortOrder
```

`GarmentAccessory` belongs to Reference and currently contains `Name`, current `Price`, and `SupplierId`. Product stores only the ID, quantity, and order of its own link; it does not copy the accessory price or name.

### Accepted invariants

- The same `GarmentAccessoryId` cannot be added twice to one Product.
- Quantity is required and must be greater than zero.
- The accessory list is available only for `Sewing`; `Ppe` cannot have any accessories.

## Productivity calculation

```text
totalOperationMinutes = Σ duration of all product operations
piecesPerShift = 480 / (totalOperationMinutes × 1.25)
```

A shift is `480` minutes, and the fixed coefficient `1.25` is applied to the total operation duration. `piecesPerShift` is always passed as the full fractional numeric value, without rounding and without a separate shortened value for completed products. A Sewing product may be a draft without operations; in that state, the metrics are not calculated. Subsequent calculations use the current operation duration.
