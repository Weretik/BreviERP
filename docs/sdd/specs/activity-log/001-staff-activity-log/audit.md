# ACT-001 «Журнал дій»: final backend audit

**Date:** 2026-11-16. **Scope:** merged PRs #184, #185 and #186.

- BuildingBlocks domain events (`DomainEventSource`, `DomainEventOutboxInterceptor`, `IDomainEventSuppression`).
- The ActivityLog module.
- The describers, events and suppressions in Crm, Production, Warehouse, Reference, Catalog and Media.

The frontend audit is in brevi-web, `docs/specs/admin/activity-log/001-staff-activity-log/audit.md`.

## What was checked

| Area | Evidence | Result |
| --- | --- | --- |
| Module boundaries | ActivityLog has its own schema `activity_log` and its own migration history, with no foreign keys. Source modules reference only `ActivityLog.Contracts`. ActivityLog reads staff names through `Identity.Contracts`. `ActivityLogDependencyTests` and the 119 architecture tests pass. | OK |
| Layers | Events live in Domain. Describers live in `<Module>.Application/Features/Activity/EventHandlers`. The interceptor and registration live in BuildingBlocks.Infrastructure. Host.Api only composes (global using, `AddDomainEvents` in the pipeline). | OK |
| Folder axiom | `FolderSizeTests` pass. New folders hold 2 or more files (`Products/Activity`, `Leads/Events`, `Features/Activity/EventHandlers`). | OK |
| Transactions | The change and its `activity.recorded` rows commit in one transaction. The interceptor opens one when the caller has none, joins the caller's otherwise, and rolls back on either failure. Covered by `DomainEventOutboxInterceptorTests`. | OK |
| Outbox | Catalog and Media gained `OutboxMessages` (migration `AddOutbox`, additive, keeps the previous release working). `has-pending-model-changes` is clean. The runbook lists the message type. | OK |
| Idempotency | `RecordActivityOutboxHandler` dedupes by event id (primary key), and a racing copy's unique violation counts as done. The audit table in the idempotency rules now lists it (fixed in this audit). | OK |
| Reads | `AsNoTracking`, filters and paging in SQL, per-word escaped `LIKE` on the trigram-indexed `SearchText`, indexes `(OccurredAt, Id)`, `(ActorId, OccurredAt)` and `(SubjectType, OccurredAt)`. | OK, see AF-01 |
| API | `getActivityEntries` and `getActivityFilters` use policy `access:activity-log` and have a FluentValidation validator with Ukrainian messages. OpenAPI is generated, and the HostApi snapshot passes. | OK |
| Who is recorded | The actor comes from the signed-in session. The scheduler and seed have no actor and write nothing. Imports, syncs and the old «Цех» transfers call `Suppress()` (R-08). | OK |
| ADR and C4 | ADR-0016 has its consequences updated. The C4 container diagram shows all six modules → activitylog through the outbox. | OK |
| Tests | Unit 1256, architecture 119, HostApi 64, integration 218 (2 skipped, unrelated), on a local PostgreSQL. | OK |

## Findings

| ID | Severity | Status | Where | Finding | Target change / plan |
| --- | --- | --- | --- | --- | --- |
| AF-01 | Low | Deferred (known deviation in database rules) | `GetActivityEntriesHandler`, `GetActivityFiltersHandler` | The default «Роль = Менеджер» filter is a `LIKE '%\|Manager\|%'` on `ActorRoles` with no index, the total is a filtered `COUNT(*)`, and the filter options group the whole table. | Fine at the expected thousands of rows a month. Above about 500 000 rows: roles as `text[]` with a GIN index, or a trigram index, and the filter options kept in a small table. |
| AF-02 | Medium | Fixed 2026-11-17 | `activity_log.ActivityEntries` | There was no retention: the journal grew forever. | The owner chose one year (R-12): the daily task `activity-log-cleanup` deletes older rows. |
| AF-03 | Low | Fixed in this audit | `docs/sdd/standards/idempotency-rules.md`, `database-rules.md` | The idempotency audit table did not list the `activity.recorded` handler, and the database rules listed four outboxes instead of six. | Rows added. |
| AF-04 | Low | Accepted | Describers | An event carries its aggregate, and the describer reads it after the save, because database ids exist only then. Two changes of one object in one save therefore both show the final values. Products avoid this by making one event per command; Media and Reference commands change an object once. | Kept. A new command that changes the same object twice in one save makes one event, like `Product`. |
| AF-05 | Low | Accepted | `UpdateProductCategoryHandler` | A category saved with a new name and a new parent gives two entries: «Змінено» and «Перенесено в інший розділ». | Kept: both are true and rare. |
| AF-06 | Info | Deferred (R-08) | Imports | Imports record nothing: no single summary entry such as «Імпорт: 120 рядків». The action `Imported` is reserved for it. | Add it when the owner asks. |

There are no blockers. AF-02 was answered by the owner on 2026-11-17 and fixed (R-12).
