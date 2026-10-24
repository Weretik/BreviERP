# BreviERP backend feature map

**Static code check completed:** 2026-10-16. This map covers current HTTP scenarios by module. Each description explains the entry point, components, data, and cross-module relationships; [SDD](../specs/README.md) holds decisions and delivery history. When adding a feature or changing its structure, update its description and this index in the same change.

See the feature card's “Code structure” section for actual file paths and roles, and “Structure diagram” for the flow between parts.

## Catalog

- [Catalog products](catalog/products.md) — create, read, replace, and delete Sewing/PPE products.
- [Sewing Articles and composition](catalog/sewing-composition.md) — sale SKUs, shared/Article material norms, shared operations and review readiness; PostgreSQL rollout verification pending.
- [Product categories](catalog/categories.md) — Admin CRUD and Storefront list.
- [Catalog media](catalog/media.md) — upload, list, and delete product files.

## Reference

- [Suppliers](reference/suppliers.md) — CRUD and list reads.
- [Fabrics](reference/fabrics.md) — CRUD and list reads.
- [Garment accessories](reference/garment-accessories.md) — CRUD and list reads.
- [Garment parts](reference/garment-parts.md) — CRUD and list reads.
- [Sewing operations](reference/garment-part-operations.md) — CRUD and list reads.
- [Additional reference data](reference/additional-references.md) — CRUD and list reads.

## Identity

- [User session](identity/session.md) — login, refresh, logout, and me.

## Other modules

[Accounting and Crm](../architecture/overview/modules.md) exist in the structure; as of the check date, they have no separate HTTP scenarios in controllers. They are not marked as completed features. See [operations](../operations/data/migrations-and-seeding.md) for schema preparation and seed.
