# Module schemas

**Status:** implementation

This change separates the modular monolith's persistent data by PostgreSQL schema without losing existing data.

## Data ownership

| Schema | Owner | Tables |
| --- | --- | --- |
| `catalog` | Catalog | products, catalog categories, media files, and product details |
| `reference` | Reference | additional references, fabrics, garment accessories, garment parts, operations, suppliers, and reference product categories |
| `identity` | Identity | ASP.NET Identity tables and refresh sessions |
| `accounting` | Accounting | transaction categories |
| `public` | PostgreSQL / shared infrastructure | existing `__EFMigrationsHistory`; after rollout, no module-owned tables remain |

`public.ProductCategories` is a legacy table with no current Reference entity mapping. The active Catalog model owns `catalog.ProductCategories`; Accounting must not reuse either table and stores its aggregate in `accounting.TransactionCategories`. If the legacy table exists, the Reference migration drops it; `DROP TABLE IF EXISTS` also supports databases where it has already been removed.

## Migration safety

- Existing Reference and Identity tables are moved between PostgreSQL schemas through generated EF migrations; this preserves rows, indexes, and constraints.
- The previous initial Accounting migration cannot be applied because it attempts to create `public.ProductCategories`, which Reference already owned. Replace it before applying any Accounting migration.
- The shared migration-history table remains in `public`; moving it would deprive each DbContext of baseline information about applied migrations.
- The Reference migration's confirmed deletion of legacy `public.ProductCategories` is irreversible. Its `Down` method restores only schema moves for active Reference tables.
