# Pricing rules and observable scenarios

## Sewing rules

- **R-001:** Calculate a sellable Article using its model-wide plus Article-specific composition lines. Each physical material cost is quantity × current approved unit cost; a non-inventory other-cost line is a distinct kind and contributes its approved amount.
- **R-002:** `minuteCost = monthlySeamstressSalary / (workDaysMonth × minutesShift)`. For each operation line, use positive quantity times effective minutes times `minuteCost` times `(1 + assemblyRate)`. Effective minutes use an explicitly supplied reviewed manual time, required for `OP-155` or an absent standard; otherwise use the approved operation norm. This retains Brevi's existing manual override capability. Sum every occurrence for sewing labor cost.
- **R-003:** Cutting, master, shop-manager and bonus costs are **multiplications** of sewing labor cost by their stored fraction rates. Overhead is total operation minutes (including quantity) × `(1 + assemblyRate)` × monthly overhead / `(workDaysMonth × minutesShift × seamstressCount)`.
- **R-004:** Full cost is material + other cost + sewing labor + cutting + master + shop-manager + bonus + overhead. The three sewing prices are full cost × approved coefficients for quantities 1–9, 10–40 and 41+. Coefficients are data, not hard-coded constants; the observed source values are 1.55, 1.32 and 1.25.
- **R-005:** Required missing or inactive composition/reference/cost, unreviewed operations, invalid divisor, absent article or missing rule blocks calculation with a specific reason. Do not return zero as a valid price. The target validates *all* required lines rather than trusting a single spreadsheet lookup row.
- **R-006:** Preserve internal decimal precision needed for parity; publish and display monetary values under one documented rounding policy. The source sewing formula contains no explicit `ROUND` and its visible price cells are formatted to two decimals. Do not apply an extra VAT multiplier to a value already designated as including VAT.

## PPE rules

- **R-007:** PPE rule scope selects one explicit row of retail/wholesale/large-wholesale markup fractions for the Product. With supplier cost `c`, fallback prices are `ROUND(c × (1 + markup), 2)` per tier. Missing or ambiguous scope blocks the fallback.
- **R-008:** When a positive approved external retail price is present, use it as retail. When both positive external wholesale and retail prices are present, wholesale is their mean rounded to two decimals. When a positive external wholesale price is present, use it as large-wholesale. Otherwise use the corresponding markup fallback. External overrides have source, date and approval in Brevi; they are not a live XML lookup by the pricing use case.
- **R-009:** Historical issued quotes retain their offered price snapshot when current rules or costs change. A new calculation uses current approved versions.
- **R-010:** Old Product pricing outputs remain compatible until consumers switch to Article/tier prices. The release includes an explicit version/contract transition, not two editable sources of price truth.

## Scenarios

- **SC-001:** Given Article `246` with model-wide and its own fabric lines, when calculated with approved inputs, then it returns a full cost breakdown and prices for 1–9, 10–40 and 41+.
- **SC-002:** Given Article `247` of the same model, when calculated, then shared labor/overhead stays the same and the distinct fabric changes material cost and all three prices.
- **SC-003:** Given rates 21.74%, 11% and 12%, when cutting/master/manager costs are calculated, then they equal labor × rate, not labor ÷ rate.
- **SC-004:** Given quantity 9, 10, 40 or 41, when a price is requested, then exactly one sewing tier applies at each boundary.
- **SC-005:** Given a missing required material price, unreviewed operation or zero divisor, when calculated, then the result is blocked with an actionable reason and no published price.
- **SC-006:** Given PPE with a scoped markup rule and no approved external prices, when calculated, then three two-decimal fallback prices result. With approved external prices, the documented overrides apply.
- **SC-007:** Given a sent quote, when a material cost or rule changes, then the sent quote keeps its historical prices while a new calculation can use the new versions.

## Exclusions and phased readiness

The sewing backend publication stage adds **R-011**: retain complete immutable calculation evidence, reject blocked/stale publication, preserve published history and append input revisions atomically. Its observable scenarios **SC-P01–SC-P05** are defined in the [publication revision](publication-implementation.md).

No live Sheet/Gateway/XML pricing dependency. No migration of historical quote records in this feature. Payroll payout and embroidery production compensation are separate domains unless a specific pricing component uses them. PPE rule scope and external-price provenance must be verified before PPE code delivery; this does not block the sewing slice.
