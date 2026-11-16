# Staff activity log «Журнал дій»

«Адміністрування → Журнал дій»: who created, changed or deleted what and when, for every staff member; the page shows managers by default. Entries come only from domain events of the source modules through their outbox; nobody edits them. [SDD](../../specs/activity-log/001-staff-activity-log/README.md), [ADR-0016](../../architecture/decisions/0016-domain-events-to-activity-log.md), [contract](../../contracts/openapi.json), [tables](../../architecture/data-model/activity-log.md), [flow diagram](../../architecture/c4/README.md#dynamic-an-action-reaches-журнал-дій). Access: page `activity-log` (view), Admin by default.

## How an entry is made

1. A command handler calls an aggregate method; the aggregate raises a domain event (`Raise(new QuoteChanged(...))`, base class `DomainEventSource` or `BaseEntity`).
2. On `SaveChanges` the shared `DomainEventOutboxInterceptor` takes the events and opens a transaction if the caller has none; after the insert (ids known) it asks every `IDomainEventOutbox` to translate them: ActivityLog's `ActivityDomainEventOutbox` asks the module's `IActivityDescriber` what happened (subject, action, changed fields) and returns an `activity.recorded` outbox message with `ActivityRecorded` and the actor from `ICurrentActor`. A second save writes the messages and the transaction commits; a failure rolls both back. Without a signed-in person nothing is written.
3. `OutboxAfterCommandBehavior` tries the message right after the command; the outbox task of the scheduler tick retries failures.
4. `RecordActivityOutboxHandler` stores the entry under the event id (a repeat does nothing) with the staff member's full name from `Identity.Contracts`.

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
  ActivityLog.Application/…/
    Abstractions/                                  # IActivityLogDbContext, IActorNames
    Features/Entries/EventHandlers/                # ActivityDomainEventOutbox (description + actor → message), RecordActivityOutboxHandler (idempotent by event id)
    Features/Entries/Queries/GetActivityEntries/   # page, filters, Kyiv period, word search; validator
    Features/Entries/Queries/GetActivityFilters/   # people and object types found in the journal
  ActivityLog.Infrastructure/…/
    Persistence/                                   # ActivityLogDbContext (schema activity_log), configuration, migrations
    Adapters/Identity/StaffActorNames.cs           # full name over Identity.Contracts
  ActivityLog.Api/…/Endpoints/Entries/             # GET /api/activity-log/entries, /filters
src/Modules/<Source>/…                             # events in Domain (<Aggregate>/Events/), IActivityDescriber in Application Features/<Feature>/EventHandlers/
```

## Covered actions

Filled in as each source module is wired (ACT-001 «Coverage»).

## Tests

`dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~UnitTests.ActivityLog|FullyQualifiedName~UnitTests.Platform.DomainEvents"`; `tests/ArchitectureTests` `ActivityLogDependencyTests`; `tests/HostApiTests` endpoint snapshot.
