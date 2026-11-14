# Workshop reports and overview («Звіти цеху», «Огляд виробництва», stage 7)

«Звіти цеху» (Admin; the director holds Admin): for whole months, at most 12, output at the КП price, what the workers earned with pay per 8 hours, fabric issued and the stock at the end at the price of «Тканини», optionally against a second period. Pieces and metres without a price are given apart. A month closed in «Зарплата» keeps its report. «Огляд виробництва» on the admin home: the «Нове» orders by the stage of their slowest item, late ones, what is ready to ship and, for the head of the workshop and Admin, the hours of this week. SDD: [stage 7](../../specs/production/007-workshop-reports/README.md); [contract](../../contracts/production/workshop-reports.openapi.yaml), [tables](../../architecture/data-model/production.md).

## Code structure

```text
src/Modules/Production/
  Production.Application/Production.Application/
    Contracts/ReportContracts.cs
    Reports/WorkshopReports.cs              # a month from the data, a kept month, the sum of months
    Reports/GetReport/GetWorkshopReportQuery.cs
    Dashboard/GetWorkshopDashboardQuery.cs
    Payroll/Close/PayrollCloseCommands.cs   # closing also keeps the report (PayrollMonth.ReportSnapshot)
  Production.Api/Production.Api/Reports/WorkshopReportsController.cs    # /api/production/reports
  Production.Api/Production.Api/Dashboard/WorkshopDashboardController.cs # /api/production/dashboard
```

## Structure diagram

```mermaid
flowchart LR
  Admin[Brevi Admin «Звіти цеху», home] --> Controllers[WorkshopReportsController, WorkshopDashboardController]
  Controllers --> UseCases[Reports and Dashboard use cases]
  UseCases --> Sheets[PayrollSheets]
  UseCases --> Db[(production.Work / Orders / FabricMovements / FabricStocks / PayrollMonths)]
  UseCases --> Fabrics[IProductionFabricReader] -.Reference.Contracts.-> Reference[«Тканини»]
```

## Tests

- `tests/UnitTests/Production/WorkshopReportHandlerTests.cs` — values, access, future and long periods, kept month, sum of months, overview.
- `tests/UnitTests/Production/WorkshopReportsContractTests.cs` — DTOs, operations and policies against the contract.

Focused run: `dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~UnitTests.Production`.

**Data:** [production schema](../../architecture/data-model/production.md)
