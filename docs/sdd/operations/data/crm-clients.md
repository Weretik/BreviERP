# Crm client migration job

CRM uses the existing Host.Seed deployment mechanism. Host.Api does not apply migrations. Normal Host.Seed now includes Crm's registered migrator; a local CRM-only mode avoids running unrelated seeders/imports.

```powershell
dotnet run --project src/Bootstrapper/Host.Seed/Host.Seed.csproj -- --crm-migrate=plan
dotnet run --project src/Bootstrapper/Host.Seed/Host.Seed.csproj -- --crm-migrate=apply
```

Resolve the intended `ConnectionStrings:Default` through existing secure configuration first; never log the full connection string. This explicit mode rejects nonlocal hosts and system databases, requires exactly one job mode, and reports database name plus pending migrations before/after. Plan reads migration metadata; apply changes only Crm schema and migration history. It does not seed clients, import sources, approve prices or invoke the normal module seeders. An empty client list is expected until users create clients or a later authorized import runs.

Review additive `InitialCustomerMaster` Up/Down and existing target/backup before apply. It creates Customers, CustomerContacts, ContactChannels, CustomerExternalIdentities and MutationRequests with indexes/constraints. It does not alter Catalog/Reference/Identity tables. Reapplying is a no-op after the migration history is written. Do not use Down after clients are used: it removes their history; disable UI/action and recover forward or restore a coordinated backup under the project procedure.

Tests use an explicit `BREVIERP_CRM_TEST_CONNECTION` to localhost and create/drop only their uniquely named `brevi_crm_test_<uuid>` database. They do not truncate the configured application database. Record actual local job outcome in [CRM task evidence](../../specs/crm/001-clients-quotes-custom-sewing/README.md), including database name/migration ID/pending count, never credentials. Historical customer import is a later separate job, not this mode.
