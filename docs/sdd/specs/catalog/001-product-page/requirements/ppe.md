# Product page — PPE

### PPE: supplier

A PPE product has exactly one supplier:

```text
Ppe Product
└── SupplierId → Reference.Supplier
```

The supplier has a name and contact details. It is required for PPE and does not apply to Sewing; supplier data is not duplicated on the product.

A PPE product also has a required manual `BasePrice` field, entered by an administrator. It belongs to the Product, must be greater than zero, and is not taken from the supplier reference.

### PPE: retail and wholesale percentages

A PPE product has two independent percentages: retail for 1–9 units and wholesale from 10 units. For each tier, the administrator independently selects a reference value or enters a custom value:

```text
PpePriceCoefficients
├── Retail: Reference(AdditionalReferenceId) | Custom(Value)
└── Wholesale: Reference(AdditionalReferenceId) | Custom(Value)
```

In `Reference` mode, Product stores only the corresponding `AdditionalReferenceId`; calculation always reads the current `AdditionalReference.Value`. A value with unit `%` is applied as a percentage using `Value / 100`, not as a ready-made multiplier. A change to the reference percentage automatically affects the next calculation for that price tier.

In `Custom` mode, Product stores only its own percentage value; the administrator can change it on the product card. This does **not** create an `AdditionalReference`: the custom percentage belongs to one product and must not clutter the shared reference data.

## Percentage rules

- Exactly one mode is set for each of the retail and wholesale tiers: `Reference` or `Custom`.
- In `Reference` mode, a percentage with unit `%` is set and the tier's manual value is absent.
- In `Custom` mode, a non-negative manual percentage is set and that tier's `AdditionalReferenceId` is absent.
- A Sewing product has no PPE `BasePrice` or retail/wholesale percentage.
- `BasePrice` is required and greater than zero only for PPE; a Sewing product has no base price.
