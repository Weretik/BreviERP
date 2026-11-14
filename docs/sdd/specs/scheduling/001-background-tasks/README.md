# SCH-001 — «Фонові задачі»: one scheduler, schedules in the database

**Status:** in progress (2026-11-14). **Date:** 2026-11-14. **Module:** Scheduling (new, schema `scheduling`); runners in `Host.Seed`; tick in `Host.Api`. **Owner decision:** 2026-11-14, thread «Управлять фоновыми задачами из админки» (plan accepted with «да»).

## Why

Every scheduled run needs its own Cloud Scheduler job, and only three are free per billing account. Brevi already uses three (`brevierp-reminders`, `brevierp-sync`, `brevierp-outbox`). The seven `brevierp-sync` steps share one time (weekdays 09:00) and their result is visible only in Cloud Logging. Nobody but the owner can change a time or start a run.

## Decision

One Cloud Scheduler job calls `POST /internal/scheduler/tick` every minute. The tick runs the outbox, then starts every due task in one execution of the Cloud Run Job `brevierp-tasks` (`Host.Seed --tasks=<keys>`). The list of tasks is code; their schedules, on/off switch and run results are rows of the new module Scheduling. The admin page «Адміністрування → Фонові задачі» shows them, changes the schedule and starts a task now.

Why a new module: no module owns schedules or run history; Reminders, Crm, Catalog, Media and Purchasing own the work the tasks do and stay unchanged. Alternatives rejected: a Cloud Scheduler job per task (the free limit); starting the Cloud Run Job every minute (1440 executions a day, outside the free tier); running the work inside the API request (1C, KeepinCRM and supplier pages take minutes and would slow the admin).

Not in scope: catalogue import and the pricing modes of `Host.Seed` (one-off developer commands), migrations and seed (`brevierp-seed`, part of the deploy).

## Rules

- **R-01 Tasks are code.** `ScheduledTaskCatalog` (Scheduling.Application) lists every task: stable kebab-case key, Ukrainian name and one-line description, default schedule, longest allowed run (minutes), whether the admin may change the schedule, and whether it runs inside the tick (outbox) or in the job. The admin cannot create, rename or delete a task. A key that leaves the catalogue disappears from the page; its rows stay and are ignored.
- **R-02 Schedule.** One of: `Interval` (every N minutes, 5–1440, optionally only between two Kyiv hours), `Daily` (one to six Kyiv times on the chosen weekdays), `Manual` (only «Запустити зараз»). Times are Kyiv (`Europe/Kyiv`), stored as UTC instants for the next run. Saving a schedule recomputes «Наступний запуск»; the admin may also set the next run once to a later instant.
- **R-03 On/off.** A disabled task is never started by the schedule; «Запустити зараз» still runs it. New tasks start enabled, except when the catalogue marks them disabled until the cloud switch (R-12).
- **R-04 Tick.** The tick (a) runs `IOutboxDispatcher.DispatchDueAsync` and stores its outcome on task `outbox`; (b) marks a run as failed when it runs longer than its task's limit since it started («Задача не завершилася за N хв»), or has not started within 240 minutes (tasks of one execution run one after another; «Задача не почалася…»); (c) takes due tasks: enabled and next run ≤ now, or «Запустити зараз» pressed, and not running; (d) claims each with one conditional update (`State = Idle` → `Running`), so two ticks never start the same task; (e) starts one job execution with all claimed keys and their run ids; (f) deletes runs older than 90 days. A failure of one part does not stop the others.
- **R-05 Job start fails.** When the job cannot be started (no permission, Cloud Run error, not configured) every claimed task is marked failed with the reason and its next run is computed as after a run, so it does not retry every minute.
- **R-06 Run.** `Host.Seed --tasks=key:runId,...` runs the tasks one after another, each in its own scope: marks the run started, sends the owning module's command, then records success with a short Ukrainian summary or failure with the main reason (≤ 500 characters). One failure does not stop the next task; the execution exits 1 when any failed. A run id that is not the task's current run is skipped (a late duplicate).
- **R-07 Next run.** After success or failure the next run is the first schedule instant after the finish; for `Manual` it is empty. «Запустити зараз» does not move the schedule.
- **R-08 Run now.** «Запустити зараз» records who pressed it and when; the next tick (≤ 1 minute) starts it. Pressing it while the task runs or is already requested answers 409 «Задача вже виконується» / «Запуск уже заплановано».
- **R-09 What the admin sees.** Per task: name, description, schedule in words, next run, running since, last success (time and summary), last failure (time and reason), on/off. Only problems are marked (last run failed, or no success for twice the schedule's interval); there is no «all OK» badge. History: the last 50 runs of a task with trigger (schedule / who pressed), start, finish, result.
- **R-10 Protected tick.** `POST /internal/scheduler/tick` accepts only a Google OIDC token with audience `Scheduler:Audience` from `Scheduler:SchedulerServiceAccount` (the same check as the outbox endpoint); anything else gets 401. Without that configuration it answers 404.
- **R-11 Access.** Page `background-tasks` in «Права доступу», group «Адміністрування»: View = see the page and history, Create = «Запустити зараз», Edit = change schedule and on/off. Default: Admin all three; the Director always passes.
- **R-12 Switch-over.** Until the owner moves Cloud Scheduler to the tick, the old modes `--sync=true` and `--reminders-dispatch=true` keep working with the same runners. Tasks done by `brevierp-sync` and `brevierp-reminders` are created **disabled**, so the old and the new schedule never both run them; the owner enables them on the page the day the old jobs are paused. The outbox task runs as soon as the tick is called.

## Scenarios

- **SC-01** Task due, idle → the tick claims it and starts one job execution with its key; a second tick a second later starts nothing.
- **SC-02** Two tasks due at 09:00 → one execution with both keys, each with its own run row.
- **SC-03** The job runs a task that succeeds → last success with the summary, state idle, next run = next schedule instant.
- **SC-04** The task fails → last failure with the reason, last success unchanged, next run computed; the next task in the same execution still runs.
- **SC-05** The job is killed → after the longest allowed run the tick marks the task failed «Задача не завершилася за N хв» and frees it.
- **SC-06** The job cannot be started → claimed tasks failed with the reason, idle, next run computed.
- **SC-07** Admin changes «Залишки» to daily 07:00 and 13:00 → next run is the next of those Kyiv times; an invalid schedule (no time, 25:00, interval 2 minutes) → 400 with a Ukrainian message.
- **SC-08** Admin presses «Запустити зараз» on a disabled `Manual` task → it runs on the next tick; pressing again while it runs → 409.
- **SC-09** A user without the page gets 403 on every endpoint; with View only, «Запустити зараз» and the schedule save get 403.
- **SC-10** Tick without a token / wrong audience / wrong account → 401; not configured → 404; with the scheduler's token → 200 `{ outbox: {processed, failed}, started: [keys] }`.
- **SC-11** Schedule instants across the daylight-saving change in Kyiv are correct (Daily 03:00 is 03:00 Kyiv on both sides).

## Tasks (Brevi)

| Key | Name | Description | Default | Runs in |
| --- | --- | --- | --- | --- |
| `outbox` | Досилання повідомлень | Повторює повідомлення, які не пішли з першого разу: заявки з сайту, «Надіслати в цех», параметри цін | every minute, not editable | tick |
| `reminders` | Нагадування в Telegram | Надсилає нагадування про строки в групи Telegram | weekdays 09:00 | job |
| `keepin-suppliers` | Постачальники з KeepinCRM | Оновлює постачальників з KeepinCRM | weekdays 09:00 | job |
| `keepin-clients` | Клієнти з KeepinCRM | Оновлює клієнтів з KeepinCRM | weekdays 09:00 | job |
| `sewing-prices` | Ціни пошиву | Публікує ціни пошиву, якщо щось змінилося поза адмінкою | weekdays 09:00 | job |
| `media-thumbnails` | Прев'ю фото | Робить прев'ю для фото бібліотеки, у яких його ще немає | weekdays 09:00 | job |
| `purchase-invoices` | Рахунки на закупку | Переносить рахунки з таблиці «финансы» | weekdays 09:00 | job |
| `leads` | Ліди | Переносить ліди з таблиці «Лиды Brevi» | weekdays 09:00 | job |
| `ppe-supplier-prices` | Ціни постачальників ЗІЗ | Оновлює ціни постачальників за розкладом «Імпорт цін» (який день — вирішує «Імпорт цін») | weekdays 09:00 | job |

## HTTP

| Method, route | operationId | Access | Body / answer |
| --- | --- | --- | --- |
| GET `/api/scheduling/tasks` | `getScheduledTasks` | View | `ScheduledTaskResponse[]` in catalogue order |
| GET `/api/scheduling/tasks/{key}/runs` | `getScheduledTaskRuns` | View | last 50 `ScheduledTaskRunResponse`, newest first; 404 unknown key |
| PUT `/api/scheduling/tasks/{key}/schedule` | `updateScheduledTaskSchedule` | Edit | `{ enabled, schedule: { kind, everyMinutes?, fromHour?, toHour?, times?[], days?[] }, nextRunAt? }` → task; 400 invalid, 404 unknown, 409 not editable |
| POST `/api/scheduling/tasks/{key}/run` | `runScheduledTaskNow` | Create | → task; 404, 409 |
| POST `/internal/scheduler/tick` | — (not in the contract) | OIDC (R-10) | `{ outbox, started }` |

Validators: `UpdateScheduledTaskScheduleValidator` (kind known; Interval: everyMinutes 5–1440, hours 0–23 and from < to when given; Daily: 1–6 distinct `HH:mm` times, 1–7 distinct weekdays; Manual: nothing else; nextRunAt not in the past), key validators (non-empty, ≤ 64).

## Data (schema `scheduling`)

- `ScheduledTasks`: `Key varchar(64)` PK; `Enabled`; `Schedule jsonb`; `NextRunAt null`; `State` (`Idle`/`Running`); `CurrentRunId uuid null`; `RunningSince null`; `RunRequestedAt null`, `RunRequestedBy varchar(200) null`; `LastSuccessAt null`, `LastSuccessSummary varchar(500) null`; `LastFailureAt null`, `LastFailureReason varchar(500) null`; `UpdatedAt`, `UpdatedBy`.
- `ScheduledTaskRuns`: `Id uuid` PK (created in the API and passed to the job: Guid by the ID strategy); `TaskKey`; `Trigger` (`Schedule`/`Manual`); `RequestedBy null`; `ClaimedAt`; `StartedAt null`; `FinishedAt null`; `Outcome` (`Running`/`Succeeded`/`Failed`); `Message varchar(500) null`. Index (`TaskKey`, `ClaimedAt` desc).
- Rows are created by the tick for catalogue keys that have none. No foreign keys to other modules. Rollback: revert the code; the schema stays and is ignored.

## Design

| Layer | Path (under `src/Modules/Scheduling/`) | Responsibility | State |
| --- | --- | --- | --- |
| Domain | `Scheduling.Domain/Schedules/TaskSchedule.cs`, `ScheduleKind.cs`, `ScheduleErrors.cs`, `KyivTime.cs` | schedule value object, validation of its own invariants, `NextAfter(instant)` in Kyiv | done |
| Domain | `Scheduling.Domain/Tasks/ScheduledTask.cs`, `TaskState.cs`, `ScheduledTaskErrors.cs` | claim, start, succeed, fail, expire, request run, change schedule | done |
| Domain | `Scheduling.Domain/Runs/ScheduledTaskRun.cs`, `RunOutcome.cs`, `RunTrigger.cs` | one run | done |
| Application | `Features/Catalog/Shared/ScheduledTaskCatalog.cs`, `ScheduledTaskDefinition.cs`, `ScheduledTaskKeys.cs` | the code list of tasks (R-01) | done |
| Application | `Features/Tick/Commands/RunSchedulerTick/` | R-04, R-05 | done |
| Application | `Features/Runs/Commands/{StartScheduledTaskRun,FinishScheduledTaskRun}/`, `Features/Runs/Shared/` | R-06, R-07 (called by the job) | done |
| Application | `Features/Tasks/Commands/{UpdateScheduledTaskSchedule,RunScheduledTaskNow}/`, `Features/Tasks/Queries/{GetScheduledTasks,GetScheduledTaskRuns}/`, `Features/Tasks/Shared/` | admin use cases, schedule input and its validator | done |
| Application | `Abstractions/ISchedulingDbContext.cs`, `ITaskJobLauncher.cs`, `ITickWork.cs` | ports | done |
| Infrastructure | `Persistence/SchedulingDbContext.cs`, `Configurations/`, `Migrations/` | schema `scheduling` | done |
| Infrastructure | `Integrations/CloudRun/CloudRunTaskJobLauncher.cs`, `CloudRunJobOptions.cs`; `Integrations/Outbox/OutboxTickWork.cs` | Cloud Run Admin API `jobs:run` with argument overrides; the outbox inside the tick | done |
| Api | `Endpoints/Tasks/*`, `Endpoints/Tick/RunSchedulerTick/`, `Access/SchedulingAccess.cs` | HTTP | done |
| Identity | `AccessCatalog`: `background-tasks` | R-11 | done |
| Host.Seed | `Jobs/Tasks/ScheduledTasksJobRunner.cs`, `IScheduledTaskRunner.cs`, `ScheduledTaskRunners.cs`, `SyncJobRunner.cs`, runners in `Keepin/`, `Pricing/`, `Office/`, `Media/` | R-06; the old `Sync/` steps became runners; `--sync`, `--reminders-dispatch` call them (R-12) | done |
| Host.Api | maps the tick and the module | composition | done |
| Deploy | `deploy-cloudrun.yml`: job `brevierp-tasks` (copied from the seed job, argument set per execution, 4 h limit), API env `Scheduler__Job__*` kept across deploys | | done |

## Prerequisites (cloud, owner through the coordinator)

- **EN-01** Runtime service account of `brevierp-api`: role `roles/run.jobsExecutorWithOverrides` on job `brevierp-tasks` only.
- **EN-02** API settings `Scheduler__Audience` (Cloud Run URL + `/internal/scheduler/tick`), `Scheduler__SchedulerServiceAccount` (`brevierp-outbox` service account), `Scheduler__Job__Project`, `__Region`, `__Name`.
- **EN-03** Cloud Scheduler `brevierp-outbox`: URL changed to `/internal/scheduler/tick` with the new audience. Then on the page enable the tasks and pause `brevierp-reminders` and `brevierp-sync` the same day; delete them a week later.

## Tasks and evidence

- **TS-01** Domain: `TaskScheduleTests` (interval, window, daily times and weekdays, Kyiv DST, validation), `ScheduledTaskTests` (claim once, expire, succeed/fail next run, run now 409).
- **TS-02** Tick (R-04, R-05): handler tests with a fake launcher and in-memory clock: SC-01, SC-02, SC-05, SC-06; outbox outcome stored.
- **TS-03** Runs (R-06, R-07): SC-03, SC-04, stale run id skipped; `ScheduledTasksRunner` argument parsing.
- **TS-04** Admin use cases and validators: SC-07, SC-08; endpoints and access SC-09; tick endpoint SC-10.
- **TS-05** Persistence: migration `InitialScheduling`, conditional claim on PostgreSQL (integration).
- **TS-06** Host.Seed runners replace `Sync/` steps; `--sync` and `--reminders-dispatch` still work; architecture tests (thin runners, folder size).
- **Evidence 2026-11-14:** `dotnet test tests/UnitTests --filter UnitTests.Scheduling` 62 passed (domain, tick, runs, admin, validators, contract); `tests/HostApiTests` 60 passed (tick endpoint, anonymous list, endpoint snapshot); `tests/ArchitectureTests` 112 passed. The claim's concurrency token is tested on SQLite with two units of work; a PostgreSQL integration test (TS-05) and a test of the `--tasks` argument parsing are not written.
- **TS-07** Deploy job, runbook `operations/jobs/background-tasks.md`, feature card, access docs, contract snapshot, spec index.
