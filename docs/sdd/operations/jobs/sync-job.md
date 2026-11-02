# Daily sync job runbook

One Cloud Run Job, `brevierp-sync`, holds every scheduled refresh from an external system and every scheduled import. It runs `Host.Seed --sync=true`: the steps listed in `src/Bootstrapper/Host.Seed/Jobs/Sync/SyncJobRunner.cs`, one after another, each in its own DI scope. A failed step is logged and the next step still runs; the execution ends with exit code 1 if any step failed. No migrations or seeding. Never put API keys into documentation, logs or chat.

## Steps

| Step | What it does | Secret | Details |
| --- | --- | --- | --- |
| `keepin-suppliers` | KeepinCRM → Brevi supplier refresh | `keepincrm-api-key` → `KeepinCrm__ApiKey` | [below](#keepin-suppliers) |
| `keepin-clients` | KeepinCRM → Brevi client refresh | the same key | [below](#keepin-clients) |

`--sync-tasks=keepin-suppliers,...` runs only the named steps (for a manual execution with overridden arguments). An unknown name stops the job before anything runs.

### Adding a step

1. Implement `ISyncTask` in `Host.Seed/Jobs/Sync/` with a kebab-case `Name`. It must be safe to repeat and change nothing when it fails.
2. Add it to `Tasks` in `SyncJobRunner`.
3. Register its services in `SeedHostServicesExtensions`.
4. Attach its secrets to the job in the `Create or update daily sync job` step of the [deploy workflow](../deployment/cloud-run-deploy.md), and add a row above.

## Schedule

Cloud Run → Jobs → `brevierp-sync` → Triggers → Add scheduler trigger: `0 7 * * *`, time zone `Europe/Kyiv`, a service account that can run the job (the one used by the reminders schedule). The deploy workflow creates the job from `brevierp-seed` if missing and updates image, argument and secrets on every deploy; it never executes it.

Logs: `Sync started: <steps>`, then per step `<step> started`, its own lines, and `<step> OK` or `<step> FAILED: <reason>`; finally `Sync finished: all N steps OK` or the error `Sync failed: <steps>`.

## keepin-suppliers

Feature: [Suppliers](../../features/reference/suppliers.md), specification [REF-001](../../specs/reference/001-suppliers-keepincrm/README.md).

| Setting | Where | Secret |
| --- | --- | --- |
| `KeepinCrm:ApiKey` | API and `brevierp-sync`, Secret Manager `keepincrm-api-key` | Yes |
| `KeepinCrm:BaseUrl` | Optional, default `https://api.keepincrm.com/v1/` | No |

The key is the same one the client's Google Sheet script kept in Script Properties as `KEEPINCRM_API_KEY`. The deploy workflow always attaches the secret to the API and the job; a deploy fails if it is missing. Create it once: Cloud Run → `brevierp-api` → Edit → Variables & Secrets → Reference a secret, name `KeepinCrm__ApiKey`, Create new secret `keepincrm-api-key`, version `latest` (this also grants the runtime service account access). Locally: `dotnet user-secrets set "KeepinCrm:ApiKey" "<key>" --project src/Bootstrapper/Host.Api/Host.Api.csproj`.

A run calls `GET /suppliers?page=N` with header `X-Auth-Token`, 25 per page, until a short or empty page. Then, in one save: a supplier already linked by `KeepinCrmId` is refreshed; an unlinked supplier with the same name (case, punctuation and ТОВ/ФОП ignored) is linked; the rest are created with the next Brevi ID. «Використовувати в проєкті» and notes are never changed, nothing is deleted, and a linked supplier missing from KeepinCRM is only reported. An error or an empty answer changes nothing.

Step log: `Received N, created N, linked N, updated N, missing in KeepinCRM: ...`.

On demand: admin page «Постачальники» → «Оновити з KeepinCRM» calls `POST /api/reference/suppliers/keepin-sync` (same use case, runs in the API, not the job). 503 means the key is missing, KeepinCRM did not answer, or it returned no suppliers.

## keepin-clients

Feature: [Clients and quotes](../../features/crm/clients-and-quotes.md), specification [CRM-002](../../specs/crm/002-keepincrm-clients/README.md). Uses the same `KeepinCrm:ApiKey`; optional `KeepinCrm:ClientOwnerUserId` (default 1) owns new clients and `KeepinCrm:ClientMaxPages` (default 1000) stops runaway paging.

A run calls `GET /clients?page=N&q[lead_false]=true` until `pagination.total_pages`. Then, in one save: a client linked by `keepincrm/client/<id>` is refreshed (name, kind, active from `archived`, the «KeepinCRM» contact with phones and e-mails); a new one is created; a linked client missing from KeepinCRM is marked not used. The daily step never deletes. An error or an empty answer changes nothing.

Step log: `Received N, created N, updated N, unchanged N, marked not used: ...`.

On demand: `POST /api/crm/customers/keepin-sync` with `{"dryRun": true}` previews and `{"dryRun": false}` applies. The one-time transfer adds `"removeUnlinked": true` (Admin only): Brevi clients without a KeepinCRM link are deleted, those used by quotes are hidden. Always run the preview first.
