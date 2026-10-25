# 003 — Brevi-owned product pricing from approved source arithmetic

**Module:** Catalog/Pricing boundary. **Status:** sewing engine, input-version updates and saved-calculation/publication API implemented locally, 2026-10-25; frontend consumer cutover and PPE remain planned. **Owner:** Catalog pricing, with Reference-owned cost/norm versions. The user selected the workbook's arithmetic as the calculation basis. There is no runtime workbook dependency. See the [implemented slice](sewing-implementation.md) and [execution evidence](sewing-evidence.md).

This SDD changes the earlier [Product page pricing](../001-product-page/requirements/pricing.md) once implemented. It does not rewrite that completed specification's historical evidence. The preceding [Article and composition SDD](../002-sewing-articles-and-composition/README.md) supplies sellable Article identity and cost inputs.

## Scope

- Calculate sewing full cost and three quantity-tier prices from each Article's applicable material, operation and other-cost lines.
- Calculate PPE retail, wholesale and large-wholesale prices from supplier cost, scoped rule markups and the specified optional source-price overrides.
- Keep settings, rule versions, supplier/material costs and published price evidence in Brevi; expose explicit blocked reasons instead of misleading zero prices.
- Reconcile selected source examples before switching existing Product price read models and storefront/quote consumers.

The first implementation slice is sewing price parity for model `MOD-025`, Articles `246`/`247`. PPE is a separate dependent slice. Quote price snapshots, bank/finance data, and production payroll are outside this SDD.

## Navigation

- [Rules and scenarios](requirements.md)
- [Verified source-versus-code comparison](comparison.md)
- [Target design and compatibility](design.md)
- [Saved calculation/publication design](publication-implementation.md) and [execution evidence](publication-evidence.md)
- [Pricing HTTP contract and consumer handoff](contracts/api-contract.md)
- [TS-005 consumer integration](consumer-integration.md)
- [Publication structure audit](structure/audit.md) and [refactoring checks](structure/verification.md)
- [Prerequisites and tasks](tasks.md)
- [Traceability and readiness](traceability.md)
- [Sewing implementation scope and ownership](sewing-implementation.md)
- [Sewing execution and verification](sewing-evidence.md)
- [Implemented feature structure](../../../features/catalog/sewing-pricing.md)

**Current behavior remains active** until a tested version of this SDD is released. In particular, existing Brevi tiers and two-price PPE output must not be described as already changed.
