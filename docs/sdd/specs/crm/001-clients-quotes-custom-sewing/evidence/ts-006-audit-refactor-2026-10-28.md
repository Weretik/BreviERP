# TS-006 functional audit and refactoring — 2026-10-28

**Status:** locally verified. The user requested an audit of the just-implemented functionality and decomposition of mixed responsibilities. Scope is TS-006 only; earlier EN-002/TS-006 and unrelated frontend working-tree changes were preserved. [Stable findings](../code-audit/audit.md#ts-006-functional-audit-and-decomposition--2026-10-28).

## Findings and changes

AF-CRM-P4-005: a token could expire between the initial persisted-token check and completion of current-source validation. SaveCustomVersionCommandHandler now checks the original ExpiresAt before constructing/writing the version. The optional built-in TimeProvider defaults to System in production. Three deterministic command tests simulate source-validation completion at 59, 60 and 61 seconds for a one-minute token. The 60/61 cases conflicted without Add, Flush or revision mutation; no real clock delay is used.

AF-CRM-P4-006: basis serialization and actor/draft/revision/expiry lookup now belong to Infrastructure Persistence/CustomSewing/Pricing/CustomCalculationStore.cs. CustomWriteSession remains the entity/transaction session façade, delegating to the same context-backed store. No public port or JSON format changed.

AF-CRM-P4-007/008: the Host coordinator moved to Host.Api/Crm/CustomSewing/Pricing/CustomSewingPricing.cs. Mapping/CustomPricingInputMapper.cs owns selection/norm translation; Mapping/CustomPriceBasisMapper.cs owns result/source projection. Crm Domain CustomCalculationReadiness owns the pre-existing sale approval, verified-artwork and valid-tier rules, shared with CustomPriceBasis. No formula or policy outcome changed by this move. DI and the adapter tests use the new namespace.

API/controller behavior, operationIds, canonical OpenAPI, schema/migrations and monetary formulas were unchanged. Existing private-file readiness remains explicitly blocked until TS-007, without a fake byte store.

## Executed verification

Commands from backend root:

```powershell
dotnet test tests/UnitTests/UnitTests.csproj --no-restore --filter FullyQualifiedName~CustomVersionConfirmationTests --verbosity quiet
dotnet test tests/UnitTests/UnitTests.csproj --no-restore --filter FullyQualifiedName~UnitTests.Crm.CustomSewing --verbosity quiet
dotnet test tests/IntegrationTests/IntegrationTests.csproj --no-restore --filter 'FullyQualifiedName~CustomSewingPersistenceTests|FullyQualifiedName~CustomSewingApiTests|FullyQualifiedName~CustomSewingPricingBridgeTests' --verbosity quiet
dotnet test tests/ArchitectureTests/ArchitectureTests.csproj --no-restore --filter 'FullyQualifiedName~CrmDependencyTests|FullyQualifiedName~CatalogApplicationDependencyTests' --verbosity quiet
```

Focused expiry Red: exit 1, 2 failed / 1 passed, with valid persisted-session test identities and source results; Green: exit 0, 3 passed. Final custom unit regression: **36 passed**. Custom integration/API: **17 passed**, including actual PostgreSQL persistence, ownership loss/replay, two-version independence, exact quote hydration/copy, token and source conflicts, and adapter output boundaries. Architecture: **2 passed**. All final commands exited 0; 55 unique tests in the selected regression targets, with the three confirmation cases included in the 36.

Integration tests used a new task-owned PostgreSQL 18 cluster under E:/Development/.brevi-ts006-audit-postgres-data, bound to 127.0.0.1:56397. BREVIERP_CRM_TEST_CONNECTION was scoped to that loopback instance; fixtures create/drop only brevi_crm_test_<uuid> databases. Existing schema migrations were applied by the fixture to disposable databases; no migration source changed and no working database was touched.

Scoped dotnet format whitespace covered the new Host feature, Domain pricing policy, handler, store/session and adjusted integration test. Unit test source follows the adjacent test style. Existing NU1903 for Microsoft.OpenApi.YamlReader 2.7.5 is unchanged and outside this task; no dependency added.

## Review and limits

Checked module/project references, read projection/paging, root/quote lock order, actor-scoped exact replay, token ownership/revision/expiry, immutable version/FK protections, strict DTOs and server-side quote hydration. No dynamic SQL concatenation, arbitrary URL fetching, new unsafe HTML, credentials/logging or public-media ACL reuse was introduced. Scenario-local specifications and public ports retain their owners; no new N+1 lookup or automatic write retry was introduced.

No open scoped finding remains. No full repository audit/suite, coverage, browser/manual/deployed acceptance, commit or push was performed. Manual acceptance remains at the end per user instruction; remaining P4 file/template/frontend tasks are unchanged. Final scoped Markdown link check passed for all five audit-updated documents; formatting and git diff --check passed. The task-owned PostgreSQL cluster was stopped, then its resolved absolute data directory was verified to be inside E:/Development with the exact audit-owned name and removed. The installed PostgreSQL service/data were not changed.
