# Design and decisions

## D-01 New module `Reminders`

The capability does not belong to Catalog, Reference, Crm, Accounting or Identity: it owns no product, customer or money data and has its own lifecycle (tasks, periods, chats, calendar). Under the [module rules](../../../standards/module-rules.md) it is a new business module with four layers, its own schema `reminders` and history table `reminders.__EFMigrationsHistory`. It has no foreign keys to other modules. The admin actor is stored as plain values (user id and name) taken from claims.

Rejected: putting it into Crm (unrelated aggregate, would widen Crm's schema) or a separate service (extra deployment and database for a small feature).

## D-02 Daily run through the existing Host.Seed job

`Host.Seed --reminders-dispatch=true` runs only the dispatcher: no migrations or seed. Cloud Scheduler executes the existing `brevierp-seed` Cloud Run Job on weekdays at 09:00 Europe/Kyiv with an argument override. The job already has the image, database connection and service account, so no new job or secret wiring is needed besides the bot token.

Rejected:
- A separate `brevierp-reminders` Cloud Run Job (first plan): duplicates image and database configuration for the same binary.
- A timer inside Host.Api: Cloud Run scales the API to zero and may run several instances, so a timer is neither reliable nor single.
- Keeping Make: the user wants it dropped.

The run also checks the Kyiv working day itself, so a misconfigured schedule (for example a Saturday trigger) sends nothing.

## D-03 Buttons through a webhook in Host.Api

Telegram delivers button presses to one webhook per bot. `POST /api/reminders/telegram/webhook` in Host.Api handles them; a job cannot. The endpoint is anonymous, hidden from OpenAPI and accepts only requests with the `X-Telegram-Bot-Api-Secret-Token` header equal to the configured secret (constant-time compare). The webhook URL is configuration, and the admin "connect" action calls `setWebhook` with `allowed_updates` message, callback_query and my_chat_member.

One bot has one webhook. The existing bot and group are connected directly; «Підключити бота» replaces the old Make webhook (see the [runbook](../../../operations/jobs/telegram-reminders.md)).

## D-04 Periods and idempotency

- `ReminderOccurrence` is one task period, unique by (task, period start). The generator creates periods whose window has opened since "active from".
- `SentReminderMessage` is unique by (period, Kyiv date) and is saved after each successful send. A repeated or retried run on the same day skips periods already sent.
- Closing is "first wins": the domain refuses a second close and the handler answers with who closed it.
- A task edit recalculates the deadline of its open periods.

## D-05 Message and button format

HTML parse mode with manual escaping of `&`, `<`, `>`. Callback data is `rm:{periodId}:{action}` with actions done, later, back, s1 (tomorrow), s2 (2 days), s3 (Monday); it fits the 64-byte limit. Ukrainian weekday names and period labels are built in code, not from the server culture.

## D-06 Time

All calendar decisions use Kyiv local date (`Europe/Kyiv`) from an injected `TimeProvider`. Timestamps are stored in UTC.

## Components

```text
Cloud Scheduler ──► brevierp-seed job (--reminders-dispatch) ──► ReminderDispatcher ──► Telegram sendMessage
Telegram ──► POST /api/reminders/telegram/webhook ──► HandleTelegramUpdate ──► ReminderButtonHandler / ReminderChatTracker
Admin UI ──► /api/reminders/{tasks,occurrences,chats,non-working-days} ──► Mediator use cases ──► RemindersDbContext
```

Domain (pure): `WorkingCalendar`, `ReminderPeriod`, `DueRule`, `ReminderSchedule`, `ReminderPlanner`, `ReminderSnooze`, entities. Application: use cases with FluentValidation validators, dispatcher, generator, message formatter, Telegram update handling. Infrastructure: EF Core context and configurations, `TelegramBotApiGateway` (HttpClient), default task seeder. Api: controllers.

## Rollback

Disable the Cloud Scheduler job. The schema is additive; removing it needs a separate migration decision.
