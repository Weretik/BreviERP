# Local PPE pricing bootstrap

[Implementation](../../specs/catalog/003-sheet-pricing-parity/ppe-implementation.md) · [Feature map](../../features/catalog/ppe-pricing.md).

Run migrations through normal Host.Seed before the explicit job. Verify the loopback target and a readable backup first. Import mode never migrates or seeds implicitly and never approves inputs. Keep source/report files in ignored `.local/ppe-pricing/`.

The version-1 JSON has spreadsheetId, capturedAt (ISO date), rules (header plus `Налаштування цін!J:M` rows), products (header plus `ЗІЗ!A:R` rows). It contains no credentials. Rule fractions are already numeric fractions, never divide by 100 again. The source SKU set must equal imported PPE profile SKUs exactly. Duplicate/invalid source and changed baseline are rejected. Missing supplier cost remains null. Shared advisory locking and one transaction prevent partial/concurrent baseline writes. Identical reruns verify original draft rows even after later approval versions exist.

```powershell
dotnet run --project src/Bootstrapper/Host.Seed/Host.Seed.csproj --no-launch-profile

dotnet run --project src/Bootstrapper/Host.Seed/Host.Seed.csproj --no-build --no-launch-profile -- --ppe-pricing-bootstrap .local/ppe-pricing/source.json --report .local/ppe-pricing/bootstrap.json
```

Review the report: expected full current source is ten rule drafts and 119 offer drafts. Source reviewed flags/dates remain evidence; no Brevi actor is invented. A manager must explicitly approve/edit the relevant input through version APIs before required fallback or external values can calculate. Generic source codes absent from the rule table are retained exactly; approved complete external prices need no fallback rule. Never invent a category-to-rule mapping.

There is no destructive automatic rollback. Preserve versions after user edits; restore a verified backup only through the separate recovery procedure. Do not run this local-only job against staging/production. Source changes require a separately designed merge, not a rerun over modified data.
