# TS-007 source reconciliation — 2026-10-25

Verified against the imported frozen workbook, not a live XML/Google recalculation. Detailed source/private reports remain ignored locally; no source bundle or credentials are committed. Both job modes are read-only and every diagnostic sets CanPublish=false. Strict approval/activity blockers remain in the report; the diagnostic approval override exists only in memory.

## Sewing

All five source categories are represented by six Articles, including both fabric variants of MOD-025. All 66 component/tier comparisons match within 0.00000001 UAH for unrounded components and exactly at the approved two-decimal tier policy. The report contains source cell, expected value, actual value, difference, settings/reference version IDs and source hash.

| Article | Actual full cost (displayed) | Retail | Wholesale | Large wholesale | Matched components/tiers |
| --- | ---: | ---: | ---: | ---: | ---: |
| 246 | 754.67 | 1169.74 | 996.17 | 943.34 | 11 / 11 |
| 247 | 1021.07 | 1582.66 | 1347.81 | 1276.34 | 11 / 11 |
| 350 | 1243.46 | 1927.36 | 1641.37 | 1554.33 | 11 / 11 |
| 374 | 627.26 | 972.26 | 827.99 | 784.08 | 11 / 11 |
| 386 | 1451.91 | 2250.46 | 1916.52 | 1814.89 | 11 / 11 |
| 472 | 2021.14 | 3132.76 | 2667.90 | 2526.42 | 11 / 11 |

Source snapshot SHA-256: `9C8079C1259377BE1ECEE6F85FF50874BFB9B2819EA1B5FFF8ECF6CC48DCBD28`. Formula: `sewing-sheet-v1`. Capture: `2026-10-25`.

## PPE

All 119 imported PPE source rows were checked. 118 Products match all three source prices (354 matched values). There are zero differing, invalid-source or missing-calculation values where the source has a numeric expected price. Each row records source cell/raw value, expected/actual/difference, offer/rule UUIDs and the formula version.

SKU `724` has no supplier cost or sale prices in the frozen source: its three comparisons are MissingSourceValue and its arithmetic prices are absent. This is a known source-data gap, not a formula mismatch. No amount was guessed and the Product remains blocked until real input is entered and explicitly approved. Its exact status is retained in the private report.

No unexplained numeric discrepancy remains in the tested catalogue scope. This does not approve data, publish prices, verify a production browser session or migrate historical quotes. Future quotes and Storefront API adoption remain outside this SDD; their price snapshots must use their own accepted contracts.
