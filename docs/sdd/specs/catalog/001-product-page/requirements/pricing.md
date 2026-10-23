# Product page — pricing

## Accepted rule

The `Sewing` price is calculated for each `product — selected fabric` pair. A product with multiple fabrics therefore has three separate prices for each fabric. Fabric price comes from the current `Reference.Fabric`, and accessory prices come from current `Reference.GarmentAccessory`; Product does not store their snapshots. A change to reference prices or coefficients changes the next calculation.

All intermediate and final values are passed as full fractional numeric values without rounding. All divisors must be greater than zero; if any is zero or missing, the price is not calculated.

## Inputs

| Name | Source |
| --- | --- |
| `metersPerProduct` | `Product.SewingProductDetails.MetersPerProduct` |
| `fabricPrice` | current `Reference.Fabric.Price` for the selected fabric |
| `accessoryQuantity`, `accessoryPrice` | Product link and current `Reference.GarmentAccessory.Price`, respectively, for each accessory |
| `totalOperationMinutes` | sum of `Min` for all selected `GarmentPartOperation` records |
| `srZpShvei` | `AdditionalReference.Key = sr_zp_shvei` |
| `workDay` | `AdditionalReference.Key = work_day` |
| `coefficientSeamstressAward` | `AdditionalReference.Key = coefficient_seamstress_award` |
| `coefficientFactor` | `AdditionalReference.Key = coefficient_factor` |
| `coefficientMaster` | `AdditionalReference.Key = coefficient_master` |
| `coefficientForeman` | `AdditionalReference.Key = coefficient_foreman` |
| `monthlyExpenses` | `AdditionalReference.Key = monthly_expenses` |
| `countShvei` | `AdditionalReference.Key = count_shvei` |

Each `AdditionalReference` value is read by its unique `Key`; its current `Value` field is passed into the calculation. For each value with unit `%`, use `percentRatio = Value / 100`: for example, `33%` becomes `0.33`.

## Sewing cost calculation

```text
piecesPerShift = 480 / (totalOperationMinutes × 1.25)

seamstressSalary = (srZpShvei / workDay) / piecesPerShift
seamstressAward = seamstressSalary × (coefficientSeamstressAward / 100)
cutterSalary = seamstressSalary / (coefficientFactor / 100)
masterSalary = seamstressSalary / (coefficientMaster / 100)
foremanSalary = seamstressSalary / (coefficientForeman / 100)

workshopSalary = seamstressSalary + cutterSalary + masterSalary + foremanSalary + seamstressAward
overheadExpenses = monthlyExpenses / workDay / countShvei / piecesPerShift

accessoriesCost = Σ(accessoryQuantity × accessoryPrice)
baseCost = (metersPerProduct × fabricPrice) + accessoriesCost + workshopSalary + overheadExpenses
```

`1.25` is a fixed coefficient applied to total labor time. A Sewing draft without operations or a product whose total `Min` is zero has no `piecesPerShift` or price.

`seamstressSalary`, `seamstressAward`, `cutterSalary`, `masterSalary`, and `foremanSalary` are costs per product. Monthly `srZpShvei` is used only to initially calculate `seamstressSalary`.

## Three prices per fabric

For each selected fabric, the following prices are calculated:

```text
price1To10 = baseCost × (1 + profit10 / 100)
price11To39 = baseCost × (1 + profit10To40 / 100)
price40Plus = baseCost × (1 + profit40 / 100)
```

| Order range | `AdditionalReference` key |
| --- | --- |
| 1–10 units | `profit_10` |
| 11–39 units | `profit_10_40` |
| 40 or more | `profit_40` |

Each price uses the current `Value` of its corresponding percentage. A value of `33` means a `33%` markup: `baseCost × (1 + 33 / 100) = baseCost × 1.33`. Accessories, workshop salary, and overhead are the same for all three prices of a given `product — fabric` pair; only `fabricPrice` and the price-tier percentage differ.

The read model also returns the minimum and maximum price across all of the product's fabrics separately for each tier: `price1To10`, `price11To39`, and `price40Plus`. The `FabricId` that produced each price is returned with the extrema so the UI can show the specific fabric.

## Ppe product type

PPE has a manual `basePrice` entered by the administrator. Retail and wholesale percentages are independently selected from `AdditionalReference` or entered manually:

```text
retailPrice = basePrice × (1 + retailPercent / 100)
wholesalePrice = basePrice × (1 + wholesalePercent / 100)
```

The retail price applies to 1–9 units; the wholesale price applies from 10 units. Both percentages have unit `%` and are applied using `/ 100`.

## Price in the admin list

Each admin Product list row sends `minimumWholesalePrice` as a numeric value for quick comparison between products:

- for `Sewing`, it is the minimum across all available `price1To10`, `price11To39`, and `price40Plus` values for all selected fabrics;
- for `Ppe`, it is `wholesalePrice` from the formula above;
- if required inputs are missing, calculation is impossible, or the product has no applicable prices, the value is `0`.

The value is a current read model, is not stored as a snapshot, and is not rounded.
