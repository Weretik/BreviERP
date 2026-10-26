# Migrations and seeding

The [local catalogue import](catalog-import.md) is an explicit Host.Seed job mode. Normal execution applies migrations and seeders; import modes validate, trial or apply a frozen source without implicit migrations or ordinary seeding.

The [local sewing pricing baseline and verification](sewing-pricing.md) adds explicit pricing-bootstrap and pricing-verify modes. Bootstrap writes versioned settings/cost inputs; verification is read-only and cannot publish.

The [PPE pricing bootstrap](ppe-pricing.md) adds an explicit `ppe-pricing-bootstrap` mode for imported draft rule/offer versions. It requires prior migrations, preserves source review separately and never approves data automatically.

## Execution order

In BreviERP, migrations and seeding run through `IDatabaseMigrator`/`ISeeder` in a separate `Host.Seed`. `Host.Api` does not run these operations itself. In the [Cloud Build configuration](../deployment/cloud-build.md), the `Host.Seed` Job runs before API deployment; check the result of a specific run to confirm success.

```text
Host.Seed
  → registered IDatabaseMigrator / DbContext
  → registered ISeeder (one instance of each seeder type)
  → operation complete
```

Catalog, Identity, Reference, and Accounting module contexts are implemented. Running a migrator or seeder can change the database schema and insert or update seed data. Do not point a local process at a database unless that effect is intended.

## Current Identity seeding

- `RoleSeeder` ensures the `Admin`, `Manager`, and `User` roles exist.
- `IdentitySeeder` creates the configured administrator only when absent and `ADMIN_DEFAULT_PASSWORD` is set.

The password is not read from `Identity:AdminUser:DefaultPassword`; the seeder reads `ADMIN_DEFAULT_PASSWORD`. If the value is absent, administrator creation is skipped and this state is logged.

## Change rules

- Create a migration only when a feature changes the persistence schema or data contract.
- Put migration design and rollout/verification steps in the [feature specification](../../specs/README.md): baseline and rollback in `design/`, rollout/verification in `tasks/`.
- Do not edit an applied migration to alter production history.
- Treat seed data changes as data changes: document idempotency, behavior with existing data, and the rollback/recovery approach.
- Check startup or Host.Seed logs after changing a migration or seeder.

## Pricing cutover diagnostics

The explicit Sewing/PPE verification modes are read-only and do not apply migrations, import, approve inputs or publish prices. See [pricing cutover operations](pricing-cutover.md) for commands, source gaps and frontend-first release order. TS-007 has no schema migration.

The [full imported pricing readiness report](pricing-readiness.md) is an explicit read-only mode before input review/activation; it does not approve or publish prices.

The [frozen-source pricing input review](pricing-input-review.md) compares current data before confirmation and is database-enforced read-only.

[Explicit local pricing activation](pricing-activation.md) is a separately authorized state-changing mode with plan, manager attribution, backup and exclusion guards. It never invokes normal migration/seed flow.
