# Pricing consumer cutover and read-only verification

[TS-007](../../specs/catalog/003-sheet-pricing-parity/cutover/README.md) owns the transition. The two current pricing boundaries are Sewing publications and current PPE input-version calculation. Product metadata price fields are deprecated: minimumWholesalePrice/retailPrice/wholesalePrice are zero placeholders and sewing.prices is null. Never treat these as current prices.

## Verification

Use the verified local target. These modes do not migrate, seed, approve or publish. Reports must use ignored local paths and retain source dates/hashes, version IDs and strict blockers.

```powershell
dotnet run --project src/Bootstrapper/Host.Seed/Host.Seed.csproj --no-build --no-launch-profile -- --pricing-verify 246,247,350,374,386,472 --report .local/pricing/ts007-sewing.json
dotnet run --project src/Bootstrapper/Host.Seed/Host.Seed.csproj --no-build --no-launch-profile -- --ppe-pricing-verify true --report .local/ppe-pricing/ts007-parity.json
```

PPE reads all imported source SKUs in batches of at most 100; each batch has a consistent version snapshot. It is not a transaction across the whole catalogue. Missing source values are reported separately; numeric differences, invalid source values and missing calculations with a numeric expected value fail after the report is saved. Diagnostic approvals exist in memory only; all real approvals/history stay unchanged. SKU 724 is the verified source gap, not a price to synthesize.

## Release order

1. Confirm active consumer inventory. Admin uses the two Pricing APIs; Storefront currently uses local data and quote API adoption is a separate feature. Unknown external consumers require review before an actual deployment.
2. Release the Admin transition first: no old PPE fallback/bands, explicit missing-input states, existing canonical price endpoints. It works against the preceding backend as well.
3. Activate the backend metadata-only reads and remove the retired arithmetic. Wire shapes are retained, but old price semantics change; do not deploy the backend alone for a client that still uses these fields.
4. Recheck current pricing and version/history behavior in the intended environment before production acceptance. This local task does not perform that deployment.

Recovery restores a compatible application pair; retain approved input/publication records and report provenance. Never re-enable the incorrect old price formula or silently approve imported data as a rollback.
