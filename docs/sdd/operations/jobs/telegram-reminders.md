# Telegram reminders runbook

Feature: [Telegram reminders](../../features/reminders/telegram-reminders.md). Do not put the bot token, webhook secret or real URLs into documentation or logs.

## Configuration

| Setting | Where | Secret |
| --- | --- | --- |
| `Reminders:Telegram:BotToken` | API and `brevierp-seed` job, Secret Manager `telegram-reminders-bot-token` | Yes |
| `Reminders:Telegram:WebhookSecret` | API only, Secret Manager `telegram-reminders-webhook-secret` (random 32+ characters, letters, digits, `_`, `-`) | Yes |
| `Reminders:Telegram:WebhookUrl` | API environment variable: public API origin + `/api/reminders/telegram/webhook` | No |
| `Reminders:Telegram:ApiBaseUrl` | Optional, default `https://api.telegram.org` | No |

`cloudbuild.yaml` attaches the two secrets only if they exist in Secret Manager, so deployment works before they are created. Without the token the API starts, the admin works, the "connect" action returns 409 and the dispatch run fails with a clear message.

## Daily run

`Host.Seed --reminders-dispatch=true` runs only the dispatcher (no migrations or seed). It is safe to repeat on the same day: each period gets at most one card per Kyiv date. On a non-working day it exits without sending. Exit code 1 means at least one card failed; the next run retries it.

Cloud Scheduler job (HTTP target, once):

- Schedule `0 9 * * 1-5`, time zone `Europe/Kyiv`.
- URL `https://run.googleapis.com/v2/projects/<project>/locations/europe-central2/jobs/brevierp-seed:run`, method POST.
- Body `{"overrides":{"containerOverrides":[{"args":["--reminders-dispatch=true"]}]}}`.
- Auth: OAuth token (scope `https://www.googleapis.com/auth/cloud-platform`) of a service account with `roles/run.jobsExecutorWithOverrides` on the job; `roles/run.invoker` alone cannot pass overrides.

Check: Cloud Run Job executions and logs show `Reminders <date>: working day True, new periods N, sent N, failed 0`.

## Connect the bot

1. Deploy the API with the token, secret and `WebhookUrl`.
2. In the admin, Нагадування → Чати → «Підключити бота» (calls `POST /api/reminders/telegram/connect`, which calls Telegram `setWebhook`).
3. Add the bot to the group (or send /start in a private chat). The chat appears as not approved; approve it, optionally send a test message, and assign it to the organizations.

## Cutover from Make

1. Test first with a separate test bot and test group (one bot has only one webhook).
2. On a Friday after the Make run: switch the token secret to the real bot, redeploy, press «Підключити бота» (this replaces the Make webhook), approve the group and assign tasks.
3. Disable the Make scenario. On Monday check the first run.

Rollback: pause the Cloud Scheduler job and set the Make webhook URL again in Telegram; reminder data stays.
