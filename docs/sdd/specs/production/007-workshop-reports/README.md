# PRD-007 «Швейний цех»: reports and the admin overview

Status: stage 7 implemented 2026-11-05. Follows [PRD-006](../006-timesheet-payroll/README.md); the archive and the cutover come with [stage 8](../008-workshop-archive/README.md).

## Goal

The owner sees what the workshop produced and spent over a period: output at the КП price, what the workers earned, the fabric issued and what is left in stock, and can compare two periods. Everyone with production orders sees on the admin home which orders are in the workshop, at which stage, which are late and what is ready to ship, instead of the demo board.

## Decisions

| ID | Decision |
| --- | --- |
| PRD-D48 | «Звіти цеху» are for Admin (the director holds Admin), as «тільки власниця» in the old «Цех». A period is whole months «РРРР-ММ» from..to, at most 12, not in the future; a second period may be given to compare. Several months are the sum of their months; the stock is the balance at the end of the last one. |
| PRD-D49 | A month is counted from the current data: output = pieces of «Пошив» within the plan, not cancelled, by work date × the item's КП report price (snapshot of the order); payroll = the month of «Зарплата» (hours, pieces, accrued; pay per 8 hours = accrued × 480 / minutes, none without hours); fabric issued = «Видача» movements not reversed by operation date × the current price of «Тканини»; stock = current balance minus every movement dated after the month, valued at the current price (negative balances count as 0 UAH). Pieces or metres without a price are given apart, never as 0 UAH. Closing the month in «Зарплата» also keeps its report as JSON, so a closed month no longer changes with later prices or corrections; reopening drops it. There is no separate «Зафіксувати» and no automatic close on the 5th as in the old «Цех». |
| PRD-D50 | The overview `GET /api/production/dashboard` (everyone with production orders, no prices or rates): «Нове» orders not deleted; an order sits in the column of its slowest item not yet shipped, from «Нове» to «Готово до відвантаження»; progress = pieces sewn within the plan / quantity; late = the earliest «Готово до» of the items not fully shipped is before today in Kyiv; ready = items at «Готово до відвантаження» with pieces not shipped. The hours of this week by active worker only for WorkshopChief and Admin. |
| PRD-D51 | Closing a month of «Зарплата» is no longer refused when the only warnings are an unchosen master or chief: that share is simply not paid (the owner pays no chief, 2026-11-05). Every other warning still blocks; `permissions.canClose` of the month says the same, so the page does not decide it. |
| PRD-D52 | Nothing is transferred: the old export has no frozen report months (`report_monthly_snapshots` is empty) and its saved report prices are all «unavailable». |
| PRD-D53 | The КП prices of the 28 moved orders come from the imported КП instead (2026-11-05, after the first report showed 272 pieces without a price): `POST /api/production/orders/report-price-import` (Admin, `dryRun` preview, any error saves nothing) takes `production-report-prices.json` and fills only items without a price, without a history line because the price is hidden from managers. The file matches the order's КП number, model and fabric; the column is the one the КП showed, «Опт» when it showed several, otherwise the cheapest shown. |

## Acceptance

| ID | Check |
| --- | --- |
| PRD-007-A1 | Output counts plan pieces only, at the КП price; fabric issued and stock are valued at the fabric price; a month without hours has no pay per 8 hours. |
| PRD-007-A2 | WorkshopChief and managers get 403; a future month is 400; more than 12 months is 400. |
| PRD-007-A3 | A closed month keeps its report when the fabric price changes. Closing without a chief succeeds. |
| PRD-007-A4 | Months add up; the stock is the last month's. |
| PRD-007-A5 | The overview puts an order at its slowest stage, marks it late, and gives the hours only to the head of the workshop and Admin. |
| PRD-007-A6 | «Ціни КП» is Admin only, a preview changes nothing, a missing position is an error, and an item that has a price keeps it. |

## File-role map

| Role | Files |
| --- | --- |
| Use cases | `Reports/WorkshopReports.cs` (a month, kept month, sum), `Reports/GetReport/GetWorkshopReportQuery.cs`, `Reports/ReportMessages.cs`, `Dashboard/GetWorkshopDashboardQuery.cs`; `Payroll/Close/PayrollCloseCommands.cs` keeps the report |
| Domain | `Payroll/PayrollMonth.cs` gains `ReportSnapshot` |
| Contracts | `Contracts/ReportContracts.cs`; `Contracts/ProductionTrackingInputs.cs` and `ProductionOrderResponses.cs` gain the «Ціни КП» import |
| Import | `Orders/ImportReportPrices/ImportProductionReportPricesCommand.cs`; `ProductionItem.FillReportPrice` |
| Persistence | `Configurations/PayrollConfiguration.cs`; migration `WorkshopReports` |
| API | `Production.Api/Reports/WorkshopReportsController.cs`, `Dashboard/WorkshopDashboardController.cs` |
| Contract | `docs/sdd/contracts/production/workshop-reports.openapi.yaml` |

## Evidence

- Unit: `dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~UnitTests.Production` — 197 passed (2026-11-05), including `WorkshopReportHandlerTests` (3) and `WorkshopReportsContractTests`.
- Architecture: `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj` — 15 passed.
