# Local catalogue import

[Feature map](../../features/catalog/catalog-import.md) · [SDD](../../specs/catalog/002-sewing-articles-and-composition/catalog-import.md) · [Field map](../../specs/catalog/002-sewing-articles-and-composition/import-field-map.md).

## Inputs and target

Only an explicitly requested local transfer is supported. The job rejects a non-loopback PostgreSQL host. ConnectionStrings:Default is resolved by Host.Seed; never print or commit credentials. The version-1 snapshot contains spreadsheet ID, capture date, sheet values, model website URLs and optional raw formula evidence. The snapshot is a frozen sequence of bounded reads, not a transactional Google backup. It excludes account/session credentials.

Keep source files, pg_dump archives and detailed contact-bearing reports in ignored local storage. The completed run used .local/catalog-import/; this path is excluded through local Git configuration, not a committed data bundle.

## Run order

1. Verify the local target and back it up with pg_dump. Verify that the archive can be listed by pg_restore.
2. Run normal Host.Seed once to apply pending Catalog and import provenance migrations. Import modes deliberately do not apply migrations or ordinary seeders.
3. Validate the frozen full source. This checks source relationships and the initial empty-Product condition without catalogue writes.
4. Trial MOD-025. The job writes its two Articles and all current composition lines, reconciles stored keys/values, then rolls back the entire transaction, including Reference and provenance writes.
5. Apply the full source and inspect the report, database counts, source mappings, and any missing prices.
6. Rerun apply against the identical file. Expected status is already-applied with unchanged row counts; data drift detected by reconciliation causes failure.

From the repository root, after the required target checks:

```powershell
dotnet run --project src/Bootstrapper/Host.Seed/Host.Seed.csproj --no-launch-profile
dotnet run --project src/Bootstrapper/Host.Seed/Host.Seed.csproj --no-build --no-launch-profile -- --catalog-import .local/catalog-import/frozen-source.json --mode validate --report .local/catalog-import/validate.json
dotnet run --project src/Bootstrapper/Host.Seed/Host.Seed.csproj --no-build --no-launch-profile -- --catalog-import .local/catalog-import/frozen-source.json --mode trial --report .local/catalog-import/trial.json
dotnet run --project src/Bootstrapper/Host.Seed/Host.Seed.csproj --no-build --no-launch-profile -- --catalog-import .local/catalog-import/frozen-source.json --mode apply --report .local/catalog-import/apply.json
```

The mode defaults to validate; report is mandatory. A changed file hash against an imported or unrelated nonempty catalogue is rejected. The report states counts, mapped references, changed Reference values and row-addressed warnings. The same report and complete source payload are stored in imports.CatalogImportBatches; imports.CatalogSourceMappings stores source-to-Brevi IDs.

## Transactions and recovery

Catalog, Reference and provenance share one serializable PostgreSQL transaction. A transaction advisory lock serializes jobs; explicit table locks protect the legacy manually allocated IDs. A concurrent write may fail with a serialization conflict; investigate and rerun explicitly rather than adding automatic write retries.

Failures and trial runs roll back all rows; PostgreSQL generated sequences may advance. After a successful import, recovery uses the pre-import backup in a separate verified restore workflow. Do not downgrade the new Catalog migration with metre accessories or repeated operations present: its Down restores earlier restrictive constraints. No destructive rollback after user edits is automatic.

## Verification boundary

The initial import is implemented. Changed-snapshot merge, production cutover, price-rule import/publication, consumer UI for Product profile metadata, and complete historical D1 transfer are separate work. Source-only descriptive/reference attributes and derived price columns remain accessible in database provenance; they are not all promoted to separately editable operational fields.

