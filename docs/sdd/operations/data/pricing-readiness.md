# Imported pricing readiness

[TS-008 specification](../../specs/catalog/003-sheet-pricing-parity/activation/README.md) defines the rules, acceptance and gated activation sequence. Use this explicit read-only mode after import/TS-007, before reviewing or publishing prices.

```powershell
$env:DOTNET_ENVIRONMENT = 'Production'
dotnet run --project src/Bootstrapper/Host.Seed/Host.Seed.csproj --no-build --no-launch-profile -- --pricing-readiness true --report .local/pricing/ts008-readiness.json
```

The host reads its configured local connection; do not print credentials. The report parent directory must exist. The mode requires a report path, rejects non-loopback database hosts and any simultaneous explicit job mode, and never invokes normal migrations/seeders. A successful report command can contain blocked items: success means the inventory completed, not that prices were accepted.

Each imported row is retained. Strict readiness comes from current Application/Domain evaluation with real approval flags. Source missing/invalid price outputs carry cell addresses separately. PPE includes selected rule/offer IDs and approval state; Sewing includes settings/reference IDs and current publication ID. An existing publication and a new calculation's readiness are separate. NextAction is advisory; CanActivate is always false.

Review all blockers, source issues and input decisions before using the existing manager workflows. Refresh this report after edits and before activation; PPE batches and the Sewing snapshot are not one catalogue-wide transaction. The output is evidence, not a write token or approval list. Preserve ignored reports locally and only commit aggregate summaries without catalogue amounts or secrets.

Readiness has no schema/data rollback because it writes only its report. For later activation, follow [the task checklist](../../specs/catalog/003-sheet-pricing-parity/activation/tasks.md); retain history and use explicit version-aware corrections rather than SQL rewrites.
