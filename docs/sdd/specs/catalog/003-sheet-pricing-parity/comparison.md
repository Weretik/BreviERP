# Source arithmetic versus current Brevi code

**Checked 2026-10-24:** workbook formulas and outputs were inspected; current Brevi `Catalog.Domain.Products.Pricing.SewingPricing` and `ProductAdminDetailMapper` were read. The workbook is the approved algorithm source for the migration. Values below are parity fixtures, not hard-coded defaults.

| Component | Approved source arithmetic | Brevi current behavior | Target change |
| --- | --- | --- | --- |
| Fabric/material | Sum applicable model + Article lines, `quantity × unit cost` | `MetersPerProduct × selected Fabric.Price` plus accessory sum | Use Article composition and every applicable line, including non-inventory other cost. |
| Seamstress | `monthlySalary/(workDays×minutesShift)` × total weighted minutes × `(1+assemblyRate)` | `piecesPerShift=480/(minutes×1.25)` then salary/workDays/piecesPerShift | Equivalent for the observed 480 minutes and 25% assembly; remove fixed numbers and use settings. |
| Cutting | Labor × `CUTTING_RATE` | Labor ÷ (`coefficient_factor/100`) | Replace division with multiplication. |
| Master | Labor × `MASTER_RATE` | Labor ÷ (`coefficient_master/100`) | Replace division with multiplication. |
| Shop manager | Labor × `SHOP_MANAGER_RATE` | Labor ÷ (`coefficient_foreman/100`) | Replace division with multiplication. |
| Bonus | Labor × `SEAMSTRESS_BONUS` | Labor × award percent/100 | Equivalent after unit conversion. |
| Overhead | Weighted minutes × `(1+assemblyRate)` × monthly overhead / (workDays × shift minutes × seamstress count) | Monthly overhead/workDays/count/piecesPerShift | Equivalent at the observed fixed shift/assembly, then setting-driven. |
| Sewing tiers | Full cost × 1.55/1.32/1.25 observed, bounds 1–9/10–40/41+ | Full cost × (1+profit%/100), bounds 1–10/11–39/40+ | Convert settings to coefficients and use source bounds. |
| PPE | Rule-name markups from the maintained table, three tiers, optional external-price overrides | Supplier base price + two manually selected/reference percentages | New rule scope, third tier and override precedence. |

Historical 12-operation source fixture `MOD-025`, checked 2026-10-24:

| Article | Materials | Sewing labor | Cutting | Master | Manager | Bonus | Overhead | Full cost | Displayed tier prices |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| `246` | 277.99 | 175.266768... | 38.102995... | 19.279344... | 21.032012... | 14.021341... | 175.266768... | 720.96 | 1117.49 / 951.67 / 901.20 |
| `247` | 544.39 | 175.266768... | 38.102995... | 19.279344... | 21.032012... | 14.021341... | 175.266768... | 987.36 | 1530.41 / 1303.31 / 1234.20 |

For identical labor `175.266768...`, applying the backend's division would produce cutting ≈806.19, master ≈1593.33 and manager ≈1460.56; the approved source produces ≈38.10, ≈19.28 and ≈21.03. This demonstrates an arithmetic difference independent of Product ID matching.

The source sewing formula does not explicitly round its internal outputs; a two-decimal number format displays them. PPE fallback formulas explicitly `ROUND(...,2)`. The price columns are labelled including VAT, but the formulas do not apply an additional VAT multiplier. The target must reproduce observed numeric output and document currency presentation separately.

The source PPE price output uses a rule table with a rule name and three markup columns. Other PPE coefficient keys also exist, but the inspected output formulas do not use them for fallback. Inspect all PPE rule assignments and override source values before the PPE phase; do not silently apply both mechanisms.

## Current executed fixture — 2026-10-25

The frozen imported source now includes 13 operations/84.84 minutes for MOD-025. Current full costs are 754.6707393292684 / 1021.0707393292685; rounded tiers are 1169.74 / 996.17 / 943.34 and 1582.66 / 1347.81 / 1276.34. All eleven compared components/tier outputs match the new backend's diagnostic calculation. [Execution evidence](sewing-evidence.md) records provenance and the separate review blockers.
