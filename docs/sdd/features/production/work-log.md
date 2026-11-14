# Work log and sewing forecast («Журнал робіт», «Прогноз пошиття», stage 5)

«Журнал робіт»: the head of the workshop records that a worker sewed or cut pieces of an item on a day; the item's sewing rate is kept with the record. Within the plan the item's quantity is the limit per kind; more goes as «понад план» with a reason. A record is never edited: a wrong one is cancelled by «Сторно» with a reason. Admin and the head of the workshop see the log and the «Робота» section of an item; managers do not. «Прогноз пошиття»: the «Нове» orders in the saved queue with the approximate end of sewing, from the work left and the capacity of «Параметри ціноутворення». Everyone with orders sees dates; the workshop reorders the queue. The old «Цех» log moves in through an import. SDD: [stage 5](../../specs/production/005-work-log/README.md); [contract](../../contracts/production/work-log.openapi.yaml), [tables](../../architecture/data-model/production.md).

## Code structure

```text
src/Modules/Production/
  Production.Domain/Production.Domain/
    Work/ProductionWork.cs       # record, kinds, over-plan reasons, «Сторно»
    Forecast/ForecastQueue.cs    # saved order of the queue, version
    Forecast/SewingForecast.cs   # work left → working days
  Production.Application/Production.Application/
    Features/Work/
      Queries/                   # GetWorkLog (month, worker, order), GetWorkChoices (workers, items, reasons), GetProductionItemWork
      Commands/                  # CreateProductionWork (plan limit), VoidProductionWork («Сторно»), ImportProductionWork (old «Цех»)
      Shared/WorkCards.cs        # done counts per item, «Робота» of an item, row mapping
      Shared/WorkMessages.cs     # also WorkshopDay (today in Kyiv)
    Features/Forecast/
      Shared/ForecastQueueReader.cs  # «Нове» orders in queue order
      Queries/GetForecast, Commands/SaveForecastQueue
  Production.Infrastructure/.../Persistence/Work/ProductionWorkConfiguration.cs
  Production.Api/Production.Api/Endpoints/Work/      # /api/production/work…, items/{itemId}/work
  Production.Api/Production.Api/Endpoints/Forecast/  # /api/production/forecast
```

## Structure diagram

```mermaid
flowchart LR
  Admin[Brevi Admin «Журнал робіт», «Робота», «Прогноз пошиття»] --> Endpoints[Work and Forecast endpoints]
  Endpoints --> UseCases[Work and Forecast use cases]
  UseCases --> Db[(production.Work / ForecastQueues)]
  UseCases --> Orders[(production.Orders / Items / Employees)]
  UseCases --> Params[IPayrollParameterReader] -.Reference.Contracts.-> Reference[«Параметри ціноутворення»]
```

## Tests

- `tests/UnitTests/Production/ProductionWorkHandlerTests.cs` — plan limit and over-plan, «Сторно», access, import, forecast calculation, forecast access and queue.
- `tests/UnitTests/Production/ProductionWorkContractTests.cs` — DTOs and operations against the contract.

Focused run: `dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~UnitTests.Production`.

**Data:** [production schema](../../architecture/data-model/production.md)
