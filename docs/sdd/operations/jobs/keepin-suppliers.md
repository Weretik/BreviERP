# KeepinCRM suppliers runbook

Feature: [Suppliers](../../features/reference/suppliers.md), specification [REF-001](../../specs/reference/001-suppliers-keepincrm/README.md). Never put the API key into documentation, logs or chat.

## Configuration

| Setting | Where | Secret |
| --- | --- | --- |
| `KeepinCrm:ApiKey` | API and `brevierp-keepin-suppliers` job, Secret Manager `keepincrm-api-key` | Yes |
| `KeepinCrm:BaseUrl` | Optional, default `https://api.keepincrm.com/v1/` | No |

The key is the same one the client's Google Sheet script kept in Script Properties as `KEEPINCRM_API_KEY`. The [deploy workflow](../deployment/cloud-run-deploy.md) always attaches the secret; a deploy fails if it is missing, so create it before the first deploy:

1. Google Cloud console → Security → Secret Manager → Create secret, name `keepincrm-api-key`, value = the key.
2. Grant the Cloud Run runtime service account «Secret Manager Secret Accessor» on it (the same account that reads `telegram-reminders-bot-token`).

## What a run does

`GET /suppliers?page=N` with header `X-Auth-Token`, 25 per page, until a short or empty page. Then, in one save: a supplier already linked by `KeepinCrmId` is refreshed; an unlinked supplier with the same name (case, punctuation and ТОВ/ФОП ignored) is linked; the rest are created with the next Brevi ID. «Використовувати в проєкті» and notes are never changed, nothing is deleted, and a linked supplier missing from KeepinCRM is only reported. An error or an empty answer changes nothing. It is safe to repeat.

## Daily job

Cloud Run Job `brevierp-keepin-suppliers` runs `Host.Seed --keepin-suppliers-sync=true`. The deploy workflow creates it from `brevierp-seed` if missing and updates image, argument and secret on every deploy.

Cloud Scheduler job (HTTP target, once):

- Schedule `0 7 * * *`, time zone `Europe/Kyiv`.
- URL `https://run.googleapis.com/v2/projects/<project>/locations/europe-central2/jobs/brevierp-keepin-suppliers:run`, method POST, no body.
- Auth: OAuth token (scope `https://www.googleapis.com/auth/cloud-platform`) of a service account that can run the job.

Logs: `KeepinCRM suppliers sync started`, then `... OK: received N, created N, linked N, updated N, missing in KeepinCRM: ...` or `... FAILED: <reason>` (exit code 1).

## On demand

Admin page «Постачальники» → «Оновити з KeepinCRM» calls `POST /api/reference/suppliers/keepin-sync` (same use case). 503 means the key is missing, KeepinCRM did not answer, or it returned no suppliers.
