# Sync job runbook

**When it runs:** Monday to Friday at 09:00 Kyiv time (Cloud Scheduler `0 9 * * 1-5`, `Europe/Kyiv`). It is not a nightly job and does not run on Saturday or Sunday; older notes and the actor name `nightly-sync` are historical names only.

One Cloud Run Job, `brevierp-sync`, holds every scheduled refresh from an external system and every scheduled import. It runs `Host.Seed --sync=true`: the steps listed in `src/Bootstrapper/Host.Seed/Jobs/Sync/SyncJobRunner.cs`, one after another, each in its own DI scope. A failed step is logged and the next step still runs; the execution ends with exit code 1 if any step failed. No migrations or seeding. Never put API keys into documentation, logs or chat.

## Steps

| Step | What it does | Secret | Details |
| --- | --- | --- | --- |
| `keepin-suppliers` | KeepinCRM → Brevi supplier refresh | `keepincrm-api-key` → `KeepinCrm__ApiKey` | [below](#keepin-suppliers) |
| `keepin-clients` | KeepinCRM → Brevi client refresh | the same key | [below](#keepin-clients) |
| `sewing-prices` | Publishes Sewing prices whose inputs changed | none | [below](#sewing-prices) |
| `media-thumbnails` | Makes size and previews for library photos without them | `storage-login`, `storage-password` → `AdmToolsStorage__Login`, `AdmToolsStorage__Password` | [below](#media-thumbnails) |
| `purchase-invoices` | Google Sheet «финансы» → «Рахунки на закупку» | none; the sheet is shared with the runtime service account | [below](#purchase-invoices) |
| `ppe-supplier-prices` | Mondays only: АВ-Центр, 3C-Маркет, Перчатка-центр and the Трєле cabinet; other days nothing | `trele-login`, `trele-password` → `SupplierCabinets__Trele__…` (optional) | [below](#ppe-supplier-prices) |

`--sync-tasks=keepin-suppliers,...` runs only the named steps (for a manual execution with overridden arguments). An unknown name stops the job before anything runs.

### Adding a step

1. Implement `ISyncTask` in `Host.Seed/Jobs/Sync/` with a kebab-case `Name`. It must be safe to repeat and change nothing when it fails.
2. Add it to `Tasks` in `SyncJobRunner`.
3. Register its services in `SeedHostServicesExtensions`.
4. Attach its secrets to the job in the `Create or update daily sync job` step of the [deploy workflow](../deployment/cloud-run-deploy.md), and add a row above.

## Schedule

Cloud Run → Jobs → `brevierp-sync` → Triggers → Add scheduler trigger: `0 9 * * 1-5` (Mon–Fri 09:00), time zone `Europe/Kyiv`, a service account that can run the job (the one used by the reminders schedule). The deploy workflow creates the job from `brevierp-seed` if missing and updates image, argument and secrets on every deploy; it never executes it.

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

## sewing-prices

Feature: [Sewing pricing](../../features/catalog/sewing-pricing.md), specification [CAT-012](../../specs/catalog/012-sewing-price-auto-publication/README.md). No secret, no settings.

The API already publishes new prices right after a fabric, accessory, operation, composition or pricing parameter is saved. This step is the safety net: it checks every Sewing model and publishes each active Article whose live price differs from the published one or was never published (actor `nightly-sync`), one transaction per model. It catches changes saved outside the API (jobs, scripts) and automatic runs that failed. On a normal run it publishes nothing. Articles that cannot be calculated keep their published price and are logged with their blocker codes.

Step log: `Published N articles, unchanged N, not calculable N, skipped models: ...`, then one warning per model without a price.

## media-thumbnails

Photo previews of the «Фото» library ([MED-001](../../specs/media/001-media-library/README.md), stage 3). New uploads get their size and preview at once; this step covers photos uploaded before previews existed and uploads whose preview failed. It takes up to 500 photos with `ImageProcessedAt` empty, oldest first, downloads each original from the CDN, writes the size and uploads a WebP preview to `/library/thumbs`. A photo that cannot be read is marked processed without a preview; a download or CDN error leaves it for the next night. Repeating the step is safe.

Step log: `Processed N photos (N small or unreadable, no preview), failed N, left for next runs N`, then one warning per failed photo.

## ppe-supplier-prices

Feature: [PPE supplier price import](../../features/catalog/ppe-supplier-prices.md), specification [CAT-017](../../specs/catalog/017-ppe-supplier-price-import/README.md) stages 2–3. The discounts are those saved in «Імпорт цін».

It works only on Kyiv Monday (09:00 run); on other days it does nothing. It reads АВ-Центр and Перчатка-центр product pages, the six 3C-Маркет Google lists, and signs in to the Трєле dealer cabinet to read the dealer price of each product link. Insight is not read here: it is updated only from its XML files in «Імпорт цін». It saves each changed supplier price on the card and as a new approved offer version (source «Імпорт цін: …», run trigger «Розклад», actor `nightly-sync`). Products with «Дозволити ручну ціну» or not active keep their price. A source that cannot be read (supplier not found, a 3C list not loading, cabinet login missing or refused) changes nothing, records its reason in «Імпорт цін», and the step fails after the other sources ran. Repeating the step is safe: unchanged prices are not saved again.

Step log: one line per source `<source>: checked N, changed N, unchanged N, skipped N, problems N`, or a warning `<source>: nothing changed: <reason>`.

| Setting | Where | Secret |
| --- | --- | --- |
| `SupplierCabinets:Trele:Login`, `…:Password` | API and `brevierp-sync`, Secret Manager `trele-login`, `trele-password` | Yes |

It is the dealer account the client's script kept in Script Properties (`TRELE_LOGIN`, `TRELE_PASSWORD`). Create the two secrets in Secret Manager and give the Cloud Run runtime service account (the one that reads `keepincrm-api-key`) the «Secret Manager Secret Accessor» role on them; the next deploy attaches them. Without both the deploy still succeeds and the Трєле cabinet stays off. The secrets `insight-login` and `insight-password` are no longer used and can be deleted. Locally: `dotnet user-secrets set "SupplierCabinets:Trele:Login" "<login>" --project src/Bootstrapper/Host.Api/Host.Api.csproj` (and the password).

On demand: «Імпорт цін» → source → «Оновити зараз» calls `POST /api/v1/pricing/ppe/supplier-prices/{key}/refresh` (preview, then `?dryRun=false`).

## purchase-invoices

Feature: [Purchase invoices](../../features/purchasing/purchase-invoices.md), specification [PUR-001](../../specs/purchasing/001-purchase-invoices/README.md). The same run as «Оновити зараз» (`POST /api/purchasing/invoices/sync`).

The step reads the Google Sheet «БРЕВІ команда» (`Purchasing:Sheet:SpreadsheetId`, `SheetId` 824424675, `SheetTitle` «финансы»; the defaults are in code) with Application Default Credentials, scope `spreadsheets.readonly`. No secret is needed. One-time setup in project `brevistoreapi`:

1. Enable Google Sheets API (APIs & Services → Library → Google Sheets API → Enable).
2. Share the spreadsheet with the runtime service account of `brevierp-api` and `brevierp-sync` (Cloud Run → service → Security → Service account; the default compute account is `<project-number>-compute@developer.gserviceaccount.com`) as a viewer.

Locally: `gcloud auth application-default login --scopes=https://www.googleapis.com/auth/cloud-platform,https://www.googleapis.com/auth/spreadsheets.readonly` with an account that can open the sheet.

The whole sheet is read and checked first, then applied in one transaction under a lock; a failure changes nothing and is saved as the last error. Step log: `Rows N, files N; messages added N, updated N; rows added N, updated N, gone N; files added N, gone N`.
