# Explicit local pricing activation

[TS-008D specification](../../specs/catalog/003-sheet-pricing-parity/activation/execution.md) and [standing exclusions](../../specs/catalog/003-sheet-pricing-parity/activation/data-decisions.md) define the accepted scope. Only a directly authorized local operator may invoke this state-changing mode. It checks the existing unlocked Admin/Manager actor; it does not create accounts or simulate an HTTP sign-in.

## Plan before write

```powershell
$env:DOTNET_ENVIRONMENT = 'Production'
dotnet run --project src/Bootstrapper/Host.Seed/Host.Seed.csproj --no-build --no-launch-profile -- --pricing-activate plan --actor-id 1 --plan .local/pricing/ts008d-activation-plan.json --settings-source .local/pricing/source.json --ppe-source .local/ppe-pricing/source.json
```

Actor 1 is the verified local account for this execution, not a universal default. Resolve and verify the intended existing actor in another environment. Plan creation requires a fresh exact input-source review. It preserves source-issue blocking outside explicitly confirmed exclusions. The immutable plan records the exact local database name, actor, source hash, semantic inputs, expected versions, common backend-calculated expected results and stable request UUIDs. Existing plan files are not overwritten.

## Backup and apply

Create a pg_dump custom-format backup of the exact local target, verify its pg_restore listing/size/header/hash and retain it under ignored local storage. Credentials are supplied through temporary environment variables, never printed or placed in command arguments. Record its proof before writing. Then invoke:

```powershell
dotnet run --project src/Bootstrapper/Host.Seed/Host.Seed.csproj --no-build --no-launch-profile -- --pricing-activate apply --confirmed true --actor-id 1 --plan .local/pricing/ts008d-activation-plan.json --plan-sha256 <verified-plan-sha256> --backup <verified-custom-dump> --backup-sha256 <verified-backup-sha256> --report .local/pricing/ts008d-activation-report.json
```

Apply validates confirmation, actor, exact plan hash, backup format/hash, loopback database/name and exclusion scope before calls to existing backend handlers. It tests the report path before any confirmation and preflights every model/PPE/version scope. Existing atomic workflows execute sequentially; progress is written atomically after each resource. Partial failures keep completed history. Resume with the same plan/request IDs/actor/backup, never a newly generated plan to hide progress.

The common composition handler increments its version twice (ReplaceComposition and SetModelCode); each Article replacement increments once. Resume recognizes exactly these existing increments and unchanged values/review scopes; it does not relax concurrency. Replacement line IDs are recorded through resulting compositions; original import mappings remain provenance, and input review resolves equivalent replacements by unique scope/reference/order.

Validate reopened current published Sewing/PPE results, not report totals alone. Rerun input review and readiness. Preserve 724 as unapproved/blocked and 931 as inactive/unapproved. No global formula or cached source price is rewritten. Current Settings/Reference inputs already accepted by the verified baseline are retained.

Recovery is an explicit versioned correction and re-publication with retained history. Full backup restoration needs a separate reviewed offline target and explicit approval. This mode never auto-restores/drops schema or runs normal seed/migrations. [Actual execution evidence](../../specs/catalog/003-sheet-pricing-parity/activation/execution-evidence.md).

## Reopen verification

Use `--pricing-activate verify --actor-id 1 --plan <same-plan> --plan-sha256 <same-hash> --report <ignored-output>` after apply. This read-only mode compares all current published Sewing/calculation identities and strict PPE results with the plan through existing read ports. It does not confirm or publish anything.
