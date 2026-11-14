# Scheduling schema

Code model reviewed 2026-11-14. `Scheduling.Infrastructure.Persistence.SchedulingDbContext` owns schema `scheduling`; migration history is `scheduling.__EFMigrationsHistory`. Migration `InitialScheduling` creates the tables ([SCH-001](../../specs/scheduling/001-background-tasks/README.md)). No foreign keys to other schemas: a task is named by its catalogue key.

| Table | Identity / relationships / purpose |
| --- | --- |
| ScheduledTasks | PK Key (catalogue key, e.g. `leads`). Enabled, Schedule (jsonb: kind, every minutes and hours, Kyiv times, ISO weekdays), NextRunAt, State (`Idle`/`Running`), CurrentRunId, RunningSince, RunRequestedAt/By («Запустити зараз»), LastSuccessAt + LastSuccessSummary, LastFailureAt + LastFailureReason (≤ 500), UpdatedAt/By, Version (concurrency token: a task is claimed by one tick only). A row is created with the catalogue defaults when missing |
| ScheduledTaskRuns | PK Id (UUID v7). TaskKey, Trigger (`Schedule`/`Manual`), RequestedBy, ClaimedAt, StartedAt, FinishedAt, Outcome (`Running`/`Succeeded`/`Failed`), Message (≤ 500). Indexes (TaskKey, ClaimedAt desc) and ClaimedAt; the tick deletes runs older than 90 days |
