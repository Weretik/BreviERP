# ActivityLog schema

Code model reviewed 2026-11-16. `ActivityLog.Infrastructure.Persistence.ActivityLogDbContext` owns schema `activity_log`; migration history is `activity_log.__EFMigrationsHistory`. Migration `InitialActivityLog` creates the table ([ACT-001](../../specs/activity-log/001-staff-activity-log/README.md), [ADR-0016](../decisions/0016-domain-events-to-activity-log.md)). No foreign keys to other schemas: the person and the object are kept by id and as text at the moment of the action.

| Table | Identity / relationships / purpose |
| --- | --- |
| ActivityEntries | PK Id (the `EventId` of the `activity.recorded` message, UUID v7; a repeat is refused by the key). OccurredAt, ActorId (Identity account id, no link), ActorName (full name when stored, else the login), ActorRoles (`\|Manager\|Admin\|`, so a role is one `LIKE`), Action, SubjectType + SubjectTypeTitle, SubjectId, SubjectTitle, Summary, Changes (jsonb: `[{field, from, to}]`, ≤ 20), SourceModule, RecordedAt, SearchText (generated over SubjectTitle and Summary, `gin_trgm_ops`). Indexes (OccurredAt desc, Id desc), (ActorId, OccurredAt desc), (SubjectType, OccurredAt desc). Entries are never changed or deleted |
