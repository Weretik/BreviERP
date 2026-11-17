# ACT-001 — «Журнал дій»: who created, changed or deleted what, and when

**Status:** implemented (2026-11-16): backend #184, #185, #186; brevi-web page; final audit in [audit.md](audit.md). **Date:** 2026-11-16. **Module:** ActivityLog (new, schema `activity_log`); domain events in BuildingBlocks; the source modules Crm, Production, Warehouse, Catalog, Reference, Media raise them. **Owner decision:** 2026-11-16, thread «Вести журнал действий менеджеров в Brevi»: plan accepted; record every staff member, the page shows managers by default through a filter; build every stage to the end. ADR: [ADR-0016](../../../architecture/decisions/0016-domain-events-to-activity-log.md).

## Why

The owner cannot see which manager created, changed or deleted a КП, a production order, a fabric movement or a catalogue entry. Some aggregates keep «хто змінив» for their last change only; nothing keeps the history across the system.

## Decision

An aggregate raises a **domain event** when a person creates, changes or deletes it (`QuoteCreated`, `ProductionOrderChanged`, …). When the module saves, a shared EF interceptor takes the events; the module's **activity describer** (`IActivityDescriber` of `ActivityLog.Contracts`) says what each means (subject, title, action, changed fields), ActivityLog adds who did it and when, and the `activity.recorded` message is written to the module's own `OutboxMessages` in the same transaction ([ADR-0009](../../../architecture/decisions/0009-transactional-outbox.md)). After the command the request tries the message once; the scheduler tick retries the rest. The handler of the new module ActivityLog stores the entry under the event id, so a repeat does nothing. The admin page «Адміністрування → Журнал дій» lists the entries with filters.

Why a new module: no module owns a history of other modules' actions; putting it in Identity or each source module would mix responsibilities. Why domain events and not an EF change interceptor that logs every changed column: the journal must show business actions in the owner's words («Змінив КП №125: Сума 1 200 → 1 500»), not table rows; columns such as `UpdatedAt`, sort orders or cached totals would drown it. Why through the outbox and not an in-process event after the command (the unused KedrStore pattern): an entry must never be lost for a saved action nor written for an action that rolled back.

Not in scope: reads (who opened what), sign-in history, bulk imports and syncs done by background tasks (Host.Seed, the tick), the public site, undo from the journal, notifications. Imports started from the admin record one entry for the whole import, not one per row (R-08).

## Rules

- **R-01 What is recorded.** Every create, change and delete a signed-in staff member makes through an admin endpoint on the pages of the coverage table below. An action with no signed-in person (background task, migration, seed, public site request) is not recorded.
- **R-02 Entry.** Id (= the domain event id), when (UTC, shown in Kyiv time), who (user id, display name, role keys at that moment), action (`Created`, `Changed`, `Deleted`, or a named action such as `Sent`, `Closed`, `Cancelled`, `Reopened`, `Voided`, `Restored`, `Imported`), subject type key, subject id (text), subject title (number or name, ≤ 200), optional summary (≤ 500, e.g. «Статус: В роботі»), and up to 20 changed fields `{ field, from, to }` (Ukrainian label ≤ 100, values ≤ 200, cut with «…»).
- **R-03 Changed fields.** A `Changed` event lists only fields the person sees and changed, with old and new values in display form (money «1 200,00 грн», dates «16.11.2026», references by name). A change that touches no listed field still records «Змінено» with no fields. Saving the same values records nothing.
- **R-04 Same transaction.** The entry's outbox message is saved in the same transaction as the change: after the change's insert (so database ids are known) a second save writes the messages, then the transaction commits. A failure of either save rolls both back.
- **R-05 Delivery and repeats.** After the command succeeds its activity messages are tried at once; failures are retried by the outbox task (ADR-0009). The ActivityLog handler looks the event id up and stores the entry under it as the primary key; a repeat (same id) is a success that writes nothing, and a racing copy refused by the key counts as done.
- **R-06 Actor.** Taken from the request's signed-in user (`ICurrentActor`): account id, login (e-mail, else user name) and role claims. The ActivityLog handler adds the full name from `Identity.Contracts` when it stores the entry (seconds later), else keeps the login. Stored as a snapshot; renaming or deleting the account later does not change old entries.
- **R-07 Kept.** Entries are never edited or deleted from the admin. They are kept for one year (R-12).
- **R-08 Bulk.** A command that changes many rows at once (import, «Синхронізувати зараз») records nothing row by row: it calls `IDomainEventSuppression.Suppress()` before its save, and the save drops the events. One summary entry per import («Імпортовано 120, оновлено 30») is left for a later version (2026-11-16: imports are rare admin jobs; the owner asked for manager actions).
- **R-09 Page.** «Журнал дій», group «Адміністрування»: a table newest first — «Коли», «Хто», «Дія», «Об'єкт» (type and title; a link to the object when the admin has a page for it and it was not deleted), «Що змінилось». Filters: «Хто» (people seen in the journal), «Роль» (default «Менеджер»; «Усі ролі» shows everyone), period (from/to, Kyiv dates), «Тип об'єкта», «Дія», and a search in the object title. Pages of 50. No «all OK» marks.
- **R-12 One year (owner 2026-11-17).** The «Фонові задачі» task `activity-log-cleanup` («Очищення журналу дій», every day 03:00 Kyiv, on by default) deletes entries whose `OccurredAt` is more than one calendar year ago, with one SQL `DELETE` on the (`OccurredAt`, `Id`) index. A repeat deletes nothing; a daily run removes about one day of rows.
- **R-10 Access.** Page `activity-log` in «Права доступу», group «Адміністрування», view only. Default: Admin; the Director always passes.

## Coverage (subject types)

The describer of each module maps its events. Stage 3 wires them in this order; each row lists the commands that raise events.

| Module | Subject keys (Ukrainian title) | Commands |
| --- | --- | --- |
| Crm | `quote` (КП), `quote-intro-template` (Шаблон КП для знайомства), `quote-settings` (Налаштування КП), `task` (Завдання менеджеру), `lead` (Лід), `lead-call-result`, `lead-category`, `customer` (Клієнт), `embroidery-calculation` (Розрахунок вишивки), `embroidery-settings` | done: see the feature card «Covered actions» (clients have no delete command; syncs and imports are not listed) |
| Production | `production-order`, `production-work`, `workshop-employee`, `workshop-settings`, `fabric-norm`, `payroll-payment`, `payroll-adjustment` | done: see the feature card «Covered actions» |
| Warehouse | `fabric-movement` (Рух тканини) | done: openings, receipts, issues, adjustments, «Сторно», edit, void, restore (stock lines are made by these movements and have no delete; the transfer from the old «Цех» is not listed) |
| Catalog | `product` (Товар), `product-category` (Категорія товарів) | done: product created, deleted, or changed as one entry per command (name and «Активний» as fields, the touched parts such as «фото», «склад», «артикули» in the summary); category created, changed (name, «Активна»), moved, deleted; the sheet import, old-site photos and PPE supplier price runs are not listed (R-08) |
| Reference | `product-section` (Розділ товарів), `fabric` (Тканина), `accessory` (Фурнітура), `color` (Колір), `garment-part` (Деталь виробу), `operation` (Операція), `ppe-markup-rule` (Правило націнки ЗІЗ), `pricing-parameter` (Параметр ціни), `supplier` (Постачальник) | done: create, change (name and price as fields), delete; sheet imports and the KeepinCRM supplier sync are not listed (R-08) |
| Media | `media-file` (Фото), `media-folder` (Папка фото) | done: upload, rename or alt text, move, delete; imported photos (with a source key) and folders made for an upload path are not listed |

The exact command list per module is decided while wiring it (each command that creates, changes or deletes on a covered page raises an event); the feature card lists the result.

## Scenarios

- **SC-01** A manager creates КП №125 → one entry «Створив · КП №125» with the manager's name and role «Менеджер».
- **SC-02** The manager changes the sum and the client of КП №125 → «Змінив · КП №125» with «Сума: 1 200,00 → 1 500,00 грн» and «Клієнт: ТОВ А → ТОВ Б».
- **SC-03** Saving КП №125 without changes → no entry.
- **SC-04** The manager deletes КП №125 → «Видалив · КП №125», no link.
- **SC-05** The save fails (validation or a conflict) → no entry.
- **SC-06** The ActivityLog handler fails once (database blip) → the outbox retries and the entry appears once.
- **SC-07** The same outbox message is handled twice → one entry.
- **SC-08** A background import changes products → no per-row entries; an import started from the admin → one «Імпортував» entry with a summary.
- **SC-09** The admin opens «Журнал дій» → managers' entries newest first; «Усі ролі» adds admins and others; filters by person, period, type, action and title search combine.
- **SC-10** A user without `activity-log` view gets 403 on the endpoints and does not see the menu item.
- **SC-11** A manager is renamed after the action → the old entry keeps the old name.

## HTTP

| Method, route | operationId | Access | Answer |
| --- | --- | --- | --- |
| GET `/api/activity-log/entries?page&pageSize&actorId&role&subjectType&action&from&to&search` | `getActivityEntries` | `access:activity-log` view | `{ items: ActivityEntryResponse[], page, pageSize, total }`; 400 invalid filter |
| GET `/api/activity-log/filters` | `getActivityFilters` | view | `{ actors: {id, name}[], subjectTypes: {key, title}[], actions: string[] }` from the journal itself |

`ActivityEntryResponse`: `id, occurredAt, actorId, actorName, actorRoles[], action, subjectType, subjectTypeTitle, subjectId, subjectTitle, summary?, changes: {field, from?, to?}[]`.

Validator `GetActivityEntriesValidator`: page ≥ 1; pageSize 1–100 (default 50); `from` ≤ `to`; action one of the known values; search ≤ 100; subjectType and role ≤ 64. Messages in Ukrainian.

No write endpoints: entries come only from the outbox.

## Data (schema `activity_log`)

- `ActivityEntries`: `Id uuid` PK (the event id, Guid v7 by the ID strategy); `OccurredAt timestamptz`; `ActorId int`; `ActorName varchar(200)`; `ActorRoles varchar(500)` (`|Manager|Admin|`); `Action varchar(32)`; `SubjectType varchar(64)`; `SubjectId varchar(64)`; `SubjectTitle varchar(200)`; `Summary varchar(500) null`; `Changes jsonb` (array of `{field, from, to}`); `SourceModule varchar(32)`; `RecordedAt timestamptz`.
- Indexes: (`OccurredAt` desc, `Id`) for the default list; (`ActorId`, `OccurredAt` desc); (`SubjectType`, `OccurredAt` desc); `gin_trgm_ops` on `SubjectTitle` for the search (each word an escaped `ILIKE`, database rules).
- Insert: look up the id, then one `SaveChanges`; a unique violation on the key is a repeat (idempotent handler, R-05, `SaveRaces.IsUniqueViolation`).
- Outbox: Catalog and Media get their `OutboxMessages` table (migration `AddOutbox`, done: `catalog.OutboxMessages`, `media.OutboxMessages`); Crm, Production, Warehouse, Reference already have it.
- No foreign keys to other schemas. Migrations are additive (new schema, new tables), compatible with the previous release. Rollback: revert the code; the schema and the extra outbox tables stay and are ignored; unhandled `activity.recorded` rows stay in the outbox with «No handler» errors and are harmless.

## Design

### Flow

```
endpoint → command handler → aggregate.Method() raises IDomainEvent
        → SaveChanges → DomainEventOutboxInterceptor (BuildingBlocks.Infrastructure)
             → (after the insert, ids known) ActivityDomainEventOutbox (ActivityLog) → the module's IActivityDescriber
               + ICurrentActor → ActivityRecorded → OutboxMessage "activity.recorded" in the module schema, same transaction
        → OutboxAfterCommandBehavior tries the saved ids at once
        → (else) scheduler tick → outbox dispatcher
        → RecordActivityOutboxHandler (ActivityLog.Application) → insert once by event id
admin page → GET /api/activity-log/entries → GetActivityEntries query → AsNoTracking projection, paged in SQL
```

### File-role map

| Layer | Path | Responsibility | State |
| --- | --- | --- | --- |
| BuildingBlocks.Domain | `Events/IDomainEvent.cs`, `Events/IHasDomainEvents.cs`, `Events/DomainEventSource.cs`; `Entity/BaseEntity.cs` | event marker, raise and clear (base class for aggregates; `BaseEntity` inherits it) | done |
| BuildingBlocks.Application | `Actors/ICurrentActor.cs`, `Actors/Actor.cs` | who acts in this request (port) | done |
| BuildingBlocks.Application | `Outbox/ISavedOutboxMessages.cs`; `Behaviors/OutboxAfterCommandBehavior.cs` | ids saved in this scope; try them after the command | done |
| BuildingBlocks.Application | `DomainEvents/IDomainEventOutbox.cs` | a module's translation of its events into outbox messages (port the interceptor calls) | done |
| BuildingBlocks.Infrastructure | `DomainEvents/DomainEventOutboxInterceptor.cs`, `DomainEventRegistrationExtensions.cs`, `SavedOutboxMessages.cs`, `NoCurrentActor.cs` | collect events of tracked entities on save, write messages, remember saved ids; nobody outside a request | done |
| BuildingBlocks.Api | `Authorization/HttpCurrentActor.cs`, `CurrentActorRegistrationExtensions.cs` | `ICurrentActor` from the request's claims | done |
| ActivityLog.Contracts | `Activity/IActivityDescriber.cs`, `ActivityDescription.cs`, `ActivityChanges.cs`, `ActivityRecorded.cs`, `ActivityActions.cs`, `ActivityMessage.cs` | what a module implements to describe its events; the diff builder; the message (type `activity.recorded`); action names | done |
| ActivityLog.Domain | `Entries/ActivityEntry.cs`, `ActivityChange.cs`, `ActivityText.cs` | the stored entry and its limits (R-02) | done |
| ActivityLog.Application | `Features/Entries/EventHandlers/ActivityDomainEventOutbox.cs` (description + actor → message), `RecordActivityOutboxHandler.cs`, `Features/Entries/Queries/{GetActivityEntries,GetActivityFilters}/`, `Abstractions/{IActivityLogDbContext,IActorNames}.cs` | store an entry; list and filters | done |
| ActivityLog.Infrastructure | `Persistence/ActivityLogDbContext.cs`, `Configurations/`, `Migrations/`; `Adapters/Identity/StaffActorNames.cs`; `DependencyInjection/` | schema `activity_log`; full names over Identity.Contracts | done |
| ActivityLog.Api | `Endpoints/Entries/{GetActivityEntries,GetActivityFilters}/`, `Access/ActivityLogAccess.cs`, `DependencyInjection/` | HTTP | done |
| Source modules | `<Module>.Domain/<Aggregate>/Events/*` | the events of each aggregate | planned (stage 3) |
| Source modules | `<Module>.Application/Features/<Feature>/EventHandlers/<Feature>Activity.cs` | event → `ActivityDescription` (`IActivityDescriber`) | planned (stage 3) |
| Identity | `AccessCatalog`: `activity-log` | R-10 | done |
| Host.Api, Host.Seed | register the module, the behavior, the actor | composition; Host.Seed gets the «nobody» actor (R-01) | done |

### Why the describer sits in the module's Application

The domain event says what happened in the module's terms (ids, values). How it reads in the journal (Ukrainian labels, money and date format, subject titles) is presentation of the module's own data, and only the module knows it; the shared interceptor stays free of module knowledge and ActivityLog stays free of every module's model. Who and when are added once, in ActivityLog, so no module repeats them.

## Prerequisites

- **EN-01** None in the cloud: the outbox task already runs in the tick. The new migration runs with the deploy.

## Tasks and evidence (TDD: each task starts with failing tests)

- **TS-01** ADR-0016, C4 update, this SDD, index entries (stage 1).
- **TS-02** BuildingBlocks: entity events, interceptor, saved-ids collector, after-command behavior, actor port. Tests: events raised and cleared; interceptor writes one message per described event in the same save; nothing on a failed save; no message without an actor; behavior dispatches saved ids once.
- **TS-03** ActivityLog module: domain limits (cutting, ≤ 20 changes), handler idempotency (same id twice → one row), queries (filters, role default left to the client, paging, search words), validator, endpoints and access (403), contract snapshot, migration `InitialActivityLog`.
- **TS-04** Crm events and describer (SC-01…SC-04), then Production, Warehouse, Catalog (+ outbox), Reference, Media (+ outbox). Per module: event tests on aggregates, describer tests, one handler test that a command writes the activity message.
- **Evidence 2026-11-16 (TS-02, TS-03):** `dotnet test tests/UnitTests --filter "UnitTests.ActivityLog|UnitTests.Platform.DomainEvents"` 22 passed (interceptor: same save, cleared, failed save, no actor, no change; behavior; entry limits; handler idempotency and names; list, role, period, search, paging, filters, validator; contract and access); `tests/ArchitectureTests` 119 passed; `tests/HostApiTests` endpoint snapshot updated.
- **TS-05** brevi-web page (separate spec in brevi-web).
- **TS-06** Full audit (backend and frontend separately), feature card, access docs, outbox runbook note, spec status.
- **Evidence 2026-11-16 (TS-04 to TS-06):** Crm and Production #184, Warehouse and Reference #185, Catalog and Media #186 (outbox tables), and the brevi-web page are merged. Unit 1256, architecture 119, HostApi 64 and integration 218 tests pass. Audit: [audit.md](audit.md), no blockers; AF-02 (retention) waited for the owner.
- **TS-07** Retention (R-12, owner 2026-11-17): `DeleteOldActivityEntries` command, runner `ActivityLogCleanupTaskRunner`, catalogue task `activity-log-cleanup`. Tests: `DeleteOldActivityEntriesTests` (older than a year deleted, the rest stay, a repeat deletes nothing), `SchedulerTickTests` (the new task starts on).
