# 002 — Sewing articles and calculation composition

The [full local catalogue import revision](catalog-import.md) extends EN-001/TS-007 under direct user authorization. It imports identity, composition and source offers independently of price calculation.

**Module:** Catalog, with Reference integration. **Status:** code, local PostgreSQL migrations and full local catalogue import verified, 2026-10-25; see [import evidence](catalog-import-evidence.md). **Scope:** additive model/article identity, editable calculation inputs and the authorized local catalogue transfer. This extends the completed [Product page SDD](../001-product-page/README.md); it does not replace its historical evidence.

## Goal and boundary

One Sewing Product can own multiple sellable articles. An article selects its own fabric inputs while model-wide material and operation inputs are maintained once. All values used for later costing come from Brevi-owned records. The first example is model `MOD-025`, articles `246` and `247`: eight shared cost lines, one fabric line per article, and twelve shared operation lines. Example identifiers are migration fixtures, not new Brevi primary keys.

This slice covers product/article relationships, composition editing, validation and read contracts. It does **not** change the accepted price formula, quantity tiers, VAT policy, historical quote data, or production orders. Those require separate specifications; [existing pricing rules](../001-product-page/requirements/pricing.md) remain the current Brevi behavior until deliberately revised. The source example includes a non-inventory `Other cost` line; this specification stores its input, while cost-version and price use are deferred to Pricing.

## Navigation

- [Rules and observable scenarios](requirements.md)
- [Data model, identifiers and migration](data-model.md)
- [Design and code responsibilities](design.md)
- [Proposed HTTP contract](contracts/api-contract.md)
- [Prerequisites and implementation tasks](tasks.md)
- [Traceability and readiness](traceability.md)
- [Implementation checkpoint and verification](implementation.md)

## Evidence and open gates

The repository model has `Products`, one-to-one `SewingProductDetails`, and product-level fabric/accessory/operation links. `SewingProductFabrics` stores no fabric quantity or article; `SewingProductOperations` stores no quantity/manual time. `CreateProductRequest` and full replacement do not contain a model code or article. The user confirmed that Brevi currently has no Product records. Keep the schema additive for API compatibility. Before executing an import, verify that the selected target database is still empty for Products and inspect its applied migrations.

**Product identity decision:** the confirmed empty Product catalogue allows one new Sewing Product per model and child Article per sale SKU, with no existing Product merge. Internal IDs remain distinct from model codes and sale SKUs. **Pricing transition:** the user selected the workbook formula as the target basis. The linked pricing SDD defines sewing tiers 1–9/10–40/41+ and their implementation; the old tiers remain current behavior until that feature is released.

The planned [pricing parity SDD](../003-sheet-pricing-parity/README.md) defines the later formula replacement and three-tier Article prices.
