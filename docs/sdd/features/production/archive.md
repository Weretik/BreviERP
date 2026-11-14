# Workshop archive («Архів цеху», stage 8)

Read-only lines of the old «Цех»: the Google sheet «ГРАФИК работы» (2017–2026) and the old requests, one book at a time, newest first, found by every word of «Пошук» and limited by date. Everyone with production orders reads it; Admin moves `workshop-archive.json` with a preview, and a repeat adds only new lines. SDD: [stage 8](../../specs/production/008-workshop-archive/README.md); [contract](../../contracts/production/workshop-archive.openapi.yaml), [tables](../../architecture/data-model/production.md).

## Code structure

```text
src/Modules/Production/
  Production.Domain/Production.Domain/Archive/WorkshopArchiveRecord.cs
  Production.Application/Production.Application/
    Features/Archive/Queries/GetWorkshopArchive/
    Features/Archive/Commands/ImportWorkshopArchive/
    Features/Archive/Shared/ArchiveMessages.cs
  Production.Infrastructure/.../Configurations/WorkshopArchiveConfiguration.cs
  Production.Api/Production.Api/Archive/WorkshopArchiveController.cs   # /api/production/archive
```

## Tests

- `tests/UnitTests/Production/WorkshopArchiveHandlerTests.cs` — access, preview, repeat, search, dates, validation.
- `tests/UnitTests/Production/WorkshopArchiveContractTests.cs` — DTOs, operations and policy against the contract.

Focused run: `dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~UnitTests.Production`.

**Data:** [production schema](../../architecture/data-model/production.md)
