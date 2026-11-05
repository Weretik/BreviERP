# PRD-008 «Швейний цех»: archive and cutover

Status: stage 8 implemented 2026-11-05. Follows [PRD-007](../007-workshop-reports/README.md); this is the last stage of the «Цех» port.

## Goal

What the workshop did before Brevi stays findable after the old «Цех» is switched off: the Google sheet «ГРАФИК работы» (2017–2026) and the requests the old app kept before it had orders. Nobody edits them. The cutover itself (a fresh export, the last transfer, switching the old app off) is the owner's step and is described below.

## Decisions

| ID | Decision |
| --- | --- |
| PRD-D54 | One table `production.ArchiveRecords` holds both old books, told apart by `Kind` (Schedule, Request). Common columns (date, number, client, product, quantity as text, fabric, status, notes) are searched and shown in the table; the rest of a line goes to `Fields` (label and value) and its Google links to `Links`. Lines are never changed: an import adds lines whose `SourceKey` is new and skips the rest, so a fresh export can be moved again. |
| PRD-D55 | Everyone with production orders reads the archive (`CanUseProductionOrders`); only Admin imports. The old «Розцінка за виготовлення» of a request is not moved because managers read the archive. |
| PRD-D56 | A sheet row without a date continues the order above it, so it takes the date of the row above (the transfer script does it); a date that cannot be read stays as `DateText`. 13 old requests had truncated JSON in the export: their visible columns are kept, the cut part is lost. |
| PRD-D57 | Closed Brevi orders are not copied into the archive: the admin page shows them through the orders list with the «Закрите» filter. |
| PRD-D58 | Cutover is the owner's: (1) a fresh export of the old «Цех», (2) the same transfer files rebuilt and moved again (every import skips what is already there), (3) a check of the counts, (4) the old app is switched off. Data entered in the old «Цех» after 23.10.2026 is not in Brevi until step 1. |

## Acceptance

| ID | Check |
| --- | --- |
| PRD-008-A1 | A Manager cannot import; a line with a non-https link fails the whole file; a preview saves nothing; a repeat adds nothing. |
| PRD-008-A2 | The archive is one book at a time, newest first; every word of «Пошук» must match, in any case; dates limit it. |
| PRD-008-A3 | The real transfer file (3,107 lines) moves with no errors. |

## File-role map

| Role | Files |
| --- | --- |
| Domain | `Production.Domain/Archive/WorkshopArchiveRecord.cs` |
| Use cases | `Archive/GetArchive/GetWorkshopArchiveQuery.cs`, `Archive/ImportArchive/ImportWorkshopArchiveCommand.cs`, `Archive/ArchiveMessages.cs` |
| Contracts | `Contracts/ArchiveContracts.cs` |
| Persistence | `Configurations/WorkshopArchiveConfiguration.cs`; migration `WorkshopArchive` |
| API | `Production.Api/Archive/WorkshopArchiveController.cs` |
| Contract | `docs/sdd/contracts/production/workshop-archive.openapi.yaml` |

## Evidence

- Unit: `dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~UnitTests.Production` — 212 passed (2026-11-05), including `WorkshopArchiveHandlerTests` and `WorkshopArchiveContractTests`.
- Transfer: `workshop-archive.json` (2,763 sheet rows and 344 requests) imported on SQLite with 0 errors.
- Architecture: `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj` — 19 passed.
