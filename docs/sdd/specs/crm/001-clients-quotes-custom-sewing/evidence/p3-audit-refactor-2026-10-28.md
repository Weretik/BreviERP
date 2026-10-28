# P3 audit and refactoring evidence — 2026-10-28

Authorized scope: newly implemented TS-005 and independent FE-005 comparison model/UI. User requested audit plus responsibility separation and authorized one local backend commit without push. No P4 or additional product behavior was added.

## Confirmed findings

[AF-CRM-P3-001–005](../code-audit/audit.md#p3-ts-005-audit-and-refactoring--2026-10-28) record exact evidence, impact, owners and fixes. The two behavior corrections are explicit archive-field presence and validation before domain mutation; structural corrections separate reprice snapshot conversion, scenario specifications and assignee HTTP composition.

## Red

- `dotnet test tests/IntegrationTests/IntegrationTests.csproj --no-restore --filter FullyQualifiedName~MissingArchiveDecisionIsRejectedBeforeHandler --logger 'console;verbosity=minimal' --verbosity quiet`: **1 failed**. Missing archived reached the sentinel handler and returned 409 instead of 400.
- `dotnet test tests/UnitTests/UnitTests.csproj --no-restore --filter 'FullyQualifiedName~InvalidVersionAuditCannotChangeRoot|FullyQualifiedName~InvalidActorCannotPartiallyMutateLifecycle' --logger 'console;verbosity=minimal' --verbosity quiet`: **5 failed**. Invalid append actor/action was accepted or failed after a pointer change; invalid lifecycle actor failed after owner/archive state changed.

## Green and regression

Focused Green: QuoteLifecycleApiTests **14 passed** and QuoteLifecycleRulesTests **10 passed**.

Final scoped backend commands:

```powershell
dotnet test tests/IntegrationTests/IntegrationTests.csproj --no-restore --filter 'FullyQualifiedName~IntegrationTests.Crm.Quotes' --logger 'console;verbosity=minimal' --verbosity quiet
dotnet test tests/UnitTests/UnitTests.csproj --no-restore --filter 'FullyQualifiedName~QuoteLifecycleRulesTests|FullyQualifiedName~Aggregate_contract_is_valid_and_all_references_resolve' --verbosity quiet
dotnet test tests/ArchitectureTests/ArchitectureTests.csproj --no-restore --filter FullyQualifiedName~CrmDependencyTests --verbosity quiet
```

Results: **41 integration, 11 domain/OpenAPI, 1 architecture tests passed; no skipped tests**. This includes P2 quote regression, copied graph/provenance, source drift and atomic confirmation, immutable V1, transfer access loss, scoped history, search/count/paging, migration upgrade/no-op and append-only event enforcement.

Tests use a separate task-owned local PostgreSQL cluster at 127.0.0.1:56389 with uniquely named disposable fixture databases. No shared/application database was changed. No migration changed in this audit.

Frontend fresh checks:

```powershell
node node_modules/vitest/vitest.mjs run --config libs/admin/clients/model/vitest.config.mts
node node_modules/vitest/vitest.mjs run --config libs/admin/clients/feature/vitest.config.mts quote-price-comparison.component.test.tsx
```

Results: **20 model and 2 comparison DOM tests passed**. Review confirmed one responsibility per primary type/file, public model/UI exports, no transport/React dependency in model and no HTTP in presentation. Comparison preserves null prices, saved inputs and reference ordering; marks unknown provenance, source change and selection mismatch. Its connected confirmation flow is still FE-005 work and is not claimed complete by this audit.

Formatting, local documentation paths and diff whitespace are checked before the authorized commit. Existing package/compiler warnings are unchanged. No broad suite, browser/manual/visual acceptance, deployment or push was performed.
