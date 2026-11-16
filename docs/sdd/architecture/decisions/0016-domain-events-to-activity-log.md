# ADR-0016: Domain events reach the activity log through each module's outbox

**Status:** accepted · **Date:** 2026-11-16 ([ACT-001](../../specs/activity-log/001-staff-activity-log/README.md)) · **Owner:** Виталий (project owner)

## Context

The owner wants to see who created, changed or deleted what across the admin (КП, production orders, fabric movements, catalogue and reference data). The actions happen in six modules; none of them owns a history of the others. Brevi had no domain events: aggregates changed state and handlers saved it. Side effects already leave through a per-module transactional outbox ([ADR-0009](0009-transactional-outbox.md)).

## Options

- Each module writes its own history table: rejected; six copies of one concern and no single page.
- An EF interceptor that logs every changed column of every entity: rejected; it records table rows (`UpdatedAt`, sort orders, cached totals) rather than business actions and cannot name them in the owner's words.
- In-process domain events dispatched after the command (the unused KedrStore `DomainEventDispatcherBehavior`): rejected; an entry is lost when the process stops after the commit and the handler would run in the source module's transaction scope.
- Domain events raised by aggregates, translated by the owning module and written to its outbox in the same save; a new ActivityLog module stores them: chosen.

## Decision

- `BuildingBlocks.Domain` gives every entity `Raise(IDomainEvent)` and the list of raised events. Events are records in the module's Domain, named in the past tense (`QuoteCreated`), carrying ids and the changed values.
- A shared `SaveChangesInterceptor` (`BuildingBlocks.Infrastructure/DomainEvents`) takes the events of tracked entities before the save and opens a transaction when the caller has none. After the save, when database-generated ids are known, the module's `IDomainEventOutbox` implementations turn the events into outbox messages, a second save in the same transaction writes them, and the transaction commits. A failure of either save rolls both back. Events of new rows with generated ids carry the aggregate, so the translation reads the id after the insert.
- Each source module registers an `IActivityDescriber` (`ActivityLog.Contracts`) that says how its events read: subject, action, changed fields. ActivityLog's one `IDomainEventOutbox` (`ActivityDomainEventOutbox`) adds the signed-in actor (`ICurrentActor`) and the time and returns the `activity.recorded` message with the `ActivityRecorded` payload; the interceptor writes it into the saving module's outbox. No actor (background work) means no entry.
- Each module groups its describers in `Application/Features/Activity/EventHandlers/`.
- After the command a Mediator behavior tries the saved messages once; the outbox task of the scheduler tick retries the rest.
- ActivityLog (schema `activity_log`) handles `activity.recorded` by inserting under the event id with `ON CONFLICT DO NOTHING`; its admin API lists entries.

## Consequences

- Source modules reference only `ActivityLog.Contracts`; ActivityLog references no module. No foreign keys between schemas.
- Every module that raises events needs an outbox table (Catalog and Media gain one).
- Entries can appear up to one tick late when the first try fails.
- Future reactions to the same events (notifications, statistics) add another `IDomainEventOutbox` translation without touching the aggregates.
- Imports and syncs call `IDomainEventSuppression.Suppress()` before saving, so their rows write no events and the journal and the outbox stay small; a summary entry per import can come later (ACT-001 R-08).

## Sources

`src/BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain/Events/`, `src/BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure/DomainEvents/`, `src/Modules/ActivityLog/`, [ACT-001](../../specs/activity-log/001-staff-activity-log/README.md).
