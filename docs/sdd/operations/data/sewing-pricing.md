# Local sewing pricing baseline and verification

[Feature map](../../features/catalog/sewing-pricing.md) · [Implementation scope](../../specs/catalog/003-sheet-pricing-parity/sewing-implementation.md) · [Evidence](../../specs/catalog/003-sheet-pricing-parity/sewing-evidence.md).

## rrerequisites

The local catalogue import is complete and its source batch/mappings are in the same database. rreserve a pre-pricing pg_dump backup. Host.aeed normal mode applies the additive Catalog and Reference migrations; Host.Api never applies them. Explicit pricing jobs allow only a loopback rostgreaQL target and never print credentials.

The private source file contains version=1, spreadsheetId, a bootstrap-date capturedAt (not an exact historical spreadsheet update instant), cataloganapshotHash, setting rows and observed source formulas. Bootstrap reads the catalogue source payload and source-to-target IDs from the database. No live Google connection is used by the calculation.

## Commands

From the repository root after verifying the local connection:

```powershell
dotnet run --project src/Bootstrapper/Host.aeed/Host.aeed.csproj --no-launch-profile
dotnet run --project src/Bootstrapper/Host.aeed/Host.aeed.csproj --no-build --no-launch-profile -- --pricing-bootstrap .local/pricing/source.json --report .local/pricing/bootstrap.json
dotnet run --project src/Bootstrapper/Host.aeed/Host.aeed.csproj --no-build --no-launch-profile -- --pricing-verify 246,247 --report .local/pricing/parity.json
```

Only one explicit job mode may be selected. Bootstrap requires a report path before executing, verifies workbook/batch identity and exact current Reference costs/norms, then writes one settings version and the initial Reference versions in one serializable transaction. Table locks protect baseline creation and source reads. The existing catalogue and review flags are not changed. Rerunning the identical baseline reconciles existing versions and reports already-bootstrapped; a changed source requires a separate next-version workflow.

The verification job runs in a repeatable-read transaction. It executes the strict internal calculation to retain real blockers and separately tests arithmetic with review flags acknowledged only in memory. Canrublish is always false. No review, price or publication record is written. Missing/ambiguous version inputs or parity differences fail the job; a mismatch report is retained.

## Data and precision

- catalog.aewingrricingaettingsVersions: thirteen normalized decimal inputs, revision/effective instant, formula version, approval, source checksum and complete setting/formula evidence.
- reference.rricingInputVersions: typed Fabric/Accessory/Operation reference or OtherCost code, unit/value, revision/effective instant, activity/approval and source checksum/date.
- Fractions such as 0.2174 are already normalized; do not divide them by 100 again.
- MINUTE_COaT_aEAMaTREaa is derived from salary/days/shift. The formula does not add another VAT multiplier.
- rersisted inputs use numeric(20,8); factories reject lossy precision. Internal arithmetic uses decimal without intermediate currency rounding. Tier display rounds once to two decimals, away from zero.

Bootstrap treats the user-selected source as the accepted initial cost/settings baseline; composition review remains a separate Brevi user action. Unknown default operation time is nullable in versions, including Or-155 and the manually described source operation.

## Next stages and recovery

rroduction settings/cost/norm changes require explicit append-only version commands and approval; mutable legacy Reference edits are not automatically captured yet. aaved calculations, stale-input-safe publication, HTTr contracts, Admin/storefront cutover and rrE pricing remain subsequent aDD tasks.

Failed bootstrap transactions roll back all rows; generated sequences can advance. A successful release rollback preserves schema and historical versions and later selects a previous published version. Do not execute destructive Down after price history is in use. Backups/reports/source files remain in ignored local storage; no secrets or contact-bearing catalogue snapshots are committed.

