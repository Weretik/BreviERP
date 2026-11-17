# Staff activity log «Журнал дій»

«Адміністрування → Журнал дій»: who created, changed or deleted what and when, for every staff member; the page shows managers by default. Entries come only from domain events of the source modules through their outbox; nobody edits them. [SDD](../../specs/activity-log/001-staff-activity-log/README.md), [ADR-0016](../../architecture/decisions/0016-domain-events-to-activity-log.md), [contract](../../contracts/openapi.json), [tables](../../architecture/data-model/activity-log.md), [flow diagram](../../architecture/c4/README.md#dynamic-an-action-reaches-журнал-дій). Access: page `activity-log` (view), Admin by default.

## How an entry is made

1. A command handler calls an aggregate method; the aggregate raises a domain event (`Raise(new QuoteChanged(...))`, base class `DomainEventSource` or `BaseEntity`).
2. On `SaveChanges` the shared `DomainEventOutboxInterceptor` takes the events and opens a transaction if the caller has none; after the insert (ids known) it asks every `IDomainEventOutbox` to translate them: ActivityLog's `ActivityDomainEventOutbox` asks the module's `IActivityDescriber` what happened (subject, action, changed fields) and returns an `activity.recorded` outbox message with `ActivityRecorded` and the actor from `ICurrentActor`. A second save writes the messages and the transaction commits; a failure rolls both back. Without a signed-in person nothing is written.
3. `OutboxAfterCommandBehavior` tries the message right after the command; the outbox task of the scheduler tick retries failures.
4. `RecordActivityOutboxHandler` stores the entry under the event id (a repeat does nothing) with the staff member's full name from `Identity.Contracts`.
5. Entries are kept one year: the «Фонові задачі» task `activity-log-cleanup` (every day 03:00) deletes older ones.

## Code structure

```text
src/BuildingBlocks/
  BuildingBlocks.Domain/…/Events/                  # IDomainEvent, IHasDomainEvents, DomainEventSource (Raise)
  BuildingBlocks.Application/…/Actors/             # ICurrentActor, Actor
  BuildingBlocks.Application/…/DomainEvents/       # IDomainEventOutbox: a module's events → outbox messages
  BuildingBlocks.Application/…/Outbox/ISavedOutboxMessages.cs
  BuildingBlocks.Application/…/Behaviors/OutboxAfterCommandBehavior.cs
  BuildingBlocks.Infrastructure/…/DomainEvents/    # DomainEventOutboxInterceptor, SavedOutboxMessages, NoCurrentActor, AddDomainEvents()
  BuildingBlocks.Api/Authorization/HttpCurrentActor.cs   # the signed-in user from the cookie claims
src/Modules/ActivityLog/
  ActivityLog.Contracts/…/Activity/                # IActivityDescriber + ActivityDescription (what a module implements), ActivityChanges (diff builder), ActivityRecorded, ActivityActions, ActivityMessage.Type
  ActivityLog.Domain/…/Entries/                    # ActivityEntry (limits, roles as |A|B|), ActivityChange, ActivityText
  ActivityLog.Domain/…/Filters/                    # ActivityActor, ActivitySubjectType: the filter choices, latest name/title
  ActivityLog.Application/…/
    Abstractions/                                  # IActivityLogDbContext, IActorNames
    Features/Entries/EventHandlers/                # ActivityDomainEventOutbox (description + actor → message), RecordActivityOutboxHandler (idempotent by event id; keeps the filter tables in the same save)
    Features/Entries/Queries/GetActivityEntries/   # page, filters, Kyiv period, word search; validator
    Features/Entries/Queries/GetActivityFilters/   # people and object types from the small filter tables (database rules 2.2)
    Features/Entries/Commands/DeleteOldActivityEntries/  # one-year retention: one SQL DELETE (R-12), then filter rows without entries
  ActivityLog.Infrastructure/…/
    Persistence/                                   # ActivityLogDbContext (schema activity_log), configuration, migrations
    Contracts/Actors/ActorNames.cs                 # IActorNames → every IActivityActorNames (ActivityLog.Contracts port, ADR-0019)
  Identity.Infrastructure/…/Adapters/ActivityLog/StaffActivityActorNames.cs  # Identity implements it: the full name
  ActivityLog.Api/…/Endpoints/Entries/             # GET /api/activity-log/entries, /filters
src/Bootstrapper/Host.Seed/Jobs/Tasks/Maintenance/ActivityLogCleanupTaskRunner.cs  # task activity-log-cleanup
src/Modules/<Source>/…                             # events in Domain (<Aggregate>/Events/), IActivityDescriber in Application Features/<Feature>/EventHandlers/
```

## Covered actions

Filled in as each source module is wired (ACT-001 «Coverage»). Actions of the scheduler have no person and are not listed. Imports and syncs started from the admin (KeepinCRM clients and suppliers, the leads sheet, the old «Цех» orders, stages and stock, norm and staff lists, the «Довідники» sheets, the catalogue sheet, old-site photos and PPE supplier prices) call `IDomainEventSuppression.Suppress()` before saving, so their rows are not listed one by one (R-08). Not listed yet: the timesheet, the forecast queue, payroll roles, embroidery stitches and «видано».

| Module | Subject (key) | Actions | Describer |
| --- | --- | --- | --- |
| Crm | КП (`quote`) | created, copied, saved with changed fields (name, client, recipient, comments, items, pages, letter), sent, repriced, embroidery added, responsible changed, returned from the archive, deleted | `Crm.Application/Features/Activity/EventHandlers/QuoteActivity.cs` |
| Crm | Шаблон КП для знайомства (`quote-intro-template`), Налаштування КП (`quote-settings`) | template created or deleted; settings saved | same |
| Crm | Завдання менеджеру (`task`) | created, edited with changed fields, status, urgent flag, step ticked or unticked, deleted | `TaskActivity.cs` (same folder) |
| Crm | Лід (`lead`), Результат дзвінка (`lead-call-result`), Категорія ліда (`lead-category`) | taken, given back, call recorded; call result added or changed; category description | `LeadActivity.cs` |
| Crm | Клієнт (`customer`) | created, details changed, contact added or changed, merged | `CustomerActivity.cs` |
| Crm | Розрахунок вишивки (`embroidery-calculation`), Налаштування вишивки (`embroidery-settings`) | calculation saved, mockup set, deleted; rates saved | `EmbroideryActivity.cs` |
| Production | Виробниче завдання (`production-order`) | created (from КП or a copy), edited (the order's own «Історія змін» lines become the fields), stages, shipment and its cancel, files and fabric plan notes, closed, cancelled with the reason, reopened, deleted | `Production.Application/Features/Activity/EventHandlers/ProductionOrderActivityDescriber.cs` |
| Production | Робота цеху (`production-work`) | recorded, voided | `ProductionWorkActivity.cs` |
| Production | Працівник цеху (`workshop-employee`), Налаштування цеху (`workshop-settings`) | added, renamed or moved, switched on or off; Telegram settings saved | `WorkshopActivity.cs` |
| Production | Норма тканини (`fabric-norm`) | added, meters changed, deleted | `FabricNormActivity.cs` |
| Production | Виплата / Коригування зарплати (`payroll-payment`, `payroll-adjustment`) | recorded, voided | `PayrollActivity.cs` |
| Warehouse | Рух тканини (`fabric-movement`) | opening, receipt, issue (to an order item or elsewhere), adjustment, «Сторно» with the line and metres; edit with changed fields, void and restore with the reason | `Warehouse.Application/Features/Activity/EventHandlers/FabricMovementActivity.cs`, `FabricRevisionActivity.cs` |
| Reference | Тканина, Фурнітура, Колір, Деталь виробу, Операція, Правило націнки ЗІЗ, Параметр ціни, Розділ товарів, Постачальник | created, changed (name and price as fields), deleted | `Reference.Application/Features/Activity/EventHandlers/ReferenceActivity.cs` (keys in `ReferenceSubject.cs`) |
| Catalog | Товар (`product`), Категорія товарів (`product-category`) | product created, deleted, or changed: one entry per command with the name and «Активний» as fields and the parts touched («опис», «фото», «склад», «артикули», «категорії», «огляд», …) in the summary; category created, changed, moved, deleted | `Catalog.Application/Features/Activity/EventHandlers/ProductActivity.cs`, `ProductCategoryActivity.cs` |
| Media | Фото (`media-file`), Папка фото (`media-folder`) | photo uploaded, renamed or alt text changed, moved, deleted; folder created, renamed, moved, deleted | `Media.Application/Features/Activity/EventHandlers/MediaFileActivity.cs`, `MediaFolderActivity.cs` |

## Tests

`dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~UnitTests.ActivityLog|FullyQualifiedName~UnitTests.Platform.DomainEvents|FullyQualifiedName~Activity"` (the describers are in `tests/UnitTests/<Module>/Activity/` and `WarehouseActivityTests`, `ReferenceActivityTests`, `ProductActivityTests`, `MediaActivityTests`); `tests/ArchitectureTests` `ActivityLogDependencyTests`; `tests/HostApiTests` endpoint snapshot.
