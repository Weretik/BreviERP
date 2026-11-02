# Telegram reminders runbook

Feature: [Telegram reminders](../../features/reminders/telegram-reminders.md). Do not put the bot token, webhook secret or real URLs into documentation or logs.

## Configuration

| Setting | Where | Secret |
| --- | --- | --- |
| `Reminders:Telegram:BotToken` | API and `brevierp-reminders` job, Secret Manager `telegram-reminders-bot-token` | Yes |
| `Reminders:Telegram:WebhookSecret` | API only, Secret Manager `telegram-reminders-webhook-secret` (random 32+ characters, letters, digits, `_`, `-`) | Yes |
| `Reminders:Telegram:WebhookUrl` | API environment variable: public API origin + `/api/reminders/telegram/webhook` | No |
| `Reminders:Telegram:ApiBaseUrl` | Optional, default `https://api.telegram.org` | No |

The [deploy workflow](../deployment/cloud-run-deploy.md) always attaches both secrets to the API and the token to the `brevierp-reminders` job; a deploy fails if a secret is missing. The Cloud Run service account needs Secret Manager Secret Accessor.

## Daily run

The separate Cloud Run Job `brevierp-reminders` runs `Host.Seed --reminders-dispatch=true`: the seed image with this argument baked in, so it only dispatches (no migrations or seed). The deploy workflow creates it if missing by copying `brevierp-seed` (database connection, service account, Cloud SQL), then updates its image, argument and token on every deploy. It is safe to repeat on the same day: each period gets at most one card per Kyiv date. On a non-working day it exits without sending. Exit code 1 means at least one card failed; the next run retries it.

Cloud Scheduler job (HTTP target, once):

- Schedule `0 9 * * 1-5`, time zone `Europe/Kyiv`.
- URL `https://run.googleapis.com/v2/projects/<project>/locations/europe-central2/jobs/brevierp-reminders:run`, method POST, no body.
- Auth: OAuth token (scope `https://www.googleapis.com/auth/cloud-platform`) of a service account with `roles/run.invoker` (or any role that can run the job).

Logs of each execution (Cloud Run → Jobs → `brevierp-reminders` → Logs):

- `Reminders run started`, then `Reminders <date>: N active task(s) with a chat, M ... without a chat`.
- One line per task: no chat (warning), no open period in the window, already sent today, chat not approved or bot left (warning), `sent to «chat»`, or `not sent ... <Telegram reason>` (error).
- Final line `Reminders <date>: OK|FAILED. ... sent N, not due yet N, already sent today N, chat unavailable N, failed N`. A run that cannot start logs `Reminders run FAILED: <reason>`.

## Connect the bot

1. Deploy the API with the token, secret and `WebhookUrl`.
2. In the admin, Нагадування → Чати → «Підключити бота» (calls `POST /api/reminders/telegram/connect`, which calls Telegram `setWebhook`).
3. Add the bot to the group (or send /start in a private chat). The chat appears as not approved; approve it, optionally send a test message, and assign it to the organizations.

## Existing bot

Use the existing bot @brevibuhtime_bot and its current group; no separate bot or group is needed. «Підключити бота» replaces any webhook set earlier (for example by Make), so the old Make scenario stops receiving updates and can be deleted.

Rollback: pause the Cloud Scheduler job; reminder data stays.
