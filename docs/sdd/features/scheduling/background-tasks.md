# Background tasks «Фонові задачі»

«Адміністрування → Фонові задачі»: every periodic task of Brevi with its name and description, schedule, next run, last success with a short summary and last failure with its main reason, run history and «Запустити зараз». The task list is code; the admin changes only the schedule, the on/off switch and the next run. One Cloud Scheduler entry calls the tick every minute; the tick sends the outbox and starts one Cloud Run Job execution for the tasks that are due. [SDD](../../specs/scheduling/001-background-tasks/README.md), [contract](../../contracts/scheduling/background-tasks.openapi.yaml), [runbook](../../operations/jobs/background-tasks.md), [tables](../../architecture/data-model/scheduling.md). Access: page `background-tasks` (view, create = «Запустити зараз», edit = schedule), Admin by default.

## Code structure

```text
src/Modules/Scheduling/
  Scheduling.Domain/Scheduling.Domain/
    Schedules/   # TaskSchedule (interval with hours, daily Kyiv times on weekdays, manual; NextAfter), KyivTime, ScheduleErrors
    Tasks/       # ScheduledTask (claim, start, succeed, fail, expire, run now, change schedule; Version), TaskState, ScheduledTaskErrors
    Runs/        # ScheduledTaskRun, RunTrigger, RunOutcome
  Scheduling.Application/Scheduling.Application/
    Abstractions/            # ISchedulingDbContext, ITaskJobLauncher, ITickWork
    Features/Catalog/Shared/ # ScheduledTaskCatalog: the code list of tasks, keys, defaults
    Features/Tick/Commands/RunSchedulerTick/   # the minute of the scheduler
    Features/Runs/Commands/{Start,Finish}ScheduledTaskRun/   # called by the job
    Features/Tasks/          # GetScheduledTasks, GetScheduledTaskRuns, UpdateScheduledTaskSchedule, RunScheduledTaskNow; Shared/ (schedule input, validator, responses)
  Scheduling.Infrastructure/Scheduling.Infrastructure/
    Persistence/             # SchedulingDbContext (schema scheduling), configurations, migrations
    Integrations/CloudRun/   # CloudRunTaskJobLauncher: jobs:run with --tasks=key:runId,...
    Integrations/Outbox/     # OutboxTickWork: the outbox inside the tick
  Scheduling.Api/Scheduling.Api/
    Endpoints/Tasks/         # /api/scheduling/tasks, one <UseCase>/ folder each
    Endpoints/Tick/RunSchedulerTick/   # POST /internal/scheduler/tick (Google OIDC token checked in the endpoint)
src/Bootstrapper/Host.Seed/Jobs/Tasks/   # IScheduledTaskRunner per key, ScheduledTasksJobRunner (--tasks), SyncJobRunner (old --sync)
```

## Tests

`dotnet test tests/UnitTests/UnitTests.csproj --filter UnitTests.Scheduling` (domain, tick, runs, admin, validators, contract); `tests/HostApiTests` `SchedulerTickEndpointTests`; `tests/ArchitectureTests` `SchedulingDependencyTests`.
