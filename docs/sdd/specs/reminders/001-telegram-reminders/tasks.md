# Tasks

| ID | Task | Depends on | Status |
| --- | --- | --- | --- |
| RM-01 | Scaffold four module projects with `dotnet new classlib`, add to the solution and hosts, add architecture test | — | done |
| RM-02 | Domain: calendar, periods, due rules, schedule, planner, snooze, entities | RM-01 | done |
| RM-03 | Persistence: context, configurations, `InitialReminders` migration, default task seeder | RM-02 | done |
| RM-04 | Admin use cases with validators and controllers | RM-03 | done |
| RM-05 | Telegram gateway, message formatter, webhook update handling, chat discovery | RM-03 | done |
| RM-06 | Daily dispatcher and `--reminders-dispatch` Host.Seed mode | RM-05 | done |
| RM-07 | Cloud Build: bot token and webhook secret wiring for the job and API | RM-06 | done |
| RM-08 | Contract, feature card, database map, operations runbook | RM-04 | done |
| RM-09 | Admin UI in brevi-web | RM-08 | separate frontend PR |
| RM-10 | Deploy, connect a test bot, cut over from Make | RM-09 | waiting for the owner |
