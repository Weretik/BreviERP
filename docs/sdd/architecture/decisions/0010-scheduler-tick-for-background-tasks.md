# ADR-0010: One scheduler tick runs the outbox and every due background task

**Status:** accepted · **Date:** 2026-11-14 (tick every five minutes chosen by the owner) · **Owner:** Виталий (project owner)

## Context

Periodic work grew piece by piece: `brevierp-sync` (KeepinCRM, Google Sheets, prices, photo previews) Monday to Friday at 09:00, `brevierp-reminders` for Telegram reminders, and the outbox. Each was a separate Cloud Run Job with its own Cloud Scheduler entry. The owner could not see in the admin what ran, when, and whether it failed, and could not change a time without opening Google Cloud.

## Options

- Keep one job and one Cloud Scheduler entry per kind of work: rejected; invisible and hard to change.
- An in-process scheduler (Hangfire, Quartz): rejected; Cloud Run scales to zero.
- One tick that starts due tasks, with schedules in the database: chosen. Every minute was considered; the owner chose five minutes for Kedr and Brevi alike.

## Decision

- The Scheduling module owns the code list of tasks, their schedules in Kyiv time (interval, daily times on weekdays, manual), state and run history, in schema `scheduling`.
- Cloud Scheduler `brevierp-tick` calls `POST /internal/scheduler/tick` every five minutes with the OIDC token of `brevierp-scheduler`, checked in the endpoint.
- The tick sends the outbox, claims due tasks with a `Version` concurrency token and starts one Cloud Run Job execution with `--tasks=key:runId,...`; the job records each task's start, summary or failure.
- On «Адміністрування → Фонові задачі» the Admin changes the schedule, the on/off switch and the next run and can press «Запустити зараз».
- `brevierp-sync` and `brevierp-reminders` are paused and then removed after the switch-over ([runbook](../../operations/jobs/background-tasks.md)).

## Consequences

- Every periodic task is visible and controlled in the admin; a new task is code, not cloud work.
- A task may start up to five minutes late.

## Sources

[Background tasks](../../features/scheduling/background-tasks.md), [SDD](../../specs/scheduling/001-background-tasks/README.md).
