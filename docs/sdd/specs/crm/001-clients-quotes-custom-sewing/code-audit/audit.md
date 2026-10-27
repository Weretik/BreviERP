# Client-slice structural audit — 2026-10-26

## Scope and criteria

Only the recently implemented customers/contacts slice (TS-001/TS-002), including its API, Application, Domain, persistence and tests. Baseline backend `bb0a9a710892a7dd25acad99def291c53ccf6da9`. Rules: repository AGENTS, backend-rules, backend-code-audit and backend-change-verification skills. User explicitly requested the fixes as well as the audit. This is local refactoring within Crm, not a cross-module redesign.

## Findings and resolution

| ID | Evidence before change | Impact | Resolution / owner | Status |
| --- | --- | --- | --- | --- |
| AF-CRM-STR-001 | `Customers/Models/CustomerDetail.cs` combined a response record and domain mapping; `Models` mixed actor, input, projection and response roles | Discovery and change scope unclear | TS-001/002: `Access`, `Contracts/Inputs`, `Contracts/Responses`, `Mapping/CustomerDetailMapper.cs`, use-case-local contact inputs/page DTO | Fixed |
| AF-CRM-STR-002 | `Persistence/CustomerMutationStore.cs` combined payload encoding, replay authorization, journal persistence and transaction orchestration | Journal changes coupled to transaction control | TS-001/002: `Customers/Idempotency/CustomerMutationJournal.cs` + encoded payload record; `Writing/CustomerMutationStore.cs` retains transaction/lock/error handling | Fixed |
| AF-CRM-STR-003 | Read repository, write session and mutation coordinator shared the context/configuration root | Persistence navigation obscured read/write responsibilities | TS-001/002: `Persistence/Customers/{Reading,Writing,Idempotency}` with updated DI/tests | Fixed |

Domain `Customer` retains one cohesive aggregate responsibility. EF context/entity/configuration/migration namespaces remain stable. No runtime source lookup, migration, API field or authorization change. DTO namespaces are internal source organization; JSON serialization remains identical. The journal uses the coordinator's context/transaction, preserving atomic intent/result storage, request lock keys, hashes and replay access checks.

## Verification

Fresh scoped checks are recorded below. Unit validation, PostgreSQL replay/concurrency/merge, HTTP authorization and Crm dependency tests cover the affected boundaries. Real PostgreSQL tests use isolated unique test databases; no import or operational database migration is part of this audit. Manual UI acceptance remains pending in the original slice evidence.

See [actual file map](../../../../features/crm/clients-and-quotes.md) and [original implementation evidence](../evidence/clients-backend-2026-10-26.md). No new product requirements/scenarios were added.

### Automated regression results

All commands run from the backend repository; exit code 0.

| Command | Result |
| --- | --- |
| `dotnet build src/Modules/Crm/Crm.Infrastructure/Crm.Infrastructure/Crm.Infrastructure.csproj --no-restore --verbosity quiet` | Build passed |
| `dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~UnitTests.Crm.Customers --no-restore --verbosity quiet` | 9 passed, 0 skipped |
| `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter FullyQualifiedName~IntegrationTests.Crm --no-restore --verbosity quiet` | 12 passed, 0 skipped (PostgreSQL + HTTP) |
| `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj --filter FullyQualifiedName~CrmDependencyTests --no-restore --verbosity quiet` | 1 passed, 0 skipped |

Integration connection supplied privately through `BREVIERP_CRM_TEST_CONNECTION` from existing local Host.Seed configuration and removed afterward. No connection values are recorded. Existing NU1903 and unrelated Identity/Accounting compiler warnings remain outside this refactoring. Self-review checked identical journal serialization/hash/lock scope, current access validation on replay, rollback/change-tracker cleanup, SQL projections/paging and cancellation. No new security/performance behavior was introduced. No migration, runtime data import, commit or push was performed.

### Formatting and documentation

`dotnet format whitespace BreviERP.sln --include <explicit changed/new CRM .cs paths> --no-restore --verbosity minimal` exited 0. The formatter reported workspace-loading warnings; affected projects independently built and passed the tests above. Explicit paths include changed CRM Application/API/Infrastructure files and the adjusted CRM tests only. Updated map/SDD/audit local links were checked successfully. `git diff --check` passed. The pre-existing unrelated pricing index edit is preserved.
