# Timesheet and payroll («Робочий час», «Зарплата», stage 6)

«Робочий час»: the head of the workshop marks the hours of each worker per day in steps of 15 minutes; every change is kept. «Зарплата»: for a month, what each worker earned from «Журнал робіт» (sewing, cutting, bonus, master and chief shares, embroidery, adjustments), was paid and is still due; all shares come from «Параметри ціноутворення». While a month is open it follows the item's current sewing rate; Admin closes it, which keeps the calculation (and the month's [report](reports.md)) and locks work, hours and pay in it; an unchosen master or chief does not block closing. The head of the workshop sees it and sets roles and embroidery; payments, adjustments, «Видано» and closing are for Admin. SDD: [stage 6](../../specs/production/006-timesheet-payroll/README.md); [contract](../../contracts/production/payroll.openapi.yaml), [tables](../../architecture/data-model/production.md).

## Code structure

```text
src/Modules/Production/
  Production.Domain/Production.Domain/
    Timesheet/WorkshopTimeEntry.cs   # hours of a worker on a day, change history
    Payroll/PayrollMonth.cs          # embroidery, close (kept JSON), reopen
    Payroll/PayrollRole.cs           # master and chief
    Payroll/PayrollMoney.cs          # payment, adjustment, «Видано»
    Payroll/PayrollCalculation.cs    # the pay of a month and its warnings
  Production.Application/Production.Application/
    Contracts/TimesheetContracts.cs, PayrollContracts.cs
    Timesheet/TimesheetQueries.cs    # day, save, TimesheetDays
    Payroll/
      PayrollSheets.cs               # builds a month or reads the kept one
      PayrollChanges.cs              # access, open month, change, fresh month
      PayrollMonths.cs               # «РРРР-ММ», closed-month lock (also used by Work/RecordWork)
      GetMonth (month, worker, year), Setup (roles, embroidery), Payments (payments, adjustments, «Видано»),
      Close (close, reopen)
  Production.Infrastructure/.../Persistence/Configurations/PayrollConfiguration.cs
  Production.Api/Production.Api/Timesheet/TimesheetController.cs  # /api/production/timesheet
  Production.Api/Production.Api/Payroll/PayrollController.cs      # /api/production/payroll…
```

## Structure diagram

```mermaid
flowchart LR
  Admin[Brevi Admin «Робочий час», «Зарплата»] --> Controllers[TimesheetController, PayrollController]
  Controllers --> UseCases[Timesheet and Payroll use cases]
  UseCases --> Db[(production.TimeEntries / Payroll*)]
  UseCases --> Work[(production.Work / Items / Employees)]
  UseCases --> Params[IPayrollParameterReader] -.Reference.Contracts.-> Reference[«Параметри ціноутворення»]
```

## Tests

- `tests/UnitTests/Production/ProductionPayrollHandlerTests.cs` — calculation, warnings, access, payments, closing lock, reopen, timesheet history and versions.
- `tests/UnitTests/Production/ProductionPayrollContractTests.cs` — DTOs and operations against the contract.

Focused run: `dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~UnitTests.Production`.

**Data:** [production schema](../../architecture/data-model/production.md)
