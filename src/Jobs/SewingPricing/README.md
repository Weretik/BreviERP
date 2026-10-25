# Sewing pricing job code navigation

Start with the [feature map](../../../docs/sdd/features/catalog/sewing-pricing.md) and [runbook](../../../docs/sdd/operations/data/sewing-pricing.md).

- Bootstrap: transaction coordinator and report; Source maps setting cells, Inputs builds/verifies Reference baselines, Reconciliation compares immutable versions.
- Reading: the job's Catalog-to-Reference contract bridge.
- Verification: repeatable-read coordinator, per-Article Diagnostics and pure source Comparison.

Arithmetic belongs to Catalog.Domain/Pricing/Sewing/Calculation. Settings and input versions belong to the Catalog and Reference modules. This operational assembly does not own business formulas or publish prices. It reads the imported catalogue provenance from CatalogImport without creating a reverse dependency.

