# PRD-006 «Швейний цех»: timesheet and payroll

Status: stage 6 implemented 2026-11-05. Follows [PRD-005](../005-work-log/README.md); reports and the dashboard come with stage 7.

## Goal

The head of the workshop marks the hours each worker worked, and the director sees, for every month, what each worker earned from the work log, was paid and is still due, then closes the month so it no longer changes. The calculation follows the old «Цех»; every share comes from «Параметри ціноутворення».

## Decisions

| ID | Decision |
| --- | --- |
| PRD-D41 | «Робочий час»: one row per worker and day with minutes 0–1440 in steps of 15 (0 = absent; no row = not marked). A day may not be later than today in Kyiv. Each change keeps who changed it, from what to what; a save with an older version of a row is 409. The day shows active workers and anyone marked that day; the month shows totals per day and per worker. Hours do not change pay; they are shown beside it. WorkshopChief and Admin. |
| PRD-D42 | Pay of a month: work not cancelled with a work date in the month, over-plan work included (it is paid). Sewing = pieces × rate; cutting = pieces × rate × CUTTING_RATE; seamstress bonus = sewing × SEAMSTRESS_BONUS when her sewing ≥ SEAMSTRESS_BONUS_PAYOUT_THRESHOLD; master = all sewing × MASTER_RATE; chief = all sewing × SHOP_MANAGER_RATE; embroidery = stitches / 1000 × EMBROIDERY_RATE_1000_STITCHES; other = Σ adjustments. Each part is rounded to kopecks. Accrued = the sum; due = accrued − active payments. Shares are fractions as kept in «Параметри ціноутворення» (0.08 = 8 %). While a month is open the work is paid by the item's current sewing rate (fixing a wrong rate fixes the pay; the rate of PRD-D34 is used when the item has none). This refines PRD-D34: the snapshot rate is the fallback, the closed month is the freeze. |
| PRD-D43 | Closing a month (Admin) keeps the whole calculation, with the work behind it, as JSON and is refused while there are warnings or before the month started. A closed month is shown as kept and refuses (409 «Місяць зарплати закрито») new work and «Сторно» with a work date in it, timesheet saves, payments, adjustments, «Видано» and embroidery. Admin can reopen it; it is then counted afresh. |
| PRD-D44 | «Керівні частки»: one master and one chief, kept until changed and used by every open month. Not chosen → the only active worker whose position contains «майстер» / «начальник» is used and shown as found by position. Embroidery of a month: stitches and the worker; not chosen → the chief. |
| PRD-D45 | Payments (advance, card, cash, other; 0.01–10 000 000 UAH; date not later than today) and adjustments (non-zero, ± 10 000 000, reason 3–500) are never edited: a wrong one is cancelled with a reason and stays crossed out. «Видано» is a note of the sum handed out (≥ 0 or empty). |
| PRD-D46 | Access: WorkshopChief and Admin (Director as Admin) see «Зарплата» and set the roles and embroidery; payments, adjustments, «Видано», closing and reopening are Admin only. Managers see neither page. Warnings: no master or chief while there is sewing and the share is above 0, stitches without a worker, an empty share in «Параметри ціноутворення». |
| PRD-D47 | Nothing is transferred: the 23.10.2026 export of the old «Цех» has no timesheet rows, payments, adjustments, «Видано» or closed months; October 2026 is open there. Its single master role is covered by the position rule of PRD-D44. |

## Acceptance

| ID | Check |
| --- | --- |
| PRD-006-A1 | The calculation pays sewing, cutting, the bonus at the threshold, master and chief shares of all sewing, embroidery and adjustments, rounded to kopecks. |
| PRD-006-A2 | Missing people or shares are warnings; closing with warnings is 409. |
| PRD-006-A3 | The chief cannot pay; a manager cannot open the month; a future month refuses payments. |
| PRD-006-A4 | A closed month refuses work, «Сторно», hours and payments; the detail of a closed month comes from the kept calculation; reopening opens it. |
| PRD-006-A5 | Timesheet: the history keeps every change, a stale version is 409, a future day is 400, minutes off the 15-minute step are 400. |

## File-role map

| Role | Files |
| --- | --- |
| Domain | `Timesheet/WorkshopTimeEntry.cs` (entry and change), `Payroll/PayrollMonth.cs` (embroidery, close, reopen), `Payroll/PayrollRole.cs`, `Payroll/PayrollMoney.cs` (payment, adjustment, «Видано»), `Payroll/PayrollCalculation.cs` |
| Use cases | `Timesheet/TimesheetQueries.cs` (day, save, `TimesheetDays`); `Payroll/` — `PayrollSheets.cs` (build or read the kept month), `PayrollChanges.cs` (common path of a change), `PayrollMonths.cs` (month text, closed lock), `GetMonth/PayrollQueries.cs` (month, worker, year), `Setup/` (roles, embroidery), `Payments/` (payments, adjustments, «Видано»), `Close/` (close, reopen); closed lock added to `Work/RecordWork` |
| Persistence | `Configurations/PayrollConfiguration.cs`; migration `TimesheetPayroll` |
| API | `Production.Api/Timesheet/TimesheetController.cs`, `Payroll/PayrollController.cs` (policy CanManageWorkshop) |
| Contract | `docs/sdd/contracts/production/payroll.openapi.yaml` |

## Evidence

- Unit: `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~Production|FullyQualifiedName~Workshop|FullyQualifiedName~FabricNorm"` — 148 passed (2026-11-05), including `ProductionPayrollHandlerTests` (4) and `ProductionPayrollContractTests`.
- Architecture: `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj` — 13 passed.
