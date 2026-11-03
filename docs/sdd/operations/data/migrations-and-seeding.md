# Migrations and seeding

The [local catalogue import](catalog-import.md) is an explicit Host.Seed job mode. Normal execution applies migrations and seeders; import modes validate, trial or apply a frozen source without implicit migrations or ordinary seeding.

The [local sewing pricing baseline and verification](sewing-pricing.md) adds explicit pricing-bootstrap and pricing-verify modes. Bootstrap writes versioned settings/cost inputs; verification is read-only and cannot publish.

The [PPE pricing bootstrap](ppe-pricing.md) adds an explicit `ppe-pricing-bootstrap` mode for imported draft rule/offer versions. It requires prior migrations, preserves source review separately and never approves data automatically.

## Execution order

In BreviERP, migrations and seeding run through `IDatabaseMigrator`/`ISeeder` in a separate `Host.Seed`. `Host.Api` does not run these operations itself. In the [deploy workflow](../deployment/cloud-run-deploy.md), the `Host.Seed` Job runs before API deployment; check the result of a specific run to confirm success.

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

## Media

Normal Host.Seed runs apply the `media` schema migrations (history in `media.__EFMigrationsHistory`). `InitialMedia` copies uploaded photos from `catalog."MediaFiles"` with the same ids into the folder «Товари» once; the old table stays as a fallback copy ([MED-001](../../specs/media/001-media-library/README.md)).

## Reminders

Normal Host.Seed runs apply the `reminders` schema migrations (history in `reminders.__EFMigrationsHistory`). `DefaultReminderTaskSeeder` adds the 25 default reminder tasks only when the task table is empty, without a chat, so nothing is sent until a chat is approved and assigned. The `--reminders-dispatch=true` mode skips migrations and seeding; see the [Telegram reminders runbook](../jobs/telegram-reminders.md).

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

## CRM clients

The [CRM-only local migration mode](crm-clients.md) plans/applies only the client schema through Host.Seed without unrelated seeders or data import. Normal deployment migration registration also includes Crm.

## Sheet product import

The one-time product transfer from the Calculator sheet ([CAT-010](../../specs/catalog/010-sheet-product-import/README.md)) runs through the API, not Host.Seed. It needs no migration. A preview never writes; the apply call writes everything in one transaction or nothing. Run the Reference imports (sections, fabrics, accessories, operations, suppliers, PPE rules) first: the product import only resolves codes that already exist.

1. Generate the body from the client's workbook. The script writes `products-import.json` (with `dryRun: true`, `deleteMissing: true`) and `products-import-summary.md`:

   ```bash
   python3 -I /mnt/project-files/brevi-calculator-references/transfer/products_rows.py brevi.xlsx out/
   ```

2. Sign in as a product manager and run the preview (PowerShell):

   ```powershell
   $api = "https://brevierp-api-443622778364.europe-central2.run.app"
   $login = Invoke-RestMethod -Method Post -Uri "$api/api/auth/session/login" -ContentType "application/json" `
     -Body (@{ email = "<manager e-mail>"; password = Read-Host "Password" } | ConvertTo-Json)
   $headers = @{ Authorization = "Bearer $($login.accessToken)" }
   $body = [System.IO.File]::ReadAllText("out\products-import.json", [System.Text.Encoding]::UTF8)
   $report = Invoke-RestMethod -Method Post -Uri "$api/api/v1/products/sheet-import" -Headers $headers `
     -ContentType "application/json; charset=utf-8" -Body ([System.Text.Encoding]::UTF8.GetBytes($body))
   $report | ConvertTo-Json -Depth 6 | Out-File -Encoding utf8 out\products-import-preview.json
   ```

3. Review the preview with the owner: `sewing` and `ppe` counts, every row of `toDelete` (old products that will be deleted), `problems` (must be empty to apply) and `warnings` (imported anyway: unknown rule, no supplier price or link). Fix problems in the sheet and repeat steps 1–3.

4. Apply: set `dryRun` to false and send the same body. `applied: true` means everything was saved; a 409 means nothing was saved, so repeat the preview.

   ```powershell
   $apply = $body -replace '"dryRun":\s*true', '"dryRun": false'
   Invoke-RestMethod -Method Post -Uri "$api/api/v1/products/sheet-import" -Headers $headers `
     -ContentType "application/json; charset=utf-8" -Body ([System.Text.Encoding]::UTF8.GetBytes($apply)) |
     ConvertTo-Json -Depth 6
   ```

5. Check: a second preview of the same body reports every row as unchanged and an empty `toDelete`.

Rollback: there is no undo endpoint. Take a database backup before step 4; deleted products, their photos, categories and PPE price history come back only from it.
