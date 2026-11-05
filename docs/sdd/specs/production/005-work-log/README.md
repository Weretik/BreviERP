# PRD-005 «Швейний цех»: work log and sewing forecast

Status: stage 5 implemented 2026-11-05. Follows [PRD-004](../004-fabric-stock/README.md); pay and closed months come with stage 6.

## Goal

The head of the workshop records who sewed or cut how many pieces of which item on which day, so the plan of each item shows what is left and stage 6 can pay by it. The forecast shows, for the queue of open orders, the approximate day sewing ends, so managers can tell clients a realistic date. The 177 records of the old «Цех» come with their history.

## Decisions

| ID | Decision |
| --- | --- |
| PRD-D34 | A work record is one worker of «Працівники цеху», one item, a kind (Пошив or Крій), a quantity (1–100 000), a work date (not later than today in Kyiv), an optional comment, who recorded it and when. It keeps the item's sewing rate as it was (snapshot), so a later rate change does not change past pay. An item without a sewing rate takes no work. |
| PRD-D35 | Within the plan the item's quantity is the limit per kind (records not cancelled and not over plan); more is refused with 409 and the number left. Work beyond it is recorded only as «понад план» with a reason: Запас, Заміна браку, Зміна кількості, Зразок, Повторне пошиття, Інше. Over-plan work does not count to the plan. |
| PRD-D36 | A record is never edited or deleted. A wrong one is cancelled by «Сторно» with a reason (who and when are kept); it stays in the log crossed out and stops counting. Locking a closed payroll month is stage 6. |
| PRD-D37 | Access: «Журнал робіт» and the «Робота» section of an item are for those who see the sewing rate (Admin, Director as Admin, WorkshopChief); managers see neither. Records and «Сторно» only on a «Нове» order. |
| PRD-D38 | Forecast: the queue is the «Нове» orders not deleted, first in the saved order, then by order date and number. Items whose current stage is «Готово» or later are left out. Left = quantity − plan pieces sewn; work = Σ left × rate (UAH); daily capacity = SEAMSTRESS_COUNT × MINUTES_SHIFT × MINUTE_COST_SEAMSTRESS of «Параметри ціноутворення» (no own copy). Finish = the ⌈cumulative work / capacity⌉-th working day (Monday–Friday) from the next working day in Kyiv. An order with pieces but no rate has no date, and every order after it is «невідомо» (unknown predecessor); beyond ten years is the same. Late = finish after the earliest «Готово до» of the items. The date is approximate sewing end, not a delivery promise. |
| PRD-D39 | Everyone with production orders sees the forecast rows and dates; sewing minutes and seamstresses / shift minutes only those who see the rate; the minute cost only Admin. The order of the queue is set by WorkshopChief and Admin with a version (409 when someone saved first); listed orders must be in the queue. Without the three parameters the page says what to fill in. |
| PRD-D40 | Old records move through `POST /api/production/work/import?dryRun` (Admin): the item by the order's old id and position, the worker by full name; rate, date, over-plan reason and «Сторно» are kept, the author is «Перенесення зі старого «Цеху»». Nothing is saved unless every row is fine; a repeat skips records already moved (old ids are kept). The plan limit is not checked for old records. |

## Acceptance

| ID | Check |
| --- | --- |
| PRD-005-A1 | Work over the plan without «понад план» is 409 with the number left; over-plan work does not count; a future date is 400; a manager cannot record. |
| PRD-005-A2 | «Сторно» keeps the record, frees the plan; a second «Сторно» is 409. Managers cannot open the log. |
| PRD-005-A3 | Import: an unknown worker is an error and nothing is saved; a preview saves nothing; a repeat is unchanged; rate and «Сторно» are kept. |
| PRD-005-A4 | Forecast counts working days, skips weekends; an order without a rate blocks those after it. |
| PRD-005-A5 | Managers see dates without minutes or parameters; Admin alone sees the minute cost; a stale queue version is 409; a saved order is followed. |

## File-role map

| Role | Files |
| --- | --- |
| Domain | `Work/ProductionWork.cs` (record, kinds, over-plan reasons, «Сторно»), `Forecast/ForecastQueue.cs` (saved order), `Forecast/SewingForecast.cs` (calculation) |
| Use cases | `Work/` — `GetWorkLog`, `GetChoices`, `GetItemWork`, `RecordWork` (create, «Сторно»), `ImportWork`; `Work/WorkCards.cs` (item section, done counts). `Forecast/` — `ForecastQueueReader.cs`, `GetForecast`, `SaveQueue` |
| Ports | `IPayrollParameterReader` (existing) gains code `MINUTES_SHIFT` |
| Persistence | `Configurations/ProductionWorkConfiguration.cs`; migration `WorkLog` |
| API | `Production.Api/Work/ProductionWorkController.cs`, `Forecast/ProductionForecastController.cs` |
| Contract | `docs/sdd/contracts/production/work-log.openapi.yaml` |

## Evidence

- Unit: `dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~UnitTests.Production` — 144 passed (2026-11-05), including `ProductionWorkHandlerTests` (5) and `ProductionWorkContractTests`.
- Architecture: `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj` — 13 passed.
- Transfer: the file of 177 records (133 sewing, 44 cutting, 4 cancelled, 13 workers) matches every item of the 28 moved orders by old id and position; no item is over its plan.
