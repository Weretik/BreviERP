# Background tasks runbook «Фонові задачі»

Specification: [SCH-001](../../specs/scheduling/001-background-tasks/README.md). Feature card: [«Фонові задачі»](../../features/scheduling/background-tasks.md). Do not put real URLs, tokens or service account keys into documentation or logs.

## How it works

```text
Cloud Scheduler brevierp-tick (every five minutes, OIDC)
  → POST /internal/scheduler/tick (Host.Api)
      (a) outbox: IOutboxDispatcher.DispatchDueAsync, result stored on task «outbox»
      (b) runs past their limit failed («Задача не завершилася за N хв.»)
      (c) due tasks claimed one by one (concurrency token: one tick wins)
      (d) one execution of Cloud Run Job brevierp-tasks with --tasks=key:runId,...  (none due: no execution)
      (e) runs older than 90 days deleted
  → brevierp-tasks (Host.Seed --tasks=...): for each task Start → runner → Finish (summary or main reason)
```

The task list is code: `ScheduledTaskCatalog` (keys, Ukrainian names and descriptions, default schedules, time limits) and one runner per key in `src/Bootstrapper/Host.Seed/Jobs/Tasks/`. Schedules, the on/off switch and the last results are rows of `scheduling.ScheduledTasks`; the admin changes them on «Адміністрування → Фонові задачі». A missing row is created with the catalogue defaults. All times are Kyiv (`Europe/Kyiv`).

| Key | Task | Default | Limit | Secrets on the job |
| --- | --- | --- | --- | --- |
| `outbox` | Досилання повідомлень (inside the tick, not editable) | every 5 minutes | 5 min | none |
| `reminders` | Нагадування в Telegram | Mon–Fri 09:00, **off** | 30 min | `telegram-reminders-bot-token` |
| `keepin-suppliers` | Постачальники з KeepinCRM | Mon–Fri 09:00, **off** | 30 min | `keepincrm-api-key` |
| `keepin-clients` | Клієнти з KeepinCRM | Mon–Fri 09:00, **off** | 30 min | `keepincrm-api-key` |
| `sewing-prices` | Ціни пошиву | Mon–Fri 09:00, **off** | 30 min | none |
| `media-thumbnails` | Прев'ю фото | Mon–Fri 09:00, **off** | 30 min | `storage-login`, `storage-password` |
| `purchase-invoices` | Рахунки на закупку | Mon–Fri 09:00, **off** | 30 min | none (sheet shared with the runtime account) |
| `leads` | Ліди | Mon–Fri 09:00, **off** | 30 min | none (sheet shared with the runtime account) |
| `ppe-supplier-prices` | Ціни постачальників ЗІЗ («Імпорт цін» decides which supplier is due) | Mon–Fri 09:00, **off** | 60 min | `trele-login`, `trele-password` (optional) |
| `activity-log-cleanup` | Очищення журналу дій: deletes «Журнал дій» entries older than one year | every day 03:00, on | 10 min | none |

The tasks of the old jobs start **off** so that they never run both from the old jobs and from the tick (R-12); `activity-log-cleanup` never ran in an old job and starts on; step 3 below turns them on. Tasks claimed in the same tick run one after another in one execution; a run that has not started within 240 minutes is failed («Задача не почалася…»), and the job itself stops after 4 hours.

## Configuration

| Setting | Where | Set by |
| --- | --- | --- |
| `Scheduler__Job__Project`, `Scheduler__Job__Region`, `Scheduler__Job__Name` | API environment | deploy workflow (`brevistoreapi`, `europe-central2`, `brevierp-tasks`) |
| `Scheduler__Audience` | API environment: Cloud Run URL + `/internal/scheduler/tick` | once, by hand (step 2) |
| `Scheduler__SchedulerServiceAccount` | API environment: e-mail of the `brevierp-scheduler` service account | once, by hand (step 2) |

Without both `Scheduler__Audience` and `Scheduler__SchedulerServiceAccount` the tick answers 404 and nothing is scheduled. Without `Scheduler__Job__*` due tasks fail with «запуск задач у Google Cloud не налаштований». The deploy workflow uses `--update-env-vars`, so values set by hand stay across deploys.

## Switch-over (cloud change: through the coordinator)

1. **Permission (EN-01).** Give the API's runtime service account the role «Cloud Run Jobs Executor With Overrides» (`roles/run.jobsExecutorWithOverrides`) on the job `brevierp-tasks` only (Cloud Run → Jobs → `brevierp-tasks` → Permissions). The job exists after the first deploy of this change.
2. **Settings (EN-02).** Create the service account `brevierp-scheduler`. On `brevierp-api` set `Scheduler__Audience` and `Scheduler__SchedulerServiceAccount` (that account).
3. **Scheduler (EN-03).** Create the Cloud Scheduler job `brevierp-tick`: every five minutes (`*/5 * * * *`, time zone Europe/Kyiv; the owner chose five minutes on 2026-11-14), POST to `<Cloud Run URL>/internal/scheduler/tick`, OIDC token of `brevierp-scheduler` with the audience of step 2. Production never had a `brevierp-outbox` entry, so the outbox first runs here, inside the tick. The same day on «Фонові задачі» set each task's schedule and switch it on, then pause the Cloud Scheduler entries of `brevierp-reminders` and `brevierp-sync`. A week later delete those two entries and jobs; `--sync` and `--reminders-dispatch` can then be removed from the code.
4. **Check.** The scheduler's last run is 200 with `{"inline":{"outbox":"…"},"started":[],"launchError":null}`; «Запустити зараз» on «Прев'ю фото» starts an execution of `brevierp-tasks` within five minutes and the page shows «Остання успішна».

Rollback: pause `brevierp-tick`, resume the two old schedulers, switch the tasks off on the page.

## Diagnose

- Tick 404: step 2 is missing on the revision. 401: wrong audience or account, or an OAuth instead of an OIDC token.
- «Не вдалося запустити задачу: …» on a task: the API cannot start the job (step 1, or the `Scheduler__Job__*` values). The task gets its next time as usual.
- «Задача не завершилася за N хв.»: the job crashed or hangs; see the execution's logs (`Tasks.<key> started/OK/FAILED`).
- Logs of the job: `Tasks started: <keys>`, per task `<key> OK: <summary>` or `<key> FAILED: <reason>`, and `<key> skipped: run … is no longer current` when the tick already gave up on that run.

## Adding a task

1. Put the work in the owning module as a command; it must be safe to repeat and change nothing when it fails.
2. Add a key and a definition (Ukrainian name and description, default schedule, limit) to `ScheduledTaskCatalog`.
3. Add a runner `IScheduledTaskRunner` with the same `Key` in `Host.Seed/Jobs/Tasks/<area>/` and list it in `ScheduledTaskRunners`. It only sends the command, logs and returns a short Ukrainian summary.
4. Register the module in `SeedHostServicesExtensions` if Host.Seed does not have it yet, and attach its secrets in the `Create or update background tasks job` step of the deploy workflow.
