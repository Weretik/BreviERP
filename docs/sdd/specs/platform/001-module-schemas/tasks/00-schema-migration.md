# Phase 00 — module schema migration

- [x] Define table ownership and confirm deletion of obsolete `public.ProductCategories`.
- [x] Configure each DbContext for its module schema.
- [x] Generate migrations through EF CLI only.
- [x] Generate and review idempotent SQL scripts.
- [x] Apply migrations to an isolated empty test database, verify schema, rollback and re-apply.
- [x] Apply verified migrations to the local database and verify rows/schema.

## Rollout order

1. Reference
2. Identity
3. Accounting
4. Catalog (already in `catalog`)

Rollback of Reference or Identity is a PostgreSQL schema move back to `public`, performed through the migration `Down` path. Accounting has no existing table to move; its first verified migration creates `accounting.TransactionCategories`.

## Verification

- `brevidb_schema_verify`: all Catalog, Reference, Identity and Accounting migrations applied; Reference, Identity and Accounting rollback/re-apply completed successfully.
- `brevidb`: Reference rows before and after move were identical: AdditionalReferences 14, Fabrics 44, Accessories 37, Operations 151, Parts 34, Suppliers 27. Identity retained 1 user, 3 roles and 0 refresh sessions.
- Final local structure has module-owned tables in `catalog`, `reference`, `identity` and `accounting`; `public` retains only `__EFMigrationsHistory`.
