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

Clean immutable local backend commits provide frontend's required sync/generate/check; final consumer provenance is recorded in frontend docs/contracts/openapi/SOURCE.json. No commit or push is inferred by implementation authorization. Preserve the pre-existing pricing-index edit in docs/sdd/specs/README.md; do not stage unrelated content just to satisfy the clean-tree gate. Use an isolated clean checkout of the authorized commit for sync if unrelated source-tree edits remain.

## Final merge provenance regression

A focused Red case proved distinct external IDs from the same source/type were incorrectly treated as conflicting. Domain now distinguishes the complete `(source,type,externalId)` key; Admin merge keeps both proven source identities. Unit `CustomerRulesTests`: 7 passed after the fix. PostgreSQL `CustomerPersistenceTests` rerun: 8 passed, including both source identities moving to the survivor. HTTP schemas/routes and the migration are unchanged. This is a correctness fix within the authorized client slice, not a new import feature.

## Final authorization and integrated handoff

AF-CRM-005, correctness, verified: proven duplicate clients with distinct source IDs from the same source/type now merge and retain both complete source keys. A valid Domain Red test failed before the correction; final CustomerRulesTests: 7 passed.

AF-CRM-006, authorization, verified: replay of an old create/contact mutation for a merged-away root originally returned its saved response after that root became inaccessible. A real PostgreSQL Red test observed Ok instead of NotFound; replay now requires an unmerged, currently accessible root. Final `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter FullyQualifiedName~IntegrationTests.Crm --no-restore --verbosity quiet --logger 'console;verbosity=normal'`: 12 passed (9 PostgreSQL + 3 HTTP), no skips. Existing architecture test: 1 passed. No HTTP schema or migration change was needed for these final corrections.

The existing Admin Clients entry is connected and its focused model/data-access/interaction tests pass (2 + 4 + 10). Consumer contract provenance is the final backend commit recorded in frontend SOURCE.json; shared generated types are reproducible. Manual browser/visual acceptance remains pending by project rules. Local schema migration is applied and repeated with zero pending migrations; no historical clients were imported.

## Final input-boundary regression

AF-CRM-007, validation, verified: null entries inside external-identity/contact-channel arrays were not rejected by their validators. Two focused Red tests observed IsValid=true. Explicit per-item NotNull guards now stop invalid requests before handler/persistence. `dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~UnitTests.Crm.Customers --no-restore --verbosity quiet --logger 'console;verbosity=normal'`: 9 passed (7 Domain + 2 input validation). Canonical schemas already require non-null objects; no schema or migration change was needed. Final focused totals: 9 unit + 12 PostgreSQL/HTTP + 1 architecture checks passed. Frontend has 16 focused checks.
