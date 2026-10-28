# BreviERP backend feature map

**Static code check completed:** 2026-10-16. This map covers current HTTP scenarios by module. Each description explains the entry point, components, data, and cross-module relationships; [SDD](../specs/README.md) holds decisions and delivery history. When adding a feature or changing its structure, update its description and this index in the same change.

See the feature card's “Code structure” section for actual file paths and roles, “Structure diagram” for the flow between parts, and “Tests” for test files and the focused run command.

## Catalog

Products:

- [Catalog products](catalog/products.md) — create, read, replace, and delete Sewing/PPE products.
- [Product categories](catalog/categories.md) — Admin CRUD and Storefront list.
- [Catalog media](catalog/media.md) — upload, list, and delete product files.
- [Sewing Articles and composition](catalog/sewing-composition.md) — sale SKUs, shared/Article material norms, shared operations and review readiness.

Pricing API (start with Sewing or PPE pricing):

- [Sewing Article pricing](catalog/sewing-pricing.md) — versioned inputs, strict calculation, saved snapshots and publication API.
- [PPE pricing](catalog/ppe-pricing.md) — versioned supplier offers, exact rule scope and three-tier calculation.
- [Current pricing boundaries](catalog/pricing-cutover.md) — Product reads carry metadata only; prices come from the pricing APIs.
- [Reference pricing versions](reference/pricing-input-versions.md) — Reference-owned cost and norm versions used by pricing.

Pricing data operations (local `Host.Seed` jobs, not HTTP), in run order:

- [Local catalogue import](catalog/catalog-import.md) — frozen source, transactional model/Article/PPE transfer, provenance and reconciliation.
- [Imported pricing readiness](catalog/pricing-readiness.md) — full strict inventory before review/activation.
- [Frozen-source pricing input review](catalog/pricing-input-review.md) — per-field source checks and activation holds.
- [Explicit local pricing activation](catalog/pricing-activation.md) — reviewed plan, confirmation/publication and progress.

## Reference

- [Suppliers](reference/suppliers.md) — CRUD and list reads.
- [Fabrics](reference/fabrics.md) — CRUD and list reads.
- [Garment accessories](reference/garment-accessories.md) — CRUD and list reads.
- [Garment parts](reference/garment-parts.md) — CRUD and list reads.
- [Sewing operations](reference/garment-part-operations.md) — CRUD and list reads.
- [Reference pricing versions](reference/pricing-input-versions.md) — atomic numeric-edit history and explicit service-cost revisions (also listed under Catalog pricing).
- [Additional reference data](reference/additional-references.md) — CRUD and list reads.

## Identity

- [User session](identity/session.md) — login, refresh, logout, and me.

## Other modules

[Accounting and Crm](../architecture/overview/modules.md) exist in the structure; as of the check date, they have no separate HTTP scenarios in controllers. They are not marked as completed features. See [operations](../operations/data/migrations-and-seeding.md) for schema preparation and seed.

## Crm

- [Clients and contacts](crm/clients-and-quotes.md) — scoped customer/contact APIs, source identity, optimistic concurrency and Admin merge; quote workflow remains planned.
