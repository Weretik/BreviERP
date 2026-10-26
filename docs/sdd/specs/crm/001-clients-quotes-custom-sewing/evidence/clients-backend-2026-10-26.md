# Client/contact backend execution — 2026-10-26

## Delivered scope

EN-001, TS-001 and TS-002 client-only portions: typed customer/contact/channel/source IDs; active state, names and normalized search; one active primary; exact source identity uniqueness; owner-scoped page/detail projection; create/revise/deactivate; Admin-only atomic merge retaining survivor/victim identity and audit. Seven canonical HTTP operations, server actor claims, canEdit/canMerge, requestId replay and expectedRevision. Five new crm tables; separate migration history. Existing Catalog/Reference/Identity tables and pricing are not altered.

Current scope has no quotes: owner-only reads are correct now; quote-derived customer access and merge/live-quote reference updates must be added/tested with TS-004/005. Issued recipient history assertions in SC-C03/05 are not marked verified prematurely. No historical client import or CRM fake seed data.

## Checks actually run

- `dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~CustomerRulesTests --no-restore --verbosity quiet`: 6 passed. Initial Red: SelectingPrimaryContactDemotesPreviousPrimary failed because two active primary contacts remained; Green after demotion invariant; final regression passed after encapsulating collections.
- `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter FullyQualifiedName~IntegrationTests.Crm --no-restore --verbosity quiet --logger 'console;verbosity=normal'`: 11 passed (8 PostgreSQL, 3 HTTP). BREVIERP_CRM_TEST_CONNECTION is localhost only; fixtures create/drop unique brevi_crm_test_<uuid> DBs. Both first migration and repeated migration exercised. Covered SQL projections/pagination, foreign reads/writes, primary change/deactivation, atomic merge, two-writer revision race, concurrent identical-request replay, concurrent source-key collision rollback and HTTP role/actor/status shape.
- `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj --filter FullyQualifiedName~CrmDependencyTests --no-restore --verbosity quiet`: 1 passed.
- Targeted `dotnet build` of Crm.Infrastructure and Crm.Api: passed without errors.
- `dotnet ef migrations add InitialCustomerMaster --project src/Modules/Crm/Crm.Infrastructure/Crm.Infrastructure/Crm.Infrastructure.csproj --startup-project src/Modules/Crm/Crm.Infrastructure/Crm.Infrastructure/Crm.Infrastructure.csproj --context CrmDbContext --output-dir Persistence/Migrations`: generated additive migration `20261026121235_InitialCustomerMaster`; reviewed tables, FKs and unique indexes. Down removes only new client tables and is unsuitable after business use.
- Scoped `dotnet format whitespace` for Crm's four projects and changed Host.Api wiring; `git diff --check`: passed.

Known existing warning: UnitTests references Microsoft.OpenApi.YamlReader 2.7.5 with NU1903 advisory. This work does not change that package; CRM tests passed. No full suite/production build or browser/manual acceptance was run.

## Scoped audit

- AF-CRM-001, correctness, verified: selecting a new primary must demote previous one; Domain test proves the invariant, PostgreSQL partial unique index and ordered flush prove physical persistence.
- AF-CRM-002, correctness, verified: merge originally severed a restrictive required child FK. Domain now reassigns typed FK before transferring collections; real PostgreSQL merge passes with preserved contact PublicId/source identity and audit.
- AF-CRM-003, domain boundary, verified: contact/channel/source collections now use private backing lists and read-only collection exposure; scoped integration regression passed after this change.
- AF-CRM-004, transaction safety, verified: request locks and root locks plus unique constraints prevent duplicate roots/audit and partial losing graphs; concurrency tests prove rollback/replay.

No outstanding client-slice architecture findings. Future quote/import/file/provider functionality is planned scope, not a workaround in this slice.

## Local job and contract handoff

`Host.Seed --crm-migrate=plan` checked named localhost application database `brevidb`; initial pending migration: 20261026121235_InitialCustomerMaster. This plan is read-only. Local apply then repeat completed successfully through Host.Seed CRM-only mode: first pendingBefore contained 20261026121235_InitialCustomerMaster, pendingAfter empty; repeat pendingBefore/pendingAfter both empty. A custom-format local pg_dump backup was created before apply (temporary artifact basename brevi-crm-before-f8f766fe87e84449a42af1bfe1debe74.dump). No clients were seeded/imported. EF emitted an expected missing-history-table log during first creation; the job completed with exit 0 and an empty pending set. Standard Host.Seed migration registration includes Crm; CRM-only local mode excludes unrelated seeders/imports.

A clean immutable backend commit is still needed for frontend's required sync/generate/check. No commit or push is inferred by implementation authorization. Preserve the pre-existing pricing-index edit in docs/sdd/specs/README.md; do not stage unrelated content just to satisfy the clean-tree gate. Use an isolated clean checkout of the authorized commit for sync if unrelated source-tree edits remain.
