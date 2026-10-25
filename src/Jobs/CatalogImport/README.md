# Catalogue import code navigation

Start with the [feature map](../../../docs/sdd/features/catalog/catalog-import.md) and [runbook](../../../docs/sdd/operations/data/catalog-import.md).

- Source: workbook rows plus Identity and References conventions; no pricing algorithm.
- Planning: identity keys/validation and dataset-specific validators.
- Execution: transaction coordinator, reports and persisted reconciliation.
- Persistence/Catalog: Categories, Products and Composition writers/mappers.
- Persistence/Reference: supplier/material/operation adapters and their coordinator.
- Persistence/Provenance: records, EF configuration, context and source mappings.
- Migrations: existing import-provenance migrations; never relocate them as ordinary source files.

The pricing job depends on this project only for frozen import provenance/source adapters. Catalogue import does not depend on the pricing job. Tests and the [structural audit](../../../docs/sdd/specs/catalog/005-import-pricing-structure/audit.md) document preserved behavior.

