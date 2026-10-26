# Frozen-source pricing input review

[TS-008C specification](../../specs/catalog/003-sheet-pricing-parity/activation/review.md) defines acceptance and the handoff to explicit confirmation. This local-only command writes reports, never corrections or approvals.

```powershell
$env:DOTNET_ENVIRONMENT = 'Production'
dotnet run --project src/Bootstrapper/Host.Seed/Host.Seed.csproj --no-build --no-launch-profile -- --pricing-input-review true --settings-source .local/pricing/source.json --ppe-source .local/ppe-pricing/source.json --report .local/pricing/ts008-input-review.json
```

Use the existing frozen sources and verified local connection. Require all three paths; never combine this mode with import/readiness/bootstrap/verification. A repeatable-read, database-enforced read-only transaction covers Catalog, Reference and provenance. Workbook/hash compatibility is checked before comparison; missing/extra/changed original composition lines and different latest inputs appear as mismatches. The report is saved before the command fails on differing subjects. A source match can still have a hold (missing supplier data or inactive Product) and is never an approval token.

Also run the existing Sewing `pricing-verify` diagnostic for the full imported SKU list rather than only the original representative sample. Its nonzero result for MOD-061 is expected until the recorded decision is resolved; do not silence it or remove its differences. [Actual review and decision evidence](../../specs/catalog/003-sheet-pricing-parity/activation/review-evidence.md).

No migration/backup is needed for this read-only mode. Before later data corrections or confirmation, take the required backup and use existing version-aware manager workflows. Re-read composition/input versions; do not apply a stale report or write through SQL. Preserve source 724 as unresolved and 931 as inactive unless an explicit owner decision changes them.
