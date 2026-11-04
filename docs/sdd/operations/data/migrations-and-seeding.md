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

- `RoleSeeder` creates any missing role: `Director`, `Admin`, `Accountant`, `Manager`, `WorkshopChief`, `User`.
- `IdentitySeeder` creates `Identity:AdminUser:Email` (`vitaliythupin@gmail.com`) as Admin when that email has no account and `ADMIN_DEFAULT_PASSWORD` is set. Other existing users do not stop it. It never changes an existing password; it only aligns `LockoutEnabled`.

The password comes only from `ADMIN_DEFAULT_PASSWORD` (Secret Manager `admin-default-password`, attached to the `brevierp-seed` job by the deploy workflow). If the value is absent, administrator creation is skipped and this state is logged. Change the password later on the «Користувачі» page, not by changing the secret.

## Production

Normal Host.Seed runs apply the `production` schema migrations (history in `production.__EFMigrationsHistory`). `InitialProduction` creates empty tables; workers and fabric norms are loaded from the Admin «Швейний цех» import with a preview ([PRD-001](../../specs/production/001-workshop-references/README.md)).

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
   $api = "https://api.brevi.com.ua"
   $session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
   Invoke-RestMethod -Method Post -Uri "$api/api/auth/cookie/login" -WebSession $session -ContentType "application/json" `
     -Body (@{ email = "<manager e-mail>"; password = Read-Host "Password" } | ConvertTo-Json) | Out-Null
   $csrf = ($session.Cookies.GetCookies($api) | Where-Object Name -eq "brevi.csrf").Value
   $headers = @{ "X-CSRF-Token" = $csrf }
   $body = [System.IO.File]::ReadAllText("out\products-import.json", [System.Text.Encoding]::UTF8)
   $report = Invoke-RestMethod -Method Post -Uri "$api/api/v1/products/sheet-import" -WebSession $session -Headers $headers `
     -ContentType "application/json; charset=utf-8" -Body ([System.Text.Encoding]::UTF8.GetBytes($body))
   $report | ConvertTo-Json -Depth 6 | Out-File -Encoding utf8 out\products-import-preview.json
   ```

3. Review the preview with the owner: `sewing` and `ppe` counts, every row of `toDelete` (old products that will be deleted), `problems` (must be empty to apply) and `warnings` (imported anyway: unknown rule, no supplier price or link). Fix problems in the sheet and repeat steps 1–3.

4. Apply: set `dryRun` to false and send the same body. `applied: true` means everything was saved; a 409 means nothing was saved, so repeat the preview.

   ```powershell
   $apply = $body -replace '"dryRun":\s*true', '"dryRun": false'
   Invoke-RestMethod -Method Post -Uri "$api/api/v1/products/sheet-import" -WebSession $session -Headers $headers `
     -ContentType "application/json; charset=utf-8" -Body ([System.Text.Encoding]::UTF8.GetBytes($apply)) |
     ConvertTo-Json -Depth 6
   ```

5. Check: a second preview of the same body reports every row as unchanged and an empty `toDelete`.

Rollback: there is no undo endpoint. Take a database backup before step 4; deleted products, their photos, categories and PPE price history come back only from it.

## Old-site photo import

The migration `MediaFileSourceKey` adds `media.Files.SourceKey` on the next deploy. The import itself ([CAT-018](../../specs/catalog/018-product-photos-and-old-site-import/README.md)) runs from the Admin page «Товари» (tab «Швейна продукція») → «Імпорт зі старого сайту», not Host.Seed: pick `old-site-photo-import.json` (built by `/mnt/project-files/brevi-photo-import/build_import.py` from the OpenCart export), press «Перевірити» (dry run, writes nothing), review the counts with the owner, then «Імпортувати» (parts of 20 products). It downloads from `https://brevi.com.ua/image/`, so the old site must stay online until it finishes. A repeat adds nothing twice; failed photos are listed and can be retried by running it again.

## Old КП transfer

The one-time transfer of КП from the old Calculator ([QTE-001 R-25](../../specs/crm/003-quotes/README.md)) runs through the API, not Host.Seed. It needs no migration. A dry run never writes; the real run writes every new КП in one transaction or nothing. Run it after the Catalog products and Reference rows are imported: items are linked by their codes.

1. Take a full export from the Calculator («Экспорт КП», all quote tables with `"complete": true`) and generate the body. The script writes `quotes-import.json` (with `dryRun: true`; only the current version of each non-deleted КП) and `quotes-import-summary.md`:

   ```bash
   python3 -I /mnt/project-files/brevi-calculator-references/transfer/quotes_rows.py brevi-quotes.json out/
   ```

2. Sign in as the Director or an Admin (the same PowerShell session as in [sheet product import](#sheet-product-import) step 2) and run the dry run:

   ```powershell
   $body = [System.IO.File]::ReadAllText("out\quotes-import.json", [System.Text.Encoding]::UTF8)
   $report = Invoke-RestMethod -Method Post -Uri "$api/api/crm/quote-import" -WebSession $session -Headers $headers `
     -ContentType "application/json; charset=utf-8" -Body ([System.Text.Encoding]::UTF8.GetBytes($body))
   $report | ConvertTo-Json -Depth 6 | Out-File -Encoding utf8 out\quotes-import-preview.json
   ```

3. Review the report with the owner: `created`, `renumbered` (numbers a new Brevi КП already had), `unknownPeople` (their КП go to the person who imports) and `warnings` (transferred with the Calculator prices, but «Перерахувати» cannot update those rows).

4. Run: set `dryRun` to false and send the same body. `applied: true` means everything was saved; a 409 means nothing was saved, so repeat the dry run.

   ```powershell
   $apply = $body -replace '"dryRun":\s*true', '"dryRun": false'
   Invoke-RestMethod -Method Post -Uri "$api/api/crm/quote-import" -WebSession $session -Headers $headers `
     -ContentType "application/json; charset=utf-8" -Body ([System.Text.Encoding]::UTF8.GetBytes($apply)) |
     ConvertTo-Json -Depth 6
   ```

5. Check: a second dry run of the same body reports `created: 0` and every number in `alreadyImported`. A later export can be sent the same way: only КП that were not transferred are added.

Rollback: there is no undo endpoint. Take a database backup before step 4; a transferred КП can also be deleted one by one in the Admin while it is a draft.
